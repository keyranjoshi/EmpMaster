using System;
using System.Collections.Generic;

namespace EmpMaster.DBEntities;

public partial class Employee
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Ssn { get; set; } = null!;

    public DateOnly Dob { get; set; }

    public string Address { get; set; } = null!;

    public string City { get; set; } = null!;

    public string State { get; set; } = null!;

    public string Zip { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public DateOnly JoinDate { get; set; }

    public DateOnly? ExitDate { get; set; }

    public virtual ICollection<EmployeeSalary> EmployeeSalaries { get; set; } = new List<EmployeeSalary>();
}
