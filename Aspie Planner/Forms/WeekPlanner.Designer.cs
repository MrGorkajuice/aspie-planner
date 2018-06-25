namespace Aspie_Planner.Forms
{
    partial class WeekPlanner
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
            this.dgvWeekPlanner = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWeekPlanner)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvWeekPlanner
            // 
            this.dgvWeekPlanner.AllowUserToAddRows = false;
            this.dgvWeekPlanner.AllowUserToDeleteRows = false;
            this.dgvWeekPlanner.AllowUserToResizeColumns = false;
            this.dgvWeekPlanner.AllowUserToResizeRows = false;
            this.dgvWeekPlanner.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvWeekPlanner.CausesValidation = false;
            this.dgvWeekPlanner.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvWeekPlanner.Location = new System.Drawing.Point(13, 13);
            this.dgvWeekPlanner.MultiSelect = false;
            this.dgvWeekPlanner.Name = "dgvWeekPlanner";
            this.dgvWeekPlanner.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgvWeekPlanner.ShowCellErrors = false;
            this.dgvWeekPlanner.ShowCellToolTips = false;
            this.dgvWeekPlanner.ShowEditingIcon = false;
            this.dgvWeekPlanner.ShowRowErrors = false;
            this.dgvWeekPlanner.Size = new System.Drawing.Size(738, 499);
            this.dgvWeekPlanner.TabIndex = 0;
            this.dgvWeekPlanner.TabStop = false;
            // 
            // WeekPlanner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1003, 524);
            this.Controls.Add(this.dgvWeekPlanner);
            this.Name = "WeekPlanner";
            this.Text = "Ugeplanlægning";
            ((System.ComponentModel.ISupportInitialize)(this.dgvWeekPlanner)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvWeekPlanner;
    }
}