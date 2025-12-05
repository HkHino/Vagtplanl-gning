using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Vagtplanlægning.Models;
using Vagtplanlægning.Repositories;

public class EmployeeRepositoryFallback : IEmployeeRepository
{
    private readonly MySqlEmployeeRepository _primary;
    private readonly MongoEmployeeRepository _secondary;
    private readonly ILogger<EmployeeRepositoryFallback> _logger;

    public EmployeeRepositoryFallback(
        MySqlEmployeeRepository primary,
        MongoEmployeeRepository secondary,
        ILogger<EmployeeRepositoryFallback> logger)
    {
        _primary = primary;
        _secondary = secondary;
        _logger = logger;
    }

    private static bool IsTransient(Exception ex)
    {
        if (ex is MySqlException)
            return true;

        if (ex is DbUpdateException dbEx && dbEx.InnerException is MySqlException)
            return true;

        if (ex is InvalidOperationException invEx && invEx.InnerException is MySqlException)
            return true;

        return false;
    }

    private async Task<T> ExecuteWithFallbackAsync<T>(
        Func<IEmployeeRepository, Task<T>> action,
        CancellationToken ct)
    {
        try
        {
            return await action(_primary);
        }
        catch (Exception ex) when (IsTransient(ex))
        {
            _logger.LogWarning(ex, "MySQL unavailable – using MongoDB fallback for employees.");
            return await action(_secondary);
        }
    }

    private async Task ExecuteWithFallbackAsync(
        Func<IEmployeeRepository, Task> action,
        CancellationToken ct)
    {
        try
        {
            await action(_primary);
        }
        catch (Exception ex) when (IsTransient(ex))
        {
            _logger.LogWarning(ex, "MySQL unavailable – using MongoDB fallback for employees.");
            await action(_secondary);
        }
    }

    public Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct = default) =>
        ExecuteWithFallbackAsync(repo => repo.GetAllAsync(ct), ct);

    public Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default) =>
        ExecuteWithFallbackAsync(repo => repo.GetByIdAsync(id, ct), ct);

    public Task AddAsync(Employee employee, CancellationToken ct = default) =>
        ExecuteWithFallbackAsync(repo => repo.AddAsync(employee, ct), ct);

    public Task UpdateAsync(Employee employee, CancellationToken ct = default) =>
        ExecuteWithFallbackAsync(repo => repo.UpdateAsync(employee, ct), ct);

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default) =>
        ExecuteWithFallbackAsync(repo => repo.DeleteAsync(id, ct), ct);

    public Task<bool> EmailInUse(string email, CancellationToken ct = default) =>
        ExecuteWithFallbackAsync(repo => repo.EmailInUse(email, ct), ct);
}
