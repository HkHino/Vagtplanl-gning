using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Models;
using Vagtplanlægning.Repositories;

namespace Vagtplanlægning.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(
            IEmployeeRepository employeeRepo,
            ILogger<EmployeesController> logger)
        {
            _employeeRepo = employeeRepo;
            _logger = logger;
        }

        // GET: api/Employees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll(CancellationToken ct)
        {
            try
            {
                var employees = await _employeeRepo.GetAllAsync(ct);

                var dtos = employees.Select(e => new EmployeeDto
                {
                    EmployeeId = e.EmployeeId,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Address = e.Address,
                    Phone = e.Phone,
                    Email = e.Email,
                    ExperienceLevel = e.ExperienceLevel
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while reading employees.");
                return StatusCode(500, new { error = "Error while reading employees." });
            }
        }

        // GET: api/Employees/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EmployeeDto>> GetById(int id, CancellationToken ct)
        {
            try
            {
                var employee = await _employeeRepo.GetByIdAsync(id, ct);
                if (employee == null)
                    return NotFound(new { error = $"Employee with id {id} not found." });

                var dto = new EmployeeDto
                {
                    EmployeeId = employee.EmployeeId,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Address = employee.Address,
                    Phone = employee.Phone,
                    Email = employee.Email,
                    ExperienceLevel = employee.ExperienceLevel
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while reading employee {EmployeeId}.", id);
                return StatusCode(500, new { error = "Error while reading employee." });
            }
        }

        // POST: api/Employees
        [HttpPost]
        public async Task<ActionResult<EmployeeDto>> Create(CreateEmployeeDto dto, CancellationToken ct)
        {
            try
            {
                var employee = new Employee
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Address = dto.Address,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    ExperienceLevel = 1 // default
                };

                await _employeeRepo.AddAsync(employee, ct);

                var createdDto = new EmployeeDto
                {
                    EmployeeId = employee.EmployeeId,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Address = employee.Address,
                    Phone = employee.Phone,
                    Email = employee.Email,
                    ExperienceLevel = employee.ExperienceLevel
                };

                return CreatedAtAction(nameof(GetById), new { id = employee.EmployeeId }, createdDto);
            }
            catch (DbUpdateException dbEx) when (dbEx.InnerException is MySqlConnector.MySqlException mySqlEx
                                                 && mySqlEx.Message.Contains("Duplicate entry")
                                                 && mySqlEx.Message.Contains("employees.email"))
            {
                return BadRequest(new { error = $"Email '{dto.Email}' already exists." });
            }
            catch (DbUpdateException dbEx) when (dbEx.InnerException is MySqlConnector.MySqlException mySqlEx
                                                 && mySqlEx.Message.Contains("Duplicate entry")
                                                 && mySqlEx.Message.Contains("employees.phone"))
            {
                return BadRequest(new { error = $"Phone '{dto.Phone}' already exists." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating employee.");
                return StatusCode(500, new { error = "Error while creating employee." });
            }
        }


        // PUT: api/Employees/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateEmployeeDto dto, CancellationToken ct)
        {
            try
            {
                var existing = await _employeeRepo.GetByIdAsync(id, ct);
                if (existing == null)
                    return NotFound(new { error = $"Employee with id {id} not found." });

                // Opdater felter
                existing.FirstName = dto.FirstName;
                existing.LastName = dto.LastName;
                existing.Address = dto.Address;
                existing.Phone = dto.Phone;
                existing.Email = dto.Email;
                
                await _employeeRepo.UpdateAsync(existing, ct);

                return NoContent();
            }
            catch (DbUpdateException dbEx) when (
                dbEx.InnerException is MySqlConnector.MySqlException mySqlEx &&
                mySqlEx.Message.Contains("Duplicate entry") &&
                mySqlEx.Message.Contains("employees.email"))
            {
                // E-mailen rammer UNIQUE constraint
                return BadRequest(new
                {
                    error = $"Email '{dto.Email}' already exists."
                });
            }
            catch (DbUpdateException dbEx) when (
                dbEx.InnerException is MySqlConnector.MySqlException mySqlEx &&
                mySqlEx.Message.Contains("Duplicate entry") &&
                mySqlEx.Message.Contains("employees.phone"))
            {
                // E-mailen rammer UNIQUE constraint
                return BadRequest(new
                {
                    error = $"Phone '{dto.Phone}' already exists."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating employee {EmployeeId}.", id);
                return StatusCode(500, new { error = "Error while updating employee." });
            }
        }


        // DELETE: api/Employees/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            try
            {
                var existing = await _employeeRepo.GetByIdAsync(id, ct);
                if (existing == null)
                    return NotFound(new { error = $"Employee with id {id} not found." });

                await _employeeRepo.DeleteAsync(id, ct);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting employee {EmployeeId}.", id);
                return StatusCode(500, new { error = "Error while deleting employee." });
            }
        }
    }
}
