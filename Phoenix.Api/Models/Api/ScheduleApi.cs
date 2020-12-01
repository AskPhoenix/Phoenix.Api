using System;
using System.Collections.Generic;
using System.Linq;
using Phoenix.DataHandle.Main.Entities;

namespace Phoenix.Api.Models.Api
{
    public class ScheduleApi : ISchedule, IModelApi
    {
        public int id { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public string Info { get; set; }

        public CourseApi Course { get; set; }
        ICourse ISchedule.Course => this.Course;

        public ClassroomApi Classroom { get; set; }
        IClassroom ISchedule.Classroom => this.Classroom;

        public IEnumerable<ILecture> Lectures { get; }
    }
}
