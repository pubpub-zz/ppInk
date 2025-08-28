using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Drawing.Imaging;
using System.Globalization;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;

namespace gInk
{
    using ListPoint = List<Point>;

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

        public Dictionary<int, AnimationStructure> Animations = new Dictionary<int, AnimationStructure>();
        public int AniPoolIdx;

        // Button/Tooblar
        /*const double NormSizePercent = 0.85;
        const double SmallSizePercent = 0.44;
        const double TopPercent = 0.06;
        const double SmallButtonNext = 0.44;
        const double InterButtonGap = NormSizePercent * .05;*/
        const double NormSizePercent = 0.96;
        const double SmallSizePercent = 0.47;
        const double TopPercent = 0.02;
        const double SmallButtonNext = 0.98 - .47;
        const double InterButtonGap = .02;


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
        public Bitmap image_dock, image_dockback;
        public Bitmap image_pointer, image_pointer_act;
        public Bitmap image_eraser_act, image_eraser;
        public Bitmap image_visible_not, image_visible;
        public Bitmap image_lasso_act, image_lasso;
        public System.Windows.Forms.Cursor cursorred, cursortarget, cursorsnap, cursorerase;
        public System.Windows.Forms.Cursor cursortip;
        public System.Windows.Forms.Cursor tempArrowCursor = null;
        public bool Initializing;

        public bool? oldShiftPensExtra = null;
        public int FirstPenDisplayed;

        public DateTime MouseTimeDown;
        public object MouseDownButtonObject;
        public int ButtonsEntering = 0;  // -1 = exiting
        public int gpButtonsLeft, gpButtonsTop, gpButtonsWidth, gpButtonsHeight; // the default location, fixed
        public Size VisibleToolbar = new Size();

        public bool gpPenWidth_MouseOn = false;
        public int gpSubTools_MouseOn = 0;

        public int PrimaryLeft, PrimaryTop;

        public int LastPenSelected = 0;
        public int SavedTool = -1;
        public int SavedFilled = -1;
        public int SavedPen = -1;
        private int PolyLineLastX = Int32.MinValue;
        private int PolyLineLastY = Int32.MinValue;
        private Stroke PolyLineInProgress = null;
        private bool FromHandToLineOnShift = false;

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
        private Object btClipSel;

        private List<Stroke> FadingList = new List<Stroke>();

        public ZoomForm ZoomForm = new ZoomForm();
        private Bitmap ZoomImage, ZoomImage2;
        int ZoomFormRePosX;
        int ZoomFormRePosY;
        string ZoomSaveStroke;
        public MouseButtons CurrentMouseButton = MouseButtons.None;

        public string SaveStrokeFile;
        public List<string> PointerModeSnaps = new List<string>();

        public Button[] Btn_SubTools;

        public ToolTip MetricToolTip = new ToolTip();

        public Strokes StrokesSelection, InprogressSelection;
        public bool AppendToSelection;

        public Stroke LineForPatterns = null;
        public int PatternLineSteps = -1;          //0 = getSize ; 1 = getDistance; 2 = getStroke
        public Bitmap PatternImage = null;
        public bool RotatingOnLine = false;
        public ListPoint PatternPoints = new List<Point>();
        public List<ListPoint> StoredPatternPoints = new List<ListPoint>();
        public int PatternLastPtIndex = -1;
        public double PatternLastPtRemain = 0;
        public double PatternDist = double.MaxValue;

        public List<Bitmap> StoredArrowImages = new List<Bitmap>();

        public int PageIndex = 0;
        public int PageMax = 0;

