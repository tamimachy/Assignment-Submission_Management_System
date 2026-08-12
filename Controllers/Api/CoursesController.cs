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
    public class CoursesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses()
        {
            var coursesList = await _context.Courses
                .Include(c => c.Subjects)
                    .ThenInclude(s => s.Teacher)
                .Include(c => c.Students)
                .ToListAsync();

            var dtos = coursesList.Select(c => new CourseResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                SubjectsCount = c.Subjects.Count,
                StudentsCount = c.Students.Count,
                Subjects = c.Subjects.Select(s => new SubjectResponseDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Code = s.Code,
                    CourseId = s.CourseId,
                    CourseName = c.Name,
                    TeacherId = s.TeacherId,
                    TeacherName = s.Teacher != null ? s.Teacher.Name : null
                }).ToList()
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourse(int id)
        {
            var c = await _context.Courses
                .Include(c => c.Subjects)
                    .ThenInclude(s => s.Teacher)
                .Include(c => c.Students)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (c == null) return NotFound(new { message = "Course not found" });

            var dto = new CourseResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                SubjectsCount = c.Subjects.Count,
                StudentsCount = c.Students.Count,
                Subjects = c.Subjects.Select(s => new SubjectResponseDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Code = s.Code,
                    CourseId = s.CourseId,
                    CourseName = c.Name,
                    TeacherId = s.TeacherId,
                    TeacherName = s.Teacher != null ? s.Teacher.Name : null
                }).ToList()
            };

            return Ok(dto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var course = new Course
            {
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto dto)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound(new { message = "Course not found" });

            course.Name = dto.Name;
            course.Code = dto.Code;
            course.Description = dto.Description;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Course updated successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound(new { message = "Course not found" });

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Course deleted successfully" });
        }

        // --- SUBJECTS ENDPOINTS ---

        [HttpGet("subjects")]
        public async Task<IActionResult> GetSubjects([FromQuery] int? courseId = null, [FromQuery] int? teacherId = null)
        {
            var query = _context.Subjects
                .Include(s => s.Course)
                .Include(s => s.Teacher)
                .AsQueryable();

            if (courseId.HasValue)
            {
                query = query.Where(s => s.CourseId == courseId.Value);
            }

            if (teacherId.HasValue)
            {
                query = query.Where(s => s.TeacherId == teacherId.Value);
            }

            var subjects = await query.Select(s => new SubjectResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code,
                CourseId = s.CourseId,
                CourseName = s.Course != null ? s.Course.Name : "",
                TeacherId = s.TeacherId,
                TeacherName = s.Teacher != null ? s.Teacher.Name : null
            }).ToListAsync();

            return Ok(subjects);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("subjects")]
        public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var subject = new Subject
            {
                Name = dto.Name,
                Code = dto.Code,
                CourseId = dto.CourseId,
                TeacherId = dto.TeacherId
            };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            return Ok(subject);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("subjects/{id}")]
        public async Task<IActionResult> UpdateSubject(int id, [FromBody] UpdateSubjectDto dto)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) return NotFound(new { message = "Subject not found" });

            subject.Name = dto.Name;
            subject.Code = dto.Code;
            subject.CourseId = dto.CourseId;
            subject.TeacherId = dto.TeacherId;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Subject updated successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("subjects/{id}/assign-teacher")]
        public async Task<IActionResult> AssignTeacher(int id, [FromBody] AssignTeacherDto dto)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) return NotFound(new { message = "Subject not found" });

            if (dto.TeacherId.HasValue)
            {
                var teacher = await _context.Users.FindAsync(dto.TeacherId.Value);
                if (teacher == null || teacher.Role != UserRole.Teacher)
                {
                    return BadRequest(new { message = "User is not a valid Teacher" });
                }
            }

            subject.TeacherId = dto.TeacherId;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Teacher assigned successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("subjects/{id}")]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) return NotFound(new { message = "Subject not found" });

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Subject deleted successfully" });
        }
    }
}
