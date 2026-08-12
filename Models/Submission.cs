using System.Text.Json.Serialization;

namespace Assignment_Submission_Management_System.Models
{
    public enum SubmissionStatus
    {
        Submitted,
        Graded,
        Late,
        NeedsRevision
    }

    public class Submission
    {
        public int Id { get; set; }
        public string AnswerContent { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;

        // Grading fields
        public int? MarksAwarded { get; set; }
        public string? Feedback { get; set; }

        // Foreign Keys
        public int AssignmentId { get; set; }
        public Assignment? Assignment { get; set; }

        public int StudentId { get; set; }
        public User? Student { get; set; }
    }
}
