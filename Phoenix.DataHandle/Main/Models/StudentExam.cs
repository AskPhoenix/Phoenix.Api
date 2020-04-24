﻿using System;
using System.Collections.Generic;

namespace Phoenix.DataHandle.Main.Models
{
    public partial class StudentExam
    {
        public int StudentId { get; set; }
        public int ExamId { get; set; }
        public decimal? Grade { get; set; }

        public virtual Exam Exam { get; set; }
        public virtual User Student { get; set; }
    }
}