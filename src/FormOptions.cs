using System;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.IO;

namespace gInk
{
	public partial class FormOptions : Form
	{
		public Root Root;

        
		Label[] lbPens = new Label[Root.MaxPenCount];
		CheckBox[] cbPens = new CheckBox[Root.MaxPenCount];
		PictureBox[] pboxPens = new PictureBox[Root.MaxPenCount];
		ComboBox[] comboPensAlpha = new ComboBox[Root.MaxPenCount];
		ComboBox[] comboPensWidth = new ComboBox[Root.MaxPenCount];
        Panel [] comboPensLineStyle = new Panel[Root.MaxPenCount];
        CheckBox[] comboPensFading = new CheckBox[Root.MaxPenCount];
        //Label lbcbPens, lbpboxPens, lbcomboPensAlpha, lbcomboPensWidth, lbcomboPensFading;

		Label[] lbHotkeyPens = new Label[Root.MaxPenCount];
		HotkeyInputBox[] hiPens = new HotkeyInputBox[Root.MaxPenCount];

        Bitmap[] ToolBarOrientationIcons = { gInk.Properties.Resources.toolbar2Left, gInk.Properties.Resources.toolbar2Right,
                                             gInk.Properties.Resources.toolbar2Up, gInk.Properties.Resources.toolbar2Down };

        public FormOptions(Root root)
		{
			Root = root;
			InitializeComponent();
            int i = PenPanel.Left;
            PenPanel.Left = 0;
            PenPanel.Width += i;
            for (int p = 0; p < Root.MaxPenCount; p++)
			{
				//int top = p * (int)(this.Height * 0.075) + (int)(this.Height * 0.09);
                //int top = lbPens0.Top + p * (lbPens1.Top-lbPens0.Top);
                /*lbPens[p] = new Label();
				lbPens[p].Left = (int)(this.Width / 500.0 * 60);
				lbPens[p].Width = 80;
				lbPens[p].Top = top;*/

                lbPens[p] = new Label();

				cbPens[p] = new CheckBox();
				cbPens[p].CheckedChanged += cbPens_CheckedChanged;

				pboxPens[p] = new PictureBox();
				pboxPens[p].Click += pboxPens_Click;

				comboPensAlpha[p] = new ComboBox();
				comboPensAlpha[p].TextChanged += comboPensAlpha_TextChanged;

				comboPensWidth[p] = new ComboBox();
				comboPensWidth[p].TextChanged += comboPensWidth_TextChanged;

                comboPensLineStyle[p] = new Panel();
                comboPensLineStyle[p].Click += comboPensLineStyle_Changed;

                comboPensFading[p] = new CheckBox();
                comboPensFading[p].CheckedChanged += comboPensFading_Changed;

                PenPanel.Controls.Add(lbPens[p]);
                PenPanel.Controls.Add(cbPens[p]);
                PenPanel.Controls.Add(pboxPens[p]);
                PenPanel.Controls.Add(comboPensAlpha[p]);
                PenPanel.Controls.Add(comboPensWidth[p]);
                PenPanel.Controls.Add(comboPensLineStyle[p]);
                PenPanel.Controls.Add(comboPensFading[p]);

                if (p >= Root.MaxDisplayedPens)
                    continue;

                lbHotkeyPens[p] = new Label();
                hiPens[p] = new HotkeyInputBox();
                hiPens[p].OnHotkeyChanged += hi_OnHotkeyChanged;

                tabPage3.Controls.Add(lbHotkeyPens[p]);
                tabPage3.Controls.Add(hiPens[p]);
            }
        }

