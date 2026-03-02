using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace WOWApi
{
    public class ItemCache
    {
        //public Dictionary<long, Item> Items = new Dictionary<long, Item>();
        //public Dictionary<long, Item> Items = new Dictionary<long, Item>();
        public List<Item> Items = new List<Item>();

        [XmlIgnore]
        public List<long> ItemIds = new List<long>();

        public void Save()
        {
            SaveToFile(Paths.ItemCachePath);
        }

        public void ClearItems()
        {
            Items.Clear();
        }

        public void AddItem(Item itemToAdd)
        {
            //Items.Add(itemToAdd.ItemId, itemToAdd);
            Items.Add(itemToAdd);
        }

        public void SaveToFile(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ItemCache));
            System.IO.FileStream file = System.IO.File.Create(filePath);
            serializer.Serialize(file, this);
            file.Close();
        }

        public void FillItemIds()
        {
            ItemIds.Clear();

            foreach (Item it in Items)
            {
                ItemIds.Add(it.Id);
            }
        }

        public static ItemCache Load()
        {
            return ItemCache.LoadFromFile(Paths.ItemCachePath);
        }
        public static ItemCache LoadFromFile(string fileName)
        {
            ItemCache returnIc = new ItemCache();
            System.IO.Stream file = File.OpenRead(fileName);
            XmlSerializer serializer = new XmlSerializer(typeof(ItemCache));
            returnIc = (ItemCache)serializer.Deserialize(file);
            file.Close();
            return returnIc;
        }

        public static ItemCache LoadFromXml(string xml)
        {
            if (xml == String.Empty)
            {
                return null;
            }
            else
            {
                ItemCache returnScript = new ItemCache();
                var serializer = new XmlSerializer(typeof(Config));
                using (var reader = new StringReader(xml))
                {
                    returnScript = (ItemCache)serializer.Deserialize(reader);
                }
                return returnScript;
            }
        }

    }

    public class Item
    {
        [XmlAttribute]
        public long Id;
        [XmlAttribute]
        public string Name;
        [XmlAttribute]
        public string ClassName;
        [XmlAttribute]
        public int ClassId;
        [XmlAttribute]
        public string SubClassName;
        [XmlAttribute]
        public int SubClassId;
        [XmlAttribute]
        public string QualityType;
        [XmlAttribute]
        public string InventoryType;
        [XmlAttribute]
        public string BindingType;
        [XmlAttribute]
        public long Level;
        [XmlAttribute]
        public long RequiredLevel;
    }
}
