using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Vagtplanlægning.DTOs;
using Vagtplanlægning.Services;
using Xunit;

namespace Vagtplanlægning.Tests.Services
{
    public class FallbackMonthlyHoursReportServiceTests
    {
        [Fact]
        public async Task UsesMySql_WhenMySqlSucceeds()
        {
            // Arrange
            var mysql = new Mock<IMonthlyHoursReportService>();
            var mongo = new Mock<IMonthlyHoursReportService>();

            var expected = new List<MonthlyHoursRow>
            {
                new MonthlyHoursRow { EmployeeId = 1, TotalMonthlyHours = 10 }
            };

            mysql.Setup(s => s.GetMonthlyHoursAsync(1, 2025, 11, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(expected);

            var service = new FallbackMonthlyHoursReportService(mysql.Object, mongo.Object);

            // Act
            var result = await service.GetMonthlyHoursAsync(1, 2025, 11, default);

            // Assert
            Assert.Equal(expected, result);
            mysql.Verify(s => s.GetMonthlyHoursAsync(1, 2025, 11, It.IsAny<CancellationToken>()), Times.Once);
            mongo.Verify(s => s.GetMonthlyHoursAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UsesMongo_WhenMySqlThrows()
        {
            // Arrange
            var mysql = new Mock<IMonthlyHoursReportService>();
            var mongo = new Mock<IMonthlyHoursReportService>();

            mysql.Setup(s => s.GetMonthlyHoursAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ThrowsAsync(new System.Exception("db meltdown"));

            var expectedMongo = new List<MonthlyHoursRow>
            {
                new MonthlyHoursRow { EmployeeId = 1, TotalMonthlyHours = 99 }
            };

            mongo.Setup(s => s.GetMonthlyHoursAsync(1, 2025, 11, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(expectedMongo);

            var service = new FallbackMonthlyHoursReportService(mysql.Object, mongo.Object);

            // Act
            var result = await service.GetMonthlyHoursAsync(1, 2025, 11, default);

            // Assert
            Assert.Equal(99, result[0].TotalMonthlyHours);
            mongo.Verify(s => s.GetMonthlyHoursAsync(1, 2025, 11, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
