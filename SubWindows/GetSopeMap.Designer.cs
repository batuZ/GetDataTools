namespace GetDataTools.SubWindows
{
    partial class GetSopeMap
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
            this.butOpen = new System.Windows.Forms.Button();
            this.butSave = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.butRun = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.saveFileDialog2 = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // butOpen
            // 
            this.butOpen.Location = new System.Drawing.Point(12, 12);
            this.butOpen.Name = "butOpen";
            this.butOpen.Size = new System.Drawing.Size(75, 23);
            this.butOpen.TabIndex = 0;
            this.butOpen.Text = "open";
            this.butOpen.UseVisualStyleBackColor = true;
            this.butOpen.Click += new System.EventHandler(this.butOpen_Click);
            // 
            // butSave
            // 
            this.butSave.Location = new System.Drawing.Point(12, 41);
            this.butSave.Name = "butSave";
            this.butSave.Size = new System.Drawing.Size(75, 23);
            this.butSave.TabIndex = 0;
            this.butSave.Text = "save";
            this.butSave.UseVisualStyleBackColor = true;
            this.butSave.Click += new System.EventHandler(this.butSave_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "IMG|*.img";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "IMG|*.img";
            // 
            // butRun
            // 
            this.butRun.Location = new System.Drawing.Point(93, 12);
            this.butRun.Name = "butRun";
            this.butRun.Size = new System.Drawing.Size(80, 81);
            this.butRun.TabIndex = 1;
            this.butRun.Text = "GO";
            this.butRun.UseVisualStyleBackColor = true;
            this.butRun.Click += new System.EventHandler(this.butRun_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 70);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "saveSHP";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // saveFileDialog2
            // 
            this.saveFileDialog2.Filter = "SHP|*.shp";
            // 
            // GetSopeMap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(188, 107);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.butRun);
            this.Controls.Add(this.butSave);
            this.Controls.Add(this.butOpen);
            this.Name = "GetSopeMap";
            this.Text = "GetSopeMap";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button butOpen;
        private System.Windows.Forms.Button butSave;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button butRun;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog2;
    }
}