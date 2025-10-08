using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Ink;

namespace gInk
{
    public partial class FormInput : Form
    {
        Microsoft.Ink.Stroke stroke;
        Root Root;
        string Saved_Txt;
        Font Saved_Font;
        DrawingAttributes Saved_Da;
        bool Saved_Frame;
        bool Saved_White;
        bool Saved_Black;


        public FormInput(string caption, string label, string txt, bool ML, gInk.Root rt = null, Microsoft.Ink.Stroke stk = null, bool invisible = false)
        {
            InitializeComponent();
            
            // local
            this.btOK.Text = rt.Local.ButtonOkText;
            this.btCancel.Text = rt.Local.ButtonCancelText;
            this.FontBtn.Text = rt.Local.ButtonFontText;
            this.ColorBtn.Text = rt.Local.OptionsPensColor;
            boxingCb.Items.AddRange(rt.Local.TextFramingText.Split(';'));

            Text = caption;
            captionLbl.Text = label;
            if (ML)
            {
                InputML.Visible = true;
                txt = txt.Replace("\r\n", "\n").Replace("\n", "\r\n"); //in order to get multiline text to be correctly editable after restore
                InputML.Text = txt;
                ActiveControl = InputML;
            }
            else
            {
                InputSL.Visible = true;
                InputSL.Text = txt;
                ActiveControl = InputSL;
            }
            Root = rt;
            stroke = stk;

            if (invisible)
            {
                if (rt.TextDlgHiddenOpacity < 1.0)
                    this.VisibleCb.Checked = true;
            }
            else
                this.VisibleCb.Visible = false;

            if (stroke == null)
            {
                FontBtn.Visible = false;
                boxingCb.Visible = false;
            }
            else
            {
                FontBtn.Visible = true;
                ColorBtn.Visible = true;
                boxingCb.Visible = true;// !stk.ExtendedProperties.Contains(Root.ISTAG_GUID);

                FontDlg.Font = new Font((string)stk.ExtendedProperties[Root.TEXTFONT_GUID].Data, (float)(double)stk.ExtendedProperties[Root.TEXTFONTSIZE_GUID].Data,
                                        (System.Drawing.FontStyle)stk.ExtendedProperties[Root.TEXTFONTSTYLE_GUID].Data);
                InputML.TextChanged += new System.EventHandler(this.InputML_TextChanged);
                int i = (stk.ExtendedProperties.Contains(Root.ISSTROKE_GUID) ? 1 : 0) +
                        (stk.ExtendedProperties.Contains(Root.ISFILLEDWHITE_GUID) ? 2 : 0) +
                        (stk.ExtendedProperties.Contains(Root.ISFILLEDBLACK_GUID) ? 4 : 0);
                boxingCb.Text = boxingCb.Items[i].ToString();

                Saved_Txt = InputML.Text;
                Saved_Da = stk.DrawingAttributes.Clone();
                Saved_Font = (Font)FontDlg.Font.Clone();
                Saved_Frame = stk.ExtendedProperties.Contains(Root.ISSTROKE_GUID);
                Saved_White = stk.ExtendedProperties.Contains(Root.ISFILLEDWHITE_GUID);
                Saved_Black = stk.ExtendedProperties.Contains(Root.ISFILLEDBLACK_GUID);
                if (ML)
                    InputML.SelectAll();
                else
                    InputSL.SelectAll();
            }
        }

        public void TextIn(string txt)
        {
            if (InputML.Visible)
                InputML.Text=txt;
            else
                InputSL.Text=txt;
        }

        public string TextOut()
        {
            if (InputML.Visible)
                return InputML.Text;
            else
                return InputSL.Text;
        }

        private void FontBtn_Click(object sender, EventArgs e)
        {
            if (FontDlg.ShowDialog() == DialogResult.OK)
            {
                stroke.ExtendedProperties.Add(Root.TEXTFONT_GUID, FontDlg.Font.Name);
                stroke.ExtendedProperties.Add(Root.TEXTFONTSIZE_GUID, (double)FontDlg.Font.Size);
                stroke.ExtendedProperties.Add(Root.TEXTFONTSTYLE_GUID, FontDlg.Font.Style);
                string st=InputML.Text;
                // to run event for refresh
                InputML.Text = "";InputML.Text = st;
            }
        }