        private void FormOptions_Load(object sender, EventArgs e)
		{
            //Root.UnsetHotkey();
            ToolbarDwg.BackColor = Color.FromArgb(Root.ToolbarBGColor[0], Root.ToolbarBGColor[1], Root.ToolbarBGColor[2], Root.ToolbarBGColor[3]);
            ToolbarOrientationBtn.BackgroundImage = ToolBarOrientationIcons[Root.ToolbarOrientation];
            if(Root.StampFileNames.Count != Root.FormCollection.ClipartsDlg.ImageListViewer.Items.Count 
                && MessageBox.Show(Root.Local.QuestionClipArtUpdate,"ppInk",MessageBoxButtons.YesNo)==DialogResult.Yes)
            {
                Root.StampFileNames.Clear();
                foreach (ListViewItem it in Root.FormCollection.ClipartsDlg.ImageListViewer.Items)
                    Root.StampFileNames.Add(it.ImageKey);// dlg.Images.Images.Keys[i]);
            }
            Clip1Btn.BackColor = ToolbarDwg.BackColor;
            try
            {
                if (Clip1Btn.BackgroundImage != null)
                    Clip1Btn.BackgroundImage.Dispose();
                Clip1Btn.BackgroundImage = FormCollection.getImgFromDiskOrRes(Root.ImageStamp1.ImageStamp);
            }
            catch
            {
                Program.WriteErrorLog(string.Format("File {0} found but can not be loaded:{1} \n", "Stamp1", Root.ImageStamp1.ImageStamp));
                Clip1Btn.BackgroundImage = FormCollection.getImgFromDiskOrRes("unknown");
            }
            Clip2Btn.BackColor = ToolbarDwg.BackColor;
            try
            {
                if (Clip2Btn.BackgroundImage != null)
                    Clip2Btn.BackgroundImage.Dispose();
                Clip2Btn.BackgroundImage = FormCollection.getImgFromDiskOrRes(Root.ImageStamp2.ImageStamp);
            }
            catch
            {
                Program.WriteErrorLog(string.Format("File {0} found but can not be loaded:{1} \n", "Stamp2", Root.ImageStamp2.ImageStamp));
                Clip1Btn.BackgroundImage = FormCollection.getImgFromDiskOrRes("unknown");
            }
            Clip3Btn.BackColor = ToolbarDwg.BackColor;
            try
            {
                if (Clip3Btn.BackgroundImage != null)
                    Clip3Btn.BackgroundImage.Dispose();
                Clip3Btn.BackgroundImage = FormCollection.getImgFromDiskOrRes(Root.ImageStamp3.ImageStamp);
            }
            catch
            {
                Program.WriteErrorLog(string.Format("File {0} found but can not be loaded:{1} \n", "Stamp3", Root.ImageStamp3.ImageStamp));
                Clip1Btn.BackgroundImage = FormCollection.getImgFromDiskOrRes("unknown");
            }
            SubToolsBar_cb.Checked = Root.SubToolsEnabled;
            PensOnTwoLinesCb.Checked = Root.PensOnTwoLines;
            if (Root.EraserEnabled)
				cbEraserEnabled.Checked = true;
			if (Root.PointerEnabled)
				cbPointerEnabled.Checked = true;
			if (Root.SnapEnabled)
				cbSnapEnabled.Checked = true;
			if (Root.UndoEnabled)
				cbUndoEnabled.Checked = true;
			if (Root.ClearEnabled)
				cbClearEnabled.Checked = true;
            WidthAtPenSelCb.Checked = Root.WidthAtPenSel;
            cbWidthEnabled.Checked = Root.PenWidthEnabled;
            if (Root.PanEnabled)
				cbPanEnabled.Checked = true;
			if (Root.InkVisibleEnabled)
				cbInkVisibleEnabled.Checked = true;
            if (Root.ToolsEnabled)
                cbToolsEnabled.Checked = true;

            if (Root.WhiteTrayIcon)
				cbWhiteIcon.Checked = true;
			if (Root.AllowDraggingToolbar)
				cbAllowDragging.Checked = true;
			if (Root.AllowHotkeyInPointerMode)
				cbAllowHotkeyInPointer.Checked = true;
            cbLoadSaveEnabled.Checked = Root.LoadSaveEnabled;
            ColorPickerEnaCb.Checked = Root.ColorPickerEnabled;
            SwapSnapsBehviorsCb.Checked = !Root.SwapSnapsBehaviors;

            AltTabActivateCb.Checked = Root.AltTabPointer;
            KeepUnfoldedPointerCb.Checked = Root.KeepUnDockedAtPointer;

            MeasureEnabledCb.Checked = Root.MeasureEnabled;
            Measure2ScaleEd.Text = Root.Measure2Scale.ToString();
            Measure2DigEd.Text = Root.Measure2Digits.ToString();
            Measure2UnitEd.Text = Root.Measure2Unit;
            MeasureAngleCb.Checked = Root.MeasureAnglCounterClockwise;

            APIRestEd.Text = Root.APIRestUrl;
            APIRestEd.BackColor = Root.APIRest.IsListening() ? Color.White : Color.Orange;

            ToolBarHeight.Text = string.Format("{0:F1}", Root.ToolbarHeight * 100);
            //MoveToolBarCb.Checked = Root.AllowDraggingToolbar;

			comboCanvasCursor.SelectedIndex = Root.CanvasCursor;

            BoardAtOpenCombo.SelectedIndex = Root.BoardAtOpening;
            BoardCustColorPnl.BackColor = Color.FromArgb(Root.Gray1[0], Root.Gray1[1], Root.Gray1[2], Root.Gray1[3]);


            tbSnapPath.Text = Root.SnapshotBasePath;
            tbSnapFileTemplate.Text = Root.SnapshotFileTemplate;
            OpenIntoSnapCb.Checked = Root.OpenIntoSnapMode;
            StartFoldedCb.Checked = Root.KeepDockedAtOpen;
            ShowFloatingWinCb.Checked = Root.FormOpacity > 0;
            ArrHdAperture.Text = (Root.ArrowAngle * 180.0 / Math.PI).ToString("#0",CultureInfo.InvariantCulture);
            ArrHdLength.Text = (Root.ArrowLen / System.Windows.SystemParameters.PrimaryScreenWidth *100.0).ToString("#0.0000",CultureInfo.InvariantCulture);
            Magnet_TB.Text = (Root.MagneticRadius / System.Windows.SystemParameters.PrimaryScreenWidth * 100.0).ToString("#0.0000", CultureInfo.InvariantCulture);
            MagnetAngleEd.Text= Root.MagneticAngle.ToString();
            DefArrStartCb.Checked = Root.DefaultArrow_start;

            ZoomEnabledCb.SelectedIndex = Root.ZoomEnabled;
            ZoomWidthEd.Text = Root.ZoomWidth.ToString();
            ZoomHeightEd.Text = Root.ZoomHeight.ToString();
            ZoomScaleEd.Text = Root.ZoomScale.ToString();
            ZoomContinousCb.Checked = Root.ZoomContinous;

            SpotColorPnl.BackColor = Root.SpotLightColor;
            SpotOnAltCb.Checked = Root.SpotOnAlt;
            SpotRadTb.Text = (Root.SpotLightRadius / System.Windows.SystemParameters.PrimaryScreenWidth * 100.0).ToString("#0.0", CultureInfo.InvariantCulture);

            CaptStrokesOnlyCb.Checked = Root.StrokesOnlySnapshot;

            lbNote.ForeColor = Color.Black;

            //lbPens[0] = lbPens0; lbPens[1] = lbPens1; lbPens[2] = lbPens2; lbPens[3] = lbPens3; lbPens[4] = lbPens4;
            //lbPens[5] = lbPens5; lbPens[6] = lbPens6; lbPens[7] = lbPens7; lbPens[8] = lbPens8; lbPens[9] = lbPens9;

            for (int p = 0; p < Root.MaxPenCount; p++)
			{
				//int top = p * (int)(this.Height * 0.075) + (int)(this.Height * 0.09);
                int top = lbPens0.Top - PenPanel.Top + p * (lbPens1.Top-lbPens0.Top);
                /*lbPens[p] = new Label();
				lbPens[p].Left = (int)(this.Width / 500.0 * 60);
				lbPens[p].Width = 80;
				lbPens[p].Top = top;*/

                lbPens[p].Left = lbPens0.Left;           
                lbPens[p].Top = top;

                cbPens[p].Left = lbcbPens.Left + 10;// (int)(this.Width / 500.0 * 30);
				cbPens[p].Width = 25;
				cbPens[p].Top = top - 5;
				cbPens[p].Text = "";
				cbPens[p].Checked = Root.PenEnabled[p];

                pboxPens[p].Left = lbpboxPens.Left + 10;// (int)(this.Width / 500.0 * 130);
				pboxPens[p].Top = top;
				pboxPens[p].Width = 15;
				pboxPens[p].Height = 15;
				pboxPens[p].BackColor = Root.PenAttr[p].Color;

                comboPensAlpha[p].Left = lbcomboPensAlpha.Left;// (int)(this.Width / 500.0 * 180);
				comboPensAlpha[p].Top = top - 2;
				comboPensAlpha[p].Width = 60;
				comboPensAlpha[p].Text = (255 - Root.PenAttr[p].Transparency).ToString();

                comboPensWidth[p].Left = lbcomboPensWidth.Left;// (int)(this.Width / 500.0 * 270);
				comboPensWidth[p].Top = top - 2;
				comboPensWidth[p].Width = 60;
				comboPensWidth[p].Text = ((int)Root.PenAttr[p].Width).ToString();

                comboPensLineStyle[p].Left = lbLineStyle.Left + 5;// (int)(this.Width / 500.0 * 270);
                comboPensLineStyle[p].Top = top - 2;
                comboPensLineStyle[p].Height = comboPensWidth[p].Height;
                comboPensLineStyle[p].Width = comboPensWidth[p].Height*2;
                comboPensLineStyle[p].BackgroundImageLayout = ImageLayout.Stretch;
                comboPensLineStyle[p].BackgroundImage = FormCollection.getImgFromDiskOrRes("DashStyle"+ Root.LineStyleToString(Root.PenAttr[p].ExtendedProperties));
                comboPensLineStyle[p].Tag = p;

                comboPensFading[p].Left = lbcomboPensFading.Left + 10;  // (int)(this.Width / 500.0 * 380);
                comboPensFading[p].Top = top - 2;
                comboPensFading[p].Width = 20;
                comboPensFading[p].Checked = Root.PenAttr[p].ExtendedProperties.Contains(Root.FADING_PEN);
            }

            FadingTimeEd.Text = Root.TimeBeforeFading.ToString();
            InverseWheelCb.Checked = Root.InverseMousewheel;
            FitToCurveEd.Checked = Root.FitToCurve;
            Click4StrokeCb.Checked = Root.ButtonClick_For_LineStyle;

            //cbAllowHotkeyInPointer.Top = (int)(this.Height * 0.18);
            ExtraPensCb.Checked = Root.PensExtraSet;
            for (int p = 0; p < Root.MaxDisplayedPens; p++)
			{
				//int top = p * (hiEraser.Top - hiLasso.Top) + hiLasso.Top;
				lbHotkeyPens[p].Left = lbHkColorEdit.Left;
				lbHotkeyPens[p].Width = 80;
				lbHotkeyPens[p].Top = p * (lbHkEraser.Top - lbHkLasso.Top) + lbHkLasso.Top;

				hiPens[p].Hotkey = Root.Hotkey_Pens[p];
				hiPens[p].Left = hiColorEdit.Left;
				hiPens[p].Width = hiColorEdit.Width;
				hiPens[p].Top = p * (hiEraser.Top - hiLasso.Top) + hiLasso.Top;
			}

            //AltAsOneCommandCb.Checked = Root.AltAsOneCommand;
            if (Root.AltAsOneCommand == 2)
                AltAsOneCommandCb.CheckState = CheckState.Checked;
            else if (Root.AltAsOneCommand == 1)
                AltAsOneCommandCb.CheckState = CheckState.Indeterminate;
            else
                AltAsOneCommandCb.CheckState = CheckState.Unchecked;
                       
            SnapInPointerHoldCb.SelectedIndex = (int)(Root.SnapInPointerHoldKey);
            SnapInPointerTwiceCb.SelectedIndex = (int)(Root.SnapInPointerPressTwiceKey);

            hiGlobal.Hotkey = Root.Hotkey_Global;
            hiFadingToggle.Hotkey = Root.Hotkey_FadingToggle;
            hiEraser.Hotkey = Root.Hotkey_Eraser;
			hiPan.Hotkey = Root.Hotkey_Pan;
			hiInkVisible.Hotkey = Root.Hotkey_InkVisible;
			hiScaleRotate.Hotkey = Root.Hotkey_ScaleRotate;
			hiSnapshot.Hotkey = Root.Hotkey_Snap;
			hiUndo.Hotkey = Root.Hotkey_Undo;
			hiRedo.Hotkey = Root.Hotkey_Redo;
			hiClear.Hotkey = Root.Hotkey_Clear;
            hiVideo.Hotkey = Root.Hotkey_Video;
            hiDockUndock.Hotkey = Root.Hotkey_DockUndock;
            hiClose.Hotkey = Root.Hotkey_Close;

            hiToolHand.Hotkey = Root.Hotkey_Hand;
            hiToolLine.Hotkey = Root.Hotkey_Line;
            hiToolRect.Hotkey = Root.Hotkey_Rect;
            hiToolOval.Hotkey = Root.Hotkey_Oval;
            hiToolArrow.Hotkey = Root.Hotkey_Arrow;
            hiToolNumb.Hotkey = Root.Hotkey_Numb;
            HiToolText.Hotkey = Root.Hotkey_Text;
            hiToolEdit.Hotkey = Root.Hotkey_Edit;
            hiToolMagnet.Hotkey = Root.Hotkey_Magnet;
            hiToolClipArt.Hotkey = Root.Hotkey_ClipArt;
            hiToolClipArt1.Hotkey = Root.Hotkey_ClipArt1;
            hiToolClipArt2.Hotkey = Root.Hotkey_ClipArt2;
            hiToolClipArt3.Hotkey = Root.Hotkey_ClipArt3;
            hiLoadStrokes.Hotkey = Root.Hotkey_LoadStrokes;
            hiSaveStrokes.Hotkey = Root.Hotkey_SaveStrokes;

            hiZoom.Hotkey = Root.Hotkey_Zoom;
            hiPenWidthPlus.Hotkey = Root.Hotkey_PenWidthPlus;
            hiPenWidthMinus.Hotkey = Root.Hotkey_PenWidthMinus;
            hiColorPickup.Hotkey = Root.Hotkey_ColorPickup;
            hiColorEdit.Hotkey = Root.Hotkey_ColorEdit;
            hiLineStyle.Hotkey = Root.Hotkey_LineStyle;
            hiLasso.Hotkey = Root.Hotkey_Lasso;

            CbHKRot_Stroke.Checked = (Root.LineStyleRotateEnabled & (int)(CbHKRot_Stroke.Tag)) != 0;
            CbHKRot_Solid.Checked = (Root.LineStyleRotateEnabled & (int)(CbHKRot_Solid.Tag)) != 0;
            CbHKRot_Dash.Checked = (Root.LineStyleRotateEnabled & (int)(CbHKRot_Dash.Tag)) != 0;
            CbHKRot_Dot.Checked = (Root.LineStyleRotateEnabled & (int)(CbHKRot_Dot.Tag)) != 0;
            CbHKRot_DashDot.Checked = (Root.LineStyleRotateEnabled & (int)(CbHKRot_DashDot.Tag)) != 0;
            CbHKRot_DashDotDot.Checked = (Root.LineStyleRotateEnabled & (int)(CbHKRot_DashDotDot.Tag)) != 0;

            WsUrlTxt.Text = Root.ObsUrl;
            WsPwdTxt.Text = Root.ObsPwd;
            FfmegFileNameTxt.Text = Root.FFMpegFileName;
            FfmpegCmdTxt.Text = Root.FFMpegCmd;

            CreateM3u.Checked = Root.CreateM3U;
            M3UIndexOnUndockCb.Checked = Root.CreateIndexOnUndock;
            hiCreateM3UIndex.Hotkey = Root.Hotkey_CreateIndex;
            M3UIndexDefaultTxt.Text = Root.IndexDefaultText;
            UndockOnM3UIndexCb.Checked = Root.UndockOnIndexCreate;
            NoEditM3UIndexCb.Checked = Root.NoEditM3UEntry;

            switch (Root.VideoRecordMode)
            {
                case VideoRecordMode.NoVideo :
                    OptNoVideo.Checked = true;
                    break;
                case VideoRecordMode.OBSRec:
                    OptObsRecord.Checked = true;
                    break;
                case VideoRecordMode.OBSBcst:
                    OptObsBcast.Checked = true;
                    break;
                case VideoRecordMode.FfmpegRec:
                    OptFfmpeg.Checked = true;
                    break;
                default:
                    throw new System.Exception(String.Format("unk video recording mode", Root.VideoRecordMode));
            }

            FormOptions_LocalReload();
		}

