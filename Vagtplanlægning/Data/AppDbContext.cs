using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Models;
namespace Vagtplanlægning.Data;

/// <summary>
/// Entity Framework Core database context for the MySQL schema defined in cykelBudDB.sql.
/// This context only models the tables we actually use from the API:
/// - Bicycles
/// - Employees
/// - Substituteds
/// - Routes
/// - ListOfShift (as Shift)
/// - WorkHoursInMonths
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // DbSets -----------------------------------------------------------------

    public DbSet<Bicycle> Bicycles => Set<Bicycle>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Substituted> Substituteds => Set<Substituted>();
    public DbSet<RouteEntity> Routes => Set<RouteEntity>();
    public DbSet<ShiftPlanRow> ShiftPlans => Set<ShiftPlanRow>();
    

    /// <summary>
    /// Represents rows in the ListOfShift table.
    /// </summary>
    public DbSet<Shift> ListOfShift => Set<Shift>();

    public DbSet<WorkHoursInMonth> WorkHoursInMonths => Set<WorkHoursInMonth>();

    // Model configuration -----------------------------------------------------

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Bicycles ------------------------------------------------------------
        modelBuilder.Entity<Bicycle>(e =>
        {
            e.ToTable("Bicycles");

            e.HasKey(x => x.BicycleId);

            e.Property(x => x.BicycleId)
                .HasColumnName("id");

            e.Property(x => x.BicycleNumber)
                .HasColumnName("bicycleNumber");

            e.Property(x => x.InOperate)
                .HasColumnName("inOperate");
        });

        // Employees -----------------------------------------------------------
        modelBuilder.Entity<Employee>(e =>
        {
            e.ToTable("Employees");

            e.HasKey(x => x.EmployeeId);

            e.Property(x => x.EmployeeId)
                .HasColumnName("employeeId");

            e.Property(x => x.FirstName)
                .HasColumnName("firstName")
                .HasMaxLength(255);

            e.Property(x => x.LastName)
                .HasColumnName("lastName")
                .HasMaxLength(255);

            e.Property(x => x.Address)
                .HasColumnName("address")
                .HasMaxLength(255);

            e.Property(x => x.Phone)
                .HasColumnName("phone")
                .HasMaxLength(20);

            e.Property(x => x.Email)
                .HasColumnName("email")
                .HasMaxLength(255);

            e.Property(x => x.ExperienceLevel)
                .HasColumnName("experienceLevel");
        });

        // Substituteds --------------------------------------------------------
        modelBuilder.Entity<Substituted>(e =>
        {
            e.ToTable("Substituteds");

            e.HasKey(x => x.SubstitutedId);

            e.Property(x => x.SubstitutedId)
                .HasColumnName("substitutedId");

            e.Property(x => x.EmployeeId)
                .HasColumnName("employeeId");

            e.Property(x => x.HasSubstituted)
                .HasColumnName("hasSubstituted");

            e.HasOne(x => x.Employee)
             .WithMany(e => e.SubstitutionRecords)
             .HasForeignKey(x => x.EmployeeId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Routes --------------------------------------------------------------
        modelBuilder.Entity<RouteEntity>(e =>
        {
            e.ToTable("Routes");
            e.HasKey(r => r.Id);

            e.Property(r => r.Id).HasColumnName("id");
            e.Property(r => r.RouteNumber).HasColumnName("routeNumber");

        });


        // Shifts (ListOfShift) -----------------------------------------------
        modelBuilder.Entity<Shift>(e =>
        {
            e.ToTable("ListOfShift");

            e.HasKey(x => x.ShiftId);

            e.Property(x => x.ShiftId)
                .HasColumnName("shiftId");

            e.Property(x => x.DateOfShift)
                .HasColumnName("dateOfShift");

            e.Property(x => x.EmployeeId)
                .HasColumnName("employeeId");

            e.Property(x => x.BicycleId)
                .HasColumnName("bicycleId");

            e.Property(x => x.SubstitutedId)
                .HasColumnName("substitutedId");

            e.Property(x => x.RouteId)
                .HasColumnName("routeId");

            e.Property(x => x.StartTime)
                .HasColumnName("startTime");

            e.Property(x => x.EndTime)
                .HasColumnName("endTime");

            e.Property(x => x.TotalHours)
                .HasColumnName("totalHours");

            // Relationships
            e.HasOne(s => s.Employee)
             .WithMany(emp => emp.Shifts)
             .HasForeignKey(s => s.EmployeeId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(s => s.Bicycle)
             .WithMany(b => b.Shifts)
             .HasForeignKey(s => s.BicycleId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(s => s.Substituted)
             .WithMany()
             .HasForeignKey(s => s.SubstitutedId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(s => s.Routes)
             .WithMany(r => r.Shifts)
             .HasForeignKey(s => s.RouteId)
             .OnDelete(DeleteBehavior.Cascade);
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


        // WorkHoursInMonths ---------------------------------------------------
        modelBuilder.Entity<WorkHoursInMonth>(e =>
        {
            e.ToTable("WorkHoursInMonths");

            e.HasKey(x => x.WorkHoursInMonthId);

            e.Property(x => x.WorkHoursInMonthId)
                .HasColumnName("workHoursInMonthId");

            e.Property(x => x.EmployeeId)
                .HasColumnName("employeeId");

            e.Property(x => x.PayrollYear)
                .HasColumnName("payrollYear");

            e.Property(x => x.PayrollMonth)
                .HasColumnName("payrollMonth");

            e.Property(x => x.PeriodStart)
                .HasColumnName("periodStart");

            e.Property(x => x.PeriodEnd)
                .HasColumnName("periodEnd");

            e.Property(x => x.TotalHours)
                .HasColumnName("totalHours");

            e.Property(x => x.HasSubstituted)
                .HasColumnName("hasSubstituted");
        });
    }
}
