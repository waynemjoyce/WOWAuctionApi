using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WOWApi.Helpers;
using System.IO;
using System.Text.RegularExpressions;

namespace WOWApi
{
    public partial class FormUtilities : Form
    {
        private FormMain mainForm;

        public FormUtilities()
        {
            InitializeComponent();
            mainForm = (FormMain) Application.OpenForms[0];
        }

        private void btnFixExport_Click(object sender, EventArgs e)
        {
            string contents = File.ReadAllText(this.txtTSMItemExportPath.Text);

            string newContents = String.Empty;
            List<string> contentList = new List<string>();

            foreach (string item in contents.Split(','))
            {
                if (!(item.Contains("::") && !(contentList.Contains(item))))
                {
                    contentList.Add(item);
                }
            }

            foreach (string uniqueItem in contentList)
            {
                newContents += uniqueItem + ",";
            }

            //Truncate final comma
            newContents = newContents.Substring(0, newContents.Length - 1);
            File.WriteAllText(this.txtTSMItemExportPath.Text.Replace(".txt", "") + "_new.txt", newContents);
        }

        private void btnPBSToI_Click(object sender, EventArgs e)
        {
            string pbsContents = System.IO.File.ReadAllText(this.txtPBSToI.Text);
            List<string> parsedItems = new List<string>();
            TsmItem regionItem;
            float saleRate = 0;
            long regionAvg = 0;
            string itemList = "";
            string strippedItem = "";

            string[] items = pbsContents.Split('^');

            foreach (string item in items)
            {
                strippedItem = item;

                if (strippedItem.Contains("\""))
                {
                    strippedItem = InBetweenQM(strippedItem);
                }

                foreach (KeyValuePair<long, Item> cachedItem in mainForm.DictionaryItemCache)
                {
                    if (cachedItem.Value.Name != null)
                    {
                        if (strippedItem.ToUpper() == cachedItem.Value.Name.ToUpper())
                        {
                            saleRate = 0;
                            regionAvg = 0;
                            mainForm.RegionItems.TryGetValue(cachedItem.Key, out regionItem);
                            if (regionItem != null)
                            {
                                saleRate = regionItem.saleRate;
                                regionAvg = regionItem.avgSalePrice;
                            }

                            //So here we should have region item and cache item, let's output stuff
                            parsedItems.Add(cachedItem.Value.Name + "," + cachedItem.Key.ToString() + "," + saleRate.ToString() + "," + StringHelper.FormatItemPriceGoldOnly(regionAvg));

                            itemList += "i:" + cachedItem.Key.ToString() + ",";
                            break;
                        }
                    }

                }
            }


            itemList = itemList.Substring(0, itemList.Length - 1);

            System.IO.File.WriteAllLines(this.txtPBSToI.Text.Replace(".txt", "") + "_parsed.txt", parsedItems);

            System.IO.File.WriteAllText(this.txtPBSToI.Text.Replace(".txt", "") + "_iformat.txt", itemList);
        }

        private string InBetweenQM(string textToCheck)
        {
            string returnVal = textToCheck;
            Regex regex = new Regex("\"(.*?)\"");

            var matches = regex.Matches(textToCheck);

            if (matches.Count > 0)
            {
                returnVal = matches[0].Groups[1].ToString();
            }

            return returnVal;
        }

        private void btnIToCSV_Click(object sender, EventArgs e)
        {
            string iContents = System.IO.File.ReadAllText(this.txtIToCSV.Text);

            List<string> parsedItems = new List<string>();
            TsmItem regionItem;
            float saleRate = 0;
            long regionAvg = 0;
            //string itemList = "";
            string strippedItem = "";

            string[] items = iContents.Split(',');

            foreach (string item in items)
            {
                strippedItem = item;
                strippedItem = strippedItem.Replace("i:", "");

                foreach (KeyValuePair<long, Item> cachedItem in mainForm.DictionaryItemCache)
                {
                    if (long.Parse(strippedItem) == cachedItem.Key)
                    {
                        saleRate = 0;
                        regionAvg = 0;
                        mainForm.RegionItems.TryGetValue(cachedItem.Key, out regionItem);
                        if (regionItem != null)
                        {
                            saleRate = regionItem.saleRate;
                            regionAvg = regionItem.avgSalePrice;
                        }

                        //So here we should have region item and cache item, let's output stuff
                        parsedItems.Add(cachedItem.Value.Name + "," + cachedItem.Key.ToString() + "," + saleRate.ToString() + "," + StringHelper.FormatItemPriceGoldOnly(regionAvg));
                        break;
                    }
                }
            }

            System.IO.File.WriteAllLines(this.txtIToCSV.Text.Replace(".txt", "") + "_parsed.csv", parsedItems);
        }

