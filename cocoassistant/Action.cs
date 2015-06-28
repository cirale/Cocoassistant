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

            Match m = Regex.Match(txt, @"^(?<app>.*)を起動");

            XmlSerializer serializer = new XmlSerializer(typeof(apps[]));
            StreamReader fs = new StreamReader(Directory.GetCurrentDirectory() + "\\" + @"settings\apps.xml",new UTF8Encoding(false));
            ap = (apps[])serializer.Deserialize(fs);
            fs.Close();
            for (int i = 0; i < ap.Length; i++) {
                if (ap[i].key.Equals(m.Groups["app"].Value, StringComparison.OrdinalIgnoreCase)) {
                    System.Diagnostics.Process.Start(ap[i].path);
                    return true;
                }
            }
            return false;
        }

        //アプリ名のみ入力されたとき 設定にあれば起動するがなければgoogleで検索する。
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
            System.Diagnostics.Process.Start(@"https://www.google.co.jp/search?q=" + txt);
        }

        //googleで検索する
        public static void searchWord(String txt) {
            Match m = Regex.Match(txt, @"^(?<word>.*)で検索");
            System.Diagnostics.Process.Start(@"https://www.google.co.jp/search?q=" + Uri.EscapeDataString(m.Groups["word"].Value));
        }

        public static String getWeather(String txt) {
            Match m = Regex.Match(txt, @"^(?<day>.*)の天気");
            if (!m.Groups["day"].Value.Equals(@"今日") && !m.Groups["day"].Value.Equals(@"明日")) return "今日と明日の天気しかわからないよ～";
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
                    high = it.temperature.max.celsius;
                    break;
                }
            }
            return m.Groups["day"].Value + "(" + date + ")の天気は" + tenki + "、最高気温は" + high + "℃、最低気温は" + low + "℃だよ！";

            
        }
    }
}
