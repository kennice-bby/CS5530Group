using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Course
    {
        public Course()
        {
            Classes = new HashSet<Class>();
        }

        public string Name { get; set; } = null!;
        public uint Number { get; set; }
        public string SubjectAbbrev { get; set; } = null!;
        public uint CourseId { get; set; }

        public virtual Department SubjectAbbrevNavigation { get; set; } = null!;
        public virtual ICollection<Class> Classes { get; set; }
    }
}
