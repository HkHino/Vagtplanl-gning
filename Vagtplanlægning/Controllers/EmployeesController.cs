using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.Data;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Models;
using System.Text.RegularExpressions;

namespace Vagtplanlægning.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _db;

    public EmployeesController(AppDbContext db)
    {
        _db = db;
    }

    // GET: api/employees
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
    {
        var employees = await _db.Employees
            .AsNoTracking()
            .Select(e => new EmployeeDto
            {
                EmployeeId = e.EmployeeId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Address = e.Address,
                Phone = e.Phone,
                Email = e.Email
            })
            .ToListAsync();

        return Ok(employees);
    }

    // GET: api/employees/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var employee = await _db.Employees
            .AsNoTracking()
            .Where(e => e.EmployeeId == id)
            .Select(e => new EmployeeDto
            {
                EmployeeId = e.EmployeeId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Address = e.Address,
                Phone = e.Phone,
                Email = e.Email
            })
            .FirstOrDefaultAsync();

        if (employee == null)
            return NotFound(new { error = $"Employee with id {id} not found." });

        return Ok(employee);
    }

    // POST: api/employees
    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
    {
        try
        {
            if (dto == null)
            {
                return BadRequest(new
                {
                    error = "Request body is missing or invalid."
                });
            }

            // Basic input-validering
            if (string.IsNullOrWhiteSpace(dto.FirstName))
            {
                return BadRequest(new { error = "FirstName is required." });
            }

            if (string.IsNullOrWhiteSpace(dto.LastName))
            {
                return BadRequest(new { error = "LastName is required." });
            }

            if (string.IsNullOrWhiteSpace(dto.Phone))
            {
                return BadRequest(new { error = "Phone is required." });
            }

            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest(new { error = "Email is required." });
            }

            var trimmedEmail = dto.Email.Trim();
            var trimmedPhone = dto.Phone.Trim();

            // --- Telefon-validering ---
            // Tilladt:
            //  - 8 cifre: "12345678"
            //  - "+45" efterfulgt af 8 cifre: "+4512345678"
            bool phoneValid =
                Regex.IsMatch(trimmedPhone, @"^[0-9]{8}$") ||
                Regex.IsMatch(trimmedPhone, @"^\+45[0-9]{8}$");

            if (!phoneValid)
            {
                return BadRequest(new
                {
                    error = "Phone must be 8 digits, or '+45' followed by 8 digits."
                });
            }

            // --- Email-validering ---
            // Simpelt mønster: noget@noget.noget
            bool emailValid = Regex.IsMatch(
                trimmedEmail,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$"
            );

            if (!emailValid)
            {
                return BadRequest(new
                {
                    error = "Email must be in the form 'name@example.com'."
                });
            }

            // Tjek om email allerede findes
            var emailExists = await _db.Employees
                .AnyAsync(e => e.Email == trimmedEmail);

            if (emailExists)
            {
                return Conflict(new
                {
                    error = $"An employee with email '{trimmedEmail}' already exists."
                });
            }

            var entity = new Employee
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Address = dto.Address?.Trim(),
                Phone = trimmedPhone,
                Email = trimmedEmail,
                ExperienceLevel = 1 // alle starter på level 1
            };

            _db.Employees.Add(entity);
            await _db.SaveChangesAsync();

            var result = new EmployeeDto
            {
                EmployeeId = entity.EmployeeId,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Address = entity.Address,
                Phone = entity.Phone,
                Email = entity.Email,
                ExperienceLevel = entity.ExperienceLevel
            };

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.EmployeeId },
                result);
        }
        catch (MySqlConnector.MySqlException ex)
            when (ex.ErrorCode == MySqlConnector.MySqlErrorCode.DuplicateKeyEntry)
        {
            return Conflict(new
            {
                error = "An employee with this email already exists.",
                details = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = "Unexpected server error while creating employee.",
                details = ex.Message
            });
        }
    }




    // PUT: api/employees/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateEmployeeDto dto)
    {
        var entity = await _db.Employees.FindAsync(id);
        if (entity == null)
            return NotFound(new { error = $"Employee with id {id} not found." });

        entity.FirstName = dto.FirstName;
        entity.LastName = dto.LastName;
        entity.Address = dto.Address;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.ExperienceLevel = dto.ExperienceLevel;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/employees/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Employees.FindAsync(id);
        if (entity == null)
            return NotFound(new { error = $"Employee with id {id} not found." });

        _db.Employees.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
