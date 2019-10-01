using System.Threading.Tasks;
using Discord.Commands;
using System.Net;
using Discord;
using System.IO;
using System;
using System.Text;
using System.Security.Cryptography;
using System.Net.Http;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.NetworkInformation;
using HackTheBox.Entities;
using CoreHtmlToImage;
using LiteDB;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using MySql.Data.MySqlClient;

namespace HackTheBox.Modules
{
    [Name("User Module")]
    public class TestModule : HTBModule
    {
        //Global HTTPClient that should be used for all calls
        static HttpClient _HTTPClient = new HttpClient();
        static Config _Config = HackTheBox.Helpers.Extensions.GetConfig();
        static Random random = new Random();


        [Command("htbinfo")]
        [Remarks("!htbinfo")]
        [Summary("Get HackTheBox information")]
        // [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task htb()
        {
            string json = "";
            using (WebClient client = new WebClient())
            {
                json = client.DownloadString("https://www.hackthebox.eu/api/stats/overview");
            }

            var request = (HttpWebRequest)WebRequest.Create("https://www.hackthebox.eu/api/stats/global");
            var postData = "";
            var data = Encoding.ASCII.GetBytes(postData);
            request.Method = "POST";
            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            var dataObject = JsonConvert.DeserializeObject<dynamic>(json);
            var dataObject2 = JsonConvert.DeserializeObject<dynamic>(responseString);

            string users = dataObject.users.ToString();
            string machines = dataObject.machines.ToString();
            string challenges = dataObject.challenges.ToString();
            string online = dataObject2.data.sessions.ToString();
            string vpn = dataObject2.data.vpn.ToString();

            var builder = new EmbedBuilder()
            {
                Color = new Color(0xFD3439),
                Description = $":information_source: Hack The Box Infomation."
            };

            builder.AddField(x =>
                {
                    x.Name = "HTB Info";
                    x.Value = $"**Users**  : {users}\n" +
                              $"**Online Members**: {online}\n" +
                              $"**Connections**: {vpn}\n" +
                              $"**Machines**: {machines}\n" +
                              $"**Challenges**: {challenges}";

                    x.IsInline = false;
                });

            await ReplyAsync("", embed: builder.Build());

            response.Close();
        }

        [Command("uid")]
        [Remarks("!uid [username]")]
        [Summary("HTB UID")]

        public async Task uid([Remainder]string username = null)
        {

            if (username == null)
            {
                await ReplyAsync("Please specify a HTB user.");
                return;
            }
            var htbUser = await Helpers.Extensions.GetHTBUser(username);

            if (htbUser == null) {
                await ReplyAsync(":thinking: That user wasn't found");
                return;
            }

            var builder = new EmbedBuilder()
            {
                Color = new Color(0xFD3439),
                Description = $":information_source: HTB User Infomation."
            };

            builder.AddField(x =>
                {
                    x.Name = "HTB User Info";
                    x.Value = $"**Username**  : {htbUser.Username}\n" +
                              $"**id**: {htbUser.UserId}";

                    x.IsInline = false;
                });

            await ReplyAsync("", embed: builder.Build());


        }


        [Command("ippsec")]
        [Remarks("!ippsec [box_name]")]
        [Summary("Get ippsec's video link")]
        public async Task ippsec(string box = null)
        {
            if (box == null)
            {
                await ReplyAsync("Please specify a box name!");
                return;
            }

            string json = "";
            using (WebClient client = new WebClient())
            {
                json = client.DownloadString($"https://www.googleapis.com/youtube/v3/search?part=snippet&channelId=UCa6eh7gCkpPo5XXUDfygQQA&maxResults=1&q={box}&key={_Config.YT_key}");
            }

            var dataObject = JsonConvert.DeserializeObject<dynamic>(json);
            string title = dataObject.items.ToString();

            if (title != "[]")
            {
                string vid_id = dataObject.items[0].id.videoId.ToString();
                var url = $"https://www.youtube.com/watch?v={vid_id}";
                await ReplyAsync(url);
            }
            else
            {
                await ReplyAsync("Please specify a valid box name.");
            }
        }

