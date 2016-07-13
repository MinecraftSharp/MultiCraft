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
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MultiCraft.Minecraft;
using MultiCraft.Properties;
using Newtonsoft.Json;

namespace MultiCraft.Account
{
    public static class AccountStore
    {
        public static AccountResult AddAccount(string minecraftUser, string minecraftPass, string encodingPassword, bool storePass)
        {
            //create hash of encodingPassword
            string sha1Password;
            using (var sha1 = new SHA1Managed())
            {

                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(encodingPassword));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (var b in hash)
                {
                    sb.Append(b.ToString("X2"));
                }

                sha1Password = sb.ToString();
            }
            //Account data
            AccountJSON accJSON;

            //See if there are already accounts stored
            if (!string.IsNullOrEmpty(Settings.Default.AccountJson))
            {
                //Load data to see if encryption pass is correct
                accJSON = JsonConvert.DeserializeObject<AccountJSON>(Settings.Default.AccountJson);
                if (accJSON.EncryptedSHA1 != sha1Password)
                    return AccountResult.WrongPass;
            }
            //if no accounts stored than set password
            else
            {
                //No password is set, set it
                accJSON = new AccountJSON {EncryptedSHA1 = sha1Password};
            }
            //Prepare class for encryption
            var accData = new AccountJSONEncoded();
            var login = new MinecraftLogin();

            //Do not add if the pass is wrong, ignore encryption password too
            if (Auth.Login(minecraftUser, minecraftPass, ref login) != MinecraftAuth.Success)
                return AccountResult.WrongPass;

            //Add the data needed
            accData.InGameName = login.SelectedProfile.Name;
            accData.LoginName = minecraftUser;
            accData.MinecraftUserID = login.SelectedProfile.Id;
            if (storePass)
                accData.LoginPassword = minecraftPass;

            //Invalidate access token
            Auth.Invalidate(login.AccessToken, login.ClientToken);

            //Encrypt and add data to be stored
            accJSON.AccountJSONEncoded.Add(JsonConvert.SerializeObject(accData).EncryptDES(encodingPassword));

            //Add data to settings
            Settings.Default.AccountJson = JsonConvert.SerializeObject(accJSON);

            //Save settings
            Settings.Default.Save();
            return AccountResult.Success;
        }
        
        public static string EncryptDES(this string originalString, string password)
        {
            //encrypt using DES
            var cryptoProvider = new DESCryptoServiceProvider();
            var memoryStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memoryStream,
                cryptoProvider.CreateEncryptor(Encoding.ASCII.GetBytes(password).Take(8).ToArray(), Encoding.ASCII.GetBytes(password).MD5Hash()), CryptoStreamMode.Write);
            var writer = new StreamWriter(cryptoStream);
            writer.Write(originalString);
            writer.Flush();
            cryptoStream.FlushFinalBlock();
            writer.Flush();
            return Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
        }

        public static string DecryptDES(this string cryptedString, string password)
        {
            //decrypt using DES
            var cryptoProvider = new DESCryptoServiceProvider();
            var memoryStream = new MemoryStream(Convert.FromBase64String(cryptedString.Replace(' ', '+')));
            var cryptoStream = new CryptoStream(memoryStream,
                cryptoProvider.CreateDecryptor(Encoding.ASCII.GetBytes(password).Take(8).ToArray(), Encoding.ASCII.GetBytes(password).MD5Hash()), CryptoStreamMode.Read);
            using (var reader = new StreamReader(cryptoStream))
            {
                var x = reader.ReadToEnd();
                cryptoProvider.Dispose();
                memoryStream.Dispose();
                return x;
            }
        }

        public static byte[] MD5Hash(this byte[] inputBytes)
        {
            //Simple way to create a MD5 Hash
            var md5 = MD5.Create();
            return md5.ComputeHash(inputBytes);
        }

        /// <summary>
        /// Retrieves stored accounts
        /// </summary>
        /// <param name="encodingPassword">the encoding password</param>
        /// <param name="accoundData">the result</param>
        /// <returns>AccountResult</returns>
        public static AccountResult GetAccounts(string encodingPassword, ref List<AccountJSONEncoded> accoundData)
        {
            //Get data
            var accStore = JsonConvert.DeserializeObject<AccountJSON>(Settings.Default.AccountJson);

            //Prevent wrong pass from being used
            using (var sha1 = new SHA1Managed())
            {

                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(encodingPassword));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (var b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                if (accStore.EncryptedSHA1 != sb.ToString())
                    return AccountResult.WrongPass;
            }
            //Decrypt data and add to range
            accoundData.AddRange(accStore.AccountJSONEncoded.Select(data => JsonConvert.DeserializeObject<AccountJSONEncoded>(data.DecryptDES(encodingPassword))));
            return AccountResult.Success;
        }
    }

    public enum AccountResult
    {
        WrongPass,
        Success
    }

    public class AccountJSON
    {
        [JsonProperty("encryptedsha1")]
        public string EncryptedSHA1 { get; set; }

        [JsonProperty("accountJSONEncoded")]
        public List<string> AccountJSONEncoded { get; set; } = new List<string>();
    }

    public class AccountJSONEncoded
    {
        [JsonProperty("loginName")]
        public string LoginName { get; set; }

        [JsonProperty("loginPassword")]
        public string LoginPassword { get; set; }

        [JsonProperty("mcUID")]
        public string MinecraftUserID { get; set; }

        [JsonProperty("ign")]
        public string InGameName { get; set; }
    }
}
