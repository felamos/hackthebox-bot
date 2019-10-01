using System.Threading.Tasks;
using System.Threading;
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
using HackTheBox.Entities;
using System.Xml;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using MySql.Data.MySqlClient;

namespace HackTheBox.Modules
{
    [Name("Account Module")]
    public class AccModule : HTBModule
    {
        static Config _Config = HackTheBox.Helpers.Extensions.GetConfig();

        [Command("register")]
        [Remarks("!register [token]")]
        [Summary("Register for some cool features")]
        public async Task register([Remainder]string token = null)
        {
            if( token == null || token.Length != 60) {
                await ReplyAsync("~register <api_token>");
                return;
            }

            try
            {
                string serverIp = _Config.MIP;
                string username = _Config.MU;
                string password = _Config.MP;
                string databaseName = _Config.MD;

                string discord_user = Context.User.Id.ToString();
                string MyConnection2 = string.Format("server={0};uid={1};pwd={2};database={3};", serverIp, username, password, databaseName);

                string Query = "insert into info(api, user_id) values('" +token+ "','" +discord_user+ "');";

                MySqlConnection MyConn2 = new MySqlConnection(MyConnection2);

                MySqlCommand MyCommand2 = new MySqlCommand(Query, MyConn2);
                MySqlDataReader MyReader2;
                MyConn2.Open();
                MyReader2 = MyCommand2.ExecuteReader();
                while (MyReader2.Read())
                {
                }
                MyConn2.Close();

                await Context.Message.DeleteAsync();
                await ReplyAsync("Successfully registered!");
            }
            catch
            {
                await Context.Message.DeleteAsync();
                await ReplyAsync("You are already registered!");
            }
        }

        [Command("reset")]
        [Remarks("!reset [box]")]
        [Summary("reset box")]

        public async Task reset([Remainder]string box = null)
        {
            if( box == null ) {
                await ReplyAsync("Box name!");
                return;
            }

            try
            {
                string discord_user = Context.User.Id.ToString();

                string serverIp = _Config.MIP;
                string username = _Config.MU;
                string password = _Config.MP;
                string databaseName = _Config.MD;

                string dbConnectionString = string.Format("server={0};uid={1};pwd={2};database={3};", serverIp, username, password, databaseName);
                string query = "SELECT * FROM info WHERE user_id = "+discord_user+";";
                //Console.Write(query);

                var conn = new MySql.Data.MySqlClient.MySqlConnection(dbConnectionString);
                conn.Open();

                var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                string token = "";
                while(reader.Read())
                {
                    token = reader.GetString(0);
                }

                var request = (HttpWebRequest)WebRequest.Create($"https://www.hackthebox.eu/api/shouts/new/?api_token={token}&text=/reset {box}");
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

                var dataObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                string success = dataObject.success.ToString();

                if (success == "0")
                {
                    await ReplyAsync("invalid Machine");
                }
                else
                {
                    string output = dataObject.output.ToString();
                    await ReplyAsync(output);
                    Thread.Sleep(134000);
                    await Context.Channel.SendMessageAsync(Context.Message.Author.Mention + $" issued a reset on {box}");
                }


            }
            catch
            {
                await ReplyAsync("You are not registered!");
            }

        }

        [Command("deleteacc")]
        [Remarks("!deleteacc")]
        [Summary("Delete your account")]

        public async Task deleteacc()
        {
            try
            {
                string discord_user = Context.User.Id.ToString();

                string serverIp = _Config.MIP;
                string username = _Config.MU;
                string password = _Config.MP;
                string databaseName = _Config.MD;

                try
                {
                    string dbConnectionString = string.Format("server={0};uid={1};pwd={2};database={3};", serverIp, username, password, databaseName);
                    string query = "SELECT * FROM info WHERE user_id = "+discord_user+";";
                    //Console.Write(query);

                    var conn = new MySql.Data.MySqlClient.MySqlConnection(dbConnectionString);
                    conn.Open();

                    var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
                    var reader = cmd.ExecuteReader();

                    string token = "";
                    while(reader.Read())
                    {
                        token = reader.GetString(0);
                    }

                    if (token.Length > 30)
                    {
                        string MyConnection2 = string.Format("server={0};uid={1};pwd={2};database={3};", serverIp, username, password, databaseName);
                        string Query = "DELETE FROM info WHERE user_id = "+discord_user+";";
                        MySqlConnection MyConn2 = new MySqlConnection(MyConnection2);

                        MySqlCommand MyCommand2 = new MySqlCommand(Query, MyConn2);
                        MySqlDataReader MyReader2;
                        MyConn2.Open();
                        MyReader2 = MyCommand2.ExecuteReader();

                        while (MyReader2.Read())
                        {
                        }
                        MyConn2.Close();

                        await ReplyAsync("Account deleted successfully!");
                    }
                    else
                    {
                        await ReplyAsync("You are not registered!");
                    }
                }
                catch
                {
                    await ReplyAsync("You are not registered!");
                }

            }
            catch
            {
                await ReplyAsync("You are not registered!");
            }
        }
    }
}
