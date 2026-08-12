using System.Text.Json.Serialization;

namespace Assignment_Submission_Management_System.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;

        // Foreign Key to Course
        public int CourseId { get; set; }
        
        [JsonIgnore]
        public Course? Course { get; set; }

        // Foreign Key to Teacher (User)
        public int? TeacherId { get; set; }
        
        public User? Teacher { get; set; }

        // Navigation property
        [JsonIgnore]
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}
