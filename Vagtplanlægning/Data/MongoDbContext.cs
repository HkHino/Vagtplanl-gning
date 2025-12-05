using MongoDB.Driver;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _db;

        public MongoDbContext(IMongoDatabase db)
        {
            _db = db;
        }

        public IMongoCollection<ShiftPlan> ShiftPlans =>
            _db.GetCollection<ShiftPlan>("ShiftPlans");

        public IMongoCollection<Employee> Employees =>
            _db.GetCollection<Employee>("Employees");

        public IMongoCollection<Shift> Shifts =>
            _db.GetCollection<Shift>("ListOfShift");

        public IMongoCollection<RouteEntity> Routes =>
            _db.GetCollection<RouteEntity>("Routes");

        public IMongoCollection<Substituted> Substituteds =>
            _db.GetCollection<Substituted>("Substituteds");

        public IMongoCollection<WorkHoursInMonth> WorkHoursInMonths =>
            _db.GetCollection<WorkHoursInMonth>("WorkHoursInMonths");

    }
}
