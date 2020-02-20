using CodeSheriff.DatabaseModel;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodeSheriff
{
    public class Program
    {
        private DiscordClient Client { get; set; }

        private CommandsNextExtension Commands { get; set; }

        public static void Main(string[] _)
        {
            Console.Title = "CodeSheriff";
            new Program().RunAsync().GetAwaiter().GetResult();
        }

        public async Task RunAsync()
        {
            //Create the Services for the bot
            var model = new Model();
            await model.Database.EnsureCreatedAsync();
            var deps = new ServiceCollection()
                .AddSingleton(model)
                .AddSingleton(new Random())
                .BuildServiceProvider();
            //We'll want to initialize our DiscordClient.
            Client = new DiscordClient(new DiscordConfiguration()
            {
                AutoReconnect = true, // Automatically reconnect on disconnect        
                LargeThreshold = 250, // Total number of members where the gateway will stop sending offline members in the guild member list
                LogLevel = LogLevel.Info, // Minimum log level you want to use
                Token = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Token.txt")), // Token
                TokenType = TokenType.Bot, // Token type.
                UseInternalLogHandler = true, // Use the internal log handler
            });
            Commands = Client.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "sheriff!", "sheriff! ", "s!", "s! " }, //Bot prefixes
                CaseSensitive = false, //None case sensitive commands
                Services = deps, //Set the dependencies
                EnableDms = false, //Disable commands in dms
                EnableDefaultHelp = true, //Enable the default help
                EnableMentionPrefix = true //Allow the bot mention to be used as a prefix
            });

            // First off, the MessageCreated event.
            Client.DebugLogger.LogMessage(LogLevel.Info, "Bot", "Initializing MessageCreated", DateTime.Now);
            Client.MessageCreated += Client_MessageCreatedAsync;
            Commands.CommandErrored += Commands_CommandErrored;

            //Register commands
            Commands.RegisterCommands<Commands>();

            // Client errored event
            Client.ClientErrored += (e) =>
            {
                Log.WriteLogMessage($"[Client Errored] \n\n{e.EventName}\n\n{e.Exception.ToString()}\n\n{e.Exception.StackTrace?.ToString()}\n\n{e.Exception.Message?.ToString()}\n\n{e.Exception.InnerException?.ToString()}\n\n{e.Exception.InnerException.InnerException?.ToString()}", LogLevel.Error);
                return Task.CompletedTask;
            };

            // The ready event
            Client.DebugLogger.LogMessage(LogLevel.Info, "Bot", "Initializing Ready", DateTime.Now);
            Client.Ready += async (e) =>
            {
                //I had to reinstall dsp and forgot what version you were using so just went to 3.2.3 stable. Feel free to edit - Li

                await Client.UpdateStatusAsync(new DiscordActivity($"people code on {e.Client.Guilds.Count} servers.", ActivityType.Watching), UserStatus.Online);
                Client.DebugLogger.LogMessage(LogLevel.Info, "Bot", "Ready!", DateTime.Now);
            };

            // Let's connect!
            Client.DebugLogger.LogMessage(LogLevel.Info, "Bot", "Connecting..", DateTime.Now);
            await Client.ConnectAsync();
            Client.DebugLogger.LogMessage(LogLevel.Info, "Bot", "Connected..", DateTime.Now);

            await Task.Delay(-1);
        }

        private Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            var db = this.Client.GetCommandsNext().Services.GetRequiredService<Model>();
            db.DbSemaphore.Release();
            Log.WriteLogMessage(e.Exception.Message, LogLevel.Critical);
            return Task.CompletedTask;
        }

        private async Task Client_MessageCreatedAsync(MessageCreateEventArgs e)
        {
            var db = Commands.Services.GetRequiredService<Model>();

            //If it's a bot return
            if (e.Message.Author.IsBot || e.Guild == null) return;
            var msg = e.Message.Content;

            //Check it is a code block
            if (!new Regex(@"```[\w]*\n[\s\S]*\n```").IsMatch(msg)) return;

            await db.DbSemaphore.WaitAsync();
            //Check that the author is being ignored
            var ignoreduser = db.IgnoredUsers.FirstOrDefault(x => x.GuildId == e.Guild.Id && x.UserId == e.Message.Author.Id);
            //If so bail out
            if (ignoreduser != null)
            {
                db.DbSemaphore.Release();
                return;
            }

            var detectedWords = new List<FlaggedWord>();
            foreach (var item in db.FlaggedWords.Where(x => x.GuildId == e.Guild.Id))
            {
                var word = item.Word.Replace(".", @"\.");
                Console.WriteLine(word);

                if (new Regex($@"(([^\/\/]|[^\/*][^""])({word})([^""]))").IsMatch(msg))
                    //If an invalid word is found, add it to the list
                    detectedWords.Add(item);
            }
            //If no words are detected, bail
            if (detectedWords.Count == 0)
            {
                db.DbSemaphore.Release();
                return;
            }

            var messageBuilder = new StringBuilder();
            var reasonBuilder = new StringBuilder();

            messageBuilder.Append("Looks like you are using one or more forbidden keywords in your code.");
            //Just grammatical stuff
            _ = (detectedWords.Count > 1) ? messageBuilder.AppendLine($" In this case they're: **{string.Join(", ", detectedWords.Select(x => x.Word))}**.\n") : messageBuilder.AppendLine($" In this case it's: **{detectedWords.First().Word}**.\n");
            messageBuilder.AppendLine("Here is a short list of what problems may be caused:");
            foreach (var item in detectedWords)
            {
                //Build the reasons
                reasonBuilder.AppendLine($"**{item.Word}**:");
                reasonBuilder.AppendLine(string.Join("\n", item.Reasons.Split(" | ").Select(x => "→ " + x)));
                reasonBuilder.AppendLine("");
            }
            //Append the reasons to the overall message
            messageBuilder.AppendLine(reasonBuilder.ToString());
            //Send that message to the channel
            await e.Channel.SendMessageAsync(messageBuilder.ToString());
            db.DbSemaphore.Release();
        }
    }
}
