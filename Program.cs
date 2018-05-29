using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using QueryMaster.GameServer;
using System.Text.RegularExpressions;
using ConsoleTables;
using ColoredConsole;
using System.Net;

namespace LKD_ASQ
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WindowHeight = 60;
            Console.WindowWidth = 130;

            #region api
            bool api = false;
            string staticinput = "";
            string format = "";
            string[] cla = Environment.GetCommandLineArgs();
            if (cla.Length > 2)
            {
                api = true;
                format = cla[1];
                staticinput = String.Join(" ", cla.Skip(2));
                if (new[] { "json", "csv" }.Any(c => format.ToLower() == c))
                {

                }
                else
                {
                    Console.WriteLine("Unknown format for API. Please use 'json', or 'csv'");
                    Environment.Exit(-1);
                }
            }
            #endregion


            #region Load JSON data
            arkserversjson serverlist = null;
            if (File.Exists(@"servers.json"))
            {
                string json = File.ReadAllText(@"servers.json");
                try
                {
                    serverlist = JsonConvert.DeserializeObject<arkserversjson>(json);
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occured whilst trying to load the servers.json, it's probably corrupt, delete the file and redownload it.\nError: " + e.Message);
                    if (!api) Console.ReadLine();
                    Environment.Exit(0);
                }

            }
            else
            {
                Console.WriteLine("No servers.json file was found. Type 'D' to download an existing one now. Type 'G' to grab a new list of servers, this takes a long time... Alternatively, type anything else and the program will quit.");
                if (api) Environment.Exit(-1);
                string resp = Console.ReadLine();
                if (resp.ToLower() == "d")
                {
                    try
                    {
                        using (WebClient wc = new WebClient())
                        {
                            wc.DownloadFile(new Uri("http://lkd70.io/servers.json"), "servers.json");
                            string json = File.ReadAllText(@"servers.json");
                            serverlist = JsonConvert.DeserializeObject<arkserversjson>(json);
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("An error occured whilst downloading the servers.json file, please send this error message to LKD70:\n" + e.Message);
                        Console.ReadLine();
                        Environment.Exit(0);
                    }
                }
                else if (resp.ToLower() == "g")
                {
                    grabHandler(api, serverlist);
                }
                else
                {
                    Environment.Exit(0);
                }
            }
            #endregion

            #region Quick and ugly listener
            while (true)
            {
                string input = "";
                while (input == "")
                {
                    Console.Clear();
                    if (!api) Console.WriteLine(string.Concat(Enumerable.Repeat("█", Console.WindowWidth)));
                    if (!api) ColorConsole.WriteLine("ooooo       oooo   oooo ooooooooo             o       oooooooo8    ooooooo   \n".Cyan(), "888         888  o88    888    88o          888     888         o888   888o \n".DarkCyan(), "888         888888      888    888         8  88     888oooooo  888     888 \n".Yellow(), "888      o  888  88o    888    888        8oooo88           888 888o  8o888 \n".DarkYellow(), "o888ooooo88 o888o o888o o888ooo88        o88o  o888o o88oooo888    88ooo88   \n".Green(), "                                                                        88o8 ".DarkGreen(), String.Format("    Loaded {0} servers", serverlist.rows.Count()).DarkGreen());
                    if (!api) Console.WriteLine("Hello, please enter a command to continue, try 'help' if you're not sure what to do.");
                    input = (api) ? staticinput : Console.ReadLine();
                    Console.Clear();
                }
                #region help
                if (input.ToLower() == "help")
                {
                    if (api) Environment.Exit(-1);
                    Console.WriteLine(string.Concat(Enumerable.Repeat("█", Console.WindowWidth)));
                    ColorConsole.WriteLine("ooooo       oooo   oooo ooooooooo             o       oooooooo8    ooooooo   \n".Cyan(), "888         888  o88    888    88o          888     888         o888   888o \n".DarkCyan(), "888         888888      888    888         8  88     888oooooo  888     888 \n".Yellow(), "888      o  888  88o    888    888        8oooo88           888 888o  8o888 \n".DarkYellow(), "o888ooooo88 o888o o888o o888ooo88        o88o  o888o o88oooo888    88ooo88   \n".Green(), "                                                                        88o8 ".DarkGreen(), String.Format("    Loaded {0} servers", serverlist.rows.Count()).DarkGreen());
                    ColorConsole.WriteLine("Help\n\n".Cyan());
                    ColorConsole.WriteLine("Available Commands:".Green());
                    ConsoleTable table = new ConsoleTable("Command Syntax", "Description");
                    table.AddRow("help", "You're looking at it!");
                    table.AddRow("query <IP>:<PORT>", "Query the given address. Returns player and server information.");
                    table.AddRow("server <SERVER_ID> [delay 1-999]", "Returns information about the given server ID with optional delay (if it's available).");
                    table.AddRow("list [map:TheIsland type:pvp]", "Lists the servers available for querying.");
                    table.AddRow("grab", "Grabs a new server list from arkdedicated. This will take some time...");
                    table.AddRow("exit", "https://youtu.be/j1ykMNtzMT8");
                    table.Options.EnableCount = false;
                    table.Write();

                    ColorConsole.WriteLine("Examples of server ID's for different maps:".Green());
                    table = new ConsoleTable("server name", "example");
                    table.AddRow("NA-PVP-Official-Aberration100", "n100");
                    table.AddRow("NA-PVP-Official-Ragnarok300", "k300");
                    table.AddRow("EU-PVP-Official-TheIsland70", "d70");
                    table.AddRow("NA-PVP-Official-ScorchedEarth30", "h30");
                    table.AddRow("NA-PVP-Official-TheCenter300", "r300");
                    table.Options.EnableCount = false;
                    table.Write();

                    ColorConsole.WriteLine("Examples of list command syntaxes:".Green());
                    table = new ConsoleTable("syntax", "expected result");
                    table.AddRow("list", "lists all servers known to LKD ASQ");
                    table.AddRow("list map:ragnarok", "Lists all known servers with a Ragnarok map");
                    table.AddRow("list type:pvp", "Lists all known PVP servers");
                    table.AddRow("list map:theisland type:pve", "List all PvE Island servers.");
                    table.AddRow("list map:theisland,aberration", "Lists all aberration and island servers");
                    table.AddRow("list type:pve,pvp map:ragnarok,theisland", "lists all island and ragnarok pve and pvp servers");
                    table.Options.EnableCount = false;
                    table.Write();

                    ColorConsole.WriteLine("Example command uses:".Green());
                    table = new ConsoleTable("Syntax", "Description");
                    table.AddRow("server d25", "Returns information about server TheIsland25");
                    table.AddRow("server k73 25", "Returns information about server Ragnarok73 every 25 seconds");
                    table.AddRow("query 123.456.789.123:27015", "Returns informaiton about the server at the given address.");
                    table.AddRow("exit", "Chicago");
                    table.Options.EnableCount = false;
                    table.Write();

                }
                #endregion
                #region favorites
                else if (Regex.Match(input, @"^(?:favorites|f)(?: json| csv|)$").Success)
                {
                    string[] parms = input.Split(' ');
                    if (parms.Length == 1)
                    {
                        if (!api) Console.WriteLine(string.Concat(Enumerable.Repeat("█", Console.WindowWidth)));
                        if (!api) ColorConsole.WriteLine("ooooo       oooo   oooo ooooooooo             o       oooooooo8    ooooooo   \n".Cyan(), "888         888  o88    888    88o          888     888         o888   888o \n".DarkCyan(), "888         888888      888    888         8  88     888oooooo  888     888 \n".Yellow(), "888      o  888  88o    888    888        8oooo88           888 888o  8o888 \n".DarkYellow(), "o888ooooo88 o888o o888o o888ooo88        o88o  o888o o88oooo888    88ooo88   \n".Green(), "                                                                        88o8 ".DarkGreen(), String.Format("    Loaded {0} servers", serverlist.rows.Count()).DarkGreen());
                        if (!api) ColorConsole.WriteLine("Your favorite servers:\n\n".Cyan());
                        foreach (favorite server in serverlist.favorites)
                        {
                            if (server == null)
                            {
                                Console.WriteLine("Sorry, I couldn't find the server: " + server.id);
                            }
                            else
                            {
                                ServerInfo serverinfo = serverlist.getServerInfoById(server.id);
                                if (serverinfo == null)
                                {
                                    Console.WriteLine("It'd seem I couldn't connect to the server: " + server.id + ", perhaps it's offline?");
                                }
                                else
                                {
                                    QueryMaster.QueryMasterCollection<PlayerInfo> playerinfoinit = serverlist.getPlayerInfoById(server.id);
                                    if (playerinfoinit == null)
                                    {
                                        Console.WriteLine("Seems I couldn't get the player information for the server " + server.id + ", that's very odd.");
                                    }
                                    else
                                    {
                                        List<PlayerInfo> playerlist = new List<PlayerInfo>();
                                        foreach (PlayerInfo player in playerinfoinit)
                                        {
                                            if (player.Name != "")
                                            {
                                                playerlist.Add(player);
                                            }
                                        }
                                        ColorConsole.WriteLine(String.Format("Server: {0}\nPlayer count: {1}/{2}\n\nPlayer List:", serverinfo.Name, playerlist.Count, serverinfo.MaxPlayers).Cyan());
                                        ConsoleTable table = new ConsoleTable("Player Name", "Time Online");
                                        foreach (PlayerInfo player in playerlist)
                                        {
                                            table.AddRow(player.Name, player.Time.ToString(@"hh\:mm"));
                                        }
                                        table.Options.EnableCount = false;
                                        table.Write();
                                        Console.WriteLine("That's all of them!".Cyan());
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (parms[1].ToLower() == "csv")
                        {
                            foreach (favorite server in serverlist.favorites)
                            {
                                if (server == null)
                                {
                                    Console.WriteLine("Unknown server: " + server.id + ", ");
                                }
                                else
                                {
                                    ServerInfo serverinfo = serverlist.getServerInfoById(server.id);
                                    if (serverinfo == null)
                                    {
                                        Console.WriteLine("Server not found: " + server.id + " perhaps it's offline?, ");
                                    }
                                    else
                                    {
                                        QueryMaster.QueryMasterCollection<PlayerInfo> playerinfoinit = serverlist.getPlayerInfoById(server.id);
                                        if (playerinfoinit == null)
                                        {
                                            Console.WriteLine("no player info: " + server.id + ", ");
                                        }
                                        else
                                        {
                                            List<PlayerInfo> playerlist = new List<PlayerInfo>();
                                            foreach (PlayerInfo player in playerinfoinit)
                                            {
                                                if (player.Name != "")
                                                {
                                                    playerlist.Add(player);
                                                }
                                            }
                                            ColorConsole.WriteLine(String.Format("Server: {0}, Player count: {1}/{2}, ", serverinfo.Name, playerlist.Count, serverinfo.MaxPlayers).Cyan());
                                            foreach (PlayerInfo player in playerlist)
                                            {
                                                Console.WriteLine(player.Name + ", " + player.Time.ToString(@"hh\:mm") + ", ");
                                            }
                                        }
                                    }
                                }
                            }
                            Console.WriteLine("END");
                        }
                        else if (parms[1].ToLower() == "json")
                        {
                            string response = "[";
                            foreach (favorite server in serverlist.favorites)
                            {
                                if (server == null)
                                {
                                    response = response + "{\"success\":0,\"error\":\"Sorry, I couldn't find the server: " + server.id + "\"}";
                                    if (server != serverlist.favorites.Last())
                                    {
                                        response = response + ",";
                                    }
                                    Console.WriteLine(response);
                                }
                                else
                                {
                                    ServerInfo serverinfo = serverlist.getServerInfoById(server.id);
                                    if (serverinfo == null)
                                    {
                                        response = response + "{\"success\":0,\"error\":\"Server not found - Perhaps it's offline? " + server.id + "\"}";
                                        if (server != serverlist.favorites.Last())
                                        {
                                            response = response + ",";
                                        }
                                        Console.WriteLine(response);
                                    }
                                    else
                                    {
                                        QueryMaster.QueryMasterCollection<PlayerInfo> playerinfoinit = serverlist.getPlayerInfoById(server.id);
                                        if (playerinfoinit == null)
                                        {
                                            response = response + "{\"success\":0,\"error\":\"Seems I couldn't get the player information for the server: " + server.id + "\"}";
                                            if (server != serverlist.favorites.Last())
                                            {
                                                response = response + ",";
                                            }
                                            Console.WriteLine(response);
                                        }
                                        else
                                        {
                                            List<PlayerInfo> playerlist = new List<PlayerInfo>();
                                            foreach (PlayerInfo player in playerinfoinit)
                                            {
                                                if (player.Name != "")
                                                {
                                                    playerlist.Add(player);
                                                }
                                            }
                                            response = response + "{\"name\":\"" + serverinfo.Name + ", \"count\": " + playerlist.Count() + ", \"max\":" + serverinfo.MaxPlayers + ", \"players\":[";
                                            foreach (PlayerInfo player in playerlist)
                                            {
                                                response = response + "{\"name\":" + player.Name + ", \"time\":\"" + player.Time.ToString(@"hh\:mm") + "\"";
                                                if (player != playerlist.Last())
                                                {
                                                    response = response + ",";
                                                }
                                            }
                                            response = response + "]}";
                                            Console.WriteLine(response);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Unknown format. Please use 'csv' or 'json'. Otherwise, don't provide any format option.");
                        }
                    }
                }
                #endregion
                #region list
                else if (Regex.Match(input, @"(?:list|servers)(?:(?: [a-zA-Z,]+:[a-zA-Z,]+)+|)").Success)
                {
                    if (!api) Console.WriteLine(string.Concat(Enumerable.Repeat("█", Console.WindowWidth)));
                    if (!api) ColorConsole.WriteLine("ooooo       oooo   oooo ooooooooo             o       oooooooo8    ooooooo   \n".Cyan(), "888         888  o88    888    88o          888     888         o888   888o \n".DarkCyan(), "888         888888      888    888         8  88     888oooooo  888     888 \n".Yellow(), "888      o  888  88o    888    888        8oooo88           888 888o  8o888 \n".DarkYellow(), "o888ooooo88 o888o o888o o888ooo88        o88o  o888o o88oooo888    88ooo88   \n".Green(), "                                                                        88o8 ".DarkGreen(), String.Format("    Loaded {0} servers", serverlist.rows.Count()).DarkGreen());
                    if (!api) ColorConsole.WriteLine("List\n\n".Cyan());
                    ConsoleTable table = new ConsoleTable("ID", "Name", "Map", "ip", "port");
                    //ColorConsole.WriteLine("Here's a complete list of all available and indexed servers. Do note that just becuase it's listed doesn't ensure iot will respond to a query".Green());
                    string[] inputargs = input.Split(' ');
                    inputargs = inputargs.Skip(1).ToArray();
                    string map = "";
                    string type = "";
                    string[] maps = null;
                    string[] types = null;
                    if (inputargs.Length > 0)
                    {
                        foreach (string inputarg in inputargs)
                        {
                            string[] ia = inputarg.Split(':');
                            if (ia[0].ToLower() == "map") map = ia[1];
                            if (ia[0].ToLower() == "maps") map = ia[1];
                            if (ia[0].ToLower() == "type") type = ia[1];
                            if (ia[0].ToLower() == "types") type = ia[1];
                            maps = map.Split(',');
                            types = type.Split(',');
                            maps = maps.Select(s => s.ToLower()).ToArray();
                            types = types.Select(s => s.ToLower()).ToArray();
                        }
                    }
                    foreach (arkserver server in serverlist.rows)
                    {
                        if (map == "" && type == "")
                        {
                            table.AddRow(server.id, server.prettyname, server.map, server.ip, server.port);
                        }
                        else if (map != "" && type != "")
                        {
                            if (Array.IndexOf(maps, server.map.ToLower()) > -1 && Array.IndexOf(types, server.type.ToLower()) > -1)
                            {
                                table.AddRow(server.id, server.prettyname, server.map, server.ip, server.port);
                            }
                            else if (map.ToLower() == server.map.ToLower() && type.ToLower() == server.type.ToLower())
                            {
                                table.AddRow(server.id, server.prettyname, server.map, server.ip, server.port);
                            }
                        }
                        else if (map != "")
                        {
                            if (Array.IndexOf(maps, server.map.ToLower()) > -1)
                            {
                                table.AddRow(server.id, server.prettyname, server.map, server.ip, server.port);
                            }
                            else if (map.ToLower() == server.map.ToLower())
                            {
                                table.AddRow(server.id, server.prettyname, server.map, server.ip, server.port);
                            }
                        }
                        else if (type != "")
                        {
                            if (Array.IndexOf(types, server.type.ToLower()) > -1)
                            {
                                table.AddRow(server.id, server.prettyname, server.map, server.ip, server.port);
                            }
                            else if (server.type.ToLower() == type.ToLower())
                            {
                                table.AddRow(server.id, server.prettyname, server.map, server.ip, server.port);
                            }
                        }
                    }
                    table.Write();

                }
                #endregion
                #region grab
                else if (input.ToLower() == "grab")
                {
                    grabHandler(api, serverlist);
                }
                #endregion
                #region server
                else if (Regex.Match(input, @"^server\s(\D\d+)(?:\s([1-9][0-9]{0,2}|1000)|)$").Success)
                {
                    Match mid = Regex.Match(input, @"^server\s(\D\d+)(?:\s([1-9][0-9]{0,2}|1000)|)$");
                    string id = mid.Groups[1].Value;
                    int delayint = 0;
                    string delay = mid.Groups[2].Value;
                    if (delay != "")
                    {
                        delayint = Int32.Parse(delay);
                    }
                    bool loop = true;

                    while (loop == true)
                    {
                        Console.Clear();
                        Console.WriteLine(string.Concat(Enumerable.Repeat("█", Console.WindowWidth)));
                        ColorConsole.WriteLine("ooooo       oooo   oooo ooooooooo             o       oooooooo8    ooooooo   \n".Cyan(), "888         888  o88    888    88o          888     888         o888   888o \n".DarkCyan(), "888         888888      888    888         8  88     888oooooo  888     888 \n".Yellow(), "888      o  888  88o    888    888        8oooo88           888 888o  8o888 \n".DarkYellow(), "o888ooooo88 o888o o888o o888ooo88        o88o  o888o o88oooo888    88ooo88   \n".Green(), "                                                                        88o8 ".DarkGreen(), String.Format("    Loaded {0} servers", serverlist.rows.Count()).DarkGreen());
                        ColorConsole.WriteLine("Server\n\n".Cyan());
                        if (delay == "") loop = false;
                        arkserver server = serverlist.getServerById(id);
                        if (server == null)
                        {
                            Console.WriteLine("Sorry, I couldn't find that server.");
                        }
                        else
                        {
                            ServerInfo serverinfo = serverlist.getServerInfo(server);
                            if (serverinfo == null)
                            {
                                Console.WriteLine("It'd seem I couldn't connect to that server, perhaps it's offline?");
                            }
                            else
                            {
                                QueryMaster.QueryMasterCollection<PlayerInfo> playerinfoinit = serverlist.getPlayerInfo(server);
                                if (playerinfoinit == null)
                                {
                                    Console.WriteLine("Seems I couldn't get the player information, that's very odd.");
                                }
                                else
                                {
                                    List<PlayerInfo> playerlist = new List<PlayerInfo>();
                                    foreach (PlayerInfo player in playerinfoinit)
                                    {
                                        if (player.Name != "")
                                        {
                                            playerlist.Add(player);
                                        }
                                    }
                                    ColorConsole.WriteLine("Last Updated: " + DateTime.Now.ToString());
                                    ColorConsole.WriteLine(String.Format("Name: {0}\nPlayer count: {1}/{2}\n\nPlayer List:", serverinfo.Name, playerlist.Count, serverinfo.MaxPlayers).Cyan());
                                    ConsoleTable table = new ConsoleTable("Player Name", "Time Online");
                                    foreach (PlayerInfo player in playerlist)
                                    {
                                        table.AddRow(player.Name, player.Time.ToString(@"hh\:mm"));
                                    }
                                    table.Options.EnableCount = false;
                                    table.Write();
                                }
                            }
                        }
                        if (delay != "")
                        {
                            System.Threading.Thread.Sleep(delayint * 1000);
                        }
                    }
                }
                #endregion
                #region exit
                else if (input == "exit")
                {
                    if (api) Console.WriteLine("Why tho?");
                    Environment.Exit(0);
                }
                #endregion
                #region query
                else if (Regex.Match(input, @"query (\d+.\d+.\d+.\d+):(\d+)").Success)
                {
                    Console.WriteLine(string.Concat(Enumerable.Repeat("█", Console.WindowWidth)));
                    ColorConsole.WriteLine("ooooo       oooo   oooo ooooooooo             o       oooooooo8    ooooooo   \n".Cyan(), "888         888  o88    888    88o          888     888         o888   888o \n".DarkCyan(), "888         888888      888    888         8  88     888oooooo  888     888 \n".Yellow(), "888      o  888  88o    888    888        8oooo88           888 888o  8o888 \n".DarkYellow(), "o888ooooo88 o888o o888o o888ooo88        o88o  o888o o88oooo888    88ooo88   \n".Green(), "                                                                        88o8 ".DarkGreen(), String.Format("    Loaded {0} servers", serverlist.rows.Count()).DarkGreen());
                    ColorConsole.WriteLine("Query\n\n".Cyan());
                    Match mid = Regex.Match(input, @"query (\d+.\d+.\d+.\d+):(\d+)");
                    string ip = mid.Groups[1].Value;
                    int port = Int32.Parse(mid.Groups[2].Value);

                    Server server = null;
                    using (server = ServerQuery.GetServerInstance(QueryMaster.EngineType.Source, ip, (ushort)port, false, 5000, 5000, 1, false))
                    {
                        var serverInfo = server.GetInfo();
                        var playerInfo = server.GetPlayers();

                        if (serverInfo == null)
                        {
                            ColorConsole.WriteLine("Sorry, I couldn't resolve that server address.");
                        }
                        else
                        {
                            List<PlayerInfo> playerlist = new List<PlayerInfo>();
                            foreach (PlayerInfo player in playerInfo)
                            {
                                if (player.Name != "")
                                {
                                    playerlist.Add(player);
                                }
                            }
                            ColorConsole.WriteLine(String.Format("Server: {0}\nPlayer count: {1}/{2}\n\nPlayer List:", serverInfo.Name, playerlist.Count, serverInfo.MaxPlayers).Cyan());
                            ConsoleTable table = new ConsoleTable("Player Name", "Time Online");
                            foreach (PlayerInfo player in playerlist)
                            {
                                table.AddRow(player.Name, player.Time.ToString(@"hh\:mm"));
                            }
                            table.Options.EnableCount = false;
                            table.Write();
                        }
                    }
                }
                #endregion
                #region failed
                else
                {
                    if (!api) Console.WriteLine(string.Concat(Enumerable.Repeat("█", Console.WindowWidth)));
                    if (!api) ColorConsole.WriteLine("ooooo       oooo   oooo ooooooooo             o       oooooooo8    ooooooo   \n".Cyan(), "888         888  o88    888    88o          888     888         o888   888o \n".DarkCyan(), "888         888888      888    888         8  88     888oooooo  888     888 \n".Yellow(), "888      o  888  88o    888    888        8oooo88           888 888o  8o888 \n".DarkYellow(), "o888ooooo88 o888o o888o o888ooo88        o88o  o888o o88oooo888    88ooo88   \n".Green(), "                                                                        88o8 ".DarkGreen(), String.Format("    Loaded {0} servers", serverlist.rows.Count()).DarkGreen());
                    if (!api) ColorConsole.WriteLine("Ruh-roh raggy!\n\n".Cyan());
                    Console.WriteLine("Not sure what you want, but I'm sure this isn't it. Click enter to return to the menu...");
                }
                #endregion
                if (!api) Console.ReadLine();
                if (api) Environment.Exit(0);
            }
            #endregion

        }

        /// <summary>
        /// Handles the grab command
        /// </summary>
        /// <param name="api"></param>
        /// <param name="serverlist"></param>
        static void grabHandler(bool api, arkserversjson serverlist)
        {
            if (api) Environment.Exit(-1);
            Console.WriteLine(string.Concat(Enumerable.Repeat("█", Console.WindowWidth)));
            ColorConsole.WriteLine("ooooo       oooo   oooo ooooooooo             o       oooooooo8    ooooooo   \n".Cyan(), "888         888  o88    888    88o          888     888         o888   888o \n".DarkCyan(), "888         888888      888    888         8  88     888oooooo  888     888 \n".Yellow(), "888      o  888  88o    888    888        8oooo88           888 888o  8o888 \n".DarkYellow(), "o888ooooo88 o888o o888o o888ooo88        o88o  o888o o88oooo888    88ooo88   \n".Green(), "                                                                        88o8 ".DarkGreen());
            ColorConsole.WriteLine("Grab\n\n".Cyan());
            Console.WriteLine("Writing to file: servers.json");
            List<server> servers = new List<server>();

            using (WebClient wc = new WebClient())
            {
                try
                {
                    if (File.Exists(@"servers.json"))
                    {
                        File.Delete(@"servers.json");
                    }
                    if (File.Exists(@"temp.txt"))
                    {
                        File.Delete(@"temp.txt");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                wc.DownloadFile(new Uri("http://arkdedicated.com/officialservers.ini"), "temp.txt");
                string[] officialservers = File.ReadAllLines(@"temp.txt");
                foreach (string officialserver in officialservers)
                {
                    server serv = new server();
                    serv.Ip = officialserver.Split(' ')[0];
                    serv.Location = officialserver.Split(' ')[1].Split('/')[2];
                    servers.Add(serv);
                }
            }

            TextWriter text = new StreamWriter(@"servers.json", true);
            text.WriteLine("{");
            text.WriteLine("\t\"favorites\":[");
            text.WriteLine("\t],");
            text.WriteLine("\t\"rows\":[");
            text.Close();

            for (int i = 0; i < servers.Count; i++)
            {
                getServerInformation(servers[i].Ip, servers[i].Location, (i == servers.Count - 1));
                Console.WriteLine("Progress: " + (i + 1).ToString() + "/" + servers.Count.ToString());
            }

            text = new StreamWriter(@"servers.json", true);
            text.WriteLine("\t]");
            text.WriteLine("}");
            text.Close();
        }

        /// <summary>
        /// Grab server information to servers.json
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="location"></param>
        /// <param name="last"></param>
        static void getServerInformation(string ip, string location, bool last)
        {
            TextWriter text = new StreamWriter(@"servers.json", true);
            Server arkserver = null;

            List<int> ports = new List<int>();
            ports.Add(27015);
            ports.Add(27017);
            ports.Add(27019);
            ports.Add(27021);
            ports.Add(27023);

            foreach (int port in ports)
            {
                using (arkserver = ServerQuery.GetServerInstance(QueryMaster.EngineType.Source, ip, (ushort)port, false, 500, 500, 1, false))
                {
                    var serverInfo = arkserver.GetInfo();
                    if (serverInfo != null)
                    {
                        string prettyname = serverInfo.Name.Split(' ')[0];

                        if (prettyname != "TrailerTeam")
                        {
                            MatchCollection match = Regex.Matches(serverInfo.Name, @"\w\d+");
                            string id = match[0].Value;
                            match = Regex.Matches(prettyname, @"(FRESH|Hardcore|CrossArk|OfflineRaidProtection|PVP|PVE|[0-9])");
                            string type = (match.Count != 0) ? match[0].Value : "Unknown";
                            if (prettyname != "")
                            {
                                text.WriteLine("\t\t{");
                                text.WriteLine("\t\t\t\"type\":\"" + type + "\", ");
                                text.WriteLine("\t\t\t\"prettyname\":\"" + prettyname + "\", ");
                                text.WriteLine("\t\t\t\"id\":\"" + id + "\", ");
                                text.WriteLine("\t\t\t\"ip\":\"" + ip + "\", ");
                                text.WriteLine("\t\t\t\"location\":\"" + location + "\", ");
                                text.WriteLine("\t\t\t\"name\":\"" + serverInfo.Name + "\", ");
                                text.WriteLine("\t\t\t\"map\":\"" + serverInfo.Map + "\", ");
                                text.WriteLine("\t\t\t\"port\":\"" + port + "\", ");
                                text.WriteLine("\t\t\t\"steamid\":\"" + serverInfo.Id + "\"");
                                string lastline = "\t\t}";
                                if (last && port == 27023)
                                {

                                }
                                else
                                {
                                    lastline = lastline + ",";
                                }
                                text.WriteLine(lastline);

                            }
                            //text.WriteLine("{\"type\":\"" + type + "\",\"prettyname\":\"" + prettyname + "\",\"id\":\"" + id + "\",\"ip\":\"" + ip + "\", \"location\": \"" + location + "\", \"name\":\"" + serverInfo.Name + "\", \"map\":\"" + serverInfo.Map + "\", \"port\":" + port + ", \"steamid\":" + serverInfo.Id + "},");
                            //Console.WriteLine("Added server: " + serverInfo.Name);
                        }

                    }
                }
            }
            text.Close();
        }
    }

    /// <summary>
    /// server definition for loading server details from arkdedicated.
    /// </summary>
    class server
    {
        private string ip = "";
        private string location = "";
        private string name = "";
        private string map = "";
        private int port = 0;
        private int steamid = 0;

        public string Ip
        {
            get
            {
                return ip;
            }

            set
            {
                ip = value;
            }
        }

        public string Location
        {
            get
            {
                return location;
            }

            set
            {
                location = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public string Map
        {
            get
            {
                return map;
            }

            set
            {
                map = value;
            }
        }

        public int Port
        {
            get
            {
                return port;
            }

            set
            {
                port = value;
            }
        }

        public int Steamid
        {
            get
            {
                return steamid;
            }

            set
            {
                steamid = value;
            }
        }
    }

    /// <summary>
    /// arkserver contains static information about an ark server.
    /// </summary>
    public class arkserver
    {
        [JsonProperty("type")]
        public string type { set; get; }

        [JsonProperty("prettyname")]
        public string prettyname { set; get; }

        [JsonProperty("id")]
        public string id { set; get; }

        [JsonProperty("ip")]
        public string ip { set; get; }

        [JsonProperty("location")]
        public string location { set; get; }

        [JsonProperty("name")]
        public string name { set; get; }

        [JsonProperty("map")]
        public string map { set; get; }

        [JsonProperty("port")]
        public int port { set; get; }
    }

    /// <summary>
    /// favorites definition. Only stores server ID.
    /// </summary>
    public class favorite
    {
        [JsonProperty("id")]
        public string id { set; get; }
    }

    /// <summary>
    /// Define server storage/halding methods.
    /// </summary>
    public class arkserversjson
    {
        /// <summary>
        /// List of arkservers
        /// </summary>
        public List<arkserver> rows { get; set; }

        public List<favorite> favorites { get; set; }

        ///<summary>
        ///<para>Returns the first instance of an arkserver matching the given ID.</para>
        ///</summary>
        public arkserver getServerById(string Id)
        {
            foreach (arkserver server in rows)
            {
                if (server.id == Id)
                {
                    return server;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns server info for the provided server ID.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ServerInfo getServerInfoById(string Id)
        {
            arkserver tempserver = getServerById(Id);
            if (tempserver == null) return null;

            Server server = null;
            using (server = ServerQuery.GetServerInstance(QueryMaster.EngineType.Source, tempserver.ip, (ushort)tempserver.port, false, 5000, 5000, 1, false))
            {
                var serverInfo = server.GetInfo();
                return serverInfo;
            }
        }

        /// <summary>
        /// Returns server info for the provided arkserver.
        /// </summary>
        /// <param name="tempserver"></param>
        /// <returns></returns>
        public ServerInfo getServerInfo(arkserver tempserver)
        {
            Server server = null;
            using (server = ServerQuery.GetServerInstance(QueryMaster.EngineType.Source, tempserver.ip, (ushort)tempserver.port, false, 5000, 5000, 1, false))
            {
                var serverInfo = server.GetInfo();
                return serverInfo;
            }
        }

        /// <summary>
        /// Returns player info for the provided server ID.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public QueryMaster.QueryMasterCollection<PlayerInfo> getPlayerInfoById(string Id)
        {
            arkserver tempserver = getServerById(Id);
            if (tempserver == null) return null;

            Server server = null;
            using (server = ServerQuery.GetServerInstance(QueryMaster.EngineType.Source, tempserver.ip, (ushort)tempserver.port, false, 5000, 5000, 1, false))
            {
                var playerInfo = server.GetPlayers();
                return playerInfo;
            }
        }

        /// <summary>
        /// Returns player information for the provided arkserver.
        /// </summary>
        /// <param name="tempserver"></param>
        /// <returns></returns>
        public QueryMaster.QueryMasterCollection<PlayerInfo> getPlayerInfo(arkserver tempserver)
        {
            Server server = null;
            using (server = ServerQuery.GetServerInstance(QueryMaster.EngineType.Source, tempserver.ip, (ushort)tempserver.port, false, 5000, 5000, 1, false))
            {
                var playerInfo = server.GetPlayers();
                return playerInfo;
            }
        }

        /// <summary>
        /// Returns rules for the provided server ID.
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public QueryMaster.QueryMasterCollection<Rule> getRulesById(string Id)
        {
            arkserver tempserver = getServerById(Id);
            if (tempserver == null) return null;

            Server server = null;
            using (server = ServerQuery.GetServerInstance(QueryMaster.EngineType.Source, tempserver.ip, (ushort)tempserver.port, false, 5000, 5000, 1, false))
            {
                var Rules = server.GetRules();
                return Rules;
            }
        }

        /// <summary>
        /// Returns the rules for the provided arkserver
        /// </summary>
        /// <param name="tempserver"></param>
        /// <returns></returns>
        public QueryMaster.QueryMasterCollection<Rule> getRules(arkserver tempserver)
        {
            Server server = null;
            using (server = ServerQuery.GetServerInstance(QueryMaster.EngineType.Source, tempserver.ip, (ushort)tempserver.port, false, 5000, 5000, 1, false))
            {
                var Rules = server.GetRules();
                return Rules;
            }
        }
    }
}
