using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using HackTheBox.Entities;
using HackTheBox.Services;
using LiteDB;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace HackTheBox.Helpers
{
    public static class Extensions
    {

        private static Config _Config;
        private static readonly string GetBadgesJsonPath = "GetBadgeCollection.json";
        public static Config GetConfig()
        {
            var configStr = File.ReadAllText("config.json");
            var config = JsonConvert.DeserializeObject<Config>(configStr);
            return config;
        }

        public static HTBBadges FixBadgeCache() {
            File.WriteAllText(GetBadgesJsonPath, JsonConvert.SerializeObject(new HTBBadges() { HTBUserBadges = new System.Collections.Generic.List<HTBBadge>() }));
            var htbBadgesStr = File.ReadAllText(GetBadgesJsonPath);
            return JsonConvert.DeserializeObject<HTBBadges>(htbBadgesStr);
        }

        public static HTBBadges GetBadgeCollection()
        {
            try {
                if (File.Exists(GetBadgesJsonPath))
                {
                    var htbBadgesStr = File.ReadAllText(GetBadgesJsonPath);
                    var badges = JsonConvert.DeserializeObject<HTBBadges>(htbBadgesStr);
                    if( badges != null ) {
                        return badges;
                    }
                    else
                    {
                        return FixBadgeCache();
                    }
                }
                else
                {
                    return FixBadgeCache();
                }
            }

            catch {
                return FixBadgeCache(); // incase it contains garbage
            }
        }

        public static string GetTimeLeft(string release)
        {
            var releaseTime = DateTime.ParseExact(release.Replace("UTC", ""), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            return String.Format("{0} Day {1}:{2} Hours", releaseTime.Subtract(DateTime.UtcNow).Days, releaseTime.Subtract(DateTime.UtcNow).Hours, releaseTime.Subtract(DateTime.UtcNow).Minutes);
        }

        public static string HTBBoxDiffCalc(int points)
        {

            if (points >= 0 && points <= 40)
                return ":joy: Easy";
            else if (points > 40 && points <= 70)
                return ":spy: Medium";
            else if (points > 70 && points <= 90)
                return ":imp: Hard";
            else
                return ":sob: Insane";
        }
        public async static Task<HttpClient> GetHTBWebClient()
        {
            var docHandler = new HtmlAgilityPack.HtmlDocument();

            var _Config = GetConfig();

            var client = new HttpClient(new HttpClientHandler()
            {
                CookieContainer = new CookieContainer()
                {

                },
                UseCookies = true

            });

            docHandler.LoadHtml((await client.GetStringAsync("https://www.hackthebox.eu/login")));

            var postDataRaw = new Dictionary<string, string> {

                    { "email", _Config.HTBU },
                    { "password", _Config.HTBP },
                    { "_token",   docHandler.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='_token']").Attributes["value"].Value }

            };

            var postData = new FormUrlEncodedContent(postDataRaw);

            await client.PostAsync("https://www.hackthebox.eu/login", postData);

            return client;
        }
        public async static Task<List<HTBBox>> GetAllHTBMachines()
        {
            var _Config = GetConfig();
            //Fire a POST reuqest to get some basic info
            using (var _HTTPClient = new HttpClient())
            {

                HttpResponseMessage response = await _HTTPClient.GetAsync($"https://www.hackthebox.eu/api/machines/get/all?api_token={_Config.HTB_api}");
                if (response.IsSuccessStatusCode)
                {
                    //read as dynamic

                    return JsonConvert.DeserializeObject<List<HTBBox>>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    return null;
                }
            }

        }
        public async static Task<HTBUser> GetHTBUser(string username)
        {
            //Get Config
            var _Config = GetConfig();
            //Fire a POST reuqest to get some basic info
            using (var _HTTPClient = new HttpClient())
            {

                HttpResponseMessage response = await _HTTPClient.PostAsync($"https://www.hackthebox.eu/api/user/id?api_token={_Config.HTB_api}&username={username}", null);
                if (response.IsSuccessStatusCode)
                {
                    //read as dynamic

                    return JsonConvert.DeserializeObject<HTBUser>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    return null;
                }
            }
        }

        public async static Task<HTBBox> GetUnreleasedBox()
        {
            var docHandler = new HtmlAgilityPack.HtmlDocument();

            //using (var client = await GetHTBWebClient())
            var client = CommandHandlingService.client;


            var stringUnr = await client.GetStringAsync("https://www.hackthebox.eu/home/machines/unreleased");
            docHandler.LoadHtml(stringUnr);

            return await HTBBox.ParseHTBBox(docHandler);
        }

        public async static Task<Writeup> Get0xdfBoxWriteups(String boxname) {

            var docHandler = new HtmlAgilityPack.HtmlDocument();
            var client = CommandHandlingService.client;
            var stringUnr = await client.GetStringAsync("https://0xdf.gitlab.io/tags.html");
            docHandler.LoadHtml(stringUnr);
            var writeups = docHandler.DocumentNode.SelectNodes("/html/body/main/div/article/div/div/div[2]/ul[2]/a/li");
            var pattern =  $"HTB:[\\s]+{boxname}";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);

            foreach(var writeup in writeups) {
                MatchCollection m = r.Matches(writeup.InnerText);
                if( m.Count > 0 )
                {
                    var url = writeup.ParentNode.Attributes["href"].Value;
                    return new Writeup() {
                        URL = url,
                        boxName = boxname
                    };
                }

            }
            return null;
        }

        public async static Task<WriteupList> Get0xdfWriteups(String keyword) {

            var docHandler = new HtmlAgilityPack.HtmlDocument();
            var client = CommandHandlingService.client;
            var stringUnr = await client.GetStringAsync("https://0xdf.gitlab.io/tags.html");
            docHandler.LoadHtml(stringUnr);
            var writeups = docHandler.DocumentNode.SelectNodes("//h2");
            var pattern =  $"{keyword}";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);

            WriteupList writeUps = new WriteupList();
            writeUps.Writeups = new List<Writeup>();
            foreach(var writeup in writeups) {
                String tag;
                if ( writeup.Attributes["Id"] != null ) {
                    tag = writeup.Attributes["Id"].Value;
                }
                else {
                    continue;
                }

                String wurl;
                MatchCollection m = r.Matches(tag);
                if( m.Count > 0 )
                {
                    var URLs = writeup.NextSibling.NextSibling.SelectNodes(".//a");
                    foreach(var url in URLs) {
                        if ( url.Attributes["href"] != null ) {
                            wurl = url.Attributes["href"].Value;
                        }
                        else {
                            continue;
                        }
                        var writeUp = new Writeup() {
                            URL = wurl,
                            boxName = keyword
                        };
                        writeUps.Writeups.Add(writeUp);

                    }
                }

            }
            return writeUps;
        }

        public static string GetPrefix(this SocketCommandContext context)
        {
            var repository = new LiteRepository("hack.db");
            var config = GetConfig();

            var guild = repository.Query<DiscordServer>()
                .Where(g => g.Id == context.Guild.Id)
                .FirstOrDefault();

            var prefix = config.Prefix;

            if (guild != null)
                prefix = guild.Prefix;

            return prefix;
        }

        public static async Task SetBadgeCollection(HTBBadges urlCollection)
        {
            File.WriteAllText("GetBadgeCollection.json", JsonConvert.SerializeObject(urlCollection));
        }
    }
}
