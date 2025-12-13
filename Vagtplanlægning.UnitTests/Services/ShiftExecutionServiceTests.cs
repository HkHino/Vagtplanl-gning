using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using Vagtplanlægning.Models;
using Vagtplanlægning.Repositories;
using Vagtplanlægning.Services;
using Xunit;

namespace Vagtplanlægning.UnitTests.Services
{
    public class ShiftExecutionServiceTests
    {
        private static IConfiguration MakeConfig(string provider)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "DatabaseProvider", provider }
                })
                .Build();
        }

        // ------------------------------------------------------------
        // AddShiftAsync: Non-MySql eller db == null => repo.AddAsync() bliver kaldt
        // ------------------------------------------------------------
        [Fact]
        public async Task AddShiftAsync_WhenNotMySqlOrDbMissing_CreatesShiftAndCallsRepository()
        {
            // Arrange
            var repo = new Mock<IShiftRepository>();
            var cfg = MakeConfig("Mongo"); // alt andet end "MySql" går repo-vejen

            Shift? captured = null;
            repo.Setup(r => r.AddAsync(It.IsAny<Shift>(), It.IsAny<CancellationToken>()))
                .Callback<Shift, CancellationToken>((s, _) => captured = s)
                .Returns(Task.CompletedTask);

            var service = new ShiftExecutionService(cfg, repo.Object, db: null);

            var day = new DateTime(2025, 11, 25);
            var meet = TimeSpan.FromHours(8);

            // Act
            await service.AddShiftAsync(day, employeeId: 1, bicycleId: 2, routeNumberId: 3, meetInTime: meet, substitutedId: 0);

            // Assert
            repo.Verify(r => r.AddAsync(It.IsAny<Shift>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.NotNull(captured);

            Assert.Equal(day, captured!.DateOfShift);
            Assert.Equal(1, captured.EmployeeId);
            Assert.Equal(2, captured.BicycleId);
            Assert.Equal(3, captured.RouteId);
            Assert.Null(captured.StartTime);
            Assert.Null(captured.EndTime);
            Assert.Null(captured.TotalHours);
            Assert.Equal(0, captured.SubstitutedId);
        }

        // ------------------------------------------------------------
        // SetStartTimeAsync: shift findes ikke => Update må IKKE kaldes
        // ------------------------------------------------------------
        [Fact]
        public async Task SetStartTimeAsync_WhenShiftNotFound_DoesNotUpdate()
        {
            // Arrange
            var repo = new Mock<IShiftRepository>();
            var cfg = MakeConfig("Mongo");

            repo.Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Shift?)null);

            var service = new ShiftExecutionService(cfg, repo.Object, db: null);

            // Act
            await service.SetStartTimeAsync(42, TimeSpan.FromHours(8));

            // Assert
            repo.Verify(r => r.UpdateAsync(It.IsAny<Shift>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ------------------------------------------------------------
        // SetStartTimeAsync: shift findes => StartTime sættes + Update kaldes
        // ------------------------------------------------------------
        [Fact]
        public async Task SetStartTimeAsync_WhenShiftExists_SetsStartTimeAndUpdates()
        {
            // Arrange
            var repo = new Mock<IShiftRepository>();
            var cfg = MakeConfig("Mongo");

            var shift = new Shift { ShiftId = 10, StartTime = null };

            repo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(shift);

            var service = new ShiftExecutionService(cfg, repo.Object, db: null);

            var start = TimeSpan.FromHours(8);

            // Act
            await service.SetStartTimeAsync(10, start);

            // Assert
            Assert.Equal(start, shift.StartTime);

            repo.Verify(r => r.UpdateAsync(
                    It.Is<Shift>(s => s.ShiftId == 10 && s.StartTime == start),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ------------------------------------------------------------
        // SetEndTimeAsync: shift findes ikke => Update må IKKE kaldes
        // ------------------------------------------------------------
        [Fact]
        public async Task SetEndTimeAsync_WhenShiftNotFound_DoesNotUpdate()
        {
            // Arrange
            var repo = new Mock<IShiftRepository>();
            var cfg = MakeConfig("Mongo");

            repo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Shift?)null);

            var service = new ShiftExecutionService(cfg, repo.Object, db: null);

            // Act
            await service.SetEndTimeAsync(99, TimeSpan.FromHours(16));

            // Assert
            repo.Verify(r => r.UpdateAsync(It.IsAny<Shift>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ------------------------------------------------------------
        // SetEndTimeAsync: StartTime mangler => TotalHours må IKKE beregnes
        // ------------------------------------------------------------
        [Fact]
        public async Task SetEndTimeAsync_WhenNoStartTime_SetsEndTimeButDoesNotSetTotalHours()
        {
            // Arrange
            var repo = new Mock<IShiftRepository>();
            var cfg = MakeConfig("Mongo");

            var shift = new Shift
            {
                ShiftId = 5,
                StartTime = null,
                EndTime = null,
                TotalHours = null
            };

            repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(shift);

            var service = new ShiftExecutionService(cfg, repo.Object, db: null);

            // Act
            await service.SetEndTimeAsync(5, TimeSpan.FromHours(16));

            // Assert
            Assert.Equal(TimeSpan.FromHours(16), shift.EndTime);
            Assert.Null(shift.TotalHours);

            repo.Verify(r => r.UpdateAsync(It.IsAny<Shift>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        // ------------------------------------------------------------
        // SetEndTimeAsync: StartTime findes => TotalHours beregnes
        // ------------------------------------------------------------
        [Fact]
        public async Task SetEndTimeAsync_WhenStartTimeExists_ComputesTotalHoursAndUpdates()
        {
            // Arrange
            var repo = new Mock<IShiftRepository>();
            var cfg = MakeConfig("Mongo");

            var shift = new Shift
            {
                ShiftId = 7,
                StartTime = TimeSpan.FromHours(8),
                EndTime = null,
                TotalHours = null
            };

            repo.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
                .ReturnsAsync(shift);

            var service = new ShiftExecutionService(cfg, repo.Object, db: null);

            // Act
            await service.SetEndTimeAsync(7, TimeSpan.FromHours(16));

            // Assert
            Assert.Equal(TimeSpan.FromHours(16), shift.EndTime);
            Assert.Equal(8m, shift.TotalHours); // 16 - 8 = 8 timer

            repo.Verify(r => r.UpdateAsync(
                    It.Is<Shift>(s => s.ShiftId == 7 && s.TotalHours == 8m),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
