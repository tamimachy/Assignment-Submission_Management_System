using Assignment_Submission_Management_System.DTOs;
using Assignment_Submission_Management_System.Models;

namespace Assignment_Submission_Management_System.Services
{
    public interface IAuthService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        string GenerateJwtToken(User user);
        AuthResponseDto CreateAuthResponse(User user, string token);
    }
}