        private void ColorBtn_Click(object sender, EventArgs e)
        {
            PenModifyDlg dlg = new PenModifyDlg(Root);
            DrawingAttributes da = stroke.DrawingAttributes.Clone();
            //dlg.hideWidth();
            if (dlg.ModifyPen(ref da))
            {
                stroke.DrawingAttributes = da;
                Root.FormDisplay.ClearCanvus();
                Root.FormDisplay.DrawStrokes();
                Root.FormDisplay.UpdateFormDisplay(true);
            }
        }

        private void InputML_TextChanged(object sender, EventArgs e)
        {
            string t = ((TextBox)sender).Text;
            if (t.Length == 0) t = " ";
            stroke.ExtendedProperties.Remove(Root.TEXT_GUID);
            stroke.ExtendedProperties.Add(Root.TEXT_GUID, t);
            if(!stroke.ExtendedProperties.Contains(Root.ISTAG_GUID))
                Root.FormCollection.ComputeTextBoxSize(ref stroke);
            Root.FormDisplay.ClearCanvus();
            Root.FormDisplay.DrawStrokes();
            Root.FormDisplay.UpdateFormDisplay(true);
        }

        private void TB_CtrlAPressed(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(1))
            {
                (sender as TextBox).SelectAll();
                e.Handled = true;
            }

        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            if(stroke != null )
            {
                InputML.Text = Saved_Txt;
                stroke.ExtendedProperties.Add(Root.TEXTFONT_GUID, Saved_Font.Name);
                stroke.ExtendedProperties.Add(Root.TEXTFONTSIZE_GUID, (double)Saved_Font.Size);
                stroke.ExtendedProperties.Add(Root.TEXTFONTSTYLE_GUID, Saved_Font.Style);
                Root.FormCollection.ComputeTextBoxSize(ref stroke);
                stroke.DrawingAttributes = Saved_Da;
                if (Saved_Frame)
                    stroke.ExtendedProperties.Add(Root.ISSTROKE_GUID, true);
                else
                    try { stroke.ExtendedProperties.Remove(Root.ISSTROKE_GUID); } catch { }

                if (Saved_Black)
                {
                    try { stroke.ExtendedProperties.Remove(Root.ISFILLEDWHITE_GUID); } catch { }
                    stroke.ExtendedProperties.Add(Root.ISFILLEDBLACK_GUID, true);
                }
                else if (Saved_White)
                {
                    try { stroke.ExtendedProperties.Remove(Root.ISFILLEDBLACK_GUID); } catch { }
                stroke.ExtendedProperties.Add(Root.ISFILLEDWHITE_GUID, true);
                }
                else
                {
                    try { stroke.ExtendedProperties.Remove(Root.ISFILLEDBLACK_GUID); } catch { }
                    try { stroke.ExtendedProperties.Remove(Root.ISFILLEDWHITE_GUID); } catch { }
                }
            }
        }

        private void boxingCb_TextChanged(object sender, EventArgs e)
        {
            int i = boxingCb.Items.IndexOf(boxingCb.Text);
            if ((i&1)!=0)
                stroke.ExtendedProperties.Add(Root.ISSTROKE_GUID, true);
            else
                try { stroke.ExtendedProperties.Remove(Root.ISSTROKE_GUID); } catch { }

            if ((i & 4) != 0)
            {
                try { stroke.ExtendedProperties.Remove(Root.ISFILLEDWHITE_GUID); } catch { }
                stroke.ExtendedProperties.Add(Root.ISFILLEDBLACK_GUID, true);
            }
            else if ((i & 2) != 0)
            {
                try { stroke.ExtendedProperties.Remove(Root.ISFILLEDBLACK_GUID); } catch { }
                stroke.ExtendedProperties.Add(Root.ISFILLEDWHITE_GUID, true);
            }
            else
            {
                try { stroke.ExtendedProperties.Remove(Root.ISFILLEDBLACK_GUID); } catch { }
                try { stroke.ExtendedProperties.Remove(Root.ISFILLEDWHITE_GUID); } catch { }
            }
            Root.FormDisplay.ClearCanvus();
            Root.FormDisplay.DrawStrokes();
            Root.FormDisplay.UpdateFormDisplay(true);
        }

        private void FormInput_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Control && (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return ))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();                
            }
        }

        private void VisibleCb_CheckedChanged(object sender, EventArgs e)
        {
            this.Opacity=VisibleCb.Checked?Root.TextDlgHiddenOpacity:1;
            if (this.InputML.Visible)
                this.InputML.Select();
            else
                this.InputML.Select();
        }
    }
}
