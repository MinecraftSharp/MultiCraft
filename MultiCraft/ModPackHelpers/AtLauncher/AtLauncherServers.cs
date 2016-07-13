using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace MultiCraft.ModPackHelpers.AtLauncher
{
    public static class AtLauncherServers
    {
        public static string GetBestServer()
        {
            try
            {
                using (var client = new WebClient())
                {
                    //Download the servers
                    var json =
                        JsonConvert.DeserializeObject<List<Servers>>(
                            client.DownloadString("http://master.atlcdn.net/launcher/json/servers.json"));
                    //Send a ping request to the servers
                    var pingSender = new Ping();
                    var tasks = json.Select(ip => !ip.BaseURL.Contains(":") ? pingSender.Send(ip.BaseURL.Split('/')[0], 80) : pingSender.Send(ip.BaseURL.Split(':')[0], 8080));
                    //Remove any failed pings
                    IList<PingReply> successPings = tasks.Where(pings => pings.Status == IPStatus.Success).ToList();
                    //Return the fastest ping, this is an IP Address
                    return successPings.Where(ping => ping.RoundtripTime == successPings.Min(pings => pings.RoundtripTime)).ToList()[0].Address.ToString();

                }
            }
            catch
            {
                //if something goes wrong just use this instead
                return "master.atlcdn.net";
            }
        }
    }

    public class Servers
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("baseURL")]
        public string BaseURL { get; set; }

        [JsonProperty("userSelectable")]
        public bool UserSelectable { get; set; }

        [JsonProperty("disabled")]
        public bool Disabled { get; set; }

        [JsonProperty("isMaster")]
        public bool IsMaster { get; set; }
    }
}
