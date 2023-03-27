namespace gInk
{
    partial class ArrowSelDlg
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.NextBtn = new System.Windows.Forms.Button();
            this.PrevBtn = new System.Windows.Forms.Button();
            this.SaveBtn = new System.Windows.Forms.Button();
            this.QuitBtn = new System.Windows.Forms.Button();
            this.AddBtn = new System.Windows.Forms.Button();
            this.DelBtn = new System.Windows.Forms.Button();
            this.ArrowTail_Pnl = new System.Windows.Forms.Panel();
            this.ArrowHead_Pnl = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.HeadScaleEd = new System.Windows.Forms.TextBox();
            this.TailScaleEd = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Location = new System.Drawing.Point(49, 33);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(156, 5);
            this.panel1.TabIndex = 2;
            // 
            // NextBtn
            // 
            this.NextBtn.Location = new System.Drawing.Point(172, 99);
            this.NextBtn.Name = "NextBtn";
            this.NextBtn.Size = new System.Drawing.Size(75, 23);
            this.NextBtn.TabIndex = 4;
            this.NextBtn.Text = "&Next";
            this.NextBtn.UseVisualStyleBackColor = true;
            this.NextBtn.Click += new System.EventHandler(this.NextBtn_Click);
            // 
            // PrevBtn
            // 
            this.PrevBtn.Location = new System.Drawing.Point(88, 99);
            this.PrevBtn.Name = "PrevBtn";
            this.PrevBtn.Size = new System.Drawing.Size(75, 23);
            this.PrevBtn.TabIndex = 5;
            this.PrevBtn.Text = "&Previous";
            this.PrevBtn.UseVisualStyleBackColor = true;
            this.PrevBtn.Click += new System.EventHandler(this.PrevBtn_Click);
            // 
            // SaveBtn
            // 
            this.SaveBtn.Location = new System.Drawing.Point(253, 122);
            this.SaveBtn.Name = "SaveBtn";
            this.SaveBtn.Size = new System.Drawing.Size(75, 23);
            this.SaveBtn.TabIndex = 6;
            this.SaveBtn.Text = "&Save";
            this.SaveBtn.UseVisualStyleBackColor = true;
            this.SaveBtn.Click += new System.EventHandler(this.SaveBtn_Click);
            // 
            // QuitBtn
            // 
            this.QuitBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.QuitBtn.Location = new System.Drawing.Point(253, 151);
            this.QuitBtn.Name = "QuitBtn";
            this.QuitBtn.Size = new System.Drawing.Size(75, 23);
            this.QuitBtn.TabIndex = 7;
            this.QuitBtn.Text = "&Quit";
            this.QuitBtn.UseVisualStyleBackColor = true;
            this.QuitBtn.Click += new System.EventHandler(this.QuitBtn_Click);
            // 
            // AddBtn
            // 
            this.AddBtn.Location = new System.Drawing.Point(12, 151);
            this.AddBtn.Name = "AddBtn";
            this.AddBtn.Size = new System.Drawing.Size(75, 23);
            this.AddBtn.TabIndex = 8;
            this.AddBtn.Text = "&Add";
            this.AddBtn.UseVisualStyleBackColor = true;
            this.AddBtn.Click += new System.EventHandler(this.AddBtn_Click);
            // 
            // DelBtn
            // 
            this.DelBtn.Location = new System.Drawing.Point(12, 122);
            this.DelBtn.Name = "DelBtn";
            this.DelBtn.Size = new System.Drawing.Size(75, 23);
            this.DelBtn.TabIndex = 9;
            this.DelBtn.Text = "&Delete";
            this.DelBtn.UseVisualStyleBackColor = true;
            this.DelBtn.Click += new System.EventHandler(this.DelBtn_Click);
            // 
            // ArrowTail_Pnl
            // 
            this.ArrowTail_Pnl.BackColor = System.Drawing.Color.Transparent;
            this.ArrowTail_Pnl.BackgroundImage = global::gInk.Properties.Resources.Arw_Tail1;
            this.ArrowTail_Pnl.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ArrowTail_Pnl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ArrowTail_Pnl.ForeColor = System.Drawing.Color.Gray;
            this.ArrowTail_Pnl.Location = new System.Drawing.Point(11, 10);
            this.ArrowTail_Pnl.Name = "ArrowTail_Pnl";
            this.ArrowTail_Pnl.Size = new System.Drawing.Size(75, 50);
            this.ArrowTail_Pnl.TabIndex = 10;
            this.ArrowTail_Pnl.Click += new System.EventHandler(this.ArrowTail_Pnl_Click);
            // 
            // ArrowHead_Pnl
            // 
            this.ArrowHead_Pnl.BackColor = System.Drawing.Color.Transparent;
            this.ArrowHead_Pnl.BackgroundImage = global::gInk.Properties.Resources.Arw_Arrow1;
            this.ArrowHead_Pnl.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ArrowHead_Pnl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ArrowHead_Pnl.ForeColor = System.Drawing.Color.Silver;
            this.ArrowHead_Pnl.Location = new System.Drawing.Point(164, 10);
            this.ArrowHead_Pnl.Name = "ArrowHead_Pnl";
            this.ArrowHead_Pnl.Size = new System.Drawing.Size(75, 50);
            this.ArrowHead_Pnl.TabIndex = 11;
            this.ArrowHead_Pnl.Click += new System.EventHandler(this.ArrowHead_Pnl_Click);
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.Black;
            this.panel4.Controls.Add(this.ArrowHead_Pnl);
            this.panel4.Controls.Add(this.ArrowTail_Pnl);
            this.panel4.Controls.Add(this.panel1);
            this.panel4.Location = new System.Drawing.Point(42, 3);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(246, 67);
            this.panel4.TabIndex = 12;
            // 
            // HeadScaleEd
            // 
            this.HeadScaleEd.Location = new System.Drawing.Point(224, 73);
            this.HeadScaleEd.Name = "HeadScaleEd";
            this.HeadScaleEd.Size = new System.Drawing.Size(45, 20);
            this.HeadScaleEd.TabIndex = 13;
            this.HeadScaleEd.Text = "0.25";
            this.HeadScaleEd.TextChanged += new System.EventHandler(this.ScaleEd_TextChanged);
            this.HeadScaleEd.Leave += new System.EventHandler(this.HeadScaleEd_Leave);
            this.HeadScaleEd.Validating += new System.ComponentModel.CancelEventHandler(this.ScaleEd_Validating);
            this.HeadScaleEd.Validated += new System.EventHandler(this.HeadScaleEd_Validated);
            // 
            // TailScaleEd
            // 
            this.TailScaleEd.Location = new System.Drawing.Point(68, 73);
            this.TailScaleEd.Name = "TailScaleEd";
            this.TailScaleEd.Size = new System.Drawing.Size(45, 20);
            this.TailScaleEd.TabIndex = 14;
            this.TailScaleEd.Text = "0.25";
            this.TailScaleEd.TextChanged += new System.EventHandler(this.ScaleEd_TextChanged);
            this.TailScaleEd.Leave += new System.EventHandler(this.TailScaleEd_Leave);
            this.TailScaleEd.Validating += new System.ComponentModel.CancelEventHandler(this.ScaleEd_Validating);
            this.TailScaleEd.Validated += new System.EventHandler(this.TailScaleEd_Validated);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(212, 76);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(12, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "x";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(57, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(12, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "x";
            // 
            // ArrowSelDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.QuitBtn;
            this.ClientSize = new System.Drawing.Size(340, 185);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TailScaleEd);
            this.Controls.Add(this.HeadScaleEd);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.DelBtn);
            this.Controls.Add(this.AddBtn);
            this.Controls.Add(this.QuitBtn);
            this.Controls.Add(this.SaveBtn);
            this.Controls.Add(this.PrevBtn);
            this.Controls.Add(this.NextBtn);
            this.Name = "ArrowSelDlg";
            this.Text = "Arrow 1/3";
            this.TopMost = true;
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button NextBtn;
        private System.Windows.Forms.Button PrevBtn;
        private System.Windows.Forms.Button SaveBtn;
        private System.Windows.Forms.Button QuitBtn;
        private System.Windows.Forms.Button AddBtn;
        private System.Windows.Forms.Button DelBtn;
        private System.Windows.Forms.Panel ArrowTail_Pnl;
        private System.Windows.Forms.Panel ArrowHead_Pnl;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.TextBox HeadScaleEd;
        private System.Windows.Forms.TextBox TailScaleEd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}