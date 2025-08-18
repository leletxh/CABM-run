namespace 启动器
{
    partial class dl
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            AntdUI.StepsItem stepsItem11 = new AntdUI.StepsItem();
            AntdUI.StepsItem stepsItem12 = new AntdUI.StepsItem();
            AntdUI.StepsItem stepsItem13 = new AntdUI.StepsItem();
            AntdUI.StepsItem stepsItem14 = new AntdUI.StepsItem();
            AntdUI.StepsItem stepsItem15 = new AntdUI.StepsItem();
            this.progress = new AntdUI.Progress();
            this.step = new AntdUI.Steps();
            this.log = new ReaLTaiizor.Controls.RichTextBoxEdit();
            this.nowlog = new AntdUI.Label();
            this.SuspendLayout();
            // 
            // progress
            // 
            this.progress.Location = new System.Drawing.Point(14, 415);
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(873, 23);
            this.progress.TabIndex = 0;
            this.progress.Text = "progress1";
            // 
            // step
            // 
            this.step.Font = new System.Drawing.Font("黑体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            stepsItem11.Title = "准备安装";
            stepsItem12.Title = "下载包体";
            stepsItem13.Title = "校验哈希";
            stepsItem14.Title = "解压包体";
            stepsItem15.Title = "完成安装";
            this.step.Items.Add(stepsItem11);
            this.step.Items.Add(stepsItem12);
            this.step.Items.Add(stepsItem13);
            this.step.Items.Add(stepsItem14);
            this.step.Items.Add(stepsItem15);
            this.step.Location = new System.Drawing.Point(14, 370);
            this.step.Name = "step";
            this.step.Size = new System.Drawing.Size(873, 39);
            this.step.TabIndex = 1;
            this.step.Text = "step";
            this.step.ItemClick += new AntdUI.StepsItemEventHandler(this.steps1_ItemClick);
            // 
            // log
            // 
            this.log.AutoWordSelection = false;
            this.log.BackColor = System.Drawing.Color.Transparent;
            this.log.BaseColor = System.Drawing.Color.Transparent;
            this.log.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.log.EdgeColor = System.Drawing.Color.White;
            this.log.Font = new System.Drawing.Font("Tahoma", 10F);
            this.log.ForeColor = System.Drawing.Color.DimGray;
            this.log.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.log.Location = new System.Drawing.Point(12, 12);
            this.log.Name = "log";
            this.log.ReadOnly = true;
            this.log.Size = new System.Drawing.Size(875, 313);
            this.log.SmoothingType = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            this.log.TabIndex = 2;
            this.log.Text = "程序已启动";
            this.log.TextBackColor = System.Drawing.Color.White;
            this.log.TextBorderStyle = System.Windows.Forms.BorderStyle.None;
            this.log.TextFont = new System.Drawing.Font("Tahoma", 10F);
            this.log.WordWrap = true;
            // 
            // nowlog
            // 
            this.nowlog.Location = new System.Drawing.Point(12, 331);
            this.nowlog.Name = "nowlog";
            this.nowlog.Size = new System.Drawing.Size(875, 33);
            this.nowlog.TabIndex = 3;
            this.nowlog.Text = "最新日志";
            // 
            // dl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(900, 450);
            this.ControlBox = false;
            this.Controls.Add(this.nowlog);
            this.Controls.Add(this.log);
            this.Controls.Add(this.step);
            this.Controls.Add(this.progress);
            this.Font = new System.Drawing.Font("黑体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "dl";
            this.Text = "安装";
            this.Load += new System.EventHandler(this.dl_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private AntdUI.Progress progress;
        private AntdUI.Steps step;
        private ReaLTaiizor.Controls.RichTextBoxEdit log;
        private AntdUI.Label nowlog;
    }
}

