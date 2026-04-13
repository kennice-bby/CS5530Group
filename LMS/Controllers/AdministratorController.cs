using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {
            var query = from d in db.Departments
                        where d.SubjectAbbrev == subject
                        select d;
            if (query.Count() < 1) { 
                Department department = new Department();
                department.Name = name;
                department.SubjectAbbrev = subject;
                db.Add(department);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false});
        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var courses = from d in db.Departments
                              where d.SubjectAbbrev == subject
                              from course in d.Courses
                              select new
                              {
                                  number = course.Number,
                                  name = course.Name
                              };

            return Json(courses);
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            var professors = from d in db.Departments
                          where d.SubjectAbbrev == subject
                          from professor in d.Professors
                          select new
                          {
                              lname = professor.LastName,
                              fname = professor.FirstName,
                              uid = professor.UId
                          };

            return Json(professors);

        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {
            var query = from c in db.Courses
                        where c.Number == number
                        && c.SubjectAbbrev == subject
                        select c;
            if (query.Count() > 0) {
                return Json(new { success = false });
            }
            Course course = new Course();
            course.Number = (uint)number;
            course.Name = name;
            course.SubjectAbbrev = subject;
            db.Add(course);
            db.SaveChanges();
            return Json(new { success = true });
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            var otherClass = from c in db.Classes
                        where c.Location == location
                        && c.SemSeason == season
                        && c.SemYear == year
                        && ((c.StartTime >= TimeOnly.FromDateTime(start) && c.StartTime <= TimeOnly.FromDateTime(end))
                        || (c.EndTime >= TimeOnly.FromDateTime(start) && c.EndTime <= TimeOnly.FromDateTime(end)))
                        select c;
            if (otherClass.Count() > 0)
            {
                return Json(new { success = false });
            }

            var sameCourse = from course in db.Courses
                             where course.Number == number
                             && course.SubjectAbbrev == subject
                             from c in course.Classes
                             where c.SemSeason == season
                             && c.SemYear == year
                             select c;
            if (sameCourse.Count() > 0)
            {
                return Json(new { success = false });
            }

            var cID = (from course in db.Courses
                        where course.Number == number
                        &&  course.SubjectAbbrev == subject
                        select course.CourseId).FirstOrDefault();
            Class newClass = new Class();
            newClass.CourseId = cID;
            newClass.SemSeason = season;
            newClass.SemYear = (uint)year;
            newClass.StartTime = TimeOnly.FromDateTime(start);
            newClass.EndTime = TimeOnly.FromDateTime(end);
            newClass.Location = location;
            newClass.ProfessorUid = instructor;

            db.Add(newClass);
            db.SaveChanges();
            return Json(new { success = true});
        }


        /*******End code to modify********/

    }
}

