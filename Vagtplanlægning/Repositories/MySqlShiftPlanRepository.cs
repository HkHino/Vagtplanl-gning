using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories;

/// <summary>
/// Repository til ShiftPlans baseret på MySQL / EF Core.
/// Læser og skriver til tabellen ShiftPlans via ShiftPlanRow (EF-entity)
/// og mapper til/fra domænemodellen ShiftPlan.
/// </summary>
public class MySqlShiftPlanRepository : IShiftPlanRepository
{
    private readonly AppDbContext _db;

    public MySqlShiftPlanRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ShiftPlan>> GetAllAsync(CancellationToken ct = default)
    {
        var rows = await _db.ShiftPlans
            .AsNoTracking()
            .ToListAsync(ct);

        return rows.Select(MapToDomain).ToList();
    }

    public async Task<ShiftPlan?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (!Guid.TryParse(id, out var guid))
        {
            // Ugyldigt id-format → ingen plan
            return null;
        }

        var row = await _db.ShiftPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ShiftPlanId == guid, ct);

        return row == null ? null : MapToDomain(row);
    }


    public async Task AddAsync(ShiftPlan plan, CancellationToken ct = default)
    {
        // Sørg for at have et id
        if (string.IsNullOrWhiteSpace(plan.ShiftPlanId))
        {
            plan.ShiftPlanId = Guid.NewGuid().ToString();
        }

        var row = MapToRow(plan);
        _db.ShiftPlans.Add(row);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        if (!Guid.TryParse(id, out var guid))
        {
            return false;
        }

        var row = await _db.ShiftPlans.FindAsync(new object?[] { guid }, ct);
        if (row == null) return false;

        _db.ShiftPlans.Remove(row);
        await _db.SaveChangesAsync(ct);
        return true;
    }


    // ---------------------------------------------------
    // Mapping: Row (EF/DB) -> Domain (ShiftPlan)
    // ---------------------------------------------------
    private static ShiftPlan MapToDomain(ShiftPlanRow row)
    {
        List<Shift> shifts;

        try
        {
            shifts = JsonSerializer.Deserialize<List<Shift>>(row.ShiftsJson) ?? new List<Shift>();
        }
        catch
        {
            shifts = new List<Shift>();
        }

        return new ShiftPlan
        {
            ShiftPlanId = row.ShiftPlanId.ToString(), // Guid -> string
            Name = row.Name,
            StartDate = row.StartDate,
            EndDate = row.EndDate,
            Shifts = shifts
        };
    }


    // ---------------------------------------------------
    // Mapping: Domain (ShiftPlan) -> Row (EF/DB)
    // ---------------------------------------------------
    private static ShiftPlanRow MapToRow(ShiftPlan plan)
    {
        // Lige nu serialiserer vi hele Shift-listen til JSON.
        var json = JsonSerializer.Serialize(plan.Shifts ?? new List<Shift>());

        // Prøv at parse det id, der ligger i domænemodellen.
        // Hvis det ikke er et gyldigt Guid, laver vi bare et nyt.
        Guid id;
        if (!Guid.TryParse(plan.ShiftPlanId, out id))
        {
            id = Guid.NewGuid();
        }

        return new ShiftPlanRow
        {
            ShiftPlanId = id,
            Name = plan.Name,
            StartDate = plan.StartDate.Date,
            EndDate = plan.EndDate.Date,
            ShiftsJson = json
        };
    }

    // ---------------------------------------------------
    // mapping til Update af eksisterende plan navn
    // ---------------------------------------------------
    public async Task UpdateAsync(ShiftPlan plan, CancellationToken ct = default)
    {
        if (!Guid.TryParse(plan.ShiftPlanId, out var guid))
        {
            throw new ArgumentException("ShiftPlanId is not a valid GUID.", nameof(plan));
        }

        var row = await _db.ShiftPlans.FindAsync(new object?[] { guid }, ct);
        if (row == null)
        {
            throw new InvalidOperationException($"ShiftPlan with id '{plan.ShiftPlanId}' not found.");
        }

        row.Name = plan.Name;
        row.StartDate = plan.StartDate;
        row.EndDate = plan.EndDate;
        row.ShiftsJson = JsonSerializer.Serialize(plan.Shifts ?? new List<Shift>());

        await _db.SaveChangesAsync(ct);
    }


}
