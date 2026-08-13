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
    public class SubmissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SubmissionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetSubmissions([FromQuery] int? assignmentId = null, [FromQuery] int? studentId = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            int.TryParse(userIdClaim, out int userId);
            var userRoleClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value ?? string.Empty;

            var isStudent = User.IsInRole("Student") || User.IsInRole("2") || userRoleClaim.Equals("Student", StringComparison.OrdinalIgnoreCase) || userRoleClaim == "2";
            var isTeacher = User.IsInRole("Teacher") || User.IsInRole("1") || userRoleClaim.Equals("Teacher", StringComparison.OrdinalIgnoreCase) || userRoleClaim == "1";

            var query = _context.Submissions
                .Include(s => s.Assignment)
                .Include(s => s.Student)
                .AsQueryable();

            if (isStudent)
            {
                query = query.Where(s => s.StudentId == userId);
            }
            else if (isTeacher)
            {
                query = query.Where(s => s.Assignment.TeacherId == userId || s.Assignment.Subject.TeacherId == userId);
            }

            if (assignmentId.HasValue)
            {
                query = query.Where(s => s.AssignmentId == assignmentId.Value);
            }

            if (studentId.HasValue && !isStudent)
            {
                query = query.Where(s => s.StudentId == studentId.Value);
            }

            var submissions = await query.Select(s => new SubmissionResponseDto
            {
                Id = s.Id,
                AssignmentId = s.AssignmentId,
                AssignmentTitle = s.Assignment.Title,
                MaximumMarks = s.Assignment.MaximumMarks,
                Deadline = s.Assignment.Deadline,
                StudentId = s.StudentId,
                StudentName = s.Student.Name,
                StudentEmail = s.Student.Email,
                AnswerContent = s.AnswerContent,
                AttachmentUrl = s.AttachmentUrl,
                SubmittedAt = s.SubmittedAt,
                UpdatedAt = s.UpdatedAt,
                Status = s.Status.ToString(),
                MarksAwarded = s.MarksAwarded,
                Feedback = s.Feedback
            }).ToListAsync();

            return Ok(submissions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubmission(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            int.TryParse(userIdClaim, out int userId);
            var userRoleClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value ?? string.Empty;

            var isStudent = User.IsInRole("Student") || User.IsInRole("2") || userRoleClaim.Equals("Student", StringComparison.OrdinalIgnoreCase) || userRoleClaim == "2";

            var s = await _context.Submissions
                .Include(s => s.Assignment)
                .Include(s => s.Student)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (s == null) return NotFound(new { message = "Submission not found" });

            if (isStudent && s.StudentId != userId)
            {
                return Forbid();
            }

            var dto = new SubmissionResponseDto
            {
                Id = s.Id,
                AssignmentId = s.AssignmentId,
                AssignmentTitle = s.Assignment.Title,
                MaximumMarks = s.Assignment.MaximumMarks,
                Deadline = s.Assignment.Deadline,
                StudentId = s.StudentId,
                StudentName = s.Student.Name,
                StudentEmail = s.Student.Email,
                AnswerContent = s.AnswerContent,
                AttachmentUrl = s.AttachmentUrl,
                SubmittedAt = s.SubmittedAt,
                UpdatedAt = s.UpdatedAt,
                Status = s.Status.ToString(),
                MarksAwarded = s.MarksAwarded,
                Feedback = s.Feedback
            };

            return Ok(dto);
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> SubmitAssignment([FromBody] CreateSubmissionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdClaim, out int userId);

            var assignment = await _context.Assignments.FindAsync(dto.AssignmentId);
            if (assignment == null) return NotFound(new { message = "Assignment not found" });

            if (assignment.IsDraft)
            {
                return BadRequest(new { message = "Cannot submit to a draft assignment" });
            }

            var existingSubmission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignmentId == dto.AssignmentId && s.StudentId == userId);

            if (existingSubmission != null)
            {
                return BadRequest(new { message = "You have already submitted for this assignment. Please edit your submission." });
            }

            var deadlineUtc = DateTime.SpecifyKind(assignment.Deadline, DateTimeKind.Utc);
            var nowUtc = DateTime.UtcNow;
            var isLate = nowUtc > deadlineUtc;

            var submission = new Submission
            {
                AssignmentId = dto.AssignmentId,
                StudentId = userId,
                AnswerContent = dto.AnswerContent,
                AttachmentUrl = dto.AttachmentUrl,
                SubmittedAt = nowUtc,
                Status = isLate ? SubmissionStatus.Late : SubmissionStatus.Submitted
            };

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSubmission), new { id = submission.Id }, submission);
        }

        [Authorize(Roles = "Student")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubmission(int id, [FromBody] UpdateSubmissionDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            int.TryParse(userIdClaim, out int userId);

            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null) return NotFound(new { message = "Submission not found" });

            if (submission.StudentId != userId)
            {
                return Forbid();
            }

            var deadlineUtc = DateTime.SpecifyKind(submission.Assignment.Deadline, DateTimeKind.Utc);
            var nowUtc = DateTime.UtcNow;

            if (nowUtc > deadlineUtc)
            {
                return BadRequest(new { message = "Cannot edit submission after the deadline has passed." });
            }

            submission.AnswerContent = dto.AnswerContent;
            submission.AttachmentUrl = dto.AttachmentUrl;
            submission.UpdatedAt = nowUtc;

            // Recalculate status if deadline was extended or submission is on time
            if (submission.Status == SubmissionStatus.Late && submission.SubmittedAt <= deadlineUtc)
            {
                submission.Status = submission.MarksAwarded.HasValue ? SubmissionStatus.Graded : SubmissionStatus.Submitted;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Submission updated successfully" });
        }

        [Authorize(Roles = "Teacher,Admin")]
        [HttpPut("{id}/grade")]
        public async Task<IActionResult> GradeSubmission(int id, [FromBody] GradeSubmissionDto dto)
        {
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null) return NotFound(new { message = "Submission not found" });

            if (dto.MarksAwarded > submission.Assignment.MaximumMarks)
            {
                return BadRequest(new { message = $"Marks awarded ({dto.MarksAwarded}) cannot exceed maximum marks ({submission.Assignment.MaximumMarks})" });
            }

            submission.MarksAwarded = dto.MarksAwarded;
            submission.Feedback = dto.Feedback;
            submission.Status = dto.Status;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Submission graded successfully" });
        }
    }
}
