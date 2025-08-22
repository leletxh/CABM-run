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
            AntdUI.StepsItem stepsItem6 = new AntdUI.StepsItem();
            AntdUI.StepsItem stepsItem7 = new AntdUI.StepsItem();
            AntdUI.StepsItem stepsItem8 = new AntdUI.StepsItem();
            AntdUI.StepsItem stepsItem9 = new AntdUI.StepsItem();
            AntdUI.StepsItem stepsItem10 = new AntdUI.StepsItem();
            this.progress = new AntdUI.Progress();
            this.step = new AntdUI.Steps();
            this.log = new ReaLTaiizor.Controls.RichTextBoxEdit();
            this.nowlog = new AntdUI.Label();
            this.label1 = new System.Windows.Forms.Label();
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
            stepsItem6.Title = "准备安装";
            stepsItem7.Title = "下载包体";
            stepsItem8.Title = "校验哈希";
            stepsItem9.Title = "解压包体";
            stepsItem10.Title = "完成安装";
            this.step.Items.Add(stepsItem6);
            this.step.Items.Add(stepsItem7);
            this.step.Items.Add(stepsItem8);
            this.step.Items.Add(stepsItem9);
            this.step.Items.Add(stepsItem10);
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 441);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(245, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "这是最新版，也可能是最后一版";
            // 
            // dl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(900, 469);
            this.ControlBox = false;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.nowlog);
            this.Controls.Add(this.log);
            this.Controls.Add(this.step);
            this.Controls.Add(this.progress);
            this.Font = new System.Drawing.Font("黑体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "dl";
            this.Text = "安装";
            this.Load += new System.EventHandler(this.dl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AntdUI.Progress progress;
        private AntdUI.Steps step;
        private ReaLTaiizor.Controls.RichTextBoxEdit log;
        private AntdUI.Label nowlog;
        private System.Windows.Forms.Label label1;
    }
}

