using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Security.Policy;
using System.Windows.Forms;

namespace WOWApi
{
    public class Config
    {
        public string ClientID = "A";
        public string ClientSecret = "B";
        public string TSMKey = "C";

        public string DefaultSearchProfile = "";
        public bool StartupFlagRealms = false;
        public bool StartupLoadRealms = false;
        public bool StartupDoSearch = false;
        public bool StartupEnableDebug = false;

        public double FastCopySpeed = 0.6;
        public double SlowCopySpeed = 1;

        public string SourcePath = "";
        public string SourcePathBackup = "";

        public List<Team> Teams = new List<Team>();
        public List<CopyObject> CopyObjects = new List<CopyObject>();
        public List<Realm> Realms = new List<Realm>();
        public List<AuctionList> AuctionLists = new List<AuctionList>();
        public List<SearchProfile> SearchProfiles = new List<SearchProfile>();

        private Color _highlight1 = Color.White;
        private Color _highlight2 = Color.White;
        private Color _highlight3 = Color.White;
        private Color _highlight4 = Color.White;
        private Color _highlight5 = Color.White;

        public string CustomFSearch1 = String.Empty;
        public string CustomFSearch2 = String.Empty;
        public string CustomFSearch3 = String.Empty;
        public string CustomFSearch4 = String.Empty;
        public string CustomFSearch5 = String.Empty;
        public string CustomFSearch6 = String.Empty;
        public string CustomFSearch7 = String.Empty;
        public string CustomFSearch8 = String.Empty;
        public string CustomFSearch9 = String.Empty;
        public string CustomFSearch10 = String.Empty;
        public string CustomFSearch11 = String.Empty;
        public string CustomFSearch12 = String.Empty;

        [XmlIgnore]
        public Color Highlight1
        {
            get { return _highlight1; }
            set { _highlight1 = value; }
        }

        [XmlElement("Highlight1")]
        public string Highlight1Html
        {
            get { return ColorTranslator.ToHtml(_highlight1); }
            set { _highlight1 = ColorTranslator.FromHtml(value); }
        }

        [XmlIgnore]
        public Color Highlight2
        {
            get { return _highlight2; }
            set { _highlight2 = value; }
        }

        [XmlElement("Highlight2")]
        public string Highlight2Html
        {
            get { return ColorTranslator.ToHtml(_highlight2); }
            set { _highlight2 = ColorTranslator.FromHtml(value); }
        }

        [XmlIgnore]
        public Color Highlight3
        {
            get { return _highlight1; }
            set { _highlight1 = value; }
        }

        [XmlElement("Highlight3")]
        public string Highlight3Html
        {
            get { return ColorTranslator.ToHtml(_highlight3); }
            set { _highlight3 = ColorTranslator.FromHtml(value); }
        }

        [XmlIgnore]
        public Color Highlight4
        {
            get { return _highlight4; }
            set { _highlight4 = value; }
        }

        [XmlElement("Highlight4")]
        public string Highlight4Html
        {
            get { return ColorTranslator.ToHtml(_highlight4); }
            set { _highlight4 = ColorTranslator.FromHtml(value); }
        }

        [XmlIgnore]
        public Color Highlight5
        {
            get { return _highlight5; }
            set { _highlight5 = value; }
        }

        [XmlElement("Highlight5")]
        public string Highlight5Html
        {
            get { return ColorTranslator.ToHtml(_highlight5); }
            set { _highlight5 = ColorTranslator.FromHtml(value); }
        }

        public string ToXml()
        {
            return XmlHelper.SerializeToString(this);
        }

        public Realm FindRealmById(long connectedRealmId)
        {
            Realm rl = new Realm();
            foreach(Realm searchRealm in Realms)
            {
                if (searchRealm.ConnectedRealmId == connectedRealmId)
                {
                    rl = searchRealm;
                    break;
                }
            }

            return rl;
        }


        public void Save()
        {
            SaveToFile(Paths.ConfigPath);
        }

        public void SaveToFile(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            System.IO.FileStream file = System.IO.File.Create(filePath);
            serializer.Serialize(file, this);
            file.Close();
        }

        public static Config Load()
        {
            return Config.LoadFromFile(Paths.ConfigPath);
        }
        public static Config LoadFromFile(string fileName)
        {
            Config returnRI = new Config();
            System.IO.Stream file = File.OpenRead(fileName);
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            returnRI = (Config)serializer.Deserialize(file);
            file.Close();
            return returnRI;
        }

