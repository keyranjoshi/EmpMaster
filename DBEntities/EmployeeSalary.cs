using System;
using System.Collections.Generic;

namespace EmpMaster.DBEntities;

public partial class EmployeeSalary
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public DateOnly FromDate { get; set; }

    public DateOnly? ToDate { get; set; }

    public string Title { get; set; } = null!;

    public decimal Salary { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
