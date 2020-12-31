using System;
using System.Collections.Generic;
using System.Linq;
using Phoenix.DataHandle.Main;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Relationships;

namespace Phoenix.Api.Models.Api
{
    public class AspNetUserApi : IAspNetUsers, IModelApi
    {
        public int id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public DateTimeOffset RegisteredAt { get; set; }
        public ApplicationType CreatedApplicationType { get; set; }

        public UserApi User { get; set; }
        IUser IAspNetUsers.User => this.User;

        public ICollection<TeacherCourseApi> TeacherCourses { get; set; }
        IEnumerable<ITeacherCourse> IAspNetUsers.TeacherCourses => this.TeacherCourses;

        public IEnumerable<IAspNetUserRoles> Roles { get; set; }

        public IEnumerable<IAspNetUserLogins> AspNetUserLogins { get; set; }

        public IEnumerable<IAttendance> Attendances { get; set; }
        public IEnumerable<IParenthood> Children { get; }
        public IEnumerable<IParenthood> Parents { get; }

        public IEnumerable<IStudentCourse> StudentCourses { get; set; }

        public IEnumerable<IStudentExam> StudentExams { get; set; }

        public IEnumerable<IStudentExercise> StudentExercises { get; set; }

        public IEnumerable<IUserSchool> UserSchools { get; set; }
    }
}
