using System;
using System.Collections.Generic;
using System.Linq;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Relationships;

namespace Phoenix.Api.Models.Api
{
    public class SchoolApi : ISchool, IModelApi
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string City { get; set; }
        public string AddressLine { get; set; }
        public string Info { get; set; }
        public string FacebookPageId { get; set; }

        public ICollection<CourseApi> Courses { get; set; }
        public IEnumerable<IUserSchool> UserSchools { get; }
        IEnumerable<ICourse> ISchool.Courses => this.Courses;

        public ICollection<ClassroomApi> Classrooms { get; set; }
        IEnumerable<IClassroom> ISchool.Classrooms => this.Classrooms;
    }
}
