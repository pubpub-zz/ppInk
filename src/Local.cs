using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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

        public string[] ButtonNamePen = new string[10];

        public string ButtonNamePenwidth = "Pen width";
        public string ButtonNameErasor = "Eraser";
        public string ButtonNamePan = "move one drawing | Pan all drawings";
        public string ButtonNameMousePointer = "Mouse pointer";
        public string ButtonNameInkVisible = "Ink visible";
        public string ButtonNameSnapshot = "Snapshot";
        public string ButtonNameUndo = "Undo";
        public string ButtonNameRedo = "Redo";
        public string ButtonNameClear = "Clear";
        public string ButtonNameExit = "Exit drawing";
        public string ButtonNameDock = "Dock/Undock";
        public string ButtonNameClose = "Close\n(in addition to Alt+F4)";
        public string ButtonNameHand = "Handfree drawing (standard | color filled | White filled | Black filled)";
        public string ButtonNameLine = "Line Shape";
        public string ButtonNameRect = "Rectangle shape (empty | color filled | White filled | Black filled)";
        public string ButtonNameOval = "Ellipsis shape (empty | color filled | White filled | Black filled)";
        public string ButtonNameArrow = "Arrow shape (head at start | at end)";
        public string ButtonNameNumb = "Numbered chip (put | reset number)";
        public string ButtonNameText = "Text (Left|Right aligned)";
        public string ButtonNameEdit = "Edit Text|chip";
        public string ButtonNameMagn = "Magnetic Effect";
        public string ButtonNameClipArt = "ClipArt";
        public string ButtonNameVideo = "Video Record";
        public string SaveStroke = "Save Strokes\n(long =opendialog ; short = {0})";
        public string LoadStroke = "Load Strokes\n(long =opendialog ; short = {0})";
        public string ButtonNameZoom = "Zoom";

        public string StrokeFileExists = "{0} Already Exists\nDo you want to override it?";
        public string FileCanNotWrite = "{0}\ncan not be written.";

        public string BoardTitle = "Clean Desktop";
        public string BoardText = "Erase all drawings and fill it with...";
        public string BoardTransparent = "Transparent";
        public string BoardWhite = "White";
        public string BoardGray = "Customed ";
        public string BoardBlack = "Black";
        public string BoardLast = "Last selected";

        public string OptionsGeneralBoardBox = "Background Board";
        public string BoardCustColorModifyTitle = "Board Customed color";
        public string OptionsGeneralBoardAtOpenLbl = "Background at toolbar opening";
        public string OptionsGeneralBoardCustColorLbl = "Customed Background color";

        public string OptionsZoomEnabled = "0 - No Zoom\n1 - Magnifier\n2 - Frozen Area\n3 - Both";

        public string OptionsZoomDim = "Dim.";
        public string OptionsZoomScale = "Scale";
        public string OptionsZoomContinous = "Continous";


        public string ButtonOkText = "&OK";
        public string ButtonCancelText = "&Cancel";
        public string ButtonFontText = "&Font";
        public string DlgTextCaption = "Edit Text";
        public string DlgTextLabel = "Text Input";
        public string DlgTagCaption = "Tag Numbering";
        public string DlgTagLabel = "Enter Starting Number";
        public string TextFramingText = "Transparent background;Frame + Transparent background;White Background;Frame + White Background;Black Background;Frame + Black Background";
        public string FormClipartsTitle = "ClipArts";
        public string ButtonInsertText = "&Insert";
        public string ButtonFromClipBText = "&From Clipboard";
        public string ButtonLoadImageText = "&Load Image";
        public string ButtonDeleteText = "&Delete entry";
        public string CheckBoxAutoCloseText = "Auto\nClose";
        public string ListFillingsText = "No Frame;Empty;Color filled;White filled;Black filled";


        public string MenuEntryExit = "Exit";
        public string MenuEntryOptions = "Options";
        public string MenuEntryAbout = "About";

        public string OptionsTabGeneral = "General";
        public string OptionsTabPens = "Pens";
        public string OptionsTabHotkeys = "Hotkeys";

        public string OptionsGeneralLanguage = "Language";
        public string OptionsGeneralToolBarColorText = "Click on toolbar to select background color";
        public string OptionsGeneralAltTabActivateText = "Engage Pointer and Auto-Fold with Alt+Tab";
        public string OptionsGeneralToolbarHeight = "height(%scr)\n\n\nchanges after closing toolbar";

        public string OptionsGeneralCanvascursor = "Canvus cursor";
        public string OptionsGeneralCanvascursorArrow = "Arrow";
        public string OptionsGeneralCanvascursorPentip = "Pen tip";
        public string OptionsGeneralSnapshotsavepath = "Snapshot save path";
        public string OptionsGeneralOpenIntoSnapMode = "Start Snapshot Capture immediately after Opening Toolbar";
        public string OptionsGeneralWhitetrayicon = "Use white tray icon";
        public string OptionsGeneralAllowdragging = "Allow dragging toolbar";
        public string OptionsGeneralShowFloatingWindow = "Show Floating Window (at next restart)";
        public string OptionsGeneralSaveFloatingWindowPos = "Save Floating &window Pos";
        public string OptionsGeneralArrowHead = "ArrowHead";
        public string OptionsGeneralArrowHeadApt = "Aperture(°)";
        public string OptionsGeneralArrowHeadLen = "Length(%Scr)";
        public string OptionsGeneralDefaultTextLbl = "Default Text";
        public string OptionsGeneralDefaultTextBtn = "Select &Font && Size";
        public string OptionsGeneralDefaultArrHdBtn = "Default Arrow Head At Start";
        public string OptionsGeneralMagnetLbl = "Magnetic radius(%Scr) (<=0 = disabled)";
        public string OptionsGeneralSaveConfigToFile = "Save to Files";
        public string OptionsGeneralNotePenwidth = "Notes: (1)pen width panel overides each individual pen width settings\n  (2) Transparency and size of floating window to be modified directly in config.ini";

        public string OptionsPensShow = "Show";
        public string OptionsPensColor = "Color";
        public string OptionsPensAlpha = "Alpha";
        public string OptionsPensWidth = "Width";
        public string OptionsPensPencil = "Pencil";
        public string OptionsPensHighlighter = "Highlighter";
        public string OptionsPensFading = "Fading (after...sec)";
        public string OptionsPensWidthAtSelection = "Apply Width on Pen Selection\n(even with width selector)";
        public string OptionsInverseMouseWheel = "Inverse MouseWheel control\nWheel = Pen Width\nShift+Wheel = Pen Selection";
        public string OptionsInverseMouseWheelChecked = "Inverse MouseWheel control\nWheel = Pen Selection\nShift+Wheel = Pen Width";

        public string OptionsPensThin = "Thin";
        public string OptionsPensNormal = "Normal";
        public string OptionsPensThick = "Thick";

        public string OptionsHotKeySnapInPointerGrp = "Snaphsot in Pointer Mode";
        public string OptionsHotKeySnapInPointerLbl = "press and hold....                .... and press twice\n\n\nBoth None = Disabled";
        public string OptionsHotKeySnapInPointerKeys = "None\nShift\nCtrl\nAlt";                            // Order to be respected!!!
        public string OptionsHotKeyAltAsOneCommand = "Process Alt as Temporary Command (Alt will be ignored in hotkeys)";
        public string OptionsHotkeysglobal = "Global hotkey (start drawing, switch between mouse pointer and drawing)";
        public string OptionsHotkeysEnableinpointer = "Enable all following hotkeys in mouse pointer mode (may cause a mess)";

        public string VideoTab = "Video";
        public string OptNoVideo = "No video recording";
        public string OptObsRecord = "OBS recording";
        public string OptObsBcast = "OBS broadcasting";
        public string LblWsUrl = "WebSocket URL";
        public string LblWsPwd = "Password";
        public string LblObsNote = "Note : OBS should be started before starting record\nOBS should be installed with Websocket plugin,with port and password configured";
        public string OptFfmpeg = "FFmpeg recording";
        public string LblFfmpegCmd = "Command Line";
        public string LblFfmpegNote = "Note :substitution in command line\n" +
                                      "            $xx$ $yy$                                             : upper left corner\n" +
                                      "            $ww$ $hh$                                             : width and height of capture\n" +
                                      "            $DD$ $MM$ $YY$ $YYYY$               : date\n" +
                                      "            $H$ $M$ $S$                                      : time\n" +
                                      "            %DD% %MM% %YY% %YYYY%     : date at ppInk startup\n" +
                                      "            %H% %M% %S%                               : time at ppInk startup\n" +
                                      "            $nn$                                                      : counter(restarted at 1 at ppInk restart)\n" +
                                      "            %VAR%                                                : use of environment variable VAR";
        public string NotificationSnapshot = "Snapshot saved. Click here to browse snapshots.";

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

            LoadKeyNames();

            LoadLocalList();
        }

        private void LoadKeyNames()
        {
            System.ComponentModel.Int32Converter conv = new System.ComponentModel.Int32Converter();
            foreach (string st1 in KeyNamesStr.Split('\n'))
            {
                string[] st = st1.Trim().Split(new char[] { '=' }, 2);
                KeyNames[(int)(conv.ConvertFromString(st[0]))] = st[1];
            }
            LocalSt.KeyNames = KeyNames;
        }

        public void LoadLocalList()
        {
            DirectoryInfo d = new DirectoryInfo("./lang/");
            if (!d.Exists)
                d = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "lang");
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
            string filename = "./lang/" + loname + ".txt";

            if (!File.Exists(filename))
                filename = AppDomain.CurrentDomain.BaseDirectory + "lang/" + loname + ".txt";
            if (!File.Exists(filename))
                return;

            FileStream fini = new FileStream(filename, FileMode.Open);
            StreamReader srini = new StreamReader(fini);
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
                    sPara = sLine.Substring(sLine.IndexOf("=") + 1).Replace("\\n", "\n");
                    sPara = sPara.Trim();
                    sPara = sPara.Trim('\"');

                    if (sName.StartsWith("ButtonNamePen"))
                    {
                        int penid = 0;
                        if (int.TryParse(sName.Substring(13, 1), out penid))
                        {
                            ButtonNamePen[penid] = sPara;
                        }
                    }

                    System.Reflection.FieldInfo fi = typeof(Local).GetField(sName);
                    if (fi != null)
                        fi.SetValue(this, sPara);
                }
            }
            fini.Close();
            LoadKeyNames();
            CurrentLanguageFile = loname;
        }
    }
}
