using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;

namespace Aspie_Planner
{
    public partial class RecurringTaskForm : Form
    {
        CalendarContent calendarContent;
        TaskOverview parentTaskOverview;
        string taskGuid;
        CalendarRecurringTask oldTask;
        Color taskBackColor, taskForeColor;
        ColorDialog colorDialog1;
        CalendarRecurringTask.ReccuranceType selectedRecurranceType;
        CalendarRecurringTask.TimeType selectedTimeType;

        public RecurringTaskForm(CalendarContent newCalendarContent, TaskOverview newParentForm)
        {
            InitializeComponent();
            MaximumSize = Size;
            MinimumSize = Size;
            taskBackColor = Color.White;
            taskForeColor = Color.Black;
            description.BackColor = taskBackColor;
            description.ForeColor = taskForeColor;
            this.calendarContent = newCalendarContent;
            this.parentTaskOverview = newParentForm;
            this.colorDialog1 = this.calendarContent.colorPicker;
            timePickerRangeEstimatedLength.Value = DateTime.Today;
            timePickerRangeFrom.Value = DateTime.Today;
            timePickerRangeTo.Value = DateTime.Today;
            timePickerSetFrom.Value = DateTime.Today;
            timePickerSetTo.Value = DateTime.Today;
            DisableAllDateInputs();
            DisableAllTimeInputs();
            selectedRecurranceType = CalendarRecurringTask.ReccuranceType.Undefined;
            selectedTimeType = CalendarRecurringTask.TimeType.Unspecified;
        }

        public RecurringTaskForm(CalendarContent newCalendarContent, TaskOverview newParentForm, string newTaskGuid)
        {
            InitializeComponent();
            this.calendarContent = newCalendarContent;
            this.parentTaskOverview = newParentForm;
            taskGuid = newTaskGuid;
            oldTask = newCalendarContent.GetTask(taskGuid);
            taskBackColor = oldTask.BackColor;
            taskForeColor = oldTask.TextColor;
            description.BackColor = oldTask.BackColor;
            description.ForeColor = oldTask.TextColor;
            description.Text = oldTask.TaskDescription;
            this.colorDialog1 = this.calendarContent.colorPicker;
            timePickerRangeEstimatedLength.Value = DateTime.Today;
            timePickerRangeFrom.Value = DateTime.Today;
            timePickerRangeTo.Value = DateTime.Today;
            timePickerSetFrom.Value = DateTime.Today;
            timePickerSetTo.Value = DateTime.Today;
            DisableAllDateInputs();
            DisableAllTimeInputs();
            selectedRecurranceType = oldTask.TaskReccuranceType;
            switch (oldTask.TaskReccuranceType)
            {
                case CalendarRecurringTask.ReccuranceType.Weekly:
                    ActivateWeekly();
                    rbWeekly.Checked = true;
                    foreach (string s in oldTask.Weekdays)
                    {
                        weekdaysSelectedBox.SetItemChecked(weekdaysSelectedBox.FindString(s), true);
                    }
                    break;
                case CalendarRecurringTask.ReccuranceType.XToYDays:
                    ActivateDayCountInterval();
                    rbDayCountInterval.Checked = true;
                    dateRangeLower.Text = oldTask.DayRangeLower.ToString();
                    dateRangeUpper.Text = oldTask.DayRangeUpper.ToString();
                    break;
                case CalendarRecurringTask.ReccuranceType.EveryNthDay:
                    ActivateDayCountFixed();
                    rbDayCountFixed.Checked = true;
                    everyNthDay.Text = oldTask.DayRangeLower.ToString();
                    firstDate.Value = oldTask.OffsetDate;
                    break;
                default:
                    break;
            }
            switch (oldTask.TaskTimeType)
            {
                case CalendarRecurringTask.TimeType.RangeTime:
                    ActivateRangeTime();
                    rbRangeTime.Checked = true;
                    timePickerRangeFrom.Value = DateTime.Today.Add(oldTask.TimeParamLower);
                    timePickerRangeTo.Value = DateTime.Today.Add(oldTask.TimeParamUpper);
                    timePickerRangeEstimatedLength.Value = DateTime.Today.Add(oldTask.GetDuration());
                    break;
                case CalendarRecurringTask.TimeType.SetTime:
                    ActivateFixedTime();
                    rbSetTime.Checked = true;
                    timePickerSetFrom.Value = DateTime.Today.Add(oldTask.TimeParamLower);
                    timePickerSetTo.Value = DateTime.Today.Add(oldTask.TimeParamUpper);
                    break;
                default:
                    break;
            }
        }

