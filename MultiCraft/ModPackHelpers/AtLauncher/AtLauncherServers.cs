using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Windows.Media.Imaging;
using System.IO;
using System.Reflection;

namespace MultiCraft.ModPackHelpers.AtLauncher
{
    public static class ATLauncherServers
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

        public static List<AtLauncherPacks> GetPacks()
        {
            using (var client = new WebClient())
            {
                var domain = GetBestServer();
                if (domain != "master.atlcdn.net")
                    domain = $"{domain}/containers/atl";

                return JsonConvert.DeserializeObject<List<AtLauncherPacks>>(client.DownloadString(
                    $"http://{domain}/launcher/json/packs.json").PackReplace());
            }
        }
        public static string PackReplace(this string input)
        {
            return input.Replace("public", "Public").Replace("private", "Private").Replace("semipublic", "Semipublic");
        }

        public static Uri GetImageFromPackName(string packName, List<AtLauncherPacks> hashes)
        {
            using (var client = new WebClient())
            {
                var packs = hashes.Where(x => x.Name == $"{packName.ToLower().ImageNameClean()}.png").ToList();
                if (!packs.Any())
                    return null;

                var savePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ATLauncher", "Images");
                
                if (!Directory.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ATLauncher")))
                {
                    Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ATLauncher"));
                    Directory.CreateDirectory(savePath);
                }

                if (File.Exists(Path.Combine(savePath, $"{packName.ToLower().ImageNameClean()}.png")))
                {
                    return new Uri(Path.Combine(savePath, $"{packName.ToLower().ImageNameClean()}.png"));
                }


                var domain = GetBestServer();
                if (domain != "master.atlcdn.net")
                    domain = $"{domain}/containers/atl";

                var imageUrl = $"http://{domain}/launcher/images/{packName.ToLower().ImageNameClean()}.png";
                client.DownloadFile(imageUrl, Path.Combine(savePath, $"{packName.ToLower().ImageNameClean()}.png"));
                return new Uri(Path.Combine(savePath, $"{packName.ToLower().ImageNameClean()}.png"));
            }
        }

        public static string ImageNameClean(this string input)
        {
            return input.Replace(" ", "").Replace(":", "").Replace("'", "").
                Replace("-", "").Replace("®", "").Replace("&", "").Replace("³", "").Replace("_", "").Replace("/", "").
                Replace("æ", "").Replace(".", "");
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

    #region AtLauncherPacksJson
    public class AtLauncherPacks
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("position")]
        public int Position { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public AtLauncherType Type { get; set; }

        [JsonProperty("devVersions")]
        public List<ATLauncherVersions> DevVersions { get; set; }

        [JsonProperty("versions")]
        public List<ATLauncherVersions> Versions { get; set; }

        [JsonProperty("createServer")]
        public bool CreateServer { get; set; }

        [JsonProperty("leaderboards")]
        public bool Leaderboards { get; set; }

        [JsonProperty("logging")]
        public bool Logging { get; set; }

        [JsonProperty("crashReports")]
        public bool CrashReports { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("supportURL")]
        public string SupportURL { get; set; }

        [JsonProperty("websiteURL")]
        public string WebsiteURL { get; set; }
    }

    public enum AtLauncherType
    {
        Public,
        Private,
        Semipublic
    }

    public class ATLauncherVersions
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("minecraft")]
        public string Minecraft { get; set; }

        [JsonProperty("hasJson")]
        public bool HasJson { get; set; }

        [JsonProperty("isDev")]
        public bool IsDev { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("isRecommended")]
        public bool IsRecommended { get; set; }

        [JsonProperty("canUpdate")]
        public bool CanUpdate { get; set; }
    }
    #endregion

    public class ATLauncherHashes
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("folder")]
        public string Folder { get; set; }

        [JsonProperty("size")]
        public uint Size { get; set; }

        [JsonProperty("md5")]
        public string MD5 { get; set; }

        [JsonProperty("sha1")]
        public string SHA1 { get; set; }
    }
}
