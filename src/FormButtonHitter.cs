using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace gInk
{
    public partial class FormButtonHitter : Form
    {
        Root Root;
        FormCollection FC;
        // DateTime MouseTimeDown;
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

        public FormButtonHitter(Root root)
        {
            Root = root;
            FC = Root.FormCollection;
            InitializeComponent();

            this.Left = FC.gpButtons.Left + FC.Left;
            this.Top = FC.gpButtons.Top + FC.Top;
            this.Width = FC.gpButtons.Width;
            this.Height = FC.gpButtons.Height;
        }

        protected override void WndProc(ref Message msg)
        {
            //if ((msg.Msg == 0x001C) || (msg.Msg == 6)) //WM_ACTIVATEAPP : generated through alt+tab
            if ((msg.Msg == 6)) //WM_ACTIVATE : generated through alt+tab
            {
                //Console.WriteLine(DateTime.Now.ToString() + " !msgH " + msg.Msg.ToString()+" - "+ msg.WParam.ToString());
                //if ((msg.Msg == 6) || ((msg.Msg == 0x001C) && (msg.WParam != IntPtr.Zero))
                {
                    //Console.WriteLine("activating from hitter " + (Root.PointerMode ? "pointer" : "not") + (Root.Docked ? "docked" : "not"));
                    Root.FormCollection.AltTabActivate();
                    return;
                }
            }
            /*else if (msg.Msg != 0x0113)
            {
                Console.WriteLine(DateTime.Now.ToString()+" msgH " + msg.Msg.ToString());
            }*/
            base.WndProc(ref msg);
        }

        private void FormButtonHitter_Click(object sender, EventArgs e)
        {
            MouseEventArgs m = (MouseEventArgs)e;
            TimeSpan tsp = DateTime.Now - Root.FormCollection.MouseTimeDown;
            //MessageBox.Show(string.Format("t = {0:N3}",tsp.TotalSeconds));
            foreach (Control control in FC.gpButtons.Controls)
            {
                if (control.GetType() == typeof(Button))
                {
                    if (m.X >= control.Left && m.X <= control.Right && m.Y >= control.Top && m.Y <= control.Bottom)
                        ((Button)control).PerformClick();
                }
            }
        }


        public void ToTopMost()
        {
            TopMost = true;
            uint dwExStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, dwExStyle | 0x00080000);
            //SetLayeredWindowAttributes(this.Handle, 0x00FFFFFF, 200, 0x2);
            SetWindowPos(this.Handle, (IntPtr)(-1), 0, 0, 0, 0, 0x0002 | 0x0001 | 0x0020);
        }

        public void timer1_Tick(object sender, EventArgs e)
        {
            if (true || this.Visible)  // we force resizing weither visible or not in order to have the good size during alt+tab
            {
                this.Left = FC.gpButtons.Left + FC.Left;
                this.Top = FC.gpButtons.Top + FC.Top;
                Size s = FC.VisibleToolbar;
                this.Width = s.Width;
                this.Height = s.Height;
            }
        }

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
        [DllImport("user32.dll")]
        public extern static bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        private void FormButtonHitter_MouseDown(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("BH", (sender as Control).Name);
            Root.FormCollection.MouseTimeDown = DateTime.Now;
        }
    }
}
