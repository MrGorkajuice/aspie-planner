using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace Aspie_Planner
{
    public class CalendarContent
    {
        private enum ChangeEvent
        {
            TaskReorder,
            AddTask,
            ModifyTask,
            DoTask,
            UndoTask,
            AddNotesToDay,
            AddNotesToTask,
            DeleteTask,
            ModifyCustomColors,
            ChangeFormHeight,
            ChangeFormWidth,
            ChangeSplitterPosition
        }
        private List<CalendarDateNote> calendarDateNotes;
        private List<CalendarRecurringTask> calendarRecurringTasks;
        private FileStream changeLog;
        private BinaryWriter changeLogWriter;
        private Boolean isParsingChangelog;
        private Boolean isWorkingCopy = false;
        public int PreferenceFormHeight { get; private set; }
        public int PreferenceFormWidth { get; private set; }
        public int PreferenceSplitterPosition { get; private set; }
        public List<KeyValuePair<DayOfWeek, TimeSpan>> PreferenceDayStart, PreferenceDayEnd;
        public ColorDialog colorPicker;

        // Creating, saving, loading...
        // Empty calendar creation
        public CalendarContent()
        {
            NewSetup();
            SetupChangelog();
        }

        // Load by filename
        public CalendarContent(string filename)
        {
            try
            {
                try
                {
                    Directory.CreateDirectory("backups");
                    string clgBackupFilename = @".\backups\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".data";
                    File.Copy(filename, clgBackupFilename, true);
                }
                catch { }
                using (var stream = File.OpenRead(filename))
                {
                    InitFromStream(stream);
                }
            }
            catch
            {
                NewSetup();
                SetupChangelog();
                SaveCalendar();
            }
        }

        // Load by filestream
        public CalendarContent(FileStream stream)
        {
            InitFromStream(stream);
        }
        private CalendarContent(bool isCloning)
        {
            NewSetup();
            if (!isCloning)
                SetupChangelog();
            else
                isWorkingCopy = true;
        }

        public CalendarContent Clone()
        {
            CalendarContent result = new CalendarContent(true);
            result.PreferenceDayStart = new List<KeyValuePair<DayOfWeek, TimeSpan>>(this.PreferenceDayStart);
            result.PreferenceDayEnd = new List<KeyValuePair<DayOfWeek, TimeSpan>>(this.PreferenceDayEnd);
            foreach (CalendarDateNote calendarDateNote in calendarDateNotes)
                result.calendarDateNotes.Add(calendarDateNote.Clone());
            foreach (CalendarRecurringTask task in calendarRecurringTasks)
                result.calendarRecurringTasks.Add(task.Clone());
            return result;
        }
        // Check content file version and forward processing accordingly
        private void InitFromStream(FileStream stream)
        {
            var reader = new BinaryReader(stream);
            int versionCheck = reader.ReadInt32();
            if (versionCheck == -4)
            {
                Version4Load(reader);
                stream.Dispose();
                ProcessChangeLogV5();
            }
            else if (versionCheck == -5)
            {
                Version5Load(reader);
                stream.Dispose();
                ProcessChangeLogV5();
            }
            else if (versionCheck == -6)
            {
                Version6Load(reader);
                stream.Dispose();
                ProcessChangeLogV6();
            }
            else
                NewSetup();
            SetupChangelog();
        }

        // Initialize content lists when creating empty calendar
        private void NewSetup()
        {
            calendarDateNotes = new List<CalendarDateNote>();
            calendarRecurringTasks = new List<CalendarRecurringTask>();
            colorPicker = new ColorDialog();
            PreferenceFormHeight = 480;
            PreferenceFormWidth = 640;
            PreferenceSplitterPosition = 200;
            DefaultAvailableHours();
        }

        // Prepare new changelog
        private void SetupChangelog()
        {
            changeLog = File.Create("aspieplanner.clg");
            changeLogWriter = new BinaryWriter(changeLog);
            isParsingChangelog = false;
        }

        // Process last changelog
        private void ProcessChangeLogV6()
        {
            try
            {
                string clgBackupFilename = @".\backups\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".clg";
                File.Copy("aspieplanner.clg", clgBackupFilename, true);
            }
            catch { }
            string description, taskGuid;
            List<string> weekdays;
            int dayRangeX, dayRangeY, weekdayCount;
            Color textColor, backColor;
            CalendarRecurringTask.ReccuranceType recurranceType;
            CalendarRecurringTask.TimeType timeType;
            TimeSpan timeLower = TimeSpan.Zero, timeUpper = TimeSpan.Zero, duration = TimeSpan.Zero;
            CalendarRecurringTask loadedTask;
            DateTime dateTime;
            try
            {
                changeLog = File.OpenRead("aspieplanner.clg");
                using (var changeLogReader = new BinaryReader(changeLog))
                {
                    Boolean keepReading = true;
                    isParsingChangelog = true;
                    while (keepReading)
                    {
                        try
                        {
                            ChangeEvent readEvent = (ChangeEvent)changeLogReader.ReadInt32();
                            switch (readEvent)
                            {
                                case ChangeEvent.TaskReorder:
                                    CalendarRecurringTask reorderTask = GetTask(changeLogReader.ReadString());
                                    int position = changeLogReader.ReadInt32();
                                    AssignTaskPosition(reorderTask, position);
                                    break;
                                case ChangeEvent.AddTask:
                                    recurranceType = (CalendarRecurringTask.ReccuranceType)changeLogReader.ReadInt32();
                                    timeType = (CalendarRecurringTask.TimeType)changeLogReader.ReadInt32();
                                    weekdayCount = changeLogReader.ReadInt32();
                                    weekdays = new List<string>();
                                    for (int i = 0; i < weekdayCount; i++)
                                    {
                                        weekdays.Add(changeLogReader.ReadString());
                                    }
                                    dayRangeX = changeLogReader.ReadInt32();
                                    dayRangeY = changeLogReader.ReadInt32();
                                    timeLower = new TimeSpan(0, changeLogReader.ReadInt32(), 0);
                                    timeUpper = new TimeSpan(0, changeLogReader.ReadInt32(), 0);
                                    duration = new TimeSpan(0, changeLogReader.ReadInt32(), 0);
                                    description = changeLogReader.ReadString();
                                    textColor = Color.FromArgb(changeLogReader.ReadInt32());
                                    backColor = Color.FromArgb(changeLogReader.ReadInt32());
                                    dateTime = DateTime.FromBinary(changeLogReader.ReadInt64());
                                    taskGuid = changeLogReader.ReadString();
                                    calendarRecurringTasks.Add(new CalendarRecurringTask(recurranceType, timeType, weekdays, dayRangeX, dayRangeY,
                                        timeLower, timeUpper, duration, description, textColor, backColor, dateTime, taskGuid));
                                    break;
                                case ChangeEvent.ModifyTask:
                                    recurranceType = (CalendarRecurringTask.ReccuranceType)changeLogReader.ReadInt32();
                                    timeType = (CalendarRecurringTask.TimeType)changeLogReader.ReadInt32();
                                    weekdayCount = changeLogReader.ReadInt32();
                                    weekdays = new List<string>();
                                    for (int i = 0; i < weekdayCount; i++)
                                    {
                                        weekdays.Add(changeLogReader.ReadString());
                                    }
                                    dayRangeX = changeLogReader.ReadInt32();
                                    dayRangeY = changeLogReader.ReadInt32();
                                    timeLower = new TimeSpan(0, changeLogReader.ReadInt32(), 0);
                                    timeUpper = new TimeSpan(0, changeLogReader.ReadInt32(), 0);
                                    duration = new TimeSpan(0, changeLogReader.ReadInt32(), 0);
                                    description = changeLogReader.ReadString();
                                    textColor = Color.FromArgb(changeLogReader.ReadInt32());
                                    backColor = Color.FromArgb(changeLogReader.ReadInt32());
                                    dateTime = DateTime.FromBinary(changeLogReader.ReadInt64());
                                    taskGuid = changeLogReader.ReadString();
                                    loadedTask = GetTask(taskGuid);
                                    loadedTask.Modify(recurranceType, timeType, weekdays, dayRangeX, dayRangeY, timeLower, timeUpper,
                                        duration, description, textColor, backColor, dateTime);
                                    break;
                                case ChangeEvent.DoTask:
                                    CalendarRecurringTask toDoTask = GetTask(changeLogReader.ReadString());
                                    toDoTask.DoTask(DateTime.FromBinary(changeLogReader.ReadInt64()));
                                    break;
                                case ChangeEvent.UndoTask:
                                    CalendarRecurringTask toUndoTask = GetTask(changeLogReader.ReadString());
                                    toUndoTask.UndoTask(DateTime.FromBinary(changeLogReader.ReadInt64()));
                                    break;
                                case ChangeEvent.AddNotesToDay:
                                    dateTime = DateTime.FromBinary(changeLogReader.ReadInt64());
                                    description = changeLogReader.ReadString();
                                    ChangeDayNotes(dateTime, description);
                                    break;
                                case ChangeEvent.AddNotesToTask:
                                    CalendarRecurringTask toNoteTask = GetTask(changeLogReader.ReadString());
                                    dateTime = DateTime.FromBinary(changeLogReader.ReadInt64());
                                    description = changeLogReader.ReadString();
                                    toNoteTask.SetNote(dateTime, description);
                                    break;
                                case ChangeEvent.DeleteTask:
                                    DeleteTask(changeLogReader.ReadString());
                                    break;
                                case ChangeEvent.ModifyCustomColors:
                                    int colorCount = changeLogReader.ReadInt32();
                                    int[] colors = new int[colorCount];
                                    for (int i = 0; i < colorCount; i++)
                                    {
                                        colors[i] = changeLogReader.ReadInt32();
                                    }
                                    colorPicker.CustomColors = colors;
                                    break;
                                case ChangeEvent.ChangeFormHeight:
                                    PreferenceFormHeight = changeLogReader.ReadInt32();
                                    break;
                                case ChangeEvent.ChangeFormWidth:
                                    PreferenceFormWidth = changeLogReader.ReadInt32();
                                    break;
                                case ChangeEvent.ChangeSplitterPosition:
                                    PreferenceSplitterPosition = changeLogReader.ReadInt32();
                                    break;
                                default:
                                    keepReading = false;
                                    break;
                            }
                        }
                        catch
                        {
                            keepReading = false;
                        }
                    }
                }
                SaveCalendar();
            }
            catch
            { }
        }

        private void ProcessChangeLogV5()
        {
            try
            {
                string clgBackupFilename = @".\backups\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".clg";
                File.Copy("aspieplanner.clg", clgBackupFilename, true);
            }
            catch { }
            string description, taskGuid;
            List<string> weekdays;
            int dayRangeX, dayRangeY, weekdayCount;
            Color textColor, backColor;
            CalendarRecurringTask.ReccuranceType recurranceType;
            CalendarRecurringTask loadedTask;
            DateTime dateTime;
            try
            {
                changeLog = File.OpenRead("aspieplanner.clg");
                using (var changeLogReader = new BinaryReader(changeLog))
                {
                    Boolean keepReading = true;
                    isParsingChangelog = true;
                    while (keepReading)
                    {
                        try
                        {
                            ChangeEvent readEvent = (ChangeEvent)changeLogReader.ReadInt32();
                            switch (readEvent)
                            {
                                case ChangeEvent.TaskReorder:
                                    CalendarRecurringTask reorderTask = GetTask(changeLogReader.ReadString());
                                    int position = changeLogReader.ReadInt32();
                                    AssignTaskPosition(reorderTask, position);
                                    break;
                                case ChangeEvent.AddTask:
                                    recurranceType = (CalendarRecurringTask.ReccuranceType)changeLogReader.ReadInt32();
                                    weekdayCount = changeLogReader.ReadInt32();
                                    weekdays = new List<string>();
                                    for (int i = 0; i < weekdayCount; i++)
                                    {
                                        weekdays.Add(changeLogReader.ReadString());
                                    }
                                    dayRangeX = changeLogReader.ReadInt32();
                                    dayRangeY = changeLogReader.ReadInt32();
                                    description = changeLogReader.ReadString();
                                    textColor = Color.FromArgb(changeLogReader.ReadInt32());
                                    backColor = Color.FromArgb(changeLogReader.ReadInt32());
                                    dateTime = DateTime.FromBinary(changeLogReader.ReadInt64());
                                    taskGuid = changeLogReader.ReadString();
                                    calendarRecurringTasks.Add(new CalendarRecurringTask(recurranceType, CalendarRecurringTask.TimeType.Unspecified, weekdays,
                                        dayRangeX, dayRangeY, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, description, textColor, backColor, dateTime, taskGuid));
                                    break;
                                case ChangeEvent.ModifyTask:
                                    recurranceType = (CalendarRecurringTask.ReccuranceType)changeLogReader.ReadInt32();
                                    weekdayCount = changeLogReader.ReadInt32();
                                    weekdays = new List<string>();
                                    for (int i = 0; i < weekdayCount; i++)
                                    {
                                        weekdays.Add(changeLogReader.ReadString());
                                    }
                                    dayRangeX = changeLogReader.ReadInt32();
                                    dayRangeY = changeLogReader.ReadInt32();
                                    description = changeLogReader.ReadString();
                                    textColor = Color.FromArgb(changeLogReader.ReadInt32());
                                    backColor = Color.FromArgb(changeLogReader.ReadInt32());
                                    dateTime = DateTime.FromBinary(changeLogReader.ReadInt64());
                                    taskGuid = changeLogReader.ReadString();
                                    loadedTask = GetTask(taskGuid);
                                    loadedTask.Modify(recurranceType, CalendarRecurringTask.TimeType.Unspecified, weekdays, dayRangeX, dayRangeY,
                                        TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, description, textColor, backColor, dateTime);
                                    break;
                                case ChangeEvent.DoTask:
                                    CalendarRecurringTask toDoTask = GetTask(changeLogReader.ReadString());
                                    toDoTask.DoTask(DateTime.FromBinary(changeLogReader.ReadInt64()));
                                    break;
                                case ChangeEvent.UndoTask:
                                    CalendarRecurringTask toUndoTask = GetTask(changeLogReader.ReadString());
                                    toUndoTask.UndoTask(DateTime.FromBinary(changeLogReader.ReadInt64()));
                                    break;
                                case ChangeEvent.AddNotesToDay:
                                    dateTime = DateTime.FromBinary(changeLogReader.ReadInt64());
                                    description = changeLogReader.ReadString();
                                    ChangeDayNotes(dateTime, description);
                                    break;
                                case ChangeEvent.AddNotesToTask:
                                    CalendarRecurringTask toNoteTask = GetTask(changeLogReader.ReadString());
                                    dateTime = DateTime.FromBinary(changeLogReader.ReadInt64());
                                    description = changeLogReader.ReadString();
                                    toNoteTask.SetNote(dateTime, description);
                                    break;
                                case ChangeEvent.DeleteTask:
                                    DeleteTask(changeLogReader.ReadString());
                                    break;
                                case ChangeEvent.ModifyCustomColors:
                                    int colorCount = changeLogReader.ReadInt32();
                                    int[] colors = new int[colorCount];
                                    for (int i = 0; i < colorCount; i++)
                                    {
                                        colors[i] = changeLogReader.ReadInt32();
                                    }
                                    colorPicker.CustomColors = colors;
                                    break;
                                case ChangeEvent.ChangeFormHeight:
                                    PreferenceFormHeight = changeLogReader.ReadInt32();
                                    break;
                                case ChangeEvent.ChangeFormWidth:
                                    PreferenceFormWidth = changeLogReader.ReadInt32();
                                    break;
                                case ChangeEvent.ChangeSplitterPosition:
                                    PreferenceSplitterPosition = changeLogReader.ReadInt32();
                                    break;
                                default:
                                    keepReading = false;
                                    break;
                            }
                        }
                        catch
                        {
                            keepReading = false;
                        }
                    }
                }
                SaveCalendar();
            }
            catch
            { }
        }

        // Load content file
        private void DefaultAvailableHours()
        {
            PreferenceDayStart = new List<KeyValuePair<DayOfWeek, TimeSpan>>()
            {
                new KeyValuePair<DayOfWeek, TimeSpan>(DayOfWeek.Monday, new TimeSpan(8, 0, 0)),
                new KeyValuePair<DayOfWeek, TimeSpan>(DayOfWeek.Tuesday, new TimeSpan(8, 0, 0)),
                new KeyValuePair<DayOfWeek, TimeSpan>(DayOfWeek.Wednesday, new TimeSpan(8, 0, 0)),
                new KeyValuePair<DayOfWeek, TimeSpan>(DayOfWeek.Thursday, new TimeSpan(8, 0, 0)),
                new KeyValuePair<DayOfWeek, TimeSpan>(DayOfWeek.Friday, new TimeSpan(8, 0, 0)),
                new KeyValuePair<DayOfWeek, TimeSpan>(DayOfWeek.Saturday, new TimeSpan(10, 0, 0)),
                new KeyValuePair<DayOfWeek, TimeSpan>(DayOfWeek.Sunday, new TimeSpan(10, 0, 0))
            };
            PreferenceDayEnd = new List<KeyValuePair<DayOfWeek, TimeSpan>>()
            {
                new KeyValuePair<DayOfWeek, TimeSpan>(DayOfWeek.Monday, new TimeSpan(22, 0, 0)),
                new KeyValuePair<DayOfWeek, TimeSpan>(DayOfWeek.Tuesday, new TimeSpan(22, 0, 0)),
                new KeyValuePair<DayOfWeek, TimeSpan>(DayOfWeek.Wednesday, new TimeSpan(22, 0, 0)),
                new KeyValuePair<DayOfWeek, TimeSpan>(DayOfWeek.Thursday, new TimeSpan(22, 0, 0)),
                new KeyValuePair<DayOfWeek, TimeSpan>(DayOfWeek.Friday, new TimeSpan(23, 0, 0)),
                new KeyValuePair<DayOfWeek, TimeSpan>(DayOfWeek.Saturday, new TimeSpan(23, 0, 0)),
                new KeyValuePair<DayOfWeek, TimeSpan>(DayOfWeek.Sunday, new TimeSpan(22, 0, 0))
            };
        }
        private void Version6Load(BinaryReader reader)
        {
            calendarDateNotes = new List<CalendarDateNote>();
            calendarRecurringTasks = new List<CalendarRecurringTask>();
            Int64 eventDate, doneDate, noteDate;
            string description, taskGuid, noteText;
            List<string> weekdays;
            int dayRangeX, dayRangeY, doneDates, weekdayCount, color;
            Color textColor, backColor;
            CalendarRecurringTask.ReccuranceType recurranceType;
            CalendarRecurringTask.TimeType timeType;
            TimeSpan timeLower = TimeSpan.Zero, timeUpper = TimeSpan.Zero, duration = TimeSpan.Zero;
            CalendarRecurringTask loadedTask;
            DateTime dateTime;
            colorPicker = new ColorDialog();

            PreferenceFormHeight = reader.ReadInt32();
            PreferenceFormWidth = reader.ReadInt32();
            PreferenceSplitterPosition = reader.ReadInt32();

            int calendarEventCount = reader.ReadInt32();
            int recurringTaskCount = reader.ReadInt32();
            for (int i = 0; i < calendarEventCount; i++)
            {
                eventDate = reader.ReadInt64();
                description = reader.ReadString();
                calendarDateNotes.Add(new CalendarDateNote(DateTime.FromBinary(eventDate), description));
            }
            for (int i = 0; i < recurringTaskCount; i++)
            {
                taskGuid = reader.ReadString();
                eventDate = reader.ReadInt64();
                description = reader.ReadString();
                color = reader.ReadInt32();
                textColor = Color.FromArgb(color);
                color = reader.ReadInt32();
                backColor = Color.FromArgb(color);
                weekdayCount = reader.ReadInt32();
                weekdays = new List<string>();
                for (int j = 0; j < weekdayCount; j++)
                {
                    weekdays.Add(reader.ReadString());
                }
                recurranceType = (CalendarRecurringTask.ReccuranceType)reader.ReadInt32();
                timeType = (CalendarRecurringTask.TimeType)reader.ReadInt32();
                dayRangeX = reader.ReadInt32();
                dayRangeY = reader.ReadInt32();
                timeLower = new TimeSpan(0, reader.ReadInt32(), 0);
                timeUpper = new TimeSpan(0, reader.ReadInt32(), 0);
                duration = new TimeSpan(0, reader.ReadInt32(), 0);
                dateTime = DateTime.FromBinary(eventDate);
                calendarRecurringTasks.Add(new CalendarRecurringTask(recurranceType,
                                                                     timeType,
                                                                     weekdays,
                                                                     dayRangeX,
                                                                     dayRangeY,
                                                                     timeLower,
                                                                     timeUpper,
                                                                     duration,
                                                                     description,
                                                                     textColor,
                                                                     backColor,
                                                                     dateTime,
                                                                     taskGuid));
                loadedTask = calendarRecurringTasks.Find(x => x.TaskGuid.ToString() == taskGuid);
                doneDates = reader.ReadInt32();
                for (int j = 0; j < doneDates; j++)
                {
                    doneDate = reader.ReadInt64();
                    loadedTask.DatesDone.Add(DateTime.FromBinary(doneDate));
                }
                int taskNotes = reader.ReadInt32();
                for (int j = 0; j < taskNotes; j++)
                {
                    noteDate = reader.ReadInt64();
                    noteText = reader.ReadString();
                    loadedTask.SetNote(DateTime.FromBinary(noteDate), noteText);
                }
            }
            int customColorCount = reader.ReadInt32();
            int[] customColors = new int[customColorCount];
            for (int i = 0; i < customColorCount; i++)
            {
                customColors[i] = reader.ReadInt32();
            }
            colorPicker.CustomColors = customColors;
            DefaultAvailableHours();
        }

        private void Version5Load(BinaryReader reader)
        {
            calendarDateNotes = new List<CalendarDateNote>();
            calendarRecurringTasks = new List<CalendarRecurringTask>();
            Int64 eventDate, doneDate, noteDate;
            string description, taskGuid, noteText;
            List<string> weekdays;
            int dayRangeX, dayRangeY, doneDates, weekdayCount, color;
            Color textColor, backColor;
            CalendarRecurringTask.ReccuranceType recurranceType;
            CalendarRecurringTask loadedTask;
            DateTime dateTime;
            colorPicker = new ColorDialog();

            PreferenceFormHeight = reader.ReadInt32();
            PreferenceFormWidth = reader.ReadInt32();
            PreferenceSplitterPosition = reader.ReadInt32();

            int calendarEventCount = reader.ReadInt32();
            int recurringTaskCount = reader.ReadInt32();
            for (int i = 0; i < calendarEventCount; i++)
            {
                eventDate = reader.ReadInt64();
                description = reader.ReadString();
                calendarDateNotes.Add(new CalendarDateNote(DateTime.FromBinary(eventDate), description));
            }
            for (int i = 0; i < recurringTaskCount; i++)
            {
                taskGuid = reader.ReadString();
                eventDate = reader.ReadInt64();
                description = reader.ReadString();
                color = reader.ReadInt32();
                textColor = Color.FromArgb(color);
                color = reader.ReadInt32();
                backColor = Color.FromArgb(color);
                weekdayCount = reader.ReadInt32();
                weekdays = new List<string>();
                for (int j = 0; j < weekdayCount; j++)
                {
                    weekdays.Add(reader.ReadString());
                }
                recurranceType = (CalendarRecurringTask.ReccuranceType)reader.ReadInt32();
                dayRangeX = reader.ReadInt32();
                dayRangeY = reader.ReadInt32();
                dateTime = DateTime.FromBinary(eventDate);
                calendarRecurringTasks.Add(new CalendarRecurringTask(recurranceType,
                                                                     CalendarRecurringTask.TimeType.Unspecified,
                                                                     weekdays,
                                                                     dayRangeX,
                                                                     dayRangeY,
                                                                     TimeSpan.Zero,
                                                                     TimeSpan.Zero,
                                                                     TimeSpan.Zero,
                                                                     description,
                                                                     textColor,
                                                                     backColor,
                                                                     dateTime,
                                                                     taskGuid));
                loadedTask = calendarRecurringTasks.Find(x => x.TaskGuid.ToString() == taskGuid);
                doneDates = reader.ReadInt32();
                for (int j = 0; j < doneDates; j++)
                {
                    doneDate = reader.ReadInt64();
                    loadedTask.DatesDone.Add(DateTime.FromBinary(doneDate));
                }
                int taskNotes = reader.ReadInt32();
                for (int j = 0; j < taskNotes; j++)
                {
                    noteDate = reader.ReadInt64();
                    noteText = reader.ReadString();
                    loadedTask.SetNote(DateTime.FromBinary(noteDate), noteText);
                }
            }
            int customColorCount = reader.ReadInt32();
            int[] customColors = new int[customColorCount];
            for (int i = 0; i < customColorCount; i++)
            {
                customColors[i] = reader.ReadInt32();
            }
            colorPicker.CustomColors = customColors;
            DefaultAvailableHours();
        }

        private void Version4Load(BinaryReader reader)
        {
            calendarDateNotes = new List<CalendarDateNote>();
            calendarRecurringTasks = new List<CalendarRecurringTask>();
            Int64 eventDate, doneDate, noteDate;
            string description, taskGuid, noteText;
            List<string> weekdays;
            int dayRangeX, dayRangeY, doneDates, weekdayCount, color;
            Color textColor, backColor;
            CalendarRecurringTask.ReccuranceType recurranceType;
            CalendarRecurringTask loadedTask;
            DateTime dateTime;
            colorPicker = new ColorDialog();

            PreferenceFormHeight = 480;
            PreferenceFormWidth = 640;
            PreferenceSplitterPosition = 200;

            int calendarEventCount = reader.ReadInt32();
            int recurringTaskCount = reader.ReadInt32();
            for (int i = 0; i < calendarEventCount; i++)
            {
                eventDate = reader.ReadInt64();
                description = reader.ReadString();
                calendarDateNotes.Add(new CalendarDateNote(DateTime.FromBinary(eventDate), description));
            }
            for (int i = 0; i < recurringTaskCount; i++)
            {
                taskGuid = reader.ReadString();
                eventDate = reader.ReadInt64();
                description = reader.ReadString();
                color = reader.ReadInt32();
                textColor = Color.FromArgb(color);
                color = reader.ReadInt32();
                backColor = Color.FromArgb(color);
                weekdayCount = reader.ReadInt32();
                weekdays = new List<string>();
                for (int j = 0; j < weekdayCount; j++)
                {
                    weekdays.Add(reader.ReadString());
                }
                recurranceType = (CalendarRecurringTask.ReccuranceType)reader.ReadInt32();
                dayRangeX = reader.ReadInt32();
                dayRangeY = reader.ReadInt32();
                dateTime = DateTime.FromBinary(eventDate);
                calendarRecurringTasks.Add(new CalendarRecurringTask(recurranceType,
                                                                     CalendarRecurringTask.TimeType.Unspecified,
                                                                     weekdays,
                                                                     dayRangeX,
                                                                     dayRangeY,
                                                                     TimeSpan.Zero,
                                                                     TimeSpan.Zero,
                                                                     TimeSpan.Zero,
                                                                     description,
                                                                     textColor,
                                                                     backColor,
                                                                     dateTime,
                                                                     taskGuid));
                loadedTask = calendarRecurringTasks.Find(x => x.TaskGuid.ToString() == taskGuid);
                doneDates = reader.ReadInt32();
                for (int j = 0; j < doneDates; j++)
                {
                    doneDate = reader.ReadInt64();
                    loadedTask.DatesDone.Add(DateTime.FromBinary(doneDate));
                }
                int taskNotes = reader.ReadInt32();
                for (int j = 0; j < taskNotes; j++)
                {
                    noteDate = reader.ReadInt64();
                    noteText = reader.ReadString();
                    loadedTask.SetNote(DateTime.FromBinary(noteDate), noteText);
                }
            }
            int customColorCount = reader.ReadInt32();
            int[] customColors = new int[customColorCount];
            for (int i = 0; i < customColorCount; i++)
            {
                customColors[i] = reader.ReadInt32();
            }
            colorPicker.CustomColors = customColors;
            DefaultAvailableHours();
        }

        // Write content file using current format (Version 6)
        private void SaveCalendar()
        {
            using (var stream = File.Create("aspieplanner.data"))
            {
                var writer = new BinaryWriter(stream);
                writer.Write(-6);
                writer.Write(PreferenceFormHeight);
                writer.Write(PreferenceFormWidth);
                writer.Write(PreferenceSplitterPosition);
                writer.Write(calendarDateNotes.Count);
                writer.Write(calendarRecurringTasks.Count);
                foreach (CalendarDateNote ce in calendarDateNotes)
                {
                    writer.Write(ce.GetDate().ToBinary());
                    writer.Write(ce.GetNote());
                }
                foreach (CalendarRecurringTask rt in calendarRecurringTasks)
                {
                    writer.Write(rt.TaskGuid.ToString());
                    writer.Write(rt.OffsetDate.ToBinary());
                    writer.Write(rt.TaskDescription);
                    writer.Write(rt.TextColor.ToArgb());
                    writer.Write(rt.BackColor.ToArgb());
                    if (rt.Weekdays != null)
                    {
                        writer.Write(rt.Weekdays.Count);
                        for (int i = 0; i < rt.Weekdays.Count; i++)
                        {
                            writer.Write(rt.Weekdays[i]);
                        }
                    }
                    else
                        writer.Write(0);
                    writer.Write((int)rt.TaskReccuranceType);
                    writer.Write((int)rt.TaskTimeType);
                    writer.Write(rt.DayRangeLower);
                    writer.Write(rt.DayRangeUpper);
                    writer.Write((int)rt.TimeParamLower.TotalMinutes);
                    writer.Write((int)rt.TimeParamUpper.TotalMinutes);
                    writer.Write((int)rt.GetDuration().TotalMinutes);
                    writer.Write(rt.DatesDone.Count());
                    foreach (DateTime dt in rt.DatesDone)
                    {
                        writer.Write(dt.ToBinary());
                    }
                    writer.Write(rt.TaskNotes.Count());
                    foreach (TaskNote tn in rt.TaskNotes)
                    {
                        writer.Write(tn.GetDate().ToBinary());
                        writer.Write(tn.GetNote());
                    }
                }
                writer.Write(colorPicker.CustomColors.Count());
                foreach (int color in colorPicker.CustomColors)
                {
                    writer.Write(color);
                }
            }
        }

        // Content access methods
        // 
        internal void ChangeDayNotes(DateTime date, string description)
        {
            LogChange((int)ChangeEvent.AddNotesToDay);
            LogChange(date.ToBinary());
            LogChange(description);
            CalendarDateNote datesNote = GetDaysNotes(date);
            if (datesNote == null && !string.IsNullOrEmpty(description))
                calendarDateNotes.Add(new CalendarDateNote(date, description));
            else
            {
                if (string.IsNullOrEmpty(description))
                    calendarDateNotes.Remove(datesNote);
                else
                    datesNote.SetNote(description);
            }
        }

        internal CalendarDateNote GetDaysNotes(DateTime theDate)
        {
            CalendarDateNote result = calendarDateNotes.Find(x => x.GetDate() == theDate);
            return result;
        }

        internal List<CalendarDateNote> GetDaysNotes(DateTime theDate, int requestedListLength)
        {
            List<CalendarDateNote> result = calendarDateNotes.FindAll(x => x.GetDate() <= theDate).OrderByDescending(x => x.GetDate()).ToList();
            return result;
        }

        internal List<DateTime> GetAllDates()
        {
            return calendarDateNotes.Select(x => x.GetDate()).ToList();
        }

        internal void AddRecurringTask(CalendarRecurringTask newTask)
        {
            LogChange((int)ChangeEvent.AddTask);
            LogChange((int)newTask.TaskReccuranceType);
            LogChange((int)newTask.TaskTimeType);
            LogChange(newTask.Weekdays.Count());
            foreach (string s in newTask.Weekdays)
            {
                LogChange(s);
            }
            LogChange(newTask.DayRangeLower);
            LogChange(newTask.DayRangeUpper);
            LogChange((int)newTask.TimeParamLower.TotalMinutes);
            LogChange((int)newTask.TimeParamUpper.TotalMinutes);
            LogChange((int)newTask.GetDuration().TotalMinutes);
            LogChange(newTask.TaskDescription);
            LogChange(newTask.TextColor.ToArgb());
            LogChange(newTask.BackColor.ToArgb());
            LogChange(newTask.OffsetDate.ToBinary());
            LogChange(newTask.TaskGuid.ToString());
            calendarRecurringTasks.Add(newTask);
        }

        internal List<CalendarRecurringTask> GetRecurringTasks()
        {
            return calendarRecurringTasks;
        }

        internal List<CalendarRecurringTask> GetRecurringTasks(DateTime date)
        {
            return calendarRecurringTasks.FindAll(x => x.GetStatus(date) != CalendarRecurringTask.TaskStatus.Any);
        }

        internal CalendarRecurringTask GetWeekdaysTask(DateTime theDate, CalendarRecurringTask.TaskStatus statusFilter)
        {
            string weekdayMatch = theDate.ToString("dddd").ToLower();
            CalendarRecurringTask result;
            if (statusFilter == CalendarRecurringTask.TaskStatus.Incomplete)
                result = calendarRecurringTasks.Find(x => x.Weekdays.Exists(y => y == weekdayMatch) &&
                                                          x.TaskReccuranceType == CalendarRecurringTask.ReccuranceType.Weekly &&
                                                          !x.DatesDone.Exists(y => y.Date == theDate));
            else if (statusFilter == CalendarRecurringTask.TaskStatus.Complete)
                result = calendarRecurringTasks.Find(x => x.Weekdays.Exists(y => y == weekdayMatch) &&
                                                          x.TaskReccuranceType == CalendarRecurringTask.ReccuranceType.Weekly &&
                                                          x.DatesDone.Exists(y => y.Date == theDate));
            else
                result = calendarRecurringTasks.Find(x => x.Weekdays.Exists(y => y == weekdayMatch) &&
                                                          x.TaskReccuranceType == CalendarRecurringTask.ReccuranceType.Weekly);
            return result;
        }

        internal CalendarRecurringTask GetTask(string guid)
        {
            CalendarRecurringTask result = calendarRecurringTasks.Find(x => x.TaskGuid.ToString() == guid);
            return result;
        }

        internal void DeleteTask(string guid)
        {
            LogChange((int)ChangeEvent.DeleteTask);
            LogChange(guid);
            CalendarRecurringTask taskToDelete = GetTask(guid);
            calendarRecurringTasks.Remove(taskToDelete);
        }

        internal void AssignTaskPosition(CalendarRecurringTask theTask, int position)
        {
            LogChange((int)ChangeEvent.TaskReorder);
            LogChange(theTask.TaskGuid.ToString());
            LogChange(position);
            int oldPosition = calendarRecurringTasks.FindIndex(x => x == theTask);
            calendarRecurringTasks.RemoveAt(oldPosition);
            if (position >= 0 && position < calendarRecurringTasks.Count)
                calendarRecurringTasks.Insert(position, theTask);
            else
                calendarRecurringTasks.Add(theTask);
        }

        internal void ModifyTask(CalendarRecurringTask theTask,
                                 CalendarRecurringTask.ReccuranceType reccuranceType,
                                 CalendarRecurringTask.TimeType timeType,
                                 List<string> weekdays,
                                 int dayRangeLower,
                                 int dayRangeUpper,
                                 TimeSpan timeLower,
                                 TimeSpan timeUpper,
                                 TimeSpan duration,
                                 string taskDescription,
                                 Color textColor,
                                 Color backColor,
                                 DateTime offsetDate)
        {
            LogChange((int)ChangeEvent.ModifyTask);
            LogChange((int)reccuranceType);
            LogChange((int)timeType);
            LogChange(weekdays.Count());
            foreach (string s in weekdays)
            {
                LogChange(s);
            }
            LogChange(dayRangeLower);
            LogChange(dayRangeUpper);
            LogChange((int)timeLower.TotalMinutes);
            LogChange((int)timeUpper.TotalMinutes);
            LogChange((int)duration.TotalMinutes);
            LogChange(taskDescription);
            LogChange(textColor.ToArgb());
            LogChange(backColor.ToArgb());
            LogChange(offsetDate.ToBinary());
            LogChange(theTask.TaskGuid.ToString());

            theTask.Modify(reccuranceType, timeType, weekdays, dayRangeLower, dayRangeUpper, timeLower, timeUpper, duration,
                taskDescription, textColor, backColor, offsetDate);
        }

        internal void DoTask(CalendarRecurringTask task, DateTime date)
        {
            LogChange((int)ChangeEvent.DoTask);
            LogChange(task.TaskGuid.ToString());
            LogChange(date.ToBinary());
            task.DoTask(date);
        }

        internal void UndoTask(CalendarRecurringTask task, DateTime date)
        {
            LogChange((int)ChangeEvent.UndoTask);
            LogChange(task.TaskGuid.ToString());
            LogChange(date.ToBinary());
            task.UndoTask(date);
        }

        internal void AddNoteToTask(CalendarRecurringTask task, DateTime date, string text)
        {
            LogChange((int)ChangeEvent.AddNotesToTask);
            LogChange(task.TaskGuid.ToString());
            LogChange(date.ToBinary());
            LogChange(text);
            task.SetNote(date, text);
        }

        internal void LogUpdateCustomColors()
        {
            LogChange((int)ChangeEvent.ModifyCustomColors);
            LogChange(colorPicker.CustomColors.Count());
            foreach (int i in colorPicker.CustomColors)
            {
                LogChange(i);
            }
        }

        internal void CloseFilestream()
        {
            changeLog.Dispose();
        }

        internal void UpdateFormHeight(int newFormHeight)
        {
            LogChange((int)ChangeEvent.ChangeFormHeight);
            LogChange(newFormHeight);
        }

        internal void UpdateFormWidth(int newFormWidth)
        {
            LogChange((int)ChangeEvent.ChangeFormWidth);
            LogChange(newFormWidth);
        }

        internal void UpdateSplitterPosition(int newSplitterPosition)
        {
            LogChange((int)ChangeEvent.ChangeSplitterPosition);
            LogChange(newSplitterPosition);
        }

        private void LogChange(int item)
        {
            if (!isWorkingCopy & !isParsingChangelog)
                changeLogWriter.Write(item);
        }
        private void LogChange(long item)
        {
            if (!isWorkingCopy & !isParsingChangelog)
                changeLogWriter.Write(item);
        }
        private void LogChange(string item)
        {
            if (!isWorkingCopy & !isParsingChangelog)
                changeLogWriter.Write(item);
        }
    }
}