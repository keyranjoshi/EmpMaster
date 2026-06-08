using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using EmpMaster.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace EmpMaster.Controllers
{
    public class Employees_AdoController : Controller
    {
        private readonly string _connectionString;

        public Employees_AdoController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Connection string 'EmployeeMasterContext' or 'DefaultConnection' is required.");
        }

        // GET: Employees_Ado
        public async Task<IActionResult> Index(string? searchString, string? sortOrder, int page = 1, int pageSize = 10)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewData["TitleSortParm"] = sortOrder == "title_asc" ? "title_desc" : "title_asc";
            ViewData["SalarySortParm"] = sortOrder == "salary_asc" ? "salary_desc" : "salary_asc";

            var today = DateTime.Today;

            var model = new List<EmployeeViewModel>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // Load employees
                var cmd = new SqlCommand(@"
                    SELECT Id, Name, Ssn, Dob, Address, City, State, Zip, Phone, JoinDate, ExitDate
                    FROM Employees", conn);

                using (var rdr = await cmd.ExecuteReaderAsync())
                {
                    while (await rdr.ReadAsync())
                    {
                        var vm = new EmployeeViewModel
                        {
                            Id = rdr.GetInt32(rdr.GetOrdinal("Id")),
                            Name = rdr["Name"] as string,
                            Ssn = rdr["Ssn"] as string,
                            Dob = rdr.GetDateTime(rdr.GetOrdinal("Dob")),
                            Address = rdr["Address"] as string,
                            City = rdr["City"] as string,
                            State = rdr["State"] as string,
                            Zip = rdr["Zip"] as string,
                            Phone = rdr["Phone"] as string,
                            JoinDate = rdr.GetDateTime(rdr.GetOrdinal("JoinDate")),
                            ExitDate = rdr.IsDBNull(rdr.GetOrdinal("ExitDate")) ? (DateTime?)null : rdr.GetDateTime(rdr.GetOrdinal("ExitDate")),
                            CurrentTitle = null,
                            CurrentSalary = null
                        };
                        model.Add(vm);
                    }
                }

                // For each employee fetch current salary/title
                var salaryCmd = new SqlCommand(@"
                    SELECT TOP(1) Title, Salary
                    FROM EmployeeSalaries
                    WHERE EmployeeId = @empId AND FromDate <= @today AND (ToDate IS NULL OR ToDate >= @today)
                    ORDER BY FromDate DESC", conn);
                salaryCmd.Parameters.Add(new SqlParameter("@empId", SqlDbType.Int));
                salaryCmd.Parameters.Add(new SqlParameter("@today", SqlDbType.Date) { Value = today.Date });

                foreach (var vm in model)
                {
                    salaryCmd.Parameters["@empId"].Value = vm.Id;
                    using (var rdr = await salaryCmd.ExecuteReaderAsync())
                    {
                        if (await rdr.ReadAsync())
                        {
                            vm.CurrentTitle = rdr["Title"] as string;
                            vm.CurrentSalary = rdr.IsDBNull(rdr.GetOrdinal("Salary")) ? (decimal?)null : rdr.GetDecimal(rdr.GetOrdinal("Salary"));
                        }
                    }
                }

                // Compute title averages for current salaries
                var avgCmd = new SqlCommand(@"
                    SELECT Title, AVG(CAST(Salary AS decimal(18,2))) AS AvgSalary
                    FROM EmployeeSalaries
                    WHERE FromDate <= @today AND (ToDate IS NULL OR ToDate >= @today)
                    GROUP BY Title", conn);
                avgCmd.Parameters.Add(new SqlParameter("@today", SqlDbType.Date) { Value = today.Date });

                var titleAverages = new Dictionary<string, decimal>();
                using (var rdr = await avgCmd.ExecuteReaderAsync())
                {
                    while (await rdr.ReadAsync())
                    {
                        var title = rdr["Title"] as string;
                        var avg = rdr.IsDBNull(rdr.GetOrdinal("AvgSalary")) ? 0m : rdr.GetDecimal(rdr.GetOrdinal("AvgSalary"));
                        if (title != null) titleAverages[title] = avg;
                    }
                }

                foreach (var vm in model)
                {
                    if (!string.IsNullOrEmpty(vm.CurrentTitle) && titleAverages.TryGetValue(vm.CurrentTitle, out var avg))
                    {
                        vm.AverageTitleSalary = avg;
                        vm.SalaryDifference = vm.CurrentSalary.HasValue ? vm.CurrentSalary.Value - avg : (decimal?)null;
                    }
                    else
                    {
                        vm.AverageTitleSalary = null;
                        vm.SalaryDifference = null;
                    }
                }
            }

            // Apply search
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var q = searchString.Trim().ToLowerInvariant();
                model = model.Where(m => (!string.IsNullOrEmpty(m.Name) && m.Name.ToLowerInvariant().Contains(q))
                    || (!string.IsNullOrEmpty(m.CurrentTitle) && m.CurrentTitle.ToLowerInvariant().Contains(q))).ToList();
            }

            // Apply sorting
            model = sortOrder switch
            {
                "name_desc" => model.OrderByDescending(m => m.Name).ToList(),
                "name_asc" => model.OrderBy(m => m.Name).ToList(),
                "title_asc" => model.OrderBy(m => m.CurrentTitle).ToList(),
                "title_desc" => model.OrderByDescending(m => m.CurrentTitle).ToList(),
                "salary_asc" => model.OrderBy(m => m.CurrentSalary).ToList(),
                "salary_desc" => model.OrderByDescending(m => m.CurrentSalary).ToList(),
                _ => model.OrderBy(m => m.Name).ToList(),
            };

            // Pagination
            var totalItems = model.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Clamp(page, 1, Math.Max(1, totalPages));
            var paged = model.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["PageSize"] = pageSize;

            return View("Index", paged);
        }

        // GET: Employees_Ado/TitleList
        public async Task<IActionResult> TitleList(int page = 1, int pageSize = 20)
        {
            var items = new List<TitleViewModel>();
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    SELECT Title, MIN(Salary) AS MinSalary, MAX(Salary) AS MaxSalary
                    FROM EmployeeSalaries
                    GROUP BY Title
                    ORDER BY Title
                    OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY", conn);
                cmd.Parameters.AddWithValue("@skip", (page - 1) * pageSize);
                cmd.Parameters.AddWithValue("@take", pageSize);

                using (var rdr = await cmd.ExecuteReaderAsync())
                {
                    while (await rdr.ReadAsync())
                    {
                        items.Add(new TitleViewModel
                        {
                            Title = rdr["Title"] as string,
                            MinSalary = (decimal)(rdr.IsDBNull(rdr.GetOrdinal("MinSalary")) ? (decimal?)null : rdr.GetDecimal(rdr.GetOrdinal("MinSalary"))),
                            MaxSalary = (decimal)(rdr.IsDBNull(rdr.GetOrdinal("MaxSalary")) ? (decimal?)null : rdr.GetDecimal(rdr.GetOrdinal("MaxSalary")))
                        });
                    }
                }
            }

            // Count total items for paging
            int totalItems = 0;
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using var cmd = new SqlCommand("SELECT COUNT(DISTINCT Title) FROM EmployeeSalaries", conn);
                totalItems = (int)await cmd.ExecuteScalarAsync();
            }

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Clamp(page, 1, Math.Max(1, totalPages));

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["PageSize"] = pageSize;

            return View("TitleList", items);
        }

        // GET: Employees_Ado/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return BadRequest();
            EmployeeViewModel vm = null;
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"SELECT Id, Name, Ssn, Dob, Address, City, State, Zip, Phone, JoinDate, ExitDate FROM Employees WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id.Value);
                using var rdr = await cmd.ExecuteReaderAsync();
                if (await rdr.ReadAsync())
                {
                    vm = new EmployeeViewModel
                    {
                        Id = rdr.GetInt32(rdr.GetOrdinal("Id")),
                        Name = rdr["Name"] as string,
                        Ssn = rdr["Ssn"] as string,
                        Dob = rdr.GetDateTime(rdr.GetOrdinal("Dob")),
                        Address = rdr["Address"] as string,
                        City = rdr["City"] as string,
                        State = rdr["State"] as string,
                        Zip = rdr["Zip"] as string,
                        Phone = rdr["Phone"] as string,
                        JoinDate = rdr.GetDateTime(rdr.GetOrdinal("JoinDate")),
                        ExitDate = rdr.IsDBNull(rdr.GetOrdinal("ExitDate")) ? (DateTime?)null : rdr.GetDateTime(rdr.GetOrdinal("ExitDate"))
                    };
                }
            }
            if (vm == null) return NotFound();
            return View("Details", vm);
        }

        // GET: Employees_Ado/Create
        public IActionResult Create()
        {
            return View("Create", new EmployeeViewModel());
        }

        // POST: Employees_Ado/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel vm)
        {
            if (!ModelState.IsValid) return View("Create", vm);

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // Check duplicate SSN
                using (var chk = new SqlCommand("SELECT COUNT(1) FROM Employees WHERE Ssn = @ssn", conn))
                {
                    chk.Parameters.AddWithValue("@ssn", vm.Ssn ?? string.Empty);
                    var exists = (int)await chk.ExecuteScalarAsync() > 0;
                    if (exists)
                    {
                        ModelState.AddModelError(nameof(vm.Ssn), "An employee with this SSN already exists.");
                        return View("Create", vm);
                    }
                }

                try
                {
                    using var ins = new SqlCommand(@"
                        INSERT INTO Employees (Name,Ssn,Dob,Address,City,State,Zip,Phone,JoinDate,ExitDate)
                        VALUES (@Name,@Ssn,@Dob,@Address,@City,@State,@Zip,@Phone,@JoinDate,@ExitDate)", conn);
                    ins.Parameters.AddWithValue("@Name", vm.Name ?? (object)DBNull.Value);
                    ins.Parameters.AddWithValue("@Ssn", vm.Ssn ?? (object)DBNull.Value);
                    ins.Parameters.AddWithValue("@Dob", vm.Dob);
                    ins.Parameters.AddWithValue("@Address", vm.Address ?? (object)DBNull.Value);
                    ins.Parameters.AddWithValue("@City", vm.City ?? (object)DBNull.Value);
                    ins.Parameters.AddWithValue("@State", vm.State ?? (object)DBNull.Value);
                    ins.Parameters.AddWithValue("@Zip", vm.Zip ?? (object)DBNull.Value);
                    ins.Parameters.AddWithValue("@Phone", vm.Phone ?? (object)DBNull.Value);
                    ins.Parameters.AddWithValue("@JoinDate", vm.JoinDate);
                    ins.Parameters.AddWithValue("@ExitDate", vm.ExitDate.HasValue ? (object)vm.ExitDate.Value : DBNull.Value);
                    await ins.ExecuteNonQueryAsync();

                    TempData["Success"] = "Employee created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (SqlException ex)
                {
                    ModelState.AddModelError(string.Empty, "Database error while creating employee: " + ex.Message);
                    return View("Create", vm);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Unexpected error: " + ex.Message);
                    return View("Create", vm);
                }
            }
        }

        // GET: Employees_Ado/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return BadRequest();
            return await Details(id); // reuse Details mapping then show Edit view
        }

        // POST: Employees_Ado/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View("Edit", vm);

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // Check duplicate SSN for other records
                using (var chk = new SqlCommand("SELECT COUNT(1) FROM Employees WHERE Ssn = @ssn AND Id <> @id", conn))
                {
                    chk.Parameters.AddWithValue("@ssn", vm.Ssn ?? string.Empty);
                    chk.Parameters.AddWithValue("@id", id);
                    var exists = (int)await chk.ExecuteScalarAsync() > 0;
                    if (exists)
                    {
                        ModelState.AddModelError(nameof(vm.Ssn), "Another employee with this SSN already exists.");
                        return View("Edit", vm);
                    }
                }

                try
                {
                    using var upd = new SqlCommand(@"
                        UPDATE Employees
                        SET Name=@Name,Ssn=@Ssn,Dob=@Dob,Address=@Address,City=@City,State=@State,Zip=@Zip,Phone=@Phone,JoinDate=@JoinDate,ExitDate=@ExitDate
                        WHERE Id=@Id", conn);
                    upd.Parameters.AddWithValue("@Id", id);
                    upd.Parameters.AddWithValue("@Name", vm.Name ?? (object)DBNull.Value);
                    upd.Parameters.AddWithValue("@Ssn", vm.Ssn ?? (object)DBNull.Value);
                    upd.Parameters.AddWithValue("@Dob", vm.Dob);
                    upd.Parameters.AddWithValue("@Address", vm.Address ?? (object)DBNull.Value);
                    upd.Parameters.AddWithValue("@City", vm.City ?? (object)DBNull.Value);
                    upd.Parameters.AddWithValue("@State", vm.State ?? (object)DBNull.Value);
                    upd.Parameters.AddWithValue("@Zip", vm.Zip ?? (object)DBNull.Value);
                    upd.Parameters.AddWithValue("@Phone", vm.Phone ?? (object)DBNull.Value);
                    upd.Parameters.AddWithValue("@JoinDate", vm.JoinDate);
                    upd.Parameters.AddWithValue("@ExitDate", vm.ExitDate.HasValue ? (object)vm.ExitDate.Value : DBNull.Value);
                    var affected = await upd.ExecuteNonQueryAsync();
                    if (affected == 0) return NotFound();

                    TempData["Success"] = "Employee updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (SqlException ex)
                {
                    ModelState.AddModelError(string.Empty, "Database error while updating employee: " + ex.Message);
                    return View("Edit", vm);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Unexpected error: " + ex.Message);
                    return View("Edit", vm);
                }
            }
        }

        // GET: Employees_Ado/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();
            EmployeeViewModel vm = null;
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"SELECT Id, Name, Ssn, Dob, Address, City, State, Zip, Phone, JoinDate, ExitDate FROM Employees WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id.Value);
                using var rdr = await cmd.ExecuteReaderAsync();
                if (await rdr.ReadAsync())
                {
                    vm = new EmployeeViewModel
                    {
                        Id = rdr.GetInt32(rdr.GetOrdinal("Id")),
                        Name = rdr["Name"] as string,
                        Ssn = rdr["Ssn"] as string,
                        Dob = rdr.GetDateTime(rdr.GetOrdinal("Dob")),
                        Address = rdr["Address"] as string,
                        City = rdr["City"] as string,
                        State = rdr["State"] as string,
                        Zip = rdr["Zip"] as string,
                        Phone = rdr["Phone"] as string,
                        JoinDate = rdr.GetDateTime(rdr.GetOrdinal("JoinDate")),
                        ExitDate = rdr.IsDBNull(rdr.GetOrdinal("ExitDate")) ? (DateTime?)null : rdr.GetDateTime(rdr.GetOrdinal("ExitDate"))
                    };
                }
            }
            if (vm == null) return NotFound();
            return View("Delete", vm);
        }

        // POST: Employees_Ado/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                try
                {
                    using var del = new SqlCommand("DELETE FROM Employees WHERE Id = @id", conn);
                    del.Parameters.AddWithValue("@id", id);
                    var affected = await del.ExecuteNonQueryAsync();
                    if (affected == 0) return NotFound();

                    TempData["Success"] = "Employee deleted successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (SqlException ex)
                {
                    TempData["Error"] = "Database error while deleting employee: " + ex.Message;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Unexpected error: " + ex.Message;
                    return RedirectToAction(nameof(Index));
                }
            }
        }
    }
}
