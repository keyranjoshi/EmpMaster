using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EmpMaster.DBEntities;

public partial class EmployeeMasterContext : DbContext
{
    public EmployeeMasterContext()
    {
    }

    public EmployeeMasterContext(DbContextOptions<EmployeeMasterContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeSalary> EmployeeSalaries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // If options are already configured (for example via DI in Program.cs), do nothing here.
        // The connection string should come from configuration (appsettings.json) and be
        // provided when registering the DbContext with AddDbContext in Program.cs.
        if (!optionsBuilder.IsConfigured)
        {
#warning To protect potentially sensitive information in your connection string, register the DbContext in Program.cs and read the connection string from configuration.
            // Intentionally left blank to avoid hard-coding the connection string in source code.
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employee");

            entity.HasIndex(e => e.Name, "IDX_Employee_Name");

            entity.HasIndex(e => e.Ssn, "IDX_Employee_SSN");

            entity.HasIndex(e => e.Ssn, "UQ__Employee__CA1E8E3C17BE53A8").IsUnique();

            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Dob).HasColumnName("DOB");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Ssn)
                .HasMaxLength(11)
                .IsUnicode(false)
                .HasColumnName("SSN");
            entity.Property(e => e.State)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Zip)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<EmployeeSalary>(entity =>
        {
            entity.ToTable("EmployeeSalary");

            entity.HasIndex(e => e.EmployeeId, "IDX_Salary_EmpId");

            entity.HasIndex(e => e.FromDate, "IDX_Salary_FromDate");

            entity.Property(e => e.Salary).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeSalaries)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_Salary_Employee");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
