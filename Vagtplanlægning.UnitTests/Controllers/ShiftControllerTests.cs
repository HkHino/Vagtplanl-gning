using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Vagtplanlægning.Controllers;
using Vagtplanlægning.Models;
using Vagtplanlægning.Repositories;
using Vagtplanlægning.Services;
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
    }
}
