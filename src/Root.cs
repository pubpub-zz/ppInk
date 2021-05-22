using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Ink;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Collections.Specialized;

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
        public const int Move = 10; public const int Copy = 11; public const int Poly = 21; public const int ClipArt = 22;
    }
    public class Filling
    {
        public const int NoFrame = -1;      // for Stamps
        public const int Empty = 0;
        public const int PenColorFilled = 1;
        public const int WhiteFilled = 2;
        public const int BlackFilled = 3;
    } // applicable to Hand,Rect,Oval

    public class Orientation
    {
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
        public int X;
        public int Y;
        public int Filling;
    };

    public enum VideoRecordMode { NoVideo = 0, OBSRec = 1, OBSBcst = 2, FfmpegRec = 3 };
    public enum VideoRecInProgress { Stopped = 0, Starting = 1, Recording = 2, Stopping = 3, Pausing = 4, Paused = 5, Resuming = 6, Streaming = 7 };

    public enum SnapInPointerKeys { None = 0, Shift = 1, Control = 2, Alt = 3 };

    public class TestMessageFilter : IMessageFilter
    {
        public Root Root;

        public TestMessageFilter(Root root)
        {
            Root = root;
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == 0x0312 || m.Msg == Program.StartInkingMsg)                    // 0x0312 is the global hotkey 
            {
                //Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);                  // The key of the hotkey that was pressed.
                //int modifier = (int)m.LParam & 0xFFFF;       // The modifier of the hotkey that was pressed.
                //int id = m.WParam.ToInt32();                                        // The id of the hotkey that was pressed.
                bool activePointer = m.Msg == 0x0312 && Root.FormCollection != null;
                Root.callshortcut();
                if (activePointer)           // StartInkingMsg is received twice, therefore we have to froce pointerMode at that time...
                {
                    Root.FormCollection.btPointer_Click(null, null);
                    if (Root.AltTabPointer && !Root.PointerMode && !Root.FormCollection.Initializing) // to unfold the bar if AltTabPointer option has been set
                    {
                        Root.UnDock();
                    }

                }
                return true;
            }
            return false;
        }
    }

    public static class globalRoot
    {
        public static bool HideInAltTab = true;
    };

    public class Root
    {
        public Local Local = new Local();
        public const int MaxPenCount = 10;

        //public Guid TYPE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 0);
        public static Guid TEXT_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 1);
        public static Guid TEXTX_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 2);
        public static Guid TEXTY_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 3);
        public static Guid TEXTHALIGN_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 4);
        public static Guid TEXTVALIGN_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 5);
        public static Guid TEXTFONT_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 6);
        public static Guid TEXTFONTSIZE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 7);
        public static Guid TEXTFONTSTYLE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 8);
        public static Guid TEXTWIDTH_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 9);
        public static Guid TEXTHEIGHT_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 10);

        public static Guid ISDELETION_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 1, 0);
        public static Guid ISSTROKE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 1, 1);
        public static Guid ISTAG_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 1, 2);
        //not yet used : 
        //public Guid ISRECT_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 1, 2);
        //public Guid ISOVAL_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 1, 3);

        public static Guid ISFILLEDCOLOR_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 1);
        public static Guid ISFILLEDWHITE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 2);
        public static Guid ISFILLEDBLACK_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 3);
        public static Guid IMAGE_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 4);
        public static Guid IMAGE_X_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 5);
        public static Guid IMAGE_Y_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 6);
        public static Guid IMAGE_W_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 7);
        public static Guid IMAGE_H_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 8);
        public static Guid ISHIDDEN_GUID = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 2, 10);

        public static Guid FADING_PEN = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 3, 1);

        public static int MIN_MAGNETIC = 25;
        // options
        public int ToolbarOrientation;
        public bool[] PenEnabled = new bool[MaxPenCount];
        public bool ToolsEnabled = true;
        public bool EraserEnabled = true;
        public bool PointerEnabled = true;
        public bool AltTabPointer;
        public bool PenWidthEnabled;
        public bool WidthAtPenSel = true;
        public bool SnapEnabled = true;
        public bool UndoEnabled = true;
        public bool ClearEnabled = true;
        public bool LoadSaveEnabled = true;
        public bool PanEnabled = true;
        public bool InkVisibleEnabled = true;
        public DrawingAttributes[] PenAttr = new DrawingAttributes[MaxPenCount];
        public bool AutoScroll;
        public bool WhiteTrayIcon;
        public string SnapshotBasePath;
        public int CanvasCursor;
        public bool AllowDraggingToolbar = true;
        public bool AllowHotkeyInPointerMode = true;
        public int gpButtonsLeft, gpButtonsTop;

        // advanced options
        public string CloseOnSnap = "blankonly";
        public bool AlwaysHideToolbar;
        public float ToolbarHeight = 0.06f;
        public bool AltAsOneCommand = true;

        public int CursorX, CursorY;
        public int CursorX0 = int.MinValue, CursorY0 = int.MinValue;

        public double LongClickTime = 1.0;

        // the two grays for "white board" effect
        public int[] Gray1 = new int[] { 80, 150, 150, 150 };
        public int[] Gray2 = new int[] { 100, 100, 100, 100 };
        public int[] ToolbarBGColor = new int[] { 245, 245, 245, 0 };
        public int BoardAtOpening;      // 0:Transparent/1:White/2:Customed/3:Black/4:AtSelection
        public int BoardSelected;       // by default transparent

        // hotkey options
        public Hotkey Hotkey_Global = new Hotkey();
        public Hotkey[] Hotkey_Pens = new Hotkey[10];
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

        public Hotkey Hotkey_Hand = new Hotkey();
        public Hotkey Hotkey_Line = new Hotkey();
        public Hotkey Hotkey_Rect = new Hotkey();
        public Hotkey Hotkey_Oval = new Hotkey();
        public Hotkey Hotkey_Arrow = new Hotkey();
        public Hotkey Hotkey_Numb = new Hotkey();
        public Hotkey Hotkey_Text = new Hotkey();
        public Hotkey Hotkey_Edit = new Hotkey();
        public Hotkey Hotkey_Move = new Hotkey();
        public Hotkey Hotkey_Magnet = new Hotkey();
        public Hotkey Hotkey_ClipArt = new Hotkey();
        public Hotkey Hotkey_ClipArt1 = new Hotkey();
        public Hotkey Hotkey_ClipArt2 = new Hotkey();
        public Hotkey Hotkey_ClipArt3 = new Hotkey();
        public Hotkey Hotkey_Zoom = new Hotkey();

        public int ToolSelected;        // indicates which tool (Hand,Line,...) is currently selected
        public int FilledSelected;      // indicates which filling (None, Selected color, ...) is currently select
        public bool EraserMode;
        public bool Docked;
        public bool PointerMode;
        public bool FingerInAction;  // true when mouse down, either drawing or snapping or whatever
        public int Snapping;  // <=0: not snapping, 1: waiting finger, 2:dragging
        public int SnappingX = -1, SnappingY = -1;
        public Rectangle SnappingRect;
        public int UponButtonsUpdate;
        public bool UponTakingSnap;
        public bool UponBalloonSnap;
        public bool UponSubPanelUpdate;
        public bool UponAllDrawingUpdate;
        public bool MouseMovedUnderSnapshotDragging; // used to pause re-drawing when mouse is not moving during dragging to take a screenshot

        public bool PanMode;
        public bool InkVisible = true;
        public int MagneticRadius = MIN_MAGNETIC;        // Magnet Radius; <=0 means off;
        public int MinMagneticRadius() { return Math.Max(Math.Abs(MagneticRadius), MIN_MAGNETIC); }
        public Stroke StrokeHovered;            // contains the "selection" for edit/move/copy/erase else is null
        public Pen SelectionFramePen = new Pen(Color.Red, 1);


        public bool DefaultArrow_start = true;

        public Ink[] UndoStrokes;
        //public Ink UponUndoStrokes;
        public int UndoP;
        public int UndoDepth, RedoDepth;

        public NotifyIcon trayIcon;
        public ContextMenu trayMenu;
        public FormCollection FormCollection;
        public FormDisplay FormDisplay;
        public FormButtonHitter FormButtonHitter;
        public FormOptions FormOptions;

        public int CurrentPen = 1;  // defaut pen
        public int LastPen = 1;
        public int GlobalPenWidth = 80;
        public bool gpPenWidthVisible;
        public string SnapshotFileFullPath = ""; // used to record the last snapshot file name, to select it when the balloon is clicked

        public int FormTop = 100, FormLeft = 100, FormWidth = 48, FormOpacity = -50; // negative opacity means that the window is not displayed
        public CallForm callForm;
        public bool OpenIntoSnapMode;

        public double ArrowAngle = 15 * Math.PI / 180;   // 15°
        public double ArrowLen = 0.0185 * System.Windows.SystemParameters.PrimaryScreenWidth; // == 1.85% of screen width

        public int TagNumbering = 1;
        public int TagSize = 25;
        public string TagFont = "";
        public bool TagItalic;
        public bool TagBold;
        public int TextSize = 25;
        public string TextFont = "Arial";
        public bool TextItalic;
        public bool TextBold;

        public VideoRecordMode VideoRecordMode;
        public string ObsUrl = "ws://localhost:4444";
        public string ObsPwd = "obs";
        public string FFMpegCmd = "ffmpeg.exe -f gdigrab  -framerate 15 -offset_x $xx$ -offset_y $yy$ -video_size $ww$x$hh$ -show_region 1 -i desktop -vcodec libx264 %USERPROFILE%/CAPT_%DD%%MM%%YY%_%H%%M%%S%_$nn$.mkv";
        public int VideoRecordCounter;
        public VideoRecInProgress VideoRecInProgress;
        public Process FFmpegProcess;
        public bool VideoRecordWindowInProgress;
        public ClientWebSocket ObsWs;
        public Task ObsRecvTask;
        public CancellationTokenSource ObsCancel = new CancellationTokenSource();

        public int StampSize = 128;
        public StringCollection StampFileNames = new StringCollection();
        public ClipArtData ImageStamp = new ClipArtData { ImageStamp = "", X = -1, Y = -1, Filling = (int)Filling.NoFrame };
        public float StampScaleRatio = .1F;
        public int ImageStampFilling;
        public string ImageStamp1 = "";
        public string ImageStamp2 = "";
        public string ImageStamp3 = "";
        public bool CleanOnHide;
        public float TimeBeforeFading = 5.0F;     //5s default

        public int ZoomWidth = 100;
        public int ZoomHeight = 100;
        public float ZoomScale = 3.0F;
        public bool ZoomContinous;
        public int ZoomEnabled = 3;

        public Rectangle WindowRect = new Rectangle(int.MinValue, int.MinValue, -1, -1);
        public bool ResizeDrawingWindow;

        public SnapInPointerKeys SnapInPointerHoldKey = SnapInPointerKeys.Shift;
        public SnapInPointerKeys SnapInPointerPressTwiceKey = SnapInPointerKeys.Control;

        public bool InverseMousewheel;

        //public string ProgramFolder;

        public string ExpandVarCmd(string cmd, int x, int y, int w, int h)
        {
            cmd = Environment.ExpandEnvironmentVariables(cmd);
            cmd = cmd.Replace("$xx$", x.ToString());
            cmd = cmd.Replace("$yy$", y.ToString());
            cmd = cmd.Replace("$ww$", w.ToString());
            cmd = cmd.Replace("$hh$", h.ToString());
            cmd = cmd.Replace("$nn$", VideoRecordCounter.ToString());
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


        public Root()
        {
            Global.ProgramFolder = Path.GetDirectoryName(Path.GetFullPath(Environment.GetCommandLineArgs()[0])).Replace('\\', '/');
            SelectionFramePen.DashPattern = new float[] { 4, 4 };       // dashed red line for selection drawing
            if (Global.ProgramFolder[Global.ProgramFolder.Length - 1] != '/')
            {
                Global.ProgramFolder += '/';
            }

            for (int p = 0; p < MaxPenCount; p++)
            {
                Hotkey_Pens[p] = new Hotkey();
            }

            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add(Local.MenuEntryAbout + "...", OnAbout);
            trayMenu.MenuItems.Add(Local.MenuEntryOptions + "...", OnOptions);
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add(Local.MenuEntryExit, OnExit);

            SetDefaultPens();
            SetDefaultConfig();
            ReadOptions("config.ini");
            ReadOptions("pens.ini");
            ReadOptions("hotkeys.ini");

            if (TagFont == "")     // if no options, we apply text parameters
            {
                TagFont = TextFont;
                TagBold = TextBold;
                TagItalic = TextItalic;
                TagSize = TextSize;
            }

            Size size = SystemInformation.SmallIconSize;
            trayIcon = new NotifyIcon
            {
                Text = "ppInk",
                ContextMenu = trayMenu,
                Visible = true
            };
            trayIcon.MouseClick += TrayIcon_Click;
            trayIcon.BalloonTipText = Local.NotificationSnapshot;
            trayIcon.BalloonTipClicked += TrayIcon_BalloonTipClicked;
            SetTrayIconColor();

            SetHotkey();

            TestMessageFilter mf = new TestMessageFilter(this);
            Application.AddMessageFilter(mf);

            FormCollection = null;
            FormDisplay = null;

            // to be done once only
            //ReadOptions("pens.ini");
            //ReadOptions("config.ini");
            //ReadOptions("hotkeys.ini");

        }

        public void callshortcut()
        {
            TagNumbering = 1; //reset tag counter 
            if (FormCollection == null && FormDisplay == null)
            {
                //if (FormOpacity > 0) callForm.Hide();
                if (FormOpacity > 0)
                {
                    callForm.Close();
                }

                StartInk();
                if (OpenIntoSnapMode)
                {
                    FormCollection.btSnap_Click(null, null);
                }
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
            string fullpath = System.IO.Path.GetFullPath(SnapshotFileFullPath);
            System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", fullpath));
        }

        private void TrayIcon_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {

                if (FormDisplay == null && FormCollection == null)
                {
                    callshortcut();
                    //ReadOptions("pens.ini");
                    //ReadOptions("config.ini");
                    //ReadOptions("hotkeys.ini");
                    //StartInk();
                }
                else if (Docked)
                {
                    UnDock();
                }
            }
        }

        public void StartInk()
        {
            if (FormDisplay != null || FormCollection != null)
            {
                return;
            }

            Docked = false;
            PointerMode = false; // we have to reset pointer mode when starting drawing;
            ResizeDrawingWindow = false;
            UponTakingSnap = false;

            FormCollection = new FormCollection(this);
            FormButtonHitter = new FormButtonHitter(this);
            FormDisplay = new FormDisplay(this);  // FormDisplay is created at the end to ensure other objects are created.
            if (CurrentPen < 0)
            {
                CurrentPen = 0;
            }

            if (!PenEnabled[CurrentPen])
            {
                CurrentPen = 0;
                while (CurrentPen < MaxPenCount && !PenEnabled[CurrentPen])
                {
                    CurrentPen++;
                }

                if (CurrentPen == MaxPenCount)
                {
                    CurrentPen = -2;
                }
            }
            SelectPen(CurrentPen);
            SetInkVisible(true);
            FormCollection.ButtonsEntering = 1;
            FormDisplay.Show();
            FormCollection.Show();
            FormDisplay.DrawButtons(true);

            if (UndoStrokes == null)
            {
                UndoStrokes = new Ink[8];
                UndoStrokes[0] = FormCollection.IC.Ink.Clone();
                UndoDepth = 0;
                UndoP = 0;
            }

            //UponUndoStrokes = FormCollection.IC.Ink.Clone();
        }
        public void StopInk()
        {
            try { FormCollection.Close(); } catch { }
            try { FormDisplay.Close(); } catch { }
            try { FormButtonHitter.Close(); } catch { }

            GC.Collect();

            FormCollection = null;
            //  The FormCollection is destroyed, therefore all following calls to the form and its controls will not hit
            ObsCancel.Cancel();
            ObsRecvTask = null;
            ObsWs = null;
            ObsCancel = new CancellationTokenSource();

            FormDisplay = null;
            FormButtonHitter = null;

            if (UponBalloonSnap)
            {
                ShowBalloonSnapshot();
                UponBalloonSnap = false;
            }
            //if (FormOpacity > 0) callForm.Show();
            if (FormOpacity > 0 && !ResizeDrawingWindow)
            {
                callForm = new CallForm(this);
                callForm.Show();
                callForm.Top = FormTop;
                callForm.Left = FormLeft;
                callForm.Width = FormWidth;
                callForm.Height = FormWidth;
                callForm.Opacity = FormOpacity / 100.0;
            }
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
            {
                return;
            }

            if (FormCollection.IC.Ink.Strokes.Count > 0 && FormCollection.IC.Ink.Strokes[FormCollection.IC.Ink.Strokes.Count - 1].ExtendedProperties.Contains(ISTAG_GUID))
            {
                TagNumbering--;
            }

            UndoP--;
            if (UndoP < 0)
            {
                UndoP = UndoStrokes.GetLength(0) - 1;
            }

            UndoDepth--;
            RedoDepth++;
            FormCollection.IC.Ink.DeleteStrokes();
            if (UndoStrokes[UndoP].Strokes.Count > 0)
            {
                FormCollection.IC.Ink.AddStrokesAtRectangle(UndoStrokes[UndoP].Strokes, UndoStrokes[UndoP].Strokes.GetBoundingBox());
                if (ToolSelected == Tools.Poly)
                {
                    FormCollection.RestorePolylineData(FormCollection.IC.Ink.Strokes[FormCollection.IC.Ink.Strokes.Count - 1]);
                }
            }
            FormDisplay.ClearCanvus();
            FormDisplay.DrawStrokes();
            FormDisplay.DrawButtons(true);
            FormDisplay.UpdateFormDisplay(true);
        }

        public void Pan(int x, int y)
        {
            if (x == 0 && y == 0)
            {
                return;
            }

            FormCollection.IC.Ink.Strokes.Move(x, y);
            // for texts
            foreach (Stroke st in FormCollection.IC.Ink.Strokes)
            {
                if (st.ExtendedProperties.Contains(TEXTX_GUID))
                {
                    st.ExtendedProperties.Add(TEXTX_GUID, (int)st.ExtendedProperties[TEXTX_GUID].Data + x);
                    st.ExtendedProperties.Add(TEXTY_GUID, (int)st.ExtendedProperties[TEXTY_GUID].Data + y);
                }
            }
            FormDisplay.ClearCanvus();
            FormDisplay.DrawStrokes();
            FormDisplay.DrawButtons(true);
            FormDisplay.UpdateFormDisplay(true);
        }

        public void SetInkVisible(bool visible)
        {
            InkVisible = visible;
            FormCollection.btInkVisible.BackgroundImage = visible ? FormCollection.image_visible : FormCollection.image_visible_not;

            FormDisplay.ClearCanvus();
            FormDisplay.DrawStrokes();
            FormDisplay.DrawButtons(true);
            FormDisplay.UpdateFormDisplay(true);
        }

        public void RedoInk()
        {
            if (RedoDepth <= 0)
            {
                return;
            }

            UndoDepth++;
            RedoDepth--;
            UndoP++;
            if (UndoP >= UndoStrokes.GetLength(0))
            {
                UndoP = 0;
            }

            FormCollection.IC.Ink.DeleteStrokes();
            if (UndoStrokes[UndoP].Strokes.Count > 0)
            {
                FormCollection.IC.Ink.AddStrokesAtRectangle(UndoStrokes[UndoP].Strokes, UndoStrokes[UndoP].Strokes.GetBoundingBox());
            }

            FormDisplay.ClearCanvus();
            FormDisplay.DrawStrokes();
            FormDisplay.DrawButtons(true);
            FormDisplay.UpdateFormDisplay(true);
        }

        public void Dock()
        {
            if (FormDisplay == null || FormCollection == null)
            {
                return;
            }

            Docked = true;
            gpPenWidthVisible = false;
            switch (ToolbarOrientation)
            {
                case Orientation.toLeft: FormCollection.btDock.BackgroundImage = gInk.Properties.Resources.dockback; break;
                case Orientation.toRight: FormCollection.btDock.BackgroundImage = gInk.Properties.Resources.dock; break;
                case Orientation.toUp: FormCollection.btDock.BackgroundImage = gInk.Properties.Resources.dockbackV; break;
                case Orientation.toDown: FormCollection.btDock.BackgroundImage = gInk.Properties.Resources.dockV; break;
            }
            FormCollection.ButtonsEntering = -1;
            UponButtonsUpdate |= 0x2;
        }

        public void UnDock()
        {
            if (FormDisplay == null || FormCollection == null)
            {
                return;
            }

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
        }

        public void Pointer()
        {
            if (PointerMode == true)
            {
                return;
            }

            if (CleanOnHide) {
                ClearInk();
                Dock();
            }
            PointerMode = true;
            FormDisplay.DrawBorder(false);
            FormCollection.ToThrough();
            FormButtonHitter.Show();
            FormButtonHitter.timer1_Tick(null, null); // Force Size recomputation for alt+tab processing
        }

        public void UnPointer()
        {
            if (PointerMode == false)
            {
                return;
            }

            if (CleanOnHide) {
                ClearInk();
            }

            PointerMode = false;
            FormCollection.ToUnThrough();
            FormCollection.ToTopMost();
            FormCollection.Activate();
            PointerMode = false;
            if(CleanOnHide) {
                FormButtonHitter.Hide();
            }
        }

        public void SelectPen(int pen)
        {
            FormCollection.SelectPen(pen);
        }

        public void SetDefaultPens()
        {
            PenEnabled[0] = false;
            PenAttr[0] = new DrawingAttributes
            {
                Color = Color.FromArgb(80, 80, 80),
                Width = 80,
                Transparency = 0
            };

            PenEnabled[1] = true;
            PenAttr[1] = new DrawingAttributes
            {
                Color = Color.FromArgb(225, 60, 60),
                Width = 80,
                Transparency = 0
            };

            PenEnabled[2] = true;
            PenAttr[2] = new DrawingAttributes
            {
                Color = Color.FromArgb(30, 110, 200),
                Width = 80,
                Transparency = 0
            };

            PenEnabled[3] = true;
            PenAttr[3] = new DrawingAttributes
            {
                Color = Color.FromArgb(235, 180, 55),
                Width = 80,
                Transparency = 0
            };

            PenEnabled[4] = true;
            PenAttr[4] = new DrawingAttributes
            {
                Color = Color.FromArgb(120, 175, 70),
                Width = 80,
                Transparency = 0
            };

            PenEnabled[5] = true;
            PenAttr[5] = new DrawingAttributes
            {
                Color = Color.FromArgb(235, 125, 15),
                Width = 500,
                Transparency = 175
            };

            PenAttr[6] = new DrawingAttributes
            {
                Color = Color.FromArgb(230, 230, 230),
                Width = 80,
                Transparency = 0
            };

            PenAttr[7] = new DrawingAttributes
            {
                Color = Color.FromArgb(250, 140, 200),
                Width = 80,
                Transparency = 0
            };

            PenAttr[8] = new DrawingAttributes
            {
                Color = Color.FromArgb(25, 180, 175),
                Width = 80,
                Transparency = 0
            };

            PenAttr[9] = new DrawingAttributes
            {
                Color = Color.FromArgb(145, 70, 160),
                Width = 500,
                Transparency = 175
            };
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
        }

        public void SetTrayIconColor()
        {
            if (WhiteTrayIcon)
            {
                trayIcon.Icon = File.Exists("icon_white.ico") ? new Icon("icon_white.ico") : global::gInk.Properties.Resources.icon_white;
            }
            else
            {
                trayIcon.Icon = File.Exists("icon_red.ico") ? new Icon("icon_red.ico") : global::gInk.Properties.Resources.icon_red;
            }


        }

        public void ReadOptions(string file)
        {
            if (!File.Exists(file))
            {
                file = AppDomain.CurrentDomain.BaseDirectory + file;
            }

            if (!File.Exists(file))
            {
                return;
            }

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
                    sLine.Contains("=") &&
                    !sLine.Substring(sLine.IndexOf("=") + 1).Contains("=")
                )
                {
                    sName = sLine.Substring(0, sLine.IndexOf("="));
                    sName = sName.Trim();
                    sName = sName.ToUpper();
                    sPara = sLine.Substring(sLine.IndexOf("=") + 1);
                    sPara = sPara.Trim();

                    if (sName.StartsWith("PEN"))
                    {
                        int penid = 0;
                        if (int.TryParse(sName.Substring(3, 1), out penid))
                        {
                            if (sName.EndsWith("_ENABLED"))
                            {
                                if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                                {
                                    PenEnabled[penid] = true;
                                }
                                else if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                                {
                                    PenEnabled[penid] = false;
                                }
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
                            if (sName.EndsWith("_FADING"))
                            {
                                float k;
                                if (sPara.ToUpper() == "Y")
                                {
                                    k = TimeBeforeFading;
                                }

                                if (sPara.ToUpper() == "N")
                                {
                                    k = -1;
                                }
                                else if (!float.TryParse(sPara, out k))
                                {
                                    k = TimeBeforeFading;
                                }

                                if (k > 0)
                                {
                                    PenAttr[penid].ExtendedProperties.Add(FADING_PEN, k);
                                }
                            }

                            if (sName.EndsWith("_HOTKEY"))
                            {
                                Hotkey_Pens[penid].Parse(sPara);
                            }
                        }

                    }

                    int tempi = 0;
                    float tempf = 0;
                    string[] tab;
                    switch (sName)
                    {
                        case "LANGUAGE_FILE":
                            ChangeLanguage(sPara);
                            break;
                        case "ALT_AS_TEMPORARY_COMMAND":
                            AltAsOneCommand = sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON";
                            break;
                        case "HOTKEY_GLOBAL":
                            Hotkey_Global.Parse(sPara);
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

                        case "WHITE_TRAY_ICON":
                            WhiteTrayIcon = sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON";
                            break;
                        case "HIDE_IN_ALTTAB":
                            globalRoot.HideInAltTab = sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON";
                            break;
                        case "SNAPSHOT_PATH":
                            SnapshotBasePath = sPara;
                            if (!SnapshotBasePath.EndsWith("/") && !SnapshotBasePath.EndsWith("\\"))
                            {
                                SnapshotBasePath += "/";
                            }

                            break;
                        case "OPEN_INTO_SNAP":
                            OpenIntoSnapMode = sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "OFF";
                            break;
                        case "CLEAN_ON_HIDE":
                            CleanOnHide = sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "OFF";
                            break;
                        case "DRAWING_ICON":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                            {
                                ToolsEnabled = false;
                            }

                            break;
                        case "ARROW":           // angle in degrees, len in % of the screen width
                            tab = sPara.Split(',');
                            if (tab.Length != 2)
                            {
                                break;
                            }

                            if (float.TryParse(tab[0], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out tempf))
                            {
                                ArrowAngle = tempf * Math.PI / 180;
                            }

                            if (float.TryParse(tab[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out tempf))
                            {
                                ArrowLen = tempf / 100.0 * System.Windows.SystemParameters.PrimaryScreenWidth;
                            }

                            break;
                        case "DEFAULT_ARROW":
                            if (sPara.ToUpper() == "START")
                            {
                                DefaultArrow_start = true;
                            }

                            if (sPara.ToUpper() == "END")
                            {
                                DefaultArrow_start = false;
                            }

                            break;
                        case "TEXT":           // Font(string),italique(boolean),Bold(boolean),size(float) of the text in % of the screen, also defines the size of the
                            {
                                tab = sPara.Split(',');
                                if (tab.Length != 4)
                                {
                                    break;
                                }

                                TextFont = tab[0];
                                string s = tab[1];
                                if (s.ToUpper() == "FALSE" || s == "0" || s.ToUpper() == "OFF")
                                {
                                    TextItalic = false;
                                }
                                else if (s.ToUpper() == "TRUE" || s == "1" || s.ToUpper() == "ON")
                                {
                                    TextItalic = true;
                                }

                                s = tab[2];
                                if (s.ToUpper() == "FALSE" || s == "0" || s.ToUpper() == "OFF")
                                {
                                    TextBold = false;
                                }
                                else if (s.ToUpper() == "TRUE" || s == "1" || s.ToUpper() == "ON")
                                {
                                    TextBold = true;
                                }

                                if (float.TryParse(tab[3], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out tempf))
                                {
                                    TextSize = (int)(tempf / 100.0 * System.Windows.SystemParameters.PrimaryScreenWidth);
                                }
                            }
                            break;
                        case "NUMBERS":           // Font(string),italique(boolean),Bold(boolean),size(float) of the text in % of the screen, also defines the size of the
                            {
                                tab = sPara.Split(',');
                                if (tab.Length != 4)
                                {
                                    break;
                                }

                                TagFont = tab[0];
                                string s = tab[1];
                                if (s.ToUpper() == "FALSE" || s == "0" || s.ToUpper() == "OFF")
                                {
                                    TagItalic = false;
                                }
                                else if (s.ToUpper() == "TRUE" || s == "1" || s.ToUpper() == "ON")
                                {
                                    TagItalic = true;
                                }

                                s = tab[2];
                                if (s.ToUpper() == "FALSE" || s == "0" || s.ToUpper() == "OFF")
                                {
                                    TagBold = false;
                                }
                                else if (s.ToUpper() == "TRUE" || s == "1" || s.ToUpper() == "ON")
                                {
                                    TagBold = true;
                                }

                                if (float.TryParse(tab[3], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out tempf))
                                {
                                    TagSize = (int)(tempf / 100.0 * System.Windows.SystemParameters.PrimaryScreenWidth);
                                }
                            }
                            break;
                        case "MAGNET":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                            {
                                MagneticRadius = -MIN_MAGNETIC;
                            }
                            else if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                            {
                                MagneticRadius = MIN_MAGNETIC;
                            }
                            else if (float.TryParse(sPara, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out tempf))
                            {
                                MagneticRadius = (int)(tempf / 100.0 * System.Windows.SystemParameters.PrimaryScreenWidth);
                            }

                            break;
                        case "ERASER_ICON":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                            {
                                EraserEnabled = false;
                            }

                            break;
                        case "POINTER_ICON":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                            {
                                PointerEnabled = false;
                            }

                            break;
                        case "ALTTAB_POINTER":
                            AltTabPointer = sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON";
                            break;
                        case "PEN_WIDTH_AT_SELECTION":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                            {
                                WidthAtPenSel = false;
                            }
                            else if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                            {
                                WidthAtPenSel = true;
                            }

                            break;
                        case "PEN_WIDTH_ICON":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                            {
                                PenWidthEnabled = false;
                            }
                            else if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                            {
                                PenWidthEnabled = true;
                            }

                            break;
                        case "SNAPSHOT_ICON":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                            {
                                SnapEnabled = false;
                            }

                            break;
                        case "CLOSE_ON_SNAP":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                            {
                                CloseOnSnap = "false";
                            }
                            else if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                            {
                                CloseOnSnap = "true";
                            }
                            else if (sPara.ToUpper() == "BLANKONLY")
                            {
                                CloseOnSnap = "blankonly";
                            }

                            break;
                        case "ALWAYS_HIDE_TOOLBAR":
                            if (sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON")
                            {
                                AlwaysHideToolbar = true;
                            }

                            break;
                        case "UNDO_ICON":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                            {
                                UndoEnabled = false;
                            }

                            break;
                        case "CLEAR_ICON":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                            {
                                ClearEnabled = false;
                            }

                            break;
                        case "PAN_ICON":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                            {
                                PanEnabled = false;
                            }

                            break;
                        case "LOADSAVE_ICON":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                            {
                                LoadSaveEnabled = false;
                            }

                            break;
                        case "INKVISIBLE_ICON":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                            {
                                InkVisibleEnabled = false;
                            }

                            break;
                        case "ALLOW_DRAGGING_TOOLBAR":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                            {
                                AllowDraggingToolbar = false;
                            }

                            break;
                        case "ALLOW_HOTKEY_IN_POINTER_MODE":
                            if (sPara.ToUpper() == "FALSE" || sPara == "0" || sPara.ToUpper() == "OFF")
                            {
                                AllowHotkeyInPointerMode = false;
                            }

                            break;
                        case "ZOOM_ICON":
                            if (int.TryParse(sPara, out tempi) && tempi >= 0 && tempi <= 3)
                            {
                                ZoomEnabled = tempi;
                            }

                            break;
                        case "TOOLBAR_LEFT":
                            if (int.TryParse(sPara, out tempi))
                            {
                                gpButtonsLeft = tempi;
                            }

                            break;
                        case "TOOLBAR_TOP":
                            if (int.TryParse(sPara, out tempi))
                            {
                                gpButtonsTop = tempi;
                            }

                            break;
                        case "TOOLBAR_HEIGHT":
                            if (float.TryParse(sPara, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out tempf))
                            {
                                ToolbarHeight = tempf;
                            }

                            break;
                        case "CANVAS_CURSOR":
                            if (sPara == "0")
                            {
                                CanvasCursor = 0;
                            }
                            else if (sPara == "1")
                            {
                                CanvasCursor = 1;
                            }

                            break;
                        case "WINDOW_POS": // if not defined, no window else 2 to 4 integers Top,Left,[Width/Height,[Opacity]]
                            tab = sPara.Split(',');
                            if (tab.Length >= 2) { FormTop = int.Parse(tab[0]); FormLeft = int.Parse(tab[1]); };
                            if (tab.Length >= 3) { FormWidth = int.Parse(tab[2]); }
                            if (tab.Length >= 4) { FormOpacity = int.Parse(tab[3]); }
                            break;
                        case "INKING_AREA": // 4 integers
                            tab = sPara.Split(',');
                            if (tab.Length <= 4)
                            {
                                int a, b, c, d;
                                if (int.TryParse(tab[0], out a) && int.TryParse(tab[1], out b) && int.TryParse(tab[2], out c) && int.TryParse(tab[3], out d))
                                {
                                    if (c > 0 && d > 0) // else default value ie full screen;
                                    {
                                        a = a < 0 ? -1 : Math.Min(Math.Max(SystemInformation.VirtualScreen.Left, a), SystemInformation.VirtualScreen.Right - c);
                                        b = b < 0 ? -1 : Math.Min(Math.Max(SystemInformation.VirtualScreen.Top, b), SystemInformation.VirtualScreen.Bottom - d);
                                        WindowRect = new Rectangle(a, b, c, d);
                                    }
                                }
                            }
                            break;
                        case "GRAY_BOARD1": // if not defined, no window else 2 to 4 integers Top,Left,[Width/Height,[Opacity]]
                            tab = sPara.Split(',');
                            if (tab.Length == 4)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    Gray1[i] = int.Parse(tab[i]);
                                }
                            };
                            break;
                        case "GRAY_BOARD2": // if not defined, no window else 2 to 4 integers Top,Left,[Width/Height,[Opacity]]
                            tab = sPara.Split(',');
                            if (tab.Length == 4)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    Gray2[i] = int.Parse(tab[i]);
                                }
                            };
                            break;
                        case "TOOLBAR_COLOR": // if not defined, no window else 2 to 4 integers Top,Left,[Width/Height,[Opacity]]
                            tab = sPara.Split(',');
                            if (tab.Length == 4)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    ToolbarBGColor[i] = int.Parse(tab[i]);
                                }
                            };
                            break;
                        case "BOARDATOPENING":
                            if (int.TryParse(sPara, out tempi))
                            {
                                BoardAtOpening = tempi;
                            }

                            if (BoardAtOpening != 4)
                            {
                                BoardSelected = BoardAtOpening;
                            }

                            break;

                        case "VIDEO_RECORD_MODE":
                            try
                            {
                                VideoRecordMode = (VideoRecordMode)Enum.Parse(typeof(VideoRecordMode), sPara, false);
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
                        case "FFMPEG_CMD":
                            FFMpegCmd = sPara;
                            break;
                        case "IMAGESTAMP_SIZE":
                            if (int.TryParse(sPara, out tempi))
                            {
                                StampSize = tempi;
                            }

                            break;
                        case "IMAGESTAMP_FILLING":
                            if (int.TryParse(sPara, out tempi))
                            {
                                ImageStampFilling = tempi;
                            }

                            break;
                        case "IMAGESTAMP_FILENAMES":
                            if (sPara.Length == 0)
                            {
                                break;
                            }

                            string[] st = sPara.Replace('\\', '/').Trim(';').Split(';');
                            foreach (string st1 in st)
                            {
                                string st2 = !Path.IsPathRooted(st1) ? Global.ProgramFolder + st1 : st1;
                                //if (!Path.IsPathFullyQualified(st1))
                                if (!StampFileNames.Contains(st2))
                                {
                                    StampFileNames.Insert(StampFileNames.Count, st2);
                                }
                            }
                            break;
                        case "IMAGESTAMP1":
                            if (sPara.Length == 0)
                            {
                                sPara = "";
                            }
                            else if (!Path.IsPathRooted(sPara))
                            {
                                sPara = Global.ProgramFolder + sPara;
                            }

                            if (!StampFileNames.Contains(sPara))        // to ensure the files are within the stamfiles;
                            {
                                StampFileNames.Insert(StampFileNames.Count, sPara);
                            }

                            ImageStamp1 = sPara;
                            break;
                        case "IMAGESTAMP2":
                            if (sPara.Length == 0)
                            {
                                sPara = "";
                            }
                            else if (!Path.IsPathRooted(sPara))
                            {
                                sPara = Global.ProgramFolder + sPara;
                            }

                            if (!StampFileNames.Contains(sPara))
                            {
                                StampFileNames.Insert(StampFileNames.Count, sPara);
                            }

                            ImageStamp2 = sPara;
                            break;
                        case "IMAGESTAMP3":
                            if (sPara.Length == 0)
                            {
                                sPara = "";
                            }
                            else if (!Path.IsPathRooted(sPara))
                            {
                                sPara = Global.ProgramFolder + sPara;
                            }

                            if (!StampFileNames.Contains(sPara))
                            {
                                StampFileNames.Insert(StampFileNames.Count, sPara);
                            }

                            ImageStamp3 = sPara;
                            break;
                        case "TOOLBAR_DIRECTION":
                            if (sPara.ToUpper() == "LEFT")
                            {
                                ToolbarOrientation = Orientation.toLeft;
                            }

                            if (sPara.ToUpper() == "RIGHT")
                            {
                                ToolbarOrientation = Orientation.toRight;
                            }

                            if (sPara.ToUpper() == "UP")
                            {
                                ToolbarOrientation = Orientation.toUp;
                            }

                            if (sPara.ToUpper() == "DOWN")
                            {
                                ToolbarOrientation = Orientation.toDown;
                            }

                            break;


                        case "FADING_TIME":
                            if (float.TryParse(sPara, out tempf))
                            {
                                TimeBeforeFading = tempf;
                            }

                            break;
                        case "ZOOM":     // Width;Height;scale(f);Continuous(Y/N)
                            if (sPara.Length == 0)
                            {
                                break;
                            }

                            try
                            {
                                string[] stt = sPara.Split(';');
                                int.TryParse(stt[0], out ZoomWidth);
                                int.TryParse(stt[1], out ZoomHeight);
                                float.TryParse(stt[2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out ZoomScale);
                                if (stt[3].ToUpper() == "TRUE" || stt[3] == "1" || stt[3].ToUpper() == "ON" || stt[3].ToUpper() == "Y")
                                {
                                    ZoomContinous = true;
                                }
                            }
                            catch { }
                            break;
                        case "INVERSE_MOUSEWHEEL_CONTROL":
                            InverseMousewheel = sPara.ToUpper() == "TRUE" || sPara == "1" || sPara.ToUpper() == "ON";
                            break;
                        case "SNAP_IN_POINTER_HOLD_KEY": //directly the int value; expected to be in hotkey.ini
                            if (int.TryParse(sPara, out tempi))
                            {
                                SnapInPointerHoldKey = (SnapInPointerKeys)tempi;
                            }

                            break;
                        case "SNAP_IN_POINTER_PRESSTWICE_KEY": //directly the int value; expected to be in hotkey.ini
                            if (int.TryParse(sPara, out tempi))
                            {
                                SnapInPointerPressTwiceKey = (SnapInPointerKeys)tempi;
                            }

                            break;
                    }
                }
            }
            fini.Close();
        }

        public void SaveOptions(string file)
        {
            bool StampFileNamesAlreadyFilled = false;
            if (!File.Exists(file))
            {
                file = AppDomain.CurrentDomain.BaseDirectory + file;
            }

            if (!File.Exists(file))
            {
                return;
            }

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
                        int penid = 0;
                        if (int.TryParse(sName.Substring(3, 1), out penid) && penid >= 0 && penid < MaxPenCount)
                        {
                            if (sName.EndsWith("_ENABLED"))
                            {
                                sPara = PenEnabled[penid] ? "True" : "False";
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
                            else if (sName.EndsWith("_FADING"))
                            {
                                if (PenAttr[penid].ExtendedProperties.Contains(FADING_PEN))
                                {
                                    float f = (float)PenAttr[penid].ExtendedProperties[FADING_PEN].Data;
                                    sPara = f == TimeBeforeFading ? "Y" : f.ToString();

                                }
                                else
                                {
                                    sPara = "N";
                                }
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
                            sPara = AltAsOneCommand ? "True" : "False";
                            break;
                        case "LANGUAGE_FILE":
                            sPara = Local.CurrentLanguageFile;
                            break;
                        case "HOTKEY_GLOBAL":
                            sPara = Hotkey_Global.ToStringInvariant();
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

                        case "WHITE_TRAY_ICON":
                            sPara = WhiteTrayIcon ? "True" : "False";
                            break;
                        case "HIDE_IN_ALTTAB":
                            sPara = WhiteTrayIcon ? "True" : "False";
                            break;
                        case "SNAPSHOT_PATH":
                            sPara = SnapshotBasePath;
                            break;
                        case "OPEN_INTO_SNAP":
                            sPara = OpenIntoSnapMode ? "True" : "False";
                            break;
                        case "CLEAN_ON_HIDE":
                            sPara = CleanOnHide ? "True" : "False";
                            break;

                        case "DRAWING_ICON":
                            sPara = ToolsEnabled ? "True" : "False";
                            break;
                        case "ARROW":           // angle in degrees, len in % of the screen width
                            sPara = (ArrowAngle / Math.PI * 180.0).ToString(CultureInfo.InvariantCulture) + "," + (ArrowLen / System.Windows.SystemParameters.PrimaryScreenWidth * 100.0).ToString(CultureInfo.InvariantCulture);
                            break;
                        case "DEFAULT_ARROW":
                            sPara = DefaultArrow_start ? "START" : "END";
                            break;
                        case "TEXT":           // size of the tag in % of the screen
                            sPara = TextFont + "," + (TextItalic ? "True" : "False") + "," + (TextBold ? "True" : "False") + "," + (TextSize / System.Windows.SystemParameters.PrimaryScreenWidth * 100.0).ToString(CultureInfo.InvariantCulture);
                            break;
                        case "NUMBERS":           // size of the tag in % of the screen
                            sPara = TagFont + "," + (TagItalic ? "True" : "False") + "," + (TagBold ? "True" : "False") + "," + (TagSize / System.Windows.SystemParameters.PrimaryScreenWidth * 100.0).ToString(CultureInfo.InvariantCulture);
                            break;
                        case "MAGNET":
                            sPara = (MagneticRadius / System.Windows.SystemParameters.PrimaryScreenWidth * 100.0).ToString(CultureInfo.InvariantCulture);
                            break;
                        case "ERASER_ICON":
                            sPara = EraserEnabled ? "True" : "False";
                            break;
                        case "POINTER_ICON":
                            sPara = PointerEnabled ? "True" : "False";
                            break;
                        case "ALTTAB_POINTER":
                            sPara = AltTabPointer ? "True" : "False";
                            break;
                        case "PEN_WIDTH_AT_SELECTION":
                            sPara = WidthAtPenSel ? "True" : "False";
                            break;
                        case "PEN_WIDTH_ICON":
                            sPara = PenWidthEnabled ? "True" : "False";
                            break;
                        case "SNAPSHOT_ICON":
                            sPara = SnapEnabled ? "True" : "False";
                            break;
                        case "CLOSE_ON_SNAP":
                            if (CloseOnSnap == "true")
                            {
                                sPara = "True";
                            }
                            else
                            {
                                sPara = CloseOnSnap == "false" ? "False" : "BlankOnly";
                            }

                            break;
                        case "ALWAYS_HIDE_TOOLBAR":
                            sPara = AlwaysHideToolbar ? "True" : "False";
                            break;
                        case "UNDO_ICON":
                            sPara = UndoEnabled ? "True" : "False";
                            break;
                        case "CLEAR_ICON":
                            sPara = ClearEnabled ? "True" : "False";
                            break;
                        case "PAN_ICON":
                            sPara = PanEnabled ? "True" : "False";
                            break;
                        case "LOADSAVE_ICON":
                            sPara = LoadSaveEnabled ? "True" : "False";
                            break;
                        case "ZOOM_ICON":
                            sPara = ZoomEnabled.ToString();
                            break;
                        case "INKVISIBLE_ICON":
                            sPara = PanEnabled ? "True" : "False";
                            break;
                        case "ALLOW_DRAGGING_TOOLBAR":
                            sPara = AllowDraggingToolbar ? "True" : "False";
                            break;
                        case "ALLOW_HOTKEY_IN_POINTER_MODE":
                            sPara = AllowHotkeyInPointerMode ? "True" : "False";
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
                            sPara = WindowRect.Width <= 0 || WindowRect.Height <= 0
                                ? "-1,-1,-1,-1"
                                : WindowRect.Left.ToString() + "," + WindowRect.Top.ToString() + "," + WindowRect.Width.ToString() + "," + WindowRect.Height.ToString();
                            break;
                        case "GRAYBOARD2":
                            sPara = Gray2[0].ToString() + "," + Gray2[1].ToString() + "," + Gray2[2].ToString() + "," + Gray2[3].ToString();
                            break;
                        case "TOOLBAR_COLOR":
                            sPara = ToolbarBGColor[0].ToString() + "," + ToolbarBGColor[1].ToString() + "," + ToolbarBGColor[2].ToString() + "," + ToolbarBGColor[3].ToString();
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
                        case "FFMPEG_CMD":
                            sPara = FFMpegCmd;
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
                                    sPara += MakeRelativePath(Global.ProgramFolder, st1).Replace('\\', '/') + ";";
                                }

                                if (sPara.Length > 1)
                                {
                                    sPara = sPara.Remove(sPara.Length - 1, 1); // to suppress last ;
                                }
                                else //if(sPara.Length <=1)
                                {
                                    sPara = " ";
                                }

                                StampFileNamesAlreadyFilled = true;
                            }
                            else
                            {
                                sPara = " ";
                            }

                            break;
                        case "IMAGESTAMP1":
                            sPara = MakeRelativePath(Global.ProgramFolder, ImageStamp1);
                            break;
                        case "IMAGESTAMP2":
                            sPara = MakeRelativePath(Global.ProgramFolder, ImageStamp2);
                            break;
                        case "IMAGESTAMP3":
                            sPara = MakeRelativePath(Global.ProgramFolder, ImageStamp3);
                            break;
                        case "TOOLBAR_DIRECTION":
                            if (ToolbarOrientation == Orientation.toLeft)
                            {
                                sPara = "Left";
                            }

                            if (ToolbarOrientation == Orientation.toRight)
                            {
                                sPara = "Right";
                            }

                            if (ToolbarOrientation == Orientation.toUp)
                            {
                                sPara = "Up";
                            }

                            if (ToolbarOrientation == Orientation.toDown)
                            {
                                sPara = "Down";
                            }

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

                    }
                }
                if (sPara != "")
                {
                    writelines.Add(sNameO + "= " + sPara);
                }
                else if (sLine != null)
                {
                    writelines.Add(sLine);
                }
            }
            fini.Close();

            FileStream frini = new FileStream(file, FileMode.Create);
            StreamWriter swini = new StreamWriter(frini)
            {
                AutoFlush = true
            };
            foreach (string line in writelines)
            {
                swini.WriteLine(line);
            }

            frini.Close();
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
            if (FormOptions != null)
            {
                return;
            }

            if (FormDisplay != null || FormCollection != null)
            {
                return;
            }

            FormOptions = new FormOptions(this);
            FormOptions.Show();
        }

        public void SetHotkey()
        {
            int modifier = 0;
            if (Hotkey_Global.Control)
            {
                modifier |= 0x2;
            }

            if (Hotkey_Global.Alt)
            {
                modifier |= 0x1;
            }

            if (Hotkey_Global.Shift)
            {
                modifier |= 0x4;
            }

            if (Hotkey_Global.Win)
            {
                modifier |= 0x8;
            }

            if (modifier != 0)
            {
                RegisterHotKey(IntPtr.Zero, 0, modifier, Hotkey_Global.Key);
            }
        }

        public void UnsetHotkey()
        {
            int modifier = 0;
            if (Hotkey_Global.Control)
            {
                modifier |= 0x2;
            }

            if (Hotkey_Global.Alt)
            {
                modifier |= 0x1;
            }

            if (Hotkey_Global.Shift)
            {
                modifier |= 0x4;
            }

            if (Hotkey_Global.Win)
            {
                modifier |= 0x8;
            }

            if (modifier != 0)
            {
                UnregisterHotKey(IntPtr.Zero, 0);
            }
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
            return Convert.ToInt32(hi * 0.037795280352161);
        }

        public int PixelToHiMetric(double pi)
        {
            return Convert.ToInt32(pi / 0.037795280352161);
        }

        public static string MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                throw new ArgumentNullException("fromPath");
            }

            if (string.IsNullOrEmpty(toPath))
            {
                throw new ArgumentNullException("toPath");
            }

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        [DllImport("user32.dll")]
        private static extern int RegisterHotKey(IntPtr hwnd, int id, int fsModifiers, int vk);
        [DllImport("user32.dll")]
        private static extern int UnregisterHotKey(IntPtr hwnd, int id);
    }
}

