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
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace MultiCraft.ModPackHelpers.AtLauncher
{
    public static class ATLauncherServers
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ATLauncherServers));
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
                    var ipaddress = successPings.Where(ping => ping.RoundtripTime == successPings.Min(pings => pings.RoundtripTime)).ToList()[0].Address.ToString();
                    log.Debug("Retrieved best server IP successfully");
                    return json.Where(x => x.IPAddress == ipaddress).First().BaseURL;
                    
                }
            }
            catch (Exception e)
            {
                log.Debug($"Failed to retrieve best server : {e.Message}");
                //if something goes wrong just use this instead
                return "master.atlcdn.net";
            }
        }

        public static List<AtLauncherPacks> GetPacks()
        {
            using (var client = new WebClient())
            {
                var domain = GetBestServer();
                //Gets all ATLauncher packs
                return JsonConvert.DeserializeObject<List<AtLauncherPacks>>(client.DownloadString(
                    $"http://{domain}/launcher/json/packs.json").PackReplace());
            }
        }
        public static string PackReplace(this string input)
        {
            //This just makes the code cleaner
            return input.Replace("public", "Public").Replace("private", "Private").Replace("semipublic", "Semipublic");
        }

        public static Uri GetImageFromPackName(string packName)
        {
            //helps with getting the picture
            var savePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ATLauncher", "Images");
            //Check if the file exists
            if (File.Exists(Path.Combine(savePath, $"{packName.ToLower().ImageNameClean()}.png")))
            {
                //File exists, return uri to it
                return new Uri(Path.Combine(savePath, $"{packName.ToLower().ImageNameClean()}.png"));
            }
            //File does not exist, use the multicraft stopcraft picture
            return null;
        }

        public static async Task<bool> DownloadAllFiles()
        {
            try
            {
                var domain = GetBestServer();
                var client = new WebClient();
                //Download the hashes json
                var files = JsonConvert.DeserializeObject<List<ATLauncherHashes>>(client.DownloadString($"http://{domain}/launcher/json/hashes.json"));
                client.Dispose();
                //Ensure that the directories exist
                var MasterPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ATLauncher");
                if (!Directory.Exists(MasterPath))
                    Directory.CreateDirectory(MasterPath);

                //Use Parallel because there are a lot of files
                Parallel.ForEach(files, (file) =>
                {
                    //Ensure directory exists
                    if (!Directory.Exists(Path.Combine(MasterPath, file.Folder)))
                    {
                        Directory.CreateDirectory(Path.Combine(MasterPath, file.Folder));
                    }
                    //Do not do pointless stuff
                    if (file.Name == "Launcher")
                        return;
                    //check if file exists
                    if (File.Exists(Path.Combine(MasterPath, file.Folder, file.Name)))
                    {
                        //Files exits check MD5 hash
                        using (var md5 = MD5.Create())
                        {
                            using (var stream = File.OpenRead(Path.Combine(MasterPath, file.Folder, file.Name)))
                            {
                                //MD5 hash different, override the old file
                                if (file.MD5 != Encoding.Default.GetString(md5.ComputeHash(stream)))
                                {
                                    stream.Dispose();
                                    File.Delete(Path.Combine(MasterPath, file.Folder, file.Name));
                                    using (var downloadClient = new WebClient())
                                    {
                                        downloadClient.DownloadFile($"http://{domain}/launcher/{file.Folder.ToLower()}/{file.Name}", Path.Combine(MasterPath, file.Folder, file.Name));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //file does not exist, download it
                        using (var downloadClient = new WebClient())
                        {
                            downloadClient.DownloadFile($"http://{domain}/launcher/{file.Folder.ToLower()}/{file.Name}", Path.Combine(MasterPath, file.Folder, file.Name));
                        }
                    }
                });
                return true;
            }
            catch { return false; }
        }

        public static string ImageNameClean(this string input)
        {
            //this helps a lot with getting image names
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

        public string IPAddress => Dns.GetHostAddresses(BaseURL.Split('/')[0].Split(':')[0])[0].ToString();
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
