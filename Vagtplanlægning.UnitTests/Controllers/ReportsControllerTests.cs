using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Vagtplanlægning.Controllers;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Services;
using Xunit;

namespace Vagtplanlægning.UnitTests.Controllers
{
    public class ReportsControllerTests1 //todo fix this naming cause this is bad AF
    {
        [Fact]
        public async Task MonthlyHours_ValidRequest_ReturnsOkAndList()
        {
            // Arrange
            var mockService = new Mock<IMonthlyHoursReportService>();

            var expected = new List<MonthlyHoursRow>
            {
                new MonthlyHoursRow
                {
                    EmployeeId = 2,
                    FirstName = "Test",
                    LastName = "Person",
                    Year = 2025,
                    Month = 11,
                    TotalMonthlyHours = 8,
                    HasSubstituted = true
                }
            };

            mockService
                .Setup(s => s.GetMonthlyHoursAsync(2, 2025, 11, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var controller = new ReportsController(mockService.Object);

            // Act
            var result = await controller.GetMonthlyHours(2, 2025, 11, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var model = Assert.IsType<List<MonthlyHoursRow>>(okResult.Value);
            Assert.Single(model);
            Assert.Equal(2, model[0].EmployeeId);

            mockService.Verify(
                s => s.GetMonthlyHoursAsync(2, 2025, 11, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task MonthlyHours_InvalidMonth_ReturnsBadRequest()
        {
            // Arrange
            var mockService = new Mock<IMonthlyHoursReportService>();
            var controller = new ReportsController(mockService.Object);

            // Act
            var result = await controller.GetMonthlyHours(2, 2025, 13, CancellationToken.None);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            var body = badRequest.Value as dynamic;

            // Vi tjekker bare at der ER en error property
            Assert.NotNull(body);
        }

        [Fact]
        public async Task MonthlyHours_InvalidYear_ReturnsBadRequest()
        {
            // Arrange
            var mockService = new Mock<IMonthlyHoursReportService>();
            var controller = new ReportsController(mockService.Object);

            // Act
            var result = await controller.GetMonthlyHours(2, 2010, 11, CancellationToken.None);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            var body = badRequest.Value as dynamic;
            Assert.NotNull(body);
        }
    }
}
