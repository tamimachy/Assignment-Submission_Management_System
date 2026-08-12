using System.ComponentModel.DataAnnotations;
using Assignment_Submission_Management_System.Models;

namespace Assignment_Submission_Management_System.DTOs
{
    public class CreateSubmissionDto
    {
        [Required]
        public int AssignmentId { get; set; }

        [Required]
        public string AnswerContent { get; set; } = string.Empty;

        public string? AttachmentUrl { get; set; }
    }

    public class UpdateSubmissionDto
    {
        [Required]
        public string AnswerContent { get; set; } = string.Empty;

        public string? AttachmentUrl { get; set; }
    }

    public class GradeSubmissionDto
    {
        [Range(0, 1000)]
        public int MarksAwarded { get; set; }

        public string? Feedback { get; set; }

        public SubmissionStatus Status { get; set; } = SubmissionStatus.Graded;
    }

    public class SubmissionResponseDto
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public int MaximumMarks { get; set; }
        public DateTime Deadline { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string AnswerContent { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? MarksAwarded { get; set; }
        public string? Feedback { get; set; }
    }
}
