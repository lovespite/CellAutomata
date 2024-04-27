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
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            loadFileToolStripMenuItem = new ToolStripMenuItem();
            saveToToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            fillToolStripMenuItem = new ToolStripMenuItem();
            clearCellsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            copyToolStripMenuItem = new ToolStripMenuItem();
            cutToolStripMenuItem = new ToolStripMenuItem();
            pasteToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            clearSelectionToolStripMenuItem = new ToolStripMenuItem();
            shrinkSelectionToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            clearAllToolStripMenuItem = new ToolStripMenuItem();
            actionToolStripMenuItem = new ToolStripMenuItem();
            btnStartStop = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)inputSpeed).BeginInit();
            ((System.ComponentModel.ISupportInitialize)inputSize).BeginInit();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pictureBox1.BackColor = Color.Black;
            pictureBox1.Location = new Point(4, 52);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(935, 619);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.DragDrop += pictureBox1_DragDrop;
            pictureBox1.DragEnter += pictureBox1_DragEnter;
            pictureBox1.Paint += pictureBox1_Paint;
            pictureBox1.MouseDown += pictureBox1_MouseDown;
            pictureBox1.MouseMove += pictureBox1_MouseMove;
            pictureBox1.MouseUp += pictureBox1_MouseUp;
            // 
            // inputSpeed
            // 
            inputSpeed.Increment = new decimal(new int[] { 20, 0, 0, 0 });
            inputSpeed.Location = new Point(140, 26);
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
            label1.Location = new Point(8, 28);
            label1.Name = "label1";
            label1.Size = new Size(126, 17);
            label1.TabIndex = 2;
            label1.Text = "Iteration speed (ms)";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(284, 30);
            label2.Name = "label2";
            label2.Size = new Size(90, 17);
            label2.TabIndex = 4;
            label2.Text = "Cell size(pixel)";
            // 
            // inputSize
            // 
            inputSize.Increment = new decimal(new int[] { 2, 0, 0, 0 });
            inputSize.Location = new Point(380, 26);
            inputSize.Maximum = new decimal(new int[] { 21, 0, 0, 0 });
            inputSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            inputSize.Name = "inputSize";
            inputSize.Size = new Size(126, 23);
            inputSize.TabIndex = 3;
            inputSize.Value = new decimal(new int[] { 10, 0, 0, 0 });
            inputSize.ValueChanged += inputSize_ValueChanged;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, actionToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.RenderMode = ToolStripRenderMode.System;
            menuStrip1.Size = new Size(943, 25);
            menuStrip1.TabIndex = 8;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { loadFileToolStripMenuItem, saveToToolStripMenuItem, toolStripSeparator1, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(39, 21);
            fileToolStripMenuItem.Text = "&File";
            // 
            // loadFileToolStripMenuItem
            // 
            loadFileToolStripMenuItem.Name = "loadFileToolStripMenuItem";
            loadFileToolStripMenuItem.ShortcutKeys = Keys.F1;
            loadFileToolStripMenuItem.Size = new Size(166, 22);
            loadFileToolStripMenuItem.Text = "&Load File";
            loadFileToolStripMenuItem.Click += btnLoad_Click;
            // 
            // saveToToolStripMenuItem
            // 
            saveToToolStripMenuItem.Name = "saveToToolStripMenuItem";
            saveToToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            saveToToolStripMenuItem.Size = new Size(166, 22);
            saveToToolStripMenuItem.Text = "&Save To";
            saveToToolStripMenuItem.Click += btnSave_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(163, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(166, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { fillToolStripMenuItem, clearCellsToolStripMenuItem, toolStripSeparator3, copyToolStripMenuItem, cutToolStripMenuItem, pasteToolStripMenuItem, toolStripSeparator2, clearSelectionToolStripMenuItem, shrinkSelectionToolStripMenuItem, toolStripSeparator4, clearAllToolStripMenuItem });
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(42, 21);
            editToolStripMenuItem.Text = "&Edit";
            // 
            // fillToolStripMenuItem
            // 
            fillToolStripMenuItem.Name = "fillToolStripMenuItem";
            fillToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.F;
            fillToolStripMenuItem.Size = new Size(240, 22);
            fillToolStripMenuItem.Text = "&Fill";
            fillToolStripMenuItem.Click += fillToolStripMenuItem_Click;
            // 
            // clearCellsToolStripMenuItem
            // 
            clearCellsToolStripMenuItem.Name = "clearCellsToolStripMenuItem";
            clearCellsToolStripMenuItem.ShortcutKeys = Keys.Delete;
            clearCellsToolStripMenuItem.Size = new Size(240, 22);
            clearCellsToolStripMenuItem.Text = "&Clear selected cells";
            clearCellsToolStripMenuItem.Click += clearSelectedCellsToolStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(237, 6);
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.C;
            copyToolStripMenuItem.Size = new Size(240, 22);
            copyToolStripMenuItem.Text = "C&opy";
            copyToolStripMenuItem.Click += copyToolStripMenuItem_Click;
            // 
            // cutToolStripMenuItem
            // 
            cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            cutToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.X;
            cutToolStripMenuItem.Size = new Size(240, 22);
            cutToolStripMenuItem.Text = "C&ut";
            cutToolStripMenuItem.Click += cutToolStripMenuItem_Click;
            // 
            // pasteToolStripMenuItem
            // 
            pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            pasteToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.V;
            pasteToolStripMenuItem.Size = new Size(240, 22);
            pasteToolStripMenuItem.Text = "&Paste";
            pasteToolStripMenuItem.Click += pasteToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(237, 6);
            // 
            // clearSelectionToolStripMenuItem
            // 
            clearSelectionToolStripMenuItem.Name = "clearSelectionToolStripMenuItem";
            clearSelectionToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.A;
            clearSelectionToolStripMenuItem.Size = new Size(240, 22);
            clearSelectionToolStripMenuItem.Text = "Clear &selection";
            clearSelectionToolStripMenuItem.Click += clearSelectionToolStripMenuItem_Click;
            // 
            // shrinkSelectionToolStripMenuItem
            // 
            shrinkSelectionToolStripMenuItem.Name = "shrinkSelectionToolStripMenuItem";
            shrinkSelectionToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.A;
            shrinkSelectionToolStripMenuItem.Size = new Size(240, 22);
            shrinkSelectionToolStripMenuItem.Text = "S&hrink selection";
            shrinkSelectionToolStripMenuItem.Click += shrinkSelectionToolStripMenuItem_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(237, 6);
            // 
            // clearAllToolStripMenuItem
            // 
            clearAllToolStripMenuItem.Name = "clearAllToolStripMenuItem";
            clearAllToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.Delete;
            clearAllToolStripMenuItem.Size = new Size(240, 22);
            clearAllToolStripMenuItem.Text = "Clear all";
            clearAllToolStripMenuItem.Click += btnClear_Click;
            // 
            // actionToolStripMenuItem
            // 
            actionToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { btnStartStop });
            actionToolStripMenuItem.Name = "actionToolStripMenuItem";
            actionToolStripMenuItem.Size = new Size(56, 21);
            actionToolStripMenuItem.Text = "&Action";
            // 
            // btnStartStop
            // 
            btnStartStop.Name = "btnStartStop";
            btnStartStop.ShortcutKeys = Keys.F5;
            btnStartStop.Size = new Size(124, 22);
            btnStartStop.Text = "&Start";
            btnStartStop.Click += btnStartStop_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(943, 675);
            Controls.Add(label2);
            Controls.Add(inputSize);
            Controls.Add(label1);
            Controls.Add(inputSpeed);
            Controls.Add(pictureBox1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "Cell Automata";
            FormClosed += Form1_FormClosed;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)inputSpeed).EndInit();
            ((System.ComponentModel.ISupportInitialize)inputSize).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private NumericUpDown inputSpeed;
        private Label label1;
        private Label label2;
        private NumericUpDown inputSize;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem loadFileToolStripMenuItem;
        private ToolStripMenuItem saveToToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem clearCellsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem copyToolStripMenuItem;
        private ToolStripMenuItem cutToolStripMenuItem;
        private ToolStripMenuItem pasteToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem clearSelectionToolStripMenuItem;
        private ToolStripMenuItem shrinkSelectionToolStripMenuItem;
        private ToolStripMenuItem fillToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem clearAllToolStripMenuItem;
        private ToolStripMenuItem actionToolStripMenuItem;
        private ToolStripMenuItem btnStartStop;
    }
}
