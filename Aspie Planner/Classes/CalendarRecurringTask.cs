using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace Aspie_Planner
{
    public class CalendarRecurringTask
    {
        public enum ReccuranceType
        {
            XToYDays,
            Weekly,
            EveryNthDay,
            Undefined
        }
        public enum TaskStatus
        {
            Complete,
            Incomplete,
            Any,
            Critical
        }

        public enum TimeType
        {
            Unspecified,
            SetTime,
            RangeTime
        }

        public ReccuranceType TaskReccuranceType { get; set; }
        public TimeType TaskTimeType { get; set; }
        public List<string> Weekdays { get; set; }
        public int DayRangeLower { get; set; }
        public int DayRangeUpper { get; set; }
        public TimeSpan TimeParamLower { get; set; }
        public TimeSpan TimeParamUpper { get; set; }
        private TimeSpan Duration;
        public DateTime OffsetDate { get; set; }
        public string TaskDescription { get; set; }
        public Guid TaskGuid { get; set; }
        public List<DateTime> DatesDone { get; set; }
        public List<TaskNote> TaskNotes { get; set; }
        public Color TextColor { get; set; }
        public Color BackColor { get; set; }

        public CalendarRecurringTask()
        {
            Weekdays = new List<string>();
            DatesDone = new List<DateTime>();
            TaskNotes = new List<TaskNote>();
        }

        public CalendarRecurringTask(ReccuranceType reccuranceType,
                                     TimeType timeType,
                                     List<string> weekdays,
                                     int dayRangeLower,
                                     int dayRangeUpper,
                                     TimeSpan timeParamLower,
                                     TimeSpan timeParamUpper,
                                     TimeSpan duration,
                                     string taskDescription,
                                     Color textColor,
                                     Color backColor,
                                     DateTime offsetDate)
        {
            TaskReccuranceType = reccuranceType;
            TaskTimeType = timeType;
            Weekdays = weekdays;
            DayRangeLower = dayRangeLower;
            DayRangeUpper = dayRangeUpper;
            TimeParamLower = timeParamLower;
            TimeParamUpper = timeParamUpper;
            Duration = duration;
            TaskDescription = taskDescription;
            OffsetDate = offsetDate;
            TextColor = textColor;
            BackColor = backColor;
            DatesDone = new List<DateTime>();
            TaskNotes = new List<TaskNote>();
            TaskGuid = Guid.NewGuid();
            if (TaskReccuranceType == ReccuranceType.EveryNthDay && DayRangeLower == 0)
                DayRangeLower = 1;
        }

        public CalendarRecurringTask(ReccuranceType reccuranceType,
                                     TimeType timeType,
                                     List<string> weekdays,
                                     int dayRangeLower,
                                     int dayRangeUpper,
                                     TimeSpan timeParamLower,
                                     TimeSpan timeParamUpper,
                                     TimeSpan duration,
                                     string taskDescription,
                                     Color textColor,
                                     Color backColor,
                                     DateTime offsetDate,
                                     string guid)
        {
            TaskReccuranceType = reccuranceType;
            TaskTimeType = timeType;
            Weekdays = weekdays;
            DayRangeLower = dayRangeLower;
            DayRangeUpper = dayRangeUpper;
            TimeParamLower = timeParamLower;
            TimeParamUpper = timeParamUpper;
            Duration = duration;
            TaskDescription = taskDescription;
            OffsetDate = offsetDate;
            TextColor = textColor;
            BackColor = backColor;
            DatesDone = new List<DateTime>();
            TaskNotes = new List<TaskNote>();
            TaskGuid = new Guid(guid);
            if (TaskReccuranceType == ReccuranceType.EveryNthDay && DayRangeLower == 0)
                DayRangeLower = 1;
        }

        public CalendarRecurringTask Clone()
        {
            CalendarRecurringTask result = new CalendarRecurringTask(this.TaskReccuranceType, this.TaskTimeType, this.Weekdays,
                this.DayRangeLower, this.DayRangeUpper, this.TimeParamLower, this.TimeParamUpper, this.Duration, this.TaskDescription,
                this.TextColor, this.BackColor, this.OffsetDate, this.TaskGuid.ToString());
            result.DatesDone = new List<DateTime>(this.DatesDone);
            return result;
        }

        public void DoTask(DateTime timeDone)
        {
            DatesDone.Add(timeDone);
        }

        public void UndoTask(DateTime timeUndone)
        {
            DatesDone.Remove(timeUndone);
            DatesDone.Sort();
        }

        public void SetNote(DateTime noteDate, string note)
        {
            if (string.IsNullOrEmpty(note))
                TaskNotes.RemoveAll(x => x.GetDate() == noteDate);
            else
            {
                TaskNote oldNote = TaskNotes.Find(x => x.GetDate() == noteDate);
                if (oldNote != null)
                    oldNote.UpdateNote(note);
                else
                    TaskNotes.Add(new TaskNote(noteDate, note));
            }
        }

        public string GetNote(DateTime noteDate)
        {
            TaskNote taskNote = TaskNotes.Find(x => x.GetDate() == noteDate);
            if (taskNote != null)
                return taskNote.GetNote();
            else
                return null;
        }

        public string GetLastXNotes(DateTime noteDate, int noteCount)
        {
            string result = "";
            TaskNote[] notes = TaskNotes.FindAll(x => x.GetDate() < noteDate).OrderByDescending(x => x.GetDate()).Take(noteCount).ToArray();
            foreach (TaskNote note in notes)
            {
                result = result + note.GetDate().ToString("dd.MM") + ": " + note.GetNote() + Environment.NewLine;
            }
            return result;
        }

        public DateTime GetLastNoteDate(DateTime relativeDate)
        {
            TaskNote note = TaskNotes.OrderByDescending(x => x.GetDate()).FirstOrDefault(x => x.GetDate() < relativeDate);
            if (note != null)
                return note.GetDate();
            else
                return DateTime.MinValue;
        }

        public TaskStatus GetStatus(DateTime checkDate)
        {
            if (DatesDone.Exists(x => x.Date == checkDate.Date))
                return TaskStatus.Complete;
            else
            {
                string weekdayMatch = checkDate.ToString("dddd").ToLower();
                if (TaskReccuranceType == ReccuranceType.Weekly && Weekdays.Exists(x => x.ToLower() == weekdayMatch))
                    return TaskStatus.Critical;
                else if (TaskReccuranceType == ReccuranceType.XToYDays)
                {
                    if ((DatesDone.Count > 0 && DatesDone.Max().AddDays(DayRangeUpper) <= checkDate) ||
                        (DatesDone.Count == 0 && OffsetDate.AddDays(DayRangeUpper) <= checkDate))
                        return TaskStatus.Critical;
                    else if ((DatesDone.Count > 0 && DatesDone.Max().AddDays(DayRangeLower) <= checkDate) ||
                             (DatesDone.Count == 0 && OffsetDate.AddDays(DayRangeLower) <= checkDate))
                        return TaskStatus.Incomplete;
                    else
                        return TaskStatus.Any;
                }
                else if (TaskReccuranceType == ReccuranceType.EveryNthDay)
                {
                    int dateDif = (checkDate - OffsetDate).Days;
                    if (dateDif % DayRangeLower == 0)
                        return TaskStatus.Critical;
                    else
                        return TaskStatus.Any;
                }
                else
                    return TaskStatus.Any;
            }
        }

        public List<DateTime> GetLastDatesOfInterest(DateTime checkDate, int datesCount)
        {
            List<DateTime> dates = new List<DateTime>();
            dates.AddRange(DatesDone);
            dates.AddRange(TaskNotes.Select(x => x.GetDate()).ToList());
            List<DateTime> result = dates.Where(x => x <= checkDate).Distinct().OrderByDescending(x => x).Take(datesCount).ToList();
            return result;
        }

        public void Modify(ReccuranceType reccuranceType,
                           TimeType timeType,
                           List<string> weekdays,
                           int dayRangeLower,
                           int dayRangeUpper,
                           TimeSpan timeParamLower,
                           TimeSpan timeParamUpper,
                           TimeSpan duration,
                           string taskDescription,
                           Color textColor,
                           Color backColor,
                           DateTime offsetDate)
        {
            TaskReccuranceType = reccuranceType;
            TaskTimeType = timeType;
            Weekdays = weekdays;
            DayRangeLower = dayRangeLower;
            DayRangeUpper = dayRangeUpper;
            TimeParamLower = timeParamLower;
            TimeParamUpper = timeParamUpper;
            Duration = duration;
            TaskDescription = taskDescription;
            OffsetDate = offsetDate;
            TextColor = textColor;
            BackColor = backColor;
            if (TaskReccuranceType == ReccuranceType.EveryNthDay && DayRangeLower == 0)
                DayRangeLower = 1;
        }

        public TimeSpan GetDuration()
        {
            TimeSpan result;
            switch (TaskTimeType)
            {
                case TimeType.SetTime:
                    result = TimeParamUpper - TimeParamLower;
                    break;
                case TimeType.RangeTime:
                    result = Duration;
                    break;
                default:
                    result = TimeSpan.Zero;
                    break;
            }
            return result;
        }

        public DateTime LastDone()
        {
            DateTime result;
            if (DatesDone.Count > 0)
                result = DatesDone.Max();
            else
                result = OffsetDate;
            return result;
        }

        public int DaysUntillCritical(DateTime checkDate)
        {
            if (TaskReccuranceType == ReccuranceType.XToYDays)
            {
                return (LastDone().AddDays(DayRangeUpper) - DateTime.Today).Days;
            }
            else
                return 0;
        }
    }
}