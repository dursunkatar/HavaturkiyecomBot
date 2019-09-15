using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace HavaTurkiyeComBot
{
    class Program
    {
        static readonly string url = "https://www.havaturkiye.com/weather/maps/city";
        static readonly List<string> tarihveSaatler = new List<string>();
        static readonly WebHeaderCollection WebHeaders;
        static readonly string GUN_DOGUMU_XPATH = "/html/body/div[2]/div[3]/div[1]/div/div[6]/div[2]/div[1]/div[3]/div[1]/table/tbody/tr[8]/td[2]";
        static readonly string GUN_BATIMI_XPATH = "/html/body/div[2]/div[3]/div[1]/div/div[6]/div[2]/div[1]/div[3]/div[1]/table/tbody/tr[9]/td[2]";

        static Program()
        {
            WebHeaders = new WebHeaderCollection();
            WebHeaders.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:68.0) Gecko/20100101 Firefox/68.0");
            WebHeaders.Add(HttpRequestHeader.Referer, "https://www.havaturkiye.com/weather/maps/city");
            WebHeaders.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3");
        }

        static void Main(string[] args)
        {
            using (StreamWriter writer = File.AppendText(@"db.txt"))
            {
                string data = "";

                for (int i = 1; i < 5000; i++)
                {
                    DateTime tarih = DateTime.Today.AddDays(-i);
                    Console.WriteLine(i + " : " + tarih.ToShortDateString());
                    data = i.ToString() + "-" + tarih.ToShortDateString();

                    var (dogus, batis) = Getir(tarih);
                    data += "-" + dogus + "-" + batis;

                    writer.Write(data + Environment.NewLine);
                    writer.Flush();
                }
            }
            Console.WriteLine("Bitti");
            Console.Read();
        }

        static (string, string) Getir(DateTime tarih)
        {
            using (var client = new WebClient())
            {
                client.Headers = WebHeaders;
                byte[] data = client.UploadValues(url, "POST", postData(tarih));
                string response = client.Encoding.GetString(data);
                return gunSaatler(response);
            }
        }

        static (string, string) gunSaatler(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            string gunDogumu = doc.DocumentNode.SelectSingleNode(GUN_DOGUMU_XPATH).InnerText.Split('=')[0].Trim();
            string gunBatimi = doc.DocumentNode.SelectSingleNode(GUN_BATIMI_XPATH).InnerText.Split('=')[0].Trim();
            return (gunDogumu, gunBatimi);
        }

        static NameValueCollection postData(DateTime tarih)
        {
            string month = tarih.Month.ToString();
            string day = tarih.Day.ToString();
            var reqparm = new NameValueCollection();
            reqparm.Add("JJ", tarih.Year.ToString());
            reqparm.Add("MM", month.Length == 1 ? "0" + month : month);
            reqparm.Add("TT", day.Length == 1 ? "0" + day : day);
            reqparm.Add("CONT", "trtr");
            reqparm.Add("LANG", "tr");
            reqparm.Add("NOREGION", "0");
            reqparm.Add("R", "0");
            reqparm.Add("WMO", "17060");
            reqparm.Add("LEVEL", "180");
            reqparm.Add("REGION", "0005");
            reqparm.Add("LAND", "TU");
            reqparm.Add("PLZ", "_____");
            reqparm.Add("PLZN", "_____");
            return reqparm;
        }
    }
}
