using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WOWApi
{
    public static class Bonuses
    {
        static List<long> SocketIDs = new List<long> { 523, 563, 564, 565, 572, 608, 1808, 3475, 3522, 4802, 6514, 6672, 6935, 7576, 7580, 7935, 8289, 8780, 8781, 8782, 8810, 9413, 9436, 9438, 9516 };

        public static bool HasSocket (List<long> bonusIds)
        {
            if (bonusIds == null)
            {
                return false;
            }

            bool returnVal = false;

            foreach (long bonus in bonusIds)
            {
                if (SocketIDs.Contains(bonus))
                {
                    returnVal = true;
                    break;
                }
            }

            return returnVal;
        }
    }
}
