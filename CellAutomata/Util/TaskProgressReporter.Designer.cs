namespace CellAutomata.Util
{
    partial class TaskProgressReporter
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
            progressBar1 = new ProgressBar();
            lbDescription = new Label();
            lbPercentage = new Label();
            lbTimeRemaining = new Label();
            btnAbort = new Button();
            lbStatus = new Label();
            SuspendLayout();
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(12, 72);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(454, 34);
            progressBar1.TabIndex = 0;
            // 
            // lbDescription
            // 
            lbDescription.AutoEllipsis = true;
            lbDescription.Location = new Point(12, 9);
            lbDescription.Name = "lbDescription";
            lbDescription.Size = new Size(454, 60);
            lbDescription.TabIndex = 1;
            lbDescription.Text = "Description.";
            // 
            // lbPercentage
            // 
            lbPercentage.AutoSize = true;
            lbPercentage.Location = new Point(12, 109);
            lbPercentage.Name = "lbPercentage";
            lbPercentage.Size = new Size(26, 17);
            lbPercentage.TabIndex = 2;
            lbPercentage.Text = "0%";
            // 
            // lbTimeRemaining
            // 
            lbTimeRemaining.AutoSize = true;
            lbTimeRemaining.Location = new Point(12, 130);
            lbTimeRemaining.Name = "lbTimeRemaining";
            lbTimeRemaining.Size = new Size(129, 17);
            lbTimeRemaining.TabIndex = 3;
            lbTimeRemaining.Text = "0 seconds remained.";
            // 
            // btnAbort
            // 
            btnAbort.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAbort.Location = new Point(364, 179);
            btnAbort.Name = "btnAbort";
            btnAbort.Size = new Size(102, 38);
            btnAbort.TabIndex = 4;
            btnAbort.Text = "&Abort";
            btnAbort.UseVisualStyleBackColor = true;
            btnAbort.Click += BtnAbort_Click;
            // 
            // lbStatus
            // 
            lbStatus.AutoEllipsis = true;
            lbStatus.Location = new Point(12, 151);
            lbStatus.Name = "lbStatus";
            lbStatus.Size = new Size(454, 20);
            lbStatus.TabIndex = 5;
            lbStatus.Text = "Status.";
            // 
            // TaskProgressReporter
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(478, 229);
            Controls.Add(lbStatus);
            Controls.Add(btnAbort);
            Controls.Add(lbTimeRemaining);
            Controls.Add(lbPercentage);
            Controls.Add(lbDescription);
            Controls.Add(progressBar1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "TaskProgressReporter";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Task Progress Reporter";
            Load += TaskProgressReporter_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ProgressBar progressBar1;
        private Label lbDescription;
        private Label lbPercentage;
        private Label lbTimeRemaining;
        private Button btnAbort;
        private Label lbStatus;
    }
}