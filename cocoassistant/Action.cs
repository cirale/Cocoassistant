using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Net;
using Codeplex.Data;
using System.Timers;

namespace cocoassistant{
    class Action{

        //指定したアプリケーションを起動
        public static bool launchApp(String txt){
            apps[] ap = new apps[256];
            /*ap[0] = new apps();
            ap[0].key = "chrome";
            ap[0].path = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
            ap[1] = new apps();
            ap[1].key = "krile";
            ap[1].path = @"D:\program files\krile\Krile.exe";


            XmlSerializer serializer = new XmlSerializer(typeof(apps[]));

            StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "\\" + @"settings\apps.xml",false,new UTF8Encoding(false));
            serializer.Serialize(sw, ap);
            sw.Close();*/

            Match m = Regex.Match(txt, @"^(?<path>.*)を(起動|開いて)");

            XmlSerializer serializer = new XmlSerializer(typeof(apps[]));
            StreamReader fs = new StreamReader(Directory.GetCurrentDirectory() + "\\" + @"settings\apps.xml",new UTF8Encoding(false));
            ap = (apps[])serializer.Deserialize(fs);
            fs.Close();
            for (int i = 0; i < ap.Length; i++) {
                if (ap[i].key.Equals(m.Groups["path"].Value, StringComparison.OrdinalIgnoreCase)) {
                    System.Diagnostics.Process.Start(ap[i].path);
                    return true;
                }
            }
            if (!File.Exists(m.Groups["path"].Value) && !Directory.Exists(m.Groups["path"].Value)) {
                return false;
            }
            System.Diagnostics.Process.Start(m.Groups["path"].Value);
            return true;
        }

        //アプリ名のみ入力されたとき 設定にあれば起動する パスが入力されていればそれを開く どちらでもなければgoogleで検索する。
        public static void launchAppByKeyOnly(String txt) {
            apps[] ap = new apps[256];

            XmlSerializer serializer = new XmlSerializer(typeof(apps[]));
            StreamReader fs = new StreamReader(Directory.GetCurrentDirectory() + "\\" + @"settings\apps.xml", new UTF8Encoding(false));
            ap = (apps[])serializer.Deserialize(fs);
            fs.Close();
            for (int i = 0; i < ap.Length; i++) {
                if (ap[i].key.Equals(txt, StringComparison.OrdinalIgnoreCase)) {
                    System.Diagnostics.Process.Start(ap[i].path);
                    return;
                }
            }
            if (File.Exists(txt) || Directory.Exists(txt)) {
                System.Diagnostics.Process.Start(txt);
                return;
            } 
            System.Diagnostics.Process.Start(@"https://www.google.co.jp/search?q=" + Uri.EscapeDataString(txt));
            return;
        }

        //googleで検索する
        public static void searchWord(String txt) {
            Match m = Regex.Match(txt, @"^(?<word>.*)で検索");
            System.Diagnostics.Process.Start(@"https://www.google.co.jp/search?q=" + Uri.EscapeDataString(m.Groups["word"].Value));
        }

        public static String getWeather(String txt) {
            Match m = Regex.Match(txt, @"^(?<day>.*)の天気");
            if (!m.Groups["day"].Value.Equals(@"今日") && !m.Groups["day"].Value.Equals(@"明日") && !m.Groups["day"].Value.Equals(@"明後日")) return "今日と明日と明後日の天気しかわからないよ～";
            String url = "http://weather.livedoor.com/forecast/webservice/json/v1?city=220040";
            WebClient wc = new WebClient();
            byte[] data = wc.DownloadData(url);
            Stream st = wc.OpenRead(url);
            Encoding ec = Encoding.GetEncoding("utf-8");
            var json = ec.GetString(data);
            var js = DynamicJson.Parse(json);
            var fc = js.forecasts;
            String tenki = "", date = "",high = "",low = "";
            
            foreach(var it in fc){
                if (it.dateLabel.Equals(m.Groups["day"].Value)) {
                    tenki = it.telop;
                    date = it.date;
                    if (it.temperature.min == null) low = "-";
                    else low = it.temperature.min.celsius;
                    if (it.temperature.max == null) high = "-";
                    else high = it.temperature.max.celsius;
                    break;
                }
            }
            if (m.Groups["day"].Value.Equals(@"明後日") && date == null) return "まだ明後日の天気はわからないよ～"; 
            return m.Groups["day"].Value + "(" + date + ")の天気は" + tenki + "、最高気温は" + high + "℃、最低気温は" + low + "℃だよ！";
        }

