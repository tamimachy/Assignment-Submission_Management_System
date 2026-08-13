# Assignment & Submission Management System

A unified C# ASP.NET Core Web Application combining REST APIs, EF Core Database Persistence, and interactive HTML/Razor Views for a role-based school/college assignment portal.

---

##  Features & Role-Based Workflows

###  Student Portal
- **Personalized Overview Dashboard**: Track **Assigned Assignments**, **Completed Assignments**, **Incompleted Assignments**, and **Average Grade Score**.
- **Instant Assignment Sync**: View published assignments for enrolled courses with real-time deadline status and maximum marks.
- **Answer Submission & Solution Editing**: Submit solutions with text content and repository links before deadlines. Easily update existing solutions before deadlines without duplicate errors.
- **Strict Privacy**: Students can only view their own submissions and grades.

###  Teacher Portal
- **Teacher Metrics Dashboard**: Track **Assigned Assignments**, **Completed Submissions**, **Pending Grading**, and **Average Class Score**.
- **Course & Subject Management**: View assigned academic courses, departments, and assigned subject lists.
- **Assignment Creation & Management**: Create, edit, publish, or draft assignments with customizable deadlines and max marks.
- **Grading & Feedback**: Review student submissions, assign marks, and provide detailed teacher feedback.

###  Admin Portal
- **System-Wide Dashboard**: Track total assignments, total submissions, overall system average score, and total users/courses count.
- **User Administration**: Create, edit, and delete user accounts (Admins, Teachers, Students) with course assignments and optional password updates.
- **Academic Hierarchy Management**: Create academic courses and subjects, and assign teachers to specific subjects.
- **Courses & Subjects Overview**: View all active courses, enrolled student counts, and assigned faculty directly from the Overview section.

---

##  How to Run the Application (Single Command!)

You do **not** need Node.js, `npm`, or complex build scripts. Everything is handled natively by ASP.NET Core!

### 1. Open Terminal in the Root Directory
```powershell
dotnet run
```

### 2. Open in Browser
   **[http://localhost:5233](http://localhost:5233)** (or `https://localhost:5000`)

>  **Automatic Database Setup**: The application automatically creates and seeds the SQLite database (`assignment_mgmt.db`) with working demo data on launch.

---

##  Demo Credentials

Click the quick demo buttons on the login page or enter:

| Role | Email Address | Password | Enrolled / Assigned Scope |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@school.com` | `Admin123!` | Global administrative access |
| **Teacher** | `teacher@school.com` | `Teacher123!` | Web Development & Software Architecture |
| **Student** | `student@school.com` | `Student123!` | Computer Science & Engineering (CSE-101) |

---

##  How to Run Unit Tests

To run the xUnit test suite (covering business rules, authorization, and submission workflows):

```powershell
dotnet test Assignment_Submission_Management_System.Tests/Assignment_Submission_Management_System.Tests.csproj
```

---

##  Technology Stack

- **Framework**: ASP.NET Core MVC & Web API (.NET 10 / C#)
- **Frontend Views**: HTML5, Tailwind CSS, FontAwesome Icons, JavaScript (Fetch API)
- **Database**: PostgreSQL / SQLite (`Npgsql` / `Microsoft.EntityFrameworkCore.Sqlite`)
- **Authentication**: JWT Bearer Tokens & Role-Based Authorization
- **Documentation**: Swagger / OpenAPI (accessible at `http://localhost:5000/swagger`)
- **Testing**: xUnit with EF Core In-Memory / SQLite Test Fixture

---

##  Project Structure

```
.
├── Controllers/
│   ├── HomeController.cs             # Serves HTML Views (Index, Privacy)
│   └── Api/
│       ├── AssignmentsController.cs  # Assignment CRUD, deadline status & role filtering REST API
│       ├── AuthController.cs         # JWT Login, Register, GetMe REST API
│       ├── CoursesController.cs      # Courses, Subjects & Teacher assignments REST API
│       ├── DashboardController.cs    # Personalized user stats & system metrics REST API
│       ├── SubmissionsController.cs   # Submissions, student scoping & teacher grading REST API
│       └── UsersController.cs        # User account management REST API
├── Data/
│   ├── ApplicationDbContext.cs      # EF Core DbContext with model configurations
│   └── DbInitializer.cs             # Auto-creation & demo data seed initializer
├── DTOs/                             # Data Transfer Objects (AuthDTOs, AssignmentDTOs, DashboardDTOs)
├── Views/
│   ├── Home/
│   │   └── Index.cshtml             # Full-stack Portal HTML View with real-time UI updates
│   └── Shared/
│       └── _Layout.cshtml           # Master Layout template
├── Models/                          # Domain models (User, Course, Subject, Assignment, Submission)
├── Assignment_Submission_Management_System.Tests/
│   └── UnitTests.cs                 # xUnit test suite
├── database.sql                     # PostgreSQL / SQLite SQL schema script
└── Program.cs                       # Web API entry point & service setup
```
