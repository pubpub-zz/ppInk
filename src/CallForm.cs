using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace gInk
{
    public partial class CallForm : Form
    {
        public Root Root;
        public bool FirstActivation = true;
        private int AltTabPressed = 0;
        public CallForm(Root r)
        {
            InitializeComponent();
            Root = r;
            if (File.Exists(Global.ProgramFolder + "FloatingCall.png"))
                BackgroundImage = new Bitmap(Global.ProgramFolder + "FloatingCall.png");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if ((GetAsyncKeyState(0x12) & 0x8000) != 0)
            {
                if ((GetAsyncKeyState(0x9) & 0x8000) != 0)
                    AltTabPressed = 10;
            }
            else
                AltTabPressed = AltTabPressed > 0 ? AltTabPressed - 1 : 0;
        }

        private void CallForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\n' || e.KeyChar == ' ')
                Root.callshortcut();
        }

        private void CallForm_Activated(object sender, EventArgs e)
        {
            if (FirstActivation)
            {
                FirstActivation = false;
                return;
            }
            Console.WriteLine(AltTabPressed);
            if (AltTabPressed > 0)
                Root.callshortcut();
        }

        private void CallForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Root.callshortcut();
        }

        private void _MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Left += e.X - Width / 2;
                Top += e.Y - Height / 2;
                Root.FormLeft = Left;
                Root.FormTop = Top;
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetKeyboardState(byte[] lpKeyState);
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

    }
}
