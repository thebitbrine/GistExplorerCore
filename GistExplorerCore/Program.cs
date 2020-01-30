using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using Newtonsoft;
using Newtonsoft.Json;
using QuickType;
using TextCopy;

namespace GistExplorer
{
    class Program
    {
        public Gists[] GlobalGists;
        [STAThread]
        static void Main(string[] args)
        {
#if DEBUG
            args = new string[] { "thebitbrine" };
#endif
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Program p = new Program();
            Console.Title = "GistExplorer v2.2";
            if (args.Length == 0)
            {
                Console.Write("Usage: \"GistExplorer.exe USERNAME\"");
                Console.ReadKey();
            }
            else
                while (true)
                {
                    p.Run(args);
                    Console.Clear();
                }

        }

        public void GetAllGists(string Username)
        {
            var Response = GetWebString($"https://api.github.com/users/{Username}/gists?per_page=10000");
            GlobalGists = JsonConvert.DeserializeObject<Gists[]>(Response);
        }

        public Gists[] Search(string Query)
        {
            List<Gists> Results = new List<Gists>();

            foreach (var Gist in GlobalGists)
                foreach (var File in Gist.Files)
                    if (File.Key.StartsWith(Query) && !Results.Contains(Gist))
                        Results.Add(Gist);

            foreach (var Gist in GlobalGists)
                foreach (var File in Gist.Files)
                    if (File.Key.ToLower().StartsWith(Query.ToLower()) && !Results.Contains(Gist))
                        Results.Add(Gist);

            foreach (var Gist in GlobalGists)
                foreach (var File in Gist.Files)
                    if (File.Key.Contains(Query) && !Results.Contains(Gist))
                        Results.Add(Gist);

            foreach (var Gist in GlobalGists)
                foreach (var File in Gist.Files)
                    if (File.Key.ToLower().Contains(Query.ToLower()) && !Results.Contains(Gist))
                        Results.Add(Gist);

            foreach (var Gist in GlobalGists)
                foreach (var File in Gist.Files)
                    if (File.Key.EndsWith(Query) && !Results.Contains(Gist))
                        Results.Add(Gist);

            foreach (var Gist in GlobalGists)
                foreach (var File in Gist.Files)
                    if (File.Key.ToLower().EndsWith(Query.ToLower()) && !Results.Contains(Gist))
                        Results.Add(Gist);

            foreach (var Gist in GlobalGists)
                foreach (var File in Gist.Files)
                    if (File.Value.Language != null && File.Value.Language.ToString().ToLower().Contains(Query.ToLower()) && !Results.Contains(Gist))
                        Results.Add(Gist);

            foreach (var Gist in GlobalGists)
                foreach (var File in Gist.Files)
                    if (File.Value.Language != null && Query.ToLower().Contains(File.Value.Language.ToString().ToLower()) && !Results.Contains(Gist))
                        Results.Add(Gist);

            foreach (var Gist in GlobalGists)
                foreach (var File in Gist.Files)
                    if (Gist.Description.ToLower().Contains(Query.ToLower()) && !Results.Contains(Gist))
                        Results.Add(Gist);

            return Results.ToArray();
        }

        public string User = "NOT SET";
        public void Run(string[] args)
        {
            User = args[0];
            if (GlobalGists == null || GlobalGists?.Count() <= 0)
                new Thread(() => GetAllGists(User)) { IsBackground = true }.Start();
            Banner();
            Console.Write("Enter query: ");
            string Query = Console.ReadLine();
            while (GlobalGists == null) Thread.Sleep(250);
            var SearchResults = Search(Query);

            if (SearchResults.Count() > 0)
            {
                int SnipSelect = -1;
                do
                {
                    Console.Clear();
                    Banner();
                    Console.WriteLine("Query results:");
                    int qIndex = 1;
                    foreach (var Snipp in SearchResults)
                    {
                        Console.WriteLine($" {qIndex.ToString().PadLeft(2, '0')}) {StringLimit(Snipp.Files.First().Key, 40).PadRight(39, ' ')}\t| {User},\t{NeatDate(Snipp.UpdatedAt)},\t{StringLimit(Snipp.Description, 50)}");
                        qIndex++;
                    }
                    Console.Write("Select snippet: ");
                    int.TryParse(Console.ReadLine(), out SnipSelect);
                } while (SnipSelect == -1);
                if (SnipSelect > 0 || SnipSelect > SearchResults.Count())
                {
                    //Console.Clear();
                    Banner();
                    Console.WriteLine($"Downloading {SearchResults[SnipSelect - 1].Files.First().Key}...");
                    //Console.Clear();
                    Banner();
                    string RawSnippet = GetWebString(SearchResults[SnipSelect - 1].Files.First().Value.RawUrl.ToString());
                    Clipboard.SetText(RawSnippet);
                    Console.WriteLine(SearchResults[SnipSelect - 1].Files.First().Key + " copied to clipboard.");
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("Nothing found.");
                Console.ReadKey();
            }
        }

        public string NeatDate(DateTimeOffset Date)
        {
            string Month = Date.ToString("MMM");
            string Day = Date.Day.ToString("D2");
            string Year = Date.Year.ToString();

            return $"{Month} {Day} {Year}";
        }

        public string StringLimit(string Text, int Limit)
        {
            if (Text.Length > Limit)
            {
                if (Text.Substring(0, Limit - 3).Last() == ' ') return Text.Substring(0, Limit - 4) + "...";
                else return Text.Substring(0, Limit - 3) + "..";
            }
            return Text;
        }

        public string GetWebString1(string URL)
        {
            var client = new System.Net.WebClient() { Encoding = System.Text.Encoding.UTF8 };
            client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (compatible; MSIE 10.0; Windows Phone 8.0; Trident/6.0; IEMobile/10.0; ARM; Touch; NOKIA; Lumia 520)");
            client.Headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            //client.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            client.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.9");
            client.Headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
            client.Headers.Add(HttpRequestHeader.Host, "gist.github.com");
            return client.DownloadString(URL);
        }



        public static string GetWebString(string URI)
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            using (var httpClient = new HttpClient(handler))
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), URI))
                {
                    request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.109 Mobile Safari/537.36");
                    return httpClient.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
                }
            }
        }


        public void Banner()
        {
            Console.WriteLine(CenterString($"[{User}]", 40));
            Console.WriteLine("  GistExplorer v2.2 (Ultimate Edition)  ");
            Console.WriteLine("========================================");
        }

        public string CenterString(string stringToCenter, int totalLength)
        {
            return stringToCenter.PadLeft(((totalLength - stringToCenter.Length) / 2)
                                + stringToCenter.Length, '=')
                       .PadRight(totalLength, '=');
        }

    }
}
