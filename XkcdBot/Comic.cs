using Newtonsoft.Json;

namespace XkcdBot;

public class Comic
{

    [JsonProperty("title")] public string? Title;
    [JsonProperty("img")] public string? Image;
    [JsonProperty("alt")] public string? Alt;

    [JsonProperty("num")] private string? _id = "1";
    public string Url => $"https://www.xkcd.com/{_id}";
    
    [JsonProperty("year")] public string? Year = "1000";
    [JsonProperty("month")] public string? Month = "10";
    [JsonProperty("day")] public string? Day = "10";

}