        private void FormOptions_Shown(object sender, EventArgs e)
        {
            FormOptions_Load(sender, e);
        }

        private void FormOptions_LocalReload()
		{   string shortTxt(string sin)
            {
                int i = sin.IndexOf("(");
                if (i < 0) i = sin.Length;
                return sin.Substring(0, i);
            }
			this.Text = Root.Local.MenuEntryOptions + " - ppInk";
			VideoTabCtrl.TabPages[0].Text = Root.Local.OptionsTabGeneral;
			VideoTabCtrl.TabPages[1].Text = Root.Local.OptionsTabPens;
			VideoTabCtrl.TabPages[2].Text = Root.Local.OptionsTabHotkeys;
            SubToolsBar_cb.Text = Root.Local.SubToolsBarCbText;
            PensOnTwoLinesCb.Text = Root.Local.OptionPensOnTwoLinesCb;
            this.ToolBarColorLbl.Text = Root.Local.OptionsGeneralToolBarColorText;
            this.ClipartsSelBtn.Text = shortTxt(Root.Local.ButtonNameClipArt);
            this.AltTabActivateCb.Text = Root.Local.OptionsGeneralAltTabActivateText;
            this.KeepUnfoldedPointerCb.Text = Root.Local.OptionsGeneralUnDockedInPointerText;
            this.lblToolbarHeight.Text = Root.Local.OptionsGeneralToolbarHeight;
			this.lbLanguage.Text = Root.Local.OptionsGeneralLanguage;
			this.lbCanvascursor.Text = Root.Local.OptionsGeneralCanvascursor;
			this.lbSnapshotsavepath.Text = Root.Local.OptionsGeneralSnapshotsavepath;
            this.OpenIntoSnapCb.Text = Root.Local.OptionsGeneralOpenIntoSnapMode;
            this.StartFoldedCb.Text = Root.Local.OptionGeneralStartFolded;
            this.cbWhiteIcon.Text = Root.Local.OptionsGeneralWhitetrayicon;
			this.cbAllowDragging.Text = Root.Local.OptionsGeneralAllowdragging;
            this.APIRestLbl.Text = Root.Local.OptionsGeneralAPIRest;
            this.ShowFloatingWinCb.Text = Root.Local.OptionsGeneralShowFloatingWindow;
            this.SaveWindowPosBtn.Text = Root.Local.OptionsGeneralSaveFloatingWindowPos;
            this.ArrwGrp.Text = Root.Local.OptionsGeneralArrowHead;
            this.ArrHdAptLbl.Text = Root.Local.OptionsGeneralArrowHeadApt;
            this.ArrHdLenLbl.Text = Root.Local.OptionsGeneralArrowHeadLen;
            this.NewArrowEditBtn.Text = shortTxt(Root.Local.ButtonNameArrow);
            this.DefTxtLbl.Text = shortTxt(Root.Local.ButtonNameText) + " - " + Root.Local.OptionsGeneralDefaultTextLbl;
            this.DefaultFontBtn.Text = Root.Local.OptionsGeneralDefaultTextBtn;
            this.DefTagLbl.Text = shortTxt(Root.Local.ButtonNameNumb)+" - "+Root.Local.OptionsGeneralDefaultTextLbl;
            this.TagFontBtn.Text = Root.Local.OptionsGeneralDefaultTextBtn;
            this.DefArrStartCb.Text = Root.Local.OptionsGeneralDefaultArrHdBtn;
            this.MagnetLbl.Text = Root.Local.OptionsGeneralMagnetLbl;
            this.SaveConfigBtn.Text = Root.Local.OptionsGeneralSaveConfigToFile;
            this.CaptStrokesOnlyCb.Text = Root.Local.OptionsCaptureStrokesOnly;
            this.ColorPickerEnaCb.Text = Root.Local.OptionsHotkeysColorPicker;
            this.SwapSnapsBehviorsCb.Text = Root.Local.OptionsSwapSnapshotBehavior;

            MeasurementBox.Text = Root.Local.OptionMeasureGroup;
            Measuse1Lbl.Text = Root.Local.OptionMeasureLenLabel;
            MeasureAngleCb.Text = Root.Local.OptionMeasureAngle;

            this.lbNote.Text = Root.Local.OptionsGeneralNotePenwidth;

            {
                int i = 0;
                foreach(string st in Root.Local.OptionsZoomEnabled.Split('\n'))
                    this.ZoomEnabledCb.Items[i++]=st;
            }
            this.ZoomBox.Text = Root.Local.ButtonNameZoom;
            this.ZoomDimLbl.Text = Root.Local.OptionsZoomDim;
            this.ZoomScaleLbl.Text = Root.Local.OptionsZoomScale;
            this.ZoomContinousCb.Text = Root.Local.OptionsZoomContinous;

            this.SpotLightBox.Text = Root.Local.OptionsSpotLightBox;
            this.SpotOnAltCb.Text = Root.Local.OptionsSpotOnAlt;
            this.SpotRadLbl.Text = Root.Local.OptionsSpotLightRadius;

            this.ActivateDbgWinBtn.Text = Root.Local.ButtonActivateDebug;

            this.SnapInPointerGrp.Text = Root.Local.OptionsHotKeySnapInPointerGrp;
            this.SnapInPointerLbl.Text = Root.Local.OptionsHotKeySnapInPointerLbl;

            {
                int i = 0;
                foreach(string st in Root.Local.OptionsHotKeySnapInPointerKeys.Split('\n'))
                {
                    this.SnapInPointerHoldCb.Items[i] = st;
                    this.SnapInPointerTwiceCb.Items[i++] = st;
                }
            }
            this.ExtraPensCb.Text = Root.Local.OptionHotkeysExtraPens;
            this.AltAsOneCommandCb.Text = Root.Local.OptionsHotKeyAltAsOneCommand;
            this.lbHkFadingToggle.Text = Root.Local.ButtonNameToogle;
			this.lbHkClear.Text = shortTxt(Root.Local.ButtonNameClear);
            this.lbHkVideo.Text = shortTxt(Root.Local.ButtonNameVideo);
			this.lbHkEraser.Text = shortTxt(Root.Local.ButtonNameErasor);
			this.lbHkInkVisible.Text = shortTxt(Root.Local.ButtonNameInkVisible);
			this.lbHkPan.Text = shortTxt(Root.Local.ButtonNamePan);
            this.lbHkScaleRotate.Text = shortTxt(Root.Local.ButtonNameScaleRotate);
			this.lbHkRedo.Text = shortTxt(Root.Local.ButtonNameRedo);
			this.lbHkSnapshot.Text = shortTxt(Root.Local.ButtonNameSnapshot);
            this.lbHkUndo.Text = shortTxt(Root.Local.ButtonNameUndo);
            this.lbHkDockUndock.Text = shortTxt(Root.Local.ButtonNameDock);
            this.lbHkClose.Text = Root.Local.ButtonNameClose;
            this.lbHkUndo.Text = shortTxt(Root.Local.ButtonNameUndo);

            this.lbHkHand.Text = shortTxt(Root.Local.ButtonNameHand);
            this.lbHkLine.Text = shortTxt(Root.Local.ButtonNameLine);
            this.lbHkRect.Text = shortTxt(Root.Local.ButtonNameRect);
            this.lbHkOval.Text = shortTxt(Root.Local.ButtonNameOval);
            this.lbHkArrow.Text = shortTxt(Root.Local.ButtonNameArrow);
            this.lbHkNumb.Text = shortTxt(Root.Local.ButtonNameNumb);
            this.lbHkText.Text = shortTxt(Root.Local.ButtonNameText);
            this.lbHkEdit.Text = shortTxt(Root.Local.ButtonNameEdit);
            this.lbHkMagn.Text = shortTxt(Root.Local.ButtonNameMagn);
            this.lbHkClipart.Text = shortTxt(Root.Local.ButtonNameClipArt);
            this.lbHkClipart1.Text = shortTxt(Root.Local.ButtonNameClipArt) + " 1";
            this.lbHkClipart2.Text = shortTxt(Root.Local.ButtonNameClipArt) + " 2";
            this.lbHkClipart3.Text = shortTxt(Root.Local.ButtonNameClipArt) + " 3";
            this.lbHkZoom.Text = shortTxt(Root.Local.ButtonNameZoom);
            this.lbHkLoadStrokes.Text = shortTxt(Root.Local.LoadStroke);
            this.lbHkSaveStrokes.Text = shortTxt(Root.Local.SaveStroke);

            this.lbHkLineStyle.Text = Root.Local.OptionsLineStyle;
            this.lbHkPenWidthPlus.Text = Root.Local.OptionsHotkeysPenWidthPlus;
            this.lbHkPenWidthMinus.Text = Root.Local.OptionsHotkeysPenWidthMinus;

            this.lbHkColorPickup.Text = Root.Local.OptionsHotkeysColorPicker;
            this.lbHkColorEdit.Text = Root.Local.OptionsHotkeysColorEdit;
            this.lbHkLasso.Text = shortTxt(Root.Local.ButtonNameLasso);

            this.lbGlobalHotkey.Text = Root.Local.OptionsHotkeysglobal;
            this.cbAllowHotkeyInPointer.Text = Root.Local.OptionsHotkeysEnableinpointer;

            foreach(Control ct in this.tabPage3.Controls)
            {
                if (ct.GetType() == typeof(HotkeyInputBox))
                    ((HotkeyInputBox)ct).UpdateText();
            }

			this.comboCanvasCursor.Items[0] = Root.Local.OptionsGeneralCanvascursorArrow;
			this.comboCanvasCursor.Items[1] = Root.Local.OptionsGeneralCanvascursorPentip;

            this.BoardBx.Text = Root.Local.OptionsGeneralBoardBox;
            this.BoardAtOpenLbl.Text = Root.Local.OptionsGeneralBoardAtOpenLbl;
            this.BoardCustColorLbl.Text = Root.Local.OptionsGeneralBoardCustColorLbl;
            this.BoardAtOpenCombo.Items[0] = Root.Local.BoardTransparent;
            this.BoardAtOpenCombo.Items[1] = Root.Local.BoardWhite;
            this.BoardAtOpenCombo.Items[2] = Root.Local.BoardGray;
            this.BoardAtOpenCombo.Items[3] = Root.Local.BoardBlack;
            this.BoardAtOpenCombo.Items[4] = Root.Local.BoardLast;

            // Video Tab
            this.VideoTab.Text = Root.Local.VideoTab;
            this.OptNoVideo.Text = Root.Local.OptNoVideo;
            this.OptObsRecord.Text = Root.Local.OptObsRecord;
            this.OptObsBcast.Text = Root.Local.OptObsBcast;
            this.LblWsUrl.Text = Root.Local.LblWsUrl;
            this.LblWsPwd.Text = Root.Local.LblWsPwd;
            this.LblObsNote.Text = Root.Local.LblObsNote;
            this.OptFfmpeg.Text = Root.Local.OptFfmpeg;
            this.LblFfmpegCmd.Text = Root.Local.LblFfmpegCmd;
            this.LblFfmpegNote.Text = Root.Local.LblFfmpegNote;

            CreateM3u.Text = Root.Local.CreateM3UGroup;
            M3UIndexOnUndockCb.Text = Root.Local.CreateIndexOnUndock;
            LblIndexHotKey.Text = Root.Local.LblM3UIndexHotKey;
            M3UIndexDefTextLbl.Text = Root.Local.M3UIndexDefaultText;
            UndockOnM3UIndexCb.Text = Root.Local.UndockOnM3UIndexCreate;
            NoEditM3UIndexCb.Text = Root.Local.NoEditM3UIndex;

            for (int p = 0; p < Root.MaxPenCount; p++)
			{
				comboPensAlpha[p].Items.Clear();
				comboPensWidth[p].Items.Clear();
				comboPensAlpha[p].Items.AddRange(new object[] { Root.Local.OptionsPensPencil, Root.Local.OptionsPensHighlighter });
				comboPensWidth[p].Items.AddRange(new object[] { Root.Local.OptionsPensThin, Root.Local.OptionsPensNormal, Root.Local.OptionsPensThick });
                lbPens[p].Text = Root.Local.ButtonNamePen[p];
                if (p >= Root.MaxDisplayedPens)
                    continue;
				lbHotkeyPens[p].Text = Root.Local.ButtonNamePen[p];

			}
            lbcbPens.Text = Root.Local.OptionsPensShow;
            lbpboxPens.Text = Root.Local.OptionsPensColor;
            lbcomboPensAlpha.Text = Root.Local.OptionsPensAlpha;
            lbcomboPensWidth.Text = Root.Local.OptionsPensWidth;
            lbcomboPensFading.Text = Root.Local.OptionsPensFading;
            WidthAtPenSelCb.Text = Root.Local.OptionsPensWidthAtSelection;
            InverseWheelCb.Text = Root.InverseMousewheel ? Root.Local.OptionsInverseMouseWheelChecked : Root.Local.OptionsInverseMouseWheel;
            FitToCurveEd.Text = Root.Local.OptionsFitToCurve;
            lbLineStyle.Text = Root.Local.OptionsLineStyle;
            Click4StrokeCb.Text = Root.Local.OptionsClick4Stroke;
            //CreateM3u.Text = Root.Local.CreateM3uText;
            //LblIndexHotKey.Text = Root.Local.LblIndexHotKey;

            comboLanguage.Items.Clear();
			List<string> langs = Root.Local.GetLanguagenames();
			foreach (string languagename in langs)
			{
				comboLanguage.Items.Add(languagename);
			}

			string ln = Root.Local.GetLanguagenameByFilename(Root.Local.CurrentLanguageFile);
			if (comboLanguage.Items.Contains(ln))
				comboLanguage.SelectedIndex = comboLanguage.Items.IndexOf(ln);
		}

