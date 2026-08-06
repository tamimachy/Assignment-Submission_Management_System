using static Assignment_Submission_Management_System.Models.AssignSubject;

namespace Assignment_Submission_Management_System.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public int MaximumMarks { get; set; }
        public bool IsDraft { get; set; } // Allows keeping it as a draft before publishing

        // Foreign Keys
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }

        public int TeacherId { get; set; }
        public User Teacher { get; set; }

        public ICollection<Submission> Submissions { get; set; }
    }
}
