
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WOWApi.Helpers;
using System.Threading;
using System.Net;
using System.IO;
using System.Timers;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Web;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Serialization;
using WOWAuctionApi.Properties;
using System.Resources;
using System.Security.AccessControl;
using System.Linq;
using RoboSharp;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using RoboSharp.Results;

namespace WOWApi
{

    public partial class FormMain : Form
    {
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);
        private const int WM_SETREDRAW = 11;

        public Config apiConfig;

        public Dictionary<int, AuctionFileContents> RealmAuctions = new Dictionary<int, AuctionFileContents>();
        public Dictionary<long, TsmItem> RegionItems = new Dictionary<long, TsmItem>();
        public Dictionary<long, TsmItem> RegionPets = new Dictionary<long, TsmItem>();
        public SortedDictionary<long, Item> DictionaryItemCache = new SortedDictionary<long, Item>();
        public SortedDictionary<long, Pet> DictionaryPetCache = new SortedDictionary<long, Pet>();

        public Dictionary<long, Item> SpecialItemCache = new Dictionary<long, Item>();

        AuctionEvent.AuctionRetrievedEventHandler auctionEventDelegate;
        SearchParameters searchParams = new SearchParameters();

        private ListViewColumnSorter lvwAuctionColumnSorter;
        private IntegerComparer lvwRealmColumnSorter;

        private Team blueTeam;
        private Team greenTeam;

        System.Windows.Forms.ListView ActiveAuctionsView = null;

        public string accessToken;

        public Realm KeypressRealm = new Realm();

        //------ Search Variables -------
        public string stringText = String.Empty;        
        public Color rowColor = Color.White;
        public bool valueMet = false;
        public bool itemMatch = false;

        //Item variables
        public TsmItem regionItem;
        public Item cachedItem;
        public Dictionary<string, SearchCount> searchItemCounts = new Dictionary<string, SearchCount>();
        public List<string> duplicateItemIds = new List<string>();
        public List<string> uniqueItemIds = new List<string>();

        //Pet variables
        public TsmItem regionPet;
        public Pet cachedPet;

        public List<SearchResult> searchResults = new List<SearchResult>();
        public List<SearchResult> orderResults = new List<SearchResult>();
        public SearchCount countItem;

        public List<long> auctionListIds = new List<long>();
        public Dictionary<long, long> auctionListFull = new Dictionary<long, long>();
        public long auctionListMaxGold = 0;

        public bool livePoll = false;
        //-------------------------------

        public FormMain()
        {
            InitializeComponent();
        }

        private void LoadRegionItems()
        {
            RegionItems.Clear();
            RegionPets.Clear();
            long itemId;

            List<TsmItem> AllRegionItems = API_TSM.GetRegionTsmItemsFromFile();

            foreach (TsmItem item in AllRegionItems)
            {

                if (item.itemId != null)
                {
                    itemId = long.Parse(item.itemId);
                    if (!RegionItems.ContainsKey(itemId))
                    {
                        RegionItems.Add(itemId, item);
                    }
                }
                else if (item.petSpeciesId != null)
                {
                    itemId = long.Parse(item.petSpeciesId);
                    if (!RegionPets.ContainsKey(itemId))
                    {
                        RegionPets.Add(itemId, item);
                    }
                }
            }
        }

        private void btnGetAuctionData_Click(object sender, EventArgs e)
        {
            livePoll = false;
            LoadAuctionDataLive("Blue");
        }

        private void SetRealmStatus(int connectedRealmId, int status, string lastModified, int auctionCount)
        {
            Realm r = apiConfig.FindRealmById(connectedRealmId);

            r.RealmsView.SuspendLayout();
            r.Status = status;
            r.NumAuctions = auctionCount;
            r.NumAuctionColor = GetNumAuctionColor(auctionCount);

            foreach (ListViewItem lvi in r.RealmsView.Items)
            {
                if (lvi.Tag != null)
                {
                    if (((Realm)lvi.Tag).ConnectedRealmId == connectedRealmId)
                    {
                        //Realm status
                        //0 Blue = live data not loaded
                        //1 Red = loading
                        //2 Yellow = old data
                        //3 Green = new data

                        lvi.ImageIndex = status;

                        if (lastModified != String.Empty)
                        {
                            lvi.SubItems[2].Text = DateTime.Parse(lastModified).ToString("hh:mm:ss");
                            lvi.SubItems[3].Text = auctionCount.ToString();
                        }
                    }
                }

            }
            r.RealmsView.ResumeLayout();
        }

        private void SetAuctionData(int connectedRealmId, AuctionFileContents afc, string lastModified, Realm r)
        {
            RealmAuctions[connectedRealmId] = afc;
            int newStatus = 2;
            //We are on a worker thread at this point - we need to somehow marshall this request back to UI thread 
            //or get UI thread to read the date on the file or something

            try
            {
                DateTime lastModifiedDate = DateTime.Parse(lastModified);
                DateTime thresholdDate = DateTime.Now.AddMinutes(-(int.Parse(this.textBoxOldThreshold.Text)));

                if (lastModifiedDate > thresholdDate)
                {
                    newStatus = 3;
                }

                SetRealmStatus(connectedRealmId, newStatus, lastModified, afc.auctions.Count);
                RefreshThreadCount();
            }
            catch
            {
                //SetRealmStatus(connectedRealmId, 1, "ERROR", 0);
            }
        }

        private void FreezeControl(Control ctrl, bool freeze, bool changeVis = true)
        {

            if (changeVis)
            {
                ctrl.Visible = !freeze;
            }
            SendMessage(ctrl.Handle, WM_SETREDRAW, !freeze, 0);
            if (!freeze)
            {
                ctrl.Refresh();
            }
        }

        private void LoadRealmsAtStart()
        {
            //We are just loading at the start, so load from file "stale" blue
            //Status = 0

            lvwRealms.SuspendLayout();
            int count = 0;
            foreach (Realm r in apiConfig.Realms)
            {

                count++;

                ListViewItem lvi = new ListViewItem();
               
                lvi.Text = "";
                lvi.UseItemStyleForSubItems = false;

                lvi.SubItems.Add(r.RealmName);
                lvi.SubItems[1].BackColor = r.BackColor;
                lvi.SubItems[1].ForeColor = r.ForeColor;

                lvi.SubItems.Add("Stale");
                lvi.SubItems[2].BackColor = r.BackColor;
                lvi.SubItems[2].ForeColor = r.ForeColor;

                lvi.SubItems.Add("0");
                lvi.SubItems[3].BackColor = r.BackColor;
                lvi.SubItems[3].ForeColor = r.ForeColor;

                //Realm status
                //0 Blue = live data not loaded
                //1 Red = loading
                //2 Yellow = old data
                //3 Green = new data

                lvi.ImageIndex = 0;
                lvi.Tag = r;
                lvi.Checked = true;
                
                r.RealmsView = lvwRealms;
                r.AuctionsView = lvwAuctionsBlue;
                r.RealmsView.Items.Add(lvi);

                if (Paths.NumberRealms > 0 && count >= Paths.NumberRealms)
                {
                    break;
                }
            }

            lvwRealms.ResumeLayout();
        }

        private void LoadRealmFileData(Realm r)
        {
            string lastModified;
            AuctionFileContents afc = API_Blizzard.GetAuctionsFromFile(r, out lastModified);
            if (afc != null)
            {
                r.LastModified = lastModified;
                SetAuctionData(r.ConnectedRealmId, afc, lastModified, r);
            }
        }

        private int GetRealmStatus(int connectedRealmId)
        {
            int returnValue = 0;
            Realm r = apiConfig.FindRealmById(connectedRealmId);

            foreach (ListViewItem lvi in r.RealmsView.Items)
            {
                if (lvi.Tag != null)
                {
                    if (((Realm)lvi.Tag).ConnectedRealmId == connectedRealmId)
                    {
                        returnValue = lvi.ImageIndex;
                        break;
                    }
                }
            }

            return returnValue;
        }

        private void LoadAuctionDataLive(string team)
        {
            //We are live-requesting auction data
            //Switch statuses to 1 (red) and then request in realm on separate thread
            
            /*
            foreach (Realm r in apiConfig.Realms)
            {
                if (RealmActive(r))
                {
                    SetRealmStatus(r.ConnectedRealmId, 1, "Fetching...", 0);
                }
            }
            */

            //Ok so loop through all realms, fire off that realms' thread and its callback will update the status
            //and then make it searchable
            //RealmAuctions.Clear();

            foreach (Realm r in apiConfig.Realms)
            {
                if (RealmActive(r) && team.Contains(r.Team))
                {
                    //WriteDebug(r.RealmName + " is active");
                    Thread ProcessAuctionsThread = new Thread(() => ProcessAuctionsForRealm(r));

                    ProcessAuctionsThread.SetApartmentState(ApartmentState.STA);
                    ProcessAuctionsThread.Start();
                }
            }
        }

        private void ProcessAuctionsForRealm(Realm r)
        {
            AuctionEvent ae = new AuctionEvent();
            ae.AuctionRetrieved += Ae_AuctionRetrieved;
            ae.DoAuctionProcess(accessToken, r, livePoll);
        }

        private void Ae_AuctionRetrieved(object sender, AuctionEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(auctionEventDelegate, sender, e);
            }
            else
            {
                this.SetAuctionData(e.ConnectedRealmId, e.Auctions, e.LastModified, e.RealmObject);
            }

