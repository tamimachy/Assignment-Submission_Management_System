using Assignment_Submission_Management_System.Models;
using Assignment_Submission_Management_System.Services;
using Microsoft.EntityFrameworkCore;

namespace Assignment_Submission_Management_System.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

            try
            {
                await context.Database.EnsureCreatedAsync();
            }
            catch
            {
                // Fallback handling if DB already setup or created
            }

            // Seed Users if none exist
            if (!await context.Users.AnyAsync())
            {
                var admin = new User
                {
                    Name = "System Admin",
                    Email = "admin@school.com",
                    PasswordHash = authService.HashPassword("Admin123!"),
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow
                };

                var teacher1 = new User
                {
                    Name = "Prof. John Doe",
                    Email = "teacher@school.com",
                    PasswordHash = authService.HashPassword("Teacher123!"),
                    Role = UserRole.Teacher,
                    CreatedAt = DateTime.UtcNow
                };

                var teacher2 = new User
                {
                    Name = "Dr. Sarah Smith",
                    Email = "teacher2@school.com",
                    PasswordHash = authService.HashPassword("Teacher123!"),
                    Role = UserRole.Teacher,
                    CreatedAt = DateTime.UtcNow
                };

                var student1 = new User
                {
                    Name = "Alice Johnson",
                    Email = "student@school.com",
                    PasswordHash = authService.HashPassword("Student123!"),
                    Role = UserRole.Student,
                    CreatedAt = DateTime.UtcNow
                };

                var student2 = new User
                {
                    Name = "Bob Williams",
                    Email = "student2@school.com",
                    PasswordHash = authService.HashPassword("Student123!"),
                    Role = UserRole.Student,
                    CreatedAt = DateTime.UtcNow
                };

                await context.Users.AddRangeAsync(admin, teacher1, teacher2, student1, student2);
                await context.SaveChangesAsync();

                // Seed Courses
                var cseCourse = new Course
                {
                    Name = "Computer Science & Engineering",
                    Code = "CSE-101",
                    Description = "Core fundamentals of computer science and software development.",
                    CreatedAt = DateTime.UtcNow
                };

                var sweCourse = new Course
                {
                    Name = "Software Engineering",
                    Code = "SWE-201",
                    Description = "Advanced software architecture, testing, and lifecycle management.",
                    CreatedAt = DateTime.UtcNow
                };

                await context.Courses.AddRangeAsync(cseCourse, sweCourse);
                await context.SaveChangesAsync();

                // Assign Students to Courses
                student1.CourseId = cseCourse.Id;
                student2.CourseId = sweCourse.Id;
                await context.SaveChangesAsync();

                // Seed Subjects
                var webDevSubject = new Subject
                {
                    Name = "Web Development",
                    Code = "CSE-WEB1",
                    CourseId = cseCourse.Id,
                    TeacherId = teacher1.Id
                };

                var dbSubject = new Subject
                {
                    Name = "Database Systems",
                    Code = "CSE-DB1",
                    CourseId = cseCourse.Id,
                    TeacherId = teacher2.Id
                };

                var archSubject = new Subject
                {
                    Name = "Software Architecture",
                    Code = "SWE-SA1",
                    CourseId = sweCourse.Id,
                    TeacherId = teacher1.Id
                };

                await context.Subjects.AddRangeAsync(webDevSubject, dbSubject, archSubject);
                await context.SaveChangesAsync();

                // Seed Assignments
                var assignment1 = new Assignment
                {
                    Title = "Full-Stack Web App Implementation",
                    Description = "Build a complete Next.js and ASP.NET Core REST API application with authentication.",
                    Deadline = DateTime.UtcNow.AddDays(7),
                    MaximumMarks = 100,
                    IsDraft = false,
                    SubjectId = webDevSubject.Id,
                    TeacherId = teacher1.Id,
                    CreatedAt = DateTime.UtcNow
                };

                var assignment2 = new Assignment
                {
                    Title = "Relational DB Schema & Indexing",
                    Description = "Design a PostgreSQL schema with foreign keys, indexes, and write complex queries.",
                    Deadline = DateTime.UtcNow.AddDays(5),
                    MaximumMarks = 50,
                    IsDraft = false,
                    SubjectId = dbSubject.Id,
                    TeacherId = teacher2.Id,
                    CreatedAt = DateTime.UtcNow
                };

                var assignment3 = new Assignment
                {
                    Title = "Microservices Design Specification (Draft)",
                    Description = "Draft architectural diagram and API contract documentation for microservices.",
                    Deadline = DateTime.UtcNow.AddDays(14),
                    MaximumMarks = 100,
                    IsDraft = true,
                    SubjectId = archSubject.Id,
                    TeacherId = teacher1.Id,
                    CreatedAt = DateTime.UtcNow
                };

                await context.Assignments.AddRangeAsync(assignment1, assignment2, assignment3);
                await context.SaveChangesAsync();

                // Seed Submissions
                var submission1 = new Submission
                {
                    AssignmentId = assignment1.Id,
                    StudentId = student1.Id,
                    AnswerContent = "Implemented full-stack Web Application with ASP.NET Core API backend and Next.js frontend. GitHub repository link included.",
                    AttachmentUrl = "https://github.com/example/assignment-submission",
                    SubmittedAt = DateTime.UtcNow.AddHours(-12),
                    Status = SubmissionStatus.Graded,
                    MarksAwarded = 95,
                    Feedback = "Outstanding project implementation! Code structure and UI design are exemplary."
                };

                await context.Submissions.AddAsync(submission1);
                await context.SaveChangesAsync();
            }
        }
    }
}
