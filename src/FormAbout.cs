using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace gInk
{
	public partial class FormAbout : Form
	{
		public FormAbout()
		{
			InitializeComponent();
		}

		private void FormAbout_Load(object sender, EventArgs e)
		{
			this.Icon = gInk.Properties.Resources.icon_red;
			string version = Application.ProductVersion.Substring(0, Application.ProductVersion.Length-2)+"    built on " +Build.Timestamp;
#if ppInkSmall
            string built="Simplified";
#else
            string built = "Full";
#endif


            string about = "ppInk "+built+ " v" + version + "\r\n";
            about += "Forked from gInk   (c) 2016-2020 Weizhi Nai\r\n";
            about += "changes (c) 2020-2024 Pubpub-ZZ\r\n";
            about += "Licensed under MIT\r\n";
			about += "https://github.com/pubpub-zz/ppInk\r\n";
			about += "\r\n";
			about += "Credits:\r\n";
			about += "Some button icons are designed by Freepik.com.\r\n";
			textBox1.Text = about;
			textBox1.Select(textBox1.Text.Length, 0);
		}
	}
}
