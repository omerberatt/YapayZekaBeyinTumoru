using SIFAIBackend.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SIFAIBackend.DataAccess
{
    public class UserDal
    {
        private readonly SifaiContext _context;

        public UserDal(SifaiContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByUsernameAndPasswordAsync(string email, string password)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
        }
    }
}
