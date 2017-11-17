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
using Newtonsoft.Json;

namespace CodeSheriff
{
    class Program
    {
        public static DiscordClient client;
        public static string token;
        public static readonly string commandPrefix = "sheriff!";
        public static Random rand = new Random(DateTime.UtcNow.Millisecond);
        public static ulong ID_CodeSheriff = 380781802639458315;
        readonly static string ignoredUsersDBPath =
            string.Join(Path.DirectorySeparatorChar, System.Reflection.Assembly.GetExecutingAssembly().Location.Split(Path.DirectorySeparatorChar).SkipLast(1))
            + string.Join(Path.DirectorySeparatorChar,new[] {"", "database", "ignoredUsers.dat" });

        public static List<ulong> ignoredUsers = new List<ulong>();

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
            client.DebugLogger.LogMessage(LogLevel.Info, "Bot", "Reading ignored users", DateTime.Now);
            if (File.Exists(ignoredUsersDBPath))
            {
                ignoredUsers = JsonConvert.DeserializeObject<List<ulong>>(File.ReadAllText(ignoredUsersDBPath));
            }

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
                if (e.Message.Content.StartsWith(commandPrefix) || e.MentionedUsers?.Any(x => x.Id == ID_CodeSheriff) == true)
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
                    else if (command == "ignoreme")
                    {
                        if (!ignoredUsers.Contains(e.Author.Id))
                        {
                            ignoredUsers.Add(e.Author.Id);
                            Directory.CreateDirectory(string.Join(Path.DirectorySeparatorChar, ignoredUsersDBPath.Split(Path.DirectorySeparatorChar).SkipLast(1)));
                            File.WriteAllText(ignoredUsersDBPath, JsonConvert.SerializeObject(ignoredUsers), Encoding.Default);
                            await e.Message.CreateReactionAsync(DiscordEmoji.FromName(e.Client, ":white_check_mark:"));
                        }
                        else
                        {
                            await e.Message.CreateReactionAsync(DiscordEmoji.FromName(e.Client, ":x:"));
                        }
                    }
                    else if (command == "unignoreme")
                    {
                        if (ignoredUsers.Contains(e.Author.Id))
                        {
                            ignoredUsers.RemoveAll(x => x == e.Author.Id);
                            Directory.CreateDirectory(string.Join(Path.DirectorySeparatorChar, ignoredUsersDBPath.Split(Path.DirectorySeparatorChar).SkipLast(1)));
                            File.WriteAllText(ignoredUsersDBPath, JsonConvert.SerializeObject(ignoredUsers), Encoding.Default);
                            await e.Message.CreateReactionAsync(DiscordEmoji.FromName(e.Client, ":white_check_mark:"));
                        }
                        else
                        {
                            await e.Message.CreateReactionAsync(DiscordEmoji.FromName(e.Client, ":x:"));
                        }
                    }
                    else if (command == "help")
                    {
                        await e.Message.RespondAsync("The following commands are available: `ignoreme`, `unignoreme`, `help`");
                    }
                }
                else if (!ignoredUsers.Contains(e.Author.Id))
                {
                    var single = Regex.Match(e.Message.Content.Replace("\\\"", "").Replace("```", "`").Replace("``", "`"), "`.*?`", RegexOptions.Singleline).Captures.Select(x => x.Value).Where(x => x.Any(y => y != '`')).Select(x => Regex.Replace(x, "\".*?\"", "\"\"", RegexOptions.Singleline));

                    string[] forbiddenWords = new[] { ".Result", "goto", "async void" };
                    var allSins = single.SelectMany(x => forbiddenWords.Where(y => x.Contains(y) && Regex.IsMatch(x, $"[^@]{y.Replace(".", @"\.")}" + @"([^a-zA-Z0-9_\(]|$)")));
                    //string forbidden = single.Where(x => forbiddenWords.Any(y => x.Contains(y) && Regex.IsMatch(x, $"{y.Replace(".", @"\.")}" + @"([^a-zA-Z0-9_]|$)"))).FirstOrDefault();
                    if (allSins.Count() > 0)
                    {
                        //string forbiddenWord = forbiddenWords.First(x => forbidden.Contains(x));
                        string suffix = "Here is a quick rundown on what could go wrong: ";

                        if (allSins.Contains("goto"))
                            suffix += "\n**goto**: \n\t►Causes the code to be messy and very difficult to follow. \n\t►Also includes deadlocks and stack corruption for free! :smiley_cat: ";

                        if (allSins.Contains(".Result"))
                            suffix += "\n**.Result**: \n\t►Deadlocks: It causes the running thread to schedule a task to continue on the same thread, which then waits for the task to finish. So they wait for each other.";

                        if (allSins.Contains("async void"))
                            suffix += "\n**async void**: \n\t►When an exception is thrown in async void, it's not propagated to the caller (as this information is no longer available by then) and instead crashes the entire runtime.";

                        var sinsListPre = allSins.Select(x => $"**{x}**");
                        string sinsList = string.Join(", ", sinsListPre.SkipLast(1)) + $"{(sinsListPre.Count() > 1 ? " & " : "")}{sinsListPre.Last()}";

                        await e.Message.RespondAsync($"Looks like you are using {(allSins.Count() == 1 ? "one" : "multiple")} forbidden keyword{(allSins.Count() == 1 ? "" : "s")} or method{(allSins.Count() == 1 ? "" : "s")} in your code. In this case {(allSins.Count() == 1 ? "it's" : "they are")} {sinsList}. You should get rid of {(allSins.Count() == 1 ? "it" : "them")} before _he_ sees it...\n" + suffix);

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