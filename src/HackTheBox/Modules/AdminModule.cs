using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using HackTheBox.Services;
using System.Diagnostics;

namespace HackTheBox.Modules
{
    [Name("Admin Module")]
    public class AdminModule: HTBModule
    {
        private readonly AdminService adminService;
        public AdminModule(AdminService service)
        {
            this.adminService = service;
        }

        [Command("say")]
        [Remarks("!say [message]")]
        [Summary("To write a message using bot")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Say([Remainder] string message)
        {
            await adminService.Say(Context.Channel as ITextChannel, message, Context.Message.Id);
        }

        [Command("die")]
        [Remarks("!die")]
        [Summary("Kill the bot")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Die()
        {
            await ReplyInfoAsync("Beep Boop I iz dying :pensive: \nSorry Senpai");
            System.Environment.Exit(0);
        }
   }
}
