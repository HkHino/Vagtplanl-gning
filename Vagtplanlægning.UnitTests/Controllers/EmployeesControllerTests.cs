using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;      
using Moq;
using Vagtplanlægning.Controllers;
using Vagtplanlægning.Models;
using Vagtplanlægning.Repositories;
using Xunit;


namespace Vagtplanlægning.UnitTests.Controllers
{
    public class EmployeesControllerTests
    {
        private readonly EmployeesController _controller;
        private readonly Mock<IEmployeeRepository> _repoMock;
        private readonly Mock<ILogger<EmployeesController>> _loggerMock;

        public EmployeesControllerTests()
        {
            _repoMock = new Mock<IEmployeeRepository>();
            _loggerMock = new Mock<ILogger<EmployeesController>>();

            _controller = new EmployeesController(_repoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithList()
        {
            var employees = new List<Employee>
            {
                new Employee { EmployeeId = 1, FirstName = "Alice", LastName = "Tester" },
                new Employee { EmployeeId = 2, FirstName = "Bob",   LastName = "Dev" }
            };

            _repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(employees);

            var result = await _controller.GetAll(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(ok.Value);

            _repoMock.Verify(
                r => r.GetAllAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_UnknownId_ReturnsNotFound()
        {
            _repoMock
                .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Employee?)null);

            var result = await _controller.GetById(999, CancellationToken.None);

            Assert.IsType<NotFoundObjectResult>(result.Result);

            _repoMock.Verify(
                r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ExistingEmployee_ReturnsOk()
        {
            var emp = new Employee
            {
                EmployeeId = 1,
                FirstName = "Alice",
                LastName = "Tester"
            };

            _repoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(emp);

            var result = await _controller.GetById(1, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(ok.Value);

            _repoMock.Verify(
                r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
