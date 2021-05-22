using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Ink;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using System.Security.Cryptography;
using System.Reflection;
using System.Drawing.Imaging;

namespace gInk
{
    public partial class FormCollection : Form
    {
        [Flags, Serializable]
        public enum RegisterTouchFlags
        {
            TWF_NONE = 0x00000000,
            TWF_FINETOUCH = 0x00000001, //Specifies that hWnd prefers noncoalesced touch input.
            TWF_WANTPALM = 0x00000002 //Setting this flag disables palm rejection which reduces delays for getting WM_TOUCH messages.
        }
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool RegisterTouchWindow(IntPtr hWnd, RegisterTouchFlags flags);

        // to load correctly customed cursor file
        static class MyNativeMethods
        {
            public static System.Windows.Forms.Cursor LoadCustomCursor(string path)
            {
                IntPtr hCurs = LoadCursorFromFile(path);
                if (hCurs == IntPtr.Zero) throw new Win32Exception();
                var curs = new System.Windows.Forms.Cursor(hCurs);
                // Note: force the cursor to own the handle so it gets released properly
                //var fi = typeof(System.Windows.Forms.Cursor).GetField("ownHandle", BindingFlags.NonPublic | BindingFlags.Instance);
                //fi.SetValue(curs, true);
                return curs;
            }
            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            private static extern IntPtr LoadCursorFromFile(string path);
        }

        // Button/Tooblar
        const double NormSizePercent = 0.85;
        const double SmallSizePercent = 0.44;
        const double TopPercent = 0.06;
        const double SmallButtonNext = 0.44;
        const double InterButtonGap = NormSizePercent * .05;

        // hotkeys
        const int VK_SHIFT = 0x10;
        const int VK_CONTROL = 0x11;
        const int VK_MENU = 0x12;
        const int VK_LCONTROL = 0xA2;
        const int VK_RCONTROL = 0xA3;
        const int VK_LSHIFT = 0xA0;
        const int VK_RSHIFT = 0xA1;
        const int VK_LMENU = 0xA4;
        const int VK_RMENU = 0xA5;
        const int VK_LWIN = 0x5B;
        const int VK_RWIN = 0x5C;
        private PenModifyDlg PenModifyDlg;
        public Root Root;
        public InkOverlay IC;

        public Button[] btPen;
        public Bitmap image_exit, image_clear, image_undo, image_snap, image_penwidth;
        public Bitmap image_dock, image_dockback;
        public Bitmap image_pencil, image_highlighter, image_pencil_act, image_highlighter_act;
        public Bitmap image_pointer, image_pointer_act;
        public Bitmap[] image_pen;
        public Bitmap[] image_pen_act;
        public Bitmap image_eraser_act, image_eraser;
        public Bitmap image_visible_not, image_visible;
        public System.Windows.Forms.Cursor cursorred, cursorsnap, cursorerase;
        public System.Windows.Forms.Cursor cursortip;
        public System.Windows.Forms.Cursor tempArrowCursor = null;
        public bool Initializing;

        public DateTime MouseTimeDown;
        public object MouseDownButtonObject;
        public int ButtonsEntering = 0;  // -1 = exiting
        public int gpButtonsLeft, gpButtonsTop, gpButtonsWidth, gpButtonsHeight; // the default location, fixed
        public Size VisibleToolbar = new Size();

        public bool gpPenWidth_MouseOn = false;

        public int PrimaryLeft, PrimaryTop;

        private int LastPenSelected = 0;
        private int SavedTool = -1;
        private int SavedFilled = -1;
        private int SavedPen = -1;
        private int PolyLineLastX = int.MinValue;
        private int PolyLineLastY = int.MinValue;
        private Stroke PolyLineInProgress = null;

        public bool SnapWithoutClosing = false;

        // we have local variables for font to have an session limited default font characteristics
        public int TextSize = 25;
        public string TextFont = "Arial";
        public bool TextItalic = false;
        public bool TextBold = false;
        public int TagSize = 16;
        public string TagFont = "Arial";
        public bool TagItalic = false;
        public bool TagBold = false;

        private bool SetWindowInputRectFlag = false;

        public ImageLister ClipartsDlg;
        private object btClipSel;

        private ZoomForm ZoomForm = new ZoomForm();
        private Bitmap ZoomImage, ZoomImage2;
        int ZoomFormRePosX;
        int ZoomFormRePosY;
        string ZoomSaveStroke;
        public MouseButtons CurrentMouseButton = MouseButtons.None;

        public string SaveStrokeFile = "";
        public List<string> PointerModeSnaps = new List<string>();