        public static Config LoadFromXml(string xml)
        {
            if (xml == String.Empty)
            {
                return null;
            }
            else
            {
                Config returnScript = new Config();
                var serializer = new XmlSerializer(typeof(Config));
                using (var reader = new StringReader(xml))
                {
                    returnScript = (Config)serializer.Deserialize(reader);
                }
                return returnScript;
            }
        }
    }

    public class CopyObject
    {
        [XmlAttribute("CopyText")]
        public string CopyText = "Unknown";
        [XmlAttribute("ShortName")]
        public string ShortName = "Unknown";
        [XmlAttribute("Active")]
        public bool Active = true;
        private Color _buttonColor = Color.FromArgb(21,21,21);

        [XmlIgnore]
        public Color ButtonColor
        {
            get { return _buttonColor; }
            set { _buttonColor = value; }
        }

        [XmlAttribute("ButtonColor")]
        public string ButtonColorHtml
        {
            get { return ColorTranslator.ToHtml(_buttonColor); }
            set { _buttonColor = ColorTranslator.FromHtml(value); }
        }

    }

    public class Team
    {
        [XmlAttribute("Name")]
        public string Name = "Unknown";
        [XmlElement("ProcessId")]
        public int ProcessId = 0;
        [XmlElement("BNetAccount")]
        public string BNetAccount = "Unknown";
        [XmlElement("WoWAccount")]
        public string WoWAccount = "Unknown";
        [XmlElement("Active")]
        public bool Active = true;

        private Color _toolBarColor = Color.White;
        private Color _searchBarColor = Color.Black;
        private Color _auctionBarColor = Color.Black;
        private Color __formBackColor = Color.Black;

        [XmlIgnore]
        public Color ToolBarColor
        {
            get { return _toolBarColor; }
            set { _toolBarColor = value; }
        }

        [XmlElement("ToolBarColor")]
        public string ToolBarColorHtml
        {
            get { return ColorTranslator.ToHtml(_toolBarColor); }
            set { _toolBarColor = ColorTranslator.FromHtml(value); }
        }

        [XmlIgnore]
        public Color SearchBarColor
        {
            get { return _searchBarColor; }
            set { _searchBarColor = value; }
        }

        [XmlElement("SearchBarColor")]
        public string SearchBarColorHtml
        {
            get { return ColorTranslator.ToHtml(_searchBarColor); }
            set { _searchBarColor = ColorTranslator.FromHtml(value); }
        }

        [XmlIgnore]
        public Color AuctionBarColor
        {
            get { return _auctionBarColor; }
            set { _auctionBarColor = value; }
        }

        [XmlElement("AuctionBarColor")]
        public string AuctionBarColorHtml
        {
            get { return ColorTranslator.ToHtml(_auctionBarColor); }
            set { _auctionBarColor = ColorTranslator.FromHtml(value); }
        }

        [XmlIgnore]
        public Color FormBackColor
        {
            get { return __formBackColor; }
            set { __formBackColor = value; }
        }

        [XmlElement("FormBackColor")]
        public string FormBackColorHtml
        {
            get { return ColorTranslator.ToHtml(__formBackColor); }
            set { __formBackColor = ColorTranslator.FromHtml(value); }
        }

        private void SetProcess(int processId)
        {
            ProcessId = processId;
        }

        private int GetProcess()
        {
            return Win32.CheckProcessId(ProcessId);
        }
    }

    public class AuctionListItem
    {
        [XmlAttribute("Name")]
        public string Name = "Unknown";
        [XmlAttribute("Id")]
        public long Id;
        [XmlAttribute("MaxGold")]
        public long MaxGold;
    }

    public class AuctionList
    {
        [XmlAttribute("Name")]
        public string Name = "Unknown";
        [XmlAttribute("MaxListGold")]
        public long MaxListGold = 0;
        public List<AuctionListItem> AuctionListItems = new List<AuctionListItem>();

    }


