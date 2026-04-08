using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LMS.Models.LMSModels
{
    public partial class LMSContext : DbContext
    {
        public LMSContext()
        {
        }

        public LMSContext(DbContextOptions<LMSContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Administrator> Administrators { get; set; } = null!;
        public virtual DbSet<Assignment> Assignments { get; set; } = null!;
        public virtual DbSet<AssignmentCategory> AssignmentCategories { get; set; } = null!;
        public virtual DbSet<Class> Classes { get; set; } = null!;
        public virtual DbSet<Course> Courses { get; set; } = null!;
        public virtual DbSet<Department> Departments { get; set; } = null!;
        public virtual DbSet<Enrollment> Enrollments { get; set; } = null!;
        public virtual DbSet<Professor> Professors { get; set; } = null!;
        public virtual DbSet<Student> Students { get; set; } = null!;
        public virtual DbSet<Submission> Submissions { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("name=LMS:LMSConnectionString", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.11.16-mariadb"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");

            modelBuilder.Entity<Administrator>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.ToTable("Administrator");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.Dob).HasColumnName("DOB");

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.LastName).HasMaxLength(100);
            });

            modelBuilder.Entity<Assignment>(entity =>
            {
                entity.HasIndex(e => new { e.CategoryId, e.Name }, "CategoryID")
                    .IsUnique();

                entity.Property(e => e.AssignmentId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("AssignmentID");

                entity.Property(e => e.CategoryId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("CategoryID");

                entity.Property(e => e.Contents).HasMaxLength(8192);

                entity.Property(e => e.DueDate).HasColumnType("datetime");

                entity.Property(e => e.MaxPoints).HasColumnType("int(10) unsigned");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Assignments)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Assignments_ibfk_1");
            });

            modelBuilder.Entity<AssignmentCategory>(entity =>
            {
                entity.HasKey(e => e.CategoryId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.ClassId, "ClassID");

                entity.HasIndex(e => new { e.Name, e.ClassId }, "Name")
                    .IsUnique();

                entity.Property(e => e.CategoryId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("CategoryID");

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("ClassID");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Weight).HasColumnType("int(10) unsigned");

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.AssignmentCategories)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("AssignmentCategories_ibfk_1");
            });

            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasIndex(e => e.CourseId, "CourseID");

                entity.HasIndex(e => e.ProfessorUid, "ProfessorUID");

                entity.HasIndex(e => new { e.SemYear, e.SemSeason, e.CourseId }, "SemYear")
                    .IsUnique();

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("ClassID");

                entity.Property(e => e.CourseId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("CourseID");

                entity.Property(e => e.EndTime).HasColumnType("time");

                entity.Property(e => e.Location).HasMaxLength(100);

                entity.Property(e => e.ProfessorUid)
                    .HasMaxLength(8)
                    .HasColumnName("ProfessorUID")
                    .IsFixedLength();

                entity.Property(e => e.SemSeason).HasColumnType("enum('Spring','Fall','Summer')");

                entity.Property(e => e.SemYear).HasColumnType("int(10) unsigned");

                entity.Property(e => e.StartTime).HasColumnType("time");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Classes_ibfk_1");

                entity.HasOne(d => d.ProfessorU)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.ProfessorUid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Classes_ibfk_2");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasIndex(e => new { e.SubjectAbbrev, e.Number }, "SubjectAbbrev")
                    .IsUnique();

                entity.Property(e => e.CourseId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("CourseID");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Number).HasColumnType("int(10) unsigned");

                entity.Property(e => e.SubjectAbbrev).HasMaxLength(4);

                entity.HasOne(d => d.SubjectAbbrevNavigation)
                    .WithMany(p => p.Courses)
                    .HasForeignKey(d => d.SubjectAbbrev)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Courses_ibfk_1");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.SubjectAbbrev)
                    .HasName("PRIMARY");

                entity.Property(e => e.SubjectAbbrev).HasMaxLength(4);

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => new { e.ClassId, e.StudentId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.HasIndex(e => e.StudentId, "StudentID");

                entity.Property(e => e.ClassId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("ClassID");

                entity.Property(e => e.StudentId)
                    .HasMaxLength(8)
                    .HasColumnName("StudentID")
                    .IsFixedLength();

                entity.Property(e => e.Grade).HasMaxLength(2);

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.Enrollments)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Enrollments_ibfk_1");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.Enrollments)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Enrollments_ibfk_2");
            });

            modelBuilder.Entity<Professor>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.ToTable("Professor");

                entity.HasIndex(e => e.Department, "Department");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.Department).HasMaxLength(4);

                entity.Property(e => e.Dob).HasColumnName("DOB");

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.LastName).HasMaxLength(100);

                entity.HasOne(d => d.DepartmentNavigation)
                    .WithMany(p => p.Professors)
                    .HasForeignKey(d => d.Department)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Professor_ibfk_1");
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.ToTable("Student");

                entity.HasIndex(e => e.Major, "Major");

                entity.Property(e => e.UId)
                    .HasMaxLength(8)
                    .HasColumnName("uID")
                    .IsFixedLength();

                entity.Property(e => e.Dob).HasColumnName("DOB");

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.LastName).HasMaxLength(100);

                entity.Property(e => e.Major).HasMaxLength(4);

                entity.HasOne(d => d.MajorNavigation)
                    .WithMany(p => p.Students)
                    .HasForeignKey(d => d.Major)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Student_ibfk_1");
            });

            modelBuilder.Entity<Submission>(entity =>
            {
                entity.ToTable("Submission");

                entity.HasIndex(e => e.AssignmentId, "AssignmentID");

                entity.HasIndex(e => new { e.StudentUid, e.AssignmentId }, "StudentUID")
                    .IsUnique();

                entity.Property(e => e.SubmissionId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("SubmissionID");

                entity.Property(e => e.AssignmentId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("AssignmentID");

                entity.Property(e => e.Contents).HasMaxLength(8192);

                entity.Property(e => e.Score).HasColumnType("int(10) unsigned");

                entity.Property(e => e.StudentUid)
                    .HasMaxLength(8)
                    .HasColumnName("StudentUID")
                    .IsFixedLength();

                entity.Property(e => e.SubmissionDate).HasColumnType("datetime");

                entity.HasOne(d => d.Assignment)
                    .WithMany(p => p.Submissions)
                    .HasForeignKey(d => d.AssignmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Submission_ibfk_1");

                entity.HasOne(d => d.StudentU)
                    .WithMany(p => p.Submissions)
                    .HasForeignKey(d => d.StudentUid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Submission_ibfk_2");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
