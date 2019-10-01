using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace HackTheBox.Entities
{
    public class HTBBoxExtra : HTBBox
    {

    }
    public class HTBBox
    {
        public static async Task<HTBBox> ParseHTBBox(HtmlDocument docHandler)
        {
            var newbox = docHandler.DocumentNode.SelectSingleNode("/html/body/div[2]/section/div/div[2]/div/div/table/tbody/tr/td[1]/a").InnerHtml;
            var oldbox = docHandler.DocumentNode.SelectSingleNode("/html/body/div[2]/section/div/div[2]/div/div/table/tbody/tr/td[2]/a").InnerHtml;
            var OS = docHandler.DocumentNode.SelectSingleNode("/html/body/div[2]/section/div/div[2]/div/div/table/tbody/tr/td[3]").InnerHtml;
            var difficulty = docHandler.DocumentNode.SelectSingleNode("/html/body/div[2]/section/div/div[2]/div/div/table/tbody/tr/td[4]/div/div/span").InnerHtml;
            var diff = Int32.Parse(difficulty.Remove(difficulty.Length - 1));
            var maker = docHandler.DocumentNode.SelectSingleNode("/html/body/div[2]/section/div/div[2]/div/div/table/tbody/tr/td[7]/a").InnerHtml;
            var timeLeft = docHandler.DocumentNode.SelectSingleNode("/html/body/div[2]/section/div/div[2]/div/div/table/tbody/tr/td[6]").InnerHtml;
            var avatar = docHandler.DocumentNode.SelectSingleNode("/html/body/div[2]/section/div/div[2]/div/div/table/tbody/tr/td[1]/noscript/img").Attributes["src"].Value;
            //DateTime releaseDate = DateTime.ParseExact(timeLeft.Replace("UTC", "+0"), "yyyy-MM-dd HH:mm:ss z", CultureInfo.InvariantCulture);
            DateTime releaseDate = DateTime.Parse(timeLeft.Replace("UTC", ""));

            return new HTBBox()
            {
                Id = 1,
                name = newbox,
                os = (OS.Contains("win")) ? "Windows" : "Linux",
                points = diff,
                //Difficulty = diffcultRating,
                avatar_thumb = avatar,
                maker = new HTBUser() { Username = maker },
                //ReleaseDate = releaseDate,
                release = releaseDate.ToString("yyyy-MM-dd HH:mm:ss"),
                retiredbox = oldbox,
                // TimeLeft = String.Format("{0} Day {1}:{2} Hours", releaseTime.Subtract(DateTime.UtcNow).Days, releaseTime.Subtract(DateTime.UtcNow).Hours, releaseTime.Subtract(DateTime.UtcNow).Minutes)
            };
        }
        public int Id { get; set; }
        public string name { get; set; }
        public string os { get; set; }
        public string ip { get; set; }
        public string avatar_thumb { get; set; }
        public int points { get; set; }
        public string release { get; set; }
        public string retired_date { get; set; }
        public HTBUser maker { get; set; }
        public HTBUser maker2 { get; set; }
        public int ratings_pro { get; set; }
        public String rating { get; set; }
        public int ratings_sucks { get; set; }
        public int user_owns { get; set; }
        public int root_owns { get; set; }
        public bool retired { get; set; }
        public bool free { get; set; }
        public string retiredbox { get; set; }
    }
}
