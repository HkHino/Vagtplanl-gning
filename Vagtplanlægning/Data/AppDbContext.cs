using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Bicycle> Bicycles => Set<Bicycle>();
        public DbSet<RouteEntity> Routes => Set<RouteEntity>();
        public DbSet<DayEntity> Days => Set<DayEntity>();
        public DbSet<Substituted> Substituteds => Set<Substituted>();
        public DbSet<Shift> ListOfShift => Set<Shift>();
        public DbSet<WorkHoursInMonth> WorkHoursInMonths => Set<WorkHoursInMonth>();

        // keyless read-model til GetMonthlyHours
        public DbSet<MonthlyHoursRow> MonthlyHours => Set<MonthlyHoursRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Employees
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
            });

            // Bicycles
            modelBuilder.Entity<Bicycle>(e =>
            {
                e.ToTable("Bicycles");
                e.HasKey(x => x.BicycleId);
                e.Property(x => x.BicycleId).HasColumnName("bicycleId");
                e.Property(x => x.InOperate).HasColumnName("inOperate");
            });

            // Route
            modelBuilder.Entity<RouteEntity>(e =>
            {
                e.ToTable("Route");
                e.HasKey(x => x.RouteNumberId);
                e.Property(x => x.RouteNumberId).HasColumnName("routeNumberId");
            });

            // Days
            modelBuilder.Entity<DayEntity>(e =>
            {
                e.ToTable("Days");
                e.HasKey(x => x.DayId);
                e.Property(x => x.DayId).HasColumnName("dayId");
                e.Property(x => x.Day).HasColumnName("day");
            });

            // Substituteds
            modelBuilder.Entity<Substituted>(e =>
            {
                e.ToTable("Substituteds");
                e.HasKey(x => x.SubstitutedId);
                e.Property(x => x.SubstitutedId).HasColumnName("substitutedId");
                e.Property(x => x.EmployeeId).HasColumnName("employeeId");
                e.Property(x => x.HasSubstituted).HasColumnName("hasSubstituted");
            });

            // ListOfShift
            modelBuilder.Entity<Shift>(e =>
            {
                e.ToTable("ListOfShift");
                e.HasKey(x => x.ShiftId);
                e.Property(x => x.ShiftId).HasColumnName("shiftId");
                e.Property(x => x.DayId).HasColumnName("dayId");
                e.Property(x => x.EmployeeId).HasColumnName("employeeId");
                e.Property(x => x.BicycleId).HasColumnName("bicycleId");
                e.Property(x => x.SubstitutedId).HasColumnName("substitutedId");
                e.Property(x => x.RouteNumberId).HasColumnName("routeNumberId");
                e.Property(x => x.MeetInTime).HasColumnName("meetInTime");
                e.Property(x => x.StartTime).HasColumnName("startTime");
                e.Property(x => x.EndTime).HasColumnName("endTime");
                e.Property(x => x.TotalHours).HasColumnName("totalHours");
            });

            // WorkHoursInMonths
            modelBuilder.Entity<WorkHoursInMonth>(e =>
            {
                e.ToTable("WorkHoursInMonths");
                e.HasKey(x => x.WorkHoursInMonthId);
                e.Property(x => x.WorkHoursInMonthId).HasColumnName("workHoursInMonthId");
                e.Property(x => x.EmployeeId).HasColumnName("employeeId");
                e.Property(x => x.PayrollYear).HasColumnName("payrollYear");
                e.Property(x => x.PayrollMonth).HasColumnName("payrollMonth");
                e.Property(x => x.PeriodStart).HasColumnName("periodStart");
                e.Property(x => x.PeriodEnd).HasColumnName("periodEnd");
                e.Property(x => x.TotalHours).HasColumnName("totalHours");
                e.Property(x => x.HasSubstituted).HasColumnName("hasSubstituted");
            });

            // MonthlyHoursRow (keyless)
            modelBuilder.Entity<MonthlyHoursRow>(e =>
            {
                e.HasNoKey();
                e.ToView(null); // ikke bundet til view
                e.Property(x => x.EmployeeId).HasColumnName("employeeId");
                e.Property(x => x.FirstName).HasColumnName("firstName");
                e.Property(x => x.LastName).HasColumnName("lastName");
                e.Property(x => x.Year).HasColumnName("year");
                e.Property(x => x.Month).HasColumnName("month");
                e.Property(x => x.TotalMonthlyHours).HasColumnName("totalMonthlyHours");
                e.Property(x => x.HasSubstituted).HasColumnName("hasSubstituted");
            });
        }
    }
}
