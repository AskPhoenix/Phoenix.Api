﻿using System;
using System.Collections.Generic;
using System.Linq;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Relationships;

namespace Phoenix.Api.Models.Api
{
    public class UserApi : IUser, IModelApi
    {
        public int id { get; set; }
        public IAspNetUsers AspNetUser { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public IEnumerable<IAttendance> Attendances { get; set; }
        public IEnumerable<IStudentCourse> StudentCourses { get; set; }
        public IEnumerable<IStudentExam> StudentExams { get; set; }
        public IEnumerable<IStudentExercise> StudentExercises { get; set; }
        public IEnumerable<ITeacherCourse> TeacherCourses { get; set; }
    }
}
