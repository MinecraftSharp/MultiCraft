/*
 *  MultiCraft, an open-source minecraft launcher designed for easy install of mods
 *  Copyright (C) 2016  eddy5641
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace MultiCraft.Minecraft.FML
{
    public static class ForgeVersions
    {
        /// <summary>
        /// Downloads all of Minecraft Forge Versions
        /// </summary>
        /// <returns>ForgeJSON</returns>
        public static ForgeJSON GetJSONData()
        {
            using (var webClient = new WebClient())
            {
                //Download the json and return the data
                var forgeData = webClient.DownloadString("http://files.minecraftforge.net/maven/net/minecraftforge/forge/json");
                var data = JsonConvert.DeserializeObject<ForgeJSON>(forgeData);
                return data;
            }
        }
    }


    /// <summary>
    /// Forge json info
    /// </summary>
    
    //Sadly because of forge's weird json naming I had to do this by hand -.-
    public class ForgeJSON
    {
        [JsonProperty("adfocus")]
        public int Adfocus { get; set; }

        [JsonProperty("artifact")]
        public string Artifact { get; set; }

        [JsonProperty("branches")]
        public Dictionary<string, List<int>> Branches { get; set; }

        [JsonProperty("homepage")]
        public string Homepage { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("number")]
        public Dictionary<int, ForgeNumber> Number { get; set; }

        [JsonProperty("promos")]
        public Dictionary<string, int> Promos { get; set; }

        [JsonProperty("webpath")]
        public string Webpath { get; set; }
    }

    public class ForgeNumber
    {
        [JsonProperty("branch")]
        public string Branch { get; set; }

        [JsonProperty("build")]
        public int Build { get; set; }

        //Actually forge??? What is this for
        [JsonProperty("files")]
        public List<List<string>> Files { get; set; }

        [JsonProperty("mcversion")]
        public string MinecraftVersion { get; set; }

        [JsonProperty("modified")]
        public float Modified { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }
}
