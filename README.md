# LKD-ASQ
A very messy C# Ark Server Query utility

## Why?
I was tricked in to thinking this would be a good investment of my time. A friend needed to a tool to track when a server was up/down and had players on-line. They didn't trust BattleMetrics and claimed it was "too slow" and "inaccurate".

So I created this! Shockingly fast, as accurate as can be... And overall a stunning masterpiece!
Jokes aside, it does what he wanted, so all is well.

## How?

Here's some basic documentation on the usage of each command...

### Command: help

It's a help command. It gives you information to try and help you...

### Command: favorites

Returns information about your favorite servers. You can see how to add favorites [here](#adding-favorite-servers)

### list

Lists servers from your servers.json that match all given options.

##### Examples:

| Syntax | Description |
| ------ | ----------- |
| list   | returns all servers from the servers.json file |
| list map:ragnarok |Lists all known servers with a Ragnarok map |
| list type:pvp | Lists all known PVP servers |
| list map:theisland type:pve | List all PvE Island servers. |
| list map:theisland,aberration | Lists all aberration and island servers |
| list type:pve,pvp map:ragnarok,theisland | lists all island and ragnarok pve and pvp servers |

No name string search option is available yet... I don't think it's needed.

### grab

grabs a list of ips from arkdedicated.com and scans their ports for ark servers. If any are found they are added to a new servers.json file. This will delete the old servers.json

### server

Returns server and player informaiton for a server matching the given [server ID](#server-id-formatting). An optional "Delay" can be given. Delay is in seconds and can range from 1 to 999. Usage of delay will have it recheck every {delay} seconds.

Syntax: `server <server ID> [optional delay 1 to 999 seconds]`

##### Examples:

| Syntax | Description |
| ------ | ----------- |
| server d1 | returns player and server information for the server TheIsland1 in the servers.json list. |
| server r10 120 | returns player and server information for the server Ragnarok10 in the servers.json list. Updates the information every 120 seconds (two minutes) |

### query

Syntax: `query 123.456.789.123:27015`

Returns server and player information for the given IP and PORT.

### exit

[HERE](https://www.youtube-nocookie.com/embed/j1ykMNtzMT8?rel=0&amp;start=4)

## Other information

### Adding Favorite Servers

To add a server to your favorites, you need to manually edit the servers.json file.

Add the following secition to the beginning of the gile before `"rows":[` but after `{` (line one)
Ensure you change the ID to the ID of your favorite server(s)
```
    "favorites": [
        {
            "id": "d25"
        }, 
        {
        	"id": "n305"
        }
    ],
```
Once done, you should end up with something like this:

![Image showing correct layout](https://i.lkd70.io/2wrnqz.png)

### Server ID formatting

Server IDs are a combination of the last letter of the map name and the number of the server. 

##### Here's some examples to explain:
| ID | Server Name |
| -- | ----------- |
| n100 | NA-PVP-Official-Aberration100 |
| k300 | NA-PVP-Official-Ragnarok300 |
| d70 | EU-PVP-Official-TheIsland70 |
| h30 | NA-PVP-Official-ScorchedEarth30 |
| r300 | NA-PVP-Official-TheCenter300 |