		private void comboPensAlpha_TextChanged(object sender, EventArgs e)
		{
			for (int p = 0; p < Root.MaxPenCount; p++)
				if ((ComboBox)sender == comboPensAlpha[p])
				{
					byte o;
					if (byte.TryParse(comboPensAlpha[p].Text, out o) && o >= 0 && o <= 255)
					{
						Root.PenAttr[p].Transparency = (byte)(255 - o);
						comboPensAlpha[p].BackColor = Color.White;
					}
					else
					{
						comboPensAlpha[p].BackColor = Color.IndianRed;
					}
				}
		}

		private void comboPensWidth_TextChanged(object sender, EventArgs e)
		{
			for (int p = 0; p < Root.MaxPenCount; p++)
				if ((ComboBox)sender == comboPensWidth[p])
				{
					float o;
					if (float.TryParse(comboPensWidth[p].Text, out o) && o > 0 && o <= 3000)
					{
						Root.PenAttr[p].Width = o;
						comboPensWidth[p].BackColor = Color.White;
					}
					else
					{
						comboPensWidth[p].BackColor = Color.IndianRed;
					}
				}
		}

        private void comboPensFading_Changed(object sender, EventArgs e)
        {
            for (int p = 0; p < Root.MaxPenCount; p++)
                if ((CheckBox)sender == comboPensFading[p])
                {
                    if (((CheckBox)sender).Checked)
                    {
                        Root.PenAttr[p].ExtendedProperties.Add(Root.FADING_PEN, Root.TimeBeforeFading);
                    }
                    else
                    {
                        try{ Root.PenAttr[p].ExtendedProperties.Remove(Root.FADING_PEN); }catch { };
                    }
                }
        }


