using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace cocoassistant
{
    public partial class Form1 : Form
    {
        private Point mousePoint;
        
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {
            //ドラッグで移動するためのハンドラをそれぞれの画像に登録する。
            pictureBox1.MouseDown += new MouseEventHandler(Form1_MouseDown);
            pictureBox1.MouseMove += new MouseEventHandler(Form1_MouseMove);
            pictureBox2.MouseDown += new MouseEventHandler(Form1_MouseDown);
            pictureBox2.MouseMove += new MouseEventHandler(Form1_MouseMove);

            //フレーム削除、背景透明化                
            this.FormBorderStyle = FormBorderStyle.None;
            this.TransparencyKey = Color.DarkGray;
            


            //常に最前面に表示
            this.TopMost = true;

           //位置復元
            this.Left = Properties.Settings.Default.Left;
            this.Top = Properties.Settings.Default.Top;

            richTextBox1.Text = "ご注文はなぁに？";
            this.pictureBox1.Focus();

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Console.WriteLine(this.Left);
            Properties.Settings.Default.Left = this.Left;
            Properties.Settings.Default.Top = this.Top;
            Properties.Settings.Default.Save();
        }

        //Form1のMouseDownイベントハンドラ
        private void Form1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left){
                //位置を記憶する
                mousePoint = new Point(e.X, e.Y);
            }
        }

        //Form1のMouseMoveイベントハンドラ
        private void Form1_MouseMove(object sender,System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left){
                this.Left += e.X - mousePoint.X;
                this.Top += e.Y - mousePoint.Y;
            }

        }

        private void richTextBox1_Enter(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            richTextBox1.ForeColor = Color.Black;
        }

        private void richTextBox1_Leave(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (richTextBox1.Text == "ばいばい") close();

            Match m = Regex.Match(richTextBox1.Text, @"^(?<app>.*)を起動");
            if (m.Groups["app"].Value.Equals("chrome", StringComparison.OrdinalIgnoreCase) || richTextBox1.Text.Equals("chrome", StringComparison.OrdinalIgnoreCase)){
                richTextBox1.Text = "まかせて!";
                System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe");
            }
        }

        private void ばいばいToolStripMenuItem_Click(object sender, EventArgs e)
        {
            close();
        }

        private void close()
        {
            richTextBox1.Text = "またね!";
            richTextBox1.Update();
            System.Threading.Thread.Sleep(500);
            this.Close();
        }

    }
}
