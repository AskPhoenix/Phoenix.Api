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
        public ICourse Course { get; set; }
        public IClassroom Classroom { get; set; }
    }
}
