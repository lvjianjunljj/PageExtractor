using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace Test
{
    class Program
    {
        static int depthMax = 3;
        static Dictionary<string, int> unload;
        static Dictionary<string, int> loaded;
        static void Main(string[] args)
        {
            unload = new Dictionary<string, int>();
            loaded = new Dictionary<string, int>();
            string baseUrl = "news.sina.com.cn";
            string saveDir = "D://test";
            unload.Add("http://" + baseUrl + "/", 0);
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }


            while (unload.Count > 0)
            {
                string url = unload.First().Key;
                int depth = unload.First().Value;
                loaded.Add(url, depth);
                unload.Remove(url);

                Console.WriteLine("Now loading " + url);

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";
                req.Accept = "text/html";
                req.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0)";

                try
                {
                    string html = null;
                    HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                    using (StreamReader reader = new StreamReader(res.GetResponseStream()))
                    {
                        html = reader.ReadToEnd();
                        if (!string.IsNullOrEmpty(html))
                        {
                            string filePath = url.Replace("/", "").Replace(":", "");
                            while (filePath.Length > 0 && filePath.EndsWith("."))
                            {
                                filePath = filePath.Substring(0, filePath.Length - 1);
                            }
                            filePath = saveDir + "/" + filePath + ".txt";
                            SaveTxt(filePath, html);
                            Console.WriteLine("Download OK!\n");
                        }
                    }
                    string[] links = GetLinks(html);
                    AddUrls(links, depth + 1, baseUrl, unload, loaded);
                }
                catch (WebException we)
                {
                    Console.WriteLine(we.Message);
                }
            }
        }

        private static string[] GetLinks(string html)
        {
            const string pattern = @"http://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection m = r.Matches(html);
            string[] links = new string[m.Count];

            for (int i = 0; i < m.Count; i++)
            {
                links[i] = m[i].ToString();
            }
            return links;
        }

        private static bool UrlAvailable(string url)
        {
            if (unload.ContainsKey(url) || loaded.ContainsKey(url))
            {
                return false;
            }
            if (url.Contains(".jpg") || url.Contains(".gif")
                || url.Contains(".png") || url.Contains(".css")
                || url.Contains(".js"))
            {
                return false;
            }
            return true;
        }

        private static void AddUrls(string[] urls, int depth, string baseUrl, Dictionary<string, int> unload, Dictionary<string, int> loaded)
        {
            if (depth >= depthMax)
            {
                return;
            }
            foreach (string url in urls)
            {
                string cleanUrl = url.Trim();
                int end = cleanUrl.IndexOf(' ');
                if (end > 0)
                {
                    cleanUrl = cleanUrl.Substring(0, end);
                }
                if (UrlAvailable(cleanUrl))
                {
                    if (cleanUrl.Contains(baseUrl))
                    {
                        unload.Add(cleanUrl, depth);
                    }
                    else
                    {
                        // 外链
                        Console.WriteLine(cleanUrl + " " + "is an external link");
                    }
                }
            }
        }

        private static void SaveTxt(string filePath, string content)
        {
            Console.WriteLine(filePath);
            FileStream fs = new FileStream(filePath, FileMode.Create);
            //获得字节数组
            byte[] data = System.Text.Encoding.Default.GetBytes(content);
            //开始写入
            fs.Write(data, 0, data.Length);
            //清空缓冲区、关闭流
            fs.Flush();
            fs.Close();
        }
    }
}
