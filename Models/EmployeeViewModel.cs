using System;
using System.ComponentModel.DataAnnotations;

namespace EmpMaster.Models
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(11)]
        public string Ssn { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Dob { get; set; }

        [Required]
        [StringLength(255)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Zip { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime JoinDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ExitDate { get; set; }
        // Current title and salary (read-only fields shown in list)
        public string? CurrentTitle { get; set; }
        public decimal? CurrentSalary { get; set; }
    }
}
