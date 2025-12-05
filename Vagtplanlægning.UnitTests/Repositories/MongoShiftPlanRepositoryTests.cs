using Mongo2Go;
using MongoDB.Driver;
using System.Threading.Tasks;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;
using Vagtplanlægning.Repositories;
using Xunit;

namespace Vagtplanlægning.Tests.Repositories
{
    public class MongoShiftPlanRepositoryTests
    {
        [Fact]
        public async Task CanInsertAndRetrievePlan()
        {
            using var runner = MongoDbRunner.Start();
            var client = new MongoClient(runner.ConnectionString);
            var db = client.GetDatabase("tests");

            var repo = new MongoShiftPlanRepository(new MongoDbContext(db));

            var plan = new ShiftPlan
            {
                ShiftPlanId = "ABC",
                Name = "Test Plan"
            };

            await repo.InsertAsync(plan, default);

            var fetched = await repo.GetByIdAsync("ABC", default);

            Assert.NotNull(fetched);
            Assert.Equal("Test Plan", fetched.Name);
        }
    }
}
