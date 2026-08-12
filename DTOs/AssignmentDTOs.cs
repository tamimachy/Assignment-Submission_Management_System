using System.ComponentModel.DataAnnotations;

namespace Assignment_Submission_Management_System.DTOs
{
    public class CreateAssignmentDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime Deadline { get; set; }

        [Range(1, 1000)]
        public int MaximumMarks { get; set; } = 100;

        public bool IsDraft { get; set; } = false;

        [Required]
        public int SubjectId { get; set; }
    }

    public class UpdateAssignmentDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime Deadline { get; set; }

        [Range(1, 1000)]
        public int MaximumMarks { get; set; } = 100;

        public bool IsDraft { get; set; } = false;

        [Required]
        public int SubjectId { get; set; }
    }

    public class AssignmentResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public int MaximumMarks { get; set; }
        public bool IsDraft { get; set; }
        public DateTime CreatedAt { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int SubmissionsCount { get; set; }
        public SubmissionResponseDto? MySubmission { get; set; }
    }
}
