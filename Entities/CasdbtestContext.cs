using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace budget_management_system_aspdotnetcore.Entities;

public partial class CasdbtestContext : DbContext
{
    public CasdbtestContext()
    {
    }

    public CasdbtestContext(DbContextOptions<CasdbtestContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments { get; set; }

    public DbSet<SpeedType> SpeedTypes { get; set; }

    public DbSet<DepartmentSpeedType> DepartmentSpeedTypes { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<BudgetAmendment> BudgetAmendments { get; set; }

    public DbSet<BudgetAmendmentSetting> BudgetAmendmentSettings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
/*        #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.*/
        => optionsBuilder.UseSqlServer("Server=172.25.17.38;Database=CASDBTEST;User Id=adm-db;Password=the2Db1737;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);

        // Configure the table if needed
        modelBuilder.Entity<BudgetAmendmentSetting>()
            .ToTable("BudgetAmendmentSettings")
            .HasKey(b => b.Id);

        modelBuilder.Entity<BudgetAmendment>()
            .Property(b => b.Status)
            .HasConversion<string>();

        OnModelCreatingPartial(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.HireDate).IsRequired();
            entity.Property(e => e.JobTitle).HasMaxLength(50);
            entity.Property(e => e.Salary).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<User>()
            .Property(u => u.Status)
            .HasConversion<string>(); // Store enum as string in the database

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>(); // Store enum as string in the database

        modelBuilder.Entity<DepartmentSpeedType>()
            .HasKey(ds => new { ds.DepartmentId, ds.SpeedTypeId }); // Composite Key

        modelBuilder.Entity<DepartmentSpeedType>()
            .HasOne(ds => ds.Department)
            .WithMany(d => d.DepartmentSpeedTypes)
            .HasForeignKey(ds => ds.DepartmentId);

        modelBuilder.Entity<DepartmentSpeedType>()
            .HasOne(ds => ds.SpeedType)
            .WithMany(st => st.DepartmentSpeedTypes)
            .HasForeignKey(ds => ds.SpeedTypeId);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
