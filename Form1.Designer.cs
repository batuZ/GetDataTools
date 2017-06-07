namespace GetDataTools
{
    partial class MasterWin
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.labInFileInfo = new System.Windows.Forms.Label();
            this.butOpen = new System.Windows.Forms.Button();
            this.butRunOutLine = new System.Windows.Forms.Button();
            this.butSaveSHP = new System.Windows.Forms.Button();
            this.openFileDSM = new System.Windows.Forms.OpenFileDialog();
            this.saveFileSHP = new System.Windows.Forms.SaveFileDialog();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.labeSaveAdd = new System.Windows.Forms.Label();
            this.butSaveAdd = new System.Windows.Forms.Button();
            this.checkAdd = new System.Windows.Forms.CheckBox();
            this.labelPOINT = new System.Windows.Forms.Label();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.labSHPout = new System.Windows.Forms.Label();
            this.labDSMin = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.labDEMsave = new System.Windows.Forms.Label();
            this.labDEMinSHP = new System.Windows.Forms.Label();
            this.labDEMin = new System.Windows.Forms.Label();
            this.butDEMsave = new System.Windows.Forms.Button();
            this.butDEMshp = new System.Windows.Forms.Button();
            this.butDEMin = new System.Windows.Forms.Button();
            this.openFilePOINT = new System.Windows.Forms.OpenFileDialog();
            this.SaveAdd = new System.Windows.Forms.SaveFileDialog();
            this.openDSMforDEM = new System.Windows.Forms.OpenFileDialog();
            this.openSHPforDEM = new System.Windows.Forms.OpenFileDialog();
            this.saveDEM = new System.Windows.Forms.SaveFileDialog();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.butMoveLayer = new System.Windows.Forms.Button();
            this.butGetDoubleFeat = new System.Windows.Forms.Button();
            this.butGetSlope = new System.Windows.Forms.Button();
            this.butCleanPoly = new System.Windows.Forms.Button();
            this.butBuffer = new System.Windows.Forms.Button();
            this.butGetHihgt = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // labInFileInfo
            // 
            this.labInFileInfo.AutoSize = true;
            this.labInFileInfo.Location = new System.Drawing.Point(413, 27);
            this.labInFileInfo.Name = "labInFileInfo";
            this.labInFileInfo.Size = new System.Drawing.Size(0, 12);
            this.labInFileInfo.TabIndex = 3;
            this.labInFileInfo.DoubleClick += new System.EventHandler(this.labInFileInfo_DoubleClick);
            // 
            // butOpen
            // 
            this.butOpen.Location = new System.Drawing.Point(12, 34);
            this.butOpen.Name = "butOpen";
            this.butOpen.Size = new System.Drawing.Size(88, 25);
            this.butOpen.TabIndex = 0;
            this.butOpen.Text = "加载DSM";
            this.butOpen.UseVisualStyleBackColor = true;
            this.butOpen.Click += new System.EventHandler(this.butOpen_Click);
            // 
            // butRunOutLine
            // 
            this.butRunOutLine.Location = new System.Drawing.Point(301, 154);
            this.butRunOutLine.Name = "butRunOutLine";
            this.butRunOutLine.Size = new System.Drawing.Size(75, 53);
            this.butRunOutLine.TabIndex = 7;
            this.butRunOutLine.Text = "GO!";
            this.butRunOutLine.UseVisualStyleBackColor = true;
            this.butRunOutLine.Click += new System.EventHandler(this.butRunOutLine_Click);
            // 
            // butSaveSHP
            // 
            this.butSaveSHP.Location = new System.Drawing.Point(12, 68);
            this.butSaveSHP.Name = "butSaveSHP";
            this.butSaveSHP.Size = new System.Drawing.Size(88, 25);
            this.butSaveSHP.TabIndex = 0;
            this.butSaveSHP.Text = "输出SHP";
            this.butSaveSHP.UseVisualStyleBackColor = true;
            this.butSaveSHP.Click += new System.EventHandler(this.butSaveSHP_Click);
            // 
            // openFileDSM
            // 
            this.openFileDSM.Filter = "DSM|*.img";
            this.openFileDSM.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDSM_FileOk);
            // 
            // saveFileSHP
            // 
            this.saveFileSHP.Filter = "shp file | *.shp";
            this.saveFileSHP.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileSHP_FileOk);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.comboBox1);
            this.groupBox1.Controls.Add(this.labeSaveAdd);
            this.groupBox1.Controls.Add(this.butSaveAdd);
            this.groupBox1.Controls.Add(this.checkAdd);
            this.groupBox1.Controls.Add(this.labelPOINT);
            this.groupBox1.Controls.Add(this.buttonAdd);
            this.groupBox1.Controls.Add(this.labSHPout);
            this.groupBox1.Controls.Add(this.labDSMin);
            this.groupBox1.Controls.Add(this.butRunOutLine);
            this.groupBox1.Controls.Add(this.butOpen);
            this.groupBox1.Controls.Add(this.butSaveSHP);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(386, 229);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "提取建筑轮廓";
            // 
            // comboBox1
            // 
            this.comboBox1.AutoCompleteCustomSource.AddRange(new string[] {
            "3- 85,80,75\t通用\t",
            "4- 85,80,75,70 \t慢",
            "5- 85,80,75,70,65\t极慢"});
            this.comboBox1.DisplayMember = "1";
            this.comboBox1.FormatString = "N0";
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "1- 预览级",
            "2- 快",
            "3- 通用",
            "4- 慢",
            "5- 极慢"});
            this.comboBox1.Location = new System.Drawing.Point(12, 104);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(268, 20);
            this.comboBox1.TabIndex = 12;
            this.comboBox1.TabStop = false;
            this.comboBox1.Tag = "";
            this.comboBox1.Text = "选择处理深度，默认为3";
            this.comboBox1.ValueMember = "1";
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // labeSaveAdd
            // 
            this.labeSaveAdd.AutoSize = true;
            this.labeSaveAdd.Location = new System.Drawing.Point(112, 174);
            this.labeSaveAdd.Name = "labeSaveAdd";
            this.labeSaveAdd.Size = new System.Drawing.Size(0, 12);
            this.labeSaveAdd.TabIndex = 12;
            // 
            // butSaveAdd
            // 
            this.butSaveAdd.Enabled = false;
            this.butSaveAdd.Location = new System.Drawing.Point(12, 168);
            this.butSaveAdd.Name = "butSaveAdd";
            this.butSaveAdd.Size = new System.Drawing.Size(88, 25);
            this.butSaveAdd.TabIndex = 15;
            this.butSaveAdd.Text = "补充结果";
            this.butSaveAdd.UseVisualStyleBackColor = true;
            this.butSaveAdd.Click += new System.EventHandler(this.butSaveAdd_Click);
            // 
            // checkAdd
            // 
            this.checkAdd.AutoSize = true;
            this.checkAdd.Location = new System.Drawing.Point(12, 201);
            this.checkAdd.Name = "checkAdd";
            this.checkAdd.Size = new System.Drawing.Size(108, 16);
            this.checkAdd.TabIndex = 14;
            this.checkAdd.Text = "伟景行定制工具";
            this.checkAdd.UseVisualStyleBackColor = true;
            this.checkAdd.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // labelPOINT
            // 
            this.labelPOINT.AutoSize = true;
            this.labelPOINT.Location = new System.Drawing.Point(112, 140);
            this.labelPOINT.Name = "labelPOINT";
            this.labelPOINT.Size = new System.Drawing.Size(0, 12);
            this.labelPOINT.TabIndex = 13;
            // 
            // buttonAdd
            // 
            this.buttonAdd.Enabled = false;
            this.buttonAdd.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonAdd.Location = new System.Drawing.Point(12, 134);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(88, 25);
            this.buttonAdd.TabIndex = 12;
            this.buttonAdd.Text = "打开点文件";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.button1_Click);
            // 
            // labSHPout
            // 
            this.labSHPout.AutoSize = true;
            this.labSHPout.Location = new System.Drawing.Point(112, 74);
            this.labSHPout.Name = "labSHPout";
            this.labSHPout.Size = new System.Drawing.Size(0, 12);
            this.labSHPout.TabIndex = 11;
            // 
            // labDSMin
            // 
            this.labDSMin.AutoSize = true;
            this.labDSMin.Location = new System.Drawing.Point(112, 40);
            this.labDSMin.Name = "labDSMin";
            this.labDSMin.Size = new System.Drawing.Size(0, 12);
            this.labDSMin.TabIndex = 10;
            this.labDSMin.TextChanged += new System.EventHandler(this.labDSMin_TextChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.labDEMsave);
            this.groupBox2.Controls.Add(this.labDEMinSHP);
            this.groupBox2.Controls.Add(this.labDEMin);
            this.groupBox2.Controls.Add(this.butDEMsave);
            this.groupBox2.Controls.Add(this.butDEMshp);
            this.groupBox2.Controls.Add(this.butDEMin);
            this.groupBox2.Location = new System.Drawing.Point(12, 247);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(386, 122);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "提取DEM";
            // 
            // labDEMsave
            // 
            this.labDEMsave.AutoSize = true;
            this.labDEMsave.Location = new System.Drawing.Point(99, 89);
            this.labDEMsave.Name = "labDEMsave";
            this.labDEMsave.Size = new System.Drawing.Size(0, 12);
            this.labDEMsave.TabIndex = 14;
            // 
            // labDEMinSHP
            // 
            this.labDEMinSHP.AutoSize = true;
            this.labDEMinSHP.Location = new System.Drawing.Point(99, 57);
            this.labDEMinSHP.Name = "labDEMinSHP";
            this.labDEMinSHP.Size = new System.Drawing.Size(0, 12);
            this.labDEMinSHP.TabIndex = 13;
            // 
            // labDEMin
            // 
            this.labDEMin.AutoSize = true;
            this.labDEMin.Location = new System.Drawing.Point(99, 25);
            this.labDEMin.Name = "labDEMin";
            this.labDEMin.Size = new System.Drawing.Size(0, 12);
            this.labDEMin.TabIndex = 13;
            // 
            // butDEMsave
            // 
            this.butDEMsave.Location = new System.Drawing.Point(18, 84);
            this.butDEMsave.Name = "butDEMsave";
            this.butDEMsave.Size = new System.Drawing.Size(75, 23);
            this.butDEMsave.TabIndex = 12;
            this.butDEMsave.Text = "输出DEM";
            this.butDEMsave.UseVisualStyleBackColor = true;
            this.butDEMsave.Click += new System.EventHandler(this.button7_Click);
            // 
            // butDEMshp
            // 
            this.butDEMshp.Location = new System.Drawing.Point(18, 52);
            this.butDEMshp.Name = "butDEMshp";
            this.butDEMshp.Size = new System.Drawing.Size(75, 23);
            this.butDEMshp.TabIndex = 12;
            this.butDEMshp.Text = "加载参考";
            this.butDEMshp.UseVisualStyleBackColor = true;
            this.butDEMshp.Click += new System.EventHandler(this.butDEMshp_Click);
            // 
            // butDEMin
            // 
            this.butDEMin.Location = new System.Drawing.Point(18, 20);
            this.butDEMin.Name = "butDEMin";
            this.butDEMin.Size = new System.Drawing.Size(75, 23);
            this.butDEMin.TabIndex = 10;
            this.butDEMin.Text = "加载DSM";
            this.butDEMin.UseVisualStyleBackColor = true;
            this.butDEMin.Click += new System.EventHandler(this.button5_Click);
            // 
            // openFilePOINT
            // 
            this.openFilePOINT.Filter = "SHP-point|*.shp";
            this.openFilePOINT.FileOk += new System.ComponentModel.CancelEventHandler(this.openFilePOINT_FileOk);
            // 
            // SaveAdd
            // 
            this.SaveAdd.Filter = "shp|*.shp";
            this.SaveAdd.FileOk += new System.ComponentModel.CancelEventHandler(this.SaveAdd_FileOk);
            // 
            // openDSMforDEM
            // 
            this.openDSMforDEM.Filter = "DSM|*.img";
            this.openDSMforDEM.FileOk += new System.ComponentModel.CancelEventHandler(this.openDSMforDEM_FileOk);
            // 
            // openSHPforDEM
            // 
            this.openSHPforDEM.Filter = "SHP|*.shp";
            this.openSHPforDEM.FileOk += new System.ComponentModel.CancelEventHandler(this.openSHPforDEM_FileOk);
            // 
            // saveDEM
            // 
            this.saveDEM.Filter = "DEM|*.img";
            this.saveDEM.FileOk += new System.ComponentModel.CancelEventHandler(this.saveDEM_FileOk);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.butMoveLayer);
            this.groupBox3.Controls.Add(this.butGetDoubleFeat);
            this.groupBox3.Controls.Add(this.butGetSlope);
            this.groupBox3.Controls.Add(this.butCleanPoly);
            this.groupBox3.Controls.Add(this.butBuffer);
            this.groupBox3.Controls.Add(this.butGetHihgt);
            this.groupBox3.Location = new System.Drawing.Point(408, 132);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(249, 236);
            this.groupBox3.TabIndex = 15;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "实用工具";
            // 
            // butMoveLayer
            // 
            this.butMoveLayer.Location = new System.Drawing.Point(17, 160);
            this.butMoveLayer.Name = "butMoveLayer";
            this.butMoveLayer.Size = new System.Drawing.Size(124, 25);
            this.butMoveLayer.TabIndex = 0;
            this.butMoveLayer.Text = "最小外接矩形";
            this.butMoveLayer.UseVisualStyleBackColor = true;
            this.butMoveLayer.Click += new System.EventHandler(this.butMoveLayer_Click);
            // 
            // butGetDoubleFeat
            // 
            this.butGetDoubleFeat.Location = new System.Drawing.Point(17, 195);
            this.butGetDoubleFeat.Name = "butGetDoubleFeat";
            this.butGetDoubleFeat.Size = new System.Drawing.Size(124, 25);
            this.butGetDoubleFeat.TabIndex = 0;
            this.butGetDoubleFeat.Text = "GetDoubleFeat";
            this.butGetDoubleFeat.UseVisualStyleBackColor = true;
            this.butGetDoubleFeat.Click += new System.EventHandler(this.butGetDoubleFeat_Click);
            // 
            // butGetSlope
            // 
            this.butGetSlope.Location = new System.Drawing.Point(17, 125);
            this.butGetSlope.Name = "butGetSlope";
            this.butGetSlope.Size = new System.Drawing.Size(124, 25);
            this.butGetSlope.TabIndex = 0;
            this.butGetSlope.Text = "GetSlopePolygon";
            this.butGetSlope.UseVisualStyleBackColor = true;
            this.butGetSlope.Click += new System.EventHandler(this.butGetSlope_Click);
            // 
            // butCleanPoly
            // 
            this.butCleanPoly.Location = new System.Drawing.Point(17, 90);
            this.butCleanPoly.Name = "butCleanPoly";
            this.butCleanPoly.Size = new System.Drawing.Size(124, 25);
            this.butCleanPoly.TabIndex = 0;
            this.butCleanPoly.Text = "CleanPolygon-ok";
            this.butCleanPoly.UseVisualStyleBackColor = true;
            this.butCleanPoly.Click += new System.EventHandler(this.butCleanPoly_Click);
            // 
            // butBuffer
            // 
            this.butBuffer.Location = new System.Drawing.Point(17, 55);
            this.butBuffer.Name = "butBuffer";
            this.butBuffer.Size = new System.Drawing.Size(124, 25);
            this.butBuffer.TabIndex = 0;
            this.butBuffer.Text = "ChangeRaVal-ok";
            this.butBuffer.UseVisualStyleBackColor = true;
            this.butBuffer.Click += new System.EventHandler(this.butBuffer_Click);
            // 
            // butGetHihgt
            // 
            this.butGetHihgt.Location = new System.Drawing.Point(17, 20);
            this.butGetHihgt.Name = "butGetHihgt";
            this.butGetHihgt.Size = new System.Drawing.Size(124, 25);
            this.butGetHihgt.TabIndex = 0;
            this.butGetHihgt.Text = "GetHihgt-ok";
            this.butGetHihgt.UseVisualStyleBackColor = true;
            this.butGetHihgt.Click += new System.EventHandler(this.butGetHihgt_Click);
            // 
            // MasterWin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(405, 376);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.labInFileInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "MasterWin";
            this.Text = "GetDataTools";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button butOpen;
        private System.Windows.Forms.Label labInFileInfo;
        private System.Windows.Forms.Button butRunOutLine;
        private System.Windows.Forms.Button butSaveSHP;
        private System.Windows.Forms.OpenFileDialog openFileDSM;
        private System.Windows.Forms.SaveFileDialog saveFileSHP;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button butDEMsave;
        private System.Windows.Forms.Button butDEMshp;
        private System.Windows.Forms.Button butDEMin;
        private System.Windows.Forms.Label labSHPout;
        private System.Windows.Forms.Label labDSMin;
        private System.Windows.Forms.OpenFileDialog openFilePOINT;
        private System.Windows.Forms.Label labelPOINT;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Label labeSaveAdd;
        private System.Windows.Forms.Button butSaveAdd;
        private System.Windows.Forms.SaveFileDialog SaveAdd;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label labDEMinSHP;
        private System.Windows.Forms.Label labDEMin;
        private System.Windows.Forms.OpenFileDialog openDSMforDEM;
        private System.Windows.Forms.OpenFileDialog openSHPforDEM;
        private System.Windows.Forms.Label labDEMsave;
        private System.Windows.Forms.SaveFileDialog saveDEM;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button butMoveLayer;
        private System.Windows.Forms.Button butGetDoubleFeat;
        private System.Windows.Forms.Button butGetSlope;
        private System.Windows.Forms.Button butCleanPoly;
        private System.Windows.Forms.Button butBuffer;
        private System.Windows.Forms.Button butGetHihgt;
        private System.Windows.Forms.CheckBox checkAdd;
    }
}

