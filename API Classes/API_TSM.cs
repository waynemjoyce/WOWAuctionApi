using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using WOWApi.Helpers;
using Newtonsoft.Json.Linq;
using System.IO;

namespace WOWApi
{
    public static class API_TSM
    {
        public static string GetAccessToken(string tsmKey)
        {
            var client = new RestClient("https://auth.tradeskillmaster.com/oauth2/token");
            var request = new RestRequest();
            request.Method = Method.Post;

            request.AddParameter("client_id", "c260f00d-1071-409a-992f-dda2e5498536");
            request.AddParameter("grant_type", "api_token");
            request.AddParameter("scope", "app:realm-api app:pricing-api");
            request.AddParameter("token", tsmKey);

            RestResponse response = client.Execute(request);

            var tokenResponse = JsonConvert.DeserializeObject<AccessTokenResponse>(response.Content);

            return tokenResponse.access_token;
        }

        public static List<TsmItem> GetRegionTsmItemsFromFile()
        {
            string regionItems = File.ReadAllText(Paths.TsmRegionDataPath);

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            return JsonConvert.DeserializeObject<List<TsmItem>>(regionItems, settings);

        }

        public static void WriteRegionTsmItems(string access_token)
        {
            var client = new RestClient("https://pricing-api.tradeskillmaster.com/");
            var request = new RestRequest($"/region/1", Method.Get);

            string returnString = String.Empty;

            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
            request.AddParameter("format", "json");
            request.AddHeader("authorization", $"Bearer {access_token}");
            RestResponse response = client.Execute(request);

            File.WriteAllText(Paths.TsmRegionDataPath, response.Content);
        }
    }


    public class TsmItem
    {
        public int regionId;
        public string itemId;
        public string petSpeciesId;
        public float quantity;
        public long marketValue;
        public long avgSalePrice;
        public float saleRate;
        public long soldPerDay;
        public long historical;
    }

    public class TsmItemOld
    {
        public int Id { get; set; }
        public string Realm { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public string Class { get; set; }
        public string SubClass { get; set; }
        public long VendorBuy { get; set; }
        public long VendorSell { get; set; }
        public long MarketValue { get; set; }
        public long MinBuyout { get; set; }
        public long Quantity { get; set; }
        public long NumAuctions { get; set; }
        public long HistoricalPrice { get; set; }
        public long RegionMarketAvg { get; set; }
        public long RegionMinBuyoutAvg { get; set; }
        public long RegionQuantity { get; set; }
        public long RegionHistoricalPrice { get; set; }
        public long RegionSaleAvg { get; set; }
        public long RegionAvgDailySold { get; set; }
        public long RegionSaleRate { get; set; }
        public string URL { get; set; }
        public int LastModified { get; set; }

        public override string ToString()
        {
            return $"{Name}({Id}) : MkPrice({StringHelper.FormatItemPrice(MarketValue)})";
        }
    }

}