        [Command("active")]
        [Remarks("!active")]
        [Summary("Get all active boxes name")]
        public async Task active()
        {
            var boxses = await Helpers.Extensions.GetAllHTBMachines();

            var builder = new EmbedBuilder()
            {
                Color = new Color(0xFD3439),
                Description = $":information_source: Active Boxes Infomation."
            };

            foreach (HTBBox hTBBox in boxses)
            {
                if (!hTBBox.retired)
                {
                    builder.AddField(x =>
                        {
                            x.Name = hTBBox.name;
                            x.Value = /*$"**Name**  : {name}\n" +
                                    $"**IP**  : {ip}\n" +
                                    $"**OS**  : {os}\n" +
                                    $"**Maker**  : {makerName}\n" +
                                    $"**Points**  : {points}\n" +
                                    $"**Release date **: {release}"*/
                                    $"**IP**  : {hTBBox.ip}\n";
                            x.IsInline = false;
                        });

                    //builder.WithThumbnailUrl($"{thumb}");

                }
            }
            await ReplyAsync("", embed: builder.Build());
        }


        [Command("boxinfo")]
        [Remarks("!boxinfo [box_name]")]
        [Summary("Get information for a box")]
        public async Task boxInfo(string boxName = null)
        {
            if(boxName == null) {
                await ReplyAsync("Enter a box name! :anger:");
                return;
            }
            var boxses = (await Helpers.Extensions.GetAllHTBMachines()).Where(x => x.name.ToLower().Equals(boxName.ToLower()));

            if (boxses.Count() > 0)
            {
                var box = boxses.FirstOrDefault();

                var builder = new EmbedBuilder()
                {
                    Color = new Color(0xFD3439),
                    Description = $":information_source: Box Infomation."
                };

                builder.AddField(x =>
                {
                    x.Name = box.name;
                    x.Value =
                                $"**IP**  : {box.ip}\n" +
                                $"**OS**  : {box.os}\n" +
                                $"**Maker**  : {box.maker.Username} " + ((box.maker2 != null) ? $" | {box.maker2.Username} \n" : "\n") +
                                $"**Points**  : {box.points}\n" +
                                $"**Ratings**  : {box.rating}\n" +
                                $"**User Owns**  : {box.user_owns}\n" +
                                $"**Root Owns**  : {box.root_owns}\n" +
                                $"**Release date **: {box.release}";
                    x.IsInline = false;
                });

                builder.WithThumbnailUrl($"{box.avatar_thumb}");
                await ReplyAsync("", embed: builder.Build());
            }

            else
                await ReplyAsync(":thinking: That box wasn't found brah");
        }

        [Command("getflags")]
        [Remarks("!getflags [box_name]")]
        [Summary("Get flags for any box")]
        public async Task getFlags(string box = null)
        {
            if (box == null)
            {
                await ReplyAsync("Hey I need a box name :( Don't you need flags?");
                return;
            }

            var builder = new EmbedBuilder()
            {
                Color = new Color(0xFD3439),
                Description = $":information_source: Get Free flag :)."
            };

            builder.AddField(x =>
            {
                x.Name = "Free flag";
                x.Value = $"**User flag**  : 1fe451905b80b52a17197d732f127c13\n" +
                        $"**Root flag **: 3436ed20e492972a93ba345271752301";
                x.IsInline = false;
            });

            builder.WithThumbnailUrl("https://i.kym-cdn.com/photos/images/newsfeed/000/096/044/trollface.jpg?1296494117");
            await ReplyAsync("", embed: builder.Build());
        }

        [Command("badge")]
        [Remarks("!badge [username]")]
        [Summary("User info")]

