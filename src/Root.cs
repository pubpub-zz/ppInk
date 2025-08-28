using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Ink;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Drawing.Drawing2D;
using gInk.Apng;

namespace gInk
{
    public static class Global
    {
        public static string ProgramFolder = "";
    }

    public class Tools
    {
        public const int Invalid = -1;
        public const int Hand = 0; public const int Line = 1; public const int Rect = 2; public const int Oval = 3;
        public const int StartArrow = 4; public const int EndArrow = 5; public const int NumberTag = 6;
        public const int Edit = 7; public const int txtLeftAligned = 8; public const int txtRightAligned = 9;
        public const int Move = 10; public const int Copy = 11; public const int Scale = 12; public const int Rotate = 13;
        public const int Poly = 21; public const int ClipArt = 22; public const int PatternLine = 23;
        public static readonly int[] All = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 21, 22, 23 };
        public static readonly string[] Names = { "Hand", "Line", "Rect", "Oval", "StartArrow", "EndArrow", "Numbering", "Edit", "Text Left Aligned", "Text Right Aligned",
                                                  "Move", "Copy", "Resize", "Rotate", "PolyLine", "ClipArt", "PatternOnStroke"};
    }
    public class Filling {
        public const int NoFrame = -1;      // for Stamps
        public const int Empty = 0;
        public const int PenColorFilled = 1;
        public const int Outside = 2;
        public const int WhiteFilled = 3;
        public const int BlackFilled = 4;
        public const int Modulo = 5;
        public const int Invalid = Modulo + 1;
        public static readonly string[] Names = { "NoFrames","Empty", "Pen Colored", "Outside", "White Colored", "Black Colored" };  //starting at -1
    } // applicable to Hand,Rect,Oval

    public class Orientation{
        public const int min = 0;
        public const int toLeft = 0;    // original
        public const int toRight = 1;
        public const int Horizontal = 1;
        public const int Vertical = 2;
        public const int toUp = 2;
        public const int toDown = 3;
        public const int max = 3;
    }

    public class ClipArtData
    {
        public string ImageStamp;
        public int X=-1;
        public int Y=-1;
        public int Wstored=-1;
        public int Hstored=-1;
        public int Filling=gInk.Filling.NoFrame;
        public bool PatternLine=false;
        public double Distance=double.MaxValue;
        public bool Store = false;
        public ClipArtData Clone() { return new ClipArtData() { ImageStamp = ImageStamp, X = X, Y = Y, Wstored = Wstored, Hstored = Hstored, Filling = Filling, PatternLine = PatternLine, Distance = Distance, Store = Store }; }
    };

    public enum VideoRecordMode {NoVideo=0 , OBSRec=1 , OBSBcst=2 , FfmpegRec=3 };
    public enum VideoRecInProgress { Dead = -2, Unknown = -1, Stopped=0, Starting=1, Recording=2, Stopping = 3, Pausing=4, Paused=5, Resuming=6, Streaming = 7 };

    public enum SnapInPointerKeys { None=0, Shift=1, Control=2, Alt=3 };

    public class AnimationStructure
    {
        public ApngImage Image;
        public long Idx;
        public DateTime T0;
        public bool DeleteAtDend;
        public bool DeleteRequested;
        public DateTime TEnd;
        public int Loop;
    }



    public class TestMessageFilter : IMessageFilter
	{
		public Root Root;

		public TestMessageFilter(Root root)
		{
			Root = root;
		}

		public bool PreFilterMessage(ref Message m)
		{
			if (m.Msg == 0x0312 || m.Msg == Program.StartInkingMsg)                    // 0x0312 is the global hotkey WM_HOTKEY
            {
                //Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);                  // The key of the hotkey that was pressed.
                //int modifier = (int)m.LParam & 0xFFFF;       // The modifier of the hotkey that was pressed.
                int id = (m.Msg == 0x0312)?m.WParam.ToInt32():0;                                        // The id of the hotkey that was pressed.
                if(id ==1)      // Hotkey_CreateIndex;
                {
                    Root?.FormCollection?.AddM3UEntry(null);
                    return true;
                }
                else
                {
                    bool activePointer = (m.Msg == 0x0312 && (Root.FormCollection != null && Root.FormCollection.Visible));
                    Root.callshortcut();
                    if (activePointer)           // StartInkingMsg is received twice, therefore we have to froce pointerMode at that time...
                    {
                        Root.FormCollection.btPointer_Click(null, null);
                        if (Root.AltTabPointer && !Root.PointerMode && !Root.IsDockedBeforePen && !Root.FormCollection.Initializing) // to unfold the bar if AltTabPointer option has been set
                        {
                            Root.UnDock();
                        }

                    }
                    return true;
                }
            }
			return false;
		}
	}

    public static class globalRoot {
        public static bool HideInAltTab=true;
    };

    public class Root
    {
        public Local Local = new Local();
        public const int MaxPenCount = 20;
        public const int MaxDisplayedPens = 10;
        public const int SavedPenDA = MaxPenCount;
        public const int LassoPercent = 80;

        //public Guid TYPE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 0);
        public static readonly Guid TEXT_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 1);
        public static readonly Guid TEXTX_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 2);
        public static readonly Guid TEXTY_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 3);
        public static readonly Guid TEXTHALIGN_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 4);
        public static readonly Guid TEXTVALIGN_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 5);
        public static readonly Guid TEXTFONT_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 6);
        public static readonly Guid TEXTFONTSIZE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 7);
        public static readonly Guid TEXTFONTSTYLE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 8);
        public static readonly Guid TEXTWIDTH_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 9);
        public static readonly Guid TEXTHEIGHT_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 10);

        public static readonly Guid ISDELETION_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 1, 0);
        public static readonly Guid ISSTROKE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 1, 1);
        public static readonly Guid ISTAG_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 1, 2);
        public static readonly Guid ISLASSO_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 1, 3);
        //not yet used : 
        //public Guid ISRECT_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 1, 2);
        //public Guid ISOVAL_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 1, 3);

        public static readonly Guid ISFILLEDCOLOR_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 12, 1);
        public static readonly Guid ISFILLEDWHITE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 12, 2);
        public static readonly Guid ISFILLEDBLACK_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 12, 3);
        public static readonly Guid ISFILLEDOUTSIDE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 12, 4);
        public static readonly Guid IMAGE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 4);
        public static readonly Guid IMAGE_X_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 5);
        public static readonly Guid IMAGE_Y_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 6);
        public static readonly Guid IMAGE_W_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 7);
        public static readonly Guid IMAGE_H_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 8);
        public static readonly Guid ISHIDDEN_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 10);
        public static readonly Guid ISBACKGROUND_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 11);
        public static readonly Guid ANIMATIONFRAMEIMG_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 12);
        public static readonly Guid IMAGE_ON_LINE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 19);
        public static readonly Guid REPETITIONDISTANCE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 20);
        public static readonly Guid LISTOFPOINTS_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 21);
        public static readonly Guid ARROWSTART_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 22);    // pointing head
        public static readonly Guid ARROWSTART_X_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 23);    // pointing head
        public static readonly Guid ARROWSTART_Y_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 24);    // pointing head
        public static readonly Guid ARROWSTART_FN_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 25);    // Original FileName
        public static readonly Guid ARROWEND_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 32);      // tail
        public static readonly Guid ARROWEND_X_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 33);      // tail
        public static readonly Guid ARROWEND_Y_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 34);      // tail
        public static readonly Guid ARROWEND_FN_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 35);    // 

        public static readonly Guid FADING_PEN = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 3, 1);
        public static readonly Guid DASHED_LINE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 3, 2);        // will contain DashStyle style
        public static readonly Guid ROTATION_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 3, 3);           // applies to both Text and Images

        public static int MIN_MAGNETIC = 25;
        // options
        public bool ReinitForms = false;
        public int ToolbarOrientation = Orientation.toLeft;
        public bool[] PenEnabled = new bool[MaxPenCount];
        public bool PensOnTwoLines = true;
        public bool PensExtraSet = true;
        public bool ToolsEnabled = true;
        public bool EraserEnabled = true;
        public bool PointerEnabled = true;
        public bool AltTabPointer = false;
        public bool PenWidthEnabled = false;
        public bool WidthAtPenSel = true;
        public bool SnapEnabled = true;
        public bool UndoEnabled = true;
        public bool ClearEnabled = true;
        public bool PagesEnabled = false;
        public bool LoadSaveEnabled = true;
        public bool PanEnabled = true;
        public bool InkVisibleEnabled = true;
        public DrawingAttributes[] PenAttr = new DrawingAttributes[MaxPenCount+1]; //+1 for SavedPenDA
        public bool AutoScroll;
        public bool WhiteTrayIcon;
        public string SnapshotBasePath;
        public string SaveStrokesPath;
        public bool AutoSaveStrokesAtExit;
        public bool SwapSnapsBehaviors=false;
        public int CanvasCursor = 0;
        public bool AllowDraggingToolbar = true;
        public bool AllowHotkeyInPointerMode = true;
        public int gpButtonsLeft, gpButtonsTop;

        // advanced options
        public string CloseOnSnap = "blankonly";
        public bool AlwaysHideToolbar = false;
        public bool KeepDockedAtOpen = false;
        public bool KeepUnDockedAtPointer = false;
        public float ToolbarHeight = 0.06f;
        public int AltAsOneCommand = 2;

        public Color WindowModeBorderUnselected = Color.DarkGray;
        public Color WindowModeBorderSelected = Color.Black;

        public string cursorarrowFileName = "cursorarrow";
        public string cursortargetFileName = "cursortarget";
        public string cursoreraserFileName = "cursoreraser";
        public string cursorsnapFileName = "cursorsnap";
        public string cursorredFileName = "cursorred";

        public int CursorX, CursorY;
        public int CursorX0 = int.MinValue, CursorY0 = int.MinValue;

        public double LongClickTime = 1.0;

        // the two grays for "white board" effect
        public int[] Gray1 = new int[] { 80, 150, 150, 150 };
        public int[] Gray2 = new int[] {100, 100, 100, 100};
        public int[] ToolbarBGColor = new int[] { 245, 245, 245, 0 };
        public int BoardAtOpening = 0;      // 0:Transparent/1:White/2:Customed/3:Black/4:AtSelection
        public int BoardSelected = 0;       // by default transparent

        //measurement tools
        public static bool MeasureEnabled = true;
        public static double Measure2Scale = 1.0;
        public static int Measure2Digits=1;
        public static string Measure2Unit = "Pixel";
        public static bool MeasureAnglCounterClockwise = true;
        public static bool MeasureWhileDrawing = false;
        public static bool Measure_Save_Scale = true;


        // hotkey options
        public Hotkey Hotkey_Global = new Hotkey();
		public Hotkey[] Hotkey_Pens = new Hotkey[MaxDisplayedPens];
        public Hotkey Hotkey_FadingToggle = new Hotkey();

        public Hotkey Hotkey_PenWidthPlus = new Hotkey();
        public Hotkey Hotkey_PenWidthMinus = new Hotkey();

        public Hotkey Hotkey_Eraser = new Hotkey();
		public Hotkey Hotkey_InkVisible = new Hotkey();
		public Hotkey Hotkey_Pointer = new Hotkey();
		public Hotkey Hotkey_Pan = new Hotkey();
		public Hotkey Hotkey_Undo = new Hotkey();
		public Hotkey Hotkey_Redo = new Hotkey();
		public Hotkey Hotkey_Snap = new Hotkey();
		public Hotkey Hotkey_Clear = new Hotkey();
        public Hotkey Hotkey_Video = new Hotkey();
        public Hotkey Hotkey_DockUndock = new Hotkey();
        public Hotkey Hotkey_Close = new Hotkey();
        public Hotkey Hotkey_SnapClose = new Hotkey(); // to keep Esc to close in snapping;

        public Hotkey Hotkey_Hand = new Hotkey();
        public Hotkey Hotkey_Line = new Hotkey();
        public Hotkey Hotkey_Rect = new Hotkey();
        public Hotkey Hotkey_Oval = new Hotkey();
        public Hotkey Hotkey_Arrow = new Hotkey();
        public Hotkey Hotkey_Numb = new Hotkey();
        public Hotkey Hotkey_Text = new Hotkey();
        public Hotkey Hotkey_Edit = new Hotkey();
        public Hotkey Hotkey_Move = new Hotkey();
        public Hotkey Hotkey_ScaleRotate = new Hotkey();
        public Hotkey Hotkey_Magnet = new Hotkey();
        public Hotkey Hotkey_ClipArt = new Hotkey();
        public Hotkey Hotkey_ClipArt1 = new Hotkey();
        public Hotkey Hotkey_ClipArt2 = new Hotkey();
        public Hotkey Hotkey_ClipArt3 = new Hotkey();
        public Hotkey Hotkey_Zoom = new Hotkey();
        public Hotkey Hotkey_ColorPickup = new Hotkey();
        public Hotkey Hotkey_ColorEdit = new Hotkey();
        public Hotkey Hotkey_LineStyle = new Hotkey();
        public Hotkey Hotkey_Lasso = new Hotkey();
        public UInt32 LineStyleRotateEnabled= 0xFF;  // field of bits

        public Hotkey Hotkey_PagePrev = new Hotkey();
        public Hotkey Hotkey_PageNext = new Hotkey();
        public Hotkey Hotkey_LoadStrokes = new Hotkey();
        public Hotkey Hotkey_SaveStrokes = new Hotkey();

        public float LongHKPressDelay = 2.5F;
        public bool AltTabStart = false;

        public bool ButtonClick_For_LineStyle = false;

        public bool DirectX = true;

        public int ToolSelected = Tools.Hand;        // indicates which tool (Hand,Line,...) is currently selected
        public int FilledSelected = 0;      // indicates which filling (None, Selected color, ...) is currently select
        public bool EraserMode = false;
		public bool Docked = false;
		public bool PointerMode = false;
        public bool IsDockedBeforePen = false;
        public DateTime PointerChangeDate = DateTime.MinValue;

        public bool FingerInAction = false;  // true when mouse down, either drawing or snapping or whatever
		public int Snapping = 0;  // <=0: not snapping, 1: waiting finger, 2:dragging
		public int SnappingX = -1, SnappingY = -1;
		public Rectangle SnappingRect;
		public int UponButtonsUpdate = 0;
		public bool UponTakingSnap = false;
		public bool UponBalloonSnap = false;
		public bool UponSubPanelUpdate = false;
		public bool UponAllDrawingUpdate = false;
		public bool MouseMovedUnderSnapshotDragging = false; // used to pause re-drawing when mouse is not moving during dragging to take a screenshot
        public int PenWidth_Delta = 5;

        public bool LassoMode = false;
        public bool PanMode = false;
		public bool InkVisible = true;
        public int MagneticRadius= MIN_MAGNETIC;        // Magnet Radius; <=0 means off;
        public int MinMagneticRadius() { return Math.Max(Math.Abs(MagneticRadius), MIN_MAGNETIC); }
        public float MagneticAngle = 15.0F;
        public float MagneticAngleTolRatio  = 0.2F;
        public float MagneticAngleTolerance = 15.0F *.2F;

        public Stroke StrokeHovered;            // contains the "selection" for edit/move/copy/erase else is null
        public Pen SelectionFramePen = new Pen(Color.Red, 1);

        public bool SubToolsEnabled = true;

        public bool DefaultArrow_start = true;

        public Ink[] UndoStrokes;
		//public Ink UponUndoStrokes;
		public int UndoP;
		public int UndoDepth, RedoDepth;

        public int CurrentArrow = 0;
        public List<string> ArrowHead = new List<string>();
        public List<string> ArrowTail = new List<string>();

        public NotifyIcon trayIcon;
		public ContextMenu trayMenu;
		public FormCollection FormCollection;
		public FormDisplay FormDisplay;
		public FormButtonHitter FormButtonHitter;
		public FormOptions FormOptions;

		public int CurrentPen = 1;  // defaut pen
		public int LastPen = 1;
		public float GlobalPenWidth = 80;

        // those are constants initialised from ini files
        public float PenWidthThin = 30;
        public float PenWidthNormal = 80;
        public float PenWidthThick = 500;

        public bool FitToCurve = true;
		public bool gpPenWidthVisible = false;
		public string SnapshotFileFullPath = ""; // used to record the last snapshot file name, to select it when the balloon is clicked
        public string SnapshotFileTemplate = "$YYYY$-$MM$-$DD$ $H$-$M$-$S$.png";
        public bool SnapIgnoreBackgroundStroke = true; // for the moment not customizable by user

        public int FormTop = 100, FormLeft = 100, FormWidth = 48, FormOpacity = -50; // negative opacity means that the window is not displayed
        public CallForm callForm = null;
        public bool OpenIntoSnapMode = false;

        public double ArrowAngle = 15 * Math.PI /180;   // 15°
        public double ArrowLen = 0.0185 * System.Windows.SystemParameters.PrimaryScreenWidth; // == 1.85% of screen width

        public int TagNumbering = 1;
        public string TagFormatting = "{0}";
        public string[] TagFormattingList = "{0};{1};{2}".Split(';');
        public int TagSize = 25;
        public string TagFont = "";
        public bool TagItalic = false;
        public bool TagBold = false;
        public int TextSize = 25;
        public string TextFont = "Arial";
        public bool TextItalic = false;
        public bool TextBold = false;

        public int TextBackground = 0;

        public VideoRecordMode VideoRecordMode = VideoRecordMode.NoVideo;
        public string ObsUrl = "ws://localhost:4444";
        public string ObsPwd = "obs";
        public string FFMpegFileName = "CAPT_%DD%%MM%%YY%_%H%%M%%S%_$nn$.mkv";
        public string FFMpegCmd = "ffmpeg.exe -f gdigrab  -framerate 15 -offset_x $xx$ -offset_y $yy$ -video_size $ww$x$hh$ -show_region 1 -i desktop -vcodec libx264 %USERPROFILE%/$FN$";

        public bool CreateM3U = true;
        public Hotkey Hotkey_CreateIndex = new Hotkey();
        public bool CreateIndexOnUndock = false;
        public string IndexDefaultText = "%H%:%M%:%S% = ";
        public bool UndockOnIndexCreate = false;
        public bool NoEditM3UEntry = false;

        //public string VideoOutFileName = "";

        public string CurrentVideoFileName= "";
        public string CurrentIndexFileName = "";
        public DateTime CurrentVideoStartTime;
        public int VideoRecordCounter = 0;
        public int IndexRecordCounter = 0;
        public VideoRecInProgress VideoRecInProgress = VideoRecInProgress.Stopped;
        public Process FFmpegProcess = null;
        public bool VideoRecordWindowInProgress = false;
        public ClientWebSocket ObsWs;
        public Task ObsRecvTask;
        public CancellationTokenSource ObsCancel = new CancellationTokenSource();

        public int StampSize = 128;
        public StringCollection StampFileNames = new StringCollection();
        public ClipArtData ImageStamp = new ClipArtData { ImageStamp = "", X = -1, Y = -1, Wstored=-1, Hstored=-1, Filling = (int)(Filling.NoFrame),PatternLine=false,Distance = -1, Store=false};
        public float StampScaleRatio = .1F;
        public int ImageStampFilling = 0;
        public ClipArtData  ImageStamp1 = new ClipArtData { ImageStamp = "", X = -1, Y = -1, Wstored = -1, Hstored = -1, Filling = (int)(Filling.NoFrame), PatternLine = false, Distance = -1, Store = false };
        public ClipArtData  ImageStamp2 = new ClipArtData { ImageStamp = "", X = -1, Y = -1, Wstored = -1, Hstored = -1, Filling = (int)(Filling.NoFrame), PatternLine = false, Distance = -1, Store = false };
        public ClipArtData  ImageStamp3 = new ClipArtData { ImageStamp = "", X = -1, Y = -1, Wstored = -1, Hstored = -1, Filling = (int)(Filling.NoFrame), PatternLine = false, Distance = -1, Store = false };

        public float TimeBeforeFading = 5.0F;     //5s default
        public byte DecreaseFading = 10;     //10 (/255)  ; must be a positive value

        public int ZoomWidth = 100;
        public int ZoomHeight = 100;
        public float ZoomScale = 3.0F;
        public bool ZoomContinous = false;
        public int ZoomEnabled = 3;

        public Color SpotLightColor = Color.FromArgb(128, Color.Orange);
        public int SpotLightRadius = 200;
        public bool SpotOnAlt = true;

        public Rectangle WindowRect = new Rectangle(Int32.MinValue, Int32.MinValue, -1, -1);
        public bool EraseOnLoosingFocus = false;
        public bool ResizeDrawingWindow = false;

        public SnapInPointerKeys SnapInPointerHoldKey = SnapInPointerKeys.Shift;
        public SnapInPointerKeys SnapInPointerPressTwiceKey = SnapInPointerKeys.Control;
        
        public bool InverseMousewheel=false;

        public string APIRestUrl="";
        public APIRest APIRest;
        public bool APIRestCloseOnSnap = false;
        public bool APIRestAltPressed = false;

        public bool StrokesOnlySnapshot=true;
        //public string ProgramFolder;

        public bool ColorPickerEnabled = true;
        public bool ColorPickerMode = false;
        public Color PickupColor;
        public byte PickupTransparency;
        

        public string ExpandVarCmd(string cmd, int x, int y, int w, int h)
        {
            cmd = Environment.ExpandEnvironmentVariables(cmd);
            cmd = cmd.Replace("$FN$", CurrentVideoFileName);
            cmd = cmd.Replace("$xx$", x.ToString());
            cmd = cmd.Replace("$yy$", y.ToString());
            cmd = cmd.Replace("$ww$", w.ToString());
            cmd = cmd.Replace("$hh$", h.ToString());
            cmd = cmd.Replace("$nn$", VideoRecordCounter.ToString());
            cmd = cmd.Replace("$NN$", VideoRecordCounter.ToString("000"));
            cmd = cmd.Replace("$ii$", IndexRecordCounter.ToString());
            cmd = cmd.Replace("$II$", IndexRecordCounter.ToString("000"));
            DateTime dt = DateTime.Now;
            cmd = cmd.Replace("$H$", dt.Hour.ToString("00"));
            cmd = cmd.Replace("$M$", dt.Minute.ToString("00"));
            cmd = cmd.Replace("$S$", dt.Second.ToString("00"));
            cmd = cmd.Replace("$DD$", dt.Day.ToString("00"));
            cmd = cmd.Replace("$MM$", dt.Month.ToString("00"));
            cmd = cmd.Replace("$YY$", (dt.Year % 100).ToString("00"));
            cmd = cmd.Replace("$YYYY$", dt.Year.ToString("00"));
            return cmd;
        }

        public bool IsVideoRecordingSelected()
        {
            return VideoRecordMode == VideoRecordMode.FfmpegRec || VideoRecordMode == VideoRecordMode.OBSRec;
        }


        public Root()
		{
            /*Global.ProgramFolder = Path.GetDirectoryName(Path.GetFullPath(Environment.GetCommandLineArgs()[0])).Replace('\\','/');
            if (Global.ProgramFolder[Global.ProgramFolder.Length - 1] != '/')
                Global.ProgramFolder += '/';
            */
            SelectionFramePen.DashPattern = new float[]{4,4};       // dashed red line for selection drawing
			for (int p = 0; p < MaxDisplayedPens; p++)
				Hotkey_Pens[p] = new Hotkey();

			trayMenu = new ContextMenu();
			trayMenu.MenuItems.Add(Local.MenuEntryAbout + "...", OnAbout);
			trayMenu.MenuItems.Add(Local.MenuEntryOptions + "...", OnOptions);
			trayMenu.MenuItems.Add("-");
			trayMenu.MenuItems.Add(Local.MenuEntryExit, OnExit);

            SetDefaultPens();
			SetDefaultConfig();
            ReadOptions(Program.RunningFolder + "defaults.ini");
            ReadOptions(Program.RunningFolder + "config.ini");
            ReadOptions(Program.RunningFolder + "pensdef.ini");
            ReadOptions(Program.RunningFolder + "pens.ini");
			ReadOptions(Program.RunningFolder + "hotkeys.ini");
            Hotkey_SnapClose.Parse("Escape");

            if (TagFont=="")     // if no options, we apply text parameters
            {
                TagFont = TextFont;
                TagBold = TextBold;
                TagItalic = TextItalic;
                TagSize = TextSize;
            }
            
			//FormCollection = null;
			//FormDisplay = null;
            FormCollection = new FormCollection(this);
            FormButtonHitter = new FormButtonHitter(this);
            FormDisplay = new FormDisplay(this);  // FormDisplay is created at the end to ensure other objects are created.
            UndoStrokes = new Ink[8];

            APIRest = new APIRest(this);

            Size size = SystemInformation.SmallIconSize;
			trayIcon = new NotifyIcon();
			trayIcon.Text = "ppInk";
			trayIcon.ContextMenu = trayMenu;
			trayIcon.Visible = true;
			trayIcon.MouseClick += TrayIcon_Click;
			trayIcon.BalloonTipText = Local.NotificationSnapshot;
			trayIcon.BalloonTipClicked += TrayIcon_BalloonTipClicked;
			SetTrayIconColor();

			SetHotkey();

			TestMessageFilter mf = new TestMessageFilter(this);
			Application.AddMessageFilter(mf);
        }

        public void callshortcut()
        {
            TagNumbering = 1; //reset tag counter 
            if ((FormCollection == null || !FormCollection.Visible) && (FormDisplay == null || !FormDisplay.Visible))
            {
                if (FormOpacity > 0) callForm?.Hide();
                //if (FormOpacity > 0) callForm.Close();
                StartInk();
                if (OpenIntoSnapMode)
                    FormCollection.btSnap_Click(null,null);
            }
            /*
            else if (PointerMode)
            {
                //Root.UnPointer();
                SelectPen(LastPen);
            }
            else
            {
                //Root.Pointer();
                SelectPen(-2);
            }*/

        }

        private void TrayIcon_BalloonTipClicked(object sender, EventArgs e)
		{
            //string snapbasepath = SnapshotBasePath;
            //snapbasepath = Environment.ExpandEnvironmentVariables(snapbasepath);
            //System.Diagnostics.Process.Start(snapbasepath);            
            string fullpath;
            try
            {
                fullpath = System.IO.Path.GetFullPath(SnapshotFileFullPath);
            }
            catch
            {
                fullpath = ExpandVarCmd(SnapshotBasePath,0,0,0,0);
            }
			System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", fullpath));
		}

		private void TrayIcon_Click(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{

                if ((FormDisplay == null || !FormDisplay.Visible) && (FormCollection == null || !FormCollection.Visible))
				{
                    callshortcut();
                    //ReadOptions("pens.ini");
                    //ReadOptions("config.ini");
                    //ReadOptions("hotkeys.ini");
                    //StartInk();
				}
				else if (Docked)
					UnDock();
			}
		}

		public void StartInk()
		{
            
            if (FormCollection == null)
                FormCollection = new FormCollection(this);
            if (FormButtonHitter == null)
                FormButtonHitter = new FormButtonHitter(this);
            if (FormDisplay == null)
                FormDisplay = new FormDisplay(this);  // FormDisplay is created at the end to ensure other objects are created.
            if (UndoStrokes == null)
                UndoStrokes = new Ink[8];           
            if (FormDisplay.Visible|| FormCollection.Visible)
				return;
            FormCollection.Initialize();
            FormButtonHitter.Initialize();
            FormDisplay.Initialize();

            Docked = KeepDockedAtOpen;//false;
            PointerMode = false; // we have to reset pointer mode when starting drawing;
            ResizeDrawingWindow = false;
            UponTakingSnap = false;

			if (CurrentPen < 0)
				CurrentPen = 0;
			if (!PenEnabled[CurrentPen])
			{
				CurrentPen = 0;
				while (CurrentPen < MaxPenCount && !PenEnabled[CurrentPen])
					CurrentPen++;
				if (CurrentPen == MaxPenCount)
					CurrentPen = -2;
			}
			SelectPen(CurrentPen);
			SetInkVisible(true);
			FormCollection.ButtonsEntering = 1;
			FormDisplay.Show();
			FormCollection.Show();
			FormDisplay.DrawButtons(true);

            for (int i = 0; i < UndoStrokes.Length; i++)
                UndoStrokes[i] = null;
            UndoStrokes[0] = FormCollection.IC.Ink.Clone();
            UndoDepth = 0;
            UndoP = 0;

            SetForegroundWindow(FormCollection.Handle);
        }
        public void StopInk()
		{
            FormCollection.Initializing = true;
            try { FormCollection.Hide(); FormCollection.tiSlide.Enabled = false; } catch { }
            try { FormDisplay.Hide(); FormDisplay.timer1.Enabled = false; } catch { }
			try { FormButtonHitter.Hide(); FormButtonHitter.timer1.Enabled = false; } catch { }


            if (ReinitForms)
            {
                FormCollection = null;
            }
            //  The FormCollection is destroyed, therefore all following calls to the form and its controls will not hit
            try
            {                
                ObsRecvTask?.Dispose();
            }
            catch { }
            finally
            {
                ObsRecvTask = null;
            }
            try
            {
                ObsWs?.Dispose();
            }
            catch { }
            finally
            {
                ObsWs = null;
            }
            try
            {
                ObsCancel.Cancel();
                ObsCancel.Dispose();
            }
            catch { }
            finally
            {
                ObsCancel = new CancellationTokenSource();
            }

            //FormDisplay = null;
            //FormButtonHitter = null;

            if (UponBalloonSnap)
			{
				ShowBalloonSnapshot();
				UponBalloonSnap = false;
			}
            //if (FormOpacity > 0) callForm.Show();
            if (FormOpacity > 0 && !ResizeDrawingWindow)
            {
                if (callForm == null)
                    callForm = new CallForm(this);
                callForm.Show();
                callForm.Top = FormTop;
                callForm.Left = FormLeft;
                callForm.Width = FormWidth;
                callForm.Height = FormWidth;
                callForm.Opacity = FormOpacity / 100.0;
            }

            GC.Collect();
        }

        public void ClearInk()
		{
			FormCollection.IC.Ink.DeleteStrokes();
            FormDisplay.ClearCanvus();
            FormDisplay.DrawButtons(true);
            FormDisplay.UpdateFormDisplay(true);
        }

        public void ShowBalloonSnapshot()
		{
			trayIcon.ShowBalloonTip(3000);
		}

		public void UndoInk()
		{
			if (UndoDepth <= 0)
				return;
            if(FormCollection.IC.Ink.Strokes.Count>0 && FormCollection.IC.Ink.Strokes[FormCollection.IC.Ink.Strokes.Count - 1].ExtendedProperties.Contains(ISTAG_GUID))
            {
                TagNumbering--;
            }

			UndoP--;
			if (UndoP < 0)
				UndoP = UndoStrokes.GetLength(0) - 1;
			UndoDepth--;
			RedoDepth++;
			FormCollection.IC.Ink.DeleteStrokes();
			if (UndoStrokes[UndoP].Strokes.Count > 0)
            {
				FormCollection.IC.Ink.AddStrokesAtRectangle(UndoStrokes[UndoP].Strokes, UndoStrokes[UndoP].Strokes.GetBoundingBox());
                if (ToolSelected == Tools.Poly)
                    FormCollection.RestorePolylineData(FormCollection.IC.Ink.Strokes[FormCollection.IC.Ink.Strokes.Count-1]);
            }
			FormDisplay.ClearCanvus();
			FormDisplay.DrawStrokes();
			FormDisplay.DrawButtons(true);
			FormDisplay.UpdateFormDisplay(true);
		}

		public void Pan(int x, int y)
		{
			if (x == 0 && y == 0)
				return;            
            FormCollection.IC.Ink.Strokes.Move(x, y);
            foreach (Stroke st in FormCollection.IC.Ink.Strokes)
                if (st.ExtendedProperties.Contains(ISBACKGROUND_GUID))
                    continue;
                else
                    FormCollection.MoveStrokeAndProperties(st, x, y, false);
            FormDisplay.ClearCanvus();
			FormDisplay.DrawStrokes();
			FormDisplay.DrawButtons(true);
			FormDisplay.UpdateFormDisplay(true);
		}

		public void SetInkVisible(bool visible)
		{
			InkVisible = visible;
			if (visible)
				FormCollection.btInkVisible.BackgroundImage = FormCollection.image_visible;
			else
				FormCollection.btInkVisible.BackgroundImage = FormCollection.image_visible_not;

			FormDisplay.ClearCanvus();
			FormDisplay.DrawStrokes();
			FormDisplay.DrawButtons(true);
			FormDisplay.UpdateFormDisplay(true);
		}

		public void RedoInk()
		{
			if (RedoDepth <= 0)
				return;

			UndoDepth++;
			RedoDepth--;
			UndoP++;
			if (UndoP >= UndoStrokes.GetLength(0))
				UndoP = 0;
			FormCollection.IC.Ink.DeleteStrokes();
			if (UndoStrokes[UndoP].Strokes.Count > 0)
				FormCollection.IC.Ink.AddStrokesAtRectangle(UndoStrokes[UndoP].Strokes, UndoStrokes[UndoP].Strokes.GetBoundingBox());

			FormDisplay.ClearCanvus();
			FormDisplay.DrawStrokes();
			FormDisplay.DrawButtons(true);
			FormDisplay.UpdateFormDisplay(true);
		}

		public void Dock()
		{
            if ((FormDisplay == null || !FormDisplay.Visible) || (FormCollection == null || !FormCollection.Visible)) 
				return;

			Docked = true;
			gpPenWidthVisible = false;
            switch(ToolbarOrientation)
            {
                case Orientation.toLeft: FormCollection.btDock.BackgroundImage = gInk.Properties.Resources.dockback; break;
                case Orientation.toRight: FormCollection.btDock.BackgroundImage = gInk.Properties.Resources.dock; break;
                case Orientation.toUp: FormCollection.btDock.BackgroundImage = gInk.Properties.Resources.dockbackV ; break;
                case Orientation.toDown: FormCollection.btDock.BackgroundImage = gInk.Properties.Resources.dockV; break;
            }
            FormCollection.ButtonsEntering = -1;
			UponButtonsUpdate |= 0x2;
		}

		public void UnDock()
		{
            if (FormCollection != null && FormCollection.AddM3UEntryInProgress)
                return;

            if ((FormDisplay == null || !FormDisplay.Visible) || (FormCollection == null || !FormCollection.Visible))
				return;

			Docked = false;
            switch (ToolbarOrientation)
            {
                case Orientation.toLeft: FormCollection.btDock.BackgroundImage = gInk.Properties.Resources.dock; break;
                case Orientation.toRight: FormCollection.btDock.BackgroundImage = gInk.Properties.Resources.dockback; break;
                case Orientation.toUp: FormCollection.btDock.BackgroundImage = gInk.Properties.Resources.dockV; break;
                case Orientation.toDown: FormCollection.btDock.BackgroundImage = gInk.Properties.Resources.dockbackV; break;
            }
			FormCollection.ButtonsEntering = 1;
			UponButtonsUpdate |= 0x2;

            if(CreateIndexOnUndock && CurrentIndexFileName != "")
            {
                FormCollection.AddM3UEntry();
            }
        }

		public void Pointer()
		{
			if (PointerMode == true)
				return;
            if (ColorPickerMode)
                FormCollection.StartStopPickUpColor(0);
            FormCollection.ClipartsDlg.Hide();
            PointerChangeDate = DateTime.Now.AddMilliseconds(100);
            PointerMode = true;
            IsDockedBeforePen = Docked;

            FormDisplay.DrawBorder(false);
			FormCollection.ToThrough();
			FormButtonHitter.Show();
            FormButtonHitter.timer1_Tick(null,null); // Force Size recomputation for alt+tab processing
            if (ColorPickerMode)
                FormCollection.StartStopPickUpColor(0);
            FormCollection.SpotLightTemp = false;
        }

        public void UnPointer()
		{
			if (PointerMode == false)
				return;
            if (FormCollection == null)
                return;

            PointerChangeDate = DateTime.Now.AddMilliseconds(100);
            PointerMode = false;

            FormCollection.AddPointerSnaps();
			FormButtonHitter.Hide();
			FormCollection.ToUnThrough();
            FormCollection.ToTopMost();
            AppGetFocus();
			FormCollection.Activate();
            FormCollection.Select();                       
		}

		public void SelectPen(int pen)
		{
			FormCollection.SelectPen(pen);
		}

		public void SetDefaultPens()
		{
			PenEnabled[0] = false;
			PenAttr[0] = new DrawingAttributes();
			PenAttr[0].Color = Color.FromArgb(80, 80, 80);
			PenAttr[0].Width = 80;
			PenAttr[0].Transparency = 0;

			PenEnabled[1] = true;
			PenAttr[1] = new DrawingAttributes();
			PenAttr[1].Color = Color.FromArgb(225, 60, 60);
			PenAttr[1].Width = 80;
			PenAttr[1].Transparency = 0;

			PenEnabled[2] = true;
			PenAttr[2] = new DrawingAttributes();
			PenAttr[2].Color = Color.FromArgb(30, 110, 200);
			PenAttr[2].Width = 80;
			PenAttr[2].Transparency = 0;

			PenEnabled[3] = true;
			PenAttr[3] = new DrawingAttributes();
			PenAttr[3].Color = Color.FromArgb(235, 180, 55);
			PenAttr[3].Width = 80;
			PenAttr[3].Transparency = 0;

			PenEnabled[4] = true;
			PenAttr[4] = new DrawingAttributes();
			PenAttr[4].Color = Color.FromArgb(120, 175, 70);
			PenAttr[4].Width = 80;
			PenAttr[4].Transparency = 0;

			PenEnabled[5] = true;
			PenAttr[5] = new DrawingAttributes();
			PenAttr[5].Color = Color.FromArgb(235, 125, 15);
			PenAttr[5].Width = 500;
			PenAttr[5].Transparency = 175;

            PenEnabled[6] = true;
            PenAttr[6] = new DrawingAttributes();
			PenAttr[6].Color = Color.FromArgb(230, 230, 230);
			PenAttr[6].Width = 80;
			PenAttr[6].Transparency = 0;

            PenEnabled[7] = true;
            PenAttr[7] = new DrawingAttributes();
			PenAttr[7].Color = Color.FromArgb(250, 140, 200);
			PenAttr[7].Width = 80;
			PenAttr[7].Transparency = 0;

            PenEnabled[8] = true;
            PenAttr[8] = new DrawingAttributes();
			PenAttr[8].Color = Color.FromArgb(25, 180, 175);
			PenAttr[8].Width = 80;
			PenAttr[8].Transparency = 0;

            PenEnabled[9] = true;
            PenAttr[9] = new DrawingAttributes();
			PenAttr[9].Color = Color.FromArgb(145, 70, 160);
			PenAttr[9].Width = 500;
			PenAttr[9].Transparency = 175;

            for(int i=10;i<=19;i++)
            {
                PenEnabled[i] = PenEnabled[i-10];
                PenAttr[i] = new DrawingAttributes();
                PenAttr[i].Color = PenAttr[i-10].Color;
                PenAttr[i].Width = PenAttr[i-10].Width;
                PenAttr[i].Transparency = PenAttr[i - 10].Transparency;
            }

            PenAttr[SavedPenDA] = null;

        }

		public void SetDefaultConfig()
		{
			Hotkey_Global.Control = true;
			Hotkey_Global.Alt = true;
			Hotkey_Global.Shift = false;
			Hotkey_Global.Win = false;
			Hotkey_Global.Key = 'G';

			AutoScroll = false;
			WhiteTrayIcon = false;
			SnapshotBasePath = "%USERPROFILE%/Pictures/gInk/";
            Environment.SetEnvironmentVariable("PPINK_SNAP_DIR", SnapshotBasePath);
            SaveStrokesPath = SnapshotBasePath;
            AutoSaveStrokesAtExit = true;

            ///
            // Forcer l'état initial des tags numérotés à fond blanc (ou utiliser BlackFilled si tu préfères)
            //TagNumbering = 3; // numéro de départ (optionnel ; choisis la valeur voulue)
            //FilledSelected = Filling.WhiteFilled; // ← important : définit l'état de remplissage initial

        }

		public void SetTrayIconColor()
		{
			if (WhiteTrayIcon)
			{
				if (File.Exists("icon_white.ico"))
					trayIcon.Icon = new Icon("icon_white.ico");
				else
					trayIcon.Icon = global::gInk.Properties.Resources.icon_white;
			}
			else
			{
				if (File.Exists("icon_red.ico"))
					trayIcon.Icon = new Icon("icon_red.ico");
				else
					trayIcon.Icon = global::gInk.Properties.Resources.icon_red;
			}
		}

        private int GetPenNumber(string sName)
        {
            int penid = -1;
            string st = sName.Substring(3, 2);
            if (st.EndsWith("_"))
                st = st.Substring(0, 1);
            int.TryParse(st, out penid);
            return penid;
        }

        public void ReadOptions(string file)
		{
            string file2 = file;
			if (!File.Exists(file))
				file = Program.RunningFolder + file2;
            if (!File.Exists(file))
                file = Program.ProgramFolder + file2;
            if (!File.Exists(file))
				return;


			FileStream fini = new FileStream(file, FileMode.Open);
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
					// && !sLine.Substring(sLine.IndexOf("=") + 1).Contains("=")
				)
				{
					sName = sLine.Substring(0, sLine.IndexOf("="));
					sName = sName.Trim();
					sName = sName.ToUpper();
					sPara = sLine.Substring(sLine.IndexOf("=") + 1);
					sPara = sPara.Trim();

					if (sName.StartsWith("PEN"))
					{
						int penid = GetPenNumber(sName);
                        if (penid>=0)
						{
							if (sName.EndsWith("_ENABLED"))
							{
								if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
									PenEnabled[penid] = true;
								else if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
									PenEnabled[penid] = false;
							}

							int penc = 0;
							if (int.TryParse(sPara, out penc))
							{
								if (sName.EndsWith("_RED") && penc >= 0 && penc <= 255)
								{
									PenAttr[penid].Color = Color.FromArgb(penc, PenAttr[penid].Color.G, PenAttr[penid].Color.B);
								}
								else if (sName.EndsWith("_GREEN") && penc >= 0 && penc <= 255)
								{
									PenAttr[penid].Color = Color.FromArgb(PenAttr[penid].Color.R, penc, PenAttr[penid].Color.B);
								}
								else if (sName.EndsWith("_BLUE") && penc >= 0 && penc <= 255)
								{
									PenAttr[penid].Color = Color.FromArgb(PenAttr[penid].Color.R, PenAttr[penid].Color.G, penc);
								}
								else if (sName.EndsWith("_ALPHA") && penc >= 0 && penc <= 255)
								{
									PenAttr[penid].Transparency = (byte)(255 - penc);
								}
								else if (sName.EndsWith("_WIDTH") && penc >= 30 && penc <= 3000)
								{
									PenAttr[penid].Width = penc;
								}
                            }
                            if (sName.EndsWith("_LINESTYLE"))
                            {
                                DashStyle ds = LineStyleFromString(sPara);
                                if(ds==DashStyle.Custom)
                                    try { PenAttr[penid].ExtendedProperties.Remove(DASHED_LINE_GUID); }catch { }
                                else
                                    PenAttr[penid].ExtendedProperties.Add(DASHED_LINE_GUID,ds);
                            }
                            if (sName.EndsWith("_FADING"))
                            {
                                float k;
                                if (sPara.ToUpper() == "Y")
                                    k = TimeBeforeFading;
                                if (sPara.ToUpper() == "N")
                                    k = -1;
                                else if (!float.TryParse(sPara, out k))
                                    k = TimeBeforeFading;
                                if (k >= 0)
                                    PenAttr[penid].ExtendedProperties.Add(FADING_PEN, k);
                            }

                            if (sName.EndsWith("_HOTKEY"))
							{
								Hotkey_Pens[penid].Parse(sPara);
							}
						}

					}

                    int tempi = 0;
					float tempf = 0;
                    double tempd = 0;
                    string[] tab;
                    switch (sName)
                    {
                        case "LANGUAGE_FILE":
                            ChangeLanguage(sPara);
                            break;
                        case "ALT_AS_TEMPORARY_COMMAND":
                            if (sPara.ToUpper() == "TRUE" || sPara.ToUpper() == "ON")
                                AltAsOneCommand = 2;
                            else if (int.TryParse(sPara,out tempi))
                                AltAsOneCommand = tempi;
                            else
                                AltAsOneCommand = 0;
                            break;
                        case "HOTKEY_GLOBAL":
                            Hotkey_Global.Parse(sPara);
                            break;
                        case "HOTKEY_TOGGLEFADING":
                            Hotkey_FadingToggle.Parse(sPara);
                            break;                            
                        case "HOTKEY_ERASER":
                            Hotkey_Eraser.Parse(sPara);
                            break;
                        case "HOTKEY_INKVISIBLE":
                            Hotkey_InkVisible.Parse(sPara);
                            break;
                        case "HOTKEY_POINTER":
                            Hotkey_Pointer.Parse(sPara);
                            break;
                        case "HOTKEY_PAN":
                            Hotkey_Pan.Parse(sPara);
                            break;
                        case "HOTKEY_SCALE_ROTATE":
                            Hotkey_ScaleRotate.Parse(sPara);
                            break;
                        case "HOTKEY_UNDO":
                            Hotkey_Undo.Parse(sPara);
                            break;
                        case "HOTKEY_REDO":
                            Hotkey_Redo.Parse(sPara);
                            break;
                        case "HOTKEY_SNAPSHOT":
                            Hotkey_Snap.Parse(sPara);
                            break;
                        case "HOTKEY_CLEAR":
                            Hotkey_Clear.Parse(sPara);
                            break;
                        case "HOTKEY_VIDEOREC":
                            Hotkey_Video.Parse(sPara);
                            break;
                        case "HOTKEY_DOCKUNDOCK":
                            Hotkey_DockUndock.Parse(sPara);
                            break;
                        case "HOTKEY_CLOSE":
                            Hotkey_Close.Parse(sPara);
                            break;
                        case "HOTKEY_HAND":
                            Hotkey_Hand.Parse(sPara);
                            break;
                        case "HOTKEY_LINE":
                            Hotkey_Line.Parse(sPara);
                            break;
                        case "HOTKEY_RECT":
                            Hotkey_Rect.Parse(sPara);
                            break;
                        case "HOTKEY_OVAL":
                            Hotkey_Oval.Parse(sPara);
                            break;
                        case "HOTKEY_ARROW":
                            Hotkey_Arrow.Parse(sPara);
                            break;
                        case "HOTKEY_TEXT":
                            Hotkey_Text.Parse(sPara);
                            break;
                        case "HOTKEY_NUMBCHIP":
                            Hotkey_Numb.Parse(sPara);
                            break;
                        case "HOTKEY_EDIT":
                            Hotkey_Edit.Parse(sPara);
                            break;
                        case "HOTKEY_MOVE":
                            Hotkey_Move.Parse(sPara);
                            break;
                        case "HOTKEY_MAGNET":
                            Hotkey_Magnet.Parse(sPara);
                            break;
                        case "HOTKEY_CLIPART":
                            Hotkey_ClipArt.Parse(sPara);
                            break;
                        case "HOTKEY_CLIPART1":
                            Hotkey_ClipArt1.Parse(sPara);
                            break;
                        case "HOTKEY_CLIPART2":
                            Hotkey_ClipArt2.Parse(sPara);
                            break;
                        case "HOTKEY_CLIPART3":
                            Hotkey_ClipArt3.Parse(sPara);
                            break;
                        case "HOTKEY_ZOOM":
                            Hotkey_Zoom.Parse(sPara);
                            break;
                        case "HOTKEY_PENWIDTH_PLUS":
                            Hotkey_PenWidthPlus.Parse(sPara);
                            break;
                        case "HOTKEY_PENWIDTH_MINUS":
                            Hotkey_PenWidthMinus.Parse(sPara);
                            break;
                        case "HOTKEY_COLORPICKUP":
                            Hotkey_ColorPickup.Parse(sPara);
                            break;
                        case "HOTKEY_COLOREDIT":
                            Hotkey_ColorEdit.Parse(sPara);
                            break;
                        case "HOTKEY_LINESTYLE":
                            Hotkey_LineStyle.Parse(sPara);
                            break;
                        case "HOTKEY_PREVPAGE":
                            Hotkey_PagePrev.Parse(sPara);
                            break;
                        case "HOTKEY_NEXTPAGE":
                            Hotkey_PageNext.Parse(sPara);
                            break;
                        case "HOTKEY_LOADSTROKES":
                            Hotkey_LoadStrokes.Parse(sPara);
                            break;
                        case "HOTKEY_SAVESTROKES":
                            Hotkey_SaveStrokes.Parse(sPara);
                            break;
                        case "HOTKEY_LASSO":
                            Hotkey_Lasso.Parse(sPara);
                            break;

                        case "CURSOR_ARROW":
                            cursorarrowFileName = sPara;
                            break;
                        case "CURSOR_TARGET":
                            cursortargetFileName = sPara;
                            break;
                        case "CURSOR_ERASER":
                            cursoreraserFileName = sPara;
                            break;
                        case "CURSOR_SNAP":
                            cursorsnapFileName = sPara;
                            break;
                        case "CURSOR_RED":
                            cursorredFileName = sPara;
                            break;

                        case "BUTTONCLICK_FOR_LINESTYLE":
                            if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                                ButtonClick_For_LineStyle = true;
                            else
                                ButtonClick_For_LineStyle = false;
                            break;

                        case "LINESTYLEROTATE":
                            try
                            {
                                LineStyleRotateEnabled = Convert.ToUInt32(Convert.ToInt32(sPara, 2));
                            }
                            catch
                            {
                                LineStyleRotateEnabled = 0xFF;
                            }
                            break;

                        case "EXTRA_PENS_SET":
                            PensExtraSet = (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON");
                            break;

                        case "PENS_ON_TWO_LINES":
                            PensOnTwoLines = (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON");
                            break;

                        case "PENWIDTH_DELTA":
                            if (int.TryParse(sPara, out tempi))
                            {
                                PenWidth_Delta=tempi;
                            }
                            break;
                        case "WHITE_TRAY_ICON":
                            if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                                WhiteTrayIcon = true;
                            else
                                WhiteTrayIcon = false;
                            break;
                        case "HIDE_IN_ALTTAB":
                            if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                                globalRoot.HideInAltTab = true;
                            else
                                globalRoot.HideInAltTab = false;
                            break;
                        case "SNAPSHOT_PATH":
                            SnapshotBasePath = sPara.Replace('\\','/');
                            if (!SnapshotBasePath.EndsWith("/"))
                                SnapshotBasePath += "/";
                            Environment.SetEnvironmentVariable("PPINK_SNAP_DIR", SnapshotBasePath);
                            break;
                        case "SAVEDSTROKE_PATH":   // only in config; no wri
                            SaveStrokesPath = Environment.ExpandEnvironmentVariables(sPara).Replace('\\', '/');
                            if (!SaveStrokesPath.EndsWith("/"))
                                SaveStrokesPath += "/";
                            break;
                        case "AUTOSAVE_STROKES":
                            if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                                AutoSaveStrokesAtExit = true;
                            else
                                AutoSaveStrokesAtExit = false;
                            break;
                        case "SNAPSHOT_FILE":
                            SnapshotFileTemplate = sPara;
                            break;
                        case "OPEN_INTO_SNAP":
                            if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                                OpenIntoSnapMode = true;
                            else
                                OpenIntoSnapMode = false;
                            break;
                        case "DRAWING_ICON":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                                ToolsEnabled = false;
                            break;
                        case "FITTOCURVE":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                                FitToCurve = false;
                            else if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                                FitToCurve = true;
                            break;
                        case "ARROW":           // angle in degrees, len in % of the screen width
                            tab = sPara.Split(',');
                            if (tab.Length != 2) break;
                            if (float.TryParse(tab[0], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out tempf))
                                ArrowAngle = tempf * Math.PI / 180;
                            if (float.TryParse(tab[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out tempf))
                                ArrowLen = tempf / 100.0 * System.Windows.SystemParameters.PrimaryScreenWidth;
                            break;
                        case "DEFAULT_ARROW":
                            if (sPara.ToUpper() == "START")
                                DefaultArrow_start = true;
                            if (sPara.ToUpper() == "END")
                                DefaultArrow_start = false;
                            break;
                        case "TEXT":           // Font(string),italique(boolean),Bold(boolean),size(float) of the text in % of the screen, also defines the size of the
                            {
                                tab = sPara.Split(',');
                                if (tab.Length != 4) break;
                                TextFont = tab[0];
                                string s = tab[1];
                                if (s.ToUpper() == "FALSE" || s == "0" || s.ToUpper() == "OFF")
                                    TextItalic = false;
                                else if (s.ToUpper() == "TRUE" || s == "1" || s.ToUpper() == "ON")
                                    TextItalic = true;
                                s = tab[2];
                                if (s.ToUpper() == "FALSE" || s == "0" || s.ToUpper() == "OFF")
                                    TextBold = false;
                                else if (s.ToUpper() == "TRUE" || s == "1" || s.ToUpper() == "ON")
                                    TextBold = true;
                                if (float.TryParse(tab[3], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out tempf))
                                    TextSize = (int)(tempf / 100.0 * System.Windows.SystemParameters.PrimaryScreenWidth);
                            }
                            break;
                        case "TEXT_BACKGROUND":
                            int.TryParse(sPara, out TextBackground);
                            if (TextBackground < 0 || TextBackground > 5)
                                TextBackground = 0;
                            break;
                        case "NUMBERS":           // Font(string),italique(boolean),Bold(boolean),size(float) of the text in % of the screen, also defines the size of the
                            {
                                tab = sPara.Split(',');
                                if (tab.Length != 4) break;
                                TagFont = tab[0];
                                string s = tab[1];
                                if (s.ToUpper() == "FALSE" || s == "0" || s.ToUpper() == "OFF")
                                    TagItalic = false;
                                else if (s.ToUpper() == "TRUE" || s == "1" || s.ToUpper() == "ON")
                                    TagItalic = true;
                                s = tab[2];
                                if (s.ToUpper() == "FALSE" || s == "0" || s.ToUpper() == "OFF")
                                    TagBold = false;
                                else if (s.ToUpper() == "TRUE" || s == "1" || s.ToUpper() == "ON")
                                    TagBold = true;
                                if (float.TryParse(tab[3], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out tempf))
                                    TagSize = (int)(tempf / 100.0 * System.Windows.SystemParameters.PrimaryScreenWidth);
                            }
                            break;



                        case "NUMBER_CURRENT_FORMAT":
                            TagFormatting = sPara;
                            break;

                        case "NUMBER_FORMATS":
                            TagFormattingList = sPara.Split(';');
                            break;

                        // Ajout: numéro de départ (ne pas redéclarer tempi)
                        case "NUMBER_START":
                            if (Int32.TryParse(sPara, out tempi))
                                TagNumbering = tempi;
                            break;

                        case "TAGNUMBERING":
                            if (Int32.TryParse(sPara, out tempi))
                                TagNumbering = tempi;
                            break;

                        // Ajout (optionnel): remplissage par défaut
                        case "NUMBER_DEFAULT_FILL":
                            {
                                string up = sPara.Trim().ToUpperInvariant();
                                if (up == "WHITE" || up == "3")
                                    FilledSelected = Filling.WhiteFilled;
                                else if (up == "BLACK" || up == "4")
                                    FilledSelected = Filling.BlackFilled;
                                else if (up == "EMPTY" || up == "0")
                                    FilledSelected = Filling.Empty;
                            }
                            break;









                        case "MAGNET":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                                MagneticRadius = -MIN_MAGNETIC;
                            else if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                                MagneticRadius = MIN_MAGNETIC;
                            else if (float.TryParse(sPara, NumberStyles.AllowDecimalPoint| NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out tempf))
                                MagneticRadius = (int)(tempf / 100.0 * System.Windows.SystemParameters.PrimaryScreenWidth);
                            break;
                        case "MAGNETIC_ANGLE_TOLERANCE": 
                            float.TryParse(sPara, out MagneticAngleTolRatio);
                            MagneticAngleTolRatio = MagneticAngleTolRatio / 100.0F;
                            MagneticAngleTolerance = MagneticAngle * MagneticAngleTolRatio;
                            break;                            
                        case "MAGNETIC_ANGLE":
                            float.TryParse(sPara, out MagneticAngle);
                            MagneticAngleTolerance = MagneticAngle * MagneticAngleTolRatio ;
                            break;
                        case "ERASER_ICON":
							if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
								EraserEnabled = false;
							break;
						case "POINTER_ICON":
							if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
								PointerEnabled = false;
                            break;
                        case "ALTTAB_POINTER":
                            if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                                AltTabPointer = true;
                            else
                                AltTabPointer = false;
                            break;
                        case "ALTTAB_START":
                            AltTabStart = (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON");
                            break;
                        case "PEN_WIDTH_AT_SELECTION":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                                WidthAtPenSel = false;
                            else if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                                WidthAtPenSel = true;
                            break;
                        case "PEN_WIDTH_ICON":
							if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
								PenWidthEnabled = false;
							else if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
								PenWidthEnabled = true;
							break;
                        case "SUBTOOLSBAR_ENABLED":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                                SubToolsEnabled = false;
                            else if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                                SubToolsEnabled = true;
                            break;
                        case "SNAPSHOT_ICON":
							if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
								SnapEnabled = false;
							break;
                        case "SNAPSHOT_STROKESONLY":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                                StrokesOnlySnapshot = false;
                            else if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                                StrokesOnlySnapshot = true;
                                break;
                        case "CLOSE_ON_SNAP":
							if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
								CloseOnSnap = "false";
							else if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
								CloseOnSnap = "true";
							else if (sPara.ToUpper() == "BLANKONLY")
								CloseOnSnap = "blankonly";
							break;
						case "ALWAYS_HIDE_TOOLBAR":
							if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
								AlwaysHideToolbar = true;
							break;
                        case "TOOLBAR_DOCKED_AT_OPEN":
                            if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                                KeepDockedAtOpen = true;                           
							break;
                        case "TOOLBAR_UNDOCKED_AT_POINTER":
                            KeepUnDockedAtPointer = (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON");
                            break;
                        case "UNDO_ICON":
							if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
								UndoEnabled = false;
							break;
						case "CLEAR_ICON":
							if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
								ClearEnabled = false;
							break;
						case "PAN_ICON":
							if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
								PanEnabled = false;
							break;
                        case "PAGES_ICON":
                            PagesEnabled = (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON");
                            break;
                        case "LOADSAVE_ICON":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                                LoadSaveEnabled = false;
                            break;
                        case "INKVISIBLE_ICON":
							if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
								InkVisibleEnabled = false;
							break;
						case "ALLOW_DRAGGING_TOOLBAR":
							if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
								AllowDraggingToolbar = false;
							break;
						case "ALLOW_HOTKEY_IN_POINTER_MODE":
							if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
								AllowHotkeyInPointerMode = false;
							break;
                        case "ZOOM_ICON":
                            if (int.TryParse(sPara, out tempi)&& tempi>=0 && tempi<=3)
                                ZoomEnabled  = tempi;
                            break;
                        case "COLORPICKUP_ENABLED":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                                ColorPickerEnabled = false;
                            else if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                                ColorPickerEnabled = true;
                            break;
                        case "TOOLBAR_LEFT":
							if (int.TryParse(sPara, out tempi))
								gpButtonsLeft = tempi;
							break;
						case "TOOLBAR_TOP":
							if (int.TryParse(sPara, out tempi))
								gpButtonsTop = tempi;
							break;
                        case "TOOLBAR_HEIGHT":
							if (float.TryParse(sPara, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out tempf))
								ToolbarHeight = tempf;
							break;
						case "CANVAS_CURSOR":
							if (sPara == "0")
								CanvasCursor = 0;
							else if (sPara == "1")
								CanvasCursor = 1;
							break;
						case "WINDOW_POS": // if not defined, no window else 2 to 4 integers Top,Left,[Width/Height,[Opacity]]
							tab = sPara.Split(',');
							if (tab.Length >= 2) { FormTop = Int32.Parse(tab[0]);FormLeft = Int32.Parse(tab[1]); };
							if (tab.Length >= 3 ) { FormWidth = Int32.Parse(tab[2]); }
							if (tab.Length >= 4) { FormOpacity = Int32.Parse(tab[3]); }
							break;
                        case "INKING_AREA": // 4 integers
                            tab = sPara.Split(',');
                            if (tab.Length <= 4)
                            {
                                int a, b, c, d;
                                if (Int32.TryParse(tab[0], out a) && Int32.TryParse(tab[1], out b) && Int32.TryParse(tab[2], out c) && Int32.TryParse(tab[3], out d))
                                {
                                    if (c > 0 && d > 0) // else default value ie full screen;
                                    {
                                        a = a < 0 ? -1 : (Math.Min(Math.Max(SystemInformation.VirtualScreen.Left, a), SystemInformation.VirtualScreen.Right - c));
                                        b = b < 0 ? -1 : (Math.Min(Math.Max(SystemInformation.VirtualScreen.Top, b), SystemInformation.VirtualScreen.Bottom - d));
                                        WindowRect = new Rectangle(a, b, c, d);
                                    }
                                }
                            }
                            break;
                        case "GRAY_BOARD1": // if not defined, no window else 2 to 4 integers Top,Left,[Width/Height,[Opacity]]
                            tab = sPara.Split(',');
                            if (tab.Length == 4)
                            {
                                for(int i=0; i<4; i++)
                                    Gray1[i] = Int32.Parse(tab[i]);
                            };
                            break;
                        case "GRAY_BOARD2": // if not defined, no window else 2 to 4 integers Top,Left,[Width/Height,[Opacity]]
                            tab = sPara.Split(',');
                            if (tab.Length == 4)
                            {
                                for (int i = 0; i < 4; i++)
                                    Gray2[i] = Int32.Parse(tab[i]);
                            };
                            break;
                        case "TOOLBAR_COLOR": // if not defined, no window else 2 to 4 integers Top,Left,[Width/Height,[Opacity]]
                            tab = sPara.Split(',');
                            if (tab.Length == 4)
                            {
                                for (int i = 0; i < 4; i++)
                                    ToolbarBGColor[i] = Int32.Parse(tab[i]);
                            };
                            break;
                        case "SPOT_COLOR": // if not defined, no window else 2 to 4 integers Top,Left,[Width/Height,[Opacity]]
                            tab = sPara.Split(',');
                            if (tab.Length == 4)
                            {
                                int[] sco = new int[4];
                                for (int i = 0; i < 4; i++)
                                    sco[i] = Int32.Parse(tab[i]);
                                SpotLightColor = Color.FromArgb(sco[0], sco[1], sco[2], sco[3]);
                            };
                            break;
                        case "SPOT_RADIUS":
                            if (float.TryParse(sPara, NumberStyles.Float | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out tempf))
                            {
                                SpotLightRadius = (int)(tempf / 100.0 * System.Windows.SystemParameters.PrimaryScreenWidth);
                            }
                            break;
                        case "SPOT_ON_ALT":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                                SpotOnAlt  = false;
                            break;

                        case "SELECTION_COLOR":
                            tab = sPara.Split(',');
                            if (tab.Length == 4)
                            {
                                int[] sco = new int[4];
                                for (int i = 0; i < 4; i++)
                                    sco[i] = Int32.Parse(tab[i]);
                                SelectionFramePen.Color =  Color.FromArgb(sco[0], sco[1], sco[2], sco[3]);
                            };
                            break;
                        case "BOARDATOPENING":
                            if (Int32.TryParse(sPara, out tempi))
                                BoardAtOpening  = tempi;
                            if (BoardAtOpening != 4)
                                BoardSelected = BoardAtOpening;
                            break;

                        case "VIDEO_RECORD_MODE":
                            try
                            {
                                this.VideoRecordMode = (VideoRecordMode)Enum.Parse(typeof(VideoRecordMode), sPara, false);
                            }
                            catch
                            {
                                ;
                            }
                            //if (Int32.TryParse(sPara, out tempi))
                            //    VideoRecordMode = (VideoRecordMode)tempi;
                            break;
                        case "OBS_WS_URL":
                            ObsUrl = sPara;
                            break;
                        case "OBS_WS_PWD":
                            ObsPwd = sPara;
                            break;
                        case "FFMPEG_FILENAME":
                            FFMpegFileName = sPara;
                            break;
                        case "FFMPEG_CMD":
                            FFMpegCmd = sPara;
                            break;
                        case "CREATE_M3U":
                            CreateM3U = (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON");
                            break;
                        case "HOTKEY_CREATEINDEX":
                            Hotkey_CreateIndex.Parse(sPara);
                            break;
                        case "CREATE_INDEX_ON_UNDOCK":
                            CreateIndexOnUndock = (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON");
                            break;
                        case "UNDOCK_ON_CREATE_INDEX":
                            UndockOnIndexCreate = (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON");
                            break;
                        case "INDEX_DEFAULT":
                            IndexDefaultText = sPara;
                            break;
                        case "NOEDIT_M3U_ENTRY":
                            NoEditM3UEntry = (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON");
                            break;
                        case "RESTSERVER_URL":
                            APIRestUrl = sPara;
                            break;
                        case "IMAGESTAMP_SIZE":
                            if (int.TryParse(sPara, out tempi))
                                StampSize = tempi;
                            break;
                        case "IMAGESTAMP_FILLING":
                            if (int.TryParse(sPara, out tempi))
                                ImageStampFilling = tempi;
                            break;
                        case "IMAGESTAMP_FILENAMES":
                            {
                                if (sPara.Length == 0) break;
                                string[] st = sPara.Replace('\\', '/').Trim(';').Split(';');
                                foreach (string st1 in st)
                                {
                                    string st2;
                                    if (!Path.IsPathRooted(st1))
                                        st2 = Program.RunningFolder + st1;
                                    else
                                        st2 = st1;
                                    if (!ContainsInsensitive(StampFileNames, st2))
                                        StampFileNames.Insert(StampFileNames.Count, st2);
                                }
                            }
                            break;
                        case "IMAGESTAMP1":
                            FillImageStampFromConfig(sPara,ref ImageStamp1);
                            break;
                        case "IMAGESTAMP2":
                            FillImageStampFromConfig(sPara, ref ImageStamp2);
                            break;
                        case "IMAGESTAMP3":
                            FillImageStampFromConfig(sPara, ref ImageStamp3);
                            break;
                        case "ARROW_HEAD":
                            {
                                if (sPara.Length == 0) break;
                                string[] st = sPara.Replace('\\', '/').Trim(';').Split(';');
                                foreach (string st1 in st)
                                {
                                    string st2;
                                    if (st1.StartsWith(".") && !Path.IsPathRooted(st1))
                                        st2 = Program.RunningFolder + st1;
                                    else
                                        st2 = st1;
                                    ArrowHead.Add(st2);
                                }
                            }
                            break;
                        case "ARROW_TAIL":
                            {
                                if (sPara.Length == 0) break;
                                string[] st = sPara.Replace('\\', '/').Trim(';').Split(';');
                                foreach (string st1 in st)
                                {
                                    string st2;
                                    if (st1.Contains(".") && !Path.IsPathRooted(st1))
                                        st2 = Program.RunningFolder + st1;
                                    else
                                        st2 = st1;
                                    ArrowTail.Add(st2);
                                }
                            }
                            break;
                        case "TOOLBAR_DIRECTION":
                            if (sPara.ToUpper() == "LEFT")
                                ToolbarOrientation = Orientation.toLeft;
                            if (sPara.ToUpper() == "RIGHT")
                                ToolbarOrientation = Orientation.toRight;
                            if (sPara.ToUpper() == "UP")
                                ToolbarOrientation = Orientation.toUp;
                            if (sPara.ToUpper() == "DOWN")
                                ToolbarOrientation = Orientation.toDown;
                            break;
                        case "FADING_TIME":
                            if (float.TryParse(sPara, out tempf))
                                TimeBeforeFading = (tempf>=0.0?tempf:0.0f);
                            break;
                        case "FADING_DECREASE":
                            if (Int32.TryParse(sPara, out tempi) && tempi>0 && tempi<255)
                                DecreaseFading = (byte)tempi;                       
                            break;
                        case "ZOOM":     // Width;Height;scale(f);Continuous(Y/N)
                            if (sPara.Length == 0) break;
                            try
                            {
                                string[] stt = sPara.Split(';');
                                Int32.TryParse(stt[0], out ZoomWidth);
                                Int32.TryParse(stt[1], out ZoomHeight);
                                float.TryParse(stt[2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out ZoomScale);
                                if (stt[3].ToUpper() == "TRUE" || stt[3] == "1" || stt[3].ToUpper() == "ON" || stt[3].ToUpper() == "Y")
                                    ZoomContinous = true;
                            }
                            catch { }
                            break;
                        case "INVERSE_MOUSEWHEEL_CONTROL":
                            InverseMousewheel = (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON");
                            break;
                        case "SNAP_IN_POINTER_HOLD_KEY": //directly the int value; expected to be in hotkey.ini
                            if (Int32.TryParse(sPara, out tempi))
                                SnapInPointerHoldKey=(SnapInPointerKeys)tempi;
                            break;
                        case "SNAP_IN_POINTER_PRESSTWICE_KEY": //directly the int value; expected to be in hotkey.ini
                            if (Int32.TryParse(sPara, out tempi))
                                SnapInPointerPressTwiceKey = (SnapInPointerKeys)tempi;
                            break;

                        case "MEASURES_ENABLED":
                            if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                                MeasureEnabled = true;
                            else if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                                MeasureEnabled  = false;
                            break;
                        case "MEASURE_WHILE_DRAWING":
                            if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                                MeasureWhileDrawing = true;
                            else if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                                MeasureWhileDrawing = false;
                            break;
                        case "MEASURE_SAVE_SCALE":
                            if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                                Measure_Save_Scale = true;
                            else if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                                Measure_Save_Scale = false;                            
                            break;
                        case "MEASURE_LEN_SCALE":
                            if (Double.TryParse(sPara, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out tempd))
                                Measure2Scale = tempd;
                            break;
                        case "MEASURE_LEN_DECIMALS":
                            if (Int32.TryParse(sPara, out tempi))
                                Measure2Digits = tempi;
                            break;
                        case "MEASURE_LEN_UNIT":                            
                            Measure2Unit = sPara;
                            break;
                        case "MEASURE_ANGLE_DIR":
                            if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                                MeasureAnglCounterClockwise = true;
                            else if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                                MeasureAnglCounterClockwise = false;
                            break;
                        case "SWAP_SNAPSHOT_BEHAVIORS":
                            if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                                SwapSnapsBehaviors = true;
                            else if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                                SwapSnapsBehaviors = false;
                            break;                            
                        // Those parameters are for init only : there is no write to file
                        case "PENWIDTH_THIN_DEFAULT":
                            if (float.TryParse(sPara, out tempf))
                                PenWidthThin = tempf;
                            break;
                        case "PENWIDTH_NORMAL_DEFAULT":
                            if (float.TryParse(sPara, out tempf))
                                PenWidthNormal = tempf;
                            break;
                        case "PENWIDTH_THICK_DEFAULT":
                            if (float.TryParse(sPara, out tempf))
                                PenWidthThick = tempf;
                            break;
                        case "DIRECTX":
                            DirectX = (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON");
                            break;
                        case "WINDOWMODE_UNSELECTED":
                            tab = sPara.Split(',');
                            if (tab.Length == 4)
                                WindowModeBorderUnselected = Color.FromArgb(int.Parse(tab[0]), int.Parse(tab[1]), int.Parse(tab[2]), int.Parse(tab[3]));
                            break;
                        case "WINDOWMODE_SELECTED":
                            tab = sPara.Split(',');
                            if (tab.Length == 4)
                                WindowModeBorderSelected = Color.FromArgb(int.Parse(tab[0]), int.Parse(tab[1]), int.Parse(tab[2]), int.Parse(tab[3]));
                            break;
                        case "ERASE_ON_FOCUSLOST":
                            if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                                EraseOnLoosingFocus = true;
                            break;
                        case "INIT_AT_EACH_UNFOLD":
                            ReinitForms = (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON");
                            break;
                    }
                }
            }
            fini.Close();
		}

        private string FillImageStampFromConfig(string sPara, ref ClipArtData ImgStamp)
        {
            if (sPara.Length == 0)
                sPara = "";
            string[] lst = sPara.Split(';');
            try { ImgStamp.X = int.Parse(lst[1]); }
                catch { ImgStamp.X = -1; }
            ImgStamp.Wstored = ImgStamp.X;
            try { ImgStamp.Y = int.Parse(lst[2]); }
                catch { ImgStamp.Y = -1; }
            ImgStamp.Hstored = ImgStamp.Y;
            try
            {
                switch(lst[3][0])
                {
                    case 'B':
                        ImgStamp.Filling = Filling.BlackFilled;
                        break;
                    case 'E':
                        ImgStamp.Filling = Filling.Empty;
                        break;
                    case 'N':
                        ImgStamp.Filling = Filling.NoFrame;
                        break;
                    case 'W':
                        ImgStamp.Filling = Filling.WhiteFilled;
                        break;
                    case 'P':
                        ImgStamp.Filling = Filling.PenColorFilled;
                        break;
                    default:
                        ImgStamp.Filling = Filling.NoFrame;
                        break;
                }
            }
            catch { ImgStamp.Filling = Filling.NoFrame; }

            try { ImgStamp.PatternLine = lst[4][0] == 'L'; }
                catch { ImgStamp.PatternLine = false; }

            try { ImgStamp.Distance = double.Parse(lst[5],CultureInfo.InvariantCulture); }
                catch { ImgStamp.Distance = Double.MaxValue; }

            sPara = lst[0];
            if (!Path.IsPathRooted(sPara))
                sPara = Program.RunningFolder + sPara;
            sPara = sPara.Replace('\\', '/').ToLower();
            if (!ContainsInsensitive(StampFileNames, sPara))
                StampFileNames.Insert(StampFileNames.Count, sPara);
            ImgStamp.ImageStamp = sPara;
            return sPara;
        }

        private string Fill2Str(int i)
        {
            if (i == Filling.NoFrame) return "NoFrame";
            else if (i == Filling.Empty) return "Empty";
            else if (i == Filling.PenColorFilled) return "PenColor";
            else if (i == Filling.WhiteFilled) return "White";
            else if (i == Filling.BlackFilled) return "Black";
            else return "????";
        }

        public void SaveOptions(string file)
		{
            bool StampFileNamesAlreadyFilled = false;
            bool ArrowHeadAlreadyFilled = false;
            bool ArrowTailAlreadyFilled = false;

            if (!File.Exists(file))
				file = Program.RunningFolder+ file;
			if (!File.Exists(file))
				return;

			FileStream fini = new FileStream(file, FileMode.Open);
			StreamReader srini = new StreamReader(fini);
			string sLine = "";
			string sNameO = "";
			string sName = "", sPara = "";

			List<string> writelines = new List<string>();

			while (sLine != null)
			{
				sPara = "";
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
					sLine.Contains("=") &&
					!sLine.Substring(sLine.IndexOf("=") + 1).Contains("=")
				)
				{
					sNameO = sLine.Substring(0, sLine.IndexOf("="));
					sName = sNameO.Trim().ToUpper();

					if (sName.StartsWith("PEN"))
					{
                        int penid = GetPenNumber(sName);
						if (penid >= 0)
						{
                            if (sName.EndsWith("_ENABLED"))
                            {
                                if (PenEnabled[penid])
                                    sPara = "True";
                                else
                                    sPara = "False";
                            }
                            else if (sName.EndsWith("_RED"))
                            {
                                sPara = PenAttr[penid].Color.R.ToString();
                            }
                            else if (sName.EndsWith("_GREEN"))
                            {
                                sPara = PenAttr[penid].Color.G.ToString();
                            }
                            else if (sName.EndsWith("_BLUE"))
                            {
                                sPara = PenAttr[penid].Color.B.ToString();
                            }
                            else if (sName.EndsWith("_ALPHA"))
                            {
                                sPara = (255 - PenAttr[penid].Transparency).ToString();
                            }
                            else if (sName.EndsWith("_WIDTH"))
                            {
                                sPara = ((int)PenAttr[penid].Width).ToString();
                            }
                            else if (sName.EndsWith("_LINESTYLE"))
                            {
                                sPara = LineStyleToString(PenAttr[penid].ExtendedProperties);
                            }
                            else if (sName.EndsWith("_FADING"))
                            {
                                if (PenAttr[penid].ExtendedProperties.Contains(FADING_PEN))
                                {
                                    float f = (float)(PenAttr[penid].ExtendedProperties[FADING_PEN].Data);
                                    if (f == TimeBeforeFading)
                                        sPara = "Y";
                                    else
                                        sPara = f.ToString();

                                }
                                else
                                    sPara = "N";
                            }
                            else if (sName.EndsWith("_HOTKEY"))
                            {
                                sPara = Hotkey_Pens[penid].ToStringInvariant();
                            }
						}
					}

					switch (sName)
					{
                        case "ALT_AS_TEMPORARY_COMMAND":
                            if (AltAsOneCommand == 2)
                                sPara = "True";
                            else if (AltAsOneCommand == 1)
                                sPara = "1";
                            else
                                sPara = "False";
                            break;
                        case "LANGUAGE_FILE":
							sPara = Local.CurrentLanguageFile;
							break;
						case "HOTKEY_GLOBAL":
							sPara = Hotkey_Global.ToStringInvariant();
							break;
                        case "HOTKEY_TOGGLEFADING":
                            sPara = Hotkey_FadingToggle.ToStringInvariant();                            
                            break;
                        case "HOTKEY_ERASER":
							sPara = Hotkey_Eraser.ToStringInvariant();
							break;
						case "HOTKEY_INKVISIBLE":
							sPara = Hotkey_InkVisible.ToStringInvariant();
							break;
						case "HOTKEY_POINTER":
							sPara = Hotkey_Pointer.ToStringInvariant();
							break;
						case "HOTKEY_PAN":
							sPara = Hotkey_Pan.ToStringInvariant();
                            break;
                        case "HOTKEY_SCALE_ROTATE":
                            sPara = Hotkey_ScaleRotate.ToStringInvariant();                            
                            break;
						case "HOTKEY_UNDO":
							sPara = Hotkey_Undo.ToStringInvariant();
							break;
						case "HOTKEY_REDO":
							sPara = Hotkey_Redo.ToStringInvariant();
							break;
						case "HOTKEY_SNAPSHOT":
							sPara = Hotkey_Snap.ToStringInvariant();
							break;
						case "HOTKEY_CLEAR":
							sPara = Hotkey_Clear.ToStringInvariant();
							break;
                        case "HOTKEY_VIDEOREC":
                            sPara = Hotkey_Video.ToStringInvariant();
                            break;
                        case "HOTKEY_DOCKUNDOCK":
                            sPara = Hotkey_DockUndock.ToStringInvariant();
                            break;
                        case "HOTKEY_CLOSE":
                            sPara = Hotkey_Close.ToStringInvariant();
                            break;
                        case "HOTKEY_HAND":
                            sPara = Hotkey_Hand.ToStringInvariant();
                            break;
                        case "HOTKEY_LINE":
                            sPara = Hotkey_Line.ToStringInvariant();
                            break;
                        case "HOTKEY_RECT":
                            sPara = Hotkey_Rect.ToStringInvariant();
                            break;
                        case "HOTKEY_OVAL":
                            sPara = Hotkey_Oval.ToStringInvariant();
                            break;
                        case "HOTKEY_ARROW":
                            sPara = Hotkey_Arrow.ToStringInvariant();
                            break;
                        case "HOTKEY_TEXT":
                            sPara = Hotkey_Text.ToStringInvariant();
                            break;
                        case "HOTKEY_NUMBCHIP":
                            sPara = Hotkey_Numb.ToStringInvariant();
                            break;
                        case "HOTKEY_EDIT":
                            sPara = Hotkey_Edit.ToStringInvariant();
                            break;
                        case "HOTKEY_MOVE":
                            sPara = Hotkey_Move.ToStringInvariant();
                            break;
                        case "HOTKEY_MAGNET":
                            sPara = Hotkey_Magnet.ToStringInvariant();
                            break;
                        case "HOTKEY_CLIPART":
                            sPara = Hotkey_ClipArt.ToStringInvariant();
                            break;
                        case "HOTKEY_CLIPART1":
                            sPara = Hotkey_ClipArt1.ToStringInvariant();
                            break;
                        case "HOTKEY_CLIPART2":
                            sPara = Hotkey_ClipArt2.ToStringInvariant();
                            break;
                        case "HOTKEY_CLIPART3":
                            sPara = Hotkey_ClipArt3.ToStringInvariant();
                            break;
                        case "HOTKEY_ZOOM":
                            sPara = Hotkey_Zoom.ToStringInvariant();
                            break;
                        case "HOTKEY_PENWIDTH_PLUS":
                            sPara = Hotkey_PenWidthPlus.ToStringInvariant();
                            break;
                        case "HOTKEY_PENWIDTH_MINUS":
                            sPara = Hotkey_PenWidthMinus.ToStringInvariant();
                            break;
                        case "HOTKEY_COLORPICKUP":
                            sPara = Hotkey_ColorPickup.ToStringInvariant();
                            break;
                        case "HOTKEY_COLOREDIT":
                            sPara = Hotkey_ColorEdit.ToStringInvariant();
                            break;
                        case "HOTKEY_LINESTYLE":
                            sPara = Hotkey_LineStyle.ToStringInvariant();
                            break;
                        case "HOTKEY_PREVPAGE":
                            sPara = Hotkey_PagePrev.ToStringInvariant();
                            break;
                        case "HOTKEY_NEXTPAGE":
                            sPara = Hotkey_PageNext.ToStringInvariant();
                            break;
                        case "HOTKEY_LOADSTROKES":                            
                            sPara = Hotkey_LoadStrokes.ToStringInvariant();
                            break;
                        case "HOTKEY_SAVESTROKES":
                            sPara = Hotkey_SaveStrokes.ToStringInvariant();
                            break;
                        case "HOTKEY_LASSO":
                            sPara = Hotkey_Lasso.ToStringInvariant();
                            break;

                        case "BUTTONCLICK_FOR_LINESTYLE":
                            sPara = ButtonClick_For_LineStyle?"True":"False";
                            break;

                        case "LINESTYLEROTATE":
                            string s = "";
                            for (int i = 0; i < 8; i++)
                            {
                                s = (((LineStyleRotateEnabled & (1 << i)) != 0) ? "1" : "0") + s;
                            }
                            sPara =s;
                            break;

                        case "EXTRA_PENS_SET":
                            sPara = PensExtraSet ? "True" : "False";
                            break;

                        case "PENS_ON_TWO_LINES":
                            sPara = PensOnTwoLines? "True" : "False";
                            break;
                        case "PENWIDTH_DELTA":
                            sPara = PenWidth_Delta.ToString();
                            break;

                        case "WHITE_TRAY_ICON":
							if (WhiteTrayIcon)
								sPara = "True";
							else
								sPara = "False";
							break;
                        case "HIDE_IN_ALTTAB":
                            if (WhiteTrayIcon)
                                sPara = "True";
                            else
                                sPara = "False";
                            break;
                        case "SNAPSHOT_PATH":
							sPara = SnapshotBasePath;
							break;
                        case "SNAPSHOT_FILE":
                            sPara = SnapshotFileTemplate;
                            break;
                        case "OPEN_INTO_SNAP":
                            sPara = OpenIntoSnapMode?"True":"False";
                            break;
                        case "DRAWING_ICON":
                            if (ToolsEnabled)
                                sPara = "True";
                            else
                                sPara = "False";
                            break;
                        case "FITTOCURVE":
                            sPara = FitToCurve?"True":"False";
                            break;
                        case "ARROW":           // angle in degrees, len in % of the screen width
                            sPara = (ArrowAngle / Math.PI * 180.0).ToString(CultureInfo.InvariantCulture) +","+ (ArrowLen / System.Windows.SystemParameters.PrimaryScreenWidth * 100.0).ToString(CultureInfo.InvariantCulture);
                            break;
                        case "DEFAULT_ARROW":
                            sPara = DefaultArrow_start ? "START" : "END";
                            break;
                        case "TEXT":           // size of the tag in % of the screen
                            sPara = TextFont+","+(TextItalic?"True":"False")+","+ (TextBold ? "True" : "False")+","+(TextSize / System.Windows.SystemParameters.PrimaryScreenWidth *100.0).ToString(CultureInfo.InvariantCulture);
                            break;
                        case "TEXT_BACKGROUND":
                            sPara = TextBackground.ToString();
                            break;
                        case "NUMBERS":           // size of the tag in % of the screen
                            sPara = TagFont + "," + (TagItalic ? "True" : "False") + "," + (TagBold ? "True" : "False") + "," + (TagSize / System.Windows.SystemParameters.PrimaryScreenWidth * 100.0).ToString(CultureInfo.InvariantCulture);
                            break;
                        case "NUMBER_CURRENT_FORMAT":
                            sPara = TagFormatting;
                            break;
                        case "NUMBER_FORMATS":
                            sPara = String.Join(";", TagFormattingList);
                            break;
                        case "MAGNET":
                            sPara = (MagneticRadius / System.Windows.SystemParameters.PrimaryScreenWidth * 100.0).ToString(CultureInfo.InvariantCulture);
                            break;
                        /*removed as this param is only adjustable through config.ini
                         * case "MAGNETIC_ANGLE_TOLERANCE":
                            sPara = (MagneticAngleTolerance/MagneticAngle *100.0F).ToString();
                            break;
                        */
                        case "MAGNETIC_ANGLE":
                            sPara = MagneticAngle.ToString();
                            break;
                        case "ERASER_ICON":
							if (EraserEnabled)
								sPara = "True";
							else
								sPara = "False";
							break;
						case "POINTER_ICON":
							if (PointerEnabled)
								sPara = "True";
							else
								sPara = "False";
							break;
                        case "ALTTAB_POINTER":
                            sPara = AltTabPointer?"True":"False";
                            break;
                        case "ALTTAB_START":
                            sPara = AltTabStart ? "True" : "False";
                            break;
                        case "PEN_WIDTH_AT_SELECTION":
                            sPara = WidthAtPenSel ? "True" : "False";
                            break;
                        case "PEN_WIDTH_ICON":
							if (PenWidthEnabled)
								sPara = "True";
							else
								sPara = "False";
							break;
                        case "SUBTOOLSBAR_ENABLED":
                            sPara = SubToolsEnabled ? "True" : "False";
                            break;
                        case "SNAPSHOT_ICON":
							if (SnapEnabled)
								sPara = "True";
							else
								sPara = "False";
							break;
                        case "SNAPSHOT_STROKESONLY":
                            sPara = StrokesOnlySnapshot ? "True" : "False";
                            break;
                        case "CLOSE_ON_SNAP":
							if (CloseOnSnap == "true")
								sPara = "True";
							else if (CloseOnSnap == "false")
								sPara = "False";
							else
								sPara = "BlankOnly";
							break;
						case "ALWAYS_HIDE_TOOLBAR":
							sPara = AlwaysHideToolbar ? "True" : "False";
                            break;
                        case "TOOLBAR_DOCKED_AT_OPEN":
                            sPara = KeepDockedAtOpen ? "True" : "False";
							break;
                        case "TOOLBAR_UNDOCKED_AT_POINTER":
                            sPara = KeepUnDockedAtPointer ? "True" : "False";
                            break;
                        case "UNDO_ICON":
							if (UndoEnabled)
								sPara = "True";
							else
								sPara = "False";
							break;
						case "CLEAR_ICON":
							if (ClearEnabled)
								sPara = "True";
							else
								sPara = "False";
							break;
                        case "PAN_ICON":
                            if (PanEnabled)
                                sPara = "True";
                            else
                                sPara = "False";
                            break;
                        case "PAGES_ICON":
                            sPara = PagesEnabled?"True":"False";
                            break;
                        case "LOADSAVE_ICON":
                            if (LoadSaveEnabled)
                                sPara = "True";
                            else
                                sPara = "False";
                            break;
                        case "ZOOM_ICON":
                            sPara = ZoomEnabled.ToString();
                            break;
                        case "COLORPICKUP_ENABLED":
                            sPara = ColorPickerEnabled.ToString();
                            break;
                        case "INKVISIBLE_ICON":
							if (PanEnabled)
								sPara = "True";
							else
								sPara = "False";
							break;
						case "ALLOW_DRAGGING_TOOLBAR":
							if (AllowDraggingToolbar)
								sPara = "True";
							else
								sPara = "False";
							break;
						case "ALLOW_HOTKEY_IN_POINTER_MODE":
							if (AllowHotkeyInPointerMode)
								sPara = "True";
							else
								sPara = "False";
							break;
						case "TOOLBAR_LEFT":
							sPara = gpButtonsLeft.ToString();
							break;
						case "TOOLBAR_TOP":
							sPara = gpButtonsTop.ToString();
							break;
						case "TOOLBAR_HEIGHT":
							sPara = ToolbarHeight.ToString(CultureInfo.InvariantCulture);
							break;
						case "CANVAS_CURSOR":
							sPara = CanvasCursor.ToString();
							break;
                        case "WINDOW_POS": // if not defined, no window else 2 to 4 integers Top,Left,[Width/Height,[Opacity]]
                            sPara = FormTop.ToString() + "," + FormLeft.ToString() + "," + FormWidth.ToString() + "," + FormOpacity.ToString();
                            break;
                        case "GRAYBOARD1": 
                            sPara = Gray1[0].ToString() + "," + Gray1[1].ToString() + "," + Gray1[2].ToString() + "," + Gray1[3].ToString();
                            break;
                        case "INKING_AREA": // 4 integers
                            if (WindowRect.Width <= 0 || WindowRect.Height <= 0)
                                sPara = "-1,-1,-1,-1";
                            else
                                sPara = WindowRect.Left.ToString() + "," + WindowRect.Top.ToString() + "," + WindowRect.Width.ToString() + "," + WindowRect.Height.ToString();
                            break;
                        case "GRAYBOARD2":
                            sPara = Gray2[0].ToString() + "," + Gray2[1].ToString() + "," + Gray2[2].ToString() + "," + Gray2[3].ToString();
                            break;
                        case "TOOLBAR_COLOR":
                            sPara = ToolbarBGColor[0].ToString() + "," + ToolbarBGColor[1].ToString() + "," + ToolbarBGColor[2].ToString() + "," + ToolbarBGColor[3].ToString();
                            break;
                        case "SPOT_COLOR": // if not defined, no window else 2 to 4 integers Top,Left,[Width/Height,[Opacity]]
                            sPara = SpotLightColor.A.ToString() + "," + SpotLightColor.R.ToString() + "," + SpotLightColor.G.ToString() + "," + SpotLightColor.B.ToString();
                            break;
                        case "SPOT_RADIUS":
                            sPara = ((1.0*SpotLightRadius) / System.Windows.SystemParameters.PrimaryScreenWidth * 100.0).ToString(CultureInfo.InvariantCulture);
                            break;
                        case "SPOT_ON_ALT":
                            sPara = SpotOnAlt ? "True" : "False";
                            break;
                        case "BOARDATOPENING":
                            sPara = BoardAtOpening.ToString();
                            break;
                        case "VIDEO_RECORD_MODE":
                            sPara = VideoRecordMode.ToString();
                            break;
                        case "OBS_WS_URL":
                            sPara = ObsUrl;
                            break;
                        case "OBS_WS_PWD":
                            sPara = ObsPwd;
                            break;
                        case "FFMPEG_FILENAME":
                            sPara = FFMpegFileName;
                            break;
                        case "FFMPEG_CMD":
                            sPara = FFMpegCmd;
                            break;
                        case "CREATE_M3U":
                            sPara = CreateM3U ? "True" : "False";
                            break;
                        case "HOTKEY_CREATEINDEX":
                            sPara = Hotkey_CreateIndex.ToStringInvariant();
                            break;
                        case "CREATE_INDEX_ON_UNDOCK":
                            sPara = CreateIndexOnUndock ? "True" : "False";                            
                            break;
                        case "UNDOCK_ON_CREATE_INDEX":
                            sPara = UndockOnIndexCreate ? "True" : "False";
                            break;
                        case "INDEX_DEFAULT":
                            sPara = IndexDefaultText;
                            break;
                        case "NOEDIT_M3U_ENTRY":
                            sPara = NoEditM3UEntry ? "True" : "False";
                            break;
                        case "RESTSERVER_URL":
                            sPara = APIRestUrl;
                            break;
                        case "IMAGESTAMP_SIZE":
                            sPara = StampSize.ToString();
                            break;
                        case "IMAGESTAMP_FILLING":
                            sPara = ImageStampFilling.ToString();
                            break;
                        case "IMAGESTAMP_FILENAMES":
                            if (!StampFileNamesAlreadyFilled)
                            {
                                sPara = "";
                                foreach (string st1 in StampFileNames)
                                {
                                    string[] sts = st1.Split('%');
                                    if (sts[0].Contains("."))
                                        sPara += MakeRelativePath(Program.RunningFolder, sts[0]).Replace('\\', '/');
                                    else
                                        sPara += sts[0];
                                    if (sts.Length == 2)
                                        sPara += "%" + sts[1];
                                    sPara += ";";
                                }
                                if (sPara.Length>1)
                                    sPara = sPara.Remove(sPara.Length - 1, 1); // to suppress last ;
                                else //if(sPara.Length <=1)
                                    sPara = " ";
                                StampFileNamesAlreadyFilled = true;
                            }
                            else
                                sPara = " ";
                            break;
                        case "IMAGESTAMP1":
                            sPara = MakeRelativePath(Program.RunningFolder, ImageStamp1.ImageStamp).Replace('\\', '/') + ";" + ImageStamp1.Wstored.ToString() + ";" + ImageStamp1.Hstored.ToString() + ";" 
                                                        + Fill2Str(ImageStamp1.Filling) + ";" + (ImageStamp1.PatternLine ? "Line;"+ImageStamp1.Distance.ToString(CultureInfo.InvariantCulture) : "Point");
                            break;
                        case "IMAGESTAMP2":
                            sPara = MakeRelativePath(Program.RunningFolder, ImageStamp2.ImageStamp).Replace('\\', '/') + ";" + ImageStamp2.Wstored.ToString() + ";" + ImageStamp2.Hstored.ToString() + ";" 
                                                        + Fill2Str(ImageStamp2.Filling) + ";" + (ImageStamp2.PatternLine ? "Line;" + ImageStamp2.Distance.ToString(CultureInfo.InvariantCulture) : "Point");
                            break;
                        case "IMAGESTAMP3":
                            sPara = MakeRelativePath(Program.RunningFolder, ImageStamp3.ImageStamp).Replace('\\', '/') + ";" + ImageStamp3.Wstored.ToString() + ";" + ImageStamp3.Hstored.ToString() + ";" 
                                                        + Fill2Str(ImageStamp3.Filling) + ";" + (ImageStamp3.PatternLine ? "Line;" + ImageStamp3.Distance.ToString(CultureInfo.InvariantCulture) : "Point");
                            break;
                        case "ARROW_HEAD":
                            if (!ArrowHeadAlreadyFilled)
                            {
                                sPara = "";
                                foreach (string st1 in ArrowHead)
                                {
                                    string[] sts = st1.Split('%');
                                    if (sts[0].Contains("."))
                                        sPara += MakeRelativePath(Program.RunningFolder, sts[0]).Replace('\\', '/');
                                    else
                                        sPara += sts[0];
                                    if (sts.Length == 2)
                                        sPara += "%" + sts[1];
                                    sPara += ";";
                                }
                                if (sPara.Length > 1)
                                    sPara = sPara.Remove(sPara.Length - 1, 1); // to suppress last ;
                                else //if(sPara.Length <=1)
                                    sPara = " ";
                                ArrowHeadAlreadyFilled= true;
                            }
                            else
                                sPara = " ";
                            break;
                        case "ARROW_TAIL":
                            if (!ArrowTailAlreadyFilled)
                            {
                                sPara = "";
                                foreach (string st1 in ArrowTail)
                                {
                                    string[] sts = st1.Split('%');
                                    if (sts[0].Contains("."))
                                        sPara += MakeRelativePath(Program.RunningFolder, sts[0]).Replace('\\', '/');
                                    else
                                        sPara += sts[0];
                                    if (sts.Length == 2)
                                        sPara += "%" + sts[1];
                                    sPara += ";";
                                }
                                if (sPara.Length > 1)
                                    sPara = sPara.Remove(sPara.Length - 1, 1); // to suppress last ;
                                else //if(sPara.Length <=1)
                                    sPara = " ";
                                ArrowTailAlreadyFilled = true;
                            }
                            else
                                sPara = " ";
                            break;
                        case "TOOLBAR_DIRECTION":
                            if (ToolbarOrientation == Orientation.toLeft)
                                sPara = "Left";
                            if (ToolbarOrientation == Orientation.toRight)
                                sPara = "Right";
                            if (ToolbarOrientation == Orientation.toUp)
                                sPara = "Up";
                            if (ToolbarOrientation == Orientation.toDown)
                                sPara = "Down";
                            break;
                        case "FADING_TIME":
                            sPara = TimeBeforeFading.ToString();
                            break;
                        case "ZOOM":     // Width;Height;scale(f);Continuous(Y/N)
                            sPara = ZoomWidth.ToString() + ";" + ZoomHeight.ToString() + ";" + ZoomScale.ToString() + ";" + (ZoomContinous ? "Y" : "N");
                            break;
                        case "INVERSE_MOUSEWHEEL_CONTROL":
                            sPara = InverseMousewheel ? "True" : "False";
                            break;
                        case "SNAP_IN_POINTER_HOLD_KEY": //directly the int value; expected to be in hotkey.ini
                            sPara = ((int)SnapInPointerHoldKey).ToString();
                            break;
                        case "SNAP_IN_POINTER_PRESSTWICE_KEY": //directly the int value; expected to be in hotkey.ini
                            sPara = ((int)SnapInPointerPressTwiceKey).ToString();
                            break;
                        case "MEASURES_ENABLED":
                            sPara = MeasureEnabled ? "True" : "False";
                            break;
                        case "MEASURE_WHILE_DRAWING":
                            sPara = MeasureWhileDrawing ? "True" : "False";
                            break;
                        case "MEASURE_SAVE_SCALE":
                            sPara = Measure_Save_Scale ? "True" : "False";
                            break;                      
                        case "MEASURE_LEN_SCALE":
                            sPara = Measure2Scale.ToString(CultureInfo.InvariantCulture);
                            break;
                        case "MEASURE_LEN_DECIMALS":
                            sPara = Measure2Digits.ToString();
                            break;
                        case "MEASURE_LEN_UNIT":
                            sPara = Measure2Unit;
                            break;
                        case "MEASURE_ANGLE_DIR":
                            sPara = MeasureAnglCounterClockwise ? "True" : "False";
                            break;
                        case "SWAP_SNAPSHOT_BEHAVIORS":
                            sPara = SwapSnapsBehaviors ? "True" : "False";
                            break;
                        case "DIRECTX":
                            sPara = DirectX ? "True" : "False";
                            break;
                    }
                }
				if (sPara != "")
					writelines.Add(sNameO + "= " + sPara);
				else if (sLine != null)
					writelines.Add(sLine);
			}
			fini.Close();

			FileStream frini = new FileStream(file, FileMode.Create);
			StreamWriter swini = new StreamWriter(frini);
			swini.AutoFlush = true;
			foreach (string line in writelines)
				swini.WriteLine(line);
			frini.Close();
		}

        public String CompleteConfig(string NewConfig, string OldConfig)
        {
            string appending = "";
            foreach(String st in NewConfig.Split('\n')) {
                String st1=st.TrimStart(' ', '\r', '\t');
                if (st1 == "" || st1.StartsWith("#") || st1.StartsWith("-") || st1.StartsWith("["))
                    continue;
                string key = st.Split('=')[0].Trim(' ', '\r', '\t');
                if (!Regex.Match(OldConfig,@"\n\s*"+key+@"\s*=").Success)
                {
                    appending += st.Trim(' ', '\r', '\t') + "\n";
                }
            }
            return appending;
        }

        

		private void OnAbout(object sender, EventArgs e)
		{
			FormAbout FormAbout = new FormAbout();
			FormAbout.Show();
		}
		/*
		private void OnPenSetting(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("notepad.exe", "pens.ini");
		}
		*/
		private void OnOptions(object sender, EventArgs e)
		{
            //ReadOptions("pens.ini");
            //ReadOptions("config.ini");
            //ReadOptions("hotkeys.ini");
            if (FormOptions == null)
                FormOptions = new FormOptions(this);
            //if (FormDisplay != null || FormCollection != null)
            if (FormDisplay.Visible|| FormCollection.Visible)
                return;
			FormOptions.Show();
		}

        public void SetHotkey()
		{
			int modifier = 0;
			if (Hotkey_Global.Control) modifier |= 0x2;
			if (Hotkey_Global.Alt) modifier |= 0x1;
			if (Hotkey_Global.Shift) modifier |= 0x4;
			if (Hotkey_Global.Win) modifier |= 0x8;
			//if (modifier != 0)
				RegisterHotKey(IntPtr.Zero, 0, modifier, Hotkey_Global.Key);
            modifier = 0;
            if(IsVideoRecordingSelected() && CreateM3U)
            {
                if (Hotkey_CreateIndex.Control) modifier |= 0x2;
                if (Hotkey_CreateIndex.Alt) modifier |= 0x1;
                if (Hotkey_CreateIndex.Shift) modifier |= 0x4;
                if (Hotkey_CreateIndex.Win) modifier |= 0x8;
                //if (modifier != 0)
                    RegisterHotKey(IntPtr.Zero, 1, modifier, Hotkey_CreateIndex.Key);
            }
        }

        public void UnsetHotkey()
		{
            try {  UnregisterHotKey(IntPtr.Zero, 0); } catch { }
            try { UnregisterHotKey(IntPtr.Zero, 1); } catch { }
        }

        public void ChangeLanguage(string filename)
		{
			Local.LoadLocalFile(filename);

			trayMenu.MenuItems.Clear();
			trayMenu.MenuItems.Add(Local.MenuEntryAbout + "...", OnAbout);
			trayMenu.MenuItems.Add(Local.MenuEntryOptions + "...", OnOptions);
			trayMenu.MenuItems.Add("-");
			trayMenu.MenuItems.Add(Local.MenuEntryExit, OnExit);
		}

		private void OnExit(object sender, EventArgs e)
		{
			UnsetHotkey();

			trayIcon.Dispose();
			Application.Exit();
		}

        public int HiMetricToPixel(double hi)
        {
            try
            {
                return Convert.ToInt32(hi * 0.037795280352161);
            }
            catch
            {
                return hi< 0 ? int.MinValue : int.MaxValue;
            }
}

public int PixelToHiMetric(double pi)
        {
            try
            {
                return Convert.ToInt32(pi / 0.037795280352161);
            }
            catch
            {
                return pi < 0 ? int.MinValue : int.MaxValue;
            }
        }

        public static String MakeRelativePath(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        // Should be considered as Obsolete as transfered in FormCollection;
        public double ConvertMeasureLength(double hl)
        {
            return hl * 0.037795280352161 * Measure2Scale;
        }

        public string LineStyleToString(ExtendedProperties props)
        {
            if (!props.Contains(DASHED_LINE_GUID))
                return "Stroke";
            else
                switch ((DashStyle)(props[DASHED_LINE_GUID].Data))
                {
                    case DashStyle.Solid:
                        return "Solid";
                    case DashStyle.Dash:
                        return "Dash";
                    case DashStyle.Dot:
                        return "Dot";
                    case DashStyle.DashDot:
                        return "DashDot";
                    case DashStyle.DashDotDot:
                        return "DashDotDot";
                }
            return "Stroke"; //by default
        }

        public DashStyle LineStyleFromString(string s)
        {
            switch (s.ToUpper())
            {
                case "STROKE":
                    return DashStyle.Custom;
                case "SOLID":
                    return DashStyle.Solid;
                case "DASH":
                    return DashStyle.Dash;
                case "DOT":
                    return DashStyle.Dot;
                case "DASHDOT":
                    return DashStyle.DashDot;
                case "DASHDOTDOT":
                    return DashStyle.DashDotDot;
            }
            throw (new Exception("Unknown LineStyle String :" + s));
        }

        public string NextLineStyleString(string s,bool CustomList=false)
        {
            UInt32 u = CustomList ? LineStyleRotateEnabled : 0xFF;
            if (u == 0)
                u = 0xFF;
            String st;

            switch (s.ToUpper())
            {
                case "STROKE":
                    st = "Solid";
                    return ((u & 0x2) != 0) ? st : NextLineStyleString(st, CustomList);
                    //break;
                case "SOLID":
                    st = "Dash";
                    return ((u & 0x4) != 0) ? st : NextLineStyleString(st, CustomList);
                    //break;
                case "DASH":
                    st = "Dot";
                    return ((u & 0x8) != 0) ? st : NextLineStyleString(st, CustomList);
                    //break;
                case "DOT":
                    st = "DashDot";
                    return ((u & 0x10) != 0) ? st : NextLineStyleString(st, CustomList);
                    //break;
                case "DASHDOT":
                    st = "DashDotDot";
                    return ((u & 0x20) != 0) ? st : NextLineStyleString(st, CustomList);
                    //break;
                case "DASHDOTDOT":
                    st = "Stroke";
                    return ((u & 0x1) != 0) ? st : NextLineStyleString(st, CustomList);
                    //break;
            }
            return "Stroke"; //default : original stroke
        }


        public void AppGetFocus()
        {
            SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
        }

        public bool ContainsInsensitive(StringCollection Arr, string key)
        {
            foreach (string s in Arr)
                if (s.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            return false;
        }

        public String InputBox(string prompt="", string title="ppInk", string deflt="")
        {
            Size size = new System.Drawing.Size(300, 100);
            Form inputBox = new Form();      
            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            inputBox.Text = title;
            inputBox.ClientSize = size;
            inputBox.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            inputBox.TopMost = true;

            Label promptText = new Label();
            promptText.Size = new System.Drawing.Size(size.Width - 10, 45);            
            promptText.Location = new System.Drawing.Point(5, 5);
            promptText.Text = prompt;
            inputBox.Controls.Add(promptText);

            TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox.Location = new System.Drawing.Point(5, 45);
            textBox.Text = deflt;
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 70);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, 70);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            if (result == DialogResult.OK)
                return textBox.Text;
            else
                return "";
        }

        [DllImport("user32.dll")]
		private static extern int RegisterHotKey(IntPtr hwnd, int id, int fsModifiers, int vk);
		[DllImport("user32.dll")]
		private static extern int UnregisterHotKey(IntPtr hwnd, int id);
        [DllImport("user32.dll")]
        internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);
    }
}

