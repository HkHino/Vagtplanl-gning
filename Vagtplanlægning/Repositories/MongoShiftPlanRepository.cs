using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Vagtplanlægning.Data;
using Vagtplanlægning.Models;

namespace Vagtplanlægning.Repositories
{
    public class MongoShiftPlanRepository : IShiftPlanRepository
    {
        private readonly MongoDbContext _mongo;
        private readonly ILogger<MongoShiftPlanRepository> _logger;

        public MongoShiftPlanRepository(
            MongoDbContext mongo,
            ILogger<MongoShiftPlanRepository> logger)
        {
            _mongo = mongo;
            _logger = logger;
        }

        public async Task<IEnumerable<ShiftPlan>> GetAllAsync(CancellationToken ct = default)
        {
            var cursor = await _mongo.ShiftPlans
                .FindAsync(_ => true, cancellationToken: ct);

            var result = await cursor.ToListAsync(ct);
            _logger.LogDebug("MongoShiftPlanRepository.GetAllAsync returned {Count} docs", result.Count);
            return result;
        }

        public async Task<ShiftPlan?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            var cursor = await _mongo.ShiftPlans
                .FindAsync(x => x.ShiftPlanId == id, cancellationToken: ct);

            return await cursor.FirstOrDefaultAsync(ct);
        }

        public async Task AddAsync(ShiftPlan plan, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(plan.ShiftPlanId))
            {
                plan.ShiftPlanId = Guid.NewGuid().ToString();
            }

            await _mongo.ShiftPlans.InsertOneAsync(plan, cancellationToken: ct);
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
        {
            var result = await _mongo.ShiftPlans.DeleteOneAsync(
                x => x.ShiftPlanId == id,
                ct);

            return result.DeletedCount > 0;
        }

        public async Task UpdateAsync(ShiftPlan plan, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(plan.ShiftPlanId))
                throw new ArgumentException("ShiftPlanId is required for update.", nameof(plan));

            var result = await _mongo.ShiftPlans.ReplaceOneAsync(
                x => x.ShiftPlanId == plan.ShiftPlanId,
                plan,
                cancellationToken: ct);

            if (result.MatchedCount == 0)
            {
                throw new InvalidOperationException(
                    $"ShiftPlan with id '{plan.ShiftPlanId}' not found in Mongo.");
            }
        }
    }
}
