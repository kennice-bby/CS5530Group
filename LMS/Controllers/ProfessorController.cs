using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
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

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var students = from d in db.Departments
                        where d.SubjectAbbrev == subject
                        from c in d.Courses
                        where c.Number == num
                        from cl in c.Classes
                        where cl.SemSeason == season && cl.SemYear == year
                        from e in cl.Enrollments
                        select new
                        {
                            fname = e.Student.FirstName,
                            lname = e.Student.LastName,
                            uid = e.Student.UId,
                            dob = e.Student.Dob,
                            grade = e.Grade
                        };
       
            return Json(students);
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            if (category == null) {
                var allAssigns = from course in db.Courses
                                 where course.SubjectAbbrev == subject && course.Number == num
                                 from c in course.Classes
                                 where c.SemSeason == season && c.SemYear == year
                                 from ac in c.AssignmentCategories
                                 from a in db.Assignments
                                 where a.CategoryId == ac.CategoryId
                                 select new
                                 {
                                     aname = a.Name,
                                     cname = ac.Name,
                                     due = a.DueDate,
                                     submissions = a.Submissions.Count
                                 };

                return Json(allAssigns);
            }
            var assignments = from d in db.Departments
                        where d.SubjectAbbrev == subject
                        from c in d.Courses
                        where c.Number == num
                        from cl in c.Classes
                        where cl.SemSeason == season && cl.SemYear == year
                        from ac in cl.AssignmentCategories
                        where ac.Name == category || ac.Name == null
                        from a in ac.Assignments
                        select new
                        {
                            aname = a.Name,
                            cname = ac.Name,
                            due = a.DueDate,
                            submissions = a.Submissions.Count,
                        };

            return Json(assignments);
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {

            var assignCats = from d in db.Departments
                              where d.SubjectAbbrev == subject
                              from c in d.Courses
                              where c.Number == num
                              from cl in c.Classes
                              where cl.SemSeason == season && cl.SemYear == year
                              from ac in cl.AssignmentCategories      
                              select new
                              {
                                  name = ac.Name,
                                  weight = ac.Weight,
                              };

            return Json(assignCats);
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            var query = from d in db.Departments
                        where d.SubjectAbbrev == subject
                        from c in d.Courses
                        where c.Number == num
                        from cl in c.Classes
                        where cl.SemSeason == season && cl.SemYear == year
                        from ac in cl.AssignmentCategories
                        where ac.Name == category
                        select c;

            if (query.Count() > 0)
            {
                return Json(new { success = false });
            }

            var classID = from d in db.Departments
                        where d.SubjectAbbrev == subject
                        from c in d.Courses
                        where c.Number == num
                        from cl in c.Classes
                        where cl.SemSeason == season && cl.SemYear == year
                        select cl.ClassId;

            AssignmentCategory newAC = new AssignmentCategory();
            newAC.Weight = (uint)catweight;
            newAC.Name = category;
            newAC.ClassId = classID.FirstOrDefault();
            db.Add(newAC);
            db.SaveChanges();
            return Json(new { success = true });
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            var categoryID = (from c in db.Classes
                             from aCategory in c.AssignmentCategories
                             where aCategory.Name == category
                             select aCategory.CategoryId).FirstOrDefault();

            Assignment assign = new Assignment();
            assign.Name = asgname;
            assign.MaxPoints = (uint) asgpoints;
            assign.Contents = asgcontents;
            assign.DueDate = asgdue;
            assign.CategoryId = categoryID;

            db.Add(assign);
            db.SaveChanges();

            var enrollments = (from d in db.Departments
                              where d.SubjectAbbrev == subject
                              from c in d.Courses
                              where c.Number == num
                              from cl in c.Classes
                              where cl.SemSeason == season && cl.SemYear == year
                              from e in cl.Enrollments
                              select e).ToList();
            foreach (Enrollment enrollment in enrollments) {
                updateGrade(enrollment.StudentId, subject, num, season, year);
            }
            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        { 
            var submissions = from d in db.Departments
                              where d.SubjectAbbrev == subject
                              from c in d.Courses
                              where c.Number == num
                              from cl in c.Classes
                              where cl.SemSeason == season && cl.SemYear == year
                              from ac in cl.AssignmentCategories
                              where ac.Name == category || ac.Name == null
                              from a in ac.Assignments
                              where a.Name == asgname
                              from s in a.Submissions
                              select new
                              {
                                  fname = s.StudentU.FirstName,
                                  lname = s.StudentU.LastName,
                                  uid = s.StudentUid,
                                  time = s.SubmissionDate,
                                  score = s.Score,
                              };

            return Json(submissions);
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            var submission =  (from d in db.Departments
                              where d.SubjectAbbrev == subject
                              from c in d.Courses
                              where c.Number == num
                              from cl in c.Classes
                              where cl.SemSeason == season && cl.SemYear == year
                              from ac in cl.AssignmentCategories
                              where ac.Name == category
                              from a in ac.Assignments
                              where a.Name == asgname
                              from s in a.Submissions
                              where s.StudentUid == uid
                              select s).FirstOrDefault();

            if (submission != null)
            {
                submission.Score = (uint)score;
                db.SaveChanges();

                updateGrade(uid, subject, num, season, year);

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        private void updateGrade(string uid, string subject, int num, string season, int year) {
            var currClass = (from d in db.Departments
                           where d.SubjectAbbrev == subject
                           from c in d.Courses
                           where c.Number == num
                           from cl in c.Classes
                           where cl.SemSeason == season && cl.SemYear == year
                           select cl).FirstOrDefault();
            double scaledTotal = 0;
            uint totalWeights = 0;
            if (currClass != null) {
                var categories = (from cat in db.AssignmentCategories
                               where currClass.ClassId == cat.ClassId
                               select cat).ToList();
                foreach (AssignmentCategory category in categories)
                {
                    uint totalPoints = 0;
                    uint totalMaxPoints = 0;
                    uint categoryWeight = (uint)category.Weight;
                    totalWeights += categoryWeight;

                    var assignments = (from a in db.Assignments
                                      where a.CategoryId == category.CategoryId
                                      select a).ToList();
                    foreach (Assignment assignment in assignments)
                    {
                        totalMaxPoints += assignment.MaxPoints;
                        var submission = (from s in db.Submissions
                                          where s.StudentUid == uid && s.AssignmentId == assignment.AssignmentId
                                          select s).FirstOrDefault();
                        if (submission != null)
                        {
                            totalPoints += submission.Score;
                        }
                    }
                    if (totalMaxPoints != 0)
                    {
                        scaledTotal += (((double)totalPoints / totalMaxPoints) * categoryWeight);
                    }

                }

                double scalingFactor = 100.0 / totalWeights;
                scaledTotal *= scalingFactor;
                string letterGrade = convertToLetterGrade(scaledTotal);

                var enrollment = (from e in db.Enrollments
                                 where e.StudentId == uid && e.ClassId == currClass.ClassId
                                 select e).FirstOrDefault();
                if (enrollment != null) {
                    enrollment.Grade = letterGrade;
                    db.SaveChanges();
                }
                
            }
            

        }

        private string convertToLetterGrade(double classPercent)
        {
            switch (classPercent)
            {
                case >= 93:
                    return "A";
                case >= 90:
                    return "A-";
                case >= 87:
                    return "B+";
                case >= 83:
                    return "B";
                case >= 80:
                    return "B-";
                case >= 77:
                    return "C+";
                case >= 73:
                    return "C";
                case >= 70:
                    return "C-";
                case >= 67:
                    return "D+";
                case >= 63:
                    return "D";
                case >= 60:
                    return "D-";
                default:
                    return "E";
            }
        }

        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var classes = from c in db.Classes
                          where c.ProfessorUid == uid
                          select new {
                              subject = c.Course.SubjectAbbrev,
                              number = c.Course.Number,
                              name = c.Course.Name,
                              season = c.SemSeason,
                              year = c.SemYear
                          };
            return Json(classes);
        }


        
        /*******End code to modify********/
    }
}

