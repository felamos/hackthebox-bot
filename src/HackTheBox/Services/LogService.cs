using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace HackTheBox.Services
{
    public class LogService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;

        public LogService(DiscordSocketClient discord, CommandService commands)
        {
            _discord = discord;
            _commands = commands;

            _discord.Log += LogDiscord;
            _commands.Log += LogCommand;
        }

        private Task LogDiscord(LogMessage message)
        {
            System.Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

        private Task LogCommand(LogMessage message)
        {
            // Return an error message for async commands
            if (message.Exception is CommandException command)
                // Don't risk blocking the logging task by awaiting a message send; ratelimits!?
                command.Context.Channel.SendMessageAsync($":expressionless: {command.InnerException.Message}");

            System.Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }
    }
}