            //If we are live polling, kick off a search
            if (livePoll)
            {
                //SearchRealm(e.RealmObject);
            }
        }

        private void WriteRegionData()
        {
            Cursor.Current = Cursors.WaitCursor;
            BackupRegionData();
            string tsmAccessToken = API_TSM.GetAccessToken(apiConfig.TSMKey);
            API_TSM.WriteRegionTsmItems(tsmAccessToken);
            Cursor.Current = Cursors.Default;
            MessageBox.Show("Region items updated");
        }

        private void LoadConfig()
        {
            apiConfig = Config.Load();
        }

        private void WriteDebug(string message)
        {
            if (this.togDebug.Checked)
            {
                //txtDebug.Text += DateTime.Now.ToString("f") + ": " + message + "\r\n";
                //txtDebug.Focus();
                //txtDebug.SelectionStart = txtDebug.Text.Length;
                //Application.DoEvents();
            }
        }

        private void CheckWowProcesses()
        {
            blueTeam.ProcessId = Win32.CheckProcessId(blueTeam.ProcessId);
            pnlBlueTeam.BackColor = blueTeam.ToolBarColor;
            lblBlueWOW.BackColor = blueTeam.ToolBarColor;
            apiConfig.Save();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;
            auctionEventDelegate = new AuctionEvent.AuctionRetrievedEventHandler(Ae_AuctionRetrieved);

            lvwRealms.SmallImageList = imgStatus;
            lvwAuctionsBlue.SmallImageList = img16;
            toolStripMain.ImageList = img32;

            LoadConfig();
            blueTeam = GetTeam("Blue");
            greenTeam = GetTeam("Green");
            CheckWowProcesses();
            LoadRegionItems();
            PopulateSearchProfiles();
            PopulateCopyButtons();
            lblBlueWOW.Text = blueTeam.ProcessId.ToString();
            accessToken = API_Blizzard.GetAccessToken(apiConfig.ClientID, apiConfig.ClientSecret);
            WriteDebug("accessToken = " + accessToken);
            //Load item cache
            DictionaryItemCache.Clear();
            ItemCache itemCache = ItemCache.Load();
            foreach (Item item in itemCache.Items)
            {
                DictionaryItemCache.Add(item.Id, item);
            }

            //Load pet cache
            DictionaryPetCache.Clear();
            PetCache petCache = PetCache.Load();
            foreach (Pet pt in petCache.Pets)
            {
                DictionaryPetCache.Add(pt.Id, pt);
            }

            AddSpecialItems();
            LoadRealmsAtStart();
            SetScreen();

            cboAuctionLists.DropDownStyle = ComboBoxStyle.DropDownList;

            foreach (AuctionList al in apiConfig.AuctionLists)
            {
                cboAuctionLists.Items.Add(al.Name);
            }
            if (cboAuctionLists.Items.Count > 0)
            {
                cboAuctionLists.SelectedIndex = 0;
            }

            lvwAuctionColumnSorter = new ListViewColumnSorter();
            this.lvwAuctionsBlue.ListViewItemSorter = lvwAuctionColumnSorter;
            lvwRealmColumnSorter = new IntegerComparer(3);
            this.lvwRealms.ListViewItemSorter = lvwRealmColumnSorter;

            RefreshThreadCount();

            //Startup choices
            if (apiConfig.StartupFlagRealms)
            {
                FlagRealms();
                Application.DoEvents();
            }

            if (apiConfig.StartupLoadRealms)
            {
                livePoll = false;
                //LoadAuctionDataLive();
            }

        }


        private void RefreshThreadCount()
        {
            lblCurrentThreads.Text = System.Diagnostics.Process.GetCurrentProcess().Threads.Count.ToString();
        }
        private void PopulateSearchProfiles()
        {
            int sIndex = 0;
            int qIndex1 = 0;
            int qIndex2 = 0;
            cboSearchProfiles.Items.Clear();

            foreach (SearchProfile sp in apiConfig.SearchProfiles)
            {
                cboSearchProfiles.Items.Add(sp.ProfileName);

                if (sp.ProfileName == apiConfig.DefaultSearchProfile)
                {
                    cboSearchProfiles.Text = sp.ProfileName;
                }


                AddNewSearchButton(sIndex, sp.IconIndex, sp.ShortName, sp.ProfileName, sp.PanelColor);
                /*
                if (sp.QuickSearch == 1)
                {
                    AddNewQuickButton(qIndex1, sp.IconIndex, sp.ShortName, sp.ProfileName, sp.PanelColor, pnlQuickButtons1);
                    qIndex1 += 1;
                }
                if (sp.QuickSearch == 2)
                {
                    AddNewQuickButton(qIndex2, sp.IconIndex, sp.ShortName, sp.ProfileName, sp.PanelColor, pnlQuickButtons1);
                    qIndex2 += 1;
                }
                */
                sIndex += 1;
            }
        }

        private void PopulateCopyButtons()
        {
            int sIndex = 0;

            foreach (CopyObject cobj in apiConfig.CopyObjects)
            {
                AddCopyTextButton(sIndex, 0, cobj.ShortName, cobj.CopyText, cobj.ButtonColor);
                sIndex += 1;
            }
        }

        private void SetSearchProfile(string searchProfile)
        {
            cboSearchProfiles.Text = searchProfile;
        }

        private SearchProfile GetSearchProfileFromCurrent()
        {
            SearchProfile prof = new SearchProfile();

            prof.MaxG = int.Parse(txtSearchMaxG.Text);
            prof.SearchPercent = float.Parse(txtSearchPercentage.Text);
            prof.WorthAtLeast = int.Parse(txtWorthAtLeast.Text);
            prof.MinSellRate = float.Parse(txtMinSellRate.Text);
            prof.Highlight = int.Parse(txtHighlight.Text);

            prof.MinItemLevel = int.Parse(txtMinItemLevel.Text);
            prof.MaxItemLevel = int.Parse(txtMaxItemLevel.Text);

            prof.OnlySearchNewData = togOnlyNewData.Checked;
            prof.OnlyLatestXpac = togOnlyLatestXpac.Checked;
            prof.IncludeItems = togIncludeItems.Checked;
            prof.IncludePets = togIncludePets.Checked;

            //TODO
            rbItemFrequency_RemoveDuplicates.Checked = (prof.ItemFrequency == 0);
            rbItemFrequency_ShowCheapest.Checked = (prof.ItemFrequency == 1);
            rbItemFrequency_ShowAll.Checked = (prof.ItemFrequency == 2);

            //TODO
            rbSearchType_Standard.Checked = (prof.SearchType == 0);
            rbSearchType_String.Checked = (prof.SearchType == 1);
            rbSearchType_List.Checked = (prof.SearchType == 2);
            rbSearchType_ListMaxGold.Checked = (prof.SearchType == 3);

            //TODO
            togItemQuality_Poor.Checked = (prof.ItemQuality[0].ToString() == "1");
            togItemQuality_Common.Checked = (prof.ItemQuality[1].ToString() == "1");
            togItemQuality_Uncommon.Checked = (prof.ItemQuality[2].ToString() == "1");
            togItemQuality_Rare.Checked = (prof.ItemQuality[3].ToString() == "1");
            togItemQuality_Epic.Checked = (prof.ItemQuality[4].ToString() == "1");
            togItemQuality_Legendary.Checked = (prof.ItemQuality[5].ToString() == "1");
            togItemQuality_Artifact.Checked = (prof.ItemQuality[6].ToString() == "1");

            togItemClass_Weapon.Checked = (prof.ItemType[0].ToString() == "1");
            togItemClass_Armor.Checked = (prof.ItemType[1].ToString() == "1");
            togItemClass_Consumable.Checked = (prof.ItemType[2].ToString() == "1");
            togItemClass_Miscellaneous.Checked = (prof.ItemType[3].ToString() == "1");
            togItemClass_Tradeskill.Checked = (prof.ItemType[4].ToString() == "1");
            togItemClass_Profession.Checked = (prof.ItemType[5].ToString() == "1");
            togItemClass_Container.Checked = (prof.ItemType[6].ToString() == "1");
            togItemClass_Quest.Checked = (prof.ItemType[7].ToString() == "1");
            togItemClass_ItemEnhancement.Checked = (prof.ItemType[8].ToString() == "1");
            togItemClass_Recipe.Checked = (prof.ItemType[9].ToString() == "1");
            togItemClass_Gem.Checked = (prof.ItemType[10].ToString() == "1");
            togItemClass_Key.Checked = (prof.ItemType[11].ToString() == "1");
            togItemClass_Glyph.Checked = (prof.ItemType[12].ToString() == "1");
            togItemClass_Reagent.Checked = (prof.ItemType[13].ToString() == "1");

            togBonuses_Speed.Checked = (prof.Bonuses[0].ToString() == "1");
            togBonuses_Leech.Checked = (prof.Bonuses[1].ToString() == "1");

            togSocket.Checked = prof.Socket;

            return prof;
        }

        private void LoadSearchProfile(SearchProfile prof)
        {
            txtSearchMaxG.Text = prof.MaxG.ToString();
            txtSearchPercentage.Text = prof.SearchPercent.ToString();
            txtWorthAtLeast.Text = prof.WorthAtLeast.ToString();
            txtMinSellRate.Text = prof.MinSellRate.ToString();
            txtHighlight.Text = prof.Highlight.ToString();

            txtMinItemLevel.Text = prof.MinItemLevel.ToString();
            txtMaxItemLevel.Text = prof.MaxItemLevel.ToString();

            togOnlyNewData.Checked = prof.OnlySearchNewData;
            togOnlyLatestXpac.Checked = prof.OnlyLatestXpac;
            togIncludeItems.Checked = prof.IncludeItems;
            togIncludePets.Checked = prof.IncludePets;

            rbItemFrequency_RemoveDuplicates.Checked = (prof.ItemFrequency == 0);
            rbItemFrequency_ShowCheapest.Checked = (prof.ItemFrequency == 1);
            rbItemFrequency_ShowAll.Checked = (prof.ItemFrequency == 2);

            rbSearchType_Standard.Checked = (prof.SearchType == 0);
            rbSearchType_String.Checked = (prof.SearchType == 1);
            rbSearchType_List.Checked = (prof.SearchType == 2);
            rbSearchType_ListMaxGold.Checked = (prof.SearchType == 3);

            togItemQuality_Poor.Checked = (prof.ItemQuality[0].ToString() == "1");
            togItemQuality_Common.Checked = (prof.ItemQuality[1].ToString() == "1");
            togItemQuality_Uncommon.Checked = (prof.ItemQuality[2].ToString() == "1");
            togItemQuality_Rare.Checked = (prof.ItemQuality[3].ToString() == "1");
            togItemQuality_Epic.Checked = (prof.ItemQuality[4].ToString() == "1");
            togItemQuality_Legendary.Checked = (prof.ItemQuality[5].ToString() == "1");
            togItemQuality_Artifact.Checked = (prof.ItemQuality[6].ToString() == "1");

            togItemClass_Weapon.Checked = (prof.ItemType[0].ToString() == "1");
            togItemClass_Armor.Checked = (prof.ItemType[1].ToString() == "1");
            togItemClass_Consumable.Checked = (prof.ItemType[2].ToString() == "1");
            togItemClass_Miscellaneous.Checked = (prof.ItemType[3].ToString() == "1");
            togItemClass_Tradeskill.Checked = (prof.ItemType[4].ToString() == "1");
            togItemClass_Profession.Checked = (prof.ItemType[5].ToString() == "1");
            togItemClass_Container.Checked = (prof.ItemType[6].ToString() == "1");
            togItemClass_Quest.Checked = (prof.ItemType[7].ToString() == "1");
            togItemClass_ItemEnhancement.Checked = (prof.ItemType[8].ToString() == "1");
            togItemClass_Recipe.Checked = (prof.ItemType[9].ToString() == "1");
            togItemClass_Gem.Checked = (prof.ItemType[10].ToString() == "1");
            togItemClass_Key.Checked = (prof.ItemType[11].ToString() == "1");
            togItemClass_Glyph.Checked = (prof.ItemType[12].ToString() == "1");
            togItemClass_Reagent.Checked = (prof.ItemType[13].ToString() == "1");

            togBonuses_Speed.Checked = (prof.Bonuses[0].ToString() == "1");
            togBonuses_Leech.Checked = (prof.Bonuses[1].ToString() == "1");

            togSocket.Checked = prof.Socket;

            this.txtStringSearch.Text = prof.SearchString;
            if (prof.AuctionList != String.Empty)
            {
                cboAuctionLists.SelectedItem = prof.AuctionList;
            }

            //picSearchProfile.Image = img32.Images[prof.IconIndex];
        }

        private void AddSpecialItems()
        {
            SpecialItemCache.Clear();

            AddSpecialItem(194641, "Design: Elemental Lariat");
            AddSpecialItem(200911, "Formula: Illusion: Primal Air");
            AddSpecialItem(200912, "Formula: Illusion: Primal Earth");
            AddSpecialItem(200913, "Formula: Illusion: Primal Fire");
            AddSpecialItem(200914, "Formula: Illusion: Primal Frost");
            AddSpecialItem(194640, "Design: Ring-Bound Hourglass");

        }

        private void AddSpecialItem(long itemId, string itemName)
        {
            Item item = new Item();
            item.Id = itemId;
            item.Name = itemName;
            SpecialItemCache.Add(itemId, item);
        }

        private void BuildPetCache(bool updateOnly)
        {
            Cursor.Current = Cursors.WaitCursor;
            BackupPetCache();

            string petName;
            int count = 0;
            int addedCount = 0;
            int regionCount = RegionPets.Count;
            tspCache.Maximum = regionCount;

            PetCache pc = new PetCache();

            if (updateOnly)
            {
                pc = PetCache.Load();
                pc.FillPetIds();
            }
            else
            {
                pc.Pets.Clear();
            }

            foreach (KeyValuePair<long, TsmItem> item in RegionPets)
            {
                count++;

                if (updateOnly == false)
                {
                    tspCache.Value = count;
                    Application.DoEvents();
                }


                try
                {
                    if (((updateOnly) && (!(pc.PetIds.Contains(item.Key))))
                        || (!updateOnly))
                    {
                        addedCount += 1;
                        BlizzPet bp = API_Blizzard.GetBlizzPetFromPetId(accessToken, item.Key);

                        if (bp != null)
                        {
                            if (updateOnly)
                            {
                                tspCache.Value = count;
                                Application.DoEvents();
                            }

                            petName = bp.name;

                            Pet pet1 = new Pet();
                            pet1.Id = item.Key;
                            pet1.Name = petName;
                            pet1.IsTradable = bp.is_tradable;
                            pet1.IsCapturable = bp.is_capturable;
                            pet1.IsHordeOnly = bp.is_horde_only;
                            pet1.IsAllianceOnly = bp.is_alliance_only;
                            pet1.IsBattlePet = bp.is_battlepet;
                            pet1.BattlePetType = bp.battle_pet_type.name;
                            pet1.Description = bp.description;

                            pc.AddPet(pet1);
                        }
                    }
                }
                catch
                { }

            }

            pc.Save();
            BackupPetCache();
            tspCache.Value = tspCache.Maximum;
            Application.DoEvents();
            MessageBox.Show("Finished building pet cache. " + count.ToString() + " region pets scanned, " + addedCount.ToString() + " new pets added.");
            Cursor.Current = Cursors.Default;
        }

        private void BuildItemCache(bool updateOnly)
        {
            Cursor.Current = Cursors.WaitCursor;
            BackupItemCache();

            string itemName;
            int count = 0;
            int addedCount = 0;
            int regionCount = RegionItems.Count;
            tspCache.Maximum = regionCount;

            ItemCache ic = new ItemCache();

            if (updateOnly)
            {
                ic = ItemCache.Load();
                ic.FillItemIds();
            }
            else
            {
                ic.Items.Clear();
            }

            foreach (KeyValuePair<long, TsmItem> item in RegionItems)
            {
                count++;

                if (updateOnly == false)
                {
                    tspCache.Value = count;
                    Application.DoEvents();
                }


                try
                {
                    if (((updateOnly) && (!(ic.ItemIds.Contains(item.Key))))
                        || (!updateOnly))
                    {
                        addedCount += 1;
                        BlizzItem bi = API_Blizzard.GetBlizzItemFromItemId(accessToken, item.Key);

                        if (bi != null)
                        {
                            if (updateOnly)
                            {
                                tspCache.Value = count;
                                //lblItemCache.Text = "Processing " + count.ToString() + " out of " + regionCount.ToString();
                                Application.DoEvents();
                            }

                            itemName = bi.name;

                            Item item1 = new Item();
                            item1.Id = item.Key;
                            item1.Name = itemName;
                            if (bi.item_class != null)
                            {
                                item1.ClassName = bi.item_class.name;
                                item1.ClassId = bi.item_class.id;
                            }

                            if (bi.item_subclass != null)
                            {
                                item1.SubClassName = bi.item_subclass.name;
                                item1.SubClassId = bi.item_subclass.id;
                            }

                            if (bi.quality != null)
                            {
                                item1.QualityType = bi.quality.type;
                            }

                            if (bi.inventory_type != null)
                            {
                                item1.InventoryType = bi.inventory_type.type;
                            }

                            item1.Level = bi.level;
                            item1.RequiredLevel = bi.required_level;

                            ic.AddItem(item1);
                        }
                    }
                }
                catch
                { }

            }

            ic.Save();
            BackupItemCache();
            tspCache.Value = tspCache.Maximum;
            Application.DoEvents();
            MessageBox.Show("Finished building item cache. " + count.ToString() + " region items scanned, " + addedCount.ToString() + " new items added.");
            Cursor.Current = Cursors.Default;

        }

        private void InitializeSearchParameters()
        {
            if (rdSearchMaxG.Checked)
            {
                searchParams.SearchMaxG = long.Parse(this.txtSearchMaxG.Text) * 10000;
                searchParams.WorthAtLeast = long.Parse(txtWorthAtLeast.Text) * 10000;
            }
            else
            {
                searchParams.SearchPercentage = float.Parse(this.txtSearchPercentage.Text) / 100;
            }

            searchParams.MinSellRate = float.Parse(this.txtMinSellRate.Text);
            searchParams.RemovingDuplicates = this.rbItemFrequency_RemoveDuplicates.Checked;
            searchParams.ShowCheapest = this.rbItemFrequency_ShowCheapest.Checked;
            searchParams.ShowAll = this.rbItemFrequency_ShowAll.Checked;
            searchParams.AllItemClasses = AllItemClassesChecked();
            searchParams.Highlight = int.Parse(txtHighlight.Text);

            searchParams.MinItemLevel = long.Parse(txtMinItemLevel.Text);
            searchParams.MaxItemLevel = long.Parse(txtMaxItemLevel.Text);

            searchParams.Socket = togSocket.Checked;
        }

        public void SetScreen()
        {
            foreach (System.Windows.Forms.Screen scr in System.Windows.Forms.Screen.AllScreens)
            {
                if (scr.DeviceName == @"\\.\DISPLAY3")
                {
                    this.WindowState = FormWindowState.Normal;
                    this.Location = scr.Bounds.Location;
                    //this.WindowState = FormWindowState.Maximized;
                    Application.DoEvents();
                }
            }
        }

        private string LXItem(long itemid)
        {
            if (itemid > 215000)
            {
                return "Y";
            }
            else
            {
                return " ";
            }
        }

        private bool IsLXItem(long itemid)
        {
            return (LXItem(itemid) == "Y");
        }

        private bool RealmChecked(int realmId)
        {
            bool retVal = false;
            Realm r = apiConfig.FindRealmById(realmId);

            foreach (ListViewItem lvi in r.RealmsView.Items)
            {
                if (lvi.Tag != null)
                {
                    if (((Realm)lvi.Tag).ConnectedRealmId == realmId)
                    {
                        retVal = lvi.Checked;
                        break;
                    }
                }
            }
            return retVal;
        }

        private void SearchInit()
        {
            WriteDebug("Search Init 1");
            InitializeSearchParameters();

            stringText = String.Empty;

            this.lvwAuctionsBlue.Items.Clear();

            if (rbSearchType_String.Checked)
            {
                if (txtStringSearch.Text.Length > 3)
                {
                    stringText = txtStringSearch.Text;
                }
                else
                {
                    return;
                }
            }
            WriteDebug("Search Init 2");
            searchResults.Clear();
            searchItemCounts.Clear();
            duplicateItemIds.Clear();
            uniqueItemIds.Clear();
            auctionListIds.Clear();
            auctionListFull.Clear();

            WriteDebug("Search Init 3");
            if ((rbSearchType_List.Checked) && (cboAuctionLists.Text != String.Empty))
            {
                WriteDebug("Search Type List");
                foreach (AuctionList al in apiConfig.AuctionLists)
                {
                    if (al.Name == cboAuctionLists.Text)
                    {
                        foreach (AuctionListItem ali in al.AuctionListItems)
                        {
                            auctionListIds.Add(ali.Id);
                        }
                        break;
                    }
                }
            }
            else if ((rbSearchType_ListMaxGold.Checked) && (cboAuctionLists.Text != String.Empty))
            {
                //public Dictionary<long, AuctionListItem> auctionListFull = new Dictionary<long, AuctionListItem>();
                WriteDebug("Search Type List Max Gold");
                foreach (AuctionList al in apiConfig.AuctionLists)
                {
                    if (al.Name == cboAuctionLists.Text)
                    {
                        auctionListMaxGold = al.MaxListGold * 10000;
                        foreach (AuctionListItem ali in al.AuctionListItems)
                        {
                            auctionListFull.Add(ali.Id, ali.MaxGold * 10000);
                        }
                        break;
                    }
                }
            }

            Application.DoEvents();
        }

        private string GetQualityTypeFromNumber(long number)
        {
            switch(number)
            {
                case 0: default: return "POOR";
                case 1: return "COMMON";
                case 2: return "UNCOMMON";
                case 3: return "RARE";
                case 4: return "EPIC";
                case 5: return "LEGENDARY";
                case 6: return "ARTIFACT";
            }
        }

        private long SearchID(string stringId)
        {
            return long.Parse(stringId.Substring(1));
        }


        private void SearchRealm(Realm r, bool special = false)
        {
            long itemLevelModifier = 0;
            long itemLevel = 0;
            string itemSuffix = String.Empty;

            if ((RealmChecked(r.ConnectedRealmId)) &&
                ((!togOnlyNewData.Checked)
                || (togIgnoreOnlyNewData.Checked || (togOnlyNewData.Checked && GetRealmStatus(r.ConnectedRealmId) == 3))))
            {

                WriteDebug("Searching realm " + r.RealmName);

                searchResults.Clear();

                duplicateItemIds.Clear();
                uniqueItemIds.Clear();
                searchItemCounts.Clear();

                AuctionFileContents targetAfc;
                RealmAuctions.TryGetValue(r.ConnectedRealmId, out targetAfc);

                string qualityType = "POOR";
                string cachedName = String.Empty;
                string className = String.Empty;
                string subClassName = String.Empty;
                float saleRate = 0;
                long marketValue = 0;
                bool isPet = false;
                string auctionId = "I0";
                bool level70found = false;

                foreach (Auction aa in targetAfc.auctions)
                {
                    itemMatch = false;
                    cachedItem = null;
                    cachedPet = null;
                    itemSuffix = String.Empty;
                    itemLevelModifier = 0;
                    itemLevel = 0;
                    level70found = false;

                    if (aa.item.pet_species_id > 0)
                    {
                        RegionPets.TryGetValue(aa.item.pet_species_id, out regionPet);
                        DictionaryPetCache.TryGetValue(aa.item.pet_species_id, out cachedPet);
                        isPet = true;
                    }
                    else
                    {
                        RegionItems.TryGetValue(aa.item.id, out regionItem);
                        DictionaryItemCache.TryGetValue(aa.item.id, out cachedItem);
                        isPet = false;
                    }

                    if (regionItem == null)
                    {
                        regionItem = new TsmItem();
                        regionItem.itemId = aa.item.id.ToString();
                    }

                    if (special)
                    {

                    }

                    if ((regionItem != null || regionPet != null) && ((cachedItem != null && cachedItem.Name != null) || (cachedPet != null && cachedPet.Name != null)) && aa.buyout > 0)
                    {
                        if (!(togOnlyLatestXpac.Checked && !IsLXItem(aa.item.id)))
                        {
                            
                            if (
                                    (togBonuses_Speed.Checked) &&
                                    (
                                       (aa.item.bonus_lists == null) ||
                                       (aa.item.bonus_lists.Contains(42) == false)
                                    )
                                )
                            {
                                continue;
                            }
                                
                            if (isPet)
                            {
                                if (!togIncludePets.Checked || regionPet == null) { continue; }
                                qualityType = GetQualityTypeFromNumber(aa.item.pet_quality_id);
                                marketValue = regionPet.marketValue;
                                className = "Battle Pet";
                                cachedName = cachedPet.Name;
                                saleRate = regionPet.saleRate;
                            }
                            else
                            {
                                if (!togIncludeItems.Checked || regionItem == null) { continue; }
                                qualityType = cachedItem.QualityType;
                                marketValue = regionItem.marketValue;
                                className = cachedItem.ClassName;
                                cachedName = cachedItem.Name;
                                saleRate = regionItem.saleRate;
                            }

                            if (saleRate < searchParams.MinSellRate)
                            {
                                continue;
                            }

                            if (rbSearchType_String.Checked)
                            {
                                SearchQuality(qualityType, out rowColor);
                                itemMatch = cachedName.ToUpper().Contains(stringText.ToUpper());
                            }
                            else if (rbSearchType_List.Checked)
                            {
                                itemMatch = auctionListIds.Contains(aa.item.id);
                                if (itemMatch)
                                {
                                    WriteDebug("Item is on the list " + aa.item.id.ToString());
                                    Application.DoEvents();
                                }
                            }
                            else if (rbSearchType_ListMaxGold.Checked)
                            {
                                long itemValue = 0;

                                if (auctionListFull.TryGetValue(aa.item.id, out itemValue))
                                {
                                    if (auctionListMaxGold > 0)
                                    {
                                        itemValue = auctionListMaxGold;
                                    }

                                    if (itemValue > 0)
                                    {
                                        itemMatch = (aa.buyout < (itemValue * 10000));
                                    }
                                }

                            }
                            else
                            {
                                if (rdSearchPercent.Checked)
                                {
                                    valueMet = ((aa.buyout < (marketValue * (searchParams.SearchPercentage))) && (marketValue > (searchParams.WorthAtLeast)));
                                }
                                else if (rdSearchMaxG.Checked)
                                {
                                    if ((searchParams.WorthAtLeast) > -1) //We care about how much the item is worth
                                    {
                                        valueMet = ((aa.buyout <= (searchParams.SearchMaxG) && marketValue > (searchParams.WorthAtLeast)));

                                    }
                                    else //Worth = -1, we don't care how much it's worth just show all items below the Max G
                                    {
                                        valueMet = (aa.buyout <= (searchParams.SearchMaxG) );
                                    }
                                    
                                }

                                if (valueMet)
                                {
                                    itemMatch = true;
                                }

                                /*
                                if (valueMet
                                    && SearchQuality(qualityType, out rowColor)
                                    && (searchParams.AllItemClasses || SearchClass(className))
                                    //&& SearchClass(className)
                                    )
                                {
                                    itemMatch = true;
                                }
                                */
                            }

                            if (itemMatch)
                            {
                                if (isPet)
                                {
                                    auctionId = "P" + aa.item.pet_species_id.ToString();
                                    subClassName = cachedPet.BattlePetType;
                                }
                                else
                                {
                                    auctionId = "S" + aa.item.id.ToString();
                                    subClassName = cachedItem.SubClassName;
                                }

                                if (cachedItem != null)
                                {
                                    itemLevel = cachedItem.Level;
                                }
                                else
                                {
                                    itemLevel = 0;
                                }

                                if (aa.item.modifiers != null)
                                {
                                    level70found = true;
                                    foreach (AuctionModifiers mod in aa.item.modifiers)
                                    {
                                        if (chkLevel70.Checked && mod.type == 9)
                                        {
                                            //This has a required level modifiers i.e. level 75 so it is not level 70
                                            level70found = false;
                                        }
                                    }
                                }

                                //Need to check, and if necessary, modify the item level
                                if (aa.item.bonus_lists != null)
                                {
                                    itemSuffix = String.Empty;
                                    foreach (long bonus in aa.item.bonus_lists)
                                    {
                                        /*
                                        //if latest expac
                                        if (IsLXItem(aa.item.id))
                                        {
                                            //These numbers changed for TWW I think - they redid the base item level id
                                            //No longer 1472
                                            if (bonus > 3220 && bonus < 3822)
                                            {
                                                itemLevelModifier = (bonus - 1472);
                                            }
                                        }
                                        */
                                        //else

                                        /*
                                        if (itemLevel == 545 && itemLevelModifier == 0)
                                        {
                                            WriteDebug("i545 found " + cachedItem.Name + " with no modifier");
                                        }
                                        */

                                        if (bonus > 1371 && bonus < 1600)
                                        {
                                            itemLevelModifier = (bonus - 1472);
                                        }
                                        else if (bonus >= 1676 && bonus <= 1717)
                                        {
                                            if (bonus >= 1676 && bonus <= 1682)
                                            {
                                                itemSuffix = "of the Quickblade";
                                            }
                                            else if (bonus >= 1683 && bonus <= 1689)
                                            {
                                                itemSuffix = "of the Peerless";
                                            }
                                            else if (bonus >= 1690 && bonus <= 1696)
                                            {
                                                itemSuffix = "of the Fireflash";
                                            }
                                            else if (bonus >= 1697 && bonus <= 1703)
                                            {
                                                itemSuffix = "of the Feverflare";
                                            }
                                            else if (bonus >= 1704 && bonus <= 1710)
                                            {
                                                itemSuffix = "of the Aurora";
                                            }
                                            else if (bonus >= 1711 && bonus <= 1717)
                                            {
                                                itemSuffix = "of the Harmonious";
                                            }

                                        }
                                    }
                                }
                            }    
                               
                        }

                        //Adjust item level by the modifier if there was one
                        if (itemLevelModifier != 0)
                        {
                            itemLevel += itemLevelModifier;
                        }

                        //Filter out non-level 70 gear if we are searching explicitly for it
                        if (chkLevel70.Checked && level70found==false)
                        {
                            continue;
                        }

                        //Filter out search result if user opted to search by item level
                        if (itemLevel < searchParams.MinItemLevel || itemLevel > searchParams.MaxItemLevel)
                        {
                            continue;
                        }

                        
                        if ((togSocket.Checked) && (!(Bonuses.HasSocket(aa.item.bonus_lists))))
                        {
                            continue;
                        }

                        SearchResult sr = new SearchResult();

                        sr.AuctionId = auctionId;
                        sr.RealmId = r.ConnectedRealmId;
                        sr.RealmName = r.RealmName;
                        sr.Buyout = aa.buyout;
                        sr.RegionMarket = marketValue;
                        sr.ItemId = aa.item.id;
                        sr.PetId = aa.item.pet_species_id;
                        sr.PetLevel = aa.item.pet_level;
                        sr.NumAuctions = r.NumAuctions;
                        sr.NumAuctionColor = r.NumAuctionColor;
                        sr.ItemName = cachedName;
                        sr.Quality = qualityType;
                        sr.Class = className;
                        sr.SubClass = subClassName;
                        sr.RowColor = rowColor;
                        sr.SaleRate = saleRate;
                        sr.Level = itemLevel;
                        sr.Suffix = itemSuffix;

                        if (aa.item.modifiers != null)
                        {
                            sr.Modifiers += "MODS: ";
                            foreach (AuctionModifiers am in aa.item.modifiers)
                            {
                                sr.Modifiers += "type = " + am.type.ToString() + ", value = " + am.value.ToString() + " | ";
                            }
                        }

                        if (aa.item.bonus_lists != null)
                        {
                            sr.BonusLists += "BONUSES: ";
                            foreach (long bonus in aa.item.bonus_lists)
                            {
                                sr.BonusLists += bonus.ToString() + " | ";
                            }
                        }

                        if (searchParams.ShowCheapest)
                        {
                            searchItemCounts.TryGetValue(auctionId, out countItem);

                            //We haven't had this before for this realm
                            if (countItem == null)
                            {
                                SearchCount sc = new SearchCount();
                                sc.AuctionId = auctionId;
                                sc.Count = 1;
                                sc.Cheapest = aa.buyout;
                                sc.Result = sr;
                                searchItemCounts.Add(auctionId, sc);
                            }
                            //We already have this, is it the cheapest?
                            else
                            {
                                countItem.Count += 1;
                                if (aa.buyout < countItem.Cheapest)
                                {
                                    countItem.Cheapest = aa.buyout;
                                    countItem.Result = sr;
                                }
                            }
                        }

                        else
                        {

                            if (searchParams.RemovingDuplicates)
                            {
                                if (uniqueItemIds.Contains(auctionId))
                                {
                                    duplicateItemIds.Add(auctionId);
                                    uniqueItemIds.Remove(auctionId);
                                }
                                else
                                {
                                    //This is unique so far, so add it to the unique item ids and to the search results
                                    uniqueItemIds.Add(auctionId);
                                }

                                searchResults.Add(sr);
                            }

                            else //Show all
                            {
                                searchResults.Add(sr);
                            }

                        }

                    }



                }

                //Show Cheapest
                //=============
                //
                //Build search results 
                if (searchParams.ShowCheapest)
                {
                    foreach (KeyValuePair<string, SearchCount> sc2 in searchItemCounts)
                    {
                        searchResults.Add(sc2.Value.Result);
                    }
                }


                //Remove all duplicates
                //=====================
                //
                //Loop through search results and take out any duplicate Ids we found
                //Also remove bid only
                if (searchParams.RemovingDuplicates)
                {
                    for (int i = searchResults.Count - 1; i >= 0; i--)
                    {
                        if (duplicateItemIds.Contains(searchResults[i].AuctionId))
                        {
                            searchResults.RemoveAt(i);
                        }
                    }
                }

                if (togAtoZ.Checked)
                {
                    orderResults = searchResults.OrderBy(x => x.ItemName).ToList();
                    searchResults = orderResults;
                }

                RenderSearchResults(searchResults, r);
            }
        }

        private void Search(bool special = false)
        {
            DateTime startTime = DateTime.Now;
            SearchInit();

            foreach (Realm r in apiConfig.Realms)
            {
               SearchRealm(r, special);
            }
        }

        public Team GetTeam(string teamName)
        {
            Team tm = new Team();

            foreach (Team tm1 in apiConfig.Teams)
            {
                if (tm1.Name == teamName)
                {
                    tm = tm1;
                }
            }

            return tm;
        }

        public bool RealmActive(Realm r)
        {
            bool checkRealm = false;

            foreach (ListViewItem lvi in r.RealmsView.Items)
            {
                if (lvi.SubItems[1].Text == r.RealmName)
                {
                    checkRealm = lvi.Checked;
                    break;
                }
            }
            return (r.Active && GetTeam(r.Team).Active && checkRealm);
        }

        public bool RealmOneG(Realm r)
        {
            return (r.Flagged);
        }

        private void SetAreaImage(ListViewItem lvi, string Area)
        {
            switch (Area)
            {
                case "US": default: lvi.ImageIndex = 0; break;
                case "OC": lvi.ImageIndex = 1; break;
                case "BR": lvi.ImageIndex = 2; break;
                case "LA": lvi.ImageIndex = 3; break;
            }
        }
        private void RenderSearchResults(List<SearchResult> searchResults, Realm r)
        {
            r.AuctionsView.SuspendLayout();
            string currentRealm = "";
            foreach (SearchResult sr3 in searchResults)
            {
                if (currentRealm != sr3.RealmName)
                {
                    currentRealm = sr3.RealmName;
                    AddBlankSearchItem(r);
                }
                float actualPercentage = (((float)sr3.Buyout / (float)sr3.RegionMarket) * 100);

                ListViewItem lvi = new ListViewItem();
                lvi.UseItemStyleForSubItems = false;
                lvi.Text = " " + sr3.RealmName;
                lvi.BackColor = r.BackColor;
                lvi.Tag = sr3;
                lvi.ToolTipText = sr3.ItemId.ToString() + " ItemLevel = (" + sr3.Level.ToString() + ") " + sr3.Modifiers + " " + sr3.BonusLists;

                if (sr3.Suffix != String.Empty)
                {
                    lvi.SubItems.Add(sr3.ItemName + " " + sr3.Suffix);
                }
                else
                {
                    lvi.SubItems.Add(sr3.ItemName);
                }

                lvi.SubItems[1].ForeColor = GetColorForQuality(sr3.Quality);

                lvi.SubItems.Add(sr3.Level.ToString());
                lvi.SubItems[2].ForeColor = GetColorForQuality(sr3.Quality);

                //Color code sale rate
                lvi.SubItems.Add(sr3.SaleRate.ToString());
                if (sr3.SaleRate < 0.001) { lvi.SubItems[3].ForeColor = Color.LightGray; }
                else if (sr3.SaleRate < 0.002) { lvi.SubItems[3].ForeColor = Color.Red; }
                else if (sr3.SaleRate < 0.010) { lvi.SubItems[3].ForeColor = Color.Orange; }
                else if (sr3.SaleRate < 0.100) { lvi.SubItems[3].ForeColor = Color.LightBlue; }
                else { lvi.SubItems[2].ForeColor = Color.Green; }

                lvi.SubItems.Add(StringHelper.FormatItemPrice(sr3.Buyout)); //Buyout $
                lvi.SubItems.Add(StringHelper.FormatItemPrice(sr3.RegionMarket)); //Region $

                lvi.SubItems.Add(sr3.PetLevel.ToString()); //Pet Level
                if (sr3.PetLevel > 0)
                {
                    lvi.SubItems[5].ForeColor = GetColorForQuality(sr3.Quality);
                }

                lvi.SubItems.Add(LXItem(sr3.ItemId));

                if (searchParams.Highlight > 0)
                {
                    CheckHighlight(lvi, r);
                }

                r.AuctionsView.Items.Add(lvi);
            }
            r.AuctionsView.ResumeLayout();
        }

        private void CheckHighlight(ListViewItem lvi, Realm r)
        {
            if (r.Highlight != "0" && r.Highlight.Contains(searchParams.Highlight.ToString()))
            {
                Color highcol = GetHighlightColor(searchParams.Highlight);
                for (int i = 1; i < lvi.SubItems.Count; i++)
                {
                    lvi.SubItems[i].BackColor = highcol;
                }
            }
        }

        private Color GetHighlightColor(int highlight)
        {
            switch (highlight)
            {
                case 1: default: return apiConfig.Highlight1;
                case 2: return apiConfig.Highlight2;
                case 3: return apiConfig.Highlight3;
                case 4: return apiConfig.Highlight4;
                case 5: return apiConfig.Highlight5;
            }
        }


        private Color GetColorForQuality(string quality)
        {
            switch (quality)
            {
                case "UNCOMMON": default: return Color.LawnGreen;
                case "RARE": return Color.CornflowerBlue;
                case "EPIC": return Color.DarkViolet;
                case "POOR": return Color.DarkGray;
                case "COMMON": return Color.White;
                case "LEGENDARY": return Color.Orange;
                case "ARTIFACT": return Color.Tan;
            }
        }

        private void AddBlankSearchItem(Realm r)
        {
            ListViewItem lvi = new ListViewItem();

            lvi.Text = "--------------------"; //Realm name

            lvi.SubItems.Add("----------------------------------------------------------"); //Item name
            lvi.SubItems.Add("-------"); //Sales pct
            lvi.SubItems.Add("------------"); //Buyout price
            lvi.SubItems.Add("------------"); //Region price
            lvi.SubItems.Add("---"); //PetLevel
            lvi.SubItems.Add("---"); //Latest xpac

            lvi.ForeColor = Color.DarkGray;

            r.AuctionsView.Items.Add(lvi);
        }

        public Color GetNumAuctionColor(int numAuctions)
        {
            Color returnCol = Color.White;

            if (numAuctions < 20000)
            {
                returnCol = Color.Red;
            }
            else if (20000 <= numAuctions && numAuctions <= 49999)
            {
                returnCol = Color.Orange;
            }
            else if (50000 <= numAuctions && numAuctions <= 90000)
            {
                returnCol = Color.Blue;
            }
            else
            {
                returnCol = Color.Green;
            }

            return returnCol;
        }

        private bool SearchQuality(string itemQuality, out Color rowColor)
        {
            bool returnResult = false;

            switch (itemQuality)
            {
                case "POOR":
                    returnResult = (togItemQuality_Poor.Checked == true);
                    rowColor = togItemQuality_Poor.ForeColor;
                    break;
                case "COMMON":
                    returnResult = (togItemQuality_Common.Checked == true);
                    rowColor = togItemQuality_Common.ForeColor;
                    break;
                case "UNCOMMON": default:
                    returnResult = (togItemQuality_Uncommon.Checked == true);
                    rowColor = togItemQuality_Uncommon.ForeColor;
                    break;
                case "RARE":
                    returnResult = (togItemQuality_Rare.Checked == true);
                    rowColor = togItemQuality_Rare.ForeColor;
                    break;
                case "EPIC":
                    returnResult = (togItemQuality_Epic.Checked == true);
                    rowColor = togItemQuality_Epic.ForeColor;
                    break;
                case "LEGENDARY":
                    returnResult = (togItemQuality_Legendary.Checked == true);
                    rowColor = togItemQuality_Legendary.ForeColor;
                    break;
                case "ARTIFACT":
                    returnResult = (togItemQuality_Artifact.Checked == true);
                    rowColor = togItemQuality_Artifact.ForeColor;
                    break;
            }

            return returnResult;

        }

        private bool SearchClass(string className)
        {
            bool returnResult = false;

            switch (className)
            {
                case "Weapon": returnResult = (togItemClass_Weapon.Checked == true); break;
                case "Armor": default: returnResult = (togItemClass_Armor.Checked == true); break;
                case "Consumable": returnResult = (togItemClass_Consumable.Checked == true); break;
                case "Miscellaneous": returnResult = (togItemClass_Miscellaneous.Checked == true); break;
                case "Tradeskill": returnResult = (togItemClass_Tradeskill.Checked == true); break;
                case "Profession": returnResult = (togItemClass_Profession.Checked == true); break;
                case "Container": returnResult = (togItemClass_Container.Checked == true); break;
                case "Quest": returnResult = (togItemClass_Quest.Checked == true); break;
                case "Item Enhancement": returnResult = (togItemClass_ItemEnhancement.Checked == true); break;
                case "Recipe": returnResult = (togItemClass_Recipe.Checked == true); break;
                case "Gem": returnResult = (togItemClass_Gem.Checked == true); break;
                case "Key": returnResult = (togItemClass_Key.Checked == true); break;
                case "Glyph": returnResult = (togItemClass_Glyph.Checked == true); break;
                case "Reagent": returnResult = (togItemClass_Reagent.Checked == true); break;
            }

            return returnResult;
        }

        public void DoBackup(string source, string destination)
        {
            this.tsbDoBackup.BackColor = Color.DarkRed;
            destination += DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + " (" + Sys.Version + @")\";
            //this.tsMenu_Progress.Visible = true;
            RoboCommand backup = new RoboCommand();
            backup.OnCommandCompleted += Backup_OnCommandCompleted;
            backup.CopyOptions.Source = source;
            backup.CopyOptions.Destination = destination;
            backup.CopyOptions.CopySubdirectories = true;
            backup.CopyOptions.UseUnbufferedIo = true;
            backup.CopyOptions.Mirror = true;
            backup.RetryOptions.RetryCount = 1;
            backup.RetryOptions.RetryWaitTime = 2;
            backup.Start();
        }

        private void Backup_OnCommandCompleted(object sender, RoboCommandCompletedEventArgs e)
        {
            this.tsbDoBackup.BackColor = Color.Transparent;
        }

        private bool AllItemClassesChecked()
        {
            return (togItemClass_Weapon.Checked == true) &&
                (togItemClass_Armor.Checked == true) &&
                (togItemClass_Consumable.Checked == true) &&
                (togItemClass_Miscellaneous.Checked == true) &&
                (togItemClass_Tradeskill.Checked == true) &&
                (togItemClass_Profession.Checked == true) &&
                (togItemClass_Container.Checked == true) &&
                (togItemClass_Quest.Checked == true) &&
                (togItemClass_ItemEnhancement.Checked == true) &&
                (togItemClass_Recipe.Checked == true) &&
                (togItemClass_Gem.Checked == true) &&
                (togItemClass_Key.Checked == true) &&
                (togItemClass_Glyph.Checked == true) &&
                (togItemClass_Reagent.Checked == true);
        }

        private void LoadAuctionItemsForList(string listName)
        {
            Item cachedItem;
            lvwAuctionListItems.Items.Clear();
            foreach (AuctionList al in apiConfig.AuctionLists)
            {
                if (al.Name == listName)
                {
                    foreach (AuctionListItem ali in al.AuctionListItems)
                    {
                        ListViewItem lvi = new ListViewItem();
                        lvi.UseItemStyleForSubItems = false;
                        lvi.Text = ali.Id.ToString();
                        lvi.ForeColor = Color.White;
                        DictionaryItemCache.TryGetValue(ali.Id, out cachedItem);

                        if (cachedItem != null)
                        {
                            lvi.SubItems.Add(cachedItem.Name);
                            lvi.SubItems[1].ForeColor = GetColorForQuality(cachedItem.QualityType);
                        }
                        lvwAuctionListItems.Items.Add(lvi);
                    }
                    break;
                }
            }
        }

        private void lvItemSearch_DoubleClick(object sender, EventArgs e)
        {
            Clipboard.SetText(lvwAuctionsBlue.SelectedItems[0].SubItems[1].Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ListViewItem lvi = lvwRealms.Items[0];

            MessageBox.Show(lvi.SubItems[1].Text);

            DateTime mydate = DateTime.Parse(lvi.SubItems[1].Text);

            MessageBox.Show(mydate.ToLongDateString() + " " + mydate.ToLongTimeString());
        }

        private void BackupItemCache()
        {
            if (System.IO.File.Exists(Paths.ItemCachePath))
            {
                string source = Paths.ItemCachePath;
                string dest = Paths.ItemCacheBackupPath.Replace("DDD", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff"));

                System.IO.File.Copy(source, dest);
            }
        }

        private void BackupPetCache()
        {
            if (System.IO.File.Exists(Paths.ItemCachePath))
            {
                string source = Paths.PetCachePath;
                string dest = Paths.PetCacheBackupPath.Replace("DDD", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));

                System.IO.File.Copy(source, dest);
            }
        }

        private void BackupRegionData()
        {
            if (System.IO.File.Exists(Paths.TsmRegionDataPath))
            {
                string source = Paths.TsmRegionDataPath;
                string dest = Paths.TsmRegionDataBackupPath.Replace("DDD", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));

                System.IO.File.Copy(source, dest);
            }
        }



        private void btnCopyIllus_Click(object sender, EventArgs e)
        {
            Clipboard.SetText("Formula: Illusion:");
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

            var logFile = File.ReadAllLines(Environment.CurrentDirectory + @"\Items.txt");
            var logList = new List<string>(logFile);

            List<string> builtList = new List<string>();

            foreach(string tryItem in logList)
            {
                builtList.Add("<AuctionListItem Name=\"" + tryItem + "\" Id=\"" + ItemIdFromCache(tryItem).ToString() + "\" />");
            }

            File.WriteAllLines(Environment.CurrentDirectory + @"\Items_processed.txt", builtList);

            //MessageBox.Show(API_Blizzard.GetAccessToken(apiConfig.ClientID, apiConfig.ClientSecret));
            /*
            TsmItem regionItem;
            //Item cachedItem;

            List<string> items = new List<string>();      

            foreach (KeyValuePair<long, Item> cachedItem in DictionaryItemCache)
            {
                RegionItems.TryGetValue(cachedItem.Key, out regionItem);

                if ((regionItem != null) && (regionItem.marketValue > 200000000) && (regionItem.saleRate > 0) && (this.IsDFItem(cachedItem.Key)))
                {
                    items.Add(cachedItem.Value.Name + "," + cachedItem.Value.Id.ToString() + ","
                        + (regionItem.marketValue / 10000).ToString() + "," + regionItem.saleRate.ToString());
                }
            }

            System.IO.File.WriteAllLines(Paths.XmlPath + "SaleRateDF_Items.csv", items);
            */

        }

        private long ItemIdFromCache(string itemName)
        {
            long retVal = 0;
            foreach (KeyValuePair<long, WOWApi.Item> cachedItem in DictionaryItemCache)
            {
                if (cachedItem.Value.Name == itemName)
                {
                    retVal = cachedItem.Key;
                    break;
                }
            }

            return retVal;
        }

        private void cboAuctionLists_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadAuctionItemsForList(cboAuctionLists.Text);
        }

        private void ListViewIntegerClick(System.Windows.Forms.ListView lvw, ListViewColumnSorter lvSorter, object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvSorter.Order == SortOrder.Ascending)
                {
                    lvSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvSorter.SortColumn = e.Column;
                lvSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            lvw.Sort();
        }

        private void ListViewClick(System.Windows.Forms.ListView lvw, ListViewColumnSorter lvSorter, object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvSorter.Order == SortOrder.Ascending)
                {
                    lvSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvSorter.SortColumn = e.Column;
                lvSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            lvw.Sort();
        }

        private void lvAuctions_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewClick(lvwAuctionsBlue, lvwAuctionColumnSorter, sender, e);
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText("Obsidian Comb");
        }

        private void FlagRealms()
        {
            foreach (ListViewItem lvi in lvwRealms.Items)
            {
                if (((Realm)lvi.Tag).Flagged)
                {
                    lvi.Checked = true;
                }
                else
                {
                    lvi.Checked = false;
                }
            }
        }

        private void cboSearchProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (SearchProfile sp in apiConfig.SearchProfiles)
            {
                if (sp.ProfileName == cboSearchProfiles.Text)
                {
                    //cboSearchProfiles.SelectedItem = sp.ProfileName;
                    LoadSearchProfile(sp);

                    break;
                }
            }
        }

        private void btnRefreshThreads_Click(object sender, EventArgs e)
        {
            RefreshThreadCount();
        }

        private void btnLivePoll_Click(object sender, EventArgs e)
        {
            livePoll = true;
            //SearchInit();
            //LoadAuctionDataLive();
        }


        private void RefreshAndSearch()
        {
            livePoll = false;
            //LoadAuctionDataLive();
            //System.Threading.Thread.Sleep(5000);
            //Search();
        }

        private void btnGetAuctionData2_Click(object sender, EventArgs e)
        {
            livePoll = false;
            LoadAuctionDataLive("Green");
        }

        private void tsbWriteRegionData_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Write region data?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                WriteRegionData();
            }   
        }

        private void tsbUpdateItemCache_Click(object sender, EventArgs e)
        {
            UpdateItemCache();
        }

        private void UpdateItemCache()
        {
            if (MessageBox.Show("Update item cache?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                BuildItemCache(true);
            }
        }

        private void SortItemCache()
        {
            if (MessageBox.Show("Sort item cache?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                BackupItemCache();
                ItemCache tempCache = new ItemCache();

                foreach (KeyValuePair<long, Item> it in DictionaryItemCache)
                {
                    tempCache.AddItem(it.Value);
                }

                tempCache.Save();
                BackupItemCache();
            }
        }

        private void UpdatePetCache()
        {
            if (MessageBox.Show("Update pet cache?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                BuildPetCache(true);
            }
        }

        private void SortPetCache()
        {
            if (MessageBox.Show("Sort pet cache?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                BackupPetCache();
                PetCache tempCache = new PetCache();

                foreach (KeyValuePair<long, Pet> it in DictionaryPetCache)
                {
                    tempCache.AddPet(it.Value);
                }

                tempCache.Save();
                BackupPetCache();
            }
        }

        private void tsbSortItemCache_Click(object sender, EventArgs e)
        {
            SortItemCache();
        }

        private void BuildItemCache()
        {
            if (MessageBox.Show("Build item cache from scratch?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (MessageBox.Show("REALLY build item cache from scratch? This will take several hours.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    BuildItemCache(false);
                }
            }
        }

        private void BuildPetCache()
        {
            if (MessageBox.Show("Build pet cache from scratch?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (MessageBox.Show("REALLY build pet cache from scratch? This may take a while.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    BuildPetCache(false);
                }
            }
        }

        private void tsbBuildItemCache_Click(object sender, EventArgs e)
        {
            BuildItemCache();
        }

        private void tsmUpdateItemCache_Click(object sender, EventArgs e)
        {
            UpdateItemCache();
        }

        private void tsmSortItemCache_Click(object sender, EventArgs e)
        {
            SortItemCache();
        }

        private void tsmBuildItemCache_Click(object sender, EventArgs e)
        {
            BuildItemCache();
        }

        private void tsmSortPetCache_Click(object sender, EventArgs e)
        {
            SortPetCache();
        }

        private void tsmUpdatePetCache_Click(object sender, EventArgs e)
        {
            UpdatePetCache();
        }

        private void tsmBuildPetCache_Click(object sender, EventArgs e)
        {
            BuildPetCache();
        }

        private void btnFlagServers_Click(object sender, EventArgs e)
        {
            FlagRealms();
        }

        private void pnlSearching_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnSearch_BigBucks_Click(object sender, EventArgs e)
        {
            RunSetSearch("Big Bucks");
        }

        private void btnSearch_SmallItems_Click(object sender, EventArgs e)
        {
            RunSetSearch("Small Items");
        }

        private void RunSetSearch(string searchname)
        {
            SetSearchProfile(searchname);
            Application.DoEvents();
            Search();
        }

        private void ProcessC()
        {
            try
            {
               CopyToWoW(ActiveAuctionsView.SelectedItems[0].SubItems[1].Text, KeypressRealm);
            }
            catch { }

        }

        private void ProcessX()
        {
            AdvancedCopy(apiConfig.FastCopySpeed);
        }

        private void ProcessZ()
        {
            AdvancedCopy(apiConfig.SlowCopySpeed);
        }

        private void AdvancedCopy(double speed)
        {
            try
            {
               AdvancedCopyToWoW(ActiveAuctionsView.SelectedItems[0].SubItems[1].Text, KeypressRealm, speed);
            }
            catch { }
        }

        private void ProcessS()
        {
            try
            {
                Clipboard.SetText(KeypressRealm.SendTo);

                Win32.MoveAndLeftClick(2069, 168, 25);
                System.Threading.Thread.Sleep(25);
                //Paste send to
                SendKeys.Send("^a");
                System.Threading.Thread.Sleep(100);
                SendKeys.Send("^v");
            }
            catch { }

        }

        private void CopyToWoW(string auctionText, Realm r)
        {
            ActivateWow(r);

            //Click the box
            Win32.MoveAndLeftClick(2206, 177, 25);
            System.Threading.Thread.Sleep(25);

            Clipboard.SetText(auctionText);
            System.Threading.Thread.Sleep(25);

            //CTRL+A (select all)
            SendKeys.Send("^a");
            System.Threading.Thread.Sleep(25);

            //CTRL+V
            SendKeys.Send("^v");
            System.Threading.Thread.Sleep(25);

            //Click search
            Win32.MoveAndLeftClick(2671, 165, 25);
            System.Threading.Thread.Sleep(25);

            //Move to topmost item
            Win32.MoveMouse(2478, 239);

        }

        private void AdvancedCopyToWoW(string auctionText, Realm r, double speed)
        {
            ActivateWow(r);

            System.Threading.Thread.Sleep(300);

            //Click the box
            Win32.MoveAndLeftClick(380, 165, 25);
            System.Threading.Thread.Sleep(Convert.ToInt32(50 * speed));

            Clipboard.SetText(auctionText);
            System.Threading.Thread.Sleep(Convert.ToInt32(50 * speed));

            //CTRL+A (select all)
            SendKeys.Send("^a");
            System.Threading.Thread.Sleep(Convert.ToInt32(50 * speed));

            //CTRL+V
            SendKeys.Send("^v");
            System.Threading.Thread.Sleep(Convert.ToInt32(50 * speed));

            //Click search
            Win32.MoveAndLeftClick(730, 165, 25);
            System.Threading.Thread.Sleep(Convert.ToInt32(750 * speed));

            //Click top most item
            Win32.MoveAndLeftClick(380, 230, 25);
            System.Threading.Thread.Sleep(Convert.ToInt32(450 * speed));

            //Click first result
            Win32.MoveAndLeftClick(380, 360, 25);
            System.Threading.Thread.Sleep(Convert.ToInt32(450 * speed));

            //Click buyout
            Win32.MoveAndLeftClick(750, 640, 25);
            System.Threading.Thread.Sleep(Convert.ToInt32(450 * speed));

            //Move to confirm
            Win32.MoveMouse(900, 200);
        }

        private void AdvancedCopyToWoWSearchOnly(string auctionText, double speed)
        {

            //Click the box
            Win32.MoveAndLeftClick(2266, 177, 25);
            System.Threading.Thread.Sleep(Convert.ToInt32(50 * speed));

            Clipboard.SetText(auctionText);
            System.Threading.Thread.Sleep(Convert.ToInt32(50 * speed));

            //CTRL+A (select all)
            SendKeys.Send("^a");
            System.Threading.Thread.Sleep(Convert.ToInt32(50 * speed));

            //CTRL+V
            SendKeys.Send("^v");
            System.Threading.Thread.Sleep(Convert.ToInt32(50 * speed));

            //Click search
            Win32.MoveAndLeftClick(2671, 175, 25);
            System.Threading.Thread.Sleep(Convert.ToInt32(150 * speed));

            //Click back on the API app again
            Win32.MoveAndLeftClick(700, 20, 25);
            System.Threading.Thread.Sleep(Convert.ToInt32(50 * speed));

        }

        private void IngameMail(Realm r, int mailVolume)
        {
            Clipboard.SetText(r.SendTo);

            ActivateWow(r);

            //Click open all mail button
            Win32.MoveAndLeftClick(185, 535, 25);
            System.Threading.Thread.Sleep((mailVolume * 2000) + 1000);

            //Click send mail tab
            Win32.MoveAndLeftClick(150, 575, 50);
            System.Threading.Thread.Sleep(100);

            //Paste send to
            SendKeys.Send("^a");
            System.Threading.Thread.Sleep(100);
            SendKeys.Send("^v");
            System.Threading.Thread.Sleep(100);

            //Add items - row 1
            Win32.MoveAndRightClick(1755, 650, 50);
            System.Threading.Thread.Sleep(250);
            Win32.MoveAndRightClick(1795, 650, 50);
            System.Threading.Thread.Sleep(250);
            Win32.MoveAndRightClick(1835, 650, 50);
            System.Threading.Thread.Sleep(250);
            Win32.MoveAndRightClick(1875, 650, 50);
            System.Threading.Thread.Sleep(250);

            if (mailVolume > 1)
            {
                //Add items - row 2
                Win32.MoveAndRightClick(1755, 690, 50);
                System.Threading.Thread.Sleep(250);
                Win32.MoveAndRightClick(1795, 690, 50);
                System.Threading.Thread.Sleep(250);
                Win32.MoveAndRightClick(1835, 690, 50);
                System.Threading.Thread.Sleep(250);
                Win32.MoveAndRightClick(1875, 690, 50);
                System.Threading.Thread.Sleep(250);
            }

            if (mailVolume > 2)
            {
                //Add items - row 3
                Win32.MoveAndRightClick(1755, 730, 50);
                System.Threading.Thread.Sleep(250);
                Win32.MoveAndRightClick(1795, 730, 50);
                System.Threading.Thread.Sleep(250);
                Win32.MoveAndRightClick(1835, 730, 50);
                System.Threading.Thread.Sleep(250);
                Win32.MoveAndRightClick(1875, 730, 50);
                System.Threading.Thread.Sleep(250);
            }

            //Move to send button
            Win32.MoveAndLeftClick(2132, 532, 50);


            System.Threading.Thread.Sleep(100);

            //Immediately move to OK send button, don't click in case it doesn't appear
            Win32.MoveAndLeftClick(220, 545, 50);

        }

        private void ActivateWow(Realm r)
        {
            //Activiate WOW
            Win32.ActivateApp(GetTeam(r.Team).ProcessId);
            System.Threading.Thread.Sleep(150);
        }

        private void ProcessMail(int mailVolume)
        {
            try
            { 
                IngameMail(KeypressRealm, mailVolume);
            }
            catch { }
        }

        private void tsbReloadConfigs_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Reload config settings?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                LoadConfig();
            }
        }

        private void tsbUtilities_Click(object sender, EventArgs e)
        {
            FormUtilities util = new FormUtilities();
            util.ShowDialog();

        }

        private void btnSelectAll_Realms_Click(object sender, EventArgs e)
        {
            SelectRealmCheckboxes(lvwRealms, true);
        }

        private void btnSelectNone_Realms_Click(object sender, EventArgs e)
        {
            SelectRealmCheckboxes(lvwRealms, false);
        }

        private void SelectCheckboxes_ItemClass(bool select)
        {
            togItemClass_Weapon.Checked = select;
            togItemClass_Armor.Checked = select;
            togItemClass_Consumable.Checked = select;
            togItemClass_Miscellaneous.Checked = select;
            togItemClass_Tradeskill.Checked = select;
            togItemClass_Profession.Checked = select;
            togItemClass_Container.Checked = select;
            togItemClass_Quest.Checked = select;
            togItemClass_ItemEnhancement.Checked = select;
            togItemClass_Recipe.Checked = select;
            togItemClass_Gem.Checked = select;
            togItemClass_Key.Checked = select;
            togItemClass_Glyph.Checked = select;
            togItemClass_Reagent.Checked = select;
        }

        private void btnSelectAll_ItemClass_Click(object sender, EventArgs e)
        {
            SelectCheckboxes_ItemClass(true);
        }

        private void btnSelectNone_ItemClass_Click(object sender, EventArgs e)
        {
            SelectCheckboxes_ItemClass(false);
        }

        private void btnSelectNone_ItemQuality_Click(object sender, EventArgs e)
        {
            SelectCheckboxes_Quality(false);
        }

        private void btnSelectAll_ItemQuality_Click(object sender, EventArgs e)
        {
            SelectCheckboxes_Quality(true);
        }

        private void SelectCheckboxes_Quality(bool select)
        {
            togItemQuality_Artifact.Checked = select;
            togItemQuality_Common.Checked = select;
            togItemQuality_Epic.Checked = select;
            togItemQuality_Legendary.Checked = select;
            togItemQuality_Poor.Checked = select;
            togItemQuality_Rare.Checked = select;
            togItemQuality_Uncommon.Checked = select;
        }

        private void btnSelectNone_Bonuses_Click(object sender, EventArgs e)
        {
            SelectCheckboxes_Bonuses(false);
        }

        private void btnSelectAll_Bonuses_Click(object sender, EventArgs e)
        {
            SelectCheckboxes_Bonuses(true);
        }

        private void SelectCheckboxes_Bonuses(bool select)
        {
            togBonuses_Leech.Checked = select;
            togBonuses_Speed.Checked = select;
        }

        private void SearchButtonClick(object sender, EventArgs e)
        {
            RunSetSearch(((ToolStripButton)sender).Tag.ToString());
        }

        private void CopyButtonClick(object sender, EventArgs e)
        {
            Clipboard.SetText(((System.Windows.Forms.Button)sender).Tag.ToString());
        }
        

        private void AddCopyTextButton(int index, int iconIndex, string shortName, string buttonCaption, Color backColor)
        {
            System.Windows.Forms.Button newCopyButton = new System.Windows.Forms.Button();

            newCopyButton.BackColor = backColor;
            newCopyButton.Font = new Font("Calibri", 9F);
            newCopyButton.ForeColor = Color.White;
            newCopyButton.Location = new Point(16, (index * 36) + 46);
            newCopyButton.Name = "bCopyText_" + shortName;
            newCopyButton.Size = new Size(375, 32);
            newCopyButton.TabIndex = 134;
            newCopyButton.Text = buttonCaption;
            newCopyButton.TextAlign = ContentAlignment.MiddleLeft;
            newCopyButton.ImageAlign = ContentAlignment.MiddleLeft;
            newCopyButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            newCopyButton.UseVisualStyleBackColor = false;
            newCopyButton.Click += new EventHandler(this.CopyButtonClick);
            
            newCopyButton.ImageList = img24;
            newCopyButton.ImageIndex = iconIndex;
            newCopyButton.Tag = buttonCaption;

            //pnlCopyTextBack.Controls.Add(newCopyButton);

        }

        private void AddNewSearchButton(int index, int iconIndex, string shortName, string longName, Color backColor)
        {
            ToolStripButton newTSB = new ToolStripButton();

            newTSB.Font = new Font("Calibri", 9F);
            newTSB.ForeColor = Color.White;
            newTSB.Name = "tsbSearch_" + shortName;
            newTSB.Size = new Size(64, 64);
            newTSB.Text = shortName;
            newTSB.TextAlign = ContentAlignment.BottomCenter;
            newTSB.TextImageRelation = TextImageRelation.ImageAboveText;
            newTSB.Click += new EventHandler(this.SearchButtonClick);

            newTSB.ImageIndex = iconIndex;
            newTSB.Tag = longName;
            //toolTip1.SetToolTip(newTSB, longName);

            toolStripMain.Items.Add(newTSB);

        }

        private void AddNewQuickButton(int index, int iconIndex, string shortName, string longName, Color backColor, Panel quickButtonPanel)
        {
            System.Windows.Forms.Button newSearchButton = new System.Windows.Forms.Button();

            newSearchButton.BackColor = backColor;
            newSearchButton.Font = new Font("Calibri", 9F);
            newSearchButton.ForeColor = Color.White;
            newSearchButton.Location = new Point((index * 36), (iconIndex / 4) *36);
            newSearchButton.Name = "bQuickSearch_" + shortName;
            newSearchButton.Size = new Size(34,34);
            newSearchButton.TabIndex = 134;
            newSearchButton.Text = "";
            newSearchButton.TextAlign = ContentAlignment.BottomCenter;
            newSearchButton.TextImageRelation = TextImageRelation.ImageAboveText;
            newSearchButton.UseVisualStyleBackColor = false;
            newSearchButton.Click += new EventHandler(this.SearchButtonClick);
            newSearchButton.ImageAlign = ContentAlignment.MiddleCenter;

            newSearchButton.ImageList = img32;
            newSearchButton.ImageIndex = iconIndex;
            newSearchButton.Tag = longName;
            toolTip1.SetToolTip(newSearchButton, longName);

            quickButtonPanel.Controls.Add(newSearchButton);

        }

        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.F1) { ProcessFSearch(apiConfig.CustomFSearch1); }
            if (e.KeyData == Keys.F2) { ProcessFSearch(apiConfig.CustomFSearch2); }
            if (e.KeyData == Keys.F3) { ProcessFSearch(apiConfig.CustomFSearch3); }
            if (e.KeyData == Keys.F4) { ProcessFSearch(apiConfig.CustomFSearch4); }
            if (e.KeyData == Keys.F5) { ProcessFSearch(apiConfig.CustomFSearch5); }
            if (e.KeyData == Keys.F6) { ProcessFSearch(apiConfig.CustomFSearch6); }

            if (e.KeyData == Keys.F7) { ProcessFSearch(apiConfig.CustomFSearch7); }
            if (e.KeyData == Keys.F8) { ProcessFSearch(apiConfig.CustomFSearch8); }
            if (e.KeyData == Keys.F9) { ProcessFSearch(apiConfig.CustomFSearch9); }
            if (e.KeyData == Keys.F10) { ProcessFSearch(apiConfig.CustomFSearch10); }
            if (e.KeyData == Keys.F11) { ProcessFSearch(apiConfig.CustomFSearch11); }
            if (e.KeyData == Keys.F12) { ProcessFSearch(apiConfig.CustomFSearch12); }
        }

        private void ProcessFSearch(string searchString)
        {
            if (searchString != String.Empty)
            {
                AdvancedCopyToWoWSearchOnly(searchString, 1);
            }
        }

        private void lvRealms_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //ListViewClick(lvRealms, lvwRealmColumnSorter, sender, e);
        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tsbWindowMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void tsbWindowNormal_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            tsbWindowMaximize.Visible = true;
            tsbWindowNormal.Visible = false;

        }

        private void tsbWindowMaximize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            tsbWindowMaximize.Visible = false;
            tsbWindowNormal.Visible = true;
        }

        private void SetDebugVisible(bool debugVis)
        {

        }

        private void FormMain_Leave(object sender, EventArgs e)
        {

        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            Search();
        }

        private void SelectRealmCheckboxes(System.Windows.Forms.ListView lvwRealmList, bool isChecked)
        {
            bool chk = lvwRealmList.Items[0].Checked;

            foreach (ListViewItem lvi in lvwRealmList.Items)
            {
                lvi.Checked = isChecked;
            }
        }

        private void tsbDoBackup_Click(object sender, EventArgs e)
        {
            DoBackup(apiConfig.SourcePath, apiConfig.SourcePathBackup);
        }

        private void FormMain_KeyPress(object sender, KeyPressEventArgs e)
        {
//TODO
        }

        private void lblHighlight_Click(object sender, EventArgs e)
        {

        }

        private void tsbTest_Click(object sender, EventArgs e)
        {
            //apiConfig.Save();

            string procs = String.Empty;

            foreach (Process p in Process.GetProcessesByName("Wow"))
            {
                procs += p.Id.ToString() + "\r\n";
            }

            MessageBox.Show(procs);
        }

        private void btnBlueWOW_Click(object sender, EventArgs e)
        {
            SetWowProcess(blueTeam, lblBlueWOW);
        }

        private void SetWowProcess(Team teamToProcess, Label processLabel)
        {

            FormWowProcess frmWow = new FormWowProcess();
            if (frmWow.ShowDialog() == DialogResult.OK)
            {
                teamToProcess.ProcessId = frmWow.WowProcess;
                apiConfig.Save();
                processLabel.Text = teamToProcess.ProcessId.ToString();
            }

        }

        private void AuctionsKeyPress(object sender, KeyPressEventArgs e)
        {
            this.ActiveAuctionsView = lvwAuctionsBlue;

            KeypressRealm = apiConfig.FindRealmById(((SearchResult)ActiveAuctionsView.SelectedItems[0].Tag).RealmId);

            if (char.ToUpper(e.KeyChar) == (char)Keys.C)
            {
                ProcessC();
            }
            else if (char.ToUpper(e.KeyChar) == (char)Keys.Q)
            {
                ProcessMail(1);
            }
            else if (char.ToUpper(e.KeyChar) == (char)Keys.W)
            {
                ProcessMail(2);
            }
            else if (char.ToUpper(e.KeyChar) == (char)Keys.E)
            {
                ProcessMail(3);
            }
            else if (char.ToUpper(e.KeyChar) == (char)Keys.S)
            {
                ProcessS();
            }
            else if (char.ToUpper(e.KeyChar) == (char)Keys.X)
            {
                ProcessX();
            }
            else if (char.ToUpper(e.KeyChar) == (char)Keys.Z)
            {
                ProcessZ();
            }
        }

        private void buttonTestMove_Click(object sender, EventArgs e)
        {
            Win32.ActivateApp(int.Parse(txtWowProcessId.Text));
                
            System.Threading.Thread.Sleep(500);

            Win32.MoveMouse(int.Parse(txtMoveX.Text), int.Parse(txtMoveY.Text));
        }
    }

    public class SearchParameters
    {
        public long SearchMaxG;
        public long WorthAtLeast;
        public float SearchPercentage;
        public float MinSellRate;
        public int Highlight;

        public bool RemovingDuplicates = true;
        public bool ShowCheapest = false;
        public bool ShowAll = false;
        public bool AllItemClasses = false;

        public long MinItemLevel;
        public long MaxItemLevel;

        public bool Socket;
    }

    public class SearchResult
    {
        public string AuctionId;
        public long RealmId;
        public string RealmName;
        public long Buyout;
        public long RegionMarket;
        public long ItemId;
        public int NumAuctions;
        public Color NumAuctionColor;
        public string ItemName;
        public string Quality;
        public string Class;
        public string SubClass;
        public float SaleRate;
        public System.Drawing.Color RowColor;
        public long PetId = 0;
        public long PetLevel = 0;
        public string Modifiers = String.Empty;
        public string BonusLists = String.Empty;
        public long Level = 0;
        public string Suffix = String.Empty;
    }

    public class SearchCount
    {
        public string AuctionId;
        public int Count;
        public long Cheapest;
        public SearchResult Result;
    }

}
