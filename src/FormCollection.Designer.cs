namespace gInk
{
	partial class FormCollection
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCollection));
            this.gpButtons = new System.Windows.Forms.Panel();
            this.btPageNext = new System.Windows.Forms.Button();
            this.btPagePrev = new System.Windows.Forms.Button();
            this.btExtraPens = new System.Windows.Forms.Button();
            this.btScaleRot = new System.Windows.Forms.Button();
            this.btLasso = new System.Windows.Forms.Button();
            this.btZoom = new System.Windows.Forms.Button();
            this.btSave = new System.Windows.Forms.Button();
            this.btLoad = new System.Windows.Forms.Button();
            this.btClip3 = new System.Windows.Forms.Button();
            this.btClip2 = new System.Windows.Forms.Button();
            this.btClip1 = new System.Windows.Forms.Button();
            this.btClipArt = new System.Windows.Forms.Button();
            this.btStop = new System.Windows.Forms.Button();
            this.btVideo = new System.Windows.Forms.Button();
            this.btInkVisible = new System.Windows.Forms.Button();
            this.btPan = new System.Windows.Forms.Button();
            this.btMagn = new System.Windows.Forms.Button();
            this.btDock = new System.Windows.Forms.Button();
            this.btPenWidth = new System.Windows.Forms.Button();
            this.btHand = new System.Windows.Forms.Button();
            this.btLine = new System.Windows.Forms.Button();
            this.btRect = new System.Windows.Forms.Button();
            this.btOval = new System.Windows.Forms.Button();
            this.btArrow = new System.Windows.Forms.Button();
            this.btNumb = new System.Windows.Forms.Button();
            this.btText = new System.Windows.Forms.Button();
            this.btEdit = new System.Windows.Forms.Button();
            this.btEraser = new System.Windows.Forms.Button();
            this.btSnap = new System.Windows.Forms.Button();
            this.btPointer = new System.Windows.Forms.Button();
            this.btClear = new System.Windows.Forms.Button();
            this.btUndo = new System.Windows.Forms.Button();
            this.tiSlide = new System.Windows.Forms.Timer(this.components);
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.Btn_SubTool1 = new System.Windows.Forms.Button();
            this.Btn_SubTool2 = new System.Windows.Forms.Button();
            this.Btn_SubTool4 = new System.Windows.Forms.Button();
            this.Btn_SubTool3 = new System.Windows.Forms.Button();
            this.Btn_SubTool6 = new System.Windows.Forms.Button();
            this.Btn_SubTool5 = new System.Windows.Forms.Button();
            this.Btn_SubTool0 = new System.Windows.Forms.Button();
            this.Btn_SubTool7 = new System.Windows.Forms.Button();
            this.Btn_SubToolClose = new System.Windows.Forms.Button();
            this.gpPenWidth = new System.Windows.Forms.Panel();
            this.pboxPenWidthIndicator = new System.Windows.Forms.PictureBox();
            this.longClickTimer = new System.Windows.Forms.Timer(this.components);
            this.FontDlg = new System.Windows.Forms.FontDialog();
            this.gpSubTools = new System.Windows.Forms.Panel();
            this.Btn_SubToolPin = new System.Windows.Forms.Button();
            this.gpButtons.SuspendLayout();
            this.gpPenWidth.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pboxPenWidthIndicator)).BeginInit();
            this.gpSubTools.SuspendLayout();
            this.SuspendLayout();
            // 
            // gpButtons
            // 
            this.gpButtons.BackColor = System.Drawing.Color.WhiteSmoke;
            this.gpButtons.Controls.Add(this.btPageNext);
            this.gpButtons.Controls.Add(this.btPagePrev);
            this.gpButtons.Controls.Add(this.btExtraPens);
            this.gpButtons.Controls.Add(this.btScaleRot);
            this.gpButtons.Controls.Add(this.btLasso);
            this.gpButtons.Controls.Add(this.btZoom);
            this.gpButtons.Controls.Add(this.btSave);
            this.gpButtons.Controls.Add(this.btLoad);
            this.gpButtons.Controls.Add(this.btClip3);
            this.gpButtons.Controls.Add(this.btClip2);
            this.gpButtons.Controls.Add(this.btClip1);
            this.gpButtons.Controls.Add(this.btClipArt);
            this.gpButtons.Controls.Add(this.btStop);
            this.gpButtons.Controls.Add(this.btVideo);
            this.gpButtons.Controls.Add(this.btInkVisible);
            this.gpButtons.Controls.Add(this.btPan);
            this.gpButtons.Controls.Add(this.btMagn);
            this.gpButtons.Controls.Add(this.btDock);
            this.gpButtons.Controls.Add(this.btPenWidth);
            this.gpButtons.Controls.Add(this.btHand);
            this.gpButtons.Controls.Add(this.btLine);
            this.gpButtons.Controls.Add(this.btRect);
            this.gpButtons.Controls.Add(this.btOval);
            this.gpButtons.Controls.Add(this.btArrow);
            this.gpButtons.Controls.Add(this.btNumb);
            this.gpButtons.Controls.Add(this.btText);
            this.gpButtons.Controls.Add(this.btEdit);
            this.gpButtons.Controls.Add(this.btEraser);
            this.gpButtons.Controls.Add(this.btSnap);
            this.gpButtons.Controls.Add(this.btPointer);
            this.gpButtons.Controls.Add(this.btClear);
            this.gpButtons.Controls.Add(this.btUndo);
            this.gpButtons.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.gpButtons.Location = new System.Drawing.Point(24, 48);
            this.gpButtons.Margin = new System.Windows.Forms.Padding(2);
            this.gpButtons.Name = "gpButtons";
            this.gpButtons.Size = new System.Drawing.Size(1466, 53);
            this.gpButtons.TabIndex = 3;
            this.gpButtons.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.gpButtons.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.gpButtons.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btPageNext
            // 
            this.btPageNext.BackColor = System.Drawing.Color.Transparent;
            this.btPageNext.BackgroundImage = global::gInk.Properties.Resources.PageNext;
            this.btPageNext.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btPageNext.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btPageNext.FlatAppearance.BorderSize = 0;
            this.btPageNext.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btPageNext.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btPageNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btPageNext.ForeColor = System.Drawing.Color.Transparent;
            this.btPageNext.Location = new System.Drawing.Point(1337, 0);
            this.btPageNext.Margin = new System.Windows.Forms.Padding(2);
            this.btPageNext.Name = "btPageNext";
            this.btPageNext.Size = new System.Drawing.Size(46, 46);
            this.btPageNext.TabIndex = 17;
            this.toolTip.SetToolTip(this.btPageNext, "Exit Drawing");
            this.btPageNext.UseVisualStyleBackColor = true;
            this.btPageNext.Click += new System.EventHandler(this.btPageNext_Click);
            // 
            // btPagePrev
            // 
            this.btPagePrev.BackColor = System.Drawing.Color.Transparent;
            this.btPagePrev.BackgroundImage = global::gInk.Properties.Resources.PagePrev;
            this.btPagePrev.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btPagePrev.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btPagePrev.FlatAppearance.BorderSize = 0;
            this.btPagePrev.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btPagePrev.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btPagePrev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btPagePrev.ForeColor = System.Drawing.Color.Transparent;
            this.btPagePrev.Location = new System.Drawing.Point(1306, 0);
            this.btPagePrev.Margin = new System.Windows.Forms.Padding(2);
            this.btPagePrev.Name = "btPagePrev";
            this.btPagePrev.Size = new System.Drawing.Size(46, 46);
            this.btPagePrev.TabIndex = 16;
            this.toolTip.SetToolTip(this.btPagePrev, "Exit Drawing");
            this.btPagePrev.UseVisualStyleBackColor = true;
            this.btPagePrev.Click += new System.EventHandler(this.btPagePrev_Click);
            // 
            // btExtraPens
            // 
            this.btExtraPens.BackColor = System.Drawing.Color.Transparent;
            this.btExtraPens.BackgroundImage = global::gInk.Properties.Resources.ExtraPens;
            this.btExtraPens.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btExtraPens.FlatAppearance.BorderSize = 0;
            this.btExtraPens.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btExtraPens.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btExtraPens.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btExtraPens.Location = new System.Drawing.Point(1437, 0);
            this.btExtraPens.Margin = new System.Windows.Forms.Padding(2);
            this.btExtraPens.Name = "btExtraPens";
            this.btExtraPens.Size = new System.Drawing.Size(12, 46);
            this.btExtraPens.TabIndex = 15;
            this.toolTip.SetToolTip(this.btExtraPens, "Snapshot");
            this.btExtraPens.UseVisualStyleBackColor = true;
            this.btExtraPens.Click += new System.EventHandler(this.ExtraPensBtn_Click);
            // 
            // btScaleRot
            // 
            this.btScaleRot.BackColor = System.Drawing.Color.Transparent;
            this.btScaleRot.BackgroundImage = global::gInk.Properties.Resources.scale;
            this.btScaleRot.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btScaleRot.FlatAppearance.BorderSize = 0;
            this.btScaleRot.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btScaleRot.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btScaleRot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btScaleRot.Location = new System.Drawing.Point(1387, 4);
            this.btScaleRot.Margin = new System.Windows.Forms.Padding(2);
            this.btScaleRot.Name = "btScaleRot";
            this.btScaleRot.Size = new System.Drawing.Size(46, 46);
            this.btScaleRot.TabIndex = 14;
            this.toolTip.SetToolTip(this.btScaleRot, "ScaleRot");
            this.btScaleRot.UseVisualStyleBackColor = true;
            this.btScaleRot.Click += new System.EventHandler(this.btScaleRot_Click);
            // 
            // btLasso
            // 
            this.btLasso.BackColor = System.Drawing.Color.Transparent;
            this.btLasso.BackgroundImage = global::gInk.Properties.Resources.lasso;
            this.btLasso.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btLasso.FlatAppearance.BorderSize = 0;
            this.btLasso.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btLasso.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btLasso.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btLasso.Location = new System.Drawing.Point(1196, 5);
            this.btLasso.Margin = new System.Windows.Forms.Padding(2);
            this.btLasso.Name = "btLasso";
            this.btLasso.Size = new System.Drawing.Size(46, 46);
            this.btLasso.TabIndex = 13;
            this.toolTip.SetToolTip(this.btLasso, "Pan");
            this.btLasso.UseVisualStyleBackColor = true;
            this.btLasso.Click += new System.EventHandler(this.btLasso_Click);
            this.btLasso.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btLasso.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btLasso.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btZoom
            // 
            this.btZoom.BackColor = System.Drawing.Color.Transparent;
            this.btZoom.BackgroundImage = global::gInk.Properties.Resources.Zoom;
            this.btZoom.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btZoom.FlatAppearance.BorderSize = 0;
            this.btZoom.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btZoom.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btZoom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btZoom.ForeColor = System.Drawing.Color.Transparent;
            this.btZoom.Location = new System.Drawing.Point(920, 3);
            this.btZoom.Margin = new System.Windows.Forms.Padding(2);
            this.btZoom.Name = "btZoom";
            this.btZoom.Size = new System.Drawing.Size(46, 46);
            this.btZoom.TabIndex = 12;
            this.toolTip.SetToolTip(this.btZoom, "Zoom");
            this.btZoom.UseVisualStyleBackColor = true;
            this.btZoom.Click += new System.EventHandler(this.btZoom_click);
            this.btZoom.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btZoom.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btZoom.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btSave
            // 
            this.btSave.BackColor = System.Drawing.Color.Transparent;
            this.btSave.BackgroundImage = global::gInk.Properties.Resources.save;
            this.btSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btSave.FlatAppearance.BorderSize = 0;
            this.btSave.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btSave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btSave.ForeColor = System.Drawing.Color.Transparent;
            this.btSave.Location = new System.Drawing.Point(88, 4);
            this.btSave.Margin = new System.Windows.Forms.Padding(2);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(46, 46);
            this.btSave.TabIndex = 11;
            this.toolTip.SetToolTip(this.btSave, "Eraser");
            this.btSave.UseVisualStyleBackColor = true;
            this.btSave.Click += new System.EventHandler(this.btSave_Click);
            this.btSave.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btSave.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btSave.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btLoad
            // 
            this.btLoad.BackColor = System.Drawing.Color.Transparent;
            this.btLoad.BackgroundImage = global::gInk.Properties.Resources.open;
            this.btLoad.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btLoad.FlatAppearance.BorderSize = 0;
            this.btLoad.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btLoad.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btLoad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btLoad.ForeColor = System.Drawing.Color.Transparent;
            this.btLoad.Location = new System.Drawing.Point(38, 4);
            this.btLoad.Margin = new System.Windows.Forms.Padding(2);
            this.btLoad.Name = "btLoad";
            this.btLoad.Size = new System.Drawing.Size(46, 46);
            this.btLoad.TabIndex = 10;
            this.toolTip.SetToolTip(this.btLoad, "Eraser");
            this.btLoad.UseVisualStyleBackColor = true;
            this.btLoad.Click += new System.EventHandler(this.btLoad_Click);
            this.btLoad.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btLoad.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btLoad.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btClip3
            // 
            this.btClip3.BackColor = System.Drawing.Color.Transparent;
            this.btClip3.BackgroundImage = global::gInk.Properties.Resources.tool_clipart;
            this.btClip3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btClip3.FlatAppearance.BorderColor = System.Drawing.Color.Orange;
            this.btClip3.FlatAppearance.BorderSize = 3;
            this.btClip3.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btClip3.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btClip3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btClip3.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btClip3.ForeColor = System.Drawing.Color.Orange;
            this.btClip3.Location = new System.Drawing.Point(864, 2);
            this.btClip3.Margin = new System.Windows.Forms.Padding(2);
            this.btClip3.Name = "btClip3";
            this.btClip3.Size = new System.Drawing.Size(44, 49);
            this.btClip3.TabIndex = 9;
            this.btClip3.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
            this.toolTip.SetToolTip(this.btClip3, "Hand Drawing");
            this.btClip3.UseVisualStyleBackColor = false;
            this.btClip3.Click += new System.EventHandler(this.btTool_Click);
            this.btClip3.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btAllButtons_MouseDown);
            this.btClip3.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btClip3.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btAllButtons_MouseUp);
            // 
            // btClip2
            // 
            this.btClip2.BackColor = System.Drawing.Color.Transparent;
            this.btClip2.BackgroundImage = global::gInk.Properties.Resources.tool_clipart;
            this.btClip2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btClip2.FlatAppearance.BorderColor = System.Drawing.Color.Orange;
            this.btClip2.FlatAppearance.BorderSize = 3;
            this.btClip2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btClip2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btClip2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btClip2.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btClip2.ForeColor = System.Drawing.Color.Orange;
            this.btClip2.Location = new System.Drawing.Point(817, 2);
            this.btClip2.Margin = new System.Windows.Forms.Padding(2);
            this.btClip2.Name = "btClip2";
            this.btClip2.Size = new System.Drawing.Size(43, 49);
            this.btClip2.TabIndex = 8;
            this.btClip2.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
            this.toolTip.SetToolTip(this.btClip2, "Hand Drawing");
            this.btClip2.UseVisualStyleBackColor = false;
            this.btClip2.Click += new System.EventHandler(this.btTool_Click);
            this.btClip2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btAllButtons_MouseDown);
            this.btClip2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btClip2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btAllButtons_MouseUp);
            // 
            // btClip1
            // 
            this.btClip1.BackColor = System.Drawing.Color.Transparent;
            this.btClip1.BackgroundImage = global::gInk.Properties.Resources.tool_clipart;
            this.btClip1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btClip1.FlatAppearance.BorderColor = System.Drawing.Color.Orange;
            this.btClip1.FlatAppearance.BorderSize = 3;
            this.btClip1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btClip1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btClip1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btClip1.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btClip1.ForeColor = System.Drawing.Color.Orange;
            this.btClip1.Location = new System.Drawing.Point(767, 2);
            this.btClip1.Margin = new System.Windows.Forms.Padding(2);
            this.btClip1.Name = "btClip1";
            this.btClip1.Size = new System.Drawing.Size(46, 49);
            this.btClip1.TabIndex = 7;
            this.btClip1.Text = "S";
            this.btClip1.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
            this.toolTip.SetToolTip(this.btClip1, "Hand Drawing");
            this.btClip1.UseVisualStyleBackColor = false;
            this.btClip1.Click += new System.EventHandler(this.btTool_Click);
            this.btClip1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btAllButtons_MouseDown);
            this.btClip1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btClip1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btAllButtons_MouseUp);
            // 
            // btClipArt
            // 
            this.btClipArt.BackColor = System.Drawing.Color.Transparent;
            this.btClipArt.BackgroundImage = global::gInk.Properties.Resources.tool_clipart;
            this.btClipArt.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btClipArt.FlatAppearance.BorderSize = 0;
            this.btClipArt.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btClipArt.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btClipArt.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btClipArt.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btClipArt.ForeColor = System.Drawing.Color.Orange;
            this.btClipArt.Location = new System.Drawing.Point(721, 3);
            this.btClipArt.Margin = new System.Windows.Forms.Padding(2);
            this.btClipArt.Name = "btClipArt";
            this.btClipArt.Size = new System.Drawing.Size(46, 46);
            this.btClipArt.TabIndex = 6;
            this.btClipArt.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
            this.toolTip.SetToolTip(this.btClipArt, "Hand Drawing");
            this.btClipArt.UseVisualStyleBackColor = false;
            this.btClipArt.Click += new System.EventHandler(this.btTool_Click);
            this.btClipArt.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btAllButtons_MouseDown);
            this.btClipArt.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btClipArt.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btAllButtons_MouseUp);
            // 
            // btStop
            // 
            this.btStop.BackColor = System.Drawing.Color.Transparent;
            this.btStop.BackgroundImage = global::gInk.Properties.Resources.exit;
            this.btStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btStop.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btStop.FlatAppearance.BorderSize = 0;
            this.btStop.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btStop.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btStop.ForeColor = System.Drawing.Color.Transparent;
            this.btStop.Location = new System.Drawing.Point(1283, 0);
            this.btStop.Margin = new System.Windows.Forms.Padding(2);
            this.btStop.Name = "btStop";
            this.btStop.Size = new System.Drawing.Size(46, 46);
            this.btStop.TabIndex = 5;
            this.toolTip.SetToolTip(this.btStop, "Exit Drawing");
            this.btStop.UseVisualStyleBackColor = true;
            this.btStop.Click += new System.EventHandler(this.btStop_Click);
            this.btStop.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btStop.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btStop.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btVideo
            // 
            this.btVideo.BackColor = System.Drawing.Color.Transparent;
            this.btVideo.BackgroundImage = global::gInk.Properties.Resources.VidStop;
            this.btVideo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btVideo.FlatAppearance.BorderSize = 0;
            this.btVideo.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btVideo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btVideo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btVideo.Location = new System.Drawing.Point(216, 3);
            this.btVideo.Margin = new System.Windows.Forms.Padding(2);
            this.btVideo.Name = "btVideo";
            this.btVideo.Size = new System.Drawing.Size(46, 46);
            this.btVideo.TabIndex = 4;
            this.toolTip.SetToolTip(this.btVideo, "Video Recording");
            this.btVideo.UseVisualStyleBackColor = true;
            this.btVideo.Click += new System.EventHandler(this.btVideo_Click);
            this.btVideo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btVideo.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btVideo.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btInkVisible
            // 
            this.btInkVisible.BackColor = System.Drawing.Color.Transparent;
            this.btInkVisible.BackgroundImage = global::gInk.Properties.Resources.visible;
            this.btInkVisible.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btInkVisible.FlatAppearance.BorderSize = 0;
            this.btInkVisible.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btInkVisible.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btInkVisible.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btInkVisible.Location = new System.Drawing.Point(1256, 3);
            this.btInkVisible.Margin = new System.Windows.Forms.Padding(2);
            this.btInkVisible.Name = "btInkVisible";
            this.btInkVisible.Size = new System.Drawing.Size(46, 46);
            this.btInkVisible.TabIndex = 3;
            this.toolTip.SetToolTip(this.btInkVisible, "Ink visible");
            this.btInkVisible.UseVisualStyleBackColor = true;
            this.btInkVisible.Click += new System.EventHandler(this.btInkVisible_Click);
            this.btInkVisible.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btInkVisible.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btInkVisible.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btPan
            // 
            this.btPan.BackColor = System.Drawing.Color.Transparent;
            this.btPan.BackgroundImage = global::gInk.Properties.Resources.pan;
            this.btPan.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btPan.FlatAppearance.BorderSize = 0;
            this.btPan.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btPan.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btPan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btPan.Location = new System.Drawing.Point(1219, 4);
            this.btPan.Margin = new System.Windows.Forms.Padding(2);
            this.btPan.Name = "btPan";
            this.btPan.Size = new System.Drawing.Size(46, 46);
            this.btPan.TabIndex = 2;
            this.toolTip.SetToolTip(this.btPan, "Pan");
            this.btPan.UseVisualStyleBackColor = true;
            this.btPan.Click += new System.EventHandler(this.btPan_Click);
            this.btPan.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btPan.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btPan.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btMagn
            // 
            this.btMagn.BackColor = System.Drawing.Color.Transparent;
            this.btMagn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btMagn.FlatAppearance.BorderSize = 0;
            this.btMagn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btMagn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btMagn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btMagn.Location = new System.Drawing.Point(1125, 5);
            this.btMagn.Margin = new System.Windows.Forms.Padding(2);
            this.btMagn.Name = "btMagn";
            this.btMagn.Size = new System.Drawing.Size(46, 46);
            this.btMagn.TabIndex = 2;
            this.toolTip.SetToolTip(this.btMagn, "Magnetic drawing");
            this.btMagn.UseVisualStyleBackColor = true;
            this.btMagn.Click += new System.EventHandler(this.btMagn_Click);
            this.btMagn.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btMagn.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btMagn.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btDock
            // 
            this.btDock.BackColor = System.Drawing.Color.Transparent;
            this.btDock.BackgroundImage = global::gInk.Properties.Resources.dock;
            this.btDock.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btDock.FlatAppearance.BorderSize = 0;
            this.btDock.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btDock.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btDock.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btDock.Location = new System.Drawing.Point(0, 3);
            this.btDock.Margin = new System.Windows.Forms.Padding(2);
            this.btDock.Name = "btDock";
            this.btDock.Size = new System.Drawing.Size(34, 46);
            this.btDock.TabIndex = 0;
            this.toolTip.SetToolTip(this.btDock, "Dock / Undock");
            this.btDock.UseVisualStyleBackColor = true;
            this.btDock.Click += new System.EventHandler(this.btDock_Click);
            this.btDock.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btDock.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btDock.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btPenWidth
            // 
            this.btPenWidth.BackColor = System.Drawing.Color.Transparent;
            this.btPenWidth.BackgroundImage = global::gInk.Properties.Resources.penwidth;
            this.btPenWidth.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btPenWidth.FlatAppearance.BorderSize = 0;
            this.btPenWidth.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btPenWidth.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btPenWidth.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btPenWidth.Location = new System.Drawing.Point(266, 3);
            this.btPenWidth.Margin = new System.Windows.Forms.Padding(2);
            this.btPenWidth.Name = "btPenWidth";
            this.btPenWidth.Size = new System.Drawing.Size(46, 46);
            this.btPenWidth.TabIndex = 0;
            this.toolTip.SetToolTip(this.btPenWidth, "Pen width");
            this.btPenWidth.UseVisualStyleBackColor = true;
            this.btPenWidth.Click += new System.EventHandler(this.btPenWidth_Click);
            this.btPenWidth.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btPenWidth.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btPenWidth.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btHand
            // 
            this.btHand.BackColor = System.Drawing.Color.Transparent;
            this.btHand.BackgroundImage = global::gInk.Properties.Resources.tool_hand;
            this.btHand.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btHand.FlatAppearance.BorderSize = 0;
            this.btHand.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btHand.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btHand.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btHand.ForeColor = System.Drawing.Color.Transparent;
            this.btHand.Location = new System.Drawing.Point(316, 4);
            this.btHand.Margin = new System.Windows.Forms.Padding(2);
            this.btHand.Name = "btHand";
            this.btHand.Size = new System.Drawing.Size(46, 46);
            this.btHand.TabIndex = 0;
            this.toolTip.SetToolTip(this.btHand, "Hand Drawing");
            this.btHand.UseVisualStyleBackColor = true;
            this.btHand.Click += new System.EventHandler(this.btTool_Click);
            this.btHand.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btHand.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btHand.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btLine
            // 
            this.btLine.BackColor = System.Drawing.Color.Transparent;
            this.btLine.BackgroundImage = global::gInk.Properties.Resources.tool_line;
            this.btLine.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btLine.FlatAppearance.BorderSize = 0;
            this.btLine.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btLine.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btLine.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btLine.ForeColor = System.Drawing.Color.Transparent;
            this.btLine.Location = new System.Drawing.Point(364, 4);
            this.btLine.Margin = new System.Windows.Forms.Padding(2);
            this.btLine.Name = "btLine";
            this.btLine.Size = new System.Drawing.Size(46, 46);
            this.btLine.TabIndex = 0;
            this.toolTip.SetToolTip(this.btLine, "Line Drawing");
            this.btLine.UseVisualStyleBackColor = true;
            this.btLine.Click += new System.EventHandler(this.btTool_Click);
            this.btLine.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btLine.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btLine.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btRect
            // 
            this.btRect.BackColor = System.Drawing.Color.Transparent;
            this.btRect.BackgroundImage = global::gInk.Properties.Resources.tool_rect;
            this.btRect.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btRect.FlatAppearance.BorderSize = 0;
            this.btRect.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btRect.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btRect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btRect.ForeColor = System.Drawing.Color.Transparent;
            this.btRect.Location = new System.Drawing.Point(421, 5);
            this.btRect.Margin = new System.Windows.Forms.Padding(2);
            this.btRect.Name = "btRect";
            this.btRect.Size = new System.Drawing.Size(46, 46);
            this.btRect.TabIndex = 0;
            this.toolTip.SetToolTip(this.btRect, "Rect Shape");
            this.btRect.UseVisualStyleBackColor = true;
            this.btRect.Click += new System.EventHandler(this.btTool_Click);
            this.btRect.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btRect.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btRect.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btOval
            // 
            this.btOval.BackColor = System.Drawing.Color.Transparent;
            this.btOval.BackgroundImage = global::gInk.Properties.Resources.tool_oval;
            this.btOval.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btOval.FlatAppearance.BorderSize = 0;
            this.btOval.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btOval.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btOval.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btOval.ForeColor = System.Drawing.Color.Transparent;
            this.btOval.Location = new System.Drawing.Point(471, 2);
            this.btOval.Margin = new System.Windows.Forms.Padding(2);
            this.btOval.Name = "btOval";
            this.btOval.Size = new System.Drawing.Size(46, 46);
            this.btOval.TabIndex = 0;
            this.toolTip.SetToolTip(this.btOval, "Oval Shape");
            this.btOval.UseVisualStyleBackColor = true;
            this.btOval.Click += new System.EventHandler(this.btTool_Click);
            this.btOval.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btOval.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btOval.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btArrow
            // 
            this.btArrow.BackColor = System.Drawing.Color.Transparent;
            this.btArrow.BackgroundImage = global::gInk.Properties.Resources.tool_stAr;
            this.btArrow.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btArrow.FlatAppearance.BorderSize = 0;
            this.btArrow.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btArrow.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btArrow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btArrow.ForeColor = System.Drawing.Color.Transparent;
            this.btArrow.Location = new System.Drawing.Point(521, 2);
            this.btArrow.Margin = new System.Windows.Forms.Padding(2);
            this.btArrow.Name = "btArrow";
            this.btArrow.Size = new System.Drawing.Size(46, 46);
            this.btArrow.TabIndex = 0;
            this.toolTip.SetToolTip(this.btArrow, "Starting or Ending Arrow Shape");
            this.btArrow.UseVisualStyleBackColor = true;
            this.btArrow.Click += new System.EventHandler(this.btTool_Click);
            this.btArrow.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btArrow.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btArrow.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btNumb
            // 
            this.btNumb.BackColor = System.Drawing.Color.Transparent;
            this.btNumb.BackgroundImage = global::gInk.Properties.Resources.tool_numb;
            this.btNumb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btNumb.FlatAppearance.BorderSize = 0;
            this.btNumb.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btNumb.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btNumb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btNumb.ForeColor = System.Drawing.Color.Transparent;
            this.btNumb.Location = new System.Drawing.Point(571, 3);
            this.btNumb.Margin = new System.Windows.Forms.Padding(2);
            this.btNumb.Name = "btNumb";
            this.btNumb.Size = new System.Drawing.Size(46, 46);
            this.btNumb.TabIndex = 0;
            this.toolTip.SetToolTip(this.btNumb, "Add number tag");
            this.btNumb.UseVisualStyleBackColor = true;
            this.btNumb.Click += new System.EventHandler(this.btTool_Click);
            this.btNumb.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btNumb.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btNumb.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btText
            // 
            this.btText.BackColor = System.Drawing.Color.Transparent;
            this.btText.BackgroundImage = global::gInk.Properties.Resources.tool_txtL;
            this.btText.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btText.FlatAppearance.BorderSize = 0;
            this.btText.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btText.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btText.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btText.ForeColor = System.Drawing.Color.Transparent;
            this.btText.Location = new System.Drawing.Point(621, 0);
            this.btText.Margin = new System.Windows.Forms.Padding(2);
            this.btText.Name = "btText";
            this.btText.Size = new System.Drawing.Size(46, 46);
            this.btText.TabIndex = 0;
            this.toolTip.SetToolTip(this.btText, "Add Left aligned text");
            this.btText.UseVisualStyleBackColor = true;
            this.btText.Click += new System.EventHandler(this.btTool_Click);
            this.btText.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btText.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btText.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btEdit
            // 
            this.btEdit.BackColor = System.Drawing.Color.Transparent;
            this.btEdit.BackgroundImage = global::gInk.Properties.Resources.tool_edit;
            this.btEdit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btEdit.FlatAppearance.BorderSize = 0;
            this.btEdit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btEdit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btEdit.ForeColor = System.Drawing.Color.Transparent;
            this.btEdit.Location = new System.Drawing.Point(671, 2);
            this.btEdit.Margin = new System.Windows.Forms.Padding(2);
            this.btEdit.Name = "btEdit";
            this.btEdit.Size = new System.Drawing.Size(46, 46);
            this.btEdit.TabIndex = 0;
            this.toolTip.SetToolTip(this.btEdit, "Edit Text");
            this.btEdit.UseVisualStyleBackColor = true;
            this.btEdit.Click += new System.EventHandler(this.btTool_Click);
            this.btEdit.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btEdit.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btEdit.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btEraser
            // 
            this.btEraser.BackColor = System.Drawing.Color.Transparent;
            this.btEraser.BackgroundImage = global::gInk.Properties.Resources.eraser;
            this.btEraser.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btEraser.FlatAppearance.BorderSize = 0;
            this.btEraser.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btEraser.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btEraser.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btEraser.ForeColor = System.Drawing.Color.Transparent;
            this.btEraser.Location = new System.Drawing.Point(166, 3);
            this.btEraser.Margin = new System.Windows.Forms.Padding(2);
            this.btEraser.Name = "btEraser";
            this.btEraser.Size = new System.Drawing.Size(46, 46);
            this.btEraser.TabIndex = 0;
            this.toolTip.SetToolTip(this.btEraser, "Eraser");
            this.btEraser.UseVisualStyleBackColor = true;
            this.btEraser.Click += new System.EventHandler(this.btEraser_Click);
            this.btEraser.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btEraser.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btEraser.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btSnap
            // 
            this.btSnap.BackColor = System.Drawing.Color.Transparent;
            this.btSnap.BackgroundImage = global::gInk.Properties.Resources.snap;
            this.btSnap.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btSnap.FlatAppearance.BorderSize = 0;
            this.btSnap.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btSnap.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btSnap.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btSnap.Location = new System.Drawing.Point(1020, 4);
            this.btSnap.Margin = new System.Windows.Forms.Padding(2);
            this.btSnap.Name = "btSnap";
            this.btSnap.Size = new System.Drawing.Size(46, 46);
            this.btSnap.TabIndex = 0;
            this.toolTip.SetToolTip(this.btSnap, "Snapshot");
            this.btSnap.UseVisualStyleBackColor = true;
            this.btSnap.Click += new System.EventHandler(this.btSnap_Click);
            this.btSnap.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btSnap.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btSnap.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btPointer
            // 
            this.btPointer.BackColor = System.Drawing.Color.Transparent;
            this.btPointer.BackgroundImage = global::gInk.Properties.Resources.pointer;
            this.btPointer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btPointer.FlatAppearance.BorderSize = 0;
            this.btPointer.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btPointer.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btPointer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btPointer.ForeColor = System.Drawing.Color.Transparent;
            this.btPointer.Location = new System.Drawing.Point(970, 4);
            this.btPointer.Margin = new System.Windows.Forms.Padding(2);
            this.btPointer.Name = "btPointer";
            this.btPointer.Size = new System.Drawing.Size(46, 46);
            this.btPointer.TabIndex = 0;
            this.toolTip.SetToolTip(this.btPointer, "Mouse pointer");
            this.btPointer.UseVisualStyleBackColor = true;
            this.btPointer.Click += new System.EventHandler(this.btPointer_Click);
            this.btPointer.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btPointer.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btPointer.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btClear
            // 
            this.btClear.BackColor = System.Drawing.Color.Transparent;
            this.btClear.BackgroundImage = global::gInk.Properties.Resources.garbage;
            this.btClear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btClear.FlatAppearance.BorderSize = 0;
            this.btClear.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btClear.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btClear.Location = new System.Drawing.Point(1120, 3);
            this.btClear.Margin = new System.Windows.Forms.Padding(2);
            this.btClear.Name = "btClear";
            this.btClear.Size = new System.Drawing.Size(46, 46);
            this.btClear.TabIndex = 1;
            this.toolTip.SetToolTip(this.btClear, "Clear");
            this.btClear.UseVisualStyleBackColor = true;
            this.btClear.RightToLeftChanged += new System.EventHandler(this.btClear_RightToLeftChanged);
            this.btClear.Click += new System.EventHandler(this.btClear_Click);
            this.btClear.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btClear.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btClear.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // btUndo
            // 
            this.btUndo.BackColor = System.Drawing.Color.Transparent;
            this.btUndo.BackgroundImage = global::gInk.Properties.Resources.undo;
            this.btUndo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btUndo.FlatAppearance.BorderSize = 0;
            this.btUndo.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btUndo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btUndo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btUndo.Location = new System.Drawing.Point(1070, 4);
            this.btUndo.Margin = new System.Windows.Forms.Padding(2);
            this.btUndo.Name = "btUndo";
            this.btUndo.Size = new System.Drawing.Size(46, 46);
            this.btUndo.TabIndex = 1;
            this.toolTip.SetToolTip(this.btUndo, "Undo");
            this.btUndo.UseVisualStyleBackColor = true;
            this.btUndo.Click += new System.EventHandler(this.btUndo_Click);
            this.btUndo.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseDown);
            this.btUndo.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseMove);
            this.btUndo.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpButtons_MouseUp);
            // 
            // tiSlide
            // 
            this.tiSlide.Interval = 15;
            this.tiSlide.Tick += new System.EventHandler(this.tiSlide_Tick);
            // 
            // Btn_SubTool1
            // 
            this.Btn_SubTool1.BackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool1.BackgroundImage = global::gInk.Properties.Resources.tool_clipart;
            this.Btn_SubTool1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Btn_SubTool1.FlatAppearance.BorderColor = System.Drawing.Color.Orange;
            this.Btn_SubTool1.FlatAppearance.BorderSize = 0;
            this.Btn_SubTool1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_SubTool1.ForeColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool1.Location = new System.Drawing.Point(51, 2);
            this.Btn_SubTool1.Margin = new System.Windows.Forms.Padding(2);
            this.Btn_SubTool1.Name = "Btn_SubTool1";
            this.Btn_SubTool1.Size = new System.Drawing.Size(46, 46);
            this.Btn_SubTool1.TabIndex = 13;
            this.Btn_SubTool1.Tag = 1;
            this.toolTip.SetToolTip(this.Btn_SubTool1, "SubTool1");
            this.Btn_SubTool1.UseVisualStyleBackColor = true;
            this.Btn_SubTool1.Click += new System.EventHandler(this.SubTool_Click);
            this.Btn_SubTool1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseDown);
            this.Btn_SubTool1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseMove);
            this.Btn_SubTool1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseUp);
            // 
            // Btn_SubTool2
            // 
            this.Btn_SubTool2.BackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool2.BackgroundImage = global::gInk.Properties.Resources.tool_clipart;
            this.Btn_SubTool2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Btn_SubTool2.FlatAppearance.BorderColor = System.Drawing.Color.Orange;
            this.Btn_SubTool2.FlatAppearance.BorderSize = 0;
            this.Btn_SubTool2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_SubTool2.ForeColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool2.Location = new System.Drawing.Point(99, 2);
            this.Btn_SubTool2.Margin = new System.Windows.Forms.Padding(2);
            this.Btn_SubTool2.Name = "Btn_SubTool2";
            this.Btn_SubTool2.Size = new System.Drawing.Size(46, 46);
            this.Btn_SubTool2.TabIndex = 14;
            this.Btn_SubTool2.Tag = 2;
            this.toolTip.SetToolTip(this.Btn_SubTool2, "SubTool2");
            this.Btn_SubTool2.UseVisualStyleBackColor = true;
            this.Btn_SubTool2.Click += new System.EventHandler(this.SubTool_Click);
            this.Btn_SubTool2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseDown);
            this.Btn_SubTool2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseMove);
            this.Btn_SubTool2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseUp);
            // 
            // Btn_SubTool4
            // 
            this.Btn_SubTool4.BackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool4.BackgroundImage = global::gInk.Properties.Resources.tool_clipart;
            this.Btn_SubTool4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Btn_SubTool4.FlatAppearance.BorderColor = System.Drawing.Color.Orange;
            this.Btn_SubTool4.FlatAppearance.BorderSize = 0;
            this.Btn_SubTool4.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool4.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_SubTool4.ForeColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool4.Location = new System.Drawing.Point(195, 2);
            this.Btn_SubTool4.Margin = new System.Windows.Forms.Padding(2);
            this.Btn_SubTool4.Name = "Btn_SubTool4";
            this.Btn_SubTool4.Size = new System.Drawing.Size(46, 46);
            this.Btn_SubTool4.TabIndex = 16;
            this.Btn_SubTool4.TabStop = false;
            this.Btn_SubTool4.Tag = 4;
            this.toolTip.SetToolTip(this.Btn_SubTool4, "SubTool4");
            this.Btn_SubTool4.UseVisualStyleBackColor = true;
            this.Btn_SubTool4.Click += new System.EventHandler(this.SubTool_Click);
            this.Btn_SubTool4.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseDown);
            this.Btn_SubTool4.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseMove);
            this.Btn_SubTool4.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseUp);
            // 
            // Btn_SubTool3
            // 
            this.Btn_SubTool3.BackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool3.BackgroundImage = global::gInk.Properties.Resources.tool_clipart;
            this.Btn_SubTool3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Btn_SubTool3.FlatAppearance.BorderColor = System.Drawing.Color.Orange;
            this.Btn_SubTool3.FlatAppearance.BorderSize = 3;
            this.Btn_SubTool3.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool3.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_SubTool3.ForeColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool3.Location = new System.Drawing.Point(147, 2);
            this.Btn_SubTool3.Margin = new System.Windows.Forms.Padding(2);
            this.Btn_SubTool3.Name = "Btn_SubTool3";
            this.Btn_SubTool3.Size = new System.Drawing.Size(46, 46);
            this.Btn_SubTool3.TabIndex = 15;
            this.Btn_SubTool3.Tag = 3;
            this.toolTip.SetToolTip(this.Btn_SubTool3, "SubTool3");
            this.Btn_SubTool3.UseVisualStyleBackColor = true;
            this.Btn_SubTool3.Click += new System.EventHandler(this.SubTool_Click);
            this.Btn_SubTool3.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseDown);
            this.Btn_SubTool3.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseMove);
            this.Btn_SubTool3.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseUp);
            // 
            // Btn_SubTool6
            // 
            this.Btn_SubTool6.BackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool6.BackgroundImage = global::gInk.Properties.Resources.tool_clipart;
            this.Btn_SubTool6.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Btn_SubTool6.FlatAppearance.BorderColor = System.Drawing.Color.Orange;
            this.Btn_SubTool6.FlatAppearance.BorderSize = 0;
            this.Btn_SubTool6.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool6.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_SubTool6.ForeColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool6.Location = new System.Drawing.Point(291, 2);
            this.Btn_SubTool6.Margin = new System.Windows.Forms.Padding(2);
            this.Btn_SubTool6.Name = "Btn_SubTool6";
            this.Btn_SubTool6.Size = new System.Drawing.Size(46, 46);
            this.Btn_SubTool6.TabIndex = 18;
            this.Btn_SubTool6.Tag = 6;
            this.toolTip.SetToolTip(this.Btn_SubTool6, "SubTool6");
            this.Btn_SubTool6.UseVisualStyleBackColor = true;
            this.Btn_SubTool6.Click += new System.EventHandler(this.SubTool_Click);
            this.Btn_SubTool6.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseDown);
            this.Btn_SubTool6.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseMove);
            this.Btn_SubTool6.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseUp);
            // 
            // Btn_SubTool5
            // 
            this.Btn_SubTool5.BackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool5.BackgroundImage = global::gInk.Properties.Resources.tool_clipart;
            this.Btn_SubTool5.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Btn_SubTool5.FlatAppearance.BorderColor = System.Drawing.Color.Orange;
            this.Btn_SubTool5.FlatAppearance.BorderSize = 0;
            this.Btn_SubTool5.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool5.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_SubTool5.ForeColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool5.Location = new System.Drawing.Point(243, 2);
            this.Btn_SubTool5.Margin = new System.Windows.Forms.Padding(2);
            this.Btn_SubTool5.Name = "Btn_SubTool5";
            this.Btn_SubTool5.Size = new System.Drawing.Size(46, 46);
            this.Btn_SubTool5.TabIndex = 17;
            this.Btn_SubTool5.Tag = 5;
            this.toolTip.SetToolTip(this.Btn_SubTool5, "SubTool5");
            this.Btn_SubTool5.UseVisualStyleBackColor = true;
            this.Btn_SubTool5.Click += new System.EventHandler(this.SubTool_Click);
            this.Btn_SubTool5.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseDown);
            this.Btn_SubTool5.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseMove);
            this.Btn_SubTool5.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseUp);
            // 
            // Btn_SubTool0
            // 
            this.Btn_SubTool0.BackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool0.BackgroundImage = global::gInk.Properties.Resources.tool_clipart;
            this.Btn_SubTool0.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Btn_SubTool0.FlatAppearance.BorderColor = System.Drawing.Color.Orange;
            this.Btn_SubTool0.FlatAppearance.BorderSize = 0;
            this.Btn_SubTool0.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool0.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool0.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_SubTool0.ForeColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool0.Location = new System.Drawing.Point(3, 2);
            this.Btn_SubTool0.Margin = new System.Windows.Forms.Padding(2);
            this.Btn_SubTool0.Name = "Btn_SubTool0";
            this.Btn_SubTool0.Size = new System.Drawing.Size(46, 46);
            this.Btn_SubTool0.TabIndex = 20;
            this.Btn_SubTool0.Tag = 0;
            this.toolTip.SetToolTip(this.Btn_SubTool0, "SubTool0");
            this.Btn_SubTool0.UseVisualStyleBackColor = true;
            this.Btn_SubTool0.Click += new System.EventHandler(this.SubTool_Click);
            this.Btn_SubTool0.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseDown);
            this.Btn_SubTool0.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseMove);
            this.Btn_SubTool0.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseUp);
            // 
            // Btn_SubTool7
            // 
            this.Btn_SubTool7.BackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool7.BackgroundImage = global::gInk.Properties.Resources.tool_clipart;
            this.Btn_SubTool7.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Btn_SubTool7.FlatAppearance.BorderColor = System.Drawing.Color.Orange;
            this.Btn_SubTool7.FlatAppearance.BorderSize = 0;
            this.Btn_SubTool7.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool7.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_SubTool7.ForeColor = System.Drawing.Color.Transparent;
            this.Btn_SubTool7.Location = new System.Drawing.Point(339, 2);
            this.Btn_SubTool7.Margin = new System.Windows.Forms.Padding(2);
            this.Btn_SubTool7.Name = "Btn_SubTool7";
            this.Btn_SubTool7.Size = new System.Drawing.Size(46, 46);
            this.Btn_SubTool7.TabIndex = 19;
            this.Btn_SubTool7.Tag = 7;
            this.toolTip.SetToolTip(this.Btn_SubTool7, "SubTool7");
            this.Btn_SubTool7.UseVisualStyleBackColor = true;
            this.Btn_SubTool7.Click += new System.EventHandler(this.SubTool_Click);
            this.Btn_SubTool7.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseDown);
            this.Btn_SubTool7.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseMove);
            this.Btn_SubTool7.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseUp);
            // 
            // Btn_SubToolClose
            // 
            this.Btn_SubToolClose.BackColor = System.Drawing.Color.Transparent;
            this.Btn_SubToolClose.BackgroundImage = global::gInk.Properties.Resources.exit;
            this.Btn_SubToolClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Btn_SubToolClose.FlatAppearance.BorderSize = 0;
            this.Btn_SubToolClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Btn_SubToolClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Btn_SubToolClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_SubToolClose.ForeColor = System.Drawing.Color.Transparent;
            this.Btn_SubToolClose.Location = new System.Drawing.Point(388, 2);
            this.Btn_SubToolClose.Margin = new System.Windows.Forms.Padding(2);
            this.Btn_SubToolClose.Name = "Btn_SubToolClose";
            this.Btn_SubToolClose.Size = new System.Drawing.Size(25, 22);
            this.Btn_SubToolClose.TabIndex = 21;
            this.toolTip.SetToolTip(this.Btn_SubToolClose, "Hand Drawing");
            this.Btn_SubToolClose.UseVisualStyleBackColor = true;
            this.Btn_SubToolClose.Click += new System.EventHandler(this.Btn_SubToolClose_Click);
            this.Btn_SubToolClose.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseDown);
            this.Btn_SubToolClose.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseMove);
            this.Btn_SubToolClose.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseUp);
            // 
            // gpPenWidth
            // 
            this.gpPenWidth.BackColor = System.Drawing.Color.WhiteSmoke;
            this.gpPenWidth.BackgroundImage = global::gInk.Properties.Resources.penwidthpanel;
            this.gpPenWidth.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gpPenWidth.Controls.Add(this.pboxPenWidthIndicator);
            this.gpPenWidth.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.gpPenWidth.Location = new System.Drawing.Point(116, 217);
            this.gpPenWidth.Name = "gpPenWidth";
            this.gpPenWidth.Size = new System.Drawing.Size(200, 53);
            this.gpPenWidth.TabIndex = 4;
            this.gpPenWidth.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpPenWidth_MouseDown);
            this.gpPenWidth.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpPenWidth_MouseMove);
            this.gpPenWidth.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpPenWidth_MouseUp);
            // 
            // pboxPenWidthIndicator
            // 
            this.pboxPenWidthIndicator.BackColor = System.Drawing.Color.Orange;
            this.pboxPenWidthIndicator.Location = new System.Drawing.Point(78, 0);
            this.pboxPenWidthIndicator.Name = "pboxPenWidthIndicator";
            this.pboxPenWidthIndicator.Size = new System.Drawing.Size(5, 53);
            this.pboxPenWidthIndicator.TabIndex = 5;
            this.pboxPenWidthIndicator.TabStop = false;
            this.pboxPenWidthIndicator.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pboxPenWidthIndicator_MouseDown);
            this.pboxPenWidthIndicator.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pboxPenWidthIndicator_MouseMove);
            this.pboxPenWidthIndicator.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pboxPenWidthIndicator_MouseUp);
            // 
            // longClickTimer
            // 
            this.longClickTimer.Interval = 1200;
            this.longClickTimer.Tick += new System.EventHandler(this.longClickTimer_Tick);
            // 
            // FontDlg
            // 
            this.FontDlg.FontMustExist = true;
            // 
            // gpSubTools
            // 
            this.gpSubTools.BackColor = System.Drawing.Color.Maroon;
            this.gpSubTools.Controls.Add(this.Btn_SubToolPin);
            this.gpSubTools.Controls.Add(this.Btn_SubToolClose);
            this.gpSubTools.Controls.Add(this.Btn_SubTool0);
            this.gpSubTools.Controls.Add(this.Btn_SubTool7);
            this.gpSubTools.Controls.Add(this.Btn_SubTool6);
            this.gpSubTools.Controls.Add(this.Btn_SubTool5);
            this.gpSubTools.Controls.Add(this.Btn_SubTool4);
            this.gpSubTools.Controls.Add(this.Btn_SubTool3);
            this.gpSubTools.Controls.Add(this.Btn_SubTool2);
            this.gpSubTools.Controls.Add(this.Btn_SubTool1);
            this.gpSubTools.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.gpSubTools.Location = new System.Drawing.Point(676, 217);
            this.gpSubTools.Name = "gpSubTools";
            this.gpSubTools.Size = new System.Drawing.Size(415, 53);
            this.gpSubTools.TabIndex = 5;
            this.gpSubTools.Visible = false;
            this.gpSubTools.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseDown);
            this.gpSubTools.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseMove);
            this.gpSubTools.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseUp);
            // 
            // Btn_SubToolPin
            // 
            this.Btn_SubToolPin.BackColor = System.Drawing.Color.Transparent;
            this.Btn_SubToolPin.BackgroundImage = global::gInk.Properties.Resources.unpinned;
            this.Btn_SubToolPin.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Btn_SubToolPin.FlatAppearance.BorderSize = 0;
            this.Btn_SubToolPin.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.Btn_SubToolPin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.Btn_SubToolPin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Btn_SubToolPin.ForeColor = System.Drawing.Color.Transparent;
            this.Btn_SubToolPin.Location = new System.Drawing.Point(388, 26);
            this.Btn_SubToolPin.Margin = new System.Windows.Forms.Padding(2);
            this.Btn_SubToolPin.Name = "Btn_SubToolPin";
            this.Btn_SubToolPin.Size = new System.Drawing.Size(25, 22);
            this.Btn_SubToolPin.TabIndex = 21;
            this.Btn_SubToolPin.Tag = 0;
            this.Btn_SubToolPin.UseVisualStyleBackColor = false;
            this.Btn_SubToolPin.Click += new System.EventHandler(this.BtnPin_Click);
            this.Btn_SubToolPin.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseDown);
            this.Btn_SubToolPin.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseMove);
            this.Btn_SubToolPin.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gpSubTools_MouseUp);
            // 
            // FormCollection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(1501, 526);
            this.Controls.Add(this.gpSubTools);
            this.Controls.Add(this.gpPenWidth);
            this.Controls.Add(this.gpButtons);
            this.ForeColor = System.Drawing.Color.Transparent;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormCollection";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormCollection_FormClosing);
            this.gpButtons.ResumeLayout(false);
            this.gpPenWidth.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pboxPenWidthIndicator)).EndInit();
            this.gpSubTools.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion
		public System.Windows.Forms.Button btClear;
		public System.Windows.Forms.Button btUndo;
		public System.Windows.Forms.Panel gpButtons;

        public System.Windows.Forms.Button btHand;
        public System.Windows.Forms.Button btLine;
        public System.Windows.Forms.Button btRect;
        public System.Windows.Forms.Button btOval;
        public System.Windows.Forms.Button btArrow;
        public System.Windows.Forms.Button btNumb;
        public System.Windows.Forms.Button btText;
        public System.Windows.Forms.Button btEdit;
        public System.Windows.Forms.Button btMagn;

        public System.Windows.Forms.Button btEraser;
		public System.Windows.Forms.Button btDock;
		public System.Windows.Forms.Button btSnap;
		public System.Windows.Forms.Button btPointer;
		public System.Windows.Forms.Button btPenWidth;
		public System.Windows.Forms.ToolTip toolTip;
		public System.Windows.Forms.Panel gpPenWidth;
		private System.Windows.Forms.PictureBox pboxPenWidthIndicator;
		public System.Windows.Forms.Button btPan;
		public System.Windows.Forms.Button btInkVisible;
        public System.Windows.Forms.Button btVideo;
        public System.Windows.Forms.FontDialog FontDlg;
        public System.Windows.Forms.Button btStop;
        public System.Windows.Forms.Button btClipArt;
        public System.Windows.Forms.Button btClip1;
        public System.Windows.Forms.Button btClip3;
        public System.Windows.Forms.Button btClip2;
        public System.Windows.Forms.Button btLoad;
        public System.Windows.Forms.Button btSave;
        public System.Windows.Forms.Button btZoom;
        public System.Windows.Forms.Button Btn_SubTool6;
        public System.Windows.Forms.Button Btn_SubTool5;
        public System.Windows.Forms.Button Btn_SubTool4;
        public System.Windows.Forms.Button Btn_SubTool3;
        public System.Windows.Forms.Button Btn_SubTool2;
        public System.Windows.Forms.Button Btn_SubTool1;
        public System.Windows.Forms.Button Btn_SubTool0;
        public System.Windows.Forms.Button Btn_SubTool7;
        public System.Windows.Forms.Button Btn_SubToolClose;
        public System.Windows.Forms.Button Btn_SubToolPin;
        public System.Windows.Forms.Panel gpSubTools;
        public System.Windows.Forms.Timer tiSlide;
        public System.Windows.Forms.Timer longClickTimer;
        public System.Windows.Forms.Button btLasso;
        public System.Windows.Forms.Button btScaleRot;
        public System.Windows.Forms.Button btExtraPens;
        public System.Windows.Forms.Button btPagePrev;
        public System.Windows.Forms.Button btPageNext;
    }
}

