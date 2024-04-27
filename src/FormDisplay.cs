using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Ink;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Drawing.Drawing2D;

//using GameOverlay.Drawing;
using GameOverlay.Windows;

namespace gInk
{
	public partial class FormDisplay : Form
	{
		public Root Root;
		IntPtr Canvus;
		IntPtr canvusDc;
		IntPtr OneStrokeCanvus;
		IntPtr onestrokeDc;
		//IntPtr BlankCanvus;
		//IntPtr blankcanvusDc;
        IntPtr OutCanvus;
        IntPtr OutcanvusDc;
        Graphics gCanvus;
		public Graphics gOneStrokeCanvus;
        public Graphics gOutCanvus;
        //Bitmap ScreenBitmap;
        IntPtr hScreenBitmap;
		IntPtr memscreenDc;

        ImageAttributes iaToolBarTransparency = new ImageAttributes();
        public ImageAttributes iaSubToolsTransparency = new ImageAttributes();
        Bitmap gpButtonsImage;
        Bitmap gpPenWidthImage;
        Bitmap gpSubToolsImage;           
		SolidBrush TransparentBrush;
		SolidBrush SemiTransparentBrush;

		byte[] screenbits;
		byte[] lastscreenbits;

        OverlayWindow DXwindow;
        GameOverlay.Drawing.Graphics DXgfx;
        GameOverlay.Drawing.IBrush DXselectionFramePen;
        Bitmap DXCapturedBmp,DXInprogressBmp;
        Graphics DXCaptureGraphics, DXInProgressGraphics;
        System.IO.MemoryStream DXCaptureMemIO;
        Ink LocalCopyInk=null;
        bool DXAllUpdate;


