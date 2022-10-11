using Newtonsoft.Json;

namespace XkcdBot;

public class Comic
{

    [JsonProperty("title")] public string? Title;
    [JsonProperty("img")] public string? Image;
    [JsonProperty("alt")] public string? Alt;

    [JsonProperty("num")] private string? _id;
    public string Url => $"https://www.xkcd.com/{_id}";
    
    [JsonProperty("year")] private string? _year = "1000";
    [JsonProperty("month")] private string? _month = "10";
    [JsonProperty("day")] private string? _day = "10";
    public string Date => $"{_day}/{_month}/{_year} (DD/MM/YYYY)";

}