using Microsoft.EntityFrameworkCore;
using MobileStore.Data;
using MobileStore.Models;

namespace MobileStore.Services
{
    public class RecommendationService
    {
        private readonly AppDbContext _db;

        public RecommendationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Phone>> GetSimilarAsync(int phoneId, int count = 4)
        {
            var source = await _db.Phones
                .Include(p => p.Brand)
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.Id == phoneId);

            if (source == null)
                return Enumerable.Empty<Phone>();

            var candidates = await _db.Phones
                .Include(p => p.Brand)
                .Include(p => p.Seller)
                .Where(p => p.Id != phoneId && p.IsAvailable && (p.Seller == null || !p.Seller.IsBlocked))
                .ToListAsync();

            var scored = candidates
                .Select(p => new
                {
                    Phone = p,
                    Score = ComputeScore(source, p)
                })
                .OrderByDescending(x => x.Score)
                .Take(count)
                .Select(x => x.Phone);

            return scored;
        }

        private static double ComputeScore(Phone source, Phone candidate)
        {
            double score = 0;

            // Same brand
            if (candidate.BrandId == source.BrandId)
                score += 30;

            // Price similarity
            double priceDiff = Math.Abs((double)(candidate.Price - source.Price)) / (double)source.Price;
            if (priceDiff <= 0.10) score += 25;
            else if (priceDiff <= 0.20) score += 15;
            else if (priceDiff <= 0.40) score += 5;

            // RAM
            if (candidate.RAM == source.RAM)
                score += 15;
            else if (Math.Abs(candidate.RAM - source.RAM) <= 2)
                score += 8;

            // Network
            if (candidate.Network == source.Network)
                score += 10;

            // Screen size
            if (Math.Abs(candidate.ScreenSize - source.ScreenSize) <= 0.3)
                score += 10;

            // Storage
            if (candidate.Storage == source.Storage)
                score += 10;

            return score;
        }
    }
}