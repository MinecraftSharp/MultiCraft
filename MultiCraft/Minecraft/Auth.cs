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

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace MultiCraft.Minecraft
{
    /// <summary>
    /// Handles Minecraft Login
    /// </summary>
    public static class Auth
    {
        /// <summary>
        /// Makes it so other users can not use the access token
        /// </summary>
        /// <param name="accessToken">The access token from Mojang</param>
        /// <param name="clientToken">The client token sent to Mojang when authenticating</param>
        /// <returns>the invalidation result</returns>
        public static MinecraftAuth Invalidate(string accessToken, string clientToken)
        {
            try
            {
                //create a web request to invalidate
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://authserver.mojang.com/invalidate");
                //set up to send to mojang
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                //Send data to mojang
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {

                    var json = new MinecraftInvalidation(accessToken, clientToken);

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                    //Success with invalidation
                    return MinecraftAuth.Success;
                }
            }
            catch
            {
                return MinecraftAuth.UnknownError;
            }
        }

        /// <summary>
        /// Makes it so other users can not use the access token
        /// </summary>
        /// <param name="loginData">the login data returned from using <seealso cref="Login"/></param>
        /// <returns>the invalidation result</returns>
        public static MinecraftAuth Invalidate(MinecraftLogin loginData)
        {
            return Invalidate(loginData.AccessToken, loginData.AccessToken);
        }

        public static MinecraftAuth Login(string loginUser, string pass, ref MinecraftLogin loginData)
        {
            try
            {
                //create a web request to try to login
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://authserver.mojang.com/authenticate");
                //set up to send data to mojang
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                //send data
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    //simple way of creating a json
                    var json = new MineLogin(loginUser, pass);

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                    //Check what mojang says
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    var responseStream = httpResponse.GetResponseStream();
                    if (responseStream == null)
                        return MinecraftAuth.StreamError;
                    //Make sure the account is Premium
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        var result = JsonConvert.DeserializeObject<MinecraftLogin>(streamReader.ReadToEnd());
                        if (result.AvailableProfiles.Count == 0)
                            return MinecraftAuth.NotPremium;

                        loginData = result;
                        return MinecraftAuth.Success;
                    }
                }
            }
            catch (IOException e)
            {
                return e.Message.Contains("authentication") ? MinecraftAuth.SSLError : MinecraftAuth.UnknownError;
            }
            catch
            {
                return MinecraftAuth.UnknownError;
            }
        }
    }

    /// <summary>
    /// All possible results from authentication actions
    /// </summary>
    public enum MinecraftAuth
    {
        NotPremium,
        Success,
        MigratedAccount,
        WrongPass,
        ServiceUnavailable,
        UnknownError,
        SSLError,
        StreamError,
        Empty
    }

    #region json
    public class Agent
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "Minecraft";

        [JsonProperty("version")]
        public int Version { get; set; } = 1;
    }

    public class MineLogin
    {
        public MineLogin(string user, string pass)
        {
            Username = user;
            Password = pass;
        }

        [JsonProperty("agent")]
        public Agent Agent { get; set; } = new Agent();

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("clientToken")]
        public string ClientToken { get; set; } = Guid.NewGuid().ToString();

        public override string ToString()
        {
            //makes it so the ToString method converts this class to a json
            return JsonConvert.SerializeObject(this);
        }
    }

    public class SelectedProfile
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class AvailableProfile
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class MinecraftLogin
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("clientToken")]
        public string ClientToken { get; set; }

        [JsonProperty("selectedProfile")]
        public SelectedProfile SelectedProfile { get; set; }

        [JsonProperty("availableProfiles")]
        public List<AvailableProfile> AvailableProfiles { get; set; }
    }

    public class MinecraftInvalidation
    {
        public MinecraftInvalidation(string accessToken, string clientToken)
        {
            AccessToken = accessToken;
            ClientToken = clientToken;
        }
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("clientToken")]
        public string ClientToken { get; set; }

        public override string ToString()
        {
            //makes it so the ToString method converts this class to a json
            return JsonConvert.SerializeObject(this);
        }
    }
    #endregion
}
