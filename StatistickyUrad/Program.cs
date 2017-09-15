using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using HtmlAgilityPack;

namespace StatistickyUrad
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (string file in Directory.GetFiles(".", "*.xml"))
            {
                string[] lines = File.ReadAllLines(file, Encoding.GetEncoding(1250)).Where(p => p.Contains("<TEXT>")).Select(p => p.Replace("<TEXT>", "").Replace("</TEXT>", "").Replace("-", " ").Trim()).ToArray();
                File.WriteAllLines(file.Replace(".xml", ".txt"), lines);
            }
        }

        static void HledejObce()
        {
            HtmlDocument doc = new HtmlDocument();
            WebClient client = new WebClient();
            doc.LoadHtml(client.DownloadString("https://www.czso.cz/staticke/sldb/sldb2001.nsf/index"));
            List<string> cities = new List<string>();
            foreach (HtmlNode areanode in doc.DocumentNode.SelectNodes("//area"))
            {
                string href = "https://www.czso.cz" + areanode.Attributes["href"].Value;
                foreach (string okres in ParseOkresy(href))
                {
                    cities.AddRange(ParseObce(okres));
                }
            }
            File.WriteAllLines("obce.txt", cities.OrderBy(p => p).Distinct());
        }

        static List<string> ParseOkresy(string page)
        {
            List<string> list = new List<string>();
            HtmlDocument doc = new HtmlDocument();
            WebClient client = new WebClient();
            try
            {
                string pageContent = Encoding.GetEncoding(1250).GetString(client.DownloadData(page.Replace("&amp;", "&")));
                doc.LoadHtml(pageContent);

                foreach (HtmlNode aNode in doc.DocumentNode.SelectNodes("//a"))
                {
                    string href = aNode.Attributes["href"].Value;
                    if (href.Contains("okresy"))
                        list.Add("https://www.czso.cz" + href);
                }
            }
            catch (Exception) { return new List<string>(); }
            return list;
        }
        static List<string> ParseObce(string page)
        {
            List<string> list = new List<string>();
            HtmlDocument doc = new HtmlDocument();
            WebClient client = new WebClient();
            byte[] buffer = client.DownloadData(page.Replace("&amp;", "&"));
            string pageContent = Encoding.UTF8.GetString(buffer);
            doc.LoadHtml(pageContent);

            foreach (HtmlNode aNode in doc.DocumentNode.SelectNodes("//a"))
            {
                string href = aNode.Attributes["href"].Value;
                if (href.Contains("obce"))
                    list.Add(aNode.InnerText);
            }
            return list;
        }
    }
}
