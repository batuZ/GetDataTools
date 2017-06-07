namespace GetDataTools.SubWindows
{
    partial class ChangeValue
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
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.button3 = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.labOpen = new System.Windows.Forms.Label();
            this.labSave = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(39, 36);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(165, 45);
            this.button1.TabIndex = 0;
            this.button1.Text = "OpenRaster";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(39, 106);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(165, 45);
            this.button2.TabIndex = 0;
            this.button2.Text = "SaveRaster";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.Location = new System.Drawing.Point(55, 190);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(113, 28);
            this.radioButton1.TabIndex = 1;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Values";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(225, 190);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(161, 28);
            this.radioButton2.TabIndex = 2;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Resolution";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(656, 36);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(153, 115);
            this.button3.TabIndex = 3;
            this.button3.Text = "GO";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "IMG|*.img";
            this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "IMG|*.img";
            this.saveFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog1_FileOk);
            // 
            // labOpen
            // 
            this.labOpen.AutoSize = true;
            this.labOpen.Location = new System.Drawing.Point(235, 46);
            this.labOpen.Name = "labOpen";
            this.labOpen.Size = new System.Drawing.Size(0, 24);
            this.labOpen.TabIndex = 5;
            // 
            // labSave
            // 
            this.labSave.AutoSize = true;
            this.labSave.Location = new System.Drawing.Point(235, 127);
            this.labSave.Name = "labSave";
            this.labSave.Size = new System.Drawing.Size(0, 24);
            this.labSave.TabIndex = 6;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "1 -无改变",
            "2",
            "3",
            "4",
            "5 -Resolution 不建议",
            "6 -Resolution 不建议",
            "7 -Resolution 不建议",
            "8 -Resolution 不建议",
            "9 -Resolution 不建议",
            "10 -Resolution 不建议"});
            this.comboBox1.Location = new System.Drawing.Point(453, 190);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(356, 32);
            this.comboBox1.TabIndex = 7;
            // 
            // ChangeValue
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(846, 248);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.labSave);
            this.Controls.Add(this.labOpen);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "ChangeValue";
            this.Text = "ChangeValue";
            this.Load += new System.EventHandler(this.ChangeValue_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label labOpen;
        private System.Windows.Forms.Label labSave;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}