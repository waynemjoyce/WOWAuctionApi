using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WOWApi
{
    public static class Paths
    {
        public static string XmlPath = Environment.CurrentDirectory + @"\Xml\";
        public static string JsonPath = Environment.CurrentDirectory + @"\Json\";
        public static string AuctionDataPath = JsonPath + @"AuctionData\";
        public static string SavedSearchPath = XmlPath + @"SavedSearches.xml";
        public static string ConfigPath = XmlPath + "Config.xml";
        public static string ItemCachePath = XmlPath + @"ItemCache\ItemCache.xml";
        public static string ItemCacheBackupPath = XmlPath + @"ItemCache\ItemCache_Backup_DDD.xml";
        public static string PetCachePath = XmlPath + @"PetCache\PetCache.xml";
        public static string PetCacheBackupPath = XmlPath + @"PetCache\PetCache_Backup_DDD.xml";
        public static string TsmRegionDataPath = JsonPath + "TsmRegionData.json";
        public static string TsmRegionDataBackupPath = JsonPath + "TsmRegionData_Backup_DDD.json";

        public static int NumberRealms = 0;
    }
}
