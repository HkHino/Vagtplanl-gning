// EmployeesController.cs
// ----------------------
// Thin CRUD controller for managing employees from the admin side of the system.
// It uses IEmployeeRepository for data access and manual mapping between
// domain models (Employee) and DTOs (EmployeeDto, CreateEmployeeDto, UpdateEmployeeDto).


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Models;
using Vagtplanlægning.Repositories;

namespace Vagtplanlægning.Controllers
{    /// <summary>
     /// API controller for the administrative "employee management" part of the system.
     ///
     /// Endpoints in this controller:
     /// - List all employees.
     /// - Get a single employee by id.
     /// - Create a new employee.
     /// - Update an existing employee.
     /// - Delete an employee.
     ///
     /// The controller is intentionally thin:
     /// - Data access is delegated to <see cref="IEmployeeRepository"/>.
     /// - Mapping is done manually between <see cref="Employee"/> and the different DTOs.
     /// </summary>
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

        /// <summary>
        /// Returns all employees.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>HTTP 200 with a list of <see cref="EmployeeDto"/>.</returns>
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

        /// <summary>
        /// Returns a single employee by id.
        /// </summary>
        /// <param name="id">The employee id.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// HTTP 200 with <see cref="EmployeeDto"/> if found,
        /// otherwise HTTP 404.
        /// </returns>
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

        /// <summary>
        /// Creates a new employee.
        /// </summary>
        /// <param name="dto">DTO containing data required to create an employee.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// HTTP 201 with the created <see cref="EmployeeDto"/> if successful.
        /// HTTP 400 if unique constraints (email/phone) are violated.
        /// </returns>
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

        /// <summary>
        /// Updates an existing employee.
        /// </summary>
        /// <param name="id">The id of the employee to update.</param>
        /// <param name="dto">DTO containing the new values for the employee.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// HTTP 204 if update succeeds,
        /// HTTP 404 if employee does not exist,
        /// HTTP 400 if unique constraints are violated.
        /// </returns>
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

        /// <summary>
        /// Deletes an employee by id.
        /// </summary>
        /// <param name="id">The id of the employee to delete.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// HTTP 204 if delete succeeds,
        /// HTTP 404 if employee does not exist.
        /// </returns>
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
