using System;
using System.Collections.Generic;
using System.Linq;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Relationships;

namespace Phoenix.Api.Models.Api
{
    public class ExerciseApi : IExercise, IModelApi
    {
        public int id { get; set; }
        public string Page { get; set; }
        public string Name { get; set; }
        public string Comments { get; set; }
        
        public BookApi Book { get; set; }
        IBook IExercise.Book => this.Book;

        public LectureApi Lecture { get; set; }
        ILecture IExercise.Lecture => this.Lecture;

        public ICollection<StudentExerciseApi> StudentExercises { get; set; }
        IEnumerable<IStudentExercise> IExercise.StudentExercises => this.StudentExercises;
    }
}
