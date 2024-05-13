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
            canvas = new PictureBox();
            label1 = new Label();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            loadFileToolStripMenuItem = new ToolStripMenuItem();
            saveToToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            fillToolStripMenuItem = new ToolStripMenuItem();
            clearCellsToolStripMenuItem = new ToolStripMenuItem();
            clearUnselectedCellsToolStripMenuItem = new ToolStripMenuItem();
            clearAllToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            copyToolStripMenuItem = new ToolStripMenuItem();
            cutToolStripMenuItem = new ToolStripMenuItem();
            pasteToolStripMenuItem = new ToolStripMenuItem();
            pasteMethodToolStripMenuItem = new ToolStripMenuItem();
            pasteOverwrite = new ToolStripMenuItem();
            pasteOr = new ToolStripMenuItem();
            pasteAnd = new ToolStripMenuItem();
            pasteXor = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            selectAllToolStripMenuItem = new ToolStripMenuItem();
            shrinkSelectionToolStripMenuItem = new ToolStripMenuItem();
            clearSelectionToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            rotateToolStripMenuItem = new ToolStripMenuItem();
            flipUpDownToolStripMenuItem = new ToolStripMenuItem();
            flipLeftRightToolStripMenuItem = new ToolStripMenuItem();
            actionToolStripMenuItem = new ToolStripMenuItem();
            btnStartStop = new ToolStripMenuItem();
            nextGenerationToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            speedUpToolStripMenuItem = new ToolStripMenuItem();
            speedDownToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            homeToolStripMenuItem = new ToolStripMenuItem();
            fitToolStripMenuItem = new ToolStripMenuItem();
            splitContainer1 = new SplitContainer();
            panel1 = new Panel();
            ((System.ComponentModel.ISupportInitialize)canvas).BeginInit();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // canvas
            // 
            canvas.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            canvas.BackColor = Color.White;
            canvas.Location = new Point(-1, -2);
            canvas.Name = "canvas";
            canvas.Size = new Size(800, 640);
            canvas.TabIndex = 0;
            canvas.TabStop = false;
            canvas.DragDrop += pictureBox1_DragDrop;
            canvas.DragEnter += pictureBox1_DragEnter;
            canvas.MouseDown += pictureBox1_MouseDown;
            canvas.MouseLeave += canvas_MouseLeave;
            canvas.MouseMove += pictureBox1_MouseMove;
            canvas.MouseUp += pictureBox1_MouseUp;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(8, 30);
            label1.Name = "label1";
            label1.Size = new Size(119, 18);
            label1.TabIndex = 2;
            label1.Text = "Iteration speed (ms)";
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, actionToolStripMenuItem, viewToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.RenderMode = ToolStripRenderMode.System;
            menuStrip1.Size = new Size(1045, 25);
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
            loadFileToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Q;
            loadFileToolStripMenuItem.Size = new Size(173, 22);
            loadFileToolStripMenuItem.Text = "&Load file";
            loadFileToolStripMenuItem.Click += btnLoad_Click;
            // 
            // saveToToolStripMenuItem
            // 
            saveToToolStripMenuItem.Name = "saveToToolStripMenuItem";
            saveToToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            saveToToolStripMenuItem.Size = new Size(173, 22);
            saveToToolStripMenuItem.Text = "&Save to";
            saveToToolStripMenuItem.Click += btnSave_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(170, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(173, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { fillToolStripMenuItem, clearCellsToolStripMenuItem, clearUnselectedCellsToolStripMenuItem, clearAllToolStripMenuItem, toolStripSeparator3, copyToolStripMenuItem, cutToolStripMenuItem, pasteToolStripMenuItem, pasteMethodToolStripMenuItem, toolStripSeparator2, selectAllToolStripMenuItem, shrinkSelectionToolStripMenuItem, clearSelectionToolStripMenuItem, toolStripSeparator4, rotateToolStripMenuItem, flipUpDownToolStripMenuItem, flipLeftRightToolStripMenuItem });
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(42, 21);
            editToolStripMenuItem.Text = "&Edit";
            // 
            // fillToolStripMenuItem
            // 
            fillToolStripMenuItem.Name = "fillToolStripMenuItem";
            fillToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.F;
            fillToolStripMenuItem.Size = new Size(280, 22);
            fillToolStripMenuItem.Text = "&Fill";
            fillToolStripMenuItem.Click += fillToolStripMenuItem_Click;
            // 
            // clearCellsToolStripMenuItem
            // 
            clearCellsToolStripMenuItem.Name = "clearCellsToolStripMenuItem";
            clearCellsToolStripMenuItem.ShortcutKeys = Keys.Delete;
            clearCellsToolStripMenuItem.Size = new Size(280, 22);
            clearCellsToolStripMenuItem.Text = "&Clear selected cells";
            clearCellsToolStripMenuItem.Click += clearSelectedCellsToolStripMenuItem_Click;
            // 
            // clearUnselectedCellsToolStripMenuItem
            // 
            clearUnselectedCellsToolStripMenuItem.Name = "clearUnselectedCellsToolStripMenuItem";
            clearUnselectedCellsToolStripMenuItem.ShortcutKeys = Keys.Shift | Keys.Delete;
            clearUnselectedCellsToolStripMenuItem.Size = new Size(280, 22);
            clearUnselectedCellsToolStripMenuItem.Text = "C&lear unselected cells";
            clearUnselectedCellsToolStripMenuItem.Click += clearUnselectedCellsToolStripMenuItem_Click;
            // 
            // clearAllToolStripMenuItem
            // 
            clearAllToolStripMenuItem.Name = "clearAllToolStripMenuItem";
            clearAllToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.Delete;
            clearAllToolStripMenuItem.Size = new Size(280, 22);
            clearAllToolStripMenuItem.Text = "Clea&r all";
            clearAllToolStripMenuItem.Click += btnClear_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(277, 6);
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.C;
            copyToolStripMenuItem.Size = new Size(280, 22);
            copyToolStripMenuItem.Text = "C&opy";
            copyToolStripMenuItem.Click += copyToolStripMenuItem_Click;
            // 
            // cutToolStripMenuItem
            // 
            cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            cutToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.X;
            cutToolStripMenuItem.Size = new Size(280, 22);
            cutToolStripMenuItem.Text = "C&ut";
            cutToolStripMenuItem.Click += cutToolStripMenuItem_Click;
            // 
            // pasteToolStripMenuItem
            // 
            pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            pasteToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.V;
            pasteToolStripMenuItem.Size = new Size(280, 22);
            pasteToolStripMenuItem.Text = "&Paste";
            pasteToolStripMenuItem.Click += pasteToolStripMenuItem_Click;
            // 
            // pasteMethodToolStripMenuItem
            // 
            pasteMethodToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { pasteOverwrite, pasteOr, pasteAnd, pasteXor });
            pasteMethodToolStripMenuItem.Name = "pasteMethodToolStripMenuItem";
            pasteMethodToolStripMenuItem.Size = new Size(280, 22);
            pasteMethodToolStripMenuItem.Text = "Paste meth&od";
            // 
            // pasteOverwrite
            // 
            pasteOverwrite.Checked = true;
            pasteOverwrite.CheckState = CheckState.Checked;
            pasteOverwrite.Name = "pasteOverwrite";
            pasteOverwrite.Size = new Size(132, 22);
            pasteOverwrite.Text = "Overwrite";
            pasteOverwrite.Click += pasteMethods_Click;
            // 
            // pasteOr
            // 
            pasteOr.Name = "pasteOr";
            pasteOr.Size = new Size(132, 22);
            pasteOr.Text = "Or";
            pasteOr.Click += pasteMethods_Click;
            // 
            // pasteAnd
            // 
            pasteAnd.Name = "pasteAnd";
            pasteAnd.Size = new Size(132, 22);
            pasteAnd.Text = "And";
            pasteAnd.Click += pasteMethods_Click;
            // 
            // pasteXor
            // 
            pasteXor.Name = "pasteXor";
            pasteXor.Size = new Size(132, 22);
            pasteXor.Text = "Xor";
            pasteXor.Click += pasteMethods_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(277, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            selectAllToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.A;
            selectAllToolStripMenuItem.Size = new Size(280, 22);
            selectAllToolStripMenuItem.Text = "S&elect all";
            selectAllToolStripMenuItem.Click += selectAllToolStripMenuItem_Click;
            // 
            // shrinkSelectionToolStripMenuItem
            // 
            shrinkSelectionToolStripMenuItem.Name = "shrinkSelectionToolStripMenuItem";
            shrinkSelectionToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Alt | Keys.A;
            shrinkSelectionToolStripMenuItem.Size = new Size(280, 22);
            shrinkSelectionToolStripMenuItem.Text = "S&hrink selection";
            shrinkSelectionToolStripMenuItem.Click += shrinkSelectionToolStripMenuItem_Click;
            // 
            // clearSelectionToolStripMenuItem
            // 
            clearSelectionToolStripMenuItem.Name = "clearSelectionToolStripMenuItem";
            clearSelectionToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.A;
            clearSelectionToolStripMenuItem.Size = new Size(280, 22);
            clearSelectionToolStripMenuItem.Text = "Clear &selection";
            clearSelectionToolStripMenuItem.Click += clearSelectionToolStripMenuItem_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(277, 6);
            // 
            // rotateToolStripMenuItem
            // 
            rotateToolStripMenuItem.Name = "rotateToolStripMenuItem";
            rotateToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.R;
            rotateToolStripMenuItem.Size = new Size(280, 22);
            rotateToolStripMenuItem.Text = "Ro&tate";
            rotateToolStripMenuItem.Click += rotateToolStripMenuItem_Click;
            // 
            // flipUpDownToolStripMenuItem
            // 
            flipUpDownToolStripMenuItem.Name = "flipUpDownToolStripMenuItem";
            flipUpDownToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Alt | Keys.Down;
            flipUpDownToolStripMenuItem.Size = new Size(280, 22);
            flipUpDownToolStripMenuItem.Text = "Flip - Up&Down";
            flipUpDownToolStripMenuItem.Click += flipUpDownToolStripMenuItem_Click;
            // 
            // flipLeftRightToolStripMenuItem
            // 
            flipLeftRightToolStripMenuItem.Name = "flipLeftRightToolStripMenuItem";
            flipLeftRightToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Alt | Keys.Right;
            flipLeftRightToolStripMenuItem.Size = new Size(280, 22);
            flipLeftRightToolStripMenuItem.Text = "Flip - LeftRi&ght";
            flipLeftRightToolStripMenuItem.Click += flipLeftRightToolStripMenuItem_Click;
            // 
            // actionToolStripMenuItem
            // 
            actionToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { btnStartStop, nextGenerationToolStripMenuItem, toolStripSeparator5, speedUpToolStripMenuItem, speedDownToolStripMenuItem });
            actionToolStripMenuItem.Name = "actionToolStripMenuItem";
            actionToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Up;
            actionToolStripMenuItem.Size = new Size(56, 21);
            actionToolStripMenuItem.Text = "&Action";
            // 
            // btnStartStop
            // 
            btnStartStop.Name = "btnStartStop";
            btnStartStop.ShortcutKeys = Keys.F5;
            btnStartStop.Size = new Size(219, 22);
            btnStartStop.Text = "&Start";
            btnStartStop.Click += btnStartStop_Click;
            // 
            // nextGenerationToolStripMenuItem
            // 
            nextGenerationToolStripMenuItem.Name = "nextGenerationToolStripMenuItem";
            nextGenerationToolStripMenuItem.ShortcutKeys = Keys.F8;
            nextGenerationToolStripMenuItem.Size = new Size(219, 22);
            nextGenerationToolStripMenuItem.Text = "Next generation";
            nextGenerationToolStripMenuItem.Click += nextGenerationToolStripMenuItem_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(216, 6);
            // 
            // speedUpToolStripMenuItem
            // 
            speedUpToolStripMenuItem.Name = "speedUpToolStripMenuItem";
            speedUpToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Up;
            speedUpToolStripMenuItem.Size = new Size(219, 22);
            speedUpToolStripMenuItem.Text = "Speed up";
            speedUpToolStripMenuItem.Click += speedUpToolStripMenuItem_Click;
            // 
            // speedDownToolStripMenuItem
            // 
            speedDownToolStripMenuItem.Name = "speedDownToolStripMenuItem";
            speedDownToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Down;
            speedDownToolStripMenuItem.Size = new Size(219, 22);
            speedDownToolStripMenuItem.Text = "Speed down";
            speedDownToolStripMenuItem.Click += speedDownToolStripMenuItem_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { homeToolStripMenuItem, fitToolStripMenuItem });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(47, 21);
            viewToolStripMenuItem.Text = "&View";
            // 
            // homeToolStripMenuItem
            // 
            homeToolStripMenuItem.Name = "homeToolStripMenuItem";
            homeToolStripMenuItem.ShortcutKeyDisplayString = "";
            homeToolStripMenuItem.ShortcutKeys = Keys.F1;
            homeToolStripMenuItem.Size = new Size(132, 22);
            homeToolStripMenuItem.Text = "Home";
            homeToolStripMenuItem.Click += homeToolStripMenuItem_Click;
            // 
            // fitToolStripMenuItem
            // 
            fitToolStripMenuItem.Name = "fitToolStripMenuItem";
            fitToolStripMenuItem.ShortcutKeys = Keys.F2;
            fitToolStripMenuItem.Size = new Size(132, 22);
            fitToolStripMenuItem.Text = "Fit";
            fitToolStripMenuItem.Click += fitToolStripMenuItem_Click;
            // 
            // splitContainer1
            // 
            splitContainer1.BorderStyle = BorderStyle.Fixed3D;
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(canvas);
            splitContainer1.Size = new Size(1030, 641);
            splitContainer1.SplitterDistance = 224;
            splitContainer1.TabIndex = 9;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.Controls.Add(splitContainer1);
            panel1.Location = new Point(8, 55);
            panel1.Name = "panel1";
            panel1.Size = new Size(1030, 641);
            panel1.TabIndex = 10;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1045, 703);
            Controls.Add(label1);
            Controls.Add(menuStrip1);
            Controls.Add(panel1);
            DoubleBuffered = true;
            Font = new Font("Trebuchet MS", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Cell Automata";
            FormClosed += Form1_FormClosed;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)canvas).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox canvas;
        private Label label1;
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
        private ToolStripMenuItem pasteMethodToolStripMenuItem;
        private ToolStripMenuItem pasteOverwrite;
        private ToolStripMenuItem pasteOr;
        private ToolStripMenuItem pasteAnd;
        private ToolStripMenuItem pasteXor;
        private ToolStripMenuItem nextGenerationToolStripMenuItem;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem homeToolStripMenuItem;
        private ToolStripMenuItem selectAllToolStripMenuItem;
        private ToolStripMenuItem fitToolStripMenuItem;
        private ToolStripMenuItem clearUnselectedCellsToolStripMenuItem;
        private ToolStripMenuItem rotateToolStripMenuItem;
        private ToolStripMenuItem flipUpDownToolStripMenuItem;
        private ToolStripMenuItem flipLeftRightToolStripMenuItem;
        private SplitContainer splitContainer1;
        private Panel panel1;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem speedUpToolStripMenuItem;
        private ToolStripMenuItem speedDownToolStripMenuItem;
    }
}
