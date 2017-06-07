namespace GetDataTools.SubWindows
{
    partial class GetHightOpenFileDialog1
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.GetHightlabel1 = new System.Windows.Forms.Label();
            this.GetHightlabel2 = new System.Windows.Forms.Label();
            this.GetHightOpen = new System.Windows.Forms.OpenFileDialog();
            this.GetHopenSHPFile = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 13);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "打开DSM";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 54);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 0;
            this.button2.Text = "打开SHP";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // GetHightlabel1
            // 
            this.GetHightlabel1.AutoSize = true;
            this.GetHightlabel1.Location = new System.Drawing.Point(93, 18);
            this.GetHightlabel1.Name = "GetHightlabel1";
            this.GetHightlabel1.Size = new System.Drawing.Size(0, 12);
            this.GetHightlabel1.TabIndex = 1;
            // 
            // GetHightlabel2
            // 
            this.GetHightlabel2.AutoSize = true;
            this.GetHightlabel2.Location = new System.Drawing.Point(93, 59);
            this.GetHightlabel2.Name = "GetHightlabel2";
            this.GetHightlabel2.Size = new System.Drawing.Size(0, 12);
            this.GetHightlabel2.TabIndex = 1;
            // 
            // GetHightOpen
            // 
            this.GetHightOpen.FileName = "openFileDialog1";
            this.GetHightOpen.Filter = "IMG|*.img";
            this.GetHightOpen.FileOk += new System.ComponentModel.CancelEventHandler(this.GetHightOpen_FileOk);
            // 
            // GetHopenSHPFile
            // 
            this.GetHopenSHPFile.FileName = "openFileDialog1";
            this.GetHopenSHPFile.Filter = "SHP|*.shp";
            this.GetHopenSHPFile.FileOk += new System.ComponentModel.CancelEventHandler(this.GetHopenSHPFile_FileOk);
            // 
            // GetHightOpenFileDialog1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(335, 93);
            this.Controls.Add(this.GetHightlabel2);
            this.Controls.Add(this.GetHightlabel1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "GetHightOpenFileDialog1";
            this.Text = "GetHight";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label GetHightlabel1;
        private System.Windows.Forms.Label GetHightlabel2;
        private System.Windows.Forms.OpenFileDialog GetHightOpen;
        private System.Windows.Forms.OpenFileDialog GetHopenSHPFile;
    }
}