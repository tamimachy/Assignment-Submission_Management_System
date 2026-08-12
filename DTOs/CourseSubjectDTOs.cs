using System.ComponentModel.DataAnnotations;

namespace Assignment_Submission_Management_System.DTOs
{
    public class CreateCourseDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    public class UpdateCourseDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    public class CourseResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int SubjectsCount { get; set; }
        public int StudentsCount { get; set; }
        public List<SubjectResponseDto> Subjects { get; set; } = new List<SubjectResponseDto>();
    }

    public class CreateSubjectDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public int CourseId { get; set; }

        public int? TeacherId { get; set; }
    }

    public class UpdateSubjectDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public int CourseId { get; set; }

        public int? TeacherId { get; set; }
    }

    public class SubjectResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; }
    }

    public class AssignTeacherDto
    {
        public int? TeacherId { get; set; }
    }

    public class EnrollStudentDto
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
    }
}
