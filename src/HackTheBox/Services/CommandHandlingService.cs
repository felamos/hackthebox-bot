using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HackTheBox.Entities;
using HackTheBox.Helpers;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Timers;

namespace HackTheBox.Services
{
    public class CommandHandlingService
    {
        private readonly DiscordSocketClient discord;
        private readonly CommandService commands;
        private readonly Config config;
        private IServiceProvider provider;
        private bool _10MinAlert = false;
        private bool _5MinAlert = false;
        private bool _2MinAlert = false;
        private bool _1MinAlert = false;
        public static HttpClient client = null;

        public CommandHandlingService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands, Config config)
        {
            this.discord = discord;
            this.commands = commands;
            this.config = config;
            this.provider = provider;

            this.discord.MessageReceived += this.MessageReceived;
        }

        public async Task InitializeAsync(IServiceProvider provider)
        {
            this.provider = provider;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), provider);
            // Add additional initialization code here...
            client = await Extensions.GetHTBWebClient();
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 10000;
            aTimer.Enabled = true;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            sendNotification().Wait();
        }

        private async Task MessageReceived(SocketMessage rawMessage)
        {
            // Ignore system messages and messages from bots
            if (!(rawMessage is SocketUserMessage message))
                return;
            if (message.Source != MessageSource.User)
                return;

            var context = new SocketCommandContext(discord, message);
            var prefix = context.GetPrefix();

            if (prefix == null)
                prefix = config.Prefix;

            int argPos = 0;
            // if (!message.HasMentionPrefix(_discord.CurrentUser, ref argPos)) return;
            if (!message.HasStringPrefix(prefix, ref argPos))
                return;

            await commands.ExecuteAsync(context, argPos, provider);
        }

        public async Task sendNotification()
        {
            if ((String.Equals(System.DateTime.Now.DayOfWeek.ToString(), "Saturday") && DateTime.UtcNow.Hour == 18))
            {
                //Get's the unreleasedBox as HTBBox object
                //HTBBox newBox = await Helpers.Extensions.GetUnreleasedBox();

               try {
                    using (var db = new LiteDB.LiteDatabase("hack.db")) {
                        var boxes = db.GetCollection<HTBBox>("boxes");
                        var newBox = boxes.FindById(1);
                        ulong id = 588067165593141249;
                        var chnl = discord.GetChannel(id) as IMessageChannel; // 4

                        Console.WriteLine(newBox.release.ToString() + " " + DateTime.UtcNow);

                        //Check if it is less then 10 minutes to release
                        if (DateTime.Parse(newBox.release) <= DateTime.UtcNow.AddMinutes(10) && _10MinAlert == false)
                        {
                            await chnl.SendMessageAsync("Announcement!\n" + "@everyone " + newBox.name + " Releases in 10 minutes!"); //
                            _10MinAlert = true;
                        }
                        //Check if it is less then 5 minutes to release
                        else if (DateTime.Parse(newBox.release) <= DateTime.UtcNow.AddMinutes(5) && _5MinAlert == false)
                        {
                            await chnl.SendMessageAsync("Announcement!\n" + "@everyone " + newBox.name + " Releases in 5 minutes!"); //
                            _5MinAlert = true;
                        }
                        //Check if it is less then 2 minutes to release
                        else if (DateTime.Parse(newBox.release) <= DateTime.UtcNow.AddMinutes(2) && _2MinAlert == false)
                        {
                            await chnl.SendMessageAsync("Announcement!\n" + "@everyone " + newBox.name + " Releases in 2 minutes!"); //
                            _2MinAlert = true;
                        }
                        //Check if it is less then 1 minutes to release
                        else if (DateTime.Parse(newBox.release) <= DateTime.UtcNow.AddMinutes(1) && _1MinAlert == false)
                        {
                            await chnl.SendMessageAsync("Announcement!\n" + "@everyone " + newBox.name + " Releases in 1 minute!"); //
                            _1MinAlert = true;
                        }
                        //Check if box released
                        else if(DateTime.Parse(newBox.release) < DateTime.UtcNow) {
                            await chnl.SendMessageAsync("Announcement!\n" + newBox.name + " is up and running boiz!"); //
                            _10MinAlert = _5MinAlert = _2MinAlert = _1MinAlert = false;
                            // box released, remove from db
                            boxes.Delete(1);
                        }

                    }
               }

                catch {
                    Console.WriteLine("boo");
                }


            }
        }

    }
}
