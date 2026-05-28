using System;

namespace EmpMaster.Models
{
    public class TitleViewModel
    {
        public string Title { get; set; } = string.Empty;
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
    }
}
