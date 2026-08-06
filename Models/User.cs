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
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } // Will store the hashed password
        public UserRole Role { get; set; }

        // Navigation properties
        public ICollection<Assignment> AssignmentsCreated { get; set; }
        public ICollection<Submission> Submissions { get; set; }
    }
}
