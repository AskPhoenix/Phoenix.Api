using System;
using System.Collections.Generic;
using System.Linq;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Relationships;

namespace Phoenix.Api.Models.Api
{
    public class CourseApi : ICourse, IModelApi
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string SubCourse { get; set; }
        public string Level { get; set; }
        public string Group { get; set; }
        public string Info { get; set; }
        public DateTimeOffset FirstDate { get; set; }
        public DateTimeOffset LastDate { get; set; }

        public SchoolApi School { get; set; }
        ISchool ICourse.School => this.School;

        public ICollection<LectureApi> Lectures { get; set; }
        IEnumerable<ILecture> ICourse.Lectures => this.Lectures;

        public ICollection<ScheduleApi> Schedules { get; set; }
        IEnumerable<ISchedule> ICourse.Schedules => this.Schedules;

        public IEnumerable<ICourseBook> CourseBooks { get; }
        public IEnumerable<IStudentCourse> StudentCourses { get; }
        public IEnumerable<ITeacherCourse> TeacherCourses { get; }
    }
}