        private void btnListAllClasses_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            List<string> classes = new List<string>();
            txtOutput1.Text = String.Empty;

            foreach (KeyValuePair<long,Item> item in mainForm.DictionaryItemCache)
            {
                if(!(classes.Contains(item.Value.ClassName)))
                {
                    classes.Add(item.Value.ClassName);
                }
            }

            foreach (string class1 in classes)
            {
                txtOutput1.Text += class1 + "\r\n";
            }

            Cursor.Current = Cursors.Default;
        }

        private void btnProcessItemCache_Click(object sender, EventArgs e)
        {
            //txtOutput1.Clear();
            txtOutput2.Clear();
            txtOutput3.Clear();
            txtOutput4.Clear();

            //Item IDs in txtOutput1

            //We want region prices in txtOutput2

            FormMain frmMain = (FormMain) Application.OpenForms[0];
            TsmItem regionItem;

            foreach (string s in txtOutput1.Lines)
            {
                regionItem = new TsmItem();
                frmMain.RegionItems.TryGetValue(long.Parse(s), out regionItem);

                try
                {
                    if (regionItem != null)
                    {
                        txtOutput2.AppendText((regionItem.marketValue / 10000).ToString() + "\r\n");
                    }
                    else
                    {
                        txtOutput2.AppendText("Not found\r\n");
                    }
                }
                catch
                {
                    txtOutput2.AppendText("Error\r\n");
                }
                    

            }

            /*
            foreach (KeyValuePair<long, Item> cachedItem in frmMain.DictionaryItemCache)
            {
                if (cachedItem.Value.Id > 215000 && cachedItem.Value.ClassName == "Recipe")
                {
                    frmMain.RegionItems.TryGetValue(cachedItem.Key, out regionItem);
                    {
                        if (regionItem != null && 
                            (
                                (chkToggle1.Checked == false && (!cachedItem.Value.Name.Contains("Algari Competitor")))
                                ||
                                (chkToggle1.Checked == true && (cachedItem.Value.Name.Contains("Algari Competitor")))
                            )
                           )
                        {

                            txtOutput1.Text += cachedItem.Value.Id.ToString() + "\r\n";
                            txtOutput2.Text += cachedItem.Value.Name + "\r\n";
                            txtOutput3.Text += (regionItem.marketValue / 10000).ToString() + "\r\n";
                            txtOutput4.Text +=
                                String.Format(
                                @"<AuctionListItem Name=""{0}"" Id =""{1}"" MaxGold = ""{2}"" />"
                                + "\r\n", cachedItem.Value.Name, cachedItem.Value.Id.ToString(), "5001");

                        }
                    }
                }
            }
            */
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FormMain frmMain = (FormMain)Application.OpenForms[0];
            BlizzItem item = API_Blizzard.GetBlizzItemFromItemId(frmMain.accessToken, 225725);
            MessageBox.Show(item.level.ToString());
        }

        /*
         * 
         *         public Config apiConfig;

        public Dictionary<int, AuctionFileContents> RealmAuctions = new Dictionary<int, AuctionFileContents>();
        public Dictionary<long, TsmItem> RegionItems = new Dictionary<long, TsmItem>();
        public Dictionary<long, TsmItem> RegionPets = new Dictionary<long, TsmItem>();
        public SortedDictionary<long, Item> DictionaryItemCache = new SortedDictionary<long, Item>();
        public SortedDictionary<long, Pet> DictionaryPetCache = new SortedDictionary<long, Pet>();

        public Dictionary<long, Item> SpecialItemCache = new Dictionary<long, Item>();

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
}
