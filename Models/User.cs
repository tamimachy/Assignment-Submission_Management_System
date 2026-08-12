using System.Text.Json.Serialization;

namespace Assignment_Submission_Management_System.Models
{
    public enum UserRole
    {
        Admin,
        Teacher,
        Student
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;
        
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key for Student -> Course enrollment
        public int? CourseId { get; set; }
        
        [JsonIgnore]
        public Course? Course { get; set; }

        // Navigation properties
        [JsonIgnore]
        public ICollection<Assignment> AssignmentsCreated { get; set; } = new List<Assignment>();
        
        [JsonIgnore]
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();

        [JsonIgnore]
        public ICollection<Subject> SubjectsTaught { get; set; } = new List<Subject>();
    }
}
