using SIFAIBackend.DataAccess;
using SIFAIBackend.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIFAIBackend.Business
{
    public class TumorService : ITumorService
    {
        private readonly SifaiContext _context;

        public TumorService(SifaiContext context)
        {
            _context = context;
        }

        // Yeni bir tümör tespiti kaydet
        public async Task<int> SaveDetectionAsync(int userId, string imageUrl, string tumorType)
        {
            var detection = new TumorDetectionHistory
            {
                UserId = userId,
                ImageUrl = imageUrl,
                TumorType = tumorType,
                DetectionDate = DateTime.UtcNow
            };

            _context.TumorDetectionHistories.Add(detection);
            await _context.SaveChangesAsync();

            return detection.Id;
        }

        // Kullanıcının geçmiş tespitlerini getir
        public async Task<List<TumorDetectionHistory>> GetHistoryByUserIdAsync(int userId)
        {
            return await Task.FromResult(
                _context.TumorDetectionHistories
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.DetectionDate)
                    .ToList()
            );
        }
    }
}
