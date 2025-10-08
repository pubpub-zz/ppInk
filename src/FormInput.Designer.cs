namespace gInk
{
    partial class FormInput
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
            this.btOK = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.InputML = new System.Windows.Forms.TextBox();
            this.InputSL = new System.Windows.Forms.TextBox();
            this.captionLbl = new System.Windows.Forms.Label();
            this.FontBtn = new System.Windows.Forms.Button();
            this.FontDlg = new System.Windows.Forms.FontDialog();
            this.ColorBtn = new System.Windows.Forms.Button();
            this.boxingCb = new System.Windows.Forms.ComboBox();
            this.VisibleCb = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btOK
            // 
            this.btOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btOK.Location = new System.Drawing.Point(395, 125);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(75, 42);
            this.btOK.TabIndex = 0;
            this.btOK.Text = "&OK";
            this.btOK.UseVisualStyleBackColor = true;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btCancel.Location = new System.Drawing.Point(476, 125);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 42);
            this.btCancel.TabIndex = 1;
            this.btCancel.Text = "&Cancel";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // InputML
            // 
            this.InputML.AcceptsReturn = true;
            this.InputML.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InputML.Location = new System.Drawing.Point(18, 26);
            this.InputML.Multiline = true;
            this.InputML.Name = "InputML";
            this.InputML.Size = new System.Drawing.Size(533, 93);
            this.InputML.TabIndex = 2;
            this.InputML.Text = "inputML\r\nline2";
            this.InputML.Visible = false;
            this.InputML.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TB_CtrlAPressed);
            this.InputML.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.FormInput_PreviewKeyDown);
            // 
            // InputSL
            // 
            this.InputSL.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InputSL.Location = new System.Drawing.Point(18, 62);
            this.InputSL.Name = "InputSL";
            this.InputSL.Size = new System.Drawing.Size(533, 26);
            this.InputSL.TabIndex = 3;
            this.InputSL.Text = "inputSL";
            this.InputSL.Visible = false;
            this.InputSL.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TB_CtrlAPressed);
            // 
            // captionLbl
            // 
            this.captionLbl.AutoSize = true;
            this.captionLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.captionLbl.Location = new System.Drawing.Point(15, 10);
            this.captionLbl.Name = "captionLbl";
            this.captionLbl.Size = new System.Drawing.Size(45, 16);
            this.captionLbl.TabIndex = 4;
            this.captionLbl.Text = "label1";
            // 
            // FontBtn
            // 
            this.FontBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FontBtn.Location = new System.Drawing.Point(18, 125);
            this.FontBtn.Name = "FontBtn";
            this.FontBtn.Size = new System.Drawing.Size(75, 42);
            this.FontBtn.TabIndex = 5;
            this.FontBtn.Text = "Font";
            this.FontBtn.UseVisualStyleBackColor = true;
            this.FontBtn.Visible = false;
            this.FontBtn.Click += new System.EventHandler(this.FontBtn_Click);
            // 
            // FontDlg
            // 
            this.FontDlg.FontMustExist = true;
            // 
            // ColorBtn
            // 
            this.ColorBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ColorBtn.Location = new System.Drawing.Point(99, 125);
            this.ColorBtn.Name = "ColorBtn";
            this.ColorBtn.Size = new System.Drawing.Size(75, 42);
            this.ColorBtn.TabIndex = 6;
            this.ColorBtn.Text = "Color";
            this.ColorBtn.UseVisualStyleBackColor = true;
            this.ColorBtn.Visible = false;
            this.ColorBtn.Click += new System.EventHandler(this.ColorBtn_Click);
            // 
            // boxingCb
            // 
            this.boxingCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.boxingCb.FormattingEnabled = true;
            this.boxingCb.Location = new System.Drawing.Point(180, 125);
            this.boxingCb.Name = "boxingCb";
            this.boxingCb.Size = new System.Drawing.Size(209, 21);
            this.boxingCb.TabIndex = 7;
            this.boxingCb.Visible = false;
            this.boxingCb.TextChanged += new System.EventHandler(this.boxingCb_TextChanged);
            // 
            // VisibleCb
            // 
            this.VisibleCb.AutoSize = true;
            this.VisibleCb.Location = new System.Drawing.Point(476, 3);
            this.VisibleCb.Name = "VisibleCb";
            this.VisibleCb.Size = new System.Drawing.Size(64, 17);
            this.VisibleCb.TabIndex = 8;
            this.VisibleCb.Text = "In&visible";
            this.VisibleCb.UseVisualStyleBackColor = true;
            this.VisibleCb.CheckedChanged += new System.EventHandler(this.VisibleCb_CheckedChanged);
            // 
            // FormInput
            // 
            this.AcceptButton = this.btOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(563, 179);
            this.Controls.Add(this.VisibleCb);
            this.Controls.Add(this.boxingCb);
            this.Controls.Add(this.ColorBtn);
            this.Controls.Add(this.FontBtn);
            this.Controls.Add(this.captionLbl);
            this.Controls.Add(this.InputSL);
            this.Controls.Add(this.InputML);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormInput";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Input";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Button btCancel;
        public  System.Windows.Forms.TextBox InputML;
        public  System.Windows.Forms.TextBox InputSL;
        public  System.Windows.Forms.Label captionLbl;
        public System.Windows.Forms.Button FontBtn;
        public System.Windows.Forms.FontDialog FontDlg;
        public System.Windows.Forms.Button ColorBtn;
        private System.Windows.Forms.ComboBox boxingCb;
        private System.Windows.Forms.CheckBox VisibleCb;
    }
}
