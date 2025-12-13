using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vagtplanlægning.Controllers;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Models;
using Vagtplanlægning.Repositories;
using Vagtplanlægning.Services;
using System.Linq;
using Xunit;


namespace Vagtplanlægning.UnitTests.Controllers
{
    public class ShiftControllerTests
    {
        private readonly Mock<IShiftRepository> _shiftRepo;
        private readonly Mock<IShiftExecutionService> _shiftExec;
        private readonly Mock<IEmployeeRepository> _employeeRepo;
        private readonly Mock<IBicycleRepository> _bicycleRepo;
        private readonly Mock<IRouteRepository> _routeRepo;
        private readonly Mock<IMapper> _mapper = new();

        private readonly ShiftController _controller;

        public ShiftControllerTests()
        {
            _shiftRepo = new Mock<IShiftRepository>();
            _shiftExec = new Mock<IShiftExecutionService>();
            _employeeRepo = new Mock<IEmployeeRepository>();
            _bicycleRepo = new Mock<IBicycleRepository>();
            _routeRepo = new Mock<IRouteRepository>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(typeof(Program));
            });
            

            _controller = new ShiftController(
                _shiftRepo.Object,
                _shiftExec.Object,
                _employeeRepo.Object,
                _bicycleRepo.Object,
                _routeRepo.Object,
                new Mapper(config)
            );
        }

        // ------------------------------------------------------------
        // SetStart: ukendt shift -> NotFound, og service må IKKE blive kaldt
        // ------------------------------------------------------------
        [Fact]
        public async Task SetStart_UnknownShift_ReturnsNotFound()
        {
            _shiftRepo
                .Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Shift?)null);

            var result = await _controller.SetStart(42, TimeSpan.FromHours(8));

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("not found", notFound.Value!.ToString());

            _shiftExec.Verify(
                s => s.SetStartTimeAsync(
                    It.IsAny<int>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // ------------------------------------------------------------
        // SetStart: eksisterende shift -> NoContent og service bliver kaldt
        // ------------------------------------------------------------
        [Fact]
        public async Task SetStart_ExistingShift_ReturnsNoContent()
        {
            _shiftRepo
                .Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Shift { ShiftId = 10 });

            var result = await _controller.SetStart(10, TimeSpan.FromHours(8));

            Assert.IsType<NoContentResult>(result);

            _shiftExec.Verify(
                s => s.SetStartTimeAsync(
                    10,
                    TimeSpan.FromHours(8),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ------------------------------------------------------------
        // SetEnd: ukendt shift -> NotFound
        // ------------------------------------------------------------
        [Fact]
        public async Task SetEnd_UnknownShift_ReturnsNotFound()
        {
            _shiftRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Shift?)null);

            var result = await _controller.SetEnd(99, TimeSpan.FromHours(16));

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("not found", notFound.Value!.ToString());

            _shiftExec.Verify(
                s => s.SetEndTimeAsync(
                    It.IsAny<int>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // ------------------------------------------------------------
        // MarkShiftSubstituted: tjekker at repo-metoden bliver kaldt
        // ------------------------------------------------------------
        [Fact]
        public async Task MarkShiftSubstituted_CallsRepository_AndReturnsNoContent()
        {
            var result = await _controller.MarkShiftSubstituted(5, true);

            Assert.IsType<NoContentResult>(result);

            _shiftRepo.Verify(
                r => r.MarkShiftSubstitutedAsync(
                    5,
                    true,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    
    [Fact]
        public async Task Add_InvalidEmployee_ReturnsBadRequest_AndDoesNotSave()
        {
            // Arrange
            var dto = new CreateShiftDto
            {
                DateOfShift = new DateTime(2025, 1, 1),
                EmployeeId = 999,
                BicycleId = 1,
                RouteId = 1,
                SubstitutedId = 0
            };

            _employeeRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Employee>
                {
                    new Employee { EmployeeId = 1 },
                    new Employee { EmployeeId = 2 }
                });

            // (de andre repos behøver ikke sættes op, fordi vi stopper før)
            // Act
            var result = await _controller.Add(dto);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(bad.Value);

            _shiftRepo.Verify(r => r.AddAsync(It.IsAny<Shift>(), It.IsAny<CancellationToken>()), Times.Never);
            _bicycleRepo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
            _routeRepo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Add_InvalidBicycle_ReturnsBadRequest_AndDoesNotSave()
        {
            // Arrange
            var dto = new CreateShiftDto
            {
                DateOfShift = new DateTime(2025, 1, 1),
                EmployeeId = 1,
                BicycleId = 999,
                RouteId = 1,
                SubstitutedId = 0
            };

            _employeeRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Employee> { new Employee { EmployeeId = 1 } });

            _bicycleRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Bicycle> { new Bicycle { BicycleId = 1 } });

            // Act
            var result = await _controller.Add(dto);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(bad.Value);

            _shiftRepo.Verify(r => r.AddAsync(It.IsAny<Shift>(), It.IsAny<CancellationToken>()), Times.Never);
            _routeRepo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Add_InvalidRoute_ReturnsBadRequest_AndDoesNotSave()
        {
            // Arrange
            var dto = new CreateShiftDto
            {
                DateOfShift = new DateTime(2025, 1, 1),
                EmployeeId = 1,
                BicycleId = 1,
                RouteId = 999,
                SubstitutedId = 0
            };

            _employeeRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Employee> { new Employee { EmployeeId = 1 } });

            _bicycleRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Bicycle> { new Bicycle { BicycleId = 1 } });

            _routeRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RouteEntity> { new RouteEntity { Id = 1 } });

            // Act
            var result = await _controller.Add(dto);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(bad.Value);

            _shiftRepo.Verify(r => r.AddAsync(It.IsAny<Shift>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // -------------------------
        // ADD() – substitutedId logic + happy path
        // -------------------------

        [Fact]
        public async Task Add_SubstitutedIdZeroOrNegative_UsesEmployeeId_ReturnsNoContent()
        {
            // Arrange
            var dto = new CreateShiftDto
            {
                DateOfShift = new DateTime(2025, 1, 1),
                EmployeeId = 10,
                BicycleId = 20,
                RouteId = 30,
                SubstitutedId = 0 // <= 0 => bindes til EmployeeId
            };

            _employeeRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Employee> { new Employee { EmployeeId = 10 } });

            _bicycleRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Bicycle> { new Bicycle { BicycleId = 20 } });

            _routeRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RouteEntity> { new RouteEntity { Id = 30 } });

            Shift? captured = null;
            _shiftRepo.Setup(r => r.AddAsync(It.IsAny<Shift>(), It.IsAny<CancellationToken>()))
                .Callback<Shift, CancellationToken>((s, _) => captured = s)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Add(dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.NotNull(captured);

            Assert.Equal(dto.EmployeeId, captured!.SubstitutedId); // 핵심regel
            Assert.Equal(dto.EmployeeId, captured.EmployeeId);
            Assert.Equal(dto.BicycleId, captured.BicycleId);
            Assert.Equal(dto.RouteId, captured.RouteId);

            _shiftRepo.Verify(r => r.AddAsync(It.IsAny<Shift>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Add_SubstitutedIdPositive_UsesProvidedValue_ReturnsNoContent()
        {
            // Arrange
            var dto = new CreateShiftDto
            {
                DateOfShift = new DateTime(2025, 1, 1),
                EmployeeId = 10,
                BicycleId = 20,
                RouteId = 30,
                SubstitutedId = 77 // > 0 => beholdes
            };

            _employeeRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Employee> { new Employee { EmployeeId = 10 } });

            _bicycleRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Bicycle> { new Bicycle { BicycleId = 20 } });

            _routeRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RouteEntity> { new RouteEntity { Id = 30 } });

            Shift? captured = null;
            _shiftRepo.Setup(r => r.AddAsync(It.IsAny<Shift>(), It.IsAny<CancellationToken>()))
                .Callback<Shift, CancellationToken>((s, _) => captured = s)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Add(dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.NotNull(captured);

            Assert.Equal(77, captured!.SubstitutedId); // 핵심regel
            _shiftRepo.Verify(r => r.AddAsync(It.IsAny<Shift>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Add_WhenRepositoryThrows_Returns500_WithErrorPayload()
        {
            // Arrange
            var dto = new CreateShiftDto
            {
                DateOfShift = new DateTime(2025, 1, 1),
                EmployeeId = 1,
                BicycleId = 1,
                RouteId = 1,
                SubstitutedId = 1
            };

            _employeeRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Employee> { new Employee { EmployeeId = 1 } });

            _bicycleRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Bicycle> { new Bicycle { BicycleId = 1 } });

            _routeRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RouteEntity> { new RouteEntity { Id = 1 } });

            _shiftRepo.Setup(r => r.AddAsync(It.IsAny<Shift>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("boom"));

            // Act
            var result = await _controller.Add(dto);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
            Assert.NotNull(obj.Value);
        }

        // -------------------------
        // START/END – NotFound / 500 / Happy
        // -------------------------

        [Fact]
        public async Task SetStart_UnknownShift_ReturnsNotFound_AndDoesNotCallService()
        {
            _shiftRepo.Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Shift?)null);

            var result = await _controller.SetStart(42, TimeSpan.FromHours(8));

            Assert.IsType<NotFoundObjectResult>(result);

            _shiftExec.Verify(s => s.SetStartTimeAsync(It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task SetStart_WhenServiceThrows_Returns500()
        {
            _shiftRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Shift { ShiftId = 10 });

            _shiftExec.Setup(s => s.SetStartTimeAsync(10, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("kaboom"));

            var result = await _controller.SetStart(10, TimeSpan.FromHours(8));

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task SetStart_HappyPath_ReturnsNoContent_AndCallsService()
        {
            _shiftRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Shift { ShiftId = 10 });

            var result = await _controller.SetStart(10, TimeSpan.FromHours(8));

            Assert.IsType<NoContentResult>(result);

            _shiftExec.Verify(s => s.SetStartTimeAsync(10, TimeSpan.FromHours(8), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SetEnd_UnknownShift_ReturnsNotFound_AndDoesNotCallService()
        {
            _shiftRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Shift?)null);

            var result = await _controller.SetEnd(99, TimeSpan.FromHours(16));

            Assert.IsType<NotFoundObjectResult>(result);

            _shiftExec.Verify(s => s.SetEndTimeAsync(It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task SetEnd_WhenServiceThrows_Returns500()
        {
            _shiftRepo.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Shift { ShiftId = 7 });

            _shiftExec.Setup(s => s.SetEndTimeAsync(7, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("kaboom"));

            var result = await _controller.SetEnd(7, TimeSpan.FromHours(16));

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task SetEnd_HappyPath_ReturnsNoContent_AndCallsService()
        {
            _shiftRepo.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Shift { ShiftId = 7 });

            var result = await _controller.SetEnd(7, TimeSpan.FromHours(16));

            Assert.IsType<NoContentResult>(result);

            _shiftExec.Verify(s => s.SetEndTimeAsync(7, TimeSpan.FromHours(16), It.IsAny<CancellationToken>()), Times.Once);
        }

        // -------------------------
        // MarkShiftSubstituted
        // -------------------------

        [Fact]
        public async Task MarkShiftSubstituted_CallsRepo_ReturnsNoContent()
        {
            var result = await _controller.MarkShiftSubstituted(5, true);

            Assert.IsType<NoContentResult>(result);

            _shiftRepo.Verify(r => r.MarkShiftSubstitutedAsync(5, true, It.IsAny<CancellationToken>()), Times.Once);
        }

        // -------------------------
        // GetAll – mapper branch
        // -------------------------

        [Fact]
        public async Task GetAll_ReturnsOk_WithMappedDtos()
        {
            // Arrange
            var shiftRepo = new Mock<IShiftRepository>();
            var shiftExec = new Mock<IShiftExecutionService>();
            var employeeRepo = new Mock<IEmployeeRepository>();
            var bicycleRepo = new Mock<IBicycleRepository>();
            var routeRepo = new Mock<IRouteRepository>();

            var mapperMock = new Mock<IMapper>();

            var shiftsFromRepo = new List<Shift>
            {
                new Shift { ShiftId = 1, RouteId = 10, SubstitutedId = 1, DateOfShift = new DateTime(2025,1,1) },
                new Shift { ShiftId = 2, RouteId = 11, SubstitutedId = 2, DateOfShift = new DateTime(2025,1,2) }
            };

            shiftRepo
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(shiftsFromRepo);

            var mappedDtos = new List<ShiftDto>
            {
                new ShiftDto { ShiftId = 1, RouteId = 10, SubstitutedId = 1, DateOfShift = new DateTime(2025,1,1) },
                new ShiftDto { ShiftId = 2, RouteId = 11, SubstitutedId = 2, DateOfShift = new DateTime(2025,1,2) }
            };

            // VIGTIGT: match input-typen, som controlleren sender ind i Map(...)
            mapperMock
                .Setup(m => m.Map<IEnumerable<ShiftDto>>(It.IsAny<IEnumerable<Shift>>()))
                .Returns(mappedDtos);

            var controller = new ShiftController(
                shiftRepo.Object,
                shiftExec.Object,
                employeeRepo.Object,
                bicycleRepo.Object,
                routeRepo.Object,
                mapperMock.Object // <-- her er fixet
            );

            // Act
            var result = await controller.GetAll();

            // Assert (status)
            var ok = Assert.IsType<OkObjectResult>(result);

            // Assert (payload)
            var dtos = Assert.IsAssignableFrom<IEnumerable<ShiftDto>>(ok.Value);
            var list = dtos.ToList();

            Assert.Equal(2, list.Count);
            Assert.Equal(1, list[0].ShiftId);
            Assert.Equal(2, list[1].ShiftId);

            // Assert (samarbejde/calls)
            shiftRepo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);

            mapperMock.Verify(
                m => m.Map<IEnumerable<ShiftDto>>(It.IsAny<IEnumerable<Shift>>()),
                Times.Once
            );
        }

        // -------------------------
        // Delete
        // -------------------------

        [Fact]
        public async Task Delete_ReturnsOk_WithRepoResult()
        {
            _shiftRepo.Setup(r => r.DeleteAsync(123, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.Delete(123);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(true, ok.Value);

            _shiftRepo.Verify(r => r.DeleteAsync(123, It.IsAny<CancellationToken>()), Times.Once);
        }

    }
}
