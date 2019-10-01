using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace HackTheBox.Entities
{
    public class HTBUser
    {


        [JsonProperty("id")]
        public int UserId { get; set; }

        [JsonProperty("name")]
        public string Username { get; set; }

        [JsonProperty("username")]
        private string Username2
        {
            set
            {
                Username = value;

            }
        }
    }
}
