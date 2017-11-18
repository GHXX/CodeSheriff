using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using System.Linq;
using System.IO;
using DSharpPlus.Entities;

namespace CodeSheriff
{
    public class Commands
    {
        [Command("ignoreme"), Description("This command tells the bot to ingore your code")]
        public async Task IgnoreMeAsync(CommandContext ctx)
        {
            var db = ctx.Dependencies.GetDependency<Database>();
            var ignored = db.IgnoredUsers.FirstOrDefault(x => x.IgnoredUserId == ctx.Member.Id);
            if (ignored == null)
            {
                await db.IgnoredUsers.AddAsync(new IgnoredUsers()
                {
                    IgnoredUserId = ctx.Member.Id
                });
                await db.SaveChangesAsync();
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            else await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));
        }

        [Command("unignoreme"), Description("This command tells the bot to keep judging your code")]
        public async Task UnIgnoreMeAsync(CommandContext ctx)
        {
            var db = ctx.Dependencies.GetDependency<Database>();
            var ignored = db.IgnoredUsers.FirstOrDefault(x => x.IgnoredUserId == ctx.Member.Id);
            if (ignored != null)
            {
                db.IgnoredUsers.Remove(ignored);
                await db.SaveChangesAsync();
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            else await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));
        }

        [Command("add"), RequireOwnerOrMod, Description("Adds a keyword to the database")]
        public async Task AddAsync(CommandContext ctx, string keyword, params string[] reasons)
        {
            var db = ctx.Dependencies.GetDependency<Database>();
            var word = db.InvaildWords.FirstOrDefault(x => x.Keyword == keyword);
            if(word == null)
            {
                await db.InvaildWords.AddAsync(new InvaildWord()
                {
                    Keyword = keyword,
                    Reasons = reasons.ToList()
                });
                await db.SaveChangesAsync();
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            else await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));
        }

        [Command("remove"), RequireOwnerOrMod, Description("Removes a keyword from the database")]
        public async Task RemoveAsync(CommandContext ctx, string keyword)
        {
            var db = ctx.Dependencies.GetDependency<Database>();
            var word = db.InvaildWords.FirstOrDefault(x => x.Keyword == keyword);
            if (word != null)
            {
                db.InvaildWords.Remove(word);
                await db.SaveChangesAsync();
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
            }
            else await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":x:"));
        }

        [Command("shutdown"), RequireOwnerOrMod, Description("Shuts down the bot"), Hidden]
        public async Task ShutdownAsync(CommandContext ctx)
        {
            //Make the bot appear offline
            await ctx.Client.UpdateStatusAsync(user_status: UserStatus.Invisible);
            await ctx.Client.DisconnectAsync();
            Console.WriteLine("Shutting down...");
        }
    }

    public class RequireOwnerOrMod : CheckBaseAttribute
    {
        public override Task<bool> CanExecute(CommandContext ctx, bool help)
        {
            return Task.Run(() =>
            {
                var modroles = ctx.Member.Roles.Where(x => x.Permissions.HasPermission(Permissions.ManageGuild));
                if (modroles != null || ctx.Member.Id == ctx.Client.CurrentApplication.Owner.Id) return true;
                else return false;
            });
        }
    }
}