        // http://www.csharp411.com/hide-form-from-alttab/
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // turn on WS_EX_TOOLWINDOW style bit
                cp.ExStyle |= 0x80;
                return cp;
            }
        }
        void PreparePenImages(int Transparency, ref Bitmap img_pen, ref Bitmap img_pen_act)
        {
            if (Transparency >= 100)
            {
                img_pen = image_highlighter;
                img_pen_act = image_highlighter_act;
            }
            else
            {
                img_pen = image_pencil;
                img_pen_act = image_pencil_act;
            }
        }

        static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CloseHandle(IntPtr hObject);

            //[DllImport("kernel32.dll")]
            //public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

            [DllImport("kernel32.dll")]
            public static extern uint SuspendThread(IntPtr hThread);

            [DllImport("kernel32.dll")]
            public static extern uint ResumeThread(IntPtr hThread);
        }

        public static System.Windows.Forms.Cursor getCursFromDiskOrRes(string name, System.Windows.Forms.Cursor nocurs)
        {
            string filename;
            string[] exts = { ".cur", ".ani", ".ico" };
            try
            {
                foreach (string ext in exts)
                {
                    filename = Global.ProgramFolder + name + ext;
                    if (File.Exists(filename))
                        try
                        {
                            return new System.Windows.Forms.Cursor(filename);
                        }
                        catch (Exception e)
                        {
                            Program.WriteErrorLog(string.Format("File {0} found but can not be loaded:\n{1}\n", filename, e));
                        }
                }
                return new System.Windows.Forms.Cursor(((System.Drawing.Icon)Properties.Resources.ResourceManager.GetObject(name)).Handle);
            }
            catch
            {
                return nocurs;
            }
        }

        string[] ImageExts = { ".png" };

        public static Bitmap getImgFromDiskOrRes(string name, string[] exts = null)
        {
            string filename;
            if (Path.HasExtension(name))
                exts = new string[] { "" };
            else if (exts == null)
            {
                exts = new string[] { ".png", ".jpg", ".jpeg" };
            }
            foreach (string ext in exts)
            {
                if (Path.IsPathRooted(name))
                    filename = name + ext;
                else
                    filename = Global.ProgramFolder + name + ext;
                if (File.Exists(filename))
                    try
                    {
                        return new Bitmap(filename);
                    }
                    catch (Exception e)
                    {
                        Program.WriteErrorLog(string.Format("File {0} found but can not be loaded:{1} \n", filename, e));
                    }
            }
            return new Bitmap((Bitmap)Properties.Resources.ResourceManager.GetObject(name));
        }

        private void SetButtonPosition(Button previous, Button current, int spacing)
        {
            if (Root.ToolbarOrientation == Orientation.toLeft)
            {
                current.Left = previous.Left + previous.Width + spacing;
                current.Top = previous.Top;
            }
            else if (Root.ToolbarOrientation == Orientation.toRight)
            {
                current.Left = previous.Left - spacing - current.Width;
                current.Top = previous.Top;
            }
            else if (Root.ToolbarOrientation == Orientation.toDown)
            {
                current.Left = previous.Left;
                current.Top = previous.Top - spacing - current.Height;
            }
            else if (Root.ToolbarOrientation == Orientation.toUp)
            {
                current.Left = previous.Left;
                current.Top = previous.Top + previous.Height + spacing;
            }
        }

        private void SetSmallButtonNext(Button previous, Button current, int incr)
        {
            if (Root.ToolbarOrientation <= Orientation.Horizontal)
            {
                current.Left = previous.Left;
                current.Top = previous.Top + incr;
            }
            else
            {
                current.Left = previous.Left + incr;
                current.Top = previous.Top;
            }
        }

        public Bitmap buildPenIcon(Color col, int transparency, bool Sel)
        {
            Bitmap fg, img;
            ImageAttributes imageAttributes = new ImageAttributes();
            bool Large = transparency >= 100;

            float[][] colorMatrixElements = {
                       new float[] {col.R/255.0f,  0,  0,  0, 0},
                       new float[] {0,  col.G / 255.0f,  0,  0, 0},
                       new float[] {0,  0,  col.B / 255.0f,  0, 0},
                       new float[] {0,  0,  0,  (255-transparency) / 255.0f, 0},
                       new float[] {0,  0,  0,     0,  1}};
            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            img = getImgFromDiskOrRes((Large ? "Lpen" : "pen") + (Sel ? "S" : "") + "_bg", ImageExts);
            fg = getImgFromDiskOrRes((Large ? "Lpen" : "pen") + (Sel ? "S" : "") + "_col", ImageExts);

            Graphics g = Graphics.FromImage(img);
            g.DrawImage(fg, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imageAttributes);
            return img;
        }

        public FormCollection(Root root)
        {
            Root = root;

            /* // Kept for debug if required
            using (StreamWriter sw = File.AppendText("LogKey.txt"))
                sw.WriteLine("Start inking");
            */

            //Console.WriteLine("A=" + (DateTime.Now.Ticks/1e7).ToString());
            InitializeComponent();
            if (Root.WindowRect.Width <= 0 || Root.WindowRect.Height <= 0)
            {
                this.Left = SystemInformation.VirtualScreen.Left;
                this.Top = SystemInformation.VirtualScreen.Top;
                this.Width = SystemInformation.VirtualScreen.Width;
                this.Height = SystemInformation.VirtualScreen.Height - 2;
                PrimaryLeft = Screen.PrimaryScreen.Bounds.Left - SystemInformation.VirtualScreen.Left;
                PrimaryTop = Screen.PrimaryScreen.Bounds.Top - SystemInformation.VirtualScreen.Top;
            }
            else // window mode
            {
                this.Left = Math.Min(Math.Max(SystemInformation.VirtualScreen.Left, Root.WindowRect.Left), SystemInformation.VirtualScreen.Right - Root.WindowRect.Width);
                this.Top = Math.Min(Math.Max(SystemInformation.VirtualScreen.Top, Root.WindowRect.Top), SystemInformation.VirtualScreen.Bottom - Root.WindowRect.Height);
                this.Width = Root.WindowRect.Width;
                this.Height = Root.WindowRect.Height;
                PrimaryLeft = 0; // top corner: Screen.PrimaryScreen.Bounds.Left - SystemInformation.VirtualScreen.Left;
                PrimaryTop = 0;  //             Screen.PrimaryScreen.Bounds.Top - SystemInformation.VirtualScreen.Top;
            }

            ZoomImage = new Bitmap(Root.ZoomWidth, Root.ZoomHeight);
            ZoomImage2 = new Bitmap(Root.ZoomWidth, Root.ZoomHeight);
            ZoomForm.pictureBox1.BackgroundImage = ZoomImage;
            ZoomForm.pictureBox2.BackgroundImage = ZoomImage2;
            ZoomFormRePosX = ZoomImage.Width / 2;
            ZoomFormRePosY = ZoomImage.Height / 2;
            ZoomForm.Width = (int)(Root.ZoomWidth * Root.ZoomScale);
            ZoomForm.Height = (int)(Root.ZoomHeight * Root.ZoomScale);
            ZoomSaveStroke = Path.GetFullPath(Environment.ExpandEnvironmentVariables("%temp%/ZoomSave.strokes.txt")).Replace('\\', '/');

            //Console.WriteLine("B=" + (DateTime.Now.Ticks/1e7).ToString());
            ClipartsDlg = new ImageLister(Root);
            Initializing = true;
            //loading default params
            TextFont = Root.TextFont;
            TextBold = Root.TextBold;
            TextItalic = Root.TextItalic;
            TextSize = Root.TextSize;
            TagFont = Root.TagFont;
            TagBold = Root.TagBold;
            TagItalic = Root.TagItalic;
            TagSize = Root.TagSize;

            gpButtons.BackColor = Color.FromArgb(Root.ToolbarBGColor[0], Root.ToolbarBGColor[1], Root.ToolbarBGColor[2], Root.ToolbarBGColor[3]);
            gpPenWidth.BackColor = Color.FromArgb(Root.ToolbarBGColor[0], Root.ToolbarBGColor[1], Root.ToolbarBGColor[2], Root.ToolbarBGColor[3]);

            longClickTimer.Interval = (int)(Root.LongClickTime * 1000 + 100);

            this.DoubleBuffered = true;

            int nbPen = 0;
            for (int b = 0; b < Root.MaxPenCount; b++)
                if (Root.PenEnabled[b])
                    nbPen++;

            // set dimensions and positions 
            int dim = (int)Math.Round(Screen.PrimaryScreen.Bounds.Height * Root.ToolbarHeight);
            int dim1 = (int)(dim * NormSizePercent);
            int dim1s = (int)(dim * SmallSizePercent);
            int dim2 = (int)(dim * TopPercent);
            int dim2s = (int)(dim * SmallButtonNext);
            int dim3 = (int)(dim * InterButtonGap);
            int dim4 = dim1 + dim3;
            int dim4s = dim1s + dim3;
            /*            const double NormSizePercent = 0.85;
            const double SmallSizePercent = 0.44;
            const double TopPercent = 0.06;
            const double SmallButtonNext = 0.44;
            const double InterButtonGap = NormSizePercent * .05;
            */
            if (Root.ToolbarOrientation <= Orientation.Horizontal)
            {
                gpButtons.Height = dim;
                gpButtons.Width = (int)((dim1 * .5 + dim3) + (nbPen * dim4 + (Root.ToolsEnabled ? (6 * dim4s + dim4s) : 0) + (Root.EraserEnabled ? dim4 : 0) + (Root.PanEnabled ? dim4 : 0) + (Root.PointerEnabled ? dim4 : 0)
                                                                         + (Root.PenWidthEnabled ? dim4 : 0) + (Root.InkVisibleEnabled ? dim4 : 0) + (Root.SnapEnabled ? dim4 : 0)
                                                                         + (Root.UndoEnabled ? dim4 : 0) + (Root.ClearEnabled ? dim4 : 0) + (Root.LoadSaveEnabled ? dim4s : 0)
                                                                         + ((Root.VideoRecordMode != VideoRecordMode.NoVideo) ? dim4 : 0)
                                                                         + dim1));
            }
            else //Vertical
            {
                gpButtons.Width = dim;
                gpButtons.Height = (int)((dim1 * .5 + dim3) + (nbPen * dim4 + (Root.ToolsEnabled ? (6 * dim4s + dim4s) : 0) + (Root.EraserEnabled ? dim4 : 0) + (Root.PanEnabled ? dim4 : 0) + (Root.PointerEnabled ? dim4 : 0)
                                                                            + (Root.PenWidthEnabled ? dim4 : 0) + (Root.InkVisibleEnabled ? dim4 : 0) + (Root.SnapEnabled ? dim4 : 0)
                                                                            + (Root.UndoEnabled ? dim4 : 0) + (Root.ClearEnabled ? dim4 : 0) + (Root.LoadSaveEnabled ? dim4s : 0)
                                                                            + ((Root.VideoRecordMode != VideoRecordMode.NoVideo) ? dim4 : 0)
                                                                            + dim1));
            }

            //

            if (Root.ToolbarOrientation == Orientation.toLeft)
            {
                btDock.Height = dim1;
                btDock.Width = dim1 / 2;
                btDock.BackgroundImage = getImgFromDiskOrRes(Root.Docked ? "dockback" : "dock");
                btDock.Top = dim2;
                btDock.Left = 0;
            }
            else if (Root.ToolbarOrientation == Orientation.toRight)
            {
                btDock.Height = dim1;
                btDock.Width = dim1 / 2;
                btDock.BackgroundImage = getImgFromDiskOrRes(!Root.Docked ? "dockback" : "dock");
                btDock.Top = dim2;
                btDock.Left = gpButtons.Width - btDock.Width;
            }
            else if (Root.ToolbarOrientation == Orientation.toDown)
            {
                btDock.Width = dim1;
                btDock.Height = dim1 / 2;
                btDock.BackgroundImage = getImgFromDiskOrRes(!Root.Docked ? "dockbackV" : "dockV");
                btDock.Top = gpButtons.Height - btDock.Height;
                btDock.Left = dim2;
            }
            else if (Root.ToolbarOrientation == Orientation.toUp)
            {
                btDock.Width = dim1;
                btDock.Height = dim1 / 2;
                btDock.BackgroundImage = getImgFromDiskOrRes(Root.Docked ? "dockbackV" : "dockV");
                btDock.Top = 0;
                btDock.Left = dim2;
            }

            Button prev = btDock;
            btPen = new Button[Root.MaxPenCount];
            image_pen = new Bitmap[Root.MaxPenCount];
            image_pen_act = new Bitmap[Root.MaxPenCount];

            for (int b = 0; b < Root.MaxPenCount; b++)
            {
                btPen[b] = new Button();
                btPen[b].Name = string.Format("pen{0}", b);
                image_pen[b] = new Bitmap(btPen[b].Width, btPen[b].Height);
                image_pen_act[b] = new Bitmap(btPen[b].Width, btPen[b].Height);
                PreparePenImages(Root.PenAttr[b].Transparency, ref image_pen[b], ref image_pen_act[b]);

                if (Root.PenEnabled[b])
                {
                    btPen[b].Width = dim1;
                    btPen[b].Height = dim1;

                    SetButtonPosition(prev, btPen[b], dim3);
                    //btPen[b].Top = (int)(gpButtons.Height * 0.08);
                    btPen[b].FlatAppearance.BorderSize = 0;
                    btPen[b].FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
                    btPen[b].FlatStyle = System.Windows.Forms.FlatStyle.Flat;

                    btPen[b].ContextMenu = new ContextMenu();
                    btPen[b].ContextMenu.Popup += new System.EventHandler(this.btColor_Click);
                    btPen[b].Click += new System.EventHandler(this.btColor_Click);

                    btPen[b].BackColor = System.Drawing.Color.Transparent;
                    btPen[b].BackgroundImageLayout = ImageLayout.Stretch;
                    this.toolTip.SetToolTip(this.btPen[b], Root.Local.ButtonNamePen[b] + " (" + Root.Hotkey_Pens[b].ToString() + ")");

                    btPen[b].MouseDown += gpButtons_MouseDown;
                    btPen[b].MouseMove += gpButtons_MouseMove;
                    btPen[b].MouseUp += gpButtons_MouseUp;

                    btPen[b].Visible = true;
                    gpButtons.Controls.Add(btPen[b]);
                    prev = btPen[b];
                }
                else
                    btPen[b].Visible = false;
            }

            if (root.ToolsEnabled)
            {
                // background images loaded/applied in SelectTool
                btHand.Height = dim1s;
                btHand.Width = dim1s;
                SetButtonPosition(prev, btHand, dim3);
                btLine.Height = dim1s;
                btLine.Width = dim1s;
                SetSmallButtonNext(btHand, btLine, dim2s);

                btRect.Height = dim1s;
                btRect.Width = dim1s;
                SetButtonPosition(btHand, btRect, dim3);
                btOval.Height = dim1s;
                btOval.Width = dim1s;
                SetSmallButtonNext(btRect, btOval, dim2s);

                btArrow.Height = dim1s;
                btArrow.Width = dim1s;
                SetButtonPosition(btRect, btArrow, dim3);
                btNumb.Height = dim1s;
                btNumb.Width = dim1s;
                SetSmallButtonNext(btArrow, btNumb, dim2s);

                btText.Height = dim1s;
                btText.Width = dim1s;
                SetButtonPosition(btArrow, btText, dim3);
                btEdit.Height = dim1s;
                btEdit.Width = dim1s;
                SetSmallButtonNext(btText, btEdit, dim2s);

                btClipArt.Height = dim1s;
                btClipArt.Width = dim1s;
                SetButtonPosition(btText, btClipArt, dim3);
                btClip1.Height = dim1s;
                btClip1.Width = dim1s;
                SetSmallButtonNext(btClipArt, btClip1, dim2s);
                btClip1.BackgroundImage = getImgFromDiskOrRes(Root.ImageStamp1, ImageExts);
                btClip1.Tag = new ClipArtData { ImageStamp = Root.ImageStamp1, X = btClip1.BackgroundImage.Size.Width, Y = btClip1.BackgroundImage.Size.Height, Filling = Root.ImageStampFilling };

                btClip2.Height = dim1s;
                btClip2.Width = dim1s;
                SetButtonPosition(btClipArt, btClip2, dim3);
                btClip2.BackgroundImage = getImgFromDiskOrRes(Root.ImageStamp2, ImageExts);
                btClip2.Tag = new ClipArtData { ImageStamp = Root.ImageStamp2, X = btClip2.BackgroundImage.Size.Width, Y = btClip2.BackgroundImage.Size.Height, Filling = Root.ImageStampFilling };
                btClip3.Height = dim1s;
                btClip3.Width = dim1s;
                SetSmallButtonNext(btClip2, btClip3, dim2s);
                btClip3.BackgroundImage = getImgFromDiskOrRes(Root.ImageStamp3, ImageExts);
                btClip3.Tag = new ClipArtData { ImageStamp = Root.ImageStamp3, X = btClip3.BackgroundImage.Size.Width, Y = btClip3.BackgroundImage.Size.Height, Filling = Root.ImageStampFilling };
                prev = btClip2;
            }
            else
            {
                btHand.Visible = false;
                btLine.Visible = false;
                btRect.Visible = false;
                btOval.Visible = false;
                btArrow.Visible = false;
                btNumb.Visible = false;
                btText.Visible = false;
                btEdit.Visible = false;
                btClipArt.Visible = false;
                btClip1.Visible = false;
                btClip2.Visible = false;
                btClip3.Visible = false;
            }

            if (root.EraserEnabled)
            {
                btEraser.Height = dim1;
                btEraser.Width = dim1;
                image_eraser_act = getImgFromDiskOrRes("eraser_act", ImageExts);
                image_eraser = getImgFromDiskOrRes("eraser", ImageExts);
                btEraser.BackgroundImage = image_eraser;
                SetButtonPosition(prev, btEraser, dim3);
                prev = btEraser;
            }
            else
                btEraser.Visible = false;

            if (root.PanEnabled)
            {
                btPan.Height = dim1;
                btPan.Width = dim1;
                btPan.BackgroundImage = getImgFromDiskOrRes("pan", ImageExts);
                SetButtonPosition(prev, btPan, dim3);
                prev = btPan;
            }
            else
                btPan.Visible = false;

            if (root.ToolsEnabled)
            {
                btMagn.Height = dim1s;
                btMagn.Width = dim1s;
                this.btMagn.BackgroundImage = getImgFromDiskOrRes((Root.MagneticRadius > 0) ? "Magnetic_act" : "Magnetic", ImageExts);
                SetButtonPosition(prev, btMagn, dim3);
                prev = btMagn;
            }
            else
                btMagn.Visible = false;

            if (root.ZoomEnabled > 0)
            {
                btZoom.Height = dim1s;
                btZoom.Width = dim1s;
                btZoom.BackgroundImage = getImgFromDiskOrRes("Zoom", ImageExts);
                SetSmallButtonNext(btMagn, btZoom, dim2s);
            }
            else
                btZoom.Visible = false;

            if (root.PointerEnabled)
            {
                btPointer.Height = dim1;
                btPointer.Width = dim1;
                image_pointer = getImgFromDiskOrRes("pointer", ImageExts);
                image_pointer_act = getImgFromDiskOrRes("pointer_act", ImageExts);
                SetButtonPosition(prev, btPointer, dim3);
                prev = btPointer;
            }
            else
                btPointer.Visible = false;

            if (root.PenWidthEnabled)
            {
                btPenWidth.Height = dim1;
                btPenWidth.Width = dim1;
                btPenWidth.BackgroundImage = getImgFromDiskOrRes("penwidth", ImageExts);
                SetButtonPosition(prev, btPenWidth, dim3);
                prev = btPenWidth;
            }
            else
                btPenWidth.Visible = false;

            if (root.InkVisibleEnabled)
            {
                btInkVisible.Height = dim1;
                btInkVisible.Width = dim1;
                image_visible_not = getImgFromDiskOrRes("visible_not", ImageExts);
                image_visible = getImgFromDiskOrRes("visible", ImageExts);
                btInkVisible.BackgroundImage = image_visible;
                SetButtonPosition(prev, btInkVisible, dim3);
                prev = btInkVisible;
            }
            else
                btInkVisible.Visible = false;

            if (root.SnapEnabled)
            {
                btSnap.Height = dim1;
                btSnap.Width = dim1;
                btSnap.BackgroundImage = getImgFromDiskOrRes("snap", ImageExts); ;
                SetButtonPosition(prev, btSnap, dim3);
                prev = btSnap;
            }
            else
                btSnap.Visible = false;

            if (root.UndoEnabled)
            {
                btUndo.Height = dim1;
                btUndo.Width = dim1;
                btUndo.BackgroundImage = getImgFromDiskOrRes("undo", ImageExts);
                SetButtonPosition(prev, btUndo, dim3);
                prev = btUndo;
            }
            else
                btUndo.Visible = false;

            if (root.ClearEnabled)
            {
                btClear.Height = dim1;
                btClear.Width = dim1;
                btClear.BackgroundImage = getImgFromDiskOrRes("garbage", ImageExts);
                SetButtonPosition(prev, btClear, dim3);
                prev = btClear;
            }
            else
                btClear.Visible = false;

            if (root.LoadSaveEnabled)
            {
                btSave.Height = dim1s;
                btSave.Width = dim1s;
                btSave.BackgroundImage = getImgFromDiskOrRes("save", ImageExts);
                SetButtonPosition(prev, btSave, dim3);
                btLoad.Height = dim1s;
                btLoad.Width = dim1s;
                btLoad.BackgroundImage = getImgFromDiskOrRes("open", ImageExts);
                SetSmallButtonNext(btSave, btLoad, dim2s);
                prev = btSave;
            }
            else
            {
                btSave.Visible = false;
                btLoad.Visible = false;
            }

            if (root.VideoRecordMode != VideoRecordMode.NoVideo)
            {
                btVideo.Height = dim1;
                btVideo.Width = dim1;
                SetButtonPosition(prev, btVideo, dim3);
                SetVidBgImage();
                if (Root.VideoRecordMode == VideoRecordMode.OBSBcst || Root.VideoRecordMode == VideoRecordMode.OBSRec)
                {

                    if (Root.ObsRecvTask == null || Root.ObsRecvTask.IsCompleted)
                    {
                        Root.VideoRecordWindowInProgress = true;
                        Root.ObsRecvTask = Task.Run(() => ReceiveObsMesgs(this));
                    }
                    while (Root.VideoRecordWindowInProgress)
                        Task.Delay(50);
                    Task.Delay(100);
                    //Task.Run(() => SendInWs(Root.ObsWs, "GetRecordingStatus", new CancellationToken()));
                    Task.Run(() => SendInWs(Root.ObsWs, "GetStreamingStatus", new CancellationToken()));
                }
                prev = btVideo;
            }
            else
                btVideo.Visible = false;

            btStop.Height = dim1;
            btStop.Width = dim1;
            btStop.BackgroundImage = getImgFromDiskOrRes("exit", ImageExts);
            SetButtonPosition(prev, btStop, dim3);

            gpButtonsWidth = gpButtons.Width;
            gpButtonsHeight = gpButtons.Height;
            VisibleToolbar.Width = gpButtonsWidth;
            VisibleToolbar.Height = gpButtonsHeight;
            gpButtonsLeft = Root.gpButtonsLeft;
            gpButtonsTop = Root.gpButtonsTop;
            if (((true || Root.AllowDraggingToolbar) && (
                  !(IsInsideVisibleScreen(gpButtonsLeft, gpButtonsTop) &&
                  IsInsideVisibleScreen(gpButtonsLeft + gpButtonsWidth, gpButtonsTop) &&
                  IsInsideVisibleScreen(gpButtonsLeft, gpButtonsTop + gpButtonsHeight) &&
                  IsInsideVisibleScreen(gpButtonsLeft + gpButtonsWidth, gpButtonsTop + gpButtonsHeight))
                  ||
                  (gpButtonsLeft == 0 && gpButtonsTop == 0)))
                || (!Root.AllowDraggingToolbar))
            {
                if (Root.WindowRect.Width <= 0 || Root.WindowRect.Height <= 0)
                {
                    switch (Root.ToolbarOrientation)
                    {
                        case Orientation.toLeft:
                            gpButtonsLeft = Screen.PrimaryScreen.WorkingArea.Right - gpButtons.Width + PrimaryLeft;
                            gpButtonsTop = Screen.PrimaryScreen.WorkingArea.Bottom - gpButtons.Height - 15 + PrimaryTop;
                            gpButtons.Left = gpButtonsLeft + gpButtons.Width;
                            gpButtons.Top = gpButtonsTop;
                            VisibleToolbar.Width = 0;
                            break;
                        case Orientation.toRight:
                            gpButtonsLeft = Screen.PrimaryScreen.WorkingArea.Left + PrimaryLeft;
                            gpButtonsTop = Screen.PrimaryScreen.WorkingArea.Bottom - gpButtons.Height - 15 + PrimaryTop;
                            gpButtons.Left = gpButtonsLeft;
                            gpButtons.Top = gpButtonsTop;
                            VisibleToolbar.Width = 0;
                            break;
                        case Orientation.toUp:
                            gpButtonsLeft = Screen.PrimaryScreen.WorkingArea.Right - gpButtons.Width - 15 + PrimaryLeft;
                            gpButtonsTop = Screen.PrimaryScreen.WorkingArea.Bottom - gpButtons.Height + PrimaryTop;
                            gpButtons.Left = gpButtonsLeft;
                            gpButtons.Top = gpButtonsTop + gpButtons.Height;
                            VisibleToolbar.Height = 0;
                            break;
                        case Orientation.toDown:
                            gpButtonsLeft = Screen.PrimaryScreen.WorkingArea.Right - gpButtons.Width - 15 + PrimaryLeft;
                            gpButtonsTop = Screen.PrimaryScreen.WorkingArea.Top + PrimaryTop;
                            gpButtons.Left = gpButtonsLeft;
                            gpButtons.Top = gpButtonsTop;
                            VisibleToolbar.Height = 0;
                            break;
                    }
                }
                else
                {
                    if (Root.ToolbarOrientation <= Orientation.Horizontal)
                    {
                        gpButtonsLeft = this.ClientRectangle.Right - gpButtons.Width;
                        gpButtonsTop = this.ClientRectangle.Bottom - gpButtons.Height;
                        gpButtons.Left = gpButtonsLeft + gpButtons.Width;
                        gpButtons.Top = gpButtonsTop;
                        VisibleToolbar.Width = 0;
                    }
                    else
                    {
                        gpButtonsLeft = this.ClientRectangle.Right - gpButtons.Width;
                        gpButtonsTop = this.ClientRectangle.Top;
                        gpButtons.Left = gpButtonsLeft;
                        gpButtons.Top = gpButtonsTop;
                        VisibleToolbar.Height = 0;
                    }
                }
                Root.gpButtonsLeft = gpButtonsLeft;
                Root.gpButtonsTop = gpButtonsTop;
            }
            else
            {
                switch (Root.ToolbarOrientation)
                {
                    case Orientation.toLeft:
                        gpButtons.Left = gpButtonsLeft + gpButtonsWidth;
                        gpButtons.Top = gpButtonsTop;
                        VisibleToolbar.Width = 0;
                        break;
                    case Orientation.toRight:
                        gpButtons.Left = gpButtonsLeft;
                        gpButtons.Top = gpButtonsTop;
                        VisibleToolbar.Width = 0;
                        break;
                    case Orientation.toUp:
                        gpButtons.Left = gpButtonsLeft + gpButtonsHeight;
                        gpButtons.Top = gpButtonsTop;
                        VisibleToolbar.Height = 0;
                        break;
                    case Orientation.toDown:
                        gpButtons.Left = gpButtonsLeft;
                        gpButtons.Top = gpButtonsTop;
                        VisibleToolbar.Height = 0;
                        break;
                }

            }
            setPenWidthBarPosition();

            pboxPenWidthIndicator.Top = 0;
            pboxPenWidthIndicator.Left = (int)Math.Sqrt(Root.GlobalPenWidth * 30);
            gpPenWidth.Controls.Add(pboxPenWidthIndicator);

            IC = new InkOverlay(this.Handle);
            IC.CollectionMode = CollectionMode.InkOnly;
            IC.AutoRedraw = false;
            IC.DynamicRendering = false;
            IC.EraserMode = InkOverlayEraserMode.StrokeErase;
            IC.CursorInRange += IC_CursorInRange;
            IC.MouseDown += IC_MouseDown;
            IC.MouseMove += IC_MouseMove;
            IC.MouseUp += IC_MouseUp;
            IC.CursorDown += IC_CursorDown;
            IC.MouseWheel += IC_MouseWheel;
            IC.Stroke += IC_Stroke;
            /*IC.DefaultDrawingAttributes.Width = 80;
            IC.DefaultDrawingAttributes.Transparency = 30;
            IC.DefaultDrawingAttributes.AntiAliased = true;
            IC.DefaultDrawingAttributes.FitToCurve = true;*/

            cursorred = getCursFromDiskOrRes("cursorarrow", System.Windows.Forms.Cursors.NoMove2D);
            cursorerase = getCursFromDiskOrRes("cursoreraser", System.Windows.Forms.Cursors.No);

            IC.Enabled = true;

            LastTickTime = DateTime.Parse("1987-01-01");
            tiSlide.Enabled = true;

            ToTransparent();
            ToTopMost();

            this.toolTip.SetToolTip(this.btDock, Root.Local.ButtonNameDock + " (" + Root.Hotkey_DockUndock.ToString() + ")");
            this.toolTip.SetToolTip(this.btPenWidth, Root.Local.ButtonNamePenwidth);
            this.toolTip.SetToolTip(this.btEraser, Root.Local.ButtonNameErasor + " (" + Root.Hotkey_Eraser.ToString() + ")");
            this.toolTip.SetToolTip(this.btPan, Root.Local.ButtonNamePan + " (" + Root.Hotkey_Pan.ToString() + ")");
            this.toolTip.SetToolTip(this.btPointer, Root.Local.ButtonNameMousePointer + " (" + Root.Hotkey_Global.ToString() + ")");
            this.toolTip.SetToolTip(this.btInkVisible, Root.Local.ButtonNameInkVisible + " (" + Root.Hotkey_InkVisible.ToString() + ")");
            this.toolTip.SetToolTip(this.btSnap, Root.Local.ButtonNameSnapshot + " (" + Root.Hotkey_Snap.ToString() + ")");
            this.toolTip.SetToolTip(this.btUndo, Root.Local.ButtonNameUndo + " (" + Root.Hotkey_Undo.ToString() + ")");
            this.toolTip.SetToolTip(this.btClear, Root.Local.ButtonNameClear + " (" + Root.Hotkey_Clear.ToString() + ")");
            this.toolTip.SetToolTip(this.btVideo, Root.Local.ButtonNameVideo + " (" + Root.Hotkey_Video.ToString() + ")");
            this.toolTip.SetToolTip(this.btStop, Root.Local.ButtonNameExit + " (" + Root.Hotkey_Close.ToString() + "/Alt+F4)");
            this.toolTip.SetToolTip(this.btHand, Root.Local.ButtonNameHand + " (" + Root.Hotkey_Hand.ToString() + ")");
            this.toolTip.SetToolTip(this.btLine, Root.Local.ButtonNameLine + " (" + Root.Hotkey_Line.ToString() + ")");
            this.toolTip.SetToolTip(this.btRect, Root.Local.ButtonNameRect + " (" + Root.Hotkey_Rect.ToString() + ")");
            this.toolTip.SetToolTip(this.btOval, Root.Local.ButtonNameOval + " (" + Root.Hotkey_Oval.ToString() + ")");
            this.toolTip.SetToolTip(this.btArrow, Root.Local.ButtonNameArrow + " (" + Root.Hotkey_Arrow.ToString() + ")");
            this.toolTip.SetToolTip(this.btNumb, Root.Local.ButtonNameNumb + " (" + Root.Hotkey_Numb.ToString() + ")");
            this.toolTip.SetToolTip(this.btText, Root.Local.ButtonNameText + " (" + Root.Hotkey_Text.ToString() + ")");
            this.toolTip.SetToolTip(this.btEdit, Root.Local.ButtonNameEdit + " (" + Root.Hotkey_Edit.ToString() + ")");
            this.toolTip.SetToolTip(this.btMagn, Root.Local.ButtonNameMagn + " (" + Root.Hotkey_Magnet.ToString() + ")");
            this.toolTip.SetToolTip(this.btZoom, Root.Local.ButtonNameZoom + " (" + Root.Hotkey_Zoom.ToString() + ")");
            this.toolTip.SetToolTip(this.btClipArt, Root.Local.ButtonNameClipArt + " (" + Root.Hotkey_ClipArt.ToString() + ")");
            this.toolTip.SetToolTip(this.btClip1, Root.Local.ButtonNameClipArt + "-1 (" + Root.Hotkey_ClipArt1.ToString() + ")");
            this.toolTip.SetToolTip(this.btClip2, Root.Local.ButtonNameClipArt + "-2 (" + Root.Hotkey_ClipArt2.ToString() + ")");
            this.toolTip.SetToolTip(this.btClip3, Root.Local.ButtonNameClipArt + "-3 (" + Root.Hotkey_ClipArt3.ToString() + ")");
            this.toolTip.SetToolTip(this.btSave, string.Format(Root.Local.SaveStroke, ""));
            this.toolTip.SetToolTip(this.btLoad, string.Format(Root.Local.LoadStroke, ""));

            foreach (Control ct in gpButtons.Controls)
            {
                if (ct.GetType() == typeof(Button))
                {
                    //Console.WriteLine("evt : " + ct.Name);
                    ct.MouseDown += new MouseEventHandler(this.btAllButtons_MouseDown);
                    ct.MouseUp += new MouseEventHandler(this.btAllButtons_MouseUp);
                    ct.ContextMenu = new ContextMenu();
                    ct.ContextMenu.Popup += new EventHandler(this.btAllButtons_RightClick);
                }
            }
            PenModifyDlg = new PenModifyDlg(Root); // It seems to be a little long to build so we prepare it.
            SelectPen(0);
            IC.DefaultDrawingAttributes.Width = Root.PenAttr[0].Width; //required to ensure width
            SelectTool(0, 0); // Select Hand Drawing by Default

            //Console.WriteLine("C=" + (DateTime.Now.Ticks/1e7).ToString());
        }


        private void setPenWidthBarPosition()
        {
            if (Root.ToolbarOrientation <= Orientation.Horizontal)
            {
                gpPenWidth.Left = gpButtonsLeft + btPenWidth.Left; // gpButtonsLeft + btPenWidth.Left - gpPenWidth.Width / 2 + btPenWidth.Width / 2;
                gpPenWidth.Top = gpButtonsTop - gpPenWidth.Height - 10;
                if (!(IsInsideVisibleScreen(gpPenWidth.Left, gpPenWidth.Top) && IsInsideVisibleScreen(gpPenWidth.Right, gpPenWidth.Bottom)))
                    gpPenWidth.Top = gpButtonsTop + gpButtonsHeight + 10;
            }
            else
            {
                gpPenWidth.Top = gpButtonsTop + btPenWidth.Top;
                gpPenWidth.Left = gpButtonsLeft - gpPenWidth.Width - 10; // gpButtonsLeft + btPenWidth.Left - gpPenWidth.Width / 2 + btPenWidth.Width / 2;
                if (!(IsInsideVisibleScreen(gpPenWidth.Left, gpPenWidth.Top) && IsInsideVisibleScreen(gpPenWidth.Right, gpPenWidth.Bottom)))
                    gpPenWidth.Left = gpButtonsLeft + gpButtonsWidth + 10;
            }
        }

        private void setClipArtDlgPosition()
        {
            if (Root.ToolbarOrientation <= Orientation.Horizontal)
            {
                ClipartsDlg.Left = gpButtons.Right - ClipartsDlg.Width - 1;
                ClipartsDlg.Top = gpButtons.Top - ClipartsDlg.Height - 1;
                if (!(IsInsideVisibleScreen(ClipartsDlg.Left, ClipartsDlg.Top) && IsInsideVisibleScreen(ClipartsDlg.Right, ClipartsDlg.Bottom)))
                    ClipartsDlg.Top = gpButtons.Bottom + 1;
            }
            else // vertical
            {
                ClipartsDlg.Left = gpButtons.Left - ClipartsDlg.Width - 1;
                ClipartsDlg.Top = gpButtons.Top + 1;
                if (!(IsInsideVisibleScreen(ClipartsDlg.Left, ClipartsDlg.Top) && IsInsideVisibleScreen(ClipartsDlg.Right, ClipartsDlg.Bottom)))
                    ClipartsDlg.Left = gpButtons.Right + 1;
            }
        }

        // I want to be able to use the space,escape,... I must not leave leave the application handle those and generate clicks...
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            return true;
        }

        //public override bool PreProcessMessage(ref Message msg)
        //[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]

        public void AltTabActivate()
        {
            if (Initializing)
            {
                Initializing = false;
                return;
            }
            if (ButtonsEntering != 0)
            {
                //Console.WriteLine("Entering");
                return;
            }
            //Console.WriteLine("activating " + (Root.PointerMode ? "pointer" : "not") + (Root.FormButtonHitter.Visible ? "visible" : "not")+ Root.FormButtonHitter.Width.ToString());
            if (Root.FormButtonHitter.Visible && (Math.Min(Root.FormButtonHitter.Width, Root.FormButtonHitter.Height) <= Math.Min(Root.FormCollection.btDock.Width, Root.FormCollection.btDock.Height) * 1.5))
            {
                //Console.WriteLine("process ");
                SelectPen(LastPenSelected);
                SelectTool(SavedTool, SavedFilled);
                SavedTool = -1;
                SavedFilled = -1;
                Root.FormDisplay.DrawBorder(true);
                Root.UnDock();
                Root.UponAllDrawingUpdate = true;
                Root.UponButtonsUpdate |= 0x7;

            }
        }
        protected override void WndProc(ref Message msg)
        {
            if (msg.Msg == 0x001C) //WM_ACTIVATEAPP : generated through alt+tab
            {
                if (Root.FormDisplay != null)
                {
                    //Console.WriteLine(Root.FormDisplay.HasFocus() ? "WM_ACT" : "!WM");
                    Root.FormDisplay.DrawBorder(Root.FormDisplay.HasFocus());
                    Root.FormDisplay.UpdateFormDisplay(true);
                }
                if (!Root.AltTabPointer)
                    return;
                if (msg.WParam == IntPtr.Zero)
                {
                    //Console.WriteLine("desactivating " + Root.PointerMode.ToString());
                    if (!Root.PointerMode)
                    {
                        //Console.WriteLine("process ");
                        SavedTool = Root.ToolSelected;
                        SavedFilled = Root.FilledSelected;
                        SelectPen(-2);
                        Root.Dock();
                        return;
                    }
                }
                else
                {
                    AltTabActivate();
                    return;
                }
            }
            base.WndProc(ref msg);
        }

        private void SetVidBgImage()
        {
            if (Root.VideoRecInProgress == VideoRecInProgress.Stopped)
                btVideo.BackgroundImage = getImgFromDiskOrRes("VidStop", ImageExts);
            else if (Root.VideoRecInProgress == VideoRecInProgress.Recording)
                btVideo.BackgroundImage = getImgFromDiskOrRes("VidRecord", ImageExts);
            else if (Root.VideoRecInProgress == VideoRecInProgress.Streaming)
                btVideo.BackgroundImage = getImgFromDiskOrRes("VidBroadcast", ImageExts);
            else if (Root.VideoRecInProgress == VideoRecInProgress.Paused)
                btVideo.BackgroundImage = getImgFromDiskOrRes("VidPause", ImageExts);
            else
            {
                btVideo.BackgroundImage = getImgFromDiskOrRes("VidUnk", ImageExts);
                //Console.WriteLine("VideoRecInProgress " + Root.VideoRecInProgress.ToString());
            }
            Root.UponButtonsUpdate |= 0x2;
        }

        private void IC_MouseWheel(object sender, CancelMouseEventArgs e)
        {
            if (Root.PointerMode)   // Wheel shall not be taken into account in edit mode
                return;
            if (ZoomForm.Visible && ((GetKeyState(VK_CONTROL)) & 0x8000) != 0)
            {
                int t = Math.Sign(e.Delta);
                ZoomForm.Height += t * (int)(10.0F * Root.ZoomHeight / Root.ZoomWidth);
                ZoomForm.Width += t * 10;
                return;
            }
            if (Root.InverseMousewheel ^ (GetKeyState(VK_SHIFT) & 0x8000) != 0)
            {
                int p = LastPenSelected + (e.Delta > 0 ? 1 : -1);
                if (p >= Root.MaxPenCount)
                    p = 0;
                if (p < 0)
                    p = Root.MaxPenCount - 1;
                while (!Root.PenEnabled[p])
                {
                    p += (e.Delta > 0 ? 1 : -1);
                    if (p >= Root.MaxPenCount)
                        p = 0;
                    if (p < 0)
                        p = Root.MaxPenCount - 1;
                }
                SelectPen(p);
                return;
            }
            else
            {
                Root.GlobalPenWidth += Root.PixelToHiMetric(e.Delta > 0 ? 2 : -2);
                if (Root.GlobalPenWidth < 1)
                    Root.GlobalPenWidth = 1;
                /*if (Root.GlobalPenWidth > 120)
                    Root.GlobalPenWidth = 120;
                */
                //Console.WriteLine(Root.GlobalPenWidth);
                IC.DefaultDrawingAttributes.Width = Root.GlobalPenWidth;
                if (Root.CanvasCursor == 1)
                    SetPenTipCursor();
                return;
            }
        }

        private bool AltKeyPressed()
        {
            return ((short)(GetKeyState(VK_LMENU) | GetKeyState(VK_RMENU)) & 0x8000) == 0x8000;
        }

        private void btAllButtons_MouseDown(object sender, MouseEventArgs e)
        {
            MouseTimeDown = DateTime.Now;
            MouseDownButtonObject = sender;
            longClickTimer.Start();
            longClickTimer.Tag = sender;
            //Console.WriteLine(string.Format("MD {0} {1}", DateTime.Now.Second, DateTime.Now.Millisecond));
            gpButtons_MouseDown(sender, e);
        }

        private void btAllButtons_MouseUp(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("MU " + (sender as Control).Name);
            MouseDownButtonObject = null;
            (sender as Button).RightToLeft = RightToLeft.No;
            longClickTimer.Stop();
            IsMovingToolbar = 0;
            gpButtons_MouseUp(sender, e);
        }

        private void btAllButtons_RightClick(object sender, EventArgs e)
        {
            MouseTimeDown = DateTime.FromBinary(0);
            MouseDownButtonObject = null;
            longClickTimer.Stop();
            sender = (sender as ContextMenu).SourceControl;
            (sender as Button).RightToLeft = RightToLeft.No;
            //Console.WriteLine(string.Format("RC {0}", (sender as Control).Name));
            (sender as Button).PerformClick();
        }

        private void longClickTimer_Tick(object sender, EventArgs e)
        {
            Button bt = MouseDownButtonObject as Button;
            MouseDownButtonObject = null;
            longClickTimer.Stop();
            //Console.WriteLine(string.Format("!LC {0}", bt.Name));
            bt.RightToLeft = RightToLeft.Yes;
            bt.PerformClick();
            if (IsMovingToolbar < 2)
                IsMovingToolbar = 0;
        }

        private void setStrokeProperties(ref Stroke st, int FilledSelected)
        {
            if (FilledSelected == Filling.Empty)
                st.ExtendedProperties.Add(Root.ISSTROKE_GUID, true);
            else if (FilledSelected == Filling.PenColorFilled)
                st.ExtendedProperties.Add(Root.ISFILLEDCOLOR_GUID, true);
            else if (FilledSelected == Filling.WhiteFilled)
                st.ExtendedProperties.Add(Root.ISFILLEDWHITE_GUID, true);
            else if (FilledSelected == Filling.BlackFilled)
                st.ExtendedProperties.Add(Root.ISFILLEDBLACK_GUID, true);
            try { st.ExtendedProperties.Add(Root.FADING_PEN, DateTime.Now.AddSeconds((float)(st.DrawingAttributes.ExtendedProperties[Root.FADING_PEN].Data)).Ticks); } catch { };

        }

        private Stroke AddEllipseStroke(int CursorX0, int CursorY0, int CursorX, int CursorY, int FilledSelected)
        {
            int NB_PTS = 36 * 3;
            Point[] pts = new Point[NB_PTS + 1];
            int dX = CursorX - CursorX0;
            int dY = CursorY - CursorY0;

            for (int i = 0; i < NB_PTS + 1; i++)
            {
                pts[i] = new Point(CursorX0 + (int)(dX * Math.Cos(Math.PI * (i + NB_PTS / 8) / (NB_PTS / 2))), CursorY0 + (int)(dY * Math.Sin(Math.PI * (i + NB_PTS / 8) / (NB_PTS / 2))));
            }
            IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pts);
            Stroke st = Root.FormCollection.IC.Ink.CreateStroke(pts);
            st.DrawingAttributes = Root.FormCollection.IC.DefaultDrawingAttributes.Clone();
            st.DrawingAttributes.AntiAliased = true;
            st.DrawingAttributes.FitToCurve = true;
            setStrokeProperties(ref st, FilledSelected);
            Root.FormCollection.IC.Ink.Strokes.Add(st);
            return st;
        }

        private Stroke AddRectStroke(int CursorX0, int CursorY0, int CursorX, int CursorY, int FilledSelected)
        {
            Point[] pts = new Point[9];
            int i = 0;
            pts[i++] = new Point(CursorX0, CursorY0);
            pts[i++] = new Point(CursorX0, (CursorY0 + CursorY) / 2);
            pts[i++] = new Point(CursorX0, CursorY);
            pts[i++] = new Point((CursorX0 + CursorX) / 2, CursorY);
            pts[i++] = new Point(CursorX, CursorY);
            pts[i++] = new Point(CursorX, (CursorY0 + CursorY) / 2);
            pts[i++] = new Point(CursorX, CursorY0);
            pts[i++] = new Point((CursorX0 + CursorX) / 2, CursorY0);
            pts[i++] = new Point(CursorX0, CursorY0);

            IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pts);
            Stroke st = Root.FormCollection.IC.Ink.CreateStroke(pts);
            st.DrawingAttributes = Root.FormCollection.IC.DefaultDrawingAttributes.Clone();
            if (FilledSelected == Filling.NoFrame)
                st.DrawingAttributes.Transparency = 255;
            st.DrawingAttributes.AntiAliased = true;
            st.DrawingAttributes.FitToCurve = false;
            setStrokeProperties(ref st, FilledSelected);
            Root.FormCollection.IC.Ink.Strokes.Add(st);
            return st;
        }

        private Stroke AddImageStroke(int CursorX0, int CursorY0, int CursorX, int CursorY, string fn, int Filling = -10)
        {
            if (Filling == -10)
                Filling = Root.ImageStamp.Filling;
            Stroke st = AddRectStroke(CursorX0, CursorY0, CursorX, CursorY, Filling);
            st.ExtendedProperties.Add(Root.IMAGE_GUID, fn);
            st.ExtendedProperties.Add(Root.IMAGE_X_GUID, CursorX0);
            st.ExtendedProperties.Add(Root.IMAGE_Y_GUID, CursorY0);
            st.ExtendedProperties.Add(Root.IMAGE_W_GUID, CursorX - CursorX0);
            st.ExtendedProperties.Add(Root.IMAGE_H_GUID, CursorY - CursorY0);
            return st;
        }

        private Stroke AddLineStroke(int CursorX0, int CursorY0, int CursorX, int CursorY)
        {
            Point[] pts = new Point[2];
            pts[0] = new Point(CursorX0, CursorY0);
            pts[1] = new Point(CursorX, CursorY);

            IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pts);
            Stroke st = Root.FormCollection.IC.Ink.CreateStroke(pts);
            st.DrawingAttributes = Root.FormCollection.IC.DefaultDrawingAttributes.Clone();
            st.DrawingAttributes.AntiAliased = true;
            st.DrawingAttributes.FitToCurve = false;
            setStrokeProperties(ref st, 0);
            Root.FormCollection.IC.Ink.Strokes.Add(st);
            return st;
        }

        private Stroke ExtendPolyLineStroke(Stroke st, int CursorX, int CursorY, int FilledSelected)
        {
            Point[] pts = st.GetPoints();
            Array.Resize<Point>(ref pts, pts.GetLength(0) + 1);
            Point[] pts2 = new Point[1];
            pts2[0] = new Point(CursorX, CursorY);

            IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pts2);
            pts[pts.Length - 1] = new Point(pts2[0].X, pts2[0].Y);

            Stroke st1 = Root.FormCollection.IC.Ink.CreateStroke(pts);
            st1.DrawingAttributes = st.DrawingAttributes.Clone();
            st1.DrawingAttributes.AntiAliased = true;
            st1.DrawingAttributes.FitToCurve = false;
            setStrokeProperties(ref st1, FilledSelected);
            Root.FormCollection.IC.Ink.DeleteStroke(st);
            Root.FormCollection.IC.Ink.Strokes.Add(st1);
            return st1;
        }

        private Stroke AddArrowStroke(int CursorX0, int CursorY0, int CursorX, int CursorY)
        // arrow at starting point
        {
            Point[] pts = new Point[5];
            double theta = Math.Atan2(CursorY - CursorY0, CursorX - CursorX0);

            pts[0] = new Point((int)(CursorX0 + Math.Cos(theta + Root.ArrowAngle) * Root.ArrowLen), (int)(CursorY0 + Math.Sin(theta + Root.ArrowAngle) * Root.ArrowLen));
            pts[1] = new Point(CursorX0, CursorY0);
            pts[2] = new Point((int)(CursorX0 + Math.Cos(theta - Root.ArrowAngle) * Root.ArrowLen), (int)(CursorY0 + Math.Sin(theta - Root.ArrowAngle) * Root.ArrowLen));
            pts[3] = new Point(CursorX0, CursorY0);
            pts[4] = new Point(CursorX, CursorY);

            IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pts);
            Stroke st = Root.FormCollection.IC.Ink.CreateStroke(pts);
            st.DrawingAttributes = Root.FormCollection.IC.DefaultDrawingAttributes.Clone();
            st.DrawingAttributes.AntiAliased = true;
            st.DrawingAttributes.FitToCurve = false;
            setStrokeProperties(ref st, 0);
            Root.FormCollection.IC.Ink.Strokes.Add(st);
            return st;
        }

        private Stroke AddNumberTagStroke(int CursorX0, int CursorY0, int CursorX, int CursorY, string txt)
        // arrow at starting point
        {
            // for the filling, filled color is not used but this state is used to note that we edit the tag number
            Stroke st = AddEllipseStroke(CursorX0, CursorY0, (int)(CursorX0 + TagSize * 1.2), (int)(CursorY0 + TagSize * 1.2), Root.FilledSelected == Filling.PenColorFilled ? 0 : Root.FilledSelected);
            st.ExtendedProperties.Add(Root.ISSTROKE_GUID, true);
            Point pt = new Point(CursorX0, CursorY0);
            IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pt);
            st.ExtendedProperties.Add(Root.ISTAG_GUID, true);
            st.ExtendedProperties.Add(Root.TEXT_GUID, txt);
            st.ExtendedProperties.Add(Root.TEXTX_GUID, pt.X);
            st.ExtendedProperties.Add(Root.TEXTY_GUID, pt.Y);
            //st.ExtendedProperties.Add(Root.TEXTFORMAT_GUID, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            st.ExtendedProperties.Add(Root.TEXTHALIGN_GUID, StringAlignment.Center);
            st.ExtendedProperties.Add(Root.TEXTVALIGN_GUID, StringAlignment.Center);
            st.ExtendedProperties.Add(Root.TEXTFONT_GUID, TagFont);
            st.ExtendedProperties.Add(Root.TEXTFONTSIZE_GUID, (float)TagSize);
            st.ExtendedProperties.Add(Root.TEXTFONTSTYLE_GUID, (TagItalic ? FontStyle.Italic : FontStyle.Regular) | (TagBold ? FontStyle.Bold : FontStyle.Regular));
            return st;
        }

        private Stroke AddTextStroke(int CursorX0, int CursorY0, int CursorX, int CursorY, string txt, StringAlignment Align)
        // arrow at starting point
        {
            Point pt = new Point(CursorX0, CursorY0);
            //IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pt);
            IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pt);
            Point[] pts = new Point[9] { pt, pt, pt, pt, pt, pt, pt, pt, pt };

            Stroke st = Root.FormCollection.IC.Ink.CreateStroke(pts);
            st.DrawingAttributes = Root.FormCollection.IC.DefaultDrawingAttributes.Clone();
            st.DrawingAttributes.Width = 100; // no width to hide the point;
            st.DrawingAttributes.FitToCurve = false;
            st.ExtendedProperties.Add(Root.TEXT_GUID, txt);
            st.ExtendedProperties.Add(Root.TEXTX_GUID, pt.X);
            st.ExtendedProperties.Add(Root.TEXTY_GUID, pt.Y);
            st.ExtendedProperties.Add(Root.TEXTHALIGN_GUID, Align);
            st.ExtendedProperties.Add(Root.TEXTVALIGN_GUID, StringAlignment.Near);
            st.ExtendedProperties.Add(Root.TEXTFONT_GUID, TextFont);
            st.ExtendedProperties.Add(Root.TEXTFONTSIZE_GUID, (float)TextSize);
            st.ExtendedProperties.Add(Root.TEXTFONTSTYLE_GUID, (TextItalic ? FontStyle.Italic : FontStyle.Regular) | (TextBold ? FontStyle.Bold : FontStyle.Regular));
            setStrokeProperties(ref st, Filling.NoFrame);
            Root.FormCollection.IC.Ink.Strokes.Add(st);
            return st;
        }

        bool TextEdited = false;    // used to prevent random toolbar closing when using esc in a dialog box
        private DialogResult ModifyTextInStroke(Stroke stk, string txt)
        {
            // required to access the dialog box
            AllowInteractions(true);
            //ToThrough();

            FormInput inp = new FormInput(Root.Local.DlgTextCaption, Root.Local.DlgTextLabel, txt, true, Root, stk);

            Point pt = stk.GetPoint(0);
            IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref pt);
            pt = PointToScreen(pt);
            inp.Top = pt.Y - inp.Height - 10;// +this.Top ;
            inp.Left = pt.X;// +this.Left;
            //Console.WriteLine("Edit {0},{1}", inp.Left, inp.Top);
            Screen scr = Screen.FromPoint(pt);
            if ((inp.Right >= scr.Bounds.Right) || (inp.Top <= scr.Bounds.Top))
            {   // if the dialog can not be displayed above the text we will display it in the middle of the primary screen
                inp.Top = ((int)(scr.Bounds.Top + scr.Bounds.Bottom - inp.Height) / 2);//System.Windows.SystemParameters.PrimaryScreenHeight)-inp.Height) / 2;
                inp.Left = ((int)(scr.Bounds.Left + scr.Bounds.Right - inp.Width) / 2);// System.Windows.SystemParameters.PrimaryScreenWidth) - inp.Width) / 2;
            }
            DialogResult ret = inp.ShowDialog();  // cancellation process is within the cancel button
            TextEdited = true;
            AllowInteractions(false);
            //ToUnThrough();

            return ret;
        }

        private float NearestStroke(Point pt, bool ptInPixel, out Stroke minStroke, out float pos, bool Search4Text = true, bool butLast = false)
        {
            if (ptInPixel)
                IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pt);

            float dst = 10000000000;
            float dst1 = dst;
            float pos1;
            pos = 0;
            //if (IC.Ink.Strokes.Count == 0)
            //    return dst;
            //minStroke = IC.Ink.Strokes[0];
            minStroke = null;
            //foreach (Stroke st in IC.Ink.Strokes)
            //reverse the order to select to most ontop :
            for (int i = IC.Ink.Strokes.Count - (butLast ? 2 : 1); i >= 0; i--)
            {
                Stroke st = IC.Ink.Strokes[i];
                if (st.ExtendedProperties.Contains(Root.ISDELETION_GUID))
                    continue;
                pos1 = st.NearestPoint(pt, out dst1);
                if ((dst1 < dst) && (!Search4Text || (st.ExtendedProperties.Contains(Root.TEXT_GUID))))
                {
                    dst = dst1;
                    minStroke = st;
                    pos = pos1;
                }
            };
            return dst;
        }

        private void MagneticEffect(int cursorX0, int cursorY0, ref int cursorX, ref int cursorY, bool Magnetic = false)
        {
            int dist(int x, int y)
            {
                if (x == int.MaxValue || y == int.MinValue)
                    return int.MaxValue;
                else
                    return x * x + y * y;
            };
            /*
                First : looking for a point on a stroke next to the pointer
            */
            Stroke st;
            float pos;
            Point pt = new Point(int.MaxValue, int.MaxValue);
            int x2 = int.MaxValue, y2 = int.MaxValue;//, x_a = int.MaxValue, y_a = int.MaxValue;
            if ((Control.ModifierKeys & Keys.Control) != Keys.None || (Control.ModifierKeys & Keys.Shift) != Keys.None)  // force temporarily Magnetic off if ctrl or shift is depressed
                Magnetic = false;
            if ((Magnetic || (Control.ModifierKeys & Keys.Control) != Keys.None) &&
                (NearestStroke(new Point(cursorX, cursorY), true, out st, out pos, false, true) < Root.PixelToHiMetric(Root.MinMagneticRadius())))
            {
                pt = st.GetPoint((int)Math.Round(pos));
                IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref pt);
                //cursorX = pt.X;
                //cursorY = pt.Y;
                //return;
            }

            /*
                Second : looking for remarquable points around text
            */
            if ((Magnetic || (ModifierKeys & Keys.Control) != Keys.None))
                foreach (Stroke stk in IC.Ink.Strokes)
                {
                    if (stk.ExtendedProperties.Contains(Root.TEXTWIDTH_GUID))
                    {
                        int x0 = Root.HiMetricToPixel((int)stk.ExtendedProperties[Root.TEXTX_GUID].Data);
                        int y0 = Root.HiMetricToPixel((int)stk.ExtendedProperties[Root.TEXTY_GUID].Data);
                        int x1, y1;
                        if ((System.Drawing.StringAlignment)stk.ExtendedProperties[Root.TEXTHALIGN_GUID].Data == StringAlignment.Near)
                        {
                            x1 = (int)(x0 + (float)stk.ExtendedProperties[Root.TEXTWIDTH_GUID].Data);
                        }
                        else
                        {
                            x1 = x0;
                            x0 = (int)(x1 - (float)stk.ExtendedProperties[Root.TEXTWIDTH_GUID].Data);
                        }
                        if ((System.Drawing.StringAlignment)stk.ExtendedProperties[Root.TEXTVALIGN_GUID].Data == StringAlignment.Near)
                        {
                            y1 = (int)(y0 + (float)stk.ExtendedProperties[Root.TEXTHEIGHT_GUID].Data);
                        }
                        else
                        {
                            y1 = y0;
                            y0 = (int)(y1 - (float)stk.ExtendedProperties[Root.TEXTHEIGHT_GUID].Data);
                        }
                        //Console.WriteLine("{0},{1}   {2},{3}    {4},{5}       <= {6},{7}", x0, y0, cursorX, cursorY, x1, y1, (float)stk.ExtendedProperties[Root.TEXTWIDTH_GUID].Data, (float)stk.ExtendedProperties[Root.TEXTHEIGHT_GUID].Data);
                        if ((x0 - Root.MinMagneticRadius()) <= cursorX && cursorX <= (x1 + Root.MinMagneticRadius())
                            && (y0 - Root.MinMagneticRadius()) <= cursorY && cursorY <= (y1 + Root.MinMagneticRadius()))
                        {
                            int d = dist(cursorX - x0, cursorY - y0);
                            x2 = x0;
                            y2 = y0;
                            int d1 = dist(cursorX - (x1 + x0) / 2, cursorY - y0);
                            if (d1 < d)
                            {
                                x2 = (x1 + x0) / 2;
                                y2 = y0;
                                d = d1;
                            };
                            d1 = dist(cursorX - x1, cursorY - y0);
                            if (d1 < d)
                            {
                                x2 = x1;
                                y2 = y0;
                                d = d1;
                            };
                            d1 = dist(cursorX - x1, cursorY - (y0 + y1) / 2);
                            if (d1 < d)
                            {
                                x2 = x1;
                                y2 = (y0 + y1) / 2;
                                d = d1;
                            };
                            d1 = dist(cursorX - x1, cursorY - y1);
                            if (d1 < d)
                            {
                                x2 = x1;
                                y2 = y1;
                                d = d1;
                            };
                            d1 = dist(cursorX - (x0 + x1) / 2, cursorY - y1);
                            if (d1 < d)
                            {
                                x2 = (x0 + x1) / 2;
                                y2 = y1;
                                d = d1;
                            };
                            d1 = dist(cursorX - x0, cursorY - y1);
                            if (d1 < d)
                            {
                                x2 = x0;
                                y2 = y1;
                                d = d1;
                            };
                            d1 = dist(cursorX - x0, cursorY - (y0 + y1) / 2);
                            if (d1 < d)
                            {
                                x2 = x0;
                                y2 = (y0 + y1) / 2;
                                d = d1;
                            };
                            // the assumption is that text are not overlaying, therefore we don't need to carry on searching...
                            break;
                            //cursorX = x2;
                            //cursorY = y2;
                            //return;
                        };
                    };
                };
            //Console.WriteLine("***** {0},{1} {2},{3}=>{4} {5},{6}=>{7}", cursorX,cursorY, pt.X, pt.Y, dist(pt.X - cursorX, pt.Y - cursorY),x2, y2, dist(x2 - cursorX, y2 - cursorY));
            if (dist(pt.X - cursorX, pt.Y - cursorY) < dist(x2 - cursorX, y2 - cursorY))
            {
                x2 = pt.X;
                y2 = pt.Y;
            }
            if (x2 != int.MaxValue && y2 != int.MaxValue)
            {
                cursorX = x2;
                cursorY = y2;
                return;
            }
            /*
                Next : on axis @+/-2 every 15�
            */
            double theta = Math.Atan2(cursorY - cursorY0, cursorX - cursorX0) * 180.0 / Math.PI;
            double theta2 = ((theta + 2.0 + 360.0) % 15.0) - 2.0;
            if ((Magnetic || (ModifierKeys & Keys.Shift) != Keys.None) &&
                (Math.Abs(theta2) < 3.0))
            {
                theta -= theta2;
                if ((Math.Abs(theta) < 45.0) || (Math.Abs(theta - 180.0) < 45.0) || (Math.Abs(theta + 180.0) < 45.0))
                    cursorY = (int)((cursorX - cursorX0) * Math.Tan(theta / 180.0 * Math.PI) + cursorY0);
                else
                    cursorX = (int)((cursorY - cursorY0) / Math.Tan(theta / 180.0 * Math.PI) + cursorX0);
                return;
            }
        }

        private void IC_Stroke(object sender, InkCollectorStrokeEventArgs e)
        {
            movedStroke = null; // reset the moving object
            Root.FingerInAction = false;        // this is done a little before MouseUp ; but it looks like the one from MouseUp is not always done...
            try { if (e.Stroke.ExtendedProperties.Contains(Root.ISSTROKE_GUID)) e.Stroke.ExtendedProperties.Remove(Root.ISSTROKE_GUID); } catch { } // the ISSTROKE set for drawin
            try { e.Stroke.ExtendedProperties.Add(Root.FADING_PEN, DateTime.Now.AddSeconds((float)(e.Stroke.DrawingAttributes.ExtendedProperties[Root.FADING_PEN].Data)).Ticks); } catch { };
            if (ZoomCapturing)
            {
                IC.Ink.DeleteStroke(e.Stroke); // the stroke that was just inserted has to be replaced.
                /* //trying to prevent capturing the rectangle but is not working
                Root.UponAllDrawingUpdate = true;
                Root.FormDisplay.timer1_Tick(null, null);
                Root.FormDisplay.Update();
                */
                if (Root.CursorX0 != int.MinValue)
                {
                    ZoomCapturing = false;
                    ZoomCaptured = true;
                }
                else
                    return;
                SaveStrokes(ZoomSaveStroke);
                Bitmap capt = new Bitmap(Math.Abs(Root.CursorX0 - Root.CursorX), Math.Abs(Root.CursorY0 - Root.CursorY));
                using (Graphics g = Graphics.FromImage(capt))
                {
                    Point p = PointToScreen(new Point(Math.Min(Root.CursorX0, Root.CursorX), Math.Min(Root.CursorY0, Root.CursorY)));
                    Size sz = new Size(capt.Width, capt.Height);
                    g.CopyFromScreen(p, Point.Empty, sz);
                    try { ClipartsDlg.Originals.Remove("_ZoomClip"); } catch { }
                    ClipartsDlg.Originals.Add("_ZoomClip", capt);
                    IC.Ink.Strokes.Clear();
                    Stroke st;
                    if (Root.WindowRect.Width > 0)
                    {
                        st = AddImageStroke(0, 0, Width, Height, "_ZoomClip", Filling.NoFrame);
                    }
                    else
                    {
                        Screen scr = Screen.FromPoint(MousePosition);
                        st = AddImageStroke(scr.Bounds.Left, scr.Bounds.Top, scr.Bounds.Right, scr.Bounds.Bottom, "_ZoomClip", Filling.NoFrame);
                    }
                    try { st.ExtendedProperties.Remove(Root.FADING_PEN); } catch { };  // if the pen was fading we need to remove that 
                    if (Root.CanvasCursor == 1)
                        SetPenTipCursor();
                }

                return;
            }
            else if (Root.ToolSelected == Tools.Hand)
            {
                Stroke st = e.Stroke;// IC.Ink.Strokes[IC.Ink.Strokes.Count-1];
                setStrokeProperties(ref st, Root.FilledSelected);
            }
            else
            {
                if (Root.CursorX0 == int.MinValue) // process when clicking touchscreen with just a short press;
                {
                    Point p = System.Windows.Forms.Cursor.Position;
                    p = Root.FormDisplay.PointToClient(p);
                    Root.CursorX = p.X;
                    Root.CursorY = p.Y;
                }
                IC.Ink.DeleteStroke(e.Stroke); // the stroke that was just inserted has to be replaced.

                if ((Root.ToolSelected == Tools.Line) && (Root.CursorX0 != int.MinValue))
                    AddLineStroke(Root.CursorX0, Root.CursorY0, Root.CursorX, Root.CursorY);
                else if ((Root.ToolSelected == Tools.Rect) && (Root.CursorX0 != int.MinValue))
                    if ((CurrentMouseButton == MouseButtons.Right) || ((int)CurrentMouseButton == 2))
                        AddRectStroke(2 * Root.CursorX0 - Root.CursorX, 2 * Root.CursorY0 - Root.CursorY, Root.CursorX, Root.CursorY, Root.FilledSelected);
                    else
                        AddRectStroke(Root.CursorX0, Root.CursorY0, Root.CursorX, Root.CursorY, Root.FilledSelected);
                else if (Root.ToolSelected == Tools.ClipArt)
                {
                    //int idx = ClipartsDlg.Images.Images.IndexOfKey(Root.ImageStamp.ImageStamp);
                    int w = Root.ImageStamp.X;
                    int h = Root.ImageStamp.Y;
                    if ((Root.CursorX0 == int.MinValue) || ((Root.CursorX0 == Root.CursorX) && (Root.CursorY0 == Root.CursorY)))
                    {
                        Root.CursorX0 = Root.CursorX;
                        Root.CursorY0 = Root.CursorY;
                        Root.CursorX = Root.CursorX0 + w;
                        Root.CursorY = Root.CursorY0 + h;
                    }
                    else if (Math.Abs((double)(Root.CursorX - Root.CursorX0) / (Root.CursorY - Root.CursorY0)) < Root.StampScaleRatio)
                    {
                        //Console.WriteLine("ratio 2 = " + ((double)(Root.CursorX - Root.CursorX0) / (Root.CursorY - Root.CursorY0)).ToString());
                        Root.CursorX = (int)(Root.CursorX0 + (double)(Root.CursorY - Root.CursorY0) / h * w);
                    }
                    else if (Math.Abs((double)(Root.CursorY - Root.CursorY0) / (Root.CursorX - Root.CursorX0)) < Root.StampScaleRatio)
                    {
                        //Console.WriteLine("ratio 1 = " + ((double)(Root.CursorY - Root.CursorY0) / (Root.CursorX - Root.CursorX0)).ToString());
                        Root.CursorY = (int)(Root.CursorY0 + (double)(Root.CursorX - Root.CursorX0) / w * h);
                    }
                    AddImageStroke(Root.CursorX0, Root.CursorY0, Root.CursorX, Root.CursorY, Root.ImageStamp.ImageStamp);
                }
                else if ((Root.ToolSelected == Tools.Oval) && (Root.CursorX0 != int.MinValue))
                    if ((CurrentMouseButton == MouseButtons.Right) || ((int)CurrentMouseButton == 2))
                        AddEllipseStroke(Root.CursorX0, Root.CursorY0, Root.CursorX, Root.CursorY, Root.FilledSelected);
                    else
                        AddEllipseStroke((Root.CursorX0 + Root.CursorX) / 2, (Root.CursorY0 + Root.CursorY) / 2, Root.CursorX, Root.CursorY, Root.FilledSelected);
                else if (((Root.ToolSelected == Tools.StartArrow) || (Root.ToolSelected == Tools.EndArrow)) && (Root.CursorX0 != int.MinValue))
                    if (((CurrentMouseButton == MouseButtons.Right) || ((int)CurrentMouseButton == 2)) ^ (Root.ToolSelected == Tools.StartArrow))
                        AddArrowStroke(Root.CursorX0, Root.CursorY0, Root.CursorX, Root.CursorY);
                    else
                        AddArrowStroke(Root.CursorX, Root.CursorY, Root.CursorX0, Root.CursorY0);
                else if (Root.ToolSelected == Tools.NumberTag)
                {
                    Stroke st = AddNumberTagStroke(Root.CursorX, Root.CursorY, Root.CursorX, Root.CursorY, Root.TagNumbering.ToString());
                    Root.TagNumbering++;
                }
                else if (Root.ToolSelected == Tools.Edit) // Edit
                {
                    float pos;
                    Stroke minStroke;
                    if (NearestStroke(new Point(Root.CursorX, Root.CursorY), true, out minStroke, out pos, false, false) < Root.PixelToHiMetric(Root.MinMagneticRadius()))
                    {
                        if (minStroke.ExtendedProperties.Contains(Root.TEXT_GUID))
                        {
                            ModifyTextInStroke(minStroke, (string)(minStroke.ExtendedProperties[Root.TEXT_GUID].Data));
                            SelectTool(0, 0);
                            ComputeTextBoxSize(ref minStroke);

                        }
                        else
                        {
                            DrawingAttributes da = minStroke.DrawingAttributes.Clone();
                            AllowInteractions(true);
                            if (PenModifyDlg.ModifyPen(ref da))
                            {
                                minStroke.DrawingAttributes = da;
                            }
                            AllowInteractions(false);
                        }
                    }
                }
                else if ((Root.ToolSelected == Tools.txtLeftAligned) || (Root.ToolSelected == Tools.txtRightAligned))  // new text
                {
                    Stroke st = AddTextStroke(Root.CursorX, Root.CursorY, Root.CursorX, Root.CursorY, "Text", (Root.ToolSelected == Tools.txtLeftAligned) ? StringAlignment.Near : StringAlignment.Far);
                    Root.FormDisplay.DrawStrokes();
                    Root.FormDisplay.UpdateFormDisplay(true);
                    if (ModifyTextInStroke(st, (string)(st.ExtendedProperties[Root.TEXT_GUID].Data)) == DialogResult.Cancel)
                        IC.Ink.DeleteStroke(st);
                    else
                    {
                        ComputeTextBoxSize(ref st);
                    }
                }
                else if ((Root.ToolSelected == Tools.Move) || (Root.ToolSelected == Tools.Copy))// Move : do Nothing
                    movedStroke = null;
                else if ((Root.ToolSelected == Tools.Poly) && ((Root.CursorX0 != int.MinValue) || (Math.Abs(Root.CursorY - PolyLineLastY) + Math.Abs(Root.CursorX - PolyLineLastX) < Root.MinMagneticRadius())))
                {
                    if (PolyLineLastX == int.MinValue)
                    {
                        PolyLineInProgress = AddLineStroke(Root.CursorX0, Root.CursorY0, Root.CursorX, Root.CursorY);
                        PolyLineLastX = Root.CursorX; PolyLineLastY = Root.CursorY;
                    }
                    else
                    {
                        if (Math.Abs(Root.CursorY - PolyLineLastY) + Math.Abs(Root.CursorX - PolyLineLastX) < Root.MinMagneticRadius())
                        {
                            PolyLineLastX = int.MinValue; PolyLineLastY = int.MinValue; PolyLineInProgress = null;
                        }
                        else
                        {
                            PolyLineLastX = Root.CursorX; PolyLineLastY = Root.CursorY;
                            PolyLineInProgress = ExtendPolyLineStroke(PolyLineInProgress, Root.CursorX, Root.CursorY, Root.FilledSelected);
                        }
                    }
                }
            }
            SaveUndoStrokes();
            Root.UponAllDrawingUpdate = true;
            /*Root.FormDisplay.ClearCanvus();
            Root.FormDisplay.DrawStrokes();
            Root.FormDisplay.DrawButtons(true);
            Root.FormDisplay.UpdateFormDisplay(true);*/

            // reset the CursorX0/Y0 : this seems to introduce a wrong interim drawing
            CurrentMouseButton = MouseButtons.None;
            Root.CursorX0 = int.MinValue;
            Root.CursorY0 = int.MinValue;
        }

        public void ComputeTextBoxSize(ref Stroke st)
        {
            System.Drawing.StringFormat stf = new System.Drawing.StringFormat(System.Drawing.StringFormatFlags.NoClip);
            stf.Alignment = (System.Drawing.StringAlignment)(st.ExtendedProperties[Root.TEXTHALIGN_GUID].Data);
            stf.LineAlignment = (System.Drawing.StringAlignment)(st.ExtendedProperties[Root.TEXTVALIGN_GUID].Data);
            SizeF layoutSize = new SizeF(2000.0F, 2000.0F);
            layoutSize = Root.FormDisplay.gOneStrokeCanvus.MeasureString((string)(st.ExtendedProperties[Root.TEXT_GUID].Data),
                            new Font((string)st.ExtendedProperties[Root.TEXTFONT_GUID].Data, (float)st.ExtendedProperties[Root.TEXTFONTSIZE_GUID].Data,
                            (System.Drawing.FontStyle)(int)st.ExtendedProperties[Root.TEXTFONTSTYLE_GUID].Data), layoutSize, stf);
            st.ExtendedProperties.Add(Root.TEXTWIDTH_GUID, layoutSize.Width);
            st.ExtendedProperties.Add(Root.TEXTHEIGHT_GUID, layoutSize.Height);
            if (!st.ExtendedProperties.Contains(Root.ISTAG_GUID))
            {
                Point pt = new Point((int)(st.ExtendedProperties[Root.TEXTX_GUID].Data), (int)(st.ExtendedProperties[Root.TEXTY_GUID].Data));
                //IC.Renderer.PixelToInkSpace(IC.Handle, ref pt);
                Point pt2 = new Point((int)layoutSize.Width, (int)layoutSize.Height);
                IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pt2);
                if (stf.Alignment == StringAlignment.Near) //align Left
                    st.SetPoints(new Point[] { pt, new Point((int)(pt.X+pt2.X / 2),pt.Y+0), new Point((int)(pt.X+pt2.X),pt.Y+0),
                                               new Point((int)(pt.X+pt2.X),(int)(pt.Y+pt2.Y/2)),new Point((int)(pt.X+pt2.X),(int)(pt.Y+pt2.Y)),
                                               new Point((int)(pt.X+pt2.X/2),(int)(pt.Y+pt2.Y)),new Point((int)(pt.X+0),(int)(pt.Y+pt2.Y)),
                                               new Point((int)(pt.X+0),(int)(pt.Y+pt2.Y/2)),pt });
                else //align right
                    st.SetPoints(new Point[] { pt, new Point((int)(pt.X-pt2.X / 2),pt.Y+0), new Point((int)(pt.X-pt2.X),pt.Y+0),
                                               new Point((int)(pt.X-pt2.X),(int)(pt.Y+pt2.Y/2)),new Point((int)(pt.X-pt2.X),(int)(pt.Y+pt2.Y)),
                                               new Point((int)(pt.X-pt2.X/2),(int)(pt.Y+pt2.Y)),new Point((int)(pt.X-0),(int)(pt.Y+pt2.Y)),
                                               new Point((int)(pt.X-0),(int)(pt.Y+pt2.Y/2)),pt });
            }
        }

        private void SaveUndoStrokes()
        {
            Root.RedoDepth = 0;
            if (Root.UndoDepth < Root.UndoStrokes.GetLength(0) - 1)
                Root.UndoDepth++;

            Root.UndoP++;
            if (Root.UndoP >= Root.UndoStrokes.GetLength(0))
                Root.UndoP = 0;

            if (Root.UndoStrokes[Root.UndoP] == null)
                Root.UndoStrokes[Root.UndoP] = new Ink();
            Root.UndoStrokes[Root.UndoP].DeleteStrokes();
            if (IC.Ink.Strokes.Count > 0)
            {
                Rectangle r = IC.Ink.Strokes.GetBoundingBox();
                if (r.Width > 0)
                    Root.UndoStrokes[Root.UndoP].AddStrokesAtRectangle(IC.Ink.Strokes, r);
            }
        }
        Stroke movedStroke = null;

        float ZoomScreenRatio;
        private void IC_CursorDown(object sender, InkCollectorCursorDownEventArgs e)
        {
            if (ZoomCapturing)
            {
                Screen scr = Screen.FromPoint(MousePosition);
                ZoomScreenRatio = (float)(scr.Bounds.Width) / scr.Bounds.Height;
                e.Stroke.ExtendedProperties.Add(Root.ISHIDDEN_GUID, true); // we set the ISTROKE_GUID in order to draw the inprogress as a line
                e.Stroke.DrawingAttributes.Color = Color.Purple;
                e.Stroke.DrawingAttributes.Transparency = 0;
                e.Stroke.DrawingAttributes.Width = Root.PixelToHiMetric(1);
            }
            else if (Root.ToolSelected == Tools.Hand)
                e.Stroke.ExtendedProperties.Add(Root.ISSTROKE_GUID, true); // we set the ISTROKE_GUID in order to draw the inprogress as a line
            else
                e.Stroke.ExtendedProperties.Add(Root.ISHIDDEN_GUID, true); // we set the ISTROKE_GUID in order to draw the inprogress as a line

            if (!Root.InkVisible && Root.Snapping <= 0)
            {
                Root.SetInkVisible(true);
            }

            Root.FormDisplay.ClearCanvus(Root.FormDisplay.gOneStrokeCanvus);
            Root.FormDisplay.DrawStrokes(Root.FormDisplay.gOneStrokeCanvus);
            Root.FormDisplay.DrawButtons(Root.FormDisplay.gOneStrokeCanvus, false);
            Point p;
            try
            {
                if (e.Stroke.BezierPoints.Length > 0)
                {
                    p = e.Stroke.BezierPoints[0];
                    IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref p);
                }
                else
                    throw new System.ApplicationException("Empty Stroke");
            }
            catch
            {
                p = System.Windows.Forms.Cursor.Position;
                p = Root.FormDisplay.PointToClient(p);
            }
            Root.CursorX = p.X;
            Root.CursorY = p.Y;
            if (Root.EraserMode) // we are deleting the nearest object for clicking...
            {
                e.Stroke.ExtendedProperties.Add(Root.ISDELETION_GUID, true);
                float pos;
                Stroke minStroke;
                if (NearestStroke(new Point(Root.CursorX, Root.CursorY), true, out minStroke, out pos, false, false) < Root.PixelToHiMetric(Root.MinMagneticRadius()))
                {
                    IC.Ink.DeleteStroke(minStroke);
                }
            }
        }

        private void IC_MouseDown(object sender, CancelMouseEventArgs e)
        {
            CurrentMouseButton = e.Button;
            if (Root.gpPenWidthVisible)
            {
                Root.gpPenWidthVisible = false;
                Root.UponSubPanelUpdate = true;
            }

            Root.FingerInAction = true;
            if (Root.Snapping == 1)
            {
                Root.SnappingX = e.X;
                Root.SnappingY = e.Y;
                Root.SnappingRect = new Rectangle(e.X, e.Y, 0, 0);
                Root.Snapping = 2;
            }

            if (!Root.InkVisible && Root.Snapping <= 0)
            {
                Root.SetInkVisible(true);
            }

            LasteXY.X = e.X;
            LasteXY.Y = e.Y;
            IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref LasteXY);
            if ((Root.ToolSelected == Tools.Poly) && (PolyLineLastX != int.MinValue))
            {
                Root.CursorX0 = PolyLineLastX;
                Root.CursorY0 = PolyLineLastY;
            }
            else
            {
                Root.CursorX0 = e.X;
                Root.CursorY0 = e.Y;
            }
            MagneticEffect(Root.CursorX0 - 1, Root.CursorY0, ref Root.CursorX0, ref Root.CursorY0, Root.MagneticRadius > 0); // analysis of magnetic will be done within the module
            if (Root.InkVisible)
            {
                Root.CursorX = Root.CursorX0;
                Root.CursorY = Root.CursorY0;
            }

            if ((Root.ToolSelected == Tools.Move) || (Root.ToolSelected == Tools.Copy)) // Move
            {
                float pos;
                if (NearestStroke(new Point(Root.CursorX, Root.CursorY), true, out movedStroke, out pos, false, true) > Root.PixelToHiMetric(Root.MinMagneticRadius()))
                    movedStroke = null;
                else if (Root.ToolSelected == Tools.Copy)
                {
                    Stroke copied = movedStroke;
                    movedStroke = Root.FormCollection.IC.Ink.CreateStroke(copied.GetPoints());
                    movedStroke.DrawingAttributes = copied.DrawingAttributes.Clone();
                    foreach (ExtendedProperty prop in copied.ExtendedProperties)
                    {
                        movedStroke.ExtendedProperties.Add(prop.Id, prop.Data);
                    }
                    Root.FormCollection.IC.Ink.Strokes.Add(movedStroke);
                }
            }
        }


        public Point LasteXY;
        private void IC_MouseMove(object sender, CancelMouseEventArgs e)
        {
            float pos;
            Root.StrokeHovered = null;

            if (e.Button == MouseButtons.None)
                if (Root.EraserMode || Root.ToolSelected == Tools.Edit || Root.ToolSelected == Tools.Move || Root.ToolSelected == Tools.Copy)
                {
                    if (NearestStroke(new Point(e.X, e.Y), true, out Root.StrokeHovered, out pos, false) > Root.PixelToHiMetric(Root.MinMagneticRadius()))
                    {
                        Root.StrokeHovered = null;
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                    return;
            //Console.WriteLine("Cursor {0},{1} - {2}", e.X, e.Y, e.Button);
            Root.CursorX = e.X;
            Root.CursorY = e.Y;
            if (ZoomCapturing)
            {
                if (Root.WindowRect.Width > 0)
                    Root.CursorY = (int)(Root.CursorY0 + (Root.CursorX - Root.CursorX0) / (1.0 * Width / Height) * Math.Sign(Root.CursorY - Root.CursorY0) * Math.Sign(Root.CursorX - Root.CursorX0));
                else
                    Root.CursorY = (int)(Root.CursorY0 + (Root.CursorX - Root.CursorX0) / ZoomScreenRatio * Math.Sign(Root.CursorY - Root.CursorY0) * Math.Sign(Root.CursorX - Root.CursorX0));
            }
            else
                MagneticEffect(Root.CursorX0, Root.CursorY0, ref Root.CursorX, ref Root.CursorY, Root.ToolSelected > Tools.Hand && Root.MagneticRadius > 0);

            if (LasteXY.X == 0 && LasteXY.Y == 0)
            {
                LasteXY.X = e.X;
                LasteXY.Y = e.Y;
                IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref LasteXY);
            }
            Point currentxy = new Point(e.X, e.Y);
            IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref currentxy);

            if (Root.Snapping == 2)
            {
                int left = Math.Min(Root.SnappingX, e.X);
                int top = Math.Min(Root.SnappingY, e.Y);
                int width = Math.Abs(Root.SnappingX - e.X);
                int height = Math.Abs(Root.SnappingY - e.Y);
                Root.SnappingRect = new Rectangle(left, top, width, height);

                if (LasteXY != currentxy)
                    Root.MouseMovedUnderSnapshotDragging = true;
            }
            else if (Root.PanMode && Root.FingerInAction)
            {
                Root.Pan(currentxy.X - LasteXY.X, currentxy.Y - LasteXY.Y);
            }
            else if ((Root.ToolSelected == Tools.Move) || (Root.ToolSelected == Tools.Copy))
            {
                if (movedStroke != null)
                {
                    //TODO: ajouter aimantation
                    /*Console.WriteLine(Root.CursorX0.ToString() + " ~ " + Root.CursorY0.ToString());
                    Point xy = new Point(Root.CursorX,Root.CursorY);
                    IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref xy);
                    */
                    movedStroke.Move(currentxy.X - LasteXY.X, currentxy.Y - LasteXY.Y);

                    if (movedStroke.ExtendedProperties.Contains(Root.TEXT_GUID))
                    {
                        movedStroke.ExtendedProperties.Add(Root.TEXTX_GUID, ((int)movedStroke.ExtendedProperties[Root.TEXTX_GUID].Data) + (currentxy.X - LasteXY.X));
                        movedStroke.ExtendedProperties.Add(Root.TEXTY_GUID, ((int)movedStroke.ExtendedProperties[Root.TEXTY_GUID].Data) + (currentxy.Y - LasteXY.Y));
                    }
                    if (movedStroke.ExtendedProperties.Contains(Root.IMAGE_X_GUID))
                    {
                        Point pt = new Point(movedStroke.GetPoint(0).X, movedStroke.GetPoint(0).Y);
                        IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref pt);
                        movedStroke.ExtendedProperties.Add(Root.IMAGE_X_GUID, pt.X);
                        movedStroke.ExtendedProperties.Add(Root.IMAGE_Y_GUID, pt.Y);
                    }
                    Root.FormDisplay.ClearCanvus();
                    Root.FormDisplay.DrawStrokes();
                    Root.FormDisplay.UpdateFormDisplay(true);
                }
            }

            LasteXY = currentxy;
        }

        private void IC_MouseUp(object sender, CancelMouseEventArgs e)
        {
            Root.FingerInAction = false;
            if (Root.Snapping == 2)
            {
                int left = Math.Min(Root.SnappingX, e.X);
                int top = Math.Min(Root.SnappingY, e.Y);
                int width = Math.Abs(Root.SnappingX - e.X);
                int height = Math.Abs(Root.SnappingY - e.Y);
                if (width < 5 || height < 5)
                {
                    if (Root.ResizeDrawingWindow)
                    {
                        left = SystemInformation.VirtualScreen.Left - this.Left;
                        top = SystemInformation.VirtualScreen.Top - this.Top;
                        width = SystemInformation.VirtualScreen.Width;
                        height = SystemInformation.VirtualScreen.Height;
                    }
                    else
                    {
                        left = 0;
                        top = 0;
                        width = this.Width;
                        height = this.Height;
                    }
                }
                Root.SnappingRect = new Rectangle(left + this.Left, top + this.Top, width, height);
                Root.UponTakingSnap = true;
                ExitSnapping();
                CurrentMouseButton = MouseButtons.None;
            }
            else if (Root.PanMode)
            {
                SaveUndoStrokes();
            }
            else
            {
                Root.UponAllDrawingUpdate = true;
            }
            Root.CursorX0 = int.MinValue;
            Root.CursorY0 = int.MinValue;
        }

        private void IC_CursorInRange(object sender, InkCollectorCursorInRangeEventArgs e)
        {
            if (e.Cursor.Inverted && Root.CurrentPen != -1)
            {
                EnterEraserMode(true);
                /*
				// temperary eraser icon light
				if (btEraser.Image == image_eraser)
				{
					btEraser.Image = image_eraser_act;
					Root.FormDisplay.DrawButtons(true);
					Root.FormDisplay.UpdateFormDisplay();
				}
				*/
            }
            else if (!e.Cursor.Inverted && Root.CurrentPen != -1)
            {
                EnterEraserMode(false);
                /*
				if (btEraser.Image == image_eraser_act)
				{
					btEraser.Image = image_eraser;
					Root.FormDisplay.DrawButtons(true);
					Root.FormDisplay.UpdateFormDisplay();
				}
				*/
            }
        }

        public void ToTransparent()
        {
            uint dwExStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, dwExStyle | 0x00080000);
            SetLayeredWindowAttributes(this.Handle, 0x00FFFFFF, 1, 0x2);
        }

        public void ToTopMost()
        {
            TopMost = true;
            SetWindowPos(this.Handle, (IntPtr)(-1), 0, 0, 0, 0, 0x0002 | 0x0001 | 0x0020);
        }

        public void ToThrough()
        {
            uint dwExStyle = GetWindowLong(this.Handle, -20);
            //SetWindowLong(this.Handle, -20, dwExStyle | 0x00080000);
            //SetWindowPos(this.Handle, (IntPtr)0, 0, 0, 0, 0, 0x0002 | 0x0001 | 0x0004 | 0x0010 | 0x0020);
            //SetLayeredWindowAttributes(this.Handle, 0x00FFFFFF, 1, 0x2);
            SetWindowLong(this.Handle, -20, dwExStyle | 0x00080000 | 0x00000020);
            //SetWindowPos(this.Handle, (IntPtr)(1), 0, 0, 0, 0, 0x0002 | 0x0001 | 0x0010 | 0x0020);
        }

        public void ToUnThrough()
        {
            uint dwExStyle = GetWindowLong(this.Handle, -20);
            //SetWindowLong(this.Handle, -20, (uint)(dwExStyle & ~0x00080000 & ~0x0020));
            SetWindowLong(this.Handle, -20, (uint)(dwExStyle & ~0x0020));
            //SetWindowPos(this.Handle, (IntPtr)(-2), 0, 0, 0, 0, 0x0002 | 0x0001 | 0x0010 | 0x0020);

            //dwExStyle = GetWindowLong(this.Handle, -20);
            //SetWindowLong(this.Handle, -20, dwExStyle | 0x00080000);
            //SetLayeredWindowAttributes(this.Handle, 0x00FFFFFF, 1, 0x2);
            //SetWindowPos(this.Handle, (IntPtr)(-1), 0, 0, 0, 0, 0x0002 | 0x0001 | 0x0020);
        }

        public void EnterEraserMode(bool enter)
        {
            int exceptiontick = 0;
            bool exc;
            do
            {
                exceptiontick++;
                exc = false;
                try
                {
                    if (enter)
                    {
                        IC.EditingMode = InkOverlayEditingMode.Delete;
                        Root.EraserMode = true;
                    }
                    else
                    {
                        IC.EditingMode = InkOverlayEditingMode.Ink;
                        Root.EraserMode = false;
                    }
                }
                catch
                {
                    Thread.Sleep(50);
                    exc = true;
                }
            }
            while (exc && exceptiontick < 3);
        }

        private readonly int[] applicableTool = { Tools.Hand, Tools.Line, Tools.Poly, Tools.Rect, Tools.Oval, Tools.NumberTag };
        public void SelectTool(int tool, int filled = -1)
        // Hand (0),Line(1),Rect(2),Oval(3),StartArrow(4),EndArrow(5),NumberTag(6),Edit(7),txtLeftAligned(8),txtRightAligned(9),Move(10),Copy(11),polyline/polygone(21)
        // filled : empty(0),PenColorFilled(1),WhiteFilled(2),BlackFilled(3)
        // filled is applicable to Hand,Rect,Oval
        {
            btHand.BackgroundImage = getImgFromDiskOrRes("tool_hand", ImageExts);
            btLine.BackgroundImage = getImgFromDiskOrRes("tool_line", ImageExts);
            btRect.BackgroundImage = getImgFromDiskOrRes("tool_rect", ImageExts);
            btOval.BackgroundImage = getImgFromDiskOrRes("tool_oval", ImageExts);
            if (Root.DefaultArrow_start)
                btArrow.BackgroundImage = getImgFromDiskOrRes("tool_stAr", ImageExts);
            else
                btArrow.BackgroundImage = getImgFromDiskOrRes("tool_enAr", ImageExts);
            btNumb.BackgroundImage = getImgFromDiskOrRes("tool_numb", ImageExts);
            btText.BackgroundImage = getImgFromDiskOrRes("tool_txtL", ImageExts);
            btEdit.BackgroundImage = getImgFromDiskOrRes("tool_edit", ImageExts);
            btClipArt.BackgroundImage = getImgFromDiskOrRes("tool_clipart", ImageExts);

            btClip1.FlatAppearance.BorderSize = btClipSel == btClip1.Tag ? 3 : 0;
            btClip2.FlatAppearance.BorderSize = btClipSel == btClip2.Tag ? 3 : 0;
            btClip3.FlatAppearance.BorderSize = btClipSel == btClip3.Tag ? 3 : 0;
            btClipSel = null;

            if (AltKeyPressed())
            {
                //if (SavedTool <= Tools.Invalid || tool != Root.ToolSelected )
                if (SavedTool <= Tools.Invalid)
                {
                    SavedTool = Root.ToolSelected;
                    SavedFilled = Root.FilledSelected;
                    if ((tool == Tools.Move || tool == Tools.Copy) && SavedPen < 0)
                        SavedPen = LastPenSelected;
                }
            }

            if (filled >= 0)
                Root.FilledSelected = filled;
            else if ((Array.IndexOf(applicableTool, tool) >= 0) && (tool == Root.ToolSelected))
                Root.FilledSelected = (Root.FilledSelected + 1) % 4;
            else
                Root.FilledSelected = 0;

            Root.UponButtonsUpdate |= 0x2;

            if (tool == Tools.Invalid)
            {
                Root.ToolSelected = Tools.Hand; // to prevent drawing
                return;
            }
            else if (tool == Tools.Hand)
            {
                if (Root.FilledSelected == Filling.Empty)
                    btHand.BackgroundImage = getImgFromDiskOrRes("tool_hand_act", ImageExts);
                else if (Root.FilledSelected == Filling.PenColorFilled)
                    btHand.BackgroundImage = getImgFromDiskOrRes("tool_hand_filledC", ImageExts);
                else if (Root.FilledSelected == Filling.WhiteFilled)
                    btHand.BackgroundImage = getImgFromDiskOrRes("tool_hand_filledW", ImageExts);
                else if (Root.FilledSelected == Filling.BlackFilled)
                    btHand.BackgroundImage = getImgFromDiskOrRes("tool_hand_filledB", ImageExts);
                EnterEraserMode(false);
            }
            else if ((tool == Tools.Line) || (tool == Tools.Poly))
            {
                if (Root.ToolSelected == Tools.Line)
                {
                    tool = Tools.Poly;
                    Root.FilledSelected = 0;
                    btLine.BackgroundImage = getImgFromDiskOrRes("tool_mlines", ImageExts);
                    filled = 0;
                    PolyLineLastX = int.MinValue; PolyLineLastY = int.MinValue;
                }
                else if ((Root.ToolSelected == Tools.Poly && (Root.FilledSelected == Filling.Empty || Root.FilledSelected > Filling.BlackFilled)) || (Root.ToolSelected != Tools.Poly))
                {
                    tool = Tools.Line;
                    Root.FilledSelected = 0;
                    btLine.BackgroundImage = getImgFromDiskOrRes("tool_line_act", ImageExts);
                }
                else // Root.ToolSelected == Tools.Poly && Root.FilledSelected != 4
                {
                    tool = Tools.Poly;
                    PolyLineLastX = int.MinValue; PolyLineLastY = int.MinValue;
                    if (Root.FilledSelected == Filling.PenColorFilled)
                        btLine.BackgroundImage = getImgFromDiskOrRes("tool_mlines_filledC", ImageExts);
                    else if (Root.FilledSelected == Filling.WhiteFilled)
                        btLine.BackgroundImage = getImgFromDiskOrRes("tool_mlines_filledW", ImageExts);
                    else if (Root.FilledSelected == Filling.BlackFilled)
                        btLine.BackgroundImage = getImgFromDiskOrRes("tool_mlines_filledB", ImageExts);
                }

            }
            else if (tool == Tools.Rect)
            {
                if (Root.FilledSelected == Filling.Empty)
                    btRect.BackgroundImage = getImgFromDiskOrRes("tool_rect_act", ImageExts);
                else if (Root.FilledSelected == Filling.PenColorFilled)
                    btRect.BackgroundImage = getImgFromDiskOrRes("tool_rect_filledC", ImageExts);
                else if (Root.FilledSelected == Filling.WhiteFilled)
                    btRect.BackgroundImage = getImgFromDiskOrRes("tool_rect_filledW", ImageExts);
                else if (Root.FilledSelected == Filling.BlackFilled)
                    btRect.BackgroundImage = getImgFromDiskOrRes("tool_rect_filledB", ImageExts);

            }
            else if (tool == Tools.ClipArt)
            {
                btClipArt.BackgroundImage = getImgFromDiskOrRes("tool_clipart_act", ImageExts);
            }
            else if (tool == Tools.Oval)
            {
                if (Root.FilledSelected == Filling.Empty)
                    btOval.BackgroundImage = getImgFromDiskOrRes("tool_oval_act", ImageExts);
                else if (Root.FilledSelected == Filling.PenColorFilled)
                    btOval.BackgroundImage = getImgFromDiskOrRes("tool_oval_filledC", ImageExts);
                else if (Root.FilledSelected == Filling.WhiteFilled)
                    btOval.BackgroundImage = getImgFromDiskOrRes("tool_oval_filledW", ImageExts);
                else if (Root.FilledSelected == Filling.BlackFilled)
                    btOval.BackgroundImage = getImgFromDiskOrRes("tool_oval_filledB", ImageExts);
            }
            else if ((tool == Tools.StartArrow) || (tool == Tools.EndArrow)) // also include tool=5
                if ((tool == Tools.EndArrow) || (Root.ToolSelected == Tools.StartArrow))
                {
                    btArrow.BackgroundImage = getImgFromDiskOrRes("tool_enAr_act", ImageExts);
                    tool = Tools.EndArrow;
                }
                else
                {
                    btArrow.BackgroundImage = getImgFromDiskOrRes("tool_stAr_act", ImageExts);
                    tool = Tools.StartArrow;
                }
            else if (tool == Tools.NumberTag)
            {
                if (Root.FilledSelected == Filling.Empty)
                    btNumb.BackgroundImage = getImgFromDiskOrRes("tool_numb_act", ImageExts);
                else if (Root.FilledSelected == Filling.PenColorFilled)
                { // we use the state FilledColor to do the modification of the tag number
                    //Console.WriteLine("avt setTag");
                    SetTagNumber();
                    //Console.WriteLine("ap setTag");
                    btNumb.BackgroundImage = getImgFromDiskOrRes("tool_numb_act", ImageExts);
                }
                else if (Root.FilledSelected == Filling.WhiteFilled)
                    btNumb.BackgroundImage = getImgFromDiskOrRes("tool_numb_fillW", ImageExts);
                else if (Root.FilledSelected == Filling.BlackFilled)
                    btNumb.BackgroundImage = getImgFromDiskOrRes("tool_numb_fillB", ImageExts);
                try
                {
                    IC.Cursor = cursorred;
                }
                catch
                {
                    IC.Cursor = getCursFromDiskOrRes("cursorarrow", System.Windows.Forms.Cursors.NoMove2D);
                }
            }
            else if (tool == Tools.Edit)
            {
                btEdit.BackgroundImage = getImgFromDiskOrRes("tool_edit_act");
                try
                {
                    IC.Cursor = cursorred;
                }
                catch
                {
                    IC.Cursor = getCursFromDiskOrRes("cursorarrow", System.Windows.Forms.Cursors.NoMove2D);
                }
            }
            else if ((tool == Tools.txtLeftAligned) || (tool == Tools.txtRightAligned))
            {
                if ((tool == Tools.txtRightAligned) || (Root.ToolSelected == Tools.txtLeftAligned))
                {
                    btText.BackgroundImage = getImgFromDiskOrRes("tool_txtR_act", ImageExts);
                    tool = Tools.txtRightAligned;
                }
                else
                {
                    btText.BackgroundImage = getImgFromDiskOrRes("tool_txtL_act", ImageExts);
                    tool = Tools.txtLeftAligned;
                }
                try
                {
                    IC.Cursor = cursorred;
                }
                catch
                {
                    IC.Cursor = getCursFromDiskOrRes("cursorarrow", System.Windows.Forms.Cursors.NoMove2D);
                }
            }
            else if (tool == Tools.Move)
            {
                //SelectPen(LastPenSelected);
                btPan.BackgroundImage = getImgFromDiskOrRes("pan1_act", ImageExts);
                try
                {
                    IC.Cursor = cursorred;
                }
                catch
                {
                    IC.Cursor = getCursFromDiskOrRes("cursorarrow", System.Windows.Forms.Cursors.NoMove2D);
                }
            }
            else if (tool == Tools.Copy)
            {
                //SelectPen(LastPenSelected);
                btPan.BackgroundImage = getImgFromDiskOrRes("pan_copy", ImageExts);
                try
                {
                    IC.Cursor = cursorred;
                }
                catch
                {
                    IC.Cursor = getCursFromDiskOrRes("cursorarrow", System.Windows.Forms.Cursors.NoMove2D);
                }
            }
            Root.ToolSelected = tool;
        }

        public void SelectPen(int pen)
        // -3 = pan, -2 = pointer, -1 = erasor, >=0 = pens
        {
            btPan.BackgroundImage = getImgFromDiskOrRes("pan", ImageExts);
            //Console.WriteLine("SelectPen : " + pen.ToString());
            //System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
            //Console.WriteLine(t.ToString());
            if (pen == -3)
            {
                if (AltKeyPressed() && SavedPen < 0)
                {
                    SavedPen = LastPenSelected;
                }
                SelectTool(-1, 0);       // Alt will be processed inhere
                for (int b = 0; b < Root.MaxPenCount; b++)
                    //btPen[b].Image = image_pen[b];
                    btPen[b].BackgroundImage = buildPenIcon(Root.PenAttr[b].Color, Root.PenAttr[b].Transparency, false);// image_pen[b];
                btEraser.BackgroundImage = image_eraser;
                btPointer.BackgroundImage = image_pointer;
                btPan.BackgroundImage = getImgFromDiskOrRes("pan_act", ImageExts);
                EnterEraserMode(false);
                Root.UnPointer();
                Root.PanMode = true;

                try
                {
                    IC.SetWindowInputRectangle(new Rectangle(0, 0, 1, 1));
                }
                catch
                {
                    Thread.Sleep(1);
                    IC.SetWindowInputRectangle(new Rectangle(0, 0, 1, 1));
                }
            }
            else if (pen == -2)
            {
                if (AltKeyPressed() && SavedPen < 0)
                {
                    SavedPen = LastPenSelected;
                }
                SelectTool(-1, 0);       // Alt will be processed inhere
                for (int b = 0; b < Root.MaxPenCount; b++)
                    //btPen[b].Image = image_pen[b];
                    btPen[b].BackgroundImage = buildPenIcon(Root.PenAttr[b].Color, Root.PenAttr[b].Transparency, false);// image_pen[b];
                btEraser.BackgroundImage = image_eraser;
                btPointer.BackgroundImage = image_pointer_act;
                EnterEraserMode(false);
                Root.Pointer();
                Root.PanMode = false;
            }
            else if (pen == -1)
            {
                if (AltKeyPressed() && SavedPen < 0)
                {
                    SavedPen = LastPenSelected;
                }
                SelectTool(-1, 0);       // Alt will be processed inhere
                                         //if (this.Cursor != System.Windows.Forms.Cursors.Default)
                                         //	this.Cursor = System.Windows.Forms.Cursors.Default;

                for (int b = 0; b < Root.MaxPenCount; b++)
                    //btPen[b].Image = image_pen[b];
                    btPen[b].BackgroundImage = buildPenIcon(Root.PenAttr[b].Color, Root.PenAttr[b].Transparency, false);// image_pen[b];

                btEraser.BackgroundImage = image_eraser_act;
                btPointer.BackgroundImage = image_pointer;
                EnterEraserMode(true);
                Root.UnPointer();
                Root.PanMode = false;
                // !!!!!!!!!!!!!!! random exception
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        IC.Cursor = new System.Windows.Forms.Cursor(cursorerase.Handle);
                    }
                    catch
                    {
                        cursorerase = getCursFromDiskOrRes("cursoreraser", System.Windows.Forms.Cursors.No);
                        //Console.WriteLine(e.Message);
                        continue;
                    }
                    break;
                }

                try
                {
                    IC.SetWindowInputRectangle(new Rectangle(0, 0, this.Width, this.Height));
                }
                catch
                {
                    Thread.Sleep(1);
                    IC.SetWindowInputRectangle(new Rectangle(0, 0, this.Width, this.Height));
                }
            }
            else if (pen >= 0)
            {
                if (AltKeyPressed() && pen != LastPenSelected && SavedPen < 0)
                {
                    SavedPen = LastPenSelected;
                }
                if (this.Cursor != System.Windows.Forms.Cursors.Default)
                    this.Cursor = System.Windows.Forms.Cursors.Default;
                float w = IC.DefaultDrawingAttributes.Width;
                IC.DefaultDrawingAttributes = Root.PenAttr[pen].Clone();
                if (pen == LastPenSelected)
                    IC.DefaultDrawingAttributes.Width = w;
                else if (Root.PenWidthEnabled && !Root.WidthAtPenSel)
                {
                    IC.DefaultDrawingAttributes.Width = Root.GlobalPenWidth;
                }
                LastPenSelected = pen;
                IC.DefaultDrawingAttributes.FitToCurve = true;
                for (int b = 0; b < Root.MaxPenCount; b++)
                    //btPen[b].Image = image_pen[b];
                    btPen[b].BackgroundImage = buildPenIcon(Root.PenAttr[b].Color, Root.PenAttr[b].Transparency, b == pen);
                //btPen[pen].Image = image_pen_act[pen];
                btEraser.BackgroundImage = image_eraser;
                btPointer.BackgroundImage = image_pointer;
                EnterEraserMode(false);
                Root.UnPointer();
                Root.PanMode = false;

                if (Root.CanvasCursor == 0)
                {
                    //cursorred = new System.Windows.Forms.Cursor(gInk.Properties.Resources.cursorred.Handle);
                    try
                    {
                        IC.Cursor = cursorred;
                    }
                    catch
                    {
                        IC.Cursor = getCursFromDiskOrRes("cursorarrow", System.Windows.Forms.Cursors.NoMove2D);
                    }
                }
                else if (Root.CanvasCursor == 1)
                    SetPenTipCursor();
                // !!!!! TODO problem re-entrant
                try
                {
                    IC.SetWindowInputRectangle(new Rectangle(0, 0, this.Width, this.Height));
                }
                catch
                {
                    //Console.WriteLine("!!excpt IC.SetWindowInputRectangle");
                    SetWindowInputRectFlag = true;
                }
            }
            Root.CurrentPen = pen;
            if (Root.gpPenWidthVisible)
            {
                Root.gpPenWidthVisible = false;
                Root.UponSubPanelUpdate = true;
            }
            else
                Root.UponButtonsUpdate |= 0x2;

            if (pen != -2)
                Root.LastPen = pen;
        }

        public void RetreatAndExit()
        {
            ToThrough();
            if (ZoomForm.Visible)
                ZoomBtn_Click(btZoom, null);
            try
            {
                string st = Path.GetFullPath(Environment.ExpandEnvironmentVariables(Root.SnapshotBasePath));
                if (!System.IO.Directory.Exists(st))
                    System.IO.Directory.CreateDirectory(st);
                SaveStrokes(st + "AutoSave.strokes.txt");
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Root.Local.FileCanNotWrite, Environment.ExpandEnvironmentVariables(Root.SnapshotBasePath + "AutoSave.strokes.txt")));
                string errorMsg = "Silent exception logged \r\n:" + ex.Message + "\r\n\r\nStack Trace:\r\n" + ex.StackTrace + "\r\n\r\n";
                Program.WriteErrorLog(errorMsg);
            }
            Root.ClearInk();
            SaveUndoStrokes();
            //Root.SaveOptions("config.ini");
            Root.gpPenWidthVisible = false;

            LastTickTime = DateTime.Now;
            ButtonsEntering = -9;
        }

        public void btDock_Click(object sender, EventArgs e)
        {
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }

            LastTickTime = DateTime.Now;
            if (!Root.Docked)
            {
                Root.Dock();
            }
            else
            {
                if (Root.PointerMode)
                {
                    btPointer_Click(null, null);
                    Root.UponButtonsUpdate |= 0x7;
                }
                Root.UnDock();
            }
        }

        public void btWindowMode_Click2(object sender, EventArgs e)
        {
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }

            PolyLineLastX = int.MinValue; PolyLineLastY = int.MinValue; PolyLineInProgress = null;
            Root.gpPenWidthVisible = false;
            try
            {
                IC.SetWindowInputRectangle(new Rectangle(0, 0, 1, 1));
            }
            catch
            {
                Thread.Sleep(1);
                IC.SetWindowInputRectangle(new Rectangle(0, 0, 1, 1));
            }
            if (ZoomForm.Visible)
                ZoomBtn_Click(btZoom, null);
            cursorsnap = new System.Windows.Forms.Cursor(gInk.Properties.Resources.cursorsnap.Handle);
            this.Cursor = cursorsnap;
            Root.ResizeDrawingWindow = true;
            this.Cursor = cursorred;
            Root.SnappingX = -1;
            Root.SnappingY = -1;
            Root.SnappingRect = new Rectangle(0, 0, 0, 0);
            Root.Snapping = 1;
            ButtonsEntering = -2;
            Root.UnPointer();
        }


        public void btPointer_Click(object sender, EventArgs e)
        {
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }

            PolyLineLastX = int.MinValue; PolyLineLastY = int.MinValue; PolyLineInProgress = null;
            Root.gpPenWidthVisible = false;
            TimeSpan tsp = DateTime.Now - MouseTimeDown;

            if (sender != null && tsp.TotalSeconds > Root.LongClickTime)
            {
                btWindowMode_Click2(sender, e);
                return;
            }

            if (!Root.PointerMode)
            {
                SavedTool = Root.ToolSelected;
                SavedFilled = Root.FilledSelected;
                SelectPen(-2);
                PointerModeSnaps.Clear();
                if (Root.AltTabPointer)
                {
                    Root.Dock();
                }
            }
            else
            {
                SelectPen(LastPenSelected);
                SelectTool(SavedTool, SavedFilled);
                SavedTool = -1;
                SavedFilled = -1;
            }
        }

        public void AddPointerSnaps()
        {
            if (PointerModeSnaps.Count > 0)
            {
                for (int i = PointerModeSnaps.Count - 1; i >= 0; i--) // we have to insert then in the reversed way...
                {
                    Bitmap capt = new Bitmap(PointerModeSnaps[i]);
                    ClipartsDlg.Originals.Add(Path.GetFileNameWithoutExtension(PointerModeSnaps[i]), capt);
                    //Stroke st = AddImageStroke(SystemInformation.VirtualScreen.Left, SystemInformation.VirtualScreen.Top, SystemInformation.VirtualScreen.Right, SystemInformation.VirtualScreen.Bottom, Path.GetFileNameWithoutExtension(PointerModeSnaps[i]), Filling.NoFrame);
                    Rectangle r = RectangleToClient(new Rectangle(Left, Top, Width, Height));
                    //Stroke st = AddImageStroke(Left,Top,Right,Bottom, Path.GetFileNameWithoutExtension(PointerModeSnaps[i]), Filling.NoFrame);
                    Stroke st = AddImageStroke(r.Left, r.Top, r.Right, r.Bottom, Path.GetFileNameWithoutExtension(PointerModeSnaps[i]), Filling.NoFrame);
                    try { st.ExtendedProperties.Remove(Root.FADING_PEN); } catch { };  // if the pen was fading we need to remove that 
                }
                SaveUndoStrokes();
                PointerModeSnaps.Clear();
                Root.UponAllDrawingUpdate = true;
            }
        }


        private void btPenWidth_Click(object sender, EventArgs e)
        {
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }

            if (Root.PointerMode)
                return;

            Root.gpPenWidthVisible = !Root.gpPenWidthVisible;
            if (Root.gpPenWidthVisible)
            {
                pboxPenWidthIndicator.Left = (int)Math.Sqrt(IC.DefaultDrawingAttributes.Width * 30);
                Root.UponButtonsUpdate |= 0x2;
            }
            else
                Root.UponSubPanelUpdate = true;
        }

        public void btSnap_Click(object sender, EventArgs e)
        {
            longClickTimer.Stop(); // for an unkown reason the mouse arrives later
            if (sender is ContextMenu)
            {
                sender = (sender as ContextMenu).SourceControl;
                MouseTimeDown = DateTime.FromBinary(0);
            }
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }

            if (ZoomForm.Visible)
                ZoomBtn_Click(btZoom, null);

            TimeSpan tsp = DateTime.Now - MouseTimeDown;

            if (Root.Snapping > 0)
                return;
            PolyLineLastX = int.MinValue; PolyLineLastY = int.MinValue; PolyLineInProgress = null;
            cursorsnap = new System.Windows.Forms.Cursor(gInk.Properties.Resources.cursorsnap.Handle);
            this.Cursor = cursorsnap;

            Root.gpPenWidthVisible = false;

            try
            {
                IC.SetWindowInputRectangle(new Rectangle(0, 0, 1, 1));
            }
            catch
            {
                Thread.Sleep(1);
                IC.SetWindowInputRectangle(new Rectangle(0, 0, 1, 1));
            }
            if (sender != null && tsp.TotalSeconds > Root.LongClickTime)
            {
                SnapWithoutClosing = true;
            }
            else
            {
                SnapWithoutClosing = false;
            }

            Root.SnappingX = -1;
            Root.SnappingY = -1;
            Root.SnappingRect = new Rectangle(0, 0, 0, 0);
            Root.Snapping = 1;
            ButtonsEntering = -2;
            Root.UnPointer();
        }

        public void ExitSnapping()
        {
            try
            {
                IC.SetWindowInputRectangle(new Rectangle(0, 0, this.Width, this.Height));
            }
            catch
            {
                Thread.Sleep(1);
                IC.SetWindowInputRectangle(new Rectangle(0, 0, this.Width, this.Height));
            }
            Root.SnappingX = -1;
            Root.SnappingY = -1;
            Root.Snapping = -60;
            ButtonsEntering = 1;
            Root.SelectPen(Root.CurrentPen);

            this.Cursor = System.Windows.Forms.Cursors.Default;
        }

        public void btStop_Click(object sender, EventArgs e)
        {
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }

            RetreatAndExit();
        }

        DateTime LastTickTime;
        bool[] LastPenStatus = new bool[10];
        bool LastEraserStatus = false;
        bool LastVisibleStatus = false;
        bool LastPointerStatus = false;
        bool LastPanStatus = false;
        bool LastUndoStatus = false;
        bool LastRedoStatus = false;
        bool LastSnapStatus = false;
        bool LastClearStatus = false;
        bool LastVideoStatus = false;
        bool LastDockStatus = false;
        bool LastHandStatus = false;
        bool LastLineStatus = false;
        bool LastRectStatus = false;
        bool LastOvalStatus = false;
        bool LastArrowStatus = false;
        bool LastNumbStatus = false;
        bool LastTextStatus = false;
        bool LastEditStatus = false;
        bool LastMoveStatus = false;
        bool LastMagnetStatus = false;
        bool LastZoomStatus = false;
        bool LastClipArtStatus = false;
        bool LastClipArt1Status = false;
        bool LastClipArt2Status = false;
        bool LastClipArt3Status = false;
        int SnappingPointerStep = 0;
        DateTime SnappingPointerReset;

        private void gpPenWidth_MouseDown(object sender, MouseEventArgs e)
        {
            gpPenWidth_MouseOn = true;
        }

        private void gpPenWidth_MouseMove(object sender, MouseEventArgs e)
        {
            if (gpPenWidth_MouseOn)
            {
                if (e.X < 10 || gpPenWidth.Width - e.X < 10)
                    return;

                Root.GlobalPenWidth = e.X * e.X / 30;
                pboxPenWidthIndicator.Left = e.X - pboxPenWidthIndicator.Width / 2;
                IC.DefaultDrawingAttributes.Width = Root.GlobalPenWidth;
                Root.UponButtonsUpdate |= 0x2;
            }
        }

        private void gpPenWidth_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.X >= 10 && gpPenWidth.Width - e.X >= 10)
            {
                Root.GlobalPenWidth = e.X * e.X / 30;
                pboxPenWidthIndicator.Left = e.X - pboxPenWidthIndicator.Width / 2;
                IC.DefaultDrawingAttributes.Width = Root.GlobalPenWidth;
            }

            if (Root.CanvasCursor == 1)
                SetPenTipCursor();

            Root.gpPenWidthVisible = false;
            Root.UponSubPanelUpdate = true;
            gpPenWidth_MouseOn = false;
        }

        private void pboxPenWidthIndicator_MouseDown(object sender, MouseEventArgs e)
        {
            gpPenWidth_MouseOn = true;
        }

        private void pboxPenWidthIndicator_MouseMove(object sender, MouseEventArgs e)
        {
            if (gpPenWidth_MouseOn)
            {
                int x = e.X + pboxPenWidthIndicator.Left;
                if (x < 10 || gpPenWidth.Width - x < 10)
                    return;

                Root.GlobalPenWidth = x * x / 30;
                pboxPenWidthIndicator.Left = x - pboxPenWidthIndicator.Width / 2;
                IC.DefaultDrawingAttributes.Width = Root.GlobalPenWidth;
                Root.UponButtonsUpdate |= 0x2;
            }
        }

        private void pboxPenWidthIndicator_MouseUp(object sender, MouseEventArgs e)
        {
            if (Root.CanvasCursor == 1)
                SetPenTipCursor();

            Root.gpPenWidthVisible = false;
            Root.UponSubPanelUpdate = true;
            gpPenWidth_MouseOn = false;
        }

        private void SetPenTipCursor()
        {
            Bitmap bitmaptip = (Bitmap)(gInk.Properties.Resources._null).Clone();
            Graphics g = Graphics.FromImage(bitmaptip);
            DrawingAttributes dda = IC.DefaultDrawingAttributes;
            Brush cbrush;
            Point widt;
            if (!Root.EraserMode)
            {
                cbrush = new SolidBrush(IC.DefaultDrawingAttributes.Color);
                //Brush cbrush = new SolidBrush(Color.FromArgb(255 - dda.Transparency, dda.Color.R, dda.Color.G, dda.Color.B));
                widt = new Point((int)IC.DefaultDrawingAttributes.Width, 0);
            }
            else
            {
                cbrush = new SolidBrush(Color.Black);
                widt = new Point(60, 0);
            }
            try
            {
                if (Root.FormDisplay != null)
                {
                    IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref widt);
                }
            }
            catch  // not in good context. considered to be able to stop processing at that time
            {
                return;
            }

            IntPtr screenDc = GetDC(IntPtr.Zero);
            const int VERTRES = 10;
            const int DESKTOPVERTRES = 117;
            int LogicalScreenHeight = GetDeviceCaps(screenDc, VERTRES);
            int PhysicalScreenHeight = GetDeviceCaps(screenDc, DESKTOPVERTRES);
            float ScreenScalingFactor = (float)PhysicalScreenHeight / (float)LogicalScreenHeight;
            ReleaseDC(IntPtr.Zero, screenDc);

            int dia = Math.Max((int)(widt.X * ScreenScalingFactor), 2);
            g.FillEllipse(cbrush, 64 - dia / 2, 64 - dia / 2, dia, dia);
            if (dia <= 5)
            {
                Pen cpen = new Pen(Color.FromArgb(50, 128, 128, 128), 2);
                dia += 6;
                g.DrawEllipse(cpen, 64 - dia / 2, 64 - dia / 2, dia, dia);
            }
            IC.Cursor = new System.Windows.Forms.Cursor(bitmaptip.GetHicon());
            System.Windows.Forms.Cursor.Position = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
        }

        short LastESCStatus = 0;
        int ZoomX = -1;
        int ZoomY = -1;

        void RecomputeZoomPos(int ZoomX, int ZoomY, ref int ZoomFormRePosX, ref int ZoomFormRePosY)
        {
            int d0, d1;
            Point p = new Point(ZoomX, ZoomY);
            Screen scr = Screen.FromPoint(p);
            if (ZoomX < (scr.Bounds.Left + scr.Bounds.Right) / 2)
            {
                d0 = ZoomX + ZoomFormRePosX - scr.Bounds.Left;
                d1 = d0 + ZoomForm.Width;
            }
            else
            {
                d0 = ZoomX + ZoomFormRePosX - scr.Bounds.Right;
                d1 = d0 + ZoomForm.Width;
            }
            if (Math.Sign(d0 * d1) < 0)
            {
                if (ZoomFormRePosX > 0)
                    ZoomFormRePosX = -ZoomImage.Width / 2 - ZoomForm.Width;
                else
                    ZoomFormRePosX = ZoomImage.Width / 2;
            }

            if (ZoomY < (scr.Bounds.Top + scr.Bounds.Bottom) / 2)
            {
                d0 = ZoomY + ZoomFormRePosY - scr.Bounds.Top;
                d1 = d0 + ZoomForm.Height;
            }
            else
            {
                d0 = ZoomY + ZoomFormRePosY - scr.Bounds.Bottom;
                d1 = d0 + ZoomForm.Height;
            }
            if (Math.Sign(d0 * d1) < 0)
            {
                if (ZoomFormRePosY > 0)
                    ZoomFormRePosY = -ZoomImage.Height / 2 - ZoomForm.Height;
                else
                    ZoomFormRePosY = ZoomImage.Height / 2;
            }
        }


        private bool KeyCodeState(SnapInPointerKeys k)
        {
            if (k == SnapInPointerKeys.None)
                return true;
            else if (k == SnapInPointerKeys.Shift)
                return (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0;
            else if (k == SnapInPointerKeys.Control)
                return (GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0;
            else if (k == SnapInPointerKeys.Alt)
                return (GetAsyncKeyState(VK_MENU) & 0x8000) != 0;
            else
                return false;
        }

        private void tiSlide_Tick(object sender, EventArgs e)
        {
            Initializing = false;

            if (ZoomForm.Visible && (Root.ZoomContinous || MousePosition.X != ZoomX || MousePosition.Y != ZoomY))
            {
                ZoomX = MousePosition.X;
                ZoomY = MousePosition.Y;
                RecomputeZoomPos(ZoomX, ZoomY, ref ZoomFormRePosX, ref ZoomFormRePosY);
                ZoomForm.Top = MousePosition.Y + ZoomFormRePosY;
                ZoomForm.Left = MousePosition.X + ZoomFormRePosX;

                Bitmap img;
                img = (ZoomForm.pictureBox1.Visible) ? ZoomImage2 : ZoomImage;

                using (Graphics g = Graphics.FromImage(img))
                {
                    Point p = new Point(MousePosition.X - ZoomImage.Width / 2, MousePosition.Y - ZoomImage.Height / 2);
                    Size sz = new Size(ZoomImage.Width, ZoomImage.Height);
                    g.CopyFromScreen(p, Point.Empty, sz);
                    if (ZoomForm.pictureBox1.Visible)
                    {
                        ZoomForm.pictureBox1.Visible = false;
                        ZoomForm.pictureBox2.Visible = true;
                        ZoomForm.pictureBox2.Refresh();
                    }
                    else
                    {
                        ZoomForm.pictureBox1.Visible = true;
                        ZoomForm.pictureBox2.Visible = false;
                        ZoomForm.pictureBox1.Refresh();
                    }
                    //ZoomForm.Refresh();
                }
            }
            if (Root.FFmpegProcess != null && Root.FFmpegProcess.HasExited)
            {
                Root.VideoRecInProgress = VideoRecInProgress.Stopped;
                btVideo.BackgroundImage = getImgFromDiskOrRes("VidStop", ImageExts);
                Root.UponButtonsUpdate |= 0x2;
            }

            if (SetWindowInputRectFlag) // alternative to prevent some error when trying to call this function from WM_ACTIVATE event handler
                IC.SetWindowInputRectangle(new Rectangle(0, 0, this.Width, this.Height));
            SetWindowInputRectFlag = false;
            // ignore the first tick
            if (LastTickTime.Year == 1987)
            {
                //Console.WriteLine("AA=" + (DateTime.Now.Ticks / 1e7).ToString());
                LastTickTime = DateTime.Now;
                return;
            }

            for (int i = IC.Ink.Strokes.Count - 1; i >= 0; i--)
            {
                Stroke st = IC.Ink.Strokes[i];
                if (st.ExtendedProperties.Contains(Root.FADING_PEN))
                {
                    long j = (long)(st.ExtendedProperties[Root.FADING_PEN].Data);
                    if (DateTime.Now.Ticks > j)
                    {
                        if (st.DrawingAttributes.Transparency == 255)
                        {
                            //IC.Ink.Strokes.RemoveAt(i);
                            IC.Ink.DeleteStroke(IC.Ink.Strokes[i]);
                        }
                        else if (st.DrawingAttributes.Transparency > 245)
                            st.DrawingAttributes.Transparency = 255;
                        else
                            st.DrawingAttributes.Transparency += 10;
                        Root.UponAllDrawingUpdate = true;
                    }
                }
            }

            Size AimedSize = new Size(gpButtonsWidth, gpButtonsHeight);
            Point AimedPos = new Point(gpButtonsLeft, gpButtonsTop);
            if (ButtonsEntering == 0)                  // do nothing
            {
                AimedPos.X = gpButtons.Left; // stay at current location
                AimedPos.Y = gpButtons.Top; // stay at current location
                AimedSize.Width = VisibleToolbar.Width;
                AimedSize.Height = VisibleToolbar.Height;
            }
            else if (ButtonsEntering == -9)              // Full Folding is requested
            {
                switch (Root.ToolbarOrientation)
                {
                    case Orientation.toLeft:
                        AimedPos.X = gpButtonsLeft + gpButtonsWidth;
                        AimedSize.Width = 0;
                        break;
                    case Orientation.toRight:
                        AimedPos.X = gpButtonsLeft;
                        AimedSize.Width = 0;
                        break;
                    case Orientation.toUp:
                        AimedPos.Y = gpButtonsTop + gpButtonsHeight;
                        AimedSize.Height = 0;
                        break;
                    case Orientation.toDown:
                        AimedPos.Y = gpButtonsTop;
                        AimedSize.Height = 0;
                        break;
                }
            }
            else if (ButtonsEntering < 0)               // folding
            {
                int d = 0;
                if (Root.Snapping > 0)                  // if folding for snapping, final should be fully closed
                    d = Math.Max(gpButtonsWidth, gpButtonsHeight) - 0;
                else if (Root.Docked)                   // else final position should show only dock button
                    d = Math.Max(gpButtonsWidth, gpButtonsHeight) - Math.Min(btDock.Width, btDock.Height);
                else                                    // folding with undock is meaningless as security we consider unfolded position for security
                    d = 0;
                switch (Root.ToolbarOrientation)
                {
                    case Orientation.toLeft:
                        AimedPos.X = gpButtonsLeft + d;
                        AimedSize.Width = gpButtonsWidth - d;
                        break;
                    case Orientation.toRight:
                        AimedPos.X = gpButtonsLeft;
                        AimedSize.Width = gpButtonsWidth - d;
                        break;
                    case Orientation.toUp:
                        AimedPos.Y = gpButtonsTop + d;
                        AimedSize.Height = gpButtonsHeight - d;
                        break;
                    case Orientation.toDown:
                        AimedPos.Y = gpButtonsTop;
                        AimedSize.Height = gpButtonsHeight - d;
                        break;
                }
            }
            else if (ButtonsEntering > 0)       //unfolding
            {
                int d = 0;
                if (Root.Docked)                //unfolding (eg from snapping mode) to docked position
                    d = Math.Max(gpButtonsWidth, gpButtonsHeight) - Math.Min(btDock.Width, btDock.Height);
                else                           //unfolding to show all toolbar
                    d = 0;
                switch (Root.ToolbarOrientation)
                {
                    case Orientation.toLeft:
                        AimedPos.X = gpButtonsLeft + d;
                        AimedSize.Width = Root.Docked ? btDock.Width : gpButtonsWidth;
                        break;
                    case Orientation.toRight:
                        AimedPos.X = gpButtonsLeft;
                        AimedSize.Width = Root.Docked ? btDock.Width : gpButtonsWidth;
                        break;
                    case Orientation.toUp:
                        AimedPos.Y = gpButtonsTop + d;
                        AimedSize.Height = Root.Docked ? btDock.Height : gpButtonsHeight;
                        break;
                    case Orientation.toDown:
                        AimedPos.Y = gpButtonsTop;
                        AimedSize.Height = Root.Docked ? btDock.Height : gpButtonsHeight;
                        break;
                }
            }

            /*Console.WriteLine(gpButtons.Left.ToString() +" "+ AimedPos.X.ToString() + " /  " + gpButtons.Top.ToString() + " " + AimedPos.Y.ToString() + " / " 
                    + gpButtons.Width.ToString() + " " + VisibleToolbar.Width.ToString() + " " + AimedSize.Width.ToString() + " - "
                    + gpButtons.Height.ToString() + " " + VisibleToolbar.Height.ToString() + " " + AimedSize.Height.ToString()  
                    + " = " + ButtonsEntering.ToString());
            */
            if ((gpButtons.Left != AimedPos.X) || (gpButtons.Top != AimedPos.Y) || (VisibleToolbar.Width != AimedSize.Width) || (VisibleToolbar.Height != AimedSize.Height))
            {
                int d;
                d = (int)(.5 * (AimedPos.X - gpButtons.Left));
                if (Math.Abs(d) < (5 * .5))
                    gpButtons.Left = AimedPos.X;
                else
                    gpButtons.Left += d;

                d = (int)(.5 * (AimedPos.Y - gpButtons.Top));
                if (Math.Abs(d) < (5 * .5))
                    gpButtons.Top = AimedPos.Y;
                else
                    gpButtons.Top += d;

                //d = (int)(gpButtons.Width * .9 + AimedSize.Width * .1)
                if (Root.ToolbarOrientation == Orientation.toRight)
                    if (Math.Abs(VisibleToolbar.Width - AimedSize.Width) < 5)
                        VisibleToolbar.Width = AimedSize.Width;
                    else
                        VisibleToolbar.Width = (int)(VisibleToolbar.Width * .5 + AimedSize.Width * .5);
                else
                    VisibleToolbar.Width = gpButtonsWidth - Math.Abs(gpButtons.Left - gpButtonsLeft);// Math.Max(gpButtonsWidth - Math.Abs(gpButtons.Left - gpButtonsLeft), btDock.Width);

                if (Root.ToolbarOrientation == Orientation.toDown)
                    if (Math.Abs(VisibleToolbar.Height - AimedSize.Height) < 5)
                        VisibleToolbar.Height = AimedSize.Height;
                    else
                        VisibleToolbar.Height = (int)(VisibleToolbar.Height * .5 + AimedSize.Height * .5);
                else
                    VisibleToolbar.Height = gpButtonsHeight - Math.Abs(gpButtons.Top - gpButtonsTop);// Math.Max(gpButtonsHeight - Math.Abs(gpButtons.Top - gpButtonsTop), btDock.Height);

                Root.UponAllDrawingUpdate = true;
                Root.UponButtonsUpdate |= 0x1;
                Root.UponButtonsUpdate |= 0x4;
            }
            else if (ButtonsEntering == -9) // and Left=X&&Top==Y
            {
                tiSlide.Enabled = false;
                Root.StopInk();
                return;
            }
            /*else if (ButtonsEntering != 0) // we need redrawing for both fold and unfold
            {
                Root.UponAllDrawingUpdate = true;
                Root.UponButtonsUpdate = 0;
            }
            */
            else if (ButtonsEntering != 0)
            {
                // add a background if required at opening but not when snapping is in progress
                if ((Root.Snapping == 0) && (IC.Ink.Strokes.Count == 0))
                {
                    if ((Root.BoardAtOpening == 1) || (Root.BoardAtOpening == 4 && Root.BoardSelected == 1)) // White
                        AddBackGround(255, 255, 255, 255);
                    else if ((Root.BoardAtOpening == 2) || (Root.BoardAtOpening == 4 && Root.BoardSelected == 2)) // Customed
                        AddBackGround(Root.Gray1[0], Root.Gray1[1], Root.Gray1[2], Root.Gray1[3]);
                    else if ((Root.BoardAtOpening == 3) || (Root.BoardAtOpening == 4 && Root.BoardSelected == 3)) // Black
                        AddBackGround(255, 0, 0, 0);
                    if (Root.BoardAtOpening != 4)    // reset the board selected at opening
                    {
                        Root.BoardSelected = Root.BoardAtOpening;
                    }
                }
                Root.UponButtonsUpdate |= 2;
                ButtonsEntering = 0;
                Console.WriteLine("AB=" + (DateTime.Now.Ticks / 1e7).ToString());
            }



            if (!Root.PointerMode && !this.TopMost)
                ToTopMost();

            // gpPenWidth status

            if (Root.gpPenWidthVisible != gpPenWidth.Visible)
                gpPenWidth.Visible = Root.gpPenWidthVisible;

            bool pressed;

            if (!Root.PointerMode)
            {
                // ESC key : Exit
                short retVal;
                if (Root.Hotkey_Close.Key != 0)
                {
                    retVal = GetKeyState(Root.Hotkey_Close.Key);
                    if ((retVal & 0x8000) == 0x8000 && (LastESCStatus & 0x8000) == 0x0000 && !TextEdited)
                    {
                        if (Root.Snapping > 0)
                        {
                            ExitSnapping();
                        }
                        else if (Root.gpPenWidthVisible)
                        {
                            Root.gpPenWidthVisible = false;
                            Root.UponSubPanelUpdate = true;
                        }
                        else if (Root.Snapping == 0)
                            RetreatAndExit();
                    }
                    LastESCStatus = retVal;
                    TextEdited = false;
                }
            }

            /* // Kept for debug if required
            var array = new byte[256];
            bool OneKeyPressed = false;
            GetKeyboardState(array);
            for(int i=0;i<256;i++)
            {
                if ((array[i] & 0x80) != 0)
                    using (StreamWriter sw = File.AppendText("LogKey.txt"))
                    {
                        sw.WriteLine((OneKeyPressed?"":"\n") + "[" + i.ToString() + "]  return? " + (Root.PointerMode ? "Pointer " : "Nopoint ") + (Root.FormDisplay.HasFocus() ? "Focus " : "NoFoc ") + (Root.AllowHotkeyInPointerMode ? "Allow " : "NoAll ") + Root.Snapping.ToString());
                        Console.WriteLine((OneKeyPressed ? "" : "\n") + "[" + i.ToString() + "]  return? " + (Root.PointerMode ? "Pointer " : "Nopoint ") + (Root.FormDisplay.HasFocus() ? "Focus " : "NoFoc ") + (Root.AllowHotkeyInPointerMode ? "Allow " : "NoAll ") + Root.Snapping.ToString());
                        OneKeyPressed = true;
                    }
            }
            */
            //Console.WriteLine("return? " + (Root.PointerMode ? "Pointer " : "Nopoint ") + (Root.FormDisplay.HasFocus() ? "Focus " : "NoFoc ") + (Root.AllowHotkeyInPointerMode ? "Allow " : "NoAll ") + Root.Snapping.ToString());
            //
            //Console.WriteLine("avt");

            if (Root.PointerMode)
            {
                // we have to use getAsyncKeyState as we do not have the focus
                switch (SnappingPointerStep)
                {
                    case 0:
                        if (KeyCodeState(Root.SnapInPointerHoldKey))
                            SnappingPointerStep += 1;
                        break;
                    case 1:   // awaiting first press
                        if (KeyCodeState(Root.SnapInPointerHoldKey))
                        {
                            if (KeyCodeState(Root.SnapInPointerPressTwiceKey))
                                SnappingPointerStep += 1;
                            // else wait for next key or should check for other keys
                        }
                        else
                            SnappingPointerStep = 0;
                        break;
                    case 2:   // awaiting release
                        if (KeyCodeState(Root.SnapInPointerHoldKey))
                        {
                            if (!KeyCodeState(Root.SnapInPointerPressTwiceKey)) // control released
                            {
                                SnappingPointerStep += 1;
                                SnappingPointerReset = DateTime.Now.AddSeconds(3.0);
                            }
                            // else wait for next key or should check for other keys
                        }
                        else
                            SnappingPointerStep = 0;
                        break;
                    case 3:   // awaiting second release
                        if (DateTime.Now > SnappingPointerReset)
                            SnappingPointerStep = 0;
                        if (KeyCodeState(Root.SnapInPointerHoldKey))
                        {
                            if (KeyCodeState(Root.SnapInPointerPressTwiceKey)) // 
                                SnappingPointerStep = 100;
                            // else wait for next key or should check for other keys
                        }
                        else
                            SnappingPointerStep = 0;
                        break;
                    case 100:
                        SnappingPointerStep += 1;
                        break;
                    case 101:
                        if ((Root.SnapInPointerHoldKey == SnapInPointerKeys.None || !KeyCodeState(Root.SnapInPointerHoldKey)) && !KeyCodeState(Root.SnapInPointerPressTwiceKey)) //all keys released
                        {
                            SnappingPointerStep = 0;
                        }
                        break;
                    default:
                        SnappingPointerStep = 0;
                        break;
                }

                Console.WriteLine(SnappingPointerStep);
                if (SnappingPointerStep == 100)
                {
                    string fn = Environment.ExpandEnvironmentVariables(DateTime.Now.ToString("'%temp%/CtrlShift'ddMMM-HHmmss'.png'"));
                    Root.FormDisplay.SnapShot(new Rectangle(Left, Top, Width, Height), fn);
                    PointerModeSnaps.Add(fn);
                    SnappingPointerStep = 101;      // for security
                    System.Media.SystemSounds.Asterisk.Play();
                }
            }
            else
                SnappingPointerStep = 0;




            if ((Root.PointerMode || (!Root.FormDisplay.HasFocus() && !Root.AllowHotkeyInPointerMode)) || Root.Snapping > 0)
            {
                return;
            }
            //Console.WriteLine("process Keys");
            //if (!AltKeyPressed() && !Root.PointerMode)//&& (SavedPen>=0 || SavedTool>=0))
            if (!AltKeyPressed())
            {
                if (SavedPen >= 0)
                {
                    SelectPen(SavedPen);
                    SavedPen = -1;
                }
                if (SavedTool >= 0)
                {
                    SelectTool(SavedTool, SavedFilled);
                    SavedTool = -1;
                    SavedFilled = -1;
                }
            }

            if ((AltKeyPressed() && !Root.FingerInAction) && tempArrowCursor is null)
            {
                tempArrowCursor = IC.Cursor;
                try
                {
                    IC.Cursor = cursorred;
                }
                catch
                {
                    IC.Cursor = getCursFromDiskOrRes("cursorarrow", System.Windows.Forms.Cursors.NoMove2D);
                }
            }
            else if (!(tempArrowCursor is null) && !AltKeyPressed())
            {
                IC.Cursor = tempArrowCursor;
                tempArrowCursor = null;
            }

            //if (!Root.FingerInAction && (!Root.PointerMode || Root.AllowHotkeyInPointerMode) && Root.Snapping <= 0)

            /* // Kept for debug if required
            if (OneKeyPressed)
                using(StreamWriter sw = File.AppendText("LogKey.txt"))
                {
                    sw.WriteLine(Root.FingerInAction ? "Finger" : "notFinger");
                    Console.WriteLine(Root.FingerInAction ? "Finger" : "notFinger");
                }
            */

            if (!Root.FingerInAction)
            {
                bool control = ((short)(GetKeyState(VK_LCONTROL) | GetKeyState(VK_RCONTROL)) & 0x8000) == 0x8000;
                //bool alt = (((short)(GetKeyState(VK_LMENU) | GetKeyState(VK_RMENU)) & 0x8000) == 0x8000);
                int alt = Root.AltAsOneCommand ? -1 : (AltKeyPressed() ? 1 : 0);
                bool shift = ((short)(GetKeyState(VK_LSHIFT) | GetKeyState(VK_RSHIFT)) & 0x8000) == 0x8000;
                bool win = ((short)(GetKeyState(VK_LWIN) | GetKeyState(VK_RWIN)) & 0x8000) == 0x8000;

                if (Root.Hotkey_Pens[0].ConflictWith(Root.Hotkey_Pens[1]))
                { // same hotkey for pen 0 and pen 1 : we have to rotate through pens
                    pressed = (GetKeyState(Root.Hotkey_Pens[0].Key) & 0x8000) == 0x8000;
                    if (pressed && !LastPenStatus[0] && Root.Hotkey_Pens[0].ModifierMatch(control, alt, shift, win))
                    {
                        int p = LastPenSelected + 1;
                        if (p >= Root.MaxPenCount)
                            p = 0;
                        while (!Root.PenEnabled[p])
                        {
                            p += 1;
                            if (p >= Root.MaxPenCount)
                                p = 0;
                        }
                        SelectPen(p);
                    }
                    LastPenStatus[0] = pressed;
                }
                else
                { // standard behavior
                    for (int p = 0; p < Root.MaxPenCount; p++)
                    {
                        pressed = (GetKeyState(Root.Hotkey_Pens[p].Key) & 0x8000) == 0x8000;
                        if (pressed && !LastPenStatus[p] && Root.Hotkey_Pens[p].ModifierMatch(control, alt, shift, win))
                        {
                            SelectPen(p);
                        }
                        LastPenStatus[p] = pressed;
                    }
                }

                pressed = (GetKeyState(Root.Hotkey_Eraser.Key) & 0x8000) == 0x8000;
                if (pressed && !LastEraserStatus && Root.Hotkey_Eraser.ModifierMatch(control, alt, shift, win))
                {
                    SelectPen(-1);
                }
                LastEraserStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_InkVisible.Key) & 0x8000) == 0x8000;
                if (pressed && !LastVisibleStatus && Root.Hotkey_InkVisible.ModifierMatch(control, alt, shift, win))
                {
                    btInkVisible_Click(null, null);
                }
                LastVisibleStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Undo.Key) & 0x8000) == 0x8000;
                if (pressed && !LastUndoStatus && Root.Hotkey_Undo.ModifierMatch(control, alt, shift, win))
                {
                    if (!Root.InkVisible)
                        Root.SetInkVisible(true);

                    Root.UndoInk();
                }
                LastUndoStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Redo.Key) & 0x8000) == 0x8000;
                if (pressed && !LastRedoStatus && Root.Hotkey_Redo.ModifierMatch(control, alt, shift, win))
                {
                    Root.RedoInk();
                }
                LastRedoStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Pointer.Key) & 0x8000) == 0x8000;
                if (pressed && !LastPointerStatus && Root.Hotkey_Pointer.ModifierMatch(control, alt, shift, win))
                {
                    //SelectPen(-2);
                    if (AltKeyPressed())
                        MouseTimeDown = DateTime.FromBinary(0);
                    else
                        MouseTimeDown = DateTime.Now;
                    btPointer_Click(btPointer, null);
                }
                LastPointerStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Pan.Key) & 0x8000) == 0x8000;
                if (pressed && !LastPanStatus && Root.Hotkey_Pan.ModifierMatch(control, alt, shift, win))
                {
                    btPan_Click(null, null);//SelectPen(-3);
                }
                LastPanStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Clear.Key) & 0x8000) == 0x8000;
                if (pressed && !LastClearStatus && Root.Hotkey_Clear.ModifierMatch(control, alt, shift, win))
                {
                    if (AltKeyPressed())
                        MouseTimeDown = DateTime.FromBinary(0);
                    else
                        MouseTimeDown = DateTime.Now;
                    btClear_Click(btClear, null);
                }
                LastClearStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Video.Key) & 0x8000) == 0x8000;
                if (pressed && !LastVideoStatus && Root.Hotkey_Video.ModifierMatch(control, alt, shift, win))
                {
                    btVideo_Click(null, null);
                }
                LastVideoStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_DockUndock.Key) & 0x8000) == 0x8000;
                if (pressed && !LastDockStatus && Root.Hotkey_DockUndock.ModifierMatch(control, alt, shift, win))
                {
                    btDock_Click(null, null);
                }
                LastDockStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Snap.Key) & 0x8000) == 0x8000;
                if (pressed && !LastSnapStatus && Root.Hotkey_Snap.ModifierMatch(control, alt, shift, win))
                {
                    if (AltKeyPressed())
                        MouseTimeDown = DateTime.FromBinary(0);
                    else
                        MouseTimeDown = DateTime.Now;
                    btSnap_Click(btSnap, null);
                }
                LastSnapStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Hand.Key) & 0x8000) == 0x8000;
                if (pressed && !LastHandStatus && Root.Hotkey_Hand.ModifierMatch(control, alt, shift, win))
                {
                    btTool_Click(btHand, null);
                }
                LastHandStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Line.Key) & 0x8000) == 0x8000;
                if (pressed && !LastLineStatus && Root.Hotkey_Line.ModifierMatch(control, alt, shift, win))
                {
                    btTool_Click(btLine, null);
                }
                LastLineStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Rect.Key) & 0x8000) == 0x8000;
                if (pressed && !LastRectStatus && Root.Hotkey_Rect.ModifierMatch(control, alt, shift, win))
                {
                    btTool_Click(btRect, null);
                }
                LastRectStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Oval.Key) & 0x8000) == 0x8000;
                if (pressed && !LastOvalStatus && Root.Hotkey_Oval.ModifierMatch(control, alt, shift, win))
                {
                    btTool_Click(btOval, null);
                }
                LastOvalStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Arrow.Key) & 0x8000) == 0x8000;
                if (pressed && !LastArrowStatus && Root.Hotkey_Arrow.ModifierMatch(control, alt, shift, win))
                {
                    btTool_Click(btArrow, null);
                }
                LastArrowStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Numb.Key) & 0x8000) == 0x8000;
                if (pressed && !LastNumbStatus && Root.Hotkey_Numb.ModifierMatch(control, alt, shift, win))
                {
                    MouseTimeDown = DateTime.Now;
                    btTool_Click(btNumb, null);
                }
                LastNumbStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Text.Key) & 0x8000) == 0x8000;
                if (pressed && !LastTextStatus && Root.Hotkey_Text.ModifierMatch(control, alt, shift, win))
                {
                    MouseTimeDown = DateTime.Now;
                    btTool_Click(btText, null);
                }
                LastTextStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Edit.Key) & 0x8000) == 0x8000;
                if (pressed && !LastEditStatus && Root.Hotkey_Edit.ModifierMatch(control, alt, shift, win))
                {
                    MouseTimeDown = DateTime.Now;
                    btTool_Click(btEdit, null);
                }
                LastEditStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Move.Key) & 0x8000) == 0x8000;
                if (pressed && !LastMoveStatus && Root.Hotkey_Move.ModifierMatch(control, alt, shift, win))
                {
                    btPan_Click(null, null);
                }
                LastMoveStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Magnet.Key) & 0x8000) == 0x8000;
                if (pressed && !LastMagnetStatus && Root.Hotkey_Magnet.ModifierMatch(control, alt, shift, win))
                {
                    btMagn_Click(null, null);
                }
                LastMagnetStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Zoom.Key) & 0x8000) == 0x8000;
                if (pressed && !LastZoomStatus && Root.Hotkey_Zoom.ModifierMatch(control, alt, shift, win))
                {
                    ZoomBtn_Click(null, null);
                }
                LastZoomStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_ClipArt.Key) & 0x8000) == 0x8000;
                if (pressed && !LastClipArtStatus && Root.Hotkey_ClipArt.ModifierMatch(control, alt, shift, win))
                {
                    btTool_Click(btClipArt, null);
                }
                LastClipArtStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_ClipArt1.Key) & 0x8000) == 0x8000;
                if (pressed && !LastClipArt1Status && Root.Hotkey_ClipArt1.ModifierMatch(control, alt, shift, win))
                {
                    MouseTimeDown = DateTime.Now;
                    btTool_Click(btClip1, null);
                }
                LastClipArt1Status = pressed;

                pressed = (GetKeyState(Root.Hotkey_ClipArt2.Key) & 0x8000) == 0x8000;
                if (pressed && !LastClipArt2Status && Root.Hotkey_ClipArt2.ModifierMatch(control, alt, shift, win))
                {
                    MouseTimeDown = DateTime.Now;
                    btTool_Click(btClip2, null);
                }
                LastClipArt2Status = pressed;

                pressed = (GetKeyState(Root.Hotkey_ClipArt3.Key) & 0x8000) == 0x8000;
                if (pressed && !LastClipArt3Status && Root.Hotkey_ClipArt3.ModifierMatch(control, alt, shift, win))
                {
                    MouseTimeDown = DateTime.Now;
                    btTool_Click(btClip3, null);
                }
                LastClipArt3Status = pressed;
            }

            if (Root.Snapping < 0)
                Root.Snapping++;
        }

        private bool IsInsideVisibleScreen(int x, int y)
        {
            if (Root.WindowRect.Width > 0 && Root.WindowRect.Height > 0)
            {
                return ClientRectangle.Contains(x, y);
            }
            x -= PrimaryLeft;
            y -= PrimaryTop;
            //foreach (Screen s in Screen.AllScreens)
            //	Console.WriteLine(s.Bounds);
            //Console.WriteLine(x.ToString() + ", " + y.ToString());

            foreach (Screen s in Screen.AllScreens)
                if (s.Bounds.Contains(x, y))
                    return true;
            return false;
        }

        int IsMovingToolbar = 0;
        Point HitMovingToolbareXY = new Point();
        bool ToolbarMoved = false;
        private void gpButtons_MouseDown(object sender, MouseEventArgs e)
        {
            if (!Root.AllowDraggingToolbar)
                return;
            if (ButtonsEntering != 0)
                return;

            ToolbarMoved = false;
            IsMovingToolbar = 1;
            HitMovingToolbareXY.X = e.X;
            HitMovingToolbareXY.Y = e.Y;
        }

        private void gpButtons_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMovingToolbar == 1)
            {
                if (Math.Abs(e.X - HitMovingToolbareXY.X) > 20 || Math.Abs(e.Y - HitMovingToolbareXY.Y) > 20)
                    IsMovingToolbar = 2;
            }
            if (IsMovingToolbar == 2)
            {
                if (e.X != HitMovingToolbareXY.X || e.Y != HitMovingToolbareXY.Y)
                {
                    /*
					gpButtonsLeft += e.X - HitMovingToolbareXY.X;
					gpButtonsTop += e.Y - HitMovingToolbareXY.Y;
					
					if (gpButtonsLeft + gpButtonsWidth > SystemInformation.VirtualScreen.Right)
						gpButtonsLeft = SystemInformation.VirtualScreen.Right - gpButtonsWidth;
					if (gpButtonsLeft < SystemInformation.VirtualScreen.Left)
						gpButtonsLeft = SystemInformation.VirtualScreen.Left;
					if (gpButtonsTop + gpButtonsHeight > SystemInformation.VirtualScreen.Bottom)
						gpButtonsTop = SystemInformation.VirtualScreen.Bottom - gpButtonsHeight;
					if (gpButtonsTop < SystemInformation.VirtualScreen.Top)
						gpButtonsTop = SystemInformation.VirtualScreen.Top;
					*/
                    int newleft = gpButtonsLeft + e.X - HitMovingToolbareXY.X;
                    int newtop = gpButtonsTop + e.Y - HitMovingToolbareXY.Y;

                    bool continuemoving;
                    bool toolbarmovedthisframe = false;
                    int dleft = 0, dtop = 0;
                    if
                    (
                        IsInsideVisibleScreen(newleft, newtop) &&
                        IsInsideVisibleScreen(newleft + gpButtonsWidth, newtop) &&
                        IsInsideVisibleScreen(newleft, newtop + gpButtonsHeight) &&
                        IsInsideVisibleScreen(newleft + gpButtonsWidth, newtop + gpButtonsHeight)
                    )
                    {
                        continuemoving = true;
                        ToolbarMoved = true;
                        toolbarmovedthisframe = true;
                        dleft = newleft - gpButtonsLeft;
                        dtop = newtop - gpButtonsTop;
                    }
                    else
                    {
                        do
                        {
                            if (dleft != newleft - gpButtonsLeft)
                                dleft += Math.Sign(newleft - gpButtonsLeft);
                            else
                                break;
                            if
                            (
                                IsInsideVisibleScreen(gpButtonsLeft + dleft, gpButtonsTop + dtop) &&
                                IsInsideVisibleScreen(gpButtonsLeft + gpButtonsWidth + dleft, gpButtonsTop + dtop) &&
                                IsInsideVisibleScreen(gpButtonsLeft + dleft, gpButtonsTop + gpButtonsHeight + dtop) &&
                                IsInsideVisibleScreen(gpButtonsLeft + gpButtonsWidth + dleft, gpButtonsTop + gpButtonsHeight + dtop)
                            )
                            {
                                continuemoving = true;
                                ToolbarMoved = true;
                                toolbarmovedthisframe = true;
                            }
                            else
                            {
                                continuemoving = false;
                                dleft -= Math.Sign(newleft - gpButtonsLeft);
                            }
                        }
                        while (continuemoving);
                        do
                        {
                            if (dtop != newtop - gpButtonsTop)
                                dtop += Math.Sign(newtop - gpButtonsTop);
                            else
                                break;
                            if
                            (
                                IsInsideVisibleScreen(gpButtonsLeft + dleft, gpButtonsTop + dtop) &&
                                IsInsideVisibleScreen(gpButtonsLeft + gpButtonsWidth + dleft, gpButtonsTop + dtop) &&
                                IsInsideVisibleScreen(gpButtonsLeft + dleft, gpButtonsTop + gpButtonsHeight + dtop) &&
                                IsInsideVisibleScreen(gpButtonsLeft + gpButtonsWidth + dleft, gpButtonsTop + gpButtonsHeight + dtop)
                            )
                            {
                                continuemoving = true;
                                ToolbarMoved = true;
                                toolbarmovedthisframe = true;
                            }
                            else
                            {
                                continuemoving = false;
                                dtop -= Math.Sign(newtop - gpButtonsTop);
                            }
                        }
                        while (continuemoving);
                    }

                    if (toolbarmovedthisframe)
                    {
                        gpButtonsLeft += dleft;
                        gpButtonsTop += dtop;
                        Root.gpButtonsLeft = gpButtonsLeft;
                        Root.gpButtonsTop = gpButtonsTop;
                        if (Root.Docked)
                            gpButtons.Left = gpButtonsLeft + gpButtonsWidth - btDock.Right;
                        else
                            gpButtons.Left = gpButtonsLeft;
                        setPenWidthBarPosition();
                        gpButtons.Top = gpButtonsTop;
                        Root.UponAllDrawingUpdate = true;
                    }
                }
            }
        }

        private void gpButtons_MouseUp(object sender, MouseEventArgs e)
        {
            IsMovingToolbar = 0;
        }

        private void btInkVisible_Click(object sender, EventArgs e)
        {
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }
            PolyLineLastX = int.MinValue; PolyLineLastY = int.MinValue; PolyLineInProgress = null;
            Root.SetInkVisible(!Root.InkVisible);
        }

        private Stroke AddBackGround(int A, int B, int C, int D)
        {
            Stroke stk = AddRectStroke(0, 0, Width, Height, Filling.PenColorFilled);
            stk.DrawingAttributes.Transparency = (byte)(255 - A);
            stk.DrawingAttributes.Color = Color.FromArgb(A, B, C, D);
            SaveUndoStrokes();
            Root.UponAllDrawingUpdate = true;
            return stk;
        }

        private int SelectCleanBackground()
        {
            void CleanBackGround_click(object sender, EventArgs e)
            {
                (sender as Control).Parent.Tag = sender;
            }
            Form prompt = new Form();
            prompt.Width = 525;
            prompt.Height = 150;
            prompt.Text = Root.Local.BoardTitle;
            prompt.StartPosition = FormStartPosition.CenterScreen;
            prompt.TopMost = true;

            Label textLabel = new Label() { Left = 50, Top = 10, AutoSize = true, Text = Root.Local.BoardText };
            prompt.Controls.Add(textLabel);

            Button btn1 = new Button() { Text = Root.Local.BoardTransparent, Left = 25, Width = 100, Top = 30, Name = "0", DialogResult = DialogResult.Yes };
            btn1.Click += CleanBackGround_click;
            prompt.Controls.Add(btn1);

            Button btn2 = new Button() { Text = Root.Local.BoardWhite, Left = 150, Width = 100, Top = 30, Name = "1", DialogResult = DialogResult.Yes };
            btn2.Click += CleanBackGround_click;
            prompt.Controls.Add(btn2);

            Button btn3 = new Button() { Text = Root.Local.BoardGray, Left = 275, Width = 100, Top = 30, Name = "2", DialogResult = DialogResult.Yes };
            btn3.BackColor = Color.FromArgb(Root.Gray1[0], Root.Gray1[1], Root.Gray1[2], Root.Gray1[3]);
            prompt.Controls.Add(btn3);
            btn3.Click += CleanBackGround_click;

            /*Button btn4 = new Button() { Text = Root.Local.BoardGray + " (2)", Left = 400, Width = 100, Top = 30, Name = "Gray2", DialogResult = DialogResult.Yes };
            prompt.Controls.Add(btn4);
            btn4.Click += CleanBackGround_click;*/

            //Button btn5 = new Button() { Text = Root.Local.BoardBlack, Left = 25, Width = 100, Top = 60, Name = "Black", DialogResult = DialogResult.Yes };
            Button btn5 = new Button() { Text = Root.Local.BoardBlack, Left = 400, Width = 100, Top = 30, Name = "3", DialogResult = DialogResult.Yes };
            prompt.Controls.Add(btn5);
            btn5.Click += CleanBackGround_click;

            Button btnCancel = new Button() { Text = Root.Local.ButtonCancelText, Left = 350, Width = 100, Top = 80, DialogResult = DialogResult.Cancel };
            prompt.Controls.Add(btnCancel);

            AllowInteractions(true);
            TextEdited = true;
            DialogResult rst = prompt.ShowDialog();
            AllowInteractions(false);

            if (rst == DialogResult.Yes)
                return int.Parse((prompt.Tag as Control).Name);
            else
                return -1;
        }

        public void btClear_Click(object sender, EventArgs e)
        {
            //if(sender != null)
            //    (sender as Button).RightToLeft = RightToLeft.No;
            btClear.RightToLeft = RightToLeft.No;
            longClickTimer.Stop(); // for an unkown reason the mouse arrives later
            if (sender is ContextMenu)
            {
                sender = (sender as ContextMenu).SourceControl;
                MouseTimeDown = DateTime.FromBinary(0);
            }
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }

            TimeSpan tsp = DateTime.Now - MouseTimeDown;
            PolyLineLastX = int.MinValue; PolyLineLastY = int.MinValue; PolyLineInProgress = null;
            if (sender != null && tsp.TotalSeconds > Root.LongClickTime)
            {
                int rst = SelectCleanBackground();
                if (rst >= 0)
                {
                    Root.BoardSelected = rst;
                }
                else
                    return;
            }
            //Root.ClearInk(false); <-- code exploded inhere removing clearcanvus
            Root.FormCollection.IC.Ink.DeleteStrokes();
            if (Root.BoardSelected == 1) // White
                AddBackGround(255, 255, 255, 255);
            else if (Root.BoardSelected == 2) // Customed
                AddBackGround(Root.Gray1[0], Root.Gray1[1], Root.Gray1[2], Root.Gray1[3]);
            else if (Root.BoardSelected == 3) // Black
                AddBackGround(255, 0, 0, 0);
            SaveUndoStrokes();
            // transferred from ClearInk to prevent some blinking
            if (Root.BoardSelected == 0)
            {
                Root.FormDisplay.ClearCanvus();
            }
            Root.FormDisplay.DrawButtons(true);
            Root.FormDisplay.UpdateFormDisplay(true);
        }

        private void btUndo_Click(object sender, EventArgs e)
        {
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }

            if (!Root.InkVisible)
                Root.SetInkVisible(true);

            Root.UndoInk();
        }

        public void btColor_LongClick(object sender)
        {
            for (int b = 0; b < Root.MaxPenCount; b++)
                if ((Button)sender == btPen[b])
                {
                    AllowInteractions(true);
                    //ToThrough();
                    TextEdited = true;

                    SelectPen(b);
                    Root.UponButtonsUpdate |= 0x2;

                    if (PenModifyDlg.ModifyPen(ref Root.PenAttr[b]))
                    {
                        if ((Root.ToolSelected == Tools.Move) || (Root.ToolSelected == Tools.Copy) || (Root.ToolSelected == Tools.Edit)) // if move
                            SelectTool(0);
                        PreparePenImages(Root.PenAttr[b].Transparency, ref image_pen[b], ref image_pen_act[b]);
                        //btPen[b].Image = image_pen_act[b];
                        btPen[b].BackgroundImage = buildPenIcon(Root.PenAttr[b].Color, Root.PenAttr[b].Transparency, false);// image_pen[b];
                        //btPen[b].BackColor = Root.PenAttr[b].Color;
                        btPen[b].FlatAppearance.MouseDownBackColor = Root.PenAttr[b].Color;
                        btPen[b].FlatAppearance.MouseOverBackColor = Root.PenAttr[b].Color;
                        SelectPen(b);
                        Root.UponButtonsUpdate |= 0x2;
                    };
                    AllowInteractions(false);
                    //ToUnThrough();
                }
        }

        public void btColor_Click(object sender, EventArgs e)
        {
            longClickTimer.Stop();
            if (sender is ContextMenu)
            {
                sender = (sender as ContextMenu).SourceControl;
                MouseTimeDown = DateTime.FromBinary(0);
            }
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }
            PolyLineLastX = int.MinValue; PolyLineLastY = int.MinValue; PolyLineInProgress = null;
            TimeSpan tsp = DateTime.Now - MouseTimeDown;
            //Console.WriteLine(string.Format("{1},t = {0:N3}", tsp.TotalSeconds,e.ToString()));
            if (sender != null && tsp.TotalSeconds > Root.LongClickTime)
            {
                btColor_LongClick(sender);
            }

            for (int b = 0; b < Root.MaxPenCount; b++)
                if ((Button)sender == btPen[b])
                {
                    if ((Root.ToolSelected == Tools.Move) || (Root.ToolSelected == Tools.Copy) || (Root.ToolSelected == Tools.Edit)) // if move
                        SelectTool(0);
                    SelectPen(b);
                }
        }

        private void btVideo_Click(object sender, EventArgs e)
        {
            // long click  = start/stop ; short click = pause(start if not started)/resume
            longClickTimer.Stop(); // for an unkown reason the mouse arrives later
            if (sender is ContextMenu)
            {
                sender = (sender as ContextMenu).SourceControl;
                MouseTimeDown = DateTime.FromBinary(0);
            }
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }
            PolyLineLastX = int.MinValue; PolyLineLastY = int.MinValue; PolyLineInProgress = null;
            TimeSpan tsp = DateTime.Now - MouseTimeDown;
            if (Root.VideoRecordMode == VideoRecordMode.NoVideo) // button should be hidden but as security we do the check
                return;

            if (Root.VideoRecInProgress == VideoRecInProgress.Stopped) // no recording so we start
            {
                VideoRecordStart();
            }
            else if ((sender != null && tsp.TotalSeconds > Root.LongClickTime) || Root.VideoRecordMode == VideoRecordMode.OBSBcst) // there is only start/stop for Broadcast 
            {
                VideoRecordStop();
            }
            else if (Root.VideoRecInProgress == VideoRecInProgress.Recording)
            {
                VideoRecordPause();
            }
            else // recording & Shortclick & paused
            {
                VideoRecordResume();
            }
        }

        public void VideoRecordStart()
        {
            Root.VideoRecordCounter += 1;
            if (Root.VideoRecordMode == VideoRecordMode.FfmpegRec)
            {
                Root.VideoRecordWindowInProgress = true;
                btSnap_Click(null, null);
            }
            else
            {
                /*try
                {
                    Console.Write("-->" + (Root.ObsRecvTask == null).ToString());
                    if (Root.ObsRecvTask != null)
                        Console.Write(" ; " + Root.ObsRecvTask.IsCompleted.ToString());
                }
                finally
                {
                    Console.WriteLine();
                }*/
                if (Root.ObsRecvTask == null || Root.ObsRecvTask.IsCompleted)
                {
                    Root.ObsRecvTask = Task.Run(() => ReceiveObsMesgs(this));
                }
                Task.Run(() => ObsStartRecording(this));
            }
        }
        public void VideoRecordStartFFmpeg(Rectangle rect)
        {
            const int VERTRES = 10;
            const int DESKTOPVERTRES = 117;

            IntPtr screenDc = GetDC(IntPtr.Zero);
            int LogicalScreenHeight = GetDeviceCaps(screenDc, VERTRES);
            int PhysicalScreenHeight = GetDeviceCaps(screenDc, DESKTOPVERTRES);
            float ScreenScalingFactor = (float)PhysicalScreenHeight / (float)LogicalScreenHeight;
            ReleaseDC(IntPtr.Zero, screenDc);

            rect.X = (int)(rect.X * ScreenScalingFactor);
            rect.Y = (int)(rect.Y * ScreenScalingFactor);
            rect.Width = (int)(rect.Width * ScreenScalingFactor / 2) * 2;
            rect.Height = (int)(rect.Height * ScreenScalingFactor / 2) * 2;

            Root.FFmpegProcess = new Process();
            string[] cmdArgs = Root.ExpandVarCmd(Root.FFMpegCmd, rect.X, rect.Y, rect.Width, rect.Height).Split(new char[] { ' ' }, 2);
            Console.WriteLine(cmdArgs[0] + " " + cmdArgs[1]);

            Root.FFmpegProcess.StartInfo.FileName = cmdArgs[0];
            Root.FFmpegProcess.StartInfo.Arguments = cmdArgs[1];

            Root.FFmpegProcess.StartInfo.UseShellExecute = false;
            Root.FFmpegProcess.StartInfo.CreateNoWindow = true;
            Root.FFmpegProcess.StartInfo.RedirectStandardInput = true;
            Root.FFmpegProcess.StartInfo.RedirectStandardOutput = true;
            Root.FFmpegProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            Root.FFmpegProcess.Start();
            IntPtr ptr = Root.FFmpegProcess.MainWindowHandle;
            ShowWindow(ptr.ToInt32(), 2);

            Root.VideoRecInProgress = VideoRecInProgress.Recording;
            SetVidBgImage();
            //ExitSnapping();
        }

        static async Task ReceiveObsMesgs(FormCollection frm)
        {
            string HashEncode(string input)
            {
                var sha256 = new SHA256Managed();

                byte[] textBytes = Encoding.ASCII.GetBytes(input);
                byte[] hash = sha256.ComputeHash(textBytes);

                return System.Convert.ToBase64String(hash);
            }

            CancellationToken ct = frm.Root.ObsCancel.Token;
            frm.Root.VideoRecordWindowInProgress = true;
            if (ct.IsCancellationRequested)
                return;
            if (frm.Root.ObsWs == null)
            {
                frm.Root.ObsWs = new ClientWebSocket();
                //Console.WriteLine("WS Created");
            }
            var rcvBytes = new byte[4096];
            var rcvBuffer = new ArraySegment<byte>(rcvBytes);
            WebSocketReceiveResult rcvResult;
            if (frm.Root.ObsWs.State != WebSocketState.Open)
            {
                await frm.Root.ObsWs.ConnectAsync(new Uri(frm.Root.ObsUrl), ct);
                //Console.WriteLine("WS Connected");
                await SendInWs(frm.Root.ObsWs, "GetAuthRequired", ct);
                rcvResult = await frm.Root.ObsWs.ReceiveAsync(rcvBuffer, ct);
                string st = Encoding.UTF8.GetString(rcvBuffer.Array, 0, rcvResult.Count);
                //Console.WriteLine("getAuth => " + st);
                if (st.Contains("authRequired\": t"))
                {
                    int i = st.IndexOf("\"challenge\":");
                    i = st.IndexOf("\"", i + "\"challenge\":".Length + 1) + 1;
                    int j = st.IndexOf("\"", i + 1);
                    string challenge = st.Substring(i, j - i);
                    i = st.IndexOf("\"salt\":");
                    i = st.IndexOf("\"", i + "\"salt\":".Length + 1) + 1;
                    j = st.IndexOf("\"", i + 1);
                    string salt = st.Substring(i, j - i);
                    //Console.WriteLine(challenge + " - " + salt);
                    string authResponse = HashEncode(HashEncode(frm.Root.ObsPwd + salt) + challenge);
                    await SendInWs(frm.Root.ObsWs, "Authenticate", ct, ",\"auth\": \"" + authResponse + "\"");
                    rcvResult = await frm.Root.ObsWs.ReceiveAsync(rcvBuffer, ct);
                    st = Encoding.UTF8.GetString(rcvBuffer.Array, 0, rcvResult.Count);
                    if (!st.Contains("\"ok\""))
                    {
                        await frm.Root.ObsWs.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Authentication failed", ct);
                        frm.Root.ObsWs = null;
                        frm.Root.ObsRecvTask = null;
                        frm.btVideo.BackgroundImage = FormCollection.getImgFromDiskOrRes("VidDead", frm.ImageExts);
                    }
                }

            }
            frm.Root.VideoRecordWindowInProgress = false;
            while (frm.Root.ObsWs != null && frm.Root.ObsWs.State == WebSocketState.Open && !ct.IsCancellationRequested) // && frm.Root.VideoRecInProgress == VideoRecInProgress.Recording )
            {
                rcvResult = await frm.Root.ObsWs.ReceiveAsync(rcvBuffer, ct);
                if (ct.IsCancellationRequested)
                    return;
                string st = Encoding.UTF8.GetString(rcvBuffer.Array, 0, rcvResult.Count);
                //Console.WriteLine("ObsReturned " + st);
                if (st.Contains("\"RecordingStopped\""))
                    frm.Root.VideoRecInProgress = VideoRecInProgress.Stopped;
                else if (st.Contains("\"RecordingPaused\""))
                    frm.Root.VideoRecInProgress = VideoRecInProgress.Paused;
                else if (st.Contains("StreamStopping"))
                    frm.Root.VideoRecInProgress = VideoRecInProgress.Stopped;
                else if (st.Contains("StreamStarted"))
                    frm.Root.VideoRecInProgress = VideoRecInProgress.Streaming;
                else if (st.Contains("\"RecordingStarted\"") || st.Contains("\"RecordingResumed\""))
                    frm.Root.VideoRecInProgress = VideoRecInProgress.Recording;
                // cases from getInitialStatus;
                else if (st.Contains("\"recording - paused\": true") || st.Contains("\"recording-paused\": true") || st.Contains("\"isRecordingPaused\": true"))
                    frm.Root.VideoRecInProgress = VideoRecInProgress.Paused;
                else if (st.Contains("\"recording\": true") || st.Contains("\"isRecording\": true"))
                    frm.Root.VideoRecInProgress = VideoRecInProgress.Recording;
                else if (st.Contains("\"streaming\": true"))
                    frm.Root.VideoRecInProgress = VideoRecInProgress.Streaming;
                else if (st.Contains("\"recording\": false") || st.Contains("\"isRecording\": false") || st.Contains("\"streaming\": false"))
                    frm.Root.VideoRecInProgress = VideoRecInProgress.Stopped;
                frm.SetVidBgImage();
                //Console.WriteLine("vidbg " + frm.Root.VideoRecInProgress.ToString());
                // for unknown reasons, button update seems unreliable : robustify repeating update after 100ms
                Thread.Sleep(100);
                frm.SetVidBgImage();
                //Console.WriteLine(frm.btVideo.BackgroundImage.ToString()+" vidbg2 " + frm.Root.UponButtonsUpdate);
            }
            frm.btVideo.BackgroundImage = FormCollection.getImgFromDiskOrRes("VidDead", frm.ImageExts); // the recv task is dead so we put the cross;
            //Console.WriteLine("endoft");
        }

        static async Task ObsStartRecording(FormCollection frm)
        {
            //Console.WriteLine("StartRec");
            while ((frm.Root.ObsWs == null || frm.Root.VideoRecordWindowInProgress) && !frm.Root.ObsCancel.Token.IsCancellationRequested)// frm.Root.ObsWs.State != WebSocketState.Open)
                await Task.Delay(50);
            if (frm.Root.VideoRecordMode == VideoRecordMode.OBSRec)
                await Task.Run(() => SendInWs(frm.Root.ObsWs, "StartRecording", frm.Root.ObsCancel.Token));
            else if (frm.Root.VideoRecordMode == VideoRecordMode.OBSBcst)
                await Task.Run(() => SendInWs(frm.Root.ObsWs, "StartStreaming", frm.Root.ObsCancel.Token));
            //Console.WriteLine("ExitStartRec");
        }

        public void VideoRecordStop()
        {
            if (Root.VideoRecordMode == VideoRecordMode.FfmpegRec)
            {
                Root.FFmpegProcess.StandardInput.WriteLine("q");    // to stop properly stops correctly file
                Thread.Sleep(250);
                try { Root.FFmpegProcess.Kill(); } catch { };
                Root.VideoRecInProgress = VideoRecInProgress.Stopped;
                btVideo.BackgroundImage = getImgFromDiskOrRes("VidStop", ImageExts);
                Root.UponButtonsUpdate |= 0x2;
            }
            else
            {
                if (Root.ObsRecvTask == null || Root.ObsRecvTask.IsCompleted)
                {
                    Root.ObsRecvTask = Task.Run(() => ReceiveObsMesgs(this));
                }
                Task.Run(() => ObsStopRecording(this));
            }
        }

        static async Task ObsStopRecording(FormCollection frm)
        {
            while ((frm.Root.ObsWs == null || frm.Root.VideoRecordWindowInProgress) && !frm.Root.ObsCancel.Token.IsCancellationRequested)// frm.Root.ObsWs.State != WebSocketState.Open)
                await Task.Delay(50);
            if (frm.Root.VideoRecordMode == VideoRecordMode.OBSRec)
                await Task.Run(() => SendInWs(frm.Root.ObsWs, "StopRecording", frm.Root.ObsCancel.Token));
            else if (frm.Root.VideoRecordMode == VideoRecordMode.OBSBcst)
                await Task.Run(() => SendInWs(frm.Root.ObsWs, "StopStreaming", frm.Root.ObsCancel.Token));
        }

        public void VideoRecordPause()
        {
            if (Root.VideoRecordMode == VideoRecordMode.FfmpegRec)
            {
                VideoRecordStop();
            }
            else if (Root.VideoRecordMode == VideoRecordMode.OBSRec)
                Task.Run(() => SendInWs(Root.ObsWs, "PauseRecording", Root.ObsCancel.Token));
            else if (Root.VideoRecordMode == VideoRecordMode.OBSRec)
                Task.Run(() => ObsStopRecording(this));
        }

        public void VideoRecordResume()
        {
            Task.Run(() => SendInWs(Root.ObsWs, "ResumeRecording", Root.ObsCancel.Token));
        }

        static async Task SendInWs(ClientWebSocket ws, string cmd, CancellationToken ct, string parameters = "")
        {
            //Console.WriteLine("enter " + cmd);
            string msg = string.Format("{{\"message-id\":\"{0}\",\"request-type\":\"{1}\" {2} }}", (int)(DateTime.UtcNow.TimeOfDay.TotalSeconds), cmd, parameters);
            byte[] sendBytes = Encoding.UTF8.GetBytes(msg);
            var sendBuffer = new ArraySegment<byte>(sendBytes);
            while ((ws.State != WebSocketState.Open) && !ct.IsCancellationRequested)// frm.Root.ObsWs.State != WebSocketState.Open)
                await Task.Delay(50);
            await ws.SendAsync(sendBuffer, WebSocketMessageType.Text, true, ct);
            //Console.WriteLine("exit " + cmd);
        }

        private void btClear_RightToLeftChanged(object sender, EventArgs e)
        {
            /* work in progress
            if((sender as Button).RightToLeft == RightToLeft.No)
                (sender as Button).BackgroundImage = global::gInk.Properties.Resources.blackboard;
            else 
                (sender as Button).BackgroundImage = global::gInk.Properties.Resources.garbage;
            */
            btClear.BackgroundImage = getImgFromDiskOrRes("garbage", ImageExts);
            //Console.WriteLine("R2L " + (sender as Button).Name + " . " + (sender as Button).RightToLeft.ToString());
            Root.UponButtonsUpdate |= 0x2;
        }

        public void SetTagNumber()
        {
            AllowInteractions(true);
            //ToThrough();
            int k = -1;
            FormInput inp = new FormInput(Root.Local.DlgTagCaption, Root.Local.DlgTagLabel, "", false, Root, null);

            while (!int.TryParse(inp.TextOut(), out k))
            {
                inp.TextIn(Root.TagNumbering.ToString());
                if (inp.ShowDialog() == DialogResult.Cancel)
                {
                    inp.TextIn("");
                    break;
                }
            }
            AllowInteractions(false);
            //ToUnThrough();
            if (inp.TextOut().Length == 0) return;
            Root.TagNumbering = k;
        }

        private void FontBtn_Modify()
        {
            AllowInteractions(true);
            FontDlg.Font = new Font(TextFont, (float)TextSize, (TextItalic ? FontStyle.Italic : FontStyle.Regular) | (TextBold ? FontStyle.Bold : FontStyle.Regular));
            if (FontDlg.ShowDialog() == DialogResult.OK)
            {
                TextFont = FontDlg.Font.Name;
                TextItalic = (FontDlg.Font.Style & FontStyle.Italic) != 0;
                TextBold = (FontDlg.Font.Style & FontStyle.Bold) != 0;
                TextSize = (int)FontDlg.Font.Size;
            }
            AllowInteractions(false);
        }
        private void TagFontBtn_Modify()
        {
            AllowInteractions(true);
            FontDlg.Font = new Font(TagFont, (float)TagSize, (TagItalic ? FontStyle.Italic : FontStyle.Regular) | (TagBold ? FontStyle.Bold : FontStyle.Regular));
            if (FontDlg.ShowDialog() == DialogResult.OK)
            {
                TagFont = FontDlg.Font.Name;
                TagItalic = (FontDlg.Font.Style & FontStyle.Italic) != 0;
                TagBold = (FontDlg.Font.Style & FontStyle.Bold) != 0;
                TagSize = (int)FontDlg.Font.Size;
            }
            AllowInteractions(false);
        }

        public void btTool_Click(object sender, EventArgs e)
        {
            //btClear.RightToLeft = RightToLeft.No;
            longClickTimer.Stop(); // for an unkown reason the mouse arrives later
            if (sender is ContextMenu)
            {
                sender = (sender as ContextMenu).SourceControl;
                MouseTimeDown = DateTime.FromBinary(0);
            }
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }

            TimeSpan tsp = DateTime.Now - MouseTimeDown;
            if (ClipartsDlg.Visible)
            {
                Console.WriteLine("Close ClipArtDlg");
                ClipartsDlg.Close();
            }

            int i = -1;
            if (((Button)sender).Name.Contains("Hand"))
                i = Tools.Hand;
            else if (((Button)sender).Name.Contains("Line"))
                i = Root.ToolSelected == Tools.Poly ? Tools.Poly : Tools.Line;    // to keep filled
            else if (((Button)sender).Name.Contains("Rect"))
                i = Tools.Rect;
            else if (((Button)sender).Name.Contains("Oval"))
                i = Tools.Oval;
            else if (((Button)sender).Name.Contains("Arrow"))
                if (Root.ToolSelected == Tools.EndArrow)
                    i = Tools.StartArrow;
                else if (Root.ToolSelected == Tools.StartArrow)
                    i = Tools.EndArrow;
                else if (Root.DefaultArrow_start)
                    i = Tools.StartArrow;
                else
                    i = Tools.EndArrow;
            //               i = (Root.DefaultArrow_start ||Root.ToolSelected==5) ?4:5 ;
            else if (((Button)sender).Name.Contains("Numb"))
            {
                if (sender != null && tsp.TotalSeconds > Root.LongClickTime)
                {
                    TagFontBtn_Modify();
                    return;
                }
                else
                    i = Tools.NumberTag;
            }
            else if (((Button)sender).Name.Contains("Text"))
                i = Tools.txtLeftAligned;
            else if (((Button)sender).Name.Contains("Edit"))
                if (sender != null && tsp.TotalSeconds > Root.LongClickTime)
                {
                    FontBtn_Modify();
                    return;
                }
                else
                    i = Tools.Edit;
            else if (((Button)sender).Name.Contains("ClipArt"))
            {
                AllowInteractions(true);
                TextEdited = true;
                setClipArtDlgPosition();
                i = -1;
                if (ClipartsDlg.ShowDialog() == DialogResult.OK)
                {
                    i = Tools.ClipArt;
                    Root.ImageStamp = new ClipArtData { ImageStamp = ClipartsDlg.ImageStamp, X = ClipartsDlg.ImgSizeX, Y = ClipartsDlg.ImgSizeY, Filling = ClipartsDlg.ImageStampFilling };
                }
                AllowInteractions(false);
                if (i < 0) return;
            }
            else if (((Button)sender).Name.Contains("Clip"))    // i.e.Clip1/Clip2/Clip3
            {
                if (sender != null && tsp.TotalSeconds > Root.LongClickTime)
                {
                    AllowInteractions(true);
                    TextEdited = true;
                    ImageLister dlg = new ImageLister(Root);
                    dlg.StartPosition = FormStartPosition.CenterScreen;
                    //dlg.Left = gpButtons.Right - dlg.Width - 1;
                    //dlg.Top = gpButtons.Top - dlg.Height - 1;
                    dlg.FromClpBtn.Visible = false;
                    dlg.LoadImageBtn.Visible = false;
                    dlg.DelBtn.Visible = false;
                    i = -1;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            ((Button)sender).BackgroundImage = getImgFromDiskOrRes(dlg.ImageStamp, ImageExts);
                            ((Button)sender).Tag = new ClipArtData { ImageStamp = dlg.ImageStamp, X = dlg.ImgSizeX, Y = dlg.ImgSizeY, Filling = dlg.ImageStampFilling };
                            i = Tools.ClipArt;
                        }
                        catch
                        { // case of failure : image from Clipboard but normally handled;
                            MessageBox.Show("error when setting clipart shortcut");
                        }
                    }
                    AllowInteractions(false);
                    if (i < 0) return;
                }
                btClipSel = ((Button)sender).Tag;
                Root.ImageStamp = (ClipArtData)btClipSel;
                i = Tools.ClipArt;
            }
            if (i >= Tools.Hand)
                SelectPen(LastPenSelected);
            SelectTool(i);
        }

        public void btEraser_Click(object sender, EventArgs e)
        {
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }

            SelectPen(-1);
        }


        private void btPan_Click(object sender, EventArgs e)
        {
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }
            if (Root.ToolSelected == Tools.Move)
            {
                SelectPen(LastPenSelected);
                SelectTool(Tools.Copy);
            }
            else if (Root.ToolSelected == Tools.Copy)
            {
                SelectPen(-3);
            }
            else
            {
                SelectPen(LastPenSelected);
                SelectTool(Tools.Move);
            }
        }

        private void btMagn_Click(object sender, EventArgs e)
        {
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }
            Root.MagneticRadius *= -1; //invert
            if (Root.MagneticRadius > 0)
                btMagn.BackgroundImage = getImgFromDiskOrRes("Magnetic_act", ImageExts);
            else
                btMagn.BackgroundImage = getImgFromDiskOrRes("Magnetic", ImageExts);
            Root.UponButtonsUpdate |= 0x2;
        }

        short LastF4Status = 0;

        public bool ZoomCapturing = false;
        public bool ZoomCaptured = false;
        private void ZoomBtn_Click(object sender, EventArgs e)
        {
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }
            if (Root.CanvasCursor == 1)
                SetPenTipCursor();
            if (ZoomForm.Visible)
            {
                ZoomForm.Hide();
                if ((Root.ZoomEnabled & 2) != 0)
                {
                    ZoomCapturing = true;
                    btZoom.BackgroundImage = getImgFromDiskOrRes("ZoomWin_act");
                    try
                    {
                        IC.Cursor = cursorred;
                    }
                    catch
                    {
                        IC.Cursor = getCursFromDiskOrRes("cursorarrow", System.Windows.Forms.Cursors.NoMove2D);
                    }
                }
                else
                {
                    btZoom.BackgroundImage = getImgFromDiskOrRes("Zoom");
                }
            }
            else if (ZoomCapturing || ZoomCaptured)
            {
                if (ZoomCaptured)
                {
                    IC.Ink.DeleteStrokes();
                    LoadStrokes(ZoomSaveStroke);
                    Root.UponAllDrawingUpdate = true;
                    Root.FormDisplay.timer1_Tick(null, null);
                }
                ZoomCapturing = false;
                ZoomCaptured = false;
                btZoom.BackgroundImage = getImgFromDiskOrRes("Zoom");
            }
            else
            {
                if ((Root.ZoomEnabled & 1) != 0)
                {
                    ZoomForm.Width = (int)(Root.ZoomWidth * Root.ZoomScale);
                    ZoomForm.Height = (int)(Root.ZoomHeight * Root.ZoomScale);
                    ZoomForm.Show();
                    btZoom.BackgroundImage = getImgFromDiskOrRes("Zoom_act");
                }
                else // if((Root.ZoomEnabled & 2)!=0)
                {
                    ZoomCapturing = true;
                    btZoom.BackgroundImage = getImgFromDiskOrRes("ZoomWin_act");
                }
            }
            Root.UponButtonsUpdate |= 0x2;
        }

        private void FormCollection_FormClosing(object sender, FormClosingEventArgs e)
        {
            // check if F4 key is pressed and we assume it's Alt+F4
            short retVal = GetKeyState(0x73);
            if ((retVal & 0x8000) == 0x8000 && (LastF4Status & 0x8000) == 0x0000)
            {
                e.Cancel = true;

                // the following block is copyed from tiSlide_Tick() where we check whether ESC is pressed
                if (Root.Snapping > 0)
                {
                    ExitSnapping();
                    Root.VideoRecordWindowInProgress = false;
                }
                else if (Root.gpPenWidthVisible)
                {
                    Root.gpPenWidthVisible = false;
                    Root.UponSubPanelUpdate = true;
                }
                else if (Root.Snapping == 0)
                    RetreatAndExit();
            }

            LastF4Status = retVal;
        }

        public void RestorePolylineData(Stroke st)
        {
            if (PolyLineLastX == int.MinValue)
                return;
            Point pt = st.GetPoint(st.GetPoints().Length - 1);
            IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref pt);
            PolyLineInProgress = st;
            PolyLineLastX = pt.X;
            PolyLineLastY = pt.Y;
        }

        public void AllowInteractions(bool enter)
        {
            if (enter)
            {
                if (IC.Enabled)
                {
                    tiSlide.Stop();
                    IC.Enabled = false;
                }
            }
            else
            {
                if (!IC.Enabled)
                {
                    IC.Enabled = true;
                    tiSlide.Start();
                    Select();
                }
            }
        }

        public void SaveStrokes(string fn = "ppinkSav.txt")
        {
            string outp = "";
            int l;
            Point p;
            DrawingAttributes da;
            using (FileStream fileout = File.Create(fn, 10, FileOptions.Asynchronous))
            {
                void writeUtf(string st)
                {
                    byte[] by = Encoding.UTF8.GetBytes(st);
                    fileout.Write(by, 0, by.Length);
                }
                writeUtf("# ppInk Stroke restoration\n");
                writeUtf("# gOneStrokeCanvus : ");
                writeUtf("# " + Root.FormDisplay.gOneStrokeCanvus.DpiX.ToString() + "; " + Root.FormDisplay.gOneStrokeCanvus.DpiY.ToString() + "/" +
                         Root.FormDisplay.gOneStrokeCanvus.PageScale.ToString() + "-" + Root.FormDisplay.gOneStrokeCanvus.PageUnit.ToString() + "/" +
                         Root.FormDisplay.gOneStrokeCanvus.RenderingOrigin.ToString() + "\n");
                Point pt;
                pt = new Point(0, 0);
                IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pt);
                writeUtf("#P2IS 0,0 -> " + pt.ToString() + "\n");

                pt = new Point(1920, 1080);
                IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pt);
                writeUtf("#P2IS 1920,1080 -> " + pt.ToString() + "\n");

                pt = new Point(0, 0);
                IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref pt);
                writeUtf("#IS2P 0,0 -> " + pt.ToString() + "\n");

                pt = new Point(10000, 20000);
                IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref pt);
                writeUtf("#IS2P 10000,20000 -> " + pt.ToString() + "\n");

                pt = new Point(20000, 10000);
                IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref pt);
                writeUtf("#IS2P 20000,10000 -> " + pt.ToString() + "\n");

                System.Windows.Forms.Control II = FromHandle(IC.Handle);
                writeUtf("# IC.Handle : ");

                pt = new Point(0, 0);
                IC.Renderer.PixelToInkSpace(IC.Handle, ref pt);
                writeUtf("#P2IS 0,0 -> " + pt.ToString() + "\n");

                pt = new Point(1920, 1080);
                IC.Renderer.PixelToInkSpace(IC.Handle, ref pt);
                writeUtf("#P2IS 1920,1080 -> " + pt.ToString() + "\n");

                pt = new Point(0, 0);
                IC.Renderer.InkSpaceToPixel(IC.Handle, ref pt);
                writeUtf("#IS2P 0,0 -> " + pt.ToString() + "\n");

                pt = new Point(10000, 20000);
                IC.Renderer.InkSpaceToPixel(IC.Handle, ref pt);
                writeUtf("#IS2P 10000,20000 -> " + pt.ToString() + "\n");

                pt = new Point(20000, 10000);
                IC.Renderer.InkSpaceToPixel(IC.Handle, ref pt);
                writeUtf("#IS2P 20000,10000 -> " + pt.ToString() + "\n");

                foreach (Stroke st in Root.FormCollection.IC.Ink.Strokes)
                {
                    l = st.GetPoints().Length;
                    writeUtf("ID = " + st.Id.ToString() + " {\npts " + l.ToString() + " = ");
                    outp = "";
                    for (int i = 0; i < l; i++)
                    {
                        p = st.GetPoint(i);
                        outp += p.X + "," + p.Y + ";";
                    }
                    writeUtf(outp + "\n");
                    Rectangle r = st.GetBoundingBox();
                    outp = "# boxed in " + r.Location.ToString() + " - " + r.Size.ToString() + "\n";
                    writeUtf(outp);
                    da = st.DrawingAttributes;
                    writeUtf("DA = Color [A=255, R=" + da.Color.R.ToString() + ", G=" + da.Color.G.ToString() + ", B=" + da.Color.B.ToString() + "] T=" + da.Transparency + (da.FitToCurve ? ", Fit, W=" : ", NotFit, W=") + da.Width.ToString() + "\n");
                    outp = "";
                    foreach (ExtendedProperty pr in st.ExtendedProperties)
                    {
                        //outp += pr.Id.ToString() + " (" + pr.Data.GetType() + ") :" + Encoding.UTF8.GetString(Encoding.Default.GetBytes(pr.Data.ToString())).Replace('\n','\r') + "\n";
                        outp += pr.Id.ToString() + "%" + pr.Data.GetType() + ":" + pr.Data.ToString().Replace("\r", "").Replace('\n', '\a') + "\n";
                    }
                    outp += "}\n";
                    writeUtf(outp);
                }
            }
        }

        public void LoadStrokes(string fn = "ppinkSav.txt")
        {
            using (StreamReader fileout = new StreamReader(fn, System.Text.Encoding.UTF8))
            {
                int j, l;
                Stroke stk = null;
                string st;

                st = fileout.ReadLine();
                if (!st.StartsWith("# ppInk"))
                    return;
                do
                {
                    st = fileout.ReadLine();
                }
                while (st != null && st.StartsWith("#"));
                while (st != null && st.StartsWith("ID"))
                {
                    do
                    {
                        st = fileout.ReadLine();
                    }
                    while (st.StartsWith("#"));
                    if (!st.StartsWith("pts"))
                        return;
                    j = st.IndexOf("=");
                    l = int.Parse(st.Substring(3, j - 3).Trim());
                    Point[] pts = new Point[l];
                    string[] sts = st.Substring(j + 1).TrimStart().Split(';');

                    for (int i = 0; i < l; i++)
                    {
                        string[] st3 = sts[i].Split(',');
                        pts[i].X = int.Parse(st3[0]);
                        pts[i].Y = int.Parse(st3[1]);
                    }
                    stk = IC.Ink.CreateStroke(pts);
                    do
                    {
                        st = fileout.ReadLine();
                    }
                    while (st.StartsWith("#"));
                    if (!st.StartsWith("DA"))
                        return;
                    j = st.IndexOf("R=") + 2;
                    l = st.IndexOf(",", j);
                    int R = int.Parse(st.Substring(j, l - j));
                    j = st.IndexOf("G=") + 2;
                    l = st.IndexOf(",", j);
                    int G = int.Parse(st.Substring(j, l - j));
                    j = st.IndexOf("B=") + 2;
                    l = st.IndexOf("]", j);
                    int B = int.Parse(st.Substring(j, l - j));
                    stk.DrawingAttributes.Color = Color.FromArgb(R, G, B);
                    j = st.IndexOf("T=") + 2;
                    l = st.IndexOf(",", j);
                    stk.DrawingAttributes.Transparency = byte.Parse(st.Substring(j, l - j));
                    stk.DrawingAttributes.FitToCurve = !st.Contains("NotFit");
                    j = st.IndexOf("W=") + 2;
                    l = st.Length;
                    stk.DrawingAttributes.Width = int.Parse(st.Substring(j, l - j));
                    do
                    {
                        st = fileout.ReadLine();
                    }
                    while (st.StartsWith("#"));
                    Guid guid;
                    while (st != "}")
                    {
                        j = st.IndexOf('%');
                        guid = new Guid(st.Substring(0, j));
                        j++;
                        l = st.IndexOf(':', j);
                        string st1 = st.Substring(j, l - j);
                        string st2 = st.Substring(l + 1);
                        object obj = null;
                        if (st.Contains("Int"))
                            obj = int.Parse(st2);
                        else if (st.Contains("Bool"))
                            obj = bool.Parse(st2);
                        else if (st.Contains("Single"))
                            obj = float.Parse(st2);
                        else if (st.Contains("String"))
                            obj = st2.Replace('\a', '\n');
                        stk.ExtendedProperties.Add(guid, obj);
                        do
                        {
                            st = fileout.ReadLine();
                        }
                        while (st.StartsWith("#"));
                    }
                    IC.Ink.Strokes.Add(stk);
                    do
                    {
                        st = fileout.ReadLine();
                    }
                    while (st != null && st.StartsWith("#"));
                }
            }
        }

        private void btLoad_Click(object sender, EventArgs e)
        {
            longClickTimer.Stop(); // for an unkown reason the mouse arrives later
            if (sender is ContextMenu)
            {
                sender = (sender as ContextMenu).SourceControl;
                MouseTimeDown = DateTime.FromBinary(0);
            }
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }
            if (sender != null && (DateTime.Now - MouseTimeDown).TotalSeconds > Root.LongClickTime)
            {
                if (SaveStrokeFile == "")
                    SaveStrokeFile = Path.GetFullPath(Environment.ExpandEnvironmentVariables(Root.SnapshotBasePath));
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = Path.GetDirectoryName(SaveStrokeFile);
                    openFileDialog.Filter = "strokes files(*.strokes.txt)|*.strokes.txt|All files (*.*)|*.*";
                    openFileDialog.FilterIndex = 1;
                    AllowInteractions(true);
                    DialogResult rst = openFileDialog.ShowDialog();
                    AllowInteractions(false);
                    if (rst == DialogResult.OK)
                    {
                        SaveStrokeFile = openFileDialog.FileName;
                        toolTip.SetToolTip(this.btLoad, string.Format(Root.Local.LoadStroke, Path.GetFileName(SaveStrokeFile).Replace(".stroke.txt", "")));
                        toolTip.SetToolTip(this.btSave, string.Format(Root.Local.SaveStroke, Path.GetFileName(SaveStrokeFile).Replace(".stroke.txt", "")));
                    }
                    else
                        return;
                }
            }
            if (SaveStrokeFile == "")
                LoadStrokes(Path.GetFullPath(Environment.ExpandEnvironmentVariables(Root.SnapshotBasePath + "AutoSave.strokes.txt")));
            else
                LoadStrokes(SaveStrokeFile);
            Root.UponAllDrawingUpdate = true;
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            longClickTimer.Stop(); // for an unkown reason the mouse arrives later
            if (sender is ContextMenu)
            {
                sender = (sender as ContextMenu).SourceControl;
                MouseTimeDown = DateTime.FromBinary(0);
            }
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }
            do
            {
                if ((sender != null && (DateTime.Now - MouseTimeDown).TotalSeconds > Root.LongClickTime) || SaveStrokeFile == "")
                {
                    string sav = SaveStrokeFile;
                    if (SaveStrokeFile == "")
                        SaveStrokeFile = Path.GetFullPath(Environment.ExpandEnvironmentVariables(Root.SnapshotBasePath));
                    using (SaveFileDialog FileDialog = new SaveFileDialog())
                    {
                        FileDialog.InitialDirectory = Path.GetDirectoryName(SaveStrokeFile);
                        FileDialog.Filter = "strokes files(*.strokes.txt)|*.strokes.txt|All files (*.*)|*.*";
                        FileDialog.FilterIndex = 1;
                        AllowInteractions(true);
                        DialogResult rst = FileDialog.ShowDialog();
                        AllowInteractions(false);
                        if (rst == DialogResult.OK)
                        {
                            SaveStrokeFile = FileDialog.FileName;
                            toolTip.SetToolTip(this.btLoad, string.Format(Root.Local.LoadStroke, Path.GetFileName(SaveStrokeFile).Replace(".stroke.txt", "")));
                            toolTip.SetToolTip(this.btSave, string.Format(Root.Local.SaveStroke, Path.GetFileName(SaveStrokeFile).Replace(".stroke.txt", "")));
                        }
                        else
                        {
                            SaveStrokeFile = sav;
                            return;
                        }
                    }
                }
            }
            while (!(!File.Exists(SaveStrokeFile) || MessageBox.Show(string.Format(Root.Local.StrokeFileExists, SaveStrokeFile), Root.Local.SaveStroke, MessageBoxButtons.OKCancel) == DialogResult.OK));
            try
            {
                SaveStrokes(SaveStrokeFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(SaveStrokeFile);
                string errorMsg = "Silent exception logged \r\n:" + ex.Message + "\r\n\r\nStack Trace:\r\n" + ex.StackTrace + "\r\n\r\n";
                Program.WriteErrorLog(errorMsg);
            };
        }




        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
        [DllImport("user32.dll")]
        public extern static bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetKeyState(int keyCode);

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll")]
        static extern bool ShowWindow(int hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetKeyboardState(byte[] lpKeyState);
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);
    }
}
