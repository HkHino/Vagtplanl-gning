using Neo4j.Driver;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories
{
    public class Neo4jEmployeeRepository : IEmployeeRepository
    {
        private readonly IDriver _driver;

        public Neo4jEmployeeRepository(IDriver driver)
        {
            _driver = driver;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct = default)
        {
            var session = _driver.AsyncSession();
            try
            {
                var cursor = await session.RunAsync(
                    "MATCH (e:Employee) RETURN e ORDER BY e.employeeId");
                var records = await cursor.ToListAsync();
                return records.Select(r =>
                {
                    var node = r["e"].As<INode>();
                    return MapNodeToEmployee(node);
                });
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var session = _driver.AsyncSession();
            try
            {
                var cursor = await session.RunAsync(
                    "MATCH (e:Employee {employeeId: $id}) RETURN e", new { id });

                var records = await cursor.ToListAsync();
                var record = records.FirstOrDefault();
                if (record == null) return null;

                var node = record["e"].As<INode>();
                return MapNodeToEmployee(node);
            }
            finally
            {
                await session.CloseAsync();
            }
        }


        public async Task AddAsync(Employee employee, CancellationToken ct = default)
        {
            var session = _driver.AsyncSession();
            try
            {
                await session.RunAsync(
                    @"CREATE (e:Employee {
                        employeeId: $employeeId,
                        firstName: $firstName,
                        lastName: $lastName,
                        address: $address,
                        phone: $phone,
                        email: $email
                    })",
                    new
                    {
                        employeeId = employee.EmployeeId,
                        firstName = employee.FirstName,
                        lastName = employee.LastName,
                        address = employee.Address,
                        phone = employee.Phone,
                        email = employee.Email
                    });
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task UpdateAsync(Employee employee, CancellationToken ct = default)
        {
            var session = _driver.AsyncSession();
            try
            {
                await session.RunAsync(
                    @"MATCH (e:Employee {employeeId: $employeeId})
                      SET e.firstName = $firstName,
                          e.lastName  = $lastName,
                          e.address   = $address,
                          e.phone     = $phone,
                          e.email     = $email",
                    new
                    {
                        employeeId = employee.EmployeeId,
                        firstName = employee.FirstName,
                        lastName = employee.LastName,
                        address = employee.Address,
                        phone = employee.Phone,
                        email = employee.Email
                    });
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var session = _driver.AsyncSession();
            try
            {
                var cursor = await session.RunAsync(
                    "MATCH (e:Employee {employeeId: $id}) DETACH DELETE e RETURN count(e) AS deleted",
                    new { id });
                var record = await cursor.SingleAsync();
                var deleted = record["deleted"].As<long>();
                return deleted > 0;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        private static Employee MapNodeToEmployee(INode node)
        {
            // defensive: Neo4j props are dynamic
            return new Employee
            {
                EmployeeId = node.Properties.TryGetValue("employeeId", out var idVal) ? Convert.ToInt32(idVal) : 0,
                FirstName = node.Properties.TryGetValue("firstName", out var f) ? f?.ToString() ?? "" : "",
                LastName = node.Properties.TryGetValue("lastName", out var l) ? l?.ToString() ?? "" : "",
                Address = node.Properties.TryGetValue("address", out var a) ? a?.ToString() ?? "" : "",
                Phone = node.Properties.TryGetValue("phone", out var p) ? p?.ToString() ?? "" : "",
                Email = node.Properties.TryGetValue("email", out var e) ? e?.ToString() ?? "" : "",
            };
        }
    }
}
