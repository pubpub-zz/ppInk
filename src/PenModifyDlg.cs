using System;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace gInk
{
    public partial class PenModifyDlg : Form
    {
        Root Root;
        Bitmap HSBitmap = null;

        public PenModifyDlg(Root root)
        {
            Root = root;
            InitializeComponent();
            OkBtn.Text = Root.Local.ButtonOkText;
            CancelBtn.Text = Root.Local.ButtonCancelText;
            this.Text = Root.Local.OptionsHotkeysColorEdit;
            string sin = Root.Local.OptionsPensFading;
            int i = sin.IndexOf("(");
            if (i < 0) i = sin.Length;
            FadingCB.Text = sin.Substring(0, i);
            DashStyleGrp.Text = Root.Local.OptionsLineStyle;
            HSBitmap = new Bitmap(SVSquare.Width, SVSquare.Height);
            SVSquare.BackgroundImage = HSBitmap;
            DoubleBuffered = true;
            CancelBtn.Select(); // in order to prevent input during hotkey hold down
        }

        public void setColor(int alpha,Color c)
        {
            colorEditorManager.Color = Color.FromArgb(255-alpha, c);
        }

        public int getAlpha()
        {
            return 255-colorEditorManager.Color.A;
        }

        public Color getColor()
        {
            return colorEditorManager.Color;
        }

        bool WidthChanging = false;
        public void setWidth(float w)
        {
            WidthChanging = true;
            WidthTb.Text = ((int)w).ToString();
            pboxPenWidthIndicator.Left = (int)(Math.Sqrt(w / 1250)* 200);
            WidthChanging = false;
        }

        private void WidthTb_Validating(object sender, CancelEventArgs e)
        {
            int i;
            if (int.TryParse(WidthTb.Text, out i))
                setWidth(i);
        }

        private void pboxPenWidthIndicator_Move(object sender, EventArgs e)
        {
            double f = (pboxPenWidthIndicator.Left / 200.0);
            if(!WidthChanging)
                WidthTb.Text = ((int)(f * f * 1250.0)).ToString();
        }

        public float getWidth()
        {
            return float.Parse(WidthTb.Text, CultureInfo.InvariantCulture);
        }

        public void setDashStyle(Microsoft.Ink.DrawingAttributes pen)
        {
            if (!pen.ExtendedProperties.Contains(Root.DASHED_LINE_GUID))
                StyleStrokeRd.Checked = true;
            else
            switch((DashStyle)(pen.ExtendedProperties[Root.DASHED_LINE_GUID].Data))
            {
                case DashStyle.Solid:
                    StyleSolidRd.Checked = true;
                    break;
                case DashStyle.Dash:
                    StyleDashRd.Checked = true;
                    break;
                case DashStyle.Dot:
                    StyleDotRd.Checked = true;
                    break;
                case DashStyle.DashDot:
                    StyleDashDotRd.Checked = true;
                    break;
                case DashStyle.DashDotDot:
                    StyleDashDotDotRd.Checked = true;
                    break;
            }

        }

        public DashStyle getDashStyle()
        {

            if (StyleStrokeRd.Checked)
                return DashStyle.Custom; // temporary means stroke
            else if (StyleSolidRd.Checked)
                return DashStyle.Solid;
            else if (StyleDashRd.Checked)
                return DashStyle.Dash;
            else if (StyleDotRd.Checked)
                return DashStyle.Dot;
            else if (StyleDashDotRd.Checked)
                return DashStyle.DashDot;
            else if (StyleDashDotDotRd.Checked)
                return DashStyle.DashDotDot;
            else
                throw (new Exception("can not identify DashStyle"));
        }

        public bool ModifyPen(ref Microsoft.Ink.DrawingAttributes pen)
        {
            int i = Filling.Invalid;
            return ModifyPenAndFilling(ref pen, ref i);
        }

            public bool ModifyPenAndFilling(ref Microsoft.Ink.DrawingAttributes pen, ref int fill)
        {
            setColor(pen.Transparency, pen.Color);
            setWidth(pen.Width);
            FadingCB.Checked = pen.ExtendedProperties.Contains(Root.FADING_PEN);
            setDashStyle(pen);
            if(fill != Filling.Invalid)
            {
                FillingLbl.Visible = true;
                FillingCombo.Visible = true;
                FillingCombo.SelectedIndex = fill;
                string[] lst = Root.Local.ListEditDlgFillingsText.Split(';');
                for(int i = 0; i < lst.Count(); i++)
                    FillingCombo.Items[i] = lst[i];
            }
            if (ShowDialog() == DialogResult.OK)
            {
                fill = FillingCombo.SelectedIndex;
                pen.Color = getColor();
                pen.Transparency = (byte)getAlpha();
                pen.Width = getWidth();
                if (FadingCB.Checked)
                    pen.ExtendedProperties.Add(Root.FADING_PEN, Root.TimeBeforeFading);
                else
                    try { pen.ExtendedProperties.Remove(Root.FADING_PEN); } catch { };
                DashStyle d = getDashStyle();
                if (d == DashStyle.Custom)
                    try { pen.ExtendedProperties.Remove(Root.DASHED_LINE_GUID); } catch { }
                else
                    pen.ExtendedProperties.Add(Root.DASHED_LINE_GUID, d);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void hideWidth()
        {
            pboxPenWidthIndicator.Visible = false;
            gpPenWidth.BackgroundImage = null;
            //gpPenWidth.Visible = false;
            //previewPanel.Visible = false;
            DashStyleGrp.Visible = false;
        }

        /// inspired from FormCollection
        bool gpPenWidth_MouseOn;
        private void gpPenWidth_MouseDown(object sender, MouseEventArgs e)
        {
            gpPenWidth_MouseOn = true;
        }

        private void gpPenWidth_MouseMove(object sender, MouseEventArgs e)
        {
            if (!gpPenWidth_MouseOn)
                return;
            if (e.X < pboxPenWidthIndicator.Width/2)
                pboxPenWidthIndicator.Left = 0;
            else if (e.X > (gpPenWidth.Width - pboxPenWidthIndicator.Width/2))
                pboxPenWidthIndicator.Left = (gpPenWidth.Width - pboxPenWidthIndicator.Width/2);
            else
                pboxPenWidthIndicator.Left = e.X - pboxPenWidthIndicator.Width / 2;
        }

        private void gpPenWidth_MouseUp(object sender, MouseEventArgs e)
        {
            gpPenWidth_MouseMove(sender, e);
            gpPenWidth_MouseOn = false;
        }

        private void pboxPenWidthIndicator_MouseDown(object sender, MouseEventArgs e)
        {
            gpPenWidth_MouseOn = true;
        }

        private void pboxPenWidthIndicator_MouseMove(object sender, MouseEventArgs e)
        {
            if (gpPenWidth_MouseOn)
                gpPenWidth_MouseMove(sender, new MouseEventArgs(e.Button, e.Clicks, e.X + (sender as Control).Left, e.Y, e.Delta));
        }

        private void pboxPenWidthIndicator_MouseUp(object sender, MouseEventArgs e)
        {
            gpPenWidth_MouseOn = false;
        }

        private void colorEditorManager_ColorChanged(object sender, EventArgs e)
        {
           gpPenWidth.BackColor = colorEditorManager.Color;
        }

        float HueMemo=1000;
        private void colorEditor_ColorChanged(object sender, EventArgs e)
        {
            CursorHSI.Left = (int)(colorEditor.Color.GetSaturation() * SVSquare.Width+.5)-4;
            CursorHSI.Top = (int)((1-colorEditor.Color.GetBrightness()) * SVSquare.Height+.5)-4;
            float Hue = colorEditor.Color.GetHue();
            if (Math.Abs(Hue-HueMemo)>=1)
            {
                HueMemo = Hue;
                Cyotek.Windows.Forms.HslColor HSL= Color.Black;
                HSL.H = Hue;
                for (int x = 0; x < SVSquare.Width; x++)
                {
                    HSL.S = (float)x / SVSquare.Width;
                    for (int y = 0; y < SVSquare.Height; y++)
                    {
                        HSL.L = 1-(float)y / SVSquare.Height;
                        HSBitmap.SetPixel(x, y, HSL.ToRgbColor());
                    }
                }
                SVSquare.Invalidate();
            }
        }

        private void SVSquare_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.Button !=MouseButtons.None)
            {
                colorEditor.Color = HSBitmap.GetPixel(Math.Min(Math.Max(0,e.X),SVSquare.Width-1), Math.Min(Math.Max(0, e.Y), SVSquare.Height-1));
            }
        }

        private void CursorHSI_MouseMove(object sender, MouseEventArgs e)
        {
            SVSquare_MouseMove(sender, new MouseEventArgs(e.Button, e.Clicks, e.X + CursorHSI.Left, e.Y + CursorHSI.Top, e.Delta));
        }

        private void PenModifyDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (getWidth() == 0)
            {
                switch (MessageBox.Show(Root.Local.QuestionMinWidth, "ppInk", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        return;
                    case DialogResult.No:
                        setWidth(1);
                        break;
                    }
                }

        }
    }
}
