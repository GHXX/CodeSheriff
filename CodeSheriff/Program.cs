using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using System.Text.RegularExpressions;   
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;

namespace CodeSheriff
{
    public class Program
    {
        public Random rand = new Random();
        private DiscordClient _client { get; set; }
        private CommandsNextModule _commands {get; set;}
        private Log _log = new Log();

        //Instead of your mess, make it an async main so we can run everything async
        public static void Main(string[] args)
        {
            Console.Title = "CodeSheriff";
            new Program().RunAsync().GetAwaiter().GetResult();
        }

        public async Task RunAsync()
        {
            //Create the Services for the bot
            var deps = new DependencyCollectionBuilder()
                .AddInstance(new Database())
                .Build();
            //We'll want to initialize our DiscordClient.
            _client = new DiscordClient(new DiscordConfiguration()
            {
                AutoReconnect = true, // Automatically reconnect on disconnect        
                LargeThreshold = 250, // Total number of members where the gateway will stop sending offline members in the guild member list
                LogLevel = LogLevel.Info, // Minimum log level you want to use
                Token = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Token.txt")), // Token
                TokenType = TokenType.Bot, // Token type.
                UseInternalLogHandler = true, // Use the internal log handler
            });
            _commands = _client.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefix = "sheriff! ", //Bot prefix
                CaseSensitive = false, //None case sensitive commands
                Dependencies = deps, //Set the dependencies
                EnableDms = false, //Disable commands in dms
                EnableDefaultHelp = true, //Enable the default help
                EnableMentionPrefix = true //Allow the bot mention to be used as a prefix
            });

            // First off, the MessageCreated event.
            _client.DebugLogger.LogMessage(LogLevel.Info, "Bot", "Initializing MessageCreated", DateTime.Now);
            _client.MessageCreated += _client_MessageCreatedAsync;

            // Client errored event
            _client.ClientErrored += (e) =>
            {
                _log.WriteLogMessage($"[_client Errored] \n\n{e.EventName}\n\n{e.Exception.ToString()}\n\n{e.Exception.StackTrace?.ToString()}\n\n{e.Exception.Message?.ToString()}\n\n{e.Exception.InnerException?.ToString()}\n\n{e.Exception.InnerException.InnerException?.ToString()}", LogOutputLevel.Error);
                return Task.CompletedTask;
            };

            // The ready event
            _client.DebugLogger.LogMessage(LogLevel.Info, "Bot", "Initializing Ready", DateTime.Now);
            _client.Ready += async (e) =>
            {
                //I had to reinstall dsp and forgot what version you were using so just went to 3.2.3 stable. Feel free to edit - Li
                await _client.UpdateStatusAsync(new DiscordGame("with code."), UserStatus.Online);
                _client.DebugLogger.LogMessage(LogLevel.Info, "Bot", "Ready!", DateTime.Now);
            };

            // Let's connect!
            _client.DebugLogger.LogMessage(LogLevel.Info, "Bot", "Connecting..", DateTime.Now);
            await _client.ConnectAsync();
            _client.DebugLogger.LogMessage(LogLevel.Info, "Bot", "Connected..", DateTime.Now);

            //Keep the task alive
            await Task.Delay(-1);
        }

        private async Task _client_MessageCreatedAsync(MessageCreateEventArgs e)
        {
            //If it's a bot return
            if (e.Message.Author.IsBot) return;
            var msg = e.Message.Content;
            //If the message does not contain a code block, return
            var db = _commands.Dependencies.GetDependency<Database>();
            //Check that the author is being ignored
            var ignoreduser = db.IgnoredUsers.FirstOrDefault(x => x.GuildId == e.Guild.Id && x.IgnoredUserId == e.Message.Author.Id);
            //If so bail out
            if (ignoreduser != null) return;
            if (!new Regex(@"```[\w]*\n[\s\S]*\n```").IsMatch(msg)) return;
            var detectedWords = new List<InvaildWord>();
            foreach(var word in db.InvaildWords)
            {
                if (word.Keyword.Contains(".")) word.Keyword.Replace(".", @"\.");
                if (new Regex($@"(([^\/\/]|[^\/*][^""])({word.Keyword})([^""]))").IsMatch(msg))
                    //If an invalid word is found, add it to the list
                    detectedWords.Add(word);
             }
            var messageBuilder = new StringBuilder();
            var reasonBuilder = new StringBuilder();
            messageBuilder.Append("Looks like you are using one or more forbidden keywords in your code.");
            //Just grammatical stuff
            if (detectedWords.Count > 1) messageBuilder.AppendLine($" In this case they're: **{string.Join(", ", detectedWords.Select(x => x.Keyword))}**.\n");
            else messageBuilder.AppendLine($"In this case its: **{detectedWords.First().Keyword}**.\n");
            messageBuilder.AppendLine("Here is a short list of what problems may be caused:");
            foreach (var word in detectedWords)
            {
                //Build the reasons
                reasonBuilder.AppendLine($"__{word.Keyword}__");
                reasonBuilder.AppendLine(string.Join("\n", word.Reasons.Select(x => x)));
                reasonBuilder.AppendLine("");
            }
            //Append the reasons to the overall message
            messageBuilder.AppendLine(reasonBuilder.ToString());
            //Send that message to the channel
            await e.Channel.SendMessageAsync(messageBuilder.ToString());
        }
    }
}
