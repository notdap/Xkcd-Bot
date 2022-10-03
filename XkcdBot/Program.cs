namespace XkcdBot;

public class Program
{
    public static void Main(string[] args)
    {
        var url = XkcdUtils.GetXkcdApiUrlFromStringAsync("hi").Result;
        var comic = XkcdUtils.GetComicAsync(url).Result;
        
        Console.WriteLine(comic.Title);
        Console.WriteLine(comic.Image);
        Console.WriteLine(comic.Date);
        Console.WriteLine(comic.Url);
        Console.WriteLine(comic.Alt);
    }
}