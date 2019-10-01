
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace HackTheBox.Modules
{
    public abstract class HTBModule: ModuleBase<SocketCommandContext>
    {
        protected virtual async Task<IUserMessage> ReplyInfoAsync(string message)
        {
            return await ReplyAsync(":bell: " + message);
        }
        protected virtual async Task<IUserMessage> ReplyErrorAsync(string message)
        {
            return await ReplyAsync(":no_entry: " + message);
        }
    }
}
