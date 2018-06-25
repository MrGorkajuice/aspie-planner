using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aspie_Planner.Forms
{
    public partial class WeekPlanner : Form
    {
        CalendarContent calendarContent;
        TimeSpan minTime, maxTime;
        WeekContent weekContent;
        public WeekPlanner(CalendarContent calendarContent)
        {
            this.calendarContent = calendarContent;
            minTime = calendarContent.PreferenceDayStart.OrderBy(x => x.Value).FirstOrDefault().Value;
            maxTime = calendarContent.PreferenceDayEnd.OrderByDescending(x => x.Value).FirstOrDefault().Value;
            InitializeComponent();
            SizeChanged += WeekPlanner_SizeChanged;
            //dgvWeekPlanner.SelectionChanged += DgvWeekPlanner_SelectionChanged;
            dgvWeekPlanner.CellPainting += DgvWeekPlanner_CellPainting;
            dgvWeekPlanner.CellFormatting += DgvWeekPlanner_CellFormatting;
            weekContent = new WeekContent(calendarContent);
            weekContent.Autocalculate();
            DrawDGVWeekPlanner();
        }

        private void DgvWeekPlanner_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex == 0)
                return;
            if (IsTheSameCellValue(e.ColumnIndex, e.RowIndex))
            {
                e.Value = "";
                e.FormattingApplied = true;
            }
        }

        bool IsTheSameCellValue(int column, int row)
        {
            DataGridViewCell cell1 = dgvWeekPlanner[column, row];
            DataGridViewCell cell2 = dgvWeekPlanner[column, row - 1];
            if (cell1.Value == null || cell2.Value == null)
            {
                return false;
            }
            return cell1.Value.ToString() == cell2.Value.ToString();
        }

        private void DgvWeekPlanner_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            e.AdvancedBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.None;
            if (e.RowIndex < 1 || e.ColumnIndex < 0)
                return;
            if (IsTheSameCellValue(e.ColumnIndex, e.RowIndex))
            {
                e.AdvancedBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;
            }
            else
            {
                e.AdvancedBorderStyle.Top = dgvWeekPlanner.AdvancedCellBorderStyle.Top;
            }
        }

        private void WeekPlanner_SizeChanged(object sender, EventArgs e)
        {
            DrawDGVWeekPlanner();
        }

        /*private void DgvWeekPlanner_SelectionChanged(object sender, EventArgs e)
        {
            dgvWeekPlanner.CurrentCell = null;
        }*/

        private void DrawDGVWeekPlanner()
        {
            dgvWeekPlanner.RowHeadersWidth = 80;
            dgvWeekPlanner.Columns.Clear();
            for (int i = 0; i < 7; i++)
            {
                DataGridViewColumn dgvCol = new DataGridViewColumn();
                dgvCol.HeaderText = DateTime.Today.AddDays(i).ToString("dd.MM.yyyy");
                
                dgvCol.Width = (dgvWeekPlanner.Width - 95) / 7;
                dgvCol.CellTemplate = new DataGridViewTextBoxCell();
                dgvWeekPlanner.Columns.Add(dgvCol);
            }

            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    TimeSpan thisTime = new TimeSpan(i, j * 15, 0);
                    if (thisTime >= minTime && thisTime < maxTime)
                    {
                        DataGridViewRow dgvRow = (DataGridViewRow)dgvWeekPlanner.RowTemplate.Clone();
                        dgvRow.CreateCells(dgvWeekPlanner);
                        dgvRow.HeaderCell.Value = thisTime.ToString("hh':'mm");
                        dgvRow.HeaderCell.Style.ForeColor = Color.LightGray;
                        dgvRow.Tag = thisTime;
                        dgvWeekPlanner.Rows.Add(dgvRow);
                    }
                }
            }

            for (int i = 0; i < 7; i++)
            {
                List<KeyValuePair<CalendarRecurringTask, TimeSpan>> daysTasks = weekContent.Data.Find(x => x.Key == DateTime.Today.AddDays(i)).Value;
                foreach (DataGridViewRow row in dgvWeekPlanner.Rows)
                {
                    try
                    {
                        CalendarRecurringTask drawTask = daysTasks.Find(x => x.Value < ((TimeSpan)row.Tag).Add(new TimeSpan(0, 15, 0)) & x.Value + 
                            x.Key.GetDuration() > (TimeSpan)row.Tag).Key;
                        if (drawTask != null)
                        {
                            dgvWeekPlanner[i, row.Index].Value = drawTask.TaskDescription;
                            dgvWeekPlanner[i, row.Index].Style.BackColor = drawTask.BackColor;
                            dgvWeekPlanner[i, row.Index].Style.ForeColor = drawTask.TextColor;
                        }
                    }
                    catch(Exception e)
                    {
                        string tmp = e.Message;
                    }
                }
            }
        }
    }
}