        public static double Measure2Scale = Root.Measure2Scale;



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
            string[] namesize = name.Split('%');
            float scale = 1.0F;
            System.Windows.Forms.Cursor curs = null;
            Bitmap bmp = null;
            int[] cursorsize = new int[2];
            int[] hotSpot = new int[2];
            if (namesize.Length > 1)
                scale = float.Parse(namesize[1], CultureInfo.InvariantCulture);
            try
            {
                {
                    string[] exts = { ".cur", ".ico" };
                    foreach (string ext in exts)
                    {
                        filename = Program.RunningFolder + namesize[0] + ext;
                        if (File.Exists(filename))
                            try
                            {
                                curs = new System.Windows.Forms.Cursor(filename);
                                hotSpot[0] = curs.HotSpot.X; hotSpot[1] = curs.HotSpot.Y;
                                // required to handle every cursor size such as 128x128
                                bmp = new Bitmap(filename);
                                cursorsize[0] = bmp.Width; cursorsize[1] = bmp.Height;
                                break;
                            }
                            catch (Exception e)
                            {
                                Program.WriteErrorLog(string.Format("File {0} found but can not be loaded:\n{1}\n", filename, e));
                            }
                    }
                }
                {
                    string[] exts = { ".ani" };
                    foreach (string ext in exts)
                    {
                        filename = Program.RunningFolder + namesize[0] + ext;
                        if (File.Exists(filename))
                            try
                            {
                                curs = new System.Windows.Forms.Cursor(filename);
                                return curs;
                            }
                            catch (Exception e)
                            {
                                Program.WriteErrorLog(string.Format("File {0} found but can not be loaded:\n{1}\n", filename, e));
                            }
                    }
                }
                {
                    string[] exts = { ".bmp", ".png", ".tif", ".jpg", ".jpeg" };
                    foreach (string ext in exts)
                    {
                        filename = Program.RunningFolder + namesize[0] + ext;
                        if (File.Exists(filename))
                            try
                            {
                                bmp = new Bitmap(filename);
                                cursorsize[0] = bmp.Width; cursorsize[1] = bmp.Height;
                                hotSpot[0] = bmp.Width / 2; hotSpot[1] = bmp.Height / 2;
                                try
                                {
                                    string fn1 = Path.GetFileNameWithoutExtension(namesize[0]);
                                    fn1 = fn1.Split('@')[1];
                                    string[] lst = fn1.Split('.');
                                    int dx = int.Parse(lst[0]);
                                    int dy = int.Parse(lst[1]);
                                    hotSpot[0] = dx;
                                    hotSpot[1] = dy;
                                }
                                catch
                                {
                                    ;
                                }
                            }
                            catch (Exception e)
                            {
                                Program.WriteErrorLog(string.Format("File {0} found but can not be loaded:\n{1}\n", filename, e));
                            }
                    }
                }
            }
            catch
            {
                return nocurs;
            }
            if (bmp == null)
            {
                curs = new System.Windows.Forms.Cursor(((System.Drawing.Icon)Properties.Resources.ResourceManager.GetObject(namesize[0])).Handle);
                cursorsize[0] = 128; cursorsize[1] = 128;
                hotSpot[0] = (int)(cursorsize[0] * (1.0 * curs.HotSpot.X) / curs.Size.Width); hotSpot[1] = (int)(cursorsize[1] * (1.0 * curs.HotSpot.Y) / curs.Size.Height);
                if (hotSpot[0] >= cursorsize[0] || hotSpot[1] >= cursorsize[1])
                {
                    hotSpot[0] = cursorsize[0] / 2;
                    hotSpot[1] = cursorsize[1] / 2;
                }
                bmp = new Bitmap(cursorsize[0], cursorsize[1], PixelFormat.Format32bppArgb);
                Graphics mg = Graphics.FromImage(bmp);
                curs.DrawStretched(mg, new Rectangle(0, 0, bmp.Width, bmp.Height));
                mg.Dispose();
            }
            if (bmp.PixelFormat != PixelFormat.Format32bppArgb)
            {
                bmp.MakeTransparent(bmp.GetPixel(0, 0));
            }
            Bitmap imgout = new Bitmap((int)Math.Round(cursorsize[0] * scale), (int)Math.Round(cursorsize[1] * scale), PixelFormat.Format32bppArgb);
            Graphics myGraphics = Graphics.FromImage(imgout);
            myGraphics.DrawImage(bmp, 0, 0, imgout.Width, imgout.Height);
            myGraphics.Dispose();
            return CreateCursorFromBitmap(imgout, (int)Math.Round(hotSpot[0] * scale), (int)Math.Round(hotSpot[1] * scale));
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
                    filename = Program.RunningFolder + name + ext;
                if (File.Exists(filename))
                    try
                    {
                        return new Bitmap(filename);
                    }
                    catch (Exception e)
                    {
                        Program.WriteErrorLog(string.Format("File {0} found but can not be loaded:{1} \n", filename, e));
                        return getImgFromDiskOrRes("unknown");
                    }
            }
            try
            {
                return new Bitmap((Bitmap)Properties.Resources.ResourceManager.GetObject(name));
            }
            catch
            {
                return getImgFromDiskOrRes("unknown");
            }
        }

        private double WidthForHalfDiag = 18.0 * global::gInk.Properties.Resources._null.Width * Math.Sqrt(2) / 2.0 / 300.0;
        private Bitmap BuildArrowBtn(string head, string tail, Color col)
        {
            Bitmap b = new Bitmap(global::gInk.Properties.Resources._null);
            Graphics g = Graphics.FromImage(b);

            g.CompositingQuality = CompositingQuality.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            int i, j;
            Bitmap b1 = PrepareArrowBitmap(head, col, 0, (int)Math.Round(WidthForHalfDiag), (float)(-225.0 * Math.PI / 180.0), out i);

            /*            g.DrawImage(b1, new Rectangle(b.Width / 2, 0, b.Width / 2, b.Height / 2), 0, b1.Height / 2, b1.Width / 2, b1.Height / 2,GraphicsUnit.Pixel);
                        b1.Dispose();
                        b1 = PrepareArrowBitmap(tail, col, 0, (int)Math.Round(WidthForHalfDiag), (float)(-45.0 * Math.PI / 180.0), out j);

                        g.DrawImage(b1, new Rectangle(0, b.Height / 2, b.Width / 2, b.Height / 2), b1.Width / 2, 0, b1.Width / 2, b1.Height / 2, GraphicsUnit.Pixel);
                        Pen p = new Pen(col,2);
                        g.DrawLine(p, (1F - .25F * i / 150.0F) * b.Width, ( .25F * i / 150.0F) * b.Height, (.25F * j / 150.0F) * b.Width, (1F - .25F * j / 150.0F) * b.Height);
            */
            g.DrawImage(b1, new Rectangle((int)(.375 * b.Width), 0, (int)(.75 * b.Width), (int)(.75 * b.Height)), 0, (int)(.375 * b1.Height), (int)(.75 * b1.Width), (int)(.75 * b1.Height), GraphicsUnit.Pixel);
            b1.Dispose();
            b1 = PrepareArrowBitmap(tail, col, 0, (int)Math.Round(WidthForHalfDiag), (float)(-45.0 * Math.PI / 180.0), out j);

            g.DrawImage(b1, new Rectangle(0, (int)(.375 * b.Height), (int)(.75 * b.Width), (int)(.75 * b.Height)), (int)(.375 * b1.Width), 0, (int)(.75 * b1.Width), (int)(.75 * b1.Height), GraphicsUnit.Pixel);
            Pen p = new Pen(col, 2);
            g.DrawLine(p, (1F - .25F * i / 150.0F) * b.Width, (.25F * i / 150.0F) * b.Height, (.25F * j / 150.0F) * b.Width, (1F - .25F * j / 150.0F) * b.Height);
            b1.Dispose();
            g.Dispose();
            return b;
        }

        private void SetButtonPosition(Button previous, Button current, int spacing, int Orient = -1)
        {
            if (Orient < Orientation.min)
                Orient = Root.ToolbarOrientation;

            if (Orient == Orientation.toLeft)
            {
                current.Left = previous.Left + previous.Width + spacing;
                current.Top = previous.Top;
            }
            else if (Orient == Orientation.toRight)
            {
                current.Left = previous.Left - spacing - current.Width;
                current.Top = previous.Top;
            }
            else if (Orient == Orientation.toDown)
            {
                current.Left = previous.Left;
                current.Top = previous.Top - spacing - current.Height;
            }
            else if (Orient == Orientation.toUp)
            {
                current.Left = previous.Left;
                current.Top = previous.Top + previous.Height + spacing;
            }
        }

        private void SetSmallButtonNext(Button previous, Button current, int incr, int Orient = -1)
        {
            if (Orient < Orientation.min)
                Orient = Root.ToolbarOrientation;

            if (Orient <= Orientation.Horizontal)
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

        public Bitmap buildPenIcon(Color col, int transparency, bool Sel, bool Fading, string LineStyle = "Stroke", float width = 100.0F)
        {
            Bitmap fg, img, Overlay;
            ImageAttributes imageAttributes = new ImageAttributes();
            bool Highlighter = transparency >= 100;
            bool Large = width >= (Root.PenWidthNormal + Root.PenWidthThick) / 2;

            float[][] colorMatrixElements = {
                       new float[] {col.R/255.0f,  0,  0,  0, 0},
                       new float[] {0,  col.G / 255.0f,  0,  0, 0},
                       new float[] {0,  0,  col.B / 255.0f,  0, 0},
                       new float[] {0,  0,  0,  (255-transparency) / 255.0f, 0},
                       new float[] {0,  0,  0,     0,  1}};
            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            img = getImgFromDiskOrRes((Highlighter ? "Lpen" : (Large ? "PRpen" : "pen")) + (Sel ? "S" : "") + "_bg", ImageExts);
            fg = getImgFromDiskOrRes((Highlighter ? "Lpen" : (Large ? "PRpen" : "pen")) + (Sel ? "S" : "") + "_col", ImageExts);

            Graphics g = Graphics.FromImage(img);
            g.DrawImage(fg, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imageAttributes);

            Overlay = getImgFromDiskOrRes("fadingTag", ImageExts);
            if (Fading)
                g.DrawImage(Overlay, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);

            Overlay.Dispose();
            Overlay = getImgFromDiskOrRes(LineStyle + "LSTag", ImageExts);
            g.DrawImage(Overlay, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
            Overlay.Dispose();
            fg.Dispose();
            return img;
        }

        public struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);
        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        /// Create a cursor from a bitmap, with the hot spot in the middle
        public static System.Windows.Forms.Cursor CreateCursorFromBitmap(Bitmap bmp, int hotX = -1, int hotY = -1)
        {
            IntPtr ptr = (bmp).GetHicon();
            IconInfo tmp = new IconInfo();
            GetIconInfo(ptr, ref tmp);
            tmp.xHotspot = hotX >= 0 ? hotX : (bmp.Width / 2);
            tmp.yHotspot = hotY >= 0 ? hotY : (bmp.Height / 2);
            tmp.fIcon = false;
            ptr = CreateIconIndirect(ref tmp);
            System.Windows.Forms.Cursor cu = new System.Windows.Forms.Cursor(ptr);
            cu.Tag = 2;
            return cu;
        }

        public Bitmap buildColorPicker(Color col, int transparency)
        {
            Bitmap img, dest;
            ImageAttributes imageAttributes = new ImageAttributes();

            float[][] colorMatrixElements = {
                       new float[] {col.R/255.0f,  0,  0,  0, 0},
                       new float[] {0,  col.G / 255.0f,  0,  0, 0},
                       new float[] {0,  0,  col.B / 255.0f,  0, 0},
                       new float[] {0,  0,  0, (255 - transparency) / 255.0f, 0},
                       new float[] {0,  0,  0,     0,  1}};
            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            img = getImgFromDiskOrRes("picker", ImageExts);
            dest = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppPArgb);
            Graphics g = Graphics.FromImage(dest);
            g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imageAttributes);
            img.Dispose();
            return dest;
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

            //Console.WriteLine("B=" + (DateTime.Now.Ticks/1e7).ToString());
            ClipartsDlg = new ImageLister(Root);
            Initializing = true;

            int nbPen = 0;
            for (int b = 0; b < Root.MaxDisplayedPens; b++)
                if (Root.PenEnabled[b])
                    nbPen++;
            btPen = new Button[Root.MaxPenCount];

            for (int b = 0; b < Root.MaxDisplayedPens; b++)
            {
                btPen[b] = new Button();
                btPen[b].Name = string.Format("pen{0}", b);
                btPen[b].FlatAppearance.BorderSize = 0;
                btPen[b].FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
                btPen[b].FlatStyle = System.Windows.Forms.FlatStyle.Flat;

                btPen[b].ContextMenu = new ContextMenu();
                btPen[b].ContextMenu.Popup += new System.EventHandler(btColor_Click);
                btPen[b].Click += new System.EventHandler(btColor_Click);

                btPen[b].BackColor = System.Drawing.Color.Transparent;
                btPen[b].BackgroundImageLayout = ImageLayout.Stretch;
                this.toolTip.SetToolTip(this.btPen[b], Root.Local.ButtonNamePen[b] + " (" + Root.Hotkey_Pens[b].ToString() + ")");

                btPen[b].MouseDown += gpButtons_MouseDown;
                btPen[b].MouseMove += gpButtons_MouseMove;
                btPen[b].MouseUp += gpButtons_MouseUp;

                gpButtons.Controls.Add(btPen[b]);
            }

            IC = new InkOverlay(this.Handle);
            Console.WriteLine("Module of IC " + IC.GetType().Module.FullyQualifiedName);
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

            StrokesSelection = IC.Ink.CreateStrokes();

            foreach (Control ct in gpButtons.Controls)
            {
                if (ct.GetType() == typeof(Button))
                {
                    ct.MouseDown += new MouseEventHandler(this.btAllButtons_MouseDown);
                    ct.MouseUp += new MouseEventHandler(this.btAllButtons_MouseUp);
                    ct.ContextMenu = new ContextMenu();
                    ct.ContextMenu.Popup += new EventHandler(this.btAllButtons_RightClick);
                }
            }
            PenModifyDlg = new PenModifyDlg(Root); // It seems to be a little long to build so we prepare it.

            Btn_SubTools = new Button[] { Btn_SubTool0, Btn_SubTool1, Btn_SubTool2, Btn_SubTool3, Btn_SubTool4, Btn_SubTool5, Btn_SubTool6, Btn_SubTool7 };

            ClipartsDlg.Initialize();
            Initialize();
        }

        public double ConvertMeasureLength(double hl)
        {
            return hl * 0.037795280352161 * Measure2Scale;
        }


        private string MemoHintClose;
        private string MemoHintDock;

        public void Initialize()
        {

            if (Root.FormOptions?.Visible ?? false)
            {
                // this is to validate the active field if the options are open. Not the best solution but nothing else found 
                Root.FormOptions.Close();
                Root.FormOptions.Show();
            }

            Console.WriteLine("A=" + (DateTime.Now.Ticks / 1e7).ToString());

            MeasureNumberFormat = (NumberFormatInfo)NumberFormatInfo.CurrentInfo.Clone();
            MeasureNumberFormat.NumberDecimalDigits = Root.Measure2Digits;
            Measure2Scale = Root.Measure2Scale;

            for (int i = 0; i < StoredArrowImages.Count; i++)
                try { StoredArrowImages[i].Dispose(); } catch { }
            StoredArrowImages.Clear();

            Root.Snapping = 0;
            Root.ColorPickerMode = false;
            Root.PenAttr[Root.SavedPenDA] = null;
            StrokesSelection.Clear();
            FadingList.Clear();

            Animations.Clear();
            AniPoolIdx = 0;

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

            try
            {
                ZoomImage?.Dispose();
            }
            catch { }
            finally
            {
                ZoomImage = new Bitmap(Root.ZoomWidth, Root.ZoomHeight);
            }
            try
            {
                ZoomImage2?.Dispose();
            }
            catch { }
            finally
            {
                ZoomImage2 = new Bitmap(Root.ZoomWidth, Root.ZoomHeight);
            }
            ZoomForm.pictureBox1.BackgroundImage = ZoomImage;
            ZoomForm.pictureBox2.BackgroundImage = ZoomImage2;
            ZoomFormRePosX = ZoomImage.Width / 2;
            ZoomFormRePosY = ZoomImage.Height / 2;
            ZoomForm.Width = (int)(Root.ZoomWidth * Root.ZoomScale);
            ZoomForm.Height = (int)(Root.ZoomHeight * Root.ZoomScale);
            ZoomSaveStroke = Path.GetFullPath(Environment.ExpandEnvironmentVariables("%temp%/ZoomSave.strokes.txt")).Replace('\\', '/');

            //ClipartsDlg.Initialize();

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
            gpSubTools.BackColor = Color.FromArgb(Root.ToolbarBGColor[0], Root.ToolbarBGColor[1], Root.ToolbarBGColor[2], Root.ToolbarBGColor[3]);

            longClickTimer.Interval = (int)(Root.LongClickTime * 1000 + 100);

            //usefull? this.DoubleBuffered = true;

            int nbPen = 0;
            for (int b = 0; b < Root.MaxDisplayedPens; b++)
                if (Root.PenEnabled[b])
                    nbPen++;

            FirstPenDisplayed = 0;
            while (!Root.PenEnabled[FirstPenDisplayed])
                FirstPenDisplayed++;
            oldShiftPensExtra = null;

            // set dimensions and positions 
            int dim = (int)Math.Round(Screen.PrimaryScreen.Bounds.Height * Root.ToolbarHeight);
            int dim1 = (int)(dim * NormSizePercent);
            int dim1s = (int)(dim * SmallSizePercent);
            int dim2 = (int)(dim * TopPercent);
            int dim2s = (int)(dim * SmallButtonNext);
            int dim3 = (int)(dim * InterButtonGap);
            int dim4 = dim1 + dim3;
            int dim4s = dim1s + dim3;

            int penSec = Root.PensOnTwoLines ? ((int)Math.Ceiling(nbPen / 2.0) * dim4s) : (nbPen * dim4);
            if (Root.ToolbarOrientation <= Orientation.Horizontal)
            {
                gpButtons.Height = dim;
                gpButtons.Width = (int)((dim1 * .5 + dim3) + (penSec + (Root.PensExtraSet ? (dim4 / 6) : 0) + (Root.ToolsEnabled ? (6 * dim4s) : 0) + (Root.EraserEnabled ? dim4 : 0) + (Root.PanEnabled ? 2 * dim4s : 0)
                                                                     + (Root.PointerEnabled ? dim4 : 0) + (Root.PenWidthEnabled ? dim4 : 0) + (Root.InkVisibleEnabled ? dim4 : 0) + (Root.ZoomEnabled > 0 ? dim4s : 0)
                                                                     + (Root.SnapEnabled ? dim4 : 0) + (Root.UndoEnabled ? dim4 : 0) + (Root.ClearEnabled ? dim4 : 0)
                                                                     + (Root.PagesEnabled ? dim4s : 0) + (Root.LoadSaveEnabled ? dim4s : 0)
                                                                     + ((Root.VideoRecordMode != VideoRecordMode.NoVideo) ? dim4 : 0)
                                                                     + dim1));
            }
            else //Vertical
            {
                gpButtons.Width = dim;
                gpButtons.Height = (int)((dim1 * .5 + dim3) + (penSec + (Root.PensExtraSet ? (dim4 / 6) : 0) + (Root.ToolsEnabled ? (6 * dim4s) : 0) + (Root.EraserEnabled ? dim4 : 0) + (Root.PanEnabled ? 2 * dim4s : 0)
                                                                      + (Root.PointerEnabled ? dim4 : 0) + (Root.PenWidthEnabled ? dim4 : 0) + (Root.InkVisibleEnabled ? dim4 : 0) + (Root.ZoomEnabled > 0 ? dim4s : 0)
                                                                      + (Root.SnapEnabled ? dim4 : 0) + (Root.UndoEnabled ? dim4 : 0) + (Root.ClearEnabled ? dim4 : 0)
                                                                      + (Root.PagesEnabled ? dim4s : 0) + (Root.LoadSaveEnabled ? dim4s : 0)
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

            bool NextBelow = false;
            for (int b = 0; b < Root.MaxDisplayedPens; b++)
            {

                if (Root.PenEnabled[b])
                {
                    if (Root.PensOnTwoLines)
                    {
                        btPen[b].Width = dim1s;
                        btPen[b].Height = dim1s;

                        if (NextBelow)
                        {
                            SetSmallButtonNext(prev, btPen[b], dim2s);
                            NextBelow = false;
                        }
                        else
                        {
                            SetButtonPosition(prev, btPen[b], dim3);
                            prev = btPen[b];
                            NextBelow = true;
                        }
                    }
                    else
                    {
                        btPen[b].Width = dim1;
                        btPen[b].Height = dim1;

                        SetButtonPosition(prev, btPen[b], dim3);
                        prev = btPen[b];
                    }

                    toolTip.SetToolTip(btPen[b], Root.Local.ButtonNamePen[b] + " (" + Root.Hotkey_Pens[b].ToString() + ")");
                    btPen[b].Visible = true;
                }
                else
                    btPen[b].Visible = false;
            }

            if (Root.PensExtraSet)
            {
                btExtraPens.Visible = true;
                if(Root.ToolbarOrientation == Orientation.toUp || Root.ToolbarOrientation == Orientation.toDown)
                {
                    btExtraPens.Height = dim1/6;
                    btExtraPens.Width = dim1;
                    btExtraPens.BackgroundImage = getImgFromDiskOrRes("ExtraPensV");
                }
                else
                {
                    btExtraPens.Height = dim1;
                    btExtraPens.Width = dim1 / 6;
                    btExtraPens.BackgroundImage = getImgFromDiskOrRes("ExtraPens");
                }
                SetButtonPosition(prev, btExtraPens, dim3);
                prev = btExtraPens;
            }
            else
                btExtraPens.Visible = false;

            if (Root.ToolsEnabled)
            {
                // background images loaded/applied in SelectTool
                btHand.Height = dim1s;
                btHand.Width = dim1s;
                btHand.Visible = true;
                SetButtonPosition(prev, btHand, dim3);
                btLine.Height = dim1s;
                btLine.Width = dim1s;
                btLine.Visible = true;
                SetSmallButtonNext(btHand, btLine, dim2s);

                btRect.Height = dim1s;
                btRect.Width = dim1s;
                btRect.Visible = true;
                SetButtonPosition(btHand, btRect, dim3);
                btOval.Height = dim1s;
                btOval.Width = dim1s;
                btOval.Visible = true;
                SetSmallButtonNext(btRect, btOval, dim2s);

                btArrow.Height = dim1s;
                btArrow.Width = dim1s;
                btArrow.Visible = true;
                SetButtonPosition(btRect, btArrow, dim3);
                btNumb.Height = dim1s;
                btNumb.Width = dim1s;
                btNumb.Visible = true;
                SetSmallButtonNext(btArrow, btNumb, dim2s);

                btText.Height = dim1s;
                btText.Width = dim1s;
                btText.Visible = true;
                SetButtonPosition(btArrow, btText, dim3);

                btClipArt.Height = dim1s;
                btClipArt.Width = dim1s;
                btClipArt.Visible = true;
                btClipArt.Text = "";
                btClipArt.Font = new Font(btClipArt.Font.Name, dim1s * .5F, btClipArt.Font.Style);
                SetButtonPosition(btText, btClipArt, dim3);
                btClip1.Height = dim1s;
                btClip1.Width = dim1s;
                btClip1.Visible = true;
                btClip1.Text = "";
                btClip1.Font = btClipArt.Font;
                SetSmallButtonNext(btClipArt, btClip1, dim2s);
                try
                {
                    if ((btClip1.Tag as ClipArtData)?.ImageStamp != Root.ImageStamp1.ImageStamp)
                    {
                        btClip1.BackgroundImage.Dispose();
                        throw (new Exception("Renew button"));
                    }
                }
                catch
                {
                    btClip1.BackgroundImage = getImgFromDiskOrRes(Root.ImageStamp1.ImageStamp, ImageExts);
                    //btClip1.Tag = new ClipArtData { ImageStamp = Root.ImageStamp1.ImageStamp, X=Root.ImageStamp1.Wstored, Y = Root.ImageStamp1.Hstored, Filling = Root.ImageStamp1.Filling };
                    btClip1.Tag = Root.ImageStamp1.Clone();
                }
                btClip2.Height = dim1s;
                btClip2.Width = dim1s;
                btClip2.Visible = true;
                btClip2.Text = "";
                btClip2.Font = btClipArt.Font;
                SetButtonPosition(btClipArt, btClip2, dim3);
                try
                {
                    if ((btClip2.Tag as ClipArtData)?.ImageStamp != Root.ImageStamp2.ImageStamp)
                    {
                        btClip2.BackgroundImage.Dispose();
                        throw (new Exception("Renew button"));
                    }
                }
                catch
                {
                    btClip2.BackgroundImage = getImgFromDiskOrRes(Root.ImageStamp2.ImageStamp, ImageExts);
                    //btClip2.Tag = new ClipArtData { ImageStamp = Root.ImageStamp2.ImageStamp, X = Root.ImageStamp2.Wstored, Y = Root.ImageStamp2.Wstored, Filling = Root.ImageStamp2.Filling };
                    btClip2.Tag = Root.ImageStamp2.Clone();
                }
                btClip3.Height = dim1s;
                btClip3.Width = dim1s;
                btClip3.Visible = true;
                btClip3.Text = "";
                btClip3.Font = btClipArt.Font;
                SetSmallButtonNext(btClip2, btClip3, dim2s);
                try
                {
                    if ((btClip3.Tag as ClipArtData)?.ImageStamp != Root.ImageStamp3.ImageStamp)
                    {
                        btClip3.BackgroundImage.Dispose();
                        throw (new Exception("Renew button"));
                    }
                }
                catch
                {
                    btClip3.BackgroundImage = getImgFromDiskOrRes(Root.ImageStamp3.ImageStamp, ImageExts);
                    //btClip3.Tag = new ClipArtData { ImageStamp = Root.ImageStamp3.ImageStamp, X = Root.ImageStamp3.Wstored, Y = Root.ImageStamp3.Wstored, Filling = Root.ImageStamp3.Filling};
                    btClip3.Tag = Root.ImageStamp3.Clone();
                }
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

                btClipArt.Visible = false;
                btClip1.Visible = false;
                btClip2.Visible = false;
                btClip3.Visible = false;
            }

            if (Root.EraserEnabled)
            {
                btEraser.Height = dim1;
                btEraser.Width = dim1;
                btEraser.Visible = true;
                image_eraser_act = getImgFromDiskOrRes("eraser_act", ImageExts);
                image_eraser = getImgFromDiskOrRes("eraser", ImageExts);
                btEraser.BackgroundImage = image_eraser;
                SetButtonPosition(prev, btEraser, dim3);
                prev = btEraser;
            }
            else
                btEraser.Visible = false;

            if (Root.PanEnabled)
            {
                btLasso.Height = dim1s;
                btLasso.Width = dim1s;
                btLasso.Visible = true;
                image_lasso_act = getImgFromDiskOrRes("lasso_act", ImageExts);
                image_lasso = getImgFromDiskOrRes("lasso", ImageExts);
                btLasso.BackgroundImage = image_lasso;
                SetButtonPosition(prev, btLasso, dim3);
                prev = btLasso;

                btPan.Height = dim1s;
                btPan.Width = dim1s;
                btPan.Visible = true;
                btPan.BackgroundImage = getImgFromDiskOrRes("pan", ImageExts);
                SetSmallButtonNext(prev, btPan, dim2s);

                btEdit.Height = dim1s;
                btEdit.Width = dim1s;
                btEdit.Visible = true;
                SetButtonPosition(prev, btEdit, dim3);
                prev = btEdit;

                btScaleRot.Height = dim1s;
                btScaleRot.Width = dim1s;
                btScaleRot.Visible = true;
                btScaleRot.BackgroundImage = getImgFromDiskOrRes("scale", ImageExts);
                SetSmallButtonNext(prev, btScaleRot, dim2s);
            }
            else
            {
                btLasso.Visible = false;
                btPan.Visible = false;
                btEdit.Visible = false;
                btScaleRot.Visible = false;
            }

            if (Root.PointerEnabled)
            {
                btPointer.Height = dim1;
                btPointer.Width = dim1;
                btPointer.Visible = true;
                image_pointer = getImgFromDiskOrRes("pointer", ImageExts);
                image_pointer_act = getImgFromDiskOrRes("pointer_act", ImageExts);
                SetButtonPosition(prev, btPointer, dim3);
                prev = btPointer;
            }
            else
                btPointer.Visible = false;

            if (Root.ZoomEnabled > 0)
            {
                btMagn.Height = dim1s;
                btMagn.Width = dim1s;
                btMagn.Visible = true;
                this.btMagn.BackgroundImage = getImgFromDiskOrRes((Root.MagneticRadius > 0) ? "Magnetic_act" : "Magnetic", ImageExts);
                SetButtonPosition(prev, btMagn, dim3);
                prev = btMagn;

                btZoom.Height = dim1s;
                btZoom.Width = dim1s;
                btZoom.Visible = true;
                btZoom.BackgroundImage = getImgFromDiskOrRes("Zoom", ImageExts);
                SetSmallButtonNext(btMagn, btZoom, dim2s);
                btZoom.Visible = true;
            }
            else
            {
                btMagn.Visible = false;
                btZoom.Visible = false;
            }

            if (Root.PenWidthEnabled)
            {
                btPenWidth.Height = dim1;
                btPenWidth.Width = dim1;
                btPenWidth.Visible = true;
                btPenWidth.BackgroundImage = getImgFromDiskOrRes("penwidth", ImageExts);
                SetButtonPosition(prev, btPenWidth, dim3);
                prev = btPenWidth;
            }
            else
                btPenWidth.Visible = false;

            if (Root.InkVisibleEnabled)
            {
                btInkVisible.Visible = true;
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

            if (Root.SnapEnabled)
            {
                btSnap.Visible = true;
                btSnap.Height = dim1;
                btSnap.Width = dim1;
                btSnap.BackgroundImage = getImgFromDiskOrRes("snap", ImageExts); ;
                SetButtonPosition(prev, btSnap, dim3);
                prev = btSnap;
            }
            else
                btSnap.Visible = false;

            if (Root.UndoEnabled)
            {
                btUndo.Visible = true;
                btUndo.Height = dim1;
                btUndo.Width = dim1;
                btUndo.BackgroundImage = getImgFromDiskOrRes("undo", ImageExts);
                SetButtonPosition(prev, btUndo, dim3);
                prev = btUndo;
            }
            else
                btUndo.Visible = false;

            if (Root.ClearEnabled)
            {
                btClear.Visible = true;
                btClear.Height = dim1;
                btClear.Width = dim1;
                btClear.BackgroundImage = getImgFromDiskOrRes("garbage", ImageExts);
                SetButtonPosition(prev, btClear, dim3);
                prev = btClear;
            }
            else
                btClear.Visible = false;

            if (Root.PagesEnabled)
            {
                btPagePrev.Height = dim1s;
                btPagePrev.Width = dim1s;
                btPagePrev.Visible = true;
                btPagePrev.BackgroundImage = getImgFromDiskOrRes("PagePrev", ImageExts);
                SetButtonPosition(prev, btPagePrev, dim3);
                btPageNext.Height = dim1s;
                btPageNext.Width = dim1s;
                btPageNext.Visible = true;
                btPageNext.BackgroundImage = getImgFromDiskOrRes("PageNext", ImageExts);
                SetSmallButtonNext(btPagePrev, btPageNext, dim2s);
                prev = btPagePrev;
            }
            else
            {
                btPagePrev.Visible = false;
                btPageNext.Visible = false;
            }

            if (Root.LoadSaveEnabled)
            {
                btSave.Height = dim1s;
                btSave.Width = dim1s;
                btSave.Visible = true;
                btSave.BackgroundImage = getImgFromDiskOrRes("save", ImageExts);
                SetButtonPosition(prev, btSave, dim3);
                btLoad.Height = dim1s;
                btLoad.Width = dim1s;
                btLoad.Visible = true;
                btLoad.BackgroundImage = getImgFromDiskOrRes("open", ImageExts);
                SetSmallButtonNext(btSave, btLoad, dim2s);
                prev = btSave;
            }
            else
            {
                btSave.Visible = false;
                btLoad.Visible = false;
            }

            if (Root.VideoRecordMode != VideoRecordMode.NoVideo)
            {
                btVideo.Height = dim1;
                btVideo.Width = dim1;
                btVideo.Visible = true;
                SetButtonPosition(prev, btVideo, dim3);
                SetVidBgImage();
                if (Root.VideoRecordMode == VideoRecordMode.OBSBcst || Root.VideoRecordMode == VideoRecordMode.OBSRec)
                {

                    if (Root.ObsRecvTask == null || Root.ObsRecvTask.IsCompleted)
                    {
                        Root.VideoRecordWindowInProgress = true;
                        try
                        {
                            Root.ObsRecvTask.Dispose();
                        }
                        catch { }
                        finally
                        {
                            Root.ObsRecvTask = Task.Run(() => ReceiveObsMesgs(this));
                        }
                    }
                    while (Root.VideoRecordWindowInProgress)
                        Task.Delay(50);
                    Task.Delay(100);
                    if (Root.VideoRecordMode == VideoRecordMode.OBSRec)
                        Task.Run(() => SendInWs(Root.ObsWs, "GetRecordingStatus", new CancellationToken()));
                    else
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

            pboxPenWidthIndicator.Top = 0;
            pboxPenWidthIndicator.Left = (int)Math.Sqrt(Root.GlobalPenWidth * 30.0F);
            gpPenWidth.Controls.Add(pboxPenWidthIndicator);

            tempArrowCursor = null;
            try
            {
                cursorred?.Dispose();
            }
            catch { }
            finally
            {
                cursorred = getCursFromDiskOrRes(Root.cursorarrowFileName, System.Windows.Forms.Cursors.NoMove2D);
            }
            try
            {
                cursortarget?.Dispose();
            }
            catch { }
            finally
            {
                cursortarget = getCursFromDiskOrRes(Root.cursortargetFileName, System.Windows.Forms.Cursors.SizeNWSE);
            }
            try
            {
                cursorerase?.Dispose();
            }
            catch { }
            finally
            {
                cursorerase = getCursFromDiskOrRes(Root.cursoreraserFileName, System.Windows.Forms.Cursors.No);
            }

            try
            {
                cursorsnap?.Dispose();
            }
            catch { }
            finally
            {
                cursorsnap = getCursFromDiskOrRes(Root.cursorsnapFileName, System.Windows.Forms.Cursors.Cross);
            }

            IC.Ink.Strokes.Clear();
            IC.Enabled = true;

            LastTickTime = DateTime.Parse("1987-01-01");
            tiSlide.Enabled = true;

            MemoHintDock = Root.Local.ButtonNameDock + " (" + Root.Hotkey_DockUndock.ToString() + ")";
            this.toolTip.SetToolTip(this.btDock, MemoHintDock);
            this.toolTip.SetToolTip(this.btExtraPens, Root.Local.ExtraPensHint);
            this.toolTip.SetToolTip(this.btPenWidth, Root.Local.ButtonNamePenwidth);
            this.toolTip.SetToolTip(this.btEraser, Root.Local.ButtonNameErasor + " (" + Root.Hotkey_Eraser.ToString() + ")");
            this.toolTip.SetToolTip(this.btPan, Root.Local.ButtonNamePan + " (" + Root.Hotkey_Pan.ToString() + ")");
            this.toolTip.SetToolTip(this.btScaleRot, Root.Local.ButtonNameScaleRotate + " (" + Root.Hotkey_ScaleRotate.ToString() + ")");
            this.toolTip.SetToolTip(this.btPointer, Root.Local.ButtonNameMousePointer + " (" + Root.Hotkey_Global.ToString() + ")");
            this.toolTip.SetToolTip(this.btInkVisible, Root.Local.ButtonNameInkVisible + " (" + Root.Hotkey_InkVisible.ToString() + ")");
            this.toolTip.SetToolTip(this.btSnap, Root.Local.ButtonNameSnapshot + " (" + Root.Hotkey_Snap.ToString() + ")");
            this.toolTip.SetToolTip(this.btUndo, Root.Local.ButtonNameUndo + " (" + Root.Hotkey_Undo.ToString() + ")");
            this.toolTip.SetToolTip(this.btClear, Root.Local.ButtonNameClear + " (" + Root.Hotkey_Clear.ToString() + ")");
            this.toolTip.SetToolTip(this.btVideo, Root.Local.ButtonNameVideo + " (" + Root.Hotkey_Video.ToString() + ")");
            MemoHintClose = Root.Local.ButtonNameExit + " (" + Root.Hotkey_Close.ToString() + "/Alt+F4)";
            this.toolTip.SetToolTip(this.btStop, MemoHintClose);
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
            this.toolTip.SetToolTip(this.btPagePrev, String.Format(Root.Local.ButtonPageNextPrev, ""));
            this.toolTip.SetToolTip(this.btPageNext, String.Format(Root.Local.ButtonPageNextPrev, ""));
            this.toolTip.SetToolTip(this.btSave, String.Format(Root.Local.SaveStroke, ""));
            this.toolTip.SetToolTip(this.btLoad, String.Format(Root.Local.LoadStroke, ""));
            this.toolTip.SetToolTip(this.btLasso, Root.Local.ButtonNameLasso + " (" + Root.Hotkey_Lasso.ToString() + ")");

            if (Root.ToolbarOrientation <= Orientation.Horizontal)
            {
                gpSubTools.Height = dim;
                gpSubTools.Width = dim1 * 8 + dim3 * 8 + dim1s;
            }
            else
            {
                gpSubTools.Width = dim;
                gpSubTools.Height = dim1 * 8 + dim3 * 8 + dim1s;
            }
            gpPenWidth.Height = dim;
            setPenWidthBarPosition();

            Btn_SubTool0.Height = dim1;
            Btn_SubTool0.Width = dim1;
            int o;
            if ((Root.ToolbarOrientation == Orientation.toLeft) || (Root.ToolbarOrientation == Orientation.toRight))
            {
                Btn_SubTool0.Top = dim2;
                Btn_SubTool0.Left = 0;
                o = Orientation.toLeft;
            }
            else
            {
                Btn_SubTool0.Top = 0;
                Btn_SubTool0.Left = dim2;
                o = Orientation.toUp;
            }
            prev = Btn_SubTool0;
            for (int i = 1; i < Btn_SubTools.Length; i++)
            {
                Btn_SubTools[i].Width = dim1;
                Btn_SubTools[i].Height = dim1;
                SetButtonPosition(prev, Btn_SubTools[i], dim3, o);
                prev = Btn_SubTools[i];
            }
            Btn_SubToolClose.Height = dim1s;
            Btn_SubToolClose.Width = dim1s;
            SetButtonPosition(prev, Btn_SubToolClose, dim3, o);
            Btn_SubToolPin.Height = dim1s;
            Btn_SubToolPin.Width = dim1s;
            SetSmallButtonNext(Btn_SubToolClose, Btn_SubToolPin, dim2s, o);

            ToTransparent();
            ToTopMost();
            StopAllZooms();
            Root.PointerMode = true; // will be set to false within SelectPen(0) below
            SelectPen(0);
            IC.DefaultDrawingAttributes.Width = Root.PenAttr[0].Width; //required to ensure width
            SelectTool(Tools.Hand, Filling.Empty); // Select Hand Drawing by Default

            SaveStrokeFile = "";

            PatternLineSteps = -1;
            LineForPatterns = null;
            PatternLastPtIndex = -1;
            PatternLastPtRemain = 0;
            PatternPoints.Clear();
            StoredPatternPoints.Clear();

            PageIndex = 0;
            PageMax = 0;

            Console.WriteLine("C=" + (DateTime.Now.Ticks / 1e7).ToString());
        }

        private void SetSubBarPosition(Panel Tb, Button RefButton)
        {
            if (Root.ToolbarOrientation <= Orientation.Horizontal)
            {
                Tb.Left = gpButtonsLeft + RefButton.Left; // gpButtonsLeft + btPenWidth.Left - gpPenWidth.Width / 2 + btPenWidth.Width / 2;
                Tb.Top = gpButtonsTop - Tb.Height - 10;
                if (!(IsInsideVisibleScreen(Tb.Left, Tb.Top) && IsInsideVisibleScreen(Tb.Right, Tb.Bottom)))
                    Tb.Top = gpButtonsTop + gpButtonsHeight + 10;
            }
            else
            {
                Tb.Top = gpButtonsTop + RefButton.Top;
                Tb.Left = gpButtonsLeft - Tb.Width - 10; // gpButtonsLeft + btPenWidth.Left - gpPenWidth.Width / 2 + btPenWidth.Width / 2;
                if (!(IsInsideVisibleScreen(Tb.Left, Tb.Top) && IsInsideVisibleScreen(Tb.Right, Tb.Bottom)))
                    Tb.Left = gpButtonsLeft + gpButtonsWidth + 10;
            }
        }

        private void setPenWidthBarPosition()
        {
            SetSubBarPosition(gpPenWidth, btPenWidth);
        }

        private void setClipArtDlgPosition()
        {
            if (Root.Docked)
            {
                ClipartsDlg.Left = Screen.PrimaryScreen.Bounds.Right - ClipartsDlg.Width - 1;
                ClipartsDlg.Top = Screen.PrimaryScreen.Bounds.Bottom - ClipartsDlg.Height - 1;
            }
            else if (Root.ToolbarOrientation <= Orientation.Horizontal)
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
            if (ButtonsEntering != 0 || DateTime.Now <= Root.PointerChangeDate)
            {
                return;
            }

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
                if (Initializing || AddM3UEntryInProgress)        // This is normally because we have not yet finish initialisation, we ignore the action...
                    return;
                //tests showed that this is useless
                /*if (Control.MouseButtons == MouseButtons.Right)
                {
                    Root.FormCollection.AddM3UEntry();
                    return;
                }*/
                if (Root.FormDisplay != null && Root.FormDisplay.Visible)
                {
                    //Console.WriteLine(Root.FormDisplay.HasFocus() ? "WM_ACT" : "!WM");
                    Root.FormDisplay.DrawBorder(Root.FormDisplay.HasFocus());
                    Root.FormDisplay.UpdateFormDisplay(true);
                }
                if (Root.FormDisplay == null || !Root.FormDisplay.Visible)
                    return;

                if (!Root.AltTabPointer || DateTime.Now < Root.PointerChangeDate)
                    return;

                if (msg.WParam == IntPtr.Zero)      // WParam = 0 => losing Focus ; WParam = 1 => Getting Focus
                {
                    //Console.WriteLine("desactivating " + Root.PointerMode.ToString());
                    if (Root.EraseOnLoosingFocus)
                    {
                        Root.ClearInk();
                    }
                    Root.Snapping = 0;
                    Root.ColorPickerMode = false;
                    if (!Root.PointerMode)
                    {
                        //Console.WriteLine("process ");
                        SavedTool = Root.ToolSelected;
                        SavedFilled = Root.FilledSelected;

                        SelectPen(-2);
                        Root.Dock();
                    }
                    return;
                }
                else
                {
                    if (Root.PointerMode)
                        AltTabActivate();
                    return;
                }
            }
            base.WndProc(ref msg);
        }

        private void SetVidBgImage()
        {
            if (Root.VideoRecInProgress == VideoRecInProgress.Dead)
                btVideo.BackgroundImage = getImgFromDiskOrRes("VidDead", ImageExts);
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
                if (Root.ColorPickerMode)
                {
                    int i = Root.PickupTransparency + (e.Delta > 0 ? 2 : -2);
                    Root.PickupTransparency = (byte)Math.Min(Math.Max(0, i), 255);
                    this.Cursor = CreateCursorFromBitmap(buildColorPicker(Root.PickupColor, Root.PickupTransparency));
                }
                else if (Root.ToolSelected == Tools.NumberTag)
                {
                    TagSize += (e.Delta > 0 ? 1 : -1);
                    TagSize = Math.Min(Math.Max(4, TagSize), 255);
                }
                else
                    PenWidth_Change(e.Delta > 0 ? Root.PenWidth_Delta : -Root.PenWidth_Delta);
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

        private int getStrokeProperties(Stroke st)
        {
            if (st.ExtendedProperties.Contains(Root.ISFILLEDBLACK_GUID))
                return Filling.BlackFilled;
            else if (st.ExtendedProperties.Contains(Root.ISFILLEDWHITE_GUID))
                return Filling.WhiteFilled;
            else if (st.ExtendedProperties.Contains(Root.ISFILLEDOUTSIDE_GUID))
                return Filling.Outside;
            else if (st.ExtendedProperties.Contains(Root.ISFILLEDCOLOR_GUID))
                return Filling.PenColorFilled;
            //NoFrame looks meaningless 
            else
                return Filling.Empty;
            // No Fading;
        }

        private void setStrokeProperties(ref Stroke st, int FilledSelected)
        {
            try { st.ExtendedProperties.Remove(Root.ISSTROKE_GUID); } catch { }
            try { st.ExtendedProperties.Remove(Root.ISFILLEDCOLOR_GUID); } catch { }
            try { st.ExtendedProperties.Remove(Root.ISFILLEDOUTSIDE_GUID); } catch { }
            try { st.ExtendedProperties.Remove(Root.ISFILLEDWHITE_GUID); } catch { }
            try { st.ExtendedProperties.Remove(Root.ISFILLEDBLACK_GUID); } catch { }

            if (FilledSelected != Filling.PenColorFilled && FilledSelected != Filling.Outside && st.DrawingAttributes.Width > 0)
                st.ExtendedProperties.Add(Root.ISSTROKE_GUID, true);
            if (FilledSelected == Filling.Empty)
                ;
            else if (FilledSelected == Filling.PenColorFilled)
                st.ExtendedProperties.Add(Root.ISFILLEDCOLOR_GUID, true);
            else if (FilledSelected == Filling.WhiteFilled)
                st.ExtendedProperties.Add(Root.ISFILLEDWHITE_GUID, true);
            else if (FilledSelected == Filling.BlackFilled)
                st.ExtendedProperties.Add(Root.ISFILLEDBLACK_GUID, true);
            else if (FilledSelected == Filling.Outside)
                st.ExtendedProperties.Add(Root.ISFILLEDOUTSIDE_GUID, true);
            try
            {
                // if the penattributes is not fading there is no properties and it will turn into an exception
                if (st.DrawingAttributes.ExtendedProperties.Contains(Root.FADING_PEN))
                    st.ExtendedProperties.Add(Root.FADING_PEN, DateTime.Now.AddSeconds((float)(st.DrawingAttributes.ExtendedProperties[Root.FADING_PEN].Data)).Ticks);
            }
            catch { };

        }

        int NB_ELLIPSE_PTS = 36 * 3;
        private Stroke AddEllipseStroke(int CursorX0, int CursorY0, int CursorX, int CursorY, int FilledSelected)
        {
            Point[] pts = new Point[NB_ELLIPSE_PTS + 1];
            int dX = CursorX - CursorX0;
            int dY = CursorY - CursorY0;

            for (int i = 0; i < NB_ELLIPSE_PTS + 1; i++)
            {
                pts[i] = new Point(CursorX0 + (int)(dX * Math.Cos(Math.PI * (i + NB_ELLIPSE_PTS / 8) / (NB_ELLIPSE_PTS / 2))),
                                   CursorY0 + (int)(dY * Math.Sin(Math.PI * (i + NB_ELLIPSE_PTS / 8) / (NB_ELLIPSE_PTS / 2))));
                Console.WriteLine("{0} - {1} - {2}", i, pts[i].X, pts[i].Y);
            }
            IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pts);
            Stroke st = Root.FormCollection.IC.Ink.CreateStroke(pts);
            st.DrawingAttributes = Root.FormCollection.IC.DefaultDrawingAttributes.Clone();
            st.DrawingAttributes.AntiAliased = true;
            st.DrawingAttributes.FitToCurve = Root.FitToCurve;
            setStrokeProperties(ref st, FilledSelected);
            Root.FormCollection.IC.Ink.Strokes.Add(st);
            if (st.ExtendedProperties.Contains(Root.FADING_PEN))
                FadingList.Add(st);
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
            if (st.ExtendedProperties.Contains(Root.FADING_PEN))
                FadingList.Add(st);
            return st;
        }

        private Stroke AddImageStroke(int CursorX0, int CursorY0, int CursorX, int CursorY, string fn, int Filling = -10)
        {
            Point org_sz;
            try
            {
                org_sz = ClipartsDlg.ImgSizes[ClipartsDlg.ImageListViewer.LargeImageList.Images.IndexOfKey(Root.ImageStamp.ImageStamp)];
            }
            catch
            {
                org_sz = new Point(128, 128); // should not impact the system
            }
            if (Filling == -10)
                Filling = Root.ImageStamp.Filling;
            Stroke st = AddRectStroke(CursorX0, CursorY0, CursorX, CursorY, Filling);
            try
            {
                string fn1 = Path.GetFileNameWithoutExtension(fn);
                fn1 = fn1.Split('@')[1];
                string[] lst = fn1.Split('.');
                int dx = int.Parse(lst[0]);
                int dy = int.Parse(lst[1]);
                dx = (int)(dx * (CursorX - CursorX0) * 1.0 / org_sz.X);
                dy = (int)(dy * (CursorY - CursorY0) * 1.0 / org_sz.Y);
                CursorX -= dx;
                CursorX0 -= dx;
                CursorY -= dy;
                CursorY0 -= dy;
            }
            catch
            {
                ;
            }
            st.ExtendedProperties.Add(Root.IMAGE_GUID, fn);
            st.ExtendedProperties.Add(Root.IMAGE_X_GUID, (double)CursorX0);
            st.ExtendedProperties.Add(Root.IMAGE_Y_GUID, (double)CursorY0);
            st.ExtendedProperties.Add(Root.IMAGE_W_GUID, (double)(CursorX - CursorX0));
            st.ExtendedProperties.Add(Root.IMAGE_H_GUID, (double)(CursorY - CursorY0));
            st.ExtendedProperties.Add(Root.ROTATION_GUID, 0.0D);
            if (st.ExtendedProperties.Contains(Root.FADING_PEN))
                FadingList.Add(st);

            if (ClipartsDlg.Animations.ContainsKey(fn))
            {
                AnimationStructure ani = buildAni(fn);
                Animations.Add(AniPoolIdx, ani);
                st.ExtendedProperties.Add(Root.ANIMATIONFRAMEIMG_GUID, AniPoolIdx);
                AniPoolIdx++;
            }
            return st;
        }

        private AnimationStructure buildAni(string fn)
        {
            AnimationStructure ani = new AnimationStructure();
            if (!ClipartsDlg.Animations.ContainsKey(fn))
                ClipartsDlg.LoadImage(fn);
            ani.Image = ClipartsDlg.Animations[fn];
            ani.Idx = 0;
            ani.DeleteRequested = false;
            ani.Loop = int.MaxValue;
            ani.TEnd = DateTime.MaxValue;
            Double d;
            string s = Regex.Match(Path.GetFileNameWithoutExtension(fn), "\\[(.*)\\]$").Groups[1].Value;
            bool l = false;
            if (s.EndsWith("X", StringComparison.InvariantCultureIgnoreCase))
            {
                s = s.Remove(s.Length - 1);
                l = true;
            }
            if (Double.TryParse(s, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d))
            {
                ani.DeleteAtDend = d < 0;
                if (l)
                    ani.Loop = (int)Math.Abs(d * ani.Image.NumFrames - 1);
                else
                    ani.TEnd = DateTime.Now.AddSeconds(.1 + Math.Abs(d));
            }
            ani.T0 = DateTime.Now.AddSeconds(.1 + ani.Image.Frames[ani.Idx].GetDelay());
            return ani;
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
            if (st.ExtendedProperties.Contains(Root.FADING_PEN))
                FadingList.Add(st);
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
            if (st1.ExtendedProperties.Contains(Root.FADING_PEN))
                FadingList.Add(st1);
            return st1;
        }

        public double ArrowVarLen()
        {
            return Root.ArrowLen * Math.Max(.5, Math.Pow(IC.DefaultDrawingAttributes.Width / Root.PenWidthNormal, .7));
        }

        public Bitmap PrepareArrowBitmap(string fn, Color col, int transparency, double PenWidth_p, float angle_r, out int conn_len)
        {
            string[] fn_size = fn.Split('%');
            float scale = 1.0F;
            if (fn_size.Length >= 2)
                scale = float.Parse(fn_size[1], CultureInfo.InvariantCulture);
            ImageAttributes imageAttributes = new ImageAttributes();
            Bitmap bmpi = getImgFromDiskOrRes(fn_size[0], ImageExts);
            // bmpi = new Bitmap(bmpi, (int)Math.Round(scale*bmpi.Width, 0), (int)Math.Round(scale*bmpi.Height, 0));
            conn_len = 0;
            int i = bmpi.Height / 2 + 1; //normally line 101
            while (conn_len < bmpi.Width && !bmpi.GetPixel(conn_len, i).ToArgb().Equals(Color.Blue.ToArgb()))
                conn_len++;
            if (conn_len == bmpi.Width)
                conn_len = bmpi.Width / 2;
            conn_len = bmpi.Width / 2 - conn_len;

            float[][] colorMatrixElements = {
                       new float[] {col.R/255.0f,  0,  0,  0, 0},
                       new float[] {0,  col.G / 255.0f,  0,  0, 0},
                       new float[] {0,  0,  col.B / 255.0f,  0, 0},
                       new float[] {0,  0,  0, (255 - transparency) / 255.0f, 0},
                       new float[] {0,  0,  0,     0,  1}};
            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            float f = (float)(scale * PenWidth_p / 18.0F);
            float w = (float)(Math.Abs(Math.Cos(angle_r)) * f * bmpi.Width + Math.Abs(Math.Sin(angle_r)) * f * bmpi.Height);
            float h = (float)(Math.Abs(Math.Sin(angle_r)) * f * bmpi.Width + Math.Abs(Math.Cos(angle_r)) * f * bmpi.Height);
            conn_len = (int)Math.Round(conn_len * f, 0);

            Bitmap bmpo = new Bitmap((int)Math.Round(w, 0), (int)Math.Round(h, 0), PixelFormat.Format32bppPArgb);
            Graphics g = Graphics.FromImage(bmpo);

            g.TranslateTransform(-bmpi.Width / 2, -bmpi.Height / 2);
            g.ScaleTransform(f, f, MatrixOrder.Append);
            if (Path.GetFileName(fn)[0] != '!')
                g.RotateTransform(180 + angle_r / (float)Math.PI * 180.0F, MatrixOrder.Append);
            g.TranslateTransform(w / 2, h / 2, MatrixOrder.Append);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            g.DrawImage(bmpi, new Rectangle(0, 0, bmpi.Width, bmpi.Height), 0, 0, bmpi.Width, bmpi.Height, GraphicsUnit.Pixel, imageAttributes);

            g.Dispose();
            bmpi.Dispose();
            return bmpo;
        }

        private Stroke AddArrowStroke(int CursorX0, int CursorY0, int CursorX, int CursorY)
        // arrow at starting point
        {
            Point[] pts = new Point[2];
            double theta = Math.Atan2(CursorY - CursorY0, CursorX - CursorX0);
            double len = Math.Sqrt((CursorX - CursorX0) * (CursorX - CursorX0) + (CursorY - CursorY0) * (CursorY - CursorY0));
            double scale = (Root.FormCollection.IC.DefaultDrawingAttributes.Width * 0.037795280352161) / 18.0;

            double l = ArrowVarLen();

            /*pts[0] = new Point((int)(CursorX0 + Math.Cos(theta + Root.ArrowAngle) * l), (int)(CursorY0 + Math.Sin(theta + Root.ArrowAngle) * l));
            pts[1] = new Point(CursorX0, CursorY0);
            pts[2] = new Point((int)(CursorX0 + Math.Cos(theta - Root.ArrowAngle) * l), (int)(CursorY0 + Math.Sin(theta - Root.ArrowAngle) * l));
            pts[3] = new Point(CursorX0, CursorY0);
            pts[4] = new Point(CursorX, CursorY);*/
            int l1, l2;
            Bitmap bmp = PrepareArrowBitmap(Root.ArrowHead[Root.CurrentArrow], Root.FormCollection.IC.DefaultDrawingAttributes.Color, Root.FormCollection.IC.DefaultDrawingAttributes.Transparency,
                                       Root.HiMetricToPixel(Root.FormCollection.IC.DefaultDrawingAttributes.Width), (float)theta, out l1);
            StoredArrowImages.Add(bmp);
            int i = StoredArrowImages.Count - 1;
            bmp = PrepareArrowBitmap(Root.ArrowTail[Root.CurrentArrow], Root.FormCollection.IC.DefaultDrawingAttributes.Color, Root.FormCollection.IC.DefaultDrawingAttributes.Transparency,
                                       Root.HiMetricToPixel(Root.FormCollection.IC.DefaultDrawingAttributes.Width), (float)(Math.PI + theta), out l2);
            StoredArrowImages.Add(bmp);
            int j = StoredArrowImages.Count - 1;

            pts[0] = new Point((int)Math.Round(CursorX0 + Math.Cos(theta) * scale * l1), (int)Math.Round(CursorY0 + Math.Sin(theta) * scale * l1));
            pts[1] = new Point((int)Math.Round(CursorX - Math.Cos(theta) * scale * l2), (int)Math.Round(CursorY - Math.Sin(theta) * scale * l2));

            IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref pts);
            Stroke st = Root.FormCollection.IC.Ink.CreateStroke(pts);
            st.DrawingAttributes = Root.FormCollection.IC.DefaultDrawingAttributes.Clone();
            st.DrawingAttributes.AntiAliased = true;
            st.DrawingAttributes.FitToCurve = false;
            setStrokeProperties(ref st, 0);
            st.ExtendedProperties.Add(Root.ARROWSTART_GUID, i);
            st.ExtendedProperties.Add(Root.ARROWSTART_X_GUID, CursorX0);
            st.ExtendedProperties.Add(Root.ARROWSTART_Y_GUID, CursorY0);
            st.ExtendedProperties.Add(Root.ARROWSTART_FN_GUID, Root.ArrowHead[Root.CurrentArrow]);
            st.ExtendedProperties.Add(Root.ARROWEND_GUID, j);
            st.ExtendedProperties.Add(Root.ARROWEND_X_GUID, CursorX);
            st.ExtendedProperties.Add(Root.ARROWEND_Y_GUID, CursorY);
            st.ExtendedProperties.Add(Root.ARROWEND_FN_GUID, Root.ArrowTail[Root.CurrentArrow]);

            Root.FormCollection.IC.Ink.Strokes.Add(st);
            if (st.ExtendedProperties.Contains(Root.FADING_PEN))
                FadingList.Add(st);
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
            st.ExtendedProperties.Add(Root.TEXTX_GUID, (double)pt.X);
            st.ExtendedProperties.Add(Root.TEXTY_GUID, (double)pt.Y);
            //st.ExtendedProperties.Add(Root.TEXTFORMAT_GUID, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            st.ExtendedProperties.Add(Root.TEXTHALIGN_GUID, StringAlignment.Center);
            st.ExtendedProperties.Add(Root.TEXTVALIGN_GUID, StringAlignment.Center);
            st.ExtendedProperties.Add(Root.TEXTFONT_GUID, TagFont);
            st.ExtendedProperties.Add(Root.TEXTFONTSIZE_GUID, (double)TagSize);
            st.ExtendedProperties.Add(Root.TEXTFONTSTYLE_GUID, (TagItalic ? FontStyle.Italic : FontStyle.Regular) | (TagBold ? FontStyle.Bold : FontStyle.Regular));
            st.ExtendedProperties.Add(Root.ROTATION_GUID, 0.0);
            return st;
        }

        double TextTheta = 0.0;
        private Stroke AddTextStroke(int CursorX0, int CursorY0, int CursorX, int CursorY, string txt, StringAlignment Align, int  fil_in = -1)
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
            st.ExtendedProperties.Add(Root.TEXTX_GUID, (double)pt.X);
            st.ExtendedProperties.Add(Root.TEXTY_GUID, (double)pt.Y);
            st.ExtendedProperties.Add(Root.TEXTHALIGN_GUID, Align);
            st.ExtendedProperties.Add(Root.TEXTVALIGN_GUID, StringAlignment.Near);
            st.ExtendedProperties.Add(Root.TEXTFONT_GUID, TextFont);
            st.ExtendedProperties.Add(Root.TEXTFONTSIZE_GUID, (double)TextSize);
            st.ExtendedProperties.Add(Root.TEXTFONTSTYLE_GUID, (TextItalic ? FontStyle.Italic : FontStyle.Regular) | (TextBold ? FontStyle.Bold : FontStyle.Regular));
            st.ExtendedProperties.Add(Root.ROTATION_GUID, TextTheta);
            int fil;
            if (fil_in < 0)
                fil_in = Root.TextBackground;
            switch (fil_in / 2)
            {
                case 1:
                    fil = Filling.WhiteFilled;
                    break;
                case 2:
                    fil = Filling.BlackFilled;
                    break;
                default:
                    fil = Filling.Empty;
                    break;
            };
            setStrokeProperties(ref st, fil);
            try { st.ExtendedProperties.Remove(Root.ISSTROKE_GUID); } catch { }
            if ((fil_in % 2) == 1)
                st.ExtendedProperties.Add(Root.ISSTROKE_GUID, true);
            Root.FormCollection.IC.Ink.Strokes.Add(st);
            if (st.ExtendedProperties.Contains(Root.FADING_PEN))
                FadingList.Add(st);
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
            try
            {
                IC.Cursor = cursorred;
            }
            catch
            {
                IC.Cursor = getCursFromDiskOrRes(Root.cursorarrowFileName, System.Windows.Forms.Cursors.NoMove2D);
            }
            System.Windows.Forms.Cursor.Position = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);

            return ret;
        }

        private float NearestStroke(Point pt, bool ptInPixel, out Stroke minStroke, out float pos, bool Search4Text = true, bool butLast = false, bool Magnet = true)
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
                //Rectangle r = st.GetBoundingBox();
                //float mag = Root.PixelToHiMetric(Root.MinMagneticRadius());
                //if (Magnet && (pt.X < (r.Left - mag) || pt.X > (r.Left + mag) || pt.Y < (r.Top - mag) || pt.Y > (r.Bottom + mag)))
                //    continue;
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
            if ((Control.ModifierKeys & Keys.Control) != Keys.None && (Control.ModifierKeys & Keys.Shift) != Keys.None)  // if both ctrl and shift are pressed, no magnetic effect
                return;
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
                        int x0 = Root.HiMetricToPixel((int)(double)stk.ExtendedProperties[Root.TEXTX_GUID].Data);
                        int y0 = Root.HiMetricToPixel((int)(double)stk.ExtendedProperties[Root.TEXTY_GUID].Data);
                        int x1, y1;
                        if ((System.Drawing.StringAlignment)stk.ExtendedProperties[Root.TEXTHALIGN_GUID].Data == StringAlignment.Near)
                        {
                            x1 = (int)(x0 + (double)(stk.ExtendedProperties[Root.TEXTWIDTH_GUID].Data));
                        }
                        else
                        {
                            x1 = x0;
                            x0 = (int)(x1 - (double)(stk.ExtendedProperties[Root.TEXTWIDTH_GUID].Data));
                        }
                        if ((System.Drawing.StringAlignment)stk.ExtendedProperties[Root.TEXTVALIGN_GUID].Data == StringAlignment.Near)
                        {
                            y1 = (int)(y0 + (double)stk.ExtendedProperties[Root.TEXTHEIGHT_GUID].Data);
                        }
                        else
                        {
                            y1 = y0;
                            y0 = (int)(y1 - (double)stk.ExtendedProperties[Root.TEXTHEIGHT_GUID].Data);
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
            double theta2;
            if (theta < 0)
                theta = theta + 360.0;
            theta2 = (((theta + Root.MagneticAngle / 2.0F) % Root.MagneticAngle) - Root.MagneticAngle / 2.0F) % 360.0;
            if (theta2 < 0) theta2 += 360.0;
            if ((Magnetic || (ModifierKeys & Keys.Shift) != Keys.None) &&
                (Math.Abs(theta2) < Root.MagneticAngleTolerance || Math.Abs(theta2) > (360.0 - Root.MagneticAngleTolerance)))
            {
                theta -= theta2;
                if ((Math.Abs(theta) < 45.0) || (Math.Abs(theta - 180.0) < 45.0) || (Math.Abs(theta + 180.0) < 45.0))
                    cursorY = (int)((cursorX - cursorX0) * Math.Tan(theta / 180.0 * Math.PI) + cursorY0);
                else
                    cursorX = (int)((cursorY - cursorY0) / Math.Tan(theta / 180.0 * Math.PI) + cursorX0);
                return;
            }
        }

        int TransformXc = int.MinValue;
        int TransformYc = int.MinValue;

        private void Scale(Strokes Sel, Stroke Hover, int Xc, int Yc, int X0, int Y0, int X, int Y)
        {
            if (Xc == int.MinValue || Xc == int.MaxValue)
            {
                if (Sel != null && Sel.Count > 0)
                {
                    Rectangle r = Sel.GetBoundingBox();
                    Xc = (r.Left + r.Right) / 2;
                    Yc = (r.Top + r.Bottom) / 2;
                }
                else if (Hover != null)
                {
                    Rectangle r = Hover.GetBoundingBox();
                    Xc = (r.Left + r.Right) / 2;
                    Yc = (r.Top + r.Bottom) / 2;
                }
                else
                    return;
            }
            double k = Math.Sqrt((X0 - Xc) * (X0 - Xc) + (Y0 - Yc) * (Y0 - Yc));
            k = Math.Sqrt((X - Xc) * (X - Xc) + (Y - Yc) * (Y - Yc)) / k;

            // alpha kept but not usable for the moment : X,Y is current point where as X0 Y0 is prev : k is factor between current and previous mot original
            //    double alpha = Math.Atan2(Y0 - Yc,  X0 - Xc) - Math.Atan2(Y - Yc, X - Xc);
            ScaleRotate(Sel, Hover, Xc, Yc, k, 0.0);
        }

        private void Rotate(Strokes Sel, Stroke Hover, int Xc, int Yc, int X0, int Y0, int X, int Y)
        {
            if (Xc == int.MinValue || Xc == int.MaxValue)
            {
                if (Sel != null && Sel.Count > 0)
                {
                    Rectangle r = Sel.GetBoundingBox();
                    Xc = (r.Left + r.Right) / 2;
                    Yc = (r.Top + r.Bottom) / 2;
                }
                else if (Hover != null)
                {
                    Rectangle r = Hover.GetBoundingBox();
                    Xc = (r.Left + r.Right) / 2;
                    Yc = (r.Top + r.Bottom) / 2;
                }
                else
                    return;
            }
            double alpha = Math.Atan2(Y - Yc, X - Xc) - Math.Atan2(Y0 - Yc, X0 - Xc);
            //double k = Math.Sqrt((X0 - Xc) * (X0 - Xc) + (Y0 - Yc) * (Y0 - Yc));
            //k = Math.Sqrt((X - Xc) * (X - Xc) + (Y - Yc) * (Y - Yc)) / k;

            // alpha kept but not usable for the moment : X,Y is current point where as X0 Y0 is prev : k is factor between current and previous mot original
            //    double alpha = Math.Atan2(Y0 - Yc,  X0 - Xc) - Math.Atan2(Y - Yc, X - Xc);
            ScaleRotate(Sel, Hover, Xc, Yc, 1.0, alpha / Math.PI * 180.0);
        }

        public void ScaleRotate(Strokes Sel, Stroke Hover, int Xc, int Yc, Double k, Double deg, bool applyOnPen = true)
        {
            void ModifyProperties(Stroke s)
            {
                if (s.ExtendedProperties.Contains(Root.IMAGE_GUID))
                {
                    Point p = s.GetPoint(0);
                    Double W, H, rot;
                    IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref p);
                    s.ExtendedProperties.Add(Root.IMAGE_X_GUID, (double)p.X);
                    s.ExtendedProperties.Add(Root.IMAGE_Y_GUID, (double)p.Y);
                    W = (double)(s.ExtendedProperties[Root.IMAGE_W_GUID].Data) * k;
                    H = (double)(s.ExtendedProperties[Root.IMAGE_H_GUID].Data) * k;
                    s.ExtendedProperties.Add(Root.IMAGE_W_GUID, W);
                    s.ExtendedProperties.Add(Root.IMAGE_H_GUID, H);
                    rot = (double)s.ExtendedProperties[Root.ROTATION_GUID].Data + deg;
                    s.ExtendedProperties.Add(Root.ROTATION_GUID, rot);
                    rot = rot * Math.PI / 180.0;
                    if (s.ExtendedProperties.Contains(Root.LISTOFPOINTS_GUID))
                    {
                        int i1 = 0;
                        double d1 = 0;
                        double d2 = (double)(s.ExtendedProperties[Root.REPETITIONDISTANCE_GUID].Data) * k;
                        s.ExtendedProperties.Add(Root.REPETITIONDISTANCE_GUID, d2);
                        ListPoint pts = getEquiPointsFromStroke(s, d2, ref i1, ref d1, -(int)(W * Math.Cos(rot) - H * Math.Sin(rot)) / 2, -(int)(W * Math.Sin(rot) + H * Math.Cos(rot)) / 2, true);
                        StoredPatternPoints[(int)s.ExtendedProperties[Root.LISTOFPOINTS_GUID].Data].Clear();
                        StoredPatternPoints[(int)s.ExtendedProperties[Root.LISTOFPOINTS_GUID].Data].AddRange(pts);
                    }
                }
                if (s.ExtendedProperties.Contains(Root.TEXTFONT_GUID))
                {
                    Point p = s.GetPoint(0);
                    if (s.ExtendedProperties.Contains(Root.ISTAG_GUID))
                    {
                        int minX = p.X, maxX=p.X, minY = p.Y, maxY=p.Y;
                        foreach(Point pt in s.GetPoints())
                        {
                            if (pt.X < minX) minX = pt.X;
                            if (pt.Y < minY) minY = pt.Y;
                            if (pt.X > maxX) maxX = pt.X;
                            if (pt.Y > maxY) maxY = pt.Y;
                        }
                        p.X = (int)(minX + .5 * (maxX - minX));
                        p.Y = (int)(minY + .5 * (maxY - minY));
                    }
                    //IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref p);
                    s.ExtendedProperties.Add(Root.TEXTX_GUID, (double)p.X);
                    s.ExtendedProperties.Add(Root.TEXTY_GUID, (double)p.Y);
                    s.ExtendedProperties.Add(Root.TEXTFONTSIZE_GUID, ((double)(s.ExtendedProperties[Root.TEXTFONTSIZE_GUID].Data) * k));
                    if(s.ExtendedProperties.Contains(Root.TEXTWIDTH_GUID))
                    {
                        s.ExtendedProperties.Add(Root.TEXTWIDTH_GUID, ((double)(s.ExtendedProperties[Root.TEXTWIDTH_GUID].Data) * k));
                        s.ExtendedProperties.Add(Root.TEXTHEIGHT_GUID, ((double)(s.ExtendedProperties[Root.TEXTHEIGHT_GUID].Data) * k));
                    }
                    s.ExtendedProperties.Add(Root.ROTATION_GUID, (double)s.ExtendedProperties[Root.ROTATION_GUID].Data + deg);
                }
                if (s.ExtendedProperties.Contains(Root.ARROWSTART_GUID))
                {
                    //Point p = new Point((int)s.ExtendedProperties[Root.ARROWSTART_X_GUID].Data,(int)s.ExtendedProperties[Root.ARROWSTART_Y_GUID].Data);
                    Double theta;
                    Point p = s.GetPoint(0);
                    Point p1 = s.GetPoint(1);
                    theta = Math.Atan2(p1.Y - p.Y, p1.X - p.X);
                    int i = (int)s.ExtendedProperties[Root.ARROWSTART_GUID].Data;
                    string fn = (string)s.ExtendedProperties[Root.ARROWSTART_FN_GUID].Data;
                    int l;
                    StoredArrowImages[i].Dispose();
                    double kk = Math.Max(1, s.DrawingAttributes.Width * 0.037795280352161);// code copied from Root.HiMetricToPixel in order to not have rounding;
                    StoredArrowImages[i] = PrepareArrowBitmap(fn, s.DrawingAttributes.Color, s.DrawingAttributes.Transparency, kk, (float)theta, out l);
                    kk = kk / 18.0f;
                    IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref p);

                    p.Offset((int)Math.Round(-kk * l * Math.Cos(theta)), (int)Math.Round(-kk * l * Math.Sin(theta)));
                    s.ExtendedProperties.Add(Root.ARROWSTART_X_GUID, (int)p.X);
                    s.ExtendedProperties.Add(Root.ARROWSTART_Y_GUID, (int)p.Y);
                }
                if (s.ExtendedProperties.Contains(Root.ARROWEND_GUID))
                {
                    //Point p = new Point((int)s.ExtendedProperties[Root.ARROWEND_X_GUID].Data,(int)s.ExtendedProperties[Root.ARROWEND_Y_GUID].Data);
                    Double theta;
                    Point p = s.GetPoint(1);
                    Point p1 = s.GetPoint(0);
                    theta = Math.Atan2(p1.Y - p.Y, p1.X - p.X);
                    int i = (int)s.ExtendedProperties[Root.ARROWEND_GUID].Data;
                    string fn = (string)s.ExtendedProperties[Root.ARROWEND_FN_GUID].Data;
                    int l;
                    StoredArrowImages[i].Dispose();
                    double kk = Math.Max(1, s.DrawingAttributes.Width * 0.037795280352161);// code copied from Root.HiMetricToPixel in order to not have rounding;
                    StoredArrowImages[i] = PrepareArrowBitmap(fn, s.DrawingAttributes.Color, s.DrawingAttributes.Transparency, kk, (float)theta, out l);
                    kk = kk / 18.0f;
                    IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref p);
                    p.Offset((int)Math.Round(-kk * l * Math.Cos(theta)), (int)Math.Round(-kk * l * Math.Sin(theta)));
                    s.ExtendedProperties.Add(Root.ARROWEND_X_GUID, (int)p.X);
                    s.ExtendedProperties.Add(Root.ARROWEND_Y_GUID, (int)p.Y);
                }
            }
            if (k == 0)
                return;
            Matrix m = new Matrix(1, 0, 0, 1, 0, 0);
            m.Translate(+Xc, +Yc);
            m.Scale((float)k, (float)k);
            m.Rotate((float)deg);
            m.Translate(-Xc, -Yc);
            if (Sel != null && Sel.Count > 0)
            {
                if (double.IsNaN(Sel.GetBoundingBox().Width * k))
                    return;
                Sel.Transform(m, false);
                foreach (Stroke s in Sel)
                {
                    if (applyOnPen)
                        s.DrawingAttributes.Width = (float)(s.DrawingAttributes.Width * k);
                    if (s.Deleted)
                        continue;
                    ModifyProperties(s);
                }
            }
            else if (Hover != null)
            {
                if (double.IsNaN(Hover.GetBoundingBox().Width * k))
                    return;
                if (applyOnPen)
                    Hover.DrawingAttributes.Width = (float)(Hover.DrawingAttributes.Width * k);
                Hover.Transform(m, false);
                ModifyProperties(Hover);
            }
        }
        private void mInkObject_StrokesDeleting(object sender, InkOverlayStrokesDeletingEventArgs e)
        {
            // Store strokes for later undo. They must be stored in 
            // a separate Ink object. 
            Console.WriteLine("deleting ");
        }

        int dbgcpt = 0;
        private Stroke currentStroke = null;
        private int HideMetricCountDown = 0;


        private void IC_Stroke(object sender, InkCollectorStrokeEventArgs e)
        {
            if (e.Cursor.Inverted)
            {
                Console.WriteLine("del");
                e.Cancel = true;
                return;
            }
            Rectangle r = e.Stroke.GetBoundingBox(BoundingBoxMode.PointsOnly);
            bool HitTouch = Math.Max(r.Width, r.Height) < 2 * e.Stroke.DrawingAttributes.Width;    // To take into account PenWidth extension done by GetBoundingBox
            //Console.WriteLine(string.Format("IC_Stroke X0={0} X0=X{1} / Type{2} / Hit{3}", Root.CursorX0, (Root.CursorX0== Root.CursorX)&&(Root.CursorY0 == Root.CursorY), e.Cursor.Tablet.DeviceKind.ToString(),HitTouch));
            movedStroke = null; // reset the moving object
            Root.FingerInAction = false;        // this is done a little before MouseUp ; but it looks like the one from MouseUp is not always done...
            try { if (e.Stroke.ExtendedProperties.Contains(Root.ISSTROKE_GUID)) e.Stroke.ExtendedProperties.Remove(Root.ISSTROKE_GUID); } catch { } // the ISSTROKE set for drawin
            // redundant ????
            /*try {
                e.Stroke.ExtendedProperties.Add(Root.FADING_PEN, DateTime.Now.AddSeconds((float)(e.Stroke.DrawingAttributes.ExtendedProperties[Root.FADING_PEN].Data)).Ticks);
            } catch { };
            */
            if (ZoomCapturing)
            {
                IC.Ink.DeleteStroke(e.Stroke); // the stroke that was just inserted has to be replaced.
                /* //trying to prevent capturing the rectangle but is not working
                Root.UponAllDrawingUpdate = true;
                Root.FormDisplay.timer1_Tick(null, null);
                Root.FormDisplay.Update();
                */
                //#if ((Root.CursorX0 == Int32.MinValue)|| ((Root.CursorX0 == Root.CursorX)&& (Root.CursorY0 == Root.CursorY)))
                if (HitTouch || ((Root.CursorX0 == Root.CursorX) && (Root.CursorY0 == Root.CursorY)))
                    return;
                else
                {
                    ZoomCapturing = false;
                    ZoomCaptured = true;
                }
                SaveStrokes(ZoomSaveStroke);
                Bitmap capt = new Bitmap(Math.Abs(Root.CursorX0 - Root.CursorX), Math.Abs(Root.CursorY0 - Root.CursorY));
                using (Graphics g = Graphics.FromImage(capt))
                {
                    Point p = PointToScreen(new Point(Math.Min(Root.CursorX0, Root.CursorX), Math.Min(Root.CursorY0, Root.CursorY)));
                    Size sz = new Size(capt.Width, capt.Height);
                    g.CopyFromScreen(p, Point.Empty, sz);
                    try { ClipartsDlg.Originals.Remove(Path.GetTempPath().Replace("\\","/")+"_ZoomClip"); } catch { }
                    ClipartsDlg.Originals.Add(Path.GetTempPath().Replace("\\", "/") + "_ZoomClip", capt);
                    IC.Ink.Strokes.Clear();
                    Stroke st;
                    if (Root.WindowRect.Width > 0)
                    {
                        st = AddImageStroke(0, 0, Width, Height, Path.GetTempPath().Replace("\\", "/") + "_ZoomClip", Filling.NoFrame);
                    }
                    else
                    {
                        Screen scr = Screen.FromPoint(MousePosition);
                        st = AddImageStroke(scr.Bounds.Left, scr.Bounds.Top, scr.Bounds.Right, scr.Bounds.Bottom, Path.GetTempPath().Replace("\\", "/") + "_ZoomClip", Filling.NoFrame);
                    }
                    try { st.ExtendedProperties.Remove(Root.FADING_PEN); } catch { };  // if the pen was fading we need to remove that 
                    //if (Root.CanvasCursor == 1)
                    SetPenTipCursor();
                }

                return;
            }
            else if (Root.ToolSelected == Tools.Hand)
            {
                Console.WriteLine("Hand");
                //Stroke st = e.Stroke;// IC.Ink.Strokes[IC.Ink.Strokes.Count-1];
                Stroke st = e.Stroke;// IC.Ink.Strokes[IC.Ink.Strokes.Count-1];
                try
                {
                    //if (e.Stroke.GetPoint(0).Equals(e.Stroke.GetPoint(1)) || e.Stroke.GetPoint(0).Equals(e.Stroke.GetPoint(2)))
                    //    st.SetPoints(st.GetPoints(1, st.GetPoints().Length - 1));
                    if (e.Stroke.GetPoints().Length >= 3)
                        if (e.Stroke.GetPoint(0).Equals(e.Stroke.GetPoint(2)))
                            st.SetPoint(0, e.Stroke.GetPoint(1));
                } catch { }
                setStrokeProperties(ref st, Root.FilledSelected);
                if (st.ExtendedProperties.Contains(Root.FADING_PEN))
                    FadingList.Add(st);
            }
            else if (Root.ToolSelected == Tools.PatternLine && PatternLineSteps == 2) //Draw the stroke and is ready for a new one ; the remaing is below
            {
                if (PatternPoints.Count == 0)
                {
                    IC.Ink.DeleteStroke(e.Stroke); // the stroke that was just inserted has to be replaced.                    
                }
                else
                {
                    try { e.Stroke.ExtendedProperties.Remove(Root.ISHIDDEN_GUID); } catch { }
                    e.Stroke.DrawingAttributes.Transparency = 255;
                    e.Stroke.ExtendedProperties.Add(Root.ISSTROKE_GUID, true);                  // Declared as stroke but with empy : that way after edition you make le line visible
                    e.Stroke.ExtendedProperties.Add(Root.IMAGE_GUID, Root.ImageStamp.ImageStamp);
                    e.Stroke.ExtendedProperties.Add(Root.IMAGE_X_GUID, (double)Root.CursorX0); // just to ensure no bug
                    e.Stroke.ExtendedProperties.Add(Root.IMAGE_Y_GUID, (double)Root.CursorY0);
                    e.Stroke.ExtendedProperties.Add(Root.IMAGE_W_GUID, (double)Root.ImageStamp.X);
                    e.Stroke.ExtendedProperties.Add(Root.IMAGE_H_GUID, (double)Root.ImageStamp.Y);
                    double angle;
                    if (Path.GetFileName(Root.ImageStamp.ImageStamp).StartsWith("~"))
                    {
                        angle = 180.0 / Math.PI * Math.Atan2(PatternPoints[1].Y - PatternPoints[0].Y, PatternPoints[1].X - PatternPoints[0].X);
                        if (angle == 0) angle = 1;
                        e.Stroke.ExtendedProperties.Add(Root.IMAGE_ON_LINE_GUID, true);
                    }
                    else
                        angle = 0;
                    e.Stroke.ExtendedProperties.Add(Root.ROTATION_GUID, angle);
                    if (e.Stroke.ExtendedProperties.Contains(Root.FADING_PEN))
                        FadingList.Add(e.Stroke);
                    e.Stroke.ExtendedProperties.Add(Root.REPETITIONDISTANCE_GUID, PatternDist);
                    StoredPatternPoints.Add(new ListPoint(PatternPoints));
                    e.Stroke.ExtendedProperties.Add(Root.LISTOFPOINTS_GUID, StoredPatternPoints.Count - 1);
                    if (ClipartsDlg.Animations.ContainsKey(Root.ImageStamp.ImageStamp))
                    {
                        AnimationStructure ani = buildAni(Root.ImageStamp.ImageStamp);
                        Animations.Add(AniPoolIdx, ani);
                        e.Stroke.ExtendedProperties.Add(Root.ANIMATIONFRAMEIMG_GUID, AniPoolIdx);
                        AniPoolIdx++;
                    }
                    Root.ImageStamp.Store = false;
                }
                LineForPatterns = null;
                PatternPoints.Clear();
            }
            else
            {
                //#if (Root.CursorX0 == Int32.MinValue) // process when clicking touchscreen with just a short press;
                if (HitTouch) // process when clicking touchscreen with just a short press;
                {
                    Point p = System.Windows.Forms.Cursor.Position;
                    p = Root.FormDisplay.PointToClient(p);
                    Root.CursorX = p.X;
                    Root.CursorY = p.Y;
                }
                if (Root.LassoMode)
                {
                    Point[] pts = e.Stroke.GetPoints();
                    if (pts.Length >= 3)
                    {
                        InprogressSelection = IC.Ink.HitTest(pts, Root.LassoPercent, out _);
                        InprogressSelection.Remove(e.Stroke);
                    }
                    Console.WriteLine("Lasso capt " + InprogressSelection?.Count.ToString() ?? "0");
                }
                IC.Ink.DeleteStroke(e.Stroke); // the stroke that was just inserted has to be replaced.

                //#if ((Root.ToolSelected == Tools.Line) && (Root.CursorX0 != Int32.MinValue))
                if ((Root.ToolSelected == Tools.Line) && (!HitTouch))
                {
                    Console.WriteLine("Line");
                    AddLineStroke(Root.CursorX0, Root.CursorY0, Root.CursorX, Root.CursorY);
                }
                //#else if ((Root.ToolSelected == Tools.Rect) && (Root.CursorX0 != Int32.MinValue))
                else if ((Root.ToolSelected == Tools.Rect) && (!HitTouch))
                {
                    Console.WriteLine("Rect");
                    if ((CurrentMouseButton == MouseButtons.Right) || ((int)CurrentMouseButton == 2))
                        AddRectStroke(2 * Root.CursorX0 - Root.CursorX, 2 * Root.CursorY0 - Root.CursorY, Root.CursorX, Root.CursorY, Root.FilledSelected);
                    else
                        AddRectStroke(Root.CursorX0, Root.CursorY0, Root.CursorX, Root.CursorY, Root.FilledSelected);
                }
                else if (Root.ToolSelected == Tools.ClipArt || (Root.ToolSelected == Tools.PatternLine && PatternLineSteps == 0))
                {
                    Console.WriteLine("ClipArt");
                    //int idx = ClipartsDlg.Images.Images.IndexOfKey(Root.ImageStamp.ImageStamp);
                    // we get directly data 
                    int w = Root.ImageStamp.X > 0 ? Root.ImageStamp.X : ClipartsDlg.ImgSizes[ClipartsDlg.ImageListViewer.LargeImageList.Images.IndexOfKey(Root.ImageStamp.ImageStamp)].X;
                    int h = Root.ImageStamp.Y > 0 ? Root.ImageStamp.Y : ClipartsDlg.ImgSizes[ClipartsDlg.ImageListViewer.LargeImageList.Images.IndexOfKey(Root.ImageStamp.ImageStamp)].Y;
                    //#if ((Root.CursorX0 == Int32.MinValue) || ((Root.CursorX0 == Root.CursorX) && (Root.CursorY0 == Root.CursorY)))
                    if (HitTouch || ((Root.CursorX0 == Root.CursorX) && (Root.CursorY0 == Root.CursorY)) || ((Root.CursorX0 == Int32.MinValue)))
                    {
                        Root.CursorX0 = Root.CursorX - ((CurrentMouseButton == MouseButtons.Right) || ((int)CurrentMouseButton == 2) ? (w / 2) : 0);
                        Root.CursorY0 = Root.CursorY - ((CurrentMouseButton == MouseButtons.Right) || ((int)CurrentMouseButton == 2) ? (h / 2) : 0);
                        Root.CursorX = Root.CursorX0 + w;
                        Root.CursorY = Root.CursorY0 + h;
                    }
                    else
                    {
                        if (Math.Abs((double)(Root.CursorX - Root.CursorX0) / (Root.CursorY - Root.CursorY0)) < Root.StampScaleRatio)
                        {
                            //Console.WriteLine("ratio 2 = " + ((double)(Root.CursorX - Root.CursorX0) / (Root.CursorY - Root.CursorY0)).ToString());
                            Root.CursorX = (int)(Root.CursorX0 + (double)(Root.CursorY - Root.CursorY0) / h * w);
                        }
                        else if (Math.Abs((double)(Root.CursorY - Root.CursorY0) / (Root.CursorX - Root.CursorX0)) < Root.StampScaleRatio)
                        {
                            //Console.WriteLine("ratio 1 = " + ((double)(Root.CursorY - Root.CursorY0) / (Root.CursorX - Root.CursorX0)).ToString());
                            Root.CursorY = (int)(Root.CursorY0 + (double)(Root.CursorX - Root.CursorX0) / w * h);
                        }
                        if ((CurrentMouseButton == MouseButtons.Right) || ((int)CurrentMouseButton == 2))
                        {
                            Root.CursorX0 -= (Root.CursorX - Root.CursorX0) / 2;
                            Root.CursorY0 -= (Root.CursorY - Root.CursorY0) / 2;
                        }
                    }
                    if (Root.ToolSelected == Tools.ClipArt)
                        AddImageStroke(Root.CursorX0, Root.CursorY0, Root.CursorX, Root.CursorY, Root.ImageStamp.ImageStamp);
                    else if (Root.ToolSelected == Tools.PatternLine)
                    {
                        Root.ImageStamp.X = Math.Abs(Root.CursorX - Root.CursorX0);
                        Root.ImageStamp.Y = Math.Abs(Root.CursorY - Root.CursorY0);
                        if (Root.ImageStamp.Store)
                        {
                            Root.ImageStamp.Wstored = Root.ImageStamp.X;
                            Root.ImageStamp.Hstored = Root.ImageStamp.Y;
                        }
                        PatternLineSteps = 1; // nextStep 
                        PatternImage?.Dispose();
                        PatternImage = new Bitmap(Root.ImageStamp.ImageStamp);
                        RotatingOnLine = Path.GetFileName(Root.ImageStamp.ImageStamp).StartsWith("~");
                        PatternPoints.Clear();
                    }
                }
                else if (Root.ToolSelected == Tools.PatternLine && PatternLineSteps == 1) // Measure distance
                {
                    Point p = new Point() { X = Root.CursorX0 - Root.CursorX, Y = Root.CursorY0 - Root.CursorY };
                    IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref p);
                    PatternDist = Math.Sqrt(p.X * p.X + p.Y * p.Y);
                    p.X = Root.ImageStamp.X;
                    p.Y = Root.ImageStamp.Y;
                    IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref p);
                    PatternDist = Math.Max(PatternDist, 0.5 * Math.Min(p.X, p.Y));
                    if (Root.ImageStamp.Store)
                        Root.ImageStamp.Distance = PatternDist;
                    PatternLineSteps = 2;
                    try
                    {
                        IC.Cursor = cursorred;
                    }
                    catch
                    {
                        IC.Cursor = getCursFromDiskOrRes(Root.cursorredFileName, System.Windows.Forms.Cursors.NoMove2D);
                    }

                }
                else if ((Root.ToolSelected == Tools.Oval) && !HitTouch)
                {
                    Console.WriteLine("Oval");
                    if ((CurrentMouseButton == MouseButtons.Right) || ((int)CurrentMouseButton == 2))
                        AddEllipseStroke(Root.CursorX0, Root.CursorY0, Root.CursorX, Root.CursorY, Root.FilledSelected);
                    else
                        AddEllipseStroke((Root.CursorX0 + Root.CursorX) / 2, (Root.CursorY0 + Root.CursorY) / 2, Root.CursorX, Root.CursorY, Root.FilledSelected);
                }
                //#else if (((Root.ToolSelected == Tools.StartArrow) || (Root.ToolSelected == Tools.EndArrow)) && (Root.CursorX0 != Int32.MinValue))
                else if (((Root.ToolSelected == Tools.StartArrow) || (Root.ToolSelected == Tools.EndArrow)) && !HitTouch)
                {
                    Console.WriteLine("Arrow");
                    if (((CurrentMouseButton == MouseButtons.Right) || ((int)CurrentMouseButton == 2)) ^ (Root.ToolSelected == Tools.StartArrow))
                        AddArrowStroke(Root.CursorX, Root.CursorY, Root.CursorX0, Root.CursorY0);
                    else
                        AddArrowStroke(Root.CursorX0, Root.CursorY0, Root.CursorX, Root.CursorY);
                }
                else if (Root.ToolSelected == Tools.NumberTag)
                {
                    Stroke st = AddNumberTagStroke(Root.CursorX, Root.CursorY, Root.CursorX, Root.CursorY, 
                                            String.Format(Root.TagFormatting, Root.TagNumbering, (Char)(65 + (Root.TagNumbering-1) % 26), (Char)(97 + (Root.TagNumbering - 1) % 26)));
                    Root.TagNumbering++;
                }
                else if (Root.ToolSelected == Tools.Edit) // Edit
                {
                    float pos;
                    Stroke minStroke;
                    if (NearestStroke(new Point(Root.CursorX, Root.CursorY), true, out minStroke, out pos, false, false) <= 1 + Root.PixelToHiMetric(Root.MinMagneticRadius() / (Root.MagneticRadius >= 0 ^ ((GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0) ? 1 : 10)))
                    {
                        if (minStroke.ExtendedProperties.Contains(Root.TEXT_GUID))
                        {
                            ModifyTextInStroke(minStroke, (string)(minStroke.ExtendedProperties[Root.TEXT_GUID].Data));
                            SelectTool(Tools.Hand, Filling.Empty); // Good Idea ????
                            ComputeTextBoxSize(ref minStroke);

                        }
                        else
                        {
                            AllowInteractions(true);
                            DrawingAttributes da = minStroke.DrawingAttributes.Clone();
                            int fil = getStrokeProperties(minStroke);
                            if (PenModifyDlg.ModifyPenAndFilling(ref da, ref fil))
                            {
                                minStroke.DrawingAttributes = da;
                                setStrokeProperties(ref minStroke, fil);
                            }
                            if (minStroke.ExtendedProperties.Contains(Root.ARROWSTART_GUID))
                            {
                                ArrowSelDlg dlg = new ArrowSelDlg(Root);
                                dlg.Initialize(minStroke);
                                dlg.ShowDialog();
                            }
                            AllowInteractions(false);
                        }
                    }
                }
                else if ((Root.ToolSelected == Tools.txtLeftAligned) || (Root.ToolSelected == Tools.txtRightAligned))  // new text
                {
                    Stroke st = AddTextStroke(Root.CursorX, Root.CursorY, Root.CursorX, Root.CursorY, Root.Local.ShortTxt(Root.Local.ButtonNameText),
                                              (Root.ToolSelected == Tools.txtLeftAligned) ? StringAlignment.Near : StringAlignment.Far);
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
                //#else if ((Root.ToolSelected == Tools.Poly) && ((Root.CursorX0 != Int32.MinValue) || (Math.Abs(Root.CursorY - PolyLineLastY) + Math.Abs(Root.CursorX - PolyLineLastX) < Root.MinMagneticRadius())))
                else if ((Root.ToolSelected == Tools.Poly) && (!HitTouch || (Math.Abs(Root.CursorY - PolyLineLastY) + Math.Abs(Root.CursorX - PolyLineLastX) < Root.MinMagneticRadius())))
                {
                    Console.WriteLine("Poly");
                    if (PolyLineLastX == Int32.MinValue)
                    {
                        PolyLineInProgress = AddLineStroke(Root.CursorX0, Root.CursorY0, Root.CursorX, Root.CursorY);
                        PolyLineLastX = Root.CursorX; PolyLineLastY = Root.CursorY;
                    }
                    else
                    {
                        if (Math.Abs(Root.CursorY - PolyLineLastY) + Math.Abs(Root.CursorX - PolyLineLastX) < Root.MinMagneticRadius())
                        {
                            PolyLineLastX = Int32.MinValue; PolyLineLastY = Int32.MinValue; PolyLineInProgress = null;
                        }
                        else
                        {
                            PolyLineLastX = Root.CursorX; PolyLineLastY = Root.CursorY;
                            PolyLineInProgress = ExtendPolyLineStroke(PolyLineInProgress, Root.CursorX, Root.CursorY, Root.FilledSelected);
                        }
                    }
                }
            }
            if (!Root.LassoMode && !Root.FormCollection.ZoomCapturing)
                SaveUndoStrokes();
            Root.UponAllDrawingUpdate = true;
            /*Root.FormDisplay.ClearCanvus();
            Root.FormDisplay.DrawStrokes();
            Root.FormDisplay.DrawButtons(true);
            Root.FormDisplay.UpdateFormDisplay(true);*/

            // reset the CursorX0/Y0 : this seems to introduce a wrong interim drawing
            //CurrentMouseButton = MouseButtons.None;
            Root.CursorX0 = Int32.MinValue;
            Root.CursorY0 = Int32.MinValue;
            bool redefineScale = ((CurrentMouseButton == MouseButtons.Right) || ((int)CurrentMouseButton == 2)); // right button pressed
            if (Root.MeasureWhileDrawing)
            {
                if (redefineScale &&
                    (Root.ToolSelected == Tools.Hand || Root.ToolSelected == Tools.Line ||
                     Root.ToolSelected == Tools.StartArrow || Root.ToolSelected == Tools.EndArrow)
                    )
                {
                    Double l = StrokeLength(IC.Ink.Strokes[IC.Ink.Strokes.Count - 1]);
                    Double f = ConvertMeasureLength(l);
                    string st;
                    Double g;
                    while (true)
                    {
                        st = string.Format(MeasureNumberFormat, "{0:N}", f);
                        AllowInteractions(true);
                        st = Root.InputBox(Root.Local.ReScalePrompt + " (" + Root.Measure2Unit + ")", "ppInk", st);
                        AllowInteractions(false);
                        if (st == "")
                            break;
                        if (Double.TryParse(st, out g))
                        {
                            Measure2Scale *= g / f;
                            Root.Measure2Scale = Measure2Scale;
                            break;
                        }

                    }
                    MetricToolTip.Show(MeasureStroke(IC.Ink.Strokes[IC.Ink.Strokes.Count - 1]), this, Root.CursorX, Root.CursorY - 80);
                }
            }
            HideMetricCountDown = 3000 / tiSlide.Interval; // in case we do not get through IC_stroke

            currentStroke = null;
            IC.Selection.Clear();
            Console.WriteLine(" ------------------ " + (dbgcpt++).ToString());
        }

        private List<Point> getEquiPointsFromStroke(Stroke stk, double dist, ref int Start, ref double Remain, int Xoff = 0, int Yoff = 0, bool ConvertInkSpaceToPixel = true)
        // remain indicates how much length remains previous extractions at position Start, updated to match new array
        {
            List<Point> o = new List<Point>();
            Point[] pts = stk.GetPoints(Start, stk.PacketCount - Start);        // hope that PacketCount==
            int i = 1;
            double x, y, p;
            while (i < pts.Length)
            {
                x = pts[i].X - pts[i - 1].X;
                y = pts[i].Y - pts[i - 1].Y;
                double d = Math.Sqrt(x * x + y * y);
                p = Remain - d;
                //Console.WriteLine("*** {0} = {1} % {2} == {3}", Remain, d, dist,"");
                while (p < 0) // the new point(s) is on latest segment
                {
                    double k = Remain / d;
                    Point pt = new Point() { X = (int)(pts[i].X + x * (k - 1)), Y = (int)(pts[i].Y + y * (k - 1)) };
                    x = pts[i].X - pt.X;
                    y = pts[i].Y - pt.Y;
                    d = Math.Sqrt(x * x + y * y);
                    Remain = dist;
                    p = Remain - d;
                    //Point pt = new Point(pts[i].X, pts[i].Y);
                    //Console.Write(pts[i - 1]); Console.Write(pts[i]); Console.Write(pt);
                    //Console.WriteLine(" - {0} - {1}", Remain, p);
                    if (ConvertInkSpaceToPixel)
                        IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref pt);
                    pt.X += Xoff;
                    pt.Y += Yoff;
                    o.Add(pt);
                    //p += dist;
                }
                Remain = p; // for next Loop
                //Console.WriteLine("* {0}", Remain);
                i++;
            }
            Start += pts.Length - 1;
            Console.WriteLine(Remain.ToString() + "*");
            return o;
        }


        public void ComputeTextBoxSize(ref Stroke st)
        {
            System.Drawing.StringFormat stf = new System.Drawing.StringFormat(System.Drawing.StringFormatFlags.NoClip);
            stf.Alignment = (System.Drawing.StringAlignment)(st.ExtendedProperties[Root.TEXTHALIGN_GUID].Data);
            stf.LineAlignment = (System.Drawing.StringAlignment)(st.ExtendedProperties[Root.TEXTVALIGN_GUID].Data);
            SizeF layoutSize = new SizeF(2000.0F, 2000.0F);
            layoutSize = Root.FormDisplay.gOneStrokeCanvus.MeasureString((string)(st.ExtendedProperties[Root.TEXT_GUID].Data),
                            new Font((string)st.ExtendedProperties[Root.TEXTFONT_GUID].Data, (float)(double)st.ExtendedProperties[Root.TEXTFONTSIZE_GUID].Data,
                            (System.Drawing.FontStyle)(int)st.ExtendedProperties[Root.TEXTFONTSTYLE_GUID].Data), layoutSize, stf);
            st.ExtendedProperties.Add(Root.TEXTWIDTH_GUID, (double)layoutSize.Width);
            st.ExtendedProperties.Add(Root.TEXTHEIGHT_GUID, (double)layoutSize.Height);
            if (!st.ExtendedProperties.Contains(Root.ISTAG_GUID))
            {
                Point pt = new Point((int)(double)(st.ExtendedProperties[Root.TEXTX_GUID].Data), (int)(double)(st.ExtendedProperties[Root.TEXTY_GUID].Data));
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
                if (st.ExtendedProperties.Contains(Root.ROTATION_GUID))
                {
                    double d = (double)st.ExtendedProperties[Root.ROTATION_GUID].Data;
                    st.ExtendedProperties.Add(Root.ROTATION_GUID, 0.0);
                    ScaleRotate(null, st, pt.X, pt.Y, 1.0, d);
                }
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


        private void IC_CursorDown(object sender, InkCollectorCursorDownEventArgs e)
        {
            Console.WriteLine("CursorDown :" + e.Cursor.Tablet.DeviceKind.ToString());
            if (e.Cursor.Inverted)
            {
                Console.WriteLine("!!!del");
                return;
            }
            if (ZoomCapturing)
            {
                e.Stroke.ExtendedProperties.Add(Root.ISHIDDEN_GUID, true); // we set the ISTROKE_GUID in order to draw the inprogress as a line
                e.Stroke.DrawingAttributes.Color = Color.Purple;
                e.Stroke.DrawingAttributes.Transparency = 0;
                e.Stroke.DrawingAttributes.Width = Root.PixelToHiMetric(1);
            }
            else if (Root.ToolSelected == Tools.Hand)
                e.Stroke.ExtendedProperties.Add(Root.ISSTROKE_GUID, true); // we set the ISTROKE_GUID in order to draw the inprogress as a line
            else
                e.Stroke.ExtendedProperties.Add(Root.ISHIDDEN_GUID, true); // Others should be hidden.

            if (Root.LassoMode)
            {
                e.Stroke.ExtendedProperties.Add(Root.ISLASSO_GUID, true);
                //ModifyStrokesSelection(AppendToSelection, ref InprogressSelection, StrokesSelection);
                //Console.WriteLine("StrokesSel " + StrokesSelection.Count.ToString());
            }

            if (!Root.InkVisible && Root.Snapping <= 0 && !(Root.LassoMode || ZoomCapturing || Root.EraserMode))
            {
                Root.SetInkVisible(true);
            }

            //Root.FormDisplay.ClearCanvus(Root.FormDisplay.gOneStrokeCanvus);
            //Root.FormDisplay.DrawStrokes(Root.FormDisplay.gOneStrokeCanvus);
            //Root.FormDisplay.DrawButtons(Root.FormDisplay.gOneStrokeCanvus, false);
            if (Root.ToolSelected != Tools.Hand)
            {
                Point p;
                try
                {
                    if (e.Stroke.BezierPoints.Length > 0)
                    {
                        p = e.Stroke.BezierPoints[0];
                        IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref p);
                    }
                    else
                    {
                        //throw new System.ApplicationException("Empty Stroke");
                        p = System.Windows.Forms.Cursor.Position;
                        p = Root.FormDisplay.PointToClient(p);
                    }
                }
                catch
                {
                    p = System.Windows.Forms.Cursor.Position;
                    p = Root.FormDisplay.PointToClient(p);
                }
                Root.CursorX = p.X;
                Root.CursorY = p.Y;
            }

            if (Root.EraserMode) // we are deleting the nearest object for clicking...
            {
                e.Stroke.ExtendedProperties.Add(Root.ISDELETION_GUID, true);
                float pos;
                Stroke minStroke;
                if (NearestStroke(new Point(Root.CursorX, Root.CursorY), true, out minStroke, out pos, false, false) < 1 + Root.PixelToHiMetric(Root.MinMagneticRadius() / (Root.MagneticRadius >= 0 ^ ((GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0) ? 1 : 10)))
                {
                    try
                    {
                        if (minStroke.ExtendedProperties.Contains(Root.ANIMATIONFRAMEIMG_GUID))
                            Animations.Remove((int)minStroke.ExtendedProperties[Root.ANIMATIONFRAMEIMG_GUID].Data);
                    }
                    catch { };
                    IC.Ink.DeleteStroke(minStroke);
                }
            }
            else if (Root.ToolSelected == Tools.PatternLine && PatternLineSteps == 2) // we are now drawing the final "stroke"
            {
                LineForPatterns = e.Stroke;
                PatternLastPtIndex = 0;
                PatternLastPtRemain = 0;
            }

            switch (Root.ToolSelected)
            {
                case Tools.Hand:
                case Tools.Line:
                case Tools.Poly:
                case Tools.Rect:
                case Tools.Oval:
                case Tools.StartArrow:
                case Tools.EndArrow:
                    currentStroke = e.Stroke;
                    break;
            }

        }

        private Stroke SavHoveredForSelection;

        float ZoomScreenRatio;

        private void IC_MouseDown(object sender, CancelMouseEventArgs e)
        {
            Console.WriteLine("MouseDown");
            CurrentMouseButton = e.Button;
            Screen scr = Screen.FromPoint(e.Location);
            ZoomScreenRatio = (float)(scr.Bounds.Width) / scr.Bounds.Height;
            if (gpSubTools.Visible && (int)(Btn_SubToolPin.Tag) != 1)
            {
                gpSubTools.Visible = false;
                Root.UponAllDrawingUpdate = true;
            }
            if (Root.gpPenWidthVisible)
            {
                Root.gpPenWidthVisible = false;
                Root.UponSubPanelUpdate = true;
            }
            if (Root.LassoMode)     // in MouseDown to get it also on short press;
            {
                ModifyStrokesSelection(AppendToSelection, ref InprogressSelection, StrokesSelection);
                //Console.WriteLine("StrokesSel " + StrokesSelection.Count.ToString());
            }

            Root.FingerInAction = true;

            if (Root.Snapping == 1)
            {
                Root.SnappingX = e.X;
                Root.SnappingY = e.Y;
                Root.SnappingRect = new Rectangle(e.X, e.Y, 0, 0);
                Root.Snapping = 2;
            }

            /*if (!Root.InkVisible && Root.Snapping <= 0)
            {
                Root.SetInkVisible(true);
            }*/

            LasteXY.X = e.X;
            LasteXY.Y = e.Y;
            IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref LasteXY);
            if ((Root.ToolSelected == Tools.Poly) && (PolyLineLastX != Int32.MinValue))
            {
                Root.CursorX0 = PolyLineLastX;
                Root.CursorY0 = PolyLineLastY;
            }
            else
            {
                Root.CursorX0 = e.X;
                Root.CursorY0 = e.Y;
            }
            if (Root.ToolSelected != Tools.Hand && !Root.LassoMode)
                MagneticEffect(Root.CursorX0 - 1, Root.CursorY0, ref Root.CursorX0, ref Root.CursorY0, Root.MagneticRadius > 0); // analysis of magnetic will be done within the function
            if (Root.InkVisible)
            {
                Root.CursorX = Root.CursorX0;
                Root.CursorY = Root.CursorY0;
            }

            if (Root.LassoMode && IC.Ink.Strokes.Count > 0)
                AppendToSelection = ((int)e.Button == 1) || (e.Button == MouseButtons.Left);

            SavHoveredForSelection = Root.LassoMode ? Root.StrokeHovered : null;

            if (Root.ToolSelected == Tools.Move || Root.ToolSelected == Tools.Copy || Root.ToolSelected == Tools.Scale || Root.ToolSelected == Tools.Rotate)  // Scale & Rotate here to init movedStroke
            {
                float pos;
                Guid[] arrows_guids = { Root.ARROWSTART_GUID, Root.ARROWEND_GUID };
                if (StrokesSelection.Count > 0)
                {
                    movedStroke = null;
                    //Console.WriteLine("## " + StrokesSelection.Count.ToString() + " / " + IC.Ink.Strokes.Count.ToString());
                    if ((Root.ToolSelected == Tools.Copy && (((int)e.Button == 1) || (e.Button == MouseButtons.Left))) ||
                        (Root.ToolSelected == Tools.Move && (((int)e.Button == 2) || (e.Button == MouseButtons.Right))))
                    {
                        foreach (Stroke s in StrokesSelection) // to ensure no deleted strokes
                            try
                            {
                                float f = s.DrawingAttributes.Width;
                            }
                            catch
                            {
                                StrokesSelection.Remove(s);
                            }
                        Stroke[] lst = new Stroke[StrokesSelection.Count];
                        for (int i = 0; i < StrokesSelection.Count; i++)
                        {
                            //lst[i] = Root.FormCollection.IC.Ink.CreateStroke(StrokesSelection[i].GetPoints());
                            TabletPropertyDescriptionCollection properties = new TabletPropertyDescriptionCollection();
                            foreach (Guid property in StrokesSelection[i].PacketDescription)
                                properties.Add(new TabletPropertyDescription(property, StrokesSelection[i].GetPacketDescriptionPropertyMetrics(property)));
                            lst[i] = Root.FormCollection.IC.Ink.CreateStroke(StrokesSelection[i].GetPacketData(), properties);
                            //lst[i] = Root.FormCollection.IC.Ink.CreateStroke(StrokesSelection[i].GetPoints());

                            lst[i].DrawingAttributes = StrokesSelection[i].DrawingAttributes.Clone();
                            foreach (ExtendedProperty prop in StrokesSelection[i].ExtendedProperties)
                            {
                                if (prop.Id == Root.LISTOFPOINTS_GUID)
                                {
                                    StoredPatternPoints.Add(new ListPoint(StoredPatternPoints[(int)prop.Data]));
                                    lst[i].ExtendedProperties.Add(prop.Id, StoredPatternPoints.Count - 1);
                                }
                                else if (arrows_guids.Contains(prop.Id))
                                {
                                    StoredArrowImages.Add(new Bitmap(StoredArrowImages[(int)prop.Data]));
                                    lst[i].ExtendedProperties.Add(prop.Id, StoredArrowImages.Count - 1);
                                }
                                else
                                    lst[i].ExtendedProperties.Add(prop.Id, prop.Data);
                            }
                            Root.FormCollection.IC.Ink.Strokes.Add(lst[i]);
                        }
                        StrokesSelection.Clear();
                        for (int i = 0; i < lst.Length; i++)
                        {
                            StrokesSelection.Add(lst[i]);
                        }
                    }
                    //Console.WriteLine("$$ " + StrokesSelection.Count.ToString() + " / " + IC.Ink.Strokes.Count.ToString());
                }
                else if (NearestStroke(new Point(Root.CursorX, Root.CursorY), true, out movedStroke, out pos, false, true) > 1 + Root.PixelToHiMetric(Root.MinMagneticRadius() / (Root.MagneticRadius >= 0 ^ ((GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0) ? 1 : 10))) //not hovering a stroke
                    movedStroke = null;
                else if (Root.ToolSelected == Tools.Copy)
                {
                    Stroke copied = movedStroke;
                    //movedStroke = Root.FormCollection.IC.Ink.CreateStroke(copied.GetPoints());
                    TabletPropertyDescriptionCollection properties = new TabletPropertyDescriptionCollection();
                    foreach (Guid property in copied.PacketDescription)
                        properties.Add(new TabletPropertyDescription(property, copied.GetPacketDescriptionPropertyMetrics(property)));
                    movedStroke = Root.FormCollection.IC.Ink.CreateStroke(copied.GetPacketData(), properties);
                    movedStroke.DrawingAttributes = copied.DrawingAttributes.Clone();
                    foreach (ExtendedProperty prop in copied.ExtendedProperties)
                    {
                        if (prop.Id == Root.LISTOFPOINTS_GUID)
                        {
                            StoredPatternPoints.Add(new ListPoint(StoredPatternPoints[(int)prop.Data]));
                            movedStroke.ExtendedProperties.Add(prop.Id, StoredPatternPoints.Count - 1);
                        }
                        else if (arrows_guids.Contains(prop.Id))
                        {
                            StoredArrowImages.Add(new Bitmap(StoredArrowImages[(int)prop.Data]));
                            movedStroke.ExtendedProperties.Add(prop.Id, StoredArrowImages.Count - 1);
                        }
                        else
                            movedStroke.ExtendedProperties.Add(prop.Id, prop.Data);
                    }
                    Root.FormCollection.IC.Ink.Strokes.Add(movedStroke);
                }
            }
            if (!(Root.EraserMode || Root.ToolSelected == Tools.Edit || Root.ToolSelected == Tools.Move || Root.ToolSelected == Tools.Copy || Root.LassoMode))
                MetricToolTip.Hide(this);

        }


        public Point LasteXY;
        private long lastHintDraw;

        private void IC_MouseMove(object sender, CancelMouseEventArgs e)
        {
            float pos;
            //Console.WriteLine("MouseMove");
            if (!Focused && Root.WindowRect.Width > 0 && !Root.AltTabPointer)
            {
                Focus();
                AltTabActivate();
                //Root.UnPointer();
                //Root.FormDisplay.DrawBorder(true);
            }

            if (Root.ColorPickerMode)
            {
                using (Bitmap bmp = new Bitmap(1, 1))
                    try
                    {
                        Point p = new Point(MousePosition.X, MousePosition.Y);
                        Graphics.FromImage(bmp).CopyFromScreen(p, Point.Empty, new Size(1, 1));
                        Root.PickupColor = bmp.GetPixel(0, 0);
                        try
                        {
                            if (this.Cursor?.Tag != null && (int)this.Cursor?.Tag == 2)
                                this.Cursor?.Dispose();
                        }
                        catch { }
                        this.Cursor = CreateCursorFromBitmap(buildColorPicker(Root.PickupColor, Root.PickupTransparency));
                    }
                    catch { }
            }

            Root.StrokeHovered = null;
            if (e.Button == MouseButtons.None)
            {
                if (Root.ToolSelected == Tools.txtLeftAligned || Root.ToolSelected == Tools.txtRightAligned)
                {
                    Stroke ms;
                    float pos1;
                    if ((Control.ModifierKeys & Keys.Control) != Keys.None
                        && NearestStroke(new Point(e.X, e.Y), true, out ms, out pos1, false, false) < 1 + Root.PixelToHiMetric(Root.MinMagneticRadius() / (Root.MagneticRadius >= 0 ^ ((GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0) ? 1 : 10))
                        && ms.PacketCount >= 2)
                    {
                        Root.StrokeHovered = ms;
                        int i = (int)Math.Floor(pos1);
                        if (i == ms.PacketCount - 1)
                            i--;
                        Point p = ms.GetPoint(i);
                        Point p1 = ms.GetPoint(i + 1);
                        TextTheta = (ms.PacketCount == 2 ? 180.0 : 0.0) + Math.Atan2(p1.Y - p.Y, p1.X - p.X) * 180.0 / Math.PI; // it looks like the arrrow is generated in the reversed order.
                        if (TextTheta >= 91.0 && TextTheta < 270.0)  // to prevent text upside down
                            TextTheta -= 180.0;
                    }
                    else
                    {
                        Root.StrokeHovered = null;
                        TextTheta = 0;
                    }
                }
                else if (Root.EraserMode || Root.ToolSelected == Tools.Edit || Root.ToolSelected == Tools.Move || Root.ToolSelected == Tools.Copy || Root.LassoMode
                         || Root.ToolSelected == Tools.Scale || Root.ToolSelected == Tools.Rotate)
                {
                    if (NearestStroke(new Point(e.X, e.Y), true, out Root.StrokeHovered, out pos, false) > 1 + Root.PixelToHiMetric(Root.MinMagneticRadius() / (Root.MagneticRadius >= 0 ^ ((GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0) ? 1 : 10)))
                    {
                        Root.StrokeHovered = null;
                        SavHoveredForSelection = null;
                        MetricToolTip.Hide(this);
                        return;
                    }
                    else if (Root.StrokeHovered?.Id != SavHoveredForSelection?.Id) // I do not know why comparing Objects is not good
                    {
                        if (Root.MeasureEnabled)
                            MetricToolTip.Show(MeasureStroke(Root.StrokeHovered)
                                               + (Root.LassoMode ? ("\n" + MeasureAllStrokes(StrokesSelection, InprogressSelection, Root.StrokeHovered)) : ""), this, e.Location.X, e.Location.Y - 80);
                        return;
                    }
                    else
                    {
                        //Console.WriteLine("{0} vs. {1}",Root.StrokeHovered?.Id,SavHoveredForSelection?.Id);
                        Root.StrokeHovered = null;
                        MetricToolTip.Hide(this);
                        return;
                    }
                }
                else
                {
                    if (!Root.MeasureWhileDrawing)
                        MetricToolTip.Hide(this);
                    return;
                }
            }

            //MetricToolTip.Hide(this);
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
            else if (Root.ToolSelected != Tools.Hand)
                MagneticEffect(Root.CursorX0, Root.CursorY0, ref Root.CursorX, ref Root.CursorY, Root.ToolSelected > Tools.Hand && Root.MagneticRadius > 0);

            /*if (LasteXY.X == 0 && LasteXY.Y == 0)
            {
                LasteXY.X = e.X;
                LasteXY.Y = e.Y;
                IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref LasteXY);
            }*/

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
                if (StrokesSelection.Count > 0)
                {
                    try
                    {
                        StrokesSelection.Move(currentxy.X - LasteXY.X, currentxy.Y - LasteXY.Y);
                        foreach (Stroke st in StrokesSelection)
                            MoveStrokeAndProperties(st, currentxy.X - LasteXY.X, currentxy.Y - LasteXY.Y, false);
                    } catch { }
                    Root.FormDisplay.ClearCanvus();
                    Root.FormDisplay.DrawStrokes();
                    Root.FormDisplay.UpdateFormDisplay(true);
                }
                else if (movedStroke != null)
                {
                    //TODO: ajouter aimantation
                    /*Console.WriteLine(Root.CursorX0.ToString() + " ~ " + Root.CursorY0.ToString());
                    Point xy = new Point(Root.CursorX,Root.CursorY);
                    IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref xy);
                    */
                    MoveStrokeAndProperties(movedStroke, currentxy.X - LasteXY.X, currentxy.Y - LasteXY.Y, true);

                    Root.FormDisplay.ClearCanvus();
                    Root.FormDisplay.DrawStrokes();
                    Root.FormDisplay.UpdateFormDisplay(true);
                }
            }

            if (Root.ToolSelected == Tools.Scale && TransformXc != int.MinValue && (((int)e.Button == 1) || (e.Button == MouseButtons.Left)))
            {
                //Console.WriteLine(String.Format("{0}.{1}   {2}.{3}   {4}.{5}", TransformXc, TransformYc, LasteXY.X, LasteXY.Y, currentxy.X, currentxy.Y));
                Scale(StrokesSelection, movedStroke, TransformXc, TransformYc, LasteXY.X, LasteXY.Y, currentxy.X, currentxy.Y);
                Root.UponAllDrawingUpdate = true;
            }
            if (Root.ToolSelected == Tools.Rotate && TransformXc != int.MinValue && (((int)e.Button == 1) || (e.Button == MouseButtons.Left)))
            {
                //Console.WriteLine(String.Format("{0}.{1}   {2}.{3}   {4}.{5}", TransformXc, TransformYc, LasteXY.X, LasteXY.Y, currentxy.X, currentxy.Y));
                Rotate(StrokesSelection, movedStroke, TransformXc, TransformYc, LasteXY.X, LasteXY.Y, currentxy.X, currentxy.Y);
                Root.UponAllDrawingUpdate = true;
            }

            if (currentStroke != null && Root.MeasureEnabled)
            {
                if ((DateTime.Now.Ticks - lastHintDraw) > (200 * 10000))
                {
                    string str = "?????";
                    Double dx = Root.CursorX0 == int.MinValue ? 0 : ConvertMeasureLength(Math.Abs(Root.PixelToHiMetric(Root.CursorX - Root.CursorX0)));
                    Double dy = Root.CursorY0 == int.MinValue ? 0 : ConvertMeasureLength(Math.Abs(Root.PixelToHiMetric(Root.CursorY - Root.CursorY0)));

                    switch (Root.ToolSelected)
                    {
                        case Tools.Hand:
                            str = string.Format(MeasureNumberFormat, Root.Local.FormatLength,
                                                ConvertMeasureLength(StrokeLength(currentStroke)), Root.Measure2Unit);
                            break;
                        case Tools.Line:
                        case Tools.EndArrow:
                        case Tools.StartArrow:
                            str = string.Format(MeasureNumberFormat, Root.Local.FormatLength,
                                                Math.Sqrt(dx * dx + dy * dy), Root.Measure2Unit);
                            break;
                        case Tools.Rect:
                            str = string.Format(MeasureNumberFormat, Root.Local.FormatRectSize, dx, dy, Root.Measure2Unit);
                            break;
                        case Tools.Oval:
                            str = string.Format(MeasureNumberFormat, Root.Local.FormatEllipseSize, dx, dy, Root.Measure2Unit);
                            break;
                        case Tools.Poly:
                            str = string.Format(MeasureNumberFormat, Root.Local.FormatLength,
                                                ConvertMeasureLength(StrokeLength(PolyLineInProgress)) + Math.Sqrt(dx * dx + dy * dy), Root.Measure2Unit);
                            break;
                    }

                    MetricToolTip.Show(str, this, e.X, e.Y - 80);
                    lastHintDraw = DateTime.Now.Ticks;
                }
            }
            HideMetricCountDown = 3000 / tiSlide.Interval;

            LasteXY = currentxy;
        }

        private void IC_MouseUp(object sender, CancelMouseEventArgs e)
        {
            Console.WriteLine("MouseUp");
            Root.FingerInAction = false;
            if (Root.ColorPickerMode)
                StartStopPickUpColor(2);
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
                ExitSnapping(false);
                //CurrentMouseButton = MouseButtons.None;
            }
            else if (Root.PanMode)
            {
                SaveUndoStrokes();
            }
            else if (Root.LassoMode && (SavHoveredForSelection != null) && (Root.CursorX0 == Int32.MinValue || (Math.Abs(e.X - Root.CursorX0) < Root.MinMagneticRadius() && Math.Abs(e.Y - Root.CursorY0) < Root.MinMagneticRadius())))
            {
                try
                {
                    if (((int)CurrentMouseButton == 1) || (CurrentMouseButton == MouseButtons.Left))
                    {
                        StrokesSelection.Add(SavHoveredForSelection);
                    }
                    else
                    {
                        StrokesSelection.Remove(SavHoveredForSelection);
                    }
                } catch { }
                Root.UponAllDrawingUpdate = true;
            }
            else
            {
                Root.UponAllDrawingUpdate = true;
            }

            if (Root.ToolSelected == Tools.Scale || Root.ToolSelected == Tools.Rotate)
            {
                Point p = new Point(e.X, e.Y);
                IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus, ref p);
                if (e.Button == MouseButtons.Right || (int)e.Button == 2)
                {
                    if (TransformXc == int.MinValue)
                    {
                        TransformXc = int.MaxValue;
                        TransformYc = int.MaxValue;
                    }
                    else
                    {
                        TransformXc = p.X;
                        TransformYc = p.Y;
                    }
                }
                else if (TransformXc == int.MaxValue || TransformXc == int.MinValue)
                {
                    TransformXc = p.X;
                    TransformYc = p.Y;
                }
                try
                {
                    IC.Cursor = cursorred;
                }
                catch
                {
                    IC.Cursor = getCursFromDiskOrRes(Root.cursorredFileName, System.Windows.Forms.Cursors.NoMove2D);
                }
            }

            // due to asynchronism, IC_MouseUp Could occur before IC_Stroke and then prevent the special strokes to be created but to be kept for pan,....
            if (Root.PanMode)
            {
                Root.CursorX0 = int.MinValue;
                Root.CursorY0 = int.MinValue;
            }
            CurrentMouseButton = MouseButtons.None;
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

        public void MoveStrokeAndProperties(Stroke movedStroke, int DeltaX, int DeltaY, bool moveStroke = true)
        {
            Point m = new Point(DeltaX, DeltaY);
            IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref m);
            if (movedStroke == null || movedStroke.Deleted)
                return;

            if (moveStroke)
                movedStroke.Move(DeltaX, DeltaY);

            if (movedStroke.ExtendedProperties.Contains(Root.TEXT_GUID))
            {
                movedStroke.ExtendedProperties.Add(Root.TEXTX_GUID, ((double)movedStroke.ExtendedProperties[Root.TEXTX_GUID].Data) + DeltaX);
                movedStroke.ExtendedProperties.Add(Root.TEXTY_GUID, ((double)movedStroke.ExtendedProperties[Root.TEXTY_GUID].Data) + DeltaY);
            }
            if (movedStroke.ExtendedProperties.Contains(Root.IMAGE_X_GUID))
            {
                Point pt = new Point(movedStroke.GetPoint(0).X, movedStroke.GetPoint(0).Y);
                IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref pt);
                movedStroke.ExtendedProperties.Add(Root.IMAGE_X_GUID, (double)pt.X);
                movedStroke.ExtendedProperties.Add(Root.IMAGE_Y_GUID, (double)pt.Y);
            }
            if (movedStroke.ExtendedProperties.Contains(Root.LISTOFPOINTS_GUID))
            {
                int ii = (int)movedStroke.ExtendedProperties[Root.LISTOFPOINTS_GUID].Data;
                //StoredPatternPoints[ii].ForEach(pt => pt.Offset(DeltaX, DeltaY));
                ListPoint lst = StoredPatternPoints[ii];
                for (int i = 0; i < lst.Count; i++)
                {
                    Point pt = new Point(lst[i].X + m.X, lst[i].Y + m.Y);
                    lst[i] = pt;
                }
            }
            if (movedStroke.ExtendedProperties.Contains(Root.ARROWSTART_X_GUID))
            {
                movedStroke.ExtendedProperties.Add(Root.ARROWSTART_X_GUID, (int)movedStroke.ExtendedProperties[Root.ARROWSTART_X_GUID].Data + m.X);
                movedStroke.ExtendedProperties.Add(Root.ARROWSTART_Y_GUID, (int)movedStroke.ExtendedProperties[Root.ARROWSTART_Y_GUID].Data + m.Y);
            }
            if (movedStroke.ExtendedProperties.Contains(Root.ARROWEND_X_GUID))
            {
                movedStroke.ExtendedProperties.Add(Root.ARROWEND_X_GUID, (int)movedStroke.ExtendedProperties[Root.ARROWEND_X_GUID].Data + m.X);
                movedStroke.ExtendedProperties.Add(Root.ARROWEND_Y_GUID, (int)movedStroke.ExtendedProperties[Root.ARROWEND_Y_GUID].Data + m.Y);
            }
        }

        public Double StrokeLength(Stroke st)
        {
            int j;
            Point pt, pt1;
            Double sum = 0.0F;

            if (st == null)
                return 0;
            pt = st.GetPoint(0);
            j = st.GetPoints().Length;
            for (int i = 1; i < j; i++)
            {
                pt1 = st.GetPoint(i);
                sum += Math.Sqrt((1.0 * pt1.X - pt.X) * (pt1.X - pt.X) + (1.0 * pt1.Y - pt.Y) * (pt1.Y - pt.Y));
                pt = pt1;
            }
            return sum;
        }

        NumberFormatInfo MeasureNumberFormat;
        public string MeasureStroke(Stroke st)
        {
            int j;
            Point pt, pt1;
            Double ang = Double.NaN;
            Double larg, lng;
            string str = "";
            const int EIGHT_NB_ELLIPSE_PTS = 15;

            try
            {
                j = st.GetPoints().Length;
            }
            catch
            {
                return "";
            }
            str = string.Format(MeasureNumberFormat, Root.Local.FormatLength, ConvertMeasureLength(StrokeLength(st)), Root.Measure2Unit);
            if (j == 9 && st.GetPoint(0) == st.GetPoint(j - 1)) // shortcut to check it is a rectangle
            {
                pt = st.GetPoint(0);
                pt1 = st.GetPoint(2);
                pt.X -= pt1.X; pt.Y -= pt1.Y;
                lng = Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
                pt = st.GetPoint(4);
                pt.X -= pt1.X; pt.Y -= pt1.Y;
                larg = Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
                str += string.Format(MeasureNumberFormat, " " + Root.Local.FormatRectSize, ConvertMeasureLength(lng), ConvertMeasureLength(larg), Root.Measure2Unit);
            }
            else if (j == NB_ELLIPSE_PTS + 1 && st.GetPoint(0) == st.GetPoint(j - 1)) // shortcut to check it is a rectangle
            {
                pt = st.GetPoint(EIGHT_NB_ELLIPSE_PTS + (NB_ELLIPSE_PTS) / 4);
                pt1 = st.GetPoint(EIGHT_NB_ELLIPSE_PTS + (3 * NB_ELLIPSE_PTS) / 4);
                pt.X -= pt1.X; pt.Y -= pt1.Y;
                lng = Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
                pt = st.GetPoint(EIGHT_NB_ELLIPSE_PTS);
                pt1 = st.GetPoint(EIGHT_NB_ELLIPSE_PTS + NB_ELLIPSE_PTS / 2);
                pt.X -= pt1.X; pt.Y -= pt1.Y;
                larg = Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
                str += string.Format(MeasureNumberFormat, " " + Root.Local.FormatEllipseSize, ConvertMeasureLength(lng), ConvertMeasureLength(larg), Root.Measure2Unit);
            }
            else if (j == 3)
            {
                pt = st.GetPoint(1);
                pt1 = st.GetPoint(0);
                ang = Math.Atan2(pt1.Y - pt.Y, pt1.X - pt.X);
                pt1 = st.GetPoint(2);
                ang -= Math.Atan2(pt1.Y - pt.Y, pt1.X - pt.X);
                str += string.Format("\n" + Root.Local.FormatAngle, (Root.MeasureAnglCounterClockwise ? 1 : -1) * ang / Math.PI * 180);
            }
            return str;
        }

        public string MeasureAllStrokes(Strokes sts1, Strokes sts2, Stroke Hovered, bool LengthOnly = false)
        {
            double sum = 0;
            int c = 0;
            foreach (Stroke st in sts1)
                try
                {
                    if (!st.Deleted)
                    {
                        sum += StrokeLength(st);
                        c++;
                    }
                } catch { }
            if (sts2 != null)
            {
                foreach (Stroke st in sts2)
                    try
                    {
                        sum += StrokeLength(st);
                        c++;
                    } catch { }
            }
            if (Hovered != null && !sts1.Contains(Hovered) && (sts2 == null || !sts2.Contains(Hovered)))
            {
                sum += StrokeLength(Hovered);
                c++;
            }
            if (LengthOnly)
                return ConvertMeasureLength(sum).ToString(CultureInfo.InvariantCulture);// for REST_API
            else
                return string.Format(MeasureNumberFormat, Root.Local.FormaTotalLength, ConvertMeasureLength(sum), Root.Measure2Unit, c);
        }

        public void ActivateStrokesInput(bool active)
        {
            // note: the "rectangle" defines the area IC is covering : when active, the cursor is IC.Cursor but it is this.Cursor when not active
            Rectangle rect;
            if (active)
                rect = new Rectangle(0, 0, 0, 0);  // full area
            else
                rect = new Rectangle(0, 0, 1, 1);  // only one pixel so inactive..
            int i = 5;
            while (i > 0)
                try
                {

                    IC.SetWindowInputRectangle(rect);
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine("SetWindowInputRect Exception :" + e.Message);
                    Thread.Sleep(1);
                    i--;
                    if (i <= 0)
                        throw (e);
                }
        }

        public void ToTransparent()
        {
            UInt32 dwExStyle = GetWindowLong(this.Handle, -20);
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
            UInt32 dwExStyle = GetWindowLong(this.Handle, -20);
            //SetWindowLong(this.Handle, -20, dwExStyle | 0x00080000);
            //SetWindowPos(this.Handle, (IntPtr)0, 0, 0, 0, 0, 0x0002 | 0x0001 | 0x0004 | 0x0010 | 0x0020);
            //SetLayeredWindowAttributes(this.Handle, 0x00FFFFFF, 1, 0x2);
            SetWindowLong(this.Handle, -20, dwExStyle | 0x00080000 | 0x00000020);
            //SetWindowPos(this.Handle, (IntPtr)(1), 0, 0, 0, 0, 0x0002 | 0x0001 | 0x0010 | 0x0020);
        }

        public void ToUnThrough()
        {
            UInt32 dwExStyle = GetWindowLong(this.Handle, -20);
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
            if (tool != Tools.StartArrow && tool != Tools.EndArrow)
            {
                btArrow.BackgroundImage.Dispose();
                btArrow.BackgroundImage = BuildArrowBtn(Root.ArrowHead[Root.CurrentArrow], Root.ArrowTail[Root.CurrentArrow], Color.Black);
            }
            btNumb.BackgroundImage = getImgFromDiskOrRes("tool_numb", ImageExts);
            btText.BackgroundImage = getImgFromDiskOrRes("tool_txtL", ImageExts);
            btEdit.BackgroundImage = getImgFromDiskOrRes("tool_edit", ImageExts);
            btClipArt.BackgroundImage = getImgFromDiskOrRes("tool_clipart", ImageExts);
            btClipArt.Text = (tool == Tools.PatternLine && (btClipSel != btClip1.Tag) && (btClipSel != btClip2.Tag) && (btClipSel != btClip3.Tag)) ? "S" : "";

            btScaleRot.BackgroundImage = getImgFromDiskOrRes("scale", ImageExts);

            btClip1.FlatAppearance.BorderSize = btClipSel == btClip1.Tag ? 3 : 0;
            btClip1.Text = (tool == Tools.PatternLine && (btClipSel == btClip1.Tag)) ? "S" : "";
            btClip2.FlatAppearance.BorderSize = btClipSel == btClip2.Tag ? 3 : 0;
            btClip2.Text = (tool == Tools.PatternLine && (btClipSel == btClip2.Tag)) ? "S" : "";
            btClip3.FlatAppearance.BorderSize = btClipSel == btClip3.Tag ? 3 : 0;
            btClip3.Text = (tool == Tools.PatternLine && (btClipSel == btClip3.Tag)) ? "S" : "";

            btClipSel = null;

            if (AltKeyPressed() && Root.AltAsOneCommand >= 1)
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

            if (filled >= Filling.Empty)
                Root.FilledSelected = filled;
            else if ((Array.IndexOf(applicableTool, tool) >= 0) && (tool == Root.ToolSelected))
            ////
            ///Root.FilledSelected = (Root.FilledSelected + 1) % Filling.Modulo;
            ///

            if (tool == Tools.NumberTag)
            {
                // Cycle limité à 2 états: Blanc <-> Noir
                if (Root.FilledSelected != Filling.WhiteFilled && Root.FilledSelected != Filling.BlackFilled)
                    Root.FilledSelected = Filling.WhiteFilled;
                else
                    Root.FilledSelected = (Root.FilledSelected == Filling.WhiteFilled)
                                            ? Filling.BlackFilled
                                            : Filling.WhiteFilled;
            }
            else
            {
                Root.FilledSelected = (Root.FilledSelected + 1) % Filling.Modulo;
            }



            else
                Root.FilledSelected = Filling.Empty;
            Root.UponButtonsUpdate |= 0x2;
            EnterEraserMode(false);

            if (tool != Tools.Move && tool != Tools.Copy && tool != Tools.Scale && tool != Tools.Rotate && tool != Tools.Edit && tool != Tools.Invalid)  // for  all std tools we clear selection
            {
                InprogressSelection = null;
                StrokesSelection.Clear();
                Root.UponAllDrawingUpdate = true;
            }

            if (tool == Tools.Invalid)
            {
                Root.ToolSelected = Tools.Hand; // to prevent drawing
                //return;
            }
            //else 
            if (tool == Tools.Hand)
            {
                if (Root.FilledSelected == Filling.Empty)
                    btHand.BackgroundImage = getImgFromDiskOrRes("tool_hand_act", ImageExts);
                else if (Root.FilledSelected == Filling.PenColorFilled)
                    btHand.BackgroundImage = getImgFromDiskOrRes("tool_hand_filledC", ImageExts);
                else if (Root.FilledSelected == Filling.WhiteFilled)
                    btHand.BackgroundImage = getImgFromDiskOrRes("tool_hand_filledW", ImageExts);
                else if (Root.FilledSelected == Filling.BlackFilled)
                    btHand.BackgroundImage = getImgFromDiskOrRes("tool_hand_filledB", ImageExts);
                else if (Root.FilledSelected == Filling.Outside)
                    btHand.BackgroundImage = getImgFromDiskOrRes("tool_hand_out", ImageExts);
                if (gpSubTools.Visible && subTools_title.Contains("Hand"))
                    changeActiveTool(Root.FilledSelected, false, 1);
            }
            else if ((tool == Tools.Line) || (tool == Tools.Poly))
            { if (filled >= Filling.Empty)
                {
                    Root.FilledSelected = filled;
                }
                else if (Root.ToolSelected == Tools.Line)
                {
                    tool = Tools.Poly;
                    Root.FilledSelected = Filling.Empty;
                    PolyLineLastX = Int32.MinValue; PolyLineLastY = Int32.MinValue;
                    if (gpSubTools.Visible && subTools_title.Contains("Line"))
                        changeActiveTool(1, false, 1);
                }
                else if ((Root.ToolSelected == Tools.Poly && (Root.FilledSelected == Filling.Empty || Root.FilledSelected > Filling.BlackFilled)) || (Root.ToolSelected != Tools.Poly))
                {
                    tool = Tools.Line;
                    Root.FilledSelected = Filling.Empty;
                    if (gpSubTools.Visible && subTools_title.Contains("Line"))
                        changeActiveTool(0, false, 1);
                }
                else // Root.ToolSelected == Tools.Poly && Root.FilledSelected != 4
                {
                    tool = Tools.Poly;
                    PolyLineLastX = Int32.MinValue; PolyLineLastY = Int32.MinValue;
                    if (gpSubTools.Visible && subTools_title.Contains("Line"))
                        changeActiveTool(Root.FilledSelected + 1, false, 1);
                }
                if (tool == Tools.Line)
                    btLine.BackgroundImage = getImgFromDiskOrRes("tool_line_act", ImageExts);
                else if (Root.FilledSelected == Filling.Empty)
                    btLine.BackgroundImage = getImgFromDiskOrRes("tool_mlines", ImageExts);
                else if (Root.FilledSelected == Filling.PenColorFilled)
                    btLine.BackgroundImage = getImgFromDiskOrRes("tool_mlines_filledC", ImageExts);
                else if (Root.FilledSelected == Filling.Outside)
                    btLine.BackgroundImage = getImgFromDiskOrRes("tool_mlines_out", ImageExts);
                else if (Root.FilledSelected == Filling.WhiteFilled)
                    btLine.BackgroundImage = getImgFromDiskOrRes("tool_mlines_filledW", ImageExts);
                else if (Root.FilledSelected == Filling.BlackFilled)
                    btLine.BackgroundImage = getImgFromDiskOrRes("tool_mlines_filledB", ImageExts);

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
                else if (Root.FilledSelected == Filling.Outside)
                    btRect.BackgroundImage = getImgFromDiskOrRes("tool_rect_out", ImageExts);
                if (gpSubTools.Visible && subTools_title.Contains("Rect"))
                    changeActiveTool(Root.FilledSelected, false, 1);
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
                else if (Root.FilledSelected == Filling.Outside)
                    btOval.BackgroundImage = getImgFromDiskOrRes("tool_oval_out", ImageExts);
                if (gpSubTools.Visible && subTools_title.Contains("Oval"))
                    changeActiveTool(Root.FilledSelected, false, 1);
            }
            else if (tool == Tools.StartArrow || tool == Tools.EndArrow) // also include tool=5
            {
                if (Root.ToolSelected == Tools.StartArrow || Root.ToolSelected == Tools.EndArrow)
                    if (++Root.CurrentArrow >= Root.ArrowHead.Count)
                        Root.CurrentArrow = 0;
                btArrow.BackgroundImage.Dispose();
                btArrow.BackgroundImage = BuildArrowBtn(Root.ArrowHead[Root.CurrentArrow], Root.ArrowTail[Root.CurrentArrow], Color.Orange);
                if (gpSubTools.Visible && subTools_title.Contains("Arrow"))
                    changeActiveTool(0, false, 1);
            }
            else if (tool == Tools.NumberTag)
            {
                if (Root.FilledSelected == Filling.Outside)
                    Root.FilledSelected = Filling.WhiteFilled;

                if (Root.FilledSelected == Filling.WhiteFilled)
                    btNumb.BackgroundImage = getImgFromDiskOrRes("tool_numb_fillW", ImageExts);
                else // BlackFilled
                    btNumb.BackgroundImage = getImgFromDiskOrRes("tool_numb_fillB", ImageExts);

                try
                {
                    IC.Cursor = cursorred;
                }
                catch
                {
                    IC.Cursor = getCursFromDiskOrRes(Root.cursorarrowFileName, System.Windows.Forms.Cursors.NoMove2D);
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
                    IC.Cursor = getCursFromDiskOrRes(Root.cursorarrowFileName, System.Windows.Forms.Cursors.NoMove2D);
                }
                ModifyStrokesSelection(true, ref InprogressSelection, StrokesSelection);
                if (StrokesSelection?.Count > 0)
                {
                    DrawingAttributes da;
                    int fil;
                    try
                    {
                        da = StrokesSelection[0].DrawingAttributes.Clone();
                        fil = getStrokeProperties(StrokesSelection[0]);
                    }
                    catch
                    {
                        da = IC.DefaultDrawingAttributes.Clone();
                        fil = Filling.Invalid;
                    }
                    AllowInteractions(true);
                    if (PenModifyDlg.ModifyPenAndFilling(ref da, ref fil))
                    {
                        Stroke stk;
                        for (int i = 0; i < StrokesSelection.Count; i++)
                        {
                            stk = StrokesSelection[i];
                            stk.DrawingAttributes = da.Clone();
                            setStrokeProperties(ref stk, fil);
                        }
                    }
                    AllowInteractions(false);
                    Root.UponAllDrawingUpdate = true;
                }
            }
            else if ((tool == Tools.txtLeftAligned) || (tool == Tools.txtRightAligned))
            {
                if ((tool == Tools.txtRightAligned) || (Root.ToolSelected == Tools.txtLeftAligned))
                {
                    btText.BackgroundImage = getImgFromDiskOrRes("tool_txtR_act", ImageExts);
                    tool = Tools.txtRightAligned;
                    if (gpSubTools.Visible && subTools_title.Contains("Text"))
                        changeActiveTool(1, false, 1);
                }
                else
                {
                    btText.BackgroundImage = getImgFromDiskOrRes("tool_txtL_act", ImageExts);
                    tool = Tools.txtLeftAligned;
                    if (gpSubTools.Visible && subTools_title.Contains("Text"))
                        changeActiveTool(0, false, 1);
                }
                try
                {
                    IC.Cursor = cursorred;
                }
                catch
                {
                    IC.Cursor = getCursFromDiskOrRes(Root.cursorarrowFileName, System.Windows.Forms.Cursors.NoMove2D);
                }
            }
            else if (tool == Tools.Move)
            {
                //SelectPen(LastPenSelected);
                ModifyStrokesSelection(AppendToSelection, ref InprogressSelection, StrokesSelection);
                btPan.BackgroundImage = getImgFromDiskOrRes("pan1_act", ImageExts);
                try
                {
                    IC.Cursor = cursorred;
                }
                catch
                {
                    IC.Cursor = getCursFromDiskOrRes(Root.cursorarrowFileName, System.Windows.Forms.Cursors.NoMove2D);
                }
            }
            else if (tool == Tools.Copy)
            {
                //SelectPen(LastPenSelected);
                ModifyStrokesSelection(AppendToSelection, ref InprogressSelection, StrokesSelection);
                btPan.BackgroundImage = getImgFromDiskOrRes("pan_copy", ImageExts);
                try
                {
                    IC.Cursor = cursorred;
                }
                catch
                {
                    IC.Cursor = getCursFromDiskOrRes(Root.cursorarrowFileName, System.Windows.Forms.Cursors.NoMove2D);
                }
            }
            else if (tool == Tools.Scale)
            {
                TransformXc = int.MinValue;
                TransformYc = int.MinValue;

                ModifyStrokesSelection(AppendToSelection, ref InprogressSelection, StrokesSelection);
                btScaleRot.BackgroundImage = getImgFromDiskOrRes("scale_act", ImageExts);
                try
                {
                    IC.Cursor = cursortarget;
                }
                catch
                {
                    IC.Cursor = getCursFromDiskOrRes(Root.cursortargetFileName, System.Windows.Forms.Cursors.NoMoveHoriz);
                }
            }
            else if (tool == Tools.Rotate)
            {
                TransformXc = int.MinValue;
                TransformYc = int.MinValue;

                ModifyStrokesSelection(AppendToSelection, ref InprogressSelection, StrokesSelection);
                btScaleRot.BackgroundImage = getImgFromDiskOrRes("rotate_act", ImageExts);
                try
                {
                    IC.Cursor = cursortarget;
                }
                catch
                {
                    IC.Cursor = getCursFromDiskOrRes(Root.cursortargetFileName, System.Windows.Forms.Cursors.NoMove2D);
                }
            }
            else if (tool == Tools.PatternLine)
            {
                StrokesSelection.Clear();

                TransformXc = int.MinValue;
                TransformYc = int.MinValue;

                if (Root.ImageStamp.Wstored > 0)
                {
                    Root.ImageStamp.X = Root.ImageStamp.Wstored;
                    Root.ImageStamp.Y = Root.ImageStamp.Hstored;
                    if (Root.ImageStamp.Distance > 0)
                    {
                        PatternDist = Root.ImageStamp.Distance;
                        PatternLineSteps = 2;
                    }
                    else
                        PatternLineSteps = 1;
                }
                else
                    PatternLineSteps = 0;

                PatternPoints.Clear();
                LineForPatterns = null;
                PatternLastPtIndex = -1;
                PatternLastPtRemain = 0;
                PatternImage?.Dispose();
                PatternImage = new Bitmap(Root.ImageStamp.ImageStamp);

                // no button image to be modified already done earlier in SelectTool
                try
                {
                    IC.Cursor = PatternLineSteps < 2 ? cursortarget : cursorred;
                }
                catch
                {
                    IC.Cursor = getCursFromDiskOrRes(PatternLineSteps < 2 ? Root.cursortargetFileName : Root.cursorredFileName, System.Windows.Forms.Cursors.NoMoveHoriz);
                }
            }
            Root.UponButtonsUpdate |= 0x2;
            Root.ToolSelected = tool;
        }

        public void SelectPen(int pen)
        // -4 = Lasso, -3 = pan, -2 = pointer, -1 = erasor, >=0 = pens
        {
            bool bb = Root.ColorPickerMode;

            btEraser.BackgroundImage = image_eraser;
            btPointer.BackgroundImage = image_pointer;
            btLasso.BackgroundImage = image_lasso;
            btPan.BackgroundImage.Dispose();    // will be set after
                                                //Console.WriteLine("SelectPen : " + pen.ToString());
                                                //System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
                                                //Console.WriteLine(t.ToString());
            if (pen == -4)
            {
                if (AltKeyPressed() && Root.AltAsOneCommand >= 1 && SavedPen < 0)
                {
                    SavedPen = LastPenSelected;
                }
                SelectTool(-1, 0);       // Alt will be processed inhere

                /*for (int b = 0; b < Root.MaxDisplayedPens; b++)
                {
                    try
                    {
                        btPen[b].BackgroundImage.Dispose();
                    }
                    catch { }
                    //btPen[b].Image = image_pen[b];
                    btPen[b].BackgroundImage = buildPenIcon(Root.PenAttr[b+FirstPenDisplayed].Color, Root.PenAttr[b + FirstPenDisplayed].Transparency, false,
                                                            Root.PenAttr[b + FirstPenDisplayed].ExtendedProperties.Contains(Root.FADING_PEN), 
                                                            Root.LineStyleToString(Root.PenAttr[b + FirstPenDisplayed].ExtendedProperties), Root.PenAttr[b + FirstPenDisplayed].Width);// image_pen[b];
                }*/
                recomputePensSet();

                btLasso.BackgroundImage = image_lasso_act;
                EnterEraserMode(false);
                Root.UnPointer();
                btPan.BackgroundImage = getImgFromDiskOrRes("pan", ImageExts);
                Root.PanMode = false;
                Root.LassoMode = true;
                StrokesSelection.Clear();
                InprogressSelection = null;
                Root.UponAllDrawingUpdate = true;
                ActivateStrokesInput(true);
                try
                {
                    if (IC.Cursor?.Tag != null && (int)IC.Cursor?.Tag == 2)
                        IC.Cursor?.Dispose();
                    IC.Cursor = cursorred;
                }
                catch
                {
                    IC.Cursor = getCursFromDiskOrRes(Root.cursorarrowFileName, System.Windows.Forms.Cursors.NoMove2D);
                }
            }
            else if (pen == -3)
            {
                if (AltKeyPressed() && Root.AltAsOneCommand >= 1 && SavedPen < 0)
                {
                    SavedPen = LastPenSelected;
                }
                SelectTool(-1, 0);       // Alt will be processed inhere
                /*for (int b = 0; b < Root.MaxDisplayedPens; b++)
                {
                    try
                    {
                        btPen[b].BackgroundImage.Dispose();
                    }
                    catch { }
                    //btPen[b].Image = image_pen[b];
                    btPen[b].BackgroundImage = buildPenIcon(Root.PenAttr[b + FirstPenDisplayed].Color, Root.PenAttr[b + FirstPenDisplayed].Transparency, false,
                                                            Root.PenAttr[b + FirstPenDisplayed].ExtendedProperties.Contains(Root.FADING_PEN), 
                                                            Root.LineStyleToString(Root.PenAttr[b + FirstPenDisplayed].ExtendedProperties), Root.PenAttr[b + FirstPenDisplayed].Width);// image_pen[b];                    
                }*/
                recomputePensSet();
                btPan.BackgroundImage = getImgFromDiskOrRes("pan_act", ImageExts);
                EnterEraserMode(false);
                Root.UnPointer();
                Root.PanMode = true;
                ModifyStrokesSelection(AppendToSelection, ref InprogressSelection, StrokesSelection); // not used inhere but just because button is in the part of the subtools
                Root.LassoMode = false;
                ActivateStrokesInput(false);
            }
            else if (pen == -2)
            {
                if (AltKeyPressed() && Root.AltAsOneCommand >= 1 && SavedPen < 0)
                {
                    SavedPen = LastPenSelected;
                }
                SelectTool(-1, 0);       // Alt will be processed inhere
                /*
                for (int b = 0; b < Root.MaxDisplayedPens; b++)
                    //btPen[b].Image = image_pen[b];
                    btPen[b].BackgroundImage = buildPenIcon(Root.PenAttr[b + FirstPenDisplayed].Color, Root.PenAttr[b + FirstPenDisplayed].Transparency, false,
                                                            Root.PenAttr[b + FirstPenDisplayed].ExtendedProperties.Contains(Root.FADING_PEN), 
                                                            Root.LineStyleToString(Root.PenAttr[b + FirstPenDisplayed].ExtendedProperties), 
                                                            Root.PenAttr[b + FirstPenDisplayed].Width);// image_pen[b];
                */
                recomputePensSet();
                btPan.BackgroundImage = getImgFromDiskOrRes("pan", ImageExts);
                btPointer.BackgroundImage = image_pointer_act;
                EnterEraserMode(false);
                Root.Pointer();
                Root.PanMode = false;
                // no change in selections : this will be reset ater returning from pointer mod
                Root.LassoMode = false;
            }
            else if (pen == -1)
            {
                if (AltKeyPressed() && Root.AltAsOneCommand >= 1 && SavedPen < 0)
                {
                    SavedPen = LastPenSelected;
                }
                SelectTool(-1, 0);       // Alt will be processed inhere
                                         //if (this.Cursor != System.Windows.Forms.Cursors.Default)
                                         //	this.Cursor = System.Windows.Forms.Cursors.Default;

                /*for (int b = 0; b < Root.MaxDisplayedPens; b++)
                    //btPen[b].Image = image_pen[b];
                    btPen[b].BackgroundImage = buildPenIcon(Root.PenAttr[b + FirstPenDisplayed].Color, Root.PenAttr[b + FirstPenDisplayed].Transparency, false,
                                                            Root.PenAttr[b + FirstPenDisplayed].ExtendedProperties.Contains(Root.FADING_PEN), 
                                                            Root.LineStyleToString(Root.PenAttr[b + FirstPenDisplayed].ExtendedProperties), 
                                                            Root.PenAttr[b + FirstPenDisplayed].Width);// image_pen[b];
                */
                recomputePensSet();
                btPan.BackgroundImage = getImgFromDiskOrRes("pan", ImageExts);
                btEraser.BackgroundImage = image_eraser_act;
                EnterEraserMode(true);
                Root.UnPointer();
                Root.PanMode = false;
                ModifyStrokesSelection(AppendToSelection, ref InprogressSelection, StrokesSelection);
                foreach (Stroke st in StrokesSelection)
                {
                    try
                    {
                        if (st.ExtendedProperties.Contains(Root.ANIMATIONFRAMEIMG_GUID))
                            Animations.Remove((int)st.ExtendedProperties[Root.ANIMATIONFRAMEIMG_GUID].Data);
                    }
                    catch { };
                }
                IC.Ink.Strokes.Remove(StrokesSelection);
                IC.Ink.DeleteStrokes(StrokesSelection);
                StrokesSelection.Clear();
                Root.LassoMode = false;
                Root.UponAllDrawingUpdate = true;
                // !!!!!!!!!!!!!!! random exception
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        IC.Cursor = new System.Windows.Forms.Cursor(cursorerase.Handle);
                    }
                    catch
                    {
                        cursorerase = getCursFromDiskOrRes(Root.cursoreraserFileName, System.Windows.Forms.Cursors.No);
                        //Console.WriteLine(e.Message);
                        continue;
                    }
                    break;
                }

                ActivateStrokesInput(true);
            }
            else if (pen >= 0)
            {
                // clearing selection or not depends on tools :  if pen is selected, action will be defined in SelectTool
                btPan.BackgroundImage = getImgFromDiskOrRes("pan", ImageExts);
                if (AltKeyPressed() && Root.AltAsOneCommand >= 1 && pen != LastPenSelected)
                {
                    if (Root.PenAttr[Root.SavedPenDA] != null)
                        Root.PenAttr[LastPenSelected] = Root.PenAttr[Root.SavedPenDA];
                    Root.PenAttr[Root.SavedPenDA] = null;
                    Root.UponButtonsUpdate |= 0x2;
                    if (SavedPen < 0)
                        SavedPen = LastPenSelected;
                }
                if (this.Cursor != System.Windows.Forms.Cursors.Arrow)
                    this.Cursor = System.Windows.Forms.Cursors.Arrow;
                float w = IC.DefaultDrawingAttributes.Width;
                IC.DefaultDrawingAttributes = Root.PenAttr[pen].Clone();
                if (pen == LastPenSelected)
                    IC.DefaultDrawingAttributes.Width = w;
                /*else if (Root.PenWidthEnabled && !Root.WidthAtPenSel)
                {
                    IC.DefaultDrawingAttributes.Width = Root.GlobalPenWidth;
                }*/
                else if (Root.PenWidthEnabled)
                {
                    if (Root.WidthAtPenSel)
                        Root.GlobalPenWidth = Root.PenAttr[pen].Width;
                    IC.DefaultDrawingAttributes.Width = Root.GlobalPenWidth;
                }
                LastPenSelected = pen;
                IC.DefaultDrawingAttributes.AntiAliased = true;
                IC.DefaultDrawingAttributes.FitToCurve = Root.FitToCurve;
                /*for (int b = 0; b < Root.MaxDisplayedPens; b++)
                {
                    //btPen[b].Image = image_pen[b];
                    bool sel;
                    int i;
                    if (FirstPenDisplayed < Root.MaxDisplayedPens)
                        sel = b == pen;
                    else
                        sel = (b + Root.MaxDisplayedPens) == pen;
                    if (sel)
                        i = pen - (pen % Root.MaxDisplayedPens);
                    else
                        i = FirstPenDisplayed;

                    btPen[b].BackgroundImage = buildPenIcon(Root.PenAttr[b + FirstPenDisplayed].Color, Root.PenAttr[b + FirstPenDisplayed].Transparency, b == pen,
                                                            Root.PenAttr[b + FirstPenDisplayed].ExtendedProperties.Contains(Root.FADING_PEN), 
                                                            Root.LineStyleToString(Root.PenAttr[b + FirstPenDisplayed].ExtendedProperties), Root.PenAttr[b + FirstPenDisplayed].Width);// image_pen[b];
                }*/
                recomputePensSet(FirstPenDisplayed, pen);
                //btPen[pen].Image = image_pen_act[pen];
                EnterEraserMode(false);
                Root.UnPointer();
                Root.LassoMode = false;
                Root.PanMode = false;
                try
                {
                    ActivateStrokesInput(true);
                }
                catch
                {
                    // as a backup we assert the flag to do it later...
                    SetWindowInputRectFlag = true;
                }

                if (bb)
                    StartStopPickUpColor(1);
                else if (Root.CanvasCursor == 0)
                {
                    //cursorred = new System.Windows.Forms.Cursor(gInk.Properties.Resources.cursorred.Handle);
                    try
                    {
                        IC.Cursor = cursorred;
                    }
                    catch
                    {
                        IC.Cursor = getCursFromDiskOrRes(Root.cursorarrowFileName, System.Windows.Forms.Cursors.NoMove2D);
                    }
                }
                else //if (Root.CanvasCursor == 1)
                    SetPenTipCursor();

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

        public void RetreatAndExit(bool Quick = false)
        {
            ToThrough();
            if (ZoomForm.Visible)
                btZoom_click(btZoom, null);
            gpSubTools.Visible = false;
            if (((ClipArtData)btClip1.Tag)?.Wstored > 0)
                Root.ImageStamp1 = ((ClipArtData)btClip1.Tag).Clone();
            if (((ClipArtData)btClip2.Tag)?.Wstored > 0)
                Root.ImageStamp2 = ((ClipArtData)btClip2.Tag).Clone();
            if (((ClipArtData)btClip3.Tag)?.Wstored > 0)
                Root.ImageStamp3 = ((ClipArtData)btClip3.Tag).Clone();
            try
            {
                string st = Path.GetFullPath(Environment.ExpandEnvironmentVariables(Root.SaveStrokesPath));
                if (!System.IO.Directory.Exists(st))
                    System.IO.Directory.CreateDirectory(st);
                if (IC.Ink.Strokes.Count > 0 && Root.AutoSaveStrokesAtExit)          // do not save it if there is no data to save
                    SaveStrokes(st + "AutoSave.strokes.txt");
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Root.Local.FileCanNotWrite, Environment.ExpandEnvironmentVariables(Root.SaveStrokesPath + "AutoSave.strokes.txt")));
                string errorMsg = "Silent exception logged \r\n:" + ex.Message + "\r\n\r\nStack Trace:\r\n" + ex.StackTrace + "\r\n\r\n";
                Program.WriteErrorLog(errorMsg);
            }
            Root.ClearInk();
            SaveUndoStrokes();
            //Root.SaveOptions("config.ini");
            Root.gpPenWidthVisible = false;
            Root.APIRestCloseOnSnap = false;
            if (Quick)
                Root.StopInk();
            else
            {
                LastTickTime = DateTime.Now;
                ButtonsEntering = -9;
            }
        }

        public void btDock_Click(object sender, EventArgs e)
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

            TimeSpan tsp = DateTime.Now - MouseTimeDown;
            if (Root.CurrentVideoFileName != "" && sender != null && tsp.TotalSeconds > Root.LongClickTime)    // longclick or rightclick on dock buttons to open addM3Entry only
            {
                AddM3UEntry();
                return;
            }


            gpSubTools.Visible = false;

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

            PolyLineLastX = Int32.MinValue; PolyLineLastY = Int32.MinValue; PolyLineInProgress = null;
            Root.gpPenWidthVisible = false;
            ActivateStrokesInput(false);
            if (ZoomForm.Visible)
                btZoom_click(btZoom, null);
            try
            {
                this.Cursor = cursorred;
            }
            catch
            {
                this.Cursor = getCursFromDiskOrRes(Root.cursorredFileName, System.Windows.Forms.Cursors.Cross);

            }
            Root.ResizeDrawingWindow = true;
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

            PolyLineLastX = Int32.MinValue; PolyLineLastY = Int32.MinValue; PolyLineInProgress = null;
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
                if (Root.AltTabPointer && !Root.KeepUnDockedAtPointer)
                {
                    Root.Dock();
                }
            }
            else
            {
                tempArrowCursor = null;
                SelectPen(LastPenSelected);
                if (SavedTool == Tools.Invalid)
                    SavedTool = Tools.Hand;
                SelectTool(SavedTool, SavedFilled);
                SavedTool = Tools.Invalid;
                SavedFilled = Filling.NoFrame;
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

        public void StartStopPickUpColor(int Active)  // Active : 1=Start ; 2 = Apply ; 0 = Cancel
        {
            if (!Root.ColorPickerEnabled || Root.CurrentPen < 0)
                Active = 0;     // we force to get it off
            if (Active == 1)
            {
                if (AltKeyPressed() && Root.PenAttr[Root.SavedPenDA] == null)
                    Root.PenAttr[Root.SavedPenDA] = Root.PenAttr[Root.CurrentPen].Clone();
                ActivateStrokesInput(false);
                Root.ColorPickerMode = true;
                Root.PickupTransparency = Root.PenAttr[Root.CurrentPen].Transparency;

                this.Cursor = CreateCursorFromBitmap(buildColorPicker(Root.PickupColor, Root.PickupTransparency));
                System.Windows.Forms.Cursor.Position = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y); // force cursor refresh

                btPenWidth.BackgroundImage = getImgFromDiskOrRes("tool_picker");
                Root.UponButtonsUpdate |= 0x2;
                return;
            }
            if (Active == 2)
            {
                Root.PenAttr[Root.CurrentPen].Transparency = Root.PickupTransparency;
                Root.PenAttr[Root.CurrentPen].Color = Root.PickupColor;
                btPen[Root.CurrentPen % Root.MaxDisplayedPens].BackgroundImage = buildPenIcon(Root.PenAttr[Root.CurrentPen].Color, Root.PenAttr[Root.CurrentPen].Transparency, true,
                                                            Root.PenAttr[Root.CurrentPen].ExtendedProperties.Contains(Root.FADING_PEN), Root.LineStyleToString(Root.PenAttr[Root.CurrentPen].ExtendedProperties), Root.PenAttr[Root.CurrentPen].Width);// image_pen[b];
                SelectPen(Root.CurrentPen);
                Active = 0;
            }
            if (Active == 0)
            {
                ActivateStrokesInput(true);
                //if (Root.CanvasCursor == 1)
                SetPenTipCursor();
                /*else
                    try
                    {
                        IC.Cursor = cursorred;
                    }
                    catch
                    {
                        IC.Cursor = getCursFromDiskOrRes("cursorarrow", System.Windows.Forms.Cursors.NoMove2D);
                    }*/
                //System.Windows.Forms.Cursor.Position = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y); // force cursor refresh
                btPenWidth.BackgroundImage = getImgFromDiskOrRes("penwidth");
                Root.UponButtonsUpdate |= 0x2;
                Root.ColorPickerMode = false;
            }
        }

        private void btPenWidth_Click(object sender, EventArgs e)
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

            if (Root.PointerMode)
                return;

            TimeSpan tsp = DateTime.Now - MouseTimeDown;

            if (sender != null && tsp.TotalSeconds > Root.LongClickTime)
            {
                StartStopPickUpColor(1);
            }
            else if (Root.ColorPickerMode)
                StartStopPickUpColor(0);
            else
            {
                Root.gpPenWidthVisible = !Root.gpPenWidthVisible;
                if (Root.gpPenWidthVisible)
                {
                    setPenWidthBarPosition();
                    pboxPenWidthIndicator.Left = (int)Math.Sqrt(IC.DefaultDrawingAttributes.Width * 30);
                    Root.UponButtonsUpdate |= 0x2;
                }
                else
                    Root.UponSubPanelUpdate = true;
            }
        }

        public void StartSnapshot(bool Continue)
        {
            if (ZoomForm.Visible)
                btZoom_click(btZoom, null);

            if (Root.Snapping > 0)
                return;
            ActivateStrokesInput(false);
            PolyLineLastX = Int32.MinValue; PolyLineLastY = Int32.MinValue; PolyLineInProgress = null;
            /*try
            {
                this.Cursor = cursorsnap;
            }
            catch*/
            {
                this.Cursor = getCursFromDiskOrRes(Root.cursorsnapFileName, System.Windows.Forms.Cursors.Cross);
            }

            Root.gpPenWidthVisible = false;


            SnapWithoutClosing = Continue;

            Root.SnappingX = -1;
            Root.SnappingY = -1;
            Root.SnappingRect = new Rectangle(0, 0, 0, 0);
            Root.Snapping = 1;
            ButtonsEntering = -2;
            Root.UnPointer();
        }

        public void btSnap_Click(object sender, EventArgs e)
        {
            longClickTimer.Stop(); // for an unkown reason the mouse arrives later

            StrokesSelection.Clear();
            SpotLightMode = false;
            Root.StrokeHovered = null;
            Root.UponAllDrawingUpdate = true;

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
                btZoom_click(btZoom, null);

            TimeSpan tsp = DateTime.Now - MouseTimeDown;

            if (Root.Snapping > 0)
                return;

            if ((sender != null && tsp.TotalSeconds > Root.LongClickTime) ^ Root.SwapSnapsBehaviors)
            {
                StartSnapshot(true);
            }
            else
            {
                StartSnapshot(false);
            }

            Root.SnappingX = -1;
            Root.SnappingY = -1;
            Root.SnappingRect = new Rectangle(0, 0, 0, 0);
            Root.Snapping = 1;
            ButtonsEntering = -2;
            Root.UnPointer();
        }

        public void ExitSnapping(bool cancel)
        {
            ActivateStrokesInput(true);

            Root.SnappingX = -1;
            Root.SnappingY = -1;
            if(cancel)
            {
                Root.Snapping = -60;
                ButtonsEntering = 1;
            }
            else
            {
                Root.Snapping = 1;
                ButtonsEntering = 0;
            }
            Root.CursorX0 = int.MinValue;
            Root.CursorY0 = int.MinValue;
            Root.SelectPen(Root.CurrentPen);

            this.Cursor = System.Windows.Forms.Cursors.Arrow;
        }

        public void btStop_Click(object sender, EventArgs e)
        {
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }
            StopAllZooms();
            RetreatAndExit();
        }

        DateTime LastTickTime;
        bool[] LastPenStatus = new bool[Root.MaxDisplayedPens];
        bool LastFadingToggle = false;
        bool LastEraserStatus = false;
        bool LastVisibleStatus = false;
        bool LastPointerStatus = false;
        bool LastPanStatus = false;
        bool LastScaleRotStatus = false;
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
        bool LastPenWidthPlus = false;
        bool LastPenWidthMinus = false;
        bool LastColorPickupStatus = false;
        bool LastColorEditStatus = false;
        bool LastLineStyleStatus = false;
        bool LastLassoStatus = false;
        bool LastPagePrevStatus = false;
        bool LastPageNextStatus = false;
        bool LastLoadStrokesStatus = false;
        bool LastSaveStrokesStatus = false;

        DateTime LongHkPress;

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

                Root.GlobalPenWidth = e.X * e.X / 30.0F;
                pboxPenWidthIndicator.Left = e.X - pboxPenWidthIndicator.Width / 2;
                IC.DefaultDrawingAttributes.Width = Root.GlobalPenWidth;
                Root.UponButtonsUpdate |= 0x2;
            }
        }

        private void gpPenWidth_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.X >= 10 && gpPenWidth.Width - e.X >= 10)
            {
                Root.GlobalPenWidth = e.X * e.X / 30.0F;
                pboxPenWidthIndicator.Left = e.X - pboxPenWidthIndicator.Width / 2;
                IC.DefaultDrawingAttributes.Width = Root.GlobalPenWidth;
            }

            //if (Root.CanvasCursor == 1)
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

                Root.GlobalPenWidth = x * x / 30.0F;
                pboxPenWidthIndicator.Left = x - pboxPenWidthIndicator.Width / 2;
                IC.DefaultDrawingAttributes.Width = Root.GlobalPenWidth;
                Root.UponButtonsUpdate |= 0x2;
            }
        }

        private void pboxPenWidthIndicator_MouseUp(object sender, MouseEventArgs e)
        {
            //if (Root.CanvasCursor == 1)
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
            if (Root.CanvasCursor == 0)
            {

                try
                {
                    IC.Cursor = cursorred;
                }
                catch
                {
                    IC.Cursor = getCursFromDiskOrRes(Root.cursorarrowFileName, System.Windows.Forms.Cursors.NoMove2D);
                }
                System.Windows.Forms.Cursor.Position = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                return;
            }
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
                    IC.Renderer.InkSpaceToPixel(Root.FormDisplay.gOneStrokeCanvus, ref widt);
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
            try
            {
                if (IC.Cursor?.Tag != null && (int)IC.Cursor?.Tag == 2)
                    IC.Cursor?.Dispose();
            }
            catch { }
            IC.Cursor = new System.Windows.Forms.Cursor(bitmaptip.GetHicon());
            IC.Cursor.Tag = 2;
            System.Windows.Forms.Cursor.Position = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
        }

        short LastESCStatus = 0;
        int ZoomX = -1;
        int ZoomY = -1;

        void RecomputeZoomPos(int ZoomX, int ZoomY, ref int ZoomFormRePosX, ref int ZoomFormRePosY)
        { int d0, d1;
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

        int Tick;
        private void tiSlide_Tick(object sender, EventArgs e)
        {
            Initializing = false;
            Tick++;

            if(HideMetricCountDown >0)
            {
                HideMetricCountDown--;
                if (HideMetricCountDown == 0)
                    MetricToolTip.Hide(this);
            }

            if (IC.EditingMode == InkOverlayEditingMode.Delete && !IC.CollectingInk && !Root.EraserMode)
                IC.EditingMode = InkOverlayEditingMode.Ink;

            if (Root.ToolSelected == Tools.PatternLine && PatternLineSteps == 2 && LineForPatterns != null)
            {
                Console.Write("Pat "); Console.WriteLine(PatternLastPtIndex);
                List<Point> r = getEquiPointsFromStroke(LineForPatterns, PatternDist, ref PatternLastPtIndex, ref PatternLastPtRemain, -Root.ImageStamp.X / 2, -Root.ImageStamp.Y / 2, true);
                PatternPoints.AddRange(r);
            }

            if (Root.FormDisplay == null || !Root.FormDisplay.Visible)
                return;

            //if (Tick % 50 == 0) Console.WriteLine("AW."+Tick.ToString()+"="+ GetCaptionOfActiveWindow());

            if (IsMovingToolbar == 2)
            {
                if (MouseButtons.Equals(MouseButtons.None))
                {
                    IsMovingToolbar = 0;
                    return;
                }
                if (MousePosition.X != HitMovingToolbareXY.X || MousePosition.Y != HitMovingToolbareXY.Y)
                {
                    int newleft = gpButtons.Left + MousePosition.X - HitMovingToolbareXY.X;
                    int newtop = gpButtons.Top + MousePosition.Y - HitMovingToolbareXY.Y;

                    if (IsInsideVisibleScreen(newleft, newtop) && IsInsideVisibleScreen(newleft + gpButtons.Width, newtop) &&
                        IsInsideVisibleScreen(newleft, newtop + gpButtons.Height) && IsInsideVisibleScreen(newleft + gpButtons.Width, newtop + gpButtons.Height))
                    {
                        gpButtonsLeft = gpButtonsLeft + newleft - gpButtons.Left;
                        gpButtonsTop = gpButtonsTop + newtop - gpButtons.Top;
                        gpButtons.Left = newleft;
                        gpButtons.Top = newtop;
                        Root.UponAllDrawingUpdate = true;
                        Root.UponButtonsUpdate |= 0x5;
                        ToolbarMoved = true;
                        Root.gpButtonsLeft = gpButtonsLeft;
                        Root.gpButtonsTop = gpButtonsTop;
                    }
                    HitMovingToolbareXY.X = MousePosition.X;
                    HitMovingToolbareXY.Y = MousePosition.Y;
                    return;
                }
            }


            if (ZoomForm.Visible && (Root.ZoomContinous || MousePosition.X != ZoomX || MousePosition.Y != ZoomY))
            {
                ZoomX = MousePosition.X;
                ZoomY = MousePosition.Y;
                RecomputeZoomPos(ZoomX, ZoomY, ref ZoomFormRePosX, ref ZoomFormRePosY);
                ZoomForm.Top = MousePosition.Y + ZoomFormRePosY;
                ZoomForm.Left = MousePosition.X + ZoomFormRePosX;

                Bitmap img;
                img = (ZoomForm.pictureBox1.Visible) ? ZoomImage2 : ZoomImage; // this is setting img to point to the ZoomImage(2) : do not dispose it !

                using (Graphics g = Graphics.FromImage(img))
                {
                    Point p = new Point(MousePosition.X - ZoomImage.Width / 2, MousePosition.Y - ZoomImage.Height / 2);
                    Size sz = new Size(ZoomImage.Width, ZoomImage.Height);

                    g.Clear(Color.Black);
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
                try
                {
                    btVideo.BackgroundImage.Dispose();
                }
                catch { }
                finally
                {
                    btVideo.BackgroundImage = getImgFromDiskOrRes("VidStop", ImageExts);
                }
                Root.UponButtonsUpdate |= 0x2;
            }
            try
            {
                if (SetWindowInputRectFlag) // alternative to prevent some error when trying to call this function from WM_ACTIVATE event handler
                    ActivateStrokesInput(true);
                SetWindowInputRectFlag = false;
            }
            catch { }
            // ignore the first tick
            if (LastTickTime.Year == 1987)
            {
                //Console.WriteLine("AA=" + (DateTime.Now.Ticks / 1e7).ToString());
                LastTickTime = DateTime.Now;
                return;
            }

            try
            {
                //for (int i = IC.Ink.Strokes.Count - 1; i >= 0; i--)                
                foreach (Stroke st in FadingList)
                {
                    //Stroke st = IC.Ink.Strokes[i];
                    if (st.Deleted)
                        FadingList.Remove(st);
                    else if (st.ExtendedProperties.Contains(Root.FADING_PEN))
                    {
                        Int64 j = (Int64)(st.ExtendedProperties[Root.FADING_PEN].Data);
                        if (DateTime.Now.Ticks > j)
                        {
                            if (st.DrawingAttributes.Transparency == 255)
                            {
                                //IC.Ink.Strokes.RemoveAt(i);
                                FadingList.Remove(st);
                                IC.Ink.DeleteStroke(st);
                            }
                            else if (st.DrawingAttributes.Transparency > (255-Root.DecreaseFading))
                                st.DrawingAttributes.Transparency = 255;
                            else
                                st.DrawingAttributes.Transparency += Root.DecreaseFading;
                            Root.UponAllDrawingUpdate = true;
                        }
                    }
                }
            }
            catch { };

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
                        if (toolTip.GetToolTip(btStop) != MemoHintDock)
                        {
                            btStop.Click -= btStop_Click;
                            btStop.Click += btDock_Click;
                            toolTip.SetToolTip(btStop, MemoHintDock);
                        }
                        break;
                    case Orientation.toUp:
                        AimedPos.Y = gpButtonsTop + d;
                        AimedSize.Height = gpButtonsHeight - d;
                        break;
                    case Orientation.toDown:
                        AimedPos.Y = gpButtonsTop;
                        AimedSize.Height = gpButtonsHeight - d;
                        if (toolTip.GetToolTip(btStop) != MemoHintDock)
                        {
                            btStop.Click -= btStop_Click;
                            btStop.Click += btDock_Click;
                            toolTip.SetToolTip(btStop, MemoHintDock);
                        }
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
                        if (toolTip.GetToolTip(btStop) != MemoHintClose)
                        {
                            btStop.Click -= btDock_Click;
                            btStop.Click += btStop_Click;
                            toolTip.SetToolTip(btStop, MemoHintClose);
                        }
                        break;
                    case Orientation.toUp:
                        AimedPos.Y = gpButtonsTop + d;
                        AimedSize.Height = Root.Docked ? btDock.Height : gpButtonsHeight;
                        break;
                    case Orientation.toDown:
                        AimedPos.Y = gpButtonsTop;
                        AimedSize.Height = Root.Docked ? btDock.Height : gpButtonsHeight;
                        if (toolTip.GetToolTip(btStop) != MemoHintClose)
                        {
                            btStop.Click -= btDock_Click;
                            btStop.Click += btStop_Click;
                            toolTip.SetToolTip(btStop, MemoHintClose);
                        }
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
                    {
                        VisibleToolbar.Width = AimedSize.Width;
                        gpButtons.Width = AimedSize.Width;
                    }
                    else
                        VisibleToolbar.Width = (int)(VisibleToolbar.Width * .5 + AimedSize.Width * .5);
                else
                {
                    VisibleToolbar.Width = gpButtonsWidth - Math.Abs(gpButtons.Left - gpButtonsLeft);// Math.Max(gpButtonsWidth - Math.Abs(gpButtons.Left - gpButtonsLeft), btDock.Width);
                    gpButtons.Width = VisibleToolbar.Width;
                }

                if (Root.ToolbarOrientation == Orientation.toDown)
                    if (Math.Abs(VisibleToolbar.Height - AimedSize.Height) < 5)
                    {
                        VisibleToolbar.Height = AimedSize.Height;
                        gpButtons.Height = AimedSize.Height;
                    }
                    else
                        VisibleToolbar.Height = (int)(VisibleToolbar.Height * .5 + AimedSize.Height * .5);
                else
                {
                    VisibleToolbar.Height = gpButtonsHeight - Math.Abs(gpButtons.Top - gpButtonsTop);// Math.Max(gpButtonsHeight - Math.Abs(gpButtons.Top - gpButtonsTop), btDock.Height);
                    gpButtons.Height = VisibleToolbar.Height;
                }

                Root.UponAllDrawingUpdate = true;
                Root.UponButtonsUpdate |= 0x5;
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
                // customized close key or ESC in key : Exit
                short retVal;
                if (Root.Hotkey_Close.Key != 0)
                {
                    retVal = GetKeyState(Root.Hotkey_Close.Key);
                    if (Root.Snapping > 0)
                        retVal |= GetKeyState(Root.Hotkey_SnapClose.Key);
                    if ((retVal & 0x8000) == 0x8000 && (LastESCStatus & 0x8000) == 0x0000 && !TextEdited)
                    {
                        if (Root.Snapping > 0)
                        {
                            ExitSnapping(true);
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

                //Console.WriteLine(SnappingPointerStep);
                if (SnappingPointerStep == 100)
                {
                    string fn = Environment.ExpandEnvironmentVariables(DateTime.Now.ToString("'%temp%/CtrlShift'ddMMM-HHmmss'.png'"));
                    Root.FormDisplay.SnapShot(new Rectangle(Left, Top, Width, Height), fn);
                    PointerModeSnaps.Add(fn);
                    Root.trayIcon.ShowBalloonTip(100, "", string.Format(Root.Local.SnappingInPointerMessage, PointerModeSnaps.Count), ToolTipIcon.Info);
                    SnappingPointerStep = 101;      // for security
                    //System.Media.SystemSounds.Asterisk.Play();
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
            if (!AltKeyPressed() && Root.AltAsOneCommand >= 1)
            {
                if (Root.PenAttr[Root.SavedPenDA] != null && Root.CurrentPen >= 0)
                {
                    Root.PenAttr[Root.CurrentPen] = Root.PenAttr[Root.SavedPenDA];
                    Root.PenAttr[Root.SavedPenDA] = null;
                    btPen[Root.CurrentPen%Root.MaxDisplayedPens].BackgroundImage = buildPenIcon(Root.PenAttr[Root.CurrentPen].Color, Root.PenAttr[Root.CurrentPen].Transparency, true, Root.PenAttr[Root.CurrentPen].ExtendedProperties.Contains(Root.FADING_PEN)
                                                             , Root.LineStyleToString(Root.PenAttr[Root.CurrentPen].ExtendedProperties), Root.PenAttr[Root.CurrentPen].Width);
                    SelectPen(Root.CurrentPen);
                    Root.UponButtonsUpdate |= 0x2;
                }

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

            if (((AltKeyPressed() || Root.APIRestAltPressed) && !Root.FingerInAction) && tempArrowCursor is null)
            {
                if (Root.SpotOnAlt)
                    SpotLightTemp = true;
                tempArrowCursor = IC.Cursor;
                try
                {
                    IC.Cursor = cursorred;
                }
                catch
                {
                    IC.Cursor = getCursFromDiskOrRes(Root.cursorarrowFileName, System.Windows.Forms.Cursors.NoMove2D);
                }
                System.Windows.Forms.Cursor.Position = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
            }
            else if (!(tempArrowCursor is null) && !(AltKeyPressed() || Root.APIRestAltPressed))
            {
                SpotLightTemp = false;
                try
                {
                    IC.Cursor = tempArrowCursor;
                    tempArrowCursor = null;
                    System.Windows.Forms.Cursor.Position = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                }
                catch
                {
                    Program.WriteErrorLog("silent exception in IC.Cursor = tempArrowCursor;");
                }
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
                int alt = Root.AltAsOneCommand == 2 ? -1 : (AltKeyPressed() ? 1 : 0);
                bool shift = ((short)(GetKeyState(VK_LSHIFT) | GetKeyState(VK_RSHIFT)) & 0x8000) == 0x8000;
                bool win = ((short)(GetKeyState(VK_LWIN) | GetKeyState(VK_RWIN)) & 0x8000) == 0x8000;

                bool recomputePens;
                
                if (Root.PensExtraSet && ((shift || control) != oldShiftPensExtra))
                {
                    recomputePens = true;
                    //Console.Write("!!!!! {0} {1} {2} ", (shift || control), FirstPenDisplayed != 0, ((shift || control) ^ (FirstPenDisplayed = 0)));
                    if(oldShiftPensExtra != null)
                        FirstPenDisplayed = (FirstPenDisplayed == 0)? Root.MaxDisplayedPens : 0;
                        while (!Root.PenEnabled[FirstPenDisplayed])
                            FirstPenDisplayed++;
                    //Console.WriteLine(" !! {0}", FirstPenDisplayed);
                }
                else
                    recomputePens = false;
                oldShiftPensExtra = (shift || control);
            

                if (recomputePens)
                    recomputePensSet(FirstPenDisplayed, Root.CurrentPen);

                if (Root.Hotkey_Pens[0].ConflictWith(Root.Hotkey_Pens[1]))
                { // same hotkey for pen 0 and pen 1 : we have to rotate through pens
                    pressed = ((GetKeyState(Root.Hotkey_Pens[0].Key) & 0x8000) == 0x8000) && Root.Hotkey_Pens[0].ModifierMatch(control, alt, shift, win);
                    if (pressed && !LastPenStatus[0])
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
                        //SelectPen(p);
                        MouseTimeDown = DateTime.Now;
                        LongHkPress = DateTime.Now.AddSeconds(Root.LongHKPressDelay);
                        btColor_Click(btPen[p], null);
                    }
                    if (LastPenStatus[0] && !pressed)
                        LongHkPress = DateTime.Now.AddYears(1);
                    if (LastPenStatus[0] && pressed && DateTime.Now.CompareTo(LongHkPress) > 0)
                    {
                        LongHkPress = DateTime.Now.AddYears(1);
                        btColor_LongClick(btPen[Root.CurrentPen]);
                    }
                    LastPenStatus[0] = pressed;
                }
                else
                { // standard behavior
                    for (int p = 0; p < Root.MaxDisplayedPens; p++)
                    {
                        pressed = ((GetKeyState(Root.Hotkey_Pens[p].Key) & 0x8000) == 0x8000) && Root.Hotkey_Pens[p].ModifierMatch(control && !Root.PensExtraSet, alt, shift && !Root.PensExtraSet, win);
                        if (pressed && !LastPenStatus[p])
                        {
                            //SelectPen(p);
                            MouseTimeDown = DateTime.Now;
                            LongHkPress = DateTime.Now.AddSeconds(Root.LongHKPressDelay);
                            btColor_Click(btPen[p], null); // behavior with ctrl or shift will be performed through FirstPenDisplayed in btColor
                        }
                        if (LastPenStatus[p] && !pressed)
                            LongHkPress = DateTime.Now.AddYears(1);
                        if (LastPenStatus[p] && pressed && DateTime.Now.CompareTo(LongHkPress) > 0)
                        {
                            LongHkPress = DateTime.Now.AddYears(1);
                            btColor_LongClick(btPen[p]);
                        }
                        LastPenStatus[p] = pressed;
                    }
                }

                pressed = (GetKeyState(Root.Hotkey_FadingToggle.Key) & 0x8000) == 0x8000;
                if (pressed && !LastFadingToggle && Root.Hotkey_FadingToggle.ModifierMatch(control, alt, shift, win))
                {
                    FadingToggle(Root.CurrentPen);
                }
                LastFadingToggle = pressed;

                pressed = (GetKeyState(Root.Hotkey_Eraser.Key) & 0x8000) == 0x8000;
                if (pressed && !LastEraserStatus && Root.Hotkey_Eraser.ModifierMatch(control, alt, shift, win))
                {
                    SelectPen(-1);
                    FromHandToLineOnShift = false;
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
                    btUndo_Click(null, null);  // prefered in order to process also undo selection
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
                    FromHandToLineOnShift = false;
                }
                LastPointerStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Pan.Key) & 0x8000) == 0x8000;
                if (pressed && !LastPanStatus && Root.Hotkey_Pan.ModifierMatch(control, alt, shift, win))
                {
                    btPan_Click(null, null);//SelectPen(-3);
                    FromHandToLineOnShift = false;
                }
                LastPanStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_ScaleRotate.Key) & 0x8000) == 0x8000;
                if (pressed && !LastScaleRotStatus && Root.Hotkey_ScaleRotate.ModifierMatch(control, alt, shift, win))
                {
                    if (Root.ToolSelected != Tools.Scale)
                        btScaleRot_Click(0, null);
                    else
                        btScaleRot_Click(1, null);
                    FromHandToLineOnShift = false;
                }
                LastScaleRotStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Clear.Key) & 0x8000) == 0x8000;
                if (pressed && !LastClearStatus && Root.Hotkey_Clear.ModifierMatch(control, alt, shift, win))
                {
                    if (AltKeyPressed())
                        MouseTimeDown = DateTime.FromBinary(0);
                    else
                        MouseTimeDown = DateTime.Now;
                    btClear_Click(btClear, null);
                    FromHandToLineOnShift = false;
                }
                LastClearStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Video.Key) & 0x8000) == 0x8000;
                if (pressed && !LastVideoStatus && Root.Hotkey_Video.ModifierMatch(control, alt, shift, win))
                {
                    btVideo_Click(null, null);
                    FromHandToLineOnShift = false;
                }
                LastVideoStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_DockUndock.Key) & 0x8000) == 0x8000;
                if (pressed && !LastDockStatus && Root.Hotkey_DockUndock.ModifierMatch(control, alt, shift, win))
                {
                    btDock_Click(null, null);
                    FromHandToLineOnShift = false;
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
                    FromHandToLineOnShift = false;
                }
                LastSnapStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Hand.Key) & 0x8000) == 0x8000;
                if (pressed && !LastHandStatus && Root.Hotkey_Hand.ModifierMatch(control, alt, shift, win))
                {
                    btTool_Click(btHand, null);
                }
                LastHandStatus = pressed;

                // if shift is pressed in handtool we go to line tool temporaly
                if (Root.AltAsOneCommand >= 1)
                {
                    if (Root.ToolSelected == Tools.Hand && (shift || control))
                    {
                        btTool_Click(btLine, null);
                        FromHandToLineOnShift = true;
                    }
                    if (FromHandToLineOnShift && !(shift || control))
                    {
                        btTool_Click(btHand, null);
                        FromHandToLineOnShift = false;
                    }
                }
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
                    FromHandToLineOnShift = false;
                }
                LastRectStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Oval.Key) & 0x8000) == 0x8000;
                if (pressed && !LastOvalStatus && Root.Hotkey_Oval.ModifierMatch(control, alt, shift, win))
                {
                    btTool_Click(btOval, null);
                    FromHandToLineOnShift = false;
                }
                LastOvalStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Arrow.Key) & 0x8000) == 0x8000;
                if (pressed && !LastArrowStatus && Root.Hotkey_Arrow.ModifierMatch(control, alt, shift, win))
                {
                    MouseTimeDown = DateTime.Now;
                    LongHkPress = DateTime.Now.AddSeconds(Root.LongHKPressDelay);
                }
                if (LastArrowStatus && !pressed && DateTime.Now.CompareTo(LongHkPress) < 0)
                {
                    LongHkPress = DateTime.Now.AddYears(1);
                    MouseTimeDown = DateTime.Now;
                    LastArrowStatus = pressed; // btSave will be long... to prevent to restart process...
                    btTool_Click(btArrow, null);
                    FromHandToLineOnShift = false;
                }
                if (LastArrowStatus && pressed && DateTime.Now.CompareTo(LongHkPress) > 0)
                {
                    LongHkPress = DateTime.Now.AddYears(1);
                    MouseTimeDown = DateTime.FromBinary(0);
                    btTool_Click(btArrow, null);
                    FromHandToLineOnShift = false;
                }
                LastArrowStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Numb.Key) & 0x8000) == 0x8000;
                if (pressed && !LastNumbStatus && Root.Hotkey_Numb.ModifierMatch(control, alt, shift, win))
                {
                    MouseTimeDown = DateTime.Now;
                    btTool_Click(btNumb, null);
                    FromHandToLineOnShift = false;
                }
                LastNumbStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Text.Key) & 0x8000) == 0x8000;
                if (pressed && !LastTextStatus && Root.Hotkey_Text.ModifierMatch(control, alt, shift, win))
                {
                    MouseTimeDown = DateTime.Now;
                    btTool_Click(btText, null);
                    FromHandToLineOnShift = false;
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
                    FromHandToLineOnShift = false;
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
                    btZoom_click(null, null);
                }
                LastZoomStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_ClipArt.Key) & 0x8000) == 0x8000;
                if (pressed && !LastClipArtStatus && Root.Hotkey_ClipArt.ModifierMatch(control, alt, shift, win))
                {
                    btTool_Click(btClipArt, null);
                    FromHandToLineOnShift = false;
                }
                LastClipArtStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_ClipArt1.Key) & 0x8000) == 0x8000;
                if (pressed && !LastClipArt1Status && Root.Hotkey_ClipArt1.ModifierMatch(control, alt, shift, win))
                {
                    MouseTimeDown = DateTime.Now;
                    btTool_Click(btClip1, null);
                    FromHandToLineOnShift = false;
                }
                LastClipArt1Status = pressed;

                pressed = (GetKeyState(Root.Hotkey_ClipArt2.Key) & 0x8000) == 0x8000;
                if (pressed && !LastClipArt2Status && Root.Hotkey_ClipArt2.ModifierMatch(control, alt, shift, win))
                {
                    MouseTimeDown = DateTime.Now;
                    btTool_Click(btClip2, null);
                    FromHandToLineOnShift = false;
                }
                LastClipArt2Status = pressed;

                pressed = (GetKeyState(Root.Hotkey_ClipArt3.Key) & 0x8000) == 0x8000;
                if (pressed && !LastClipArt3Status && Root.Hotkey_ClipArt3.ModifierMatch(control, alt, shift, win))
                {
                    MouseTimeDown = DateTime.Now;
                    btTool_Click(btClip3, null);
                    FromHandToLineOnShift = false;
                }
                LastClipArt3Status = pressed;

                pressed = (GetKeyState(Root.Hotkey_PenWidthPlus.Key) & 0x8000) == 0x8000;
                if (pressed && !LastPenWidthPlus && Root.Hotkey_PenWidthPlus.ModifierMatch(control, alt, shift, win))
                {
                    MouseTimeDown = DateTime.Now;
                    PenWidth_Change(Root.PenWidth_Delta);
                    FromHandToLineOnShift = false;
                }
                LastPenWidthPlus = pressed;

                pressed = (GetKeyState(Root.Hotkey_PenWidthMinus.Key) & 0x8000) == 0x8000;
                if (pressed && !LastPenWidthMinus && Root.Hotkey_PenWidthMinus.ModifierMatch(control, alt, shift, win))
                {
                    MouseTimeDown = DateTime.Now;
                    PenWidth_Change(-Root.PenWidth_Delta);
                    FromHandToLineOnShift = false;
                }
                LastPenWidthMinus = pressed;

                pressed = (GetKeyState(Root.Hotkey_ColorPickup.Key) & 0x8000) == 0x8000;
                if (pressed && !LastColorPickupStatus && Root.Hotkey_ColorPickup.ModifierMatch(control, alt, shift, win))
                {
                    if (!Root.ColorPickerMode)
                        StartStopPickUpColor(1);
                    else
                        StartStopPickUpColor(0);
                    FromHandToLineOnShift = false;
                }
                LastColorPickupStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_Lasso.Key) & 0x8000) == 0x8000;
                if (pressed && !LastLassoStatus && Root.Hotkey_Lasso.ModifierMatch(control, alt, shift, win))
                {
                    btLasso_Click(null, null);
                    FromHandToLineOnShift = false;
                }
                LastLassoStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_ColorEdit.Key) & 0x8000) == 0x8000;
                if (pressed && !LastColorEditStatus && Root.Hotkey_ColorEdit.ModifierMatch(control, alt, shift, win))
                {
                    if (Root.CurrentPen >= 0)
                        btColor_LongClick(btPen[Root.CurrentPen]);
                    FromHandToLineOnShift = false;
                }
                LastColorEditStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_LineStyle.Key) & 0x8000) == 0x8000;
                if (pressed && !LastLineStyleStatus && Root.Hotkey_LineStyle.ModifierMatch(control, alt, shift, win))
                {
                    if (Root.CurrentPen >= 0)
                        SelectNextLineStyle(btPen[Root.CurrentPen]);
                    FromHandToLineOnShift = false;
                }
                LastLineStyleStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_PagePrev.Key) & 0x8000) == 0x8000;
                if (pressed && !LastPagePrevStatus && Root.Hotkey_PagePrev.ModifierMatch(control, alt, shift, win))
                {
                    btPagePrev_Click(btPagePrev,null);
                    LastPagePrevStatus = false;
                }
                LastPagePrevStatus = pressed;

                pressed = (GetKeyState(Root.Hotkey_PageNext.Key) & 0x8000) == 0x8000;
                if (pressed && !LastPageNextStatus && Root.Hotkey_PageNext.ModifierMatch(control, alt, shift, win))
                {
                    btPageNext_Click(btPagePrev,null);
                    LastPageNextStatus = false;
                }
                LastPageNextStatus = pressed;

                pressed = ((GetKeyState(Root.Hotkey_LoadStrokes.Key) & 0x8000) == 0x8000) && Root.Hotkey_LoadStrokes.ModifierMatch(control, alt, shift, win);
                if (pressed && !LastLoadStrokesStatus)
                {
                    /*if (AltKeyPressed())
                        MouseTimeDown = DateTime.FromBinary(0);
                    else
                        MouseTimeDown = DateTime.Now;*/
                    LongHkPress = DateTime.Now.AddSeconds(Root.LongHKPressDelay);
                }
                if (LastLoadStrokesStatus && !pressed && DateTime.Now.CompareTo(LongHkPress) < 0)
                {
                    LongHkPress = DateTime.Now.AddYears(1);
                    MouseTimeDown = DateTime.Now;
                    LastSaveStrokesStatus = pressed; // btSave will be long... to prevent to restart process...
                    btLoad_Click(btLoad, null);
                }
                if (LastLoadStrokesStatus && pressed && DateTime.Now.CompareTo(LongHkPress) > 0)
                {
                    LongHkPress = DateTime.Now.AddYears(1);
                    MouseTimeDown = DateTime.FromBinary(0);
                    btLoad_Click(btLoad, null);
                }
                LastLoadStrokesStatus = pressed;

                pressed = ((GetKeyState(Root.Hotkey_SaveStrokes.Key) & 0x8000) == 0x8000) && Root.Hotkey_SaveStrokes.ModifierMatch(control, alt, shift, win);
                //if (pressed && !LastSaveStrokesStatus && Root.Hotkey_SaveStrokes.ModifierMatch(control, alt, shift, win))
                if (pressed && !LastSaveStrokesStatus)
                {
                    /*if (AltKeyPressed())
                        MouseTimeDown = DateTime.FromBinary(0);
                    else
                        MouseTimeDown = DateTime.Now;*/
                    LongHkPress = DateTime.Now.AddSeconds(Root.LongHKPressDelay);
                }
                if (LastSaveStrokesStatus && !pressed && DateTime.Now.CompareTo(LongHkPress) < 0)
                {
                    LongHkPress = DateTime.Now.AddYears(1);
                    MouseTimeDown = DateTime.Now;
                    LastSaveStrokesStatus = pressed; // btSave will be long... to prevent to restart process...
                    btSave_Click(btSave, null);
                }
                if (LastSaveStrokesStatus && pressed && DateTime.Now.CompareTo(LongHkPress) > 0)
                {
                    LongHkPress = DateTime.Now.AddYears(1);
                    MouseTimeDown = DateTime.FromBinary(0);
                    btSave_Click(btSave, null);
                }
                LastSaveStrokesStatus = pressed;

                //Console.WriteLine("LongHkPress" + LongHkPress.ToBinary().ToString());
            }

            if (Root.Snapping < 0)
                Root.Snapping++;
            if (Tick % 100 == 0)
                GC.Collect();

        }

        public void recomputePensSet(int firstPen = -1, int currentPen = -1)
        {
            if(firstPen<0)
            {
                firstPen = FirstPenDisplayed;
                currentPen = Root.CurrentPen;
            }
            int i;
            bool sel;
            for (int b = 0; b < Root.MaxDisplayedPens; b++)
            {

                if (firstPen < Root.MaxDisplayedPens)
                    if ((currentPen >= Root.MaxDisplayedPens) && ((b + Root.MaxDisplayedPens) == currentPen))
                        sel = true;
                    else
                        sel = b == currentPen;
                else
                    sel = (b + Root.MaxDisplayedPens) == currentPen;

                if (sel)
                    i = currentPen - (currentPen % Root.MaxDisplayedPens);
                else
                    i = firstPen;
                try
                {
                    btPen[b].BackgroundImage.Dispose();
                }
                catch { };
                btPen[b].BackgroundImage = buildPenIcon(Root.PenAttr[b + i].Color, Root.PenAttr[b + i].Transparency, sel,
                                        Root.PenAttr[b + i].ExtendedProperties.Contains(Root.FADING_PEN),
                                        Root.LineStyleToString(Root.PenAttr[b + i].ExtendedProperties), Root.PenAttr[b + i].Width);// image_pen[b];
            }
            Root.UponAllDrawingUpdate = true;
            Root.UponButtonsUpdate |= 0x7;
        }

        public void PenWidth_Change(int n)
        {
            Root.GlobalPenWidth += n;
            if (Root.GlobalPenWidth < 1)
                Root.GlobalPenWidth = 1;
            IC.DefaultDrawingAttributes.Width = Root.GlobalPenWidth;
            //if (Root.CanvasCursor == 1)
            if(Root.ToolSelected != Tools.Edit && Root.ToolSelected != Tools.txtLeftAligned && Root.ToolSelected != Tools.txtRightAligned && Root.ToolSelected != Tools.NumberTag && Root.ToolSelected != Tools.Invalid)
                SetPenTipCursor();
            return;
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
			HitMovingToolbareXY.X = MousePosition.X;
			HitMovingToolbareXY.Y = MousePosition.Y;
		}

		private void gpButtons_MouseMove(object sender, MouseEventArgs e)
		{            
			if (IsMovingToolbar == 1)
			{
				if (Math.Abs(MousePosition.X - HitMovingToolbareXY.X) > 20 || Math.Abs(MousePosition.Y - HitMovingToolbareXY.Y) > 20)
					IsMovingToolbar = 2;
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
            PolyLineLastX = Int32.MinValue; PolyLineLastY = Int32.MinValue; PolyLineInProgress = null;
            Root.SetInkVisible(!Root.InkVisible);
		}

        private Stroke AddBackGround(int A, int B, int C, int D)
        {
            Stroke stk = AddRectStroke(0,0,Width ,Height , Filling.PenColorFilled);
            stk.DrawingAttributes.Transparency = (byte)(255 - A);
            stk.DrawingAttributes.Color = Color.FromArgb(A, B, C, D);
            stk.ExtendedProperties.Add(Root.ISBACKGROUND_GUID, true);
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
                return Int32.Parse((prompt.Tag as Control).Name);
            else
                return -1;
        }

        public void FadingToggle(int pen)
        {
            if (pen < 0)
                return;
            if (AltKeyPressed() && Root.PenAttr[Root.SavedPenDA] == null)
                Root.PenAttr[Root.SavedPenDA] = Root.PenAttr[pen].Clone();

            if (Root.PenAttr[pen].ExtendedProperties.Contains(Root.FADING_PEN))
            {
                try { Root.PenAttr[pen].ExtendedProperties.Remove(Root.FADING_PEN); } catch { };
            }
            else
                Root.PenAttr[pen].ExtendedProperties.Add(Root.FADING_PEN, Root.TimeBeforeFading);
            //btPen[pen].BackgroundImage = buildPenIcon(Root.PenAttr[pen].Color, Root.PenAttr[pen].Transparency, true, Root.PenAttr[pen].ExtendedProperties.Contains(Root.FADING_PEN));
            //Root.UponButtonsUpdate |= 0x2;
            SelectPen(pen);
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

            StopAllZooms();

            TimeSpan tsp = DateTime.Now - MouseTimeDown;
            PolyLineLastX = Int32.MinValue; PolyLineLastY = Int32.MinValue; PolyLineInProgress = null;
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
            InprogressSelection = null;
            StrokesSelection.Clear();
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

            if (Root.LassoMode)
            {
                InprogressSelection = null;
                Root.UponAllDrawingUpdate = true;
            }
            else
			    Root.UndoInk();
		}

        public void SelectNextLineStyle(object sender)
        {   
            for (int b = 0; b < Root.MaxPenCount; b++)
                if ((Button)sender == btPen[b])
                {
                    // inspired from FormOptions / comboPensLineStyle_Changed
                    if (AltKeyPressed() && Root.PenAttr[Root.SavedPenDA] == null)
                        Root.PenAttr[Root.SavedPenDA] = Root.PenAttr[b].Clone();
                    string s = Root.NextLineStyleString(Root.LineStyleToString(Root.PenAttr[b].ExtendedProperties),true);
                    DashStyle ds = Root.LineStyleFromString(s);
                    if (ds == DashStyle.Custom)
                        try { Root.PenAttr[b].ExtendedProperties.Remove(Root.DASHED_LINE_GUID); } catch { }
                    else
                        Root.PenAttr[b].ExtendedProperties.Add(Root.DASHED_LINE_GUID, ds);
                    btPen[b].BackgroundImage = buildPenIcon(Root.PenAttr[b + FirstPenDisplayed].Color, Root.PenAttr[b + FirstPenDisplayed].Transparency, false,
                                                            Root.PenAttr[b + FirstPenDisplayed].ExtendedProperties.Contains(Root.FADING_PEN), 
                                                            Root.LineStyleToString(Root.PenAttr[b + FirstPenDisplayed].ExtendedProperties),Root.PenAttr[b + FirstPenDisplayed].Width);
                    if((b + FirstPenDisplayed) == Root.CurrentPen)
                        SelectPen(Root.CurrentPen);
                    Root.UponButtonsUpdate |= 0x2;  // necessary in case b!= from CurrentPen
                }
        }

        public void btColor_LongClick(object sender)
        {
            for (int b = 0; b < Root.MaxPenCount; b++)
                if ((Button)sender == btPen[b])
                {
                    AllowInteractions(true);
                    //ToThrough();
                    TextEdited = true;

                    SelectPen(b + FirstPenDisplayed);
                    Root.UponButtonsUpdate |= 0x2;
                    float w = Root.PenAttr[b + FirstPenDisplayed].Width;
                    if (PenModifyDlg.ModifyPen(ref Root.PenAttr[b + FirstPenDisplayed]))
                    {
                        if(w!= Root.PenAttr[b + FirstPenDisplayed].Width)
                            LastPenSelected = -1;
                        if ((Root.ToolSelected == Tools.Move) || (Root.ToolSelected == Tools.Copy) || (Root.ToolSelected == Tools.Edit)) // if move
                            SelectTool(Tools.Hand,Filling.Empty);
                        //PreparePenImages(Root.PenAttr[b].Transparency, ref image_pen[b], ref image_pen_act[b]);
                        //btPen[b].Image = image_pen_act[b];
                        btPen[b].BackgroundImage = buildPenIcon(Root.PenAttr[b + FirstPenDisplayed].Color, Root.PenAttr[b + FirstPenDisplayed].Transparency, false,
                                                                Root.PenAttr[b + FirstPenDisplayed].ExtendedProperties.Contains(Root.FADING_PEN),
                                                                Root.LineStyleToString(Root.PenAttr[b + FirstPenDisplayed].ExtendedProperties), Root.PenAttr[b + FirstPenDisplayed].Width);// image_pen[b];
                        //btPen[b].FlatAppearance.MouseDownBackColor = Root.PenAttr[b].Color;
                        //btPen[b].FlatAppearance.MouseOverBackColor = Root.PenAttr[b].Color;
                        SelectPen(b + FirstPenDisplayed);
                        Root.UponButtonsUpdate |= 0x2;
                    };
                    AllowInteractions(false);
                    //ToUnThrough();
                }
            if (ExtraPensByClick)
            {
                FirstPenDisplayed = 0;
                while (!Root.PenEnabled[FirstPenDisplayed])
                    FirstPenDisplayed++;
                recomputePensSet(FirstPenDisplayed, Root.CurrentPen);
                ExtraPensByClick = false;
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
            PolyLineLastX = Int32.MinValue; PolyLineLastY = Int32.MinValue; PolyLineInProgress = null;
            TimeSpan tsp = DateTime.Now - MouseTimeDown;
            //Console.WriteLine(string.Format("{1},t = {0:N3}", tsp.TotalSeconds,e.ToString()));
            if (sender != null && tsp.TotalSeconds > Root.LongClickTime)
            {
                btColor_LongClick(sender);
            }

            for (int b = 0; b < Root.MaxPenCount; b++)
                if ((Button)sender == btPen[b])
                {
                    if (Root.ButtonClick_For_LineStyle && b == Root.CurrentPen)
                        SelectNextLineStyle(btPen[b]);
                    SelectPen(b + FirstPenDisplayed);
                    if (Root.ToolSelected == Tools.Invalid || Root.ToolSelected == Tools.Move || Root.ToolSelected == Tools.Copy || Root.ToolSelected == Tools.Scale || Root.ToolSelected == Tools.Rotate
                        || (Root.ToolSelected == Tools.Edit || Root.PanMode  || Root.EraserMode )) // if move
                        SelectTool(Tools.Hand, Filling.Empty);
                }
            if(ExtraPensByClick)
            {
                FirstPenDisplayed = 0;
                while (!Root.PenEnabled[FirstPenDisplayed])
                    FirstPenDisplayed++;
                recomputePensSet(FirstPenDisplayed, Root.CurrentPen);
                ExtraPensByClick = false;
            }        
        }

        bool ExtraPensByClick = false;

        private void ExtraPensBtn_Click(object sender, EventArgs e)
        {
            if (FirstPenDisplayed == 0)
                FirstPenDisplayed = Root.MaxDisplayedPens;
            else
                FirstPenDisplayed = 0;
            while (!Root.PenEnabled[FirstPenDisplayed])
                FirstPenDisplayed++;
            recomputePensSet(FirstPenDisplayed, Root.CurrentPen);
            ExtraPensByClick = true;
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
            PolyLineLastX = Int32.MinValue; PolyLineLastY = Int32.MinValue; PolyLineInProgress = null;
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
            Root.CurrentVideoFileName = Root.ExpandVarCmd(Root.FFMpegFileName, rect.X, rect.Y, rect.Width, rect.Height);
            Root.IndexRecordCounter = 0;
            Root.CurrentVideoStartTime = DateTime.Now;
            string[] cmdArgs = Root.ExpandVarCmd(Root.FFMpegCmd, rect.X, rect.Y, rect.Width, rect.Height).Split(new char[] { ' ' }, 2);
            Console.WriteLine(cmdArgs[0]+" "+cmdArgs[1]);

            Root.FFmpegProcess.StartInfo.FileName = cmdArgs[0];
            Root.FFmpegProcess.StartInfo.Arguments = cmdArgs[1];

            Root.FFmpegProcess.StartInfo.UseShellExecute = false;
            Root.FFmpegProcess.StartInfo.CreateNoWindow = true;
            Root.FFmpegProcess.StartInfo.RedirectStandardInput  = true;
            Root.FFmpegProcess.StartInfo.RedirectStandardOutput = true;
            Root.FFmpegProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            Root.FFmpegProcess.Start();
            IntPtr ptr = Root.FFmpegProcess.MainWindowHandle;
            ShowWindow(ptr.ToInt32(), 2);

            Root.VideoRecInProgress = VideoRecInProgress.Recording;
            SetVidBgImage();
            //ExitSnapping();
        }

        DateTime ObsTimeCode;
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
            {
                frm.Root.VideoRecordWindowInProgress = false;
                return;
            }
#if !ppInkSmall
            if (frm.Root.ObsWs == null)
            {
                frm.Root.ObsWs = new ClientWebSocket();
                //Console.WriteLine("WS Created");
            }
#else
            return;
#endif
            var rcvBytes = new byte[4096];
            var rcvBuffer = new ArraySegment<byte>(rcvBytes);
            WebSocketReceiveResult rcvResult;
            if (frm.Root.ObsWs.State != WebSocketState.Open)
            {
                try
                {
                    await frm.Root.ObsWs.ConnectAsync(new Uri(frm.Root.ObsUrl), new CancellationTokenSource(200).Token);

                    await SendInWs(frm.Root.ObsWs, "GetAuthRequired", ct);
                    rcvResult = await frm.Root.ObsWs.ReceiveAsync(rcvBuffer, ct);
                    string st = Encoding.UTF8.GetString(rcvBuffer.Array, 0, rcvResult.Count);

                    var ReturnStructure = (Dictionary<string, object>)st.FromJson<object>();
                    if ((ReturnStructure["authRequired"] as bool?) != true)
                        throw new Exception("authentification not required whereas mandatory");
                    string challenge = ReturnStructure["challenge"] as string;// st.Substring(i, j - i);
                    string salt = ReturnStructure["salt"] as string;// st.Substring(i, j - i);

                    string authResponse = HashEncode(HashEncode(frm.Root.ObsPwd + salt) + challenge);
                    await SendInWs(frm.Root.ObsWs, "Authenticate", ct, ",\"auth\": \"" + authResponse + "\"");
                    rcvResult = await frm.Root.ObsWs.ReceiveAsync(rcvBuffer, ct);
                    st = Encoding.UTF8.GetString(rcvBuffer.Array, 0, rcvResult.Count);
                    if (!st.Contains("\"ok\""))
                        throw new Exception("authentification failed");
                }
                catch
                {
                    try
                    {
                        await frm.Root.ObsWs.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Authentication failed", new CancellationTokenSource(200).Token);
                    }
                    catch { };
                    frm.Root.ObsWs = null;
                    frm.Root.ObsRecvTask = null;
                    frm.Root.VideoRecInProgress = VideoRecInProgress.Dead;
                    frm.Root.VideoRecordWindowInProgress = false;
                    frm.Root.ObsCancel.Cancel();
                    frm.btVideo.BackgroundImage = FormCollection.getImgFromDiskOrRes("VidDead", frm.ImageExts);
                }
            }
            frm.Root.VideoRecordWindowInProgress = false;
            while (frm.Root.ObsWs != null && frm.Root.ObsWs.State == WebSocketState.Open && !ct.IsCancellationRequested) // && frm.Root.VideoRecInProgress == VideoRecInProgress.Recording )
            {
                //Console.WriteLine("Awaiting....");
                rcvResult = await frm.Root.ObsWs.ReceiveAsync(rcvBuffer, ct);
                //Console.WriteLine("Received....");
                if (ct.IsCancellationRequested)
                    return;
                string st = Encoding.UTF8.GetString(rcvBuffer.Array, 0, rcvResult.Count);
                //Console.WriteLine("resp = " + st);
                var ReturnStructure = (Dictionary<string, object>)st.FromJson<object>();
                object getFromReturnStructure(string k)
                {
                    object o;
                    if (!ReturnStructure.TryGetValue(k, out o))
                        o = null;
                    return o;
                }

                try { frm.Root.CurrentVideoFileName = ReturnStructure["recordingFilename"] as string; } catch { }
                try
                {
                    string stime = ReturnStructure["recordTimecode"] as string;
                    frm.ObsTimeCode = DateTime.ParseExact(stime, "H:mm:ss.fff", CultureInfo.InvariantCulture);                    
                } catch { }

                if (st.Contains("\"RecordingStopped\""))
                    frm.Root.VideoRecInProgress = VideoRecInProgress.Stopped;
                else if (st.Contains("\"RecordingPaused\""))
                    frm.Root.VideoRecInProgress = VideoRecInProgress.Paused;
                else if (st.Contains("StreamStopping"))
                    frm.Root.VideoRecInProgress = VideoRecInProgress.Stopped;
                else if (st.Contains("StreamStarted"))
                    frm.Root.VideoRecInProgress = VideoRecInProgress.Streaming;
                else if (st.Contains("\"RecordingStarted\"") || st.Contains("\"RecordingStarting\"") || st.Contains("\"RecordingResumed\""))
                    frm.Root.VideoRecInProgress = VideoRecInProgress.Recording;
                // cases from getInitialStatus;
                //else if (st.Contains("\"recording - paused\": true") || st.Contains("\"recording-paused\": true") || st.Contains("\"isRecordingPaused\": true"))
                else if ((getFromReturnStructure("recording-paused") as bool? == true) || (getFromReturnStructure("isRecordingPaused") as bool? == true))
                    frm.Root.VideoRecInProgress = VideoRecInProgress.Paused;
                //else if (st.Contains("\"recording\": true") || st.Contains("\"isRecording\": true"))
                else if ((getFromReturnStructure("recording") as bool? == true)|| (getFromReturnStructure("isRecording") as bool? == true))
                    frm.Root.VideoRecInProgress = VideoRecInProgress.Recording;
                //else if (st.Contains("\"streaming\": true"))
                else if ((getFromReturnStructure("streaming") as bool? == true))
                    frm.Root.VideoRecInProgress = VideoRecInProgress.Streaming;
                //else if (st.Contains("\"recording\": false") || st.Contains("\"isRecording\": false") || st.Contains("\"streaming\": false"))
                else if ((getFromReturnStructure("recording") as bool? != true) && (getFromReturnStructure("isRecording") as bool? != true) && (getFromReturnStructure("streaming") as bool? != true))
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
#if ppInkSmall
            return;
#endif
            while ((frm.Root.ObsWs == null || frm.Root.VideoRecordWindowInProgress) && !frm.Root.ObsCancel.Token.IsCancellationRequested)// frm.Root.ObsWs.State != WebSocketState.Open)
                await Task.Delay(50);
            frm.Root.CurrentIndexFileName = "";
            frm.Root.IndexRecordCounter = 0;
            if (frm.Root.VideoRecordMode == VideoRecordMode.OBSRec)
                await Task.Run(() => SendInWs(frm.Root.ObsWs, "StartRecording", frm.Root.ObsCancel.Token));
            else if (frm.Root.VideoRecordMode == VideoRecordMode.OBSBcst)
                await Task.Run(() => SendInWs(frm.Root.ObsWs, "StartStreaming", frm.Root.ObsCancel.Token));
            await Task.Delay(100);
            await Task.Run(() => SendInWs(frm.Root.ObsWs, "GetStreamingStatus", frm.Root.ObsCancel.Token));
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
            Root.CurrentVideoFileName = "";
            Root.CurrentIndexFileName = "";            
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
            int i = (int)(DateTime.UtcNow.TimeOfDay.TotalMilliseconds);
            //Console.WriteLine(i.ToString()+" : enter " + cmd);
            string msg = string.Format("{{\"message-id\":\"{0}\",\"request-type\":\"{1}\" {2} }}", i, cmd, parameters);
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

        public void SetTagNumber(string init = "")
        {
            AllowInteractions(true);
            //ToThrough();
            int k = -1;
            FormInput inp = new FormInput(Root.Local.DlgTagCaption, Root.Local.DlgTagLabel, "", false, Root, null);
            string s = init;
            if (init == "")
                s = String.Format(Root.TagFormatting, Root.TagNumbering, (Char)(64 + Root.TagNumbering), (Char)(96 + Root.TagNumbering));
            inp.TextIn(s);
            while (inp.TextOut().Length > 0)
            {
                if (init == "")
                    if (inp.ShowDialog() == DialogResult.Cancel)
                    {
                        inp.TextIn("");
                        break;
                    }
                init = "";
                if (Int32.TryParse(inp.TextOut(), out k))
                {
                    Root.TagFormatting = Root.TagFormattingList[0];
                    break;
                }
                if ('A' <= inp.TextOut()[0] && inp.TextOut()[0] <= 'Z')
                {
                    k = inp.TextOut()[0] - 'A' + 1;
                    Root.TagFormatting = Root.TagFormattingList[1];
                    break;
                }
                if ('a' <= inp.TextOut()[0] && inp.TextOut()[0] <= 'z')
                {
                    k = inp.TextOut()[0] - 'a' + 1;
                    Root.TagFormatting = Root.TagFormattingList[2];
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
            if(ClipartsDlg.Visible)
            {
                //Console.WriteLine("Close ClipArtDlg");
                ClipartsDlg.Close();
            }

            int i = -1;
            if (((Button)sender).Name.Contains("Hand"))
            {
                CustomizeAndOpenSubTools(-1 , "SubToolsHand", new string[] { "tool_hand_act", "tool_hand_filledC", "tool_hand_out", "tool_hand_filledW", "tool_hand_filledB" },Root.Local.HandSubToolsHints,
                                     new Func<int, bool>[] { ii => { SelectTool(Tools.Hand,Filling.Empty); return true; },
                                                             ii => { SelectTool(Tools.Hand,Filling.PenColorFilled); return true; },
                                                             ii => { SelectTool(Tools.Hand,Filling.Outside ); return true; },
                                                             ii => { SelectTool(Tools.Hand,Filling.WhiteFilled); return true; },
                                                             ii => { SelectTool(Tools.Hand,Filling.BlackFilled ); return true; } });
                i = Tools.Hand;
                
            }
            else if (((Button)sender).Name.Contains("Line"))
            {
                CustomizeAndOpenSubTools(-1, "SubToolsLines", new string[] { "tool_line_act", "tool_mlines", "tool_mlines_filledC", "tool_mlines_out", "tool_mlines_filledW", "tool_mlines_filledB" }, Root.Local.LineSubToolsHints,
                                     new Func<int, bool>[] { ii => { SelectTool(Tools.Line,Filling.Empty); return true; },
                                                             ii => { SelectTool(Tools.Poly ,Filling.Empty); return true; },
                                                             ii => { SelectTool(Tools.Poly ,Filling.PenColorFilled); return true; },
                                                             ii => { SelectTool(Tools.Poly, Filling.Outside); return true; },
                                                             ii => { SelectTool(Tools.Poly,Filling.WhiteFilled); return true; },
                                                             ii => { SelectTool(Tools.Poly,Filling.BlackFilled ); return true; } });
            i = Root.ToolSelected == Tools.Poly ? Tools.Poly : Tools.Line;    // to keep filled
            }

            else if (((Button)sender).Name.Contains("Rect"))
            {
                CustomizeAndOpenSubTools(-1, "SubToolsRect", new string[] { "tool_rect_act", "tool_rect_filledC", "tool_rect_out", "tool_rect_filledW", "tool_rect_filledB" }, Root.Local.RectSubToolsHints,
                                     new Func<int, bool>[] { ii => { SelectTool(Tools.Rect,Filling.Empty); return true; },
                                                             ii => { SelectTool(Tools.Rect,Filling.PenColorFilled); return true; },
                                                             ii => { SelectTool(Tools.Rect,Filling.Outside); return true; },
                                                             ii => { SelectTool(Tools.Rect,Filling.WhiteFilled); return true; },
                                                             ii => { SelectTool(Tools.Rect,Filling.BlackFilled); return true; } });
                i = Tools.Rect;

            }
            else if (((Button)sender).Name.Contains("Oval"))
            {
                CustomizeAndOpenSubTools(-1, "SubToolsOval", new string[] { "tool_oval_act", "tool_oval_filledC", "tool_oval_out", "tool_oval_filledW", "tool_oval_filledB" }, Root.Local.OvalSubToolsHints,
                                     new Func<int, bool>[] { ii => { SelectTool(Tools.Oval,Filling.Empty); return true; },
                                                             ii => { SelectTool(Tools.Oval,Filling.PenColorFilled); return true; },
                                                             ii => { SelectTool(Tools.Oval,Filling.Outside ); return true; },
                                                             ii => { SelectTool(Tools.Oval,Filling.WhiteFilled); return true; },
                                                             ii => { SelectTool(Tools.Oval,Filling.BlackFilled ); return true; } });
                i = Tools.Oval;

            }
            else if (((Button)sender).Name.Contains("Arrow"))
            {
                CustomizeAndOpenSubTools(-1, "SubToolsArrow", new string[] { "tool_enAr_act", "tool_stAr_act" }, Root.Local.ArrowSubToolsHints,
                                     new Func<int, bool>[] { ii => { SelectTool(Tools.StartArrow ,Filling.Empty); return true; },
                                                             ii => { SelectTool(Tools.EndArrow ,Filling.Empty); return true; } });
                if (sender != null && tsp.TotalSeconds > Root.LongClickTime)
                {
                    AllowInteractions(true);
                    ArrowSelDlg dlg = new ArrowSelDlg(Root);
                    dlg.ShowDialog();//==DialogResult.Cancel
                    if(--Root.CurrentArrow<0)
                        Root.CurrentArrow=Root.ArrowHead.Count-1;
                    AllowInteractions(false);
                }
                i = Root.DefaultArrow_start ? Tools.EndArrow : Tools.StartArrow;
            }
            else if (((Button)sender).Name.Contains("Numb"))
            {
                CustomizeAndOpenSubTools(-1, "SubToolsNumb", new string[] { "tool_numb_fillW", "tool_numb_fillB" }, Root.Local.OvalSubToolsHints,
                     new Func<int, bool>[] { 
                                                             ii => { SelectTool(Tools.NumberTag,Filling.WhiteFilled); return true; },
                                                             ii => { SelectTool(Tools.NumberTag,Filling.BlackFilled ); return true; } });

                if (sender != null && tsp.TotalSeconds > Root.LongClickTime)
                {
                    TagFontBtn_Modify();
                    return;
                }
                else
                    i = Tools.NumberTag;
            }
            else if (((Button)sender).Name.Contains("Text"))
            {
                CustomizeAndOpenSubTools(-1, "SubToolsText", new string[] { "tool_txtL_act", "tool_txtR_act" }, Root.Local.TextSubToolsHints,
                                     new Func<int, bool>[] { ii => { SelectTool(Tools.txtLeftAligned ,Filling.Empty); return true; },
                                                             ii => { SelectTool(Tools.txtRightAligned ,Filling.Empty); return true; } });

                i = Tools.txtLeftAligned;
            }
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
                if (ClipartsDlg.Visible)
                {
                    ClipartsDlg.Hide();
                    ClipartsDlg.Visible = false;
                }
                i = -1;
                if (ClipartsDlg.ShowDialog() == DialogResult.OK)
                {
                    //Root.ImageStamp = new ClipArtData { ImageStamp = ClipartsDlg.ImageStamp, X = ClipartsDlg.ImgSizeX, Y = ClipartsDlg.ImgSizeY, Filling = ClipartsDlg.ImageStampFilling,PatternLine = ClipartsDlg.PutClipartOnLine };
                    Root.ImageStamp = ClipartsDlg.getClipArtData();
                    if (ClipartsDlg.PutClipartOnLine)
                        i = Tools.PatternLine;
                    else
                        i = Tools.ClipArt;
                    
                    LineForPatterns = null;
                    PatternLastPtIndex = -1;
                    PatternLastPtRemain = 0;
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
                            //((Button)sender).Tag = new ClipArtData { ImageStamp = dlg.ImageStamp, X = dlg.ImgSizeX, Y = dlg.ImgSizeY, Filling = dlg.ImageStampFilling, PatternLine = ClipartsDlg.PutClipartOnLine };
                            ((Button)sender).Tag = dlg.getClipArtData();
                            if (dlg.PutClipartOnLine)
                                i = Tools.PatternLine;
                            else
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
                if (((ClipArtData)btClipSel).PatternLine)
                    i = Tools.PatternLine;
                else
                    i = Tools.ClipArt;
            }
            int f = -1;
            if (!AltKeyPressed() && Root.AltAsOneCommand>=1 & (Root.PointerMode || Root.EraserEnabled || Root.PanMode || Root.LassoMode) & SavedTool != -1) 
            {
                SavedPen = -1;
                SavedTool = -1;
                f = SavedFilled;
                SavedFilled = -1;
            }
            if (i >= Tools.Hand)
                SelectPen(LastPenSelected);

            ////////
            ///
            // Forcer le remplissage par défaut au premier clic sur NumberTag (éviter l'état Empty)
            // Forcer le remplissage UNIQUEMENT lors de la première activation de NumberTag
            if (i == Tools.NumberTag && f < Filling.Empty && Root.ToolSelected != Tools.NumberTag)
            {
                int df = (Root.FilledSelected == Filling.WhiteFilled || Root.FilledSelected == Filling.BlackFilled)
                ? Root.FilledSelected
                : Filling.WhiteFilled; // ou Filling.BlackFilled si tu préfères démarrer en noir
                f = df;
            }
            //////
            ///





            SelectTool(i,f);
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
            CustomizeAndOpenSubTools(-1, "PanSubTools",new string[] { "pan1_act" , "pan_copy","pan_act"  } , Root.Local.PanSubToolsHints,
                                     new Func<int, bool>[] { i => { SelectPen(LastPenSelected); SelectTool(Tools.Move); return true; },
                                                             i => { SelectPen(LastPenSelected); SelectTool(Tools.Copy); return true; },
                                                             i => { SelectPen(-3); return true; } });
			if (ToolbarMoved)
			{
				ToolbarMoved = false;
				return;
            }
            if(sender is int)
            {
                changeActiveTool((int)sender, true, Root.SubToolsEnabled ? 1 : -1);
            }
            else if (Root.ToolSelected == Tools.Move)
            {
                changeActiveTool(1, true, Root.SubToolsEnabled ? 1 : -1);
            }
            else if (Root.ToolSelected == Tools.Copy)
            {
                changeActiveTool(2, true, Root.SubToolsEnabled ? 1 : -1);
            }
            else
            {
                changeActiveTool(0, true, Root.SubToolsEnabled ? 1 : -1);
            }
        }

        private void btScaleRot_Click(object sender, EventArgs e)
        {
            CustomizeAndOpenSubTools(-1, "PanSubTools", new string[] { "scale_act", "rotate_act" }, Root.Local.ScaleSubToolsHints,
                                     new Func<int, bool>[] { i => { SelectPen(LastPenSelected); SelectTool(Tools.Scale); return true; },
                                                             i => { SelectPen(LastPenSelected); SelectTool(Tools.Rotate); return true; }});
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }
            if (sender is int)
            {
                changeActiveTool((int)sender, true, Root.SubToolsEnabled ? 1 : -1);
            }
            else if (Root.ToolSelected == Tools.Scale)
            {
                changeActiveTool(1, true, Root.SubToolsEnabled ? 1 : -1);
            }
            else
            {
                changeActiveTool(0, true, Root.SubToolsEnabled ? 1 : -1);
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

        public bool ZoomCapturing=false;
        public bool ZoomCaptured=false;
        public bool SpotLightMode = false;
        public bool SpotLightTemp = false;
       
        private void btZoom_click(object sender, EventArgs e)
        {
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }
            if (ZoomForm.Visible)
            {
                ZoomForm.Hide();
                if ((Root.ZoomEnabled & 2) != 0)
                {
                    StartZoomCapt();
                }
                else
                {
                    SpotLightMode = true;
                    btZoom.BackgroundImage = getImgFromDiskOrRes("flashLight");
                }
            }
            else if (ZoomCapturing || ZoomCaptured)
            {
                bool z = ZoomCaptured;
                StopAllZooms();
                if(!z)
                {
                    SpotLightMode = true;
                    btZoom.BackgroundImage = getImgFromDiskOrRes("flashLight");
                }
            }
            else if (SpotLightMode)
            {
                StopAllZooms();
            }
            else
            {
                if ((Root.ZoomEnabled & 1) != 0)
                    ActivateZoomDyn();
                else // if((Root.ZoomEnabled & 2)!=0)
                    StartZoomCapt();
            }
            Root.UponButtonsUpdate |= 0x2;
        }

        public void StopAllZooms()
        {
            ZoomForm.Hide();
            if (ZoomCaptured)
            {
                IC.Ink.DeleteStrokes();
                LoadStrokes(ZoomSaveStroke);
                Root.UponAllDrawingUpdate = true;
                Root.FormDisplay.timer1_Tick(null, null);
            }
            ZoomCapturing = false;
            ZoomCaptured = false;
            //if (Root.CanvasCursor == 1)
            SetPenTipCursor();
            SpotLightMode = false;
            btZoom.BackgroundImage = getImgFromDiskOrRes("Zoom");
            Root.UponButtonsUpdate |= 0x2;
        }

        public void StartZoomCapt()
        {
            ZoomForm.Hide();
            ZoomCapturing = true;
            try
            {
                IC.Cursor = cursorred;
            }
            catch
            {
                IC.Cursor = getCursFromDiskOrRes(Root.cursorarrowFileName, System.Windows.Forms.Cursors.NoMove2D);
            }
            btZoom.BackgroundImage = getImgFromDiskOrRes("ZoomWin_act");
        }

        public void ActivateZoomDyn()
        {
            //if (Root.CanvasCursor == 1)
            SetPenTipCursor();
            ZoomForm.Width = (int)(Root.ZoomWidth * Root.ZoomScale);
            ZoomForm.Height = (int)(Root.ZoomHeight * Root.ZoomScale);
            ZoomForm.Show();
            btZoom.BackgroundImage = getImgFromDiskOrRes("Zoom_act");
        }

        public void ActivateSpot()
        {
            StopAllZooms();
            SpotLightMode = true;
            btZoom.BackgroundImage = getImgFromDiskOrRes("flashLight");
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
					ExitSnapping(false);
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

        // active (-1 == none)
        // strings [inactive,active,...] ; empty string = button non visible
        // strings TextHints
        // lambda fonctions when clicking

        int subTools_activeTool = -1;
        string subTools_title;
        string[] subTools_icons=null;
        Func<int, bool>[] subTools_actions=null;

        const int ACTIVE_SUBTOOL_BORDERSIZE=3;

        public void CustomizeAndOpenSubTools(int active, string title, string [] icons, string TextHintsStr, Func<int,bool> [] clickFuncts)
        {
            if (title != "" && gpSubTools.Visible && title == subTools_title ) // already configured
                return;

            int dim = (int)Math.Round(Screen.PrimaryScreen.Bounds.Height * Root.ToolbarHeight);
            int dim2s = (int)(dim * SmallButtonNext);
            int dim3 = (int)(dim * InterButtonGap);

            subTools_title = title;
            subTools_activeTool = active;
            subTools_icons = icons;
            subTools_actions = clickFuncts;
            string[] TextHints = TextHintsStr.Split('\n');
            for(int i=0 ; i<Btn_SubTools.Length ; i++)
            {
                if (i <= icons.GetLength(0) - 1)
                {
                    Btn_SubTools[i].Visible = true;
                    Btn_SubTools[i].BackgroundImage = getImgFromDiskOrRes(icons[i]);
                    Btn_SubTools[i].FlatAppearance.BorderSize = i == active ? ACTIVE_SUBTOOL_BORDERSIZE : 0;
                    try
                    {
                        toolTip.SetToolTip(Btn_SubTools[i], TextHints[i]);
                    }
                    catch
                    {
                        toolTip.SetToolTip(Btn_SubTools[i], "");
                    }
                }
                else
                {
                    Btn_SubTools[i].Visible = false;
                }
                if (i == icons.GetLength(0)-1)
                {
                    int o = Root.ToolbarOrientation <= Orientation.Horizontal ? Orientation.toLeft : Orientation.toUp;
                    SetButtonPosition(Btn_SubTools[i], Btn_SubToolClose, dim3,o);
                    SetSmallButtonNext(Btn_SubToolClose, Btn_SubToolPin, dim2s,o);
                    if (Root.ToolbarOrientation <= Orientation.Horizontal)
                        gpSubTools.Width = Btn_SubToolClose.Right;
                    else
                        gpSubTools.Height = Btn_SubToolClose.Bottom;
                }
            }
            if(!gpSubTools.Visible)
            {
                SetSubBarPosition(gpSubTools, btHand);
            }
            gpSubTools.Visible = Root.SubToolsEnabled;
            Root.UponAllDrawingUpdate = true;
            Root.UponButtonsUpdate |= 0x7;
            Task.Run(() => { for (int i = 1; i <= 10; i++)
                             {
                                ColorMatrix cm = new ColorMatrix();
                                cm.Matrix00 = 1f; cm.Matrix11 = 1f; cm.Matrix22 = 1f;
                                cm.Matrix33 = i *Root.ToolbarBGColor[0] / 2550f;
                                try{ Root.FormDisplay.iaSubToolsTransparency.SetColorMatrix(cm); }catch { };
                                Root.UponAllDrawingUpdate = true;
                                Thread.Sleep(30);
                             }
            });
        }

        public void changeActiveTool(int active = -1,bool click=false,int visibility=0)     // visibility = -1 = force off ; 0 = auto ; 1 = force on
        {
            if (click)
                SubTool_Click(Btn_SubTools[active], null);
                // active buttons will be set through the click and will keep previous active state
            else
            {
                if (subTools_activeTool >= 0) Btn_SubTools[subTools_activeTool].FlatAppearance.BorderSize = 0;
                subTools_activeTool = active;
                if (subTools_activeTool >= 0) Btn_SubTools[subTools_activeTool].FlatAppearance.BorderSize = ACTIVE_SUBTOOL_BORDERSIZE;
            }
            if (visibility == -1)
                gpSubTools.Visible = false;
            else if (visibility == 1)
                gpSubTools.Visible = true;
            Root.UponButtonsUpdate |= 0x2;
        }

        private void SubTool_Click(object sender, EventArgs e)
        {
            bool First = subTools_activeTool == -1;
            if (gpSubTools_MouseOn == 2)
            {
                gpSubTools_MouseOn = 0;
                return;
            }
            Console.WriteLine("SubTool_Click");
            changeActiveTool((int)(((Button)sender).Tag),false);
            subTools_actions[(int)(((Button)sender).Tag)]((int)(((Button)sender).Tag));
            // close if not pinned
            if (!First && (int)(Btn_SubToolPin.Tag) != 1)
            {
                gpSubTools.Visible = false;
            }
            //Root.UponAllDrawingUpdate = true;
            Root.UponButtonsUpdate |= 0x2;
        }

        private void gpSubTools_MouseDown(object sender, MouseEventArgs e)
        {
            gpSubTools_MouseOn = 1;
            HitMovingToolbareXY.X = e.X;
            HitMovingToolbareXY.Y = e.Y;
        }

        private void gpSubTools_MouseMove(object sender, MouseEventArgs e)
        {
            //Console.WriteLine(e.X.ToString() + " ; " + e.Y.ToString() + " - " + HitMovingToolbareXY.X.ToString() + " ; " + HitMovingToolbareXY.Y.ToString() + " / "+ gpSubTools_MouseOn.ToString());
            if (gpSubTools_MouseOn == 1)
            {
                if (Math.Abs(e.X - HitMovingToolbareXY.X) > 20 || Math.Abs(e.Y - HitMovingToolbareXY.Y) > 20)
                    gpSubTools_MouseOn = 2;
            }
            else if (gpSubTools_MouseOn == 2)
            {
                if (e.X != HitMovingToolbareXY.X || e.Y != HitMovingToolbareXY.Y)
                {
                    int newleft = gpSubTools.Left + e.X - HitMovingToolbareXY.X;
                    int newtop = gpSubTools.Top + e.Y - HitMovingToolbareXY.Y;

                    if ( IsInsideVisibleScreen(newleft, newtop) && IsInsideVisibleScreen(newleft + gpSubTools.Width, newtop) &&
                         IsInsideVisibleScreen(newleft, newtop + gpSubTools.Height) && IsInsideVisibleScreen(newleft + gpSubTools.Width, newtop + gpSubTools.Height))
                    {
                        HitMovingToolbareXY.X = e.X - newleft + gpSubTools.Left;
                        HitMovingToolbareXY.Y = e.Y - newtop + gpSubTools.Top;
                        gpSubTools.Left = newleft;
                        gpSubTools.Top = newtop;
                        Root.UponAllDrawingUpdate = true;
                        Root.UponButtonsUpdate |= 0x5;
                    }
                }
            }
        }

        private void gpSubTools_MouseUp(object sender, MouseEventArgs e)
        {
            gpSubTools_MouseOn = 0;
        }

        private void Btn_SubToolClose_Click(object sender, EventArgs e)
        {
            gpSubTools.Visible = false;
            Root.UponAllDrawingUpdate = true;
            Root.UponButtonsUpdate |= 0x5;
        }

        private void BtnPin_Click(object sender, EventArgs e)
        {
            if((int)(Btn_SubToolPin.Tag) != 1 )
            {
                Btn_SubToolPin.Tag = 1;
                Btn_SubToolPin.BackgroundImage?.Dispose();
                Btn_SubToolPin.BackgroundImage = getImgFromDiskOrRes("pinned");
            }
            else
            {
                Btn_SubToolPin.Tag = 0;
                Btn_SubToolPin.BackgroundImage?.Dispose();
                Btn_SubToolPin.BackgroundImage = getImgFromDiskOrRes("unpinned");
            }
            Root.UponButtonsUpdate |= 0x2;
        }

        public void RestorePolylineData(Stroke st)
        {
            if (PolyLineLastX == Int32.MinValue)
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

        public void SaveStrokes(string fn= "ppinkSav.txt")
        {
            string outp = "";
            int l;
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
                writeUtf("# "+Root.FormDisplay.gOneStrokeCanvus.DpiX.ToString()+"; "+ Root.FormDisplay.gOneStrokeCanvus.DpiY.ToString()+"/"+
                         Root.FormDisplay.gOneStrokeCanvus.PageScale.ToString()+"-"+ Root.FormDisplay.gOneStrokeCanvus.PageUnit.ToString()+"/"+
                         Root.FormDisplay.gOneStrokeCanvus.RenderingOrigin.ToString()+"\n");
                Point pt;
                pt = new Point(0, 0);
                IC.Renderer.PixelToInkSpace(Root.FormDisplay.gOneStrokeCanvus,ref pt);
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
                    writeUtf("ID = " + st.Id.ToString() + "\nguid(" + l.ToString() + ") =");
                    outp = "";
                    foreach(Guid g in st.PacketDescription)
                    {
                        TabletPropertyMetrics m = st.GetPacketDescriptionPropertyMetrics(g);
                        outp += string.Format(CultureInfo.InvariantCulture,";{0},{1},{2},{3},{4}",g,m.Minimum,m.Resolution,m.Maximum,m.Units);
                    }
                    writeUtf(outp.Substring(1));
                    writeUtf("\n");
                    /*outp = "";                    
                    for (int i = 0; i < l; i++)
                    {
                        p = st.GetPoint(i);
                        outp += p.X + "," + p.Y + ";";
                    }
                    writeUtf(outp + "\n");*/
                    foreach (int i in st.GetPacketData())
                    {
                        writeUtf(";" + i.ToString());
                    }
                    writeUtf("\n");
                    Rectangle r=st.GetBoundingBox();
                    outp = "# boxed in " + r.Location.ToString() + " - " + r.Size.ToString()+"\n";
                    writeUtf(outp);
                    da = st.DrawingAttributes;
                    writeUtf("DA = Color [A=255, R=" + da.Color.R.ToString() + ", G=" + da.Color.G.ToString() + ", B=" + da.Color.B.ToString() + "] T=" + da.Transparency 
                             + (da.FitToCurve ? ", Fit, W=" : ", NotFit, W=") + da.Width.ToString() + ", S="+Root.LineStyleToString(da.ExtendedProperties)+"\n");
                    outp = "";
                    foreach (ExtendedProperty pr in st.ExtendedProperties)
                    {
                        //outp += pr.Id.ToString() + " (" + pr.Data.GetType() + ") :" + Encoding.UTF8.GetString(Encoding.Default.GetBytes(pr.Data.ToString())).Replace('\n','\r') + "\n";
                        if(pr.Id == Root.LISTOFPOINTS_GUID)
                        {
                            outp += pr.Id.ToString() + "%List:" + string.Join(";", StoredPatternPoints[(int)pr.Data])+"\n";                            
                        }
                        else
                            outp += pr.Id.ToString() + "%" + pr.Data.GetType() + ":" + pr.Data.ToString().Replace("\r", "").Replace('\n', '\a') + "\n";
                    }
                    outp += "}\n";
                    writeUtf(outp);
                }
            }
        }

        private TabletPropertyMetricUnit TabletPropertyMetricUnitFromString(string s)
        {
            if (s[0] == 'C')
                return TabletPropertyMetricUnit.Centimeters;
            else if (s[0] == 'D')
                return TabletPropertyMetricUnit.Degrees;
            else if (s[0] == 'G')
                return TabletPropertyMetricUnit.Grams;
            else if (s[0] == 'I')
                return TabletPropertyMetricUnit.Inches;
            else if (s[0] == 'P')
                return TabletPropertyMetricUnit.Pounds;
            else if (s[0] == 'R')
                return TabletPropertyMetricUnit.Radians;
            else if (s[0] == 'S')
                return TabletPropertyMetricUnit.Seconds;
            else
                return TabletPropertyMetricUnit.Default;
        }

        public void LoadStrokes(string fn = "ppinkSav.txt")
        {
            if (!File.Exists(fn))
                return;
            using (StreamReader fileout = new StreamReader(fn, System.Text.Encoding.UTF8))
            {
                int j, l;
                Stroke stk=null;
                string st;
                    
                st = fileout.ReadLine();
                if (!st.StartsWith("# ppInk"))
                    return;
                do
                {
                    st = fileout.ReadLine();
                }
                while (st !=null && st.StartsWith("#"));
                while (st !=null && st.StartsWith("ID"))
                {
                    do
                    {
                        st = fileout.ReadLine();
                    }
                    while (st.StartsWith("#"));
                    //                    writeUtf("ID " + st.Id.ToString() + "; guid(" + l.ToString() + ") = ");                    
                    if (!st.StartsWith("guid"))
                        return;
                    j = st.IndexOf("=");                  
                    TabletPropertyDescriptionCollection td = new TabletPropertyDescriptionCollection();
                    foreach(string s in st.Substring(j + 1).Split(';'))
                    {//CultureInfo.InvariantCulture,";{0},{1},{2},{3},{4}",g,m.Minimum,m.Resolution,m.Maximum,m.Units
                        string[] sa = s.Split(',');
                        Guid g = Guid.Parse(sa[0]);
                        TabletPropertyMetrics m = new TabletPropertyMetrics()
                        {
                            Minimum = int.Parse(sa[1]),
                            Resolution = float.Parse(sa[2], CultureInfo.InvariantCulture),
                            Maximum = int.Parse(sa[3]),
                            Units = TabletPropertyMetricUnitFromString(sa[4])
                        };
                        td.Add(new TabletPropertyDescription(g, m));
                    }
                    do
                    {
                        st = fileout.ReadLine();
                    }
                    while (st.StartsWith("#"));
                    if(!st.StartsWith(";"))
                        return;
                    int[] a = Array.ConvertAll(st.Substring(1).Split(';'), int.Parse);
                    stk = IC.Ink.CreateStroke(a, td);

                    do
                    {
                        st = fileout.ReadLine();
                    }
                    while (st.StartsWith("#"));

                    if (!st.StartsWith("DA"))
                        return;
                    j = st.IndexOf("R=")+2;
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
                    l = st.IndexOf(",", j);
                    stk.DrawingAttributes.Width = float.Parse(st.Substring(j, l - j), CultureInfo.InvariantCulture);
                    j = st.IndexOf("S=") + 2;
                    if (j > 0)
                    {
                        l = st.Length;
                        DashStyle ds = Root.LineStyleFromString(st.Substring(j, l - j));
                        if (ds != DashStyle.Custom)
                        {
                            stk.DrawingAttributes.ExtendedProperties.Add(Root.DASHED_LINE_GUID, ds);
                        }
                        //else
                        //    try { stk.DrawingAttributes.ExtendedProperties.Remove(Root.DASHED_LINE_GUID); } catch { }
                    }
                    //else
                    //    try { stk.DrawingAttributes.ExtendedProperties.Remove(Root.DASHED_LINE_GUID); } catch { }
                    do
                    {
                        st = fileout.ReadLine();
                    }
                    while (st.StartsWith("#"));
                    Guid guid;
                    while(st != "}")
                    {
                        j = st.IndexOf('%');
                        guid = new Guid(st.Substring(0, j));
                        j++;
                        l = st.IndexOf(':', j);
                        string st1 = st.Substring(j, l - j);
                        string st2 = st.Substring(l + 1);
                        object obj = null;
                        if (guid == Root.LISTOFPOINTS_GUID)
                        {
                            st2=st2.Replace("{X=", "").Replace(",Y=", ",").Replace("}", "");
                            ListPoint pts = new ListPoint();
                            foreach(String s1 in st2.Split(';'))
                            {
                                if (s1 == "") continue;
                                String[] ss1 = s1.Split(',');
                                pts.Add(new Point(int.Parse(ss1[0]), int.Parse(ss1[1])));
                            }
                            StoredPatternPoints.Add(pts);
                            obj = StoredPatternPoints.Count-1;
                        }
                        else
                        {
                            if (st.Contains("Int"))
                                try
                                {
                                    obj = int.Parse(st2);
                                }
                                catch
                                {
                                    obj = Int64.Parse(st2); // for Fading...
                                }
                            else if (st.Contains("Bool"))
                                obj = bool.Parse(st2);
                            else if (st.Contains("Single"))
                                obj = float.Parse(st2, CultureInfo.InvariantCulture);
                            else if (st.Contains("Double"))
                                obj = double.Parse(st2, CultureInfo.InvariantCulture);
                            else if (st.Contains("String"))
                                obj = st2.Replace('\a', '\n');
                            if (guid == Root.IMAGE_GUID && !ClipartsDlg.Originals.ContainsKey(st2))
                            {
                                try { ClipartsDlg.LoadImage(st2); } catch { }
                            }
                            if (guid == Root.ANIMATIONFRAMEIMG_GUID)
                            {
                                AnimationStructure ani = buildAni((string)(stk.ExtendedProperties[Root.IMAGE_GUID].Data));
                                Animations.Add(AniPoolIdx, ani);
                                stk.ExtendedProperties.Add(Root.ANIMATIONFRAMEIMG_GUID, AniPoolIdx);
                                AniPoolIdx++;
                            }
                        }
                        stk.ExtendedProperties.Add(guid, obj);
                        do
                        {
                            st = fileout.ReadLine();
                        }
                        while (st.StartsWith("#"));
                    }
                    if (stk.ExtendedProperties.Contains(Root.ARROWSTART_GUID))
                    {
                        double theta = Math.Atan2((int)stk.ExtendedProperties[Root.ARROWEND_Y_GUID].Data - (int)stk.ExtendedProperties[Root.ARROWSTART_Y_GUID].Data,
                                                  (int)stk.ExtendedProperties[Root.ARROWEND_X_GUID].Data - (int)stk.ExtendedProperties[Root.ARROWSTART_X_GUID].Data);
                        Bitmap bmp = PrepareArrowBitmap(Root.ArrowHead[Root.CurrentArrow], stk.DrawingAttributes.Color, stk.DrawingAttributes.Transparency,
                                                   Root.HiMetricToPixel(stk.DrawingAttributes.Width), (float)theta, out l);
                        StoredArrowImages.Add(bmp);
                        stk.ExtendedProperties.Add(Root.ARROWSTART_GUID, StoredArrowImages.Count - 1);
                    }
                    if (stk.ExtendedProperties.Contains(Root.ARROWEND_GUID))
                    {
                        double theta = Math.Atan2((int)stk.ExtendedProperties[Root.ARROWEND_Y_GUID].Data - (int)stk.ExtendedProperties[Root.ARROWSTART_Y_GUID].Data,
                                                  (int)stk.ExtendedProperties[Root.ARROWEND_X_GUID].Data - (int)stk.ExtendedProperties[Root.ARROWSTART_X_GUID].Data);
                        Bitmap bmp = PrepareArrowBitmap(Root.ArrowTail[Root.CurrentArrow], stk.DrawingAttributes.Color, stk.DrawingAttributes.Transparency,
                                                   Root.HiMetricToPixel(stk.DrawingAttributes.Width), (float)(Math.PI + theta), out l);
                        StoredArrowImages.Add(bmp);
                        stk.ExtendedProperties.Add(Root.ARROWEND_GUID, StoredArrowImages.Count - 1);

                    }
                    stk.DrawingAttributes = stk.DrawingAttributes.Clone();
                    IC.Ink.Strokes.Add(stk);
                    do
                    {
                        st = fileout.ReadLine();
                    }
                    while (st!=null && st.StartsWith("#"));
                }
            }
            Root.UponAllDrawingUpdate = true;
        }

        public void btLoad_Click(object sender, EventArgs e)
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
                    SaveStrokeFile = Path.GetFullPath(Environment.ExpandEnvironmentVariables(Root.SaveStrokesPath + "trunk.strokes.txt"));
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = Path.GetDirectoryName(SaveStrokeFile); 
                    openFileDialog.Filter = "strokes files(*.strokes.txt)|*.strokes.txt|All files (*.*)|*.*";
                    openFileDialog.FilterIndex = 1;
                    AllowInteractions(true);
                    DialogResult rst = openFileDialog.ShowDialog();
                    AllowInteractions(false);
                    if ( rst == DialogResult.OK)
                    {
                        SaveStrokeFile = openFileDialog.FileName;
                        toolTip.SetToolTip(this.btLoad, String.Format(Root.Local.LoadStroke, Path.GetFileName(SaveStrokeFile).Replace(".stroke.txt","")));
                        toolTip.SetToolTip(this.btSave, String.Format(Root.Local.SaveStroke, Path.GetFileName(SaveStrokeFile).Replace(".stroke.txt", "")));
                    }
                    else
                        return;
                }
            }
            if (SaveStrokeFile == "")
                LoadStrokes(Path.GetFullPath(Environment.ExpandEnvironmentVariables(Root.SaveStrokesPath + "AutoSave.strokes.txt")));
            else
                LoadStrokes(SaveStrokeFile);
            SaveUndoStrokes();
            Root.UponAllDrawingUpdate = true;
        }

        private void btLasso_Click(object sender, EventArgs e)
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
            StrokesSelection.Clear();
            SelectPen(-4);

            return;
        }

        public void btSave_Click(object sender, EventArgs e)
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
                if ((sender != null &&  (DateTime.Now - MouseTimeDown).TotalSeconds > Root.LongClickTime)|| SaveStrokeFile == "")
                {
                    string sav = SaveStrokeFile;
                    if (SaveStrokeFile == "")
                        SaveStrokeFile = Path.GetFullPath(Environment.ExpandEnvironmentVariables(Root.SaveStrokesPath))+"Trunk.strokes.txt";
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
                            toolTip.SetToolTip(this.btLoad, String.Format(Root.Local.LoadStroke, Path.GetFileName(SaveStrokeFile).Replace(".stroke.txt", "")));
                            toolTip.SetToolTip(this.btSave, String.Format(Root.Local.SaveStroke, Path.GetFileName(SaveStrokeFile).Replace(".stroke.txt", "")));
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
            catch(Exception ex)
            {
                MessageBox.Show($"error saving into {SaveStrokeFile}");
                string errorMsg = "Silent exception logged \r\n:"+ex.Message + "\r\n\r\nStack Trace:\r\n" + ex.StackTrace + "\r\n\r\n";
                Program.WriteErrorLog(errorMsg);
            };
        }

        private Strokes IsInside(Stroke lasso, float percent = 0)
        {
            try
            {
                Point[] pts;
                Strokes sts = IC.Ink.HitTest(lasso.GetPoints(), percent, out pts);
                return sts;
            }
            catch
            {
                return null;
            }
        }

        public void ModifyStrokesSelection()
        {
            ModifyStrokesSelection(true, ref InprogressSelection, StrokesSelection);
        }

        public void ModifyStrokesSelection(bool AppendToSelection, ref Strokes InprogressSelection, Strokes StrokesSelection)
        {
            if (InprogressSelection == null) return;
            foreach(Stroke st in InprogressSelection)
            {
                if(AppendToSelection)
                    StrokesSelection.Add(st);
                else
                    try
                    {
                        StrokesSelection.Remove(st);
                    }
                    catch { }
            }
            InprogressSelection = null;
        }


        private string GetMainModuleFileName(Process process, int buffer = 1024)
        {
            try
            {
                var fileNameBuilder = new StringBuilder(buffer);
                uint bufferLength = (uint)fileNameBuilder.Capacity + 1;
                return QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength) ?
                    fileNameBuilder.ToString() :
                    null;

            }
            catch
            {
                return "?????";
            }
        }

        public bool AddM3UEntryInProgress = false;
        public bool AddM3UEntry(string st = null)
        {
            try
            {
                if (Root.CurrentVideoFileName == "")
                    return false;
                int sec;
                if (Root.VideoRecordMode==VideoRecordMode.OBSRec)
                {
                    ObsTimeCode = DateTime.MinValue;
                    int i=100;
                    SendInWs(Root.ObsWs, "GetRecordingStatus", new CancellationToken());
                    Console.WriteLine(DateTime.Now);
                    while(ObsTimeCode== DateTime.MinValue && i-->=0)
                        Task.Delay(50);
                    Console.WriteLine(i.ToString()+" - "+DateTime.Now.ToString());
                    sec = (int)(ObsTimeCode.TimeOfDay.TotalSeconds);
                }
                else
                    sec=(int)((DateTime.Now - Root.CurrentVideoStartTime).TotalSeconds);

                if (st == null)
                {
                    st = Root.ExpandVarCmd(Root.IndexDefaultText, 0, 0, 0, 0);
                    if (Root.NoEditM3UEntry)
                    {
                        Root.trayIcon.ShowBalloonTip(100, "", string.Format(Root.Local.M3UBalloonText, st), ToolTipIcon.Info);
                    }
                    else
                    {
                        AddM3UEntryInProgress = true;
                        bool interact = Root.FormCollection.Visible && Root.PointerMode;
                        if (interact) AllowInteractions(false);
                        Screen scr = Screen.FromPoint(MousePosition);
                        FormInput inp = new FormInput(Root.Local.M3UTextCaption, Root.Local.M3UTextLabel, Root.ExpandVarCmd(Root.IndexDefaultText, 0, 0, 0, 0), false, Root, null);
                        inp.Top = ((int)(scr.Bounds.Top + scr.Bounds.Bottom - inp.Height) / 2);//System.Windows.SystemParameters.PrimaryScreenHeight)-inp.Height) / 2;
                        inp.Left = ((int)(scr.Bounds.Left + scr.Bounds.Right - inp.Width) / 2);// System.Windows.SystemParameters.PrimaryScreenWidth) - inp.Width) / 2;
                        DialogResult ret = inp.ShowDialog();  // cancellation process is within the cancel button
                        AddM3UEntryInProgress = false;
                        if (interact)
                        {
                            TextEdited = true;
                            AllowInteractions(false);
                        }
                        if (ret == DialogResult.OK)
                            st = inp.InputSL.Text;
                        else
                            return false;
                    }
                }
                
                if (Root.CurrentIndexFileName == "")
                    Root.CurrentIndexFileName = Path.ChangeExtension(Root.CurrentVideoFileName, ".m3u");
                if (!File.Exists(Root.CurrentIndexFileName))
                {
                    File.WriteAllText(Root.CurrentIndexFileName, "#EXTM3U\n\n");
                }
                File.AppendAllText(Root.CurrentIndexFileName, string.Format("#EXTINF:-1,{0}\n#EXTVLCOPT:start-time={1}\n{2}\n\n", st, sec, Root.MakeRelativePath(Root.CurrentIndexFileName, Root.CurrentVideoFileName)));
                if (Root.UndockOnIndexCreate)
                    Root.UnDock();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("!!! @m3u: " + e.Message);
                return false;
            }
        }


        public string GetCaptionOfActiveWindow()
        {
            var strTitle = string.Empty;
            var handle = GetForegroundWindow();
            // Obtain the length of the text   
            var intLength = GetWindowTextLength(handle) + 1;
            var stringBuilder = new StringBuilder(intLength);
            if (GetWindowText(handle, stringBuilder, intLength) > 0)
            {
                strTitle = stringBuilder.ToString();
            }
            uint pid;
            GetWindowThreadProcessId(handle , out pid);
            Process p = Process.GetProcessById((int)pid);
            return GetMainModuleFileName(p)+" / "+strTitle;
        }


        [DllImport("user32.dll")]
		static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
		[DllImport("user32.dll", SetLastError = true)]
		static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);
		[DllImport("user32.dll")]
		static extern int SetWindowLong(IntPtr hWnd, int nIndex, UInt32 dwNewLong);

        public void btPagePrev_Click(object sender, EventArgs e)
        {
            string st = Path.GetFullPath(Environment.ExpandEnvironmentVariables(Root.SaveStrokesPath));

            btPagePrev.RightToLeft = RightToLeft.No;
            longClickTimer.Stop(); // for an unkown reason the mouse arrives later
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }
            StopAllZooms();

            if (PageIndex>0)
            {
                try
                {
                    SaveStrokes(st+"Page"+PageIndex.ToString()+".strokes.txt");
                    Console.WriteLine("Load "+st + "Page" + PageIndex.ToString() + ".strokes.txt");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(SaveStrokeFile);
                    string errorMsg = "Silent exception logged \r\n:" + ex.Message + "\r\n\r\nStack Trace:\r\n" + ex.StackTrace + "\r\n\r\n";
                    Program.WriteErrorLog(errorMsg);
                };
                PageIndex--;
                btClear_Click(null, null);
                try
                {
                    LoadStrokes(st + "Page" + PageIndex.ToString() + ".strokes.txt");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(SaveStrokeFile);
                    string errorMsg = "Silent exception logged \r\n:" + ex.Message + "\r\n\r\nStack Trace:\r\n" + ex.StackTrace + "\r\n\r\n";
                    Program.WriteErrorLog(errorMsg);
                };
            }
        }

        public void btPageNext_Click(object sender, EventArgs e)
        {
            string st = Path.GetFullPath(Environment.ExpandEnvironmentVariables(Root.SaveStrokesPath));

            btPageNext.RightToLeft = RightToLeft.No;
            longClickTimer.Stop(); // for an unkown reason the mouse arrives later
            if (ToolbarMoved)
            {
                ToolbarMoved = false;
                return;
            }
            StopAllZooms();

            try
            {
                SaveStrokes(st + "Page" + PageIndex.ToString() + ".strokes.txt");
                Console.WriteLine("Load " + st + "Page" + PageIndex.ToString() + ".strokes.txt");
            }
            catch (Exception ex)
            {
                MessageBox.Show(SaveStrokeFile);
                string errorMsg = "Silent exception logged \r\n:" + ex.Message + "\r\n\r\nStack Trace:\r\n" + ex.StackTrace + "\r\n\r\n";
                Program.WriteErrorLog(errorMsg);
            };
            PageIndex++;
            btClear_Click(null, null);

            if (PageIndex <= PageMax)
            {
                try
                {
                    LoadStrokes(st + "Page" + PageIndex.ToString() + ".strokes.txt");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(SaveStrokeFile);
                    string errorMsg = "Silent exception logged \r\n:" + ex.Message + "\r\n\r\nStack Trace:\r\n" + ex.StackTrace + "\r\n\r\n";
                    Program.WriteErrorLog(errorMsg);
                };
            }
            else
            {
                PageMax++;
            }
        }

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

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);
        [DllImport("Kernel32.dll")]
        private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

    }
}
