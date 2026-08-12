-- ==========================================================
-- Assignment & Submission Management System Database Script
-- Compatible with PostgreSQL & SQLite
-- ==========================================================

-- 1. Users Table
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Email" VARCHAR(100) NOT NULL UNIQUE,
    "PasswordHash" TEXT NOT NULL,
    "Role" INT NOT NULL, -- 0: Admin, 1: Teacher, 2: Student
    "CourseId" INT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 2. Courses Table
CREATE TABLE IF NOT EXISTS "Courses" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Code" VARCHAR(50) NOT NULL UNIQUE,
    "Description" TEXT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 3. Subjects Table
CREATE TABLE IF NOT EXISTS "Subjects" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Code" VARCHAR(50) NOT NULL,
    "CourseId" INT NOT NULL,
    "TeacherId" INT NULL,
    CONSTRAINT "FK_Subjects_Courses" FOREIGN KEY ("CourseId") REFERENCES "Courses"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Subjects_Users" FOREIGN KEY ("TeacherId") REFERENCES "Users"("Id") ON DELETE SET NULL
);

-- 4. Assignments Table
CREATE TABLE IF NOT EXISTS "Assignments" (
    "Id" SERIAL PRIMARY KEY,
    "Title" VARCHAR(200) NOT NULL,
    "Description" TEXT NOT NULL,
    "Deadline" TIMESTAMP NOT NULL,
    "MaximumMarks" INT NOT NULL,
    "IsDraft" BOOLEAN NOT NULL DEFAULT FALSE,
    "SubjectId" INT NOT NULL,
    "TeacherId" INT NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_Assignments_Subjects" FOREIGN KEY ("SubjectId") REFERENCES "Subjects"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Assignments_Users" FOREIGN KEY ("TeacherId") REFERENCES "Users"("Id") ON DELETE RESTRICT
);

-- 5. Submissions Table
CREATE TABLE IF NOT EXISTS "Submissions" (
    "Id" SERIAL PRIMARY KEY,
    "AssignmentId" INT NOT NULL,
    "StudentId" INT NOT NULL,
    "AnswerContent" TEXT NOT NULL,
    "AttachmentUrl" TEXT NULL,
    "SubmittedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NULL,
    "Status" INT NOT NULL DEFAULT 0, -- 0: Submitted, 1: Late, 2: Graded, 3: NeedsRevision
    "MarksAwarded" INT NULL,
    "Feedback" TEXT NULL,
    CONSTRAINT "FK_Submissions_Assignments" FOREIGN KEY ("AssignmentId") REFERENCES "Assignments"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Submissions_Users" FOREIGN KEY ("StudentId") REFERENCES "Users"("Id") ON DELETE RESTRICT
);

-- Foreign Key for Users -> Course
ALTER TABLE "Users" ADD CONSTRAINT "FK_Users_Courses" FOREIGN KEY ("CourseId") REFERENCES "Courses"("Id") ON DELETE SET NULL;

-- Sample Demo Seed Data (Passwords are BCrypt hashed for 'Admin123!', 'Teacher123!', 'Student123!')
-- Admin: admin@school.com / Admin123! ($2a$11$w8NlB8ZlP...)
-- Teacher: teacher@school.com / Teacher123!
-- Student: student@school.com / Student123!