        public async Task whoami([Remainder]string username = null)
        {

            try {

                //Check if username is not null or whitespace
                if (String.IsNullOrWhiteSpace(username))
                {
                    await ReplyAsync("Please specify a HTB username.");
                    return;
                }

                var htbUser = await Helpers.Extensions.GetHTBUser(username);
                //Build badge
                var builder = new EmbedBuilder()
                {
                    Color = new Color(0xFD3439),
                    // Description = $"HTB User Infomation."
                };

                //Check if we already have an URL for this user
                var urlCollection = HackTheBox.Helpers.Extensions.GetBadgeCollection();

                if (urlCollection.HTBUserBadges?.Where(x => x.UserId.Equals(htbUser.UserId.ToString())
                && x.TimeStamp.Date > DateTime.Now.AddDays(-10)
                ).Count() > 0)
                {
                    //We already have his logo let's use that
                    builder.ImageUrl = urlCollection.HTBUserBadges.Where(y => y.UserId.Equals(htbUser.UserId.ToString())).FirstOrDefault().Url;
                    await ReplyAsync("", embed: builder.Build());
                }
                //If the picture is more then 10 days old, we will fetch an updated one
                else
                {
                    var request = await _HTTPClient.GetAsync($"https://www.hackthebox.eu/badge/{htbUser.UserId}");
                    var stringContentBuffer = await request.Content.ReadAsStringAsync();

                    stringContentBuffer = stringContentBuffer.Replace("document.write(window.atob(\"", "").Replace("\"))", "");

                    //Download stuff as base64 and decode
                    var badgeHTML = Encoding.UTF8.GetString(Convert.FromBase64String(stringContentBuffer));


                    //Replace stuff
                    badgeHTML = badgeHTML.Replace("https://www.hackthebox.eu/images/screenshot.png", "https://github.com/SkiddieTech/HTB-HDBadgeGenerator/raw/master/assets/htb_crosshair.png");
                    badgeHTML = badgeHTML.Replace("_thumb.png", ".png");
                    badgeHTML = badgeHTML.Replace("https://www.hackthebox.eu/images/star.png", "https://github.com/SkiddieTech/HTB-HDBadgeGenerator/raw/master/assets/htb_star.png");
                    badgeHTML = badgeHTML.Replace("url(https://www.hackthebox.eu/images/icon20.png); ", "url('https://github.com/SkiddieTech/HTB-HDBadgeGenerator/raw/master/assets/htb_logo.webp'); background-size: 20px;");

                    // ((\w+)px;)
                    //Scale everything x2
                    MatchCollection matchList = Regex.Matches(badgeHTML, @"((\w+)px;)");
                    var listOfSizes = matchList.Cast<Match>().Select(match => match.Value).ToList();

                    foreach (var size in listOfSizes.Distinct())
                    {
                        int factor = 0;

                        int.TryParse(new string(size
                         .SkipWhile(x => !char.IsDigit(x))
                         .TakeWhile(x => char.IsDigit(x))
                         .ToArray()), out factor);

                        var newSize = (factor * 2).ToString() + size.Replace(factor.ToString(), "");

                        badgeHTML = badgeHTML.Replace(size, newSize.ToString());
                    }

                    //Final touch
                    badgeHTML = badgeHTML.Replace("margin-top:20px; margin-left:20px;", "margin-top:10px; margin-left:20px;");
                    badgeHTML = badgeHTML.Replace("right 5px bottom 20px", "right 5px bottom 10px");
                    badgeHTML = badgeHTML.Replace("border-radius:8px;", "");

                    byte[] result;

                    using (var client = new WebClient())
                    {
                        string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(_Config.HCTI_Combos[random.Next(0, _Config.HCTI_Combos.Length)]));
                        client.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;

                        result = client.UploadValues(
                            "https://hcti.io/v1/image",
                            "POST",
                            new System.Collections.Specialized.NameValueCollection()
                            {
                            { "html", badgeHTML }
                            }
                        );
                    }


                    var dataResponse = JsonConvert.DeserializeObject<dynamic>(System.Text.Encoding.UTF8.GetString(result));
                    string url = dataResponse.url.ToString();

                    //Save this to "cache"
                    var badgeCache = new HTBBadge()
                    {
                        TimeStamp = DateTime.Now,
                        Url = url,
                        UserId = htbUser.UserId.ToString()
                    };
                    urlCollection.HTBUserBadges.Add(badgeCache);

                    //Save the cache
                    await HackTheBox.Helpers.Extensions.SetBadgeCollection(urlCollection);

                    builder.ImageUrl = url;
                    await ReplyAsync("", embed: builder.Build());
                }
            }

