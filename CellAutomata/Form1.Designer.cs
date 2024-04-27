namespace CellAutomata
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pictureBox1 = new PictureBox();
            inputSpeed = new NumericUpDown();
            label1 = new Label();
            label2 = new Label();
            inputSize = new NumericUpDown();
            btnStartStop = new Button();
            btnClear = new Button();
            btnSave = new Button();
            btnLoad = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)inputSpeed).BeginInit();
            ((System.ComponentModel.ISupportInitialize)inputSize).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pictureBox1.BackColor = Color.Black;
            pictureBox1.Location = new Point(12, 45);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(838, 618);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.Paint += pictureBox1_Paint;
            pictureBox1.MouseDown += pictureBox1_MouseDown;
            pictureBox1.MouseMove += pictureBox1_MouseMove;
            pictureBox1.MouseUp += pictureBox1_MouseUp;
            // 
            // inputSpeed
            // 
            inputSpeed.Increment = new decimal(new int[] { 20, 0, 0, 0 });
            inputSpeed.Location = new Point(144, 11);
            inputSpeed.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            inputSpeed.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            inputSpeed.Name = "inputSpeed";
            inputSpeed.Size = new Size(126, 23);
            inputSpeed.TabIndex = 1;
            inputSpeed.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 13);
            label1.Name = "label1";
            label1.Size = new Size(126, 17);
            label1.TabIndex = 2;
            label1.Text = "Iteration speed (ms)";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(288, 15);
            label2.Name = "label2";
            label2.Size = new Size(90, 17);
            label2.TabIndex = 4;
            label2.Text = "Cell size(pixel)";
            // 
            // inputSize
            // 
            inputSize.Increment = new decimal(new int[] { 2, 0, 0, 0 });
            inputSize.Location = new Point(384, 11);
            inputSize.Maximum = new decimal(new int[] { 11, 0, 0, 0 });
            inputSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            inputSize.Name = "inputSize";
            inputSize.Size = new Size(126, 23);
            inputSize.TabIndex = 3;
            inputSize.Value = new decimal(new int[] { 6, 0, 0, 0 });
            inputSize.ValueChanged += inputSize_ValueChanged;
            // 
            // btnStartStop
            // 
            btnStartStop.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnStartStop.Location = new Point(858, 45);
            btnStartStop.Name = "btnStartStop";
            btnStartStop.Size = new Size(75, 33);
            btnStartStop.TabIndex = 5;
            btnStartStop.Text = "Start";
            btnStartStop.UseVisualStyleBackColor = true;
            btnStartStop.Click += btnStartStop_Click;
            // 
            // btnClear
            // 
            btnClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClear.Location = new Point(858, 84);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(75, 33);
            btnClear.TabIndex = 6;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.Location = new Point(858, 123);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 33);
            btnSave.TabIndex = 7;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnLoad
            // 
            btnLoad.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLoad.Location = new Point(858, 162);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(75, 33);
            btnLoad.TabIndex = 7;
            btnLoad.Text = "Load";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += btnLoad_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(943, 675);
            Controls.Add(btnLoad);
            Controls.Add(btnSave);
            Controls.Add(btnClear);
            Controls.Add(btnStartStop);
            Controls.Add(label2);
            Controls.Add(inputSize);
            Controls.Add(label1);
            Controls.Add(inputSpeed);
            Controls.Add(pictureBox1);
            Name = "Form1";
            Text = "Cell Automata";
            FormClosed += Form1_FormClosed;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)inputSpeed).EndInit();
            ((System.ComponentModel.ISupportInitialize)inputSize).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private NumericUpDown inputSpeed;
        private Label label1;
        private Label label2;
        private NumericUpDown inputSize;
        private Button btnStartStop;
        private Button btnClear;
        private Button btnSave;
        private Button btnLoad;
    }
}
