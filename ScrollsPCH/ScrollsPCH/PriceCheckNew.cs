using JsonFx.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;


namespace ScrollsPCH
{
    class PriceCheckNew : ChatComm
    {
        public override bool hooksSend(RoomChatMessageMessage rcmm)
        {
            if (rcmm.text.StartsWith("/lpc") || rcmm.text.StartsWith("/npc") || rcmm.text.StartsWith("/2pc"))
            {
                String[] splitted = rcmm.text.Split(new char[] { ' ' }, 2, StringSplitOptions.None);

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
            try
            {
                wc.DownloadStringAsync(new Uri("http://scrollspc.com/api_single_scrollv2.php?n=" + playerName));
            }
            catch (Exception e)
            {
                msg(String.Format("<color=#ede79f>WebClient exception - contact the author of this plugin or try again later.</color>"));
                Console.WriteLine("[SPCH WebClient Exception] "+e.Message);
            }
        }

        private void proc(String result, String playerName)
        {
            try
            {
                Console.WriteLine("[SPCH Server Response] " + result);
                APIResult ar = (APIResult)new JsonReader().Read(result, System.Type.GetType("APIResult"));
                if (ar.msg.Equals("success"))
                {
                    if (ar.data.live_price != null)
                    {
                        msg(String.Format("<color=#eae8ce>{0}</color>'s <color=#ede79f>Live Price currently is</color> <color=#FFCC00>{1} Gold</color>.", ar.data.name, ar.data.live_price));
                    }
                    else
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
                        msg(String.Format("<color=#ede79f>There currently isn't a live price for </color><color=#eae8ce>{0}</color>. Its static price is <color=#FFCC00>{1} Gold</color>.", ar.data.name, price));

                        
                    }            
                }
                else
                {
                    msg(String.Format("<color=#ede79f>Failed to load price for scroll </color><color=#eae8ce>'{0}'</color>.", playerName));
                    Console.WriteLine("[SPCH] Scroll not found");
                }
            }
            catch (Exception e)
            {
                msg(String.Format("<color=#ede79f>Failed to load price for scroll </color><color=#eae8ce>'{0}'</color><color=#ede79f>. Try again later.</color>", playerName));
                Console.WriteLine("[SPCH] proc() Exception caught: " + e.Message);
            }
        }
    }
}