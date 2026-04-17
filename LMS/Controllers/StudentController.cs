using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var classes = from s in db.Students
                          where s.UId == uid
                          from e in s.Enrollments
                          select new
                          {
                              subject = e.Class.Course.SubjectAbbrev,
                              number = e.Class.Course.Number,
                              name = e.Class.Course.Name,
                              season = e.Class.SemSeason,
                              year = e.Class.SemYear,
                              grade = e.Grade
                          };

            return Json(classes);
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var currClass = from d in db.Departments
                            where d.SubjectAbbrev == subject
                            from c in d.Courses
                            where c.Number == num
                            from cl in c.Classes
                            where cl.SemSeason == season && cl.SemYear == year
                            from e in cl.Enrollments
                            where e.Student.UId == uid
                            from cat in cl.AssignmentCategories
                            from a in db.Assignments
                            from s in a.Submissions.Where(s => s.StudentUid == uid).DefaultIfEmpty()
                            select new
                            {
                                aname = a.Name,
                                cname = cat.Name,
                                due = a.DueDate,
                                score = s == null ? 0 : s.Score
                            };

            return Json(currClass);
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            var assignment = (from d in db.Departments
                              where d.SubjectAbbrev == subject
                              from c in d.Courses
                              where c.Number == num
                              from cl in c.Classes
                              where cl.SemSeason == season && cl.SemYear == year
                              from cat in cl.AssignmentCategories
                              where cat.Name == category
                              from a in cat.Assignments
                              where a.Name == asgname
                              select a).FirstOrDefault();
            if(assignment == null) {
                return Json(new { success = false });
            }

            var submission = (from s in db.Submissions
                              where s.AssignmentId == assignment.AssignmentId
                              && s.StudentUid == uid
                              select s).FirstOrDefault();

            if (submission != null) {
                submission.Contents = contents;
                submission.SubmissionDate = DateTime.Now;
                db.SaveChanges();
                return Json(new { success = true });
            }

            Submission newSubmission = new Submission();
            newSubmission.Contents = contents;
            newSubmission.SubmissionDate = DateTime.Now;
            newSubmission.Score = 0;
            newSubmission.StudentUid = uid;
            newSubmission.AssignmentId = assignment.AssignmentId;


            db.Add(newSubmission);
            db.SaveChanges();

            return Json(new { success = true});
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            var currClass = (from d in db.Departments
                          where d.SubjectAbbrev == subject
                          from c in d.Courses
                          where c.Number == num
                          from cl in c.Classes
                          where cl.SemSeason == season && cl.SemYear == year
                          select cl).FirstOrDefault();
            if (currClass == null) {
                return Json(new { success = false });
            }

            var student = from e in currClass.Enrollments
                              where e.StudentId == uid
                              select e.Student;

            if (student.Any()) {
                return Json(new { success = false });
            }

            Enrollment enrollment = new Enrollment();
            enrollment.Grade = "--";
            enrollment.StudentId = uid;
            enrollment.ClassId = currClass.ClassId;
            db.Add(enrollment);
            db.SaveChanges();
            return Json(new { success = true });
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            var enrollements = (from s in db.Students
                               where s.UId == uid
                               select s.Enrollments).FirstOrDefault();
            if (enrollements == null) {
                return Json(new { gpa = 0.0 });
            }

            double totalPointValue = 0;
            int count = 0;
            // adding valid grades
            foreach (var enrollement in enrollements) {
                switch (enrollement.Grade)
                {
                    case "A":
                        totalPointValue += 4.0;
                        count++;
                        break;
                    case "A-":
                        totalPointValue += 3.7;
                        count++;
                        break;
                    case "B+":
                        totalPointValue += 3.3;
                        count++;
                        break;
                    case "B":
                        totalPointValue += 3.0;
                        count++;
                        break;
                    case "B-":
                        totalPointValue += 2.7;
                        count++;
                        break;
                    case "C+":
                        totalPointValue += 2.3;
                        count++;
                        break;
                    case "C":
                        totalPointValue += 2.0;
                        count++;
                        break;
                    case "C-":
                        totalPointValue += 1.7;
                        count++;
                        break;
                    case "D+":
                        totalPointValue += 1.3;
                        count++;
                        break;
                    case "D":
                        totalPointValue += 1.0;
                        count++;
                        break;
                    case "D-":
                        totalPointValue += 0.7;
                        count++;
                        break;
                    case "E":
                        totalPointValue += 0.0;
                        count++;
                        break;
                    default:
                        break;
                }
            }

            if(totalPointValue == 0 && count == 0)
            {
                return Json(new { gpa = 0.0 });
            }

            totalPointValue /= count;

            return Json(new { gpa = totalPointValue });
        }
                
        /*******End code to modify********/

    }
}

