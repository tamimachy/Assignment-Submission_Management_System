namespace Assignment_Submission_Management_System.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalCourses { get; set; }
        public int TotalSubjects { get; set; }
        public int TotalAssignments { get; set; }
        public int TotalSubmissions { get; set; }
        public int PendingSubmissionsCount { get; set; }
        public int GradedSubmissionsCount { get; set; }
        public double AverageClassScore { get; set; }
    }

    public class UserDashboardStatsDto
    {
        public int TotalAssignedAssignments { get; set; }
        public int CompletedAssignments { get; set; }
        public int IncompletedAssignments { get; set; }
        public double AverageScore { get; set; }
        public int PendingGradingCount { get; set; }
        public int TotalUsers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalAssignments { get; set; }
        public int TotalSubmissions { get; set; }
        public double AverageClassScore { get; set; }
    }
}