		// http://www.csharp411.com/hide-form-from-alttab/
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				// turn on WS_EX_TOOLWINDOW style bit
				if (globalRoot.HideInAltTab)
                    cp.ExStyle |= 0x80;
				return cp;
			}
		}

		public FormDisplay(Root root)
		{
            Root = root;

            ColorMatrix cm = new ColorMatrix();
            cm.Matrix00 = 1f; cm.Matrix11 = 1f; cm.Matrix22 = 1f;
            cm.Matrix33 = Root.ToolbarBGColor[0]/255f;
            iaToolBarTransparency.SetColorMatrix(cm);

            cm = new ColorMatrix();
            cm.Matrix00 = 1f; cm.Matrix11 = 1f; cm.Matrix22 = 1f;
            cm.Matrix33 = Root.ToolbarBGColor[0] / 255f;
            iaSubToolsTransparency.SetColorMatrix(cm);

            InitializeComponent();

            Initialize();
		}

        public void Initialize() 
        /// part of the Constructor to be called everytime the window is displayed at startInking
        {
            if (Root.WindowRect.Width <= 0 || Root.WindowRect.Height <= 0)
            {
                this.Left = SystemInformation.VirtualScreen.Left;
                this.Top = SystemInformation.VirtualScreen.Top;
                this.Width = SystemInformation.VirtualScreen.Width;
                this.Height = SystemInformation.VirtualScreen.Height - 2;
            }
            else
            {
                this.Left = Math.Min(Math.Max(SystemInformation.VirtualScreen.Left, Root.WindowRect.Left), SystemInformation.VirtualScreen.Right - Root.WindowRect.Width);
                this.Top = Math.Min(Math.Max(SystemInformation.VirtualScreen.Top, Root.WindowRect.Top), SystemInformation.VirtualScreen.Bottom - Root.WindowRect.Height);
                this.Width = Root.WindowRect.Width;
                this.Height = Root.WindowRect.Height;
            }

            Bitmap InitCanvus = new Bitmap(this.Width, this.Height);
            Bitmap Init2Canvus = new Bitmap(this.Width, this.Height);
            Canvus = InitCanvus.GetHbitmap(Color.FromArgb(0));
            OneStrokeCanvus = InitCanvus.GetHbitmap(Color.FromArgb(0));
            OutCanvus = Init2Canvus.GetHbitmap(Color.FromArgb(0));

            IntPtr screenDc = GetDC(IntPtr.Zero);
            canvusDc = CreateCompatibleDC(screenDc);
            SelectObject(canvusDc, Canvus);
            onestrokeDc = CreateCompatibleDC(screenDc);
            SelectObject(onestrokeDc, OneStrokeCanvus);
            OutcanvusDc = CreateCompatibleDC(screenDc);
            SelectObject(OutcanvusDc, OutCanvus);
            gCanvus = Graphics.FromHdc(canvusDc);
            gCanvus.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver; // source Over else we get some issues displaying text
            gCanvus.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            gOneStrokeCanvus = Graphics.FromHdc(onestrokeDc);
            gOneStrokeCanvus.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            gOutCanvus = Graphics.FromHdc(OutcanvusDc);
            gOutCanvus.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

            if (Root.AutoScroll) // apparently never called ; Autoscroll set to False never modified
            {
                hScreenBitmap = InitCanvus.GetHbitmap(Color.FromArgb(0));
                memscreenDc = CreateCompatibleDC(screenDc);
                SelectObject(memscreenDc, hScreenBitmap);
                screenbits = new byte[50000000];
                lastscreenbits = new byte[50000000];
            }
            ReleaseDC(IntPtr.Zero, screenDc);

            /* PPzz : 
             *     this is my understandarding about drawing: 
             *     gCanvus is the graphics where in standard the strokes are drawn, I've introduced there also the drawing
             *     gOnestrokeCanvus is filled with the current screen before drawing : it seems to be used when deleting(undo ?) a strike.
             *     I've introduced gOutCanvus in order to have a graphics where I can draw in the inprogress shapes (Line,Ellipsis,Rectangular,Arrow) the previous ones, being strokes, are drawn on gCanvus
             *     the timer1 refresh the window regularly
             */

            InitCanvus.Dispose();
            Init2Canvus.Dispose();
            //this.DoubleBuffered = true;

            int gpheight = (int)(Screen.PrimaryScreen.Bounds.Height * Root.ToolbarHeight);
            gpButtonsImage = new Bitmap(2000, 2000, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            gpPenWidthImage = new Bitmap(200, 200, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            gpSubToolsImage = new Bitmap(500, 500, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            TransparentBrush = new SolidBrush(Color.Transparent);
            SemiTransparentBrush = new SolidBrush(Color.FromArgb(120, 255, 255, 255));

            timer1.Enabled = true;
            ToTopMostThrough();

            if (Root.DirectX)
            {
                DXCapturedBmp = new Bitmap(this.Width, this.Height, PixelFormat.Format32bppArgb);
                DXInprogressBmp = new Bitmap(this.Width, this.Height, PixelFormat.Format32bppArgb);
                DXCaptureGraphics = Graphics.FromImage(DXCapturedBmp);
                DXInProgressGraphics = Graphics.FromImage(DXInprogressBmp);
                DXCaptureMemIO = new System.IO.MemoryStream();
                if (DXwindow == null)
                {
                    DXwindow = new OverlayWindow(this.Left, this.Right, this.Width, this.Height)
                    {
                        IsTopmost = true,
                        IsVisible = true,
                        Title = "ppInk DX"
                    };
                    DXwindow.Create();
                    DXwindow.PlaceAbove(this.Handle);
                }
                if (DXgfx == null)
                {
                    DXgfx = new GameOverlay.Drawing.Graphics(DXwindow.Handle, DXwindow.Width, DXwindow.Height)
                    {
                        MeasureFPS = true,
                        PerPrimitiveAntiAliasing = true,
                        TextAntiAliasing = true
                    };
                    DXgfx.Setup();
                }
                if (DXselectionFramePen == null)
                    DXselectionFramePen = DXgfx.CreateSolidBrush(255, 0, 0, 255);
                DXAllUpdate = true;
                timerDirectX.Enabled = true;
                LocalCopyInk?.Dispose();
                LocalCopyInk = null;
            }
            else
            {
                timerDirectX.Enabled = false;
                DXwindow?.Dispose();
                DXwindow = null;
                DXgfx?.Dispose();
                DXgfx = null;
                DXselectionFramePen = null;
                DXCaptureGraphics?.Dispose();
                DXInProgressGraphics?.Dispose();
                DXCapturedBmp?.Dispose();
                DXInprogressBmp?.Dispose();
                DXCapturedBmp = null;
                DXInprogressBmp = null;
                DXCaptureGraphics = null;
                DXInProgressGraphics = null;
                DXCaptureMemIO?.Dispose();
                DXCaptureMemIO = null;
                DXAllUpdate = false;
                LocalCopyInk?.Dispose();
                LocalCopyInk = null;
            }
        }

        public void ToTopMostThrough()
		{
			UInt32 dwExStyle = GetWindowLong(this.Handle, -20);
			SetWindowLong(this.Handle, -20, dwExStyle | 0x00080000);
			SetWindowPos(this.Handle, (IntPtr)0, 0, 0, 0, 0, 0x0002 | 0x0001 | 0x0004 | 0x0010 | 0x0020);
			//SetLayeredWindowAttributes(this.Handle, 0x00FFFFFF, 1, 0x2);
			SetWindowLong(this.Handle, -20, dwExStyle | 0x00080000 | 0x00000020);
			SetWindowPos(this.Handle, (IntPtr)(-1), 0, 0, 0, 0, 0x0002 | 0x0001 | 0x0010 | 0x0020);
		}

		public void ClearCanvus()
		{
            //gCanvus.Clear(Color.Transparent);
            ClearCanvus(gCanvus);
		}

        public void ClearCanvus(Graphics g)
		{
			g.Clear(Color.Transparent);
            // to draw a border to the area. Only really visible in window mode


            DrawBorder(HasFocus(), g);
        }

        public void DrawBorder(bool Focus,Graphics g=null)
        {
            if (g == null)
                g = gCanvus;
            if((Root.WindowRect.Width> 0)&&(Root.WindowRect.Width > 0))
            {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                g.DrawRectangle(new Pen(Focus ? Root.WindowModeBorderSelected : Root.WindowModeBorderUnselected, 1), new Rectangle(0, 0, this.ClientSize.Width - 1, this.ClientSize.Height - 1));
            }
        }


        public void DrawSnapping(Rectangle rect)
		{
			gCanvus.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
			if (rect.Width > 0 && rect.Height > 0)
			{
				gCanvus.FillRectangle(SemiTransparentBrush, new Rectangle(0, 0, rect.Left, this.Height));
				gCanvus.FillRectangle(SemiTransparentBrush, new Rectangle(rect.Right, 0, this.Width - rect.Right, this.Height));
				gCanvus.FillRectangle(SemiTransparentBrush, new Rectangle(rect.Left, 0, rect.Width, rect.Top));
				gCanvus.FillRectangle(SemiTransparentBrush, new Rectangle(rect.Left, rect.Bottom, rect.Width, this.Height - rect.Bottom));
                Pen pen = new Pen(Root.ResizeDrawingWindow? Color.FromArgb(200, 255, 0, 0):Color.FromArgb(200, 80, 80, 80));
				pen.Width = 3;
				gCanvus.DrawRectangle(pen, rect);
			}
			else
			{
                gCanvus.FillRectangle(SemiTransparentBrush, new Rectangle(0, 0, this.Width, this.Height));
                ;
			}
			//gCanvus.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
		}

        public void DrawButtons(bool redrawbuttons = true, bool exiting = false)
        {
            DrawButtons(gCanvus, redrawbuttons, exiting);
        }

        public void DrawButtons(Graphics g, bool redrawbuttons=true, bool exiting = false)
		{
			if (Root.AlwaysHideToolbar)
				return;

			int top, height, left, width;
			int fullwidth;
			int gpbl;
			int drawwidth;

			top = Root.FormCollection.gpButtons.Top;
			height = Root.FormCollection.gpButtons.Height;
			left = Root.FormCollection.gpButtons.Left;
			width = Root.FormCollection.gpButtons.Width;
			fullwidth = Root.FormCollection.gpButtonsWidth;
			drawwidth = width;
			gpbl = Root.FormCollection.gpButtonsLeft;
			if (left + width > gpbl + fullwidth)
				drawwidth = gpbl + fullwidth - left;

            if (redrawbuttons && Root.FormCollection.gpButtons.Width>0 && Root.FormCollection.gpButtons.Height>0)
                Root.FormCollection.gpButtons.DrawToBitmap(gpButtonsImage, new Rectangle(0, 0, Root.FormCollection.gpButtonsWidth, Root.FormCollection.gpButtonsHeight));

            if (exiting)
			{
				int clearleft = Math.Max(left - 120, gpbl);
				//gCanvus.FillRectangle(TransparentBrush, clearleft, top, fullwidth * 2, height);
				g.FillRectangle(TransparentBrush, clearleft, top, drawwidth, height);
			}
            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            g.FillRectangle(TransparentBrush, left, top, drawwidth, height);
            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            //gCanvus.DrawImage(gpButtonsImage, new Rectangle(left, top, drawwidth, height), 0, 0, drawwidth, height, GraphicsUnit.Pixel,iaToolBarTransparency);
            Size s = Root.FormCollection.VisibleToolbar;
            if (Root.ToolbarOrientation == Orientation.toLeft || Root.ToolbarOrientation == Orientation.toUp)
                g.DrawImage(gpButtonsImage, new Rectangle(left, top, drawwidth, height), 0, 0, drawwidth, height, GraphicsUnit.Pixel, iaToolBarTransparency);
            else
                g.DrawImage(gpButtonsImage, new Rectangle(left, top, s.Width, s.Height),
                                    Root.FormCollection.gpButtonsWidth- s.Width, Root.FormCollection.gpButtonsHeight - s.Height,
                                    s.Width, s.Height, GraphicsUnit.Pixel, iaToolBarTransparency);

            if (Root.gpPenWidthVisible)
			{
				top = Root.FormCollection.gpPenWidth.Top;
				height = Root.FormCollection.gpPenWidth.Height;
				left = Root.FormCollection.gpPenWidth.Left;
				width = Root.FormCollection.gpPenWidth.Width;
				if (redrawbuttons)
					Root.FormCollection.gpPenWidth.DrawToBitmap(gpPenWidthImage, new Rectangle(0, 0, width, height));

                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                g.FillRectangle(TransparentBrush, left, top, width, height);
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                g.DrawImage(gpPenWidthImage, new Rectangle(left, top, width, height), 0, 0, width, height, GraphicsUnit.Pixel, iaToolBarTransparency);
			}
            if (Root.FormCollection.gpSubTools.Visible)
            {
                top = Root.FormCollection.gpSubTools.Top;
                height = Root.FormCollection.gpSubTools.Height;
                left = Root.FormCollection.gpSubTools.Left;
                width = Root.FormCollection.gpSubTools.Width;
                if (redrawbuttons)
                    Root.FormCollection.gpSubTools.DrawToBitmap(gpSubToolsImage, new Rectangle(0, 0, width, height));

                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                g.FillRectangle(TransparentBrush, left, top, width, height);
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                g.DrawImage(gpSubToolsImage, new Rectangle(left, top, width, height), 0, 0, width, height, GraphicsUnit.Pixel, iaSubToolsTransparency);
            }
        }

        public void DrawOneStroke(Graphics g,Stroke st,DrawingAttributes DA=null,Bitmap bmp=null)
        {
            if (DA == null)
                DA = st.DrawingAttributes;
            //if (st.ExtendedProperties.Contains(Root.DASHED_LINE_GUID)||DA.ExtendedProperties.Contains(Root.DASHED_LINE_GUID))
            if (DA.ExtendedProperties.Contains(Root.DASHED_LINE_GUID))
                try
                {
                    Pen p = new Pen(Color.FromArgb(255-DA.Transparency,DA.Color), Root.HiMetricToPixel(DA.Width));                   
                    //try
                    //{
                        p.DashStyle = (DashStyle)(int)(DA.ExtendedProperties[Root.DASHED_LINE_GUID].Data);
                    //}
                    //catch
                    //{
                    //p.DashStyle = (DashStyle)(int)(st.ExtendedProperties[Root.DASHED_LINE_GUID].Data);
                    //}
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    if (DA.FitToCurve)
                    {
                        Point[] pts = st.GetPoints();
                        //Root.FormCollection.IC.Renderer.InkSpaceToPixel(gOneStrokeCanvus, ref pts);
                        //Console.WriteLine(pts.ToString());
                        pts = st.GetFlattenedBezierPoints(5);
                        //pts = st.GetPoints();
                        Root.FormCollection.IC.Renderer.InkSpaceToPixel(gOneStrokeCanvus, ref pts);                   
                        g.DrawCurve(p, pts, .5F);
                        /*using (StreamWriter f = File.AppendText("strokes.log"))
                        {
                            foreach (Point pp in pts)
                                f.Write(pp.X.ToString() + "," + pp.Y.ToString() + " ");
                            f.WriteLine();
                        }*/
                    }
                    else
                    {
                        Point[] pts = st.GetPoints();
                        Root.FormCollection.IC.Renderer.InkSpaceToPixel(gOneStrokeCanvus, ref pts);
                        g.DrawLines(p, pts);
                    }
                }
                catch 
                {
                    /*
                    var sta = new StackTrace(e, true);
                    var frame = sta.GetFrame(0);
                    Console.WriteLine(sta.ToString());
                    Console.WriteLine(frame.GetFileLineNumber().ToString() + " / " + e.Message);
                    if (bmp != null)
                        Root.FormCollection.IC.Renderer.Draw(bmp, st);
                    else
                        Root.FormCollection.IC.Renderer.Draw(g, st);
                    */
                }
            else
            {
                if (bmp != null)
                    Root.FormCollection.IC.Renderer.Draw(bmp, st);
                else
                    Root.FormCollection.IC.Renderer.Draw(g, st);
            }
        }

        public void DrawStrokes()
		{
            DrawStrokes(gCanvus);
		}        

		public void DrawStrokes(Graphics g)
		{
			if (Root.InkVisible)
            {
                foreach (Stroke st in Root.FormCollection.IC.Ink.Strokes)
                {
                    if (((Root.StrokeHovered != null) && (st.Id == Root.StrokeHovered.Id)) 
                        || (Root.FormCollection.AppendToSelection && (Root.FormCollection.StrokesSelection.Contains(st)
                                                                      || (Root.FormCollection.InprogressSelection != null && Root.FormCollection.InprogressSelection.Contains(st))))
                        || (!Root.FormCollection.AppendToSelection && (Root.FormCollection.StrokesSelection.Contains(st)
                                                                       && (Root.FormCollection.InprogressSelection == null || !Root.FormCollection.InprogressSelection.Contains(st)))))
                    {
                        Rectangle rect = st.GetBoundingBox();
                        Point pt = rect.Location;
                        Root.FormCollection.IC.Renderer.InkSpaceToPixel(gOneStrokeCanvus, ref pt);
                        rect.Location = pt;
                        pt.X = rect.Width;
                        pt.Y = rect.Height;
                        Root.FormCollection.IC.Renderer.InkSpaceToPixel(gOneStrokeCanvus, ref pt);
                        rect.Width = pt.X;
                        rect.Height = pt.Y;
                        g.DrawRectangle(Root.SelectionFramePen, rect);
                    }
                    if (st.ExtendedProperties.Contains(Root.ISHIDDEN_GUID))
                        continue;
                    //else //Should not be drawn as a stroke : for the moment only filled values.
                    if (st.ExtendedProperties.Contains(Root.ISFILLEDOUTSIDE_GUID))
                    {
                        SolidBrush bru = new SolidBrush(Color.FromArgb(255 - st.DrawingAttributes.Transparency, st.DrawingAttributes.Color));
                        try
                        {
                            GraphicsPath gp = new GraphicsPath();
                            gp.AddRectangle(new Rectangle(0, 0, this.Width, this.Height));
                            Point[] pts = st.DrawingAttributes.FitToCurve ? st.GetFlattenedBezierPoints(0) : st.GetPoints();
                            Root.FormCollection.IC.Renderer.InkSpaceToPixel(gOneStrokeCanvus, ref pts);
                            gp.AddPolygon(pts);
                            g.FillPath(bru, gp);
                        }
                        catch { }
                    }
                    else if (st.ExtendedProperties.Contains(Root.ISFILLEDCOLOR_GUID) || st.ExtendedProperties.Contains(Root.ISFILLEDWHITE_GUID) || st.ExtendedProperties.Contains(Root.ISFILLEDBLACK_GUID) )
                    {
                        SolidBrush bru;
                        if (st.ExtendedProperties.Contains(Root.ISFILLEDCOLOR_GUID))
                            bru = new SolidBrush(Color.FromArgb(255 - st.DrawingAttributes.Transparency, st.DrawingAttributes.Color));
                        else if (st.ExtendedProperties.Contains(Root.ISFILLEDWHITE_GUID))
                            bru = new SolidBrush(Color.White);
                        else if (st.ExtendedProperties.Contains(Root.ISFILLEDBLACK_GUID))
                            bru = new SolidBrush(Color.Black);
                        else
                            continue;
                            //bru = new SolidBrush(Color.Purple);
                        if (st.DrawingAttributes.FitToCurve)
                        {
                            try
                            {
                                Point[] pts = st.GetFlattenedBezierPoints(0); // 0 to get a good fitting curve
                                Root.FormCollection.IC.Renderer.InkSpaceToPixel(gOneStrokeCanvus, ref pts);
                                g.FillClosedCurve(bru, pts);
                            }
                            catch { }
                        }
                        else
                        {
                            Point[] pts = st.GetPoints();
                            Root.FormCollection.IC.Renderer.InkSpaceToPixel(gOneStrokeCanvus, ref pts);
                            g.FillPolygon(bru, pts);

                        }

                    }
                    if (st.ExtendedProperties.Contains(Root.ARROWSTART_GUID))
                    {
                        Point pt = new Point((int)st.ExtendedProperties[Root.ARROWSTART_X_GUID].Data, (int)st.ExtendedProperties[Root.ARROWSTART_Y_GUID].Data);
                        Bitmap b = Root.FormCollection.StoredArrowImages[(int)st.ExtendedProperties[Root.ARROWSTART_GUID].Data];
                        pt.Offset(-b.Width/2, -b.Height / 2);
                        g.DrawImage(b, new Rectangle(pt.X, pt.Y, b.Width, b.Height));
                    }
                    if (st.ExtendedProperties.Contains(Root.ARROWEND_GUID))
                    {
                        Point pt;
                        if ((int)st.ExtendedProperties[Root.ARROWEND_X_GUID].Data != Int32.MinValue)
                            pt = new Point((int)st.ExtendedProperties[Root.ARROWEND_X_GUID].Data, (int)st.ExtendedProperties[Root.ARROWEND_Y_GUID].Data);
                        else
                            pt = new Point((int)st.ExtendedProperties[Root.ARROWSTART_X_GUID].Data, (int)st.ExtendedProperties[Root.ARROWSTART_Y_GUID].Data);                                           
                        Bitmap b = Root.FormCollection.StoredArrowImages[(int)st.ExtendedProperties[Root.ARROWEND_GUID].Data];
                        pt.Offset(-b.Width / 2, -b.Height / 2);
                        g.DrawImage(b, new Rectangle(pt.X, pt.Y, b.Width, b.Height));
                    }
                    if (st.ExtendedProperties.Contains(Root.IMAGE_GUID))
                    {
                        //Image img = Root.FormCollection.ClipartsDlg.Images.Images[(int)(st.ExtendedProperties[Root.IMAGE_GUID].Data)];
                        Image img;
                        if(st.ExtendedProperties.Contains(Root.ANIMATIONFRAMEIMG_GUID))
                        {
                            try
                            {
                                AnimationStructure ani = Root.FormCollection.Animations[(int)(st.ExtendedProperties[Root.ANIMATIONFRAMEIMG_GUID].Data)];
                                if (ani.DeleteRequested)
                                {
                                    Root.FormCollection.IC.Ink.DeleteStroke(st);
                                    continue;
                                }
                                img = ani.Image.Frames[ani.Idx].GetImage();
                            }
                            catch
                            {
                                img = gInk.Properties.Resources.unknown;
                            }

                        }
                        else
                            try
                            {
                                img = Root.FormCollection.ClipartsDlg.Originals[(string)(st.ExtendedProperties[Root.IMAGE_GUID].Data)];
                            }
                            catch
                            {
                                img = gInk.Properties.Resources.unknown;
                            }
                        // I came back to this solution of using IMAGE_?_GUID in order to have a more accurate position and therefore prevent blurry image
                        int X = (int)(double)(st.ExtendedProperties[Root.IMAGE_X_GUID].Data);
                        int Y = (int)(double)(st.ExtendedProperties[Root.IMAGE_Y_GUID].Data);
                        int W = (int)(double)(st.ExtendedProperties[Root.IMAGE_W_GUID].Data);
                        int H = (int)(double)(st.ExtendedProperties[Root.IMAGE_H_GUID].Data);
                        bool ImageOnLine = st.ExtendedProperties.Contains(Root.IMAGE_ON_LINE_GUID);
                        if (st.ExtendedProperties.Contains(Root.LISTOFPOINTS_GUID))
                        {
                            Double Rotation = st.ExtendedProperties.Contains(Root.ROTATION_GUID) ? (double)st.ExtendedProperties[Root.ROTATION_GUID].Data : 0;
                            List<Point> pts = Root.FormCollection.StoredPatternPoints[(int)(st.ExtendedProperties[Root.LISTOFPOINTS_GUID].Data)];
                            Point PrevPoint = new Point(Int32.MinValue, Int32.MinValue);
                            foreach (Point pt in pts)
                            {                                
                                if (PrevPoint.X != Int32.MinValue)
                                {
                                    Rotation = Math.Atan2(pt.Y - PrevPoint.Y, pt.X- PrevPoint.X)/Math.PI*180.0;
                                }
                                if(ImageOnLine)
                                {
                                    PrevPoint.X = pt.X;
                                    PrevPoint.Y = pt.Y;
                                }

                                if (Rotation != 0)
                                {
                                    g.TranslateTransform(pt.X + W / 2, pt.Y + H / 2);
                                    g.RotateTransform((float)Rotation);
                                    g.TranslateTransform(-(pt.X + W / 2), -(pt.Y + H / 2));
                                }
                                g.DrawImage(img, new Rectangle(pt.X, pt.Y, W, H));
                                g.ResetTransform();
                            }
                        }
                        else
                        {
                            if (st.ExtendedProperties.Contains(Root.ROTATION_GUID))
                            {
                                Double Rotation = (double)st.ExtendedProperties[Root.ROTATION_GUID].Data;
                                g.TranslateTransform(X, Y);
                                g.RotateTransform((float)Rotation);
                                g.TranslateTransform(-X, -Y);
                            }
                            g.DrawImage(img, new Rectangle(X, Y, W, H));
                            g.ResetTransform();
                        }
                    }
                    /*else */
                    if (st.ExtendedProperties.Contains(Root.ISSTROKE_GUID))
                        DrawOneStroke(g, st, null);

                    if (st.ExtendedProperties.Contains(Root.TEXT_GUID))
                    {
                        Point pt = new Point((int)(double)st.ExtendedProperties[Root.TEXTX_GUID].Data, (int)(double)st.ExtendedProperties[Root.TEXTY_GUID].Data);
                        Root.FormCollection.IC.Renderer.InkSpaceToPixel(gOneStrokeCanvus, ref pt);
                        System.Drawing.StringFormat stf = new System.Drawing.StringFormat(System.Drawing.StringFormatFlags.NoClip);
                        stf.Alignment = (System.Drawing.StringAlignment)(st.ExtendedProperties[Root.TEXTHALIGN_GUID].Data);
                        stf.LineAlignment = (System.Drawing.StringAlignment)(st.ExtendedProperties[Root.TEXTVALIGN_GUID].Data);
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        if (st.ExtendedProperties.Contains(Root.ROTATION_GUID))
                        {
                            Double Rotation = (double)st.ExtendedProperties[Root.ROTATION_GUID].Data;
                            int W = 0, H = 0;
                            g.TranslateTransform(pt.X + W / 2, pt.Y + H / 2);
                            g.RotateTransform((float)Rotation);
                            g.TranslateTransform(-pt.X - W / 2, -pt.Y - H / 2);
                        }
                        g.DrawString((string)(st.ExtendedProperties[Root.TEXT_GUID].Data),
                                     new Font((string)st.ExtendedProperties[Root.TEXTFONT_GUID].Data, (float)(double)st.ExtendedProperties[Root.TEXTFONTSIZE_GUID].Data,
                                        (System.Drawing.FontStyle)(int)st.ExtendedProperties[Root.TEXTFONTSTYLE_GUID].Data),
                                     new SolidBrush(Color.FromArgb(255 - st.DrawingAttributes.Transparency, st.DrawingAttributes.Color)), pt.X, pt.Y, stf);
                        g.ResetTransform();
                    }
                }
            }
        }

        // for an unknown reason drawing strokes through the graphics is not taking into account transparency
        // I've adapted DrawStrokes to Bitmap for snapshots
        public void DrawStrokes(Bitmap bmp, bool IgnoreBackground=false)
        {
            Graphics g = Graphics.FromImage(bmp);

            Ink Ink2 = Root.FormCollection.IC.Ink.ExtractStrokes(Root.FormCollection.IC.Ink.Strokes, ExtractFlags.CopyFromOriginal);

            if (Root.InkVisible)
            {
                //foreach (Stroke st in Root.FormCollection.IC.Ink.Strokes)
                foreach (Stroke st in Ink2.Strokes)
                {
                    if (IgnoreBackground && st.ExtendedProperties.Contains(Root.ISBACKGROUND_GUID))
                        continue;
                    if (st.ExtendedProperties.Contains(Root.ISHIDDEN_GUID))
                        continue;
                    //else //Should not be drawn as a stroke : for the moment only filled values.
                    if (st.ExtendedProperties.Contains(Root.ISFILLEDOUTSIDE_GUID))
                    {
                        SolidBrush bru = new SolidBrush(Color.FromArgb(255 - st.DrawingAttributes.Transparency, st.DrawingAttributes.Color));
                        try
                        {
                            GraphicsPath gp = new GraphicsPath();
                            gp.AddRectangle(new Rectangle(0, 0, this.Width, this.Height));
                            Point[] pts = st.DrawingAttributes.FitToCurve ? st.GetFlattenedBezierPoints(0) : st.GetPoints();
                            Root.FormCollection.IC.Renderer.InkSpaceToPixel(gOneStrokeCanvus, ref pts);
                            gp.AddPolygon(pts);
                            g.FillPath(bru, gp);
                        }
                        catch { }
                    }
                    else if (st.ExtendedProperties.Contains(Root.ISFILLEDCOLOR_GUID) || st.ExtendedProperties.Contains(Root.ISFILLEDWHITE_GUID) || st.ExtendedProperties.Contains(Root.ISFILLEDBLACK_GUID))
                    {
                        SolidBrush bru;
                        if (st.ExtendedProperties.Contains(Root.ISFILLEDCOLOR_GUID))
                            bru = new SolidBrush(Color.FromArgb(255 - st.DrawingAttributes.Transparency, st.DrawingAttributes.Color));
                        else if (st.ExtendedProperties.Contains(Root.ISFILLEDWHITE_GUID))
                            bru = new SolidBrush(Color.White);
                        else if (st.ExtendedProperties.Contains(Root.ISFILLEDBLACK_GUID))
                            bru = new SolidBrush(Color.Black);
                        else
                            continue;
                        //bru = new SolidBrush(Color.Purple);
                        if (st.DrawingAttributes.FitToCurve)
                        {
                            try
                            {
                                Point[] pts = st.GetFlattenedBezierPoints(0); // 0 to get a good fitting curve
                                Root.FormCollection.IC.Renderer.InkSpaceToPixel(gOneStrokeCanvus, ref pts);
                                g.FillClosedCurve(bru, pts);
                            }
                            catch { }
                        }
                        else
                        {
                            Point[] pts = st.GetPoints();
                            Root.FormCollection.IC.Renderer.InkSpaceToPixel(gOneStrokeCanvus, ref pts);
                            g.FillPolygon(bru, pts);

                        }

                    }
                    /*else */
                    if (st.ExtendedProperties.Contains(Root.IMAGE_GUID))
                    {
                        //Image img = Root.FormCollection.ClipartsDlg.Images.Images[(int)(st.ExtendedProperties[Root.IMAGE_GUID].Data)];
                        Image img;
                        if (st.ExtendedProperties.Contains(Root.ANIMATIONFRAMEIMG_GUID))
                        {
                            try
                            {
                                AnimationStructure ani = Root.FormCollection.Animations[(int)(st.ExtendedProperties[Root.ANIMATIONFRAMEIMG_GUID].Data)];
                                if (ani.DeleteRequested)
                                {
                                    Root.FormCollection.IC.Ink.DeleteStroke(st);
                                    continue;
                                }
                                img = ani.Image.Frames[ani.Idx].GetImage();
                            }
                            catch
                            {
                                img = gInk.Properties.Resources.unknown;
                            }

                        }
                        else
                            try
                            {
                                img = Root.FormCollection.ClipartsDlg.Originals[(string)(st.ExtendedProperties[Root.IMAGE_GUID].Data)];
                            }
                            catch
                            {
                                img = gInk.Properties.Resources.unknown;
                            }
                        // I came back to this solution of using IMAGE_?_GUID in order to have a more accurate position and therefore prevent blurry image
                        int X = (int)(double)(st.ExtendedProperties[Root.IMAGE_X_GUID].Data);
                        int Y = (int)(double)(st.ExtendedProperties[Root.IMAGE_Y_GUID].Data);
                        int W = (int)(double)(st.ExtendedProperties[Root.IMAGE_W_GUID].Data);
                        int H = (int)(double)(st.ExtendedProperties[Root.IMAGE_H_GUID].Data);
                        if (st.ExtendedProperties.Contains(Root.LISTOFPOINTS_GUID))
                        {
                            Double Rotation = st.ExtendedProperties.Contains(Root.ROTATION_GUID)?(double)st.ExtendedProperties[Root.ROTATION_GUID].Data:0;
                            List<Point> pts = Root.FormCollection.StoredPatternPoints[(int)(st.ExtendedProperties[Root.LISTOFPOINTS_GUID].Data)];
                            foreach (Point pt in pts)
                            {
                                if(Rotation!=0)
                                {
                                    g.TranslateTransform(pt.X, pt.Y);
                                    g.RotateTransform((float)Rotation);
                                    g.TranslateTransform(-pt.X, -pt.Y);
                                }
                                g.DrawImage(img, new Rectangle(pt.X, pt.Y, W, H));
                                g.ResetTransform();
                            }
                        }
                        else
                        {
                            if (st.ExtendedProperties.Contains(Root.ROTATION_GUID))
                            {
                                Double Rotation = (double)st.ExtendedProperties[Root.ROTATION_GUID].Data;
                                g.TranslateTransform(X, Y);
                                g.RotateTransform((float)Rotation);
                                g.TranslateTransform(-X, -Y);
                            }
                            g.DrawImage(img, new Rectangle(X, Y, W, H));
                            g.ResetTransform();
                        }
                    }
                    /*else */
                    if (st.ExtendedProperties.Contains(Root.ISSTROKE_GUID))
                        DrawOneStroke(g, st, null, bmp);
                        //Root.FormCollection.IC.Renderer.Draw(bmp, st);

                    if (st.ExtendedProperties.Contains(Root.TEXT_GUID))
                    {
                        Point pt = new Point((int)(double)st.ExtendedProperties[Root.TEXTX_GUID].Data, (int)(double)st.ExtendedProperties[Root.TEXTY_GUID].Data);
                        Root.FormCollection.IC.Renderer.InkSpaceToPixel(gOneStrokeCanvus, ref pt);
                        System.Drawing.StringFormat stf = new System.Drawing.StringFormat(System.Drawing.StringFormatFlags.NoClip);
                        stf.Alignment = (System.Drawing.StringAlignment)(st.ExtendedProperties[Root.TEXTHALIGN_GUID].Data);
                        stf.LineAlignment = (System.Drawing.StringAlignment)(st.ExtendedProperties[Root.TEXTVALIGN_GUID].Data);
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        if (st.ExtendedProperties.Contains(Root.ROTATION_GUID))
                        {
                            Double Rotation = (double)st.ExtendedProperties[Root.ROTATION_GUID].Data;
                            int W = 0, H = 0;
                            g.TranslateTransform(pt.X + W / 2, pt.Y + H / 2);
                            g.RotateTransform((float)Rotation);
                            g.TranslateTransform(-pt.X - W / 2, -pt.Y - H / 2);
                        }
                        g.DrawString((string)(st.ExtendedProperties[Root.TEXT_GUID].Data),
                                     new Font((string)st.ExtendedProperties[Root.TEXTFONT_GUID].Data, (float)(double)st.ExtendedProperties[Root.TEXTFONTSIZE_GUID].Data,
                                        (System.Drawing.FontStyle)(int)st.ExtendedProperties[Root.TEXTFONTSTYLE_GUID].Data),
                                     new SolidBrush(Color.FromArgb(255 - st.DrawingAttributes.Transparency, st.DrawingAttributes.Color)), pt.X, pt.Y, stf);
                        g.ResetTransform();
                    }
                }
            }
            Ink2?.Dispose();
        }


        public void MoveStrokes(int dy)
		{
			Point pt1 = new Point(0, 0);
			Point pt2 = new Point(0, 100);
			Root.FormCollection.IC.Renderer.PixelToInkSpace(gOneStrokeCanvus, ref pt1);
			Root.FormCollection.IC.Renderer.PixelToInkSpace(gOneStrokeCanvus, ref pt2);
			float unitperpixel = (pt2.Y - pt1.Y) / 100.0f;
			float shouldmove = dy * unitperpixel;
			foreach (Stroke stroke in Root.FormCollection.IC.Ink.Strokes)
				if (!stroke.Deleted)
					stroke.Move(0, shouldmove);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			UpdateFormDisplay(true);
		}


		public uint N1(int i, int j)
		{
			//return BitConverter.ToUInt32(screenbits, (this.Width * j + i) * 4);
			Nlastp1 = (this.Width * j + i) * 4 + 1;
			return screenbits[Nlastp1];
		}
		public uint N2(int i, int j)
		{
			//return BitConverter.ToUInt32(screenbits, (this.Width * j + i) * 4);
			Nlastp2 = (this.Width * j + i) * 4 + 1;
			return screenbits[Nlastp2];
		}
		public uint L(int i, int j)
		{
			//return BitConverter.ToUInt32(lastscreenbits, (this.Width * j + i) * 4);
			Llastp = (this.Width * j + i) * 4 + 1;
			return lastscreenbits[Llastp];
		}
		int Nlastp1, Nlastp2, Llastp;
		public uint Nnext1()
		{
			Nlastp1 += 40;
			return screenbits[Nlastp1];
		}
		public uint Nnext2()
		{
			Nlastp2 += 40;
			return screenbits[Nlastp2];
		}
		public uint Lnext()
		{
			Llastp += 40;
			return lastscreenbits[Llastp];
		}

		public void SnapShot(Rectangle rect,string dest="")
		{
			string snapbasepath = Root.ExpandVarCmd(Root.SnapshotBasePath,0,0,0,0);
			//snapbasepath = Environment.ExpandEnvironmentVariables(snapbasepath);
			//if (Root.SnapshotBasePath == "%USERPROFILE%/Pictures/gInk/")
			if (!System.IO.Directory.Exists(snapbasepath))
				System.IO.Directory.CreateDirectory(snapbasepath);

			if (System.IO.Directory.Exists(snapbasepath))
			{
				IntPtr screenDc = GetDC(IntPtr.Zero);

				const int VERTRES = 10;
				const int DESKTOPVERTRES = 117;
				int LogicalScreenHeight = GetDeviceCaps(screenDc, VERTRES);
				int PhysicalScreenHeight = GetDeviceCaps(screenDc, DESKTOPVERTRES);
				float ScreenScalingFactor = (float)PhysicalScreenHeight / (float)LogicalScreenHeight;

				rect.X = (int)(rect.X * ScreenScalingFactor);
				rect.Y = (int)(rect.Y * ScreenScalingFactor);
				rect.Width = (int)(rect.Width * ScreenScalingFactor);
				rect.Height = (int)(rect.Height * ScreenScalingFactor);


				Bitmap tempbmp = new Bitmap(rect.Width, rect.Height);
                Graphics g = Graphics.FromImage(tempbmp);
                if(Root.StrokesOnlySnapshot)
                {
                    Bitmap temp2bmp = new Bitmap(this.Width, this.Height, PixelFormat.Format32bppArgb);
                    DrawStrokes(temp2bmp,Root.SnapIgnoreBackgroundStroke); 
                    g.DrawImage(temp2bmp, new Rectangle(0, 0, rect.Width, rect.Height), rect, GraphicsUnit.Pixel);
                    temp2bmp.Dispose();
                }
                else
                {
                    g.Clear(Color.Red);

                    IntPtr hDest = CreateCompatibleDC(screenDc);
                    IntPtr hBmp = tempbmp.GetHbitmap();
                    SelectObject(hDest, hBmp);
                    bool b = BitBlt(hDest, 0, 0, rect.Width, rect.Height, screenDc, rect.Left, rect.Top, (uint)(CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt));
                    tempbmp = Bitmap.FromHbitmap(hBmp);

                    if (!b)
                    {
                        g = Graphics.FromImage(tempbmp);
                        g.Clear(Color.Blue);
                        g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(rect.Width, rect.Height));
                    }
                    DeleteObject(hBmp);
                    ReleaseDC(IntPtr.Zero, screenDc);
                    DeleteDC(hDest);
                }

                if (dest == "")
                    Clipboard.SetImage(tempbmp);
                //DateTime now = DateTime.Now;
                //string nowstr = now.Year.ToString() + "-" + now.Month.ToString("D2") + "-" + now.Day.ToString("D2") + " " + now.Hour.ToString("D2") + "-" + now.Minute.ToString("D2") + "-" + now.Second.ToString("D2");
                string savefilename = Root.ExpandVarCmd(Root.SnapshotFileTemplate, 0, 0, 0, 0);//DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss'.png'");
                Root.SnapshotFileFullPath = snapbasepath + savefilename;

                if (dest == "")
                    dest = Root.SnapshotFileFullPath;
                else
                    dest = Environment.ExpandEnvironmentVariables(dest);

                ImageFormat ifo=null;
                switch(System.IO.Path.GetExtension(dest).ToLower())
                {
                    case ".png": ifo = ImageFormat.Png;break;
                    case ".jpg":
                    case ".jpeg": ifo = ImageFormat.Jpeg; break;
                    case ".bmp": ifo = ImageFormat.Bmp; break;
                    case ".emf": ifo = ImageFormat.Emf; break;
                    case ".exf":
                    case ".exif": ifo = ImageFormat.Exif; break;
                    case ".tif":
                    case ".tiff": ifo = ImageFormat.Tiff; break;
                    case ".wmf": ifo = ImageFormat.Wmf; break;
                    case ".gif": ifo = ImageFormat.Gif; break;
                }
                if(ifo==null)
                    tempbmp.Save(dest);
                else
                    tempbmp.Save(dest, ifo);

				tempbmp.Dispose();

				// transfered out :Root.UponBalloonSnap = true;
			}
		}

        private Pen PenForDrawOn(DrawingAttributes dr, DashStyle st)
        {
            if (dr == null)
                dr = Root.FormCollection.IC.DefaultDrawingAttributes;
            Pen p = new Pen(Color.FromArgb(255 - dr.Transparency, dr.Color), Root.HiMetricToPixel(dr.Width));
            p.DashStyle = st != DashStyle.Custom ? st : DashStyle.Solid;

            return p;
        }

        public void DrawLineOnGraphic(Graphics g, int CursorX0, int CursorY0, int CursorX, int CursorY, DrawingAttributes dr = null, DashStyle st = DashStyle.Solid)
        {
            Pen p = PenForDrawOn(dr, st);

            gOutCanvus.DrawLine(p, CursorX0, CursorY0 , CursorX, CursorY);
            p.Dispose();
        }
        public void DrawRectOnGraphic(Graphics g, int CursorX0, int CursorY0, int CursorX, int CursorY,DrawingAttributes dr=null, DashStyle st=DashStyle.Solid)
        {
            int dX = Math.Abs(CursorX - CursorX0);
            int dY = Math.Abs(CursorY - CursorY0);

            Pen p = PenForDrawOn(dr, st);
            gOutCanvus.DrawRectangle(p,Math.Min(CursorX0,CursorX), Math.Min(CursorY0, CursorY), dX, dY);
            p.Dispose();
        }
        public void DrawImagesOnGraphic(Graphics g, List<Point> pts, Image img, int W, int H, DrawingAttributes dr = null, DashStyle st = DashStyle.Solid, bool OnLine = false)
        {
            Double Rotation = 0.0D;
            if (pts == null)
                return;
            //Pen p = PenForDrawOn(dr, st);
            Point PrevPoint = new Point(Int32.MinValue, Int32.MinValue);
            if(OnLine && pts.Count >= 2)
            {
                Rotation = Math.Atan2(pts[1].Y - pts[0].Y, pts[1].X - pts[0].X) / Math.PI * 180.0;
            }
            foreach (Point pt in pts)
            {
                if (PrevPoint.X != Int32.MinValue)
                {
                    Rotation = Math.Atan2(pt.Y - PrevPoint.Y, pt.X - PrevPoint.X) / Math.PI * 180.0;
                }
                if (OnLine)
                {
                    PrevPoint.X = pt.X;
                    PrevPoint.Y = pt.Y;
                }

                if (Rotation != 0)
                {
                    gOutCanvus.TranslateTransform(pt.X + W / 2, pt.Y + H / 2);
                    gOutCanvus.RotateTransform((float)Rotation);
                    gOutCanvus.TranslateTransform(-(pt.X + W / 2), -(pt.Y + H / 2));
                }
                gOutCanvus.DrawImage(img, new Rectangle(pt.X, pt.Y, W, H));
                gOutCanvus.ResetTransform();
            }
            //p.Dispose();
        }
        public void DrawEllipseOnGraphic(Graphics g, int CursorX0, int CursorY0, int CursorX, int CursorY, DrawingAttributes dr = null, DashStyle st = DashStyle.Solid)
        {
            int dX = Math.Abs(CursorX - CursorX0);
            int dY = Math.Abs(CursorY - CursorY0);

            Pen p = PenForDrawOn(dr, st);        
            gOutCanvus.DrawEllipse(p, CursorX0 - dX, CursorY0 - dY, 2 * dX, 2 * dY);
            p.Dispose();
        }

        public void DrawArrowOnGraphic(Graphics g, int CursorX0, int CursorY0, int CursorX, int CursorY, DrawingAttributes dr = null, DashStyle st = DashStyle.Solid)
        {
            Point[] pts = new Point[5];
            double theta = Math.Atan2(CursorY - CursorY0, CursorX - CursorX0);
            Pen p = PenForDrawOn(dr, st);

            double l = Root.FormCollection.ArrowVarLen();

            gOutCanvus.DrawLine(p,CursorX0, CursorY0, (int)(CursorX0 + Math.Cos(theta + Root.ArrowAngle) * l), (int)(CursorY0 + Math.Sin(theta + Root.ArrowAngle) * l));
            gOutCanvus.DrawLine(p, CursorX0, CursorY0, (int)(CursorX0 + Math.Cos(theta - Root.ArrowAngle) * l), (int)(CursorY0 + Math.Sin(theta - Root.ArrowAngle) * l));
            gOutCanvus.DrawLine(p, CursorX0, CursorY0, CursorX,CursorY);

            p.Dispose();
        }

        public void DrawCustomOnGraphic(Graphics g, int CursorX0, int CursorY0, int CursorX, int CursorY)
        {
            if ((CursorX0 != int.MinValue) || (CursorY0 != int.MinValue))
            {
                DrawingAttributes da = Root.FormCollection.IC.DefaultDrawingAttributes; ;
                DashStyle ds;
                try
                {
                    ds = Root.LineStyleFromString(Root.LineStyleToString(da.ExtendedProperties));
                    if (ds == DashStyle.Custom) 
                        ds = DashStyle.Solid;
                }
                catch
                {
                    ds = DashStyle.Solid;
                }
                if (Root.FormCollection.ZoomCapturing)
                    DrawRectOnGraphic(g, CursorX0, CursorY0, CursorX, CursorY, da, ds);
                //DrawRectOnGraphic(g, CursorX0, CursorY0, CursorX, CursorY,Root.FormCollection.IC.Ink.Strokes[Root.FormCollection.IC.Ink.Strokes.Count-1].DrawingAttributes);
                else if ((Root.ToolSelected == Tools.Line) || (Root.ToolSelected == Tools.Poly))
                    DrawLineOnGraphic(g, CursorX0, CursorY0, CursorX, CursorY, da, ds);
                else if ((Root.ToolSelected == Tools.Rect) || (Root.ToolSelected == Tools.ClipArt) || (Root.ToolSelected == Tools.PatternLine && Root.FormCollection.PatternLineSteps == 0))
                    if ((Root.FormCollection.CurrentMouseButton == MouseButtons.Right) || ((int)(Root.FormCollection.CurrentMouseButton) == 2))
                        DrawRectOnGraphic(g, 2 * CursorX0 - CursorX, 2 * CursorY0 - CursorY, CursorX, CursorY, da, ds);
                    else
                        DrawRectOnGraphic(g, CursorX0, CursorY0, CursorX, CursorY, da, ds);
                else if (Root.ToolSelected == Tools.PatternLine && Root.FormCollection.PatternLineSteps == 1)
                {
                    bool m = (Root.FormCollection.CurrentMouseButton == MouseButtons.Right) || ((int)(Root.FormCollection.CurrentMouseButton) == 2);
                    List<Point> pts = new List<Point>();
                    pts.Add(new Point() { X = CursorX0 - (m ? (Root.ImageStamp.X / 2) : 0), Y = CursorY0 - (m ? (Root.ImageStamp.Y / 2) : 0) });
                    pts.Add(new Point() { X = CursorX - (m ? (Root.ImageStamp.X / 2) : 0), Y = CursorY - (m ? (Root.ImageStamp.Y / 2) : 0) });
                    DrawImagesOnGraphic(g, pts, Root.FormCollection.PatternImage, Root.ImageStamp.X, Root.ImageStamp.Y,OnLine: Root.FormCollection.RotatingOnLine);
                }
                else if (Root.ToolSelected == Tools.PatternLine && Root.FormCollection.PatternLineSteps == 2)
                    DrawImagesOnGraphic(g, Root.FormCollection.PatternPoints,Root.FormCollection.PatternImage, Root.ImageStamp.X, Root.ImageStamp.Y,OnLine: Root.FormCollection.RotatingOnLine);
                else if (Root.ToolSelected == Tools.Oval)
                    if ((Root.FormCollection.CurrentMouseButton == MouseButtons.Right) || ((int)(Root.FormCollection.CurrentMouseButton) == 2))
                        DrawEllipseOnGraphic(g, CursorX0, CursorY0, CursorX, CursorY, da, ds);
                    else
                        DrawEllipseOnGraphic(g, (CursorX0 + CursorX) / 2, (CursorY0 + CursorY) / 2, CursorX, CursorY, da, ds);
                else if ((Root.ToolSelected == Tools.StartArrow) || (Root.ToolSelected == Tools.EndArrow))
                    if ((Root.ToolSelected == Tools.StartArrow) ^ ((Root.FormCollection.CurrentMouseButton == MouseButtons.Right) || ((int)(Root.FormCollection.CurrentMouseButton) == 2)))
                        DrawArrowOnGraphic(g, CursorX, CursorY, CursorX0, CursorY0, da, ds);
                    else
                        DrawArrowOnGraphic(g, CursorX0, CursorY0, CursorX, CursorY, da, ds);
            }
        }

        public int Test()
		{
			IntPtr screenDc = GetDC(IntPtr.Zero);

			// big time consuming, but not CPU consuming
			BitBlt(memscreenDc, Width / 4, 0, Width / 2, this.Height, screenDc, Width / 4, 0, 0x00CC0020);
			// <1% CPU
			GetBitmapBits(hScreenBitmap, this.Width * this.Height * 4, screenbits);

			int dj;
			int maxidpixels = 0;
			//float maxidchdrio = 0;
			int maxdj = 0;


			// 25% CPU with 1x10x10 sample rate?
			int istart = Width / 2 - Width / 4;
			int iend = Width / 2 + Width / 4;
			for (dj = -Height * 3 / 8 + 1; dj < Height * 3 / 8 - 1; dj++)
			{
                //int chdpixels = 0;
                int idpixels = 0;
				for (int j = Height / 2 - Height / 8; j < Height / 2 + Height / 8; j += 10)
				{
					L(istart - 10, j);
					N1(istart - 10, j);
					N2(istart - 10, j + dj);
					for (int i = istart; i < iend; i += 10)
					{
						//uint l = Lnext();
						//uint n1 = Nnext1();
						//uint n2 = Nnext2();
						//if (l != n1)
						//{
						//	chdpixels++;
						//	if (l == n2)
						//		idpixels++;
						//}


						if (Lnext() == Nnext2())
							idpixels++;
					}
				}

				//float idchdrio = (float)idpixels / chdpixels;
				if (idpixels > maxidpixels)
				//if (idchdrio > maxidchdrio)
				{
					//maxidchdrio = idchdrio;
					maxidpixels = idpixels;
					maxdj = dj;
				}
			}

			//if (maxidchdrio < 0.1 || maxidpixels < 30)
			if (maxidpixels < 100)
				maxdj = 0;


			// 2% CPU
			IntPtr pscreenbits = Marshal.UnsafeAddrOfPinnedArrayElement(screenbits, (int)(this.Width * this.Height * 4 * 0.375));
			IntPtr plastscreenbits = Marshal.UnsafeAddrOfPinnedArrayElement(lastscreenbits, (int)(this.Width * this.Height * 4 * 0.375));
			memcpy(plastscreenbits, pscreenbits, this.Width * this.Height * 4 / 4);

			ReleaseDC(IntPtr.Zero, screenDc);
			return maxdj;
		}

        private bool MemoSpotLight = false;
		public void UpdateFormDisplay(bool draw,bool prepared=false)
		{
			IntPtr screenDc = GetDC(IntPtr.Zero);
            IntPtr blankcanvusDc = new IntPtr(0) ;
            //Display-rectangle
            Size size = new Size(this.Width, this.Height);
			Point pointSource = new Point(0, 0);
			Point topPos = new Point(this.Left, this.Top);

			//Set up blending options
			BLENDFUNCTION blend = new BLENDFUNCTION();
			blend.BlendOp = AC_SRC_OVER;
			blend.BlendFlags = 0;
			blend.SourceConstantAlpha = 255;  // additional alpha multiplier to the whole image. value 255 means multiply with 1.
			blend.AlphaFormat = AC_SRC_ALPHA;

            //#define SRCCOPY             (DWORD)0x00CC0020 /* dest = source                   */
            if (!prepared)
            {
                BitBlt(OutcanvusDc, 0, 0, this.Width, this.Height, canvusDc, 0, 0, 0x00CC0020);
                if (Root.LassoMode && Root.FormCollection.IC.Ink.Strokes.Count>0 
                    && Root.FormCollection.IC.Ink.Strokes[Root.FormCollection.IC.Ink.Strokes.Count - 1].ExtendedProperties.Contains(Root.ISLASSO_GUID))
                {
                    Stroke stroke = Root.FormCollection.IC.Ink.Strokes[Root.FormCollection.IC.Ink.Strokes.Count - 1];
                    Point[] pts = stroke.GetPoints();
                    Root.FormCollection.IC.Renderer.InkSpaceToPixel(gOutCanvus, ref pts);
                    Pen p = new Pen(Root.FormCollection.AppendToSelection?Color.Red:Color.Violet, 2);  // stroke.DrawingAttributes.Color, Root.HiMetricToPixel(stroke.DrawingAttributes.Width));
                    p.DashStyle = DashStyle.Dash;
                    if (pts.Length >= 3) 
                        gOutCanvus.DrawPolygon(p, pts);
                    p.Dispose();
                }
                else if (Root.Snapping<=0)
                    DrawCustomOnGraphic(gOutCanvus, Root.CursorX0, Root.CursorY0, Root.CursorX, Root.CursorY);
            }

            if (Root.FormCollection.SpotLightMode || Root.FormCollection.SpotLightTemp)
            {
                GraphicsPath gp = new GraphicsPath();
                Brush bru = new SolidBrush(Root.SpotLightColor);
                gp.AddRectangle(new Rectangle(0, 0, this.Width, this.Height));
                Point pt = PointToClient(MousePosition);
                pt.Offset(-Root.SpotLightRadius, -Root.SpotLightRadius);
                gp.AddEllipse(new Rectangle(pt.X, pt.Y, 2 * Root.SpotLightRadius, 2 * Root.SpotLightRadius));
                gOutCanvus.FillPath(bru, gp);
                MemoSpotLight = true;
            }
            else
                MemoSpotLight = false;


            if (draw)
                UpdateLayeredWindow(this.Handle, screenDc, ref topPos, ref size, OutcanvusDc, ref pointSource, 0, ref blend, ULW_ALPHA);
            else
                UpdateLayeredWindow(this.Handle, screenDc, ref topPos, ref size, blankcanvusDc, ref pointSource, 0, ref blend, ULW_ALPHA);

			//Clean-up
			ReleaseDC(IntPtr.Zero, screenDc);
        }

		int stackmove = 0;
		int Tick = 0;
        //DateTime TickStartTime;
        bool SelectionDrawnPreviously=false;

		public void timer1_Tick(object sender, EventArgs e)
		{
            DXAllUpdate |= Root.UponAllDrawingUpdate;
            if (Root.FormCollection == null || !Root.FormCollection.Visible)
                return; // the initialisation is not yet completed. we wait for
			Tick++;

            /*
			if (Tick == 1)
				TickStartTime = DateTime.Now;
			else if (Tick % 60 == 0)
			{
				Console.WriteLine(60 / (DateTime.Now - TickStartTime).TotalMilliseconds * 1000);
				TickStartTime = DateTime.Now;
			}
			*/

            DateTime dt = DateTime.Now;
            if(!Root.FingerInAction)
                foreach (int k in Root.FormCollection.Animations.Keys)
                {
                    AnimationStructure ani = Root.FormCollection.Animations[k];
                    if (dt >= ani.T0)
                    {
                        do
                        {
                            ani.Idx++;
                            ani.Loop--;
                            if (ani.Idx >= ani.Image.NumFrames)
                            {
                                ani.Idx = 0;//= (ani.Idx + 1) % (ani.Image.NumFrames);
                            }
                            else
                                ani.T0 = ani.T0.AddSeconds(ani.Image.Frames[ani.Idx].GetDelay());
                        }
                        while (dt > ani.T0);
                        Root.UponAllDrawingUpdate = true;
                    }
                    if (ani.Loop <= 0 || dt > ani.TEnd)
                    {
                        ani.T0 = DateTime.MaxValue;
                        ani.TEnd = DateTime.MaxValue;
                        ani.Loop = int.MaxValue;
                        if (ani.DeleteAtDend)
                            ani.DeleteRequested = true;
                    }
                }

            if (Root.UponAllDrawingUpdate)
            {
                ClearCanvus();
                DrawStrokes();
                DrawButtons(Root.UponButtonsUpdate > 0);
                Root.UponButtonsUpdate = 0;
                if (Root.Snapping > 0)
                    DrawSnapping(Root.SnappingRect);
				UpdateFormDisplay(true);
				Root.UponAllDrawingUpdate = false;
			}

			else if (Root.UponTakingSnap)
            {
                if (Root.ResizeDrawingWindow)
                {
                    if ((Root.SnappingRect.Width == SystemInformation.VirtualScreen.Width) && (Root.SnappingRect.Height == SystemInformation.VirtualScreen.Height))
                        Root.WindowRect = new Rectangle(Int32.MinValue, Int32.MinValue, -1, -1);
                    else
                        Root.WindowRect = new Rectangle(Root.SnappingRect.Left, Root.SnappingRect.Top, Root.SnappingRect.Width, Root.SnappingRect.Height);
                    Root.StopInk();
                    Root.StartInk();
                    return;
                }
                if (Root.SnappingRect.Width == this.Width && Root.SnappingRect.Height == this.Height)
					System.Threading.Thread.Sleep(200);
				ClearCanvus();
				DrawStrokes();
				//DrawButtons(false);
				UpdateFormDisplay(true);
                if (Root.VideoRecordWindowInProgress)
                {
                    Root.FormCollection.VideoRecordStartFFmpeg(Root.SnappingRect);
                    Root.UponTakingSnap = false;
                    Root.VideoRecordWindowInProgress = false;
                    return;
                }
                SnapShot(Root.SnappingRect);
                if (Root.FormCollection.SnapWithoutClosing)
                {
                    Root.Snapping = 0;
                    Root.FormCollection.ButtonsEntering = 1;
                }
                Root.UponBalloonSnap = true;
                Root.UponTakingSnap = false;
                if (!Root.FormCollection.SnapWithoutClosing && (Root.APIRestCloseOnSnap || Root.CloseOnSnap == "true" || (Root.CloseOnSnap == "blankonly" && Root.FormCollection.IC.Ink.Strokes.Count == 0)))
                    Root.FormCollection.RetreatAndExit(true); // Quick exit
                Root.FormCollection.SnapWithoutClosing = false;
                /*
                if (Root.CloseOnSnap == "true")
				{
					Root.FormCollection.RetreatAndExit();
				}
				else if (Root.CloseOnSnap == "blankonly")
				{
					if ((Root.FormCollection.IC.Ink.Strokes.Count == 0))
						Root.FormCollection.RetreatAndExit();
				}*/
            }

			else if (Root.Snapping == 2)
			{
				if (Root.MouseMovedUnderSnapshotDragging)
				{
					ClearCanvus();
					DrawStrokes();
					DrawButtons(false);
					DrawSnapping(Root.SnappingRect);
					UpdateFormDisplay(true);
					Root.MouseMovedUnderSnapshotDragging = false;
				}
			}

			else if ((Root.FormCollection != null && Root.FormCollection.Visible) && (Root.FormCollection.IC != null && Root.FormCollection.Visible) && Root.FormCollection.IC.CollectingInk && Root.EraserMode == false && Root.InkVisible)
			{ // Drawing in progress : we get the last stroke in the list, if we have to draw because not deleted and not a shape in progress
              //we replace the rectangle containing the stroke by the saved one and 

				if (Root.FormCollection.IC.Ink.Strokes.Count > 0)
				{
					Stroke stroke = Root.FormCollection.IC.Ink.Strokes[Root.FormCollection.IC.Ink.Strokes.Count - 1];
					if ((!stroke.Deleted) && (!Root.FormCollection.ZoomCapturing)&&(Root.ToolSelected == Tools.Hand))
                    {
                        BitBlt(OutcanvusDc, 0, 0, this.Width, this.Height, canvusDc, 0, 0, 0x00CC0020);
                        //Root.FormCollection.IC.Renderer.Draw(gOutCanvus, stroke, Root.FormCollection.IC.DefaultDrawingAttributes);
                        DrawOneStroke(gOutCanvus, stroke, Root.FormCollection.IC.DefaultDrawingAttributes);
                    }
                    UpdateFormDisplay(true, (Root.ToolSelected == Tools.Hand) && (!Root.FormCollection.ZoomCapturing));
                    
                }
            }

			else if ((Root.FormCollection.IC.CollectingInk && Root.EraserMode == true) || Root.StrokeHovered != null || SelectionDrawnPreviously)
            {
                SelectionDrawnPreviously = Root.StrokeHovered != null; // to erase selection at the end
                ClearCanvus();
				DrawStrokes();
				DrawButtons(false);
				UpdateFormDisplay(true);
			}

			else if (Root.Snapping < -58)
			{
				ClearCanvus();
				DrawStrokes();
				DrawButtons(false);
				UpdateFormDisplay(true);
			}

			else if (Root.UponButtonsUpdate > 0)
			{
				if ((Root.UponButtonsUpdate & 0x2) > 0)
					DrawButtons(true, (Root.UponButtonsUpdate & 0x4) > 0);
				else if ((Root.UponButtonsUpdate & 0x1) > 0)
					DrawButtons(false, (Root.UponButtonsUpdate & 0x4) > 0);
				UpdateFormDisplay(true);
				Root.UponButtonsUpdate = 0;
			}

			else if (Root.UponSubPanelUpdate)
			{
				ClearCanvus();
				DrawStrokes();
				DrawButtons(false);
				UpdateFormDisplay(true);
				Root.UponSubPanelUpdate = false;
			}
            else if (Root.FormCollection.SpotLightMode || Root.FormCollection.SpotLightTemp || MemoSpotLight)
            {
                UpdateFormDisplay(true);
            }


            if (Root.AutoScroll && Root.PointerMode)
			{
				int moved = Test();
				stackmove += moved;

				if (stackmove != 0 && Tick % 10 == 1)
				{
					MoveStrokes(stackmove);
					ClearCanvus();
					DrawStrokes();
					DrawButtons(false);
					UpdateFormDisplay(true);
					stackmove = 0;
				}
			}
		}

        public void DrawInProgressStroke(GameOverlay.Drawing.Graphics g)
        {
            Bitmap bmp = new Bitmap(this.Width, this.Height, PixelFormat.Format32bppArgb);
            DrawStrokes(bmp, true);
            System.IO.MemoryStream io = new System.IO.MemoryStream();
            bmp.Save(io, ImageFormat.Png);
            g.BeginScene();
            g.ClearScene();
            g.DrawImage(new GameOverlay.Drawing.Image(g, io.GetBuffer()), new GameOverlay.Drawing.Point(0, 0));
            g.EndScene();
        }

        public void DXUpdateFormDisplay(bool prepared = false)
        {
            if (!prepared)
            {
                if (Root.Snapping <= 0)
                    DrawCustomOnGraphic(gOutCanvus, Root.CursorX0, Root.CursorY0, Root.CursorX, Root.CursorY);
            }
        }

        public void timerDX_Tick(object sender, EventArgs e)
        {
            //string st = "DX " + Root.FormCollection.ButtonsEntering.ToString();
            //Console.WriteLine(st);
            DateTime dt = DateTime.Now;
            if (!this.Visible)
                return;

            if (Root.FingerInAction)
            {
                if (Root.FormCollection.IC.Ink.Strokes.Count > 0)
                {                    
                    Stroke stroke = Root.FormCollection.IC.Ink.Strokes[Root.FormCollection.IC.Ink.Strokes.Count - 1];
                    if ((!stroke.Deleted) && (!Root.FormCollection.ZoomCapturing))
                    {
                        DXInProgressGraphics.Clear(Color.Transparent);
                        if (Root.ToolSelected == Tools.Hand)
                            DrawOneStroke(DXInProgressGraphics, stroke, Root.FormCollection.IC.DefaultDrawingAttributes, DXInprogressBmp);
                        else
                            DrawCustomOnGraphic(DXInProgressGraphics, Root.CursorX0, Root.CursorY0, Root.CursorX, Root.CursorY);
                        System.IO.MemoryStream io = new System.IO.MemoryStream();
                        DXInprogressBmp.Save(io, ImageFormat.Png);
                        DXgfx.BeginScene();
                        DXgfx.ClearScene();
                        DXgfx.DrawImage(new GameOverlay.Drawing.Image(DXgfx, DXCaptureMemIO.GetBuffer()), new GameOverlay.Drawing.Point(0, 0));
                        DXgfx.DrawImage(new GameOverlay.Drawing.Image(DXgfx, io.GetBuffer()), new GameOverlay.Drawing.Point(0, 0));
                        DXgfx.EndScene();
                    }
                }
                Console.WriteLine("dxi = " + (DateTime.Now - dt).TotalMilliseconds.ToString());
            }
            else if (DXgfx != null && DXAllUpdate && Root.FormCollection.ButtonsEntering == 0)
            {
                DXAllUpdate = false;
                Console.WriteLine("DX");
                LocalCopyInk?.Dispose();
                LocalCopyInk = Root.FormCollection.IC.Ink.Clone();
                DXCaptureGraphics.Clear(Color.Transparent);
                DrawStrokes(DXCapturedBmp);
                Console.WriteLine("dx1 = " + (DateTime.Now - dt).TotalMilliseconds.ToString());
                DXCaptureMemIO.Position = 0;
                Console.WriteLine("dx2 = " + (DateTime.Now - dt).TotalMilliseconds.ToString());
                DXCaptureMemIO.SetLength(0);
                DXCapturedBmp.Save(DXCaptureMemIO, ImageFormat.Png);
                Console.WriteLine("dx3 = " + (DateTime.Now - dt).TotalMilliseconds.ToString());
                DXgfx.BeginScene();
                DXgfx.ClearScene();
                DXgfx.DrawImage(new GameOverlay.Drawing.Image(DXgfx, DXCaptureMemIO.GetBuffer()), new GameOverlay.Drawing.Point(0, 0));
                DXgfx.EndScene();

                //DXUpdateFormDisplay();
                Console.WriteLine("dx = "+(DateTime.Now-dt).TotalMilliseconds.ToString());
            }
        }

        private void FormDisplay_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                ;
            }
            else
            {
                FormDisplay_FormClosed(null,null);
            }

        }

        private void FormDisplay_FormClosed(object sender, FormClosedEventArgs e)
		{
			DeleteObject(Canvus);
            DeleteObject(OutCanvus);
            DeleteObject(OneStrokeCanvus);
            //DeleteObject(BlankCanvus);
            DeleteDC(canvusDc);
            DeleteDC(OutcanvusDc);
            DeleteDC(onestrokeDc);
            if (gpButtonsImage != null)
                gpButtonsImage.Dispose();
            if (gpPenWidthImage != null)
                gpPenWidthImage.Dispose();
            if (gpSubToolsImage != null)
                gpSubToolsImage.Dispose();
            if (TransparentBrush != null)
                TransparentBrush.Dispose();
            if (SemiTransparentBrush != null)
                SemiTransparentBrush.Dispose();

            if (Root.AutoScroll)
			{
				DeleteObject(hScreenBitmap);
				DeleteDC(memscreenDc);
			}
		}




        public bool HasFocus()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }

        [DllImport("user32.dll")]
		static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
		[DllImport("user32.dll", SetLastError = true)]
		static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);
		[DllImport("user32.dll")]
		static extern int SetWindowLong(IntPtr hWnd, int nIndex, UInt32 dwNewLong);
		[DllImport("user32.dll")]
		public extern static bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

		[DllImport("gdi32.dll")]
		static extern int GetBitmapBits(IntPtr hbmp, int cbBuffer, [Out] byte[] lpvBits);
		[DllImport("gdi32.dll")]
		static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

		[DllImport("gdi32.dll")]
		static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

		[DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
		public static extern IntPtr memcpy(IntPtr dest, IntPtr src, int count);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
		[DllImport("user32.dll")]
		static extern IntPtr GetDC(IntPtr hWnd);
		[DllImport("user32.dll")]
		static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);
		[DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
		public static extern bool DeleteDC([In] IntPtr hdc);
		[DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", SetLastError = true)]
		static extern IntPtr CreateCompatibleDC([In] IntPtr hdc);
		[DllImport("gdi32.dll", EntryPoint = "SelectObject")]
		public static extern IntPtr SelectObject([In] IntPtr hdc, [In] IntPtr hgdiobj);
		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DeleteObject([In] IntPtr hObject);
		[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
		static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pptSrc, uint crKey, [In] ref BLENDFUNCTION pblend, uint dwFlags);
		[DllImport("gdi32.dll")]
		public static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);
		[DllImport("gdi32.dll")]
		public static extern bool StretchBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int nWidthSrc, int nHeightSrc, long dwRop);


		[StructLayout(LayoutKind.Sequential)]
		public struct BLENDFUNCTION
		{
			public byte BlendOp;
			public byte BlendFlags;
			public byte SourceConstantAlpha;
			public byte AlphaFormat;

			public BLENDFUNCTION(byte op, byte flags, byte alpha, byte format)
			{
				BlendOp = op;
				BlendFlags = flags;
				SourceConstantAlpha = alpha;
				AlphaFormat = format;
			}
		}

		const int ULW_ALPHA = 2;
		const int AC_SRC_OVER = 0x00;
		const int AC_SRC_ALPHA = 0x01;
    }
}
