using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static System.Collections.Specialized.BitVector32;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Security.Policy;

namespace WOWApi
{
    public static class API_Blizzard
    {
        public static string GetAccessToken(string clientId, string clientSecret)
        {
            var client = new RestClient("https://us.battle.net/oauth/token");
            var request = new RestRequest(); 
            request.Method = Method.Post;

            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", $"grant_type=client_credentials&client_id={clientId}&client_secret={clientSecret}", ParameterType.RequestBody);
            RestResponse response = client.Execute(request);

            var tokenResponse = JsonConvert.DeserializeObject<AccessTokenResponse>(response.Content);

            return tokenResponse.access_token;
        }



        public static string GetGoldPrice(string token)
        {
            var client = new RestClient("https://us.api.blizzard.com");
            var request = new RestRequest($"/data/wow/token/?namespace=dynamic-us&access_token=" + token, Method.Get);

            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
            RestResponse response = client.Execute(request);
            var data = JsonConvert.DeserializeObject<WowApiResponse>(response.Content);

            return (data.price / 10000).ToString();
        }

        public static AuctionFileContents GetAuctionsFromAPI(string token, Realm r, out HttpStatusCode statusCode, out string lastModified)
        {
            var client = new RestClient("https://us.api.blizzard.com");
            var request = new RestRequest($"/data/wow/connected-realm/" + r.ConnectedRealmId.ToString() + "/auctions", Method.Get);
            AuctionFileContents afc = new AuctionFileContents();
            
            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
            request.AddParameter("namespace", "dynamic-us");
            request.AddParameter("locale", "en_US");
            //request.AddParameter("access_token", token);



            if (r.LastModified != String.Empty)
            {
                request.AddHeader("If-Modified-Since", r.LastModified);
            }
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            request.AddHeader("Authorization", $"Bearer {token}");

            //var authString = "jwt token";
            //request.Headers.Add("Authorization", $"Bearer {authString}");

            RestResponse response = client.Execute(request);

            

            //need to add If-Modified-Since
            //If r.LastModified isn't an empty string


            statusCode = response.StatusCode;


            //File.WriteAllText(Paths.AuctionDataPath + r.ConnectedRealmId.ToString() + ".json", response.Content);

            try
            {
                afc = JsonConvert.DeserializeObject<AuctionFileContents>(response.Content);
            }
            catch { };

            Dictionary<string, string> headersList = new Dictionary<string, string>();

            foreach (HeaderParameter item in response.ContentHeaders)
            {                
                if (!headersList.ContainsKey(item.Name))
                {
                    headersList.Add(item.Name, item.Value.ToString());
                }
            }

            headersList.TryGetValue("Last-Modified", out lastModified);

            //if (r.LastModified != lastModified)
            //{
            //    DeleteRealmAuctionData(r);
            //    File.WriteAllText(Paths.AuctionDataPath + r.ConnectedRealmId.ToString() + "=" + lastModified.Replace(":","+") + ".json", response.Content);
            //}
           
            return afc;
        }

        public static void DeleteRealmAuctionData(Realm r)
        {
            DirectoryInfo auctionDataDirectory = new DirectoryInfo(Paths.AuctionDataPath);

            foreach (FileInfo fi in auctionDataDirectory.GetFiles())
            {
                if (fi.Name.Contains(r.ConnectedRealmId.ToString() + "="))
                {
                    fi.Delete();
                    break;
                }
            }
        }

        public static AuctionFileContents GetAuctionsFromFile(Realm r, out string lastModified)
        {
            lastModified = String.Empty;
            DirectoryInfo auctionDataDirectory = new DirectoryInfo(Paths.AuctionDataPath);

            foreach(FileInfo fi in auctionDataDirectory.GetFiles())
            {
                if (fi.Name.Contains(r.ConnectedRealmId.ToString() + "="))
                {
                    lastModified = GetSubstringAfterEquals(fi.Name).Replace(".json", "").Replace("+", ":");
                    return JsonConvert.DeserializeObject<AuctionFileContents>(File.ReadAllText(fi.FullName));
                }
            }
            return null;
        }

