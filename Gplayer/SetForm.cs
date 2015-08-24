using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Gplayer
{
    public partial class SetForm : Form
    {
        public SetForm()
        {
            InitializeComponent();
        }

        private void setForm_Load(object sender, EventArgs e)
        {
            string[] initInfo=File.ReadAllLines(@"init.ini");
            textBox1.Text = initInfo[0];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd=new FolderBrowserDialog ();
            fbd.Description = "选择一个文件夹";
            fbd.SelectedPath = textBox1.Text;
            fbd.ShowDialog();
            textBox1.Text=fbd.SelectedPath ;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string[] str = { textBox1.Text };
            File.WriteAllLines(@"init.ini", str);
            
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
