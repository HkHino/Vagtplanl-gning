using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Vagtplanlægning.Controllers;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Services;
using Xunit;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;


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

        // Boundery vaule analyse (BVA): år lige under grænsen (2019) -> BadRequest
        [Fact]
        public async Task GetMonthlyHours_YearBelow2020_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetMonthlyHours(
                employeeId: null, year: 2019, month: 11, CancellationToken.None);

            // Assert
            var badReq = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("2020", badReq.Value!.ToString());
            _serviceMock.Verify(
                s => s.GetMonthlyHoursAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // BVA: month = 0 -> BadRequest
        [Fact]
        public async Task GetMonthlyHours_MonthZero_ReturnsBadRequest()
        {
            var result = await _controller.GetMonthlyHours(null, 2025, 0, CancellationToken.None);

            var badReq = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Month must be between 1 and 12", badReq.Value!.ToString());
            _serviceMock.Verify(
                s => s.GetMonthlyHoursAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // BVA: month = 13 -> BadRequest
        [Fact]
        public async Task GetMonthlyHours_MonthThirteen_ReturnsBadRequest()
        {
            var result = await _controller.GetMonthlyHours(null, 2025, 13, CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(result.Result);
            _serviceMock.Verify(
                s => s.GetMonthlyHoursAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // Equivalence Partitioning (EP) + positiv test: gyldige data -> Ok + kalder service
        [Fact]
        public async Task GetMonthlyHours_ValidInput_ReturnsOkWithData()
        {
            // Arrange
            var rows = new List<MonthlyHoursRow>
            {
                new MonthlyHoursRow
                {
                    EmployeeId = 2,
                    Year = 2025,
                    Month = 11,
                    TotalMonthlyHours = 8m,
                    HasSubstituted = true
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
            Assert.Single(model);

            _serviceMock.Verify(
                s => s.GetMonthlyHoursAsync(2, 2025, 11, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
