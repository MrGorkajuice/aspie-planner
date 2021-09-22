namespace Aspie_Planner
{
    partial class RecurringTaskForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelDayCountInterval1 = new System.Windows.Forms.Label();
            this.labelDayCountInterval2 = new System.Windows.Forms.Label();
            this.description = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.recurringSave = new System.Windows.Forms.Button();
            this.dateRangeLower = new System.Windows.Forms.TextBox();
            this.dateRangeUpper = new System.Windows.Forms.TextBox();
            this.weekdaysSelectedBox = new System.Windows.Forms.CheckedListBox();
            this.labelDayCountFixed1 = new System.Windows.Forms.Label();
            this.everyNthDay = new System.Windows.Forms.TextBox();
            this.labelDayCountFixed2 = new System.Windows.Forms.Label();
            this.labelDayCountFixed3 = new System.Windows.Forms.Label();
            this.firstDate = new System.Windows.Forms.DateTimePicker();
            this.backColorPickButton = new System.Windows.Forms.Button();
            this.textColorPickButton = new System.Windows.Forms.Button();
            this.rbDayCountInterval = new System.Windows.Forms.RadioButton();
            this.rbDayCountFixed = new System.Windows.Forms.RadioButton();
            this.rbWeekly = new System.Windows.Forms.RadioButton();
            this.statusMessage = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.timePickerRangeEstimatedLength = new System.Windows.Forms.DateTimePicker();
            this.labelEstimatedDuration = new System.Windows.Forms.Label();
            this.timePickerRangeTo = new System.Windows.Forms.DateTimePicker();
            this.labelRangeTimeTo = new System.Windows.Forms.Label();
            this.timePickerRangeFrom = new System.Windows.Forms.DateTimePicker();
            this.labelRangeTimeFrom = new System.Windows.Forms.Label();
            this.labelRangeTimeDesc = new System.Windows.Forms.Label();
            this.rbRangeTime = new System.Windows.Forms.RadioButton();
            this.rbNoTime = new System.Windows.Forms.RadioButton();
            this.timePickerSetTo = new System.Windows.Forms.DateTimePicker();
            this.labelSetTimeTo = new System.Windows.Forms.Label();
            this.timePickerSetFrom = new System.Windows.Forms.DateTimePicker();
            this.labelSetTimeFrom = new System.Windows.Forms.Label();
            this.rbSetTime = new System.Windows.Forms.RadioButton();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelDayCountInterval1
            // 
            this.labelDayCountInterval1.AutoSize = true;
            this.labelDayCountInterval1.Location = new System.Drawing.Point(77, 33);
            this.labelDayCountInterval1.Name = "labelDayCountInterval1";
            this.labelDayCountInterval1.Size = new System.Drawing.Size(14, 13);
            this.labelDayCountInterval1.TabIndex = 5;
            this.labelDayCountInterval1.Text = "til";
            // 
            // labelDayCountInterval2
            // 
            this.labelDayCountInterval2.AutoSize = true;
            this.labelDayCountInterval2.Location = new System.Drawing.Point(147, 33);
            this.labelDayCountInterval2.Name = "labelDayCountInterval2";
            this.labelDayCountInterval2.Size = new System.Drawing.Size(31, 13);
            this.labelDayCountInterval2.TabIndex = 7;
            this.labelDayCountInterval2.Text = "dage";
            // 
            // description
            // 
            this.description.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.description.Location = new System.Drawing.Point(6, 20);
            this.description.Name = "description";
            this.description.Size = new System.Drawing.Size(369, 20);
            this.description.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(3, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(91, 17);
            this.label6.TabIndex = 9;
            this.label6.Text = "Beskrivelse";
            // 
            // recurringSave
            // 
            this.recurringSave.Location = new System.Drawing.Point(204, 296);
            this.recurringSave.Name = "recurringSave";
            this.recurringSave.Size = new System.Drawing.Size(186, 61);
            this.recurringSave.TabIndex = 7;
            this.recurringSave.Text = "Gem";
            this.recurringSave.UseVisualStyleBackColor = true;
            this.recurringSave.Click += new System.EventHandler(this.RecurringSave_Click);
            // 
            // dateRangeLower
            // 
            this.dateRangeLower.Location = new System.Drawing.Point(21, 30);
            this.dateRangeLower.Name = "dateRangeLower";
            this.dateRangeLower.Size = new System.Drawing.Size(50, 20);
            this.dateRangeLower.TabIndex = 2;
            // 
            // dateRangeUpper
            // 
            this.dateRangeUpper.Location = new System.Drawing.Point(97, 30);
            this.dateRangeUpper.Name = "dateRangeUpper";
            this.dateRangeUpper.Size = new System.Drawing.Size(44, 20);
            this.dateRangeUpper.TabIndex = 3;
            // 
            // weekdaysSelectedBox
            // 
            this.weekdaysSelectedBox.CheckOnClick = true;
            this.weekdaysSelectedBox.FormattingEnabled = true;
            this.weekdaysSelectedBox.Items.AddRange(new object[] {
            "Mandag",
            "Tirsdag",
            "Onsdag",
            "Torsdag",
            "Fredag",
            "Lørdag",
            "Søndag"});
            this.weekdaysSelectedBox.Location = new System.Drawing.Point(21, 153);
            this.weekdaysSelectedBox.Name = "weekdaysSelectedBox";
            this.weekdaysSelectedBox.Size = new System.Drawing.Size(120, 109);
            this.weekdaysSelectedBox.TabIndex = 6;
            // 
            // labelDayCountFixed1
            // 
            this.labelDayCountFixed1.AutoSize = true;
            this.labelDayCountFixed1.Location = new System.Drawing.Point(18, 80);
            this.labelDayCountFixed1.Name = "labelDayCountFixed1";
            this.labelDayCountFixed1.Size = new System.Drawing.Size(66, 13);
            this.labelDayCountFixed1.TabIndex = 16;
            this.labelDayCountFixed1.Text = "Gentag hver";
            // 
            // everyNthDay
            // 
            this.everyNthDay.Location = new System.Drawing.Point(90, 77);
            this.everyNthDay.Name = "everyNthDay";
            this.everyNthDay.Size = new System.Drawing.Size(45, 20);
            this.everyNthDay.TabIndex = 4;
            // 
            // labelDayCountFixed2
            // 
            this.labelDayCountFixed2.AutoSize = true;
            this.labelDayCountFixed2.Location = new System.Drawing.Point(141, 80);
            this.labelDayCountFixed2.Name = "labelDayCountFixed2";
            this.labelDayCountFixed2.Size = new System.Drawing.Size(31, 13);
            this.labelDayCountFixed2.TabIndex = 18;
            this.labelDayCountFixed2.Text = ". dag";
            // 
            // labelDayCountFixed3
            // 
            this.labelDayCountFixed3.AutoSize = true;
            this.labelDayCountFixed3.Location = new System.Drawing.Point(18, 110);
            this.labelDayCountFixed3.Name = "labelDayCountFixed3";
            this.labelDayCountFixed3.Size = new System.Drawing.Size(66, 13);
            this.labelDayCountFixed3.TabIndex = 19;
            this.labelDayCountFixed3.Text = "Første gang:";
            // 
            // firstDate
            // 
            this.firstDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.firstDate.Location = new System.Drawing.Point(90, 104);
            this.firstDate.Name = "firstDate";
            this.firstDate.Size = new System.Drawing.Size(88, 20);
            this.firstDate.TabIndex = 5;
            // 
            // backColorPickButton
            // 
            this.backColorPickButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.backColorPickButton.Location = new System.Drawing.Point(192, 3);
            this.backColorPickButton.Name = "backColorPickButton";
            this.backColorPickButton.Size = new System.Drawing.Size(183, 28);
            this.backColorPickButton.TabIndex = 21;
            this.backColorPickButton.Text = "Baggrundsfarve";
            this.backColorPickButton.UseVisualStyleBackColor = true;
            this.backColorPickButton.Click += new System.EventHandler(this.ColorPickButton_Click);
            // 
            // textColorPickButton
            // 
            this.textColorPickButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textColorPickButton.Location = new System.Drawing.Point(3, 3);
            this.textColorPickButton.Name = "textColorPickButton";
            this.textColorPickButton.Size = new System.Drawing.Size(183, 28);
            this.textColorPickButton.TabIndex = 22;
            this.textColorPickButton.Text = "Tekstfarve";
            this.textColorPickButton.UseVisualStyleBackColor = true;
            this.textColorPickButton.Click += new System.EventHandler(this.TextColorPickButton_Click);
            // 
            // rbDayCountInterval
            // 
            this.rbDayCountInterval.AutoSize = true;
            this.rbDayCountInterval.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbDayCountInterval.Location = new System.Drawing.Point(3, 3);
            this.rbDayCountInterval.Name = "rbDayCountInterval";
            this.rbDayCountInterval.Size = new System.Drawing.Size(144, 21);
            this.rbDayCountInterval.TabIndex = 23;
            this.rbDayCountInterval.TabStop = true;
            this.rbDayCountInterval.Text = "Ca. dagsinterval";
            this.rbDayCountInterval.UseVisualStyleBackColor = true;
            this.rbDayCountInterval.CheckedChanged += new System.EventHandler(this.RbDayCountInterval_CheckedChanged);
            // 
            // rbDayCountFixed
            // 
            this.rbDayCountFixed.AutoSize = true;
            this.rbDayCountFixed.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbDayCountFixed.Location = new System.Drawing.Point(3, 56);
            this.rbDayCountFixed.Name = "rbDayCountFixed";
            this.rbDayCountFixed.Size = new System.Drawing.Size(151, 21);
            this.rbDayCountFixed.TabIndex = 24;
            this.rbDayCountFixed.TabStop = true;
            this.rbDayCountFixed.Text = "Fast dagsinterval";
            this.rbDayCountFixed.UseVisualStyleBackColor = true;
            this.rbDayCountFixed.CheckedChanged += new System.EventHandler(this.RbDayCountFixed_CheckedChanged);
            // 
            // rbWeekly
            // 
            this.rbWeekly.AutoSize = true;
            this.rbWeekly.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbWeekly.Location = new System.Drawing.Point(3, 126);
            this.rbWeekly.Name = "rbWeekly";
            this.rbWeekly.Size = new System.Drawing.Size(86, 21);
            this.rbWeekly.TabIndex = 25;
            this.rbWeekly.TabStop = true;
            this.rbWeekly.Text = "Ugentlig";
            this.rbWeekly.UseVisualStyleBackColor = true;
            this.rbWeekly.CheckedChanged += new System.EventHandler(this.RbWeekly_CheckedChanged);
            // 
            // statusMessage
            // 
            this.statusMessage.AutoSize = true;
            this.statusMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusMessage.ForeColor = System.Drawing.Color.Red;
            this.statusMessage.Location = new System.Drawing.Point(205, 360);
            this.statusMessage.Name = "statusMessage";
            this.statusMessage.Size = new System.Drawing.Size(41, 13);
            this.statusMessage.TabIndex = 26;
            this.statusMessage.Text = "label1";
            this.statusMessage.Visible = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rbDayCountInterval);
            this.panel1.Controls.Add(this.labelDayCountInterval1);
            this.panel1.Controls.Add(this.rbWeekly);
            this.panel1.Controls.Add(this.labelDayCountInterval2);
            this.panel1.Controls.Add(this.rbDayCountFixed);
            this.panel1.Controls.Add(this.dateRangeLower);
            this.panel1.Controls.Add(this.dateRangeUpper);
            this.panel1.Controls.Add(this.weekdaysSelectedBox);
            this.panel1.Controls.Add(this.labelDayCountFixed1);
            this.panel1.Controls.Add(this.firstDate);
            this.panel1.Controls.Add(this.everyNthDay);
            this.panel1.Controls.Add(this.labelDayCountFixed3);
            this.panel1.Controls.Add(this.labelDayCountFixed2);
            this.panel1.Location = new System.Drawing.Point(12, 101);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(186, 272);
            this.panel1.TabIndex = 27;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tableLayoutPanel1);
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.description);
            this.panel2.Location = new System.Drawing.Point(12, 12);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(378, 83);
            this.panel2.TabIndex = 28;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.textColorPickButton, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.backColorPickButton, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 46);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(378, 34);
            this.tableLayoutPanel1.TabIndex = 29;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.timePickerRangeEstimatedLength);
            this.panel3.Controls.Add(this.labelEstimatedDuration);
            this.panel3.Controls.Add(this.timePickerRangeTo);
            this.panel3.Controls.Add(this.labelRangeTimeTo);
            this.panel3.Controls.Add(this.timePickerRangeFrom);
            this.panel3.Controls.Add(this.labelRangeTimeFrom);
            this.panel3.Controls.Add(this.labelRangeTimeDesc);
            this.panel3.Controls.Add(this.rbRangeTime);
            this.panel3.Controls.Add(this.rbNoTime);
            this.panel3.Controls.Add(this.timePickerSetTo);
            this.panel3.Controls.Add(this.labelSetTimeTo);
            this.panel3.Controls.Add(this.timePickerSetFrom);
            this.panel3.Controls.Add(this.labelSetTimeFrom);
            this.panel3.Controls.Add(this.rbSetTime);
            this.panel3.Location = new System.Drawing.Point(204, 101);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(186, 189);
            this.panel3.TabIndex = 29;
            // 
            // timePickerRangeEstimatedLength
            // 
            this.timePickerRangeEstimatedLength.CustomFormat = "HH:mm";
            this.timePickerRangeEstimatedLength.Enabled = false;
            this.timePickerRangeEstimatedLength.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.timePickerRangeEstimatedLength.Location = new System.Drawing.Point(115, 141);
            this.timePickerRangeEstimatedLength.Name = "timePickerRangeEstimatedLength";
            this.timePickerRangeEstimatedLength.ShowUpDown = true;
            this.timePickerRangeEstimatedLength.Size = new System.Drawing.Size(51, 20);
            this.timePickerRangeEstimatedLength.TabIndex = 12;
            this.timePickerRangeEstimatedLength.Visible = false;
            // 
            // labelEstimatedDuration
            // 
            this.labelEstimatedDuration.AutoSize = true;
            this.labelEstimatedDuration.Enabled = false;
            this.labelEstimatedDuration.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelEstimatedDuration.Location = new System.Drawing.Point(18, 144);
            this.labelEstimatedDuration.Name = "labelEstimatedDuration";
            this.labelEstimatedDuration.Size = new System.Drawing.Size(91, 13);
            this.labelEstimatedDuration.TabIndex = 13;
            this.labelEstimatedDuration.Text = "Forventet længde";
            this.labelEstimatedDuration.Visible = false;
            // 
            // timePickerRangeTo
            // 
            this.timePickerRangeTo.CustomFormat = "HH:mm";
            this.timePickerRangeTo.Enabled = false;
            this.timePickerRangeTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.timePickerRangeTo.Location = new System.Drawing.Point(128, 115);
            this.timePickerRangeTo.Name = "timePickerRangeTo";
            this.timePickerRangeTo.ShowUpDown = true;
            this.timePickerRangeTo.Size = new System.Drawing.Size(51, 20);
            this.timePickerRangeTo.TabIndex = 11;
            this.timePickerRangeTo.Visible = false;
            // 
            // labelRangeTimeTo
            // 
            this.labelRangeTimeTo.AutoSize = true;
            this.labelRangeTimeTo.Enabled = false;
            this.labelRangeTimeTo.Location = new System.Drawing.Point(107, 117);
            this.labelRangeTimeTo.Name = "labelRangeTimeTo";
            this.labelRangeTimeTo.Size = new System.Drawing.Size(14, 13);
            this.labelRangeTimeTo.TabIndex = 10;
            this.labelRangeTimeTo.Text = "til";
            this.labelRangeTimeTo.Visible = false;
            // 
            // timePickerRangeFrom
            // 
            this.timePickerRangeFrom.CustomFormat = "HH:mm";
            this.timePickerRangeFrom.Enabled = false;
            this.timePickerRangeFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.timePickerRangeFrom.Location = new System.Drawing.Point(46, 115);
            this.timePickerRangeFrom.Name = "timePickerRangeFrom";
            this.timePickerRangeFrom.ShowUpDown = true;
            this.timePickerRangeFrom.Size = new System.Drawing.Size(54, 20);
            this.timePickerRangeFrom.TabIndex = 8;
            this.timePickerRangeFrom.Visible = false;
            // 
            // labelRangeTimeFrom
            // 
            this.labelRangeTimeFrom.AutoSize = true;
            this.labelRangeTimeFrom.Enabled = false;
            this.labelRangeTimeFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRangeTimeFrom.Location = new System.Drawing.Point(18, 118);
            this.labelRangeTimeFrom.Name = "labelRangeTimeFrom";
            this.labelRangeTimeFrom.Size = new System.Drawing.Size(22, 13);
            this.labelRangeTimeFrom.TabIndex = 9;
            this.labelRangeTimeFrom.Text = "Fra";
            this.labelRangeTimeFrom.Visible = false;
            // 
            // labelRangeTimeDesc
            // 
            this.labelRangeTimeDesc.AutoSize = true;
            this.labelRangeTimeDesc.Enabled = false;
            this.labelRangeTimeDesc.Location = new System.Drawing.Point(18, 99);
            this.labelRangeTimeDesc.Name = "labelRangeTimeDesc";
            this.labelRangeTimeDesc.Size = new System.Drawing.Size(82, 13);
            this.labelRangeTimeDesc.TabIndex = 7;
            this.labelRangeTimeDesc.Text = "Tidsrum for start";
            this.labelRangeTimeDesc.Visible = false;
            // 
            // rbRangeTime
            // 
            this.rbRangeTime.AutoSize = true;
            this.rbRangeTime.Enabled = false;
            this.rbRangeTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbRangeTime.Location = new System.Drawing.Point(4, 76);
            this.rbRangeTime.Name = "rbRangeTime";
            this.rbRangeTime.Size = new System.Drawing.Size(111, 20);
            this.rbRangeTime.TabIndex = 6;
            this.rbRangeTime.TabStop = true;
            this.rbRangeTime.Text = "Fleksibel tid";
            this.rbRangeTime.UseVisualStyleBackColor = true;
            this.rbRangeTime.Visible = false;
            this.rbRangeTime.CheckedChanged += new System.EventHandler(this.RbRangeTime_CheckedChanged);
            // 
            // rbNoTime
            // 
            this.rbNoTime.AutoSize = true;
            this.rbNoTime.Enabled = false;
            this.rbNoTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbNoTime.Location = new System.Drawing.Point(4, 4);
            this.rbNoTime.Name = "rbNoTime";
            this.rbNoTime.Size = new System.Drawing.Size(139, 20);
            this.rbNoTime.TabIndex = 5;
            this.rbNoTime.TabStop = true;
            this.rbNoTime.Text = "Uspecificeret tid";
            this.rbNoTime.UseVisualStyleBackColor = true;
            this.rbNoTime.Visible = false;
            this.rbNoTime.CheckedChanged += new System.EventHandler(this.RbNoTime_CheckedChanged);
            // 
            // timePickerSetTo
            // 
            this.timePickerSetTo.CustomFormat = "HH:mm";
            this.timePickerSetTo.Enabled = false;
            this.timePickerSetTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.timePickerSetTo.Location = new System.Drawing.Point(128, 56);
            this.timePickerSetTo.Name = "timePickerSetTo";
            this.timePickerSetTo.ShowUpDown = true;
            this.timePickerSetTo.Size = new System.Drawing.Size(51, 20);
            this.timePickerSetTo.TabIndex = 4;
            this.timePickerSetTo.Visible = false;
            // 
            // labelSetTimeTo
            // 
            this.labelSetTimeTo.AutoSize = true;
            this.labelSetTimeTo.Enabled = false;
            this.labelSetTimeTo.Location = new System.Drawing.Point(107, 58);
            this.labelSetTimeTo.Name = "labelSetTimeTo";
            this.labelSetTimeTo.Size = new System.Drawing.Size(14, 13);
            this.labelSetTimeTo.TabIndex = 3;
            this.labelSetTimeTo.Text = "til";
            this.labelSetTimeTo.Visible = false;
            // 
            // timePickerSetFrom
            // 
            this.timePickerSetFrom.CustomFormat = "HH:mm";
            this.timePickerSetFrom.Enabled = false;
            this.timePickerSetFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.timePickerSetFrom.Location = new System.Drawing.Point(46, 56);
            this.timePickerSetFrom.Name = "timePickerSetFrom";
            this.timePickerSetFrom.ShowUpDown = true;
            this.timePickerSetFrom.Size = new System.Drawing.Size(54, 20);
            this.timePickerSetFrom.TabIndex = 1;
            this.timePickerSetFrom.Visible = false;
            // 
            // labelSetTimeFrom
            // 
            this.labelSetTimeFrom.AutoSize = true;
            this.labelSetTimeFrom.Enabled = false;
            this.labelSetTimeFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSetTimeFrom.Location = new System.Drawing.Point(18, 59);
            this.labelSetTimeFrom.Name = "labelSetTimeFrom";
            this.labelSetTimeFrom.Size = new System.Drawing.Size(22, 13);
            this.labelSetTimeFrom.TabIndex = 2;
            this.labelSetTimeFrom.Text = "Fra";
            this.labelSetTimeFrom.Visible = false;
            // 
            // rbSetTime
            // 
            this.rbSetTime.AutoSize = true;
            this.rbSetTime.Enabled = false;
            this.rbSetTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbSetTime.Location = new System.Drawing.Point(4, 30);
            this.rbSetTime.Name = "rbSetTime";
            this.rbSetTime.Size = new System.Drawing.Size(77, 20);
            this.rbSetTime.TabIndex = 0;
            this.rbSetTime.TabStop = true;
            this.rbSetTime.Text = "Fast tid";
            this.rbSetTime.UseVisualStyleBackColor = true;
            this.rbSetTime.Visible = false;
            this.rbSetTime.CheckedChanged += new System.EventHandler(this.RbSetTime_CheckedChanged);
            // 
            // RecurringTaskForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(401, 380);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusMessage);
            this.Controls.Add(this.recurringSave);
            this.Name = "RecurringTaskForm";
            this.Text = "Ny Gentagende Opgave";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelDayCountInterval1;
        private System.Windows.Forms.Label labelDayCountInterval2;
        private System.Windows.Forms.TextBox description;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button recurringSave;
        private System.Windows.Forms.TextBox dateRangeLower;
        private System.Windows.Forms.TextBox dateRangeUpper;
        private System.Windows.Forms.CheckedListBox weekdaysSelectedBox;
        private System.Windows.Forms.Label labelDayCountFixed1;
        private System.Windows.Forms.TextBox everyNthDay;
        private System.Windows.Forms.Label labelDayCountFixed2;
        private System.Windows.Forms.Label labelDayCountFixed3;
        private System.Windows.Forms.DateTimePicker firstDate;
        private System.Windows.Forms.Button backColorPickButton;
        private System.Windows.Forms.Button textColorPickButton;
        private System.Windows.Forms.RadioButton rbDayCountInterval;
        private System.Windows.Forms.RadioButton rbDayCountFixed;
        private System.Windows.Forms.RadioButton rbWeekly;
        private System.Windows.Forms.Label statusMessage;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.RadioButton rbSetTime;
        private System.Windows.Forms.DateTimePicker timePickerSetFrom;
        private System.Windows.Forms.DateTimePicker timePickerSetTo;
        private System.Windows.Forms.Label labelSetTimeTo;
        private System.Windows.Forms.Label labelSetTimeFrom;
        private System.Windows.Forms.RadioButton rbNoTime;
        private System.Windows.Forms.DateTimePicker timePickerRangeTo;
        private System.Windows.Forms.Label labelRangeTimeTo;
        private System.Windows.Forms.DateTimePicker timePickerRangeFrom;
        private System.Windows.Forms.Label labelRangeTimeFrom;
        private System.Windows.Forms.Label labelRangeTimeDesc;
        private System.Windows.Forms.RadioButton rbRangeTime;
        private System.Windows.Forms.DateTimePicker timePickerRangeEstimatedLength;
        private System.Windows.Forms.Label labelEstimatedDuration;
    }
}