using System.Text.Json.Serialization;

namespace Assignment_Submission_Management_System.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public int MaximumMarks { get; set; }
        public bool IsDraft { get; set; } = false; // Allows draft vs published state
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }

        public int TeacherId { get; set; }
        public User? Teacher { get; set; }

        [JsonIgnore]
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
}
