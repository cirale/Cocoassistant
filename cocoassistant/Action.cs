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
    public class Action{


        //指定したアプリケーションを起動
        public static bool launchApp(String txt){
            List<apps> ap = new List<apps>();
            Match m = Regex.Match(txt, @"^(?<path>.*)を(起動|開いて)");
            String key,opt = "";
            XmlSerializer serializer = new XmlSerializer(typeof(List<apps>));
            StreamReader fs = new StreamReader(Directory.GetCurrentDirectory() + "\\" + @"settings\apps.xml",new UTF8Encoding(false));
            ap = (List<apps>)serializer.Deserialize(fs);
            fs.Close();

            if (Regex.IsMatch(m.Groups["path"].Value, @"^(?<key>.*?):(?<cmdop>.*)$")) {
                Match op = Regex.Match(m.Groups["path"].Value, @"^(?<key>.*?):(?<cmdop>.*)$");
                key = op.Groups["key"].Value;
                opt = op.Groups["cmdop"].Value;
            } else {
                key = m.Groups["path"].Value;
            }

            for (int i = 0; i < ap.Count; i++) {
                if (ap[i].key.Equals(key, StringComparison.CurrentCultureIgnoreCase) && (File.Exists(ap[i].path) || Directory.Exists(ap[i].path))) {
                    Console.WriteLine(opt);
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                    psi.FileName = ap[i].path;
                    psi.Arguments = opt;
                    System.Diagnostics.Process.Start(psi);
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
            List<apps> ap = new List<apps>();
            String key, opt = "";
            XmlSerializer serializer = new XmlSerializer(typeof(List<apps>));
            StreamReader fs = new StreamReader(Directory.GetCurrentDirectory() + "\\" + @"settings\apps.xml", new UTF8Encoding(false));
            ap = (List<apps>)serializer.Deserialize(fs);
            fs.Close();

            if (Regex.IsMatch(txt, @"^(?<key>.*?):(?<cmdop>.*)$")) {
                Match op = Regex.Match(txt, @"^(?<key>.*?):(?<cmdop>.*)$");
                key = op.Groups["key"].Value;
                opt = op.Groups["cmdop"].Value;
            } else {
                key = txt;
            }

            for (int i = 0; i < ap.Count; i++) {
                if (ap[i].key.Equals(key, StringComparison.OrdinalIgnoreCase) && (File.Exists(ap[i].path) || Directory.Exists(ap[i].path))) {
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                    psi.FileName = ap[i].path;
                    psi.Arguments = opt;
                    System.Diagnostics.Process.Start(psi);
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

        //アプリ追加
        public static String addApp(String txt) 
        {
            Match m = Regex.Match(txt, @"(?<key>.*?):(?<path>.*)を追加して");
            Match op;

            op = Regex.Match(m.Groups["path"].Value, @"(?<path>[a-zA-Z]:.*?):(?<cmdop>.*)$");
            if (!File.Exists(m.Groups["path"].Value) && !Directory.Exists(m.Groups["path"].Value) && !File.Exists(op.Groups["path"].Value) && !Directory.Exists(op.Groups["path"].Value)) {
                return "パスが間違ってるよ～";
            }
            List<apps> ap = new List<apps>();

            XmlSerializer serializer = new XmlSerializer(typeof(List<apps>));
            StreamReader fs = new StreamReader(Directory.GetCurrentDirectory() + "\\" + @"settings\apps.xml", new UTF8Encoding(false));
            ap = (List<apps>)serializer.Deserialize(fs);
            fs.Close();

            for (int i = 0; i < ap.Count; i++) {
                if (ap[i].key.Equals(m.Groups["key"].Value)) return "キーが被ってるよ～";
            }
            ap.Add(new apps(m.Groups["key"].Value, op.Groups["path"].Value.Equals(String.Empty) ? m.Groups["path"].Value : op.Groups["path"].Value, op.Groups["cmdop"].Value.Equals(String.Empty) ? "" : op.Groups["cmdop"].Value));
            StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "\\" + @"settings\apps.xml",false, new UTF8Encoding(false));
            serializer.Serialize(sw,ap);
            sw.Close();
            return "追加したよ！";
        }

        //アプリ削除
        public static String deleteApp(String txt) {
            Match m = Regex.Match(txt, @"(?<key>.*?)を削除して");
            List<apps> ap = new List<apps>();

            XmlSerializer serializer = new XmlSerializer(typeof(List<apps>));
            StreamReader fs = new StreamReader(Directory.GetCurrentDirectory() + "\\" + @"settings\apps.xml", new UTF8Encoding(false));
            ap = (List<apps>)serializer.Deserialize(fs);
            fs.Close();

            for (int i = 0; i < ap.Count; i++) {
                if (ap[i].key.Equals(m.Groups["key"].Value)) {
                    ap.RemoveAt(i);
                    StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "\\" + @"settings\apps.xml", false, new UTF8Encoding(false));
                    serializer.Serialize(sw, ap);
                    sw.Close();
                    return "削除したよ！";
                }
            }
            return "アプリ名が間違ってるよ～";
        }

        public static String getAppList() {
            List<apps> ap = new List<apps>();

            XmlSerializer serializer = new XmlSerializer(typeof(List<apps>));
            StreamReader fs = new StreamReader(Directory.GetCurrentDirectory() + "\\" + @"settings\apps.xml", new UTF8Encoding(false));
            ap = (List<apps>)serializer.Deserialize(fs);
            fs.Close();

            String res = "全部で" + ap.Count + "件だよ！\n";
            for (int i = 0; i < ap.Count; i++) {
                res += ap[i].key + ":" + ap[i].path + (ap[i].cmdop == null ? "\n" : ":" + ap[i].cmdop + "\n");
            }
            return res;
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
            if (m.Groups["day"].Value.Equals(@"明後日") && date == "") return "まだ明後日の天気はわからないよ～"; 
            return m.Groups["day"].Value + "(" + date + ")の天気は" + tenki + ((high!="-") ? "、最高気温は" + high + "℃" : "") + ((low!="-") ? "、最低気温は" + low + "℃" : "") + "だよ！";
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
                try {
                    int t = int.Parse(m.Groups["num"].Value);
                    dt = DateTime.Today.AddDays(-t);
                } catch {
                    return "わからないよ～";
                }
            } else if (Regex.IsMatch(m.Groups["date"].Value, @"[0-9]*日後")) {
                m = Regex.Match(m.Groups["date"].Value, @"(?<num>[0-9]*)日後");
                try {
                    int t = int.Parse(m.Groups["num"].Value);
                    dt = DateTime.Today.AddDays(t);
                } catch {
                    return "わからないよ～";
                }

            } else return "わからないよ～";

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
                try {
                    dt = new DateTime(y, n, d);
                } catch {
                    return "わからないよ～";
                }
            } else return "わからないよ～";
            return Regex.Match(txt, @"^(?<date>.*)は何曜日").Groups["date"].Value + "は" + dt.ToString("dddd") + "だよ!";
        }

        public static String processInfo() {
            String res = "私のプロセス情報だよ！\n";
            System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess();
            p.Refresh();
            res += "プロセスID:" + p.Id + "\n";
            res += "プロセス名:" + p.ProcessName + "\n";
            res += "基本優先度:" + p.BasePriority + "\n";
            res += "物理メモリ:" + p.WorkingSet64 + "\n";
            res += "仮想メモリ:" + p.VirtualMemorySize64 + "\n";
            return res;
        }
    }
}
