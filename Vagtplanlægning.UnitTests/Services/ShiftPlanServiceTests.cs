using System;
using Xunit;
using Vagtplanlægning.Services;

namespace Vagtplanlægning.Tests.Services
{
    public class ShiftPlanServiceTests
    {
        [Fact]
        public void Generate6WeekPlan_Throws_WhenStartDateIsDefault()
        {
            var service = new ShiftPlanService();

            Assert.Throws<ArgumentException>(() =>
                service.Generate6WeekPlan(default));
        }

        [Fact]
        public void Generate6WeekPlan_ReturnsCorrectEndDate()
        {
            var start = new DateTime(2025, 11, 1);
            var service = new ShiftPlanService();

            var result = service.Generate6WeekPlan(start);

            Assert.Equal(start.AddDays(7 * 6), result.EndDate);
        }

        [Fact]
        public void Generate6WeekPlan_ReturnsUniqueId()
        {
            var service = new ShiftPlanService();
            var result = service.Generate6WeekPlan(new DateTime(2025, 10, 1));

            Assert.False(string.IsNullOrWhiteSpace(result.ShiftPlanId));
        }

        [Fact]
        public void Generate6WeekPlan_ProducesName()
        {
            var start = new DateTime(2025, 11, 1);
            var service = new ShiftPlanService();
            var result = service.Generate6WeekPlan(start);

            Assert.Contains("2025", result.Name);
        }
    }
}
