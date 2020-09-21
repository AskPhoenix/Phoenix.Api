﻿using System;
using System.Collections.Generic;
using System.Linq;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Relationships;

namespace Phoenix.Api.Models.Api
{
    public class StudentCourseApi : IStudentCourse
    {
        public decimal? Grade { get; set; }

        public UserApi User { get; set; }
        IUser IStudentCourse.Student => this.User;

        public CourseApi Course { get; set; }
        ICourse IStudentCourse.Course => this.Course;
    }
}