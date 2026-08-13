using System.Security.Claims;
using Assignment_Submission_Management_System.Data;
using Assignment_Submission_Management_System.DTOs;
using Assignment_Submission_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment_Submission_Management_System.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("user-stats")]
        public async Task<IActionResult> GetUserStats()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            int.TryParse(userIdClaim, out int userId);
            var userRoleClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value ?? string.Empty;

            var isStudent = User.IsInRole("Student") || User.IsInRole("2") || userRoleClaim.Equals("Student", StringComparison.OrdinalIgnoreCase) || userRoleClaim == "2";
            var isTeacher = User.IsInRole("Teacher") || User.IsInRole("1") || userRoleClaim.Equals("Teacher", StringComparison.OrdinalIgnoreCase) || userRoleClaim == "1";

            var dto = new UserDashboardStatsDto();

            if (isStudent)
            {
                var student = await _context.Users.FindAsync(userId);
                var assignmentsQuery = _context.Assignments.Where(a => !a.IsDraft);
                if (student?.CourseId != null)
                {
                    assignmentsQuery = assignmentsQuery.Where(a => a.Subject.CourseId == student.CourseId.Value);
                }

                var assignedList = await assignmentsQuery.Select(a => a.Id).ToListAsync();
                dto.TotalAssignedAssignments = assignedList.Count;

                var studentSubs = await _context.Submissions
                    .Where(s => s.StudentId == userId)
                    .ToListAsync();

                dto.CompletedAssignments = studentSubs.Count;
                dto.IncompletedAssignments = Math.Max(0, dto.TotalAssignedAssignments - dto.CompletedAssignments);

                var gradedScores = studentSubs
                    .Where(s => s.MarksAwarded.HasValue)
                    .Select(s => (double)s.MarksAwarded!.Value)
                    .ToList();

                dto.AverageScore = gradedScores.Any() ? Math.Round(gradedScores.Average(), 2) : 0;
            }
            else if (isTeacher)
            {
                var teacherAssignments = _context.Assignments.Where(a => a.TeacherId == userId || a.Subject.TeacherId == userId);
                dto.TotalAssignedAssignments = await teacherAssignments.CountAsync();

                var teacherSubs = _context.Submissions.Where(s => s.Assignment.TeacherId == userId || s.Assignment.Subject.TeacherId == userId);
                dto.CompletedAssignments = await teacherSubs.CountAsync();
                dto.PendingGradingCount = await teacherSubs.CountAsync(s => s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.Late);
                dto.IncompletedAssignments = dto.PendingGradingCount;

                var gradedScores = await teacherSubs
                    .Where(s => s.MarksAwarded.HasValue)
                    .Select(s => (double)s.MarksAwarded!.Value)
                    .ToListAsync();

                dto.AverageScore = gradedScores.Any() ? Math.Round(gradedScores.Average(), 2) : 0;
            }
            else
            {
                dto.TotalAssignments = await _context.Assignments.CountAsync();
                dto.TotalSubmissions = await _context.Submissions.CountAsync();
                dto.TotalUsers = await _context.Users.CountAsync();
                dto.TotalCourses = await _context.Courses.CountAsync();

                var gradedMarks = await _context.Submissions
                    .Where(s => s.MarksAwarded.HasValue)
                    .Select(s => (double)s.MarksAwarded!.Value)
                    .ToListAsync();

                dto.AverageClassScore = gradedMarks.Any() ? Math.Round(gradedMarks.Average(), 2) : 0;
            }

            return Ok(dto);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalTeachers = await _context.Users.CountAsync(u => u.Role == UserRole.Teacher);
            var totalStudents = await _context.Users.CountAsync(u => u.Role == UserRole.Student);
            var totalCourses = await _context.Courses.CountAsync();
            var totalSubjects = await _context.Subjects.CountAsync();
            var totalAssignments = await _context.Assignments.CountAsync();
            var totalSubmissions = await _context.Submissions.CountAsync();

            var pendingGrading = await _context.Submissions.CountAsync(s => s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.Late);
            var gradedCount = await _context.Submissions.CountAsync(s => s.Status == SubmissionStatus.Graded);

            var gradedMarks = await _context.Submissions
                .Where(s => s.MarksAwarded.HasValue)
                .Select(s => (double)s.MarksAwarded!.Value)
                .ToListAsync();

            var avgScore = gradedMarks.Any() ? Math.Round(gradedMarks.Average(), 2) : 0;

            var stats = new DashboardStatsDto
            {
                TotalUsers = totalUsers,
                TotalTeachers = totalTeachers,
                TotalStudents = totalStudents,
                TotalCourses = totalCourses,
                TotalSubjects = totalSubjects,
                TotalAssignments = totalAssignments,
                TotalSubmissions = totalSubmissions,
                PendingSubmissionsCount = pendingGrading,
                GradedSubmissionsCount = gradedCount,
                AverageClassScore = Math.Round(avgScore, 2)
            };

            return Ok(stats);
        }
    }
}
