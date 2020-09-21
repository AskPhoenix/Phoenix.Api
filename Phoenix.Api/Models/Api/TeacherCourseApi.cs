using System;
using System.Collections.Generic;
using System.Linq;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Relationships;

namespace Phoenix.Api.Models.Api
{
    public class TeacherCourseApi : ITeacherCourse
    {
        public UserApi User { get; set; }
        IUser ITeacherCourse.Teacher => this.User;

        public CourseApi Course { get; set; }
        ICourse ITeacherCourse.Course => this.Course;
    }
}