        private void RecurringSave_Click(object sender, EventArgs e)
        {
            statusMessage.Visible = false;
            if (taskGuid == null)
            {
                CalendarRecurringTask.ReccuranceType recurranceType = CalendarRecurringTask.ReccuranceType.Undefined;
                CalendarRecurringTask.TimeType timeType = CalendarRecurringTask.TimeType.Unspecified;
                List<string> weekdaysPicked = new List<string>();
                int rangeLower = 0, rangeUpper = 0;
                TimeSpan timeLower = TimeSpan.Zero, timeUpper = TimeSpan.Zero, duration = TimeSpan.Zero;
                DateTime offsetDate = new DateTime();
                Boolean taskReady = false;
                switch(selectedRecurranceType)
                {
                    case CalendarRecurringTask.ReccuranceType.XToYDays:
                        try
                        {
                            recurranceType = CalendarRecurringTask.ReccuranceType.XToYDays;
                            rangeLower = Convert.ToInt32(dateRangeLower.Text);
                            rangeUpper = Convert.ToInt32(dateRangeUpper.Text);
                            if (rangeLower > 0 && rangeUpper > 0)
                                taskReady = true;
                            else
                                throw new Exception();
                        }
                        catch
                        {
                            statusMessage.Text = "Indtast gyldigt tal i begge felter\n for dagsinterval";
                            statusMessage.ForeColor = Color.Red;
                            statusMessage.Visible = true;
                        }
                        break;
                    case CalendarRecurringTask.ReccuranceType.EveryNthDay:
                        try
                        {
                            recurranceType = CalendarRecurringTask.ReccuranceType.EveryNthDay;
                            rangeLower = Convert.ToInt32(everyNthDay.Text);
                            offsetDate = firstDate.Value.Date;
                            if (rangeLower > 0)
                                taskReady = true;
                            else
                                throw new Exception();
                        }
                        catch
                        {
                            statusMessage.Text = "Indtast gyldigt tal i feltet\n for fast dagsinterval";
                            statusMessage.ForeColor = Color.Red;
                            statusMessage.Visible = true;
                        }
                        break;
                    case CalendarRecurringTask.ReccuranceType.Weekly:
                        try
                        {
                            recurranceType = CalendarRecurringTask.ReccuranceType.Weekly;
                            foreach (object itemChecked in weekdaysSelectedBox.CheckedItems)
                                weekdaysPicked.Add(itemChecked.ToString());
                            if (weekdaysPicked.Count > 0)
                                taskReady = true;
                            else
                                throw new Exception();
                        }
                        catch
                        {
                            statusMessage.Text = "Vælg mindst en ugedag\n for ugentlig begivenhed";
                            statusMessage.ForeColor = Color.Red;
                            statusMessage.Visible = true;
                        }
                        break;
                    default:
                        statusMessage.Text = "Vælg gentagelsesmønster";
                        statusMessage.ForeColor = Color.Red;
                        statusMessage.Visible = true;
                        break;
                }

                if (taskReady)
                {
                    switch (selectedTimeType)
                    {
                        case CalendarRecurringTask.TimeType.SetTime:
                            timeType = CalendarRecurringTask.TimeType.SetTime;
                            timeLower = new TimeSpan(timePickerSetFrom.Value.Hour, timePickerSetFrom.Value.Minute, 0);
                            timeUpper = new TimeSpan(timePickerSetTo.Value.Hour, timePickerSetTo.Value.Minute, 0);
                            break;
                        case CalendarRecurringTask.TimeType.RangeTime:
                            timeType = CalendarRecurringTask.TimeType.RangeTime;
                            timeLower = new TimeSpan(timePickerRangeFrom.Value.Hour, timePickerRangeFrom.Value.Minute, 0);
                            timeUpper = new TimeSpan(timePickerRangeTo.Value.Hour, timePickerRangeTo.Value.Minute, 0);
                            duration = new TimeSpan(timePickerRangeEstimatedLength.Value.Hour, timePickerRangeEstimatedLength.Value.Minute, 0);
                            break;
                        default:
                            break;
                    }
                    calendarContent.AddRecurringTask(new CalendarRecurringTask(recurranceType, timeType, weekdaysPicked, rangeLower, rangeUpper,
                        timeLower, timeUpper, duration, description.Text, taskForeColor, taskBackColor, offsetDate));
                    statusMessage.Text = "Begivenhed gemt";
                    statusMessage.ForeColor = Color.Black;
                    statusMessage.Visible = true;
                }
            }
            else
            {
                CalendarRecurringTask.ReccuranceType recurranceType = CalendarRecurringTask.ReccuranceType.Undefined;
                CalendarRecurringTask.TimeType timeType = CalendarRecurringTask.TimeType.Unspecified;
                List<string> weekdaysPicked = new List<string>();
                int rangeLower = 0, rangeUpper = 0;
                TimeSpan timeLower = TimeSpan.Zero, timeUpper = TimeSpan.Zero, duration = TimeSpan.Zero;
                DateTime offsetDate = new DateTime();
                Boolean taskReady = false;
                
                switch (selectedRecurranceType)
                {
                    case CalendarRecurringTask.ReccuranceType.EveryNthDay:
                        try
                        {
                            recurranceType = CalendarRecurringTask.ReccuranceType.EveryNthDay;
                            rangeLower = Convert.ToInt32(everyNthDay.Text);
                            offsetDate = firstDate.Value.Date;
                            if (rangeLower > 0)
                                taskReady = true;
                            else
                                throw new Exception();
                        }
                        catch
                        {
                            statusMessage.Text = "Indtast gyldigt tal i feltet\n for fast dagsinterval";
                            statusMessage.ForeColor = Color.Red;
                            statusMessage.Visible = true;
                        }
                        break;
                    case CalendarRecurringTask.ReccuranceType.Weekly:
                        try
                        {
                            recurranceType = CalendarRecurringTask.ReccuranceType.Weekly;
                            foreach (object itemChecked in weekdaysSelectedBox.CheckedItems)
                                weekdaysPicked.Add(itemChecked.ToString());
                            offsetDate = oldTask.OffsetDate;
                            if (weekdaysPicked.Count > 0)
                                taskReady = true;
                            else
                                throw new Exception();
                        }
                        catch
                        {
                            statusMessage.Text = "Vælg mindst en ugedag\n for ugentlig gentagelse";
                            statusMessage.ForeColor = Color.Red;
                            statusMessage.Visible = true;
                        }
                        break;
                    case CalendarRecurringTask.ReccuranceType.XToYDays:
                        try
                        {
                            recurranceType = CalendarRecurringTask.ReccuranceType.XToYDays;
                            rangeLower = Convert.ToInt32(dateRangeLower.Text);
                            rangeUpper = Convert.ToInt32(dateRangeUpper.Text);
                            offsetDate = oldTask.OffsetDate;
                            if (rangeLower > 0 && rangeUpper > 0)
                                taskReady = true;
                            else
                                throw new Exception();
                        }
                        catch
                        {
                            statusMessage.Text = "Indtast gyldigt tal i begge felter\n for dagsinterval";
                            statusMessage.ForeColor = Color.Red;
                            statusMessage.Visible = true;
                        }
                        break;
                    default:
                        statusMessage.Text = "Vælg gentagelsesmønster";
                        statusMessage.ForeColor = Color.Red;
                        statusMessage.Visible = true;
                        break;
                }
                
                if (taskReady)
                {
                    switch (selectedTimeType)
                    {
                        case CalendarRecurringTask.TimeType.SetTime:
                            timeType = CalendarRecurringTask.TimeType.SetTime;
                            timeLower = new TimeSpan(timePickerSetFrom.Value.Hour, timePickerSetFrom.Value.Minute, 0);
                            timeUpper = new TimeSpan(timePickerSetTo.Value.Hour, timePickerSetTo.Value.Minute, 0);
                            break;
                        case CalendarRecurringTask.TimeType.RangeTime:
                            timeType = CalendarRecurringTask.TimeType.RangeTime;
                            timeLower = new TimeSpan(timePickerRangeFrom.Value.Hour, timePickerRangeFrom.Value.Minute, 0);
                            timeUpper = new TimeSpan(timePickerRangeTo.Value.Hour, timePickerRangeTo.Value.Minute, 0);
                            duration = new TimeSpan(timePickerRangeEstimatedLength.Value.Hour, timePickerRangeEstimatedLength.Value.Minute, 0);
                            break;
                        default:
                            break;
                    }
                    calendarContent.ModifyTask(oldTask, recurranceType, timeType, weekdaysPicked, rangeLower, rangeUpper, timeLower,
                    timeUpper, duration, description.Text, taskForeColor, taskBackColor, offsetDate);
                    statusMessage.Text = "Begivenhed gemt";
                    statusMessage.ForeColor = Color.Black;
                    statusMessage.Visible = true;
                }
            }
            if (parentTaskOverview != null)
            {
                parentTaskOverview.RepopulateViews();
            }
        }

