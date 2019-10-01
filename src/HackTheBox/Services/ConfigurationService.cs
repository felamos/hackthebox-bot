
using System.Threading.Tasks;
using Discord;
using LiteDB;
using HackTheBox.Entities;

namespace HackTheBox.Services
{
    public class ConfigurationService
    {
        private readonly LiteRepository _repository;
        public ConfigurationService(LiteRepository repository)
        {
            this._repository  = repository;
        }

        public Task SetPrefix(IGuild guild, string prefix)
        {
            var _guild = new DiscordServer()
            {
                Id = guild.Id,
                Prefix = prefix
            };

            _repository.Upsert(_guild);
            return Task.FromResult(0);
        }
    }
}
