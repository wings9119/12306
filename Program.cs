using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace _12306Helper
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            login l = new login();
            if (l.ShowDialog() == DialogResult.OK)
            {
                string name = l.name;
                Application.Run(new main(name));//如果登录成功则打开主窗体  
            }
            else
            {
                Application.Exit();
            }  
        }
    }
}
