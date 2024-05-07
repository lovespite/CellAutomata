﻿namespace CellAutomata
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
            inputSpeed = new NumericUpDown();
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
            clearSelectionToolStripMenuItem = new ToolStripMenuItem();
            shrinkSelectionToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            clearAllToolStripMenuItem = new ToolStripMenuItem();
            actionToolStripMenuItem = new ToolStripMenuItem();
            btnStartStop = new ToolStripMenuItem();
            nextGenerationToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            homeToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)canvas).BeginInit();
            ((System.ComponentModel.ISupportInitialize)inputSpeed).BeginInit();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // canvas
            // 
            canvas.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            canvas.BackColor = Color.White;
            canvas.Location = new Point(4, 55);
            canvas.Name = "canvas";
            canvas.Size = new Size(935, 655);
            canvas.TabIndex = 0;
            canvas.TabStop = false;
            canvas.DragDrop += pictureBox1_DragDrop;
            canvas.DragEnter += pictureBox1_DragEnter;
            canvas.MouseDown += pictureBox1_MouseDown;
            canvas.MouseLeave += canvas_MouseLeave;
            canvas.MouseMove += pictureBox1_MouseMove;
            canvas.MouseUp += pictureBox1_MouseUp;
            // 
            // inputSpeed
            // 
            inputSpeed.Increment = new decimal(new int[] { 20, 0, 0, 0 });
            inputSpeed.Location = new Point(140, 28);
            inputSpeed.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            inputSpeed.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            inputSpeed.Name = "inputSpeed";
            inputSpeed.Size = new Size(126, 21);
            inputSpeed.TabIndex = 1;
            inputSpeed.Value = new decimal(new int[] { 100, 0, 0, 0 });
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
            loadFileToolStripMenuItem.ShortcutKeys = Keys.F2;
            loadFileToolStripMenuItem.Size = new Size(180, 22);
            loadFileToolStripMenuItem.Text = "&Load file";
            loadFileToolStripMenuItem.Click += btnLoad_Click;
            // 
            // saveToToolStripMenuItem
            // 
            saveToToolStripMenuItem.Name = "saveToToolStripMenuItem";
            saveToToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            saveToToolStripMenuItem.Size = new Size(180, 22);
            saveToToolStripMenuItem.Text = "&Save to";
            saveToToolStripMenuItem.Click += btnSave_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(177, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(180, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { fillToolStripMenuItem, clearCellsToolStripMenuItem, toolStripSeparator3, copyToolStripMenuItem, cutToolStripMenuItem, pasteToolStripMenuItem, pasteMethodToolStripMenuItem, toolStripSeparator2, clearSelectionToolStripMenuItem, shrinkSelectionToolStripMenuItem, toolStripSeparator4, clearAllToolStripMenuItem });
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
            // pasteMethodToolStripMenuItem
            // 
            pasteMethodToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { pasteOverwrite, pasteOr, pasteAnd, pasteXor });
            pasteMethodToolStripMenuItem.Name = "pasteMethodToolStripMenuItem";
            pasteMethodToolStripMenuItem.Size = new Size(240, 22);
            pasteMethodToolStripMenuItem.Text = "Paste method";
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
            actionToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { btnStartStop, nextGenerationToolStripMenuItem });
            actionToolStripMenuItem.Name = "actionToolStripMenuItem";
            actionToolStripMenuItem.Size = new Size(56, 21);
            actionToolStripMenuItem.Text = "&Action";
            // 
            // btnStartStop
            // 
            btnStartStop.Name = "btnStartStop";
            btnStartStop.ShortcutKeys = Keys.F5;
            btnStartStop.Size = new Size(191, 22);
            btnStartStop.Text = "&Start";
            btnStartStop.Click += btnStartStop_Click;
            // 
            // nextGenerationToolStripMenuItem
            // 
            nextGenerationToolStripMenuItem.Name = "nextGenerationToolStripMenuItem";
            nextGenerationToolStripMenuItem.ShortcutKeys = Keys.F8;
            nextGenerationToolStripMenuItem.Size = new Size(191, 22);
            nextGenerationToolStripMenuItem.Text = "Next generation";
            nextGenerationToolStripMenuItem.Click += nextGenerationToolStripMenuItem_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { homeToolStripMenuItem });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(47, 21);
            viewToolStripMenuItem.Text = "&View";
            // 
            // homeToolStripMenuItem
            // 
            homeToolStripMenuItem.Name = "homeToolStripMenuItem";
            homeToolStripMenuItem.ShortcutKeyDisplayString = "";
            homeToolStripMenuItem.ShortcutKeys = Keys.F1;
            homeToolStripMenuItem.Size = new Size(180, 22);
            homeToolStripMenuItem.Text = "Home";
            homeToolStripMenuItem.Click += homeToolStripMenuItem_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(943, 715);
            Controls.Add(label1);
            Controls.Add(inputSpeed);
            Controls.Add(canvas);
            Controls.Add(menuStrip1);
            DoubleBuffered = true;
            Font = new Font("Trebuchet MS", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "Cell Automata";
            FormClosed += Form1_FormClosed;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)canvas).EndInit();
            ((System.ComponentModel.ISupportInitialize)inputSpeed).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox canvas;
        private NumericUpDown inputSpeed;
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
    }
}
