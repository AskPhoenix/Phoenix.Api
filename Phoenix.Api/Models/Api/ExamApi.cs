using System;
using System.Collections.Generic;
using System.Linq;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Relationships;

namespace Phoenix.Api.Models.Api
{
    public class ExamApi : IExam, IModelApi
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Comments { get; set; }

        public LectureApi Lecture { get; set; }
        ILecture IExam.Lecture => this.Lecture;

        public ICollection<MaterialApi> Materials { get; set; }
        IEnumerable<IMaterial> IExam.Materials => this.Materials;

        public ICollection<StudentExamApi> StudentExams { get; set; }
        IEnumerable<IStudentExam> IExam.StudentExams => this.StudentExams;
    }
}