        public static String replyDate(String txt) {
            DateTime dt;
            Match m = Regex.Match(txt, @"^(?<date>.*)は何日");
            if (m.Groups["date"].Value.Equals("今日")) dt = DateTime.Today;
            else if (m.Groups["date"].Value.Equals("明日")) dt = DateTime.Today.AddDays(1);
            else if (m.Groups["date"].Value.Equals("明後日")) dt = DateTime.Today.AddDays(2);
            else if (m.Groups["date"].Value.Equals("昨日")) dt = DateTime.Today.AddDays(-1);
            else if (m.Groups["date"].Value.Equals("一昨日")) dt = DateTime.Today.AddDays(-2);
            else if (Regex.IsMatch(m.Groups["date"].Value, @"[0-9]*日前")) {
                m = Regex.Match(m.Groups["date"].Value, @"(?<num>[0-9]*)日前");
                int t = int.Parse(m.Groups["num"].Value);
                dt = DateTime.Today.AddDays(-t);
            } else if (Regex.IsMatch(m.Groups["date"].Value, @"[0-9]*日後")) {
                m = Regex.Match(m.Groups["date"].Value, @"(?<num>[0-9]*)日後");
                int t = int.Parse(m.Groups["num"].Value);
                dt = DateTime.Today.AddDays(t);
            } else return "いつ？";
            return Regex.Match(txt, @"^(?<date>.*)は何日").Groups["date"].Value + "は" + dt.ToString("yyyy年MM月dd日、dddd") + "だよ!";
        }

        public static String replyWeekDay(String txt) {
            DateTime dt;
            Match m = Regex.Match(txt, @"^(?<date>.*)は何曜日");
            if (m.Groups["date"].Value.Equals("今日")) dt = DateTime.Today;
            else if (m.Groups["date"].Value.Equals("明日")) dt = DateTime.Today.AddDays(1);
            else if (m.Groups["date"].Value.Equals("明後日")) dt = DateTime.Today.AddDays(2);
            else if (m.Groups["date"].Value.Equals("昨日")) dt = DateTime.Today.AddDays(-1);
            else if (m.Groups["date"].Value.Equals("一昨日")) dt = DateTime.Today.AddDays(-2);
            else if (Regex.IsMatch(m.Groups["date"].Value, @"[0-9]*日前")) {
                m = Regex.Match(m.Groups["date"].Value, @"(?<num>[0-9]*)日前");
                int t = int.Parse(m.Groups["num"].Value);
                dt = DateTime.Today.AddDays(-t);
            } else if (Regex.IsMatch(m.Groups["date"].Value, @"[0-9]*日後")) {
                m = Regex.Match(m.Groups["date"].Value, @"(?<num>[0-9]*)日後");
                int t = int.Parse(m.Groups["num"].Value);
                dt = DateTime.Today.AddDays(t);
            } else if (Regex.IsMatch(m.Groups["date"].Value, @"[0-9]{1,}年[0-9]{1,2}月[0-9]{1,2}日")) {
                m = Regex.Match(m.Groups["date"].Value, @"(?<y>[0-9]{1,})年(?<m>[0-9]{1,2})月(?<d>[0-9]{1,2})*日");
                int y = int.Parse(m.Groups["y"].Value);
                int n = int.Parse(m.Groups["m"].Value);
                int d = int.Parse(m.Groups["d"].Value);
                dt = new DateTime(y,n,d);
            } else return "いつ？";
            return Regex.Match(txt, @"^(?<date>.*)は何曜日").Groups["date"].Value + "は" + dt.ToString("dddd") + "だよ!";
        }
    }
}
