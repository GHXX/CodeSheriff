using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using DSharpPlus.Entities;
using System.Text.RegularExpressions;

namespace CodeSheriff
{
    class Program
    {
        public static DiscordClient client;
        public static string token;
        public static readonly string commandPrefix = "sheriff!";
        public static Random rand = new Random(DateTime.UtcNow.Millisecond);
        public static ulong ID_CodeSheriff = 380781802639458315;

        static void Main(string[] args)
        {
            token = "MzgwNzgxODAyNjM5NDU4MzE1.DO9mNQ.3hsYFyJ89lclffJo5geqS-AAThs";
            // First we'll want to initialize our DiscordClient.

            client = new DiscordClient(new DiscordConfiguration()
            {
                AutoReconnect = true, // Whether you want DSharpPlus to automatically reconnect                
                //DiscordBranch =  Branch.Stable, // API branch you want to use. Stable is recommended!
                LargeThreshold = 250, // Total number of members where the gateway will stop sending offline members in the guild member list
                LogLevel = LogLevel.Info, // Minimum log level you want to use
                Token = token, // Your token
                TokenType = TokenType.Bot, // Your token type. Most likely "Bot"
                UseInternalLogHandler = true, // Whether you want to use the internal log handler
            });
            Console.Title = "CSharp CodeSheriff";

            // Now we'll want to define our events
            client.DebugLogger.LogMessage(LogLevel.Info, "Bot", "Initializing events", DateTime.Now);

            // First off, the MessageCreated event.
            client.DebugLogger.LogMessage(LogLevel.Info, "Bot", "Initializing MessageCreated", DateTime.Now);

            client.MessageCreated += Client_MessageCreatedAsync;

            client.ClientErrored += async (e) =>
            {
                await Task.Yield();
                Log.WriteLogMessage($"[Client Errored] \n\n{e.EventName}\n\n{e.Exception.ToString()}\n\n{e.Exception.StackTrace?.ToString()}\n\n{e.Exception.Message?.ToString()}\n\n{e.Exception.InnerException?.ToString()}\n\n{e.Exception.InnerException.InnerException?.ToString()}", LogOutputLevel.Error);
            };

            // ChannelCreated event
            client.DebugLogger.LogMessage(LogLevel.Info, "Bot", "Initializing ChannelCreated", DateTime.Now);

            // Last but not least, the ready event
            client.DebugLogger.LogMessage(LogLevel.Info, "Bot", "Initializing Ready", DateTime.Now);


            client.Ready += async (e) =>
            {
                await client.UpdateStatusAsync(new DiscordActivity("people code.", ActivityType.Watching), UserStatus.Online);

                client.DebugLogger.LogMessage(LogLevel.Info, "Bot", "Ready! Setting status message..", DateTime.Now);
            };

            // Now we'll boot up Voice Next
            client.DebugLogger.LogMessage(LogLevel.Info, "Bot", "Starting Voice Next", DateTime.Now);

            // Let's connect!
            client.DebugLogger.LogMessage(LogLevel.Info, "Bot", "Connecting..", DateTime.Now);

            client.ConnectAsync();
            // Make sure to not automatically close down
            string concmd = "";

            client.DebugLogger.LogMessage(LogLevel.Info, "Bot", "Connected..", DateTime.Now);

            while (true)
            {
                concmd = Console.ReadLine();
                try
                {
                    switch (concmd.ToLower())
                    {
                        case "exit": Shutdown(); return;
                        case "reconnect": client.ReconnectAsync(); break;

                        default: Console.WriteLine("Unknown command."); break;
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLogMessage("ERROR: " + ex.ToString(), LogOutputLevel.Error);
                    //throw ex;
                }
            }
        }

        private static async Task Client_MessageCreatedAsync(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            if (!e.Message.Author.IsBot)
            {
                if (e.Message.Content.StartsWith(commandPrefix) || e.MentionedUsers.Any(x => x.Id == ID_CodeSheriff))
                {
                    string command = string.Join(" ", e.Message.Content.Split(' ').Skip(1)).ToLowerInvariant();
                    var member = await e.Guild.GetMemberAsync(e.Author.Id);
                    if ((command == "exit" || command == "shutdown") 
                        && (member.Roles.Any(x => x.CheckPermission(Permissions.KickMembers) == PermissionLevel.Allowed 
                            || x.CheckPermission(Permissions.Administrator) == PermissionLevel.Allowed) || member.Id == 109262887310065664
                            )
                       )
                    {
                        await client.DisconnectAsync();
                        Environment.Exit(0);
                    }
                }
                else
                {
                    var single = Regex.Match(e.Message.Content.Replace("```", "`").Replace("``", "`"), "`.*?`", RegexOptions.Singleline).Captures.Select(x => x.Value).Where(x => x.Any(y => y != '`'));

                    string[] forbiddenWords = new[] { ".Result", "goto" };
                    string forbidden = single.Where(x => forbiddenWords.Any(y => x.Contains(y))).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(forbidden))
                    {
                        string forbiddenWord = forbiddenWords.First(x => forbidden.Contains(x));
                        string suffix = "";
                        if (forbiddenWord == "goto")
                        {
                            suffix = "Here is a short list of what problems may be caused: \n►Makes the code messy and very difficult to follow. \n►Also includes deadlocks and stack corruption for free! :smiley_cat: ";
                        }
                        else if (forbiddenWord == ".Result")
                        {
                            suffix = "Here is a short list of what problems may be caused: \n►Deadlocks: It causes the running thread to schedule a task to continue on the same thread, which then waits for the task to finish. So they wait for each other.";
                        }
                        await e.Message.RespondAsync($"Looks like you are using a forbidden keyword or method in your code. In this case its **{forbiddenWord}**. You should get rid of it before _he_ sees it...\n"+suffix);
                    }
                }
            }
        }

        internal static void Shutdown()
        {
            client.DisconnectAsync();
            Console.WriteLine("Shutting down. This may take up to 30 seconds.");
        }
    }
}