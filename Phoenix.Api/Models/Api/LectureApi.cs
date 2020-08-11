using System;
using System.Collections.Generic;
using System.Linq;
using Phoenix.DataHandle.Main;
using Phoenix.DataHandle.Main.Entities;
using Phoenix.DataHandle.Main.Relationships;

namespace Phoenix.Api.Models.Api
{
    public class LectureApi : ILecture, IModelApi
    {
        public int id { get; set; }
        public DateTimeOffset StartDateTime { get; set; }
        public DateTimeOffset EndDateTime { get; set; }
        public LectureStatus Status { get; set; }
        public string OnlineMeetingLink { get; set; }
        public string Info { get; set; }
        public LectureCreatedBy CreatedBy { get; set; }

        public CourseApi Course { get; set; }
        ICourse ILecture.Course => this.Course;

        public ScheduleApi Schedule { get; set; }
        ISchedule ILecture.Schedule => this.Schedule;

        public ClassroomApi Classroom { get; set; }
        IClassroom ILecture.Classroom => this.Classroom;

        public ExamApi Exam { get; set; }
        IExam ILecture.Exam => this.Exam;

        public ICollection<ExerciseApi> Exercises { get; set; }
        IEnumerable<IExercise> ILecture.Exercises => this.Exercises;

        public IEnumerable<IAttendance> Attendances { get; }
    }
}