        public static string GetSubstringAfterEquals(string auctionFileString)
        {
            const int indexCharLength = 1;//Length of the character we want to skip over for the substring
            const char indexChar = '=';//Character that we want to find the index to
            int substringStartIndex = auctionFileString.IndexOf(indexChar) + indexCharLength;//Get an index of the hyphen plus 1 to get the start index just after the hyphen
            string returnAuction = auctionFileString.Substring(substringStartIndex);//Start index is determined by IndexOf then the rest of the string is returned in the substring
            return returnAuction;
        }

        public static BlizzItem GetBlizzItemFromItemId(string token, long itemId)
        {
            var client = new RestClient("https://us.api.blizzard.com");
            var request = new RestRequest($"/data/wow/item/" + itemId.ToString(), Method.Get);

            string returnString = String.Empty;

            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
            request.AddParameter("namespace", "static-us");
            request.AddParameter("locale", "en_US");
            request.AddParameter("access_token", token);
            RestResponse response = client.Execute(request);

            try
            {
                BlizzItem ba = JsonConvert.DeserializeObject<BlizzItem>(response.Content);
                return ba;
            }
            catch
            {
                BlizzItem ba = new BlizzItem();
                ba.id = itemId;
                ba.name = "{Serialization error}";
                return ba;
            }

        }

        public static BlizzPet GetBlizzPetFromPetId(string token, long petId)
        {
            var client = new RestClient("https://us.api.blizzard.com");
            var request = new RestRequest($"/data/wow/pet/" + petId.ToString(), Method.Get);

            string returnString = String.Empty;

            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
            request.AddParameter("namespace", "static-us");
            request.AddParameter("locale", "en_US");
            request.AddParameter("access_token", token);
            RestResponse response = client.Execute(request);

            try
            {
                BlizzPet ba = JsonConvert.DeserializeObject<BlizzPet>(response.Content);
                return ba;
            }
            catch
            {
                BlizzPet ba = new BlizzPet();
                ba.id = petId;
                ba.name = "{Serialization error}";
                return ba;
            }

        }

        public static string GetRealmIds(string token)
        {
            var client = new RestClient(@"https://us.api.blizzard.com/data/wow/connected-realm/index");
            var request = new RestRequest();

            request.Method = Method.Get;
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("authorization", $"Bearer {token}");
            RestResponse response = client.Execute(request);

            return response.Content;
        }
    }

    public class WowApiResponse
    {
        public long price { get; set; }
    }

    public class BlizzItem
    {
        public long id;
        public string name;

        public ItemClass item_class;
        public ItemSubClass item_subclass;
        public Quality quality;
        public InventoryType inventory_type;

        public long level;
        public long required_level;
    }

    public class BlizzBattlePetType
    {
        public long id;
        public string type;
        public string name;
    }

    public class BlizzPet
    {
        public long id;
        public string name;
        public string description;

        public BlizzBattlePetType battle_pet_type;

        public bool is_capturable;
        public bool is_tradable;
        public bool is_battlepet;
        public bool is_alliance_only;
        public bool is_horde_only;
    }

    public class ItemClass
    {
        public string name;
        public int id;
    }

    public class Quality
    {
        public string type;
        public string name;
    }

    public class InventoryType
    {
        public string name;
        public string type;
    }

    public class ItemSubClass
    {
        public string name;
        public int id;
    }

    /*

    public class AuctionApiResponse
    {
        public List<AuctionFile> files { get; set; }
    }

    public class AuctionFile
    {
        public string url { get; set; }
        public long lastModified { get; set; }
    }
    */

    public class AuctionFileContents
    {
        public List<Auction> auctions { get; set; }
    }

    public class Auction
    {
        public int id { get; set; } // This is the item's ID
        public long buyout { get; set; } // This is the buyout price in silver now?
        public AuctionItem item;
    }

    public class SmartAuction
    {
        public TsmItem SmartRegionItem;
        public Auction SmartAuctionItem;
        public Item SmartCachedItem;
        public System.Drawing.Color SmartRowColor;
    }
    
    public class AuctionItem
    {
        public long id;
        public List<AuctionModifiers> modifiers;
        public List<long> bonus_lists;

        public long pet_breed_id = 0;
        public long pet_level = 0;
        public long pet_quality_id = 0;
        public long pet_species_id = 0;
    }

    public class AuctionModifiers
    {
        public long type;
        public long value;
    }

    public class AccessTokenResponse
    {
        public string access_token { get; set; }
    }
}
