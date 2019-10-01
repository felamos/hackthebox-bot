
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using LiteDB;
using System.Collections.Generic;
using HackTheBox.Entities;
using HackTheBox.Helpers;

namespace HackTheBox.Modules
{
    [Name("Help Module")]
    public class HelpModule: HTBModule
    {
        private readonly CommandService _service;
        private readonly Config _config;
        private readonly LiteRepository _repository;

        public HelpModule(CommandService service, Config config, LiteRepository repository)
        {
            _service = service;
            _config = config;
            _repository = repository;
        }

        private async Task Help()
        {
            var prefix  = this.Context.GetPrefix();

            if (prefix == null) prefix = _config.Prefix;

            var builder = new EmbedBuilder()
            {
                Color = new Color(0xFD3439),
                Description = ":closed_book: Here are all available commands"
            };

            var embedList = new List<Embed>();

            foreach (var module in _service.Modules)
            {
                string description = "";
                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                    {
                        description += $"**{prefix}{cmd.Aliases.FirstOrDefault()}**\n";
                        description += cmd.Summary + "\n\n";
                    }
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });

                    if (embedList.Count == 0)
                    {
                        embedList.Add(builder.Build());
                        continue;
                    }

                    var moduleCommands = new EmbedBuilder()
                    {
                        Color = new Color(0xFD3439),
                    };

                    moduleCommands.AddField(builder.Fields.Last());
                    embedList.Add(moduleCommands.Build());
                }
            }

            foreach (var embed in embedList)
                await ReplyAsync(embed: embed);
        }

        [Command("help")]
        [Remarks("!help")]
        [Summary("Prints this help message.")]
        public async Task Help([Remainder] string command = null)
        {
            if (command == null)
            {
                await Help();
                return;
            }

            var result = _service.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($":expressionless: I couldn't find that command.");
                return;
            }

            var builder = new EmbedBuilder()
            {
                Color = new Color(0xFD3439),
                Description = $":closed_book: Here are some commands like **{command}**"
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = $"Usage  : {cmd.Remarks}\n" +
                              $"Summary: {cmd.Summary}";

                    x.IsInline = false;
                });
            }

            await ReplyAsync("", embed: builder.Build());
        }
    }
}
