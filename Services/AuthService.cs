using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Assignment_Submission_Management_System.DTOs;
using Assignment_Submission_Management_System.Models;
using Microsoft.IdentityModel.Tokens;

namespace Assignment_Submission_Management_System.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword)) return false;
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }

        public string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["Key"] ?? "Your_Super_Secret_Key_Which_Must_Be_At_Least_32_Bytes_Long!";
            var issuer = jwtSettings["Issuer"] ?? "AssignmentMgmtAPI";
            var audience = jwtSettings["Audience"] ?? "AssignmentMgmtUsers";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            if (user.CourseId.HasValue)
            {
                claims.Add(new Claim("CourseId", user.CourseId.Value.ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public AuthResponseDto CreateAuthResponse(User user, string token)
        {
            return new AuthResponseDto
            {
                Token = token,
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                CourseId = user.CourseId,
                CourseName = user.Course?.Name
            };
        }
    }
}
