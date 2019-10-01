using System.Collections.Generic;
using Newtonsoft.Json;

namespace HackTheBox.Entities
{
    public class Config
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("prefix")]
        public string Prefix { get; set; }

        [JsonProperty("status")]
        public string GameStatus { get; set; }

        [JsonProperty("owners")]
        public List<ulong> Owners { get; set; }

        [JsonProperty("invite")]
        public string InviteUrl { get; set; }

        [JsonProperty("htb_api")]
        public string HTB_api { get; set; }

        [JsonProperty("hcti_combos")]
        public string[] HCTI_Combos { get; set; }

        [JsonProperty("yt_key")]
        public string YT_key { get; set; }

        [JsonProperty("mysql_ip")]
        public string MIP { get; set; }

        [JsonProperty("mysql_user")]
        public string MU { get; set; }

        [JsonProperty("mysql_pass")]
        public string MP { get; set; }

        [JsonProperty("mysql_db")]
        public string MD { get; set; }

        [JsonProperty("htb_username")]
        public string HTBU { get; set; }

        [JsonProperty("htb_password")]
        public string HTBP { get; set; }
    }
}