        private void pboxPens_Click(object sender, EventArgs e)
		{
			for (int p = 0; p < Root.MaxPenCount; p++)
				if ((PictureBox)sender == pboxPens[p])
				{
                    PenModifyDlg dlg = new PenModifyDlg(Root);
                    if (dlg.ModifyPen(ref Root.PenAttr[p]))
                    {
                        (sender as PictureBox).BackColor = Color.FromArgb(255, Root.PenAttr[p].Color);
                        comboPensAlpha[p].Text = string.Format("{0}", 255-Root.PenAttr[p].Transparency);
                        comboPensWidth[p].Text = string.Format("{0:N0}", Root.PenAttr[p].Width);
                        comboPensFading[p].Checked = Root.PenAttr[p].ExtendedProperties.Contains(Root.FADING_PEN);
                        comboPensLineStyle[p].BackgroundImage = FormCollection.getImgFromDiskOrRes("DashStyle" + Root.LineStyleToString(Root.PenAttr[p].ExtendedProperties));
                    }
				}
		}

		private void cbPens_CheckedChanged(object sender, EventArgs e)
		{
			for (int p = 0; p < Root.MaxPenCount; p++)
				if ((CheckBox)sender == cbPens[p])
					Root.PenEnabled[p] = cbPens[p].Checked;
		}

		private void FormOptions_FormClosing(object sender, FormClosingEventArgs e)
		{
            try
            {
                ActiveControl.SelectNextControl(ActiveControl, true, true, false, true);
                ActiveControl.SelectNextControl(ActiveControl, false, true, false, true);
            }
            catch { }
            try
            {
                Root.SetHotkey();
            }
            catch { }
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            GC.Collect();
        }

        private void cbWidthEnabled_CheckedChanged(object sender, EventArgs e)
		{
			Root.PenWidthEnabled = cbWidthEnabled.Checked;
			lbNote.ForeColor = Color.Red;
		}

		private void cbEraserEnabled_CheckedChanged(object sender, EventArgs e)
		{
			Root.EraserEnabled = cbEraserEnabled.Checked;
		}

		private void cbPointerEnabled_CheckedChanged(object sender, EventArgs e)
		{
			Root.PointerEnabled = cbPointerEnabled.Checked;
		}

		private void cbSnapEnabled_CheckedChanged(object sender, EventArgs e)
		{
			Root.SnapEnabled = cbSnapEnabled.Checked;
		}

		private void cbUndoEnabled_CheckedChanged(object sender, EventArgs e)
		{
			Root.UndoEnabled = cbUndoEnabled.Checked;
		}

		private void cbClearEnabled_CheckedChanged(object sender, EventArgs e)
		{
			Root.ClearEnabled = cbClearEnabled.Checked;
		}

		private void cbPanEnabled_CheckedChanged(object sender, EventArgs e)
		{
			Root.PanEnabled = cbPanEnabled.Checked;
		}

		private void cbInkVisibleEnabled_CheckedChanged(object sender, EventArgs e)
		{
			Root.InkVisibleEnabled = cbInkVisibleEnabled.Checked;
		}

		private void cbWhiteIcon_CheckedChanged(object sender, EventArgs e)
		{
			Root.WhiteTrayIcon = cbWhiteIcon.Checked;
			Root.SetTrayIconColor();
		}

		private void btSnapPath_Click(object sender, EventArgs e)
		{
			folderBrowserDialog1 = new FolderBrowserDialog();
			folderBrowserDialog1.SelectedPath = Root.SnapshotBasePath;

			DialogResult result = folderBrowserDialog1.ShowDialog();

			if (result == DialogResult.OK && !string.IsNullOrEmpty(folderBrowserDialog1.SelectedPath))
			{
				tbSnapPath.Text = folderBrowserDialog1.SelectedPath.Replace('\\','/');
                if (!tbSnapPath.Text.EndsWith("/"))
                    tbSnapPath.Text += '/';
                Root.SnapshotBasePath = tbSnapPath.Text;
			}
		}

		private void tbSnapPath_ModifiedChanged(object sender, EventArgs e)
		{
            tbSnapPath.Text = folderBrowserDialog1.SelectedPath.Replace('\\', '/');
            if (!tbSnapPath.Text.EndsWith("/"))
                tbSnapPath.Text += '/';
            Root.SnapshotBasePath = tbSnapPath.Text;
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			for (int p = 0; p < Root.MaxPenCount; p++)
			{
				if (comboPensWidth[p].Text == Root.Local.OptionsPensThin)
				{
					comboPensWidth[p].Text = Root.PenWidthThin.ToString();
				}
				else if (comboPensWidth[p].Text == Root.Local.OptionsPensNormal)
				{
					comboPensWidth[p].Text = Root.PenWidthNormal.ToString();
				}
				else if (comboPensWidth[p].Text == Root.Local.OptionsPensThick)
				{
					comboPensWidth[p].Text = Root.PenWidthThick.ToString();
				}

				if (comboPensAlpha[p].Text == Root.Local.OptionsPensPencil)
				{
					comboPensAlpha[p].Text = "255";
				}
				else if (comboPensAlpha[p].Text == Root.Local.OptionsPensHighlighter)
				{
					comboPensAlpha[p].Text = "80";
				}
			}
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			Root.CanvasCursor = comboCanvasCursor.SelectedIndex;
		}

		private void cbAllowDragging_CheckedChanged(object sender, EventArgs e)
		{
			Root.AllowDraggingToolbar = cbAllowDragging.Checked;
		}

        private void cbToolsEnabled_CheckedChanged(object sender, EventArgs e)
        {
            Root.ToolsEnabled = cbToolsEnabled.Checked;
        }

        private void SaveWindowPosBtn_Click(object sender, EventArgs e)
        {
            Root.FormTop = Root.callForm.Top;
            Root.FormLeft = Root.callForm.Left;
        }

        private void SaveConfigBtn_Click(object sender, EventArgs e)
        {
            String config_ini,st;
            using (var f = new StreamReader("config.ini"))
                config_ini = f.ReadToEnd();
            using (var f = new StreamReader("defaults.ini"))
                st=Root.CompleteConfig(f.ReadToEnd(), config_ini);
            if (st != "")
                using(var f = new StreamWriter("config.ini"))
                    f.Write(config_ini + "\n" + st);

            using (var f = new StreamReader("pens.ini"))
                config_ini = f.ReadToEnd();
            using (var f = new StreamReader("pensdef.ini"))
                st = Root.CompleteConfig(f.ReadToEnd(), config_ini);
            if (st != "")
                using (var f = new StreamWriter("pens.ini"))
                    f.Write(config_ini + "\n" + st);

            Root.SaveOptions("pens.ini");
            Root.SaveOptions("config.ini");
            Root.SaveOptions("hotkeys.ini");
        }

