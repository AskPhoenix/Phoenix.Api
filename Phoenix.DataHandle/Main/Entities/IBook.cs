﻿using System.Collections.Generic;
using Phoenix.DataHandle.Main.Relationships;

namespace Phoenix.DataHandle.Main.Entities
{
    public interface IBook
    {
        string Name { get; set; }

        IEnumerable<ICourseBook> CourseBooks { get; }
        IEnumerable<IExercise> Exercises { get; }
        IEnumerable<IMaterial> Materials { get; }
    }
}