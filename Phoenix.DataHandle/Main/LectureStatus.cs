﻿namespace Phoenix.DataHandle.Main
{
    public enum LectureStatus
    {
        Unknown = -1,
        Scheduled = 1,
        Cancelled = 2,
    }

    public static class LectureStatusExtensions
    {
        public static string ToGreekString(this LectureStatus ls)
        {
            switch (ls)
            {
                case LectureStatus.Unknown:
                    return "Άγνωστη";
                case LectureStatus.Scheduled:
                    return "Κανονικά";
                case LectureStatus.Cancelled:
                    return "Ακυρώθηκε";
            }

            return ls.ToString();
        }
    }
}