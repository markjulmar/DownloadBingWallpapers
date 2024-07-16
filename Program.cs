using System.Text.Json;

const string bingSite = "https://www.bing.com";
const string latestWallpapers = $"{bingSite}/HPImageArchive.aspx?format=js&idx=0&n=8&mkt=en-ww";

using var client = new HttpClient();

var wallpaperJson = await client.GetStringAsync(latestWallpapers);
var wallpaperList = JsonSerializer.Deserialize<Wallpapers>(wallpaperJson, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
if (wallpaperList is null or { Images.Count: 0 })
{
    Console.WriteLine("No wallpapers found.");
    return;
}

for (var i = 0; i < wallpaperList.Images?.Count; i++)
{
    var item = wallpaperList.Images[i];
    try
    {
        if (item.Url != null)
        {
            await DownloadWallpaperAsync(client, item.Url, i);
        }
    }
    catch (Exception ex)
    {
        await Console.Error.WriteLineAsync($"Failed to download {item.Url}: {ex.Message}");
    }
}

return;

static async Task DownloadWallpaperAsync(HttpClient client, string url, int pos)
{
    var filename = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        GetFilename(url, pos));

    var stream = await client.GetStreamAsync(bingSite + url);
    await using var writer = File.OpenWrite(filename);
    await stream.CopyToAsync(writer);
}

static string GetFilename(string url, int pos)
{
    const string idKey = "?id=";

    // /th?id=OHR.BourgesAerial_ROW9185097510_1920x1080.jpg&rf=LaDigue_1920x1080.jpg&pid=hp
    var startPos = url.IndexOf(idKey, StringComparison.Ordinal);
    if (startPos > 0)
    {
        startPos += idKey.Length;
        var endPos = url.IndexOf('&', startPos);
        if (endPos > 0)
        {
            return url[startPos..endPos];
        }
    }

    return "wallpaper_" + pos;
}

public sealed class BingWallpaperImage
{
    //public string? Title { get; set; }
    //public string? Copyright { get; set; }
    public string? Url { get; set; }
}

public sealed class Wallpapers
{
    public List<BingWallpaperImage>? Images { get; set; }
}