        private void Float_Validating(object sender, CancelEventArgs e)
        {
            float tempf;
            if (float.TryParse(((TextBox)sender).Text.Replace(",","."), NumberStyles.AllowDecimalPoint| NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out tempf))
                ((TextBox)sender).BackColor = Color.White;
            else
            {
                e.Cancel = true;
                ((TextBox)sender).BackColor = Color.Orange;
                ((TextBox)sender).Select();
            }
        }

        private void DefaultFontBtn_Click(object sender, EventArgs e)
        {
            FontDlg.Font = new Font(Root.TextFont, (float)Root.TextSize,
                                        (Root.TextItalic ? FontStyle.Italic : FontStyle.Regular) | (Root.TextBold ? FontStyle.Bold : FontStyle.Regular));
            if (FontDlg.ShowDialog() == DialogResult.OK)
            {
                Root.TextFont = FontDlg.Font.Name;
                Root.TextItalic = (FontDlg.Font.Style & FontStyle.Italic) != 0;
                Root.TextBold = (FontDlg.Font.Style & FontStyle.Bold) != 0;
                Root.TextSize = (int)FontDlg.Font.Size;
            }
        }

        private void TagFontBtn_Click(object sender, EventArgs e)
        {
            FontDlg.Font = new Font(Root.TagFont, (float)Root.TagSize,
                                        (Root.TagItalic ? FontStyle.Italic : FontStyle.Regular) | (Root.TagBold ? FontStyle.Bold : FontStyle.Regular));
            if (FontDlg.ShowDialog() == DialogResult.OK)
            {
                Root.TagFont = FontDlg.Font.Name;
                Root.TagItalic = (FontDlg.Font.Style & FontStyle.Italic) != 0;
                Root.TagBold = (FontDlg.Font.Style & FontStyle.Bold) != 0;
                Root.TagSize = (int)FontDlg.Font.Size;
            }
        }


        private void ArrHdAperture_Validated(object sender, EventArgs e)
        {
            Root.ArrowAngle = float.Parse(ArrHdAperture.Text.Replace(",","."), CultureInfo.InvariantCulture) /180.0*Math.PI;
        }

        private void ArrHdLength_Validated(object sender, EventArgs e)
        {
            Root.ArrowLen = float.Parse(ArrHdLength.Text.Replace(",", "."), CultureInfo.InvariantCulture) / 100.0 * System.Windows.SystemParameters.PrimaryScreenWidth;
        }

        private void ShowFloatingWinCb_Click(object sender, EventArgs e)
        {
            Root.FormOpacity = ((((CheckBox)sender).Checked)?1:-1) * Math.Abs(Root.FormOpacity);
            if (((CheckBox)sender).Checked && (Root.FormTop <= Screen.PrimaryScreen.WorkingArea.Top || Root.FormTop >= Screen.PrimaryScreen.WorkingArea.Bottom || 
                                               Root.FormLeft <= Screen.PrimaryScreen.WorkingArea.Left || Root.FormLeft >= Screen.PrimaryScreen.WorkingArea.Right))
            {
                Root.FormTop = Screen.PrimaryScreen.WorkingArea.Top + 100;
                Root.FormLeft = Screen.PrimaryScreen.WorkingArea.Left + 100;
            }
            if (Root.FormOpacity > 0)
            {
                if(Root.callForm == null)
                    Root.callForm = new CallForm(Root);
                Root.callForm.Show();
                Root.callForm.Top = Root.FormTop;
                Root.callForm.Left = Root.FormLeft;
                Root.callForm.Width = Root.FormWidth;
                Root.callForm.Height = Root.FormWidth;
                Root.callForm.Opacity = Root.FormOpacity / 100.0;
            }
            else
            {
                Root.callForm.Hide();
            }
        }

        private void Magnet_TB_Validated(object sender, EventArgs e)
        {
            Root.MagneticRadius = (int)(float.Parse(Magnet_TB.Text.Replace(",", "."), CultureInfo.InvariantCulture) / 100.0 * System.Windows.SystemParameters.PrimaryScreenWidth);
        }

        private void DefArrStartCb_CheckedChanged(object sender, EventArgs e)
        {
            Root.DefaultArrow_start = DefArrStartCb.Checked;
        }

        private void OpenIntoSnapCb_CheckedChanged(object sender, EventArgs e)
        {
            Root.OpenIntoSnapMode = OpenIntoSnapCb.Checked;
        }

        private void WidthAtPenSelCb_CheckedChanged(object sender, EventArgs e)
        {
            Root.WidthAtPenSel = WidthAtPenSelCb.Checked;
        }

        private void ToolBarHeight_Validated(object sender, EventArgs e)
        {
            Root.ToolbarHeight = float.Parse(ToolBarHeight.Text.Replace(",", "."), CultureInfo.InvariantCulture) / 100;
        }

        private void ValidateOnEnter(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar=='\r')
            {
                this.SelectNextControl(this.ActiveControl, true, true, true, true);
                (sender as TextBox).Select();
                e.Handled = true;
            }
        }

