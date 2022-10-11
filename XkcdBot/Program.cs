using Discord;
using Discord.WebSocket;

namespace XkcdBot;

public static class Program
{

    private static DiscordSocketClient? _client;
    
    private static async Task MainAsync()
    {
        await XkcdUtils.LoadCacheAsync();
        
        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.None
        };
        _client = new DiscordSocketClient(config);

        _client.Log += Log;

        if (!File.Exists("Token.txt"))
        {
            File.Create("Token.txt");
        }
        var token = await File.ReadAllTextAsync("Token.txt");

        _client.Ready += OnReadyAsync;
        _client.SlashCommandExecuted += OnSlashCommandAsync;
        
        await _client.SetStatusAsync(UserStatus.AFK);
        await _client.SetActivityAsync(new Game("xkcd"));
        
        await _client.LoginAsync(TokenType.Bot, token.Trim());
        await _client.StartAsync();
        
        await Task.Delay(-1);
    }

    private static async Task OnReadyAsync()
    {
        if (_client == null) return;
        
        var command = new SlashCommandBuilder()
            .WithName("xkcd")
            .WithDescription("Sends an xkcd comic in chat")
            .AddOption(new SlashCommandOptionBuilder()
                .WithRequired(false)
                .WithName("query")
                .WithDescription("The comic's number or name").WithType(ApplicationCommandOptionType.String)
            );
        
        var basicCommand = new SlashCommandBuilder()
            .WithName("bxkcd")
            .WithDescription("Same as /xkcd but much, much simpler (just the image)")
            .AddOption(new SlashCommandOptionBuilder()
                .WithRequired(false)
                .WithName("query")
                .WithDescription("The comic's number or name").WithType(ApplicationCommandOptionType.String)
            );

        await _client.CreateGlobalApplicationCommandAsync(command.Build());
        await _client.CreateGlobalApplicationCommandAsync(basicCommand.Build());
    }

    private static async Task OnSlashCommandAsync(SocketSlashCommand command)
    {
        await command.DeferAsync();

        var query = command.Data.Options.Count is 0 
            ? "standards" 
            : (command.Data.Options.First().Value as string ?? "1").Trim();

        string url;
        try
        {
            var queryInt = int.Parse(query);
            url = XkcdUtils.GetXkcdApiUrlFromInt(queryInt);
        }
        catch (Exception)
        {
            url = await XkcdUtils.GetXkcdApiUrlFromStringAsync(query);
        }

        Comic comic;
        try
        {
            comic = await XkcdUtils.GetComicAsync(url);
        }
        catch (Exception)
        {
            var errorEmbed = new EmbedBuilder()
                .WithAuthor("There was an error", _client?.CurrentUser.GetDefaultAvatarUrl())
                .WithDescription($"The provided query *({query})* did not return any valid comic.")
                .WithColor(Color.Red);

            await command.FollowupAsync(embed: errorEmbed.Build());
            return;
        }

        if (command.Data.Name is "xkcd")
        {
            var footer = "No query was provided";
            if (command.Data.Options?.Count is not 0)
                footer = $"Query: {query}";
        
            var embed = new EmbedBuilder()
                .WithAuthor(comic.Title, _client?.CurrentUser.GetDefaultAvatarUrl(), comic.Url)
                .WithImageUrl(comic.Image)
                .WithDescription(comic.Alt)
                .WithFooter(footer)
                .WithCurrentTimestamp()
                .WithColor(Color.Green);

            await command.FollowupAsync(embed: embed.Build()); 
        }
        else
        {
            await command.FollowupAsync(comic.Image);
        }
    }

    private static Task Log(LogMessage message)
    {
        switch (message.Severity)
        {
            case LogSeverity.Error:
            case LogSeverity.Critical:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case LogSeverity.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case LogSeverity.Debug:
            case LogSeverity.Verbose:
                Console.ForegroundColor = ConsoleColor.Green;
                break;
            default:
            case LogSeverity.Info:
                Console.ForegroundColor = ConsoleColor.White;
                break;
        }

        Console.Write(message.Exception is null
            ? $"[{DateTime.Now.ToLongTimeString()}] [{message.Severity.ToString().ToUpper()}] {message.Message}\n"
            : $"[{DateTime.Now.ToLongTimeString()}] [{message.Severity.ToString().ToUpper()}] {message.Message} {message.Exception.ToString()}\n");

        return Task.CompletedTask;
    }
    
    public static Task Main() => MainAsync();
    
}