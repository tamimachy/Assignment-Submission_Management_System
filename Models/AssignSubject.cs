namespace Assignment_Submission_Management_System.Models
{
    public class AssignSubject
    {
        public class Course
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public ICollection<Subject> Subjects { get; set; }
        }
        public class Subject
        {
            public int Id { get; set; }
            public string Name { get; set; }

            // Foreign Key
            public int CourseId { get; set; }
            public Course Course { get; set; }

            // Teacher assigned to the subject
            public int? TeacherId { get; set; }
            public User Teacher { get; set; }

            public ICollection<Assignment> Assignments { get; set; }
        }
    }
}
