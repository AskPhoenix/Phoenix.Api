﻿using System;
using System.Collections.Generic;
using System.Linq;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Relationships;

namespace Phoenix.Api.Models.Api
{
    public class StudentExerciseApi : IStudentExercise
    {
        public decimal? Grade { get; set; }

        public UserApi Student { get; set; }
        IUser IStudentExercise.Student => this.Student;

        public ExerciseApi Exercise { get; set; }
        IExercise IStudentExercise.Exercise => this.Exercise;
    }
}
