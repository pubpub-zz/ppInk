using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace gInk
{
    public static class LocalSt
    {
        public static Dictionary<int, string> KeyNames;
    }

    public class Local
	{
		Dictionary<string, string> Languages = new Dictionary<string, string>();

		public string CurrentLanguageFile;

		public string[] ButtonNamePen = new string[Root.MaxPenCount];

        public string ButtonNameToogle;
        public string ButtonNamePenwidth;
		public string ButtonNameErasor;
		public string ButtonNamePan;
        public string ButtonNameScaleRotate;
        public string ButtonNameMousePointer;
        public string ButtonNameInkVisible;
        public string ButtonNameSnapshot;
        public string ButtonNameUndo;
        public string ButtonNameRedo;
        public string ButtonNameClear;
        public string ButtonNameExit;
        public string ButtonNameDock;
        public string ButtonNameClose;
        public string ButtonNameHand;
        public string ButtonNameLine;
        public string ButtonNameRect;
        public string ButtonNameOval;
        public string ButtonNameArrow;
        public string ButtonNameNumb;
        public string ButtonNameText;
        public string ButtonNameEdit;
        public string ButtonNameMagn;
        public string ButtonNameClipArt;
        public string ButtonNameVideo;
        public string PagePrevHK;
        public string PageNextHK;
        public string SaveStroke;
        public string LoadStroke;
        public string ButtonNameZoom;
        public string ButtonNameLasso;

        public string QuestionClipArtUpdate;
        public string ButtonActivateDebug;

        public string SubToolsBarCbText;
        public string OptionPensOnTwoLinesCb;
        public string OptionsSwapSnapshotBehavior;


        public string StrokeFileExists;
        public string FileCanNotWrite;
        public string SnappingInPointerMessage;

        public string BoardTitle;
        public string BoardText;
        public string BoardTransparent;
        public string BoardWhite;
        public string BoardGray;
        public string BoardBlack;
        public string BoardLast;

        public string OptionsGeneralBoardBox;
        public string BoardCustColorModifyTitle;
        public string OptionsGeneralBoardAtOpenLbl;
        public string OptionsGeneralBoardCustColorLbl;

        public string OptionsGeneralAPIRest;

        public string OptionsZoomEnabled;

        public string OptionsZoomDim;
        public string OptionsZoomScale;
        public string OptionsZoomContinous;

        public string OptionsSpotLightBox;
        public string OptionsSpotLightRadius;
        public string OptionsSpotOnAlt;

        public string ArrowDlg;


        public string OptionMeasureGroup;
        public string OptionMeasureLenLabel;
        public string OptionMeasureAngle;
        public string OptionMeasureWhileDrawing;
        public string FormatLength;
        public string FormatRectSize;
        public string FormatEllipseSize;    
        public string FormatAngle;
        public string FormaTotalLength;

        public string ReScalePrompt;

        public string ButtonOkText;
        public string ButtonCancelText;
        public string ButtonExitText;
        public string ButtonFontText;
        public string ButtonPageNextPrev;
        public string ButtonSaveText;
        public string ButtonPrevText;
        public string ButtonNextText;
        public string ButtonAddText;
        public string ButtonDelText;
        public string DlgTextCaption;
        public string DlgTextLabel;
        public string DlgTagCaption;
        public string DlgTagLabel;
        public string TextFramingText;
        public string FormClipartsTitle;
        public string ButtonInsertText;
        public string ButtonFromClipBText;
        public string ButtonLoadImageText;
        public string ButtonDeleteText;
        public string CheckBoxAutoCloseText;
        public string PatternStoreParamTxt;
        public string ListFillingsText;
        public int LineOfPatternsListPos = 5; // provide Line Of Patterns Positions in list above


        public string MenuEntryExit;
		public string MenuEntryOptions;
        public string MenuEntryAbout;

        public string OptionsTabGeneral;
        public string OptionsTabPens;
        public string OptionsTabHotkeys;

        public string OptionsGeneralLanguage;
        public string OptionsGeneralToolBarColorText;
        public string OptionsGeneralAltTabActivateText;
        public string OptionsGeneralUnDockedInPointerText;
        public string OptionsGeneralToolbarHeight;

        public string OptionsGeneralCanvascursor;
        public string OptionsGeneralCanvascursorArrow;
        public string OptionsGeneralCanvascursorPentip;
        public string OptionsGeneralSnapshotsavepath;
        public string OptionsGeneralOpenIntoSnapMode;
        public string OptionGeneralStartFolded;
        public string OptionsGeneralWhitetrayicon;
		public string OptionsGeneralAllowdragging;
        public string OptionsGeneralShowFloatingWindow;
        public string OptionsGeneralSaveFloatingWindowPos;
        public string OptionsGeneralArrowHead;
        public string OptionsGeneralArrowHeadApt;
        public string OptionsGeneralArrowHeadLen;
        public string OptionsGeneralDefaultTextLbl;
        public string OptionsGeneralDefaultTextBtn;
        public string OptionsGeneralDefaultArrHdBtn;
        public string OptionsGeneralMagnetLbl;
        public string OptionsGeneralSaveConfigToFile;
        public string OptionsCaptureStrokesOnly;
        public string OptionsGeneralNotePenwidth;

        public string OptionsPensShow;
        public string OptionsPensColor;
        public string OptionsPensAlpha;
        public string OptionsPensWidth;
        public string OptionsPensPencil;
        public string OptionsPensHighlighter;
        public string OptionsPensFading;
        public string OptionsPensWidthAtSelection;
        public string OptionsInverseMouseWheel;
        public string OptionsInverseMouseWheelChecked;
        public string OptionsFitToCurve;
        public string OptionsClick4Stroke;

        public string QuestionMinWidth;
        public string ListEditDlgFillingsText;

        public string OptionsPensThin;
        public string OptionsPensNormal;
        public string OptionsPensThick;

        public string OptionHotkeysExtraPens;
        public string ExtraPensHint;
        public string OptionsHotKeySnapInPointerGrp;
        public string OptionsHotKeySnapInPointerLbl;
        public string OptionsHotKeySnapInPointerKeys = "None\nShift\nCtrl\nAlt";                            // Order to be respected!!!
        public string OptionsHotKeyAltAsOneCommand;
        public string OptionsHotkeysglobal;
        public string OptionsHotkeysEnableinpointer;

        public string OptionsHotkeysPenWidthPlus;
        public string OptionsHotkeysPenWidthMinus;
        public string OptionsHotkeysColorPicker;
        public string OptionsHotkeysColorEdit;
        public string OptionsLineStyle;

        public string VideoTab;
        public string OptNoVideo;
        public string OptObsRecord;
        public string OptObsBcast;
        public string LblWsUrl;
        public string LblWsPwd;
        public string LblObsNote;
        public string OptFfmpeg;
        public string LblFfmpegCmd;
        public string LblFfmpegNote;
        public string NotificationSnapshot;

        public string PanSubToolsHints;
        public string ScaleSubToolsHints;
        public string HandSubToolsHints;
        public string LineSubToolsHints;
        public string RectSubToolsHints;
        public string OvalSubToolsHints;
        public string NumberSubToolsHints;
        public string ArrowSubToolsHints;
        public string TextSubToolsHints;

        public string M3UTextCaption;
        public string M3UTextLabel;
        public string M3UBalloonText;
        public string CreateM3UGroup;
        public string CreateIndexOnUndock;
        public string LblM3UIndexHotKey;
        public string M3UIndexDefaultText;
        public string UndockOnM3UIndexCreate;
        public string NoEditM3UIndex;



        public string KeyNamesStr = "0x00=None \n0x01=LButton \n0x02=RButton \n0x03=Cancel \n0x04=MButton \n0x05=XButton1 \n0x06=XButton2 \n0x08=Back \n0x09=Tab \n0x0A=LineFeed \n0x0C=Clear \n0x0D=Enter \n0x10=Shift \n0x11=Control \n0x12=Menu \n0x13=Pause \n0x14=CapsLock \n0x15=KanaMode \n0x17=JunjaMode \n0x18=FinalMode \n0x19=KanjiMode \n0x1B=Escape \n0x1C=IMEConvert \n0x1D=IMENonconvert \n0x1E=IMEAccept \n0x1F=IMEModeChange \n0x20=Space \n0x21=PageUp \n0x22=PageDown \n0x23=End \n0x24=Home \n0x25=Left \n0x26=Up \n0x27=Right \n0x28=Down \n0x29=Select \n0x2A=Print \n0x2B=Execute \n0x2C=PrintScreen \n0x2D=Insert \n0x2E=Delete \n0x2F=Help \n0x30=0 \n0x31=1 \n0x32=2 \n0x33=3 \n0x34=4  \n0x35=5 \n0x36=6 \n0x37=7 \n0x38=8  \n0x39=9 \n0x41=A \n0x42=B \n0x43=C \n0x44=D \n0x45=E \n0x46=F \n0x47=G \n0x48=H \n0x49=I \n0x4A=J \n0x4B=K \n0x4C=L \n0x4D=M \n0x4E=N \n0x4F=O \n0x50=P \n0x51=Q \n0x52=R \n0x53=S \n0x54=T \n0x55=U \n0x56=V \n0x57=W \n0x58=X \n0x59=Y \n0x5A=Z \n0x5B=LWin \n0x5C=RWin \n0x5D=Apps \n0x5F=Sleep \n0x60=NumPad0 \n0x61=NumPad1 \n0x62=NumPad2 \n0x63=NumPad3 \n0x64=NumPad4 \n0x65=NumPad5 \n0x66=NumPad6 \n0x67=NumPad7 \n0x68=NumPad8 \n0x69=NumPad9 \n0x6A=Multiply \n0x6B=Add \n0x6C=Separator \n0x6D=Subtract \n0x6E=Decimal \n0x6F=Divide \n0x70=F1 \n0x71=F2 \n0x72=F3 \n0x73=F4 \n0x74=F5 \n0x75=F6 \n0x76=F7 \n0x77=F8 \n0x78=F9 \n0x79=F10 \n0x7A=F11 \n0x7B=F12 \n0x7C=F13 \n0x7D=F14 \n0x7E=F15 \n0x7F=F16 \n0x80=F17 \n0x81=F18 \n0x82=F19 \n0x83=F20 \n0x84=F21 \n0x85=F22 \n0x86=F23 \n0x87=F24 \n0x90=NumLock \n0x91=Scroll \n0xA0=LShiftKey \n0xA1=RShiftKey \n0xA2=LControlKey \n0xA3=RControlKey \n0xA4=LMenu \n0xA5=RMenu \n0xA6=BrowserBack \n0xA7=BrowserForward \n0xA8=BrowserRefresh \n0xA9=BrowserStop \n0xAA=BrowserSearch \n0xAB=BrowserFavorites \n0xAC=BrowserHome \n0xAD=VolumeMute \n0xAE=VolumeDown \n0xAF=VolumeUp \n0xB0=MediaNextTrack \n0xB1=MediaPreviousTrack \n0xB2=MediaStop \n0xB3=MediaPlayPause \n0xB4=LaunchMail \n0xB5=SelectMedia \n0xB6=LaunchApplication1 \n0xB7=LaunchApplication2 \n0xBA=OemSemicolon \n0xBB=Oemplus \n0xBC=Oemcomma \n0xBD=OemMinus \n0xBE=OemPeriod \n0xBF=OemQuestion \n0xC0=Oemtilde \n0xDB=OemOpenBrackets \n0xDC=OemPipe \n0xDD=OemCloseBrackets \n0xDE=OemQuotes \n0xDF=Oem8 \n0xE2=OemBackslash \n0xE5=ProcessKey \n0xE7=Packet \n0xF6=Attn \n0xF7=Crsel \n0xF8=Exsel \n0xF9=EraseEof \n0xFA=Play \n0xFB=Zoom \n0xFC=NoName \n0xFD=Pa1 \n0xFE=OemClear \n0x00010000=Shift \n0x00020000=Control \n0x00040000=Alt";

        public Dictionary<int, string> KeyNames = new Dictionary<int, string>();

        public Local()
		{
			ButtonNamePen[0] = "Pen 0";
			ButtonNamePen[1] = "Pen 1";
			ButtonNamePen[2] = "Pen 2";
			ButtonNamePen[3] = "Pen 3";
			ButtonNamePen[4] = "Pen 4";
			ButtonNamePen[5] = "Pen 5";
			ButtonNamePen[6] = "Pen 6";
			ButtonNamePen[7] = "Pen 7";
			ButtonNamePen[8] = "Pen 8";
			ButtonNamePen[9] = "Pen 9";
            ButtonNamePen[10] = "Pen 10";
            ButtonNamePen[11] = "Pen 11";
            ButtonNamePen[12] = "Pen 12";
            ButtonNamePen[13] = "Pen 13";
            ButtonNamePen[14] = "Pen 14";
            ButtonNamePen[15] = "Pen 15";
            ButtonNamePen[16] = "Pen 16";
            ButtonNamePen[17] = "Pen 17";
            ButtonNamePen[18] = "Pen 18";
            ButtonNamePen[19] = "Pen 19";

            LoadKeyNames();

			LoadLocalList();
		}

        private void LoadKeyNames()
        {
            System.ComponentModel.Int32Converter conv = new System.ComponentModel.Int32Converter();
            foreach (string st1 in KeyNamesStr.Split('\n'))
            {
                string[] st = st1.Trim().Split(new char[] { '=' },2);
                KeyNames[(int)(conv.ConvertFromString(st[0]))]=st[1];
            }
            LocalSt.KeyNames = KeyNames;
        }

		public void LoadLocalList()
		{
			DirectoryInfo d = new DirectoryInfo("./lang/");
			if (!d.Exists)
				d = new DirectoryInfo(Program.RunningFolder + "lang");
            if (!d.Exists)
                d = new DirectoryInfo(Program.ProgramFolder + "lang");
            if (!d.Exists)
				return;

			FileInfo[] Files = d.GetFiles("*.txt");
			foreach (FileInfo file in Files)
			{
				FileStream fini = new FileStream(file.FullName, FileMode.Open);
				StreamReader srini = new StreamReader(fini);
				string sLine;
				do
				{
					sLine = srini.ReadLine();
				}
				while (sLine != null && !sLine.StartsWith("LanguageName"));
				if (sLine == null)
					continue;
				string sPara = sLine.Substring(sLine.IndexOf("=") + 1);
				sPara = sPara.Trim();
				sPara = sPara.Trim('\"');
				string languagename = sPara;

				Languages.Add(file.Name.Substring(0, file.Name.Length - 4), sPara);

				fini.Close();
			}
        }

        public string ShortTxt(string sin)
        {
            int i = sin.IndexOf("(");
            if (i < 0) i = sin.Length;
            return sin.Substring(0, i);
        }


        public List<string> GetLanguagenames()
		{
			List<string> names = new List<string>();
			foreach (KeyValuePair<string, string> pair in Languages)
				names.Add(pair.Value);

			return names;
		}

		public string GetFilenameByLanguagename(string languagename)
		{
			foreach (KeyValuePair<string, string> pair in Languages)
				if (pair.Value == languagename)
					return pair.Key;

			return "";
		}

		public string GetLanguagenameByFilename(string filename)
		{
			foreach (KeyValuePair<string, string> pair in Languages)
				if (pair.Key == filename)
					return pair.Value;

			return "";
		}

        public void LoadLocalFile(string loname)
        {
            //
            LoadLocalStream(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(Properties.Resources.en_us))));
                        
            string filename = "./lang/" + loname + ".txt";

            if (!File.Exists(filename))
                filename = Program.RunningFolder + "lang/" + loname + ".txt";
            if (!File.Exists(filename))
                filename = Program.ProgramFolder + "lang/" + loname + ".txt";
            if (!File.Exists(filename))
                return;

            FileStream fini = new FileStream(filename, FileMode.Open);
            StreamReader srini = new StreamReader(fini);
            LoadLocalStream(srini);
            fini.Close();
            CurrentLanguageFile = loname;
        }

        public void LoadLocalStream(StreamReader srini)
        {
			string sLine = "";
			string sName = "", sPara = "";
			while (sLine != null)
			{
				sLine = srini.ReadLine();
				if
				(
					sLine != null &&
					sLine != "" &&
					sLine.Substring(0, 1) != "-" &&
					sLine.Substring(0, 1) != "%" &&
					sLine.Substring(0, 1) != "'" &&
					sLine.Substring(0, 1) != "/" &&
					sLine.Substring(0, 1) != "!" &&
					sLine.Substring(0, 1) != "[" &&
					sLine.Substring(0, 1) != "#" &&
                    sLine.Contains("=")
                    //sLine.Contains("=") &&
                    //!sLine.Substring(sLine.IndexOf("=") + 1).Contains("=")
                )
				{
                    sName = sLine.Substring(0, sLine.IndexOf("="));
					sName = sName.Trim();
					sPara = sLine.Substring(sLine.IndexOf("=") + 1).Replace("\\n","\n");
					sPara = sPara.Trim();
					sPara = sPara.Trim('\"');

					if (sName.StartsWith("ButtonNamePen"))
					{
						int penid = 0;
						if (int.TryParse(sName.Substring(13), out penid))
						{
							ButtonNamePen[penid] = sPara;
						}
					}

					System.Reflection.FieldInfo fi = typeof(Local).GetField(sName);
					if (fi != null)
						fi.SetValue(this, sPara);
				}
			}
			//fini.Close();
            LoadKeyNames();
			//CurrentLanguageFile = loname;
		}
	}
}