        private void BoardAtOpenCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            Root.BoardAtOpening = BoardAtOpenCombo.SelectedIndex;
        }

        private void BoardCustColorPnl_Click(object sender, EventArgs e)
        {
            PenModifyDlg dlg = new PenModifyDlg(Root);
            dlg.Text = Root.Local.BoardCustColorModifyTitle;
            Microsoft.Ink.DrawingAttributes at = new Microsoft.Ink.DrawingAttributes();

            at.Transparency = (byte)(255 - Root.Gray1[0]);
            at.Color = Color.FromArgb(Root.Gray1[0], Root.Gray1[1], Root.Gray1[2], Root.Gray1[3]);
            at.Width = 0;

            if (dlg.ModifyPen(ref at))
            {
                Root.Gray1[0] = 255 - at.Transparency;
                Root.Gray1[1] = at.Color.R;
                Root.Gray1[2] = at.Color.G;
                Root.Gray1[3] = at.Color.B;
                BoardCustColorPnl.BackColor = Color.FromArgb(Root.Gray1[0], at.Color);
            }
            dlg.Dispose();
        }

        private void WsUrlTxt_TextChanged(object sender, EventArgs e)
        {
            Root.ObsUrl = WsUrlTxt.Text;
        }

        private void WsPwdTxt_TextChanged(object sender, EventArgs e)
        {
            Root.ObsPwd = WsPwdTxt.Text;
        }

        private void FfmpegCmdTxt_TextChanged(object sender, EventArgs e)
        {
            Root.FFMpegCmd = FfmpegCmdTxt.Text;
        }

        private void VideoOption_Changed(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
                Root.VideoRecordMode = (VideoRecordMode)Int32.Parse((string)(sender as Control).Tag);
            M3UOptions.Enabled = Root.IsVideoRecordingSelected() && Root.CreateM3U;
            Root.UnsetHotkey();
            Root.SetHotkey();
        }

        private void ToolbarDwg_Click(object sender, EventArgs e)
        {
            PenModifyDlg dlg = new PenModifyDlg(Root);
            dlg.Text = "";
            Microsoft.Ink.DrawingAttributes at = new Microsoft.Ink.DrawingAttributes();

            at.Transparency = (byte)(255 - Root.ToolbarBGColor[0]);
            at.Color = Color.FromArgb(Root.ToolbarBGColor[0], Root.ToolbarBGColor[1], Root.ToolbarBGColor[2], Root.ToolbarBGColor[3]);
            at.Width = 0;

            if (dlg.ModifyPen(ref at))
            {
                Root.ToolbarBGColor[0] = 255 - at.Transparency;
                Root.ToolbarBGColor[1] = at.Color.R;
                Root.ToolbarBGColor[2] = at.Color.G;
                Root.ToolbarBGColor[3] = at.Color.B;
                ToolbarDwg.BackColor = Color.FromArgb(Root.ToolbarBGColor[0], at.Color);
                Clip1Btn.BackColor = ToolbarDwg.BackColor;
                Clip2Btn.BackColor = ToolbarDwg.BackColor;
                Clip3Btn.BackColor = ToolbarDwg.BackColor;
            }
            dlg.Dispose();
        }

        private void AltTabActivateCb_CheckedChanged(object sender, EventArgs e)
        {
            Root.AltTabPointer = AltTabActivateCb.Checked;
        }

        private void ClipartsSelBtn_Click(object sender, EventArgs e)
        {
            ImageLister dlg = new ImageLister(Root);
            dlg.FromClpBtn.Visible = false;
            dlg.InsertBtn.Text = Root.Local.ButtonOkText;
            dlg.AutoCloseCb.Visible = false;

            if(dlg.ShowDialog() == DialogResult.OK)
            {
                Root.ImageStampFilling = dlg.ImageStampFilling;
                Root.StampFileNames.Clear();
//                for(int i=0;i<dlg.Images.Images.Count;i++)
                foreach(ListViewItem it in dlg.ImageListViewer.Items)
                {
                    Root.StampFileNames.Add(it.ImageKey);// dlg.Images.Images.Keys[i]);
                }
                Root.FormCollection.ClipartsDlg.Initialize();
            }
            dlg.Dispose();
        }

        private void ClipBtn_Click(object sender, EventArgs e)
        {
            ImageLister dlg = new ImageLister(Root);
            dlg.FromClpBtn.Visible = false;
            dlg.LoadImageBtn.Visible = false;
            dlg.DelBtn.Visible = false;
            dlg.FillingCombo.Visible = false;
            if(dlg.ShowDialog()==DialogResult.OK)
            {
                ((Button)sender).BackgroundImage = FormCollection.getImgFromDiskOrRes(dlg.ImageStamp);
                if((string)(((Control)sender).Tag) == "1")
                    Root.ImageStamp1.ImageStamp = dlg.ImageStamp;
                else if ((string)(((Control)sender).Tag) == "2")
                    Root.ImageStamp2.ImageStamp = dlg.ImageStamp;
                else if ((string)(((Control)sender).Tag) == "3")
                    Root.ImageStamp3.ImageStamp = dlg.ImageStamp;
            }
            dlg.Dispose();
        }

        private void cbLoadSaveEnabled_CheckedChanged(object sender, EventArgs e)
        {
            Root.LoadSaveEnabled = cbLoadSaveEnabled.Checked;
        }

        private void ToolbarOrientationBtn_Click(object sender, EventArgs e)
        {
            Root.ToolbarOrientation += 1;
            if (Root.ToolbarOrientation > Orientation.max)
                Root.ToolbarOrientation = Orientation.min;
            ToolbarOrientationBtn.BackgroundImage = ToolBarOrientationIcons[Root.ToolbarOrientation];
        }

        private void FadingTimeEd_Validating(object sender, CancelEventArgs e)
        {
            float f = -1;
            TextBox tb = (TextBox)sender;
            if (float.TryParse(tb.Text, out f) && f >= 0)
            {
                Root.TimeBeforeFading = f;
                tb.BackColor = SystemColors.Window;
                e.Cancel = false;
                for (int i=0;i<Root.MaxPenCount;i++)
                {
                    if(Root.PenAttr[i].ExtendedProperties.Contains(Root.FADING_PEN))
                        Root.PenAttr[i].ExtendedProperties.Add(Root.FADING_PEN, Root.TimeBeforeFading);
                }
            }
            else
            {
                tb.BackColor = Color.Orange;
                e.Cancel = true;
            }
        }

        private void ZoomWidthEd_Validating(object sender, CancelEventArgs e)
        {
            if (Int32.TryParse(ZoomWidthEd.Text, out Root.ZoomWidth))
                ZoomWidthEd.BackColor = SystemColors.Window;
            else
                ZoomWidthEd.BackColor = Color.Orange;
        }

        private void ZoomHeightEd_Validating(object sender, CancelEventArgs e)
        {
            if (Int32.TryParse(ZoomHeightEd.Text, out Root.ZoomHeight))
                ZoomHeightEd.BackColor = SystemColors.Window;
            else
                ZoomHeightEd.BackColor = Color.Orange;
        }

        private void ZoomScaleEd_Validating(object sender, CancelEventArgs e)
        {
            if (float.TryParse(ZoomScaleEd.Text, out Root.ZoomScale))
                ZoomScaleEd.BackColor = SystemColors.Window;
            else
                ZoomScaleEd.BackColor = Color.Orange;
        }

        private void ZoomContinousCb_CheckedChanged(object sender, EventArgs e)
        {
            Root.ZoomContinous = ZoomContinousCb.Checked;
        }

        private void ZoomEnabledCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            Root.ZoomEnabled = ZoomEnabledCb.SelectedIndex;
        }

        private void hiGlobal_Enter(object sender, EventArgs e)
        {
            Root.UnsetHotkey();
        }

        private void hiGlobal_Leave(object sender, EventArgs e)
        {
            Root.SetHotkey();
        }

        private void FormOptions_Leave(object sender, EventArgs e)
        {
        }

        private void cbAllowHotkeyInPointer_CheckedChanged(object sender, EventArgs e)
		{
			Root.AllowHotkeyInPointerMode = cbAllowHotkeyInPointer.Checked;
		}

		private void hi_OnHotkeyChanged(object sender, EventArgs e)
		{
			foreach (Control c in tabPage3.Controls)
			{
				if (c.GetType() != typeof(HotkeyInputBox))
					continue;
				HotkeyInputBox hi = (HotkeyInputBox)c;

				hi.ExternalConflictFlag = false;
				foreach (Control c2 in tabPage3.Controls)
				{
					if (c2.GetType() != typeof(HotkeyInputBox))
						continue;
					if (c == c2)
						continue;
					HotkeyInputBox hi2 = (HotkeyInputBox)c2;

					if (hi.Hotkey.ConflictWith(hi2.Hotkey))
					{
						hi.ExternalConflictFlag = true;
						break;
					}
				}
			}
		}

		private void comboLanguage_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboLanguage.Text != Root.Local.GetLanguagenameByFilename(Root.Local.CurrentLanguageFile))
			{
				Root.ChangeLanguage(Root.Local.GetFilenameByLanguagename(comboLanguage.Text));
				FormOptions_LocalReload();
			}
            String local, st;
            using (var f = new StreamReader("lang/" + Root.Local.CurrentLanguageFile + ".txt"))
                local = f.ReadToEnd();
            using (var f = new StreamReader("lang/en-us.txt"))
                st = Root.CompleteConfig(f.ReadToEnd(), "\n" + local);
            if (st != "" && MessageBox.Show("The translation file seems to not include all the required translation. Do you want to add the missing entries ?", Root.Local.CurrentLanguageFile, MessageBoxButtons.YesNo) == DialogResult.Yes)
                using(var f = new StreamWriter("lang/" + Root.Local.CurrentLanguageFile + ".txt"))
                    f.Write(local + "\n" + st);
        }

        private void InverseWheelCb_CheckedChanged(object sender, EventArgs e)
        {
            Root.InverseMousewheel = InverseWheelCb.Checked;
            InverseWheelCb.Text = Root.InverseMousewheel ? Root.Local.OptionsInverseMouseWheelChecked : Root.Local.OptionsInverseMouseWheel;
        }

        private void SnapInPointerKeysChanged(object sender, EventArgs e)
        {
            if(SnapInPointerHoldCb.SelectedIndex>=0)
                Root.SnapInPointerHoldKey = (SnapInPointerKeys)(SnapInPointerHoldCb.SelectedIndex);
            if(SnapInPointerTwiceCb.SelectedIndex>=0)
                Root.SnapInPointerPressTwiceKey = (SnapInPointerKeys)(SnapInPointerTwiceCb.SelectedIndex);
        }

        private void SubToolsBar_cb_CheckedChanged(object sender, EventArgs e)
        {
            Root.SubToolsEnabled = SubToolsBar_cb.Checked;
        }

        private void FormOptions_FormClosed(object sender, FormClosedEventArgs e)
        {
            GC.Collect();
        }

        private void ActivateDbgWinBtn_Click(object sender, EventArgs e)
        {
            Program.ShowWindow(Program.GetConsoleWindow(), 1);
            Console.WriteLine("Debug Window activated");
        }

        private void APIRestEd_Validating(object sender, CancelEventArgs e)
        {
            if (Root.APIRest.ChangeAddress(APIRestEd.Text))
            {
                e.Cancel = false;
                APIRestEd.BackColor = Color.White;
                Root.APIRestUrl = APIRestEd.Text;
            }
            else
            {
                e.Cancel = true;
                APIRestEd.BackColor = Color.Orange;
                Root.APIRest.ChangeAddress(Root.APIRestUrl);
            }
        }

        private void APIRestEd_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar=='\r')
            {
                this.SelectNextControl(this.ActiveControl, true, true, true, true);
                (sender as TextBox).Select();
                e.Handled = true;
            }
        }

        private void FitToCurveEd_CheckedChanged(object sender, EventArgs e)
        {
            Root.FitToCurve = FitToCurveEd.Checked;
        }

        private void CaptStrokesOnlyCb_CheckedChanged(object sender, EventArgs e)
        {
            Root.StrokesOnlySnapshot = CaptStrokesOnlyCb.Checked;
        }

        private void PensOnTwoLinesCb_CheckedChanged(object sender, EventArgs e)
        {
            Root.PensOnTwoLines = PensOnTwoLinesCb.Checked;
        }

        private void MeasureEnabledCb_CheckedChanged(object sender, EventArgs e)
        {
            Root.MeasureEnabled = MeasureEnabledCb.Checked;
            MeasurementBox.Enabled = Root.MeasureEnabled;
        }

        private void Measure2ScaleEd_Validated(object sender, EventArgs e)
        {
            Root.Measure2Scale = Double.Parse(Measure2ScaleEd.Text);
        }

        private void Measure2DigEd_Validated(object sender, EventArgs e)
        {
            Root.Measure2Digits = int.Parse(Measure2DigEd.Text);
        }

        private void Measure2DigEd_Validating(object sender, CancelEventArgs e)
        {
            int tempi;
            if ((int.TryParse(((TextBox)sender).Text.Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out tempi))&& tempi>=0 && tempi<=9)
                ((TextBox)sender).BackColor = Color.White;
            else
            {
                e.Cancel = true;
                ((TextBox)sender).BackColor=Color.Orange;
                ((TextBox)sender).Select();
            }
        }

        private void Measure2UnitEd_TextChanged(object sender, EventArgs e)
        {
            Root.Measure2Unit = Measure2UnitEd.Text;
        }

        private void MeasureAngleCb_CheckedChanged(object sender, EventArgs e)
        {
            Root.MeasureAnglCounterClockwise = MeasureAngleCb.Checked;
        }

        private void ColorPickerEnaCb_CheckedChanged(object sender, EventArgs e)
        {
            Root.ColorPickerEnabled = ColorPickerEnaCb.Checked;
        }

        private void comboPensLineStyle_Changed(object sender,EventArgs e)
        {
            Panel p = (Panel)sender;
            string s = Root.NextLineStyleString(Root.LineStyleToString(Root.PenAttr[(int)p.Tag].ExtendedProperties));
            p.BackgroundImage = FormCollection.getImgFromDiskOrRes("DashStyle" + s);
            DashStyle ds = Root.LineStyleFromString(s);
            if (ds == DashStyle.Custom)
                try { Root.PenAttr[(int)p.Tag].ExtendedProperties.Remove(Root.DASHED_LINE_GUID); }catch { }
            else
                Root.PenAttr[(int)p.Tag].ExtendedProperties.Add(Root.DASHED_LINE_GUID, ds);
        }

        private void SwapSnapBehaviorsCb_CheckedChanged(object sender, EventArgs e)
        {
            Root.SwapSnapsBehaviors = !SwapSnapsBehviorsCb.Checked;
        }

        private void AltAsOneCommandCb_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                AltAsOneCommandCb.CheckState = CheckState.Indeterminate;
            //else
            //    AltAsOneCommandCb.Checked = !AltAsOneCommandCb.Checked;
        }

        private void AltAsOneCommandCb_CheckStateChanged(object sender, EventArgs e)
        {
           switch(AltAsOneCommandCb.CheckState)
           {
                case CheckState.Checked:
                    Root.AltAsOneCommand = 2;
                    break;
                case CheckState.Indeterminate:
                    Root.AltAsOneCommand = 1;
                    break;
                case CheckState.Unchecked:
                    Root.AltAsOneCommand = 0;
                    break;
           }
        }

        private void AltAsOneCommandCb_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                AltAsOneCommandCb.CheckState = CheckState.Indeterminate;
        }

        private void CbHKRot_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.Checked)
            {
                Root.LineStyleRotateEnabled |= Convert.ToUInt32((int)(cb.Tag));                
            }
            else
            {
                Root.LineStyleRotateEnabled &= 0xFF ^ Convert.ToUInt32((int)(cb.Tag));
            }
        }

        private void Click4StrokeCb_CheckedChanged(object sender, EventArgs e)
        {
            Root.ButtonClick_For_LineStyle = Click4StrokeCb.Checked;
        }

        private void SpotColorPnl_Click(object sender, EventArgs e)
        {   // Copied from  ToolbarDwg_Click
            PenModifyDlg dlg = new PenModifyDlg(Root);
            dlg.Text = "";
            Microsoft.Ink.DrawingAttributes at = new Microsoft.Ink.DrawingAttributes();

            at.Transparency = (byte)(255 - Root.SpotLightColor.A);
            at.Color = Color.FromArgb(Root.SpotLightColor.A, Root.SpotLightColor.R, Root.SpotLightColor.G, Root.SpotLightColor.B);
            at.Width = 0;

            if (dlg.ModifyPen(ref at))
            {
                Root.SpotLightColor = Color.FromArgb(255 - at.Transparency, at.Color);
                SpotColorPnl.BackColor = Root.SpotLightColor;
            }
            dlg.Dispose();
        }

        private void SpotOnAltCb_CheckedChanged(object sender, EventArgs e)
        {
            Root.SpotOnAlt = SpotOnAltCb.Checked;
        }

        private void SpotRadTb_Validated(object sender, EventArgs e)
        {
            Root.SpotLightRadius = (int)(float.Parse(SpotRadTb.Text, CultureInfo.InvariantCulture) /100.0F*System.Windows.SystemParameters.PrimaryScreenWidth);
        }

        private void NewArrowEditBtn_Click(object sender, EventArgs e)
        {
            ArrowSelDlg dlg = new ArrowSelDlg(Root);
            dlg.ShowDialog();
        }

        private void MagnetAngleEd_Validated(object sender, EventArgs e)
        {
            Root.MagneticAngle = float.Parse(MagnetAngleEd.Text);
            Root.MagneticAngleTolerance = Root.MagneticAngle * (Root.MagneticAngleTolRatio);
        }


        private void IndexOnDockUndockCb_CheckedChanged(object sender, EventArgs e)
        {
            Root.CreateIndexOnUndock = M3UIndexOnUndockCb.Checked;
        }

        private void FfmegFileNameTxt_Validated(object sender, EventArgs e)
        {
            Root.FFMpegFileName = FfmegFileNameTxt.Text;
        }

        private void IndexDefaultTxt_Validated(object sender, EventArgs e)
        {
            Root.IndexDefaultText = M3UIndexDefaultTxt.Text;
        }

        private void CreateM3u_CheckedChanged(object sender, EventArgs e)
        {
            Root.CreateM3U = CreateM3u.Checked;
            M3UOptions.Enabled = Root.IsVideoRecordingSelected() && Root.CreateM3U;
        }

        private void IndexDefaultTxt_TextChanged(object sender, EventArgs e)
        {

        }

        private void UndockOnIndexCb_CheckedChanged(object sender, EventArgs e)
        {
            Root.UndockOnIndexCreate = UndockOnM3UIndexCb.Checked;
        }

        private void NoEditM3Cb_CheckedChanged(object sender, EventArgs e)
        {
            Root.NoEditM3UEntry = NoEditM3UIndexCb.Checked;
        }

        private void tbSnapFileTemplate_TextChanged(object sender, EventArgs e)
        {
            string s = Root.ExpandVarCmd(tbSnapFileTemplate.Text, 0, 0, 0, 0);
            if (s.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                tbSnapFileTemplate.BackColor = Color.Orange;
            }
            else
            {
                tbSnapFileTemplate.BackColor = Color.White;
            }
            toolTip.SetToolTip(tbSnapFileTemplate, "ex: " + s);
        }

        private void tbSnapFileTemplate_Validating(object sender, CancelEventArgs e)
        {
            if (Root.ExpandVarCmd(tbSnapFileTemplate.Text,0,0,0,0).IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                tbSnapFileTemplate.BackColor = Color.Orange;
                e.Cancel = true;
            }
            else
            {
                tbSnapFileTemplate.BackColor = Color.White;
                e.Cancel = false;
            }
        }

        private void tbSnapFileTemplate_Validated(object sender, EventArgs e)
        {
            Root.SnapshotFileTemplate = tbSnapFileTemplate.Text;
        }

        private void tbSnapPath_TextChanged(object sender, EventArgs e)
        {
            string s = Root.ExpandVarCmd(tbSnapPath.Text, 0, 0, 0, 0).Replace("/","\\");
            if (s.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                tbSnapPath.BackColor = Color.Orange;
            }
            else
            {
                tbSnapPath.BackColor = Color.White;
            }
            toolTip.SetToolTip(tbSnapPath, "ex: " + s);
        }

        private void StartFoldedCb_CheckedChanged(object sender, EventArgs e)
        {
            Root.KeepDockedAtOpen = StartFoldedCb.Checked;
        }

        private void KeepUnfoldedPointerCb_CheckedChanged(object sender, EventArgs e)
        {
            Root.KeepUnDockedAtPointer = KeepUnfoldedPointerCb.Checked;
        }

        private void ExtraPensCb_CheckedChanged(object sender, EventArgs e)
        {
            Root.PensExtraSet = ExtraPensCb.Checked;
        }
    }
}
 