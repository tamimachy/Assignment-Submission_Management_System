namespace Assignment_Submission_Management_System.Models
{
    public enum SubmissionStatus
    {
        Submitted,
        Graded,
        Late
    }
    public class Submission
    {
        public int Id { get; set; }
        public string AnswerContent { get; set; } // Could be text or a file URL depending on your preference
        public DateTime SubmittedAt { get; set; }
        public SubmissionStatus Status { get; set; }

        // Grading fields
        public int? MarksAwarded { get; set; }
        public string Feedback { get; set; }

        // Foreign Keys
        public int AssignmentId { get; set; }
        public Assignment Assignment { get; set; }

        public int StudentId { get; set; }
        public User Student { get; set; }
    }
}