        private void TextColorPickButton_Click(object sender, EventArgs e)
        {
            int[] customColors = colorDialog1.CustomColors.ToArray();
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                taskForeColor = colorDialog1.Color;
                description.ForeColor = taskForeColor;
            }
            if (!customColors.SequenceEqual(colorDialog1.CustomColors))
            {
                calendarContent.LogUpdateCustomColors();
            }
        }

        private void DisableAllDateInputs()
        {
            dateRangeLower.Enabled = false;
            dateRangeUpper.Enabled = false;
            labelDayCountInterval1.Enabled = false;
            labelDayCountInterval2.Enabled = false;
            everyNthDay.Enabled = false;
            firstDate.Enabled = false;
            labelDayCountFixed1.Enabled = false;
            labelDayCountFixed2.Enabled = false;
            labelDayCountFixed3.Enabled = false;
            weekdaysSelectedBox.Enabled = false;
        }

        private void ActivateDayCountInterval()
        {
            dateRangeLower.Enabled = true;
            dateRangeUpper.Enabled = true;
            labelDayCountInterval1.Enabled = true;
            labelDayCountInterval2.Enabled = true;

        }

        private void RbDayCountInterval_CheckedChanged(object sender, EventArgs e)
        {
            DisableAllDateInputs();
            ActivateDayCountInterval();
            selectedRecurranceType = CalendarRecurringTask.ReccuranceType.XToYDays;
        }

