using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace gInk
{
    public class Hotkey
    {
        public int Key;
        public bool Control;
        public bool Alt;
        public bool Shift;
        public bool Win;

        public static bool IsValidKey(Keys key)
        {
            // we ignore ctrl,alt,shift as unique keys : they must be part of a global key
            if (key != Keys.ControlKey && key != Keys.LControlKey && key != Keys.RControlKey
                 && key != Keys.Menu && key != Keys.LMenu && key != Keys.RMenu
                 && key != Keys.ShiftKey && key != Keys.LShiftKey && key != Keys.RShiftKey)
                return true;
            else
                return false;
        }

        public override string ToString()
        {
            if (Key > 0)
            {
                string str = "";
                if (Control) str += LocalSt.KeyNames[0x00020000] + " + ";
                if (Alt) str += LocalSt.KeyNames[0x00040000] + " + ";
                if (Shift) str += LocalSt.KeyNames[0x00010000] + " + ";
                if (Win) str += LocalSt.KeyNames[0x5B] + " + ";
                str += LocalSt.KeyNames[Key];
                return str;
            }
            else
            {
                return LocalSt.KeyNames[0];
            }
        }
        public string ToStringInvariant()
        {
            if (Key > 0)
            {
                string str = "";
                if (Control) str += "Ctrl + ";
                if (Alt) str += "Alt + ";
                if (Shift) str += "Shift + ";
                if (Win) str += "Win + ";
                str += ((Keys)Key).ToString();
                return str;
            }
            else
            {
                return "None";
            }
        }

        public bool Parse(string para)
        {
            Keys kk;
            if (para == "None")
            {
                Key = 0;
                Control = false;
                Alt = false;
                Shift = false;
                Win = false;
                return true;
            }
            else if (para.Length >= 1)
            {
                try
                {
                    Control = para.Contains("Control") || para.Contains("Ctrl");
                    Alt = para.Contains("Alt");
                    Shift = para.Contains("Shift");
                    Win = para.Contains("Win");
                    para = para.Replace(" ", "");
                    para = para.Split('+').Last();
                    kk = (Keys)Enum.Parse(typeof(Keys), para, true);
                    Key = (int)kk;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool ModifierMatch(bool control, bool alt, bool shift, bool win)
        {
            return Control == control && Alt == alt && Shift == shift && Win == win;
        }

        public bool ModifierMatch(bool control, int alt, bool shift, bool win)
        {
            if (alt == -1)
                return Control == control /*&& Alt == alt */ && Shift == shift && Win == win;
            else
                return Control == control && Alt == (alt == 1) && Shift == shift && Win == win;
        }

        public bool ConflictWith(Hotkey hotkey)
        {
            if (Key == 0 || hotkey.Key == 0)
                return false;
            else if (Control == hotkey.Control && Alt == hotkey.Alt && Shift == hotkey.Shift && Win == hotkey.Win && Key == hotkey.Key)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
