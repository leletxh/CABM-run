using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 启动器
{
    public partial class browser : Form
    {
        public browser()
        {
            InitializeComponent();
        }

        private void browser_Load(object sender, EventArgs e)
        {
            // 窗口加载时显示在最前面
            this.Activate();
            this.BringToFront();
        }

        // 重写OnFormClosed方法，在窗口关闭时退出整个应用程序
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Application.Exit();
        }
    }
}