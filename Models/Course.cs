using System.Text.Json.Serialization;

namespace Assignment_Submission_Management_System.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
        
        [JsonIgnore]
        public ICollection<User> Students { get; set; } = new List<User>();
    }
}
