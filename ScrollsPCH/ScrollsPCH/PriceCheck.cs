using JsonFx.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ScrollsPCH
{
    class PriceCheck : ChatComm
    {
        public override bool hooksSend(RoomChatMessageMessage rcmm)
        {
            if (rcmm.text.StartsWith("/price") || rcmm.text.StartsWith("/pc") || rcmm.text.StartsWith("/pricecheck"))
            {
                String[] splitted = rcmm.text.Split(new char[] {' '}, 2, StringSplitOptions.None);

                if (splitted.Length >= 2)
                {
                    String playerName = splitted[1];
                    loadPlayerInfo(playerName);
                }
                else
                {
                    msg(String.Format("<color=#ede79f>Correct usage - </color><color=#eae8ce>/pc <scroll name></color><color=#ede79f>. Example -</color><color=#eae8ce> /pc noaidi</color>"));
                }

                return true;
            }
            return false;
        }

        private void loadPlayerInfo(String playerName)
        {
            WebClientTimeOut wc = new WebClientTimeOut();
            wc.TimeOut = 5000;
            wc.DownloadStringCompleted += (sender, e) =>
            {
                proc(e.Result, playerName);
            };
            wc.DownloadStringAsync(new Uri("http://scrollspc.com/api_single_scroll.php?n=" + playerName));
        }

        private void proc(String result, String playerName)
        {
            try
            {
                APIResult ar = (APIResult)new JsonReader().Read(result, System.Type.GetType("APIResult"));
                if (ar.msg.Equals("success"))
                {
                    String price;
                    if (Convert.ToInt32(ar.data.price_max) == 0)
                    {
                        price = ar.data.price;
                    }
                    else
                    {
                        price = ar.data.price + '-' + ar.data.price_max;
                    }
                    msg(String.Format("<color=#eae8ce>{0}</color> <color=#ede79f>currently costs</color> <color=#FFCC00>{1} Gold</color>.", ar.data.name, price));
                }
                else
                {
                    msg(String.Format("<color=#ede79f>Failed to load price for scroll </color><color=#eae8ce>'{0}'</color>.", playerName));
                }
            }
            catch
            {
                msg(String.Format("<color=#ede79f>Failed to load price for scroll </color><color=#eae8ce>'{0}'</color><color=#ede79f>. Try again later.</color>", playerName));
            }
        }
    }
}