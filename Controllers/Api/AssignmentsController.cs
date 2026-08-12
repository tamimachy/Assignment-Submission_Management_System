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
    public class AssignmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AssignmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAssignments([FromQuery] int? subjectId = null, [FromQuery] int? courseId = null, [FromQuery] bool? isDraft = null)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdClaim, out int userId);

            var query = _context.Assignments
                .Include(a => a.Subject)
                    .ThenInclude(s => s.Course)
                .Include(a => a.Teacher)
                .Include(a => a.Submissions)
                .AsQueryable();

            // Role filtering
            if (userRole == UserRole.Student.ToString())
            {
                // Students only see published assignments (IsDraft == false) for their enrolled course
                var student = await _context.Users.FindAsync(userId);
                if (student?.CourseId != null)
                {
                    query = query.Where(a => a.Subject.CourseId == student.CourseId.Value && !a.IsDraft);
                }
                else
                {
                    // Student not enrolled in any course
                    return Ok(new List<AssignmentResponseDto>());
                }
            }
            else if (userRole == UserRole.Teacher.ToString())
            {
                // Teachers see assignments they created or assigned to their subjects
                query = query.Where(a => a.TeacherId == userId || a.Subject.TeacherId == userId);
            }
            // Admin sees all

            if (subjectId.HasValue)
            {
                query = query.Where(a => a.SubjectId == subjectId.Value);
            }

            if (courseId.HasValue)
            {
                query = query.Where(a => a.Subject.CourseId == courseId.Value);
            }

            if (isDraft.HasValue)
            {
                query = query.Where(a => a.IsDraft == isDraft.Value);
            }

            var assignmentsList = await query.ToListAsync();

            var assignments = assignmentsList.Select(a => new AssignmentResponseDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Deadline = a.Deadline,
                MaximumMarks = a.MaximumMarks,
                IsDraft = a.IsDraft,
                CreatedAt = a.CreatedAt,
                SubjectId = a.SubjectId,
                SubjectName = a.Subject?.Name ?? "",
                CourseId = a.Subject?.CourseId ?? 0,
                CourseName = a.Subject?.Course?.Name ?? "",
                TeacherId = a.TeacherId,
                TeacherName = a.Teacher?.Name ?? "",
                SubmissionsCount = a.Submissions?.Count ?? 0,
                MySubmission = userRole == UserRole.Student.ToString() && a.Submissions != null
                    ? a.Submissions.Where(s => s.StudentId == userId).Select(s => new SubmissionResponseDto
                    {
                        Id = s.Id,
                        AssignmentId = s.AssignmentId,
                        AssignmentTitle = a.Title,
                        MaximumMarks = a.MaximumMarks,
                        Deadline = a.Deadline,
                        StudentId = s.StudentId,
                        StudentName = s.Student?.Name ?? "",
                        StudentEmail = s.Student?.Email ?? "",
                        AnswerContent = s.AnswerContent,
                        AttachmentUrl = s.AttachmentUrl,
                        SubmittedAt = s.SubmittedAt,
                        UpdatedAt = s.UpdatedAt,
                        Status = s.Status.ToString(),
                        MarksAwarded = s.MarksAwarded,
                        Feedback = s.Feedback
                    }).FirstOrDefault()
                    : null
            }).ToList();

            return Ok(assignments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssignment(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdClaim, out int userId);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var a = await _context.Assignments
                .Include(a => a.Subject)
                    .ThenInclude(s => s.Course)
                .Include(a => a.Teacher)
                .Include(a => a.Submissions)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (a == null) return NotFound(new { message = "Assignment not found" });

            // Draft restriction for students
            if (userRole == UserRole.Student.ToString() && a.IsDraft)
            {
                return NotFound(new { message = "Assignment not found" });
            }

            var dto = new AssignmentResponseDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Deadline = a.Deadline,
                MaximumMarks = a.MaximumMarks,
                IsDraft = a.IsDraft,
                CreatedAt = a.CreatedAt,
                SubjectId = a.SubjectId,
                SubjectName = a.Subject.Name,
                CourseId = a.Subject.CourseId,
                CourseName = a.Subject.Course.Name,
                TeacherId = a.TeacherId,
                TeacherName = a.Teacher.Name,
                SubmissionsCount = a.Submissions.Count,
                MySubmission = userRole == UserRole.Student.ToString()
                    ? a.Submissions.Where(s => s.StudentId == userId).Select(s => new SubmissionResponseDto
                    {
                        Id = s.Id,
                        AssignmentId = s.AssignmentId,
                        AssignmentTitle = a.Title,
                        MaximumMarks = a.MaximumMarks,
                        Deadline = a.Deadline,
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
                    }).FirstOrDefault()
                    : null
            };

            return Ok(dto);
        }

        [Authorize(Roles = "Teacher,Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdClaim, out int userId);

            var subject = await _context.Subjects.FindAsync(dto.SubjectId);
            if (subject == null) return BadRequest(new { message = "Invalid SubjectId" });

            var assignment = new Assignment
            {
                Title = dto.Title,
                Description = dto.Description,
                Deadline = dto.Deadline.ToUniversalTime(),
                MaximumMarks = dto.MaximumMarks,
                IsDraft = dto.IsDraft,
                SubjectId = dto.SubjectId,
                TeacherId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAssignment), new { id = assignment.Id }, assignment);
        }

        [Authorize(Roles = "Teacher,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAssignment(int id, [FromBody] UpdateAssignmentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null) return NotFound(new { message = "Assignment not found" });

            assignment.Title = dto.Title;
            assignment.Description = dto.Description;
            assignment.Deadline = dto.Deadline.ToUniversalTime();
            assignment.MaximumMarks = dto.MaximumMarks;
            assignment.IsDraft = dto.IsDraft;
            assignment.SubjectId = dto.SubjectId;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Assignment updated successfully" });
        }

        [Authorize(Roles = "Teacher,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null) return NotFound(new { message = "Assignment not found" });

            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Assignment deleted successfully" });
        }
    }
}
