using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Bicycle> Bicycles => Set<Bicycle>();
        public DbSet<RouteEntity> Routes => Set<RouteEntity>();
        public DbSet<DayEntity> Days => Set<DayEntity>();
        public DbSet<Substituted> Substituteds { get; set; }

        public DbSet<Shift> ListOfShift => Set<Shift>();
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<WorkHoursInMonth> WorkHoursInMonths => Set<WorkHoursInMonth>();

        // keyless read-model til GetMonthlyHours
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Employee ---------------------------------------------------------
            modelBuilder.Entity<Employee>(e =>
            {
                e.ToTable("Employees");
                e.HasKey(x => x.EmployeeId);
                e.Property(x => x.EmployeeId).HasColumnName("employeeId");
                e.Property(x => x.FirstName).HasColumnName("firstName");
                e.Property(x => x.LastName).HasColumnName("lastName");
                e.Property(x => x.Address).HasColumnName("address");
                e.Property(x => x.Phone).HasColumnName("phone");
                e.Property(x => x.Email).HasColumnName("email");
                e.Property(x => x.ExperienceLevel).HasColumnName("experienceLevel");
            });

            // Route ------------------------------------------------------------
            modelBuilder.Entity<RouteEntity>(e =>
            {
                e.ToTable("Bicycles");
                e.HasKey(b => b.BicycleId);
                
                e.Property(b => b.BicycleId).HasColumnName("id");
                e.Property(b => b.BicycleNumber).HasColumnName("bicycleNumber");
                e.Property(b => b.InOperate).HasColumnName("inOperate");
            });


            // Routes
            modelBuilder.Entity<RouteEntity>(e =>
            {
                e.ToTable("Route");
                e.HasKey(r => r.RouteNumberId);

                // Map C# RouteNumberId -> SQL 'id'
                e.Property(r => r.RouteNumberId).HasColumnName("id");

                // Map C# RouteNumber -> SQL 'routeNumber'
                e.Property(r => r.RouteNumber).HasColumnName("routeNumber");
            });


            // Days
            modelBuilder.Entity<DayEntity>(e =>
            {
                e.ToTable("Days");
                e.HasKey(d => d.DayId);
                e.Property(d => d.DayId).HasColumnName("dayId");
                e.Property(d => d.Day).HasColumnName("day");
            });

            // Substituted ------------------------------------------------------
            modelBuilder.Entity<Substituted>(e =>
            {
                e.ToTable("Substituteds");
                e.HasKey(s => s.SubstitutedId);
                e.Property(s => s.SubstitutedId).HasColumnName("substitutedId");
                e.Property(s => s.EmployeeId).HasColumnName("employeeId");
                e.Property(s => s.HasSubstituted).HasColumnName("hasSubstituted");
            });

            // Shifts / ListOfShift
            modelBuilder.Entity<Shift>(e =>
            {
                e.ToTable("ListOfShift");
                e.HasKey(x => x.ShiftId);

                e.Property(x => x.ShiftId).HasColumnName("shiftId");
                e.Property(x => x.DateOfShift).HasColumnName("dateOfShift");
                e.Property(x => x.EmployeeId).HasColumnName("employeeId");
                e.Property(x => x.BicycleId).HasColumnName("bicycleId");
                e.Property(x => x.SubstitutedId).HasColumnName("substitutedId");
                e.Property(x => x.RouteId).HasColumnName("routeId");
                e.Property(x => x.StartTime).HasColumnName("startTime");
                e.Property(x => x.EndTime).HasColumnName("endTime");
                e.Property(x => x.TotalHours).HasColumnName("totalHours");
            });


            // WorkHoursInMonths
            modelBuilder.Entity<WorkHoursInMonth>(e =>
            {
                e.ToTable("WorkHoursInMonths");
                e.HasKey(w => w.WorkHoursInMonthId);
                e.Property(w => w.WorkHoursInMonthId).HasColumnName("workHoursInMonthId");
                e.Property(w => w.EmployeeId).HasColumnName("employeeId");
                e.Property(w => w.PayrollYear).HasColumnName("payrollYear");
                e.Property(w => w.PayrollMonth).HasColumnName("payrollMonth");
                e.Property(w => w.PeriodStart).HasColumnName("periodStart");
                e.Property(w => w.PeriodEnd).HasColumnName("periodEnd");
                e.Property(w => w.TotalHours).HasColumnName("totalHours");
                e.Property(w => w.HasSubstituted).HasColumnName("hasSubstituted");
            });

        }
    }
}
