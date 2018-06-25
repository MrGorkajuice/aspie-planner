using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspie_Planner
{
    class WeekContent
    {
        public List<KeyValuePair<DateTime, List<KeyValuePair<CalendarRecurringTask, TimeSpan>>>> Data { get; private set; }
        CalendarContent calendarContent;
        TimeSpan planningResolution = new TimeSpan(0, 15, 0);

        public WeekContent(CalendarContent calendarContent)
        {
            this.calendarContent = calendarContent;
            Data = new List<KeyValuePair<DateTime, List<KeyValuePair<CalendarRecurringTask, TimeSpan>>>>();
            for (int i = 0; i < 7; i++)
            {
                Data.Add(new KeyValuePair<DateTime, List<KeyValuePair<CalendarRecurringTask, TimeSpan>>>
                    (DateTime.Today.AddDays(i), new List<KeyValuePair<CalendarRecurringTask, TimeSpan>>()));
            }
        }

        public void Autocalculate()
        {
            CalendarContent workingCopy = calendarContent.Clone();
            for (int i = 0; i < 7; i++)
            {
                DateTime workingDay = DateTime.Today.AddDays(i);
                List<CalendarRecurringTask> todaysTasks = workingCopy.GetRecurringTasks(workingDay);
                // Tasks that must occur on specific dates at specific times have top priority
                List<CalendarRecurringTask> firstPriorityTasks = todaysTasks.FindAll(x => x.TaskTimeType == CalendarRecurringTask.TimeType.SetTime 
                    && (x.TaskReccuranceType == CalendarRecurringTask.ReccuranceType.Weekly || x.TaskReccuranceType == CalendarRecurringTask.ReccuranceType.EveryNthDay));
                foreach (CalendarRecurringTask task in firstPriorityTasks)
                {
                    if (TryAdd(workingDay, task))
                        workingCopy.DoTask(task, workingDay);
                }
                // Tasks that must occur on specific dates at flexible times have second priority
                List<CalendarRecurringTask> secondPriorityTasks = todaysTasks.FindAll(x => x.TaskTimeType == CalendarRecurringTask.TimeType.RangeTime
                    && (x.TaskReccuranceType == CalendarRecurringTask.ReccuranceType.Weekly || x.TaskReccuranceType == CalendarRecurringTask.ReccuranceType.EveryNthDay));
                foreach (CalendarRecurringTask task in secondPriorityTasks)
                {
                    if (TryAdd(workingDay, task))
                        workingCopy.DoTask(task, workingDay);
                }
                // Tasks that can occur on a range of dates, but at a specific time have third priority and are sorted internally by days until they go critical
                List<CalendarRecurringTask> thirdPriorityTasks = todaysTasks.FindAll(x => x.TaskTimeType == CalendarRecurringTask.TimeType.SetTime
                    && x.TaskReccuranceType == CalendarRecurringTask.ReccuranceType.XToYDays).OrderBy(x => x.DaysUntillCritical(workingDay)).ToList();
                foreach (CalendarRecurringTask task in thirdPriorityTasks)
                {
                    if (TryAdd(workingDay, task))
                        workingCopy.DoTask(task, workingDay);
                }
                // Tasks that can occur on a range of dates at flexible times have last priority and are sorted internally by days until they go critical
                List<CalendarRecurringTask> fourthPriorityTasks = todaysTasks.FindAll(x => x.TaskTimeType == CalendarRecurringTask.TimeType.RangeTime
                    && x.TaskReccuranceType == CalendarRecurringTask.ReccuranceType.XToYDays).OrderBy(x => x.DaysUntillCritical(workingDay)).ToList();
                foreach (CalendarRecurringTask task in fourthPriorityTasks)
                {
                    if (TryAdd(workingDay, task))
                        workingCopy.DoTask(task, workingDay);
                }
            }
        }

        private bool TryAdd(DateTime theDate, CalendarRecurringTask task)
        {
            if (task.TaskTimeType == CalendarRecurringTask.TimeType.SetTime)
                if (TimeslotAvailable(theDate, task.TimeParamLower, task.TimeParamUpper))
                {
                    Data.Find(x => x.Key == theDate).Value.Add(new KeyValuePair<CalendarRecurringTask, TimeSpan>(task, task.TimeParamLower));
                    return true;
                }
            if (task.TaskTimeType == CalendarRecurringTask.TimeType.RangeTime)
                {
                    TimeSpan minStartTime = task.TimeParamLower;
                    TimeSpan maxStartTime = task.TimeParamUpper - task.GetDuration();
                    TimeSpan tryTime = minStartTime;
                    Boolean timeslotFound = false, rangeExhausted = false;
                    while (!timeslotFound && !rangeExhausted)
                    {
                        if (TimeslotAvailable(theDate, tryTime, tryTime + task.GetDuration()))
                        {
                            Data.Find(x => x.Key == theDate).Value.Add(new KeyValuePair<CalendarRecurringTask, TimeSpan>(task, tryTime));
                            timeslotFound = true;
                        }
                        else
                        {
                            tryTime = tryTime + planningResolution;
                            if (tryTime > maxStartTime)
                                rangeExhausted = true;
                        }
                    }
                    return timeslotFound;
                }
            return false;
        }

        private bool TimeslotAvailable(DateTime theDate, TimeSpan startTime, TimeSpan endTime)
        {
            try
            {
                List<KeyValuePair<CalendarRecurringTask, TimeSpan>> datesTasks = Data.Find(x => x.Key == theDate).Value;
                if (datesTasks.Exists(x => x.Value < endTime && x.Value + x.Key.GetDuration() > startTime))
                    return false;
                else
                    return true;
            }
            catch
            {
                return false;
            }
        }
    }
}