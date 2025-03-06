using SIFAIBackend.DataAccess;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using SIFAIBackend.Entities;

namespace SIFAIBackend.Business
{
    public class AuthService
    {
        private readonly UserDal _userDal;
        private readonly string _jwtSecret;
        private readonly double _jwtExpireHours;

        public AuthService(UserDal userDal, string jwtSecret, double jwtExpireHours)
        {
            _userDal = userDal;
            _jwtSecret = jwtSecret;
            _jwtExpireHours = jwtExpireHours;
        }

        // Kullanıcıyı doğrula ve JWT token oluştur
        public async Task<(string Token, User User)> AuthenticateAsync(string email, string password)
        {
            var user = await _userDal.GetUserByUsernameAndPasswordAsync(email, password);
            if (user != null)
            {
                // Kullanıcı doğrulandı, token oluştur
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSecret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Name)
                    }),
                    Expires = DateTime.UtcNow.AddHours(_jwtExpireHours),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return (tokenHandler.WriteToken(token), user);
            }
            else
            {
                throw new Exception("Invalid email or password");
            }
        }
    }
}
