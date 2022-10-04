using Discord;
using Discord.WebSocket;

namespace XkcdBot;

public static class Program
{

    private static DiscordSocketClient? _client;
    
    private static async Task MainAsync()
    {
        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.None
        };
        _client = new DiscordSocketClient(config);

        _client.Log += Log;
        
        var token = await File.ReadAllTextAsync("token.txt");

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

        await _client.CreateGlobalApplicationCommandAsync(command.Build());
    }

    private static async Task OnSlashCommandAsync(SocketSlashCommand command)
    {
        await command.DeferAsync();

        string query;
        if (command.Data.Options.Count is 0)
        {
            query = "standards";
        }
        else
        {
            query = command.Data.Options.First().Value as string ?? "1";
        }

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

    public static void LogInfo(string message, bool newLine = true)
    {
        if (!newLine)
        {
            Console.Write(message);
        }
        else
        {
            Log(new LogMessage(LogSeverity.Info, "", message));
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
            ? $"\n[{DateTime.Now.ToShortTimeString()}] [{message.Severity.ToString().ToUpper()}] {message.Message}"
            : $"\n[{DateTime.Now.ToShortTimeString()}] [{message.Severity.ToString().ToUpper()}] {message.Message} {message.Exception.ToString()}");

        return Task.CompletedTask;
    }
    
    public static Task Main() => MainAsync();
    
}