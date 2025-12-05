using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Vagtplanlægning.Repositories;
using Vagtplanlægning.Services;
using Xunit;

namespace Vagtplanlægning.UnitTests.Services
{
    public class ShiftPlanServiceTests
    {
        private readonly ShiftPlanService _service;

        public ShiftPlanServiceTests()
        {
            var employeeRepo = new Mock<IEmployeeRepository>();
            var routeRepo = new Mock<IRouteRepository>();
            var shiftPlanRepo = new Mock<IShiftPlanRepository>();

            _service = new ShiftPlanService(
                employeeRepo.Object,
                routeRepo.Object,
                shiftPlanRepo.Object
            );
        }

        [Fact]
        public async Task Generate6WeekPlanAsync_Throws_WhenStartDateDefault()
        {
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.Generate6WeekPlanAsync(default, CancellationToken.None));
        }

        [Fact]
        public async Task Generate6WeekPlanAsync_ReturnsPlanWithNonEmptyId()
        {
            var start = new DateTime(2025, 11, 1);

            var plan = await _service.Generate6WeekPlanAsync(start, CancellationToken.None);

            Assert.False(string.IsNullOrWhiteSpace(plan.ShiftPlanId));
        }

        [Fact]
        public async Task Generate6WeekPlanAsync_ReturnsPlanWithCorrectStartDate()
        {
            var start = new DateTime(2025, 11, 1);

            var plan = await _service.Generate6WeekPlanAsync(start, CancellationToken.None);

            Assert.Equal(start, plan.StartDate);
        }
    }
}