    public class SearchProfile
    {
        [XmlAttribute("ProfileName")]
        public string ProfileName = String.Empty;
        [XmlAttribute("QuickSearch")]
        public int QuickSearch = 0;
        [XmlAttribute("Highlight")]
        public int Highlight = 0;
        [XmlAttribute("ShortName")]
        public string ShortName = String.Empty;
        [XmlAttribute("IconIndex")]
        public int IconIndex = 0;
        [XmlAttribute("SearchPercent")]
        public float SearchPercent = 1;
        [XmlAttribute("MaxG")]
        public int MaxG = 200;
        [XmlAttribute("WorthAtLeast")]
        public int WorthAtLeast = 20000;
        [XmlAttribute("MinSellRate")]
        public float MinSellRate = -1;
        [XmlAttribute("OnlySearchNewData")]
        public bool OnlySearchNewData = true;
        [XmlAttribute("OnlyLatestXpac")]
        public bool OnlyLatestXpac = false;
        [XmlAttribute("IncludeItems")]
        public bool IncludeItems = true;
        [XmlAttribute("IncludePets")]
        public bool IncludePets = false;
        [XmlAttribute("ItemFrequency")]
        public int ItemFrequency = 0;
        [XmlAttribute("ItemQuality")]
        public string ItemQuality = "1110000";
        [XmlAttribute("ItemType")]
        public string ItemType = "11111111111111";
        [XmlAttribute("Bonuses")]
        public string Bonuses = "00";
        [XmlAttribute("SearchType")]
        public int SearchType = 0;
        [XmlAttribute("SearchString")]
        public string SearchString = String.Empty;
        [XmlAttribute("AuctionList")]
        public string AuctionList = String.Empty;
        [XmlAttribute("MinItemLevel")]
        public int MinItemLevel = 0;
        [XmlAttribute("MaxItemLevel")]
        public int MaxItemLevel = 0;
        [XmlAttribute("Socket")]
        public bool Socket = false;

        private Color _titleColor = Color.FromArgb(51, 51, 51);
        private Color _panelColor = Color.FromArgb(21, 21, 21);

        [XmlIgnore]
        public Color TitleColor
        {
            get { return _titleColor; }
            set { _titleColor = value; }
        }

        [XmlAttribute("TitleColor")]
        public string TitleColorHtml
        {
            get { return ColorTranslator.ToHtml(_titleColor); }
            set { _titleColor = ColorTranslator.FromHtml(value); }
        }

        [XmlIgnore]
        public Color PanelColor
        {
            get { return _panelColor; }
            set { _panelColor = value; }
        }

        [XmlAttribute("PanelColor")]
        public string PanelColorHtml
        {
            get { return ColorTranslator.ToHtml(_panelColor); }
            set { _panelColor = ColorTranslator.FromHtml(value); }
        }
    }

    public class Realm
    {
        [XmlIgnore]
        public int Status = 0;
        [XmlIgnore]
        public string LastModified = String.Empty;

        [XmlAttribute("RealmName")]
        public string RealmName;
        [XmlAttribute("Highlight")]
        public string Highlight = "0";
        [XmlAttribute("SendTo")]
        public string SendTo = "NA";
        [XmlAttribute("Area")]
        public string Area;
        [XmlAttribute("ConnectedRealmId")]
        public int ConnectedRealmId;
        [XmlAttribute("Team")]
        public string Team;
        [XmlAttribute("Account")]
        public string Account;
        [XmlAttribute("Active")]
        public bool Active = true;
        [XmlAttribute("Flagged")]
        public bool Flagged = true;
        [XmlIgnore]
        public int NumAuctions = 0;
        [XmlIgnore]
        public Color NumAuctionColor = Color.White;
        [XmlIgnore]
        public ListView RealmsView = null;
        [XmlIgnore]
        public ListView AuctionsView = null;

        private Color _backColor = Color.FromArgb(31, 31, 31);
        private Color _foreColor = Color.FromArgb(255, 255, 255);

        [XmlIgnore]
        public Color BackColor
        {
            get { return _backColor; }
            set { _backColor = value; }
        }

        [XmlAttribute("BackColor")]
        public string BackColorHtml
        {
            get { return ColorTranslator.ToHtml(_backColor); }
            set { _backColor = ColorTranslator.FromHtml(value); }
        }

        [XmlIgnore] 
        public Color ForeColor
        {
            get { return _foreColor; }
            set { _foreColor = value; }
        }

        [XmlAttribute("ForeColor")]
        public string ForeColorHtml
        {
            get { return ColorTranslator.ToHtml(_foreColor); }
            set { _foreColor = ColorTranslator.FromHtml(value); }
        }
    }
}

