using System.Security.Claims;
using Assignment_Submission_Management_System.Controllers.Api;
using Assignment_Submission_Management_System.Data;
using Assignment_Submission_Management_System.DTOs;
using Assignment_Submission_Management_System.Models;
using Assignment_Submission_Management_System.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Assignment_Submission_Management_System.Tests
{
    public class UnitTests : IDisposable
    {
        private readonly SqliteConnection _connection;

        public UnitTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
        }

        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }

        private AuthService GetAuthService()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"Jwt:Key", "Super_Secret_Key_For_Testing_Purposes_32ByteLength!"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            return new AuthService(configuration);
        }

        private void SetUserContext(ControllerBase controller, int userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public void AuthService_HashAndVerifyPassword_ReturnsTrueForValidPassword()
        {
            var service = GetAuthService();
            var password = "SecurePassword123!";

            var hash = service.HashPassword(password);
            var isValid = service.VerifyPassword(password, hash);
            var isInvalid = service.VerifyPassword("WrongPassword", hash);

            Assert.True(isValid);
            Assert.False(isInvalid);
        }

        [Fact]
        public void AuthService_GenerateJwtToken_ReturnsValidStringToken()
        {
            var service = GetAuthService();
            var user = new User
            {
                Id = 1,
                Name = "Test Admin",
                Email = "admin@test.com",
                Role = UserRole.Admin
            };

            var token = service.GenerateJwtToken(user);

            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public async Task Student_CannotSeeDraftAssignments()
        {
            using var context = GetInMemoryDbContext();

            var course = new Course { Id = 1, Name = "CS", Code = "CS101" };
            var teacher = new User { Id = 20, Name = "Teacher", Email = "t@t.com", Role = UserRole.Teacher };
            var subject = new Subject { Id = 1, Name = "Web", Code = "WEB", CourseId = 1, TeacherId = 20 };
            var student = new User { Id = 10, Name = "Student", Email = "s@s.com", Role = UserRole.Student, CourseId = 1 };

            var draftAssignment = new Assignment
            {
                Id = 1,
                Title = "Draft Assignment",
                IsDraft = true,
                SubjectId = 1,
                TeacherId = 20,
                Deadline = DateTime.UtcNow.AddDays(5),
                MaximumMarks = 100
            };

            var publishedAssignment = new Assignment
            {
                Id = 2,
                Title = "Published Assignment",
                IsDraft = false,
                SubjectId = 1,
                TeacherId = 20,
                Deadline = DateTime.UtcNow.AddDays(5),
                MaximumMarks = 100
            };

            context.Courses.Add(course);
            context.Users.AddRange(teacher, student);
            context.Subjects.Add(subject);
            context.Assignments.AddRange(draftAssignment, publishedAssignment);
            await context.SaveChangesAsync();

            var controller = new AssignmentsController(context);
            SetUserContext(controller, student.Id, UserRole.Student.ToString());

            var result = await controller.GetAssignments();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var assignments = Assert.IsType<List<AssignmentResponseDto>>(okResult.Value);

            Assert.Single(assignments);
            Assert.Equal("Published Assignment", assignments[0].Title);
        }

        [Fact]
        public async Task Submission_SubmittedAfterDeadline_IsMarkedLate()
        {
            using var context = GetInMemoryDbContext();

            var course = new Course { Id = 1, Name = "CS", Code = "CS101" };
            var teacher = new User { Id = 1, Name = "Teacher", Email = "t@t.com", Role = UserRole.Teacher };
            var student = new User { Id = 5, Name = "Student", Email = "s@s.com", Role = UserRole.Student, CourseId = 1 };
            var subject = new Subject { Id = 1, Name = "Math", Code = "M101", CourseId = 1, TeacherId = 1 };

            var assignment = new Assignment
            {
                Id = 1,
                Title = "Past Assignment",
                Deadline = DateTime.UtcNow.AddDays(-2),
                MaximumMarks = 100,
                IsDraft = false,
                SubjectId = 1,
                TeacherId = 1
            };

            context.Courses.Add(course);
            context.Users.AddRange(teacher, student);
            context.Subjects.Add(subject);
            context.Assignments.Add(assignment);
            await context.SaveChangesAsync();

            var controller = new SubmissionsController(context);
            SetUserContext(controller, student.Id, UserRole.Student.ToString());

            var dto = new CreateSubmissionDto
            {
                AssignmentId = 1,
                AnswerContent = "My late answer"
            };

            var result = await controller.SubmitAssignment(dto);
            var createdAtAction = Assert.IsType<CreatedAtActionResult>(result);
            var submission = Assert.IsType<Submission>(createdAtAction.Value);

            Assert.Equal(SubmissionStatus.Late, submission.Status);
        }

        [Fact]
        public async Task GradeSubmission_ExceedingMaxMarks_ReturnsBadRequest()
        {
            using var context = GetInMemoryDbContext();

            var course = new Course { Id = 1, Name = "CS", Code = "CS101" };
            var teacher = new User { Id = 1, Name = "Teacher", Email = "t@t.com", Role = UserRole.Teacher };
            var student = new User { Id = 2, Name = "Student", Email = "s@s.com", Role = UserRole.Student, CourseId = 1 };
            var subject = new Subject { Id = 1, Name = "Physics", Code = "P101", CourseId = 1, TeacherId = 1 };

            var assignment = new Assignment
            {
                Id = 1,
                Title = "Test Assignment",
                MaximumMarks = 50,
                Deadline = DateTime.UtcNow.AddDays(5),
                SubjectId = 1,
                TeacherId = 1
            };

            var submission = new Submission
            {
                Id = 1,
                AssignmentId = 1,
                StudentId = 2,
                AnswerContent = "Answer",
                Status = SubmissionStatus.Submitted
            };

            context.Courses.Add(course);
            context.Users.AddRange(teacher, student);
            context.Subjects.Add(subject);
            context.Assignments.Add(assignment);
            context.Submissions.Add(submission);
            await context.SaveChangesAsync();

            var controller = new SubmissionsController(context);
            SetUserContext(controller, teacher.Id, UserRole.Teacher.ToString());

            var dto = new GradeSubmissionDto
            {
                MarksAwarded = 75, // Exceeds max 50
                Feedback = "Over-rated marks"
            };

            var result = await controller.GradeSubmission(1, dto);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.NotNull(badRequest.Value);
        }
    }
}
