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
            saveToToolStripMenuItem = new ToolStripMenuItem();
            loadFileToolStripMenuItem = new ToolStripMenuItem();
            readGollyFileToolStripMenuItem = new ToolStripMenuItem();
            loadBmpImageToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            fillToolStripMenuItem = new ToolStripMenuItem();
            clearCellsToolStripMenuItem = new ToolStripMenuItem();
            clearUnselectedCellsToolStripMenuItem = new ToolStripMenuItem();
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
            toolStripSeparator6 = new ToolStripSeparator();
            randomizeToolStripMenuItem = new ToolStripMenuItem();
            randomFill25ToolStripMenuItem = new ToolStripMenuItem();
            randomFille50ToolStripMenuItem = new ToolStripMenuItem();
            berlinNoiseToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator10 = new ToolStripSeparator();
            clearAllToolStripMenuItem = new ToolStripMenuItem();
            actionToolStripMenuItem = new ToolStripMenuItem();
            setRuleToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator7 = new ToolStripSeparator();
            btnStartStop = new ToolStripMenuItem();
            nextGenerationToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            speedUpToolStripMenuItem = new ToolStripMenuItem();
            speedDownToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            homeToolStripMenuItem = new ToolStripMenuItem();
            fitToolStripMenuItem = new ToolStripMenuItem();
            movePointToCenterToolStripMenuItem = new ToolStripMenuItem();
            moveSelectionToCenterToolStripMenuItem = new ToolStripMenuItem();
            moveToToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator8 = new ToolStripSeparator();
            createNewViewToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator9 = new ToolStripSeparator();
            zoomInToolStripMenuItem = new ToolStripMenuItem();
            zoomOutToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator11 = new ToolStripSeparator();
            BtnSuspendView = new ToolStripMenuItem();
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
            canvas.DragDrop += Canvas_DragDrop;
            canvas.DragEnter += Canvas_DragEnter;
            canvas.MouseDown += Canvas_MouseDown;
            canvas.MouseLeave += Canvas_MouseLeave;
            canvas.MouseMove += Canvas_MouseMove;
            canvas.MouseUp += Canvas_MouseUp;
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
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { saveToToolStripMenuItem, loadFileToolStripMenuItem, readGollyFileToolStripMenuItem, loadBmpImageToolStripMenuItem, toolStripSeparator1, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(39, 21);
            fileToolStripMenuItem.Text = "&File";
            // 
            // saveToToolStripMenuItem
            // 
            saveToToolStripMenuItem.Name = "saveToToolStripMenuItem";
            saveToToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            saveToToolStripMenuItem.Size = new Size(204, 22);
            saveToToolStripMenuItem.Text = "&Save to";
            saveToToolStripMenuItem.Click += File_Save_Click;
            // 
            // loadFileToolStripMenuItem
            // 
            loadFileToolStripMenuItem.Name = "loadFileToolStripMenuItem";
            loadFileToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Q;
            loadFileToolStripMenuItem.Size = new Size(204, 22);
            loadFileToolStripMenuItem.Text = "&Load file";
            loadFileToolStripMenuItem.Click += File_Load_Click;
            // 
            // readGollyFileToolStripMenuItem
            // 
            readGollyFileToolStripMenuItem.Name = "readGollyFileToolStripMenuItem";
            readGollyFileToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.G;
            readGollyFileToolStripMenuItem.Size = new Size(204, 22);
            readGollyFileToolStripMenuItem.Text = "Load golly file";
            readGollyFileToolStripMenuItem.Click += File_LoadGollyFile_Click;
            // 
            // loadBmpImageToolStripMenuItem
            // 
            loadBmpImageToolStripMenuItem.Name = "loadBmpImageToolStripMenuItem";
            loadBmpImageToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.I;
            loadBmpImageToolStripMenuItem.Size = new Size(204, 22);
            loadBmpImageToolStripMenuItem.Text = "Load image";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(201, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(204, 22);
            exitToolStripMenuItem.Text = "&Exit";
            exitToolStripMenuItem.Click += File_Exit_Click;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { fillToolStripMenuItem, clearCellsToolStripMenuItem, clearUnselectedCellsToolStripMenuItem, toolStripSeparator3, copyToolStripMenuItem, cutToolStripMenuItem, pasteToolStripMenuItem, pasteMethodToolStripMenuItem, toolStripSeparator2, selectAllToolStripMenuItem, shrinkSelectionToolStripMenuItem, clearSelectionToolStripMenuItem, toolStripSeparator4, rotateToolStripMenuItem, flipUpDownToolStripMenuItem, flipLeftRightToolStripMenuItem, toolStripSeparator6, randomizeToolStripMenuItem, toolStripSeparator10, clearAllToolStripMenuItem });
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(42, 21);
            editToolStripMenuItem.Text = "&Edit";
            // 
            // fillToolStripMenuItem
            // 
            fillToolStripMenuItem.Name = "fillToolStripMenuItem";
            fillToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.F;
            fillToolStripMenuItem.Size = new Size(270, 22);
            fillToolStripMenuItem.Text = "&Fill";
            fillToolStripMenuItem.Click += Edit_FillSelectedRegion_Click;
            // 
            // clearCellsToolStripMenuItem
            // 
            clearCellsToolStripMenuItem.Name = "clearCellsToolStripMenuItem";
            clearCellsToolStripMenuItem.ShortcutKeys = Keys.Delete;
            clearCellsToolStripMenuItem.Size = new Size(270, 22);
            clearCellsToolStripMenuItem.Text = "&Clear selected cells";
            clearCellsToolStripMenuItem.Click += Edit_ClearSelected_Click;
            // 
            // clearUnselectedCellsToolStripMenuItem
            // 
            clearUnselectedCellsToolStripMenuItem.Name = "clearUnselectedCellsToolStripMenuItem";
            clearUnselectedCellsToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.Delete;
            clearUnselectedCellsToolStripMenuItem.Size = new Size(270, 22);
            clearUnselectedCellsToolStripMenuItem.Text = "C&lear unselected cells";
            clearUnselectedCellsToolStripMenuItem.Click += Edit_ClearUnselected_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(267, 6);
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.C;
            copyToolStripMenuItem.Size = new Size(270, 22);
            copyToolStripMenuItem.Text = "C&opy";
            copyToolStripMenuItem.Click += Edit_Copy_Click;
            // 
            // cutToolStripMenuItem
            // 
            cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            cutToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.X;
            cutToolStripMenuItem.Size = new Size(270, 22);
            cutToolStripMenuItem.Text = "C&ut";
            cutToolStripMenuItem.Click += Edit_Cut_Click;
            // 
            // pasteToolStripMenuItem
            // 
            pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            pasteToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.V;
            pasteToolStripMenuItem.Size = new Size(270, 22);
            pasteToolStripMenuItem.Text = "&Paste";
            pasteToolStripMenuItem.Click += Edit_Paste_Click;
            // 
            // pasteMethodToolStripMenuItem
            // 
            pasteMethodToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { pasteOverwrite, pasteOr, pasteAnd, pasteXor });
            pasteMethodToolStripMenuItem.Name = "pasteMethodToolStripMenuItem";
            pasteMethodToolStripMenuItem.Size = new Size(270, 22);
            pasteMethodToolStripMenuItem.Text = "Paste metho&d";
            // 
            // pasteOverwrite
            // 
            pasteOverwrite.Checked = true;
            pasteOverwrite.CheckState = CheckState.Checked;
            pasteOverwrite.Name = "pasteOverwrite";
            pasteOverwrite.Size = new Size(132, 22);
            pasteOverwrite.Text = "&Overwrite";
            pasteOverwrite.Click += Edit_ChangePasteMethods_Click;
            // 
            // pasteOr
            // 
            pasteOr.Name = "pasteOr";
            pasteOr.Size = new Size(132, 22);
            pasteOr.Text = "O&r";
            pasteOr.Click += Edit_ChangePasteMethods_Click;
            // 
            // pasteAnd
            // 
            pasteAnd.Name = "pasteAnd";
            pasteAnd.Size = new Size(132, 22);
            pasteAnd.Text = "&And";
            pasteAnd.Click += Edit_ChangePasteMethods_Click;
            // 
            // pasteXor
            // 
            pasteXor.Name = "pasteXor";
            pasteXor.Size = new Size(132, 22);
            pasteXor.Text = "&Xor";
            pasteXor.Click += Edit_ChangePasteMethods_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(267, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            selectAllToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.A;
            selectAllToolStripMenuItem.Size = new Size(270, 22);
            selectAllToolStripMenuItem.Text = "S&elect all";
            selectAllToolStripMenuItem.Click += Edit_SelectAll_Click;
            // 
            // shrinkSelectionToolStripMenuItem
            // 
            shrinkSelectionToolStripMenuItem.Name = "shrinkSelectionToolStripMenuItem";
            shrinkSelectionToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Alt | Keys.A;
            shrinkSelectionToolStripMenuItem.Size = new Size(270, 22);
            shrinkSelectionToolStripMenuItem.Text = "S&hrink selection";
            shrinkSelectionToolStripMenuItem.Click += Edit_ShrinkSelection_Click;
            // 
            // clearSelectionToolStripMenuItem
            // 
            clearSelectionToolStripMenuItem.Name = "clearSelectionToolStripMenuItem";
            clearSelectionToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.A;
            clearSelectionToolStripMenuItem.Size = new Size(270, 22);
            clearSelectionToolStripMenuItem.Text = "Clear &selection";
            clearSelectionToolStripMenuItem.Click += Edit_ClearSelection_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(267, 6);
            // 
            // rotateToolStripMenuItem
            // 
            rotateToolStripMenuItem.Name = "rotateToolStripMenuItem";
            rotateToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.R;
            rotateToolStripMenuItem.Size = new Size(270, 22);
            rotateToolStripMenuItem.Text = "Ro&tate";
            rotateToolStripMenuItem.Click += Edit_RotateSelected_Click;
            // 
            // flipUpDownToolStripMenuItem
            // 
            flipUpDownToolStripMenuItem.Name = "flipUpDownToolStripMenuItem";
            flipUpDownToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Alt | Keys.Down;
            flipUpDownToolStripMenuItem.Size = new Size(270, 22);
            flipUpDownToolStripMenuItem.Text = "Flip - UpDo&wn";
            flipUpDownToolStripMenuItem.Click += Edit_FlipUpDownSelected_Click;
            // 
            // flipLeftRightToolStripMenuItem
            // 
            flipLeftRightToolStripMenuItem.Name = "flipLeftRightToolStripMenuItem";
            flipLeftRightToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Alt | Keys.Right;
            flipLeftRightToolStripMenuItem.Size = new Size(270, 22);
            flipLeftRightToolStripMenuItem.Text = "Flip - LeftRi&ght";
            flipLeftRightToolStripMenuItem.Click += Edit_FlipLeftRight_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(267, 6);
            // 
            // randomizeToolStripMenuItem
            // 
            randomizeToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { randomFill25ToolStripMenuItem, randomFille50ToolStripMenuItem, berlinNoiseToolStripMenuItem });
            randomizeToolStripMenuItem.Name = "randomizeToolStripMenuItem";
            randomizeToolStripMenuItem.Size = new Size(270, 22);
            randomizeToolStripMenuItem.Text = "R&andomize";
            // 
            // randomFill25ToolStripMenuItem
            // 
            randomFill25ToolStripMenuItem.Name = "randomFill25ToolStripMenuItem";
            randomFill25ToolStripMenuItem.Size = new Size(192, 22);
            randomFill25ToolStripMenuItem.Text = "&Random Fill(25%)";
            randomFill25ToolStripMenuItem.Click += Edit_RandomFill25_Click;
            // 
            // randomFille50ToolStripMenuItem
            // 
            randomFille50ToolStripMenuItem.Name = "randomFille50ToolStripMenuItem";
            randomFille50ToolStripMenuItem.Size = new Size(192, 22);
            randomFille50ToolStripMenuItem.Text = "Random &Fill(50%)";
            randomFille50ToolStripMenuItem.Click += Edit_RandomFill50_Click;
            // 
            // berlinNoiseToolStripMenuItem
            // 
            berlinNoiseToolStripMenuItem.Name = "berlinNoiseToolStripMenuItem";
            berlinNoiseToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.B;
            berlinNoiseToolStripMenuItem.Size = new Size(192, 22);
            berlinNoiseToolStripMenuItem.Text = "&Berlin Noise";
            berlinNoiseToolStripMenuItem.Click += Edit_FillBerlinNoise_Click;
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new Size(267, 6);
            // 
            // clearAllToolStripMenuItem
            // 
            clearAllToolStripMenuItem.Name = "clearAllToolStripMenuItem";
            clearAllToolStripMenuItem.ShortcutKeys = Keys.F12;
            clearAllToolStripMenuItem.Size = new Size(270, 22);
            clearAllToolStripMenuItem.Text = "&Reset";
            clearAllToolStripMenuItem.Click += Edit_Reset_Click;
            // 
            // actionToolStripMenuItem
            // 
            actionToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { setRuleToolStripMenuItem, toolStripSeparator7, btnStartStop, nextGenerationToolStripMenuItem, toolStripSeparator5, speedUpToolStripMenuItem, speedDownToolStripMenuItem });
            actionToolStripMenuItem.Name = "actionToolStripMenuItem";
            actionToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Up;
            actionToolStripMenuItem.Size = new Size(56, 21);
            actionToolStripMenuItem.Text = "&Action";
            // 
            // setRuleToolStripMenuItem
            // 
            setRuleToolStripMenuItem.Name = "setRuleToolStripMenuItem";
            setRuleToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.D;
            setRuleToolStripMenuItem.Size = new Size(219, 22);
            setRuleToolStripMenuItem.Text = "Set &rule";
            setRuleToolStripMenuItem.Click += Action_SetRule_Click;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new Size(216, 6);
            // 
            // btnStartStop
            // 
            btnStartStop.Name = "btnStartStop";
            btnStartStop.ShortcutKeys = Keys.F5;
            btnStartStop.Size = new Size(219, 22);
            btnStartStop.Text = "&Start";
            btnStartStop.Click += Action_StartStop_Click;
            // 
            // nextGenerationToolStripMenuItem
            // 
            nextGenerationToolStripMenuItem.Name = "nextGenerationToolStripMenuItem";
            nextGenerationToolStripMenuItem.ShortcutKeys = Keys.F8;
            nextGenerationToolStripMenuItem.Size = new Size(219, 22);
            nextGenerationToolStripMenuItem.Text = "&Next generation";
            nextGenerationToolStripMenuItem.Click += Action_NextGeneration_Click;
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
            speedUpToolStripMenuItem.Text = "Speed &up";
            speedUpToolStripMenuItem.Click += Action_SpeedUp_Click;
            // 
            // speedDownToolStripMenuItem
            // 
            speedDownToolStripMenuItem.Name = "speedDownToolStripMenuItem";
            speedDownToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Down;
            speedDownToolStripMenuItem.Size = new Size(219, 22);
            speedDownToolStripMenuItem.Text = "Speed &down";
            speedDownToolStripMenuItem.Click += Action_SpeedDown_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { homeToolStripMenuItem, fitToolStripMenuItem, movePointToCenterToolStripMenuItem, moveSelectionToCenterToolStripMenuItem, moveToToolStripMenuItem, toolStripSeparator8, createNewViewToolStripMenuItem, toolStripSeparator9, zoomInToolStripMenuItem, zoomOutToolStripMenuItem, toolStripSeparator11, BtnSuspendView });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.ShortcutKeyDisplayString = "";
            viewToolStripMenuItem.Size = new Size(47, 21);
            viewToolStripMenuItem.Text = "&View";
            // 
            // homeToolStripMenuItem
            // 
            homeToolStripMenuItem.Name = "homeToolStripMenuItem";
            homeToolStripMenuItem.ShortcutKeyDisplayString = "";
            homeToolStripMenuItem.ShortcutKeys = Keys.F1;
            homeToolStripMenuItem.Size = new Size(302, 22);
            homeToolStripMenuItem.Text = "&Home";
            homeToolStripMenuItem.Click += View_MoveToHome_Click;
            // 
            // fitToolStripMenuItem
            // 
            fitToolStripMenuItem.Name = "fitToolStripMenuItem";
            fitToolStripMenuItem.ShortcutKeys = Keys.F2;
            fitToolStripMenuItem.Size = new Size(302, 22);
            fitToolStripMenuItem.Text = "&Fit";
            fitToolStripMenuItem.Click += View_MoveToCenterOfAllCells_Click;
            // 
            // movePointToCenterToolStripMenuItem
            // 
            movePointToCenterToolStripMenuItem.Name = "movePointToCenterToolStripMenuItem";
            movePointToCenterToolStripMenuItem.ShortcutKeys = Keys.F3;
            movePointToCenterToolStripMenuItem.Size = new Size(302, 22);
            movePointToCenterToolStripMenuItem.Text = "&Move mouse point to center";
            movePointToCenterToolStripMenuItem.Click += View_MovePointedCellToCenter_Click;
            // 
            // moveSelectionToCenterToolStripMenuItem
            // 
            moveSelectionToCenterToolStripMenuItem.Name = "moveSelectionToCenterToolStripMenuItem";
            moveSelectionToCenterToolStripMenuItem.ShortcutKeys = Keys.F4;
            moveSelectionToCenterToolStripMenuItem.Size = new Size(302, 22);
            moveSelectionToCenterToolStripMenuItem.Text = "Move &selection to center";
            moveSelectionToCenterToolStripMenuItem.Click += View_MoveToCenterOfSelection_Click;
            // 
            // moveToToolStripMenuItem
            // 
            moveToToolStripMenuItem.Name = "moveToToolStripMenuItem";
            moveToToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.F1;
            moveToToolStripMenuItem.Size = new Size(302, 22);
            moveToToolStripMenuItem.Text = "Move &to";
            moveToToolStripMenuItem.Click += View_MoveTo_Click;
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new Size(299, 6);
            // 
            // createNewViewToolStripMenuItem
            // 
            createNewViewToolStripMenuItem.Name = "createNewViewToolStripMenuItem";
            createNewViewToolStripMenuItem.Size = new Size(302, 22);
            createNewViewToolStripMenuItem.Text = "&Create a new view";
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new Size(299, 6);
            // 
            // zoomInToolStripMenuItem
            // 
            zoomInToolStripMenuItem.Name = "zoomInToolStripMenuItem";
            zoomInToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl + Plus, Mouse Wheel";
            zoomInToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Oemplus;
            zoomInToolStripMenuItem.Size = new Size(302, 22);
            zoomInToolStripMenuItem.Text = "Zoom &In";
            zoomInToolStripMenuItem.Click += View_ZoomIn_Click;
            // 
            // zoomOutToolStripMenuItem
            // 
            zoomOutToolStripMenuItem.Name = "zoomOutToolStripMenuItem";
            zoomOutToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl + Minus, Mouse Wheel";
            zoomOutToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.OemMinus;
            zoomOutToolStripMenuItem.Size = new Size(302, 22);
            zoomOutToolStripMenuItem.Text = "Zoom &Out";
            zoomOutToolStripMenuItem.Click += View_ZoomOut_Click;
            // 
            // toolStripSeparator11
            // 
            toolStripSeparator11.Name = "toolStripSeparator11";
            toolStripSeparator11.Size = new Size(299, 6);
            // 
            // BtnSuspendView
            // 
            BtnSuspendView.Name = "BtnSuspendView";
            BtnSuspendView.ShortcutKeys = Keys.Control | Keys.F11;
            BtnSuspendView.Size = new Size(302, 22);
            BtnSuspendView.Text = "S&uspend";
            BtnSuspendView.Click += View_Suspend_Click;
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
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripMenuItem randomizeToolStripMenuItem;
        private ToolStripMenuItem randomFill25ToolStripMenuItem;
        private ToolStripMenuItem randomFille50ToolStripMenuItem;
        private ToolStripMenuItem berlinNoiseToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripMenuItem setRuleToolStripMenuItem;
        private ToolStripMenuItem movePointToCenterToolStripMenuItem;
        private ToolStripMenuItem moveSelectionToCenterToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripMenuItem createNewViewToolStripMenuItem;
        private ToolStripMenuItem zoomInToolStripMenuItem;
        private ToolStripMenuItem zoomOutToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator9;
        private ToolStripMenuItem readGollyFileToolStripMenuItem;
        private ToolStripMenuItem loadBmpImageToolStripMenuItem;
        private ToolStripMenuItem moveToToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripSeparator toolStripSeparator11;
        private ToolStripMenuItem BtnSuspendView;
    }
}
