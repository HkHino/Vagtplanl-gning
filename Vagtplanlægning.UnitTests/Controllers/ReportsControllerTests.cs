using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vagtplanlægning.Controllers;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Services;
using Xunit;

namespace Vagtplanlægning.UnitTests.Controllers
{
    public class ReportsControllerTests
    {
        private readonly Mock<IMonthlyHoursReportService> _serviceMock;
        private readonly ReportsController _controller;

        public ReportsControllerTests()
        {
            _serviceMock = new Mock<IMonthlyHoursReportService>();
            _controller = new ReportsController(_serviceMock.Object);
        }

        // ------------------------------------------------------------
        // BVA + EP på YEAR (år < 2020 er ulovligt)
        // ------------------------------------------------------------
        [Theory]
        [InlineData(2019)]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetMonthlyHours_InvalidYear_ReturnsBadRequest(int year)
        {
            // Act
            var result = await _controller.GetMonthlyHours(
                employeeId: null,
                year: year,
                month: 11,
                CancellationToken.None);

            // Assert
            var badReq = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("2020", badReq.Value!.ToString());

            _serviceMock.Verify(
                s => s.GetMonthlyHoursAsync(
                    It.IsAny<int?>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // ------------------------------------------------------------
        // BVA + EP på MONTH (udenfor [1;12] er ulovligt)
        // ------------------------------------------------------------
        [Theory]
        [InlineData(0)]   // lige under grænsen
        [InlineData(13)]  // lige over grænsen
        [InlineData(-1)]  // langt udenfor
        public async Task GetMonthlyHours_InvalidMonth_ReturnsBadRequest(int month)
        {
            // Act
            var result = await _controller.GetMonthlyHours(
                employeeId: null,
                year: 2025,
                month: month,
                CancellationToken.None);

            // Assert
            var badReq = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Month must be between 1 and 12", badReq.Value!.ToString());

            _serviceMock.Verify(
                s => s.GetMonthlyHoursAsync(
                    It.IsAny<int?>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // ------------------------------------------------------------
        // EP + positiv test: gyldige data -> Ok + service kaldt
        // ------------------------------------------------------------
        [Fact]
        public async Task GetMonthlyHours_ValidInput_ReturnsOkWithData()
        {
            // Arrange – repræsentant for “valid partition”:
            // year >= 2020, month in [1;12]
            var rows = new List<MonthlyHoursRow>
            {
                new MonthlyHoursRow
                {
                    EmployeeId       = 2,
                    FirstName        = "Test",
                    LastName         = "Person",
                    Year             = 2025,
                    Month            = 11,
                    TotalMonthlyHours = 8m,
                    HasSubstituted    = true
                }
            };

            _serviceMock
                .Setup(s => s.GetMonthlyHoursAsync(2, 2025, 11, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rows);

            // Act
            var result = await _controller.GetMonthlyHours(2, 2025, 11, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var model = Assert.IsAssignableFrom<IEnumerable<MonthlyHoursRow>>(ok.Value);

            var single = Assert.Single(model); // sikrer præcis 1 element
            Assert.Equal(2, single.EmployeeId);
            Assert.Equal("Test", single.FirstName);
            Assert.Equal("Person", single.LastName);
            Assert.Equal(8m, single.TotalMonthlyHours);

            _serviceMock.Verify(
                s => s.GetMonthlyHoursAsync(2, 2025, 11, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // (Evt. en ekstra positiv test for boundary cases, hvis du vil være ekstra grundig)
        [Theory]
        [InlineData(2020, 1)]
        [InlineData(2020, 12)]
        public async Task GetMonthlyHours_ValidBoundaryValues_ReturnsOk(int year, int month)
        {
            // Arrange – boundary cases indenfor valid range
            var rows = new List<MonthlyHoursRow>
            {
                new MonthlyHoursRow
                {
                    EmployeeId = 1,
                    Year = year,
                    Month = month,
                    TotalMonthlyHours = 5m
                }
            };

            _serviceMock
                .Setup(s => s.GetMonthlyHoursAsync(1, year, month, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rows);

            // Act
            var result = await _controller.GetMonthlyHours(1, year, month, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var model = Assert.IsAssignableFrom<IEnumerable<MonthlyHoursRow>>(ok.Value);
            Assert.Single(model);

            _serviceMock.Verify(
                s => s.GetMonthlyHoursAsync(1, year, month, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
