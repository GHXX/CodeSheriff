using System;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using System.Linq;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using CodeSheriff.DatabaseModel;

namespace CodeSheriff
{
    public class Commands : BaseCommandModule
    {
        [Command("ignoreme"), Description("This command tells the bot to ingore your code")]
        public async Task IgnoreMeAsync(CommandContext ctx)
        {
            var db = ctx.Services.GetRequiredService<Model>();
            await db.DbSemaphore.WaitAsync();
            var ignored = db.IgnoredUsers.FirstOrDefault(x => x.UserId == ctx.Member.Id && x.GuildId == ctx.Guild.Id);
            if (ignored == null)
            {
                db.IgnoredUsers.Add(new IgnoredUser()
                {
                    UserId = ctx.Member.Id,
                    GuildId = ctx.Guild.Id
                });
                await db.SaveChangesAsync();
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            else await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));
            db.DbSemaphore.Release();
        }

        [Command("unignoreme"), Description("This command tells the bot to keep judging your code")]
        public async Task UnIgnoreMeAsync(CommandContext ctx)
        {
            var db = ctx.Services.GetRequiredService<Model>();
            await db.DbSemaphore.WaitAsync();
            var ignored = db.IgnoredUsers.FirstOrDefault(x => x.UserId == ctx.Member.Id && x.GuildId == ctx.Guild.Id);
            if (ignored != null)
            {
                db.IgnoredUsers.Remove(ignored);
                await db.SaveChangesAsync();
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            else await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));
            db.DbSemaphore.Release();
        }

        [Command("add"), RequireOwnerOrMod, Description("Adds a keyword to the file.")]
        public async Task AddKeywordAsync(CommandContext ctx, string _keyword, [RemainingText] string reasons)
        {
            var db = ctx.Services.GetRequiredService<Model>();
            await db.DbSemaphore.WaitAsync();
            var word = db.FlaggedWords.FirstOrDefault(x => x.Word == _keyword && x.GuildId == ctx.Guild.Id);
            if (word == null)
            {
                db.FlaggedWords.Add(new FlaggedWord()
                {
                    GuildId = ctx.Guild.Id,
                    Word = _keyword,
                    Reasons = reasons
                });
                await db.SaveChangesAsync();
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            else await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));
            db.DbSemaphore.Release();
        }

        [Command("remove"), RequireOwnerOrMod, Description("Removes a keyword from the database")]
        public async Task RemoveKeywordAsync(CommandContext ctx, string keyword)
        {
            var db = ctx.Services.GetRequiredService<Model>();
            await db.DbSemaphore.WaitAsync();
            var word = db.FlaggedWords.FirstOrDefault(x => x.Word == keyword && x.GuildId == ctx.Guild.Id);
            if (word.Word != null)
            {
                db.FlaggedWords.Remove(word);
                await db.SaveChangesAsync();
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            else await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));
            db.DbSemaphore.Release();
        }

        [Command("shutdown"), Aliases("exit"), RequireOwner, Description("Shuts down the bot"), Hidden]
        public async Task ShutdownBotAsync(CommandContext ctx)
        {
            Console.WriteLine("Shutting down...");
            await ctx.Client.UpdateStatusAsync(userStatus: UserStatus.Offline);
            var db = ctx.Services.GetRequiredService<Model>();
            await db.DbSemaphore.WaitAsync();
            await db.SaveChangesAsync();
            db.DbSemaphore.Release();

            await ctx.Client.UpdateStatusAsync(userStatus: UserStatus.Invisible);
            await ctx.Client.DisconnectAsync();
        }

        [Command("serverinfo"), Aliases("sinfo", "guildinfo", "ginfo", "server", "guild")]
        public async Task ServerInfoAsync(CommandContext ctx) => await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
        {
            Author = new DiscordEmbedBuilder.EmbedAuthor()
            {
                Name = ctx.Guild.Name,
                IconUrl = ctx.Guild.IconUrl
            },
            Color = DiscordColor.Aquamarine,
            Timestamp = DateTime.Now,
            Description = $"**Members**: {ctx.Guild.MemberCount}\n**Role Count**: {ctx.Guild.Roles.Count}\n**Mfa Level**: {ctx.Guild.MfaLevel}\n**Owner**:{ctx.Guild.Owner.Mention}"
        });
    }

    public class RequireOwnerOrMod : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            return Task.Run(() =>
            {
                //Maybe change this so users can assign a specific mod role instead of having a single perm check
                var IsMod = ctx.Member.Roles.Any(x => x.Permissions.HasPermission(Permissions.ManageChannels));
                var IsBotOwner = ctx.Client.CurrentApplication.Owners.FirstOrDefault(x => x.Id == ctx.Member.Id) != null;
                return (IsMod || IsBotOwner);
            });
        }
    }
}
