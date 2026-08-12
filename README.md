# Assignment & Submission Management System

A unified C# ASP.NET Core Web Application combining REST APIs, EF Core Database Persistence, and HTML/Razor Views for a role-based school/college assignment portal.

---

## 🌟 Overview

The **Assignment & Submission Management System** is built entirely with C# and ASP.NET Core MVC & Web APIs:
- **Teachers** can create, publish, or draft assignments, assign them to specific courses and subjects, set deadlines, view student submissions, and award marks with feedback.
- **Students** can view published assignments for their enrolled course, submit solutions with text and optional repository links before deadlines, track submission status, and review marks and teacher feedback.
- **Admins** can manage users, courses, subjects, assign teachers to subjects, and view all system activity.

---

## 🚀 How to Run the Application (Single Command!)

You do **not** need Node.js, `npm`, or XML build scripts. Everything is handled natively by ASP.NET Core!

### 1. Open Terminal in the Root Directory
```powershell
dotnet run
```

### 2. Open in Browser
👉 **[http://localhost:5000](http://localhost:5233)** (or `https://localhost:5000`)

> 💡 **Automatic Database Setup**: The application automatically creates and seeds the SQLite database (`assignment_mgmt.db`) with working demo data on launch.

---

## 🔑 Demo Credentials

Click the quick demo buttons on the login page or enter:

| Role | Email Address | Password | Enrolled / Assigned Scope |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@school.com` | `Admin123!` | Global administrative access |
| **Teacher** | `teacher@school.com` | `Teacher123!` | Web Development & Software Architecture |
| **Student** | `student@school.com` | `Student123!` | Computer Science & Engineering (CSE-101) |

---

## 🧪 How to Run Unit Tests

To run the xUnit test suite (covering business rules, authorization, and submission workflows):

```powershell
dotnet test Assignment_Submission_Management_System.Tests/Assignment_Submission_Management_System.Tests.csproj
```

---

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core MVC & Web API (.NET 10 / C#)
- **Frontend Views**: HTML5, Tailwind CSS, FontAwesome Icons, JavaScript (Fetch API)
- **Database**: PostgreSQL / SQLite (`Npgsql` / `Microsoft.EntityFrameworkCore.Sqlite`)
- **Authentication**: JWT Bearer Tokens & Role-Based Authorization
- **Documentation**: Swagger / OpenAPI (accessible at `http://localhost:5000/swagger`)
- **Testing**: xUnit with EF Core In-Memory / SQLite Test Fixture

---

## 📁 Project Structure

```
.
├── Controllers/
│   ├── HomeController.cs             # Serves HTML Views (Index, Privacy)
│   └── Api/
│       ├── AssignmentsController.cs  # Assignment CRUD & role filtering REST API
│       ├── AuthController.cs         # JWT Login, Register, GetMe REST API
│       ├── CoursesController.cs      # Courses, Subjects & Teacher assignments REST API
│       ├── DashboardController.cs    # Aggregate metrics & statistics REST API
│       ├── SubmissionsController.cs   # Submissions & teacher grading REST API
│       └── UsersController.cs        # User account management REST API
├── Data/
│   ├── ApplicationDbContext.cs      # EF Core DbContext with model configurations
│   └── DbInitializer.cs             # Auto-creation & demo data seed initializer
├── Views/
│   ├── Home/
│   │   └── Index.cshtml             # Full-stack Portal HTML View
│   └── Shared/
│       └── _Layout.cshtml           # Master Layout template
├── Models/                          # Domain models (User, Course, Subject, Assignment, Submission)
├── Assignment_Submission_Management_System.Tests/
│   └── UnitTests.cs                 # xUnit test suite
├── database.sql                     # PostgreSQL / SQLite SQL schema script
└── Program.cs                       # Web API entry point & service setup
```
