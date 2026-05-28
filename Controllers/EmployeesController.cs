using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmpMaster.DBEntities;
using EmpMaster.Models;

namespace EmpMaster.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly EmployeeMasterContext _context;

        public EmployeesController(EmployeeMasterContext context)
        {
            _context = context;
        }

        // GET: Employees
        public async Task<IActionResult> Index(string? searchString, string? sortOrder, int page = 1, int pageSize = 10)
        {
            // sortOrder values: name_asc, name_desc, title_asc, title_desc, salary_asc, salary_desc
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewData["TitleSortParm"] = sortOrder == "title_asc" ? "title_desc" : "title_asc";
            ViewData["SalarySortParm"] = sortOrder == "salary_asc" ? "salary_desc" : "salary_asc";

            var today = DateOnly.FromDateTime(DateTime.Today);

            // Load employees with related salaries
            var items = await _context.Employees
                .Include(e => e.EmployeeSalaries)
                .AsNoTracking()
                .ToListAsync();

            // Map and compute current title/salary
            var model = items.Select(e =>
            {
                var current = e.EmployeeSalaries
                    .Where(s => s.FromDate <= today && (s.ToDate == null || s.ToDate >= today))
                    .OrderByDescending(s => s.FromDate)
                    .FirstOrDefault();
                var vm = ToViewModel(e);
                vm.CurrentTitle = current?.Title;
                vm.CurrentSalary = current?.Salary;
                return vm;
            }).ToList();

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

            return View(paged);
        }

        // GET: Employees/TitleList
        public async Task<IActionResult> TitleList(int page = 1, int pageSize = 20)
        {
            var query = _context.EmployeeSalaries.AsNoTracking()
                .GroupBy(s => s.Title)
                .Select(g => new TitleViewModel
                {
                    Title = g.Key,
                    MinSalary = g.Min(s => s.Salary),
                    MaxSalary = g.Max(s => s.Salary)
                })
                .OrderBy(t => t.Title);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Clamp(page, 1, Math.Max(1, totalPages));

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["PageSize"] = pageSize;

            return View(items);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return BadRequest();
            var employee = await _context.Employees.FindAsync(id.Value);
            if (employee == null) return NotFound();
            return View(ToViewModel(employee));
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            return View(new EmployeeViewModel());
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Check duplicate SSN
            if (await _context.Employees.AnyAsync(x => x.Ssn == vm.Ssn))
            {
                ModelState.AddModelError(nameof(vm.Ssn), "An employee with this SSN already exists.");
                return View(vm);
            }

            var entity = ToEntity(vm);
            try
            {
                _context.Add(entity);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Employee created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "Database error while creating employee: " + ex.Message);
                return View(vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Unexpected error: " + ex.Message);
                return View(vm);
            }
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return BadRequest();
            var employee = await _context.Employees.FindAsync(id.Value);
            if (employee == null) return NotFound();
            return View(ToViewModel(employee));
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var entity = await _context.Employees.FindAsync(id);
            if (entity == null) return NotFound();

            // Check duplicate SSN for other records
            if (await _context.Employees.AnyAsync(x => x.Ssn == vm.Ssn && x.Id != id))
            {
                ModelState.AddModelError(nameof(vm.Ssn), "Another employee with this SSN already exists.");
                return View(vm);
            }

            ToEntity(vm, entity);

            try
            {
                _context.Update(entity);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Employee updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await EmployeeExists(vm.Id)) return NotFound();
                ModelState.AddModelError(string.Empty, "The record was modified by another user. Please reload and try again.");
                return View(vm);
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "Database error while updating employee: " + ex.Message);
                return View(vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Unexpected error: " + ex.Message);
                return View(vm);
            }
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();
            var employee = await _context.Employees.FindAsync(id.Value);
            if (employee == null) return NotFound();
            return View(ToViewModel(employee));
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();
            try
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Employee deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
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

        private EmployeeViewModel ToViewModel(Employee e)
        {
            return new EmployeeViewModel
            {
                Id = e.Id,
                Name = e.Name,
                Ssn = e.Ssn,
                Dob = e.Dob.ToDateTime(TimeOnly.MinValue),
                Address = e.Address,
                City = e.City,
                State = e.State,
                Zip = e.Zip,
                Phone = e.Phone,
                JoinDate = e.JoinDate.ToDateTime(TimeOnly.MinValue),
                ExitDate = e.ExitDate.HasValue ? e.ExitDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                CurrentTitle = null,
                CurrentSalary = null
            };
        }

        private Employee ToEntity(EmployeeViewModel vm, Employee? entity = null)
        {
            var e = entity ?? new Employee();
            e.Name = vm.Name;
            e.Ssn = vm.Ssn;
            e.Dob = DateOnly.FromDateTime(vm.Dob);
            e.Address = vm.Address;
            e.City = vm.City;
            e.State = vm.State;
            e.Zip = vm.Zip;
            e.Phone = vm.Phone;
            e.JoinDate = DateOnly.FromDateTime(vm.JoinDate);
            e.ExitDate = vm.ExitDate.HasValue ? DateOnly.FromDateTime(vm.ExitDate.Value) : null;
            return e;
        }

        private async Task<bool> EmployeeExists(int id)
        {
            return await _context.Employees.AnyAsync(e => e.Id == id);
        }
    }
}
