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
using System.IO;

namespace cocoassistant{
    public partial class Form1 : Form{
        private Point mousePoint;
       
        public Form1(){
            InitializeComponent();
        }

        private void Form1_Load(object sender, System.EventArgs e){

            pictureBox1.Image = new Bitmap(Directory.GetCurrentDirectory() + "\\" + "img\\chara.png");
            pictureBox2.Image = new Bitmap(Directory.GetCurrentDirectory() + "\\" + "img\\hukidashi.png");

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

        //Form1のMouseDownイベントハンドラ
        private void Form1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e){
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left){
                //位置を記憶する
                mousePoint = new Point(e.X, e.Y);
            }
        }

        //Form1のMouseMoveイベントハンドラ
        private void Form1_MouseMove(object sender,System.Windows.Forms.MouseEventArgs e){
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left){
                this.Left += e.X - mousePoint.X;
                this.Top += e.Y - mousePoint.Y;
            }
        }

        private void richTextBox1_Enter(object sender, EventArgs e){
            richTextBox1.Text = "";
            richTextBox1.ForeColor = Color.Black;
        }

        private void button1_Click(object sender, EventArgs e){

            richTextBox1.Text = richTextBox1.Text.Trim();

            if (richTextBox1.Text == "ばいばい") formClose();

            else if (Regex.IsMatch(richTextBox1.Text, @"^(.*)を起動")){
                String str = richTextBox1.Text;
                richTextBox1.Text = "まかせて!";
                if(!Action.launchApp(str)) richTextBox1.Text = "名前が違うみたい";

            }else if (Regex.IsMatch(richTextBox1.Text, @"^(.*)で検索")){
                String str = richTextBox1.Text;
                richTextBox1.Text = "まかせて!";
                Action.searchWord(str);

            } else if(Regex.IsMatch(richTextBox1.Text, @"^(.*)の天気")){
                String str = richTextBox1.Text;
                richTextBox1.Text = Action.getWeather(str);

            } else {
                String str = richTextBox1.Text;
                richTextBox1.Text = "まかせて!";
                Action.launchAppByKeyOnly(str);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e){
            formClose();
        }

        private void ばいばいToolStripMenuItem_Click(object sender, EventArgs e){
            formClose();
        }

        private void formClose(){
            Properties.Settings.Default.Left = this.Left;
            Properties.Settings.Default.Top = this.Top;
            Properties.Settings.Default.Save();
            richTextBox1.Text = "またね!";
            richTextBox1.Update();
            System.Threading.Thread.Sleep(500);
            this.Close();
        }

    }
}
