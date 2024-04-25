namespace gInk
{
    partial class PenModifyDlg
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
            this.lightnessColorSlider = new Cyotek.Windows.Forms.LightnessColorSlider();
            this.screenColorPicker = new Cyotek.Windows.Forms.ScreenColorPicker();
            this.colorWheel = new Cyotek.Windows.Forms.ColorWheel();
            this.colorEditor = new Cyotek.Windows.Forms.ColorEditor();
            this.colorGrid = new Cyotek.Windows.Forms.ColorGrid();
            this.previewPanel = new System.Windows.Forms.Panel();
            this.gpPenWidth = new System.Windows.Forms.Panel();
            this.pboxPenWidthIndicator = new System.Windows.Forms.PictureBox();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.OkBtn = new System.Windows.Forms.Button();
            this.colorEditorManager = new Cyotek.Windows.Forms.ColorEditorManager();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.FadingCB = new System.Windows.Forms.CheckBox();
            this.StyleStrokeRd = new System.Windows.Forms.RadioButton();
            this.DashStyleGrp = new System.Windows.Forms.GroupBox();
            this.panel6 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.StyleDashDotDotRd = new System.Windows.Forms.RadioButton();
            this.StyleDotRd = new System.Windows.Forms.RadioButton();
            this.StyleSolidRd = new System.Windows.Forms.RadioButton();
            this.StyleDashDotRd = new System.Windows.Forms.RadioButton();
            this.StyleDashRd = new System.Windows.Forms.RadioButton();
            this.SVSquare = new System.Windows.Forms.Panel();
            this.CursorHSI = new System.Windows.Forms.PictureBox();
            this.WidthTb = new System.Windows.Forms.TextBox();
            this.previewPanel.SuspendLayout();
            this.gpPenWidth.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pboxPenWidthIndicator)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.DashStyleGrp.SuspendLayout();
            this.SVSquare.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CursorHSI)).BeginInit();
            this.SuspendLayout();
            // 
            // lightnessColorSlider
            // 
            this.lightnessColorSlider.Location = new System.Drawing.Point(12, 191);
            this.lightnessColorSlider.Name = "lightnessColorSlider";
            this.lightnessColorSlider.Size = new System.Drawing.Size(192, 28);
            this.lightnessColorSlider.TabIndex = 27;
            // 
            // screenColorPicker
            // 
            this.screenColorPicker.Color = System.Drawing.Color.Black;
            this.screenColorPicker.Cursor = System.Windows.Forms.Cursors.Default;
            this.screenColorPicker.Location = new System.Drawing.Point(93, 303);
            this.screenColorPicker.Name = "screenColorPicker";
            this.screenColorPicker.Size = new System.Drawing.Size(84, 65);
            this.screenColorPicker.Zoom = 6;
            // 
            // colorWheel
            // 
            this.colorWheel.Color = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.colorWheel.Location = new System.Drawing.Point(12, 12);
            this.colorWheel.Name = "colorWheel";
            this.colorWheel.Size = new System.Drawing.Size(184, 182);
            this.colorWheel.TabIndex = 24;
            // 
            // colorEditor
            // 
            this.colorEditor.Color = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.colorEditor.Location = new System.Drawing.Point(418, 12);
            this.colorEditor.Name = "colorEditor";
            this.colorEditor.Size = new System.Drawing.Size(243, 236);
            this.colorEditor.TabIndex = 23;
            this.colorEditor.ColorChanged += new System.EventHandler(this.colorEditor_ColorChanged);
            // 
            // colorGrid
            // 
            this.colorGrid.AutoAddColors = false;
            this.colorGrid.CellBorderStyle = Cyotek.Windows.Forms.ColorCellBorderStyle.None;
            this.colorGrid.EditMode = Cyotek.Windows.Forms.ColorEditingMode.Both;
            this.colorGrid.Location = new System.Drawing.Point(12, 225);
            this.colorGrid.Name = "colorGrid";
            this.colorGrid.Padding = new System.Windows.Forms.Padding(0);
            this.colorGrid.Palette = Cyotek.Windows.Forms.ColorPalette.Paint;
            this.colorGrid.SelectedCellStyle = Cyotek.Windows.Forms.ColorGridSelectedCellStyle.Standard;
            this.colorGrid.ShowCustomColors = false;
            this.colorGrid.Size = new System.Drawing.Size(192, 72);
            this.colorGrid.Spacing = new System.Drawing.Size(0, 0);
            this.colorGrid.TabIndex = 25;
            // 
            // previewPanel
            // 
            this.previewPanel.BackgroundImage = global::gInk.Properties.Resources.cellbackground;
            this.previewPanel.Controls.Add(this.gpPenWidth);
            this.previewPanel.Location = new System.Drawing.Point(395, 291);
            this.previewPanel.Name = "previewPanel";
            this.previewPanel.Size = new System.Drawing.Size(160, 53);
            this.previewPanel.TabIndex = 26;
            // 
            // gpPenWidth
            // 
            this.gpPenWidth.BackColor = System.Drawing.Color.Transparent;
            this.gpPenWidth.BackgroundImage = global::gInk.Properties.Resources.penwidthpanel;
            this.gpPenWidth.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gpPenWidth.Controls.Add(this.pboxPenWidthIndicator);
            this.gpPenWidth.ForeColor = System.Drawing.SystemColors.ControlText;
            this.gpPenWidth.Location = new System.Drawing.Point(0, 0);
            this.gpPenWidth.Name = "gpPenWidth";
            this.gpPenWidth.Size = new System.Drawing.Size(159, 53);
            this.gpPenWidth.TabIndex = 30;
            this.gpPenWidth.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpPenWidth_MouseDown);
            this.gpPenWidth.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpPenWidth_MouseMove);
            this.gpPenWidth.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpPenWidth_MouseUp);
            // 
            // pboxPenWidthIndicator
            // 
            this.pboxPenWidthIndicator.BackColor = System.Drawing.Color.Orange;
            this.pboxPenWidthIndicator.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pboxPenWidthIndicator.Location = new System.Drawing.Point(0, 0);
            this.pboxPenWidthIndicator.Name = "pboxPenWidthIndicator";
            this.pboxPenWidthIndicator.Size = new System.Drawing.Size(5, 53);
            this.pboxPenWidthIndicator.TabIndex = 5;
            this.pboxPenWidthIndicator.TabStop = false;
            this.pboxPenWidthIndicator.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pboxPenWidthIndicator_MouseDown);
            this.pboxPenWidthIndicator.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pboxPenWidthIndicator_MouseMove);
            this.pboxPenWidthIndicator.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pboxPenWidthIndicator_MouseUp);
            this.pboxPenWidthIndicator.Move += new System.EventHandler(this.pboxPenWidthIndicator_Move);
            // 
            // CancelBtn
            // 
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.Location = new System.Drawing.Point(578, 370);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 23);
            this.CancelBtn.TabIndex = 28;
            this.CancelBtn.Text = "&Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            // 
            // OkBtn
            // 
            this.OkBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OkBtn.Location = new System.Drawing.Point(458, 370);
            this.OkBtn.Name = "OkBtn";
            this.OkBtn.Size = new System.Drawing.Size(75, 24);
            this.OkBtn.TabIndex = 29;
            this.OkBtn.Text = "&OK";
            this.OkBtn.UseVisualStyleBackColor = true;
            // 
            // colorEditorManager
            // 
            this.colorEditorManager.ColorEditor = this.colorEditor;
            this.colorEditorManager.ColorGrid = this.colorGrid;
            this.colorEditorManager.ColorWheel = this.colorWheel;
            this.colorEditorManager.LightnessColorSlider = this.lightnessColorSlider;
            this.colorEditorManager.ScreenColorPicker = this.screenColorPicker;
            this.colorEditorManager.ColorChanged += new System.EventHandler(this.colorEditorManager_ColorChanged);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::gInk.Properties.Resources.eyedropper;
            this.pictureBox1.Location = new System.Drawing.Point(46, 303);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(41, 35);
            this.pictureBox1.TabIndex = 31;
            this.pictureBox1.TabStop = false;
            // 
            // FadingCB
            // 
            this.FadingCB.AutoSize = true;
            this.FadingCB.Location = new System.Drawing.Point(458, 254);
            this.FadingCB.Name = "FadingCB";
            this.FadingCB.Size = new System.Drawing.Size(58, 17);
            this.FadingCB.TabIndex = 32;
            this.FadingCB.Text = "Fading";
            this.FadingCB.UseVisualStyleBackColor = true;
            // 
            // StyleStrokeRd
            // 
            this.StyleStrokeRd.AutoSize = true;
            this.StyleStrokeRd.Location = new System.Drawing.Point(6, 19);
            this.StyleStrokeRd.Name = "StyleStrokeRd";
            this.StyleStrokeRd.Size = new System.Drawing.Size(14, 13);
            this.StyleStrokeRd.TabIndex = 33;
            this.StyleStrokeRd.TabStop = true;
            this.StyleStrokeRd.UseVisualStyleBackColor = true;
            // 
            // DashStyleGrp
            // 
            this.DashStyleGrp.Controls.Add(this.panel6);
            this.DashStyleGrp.Controls.Add(this.panel5);
            this.DashStyleGrp.Controls.Add(this.panel4);
            this.DashStyleGrp.Controls.Add(this.panel3);
            this.DashStyleGrp.Controls.Add(this.panel2);
            this.DashStyleGrp.Controls.Add(this.panel1);
            this.DashStyleGrp.Controls.Add(this.StyleDashDotDotRd);
            this.DashStyleGrp.Controls.Add(this.StyleDotRd);
            this.DashStyleGrp.Controls.Add(this.StyleSolidRd);
            this.DashStyleGrp.Controls.Add(this.StyleDashDotRd);
            this.DashStyleGrp.Controls.Add(this.StyleDashRd);
            this.DashStyleGrp.Controls.Add(this.StyleStrokeRd);
            this.DashStyleGrp.Location = new System.Drawing.Point(561, 254);
            this.DashStyleGrp.Name = "DashStyleGrp";
            this.DashStyleGrp.Size = new System.Drawing.Size(100, 100);
            this.DashStyleGrp.TabIndex = 34;
            this.DashStyleGrp.TabStop = false;
            // 
            // panel6
            // 
            this.panel6.BackgroundImage = global::gInk.Properties.Resources.DashStyleDashDotDot;
            this.panel6.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel6.Location = new System.Drawing.Point(67, 77);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(25, 12);
            this.panel6.TabIndex = 37;
            // 
            // panel5
            // 
            this.panel5.BackgroundImage = global::gInk.Properties.Resources.DashStyleDashDot;
            this.panel5.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel5.Location = new System.Drawing.Point(22, 77);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(25, 12);
            this.panel5.TabIndex = 37;
            // 
            // panel4
            // 
            this.panel4.BackgroundImage = global::gInk.Properties.Resources.DashStyleDot;
            this.panel4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel4.Location = new System.Drawing.Point(67, 49);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(25, 12);
            this.panel4.TabIndex = 37;
            // 
            // panel3
            // 
            this.panel3.BackgroundImage = global::gInk.Properties.Resources.DashStyleDash;
            this.panel3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel3.Location = new System.Drawing.Point(22, 49);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(25, 12);
            this.panel3.TabIndex = 39;
            // 
            // panel2
            // 
            this.panel2.BackgroundImage = global::gInk.Properties.Resources.DashStyleSolid;
            this.panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel2.Location = new System.Drawing.Point(67, 20);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(25, 12);
            this.panel2.TabIndex = 36;
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = global::gInk.Properties.Resources.DashStyleStroke;
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel1.Location = new System.Drawing.Point(22, 19);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(25, 12);
            this.panel1.TabIndex = 35;
            // 
            // StyleDashDotDotRd
            // 
            this.StyleDashDotDotRd.AutoSize = true;
            this.StyleDashDotDotRd.Location = new System.Drawing.Point(51, 77);
            this.StyleDashDotDotRd.Name = "StyleDashDotDotRd";
            this.StyleDashDotDotRd.Size = new System.Drawing.Size(14, 13);
            this.StyleDashDotDotRd.TabIndex = 38;
            this.StyleDashDotDotRd.TabStop = true;
            this.StyleDashDotDotRd.UseVisualStyleBackColor = true;
            // 
            // StyleDotRd
            // 
            this.StyleDotRd.AutoSize = true;
            this.StyleDotRd.Location = new System.Drawing.Point(51, 48);
            this.StyleDotRd.Name = "StyleDotRd";
            this.StyleDotRd.Size = new System.Drawing.Size(14, 13);
            this.StyleDotRd.TabIndex = 37;
            this.StyleDotRd.TabStop = true;
            this.StyleDotRd.UseVisualStyleBackColor = true;
            // 
            // StyleSolidRd
            // 
            this.StyleSolidRd.AutoSize = true;
            this.StyleSolidRd.Location = new System.Drawing.Point(51, 19);
            this.StyleSolidRd.Name = "StyleSolidRd";
            this.StyleSolidRd.Size = new System.Drawing.Size(14, 13);
            this.StyleSolidRd.TabIndex = 36;
            this.StyleSolidRd.TabStop = true;
            this.StyleSolidRd.UseVisualStyleBackColor = true;
            // 
            // StyleDashDotRd
            // 
            this.StyleDashDotRd.AutoSize = true;
            this.StyleDashDotRd.Location = new System.Drawing.Point(6, 77);
            this.StyleDashDotRd.Name = "StyleDashDotRd";
            this.StyleDashDotRd.Size = new System.Drawing.Size(14, 13);
            this.StyleDashDotRd.TabIndex = 35;
            this.StyleDashDotRd.TabStop = true;
            this.StyleDashDotRd.UseVisualStyleBackColor = true;
            // 
            // StyleDashRd
            // 
            this.StyleDashRd.AutoSize = true;
            this.StyleDashRd.Location = new System.Drawing.Point(6, 48);
            this.StyleDashRd.Name = "StyleDashRd";
            this.StyleDashRd.Size = new System.Drawing.Size(14, 13);
            this.StyleDashRd.TabIndex = 34;
            this.StyleDashRd.TabStop = true;
            this.StyleDashRd.UseVisualStyleBackColor = true;
            // 
            // SVSquare
            // 
            this.SVSquare.Controls.Add(this.CursorHSI);
            this.SVSquare.Location = new System.Drawing.Point(210, 34);
            this.SVSquare.Name = "SVSquare";
            this.SVSquare.Size = new System.Drawing.Size(200, 200);
            this.SVSquare.TabIndex = 35;
            this.SVSquare.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SVSquare_MouseMove);
            this.SVSquare.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SVSquare_MouseMove);
            // 
            // CursorHSI
            // 
            this.CursorHSI.BackColor = System.Drawing.Color.Transparent;
            this.CursorHSI.BackgroundImage = global::gInk.Properties.Resources.cursorHSI;
            this.CursorHSI.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.CursorHSI.Location = new System.Drawing.Point(87, 105);
            this.CursorHSI.Name = "CursorHSI";
            this.CursorHSI.Size = new System.Drawing.Size(9, 9);
            this.CursorHSI.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.CursorHSI.TabIndex = 0;
            this.CursorHSI.TabStop = false;
            this.CursorHSI.MouseMove += new System.Windows.Forms.MouseEventHandler(this.CursorHSI_MouseMove);
            // 
            // WidthTb
            // 
            this.WidthTb.Location = new System.Drawing.Point(515, 271);
            this.WidthTb.Name = "WidthTb";
            this.WidthTb.Size = new System.Drawing.Size(39, 20);
            this.WidthTb.TabIndex = 36;
            this.WidthTb.Text = "000";
            this.WidthTb.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.WidthTb.Validating += new System.ComponentModel.CancelEventHandler(this.WidthTb_Validating);
            // 
            // PenModifyDlg
            // 
            this.AcceptButton = this.OkBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelBtn;
            this.ClientSize = new System.Drawing.Size(673, 402);
            this.Controls.Add(this.WidthTb);
            this.Controls.Add(this.SVSquare);
            this.Controls.Add(this.DashStyleGrp);
            this.Controls.Add(this.FadingCB);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.OkBtn);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.lightnessColorSlider);
            this.Controls.Add(this.screenColorPicker);
            this.Controls.Add(this.colorWheel);
            this.Controls.Add(this.colorEditor);
            this.Controls.Add(this.colorGrid);
            this.Controls.Add(this.previewPanel);
            this.Name = "PenModifyDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pen Adjustment";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PenModifyDlg_FormClosing);
            this.previewPanel.ResumeLayout(false);
            this.gpPenWidth.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pboxPenWidthIndicator)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.DashStyleGrp.ResumeLayout(false);
            this.DashStyleGrp.PerformLayout();
            this.SVSquare.ResumeLayout(false);
            this.SVSquare.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CursorHSI)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Cyotek.Windows.Forms.LightnessColorSlider lightnessColorSlider;
        private Cyotek.Windows.Forms.ScreenColorPicker screenColorPicker;
        private Cyotek.Windows.Forms.ColorWheel colorWheel;
        private Cyotek.Windows.Forms.ColorGrid colorGrid;
        private System.Windows.Forms.Panel previewPanel;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Button OkBtn;
        public System.Windows.Forms.Panel gpPenWidth;
        private System.Windows.Forms.PictureBox pboxPenWidthIndicator;
        public Cyotek.Windows.Forms.ColorEditorManager colorEditorManager;
        public Cyotek.Windows.Forms.ColorEditor colorEditor;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.CheckBox FadingCB;
        private System.Windows.Forms.RadioButton StyleStrokeRd;
        private System.Windows.Forms.GroupBox DashStyleGrp;
        private System.Windows.Forms.RadioButton StyleDashDotDotRd;
        private System.Windows.Forms.RadioButton StyleDotRd;
        private System.Windows.Forms.RadioButton StyleSolidRd;
        private System.Windows.Forms.RadioButton StyleDashDotRd;
        private System.Windows.Forms.RadioButton StyleDashRd;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel SVSquare;
        private System.Windows.Forms.PictureBox CursorHSI;
        private System.Windows.Forms.TextBox WidthTb;
    }
}