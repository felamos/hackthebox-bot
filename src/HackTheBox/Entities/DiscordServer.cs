using LiteDB;

namespace HackTheBox.Entities
{
    public class DiscordServer
    {
        [BsonId]
        public ulong Id { get; set; }
        public string Prefix { get; set; }
    }
}
