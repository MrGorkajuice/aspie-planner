using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspie_Planner
{
    class TaskDay
    {
        public CalendarRecurringTask task;
        public DateTime date;

        public TaskDay(CalendarRecurringTask task, DateTime date)
        {
            this.task = task;
            this.date = date;
        }

        public CalendarRecurringTask.TaskStatus GetStatus()
        {
            return task.GetStatus(date);
        }

        public void SetDone()
        {
            this.task.DoTask(this.date);
        }

        public void SetUndone()
        {
            this.task.UndoTask(this.date);
        }
    }
}
