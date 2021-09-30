using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aspie_Planner
{
    public class CalendarContent
    {
        // Saved content
        public int JsonVersion { get; set; } = 1;
        public List<CalendarDateNote> calendarDateNotes { get; set; }
        public List<CalendarRecurringTask> calendarRecurringTasks { get; set; }
        public int PreferenceFormHeight { get; set; }
        public int PreferenceFormWidth { get; set; }
        public int PreferenceSplitterPosition { get; set; }
        public List<KeyValuePair<DayOfWeek, TimeSpan>> PreferenceDayStart { get; set; }
        public List<KeyValuePair<DayOfWeek, TimeSpan>> PreferenceDayEnd { get; set; }
        public ColorDialog colorPicker { get; set; }
        public string FirstChangelogItem { get; set; }
        
        // Internal items
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
        private FileStream changeLog;
        private Boolean isParsingChangelog;
        //private Boolean isWorkingCopy = false;
        private string NextChangelogItem;
        private JObject logBuffer;

        public CalendarContent()
        {
            string filename = "aspieplannerdata.json";
            try
            {
                bool UseJsonDataFile = File.Exists(filename);
                if (UseJsonDataFile)
                {
                    Directory.CreateDirectory("backups");
                    string backupFilename = @".\backups\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
                    File.Copy(filename, backupFilename, true);
                    string fileContent = File.ReadAllText(filename);
                    InitFromText(fileContent);
                }
                else
                {
                    filename = "aspieplanner.data";
                    Directory.CreateDirectory("backups");
                    string backupFilename = @".\backups\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".data";
                    File.Copy(filename, backupFilename, true);
                    using (var stream = File.OpenRead(filename))
                    {
                        InitFromStream(stream);
                    }
                }
            }
            catch (Exception e)
            {
                NewSetup();
                PrepareChangelogging();
                SaveCalendar();
            }
        }

        private CalendarContent(bool isCloning)
        {
            NewSetup();
            if (!isCloning)
                PrepareChangelogging();
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
        }

        private void InitFromText(string content)
        {
            JObject jObject = JObject.Parse(content);
            calendarDateNotes = jObject.SelectToken("calendarDateNotes").ToObject<List<CalendarDateNote>>();
            calendarRecurringTasks = jObject.SelectToken("calendarRecurringTasks").ToObject<List<CalendarRecurringTask>>();
            PreferenceFormHeight = jObject.SelectToken("PreferenceFormHeight").ToObject<int>();
            PreferenceFormWidth = jObject.SelectToken("PreferenceFormWidth").ToObject<int>();
            PreferenceSplitterPosition = jObject.SelectToken("PreferenceSplitterPosition").ToObject<int>();
            PreferenceDayStart = jObject.SelectToken("PreferenceDayStart").ToObject<List<KeyValuePair<DayOfWeek, TimeSpan>>>();
            PreferenceDayEnd = jObject.SelectToken("PreferenceDayEnd").ToObject<List<KeyValuePair<DayOfWeek, TimeSpan>>>();
            colorPicker = jObject.SelectToken("colorPicker").ToObject<ColorDialog>();
            FirstChangelogItem = jObject.SelectToken("FirstChangelogItem").ToObject<string>();
            ProcessJsonChangeLog();
            SaveCalendar();
        }

        private T SelectTokenSafe<T>(JObject jObject, string TokenName)
        {
            try
            {
                T result = jObject.SelectToken(TokenName).ToObject<T>();
                return result;
            }
            catch
            {
                return default(T);
            }
        }

        private Color SafeColor(Color color)
        {
            Color result = Color.FromArgb(255, color.R, color.G, color.B);
            return result;
        }

        private void ProcessJsonChangeLog()
        {
            bool Done = false;
            isParsingChangelog = true;
            string filename = FirstChangelogItem + ".json";
            while (!Done)
            {
                try
                {
                    string fileContent = File.ReadAllText(@".\changelog\" + filename);
                    JObject jObject = JObject.Parse(fileContent);
                    ChangeEvent eventType = (ChangeEvent)jObject.SelectToken("EventType").Value<int>();
                    CalendarRecurringTask.ReccuranceType recurranceType;
                    CalendarRecurringTask.TimeType timeType;
                    List<string> weekdays;
                    int dayRangeX, dayRangeY;
                    TimeSpan timeLower, timeUpper, duration;
                    string description;
                    Color textColor, backColor;
                    DateTime dateTime;
                    string taskGuid; 
                    switch (eventType)
                    {
                        case ChangeEvent.TaskReorder:
                            CalendarRecurringTask reorderTask = GetTask(jObject.SelectToken("Guid").Value<string>());
                            int position = jObject.SelectToken("Position").Value<int>();
                            AssignTaskPosition(reorderTask, position);
                            break;
                        case ChangeEvent.AddTask:
                            recurranceType = (CalendarRecurringTask.ReccuranceType)jObject.SelectToken("RecurranceType").Value<int>();
                            timeType = (CalendarRecurringTask.TimeType)jObject.SelectToken("TimeType").Value<int>();
                            weekdays = SelectTokenSafe<List<string>>(jObject, "Weekdays");
                            dayRangeX = SelectTokenSafe<int>(jObject, "DayRangeLower");
                            dayRangeY = SelectTokenSafe<int>(jObject, "DayRangeUpper");
                            timeLower = new TimeSpan(0, SelectTokenSafe<int>(jObject, "TimeLower"), 0);
                            timeUpper = new TimeSpan(0, SelectTokenSafe<int>(jObject, "TimeUpper"), 0);
                            duration = new TimeSpan(0, SelectTokenSafe<int>(jObject, "Duration"), 0);
                            description = SelectTokenSafe<string>(jObject, "Description");
                            textColor = SafeColor(Color.FromArgb(SelectTokenSafe<int>(jObject, "TextColor")));
                            backColor = SafeColor(Color.FromArgb(SelectTokenSafe<int>(jObject, "BackColor")));
                            dateTime = SelectTokenSafe<DateTime>(jObject, "OffsetDate");
                            taskGuid = jObject.SelectToken("Guid").Value<string>();
                            calendarRecurringTasks.Add(new CalendarRecurringTask(recurranceType, timeType, weekdays, dayRangeX, dayRangeY,
                                timeLower, timeUpper, duration, description, textColor, backColor, dateTime, taskGuid));
                            break;
                        case ChangeEvent.ModifyTask:
                            recurranceType = (CalendarRecurringTask.ReccuranceType)jObject.SelectToken("RecurranceType").Value<int>();
                            timeType = (CalendarRecurringTask.TimeType)jObject.SelectToken("TimeType").Value<int>();
                            weekdays = SelectTokenSafe<List<string>>(jObject, "Weekdays");
                            dayRangeX = SelectTokenSafe<int>(jObject, "DayRangeLower");
                            dayRangeY = SelectTokenSafe<int>(jObject, "DayRangeUpper");
                            timeLower = new TimeSpan(0, SelectTokenSafe<int>(jObject, "TimeLower"), 0);
                            timeUpper = new TimeSpan(0, SelectTokenSafe<int>(jObject, "TimeUpper"), 0);
                            duration = new TimeSpan(0, SelectTokenSafe<int>(jObject, "Duration"), 0);
                            description = SelectTokenSafe<string>(jObject, "Description");
                            textColor = SafeColor(Color.FromArgb(SelectTokenSafe<int>(jObject, "TextColor")));
                            backColor = SafeColor(Color.FromArgb(SelectTokenSafe<int>(jObject, "BackColor")));
                            dateTime = SelectTokenSafe<DateTime>(jObject, "OffsetDate");
                            taskGuid = jObject.SelectToken("Guid").Value<string>();
                            CalendarRecurringTask loadedTask = GetTask(taskGuid);
                            loadedTask.Modify(recurranceType, timeType, weekdays, dayRangeX, dayRangeY, timeLower, timeUpper,
                                duration, description, textColor, backColor, dateTime);
                            break;
                        case ChangeEvent.DoTask:
                            CalendarRecurringTask toDoTask = GetTask(jObject.SelectToken("Guid").Value<string>());
                            toDoTask.DoTask(jObject.SelectToken("Date").Value<DateTime>());
                            break;
                        case ChangeEvent.UndoTask:
                            CalendarRecurringTask toUndoTask = GetTask(jObject.SelectToken("Guid").Value<string>());
                            toUndoTask.UndoTask(jObject.SelectToken("Date").Value<DateTime>());
                            break;
                        case ChangeEvent.AddNotesToDay:
                            dateTime = jObject.SelectToken("Date").Value<DateTime>();
                            description = jObject.SelectToken("Description").Value<string>();
                            ChangeDayNotes(dateTime, description);
                            break;
                        case ChangeEvent.AddNotesToTask:
                            CalendarRecurringTask toNoteTask = GetTask(jObject.SelectToken("Guid").Value<string>());
                            dateTime = jObject.SelectToken("Date").Value<DateTime>();
                            description = jObject.SelectToken("Text").Value<string>();
                            toNoteTask.SetNote(dateTime, description);
                            break;
                        case ChangeEvent.DeleteTask:
                            DeleteTask(jObject.SelectToken("Guid").Value<string>());
                            break;
                        case ChangeEvent.ModifyCustomColors:
                            colorPicker.CustomColors = jObject.SelectToken("Colors").Value<int[]>();
                            break;
                        case ChangeEvent.ChangeFormHeight:
                            PreferenceFormHeight = jObject.SelectToken("FormHeight").Value<int>();
                            break;
                        case ChangeEvent.ChangeFormWidth:
                            PreferenceFormWidth = jObject.SelectToken("FormWidth").Value<int>();
                            break;
                        case ChangeEvent.ChangeSplitterPosition:
                            PreferenceSplitterPosition = jObject.SelectToken("SplitterPosition").Value<int>();
                            break;
                    }
                    File.Move(@".\changelog\" + filename, @".\backups\" + filename);
                    filename = jObject.SelectToken("NextLogItem").Value<string>() + ".json";
                }
                catch
                {
                    Done = true;
                }
            }
            isParsingChangelog = false;
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
        private void PrepareChangelogging()
        {
            FirstChangelogItem = Guid.NewGuid().ToString();
            NextChangelogItem = FirstChangelogItem;
            Directory.CreateDirectory("changelog");
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

        // Write content file using current format (JSON 1)
        private void SaveCalendar()
        {
            PrepareChangelogging();
            File.WriteAllText("aspieplannerdata.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        // Content access methods
        // 
        internal void ChangeDayNotes(DateTime date, string description)
        {
            InitLogEntry(ChangeEvent.AddNotesToDay);
            AddLogDetail("Date", date);
            AddLogDetail("Description", description);
            CloseLogEntry();
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
            InitLogEntry(ChangeEvent.AddTask);
            AddLogDetail("RecurranceType", (int)newTask.TaskReccuranceType);
            AddLogDetail("TimeType", (int)newTask.TaskTimeType);
            AddLogDetail("Weekdays", newTask.Weekdays);
            AddLogDetail("DayRangeLower", newTask.DayRangeLower);
            AddLogDetail("DayRangeUpper", newTask.DayRangeUpper);
            AddLogDetail("TimeLower", (int)newTask.TimeParamLower.TotalMinutes);
            AddLogDetail("TimeUpper", (int)newTask.TimeParamUpper.TotalMinutes);
            AddLogDetail("Duration", (int)newTask.GetDuration().TotalMinutes);
            AddLogDetail("Description", newTask.TaskDescription);
            AddLogDetail("TextColor", newTask.TextColor.ToArgb());
            AddLogDetail("BackColor", newTask.BackColor.ToArgb());
            AddLogDetail("OffsetDate", newTask.OffsetDate);
            AddLogDetail("Guid", newTask.TaskGuid.ToString());
            CloseLogEntry();
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
            InitLogEntry(ChangeEvent.DeleteTask);
            AddLogDetail("Guid", guid);
            CloseLogEntry();
            CalendarRecurringTask taskToDelete = GetTask(guid);
            calendarRecurringTasks.Remove(taskToDelete);
        }

        internal void AssignTaskPosition(CalendarRecurringTask theTask, int position)
        {
            InitLogEntry(ChangeEvent.TaskReorder);
            AddLogDetail("Guid", theTask.TaskGuid.ToString());
            AddLogDetail("Position", position);
            CloseLogEntry();
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
            InitLogEntry(ChangeEvent.ModifyTask);
            AddLogDetail("RecurranceType", (int)reccuranceType);
            AddLogDetail("TimeType", (int)timeType);
            AddLogDetail("Weekdays", weekdays);
            AddLogDetail("DayRangeLower", dayRangeLower);
            AddLogDetail("DayRangeUpper", dayRangeUpper);
            AddLogDetail("TimeLower", (int)timeLower.TotalMinutes);
            AddLogDetail("TimeUpper", (int)timeUpper.TotalMinutes);
            AddLogDetail("Duration", (int)duration.TotalMinutes);
            AddLogDetail("Description", taskDescription);
            AddLogDetail("TextColor", textColor.ToArgb());
            AddLogDetail("BackColor", backColor.ToArgb());
            AddLogDetail("OffsetDate", offsetDate);
            AddLogDetail("Guid", theTask.TaskGuid.ToString());
            CloseLogEntry();
            theTask.Modify(reccuranceType, timeType, weekdays, dayRangeLower, dayRangeUpper, timeLower, timeUpper, duration,
                taskDescription, textColor, backColor, offsetDate);
        }

        internal void DoTask(CalendarRecurringTask task, DateTime date)
        {
            InitLogEntry(ChangeEvent.DoTask);
            AddLogDetail("Guid", task.TaskGuid.ToString());
            AddLogDetail("Date", date);
            CloseLogEntry();
            task.DoTask(date);
        }

        internal void UndoTask(CalendarRecurringTask task, DateTime date)
        {
            InitLogEntry(ChangeEvent.UndoTask);
            AddLogDetail("Guid", task.TaskGuid.ToString());
            AddLogDetail("Date", date);
            CloseLogEntry();
            task.UndoTask(date);
        }

        internal void AddNoteToTask(CalendarRecurringTask task, DateTime date, string text)
        {
            InitLogEntry(ChangeEvent.AddNotesToTask);
            AddLogDetail("Guid", task.TaskGuid.ToString());
            AddLogDetail("Date", date);
            AddLogDetail("Text", text);
            CloseLogEntry();
            task.SetNote(date, text);
        }

        internal void LogUpdateCustomColors()
        {
            InitLogEntry(ChangeEvent.ModifyCustomColors);
            AddLogDetail("CustomColors", colorPicker.CustomColors);
            CloseLogEntry();
        }

        internal void UpdateFormHeight(int newFormHeight)
        {
            InitLogEntry(ChangeEvent.ChangeFormHeight);
            AddLogDetail("FormHeight", newFormHeight);
            CloseLogEntry();
        }

        internal void UpdateFormWidth(int newFormWidth)
        {
            InitLogEntry(ChangeEvent.ChangeFormWidth);
            AddLogDetail("FormWidth", newFormWidth);
            CloseLogEntry();
        }

        internal void UpdateSplitterPosition(int newSplitterPosition)
        {
            InitLogEntry(ChangeEvent.ChangeSplitterPosition);
            AddLogDetail("SplitterPosition", newSplitterPosition);
            CloseLogEntry();
        }

        private void InitLogEntry(ChangeEvent logEventType)
        {
            if (!isParsingChangelog)
            {
                logBuffer = new JObject();
                logBuffer.Add(new JProperty("EventType", logEventType));
            }
        }

        private void AddLogDetail(string property, object value)
        {
            if (!isParsingChangelog)
                logBuffer.Add(new JProperty(property, value));
        }

        private void CloseLogEntry()
        {
            if (!isParsingChangelog)
            {
                string ThisChangelogItem = NextChangelogItem;
                NextChangelogItem = Guid.NewGuid().ToString();
                logBuffer.Add(new JProperty("NextLogItem", NextChangelogItem));
                File.WriteAllText(@".\changelog\" + ThisChangelogItem + ".json", JsonConvert.SerializeObject(logBuffer, Formatting.Indented));
            }
        }
    }
}