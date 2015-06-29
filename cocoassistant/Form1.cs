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
using System.Timers;

namespace cocoassistant{
    public partial class Form1 : Form{
        private Point mousePoint;
        private String time;
        private String stime;
        private System.Timers.Timer timer;
        private int sInterval;
       
        public Form1(){
            InitializeComponent();
        }

        private void Form1_Load(object sender, System.EventArgs e){

            this.ShowInTaskbar = false;

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
            this.TopMost = Properties.Settings.Default.MostTop;
            toolStripMenuItem1.Checked = Properties.Settings.Default.MostTop;

           //位置復元
            this.Left = Properties.Settings.Default.Left;
            this.Top = Properties.Settings.Default.Top;

            richTextBox1.Text = "ご注文は何ですか？";
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

            else if (richTextBox1.Text == "ありがとう") richTextBox1.Text = "どういたしまして!";

                //ファイルまたはフォルダを開く
            else if (Regex.IsMatch(richTextBox1.Text, @"^(.*)を(起動|開いて)")){
                String str = richTextBox1.Text;
                richTextBox1.Text = "まかせて!";
                if(!Action.launchApp(str)) richTextBox1.Text = "開けなかったよ～";
                //ググる
            }else if (Regex.IsMatch(richTextBox1.Text, @"^(.*)で検索")){
                String str = richTextBox1.Text;
                richTextBox1.Text = "まかせて!";
                Action.searchWord(str);
                //天気を取得
            } else if(Regex.IsMatch(richTextBox1.Text, @"^(.*)の天気")){
                String str = richTextBox1.Text;
                richTextBox1.Text = Action.getWeather(str);
                //日付を取得
            } else if(Regex.IsMatch(richTextBox1.Text, @"^(.*)は何日")){
                String str = richTextBox1.Text;
                richTextBox1.Text = Action.replyDate(str);
                //曜日を取得
            } else if (Regex.IsMatch(richTextBox1.Text, @"^(.*)は何曜日")) {
                String str = richTextBox1.Text;
                richTextBox1.Text = Action.replyWeekDay(str);
                //タイマー Actionに含められない…
            } else if (Regex.IsMatch(richTextBox1.Text, @"^(.*)(時間|分|秒)(経ったら教えて|計って)")) {
                String str = richTextBox1.Text;
                richTextBox1.Text = "まかせて!";
                int interval = 0;
                if (Regex.IsMatch(str, @"([0-9]+)秒")) {
                    interval = int.Parse(Regex.Match(str, @"(?<s>[0-9]+)秒").Groups["s"].Value);
                }
                if (Regex.IsMatch(str, @"([0-9]+)分")) {
                    interval += int.Parse(Regex.Match(str, @"(?<s>[0-9]+)分").Groups["s"].Value) * 60;
                }
                if (Regex.IsMatch(str, @"([0-9]+)時間")) {
                    interval += int.Parse(Regex.Match(str, @"(?<s>[0-9]+)時間").Groups["s"].Value) * 60 * 60;
                }
                interval *= 1000;
                sInterval = interval;
                if (interval == 0) return;
                time = str.Replace("経ったら教えて", "").Replace("計って","");
                timer = new System.Timers.Timer();
                timer.Elapsed += new ElapsedEventHandler(endTimer);
                timer.Interval = interval;
                timer.AutoReset = false;
                timer.Start();

            } else if (Regex.IsMatch(richTextBox1.Text, @"^(.*)(になったら教えて)")) {
                String str = richTextBox1.Text;
                DateTime dt, tdt;
                int m = -1, h = -1, d = -1, n = -1, y = -1;
                richTextBox1.Text = "まかせて!";
                int interval = 0;
                if (Regex.IsMatch(str, @"([0-9]+)分")) {
                    m = int.Parse(Regex.Match(str, @"(?<m>[0-9]+)分").Groups["m"].Value);
                }
                if (Regex.IsMatch(str, @"([0-9]+)時")) {
                    h = int.Parse(Regex.Match(str, @"(?<h>[0-9]+)時").Groups["h"].Value);
                }
                if (Regex.IsMatch(str, @"([0-9]+)日")) {
                    d = int.Parse(Regex.Match(str, @"(?<d>[0-9]+)日").Groups["d"].Value);
                }
                if (Regex.IsMatch(str, @"([0-9]+)月")) {
                    n = int.Parse(Regex.Match(str, @"(?<n>[0-9]+)月").Groups["n"].Value);
                }
                if (Regex.IsMatch(str, @"([0-9]+)年")) {
                    y = int.Parse(Regex.Match(str, @"(?<y>[0-9]+)年").Groups["y"].Value);
                }
                tdt = DateTime.Now;
                dt = new DateTime((y>=0) ? y : tdt.Year, (n>=0) ? n : tdt.Month, (d>=0) ? d : tdt.Day, (h>=0) ? h : tdt.Hour, (m>=0) ? m : tdt.Minute, 0);
                TimeSpan tp = dt - tdt;
                Console.WriteLine((int)tp.TotalMilliseconds);
                interval = (int)tp.TotalMilliseconds;
                stime = str.Replace("になったら教えて", "");
                timer = new System.Timers.Timer();
                timer.Elapsed += new ElapsedEventHandler(endTimer2);
                timer.Interval = interval;
                timer.AutoReset = false;
                timer.Start();

                //タイマーを止める
            }else if(Regex.IsMatch(richTextBox1.Text, @"タイマー止めて")){
                richTextBox1.Text = "わかった!";
                timer.Stop();

            } else if (Regex.IsMatch(richTextBox1.Text, @"もう一回計って")) {
                richTextBox1.Text = "わかった!";
                timer = new System.Timers.Timer();
                timer.Elapsed += new ElapsedEventHandler(endTimer);
                timer.Interval = sInterval;
                timer.AutoReset = false;
                timer.Start();

                //その他 アプリ名をもとにソフト起動したりフォルダ・ファイル開いたりググったり
            }else {
                String str = richTextBox1.Text;
                richTextBox1.Text = "まかせて!";
                Action.launchAppByKeyOnly(str);
            }
        }

        private void endTimer(object sender, ElapsedEventArgs e) {
            this.notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            this.notifyIcon1.BalloonTipText = time + "経ったよ！";
            this.notifyIcon1.ShowBalloonTip(3000);
        }

        private void endTimer2(object sender, ElapsedEventArgs e) {
            this.notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            this.notifyIcon1.BalloonTipText = stime + "になったよ！";
            this.notifyIcon1.ShowBalloonTip(3000);
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
            Properties.Settings.Default.MostTop = this.TopMost;
            Properties.Settings.Default.Save();
            richTextBox1.Text = "またね!";
            richTextBox1.Update();
            System.Threading.Thread.Sleep(500);
            this.Close();
        }

        private void toolStripMenuItem1_Changed(object sender, EventArgs e) {
            if (toolStripMenuItem1.Checked) this.TopMost = true;
            else this.TopMost = false;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e) {
            toolStripMenuItem1.Checked = !toolStripMenuItem1.Checked;
        }


        private void notifyIcon1_Click(object sender, EventArgs e) {
            if (!this.TopMost) {
                this.TopMost = true;
                this.TopMost = false;
            }

        }

    }
}
