using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DownloadBingWallpapers
{
    [JsonObject]
    public class BingWallpaperImage
    {
        [JsonProperty("startdate")]
        public string startDate { get; set; }
        [JsonProperty("enddate")]
        public string endDate { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        public string copyright { get; set; }
        public string title { get; set; }
    }

    public class Wallpapers
    {
        public List<BingWallpaperImage> images { get; set; }
    }

    class Program
    {
        const string LatestWallpapers = @"https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=8&mkt=en-ww";
        const string BingSite = @"https://www.bing.com";

        static async Task Main()
        {
            HttpClient client = new HttpClient();

            var wallpaperJson = await client.GetStringAsync(LatestWallpapers);
            var wallpaperList = JsonConvert.DeserializeObject<Wallpapers>(wallpaperJson);
            for (int i = 0; i < wallpaperList.images.Count; i++)
            {
                BingWallpaperImage item = (BingWallpaperImage)wallpaperList.images[i];
                try
                {
                    await DownloadWallpaperAsync(client, item.Url, i);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to download {item.Url}: {ex.Message}");
                }
            }
        }

        private static async Task DownloadWallpaperAsync(HttpClient client, string url, int pos)
        {
            string filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), GetFilename(url, pos));

            var stream = await client.GetStreamAsync(BingSite + url);
            using var writer = File.OpenWrite(filename);
            await stream.CopyToAsync(writer);
        }

        private static string GetFilename(string url, int pos)
        {
            const string idKey = "?id=";

            // /th?id=OHR.BourgesAerial_ROW9185097510_1920x1080.jpg&rf=LaDigue_1920x1080.jpg&pid=hp
            int startPos = url.IndexOf(idKey);
            if (startPos > 0)
            {
                startPos += idKey.Length;
                int endPos = url.IndexOf('&', startPos);
                if (endPos > 0)
                {
                    return url.Substring(startPos, endPos - startPos);
                }
            }

            return "wallpaper_" + pos.ToString();
        }
    }
}
