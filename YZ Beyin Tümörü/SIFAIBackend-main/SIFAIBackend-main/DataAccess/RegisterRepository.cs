using SIFAIBackend.Entities;
using System.Threading.Tasks;

namespace SIFAIBackend.DataAccess
{
    public interface IRegisterRepository
    {
        Task<int> AddRegisterAsync(User user);
    }

    public class RegisterRepository : IRegisterRepository
    {
        private readonly SifaiContext _context;

        public RegisterRepository(SifaiContext context)
        {
            _context = context;
        }

        public async Task<int> AddRegisterAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user.Id;
        }
    }

}
