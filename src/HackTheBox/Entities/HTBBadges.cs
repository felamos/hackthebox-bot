using System;
using System.Collections.Generic;
using System.Text;

namespace HackTheBox.Entities
{
    public class HTBBadges
    {
        public List<HTBBadge> HTBUserBadges { get; set; }

    }
    public class HTBBadge
    {
        public string UserId { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Url { get; set; }
    }
}