        private void ActivateDayCountFixed()
        {
            everyNthDay.Enabled = true;
            firstDate.Enabled = true;
            labelDayCountFixed1.Enabled = true;
            labelDayCountFixed2.Enabled = true;
            labelDayCountFixed3.Enabled = true;
        }

        private void RbDayCountFixed_CheckedChanged(object sender, EventArgs e)
        {
            DisableAllDateInputs();
            ActivateDayCountFixed();
            selectedRecurranceType = CalendarRecurringTask.ReccuranceType.EveryNthDay;
        }

        private void ActivateWeekly()
        {
            weekdaysSelectedBox.Enabled = true;
        }

        private void RbWeekly_CheckedChanged(object sender, EventArgs e)
        {
            DisableAllDateInputs();
            ActivateWeekly();
            selectedRecurranceType = CalendarRecurringTask.ReccuranceType.Weekly;
        }

        private void DisableAllTimeInputs()
        {
            labelSetTimeFrom.Enabled = false;
            labelSetTimeTo.Enabled = false;
            timePickerSetFrom.Enabled = false;
            timePickerSetTo.Enabled = false;
            labelRangeTimeDesc.Enabled = false;
            labelRangeTimeFrom.Enabled = false;
            labelRangeTimeTo.Enabled = false;
            labelEstimatedDuration.Enabled = false;
            timePickerRangeFrom.Enabled = false;
            timePickerRangeTo.Enabled = false;
            timePickerRangeEstimatedLength.Enabled = false;
        }

        private void RbNoTime_CheckedChanged(object sender, EventArgs e)
        {
            DisableAllTimeInputs();
            selectedTimeType = CalendarRecurringTask.TimeType.Unspecified;
        }

        private void ActivateFixedTime()
        {
            labelSetTimeFrom.Enabled = true;
            labelSetTimeTo.Enabled = true;
            timePickerSetFrom.Enabled = true;
            timePickerSetTo.Enabled = true;
        }

        private void RbSetTime_CheckedChanged(object sender, EventArgs e)
        {
            DisableAllTimeInputs();
            ActivateFixedTime();
            selectedTimeType = CalendarRecurringTask.TimeType.SetTime;
        }

        private void ActivateRangeTime()
        {
            labelRangeTimeDesc.Enabled = true;
            labelRangeTimeFrom.Enabled = true;
            labelRangeTimeTo.Enabled = true;
            labelEstimatedDuration.Enabled = true;
            timePickerRangeFrom.Enabled = true;
            timePickerRangeTo.Enabled = true;
            timePickerRangeEstimatedLength.Enabled = true;
        }

        private void RbRangeTime_CheckedChanged(object sender, EventArgs e)
        {
            DisableAllTimeInputs();
            ActivateRangeTime();
            selectedTimeType = CalendarRecurringTask.TimeType.RangeTime;
        }

        private void ColorPickButton_Click(object sender, EventArgs e)
        {
            int[] customColors = colorDialog1.CustomColors.ToArray();
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                taskBackColor = colorDialog1.Color;
                description.BackColor = taskBackColor;
            }
            if (!customColors.SequenceEqual(colorDialog1.CustomColors))
            {
                calendarContent.LogUpdateCustomColors();
            }
        }
    }
}