using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace XkcdBot;

public static class XkcdUtils
{

    private static readonly Dictionary<string, Comic> Cache = new();

    private static HttpClient Client => new();
    
    private static string UserAgent => "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.5112.126 Safari/537.36";
    private static string DuckDuckGoUrlTemplate => "https://html.duckduckgo.com/html/?q=site:xkcd.com+";
    private static Regex XkcdRegex => new(@"xkcd\.com\/\d+\/?/");
    
    public static async Task<string> GetXkcdApiUrlFromStringAsync(string query)
    {
        Program.LogInfo("Retrieving Xkcd API URL from a string... ");
        var duckDuckGoRequest = DuckDuckGoUrlTemplate + query.Replace(" ", "+");
        
        var request = new HttpRequestMessage(HttpMethod.Get, duckDuckGoRequest);
        request.Headers.Add("User-Agent", UserAgent);

        var response = await Client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        var rawUrl = XkcdRegex.Match(content).Value;
        var url = $"https://www.{rawUrl}info.0.json";
        
        Program.LogInfo("Success!", false);
        return url;
    }

    public static string GetXkcdApiUrlFromInt(int? query)
    {
        return query == null ? "https://www.xkcd.com/info.0.json" : $"https://www.xkcd.com/{query}/info.0.json";
    }

    public static async Task<Comic> GetComicAsync(string url)
    {
        Program.LogInfo("Fetching Comic... ");
        
        if (Cache.ContainsKey(url))
        {
            Program.LogInfo("Success (Cached)!", false);
            return Cache[url];
        }
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", UserAgent);

        var response = await Client.SendAsync(request);

        if (response.StatusCode is not HttpStatusCode.OK)
        {
            throw new NullReferenceException("Could not find comic");
        }

        var content = await response.Content.ReadAsStringAsync();
        var comic = JsonConvert.DeserializeObject<Comic>(content);
        
        Cache.Add(url, comic ?? throw new InvalidOperationException("Invalid Comic"));

        Program.LogInfo("Success (API Call)!", false);
        return comic;
    }
    
}