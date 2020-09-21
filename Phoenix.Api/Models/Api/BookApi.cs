using System;
using System.Collections.Generic;
using System.Linq;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Relationships;

namespace Phoenix.Api.Models.Api
{
    public class BookApi : IBook, IModelApi
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Publisher { get; set; }
        public string Info { get; set; }

        public IEnumerable<ICourseBook> CourseBooks { get; }
        public IEnumerable<IExercise> Exercises { get; }
        public IEnumerable<IMaterial> Materials { get; }
    }
}
