using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // employees
        public DbSet<Employee> Employees { get; set; }

        // routes – expose both singular and plural, to satisfy existing code
        public DbSet<RouteEntity> Route { get; set; }
        public DbSet<RouteEntity> Routes { get; set; }

        // bicycles
        public DbSet<Bicycle> Bicycles { get; set; }

        // days
        public DbSet<DayEntity> Days { get; set; }

        // shifts
        public DbSet<Shift> ListOfShift { get; set; }

        // substituteds
        public DbSet<Substituted> Substituteds { get; set; }

        // monthly/work hours – again expose both
        public DbSet<WorkHoursInMonth> WorkHoursInMonths { get; set; }
        public DbSet<WorkHoursInMonth> MonthlyHours { get; set; }

        // shift plans
        public DbSet<ShiftPlanRow> ShiftPlans { get; set; }  // the “row” version with JSON

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
                e.ToTable("Route");
                e.HasKey(r => r.RouteNumberId);
                e.Property(r => r.RouteNumberId).HasColumnName("routeNumberId");
            });

            // Bicycle ----------------------------------------------------------
            modelBuilder.Entity<Bicycle>(e =>
            {
                e.ToTable("Bicycles");
                e.HasKey(b => b.BicycleId);
                e.Property(b => b.BicycleId).HasColumnName("bicycleId");
                e.Property(b => b.InOperate).HasColumnName("inOperate");
            });

            // Day --------------------------------------------------------------
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

            // Shift (ListOfShift) ----------------------------------------------
            modelBuilder.Entity<Shift>(e =>
            {
                e.ToTable("ListOfShift");
                e.HasKey(s => s.ShiftId);
                e.Property(s => s.ShiftId).HasColumnName("shiftId");
                e.Property(s => s.DayId).HasColumnName("dayId");
                e.Property(s => s.EmployeeId).HasColumnName("employeeId");
                e.Property(s => s.BicycleId).HasColumnName("bicycleId");
                e.Property(s => s.SubstitutedId).HasColumnName("substitutedId");
                e.Property(s => s.RouteNumberId).HasColumnName("routeNumberId");
                e.Property(s => s.MeetInTime).HasColumnName("meetInTime");
                e.Property(s => s.StartTime).HasColumnName("startTime");
                e.Property(s => s.EndTime).HasColumnName("endTime");
                e.Property(s => s.TotalHours).HasColumnName("totalHours");
            });

            // WorkHoursInMonth -------------------------------------------------
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

            modelBuilder.Entity<ShiftPlanRow>(e =>
            {
                e.ToTable("ShiftPlans");
                e.HasKey(x => x.ShiftPlanId);
                e.Property(x => x.ShiftPlanId).HasColumnName("shiftPlanId");
                e.Property(x => x.Name).HasColumnName("name");
                e.Property(x => x.StartDate).HasColumnName("startDate");
                e.Property(x => x.EndDate).HasColumnName("endDate");
                e.Property(x => x.ShiftsJson).HasColumnName("shifts");
            });

        }
    }
}
