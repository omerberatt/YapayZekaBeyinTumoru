using SIFAIBackend.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SIFAIBackend.Business
{
    public interface ITumorService
    {
        Task<int> SaveDetectionAsync(int userId, string imageUrl, string tumorType);
        Task<List<TumorDetectionHistory>> GetHistoryByUserIdAsync(int userId);
    }
}