            catch
            {
                await ReplyErrorAsync("That user wasn't found, stop playing around!");
            }
        }

        [Command("unreleased")]
        [Remarks("!unreleased")]
        [Summary("View unreleased box information")]

        public async Task unreleasedBox()
        {
            using(var db = new LiteDB.LiteDatabase("hack.db")) {
                var boxes = db.GetCollection<HTBBox>("boxes");
                var box = boxes.FindById(1); // get and check if unreleased box is in DB
                if(box == null) {
                    try {
                        box = await Helpers.Extensions.GetUnreleasedBox();
                    }

                    catch {
                        await ReplyAsync("Sorry, no unreleased box yet :sob:");
                        return;
                    }
                    boxes.Insert(box); // save to DB if absent
                    var builder = new EmbedBuilder()
                    {
                        Color = new Color(0xFD3439),
                        Description = $":information_source: Unreleased boxes"
                    };

                    builder.AddField(x =>
                    {
                        x.Name = $"**Name**: {box.name}\n";
                        x.Value =
                                $"**Retiring**  : {box.retiredbox}\n" +
                                $"**OS**: {box.os}\n" +
                                $"**Difficulty**: {Helpers.Extensions.HTBBoxDiffCalc(box.points)}\n" +
                                $"**Maker**: {box.maker.Username}\n" +
                                $"**Time Left**: {Helpers.Extensions.GetTimeLeft(box.release)}";
                        x.IsInline = false;
                    });

                    builder.WithThumbnailUrl(box.avatar_thumb);

                    await ReplyAsync("", embed: builder.Build());
                }
                else {
                    var builder = new EmbedBuilder()
                    {
                        Color = new Color(0xFD3439),
                        Description = $":information_source: Unreleased boxes"
                    };

                    builder.AddField(x =>
                    {
                        x.Name = $"**Name**: {box.name}\n";
                        x.Value =
                                $"**Retiring**  : {box.retiredbox}\n" +
                                $"**OS**: {box.os}\n" +
                                $"**Difficulty**: {Helpers.Extensions.HTBBoxDiffCalc(box.points)}\n" +
                                $"**Maker**: {box.maker.Username}\n" +
                                $"**Time Left**: {Helpers.Extensions.GetTimeLeft(box.release)}";
                        x.IsInline = false;
                    });

                    builder.WithThumbnailUrl(box.avatar_thumb);

                    await ReplyAsync("", embed: builder.Build());
                }
            }
        }

        [Command("0xdf")]
        [Remarks("!0xdf [boxname]")]
        [Summary("0xdf blog")]

        public async Task df([Remainder]string keyword = null)
        {
            if( keyword == null ) {
                await ReplyAsync("What do you want me to search for?");
                return;
            }

            var builder = new EmbedBuilder()
            {
                Color = new Color(0xFD3439),
                Description = $":information_source: 0xdf WriteUps for loot!"
            };

            String URLs = null;
            var writeups = await Helpers.Extensions.Get0xdfWriteups(keyword);
            if ( writeups.Writeups == null)  {
                await ReplyAsync("Writeup for the keyword wasn't found! @0xdf Y U No Do Dis?");
                return;
            }
            else {
                foreach( var writeup in writeups.Writeups) {
                    URLs+= $"{writeup.URL}\n";
                }
            }

            if (URLs == null)
            {
                await ReplyAsync("Not found");
            }
            else
            {
                builder.AddField(x =>
                {
                    x.Name = keyword;
                    x.Value = URLs;
                    x.IsInline = false;
                });

                await ReplyAsync("", embed: builder.Build());
            }
        }


        [Command("writeup")]
        [Remarks("!writeup [boxname]")]
        [Summary("HTB write-ups from various users")]
        public async Task writeup([Remainder]string boxname = null)
        {
            if( boxname == null ) {
                await ReplyAsync("Specify a box name boi!");
                return;
            }

            var writeup = await Helpers.Extensions.Get0xdfBoxWriteups(boxname);
            if ( writeup == null)  {
                await ReplyAsync("Writeup for the box wasn't found!");
                return;
            }
            else {
                await ReplyAsync(writeup.URL);
            }
        }
    }
}
