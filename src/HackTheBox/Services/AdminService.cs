using System.Threading.Tasks;
using Discord;
using System.Diagnostics;

namespace HackTheBox.Services
{
    public class AdminService
    {
        public async Task Say(ITextChannel channel, string message, ulong prevMessageId)
        {
            await channel.DeleteMessageAsync(prevMessageId);
            await channel.SendMessageAsync(message);
        }
    }
}
