using Aspie_Planner.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Aspie_Planner
{
    public partial class TaskOverview : Form
    {
        const int displayDays = 50;
        const int taskFontSize = 10, noteFontSize = 8;
        const int iconSize = 16;
        const int gridHeight = 20;
        const int gridWidth = 40;
        const int maxRecentNotes = 10;
        const int dateDifUpdateThreshold = 15;
        const string dateFormat = "ddd dd.MM";
        Font taskFont, noteFont;
        CalendarContent calendarContent;
        //CalendarRecurringTask viewingTask;
        DateTime viewRelativeDate;
        Bitmap iconRed, iconGreen, iconYellow, iconBlank, iconExclamation;
        Point lastCellCoordinates;

        private Rectangle dragBoxFromMouseDown;
        private int rowIndexFromMouseDown;
        private int rowIndexOfItemUnderMouseToDrop;

        // Initialization of main form, run on startup
        public TaskOverview()
        {
            InitializeComponent();
            // Load content file
            calendarContent = new CalendarContent("aspieplanner.data");
            try
            {
                Height = calendarContent.PreferenceFormHeight;
                Width = calendarContent.PreferenceFormWidth;
                splitContainer1.SplitterDistance = calendarContent.PreferenceSplitterPosition;
            }
            catch
            {
                Height = 480;
                Width = 640;
                splitContainer1.SplitterDistance = 200;
            }
            // Set display dates - viewingDate is date of presently selected cell. viewRelativeDate is date of first column
            viewRelativeDate = DateTime.Now.Date;
            // Load icons
            iconRed = LoadIcon(@"customIcons\red.png", Properties.Resources.red);
            iconGreen = LoadIcon(@"customIcons\green.png", Properties.Resources.green);
            iconYellow = LoadIcon(@"customIcons\yellow.png", Properties.Resources.yellow);
            iconExclamation = LoadIcon(@"customIcons\exclamation.png", Properties.Resources.exclamation_mark);
            iconBlank = LoadIcon(@"customIcons\blank.png", Properties.Resources.blank);
            // Bind event handlers
            taskListGrid.CellDoubleClick += TaskListGrid_CellDoubleClick;
            taskListGrid.MouseClick += TaskListGrid_MouseClick;
            taskListGrid.MouseDown += TaskListGrid_MouseDown;
            taskListGrid.MouseMove += TaskListGrid_MouseMove;
            taskListGrid.DragOver += TaskListGrid_DragOver;
            taskListGrid.DragDrop += TaskListGrid_DragDrop;
            taskListGrid.CurrentCellChanged += TaskListGrid_CurrentCellChanged;
            eventHistory.CellDoubleClick += EventHistory_CellDoubleClick;
            monthCalendar1.MouseWheel += MonthCalendar_MouseWheel;
            Microsoft.Win32.SystemEvents.SessionEnding += SystemEvents_SessionEnding;
            FormClosing += TaskOverview_FormClosing;
            splitContainer1.SplitterMoved += SplitContainer1_SplitterMoved;
            SizeChanged += TaskOverview_SizeChanged;
            // Set font
            taskFont = new Font(Font.FontFamily, taskFontSize);
            noteFont = new Font(Font.FontFamily, noteFontSize);
            // 
            RepopulateViews();
        }

        private void TaskOverview_SizeChanged(object sender, EventArgs e)
        {
            calendarContent.UpdateFormHeight(Height);
            calendarContent.UpdateFormWidth(Width);
        }

        private void SplitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            calendarContent.UpdateSplitterPosition(splitContainer1.SplitterDistance);
        }

        // Program shutdown event handler
        private void TaskOverview_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveNotes();
            calendarContent.CloseFilestream();
        }

        // System shutdown event handler
        private void SystemEvents_SessionEnding(object sender, Microsoft.Win32.SessionEndingEventArgs e)
        {
            calendarContent.CloseFilestream();
        }

        // Populate data in main form
        public void RepopulateViews()
        {
            taskListGrid.Columns.Clear();
            taskListGrid.RowHeadersWidth = 210;
            taskNotes.Text = null;
            //recentNotes.Text = null;
            DataGridViewRow dgvRow;
            DataGridViewImageColumn dgvImgCol;
            DataGridViewImageCell dgvImgCell;
            for (int i = 0; i < displayDays; i++)
            {
                dgvImgCol = new DataGridViewImageColumn();
                dgvImgCol.Tag = viewRelativeDate.AddDays(i);
                dgvImgCol.HeaderText = viewRelativeDate.AddDays(i).ToString(dateFormat);
                dgvImgCol.DefaultCellStyle.NullValue = null;
                dgvImgCol.Width = gridWidth;
                dgvImgCol.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                dgvImgCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                if (viewRelativeDate.AddDays(i) == DateTime.Now.Date)
                {
                    dgvImgCol.DefaultCellStyle.BackColor = Color.Lavender;
                    dgvImgCol.HeaderCell.Style.BackColor = Color.Lavender;
                }
                else
                    dgvImgCol.DefaultCellStyle.BackColor = Color.White;
                taskListGrid.Columns.Add(dgvImgCol);
            }
            dgvRow = (DataGridViewRow)taskListGrid.RowTemplate.Clone();
            dgvRow.CreateCells(taskListGrid);
            dgvRow.HeaderCell.Value = "Noter";
            dgvRow.HeaderCell.Style.Font = taskFont;
            for (int i = 0; i < displayDays; i++)
            {
                if (calendarContent.GetDaysNotes(viewRelativeDate.AddDays(i)) != null)
                {
                    dgvImgCell = new DataGridViewImageCell();
                    dgvImgCell.Value = iconExclamation;
                    dgvImgCell.ToolTipText = "Note";
                    dgvRow.Cells[i] = dgvImgCell;
                }
            }
            dgvRow.Height = gridHeight;
            taskListGrid.Rows.Add(dgvRow);
            foreach (CalendarRecurringTask rt in calendarContent.GetRecurringTasks())
            {
                dgvRow = (DataGridViewRow)taskListGrid.RowTemplate.Clone();
                dgvRow.CreateCells(taskListGrid);
                dgvRow.DefaultCellStyle.BackColor = rt.BackColor;
                dgvRow.DefaultCellStyle.ForeColor = rt.TextColor;
                dgvRow.Tag = rt;
                dgvRow.HeaderCell.Value = rt.TaskDescription;
                dgvRow.HeaderCell.Style.BackColor = rt.BackColor;
                dgvRow.HeaderCell.Style.ForeColor = rt.TextColor;
                dgvRow.HeaderCell.Style.Font = taskFont;

                for (int i = 0; i < displayDays; i++)
                {
                    dgvRow.Cells[i] = TaskCell(rt, viewRelativeDate.AddDays(i));
                    int colorR, colorB, colorG;
                    colorR = (dgvRow.DefaultCellStyle.BackColor.R * taskListGrid.Columns[i].DefaultCellStyle.BackColor.R) / 255;
                    colorG = (dgvRow.DefaultCellStyle.BackColor.G * taskListGrid.Columns[i].DefaultCellStyle.BackColor.G) / 255;
                    colorB = (dgvRow.DefaultCellStyle.BackColor.B * taskListGrid.Columns[i].DefaultCellStyle.BackColor.B) / 255;
                    Color cellColor = Color.FromArgb(colorR, colorG, colorB);
                    dgvRow.Cells[i].Style.BackColor = cellColor;
                }
                dgvRow.Height = gridHeight;
                taskListGrid.Rows.Add(dgvRow);
            }
            monthCalendar1.BoldedDates = calendarContent.GetAllDates().ToArray();
        }

        // Populate history
        private void PopulateEventHistory()
        {
            if (taskListGrid.CurrentCellAddress.Y != 0)
            {
                try
                {
                    CalendarRecurringTask viewingTask = (CalendarRecurringTask)taskListGrid.Rows[taskListGrid.CurrentCell.RowIndex].Tag;
                    List<DateTime> datesOfInterest = viewingTask.GetLastDatesOfInterest((DateTime)taskListGrid.Columns[taskListGrid.CurrentCellAddress.X].Tag, 50);
                    eventHistory.Columns.Clear();
                    eventHistory.RowHeadersWidth = 120;
                    eventHistory.RowHeadersDefaultCellStyle.Font = noteFont;
                    eventHistory.ColumnHeadersDefaultCellStyle.Font = noteFont;
                    eventHistory.DefaultCellStyle.Font = noteFont;
                    eventHistory.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
                    DataGridViewRow dgvRow;
                    DataGridViewColumn dgvCol;
                    DataGridViewImageColumn dgvImgCol;
                    dgvImgCol = new DataGridViewImageColumn();
                    dgvImgCol.HeaderText = "Status";
                    dgvImgCol.DefaultCellStyle.NullValue = null;
                    dgvImgCol.Width = 50;
                    dgvImgCol.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                    dgvImgCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    eventHistory.Columns.Add(dgvImgCol);
                    dgvCol = new DataGridViewColumn();
                    dgvCol.CellTemplate = new DataGridViewTextBoxCell();
                    dgvCol.HeaderText = viewingTask.TaskDescription;
                    dgvCol.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                    dgvCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    eventHistory.Columns.Add(dgvCol);
                    foreach (DateTime dateOfInterest in datesOfInterest)
                    {
                        dgvRow = (DataGridViewRow)eventHistory.RowTemplate.Clone();
                        dgvRow.CreateCells(eventHistory);
                        dgvRow.HeaderCell.Value = dateOfInterest.ToString(dateFormat);
                        dgvRow.Cells[0] = TaskCell(viewingTask, dateOfInterest);
                        dgvRow.Cells[1].Value = viewingTask.GetNote(dateOfInterest);
                        dgvRow.Height = 20;
                        dgvRow.Tag = dateOfInterest;
                        eventHistory.Rows.Add(dgvRow);
                    }
                    eventHistory.CurrentCell.Selected = false;
                }
                catch { }
            }
            else
            {
                try
                {
                    List<CalendarDateNote> dateNotes = calendarContent.GetDaysNotes((DateTime)taskListGrid.Columns[taskListGrid.CurrentCellAddress.X].Tag, 50);
                    eventHistory.Columns.Clear();
                    eventHistory.RowHeadersWidth = 120;
                    eventHistory.RowHeadersDefaultCellStyle.Font = noteFont;
                    eventHistory.ColumnHeadersDefaultCellStyle.Font = noteFont;
                    eventHistory.DefaultCellStyle.Font = noteFont;
                    eventHistory.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
                    DataGridViewRow dgvRow;
                    DataGridViewColumn dgvCol;
                    DataGridViewImageColumn dgvImgCol;
                    dgvImgCol = new DataGridViewImageColumn();
                    dgvImgCol.HeaderText = "Status";
                    dgvImgCol.DefaultCellStyle.NullValue = null;
                    dgvImgCol.Width = 50;
                    dgvImgCol.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                    dgvImgCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    eventHistory.Columns.Add(dgvImgCol);
                    dgvCol = new DataGridViewColumn();
                    dgvCol.CellTemplate = new DataGridViewTextBoxCell();
                    dgvCol.HeaderText = "Noter";
                    dgvCol.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                    dgvCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    eventHistory.Columns.Add(dgvCol);
                    {
                        foreach (CalendarDateNote dateNote in dateNotes)
                        {
                            dgvRow = (DataGridViewRow)eventHistory.RowTemplate.Clone();
                            dgvRow.CreateCells(eventHistory);
                            dgvRow.HeaderCell.Value = dateNote.GetDate().ToString(dateFormat);
                            DataGridViewImageCell dgvImgCell = new DataGridViewImageCell();
                            dgvImgCell.Value = iconExclamation;
                            dgvRow.Cells[0] = dgvImgCell;
                            dgvRow.Cells[1].Value = dateNote.GetNote();
                            dgvRow.Height = 20;
                            dgvRow.Tag = dateNote.GetDate();
                            eventHistory.Rows.Add(dgvRow);
                        }
                    }
                }
                catch { }
            }
        }

        // Load icons - custom or fallback to default resources
        private Bitmap LoadIcon(string customIconFilepath, Bitmap fallback)
        {
            Bitmap result;
            try
            {
                result = ResizeImage(Image.FromFile(customIconFilepath), iconSize, iconSize);
            }
            catch (FileNotFoundException)
            {
                result = ResizeImage(fallback, iconSize, iconSize);
            }
            return result;
        }

        // Resizing icons
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        // Set state for a cell
        private DataGridViewImageCell TaskCell(CalendarRecurringTask task, DateTime date)
        {
            DataGridViewImageCell result = new DataGridViewImageCell();
            Bitmap resultIcon = new Bitmap(iconSize, iconSize, PixelFormat.Format32bppArgb);
            var graphics = Graphics.FromImage(resultIcon);
            graphics.CompositingMode = CompositingMode.SourceOver;
            CalendarRecurringTask.TaskStatus status = task.GetStatus(date);
            switch (status)
            {
                case CalendarRecurringTask.TaskStatus.Complete:
                    graphics.DrawImage(iconGreen, 0, 0);
                    result.ToolTipText = "Færdig";
                    break;
                case CalendarRecurringTask.TaskStatus.Incomplete:
                    graphics.DrawImage(iconYellow, 0, 0);
                    result.ToolTipText = "Bør gøres";
                    break;
                case CalendarRecurringTask.TaskStatus.Critical:
                    graphics.DrawImage(iconRed, 0, 0);
                    result.ToolTipText = "Vigtig!";
                    break;
                default:
                    graphics.DrawImage(iconBlank, 0, 0);
                    break;
            }
            string note = task.GetNote(date);
            if (!string.IsNullOrEmpty(note))
                graphics.DrawImage(iconExclamation, 0, 0);
            result.Value = resultIcon;
            return result;
        }

        // Handlers for buttons
        // Jump to todays date
        private void btToday_Click(object sender, EventArgs e)
        {
            Point activeCell = taskListGrid.CurrentCellAddress;
            monthCalendar1.SetDate(DateTime.Today);
            taskListGrid.CurrentCell = taskListGrid[0, activeCell.Y];
        }

        // Save edits in notes
        private void SaveNotes_Click(object sender, EventArgs e)
        {
            Point activeCell = taskListGrid.CurrentCellAddress;
            DateTime viewingDate = (DateTime)taskListGrid.Columns[activeCell.X].Tag;
            calendarContent.ChangeDayNotes(viewingDate, todaysNotes.Text);
            CalendarRecurringTask viewingTask = (CalendarRecurringTask)taskListGrid.Rows[taskListGrid.CurrentCell.RowIndex].Tag;
            if (viewingTask != null)
            {
                calendarContent.AddNoteToTask(viewingTask, viewingDate, taskNotes.Text);
            }
            RepopulateViews();
            taskListGrid.CurrentCell = taskListGrid[activeCell.X, activeCell.Y];
        }

        // Save edits automatically
        private void SaveNotes()
        {
            if (taskNotes.Modified)
            {
                DateTime lastDate = (DateTime)taskListGrid.Columns[lastCellCoordinates.X].Tag;
                CalendarRecurringTask lastTask = (CalendarRecurringTask)taskListGrid.Rows[lastCellCoordinates.Y].Tag;
                calendarContent.AddNoteToTask(lastTask, lastDate, taskNotes.Text);
                taskListGrid[lastCellCoordinates.X, lastCellCoordinates.Y] = TaskCell(lastTask, lastDate);
            }

            if (todaysNotes.Modified)
            {
                DateTime lastDate = (DateTime)taskListGrid.Columns[lastCellCoordinates.X].Tag;
                calendarContent.ChangeDayNotes(lastDate, todaysNotes.Text);
                if (!String.IsNullOrEmpty(todaysNotes.Text))
                    ((DataGridViewImageCell)taskListGrid[lastCellCoordinates.X, 0]).Value = iconExclamation;
                else
                    ((DataGridViewImageCell)taskListGrid[lastCellCoordinates.X, 0]).Value = null;
            }
        }

        // Create new task
        private void NewRecurringTask_Click(object sender, EventArgs e)
        {
            RecurringTaskForm recurringForm = new RecurringTaskForm(calendarContent, this);
            recurringForm.Show();
        }

        // Event handlers
        // Main form
        // Drag-drop: On completed drag-drop action, reorder calendar contents
        private void TaskListGrid_DragDrop(object sender, DragEventArgs e)
        {
            Point clientPoint = taskListGrid.PointToClient(new Point(e.X, e.Y));

            rowIndexOfItemUnderMouseToDrop =
                taskListGrid.HitTest(clientPoint.X, clientPoint.Y).RowIndex;

            if (e.Effect == DragDropEffects.Move)
            {
                DataGridViewRow rowToMove = e.Data.GetData(typeof(DataGridViewRow)) as DataGridViewRow;
                calendarContent.AssignTaskPosition((CalendarRecurringTask)rowToMove.Tag, rowIndexOfItemUnderMouseToDrop - 1);
                RepopulateViews();
            }
        }

        // Drag-drop: Enable drag-drop reordering
        private void TaskListGrid_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        // Drag-drop: Defines distance to move mouse after mousepress to initiate drag-drop behaviour
        private void TaskListGrid_MouseDown(object sender, MouseEventArgs e)
        {
            int columnIndexFromMouse = taskListGrid.HitTest(e.X, e.Y).ColumnIndex;
            rowIndexFromMouseDown = taskListGrid.HitTest(e.X, e.Y).RowIndex;
            if (rowIndexFromMouseDown != -1 && columnIndexFromMouse == -1)
            {
                Size dragSize = SystemInformation.DragSize;
                dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2),
                                                               e.Y - (dragSize.Height / 2)),
                                                     dragSize);
            }
            else
                dragBoxFromMouseDown = Rectangle.Empty;
        }

        // Drag-drop: Track mouse movement with mouse button pressed and initiate dragdrop if minimum distance moved
        private void TaskListGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                if (dragBoxFromMouseDown != Rectangle.Empty &&
                    !dragBoxFromMouseDown.Contains(e.X, e.Y))
                {
                    DragDropEffects dropEffect = taskListGrid.DoDragDrop(taskListGrid.Rows[rowIndexFromMouseDown],
                                                                         DragDropEffects.Move);
                }
            }
        }

        // Populate todays notes and tasks notes based on active cell
        private void TaskListGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            SaveNotes();
            taskNotes.Text = null;
            //recentNotes.Text = null;
            if (taskListGrid.CurrentCell != null)
            {
                if (taskListGrid.CurrentCell.ColumnIndex > -1)
                {
                    DateTime viewingDate = (DateTime)taskListGrid.Columns[taskListGrid.CurrentCell.ColumnIndex].Tag;
                    CalendarDateNote clickedDaysEvent = calendarContent.GetDaysNotes(viewingDate);
                    if (clickedDaysEvent != null)
                        todaysNotes.Text = clickedDaysEvent.GetNote();
                    else
                        todaysNotes.Text = null;
                    if (taskListGrid.CurrentCell.RowIndex > 0)
                    {
                        CalendarRecurringTask viewingTask = (CalendarRecurringTask)taskListGrid.Rows[taskListGrid.CurrentCell.RowIndex].Tag;
                        taskNotes.Enabled = true;
                        string taskNotesToday = viewingTask.GetNote(viewingDate);
                        if (!string.IsNullOrEmpty(taskNotesToday))
                            taskNotes.Text = taskNotesToday;
                        string taskNotesRecent = viewingTask.GetLastXNotes(viewingDate, maxRecentNotes);
                        /*if (!string.IsNullOrEmpty(taskNotesRecent))
                            recentNotes.Text = taskNotesRecent;*/
                    }
                    else
                    {
                        //viewingTask = null;
                        taskNotes.Enabled = false;
                    }
                }
                lastCellCoordinates = new Point(taskListGrid.CurrentCell.ColumnIndex, taskListGrid.CurrentCell.RowIndex);
            }
            else
            {
                //viewingTask = null;
                taskNotes.Enabled = false;
            }
            PopulateEventHistory();
        }

        // Handling grid doubleclick (quick done/undone)
        private void TaskListGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            CalendarRecurringTask task;
            DateTime taskDate;
            if (e.RowIndex > 0) // Double click in todays notes row has no effect
            {
                task = (CalendarRecurringTask)taskListGrid.Rows[e.RowIndex].Tag;
                if (e.ColumnIndex == -1) // Double click in description column opens task edit form
                {
                    RecurringTaskForm recurringForm = new RecurringTaskForm(calendarContent, this, task.TaskGuid.ToString());
                    recurringForm.Show();
                }
                else
                {
                    taskDate = (DateTime)taskListGrid.Columns[e.ColumnIndex].Tag;
                    // Do/undo task
                    switch (task.GetStatus(taskDate))
                    {
                        case CalendarRecurringTask.TaskStatus.Complete:
                            calendarContent.UndoTask(task, taskDate);
                            RepopulateViews();
                            break;
                        case CalendarRecurringTask.TaskStatus.Critical:
                        case CalendarRecurringTask.TaskStatus.Incomplete:
                            calendarContent.DoTask(task, taskDate);
                            RepopulateViews();
                            break;
                        default:
                            if (task.TaskReccuranceType == CalendarRecurringTask.ReccuranceType.XToYDays)
                            {
                                calendarContent.DoTask(task, taskDate);
                                RepopulateViews();
                            }
                            break;
                    }
                }
            }
            if (e.ColumnIndex >= 0)
                taskListGrid.CurrentCell = taskListGrid[e.ColumnIndex, e.RowIndex];
        }

        // Creating context menus in grid
        private void TaskListGrid_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ContextMenu cm = new ContextMenu();
                int currentMouseOverRow = taskListGrid.HitTest(e.X, e.Y).RowIndex;
                int currentMouseOverColumn = taskListGrid.HitTest(e.X, e.Y).ColumnIndex;
                if (currentMouseOverRow >= 1)
                {
                    CalendarRecurringTask task = (CalendarRecurringTask)taskListGrid.Rows[currentMouseOverRow].Tag;
                    if (currentMouseOverColumn >= 0)
                    {
                        TaskDay taskDay = new TaskDay(task,
                                              (DateTime)taskListGrid.Columns[currentMouseOverColumn].Tag);
                        if ((taskDay.GetStatus() != CalendarRecurringTask.TaskStatus.Complete &&
                            taskDay.task.TaskReccuranceType == CalendarRecurringTask.ReccuranceType.XToYDays) ||
                            taskDay.GetStatus() == CalendarRecurringTask.TaskStatus.Incomplete ||
                            taskDay.GetStatus() == CalendarRecurringTask.TaskStatus.Critical)
                        {
                            MenuItem menuSetDone = new MenuItem("Markér gjort", OnDoneClick);
                            menuSetDone.Tag = taskDay;
                            cm.MenuItems.Add(menuSetDone);
                        }
                        else if (taskDay.GetStatus() == CalendarRecurringTask.TaskStatus.Complete)
                        {
                            MenuItem menuSetDone = new MenuItem("Fortryd gjort", OnUndoClick);
                            menuSetDone.Tag = taskDay;
                            cm.MenuItems.Add(menuSetDone);
                        }
                        if (task.GetLastNoteDate((DateTime)taskListGrid.Columns[currentMouseOverColumn].Tag) != DateTime.MinValue)
                        {
                            MenuItem menuLastNote = new MenuItem("Seneste Note", OnLastNoteClick);
                            menuLastNote.Tag = new Point(currentMouseOverColumn, currentMouseOverRow);
                            cm.MenuItems.Add(menuLastNote);
                        }
                    }
                    else
                    {
                        MenuItem menuDelete = new MenuItem("Slet", OnDeleteClick);
                        menuDelete.Tag = task.TaskGuid;
                        cm.MenuItems.Add(menuDelete);
                    }
                    cm.Show(taskListGrid, new Point(e.X, e.Y));
                }
            }
        }

        // Context menu option handling
        // Processing "Undo" click
        private void OnUndoClick(object sender, EventArgs e)
        {
            ((TaskDay)((MenuItem)sender).Tag).SetUndone();
            RepopulateViews();
        }

        private void nyOpgaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RecurringTaskForm recurringForm = new RecurringTaskForm(calendarContent, this);
            recurringForm.Show();
        }

        private void foreslåDagsprogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WeekPlanner weekPlanner = new WeekPlanner(calendarContent);
            weekPlanner.Show();
        }

        private void ugeplanlægningToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        // Processing "Done" click
        private void OnDoneClick(object sender, EventArgs e)
        {
            ((TaskDay)((MenuItem)sender).Tag).SetDone();
            RepopulateViews();
        }

        // Processing "Delete" click
        private void OnDeleteClick(object sender, EventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            calendarContent.DeleteTask(mi.Tag.ToString());
            RepopulateViews();
        }

        // Processing context menu "Most recent note" click
        private void OnLastNoteClick(object sender, EventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            Point senderCellAddress = (Point)mi.Tag;
            monthCalendar1.SetDate(((CalendarRecurringTask)taskListGrid.Rows[senderCellAddress.Y].Tag).GetLastNoteDate((DateTime)taskListGrid.Columns[senderCellAddress.X].Tag));
            taskListGrid.CurrentCell = taskListGrid[0, senderCellAddress.Y];
        }

        // Event history
        // Jump to date when doubleclicking history entry
        private void EventHistory_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            Point activeCell = taskListGrid.CurrentCellAddress;
            monthCalendar1.SetDate((DateTime)eventHistory.Rows[e.RowIndex].Tag);
            taskListGrid.CurrentCell = taskListGrid[0, activeCell.Y];
        }

        // Date picker
        // Ignore mouse wheel scrolling in calendar
        private void MonthCalendar_MouseWheel(object sender, MouseEventArgs e)
        {
            ((HandledMouseEventArgs)e).Handled = true;
        }

        // Update in response to date changed via calendar
        private void MonthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            // Redraw main form if date moved to before viewRelativeDate, or sufficiently ahead of:
            if (viewRelativeDate > e.Start.Date || viewRelativeDate < e.Start.Date.AddDays(dateDifUpdateThreshold))
            {
                int rowIdx = taskListGrid.CurrentCell.RowIndex;
                viewRelativeDate = e.Start.Date;
                RepopulateViews();
                taskListGrid.CurrentCell = taskListGrid[0, rowIdx];
            }
            taskListGrid.Focus();
        }
    }
}
