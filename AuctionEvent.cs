using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WOWApi;

namespace WOWApi
{


    public class AuctionEvent
    {
        public event AuctionRetrievedEventHandler AuctionRetrieved;

        public delegate void AuctionRetrievedEventHandler(object sender, AuctionEventArgs e);

        protected virtual void OnAuctionRetrieved(AuctionEventArgs e)
        {
            AuctionRetrievedEventHandler handler = AuctionRetrieved;
            handler?.Invoke(this, e);
        }

        public void DoAuctionProcess(string accessToken, Realm r, bool livePoll)
        {
            HttpStatusCode statusCode = new HttpStatusCode();
            string lastModified = String.Empty;
            AuctionFileContents afc;

            //Process the auction
            afc = API_Blizzard.GetAuctionsFromAPI(accessToken, r, out statusCode, out lastModified);


            if (statusCode == HttpStatusCode.NotFound)
            {
                //Not found so nothing modified since last time
                if (livePoll)
                {
                    System.Threading.Thread.Sleep(3000);
                    DoAuctionProcess(accessToken, r, livePoll);
                }
            }
            else if (afc == null || afc.auctions == null)
            {
                //Wait 2 seconds then try again
                //System.Threading.Thread.Sleep(5000);
                //DoAuctionProcess(accessToken, r);
            }
            else
            {
                //Raise an event once we're done
                AuctionEventArgs aucArgs = new AuctionEventArgs();
                aucArgs.Auctions = afc;
                aucArgs.ConnectedRealmId = r.ConnectedRealmId;
                aucArgs.StatusCode = statusCode;
                aucArgs.LastModified = lastModified;
                aucArgs.RealmObject = r;
                r.LastModified = lastModified;

                OnAuctionRetrieved(aucArgs);
            }
        }
    }

    public class AuctionEventArgs : EventArgs
    {
        public int ConnectedRealmId { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public AuctionFileContents Auctions { get; set; }

        public Realm RealmObject { get; set; }

        public string LastModified { get; set; }
    }
}
