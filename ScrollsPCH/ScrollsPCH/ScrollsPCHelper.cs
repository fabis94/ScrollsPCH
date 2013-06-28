using Mono.Cecil;
using ScrollsModLoader.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using JsonFx.Json;

namespace ScrollsPCH
{
    public class ScrollsPCHelper : BaseMod, ICommListener
	{
		private List<ChatComm> commands = new List<ChatComm>();

        private bool inTrade = false;

        private Card[] mycards;
        private Card[] hiscards;

        private LibraryViewMessage me;
        private LibraryViewMessage him;

		public ScrollsPCHelper()
		{
			commands.Add(new PriceCheck());
            App.Communicator.addListener(this);
		}

		public static string GetName()
		{
			return "ScrollsPC Helper";
		}

		public static int GetVersion()
		{
			return 2;
		}

        public void proc(String result)
        {
            try
            {
                Console.WriteLine("[SPCH Upload] "+result);
            }
            catch (Exception e)
            {
                Console.WriteLine("[SPCH Upload] Exception: "+e.Message);
            }
            
        }

        public void handleMessage(Message msg)
        {
            if (msg is TradeViewMessage)
            {
                //From is the one who sent the request, To is the one who accepted it
                TradeViewMessage tvm = (TradeViewMessage)msg;
                if (tvm.from.accepted && tvm.to.accepted)
                {
                    //Trade complete send data now

                    string fromto = "null";
                    string myid = App.MyProfile.ProfileInfo.id;
                    long[] fromCardIDs, toCardIDs;
                    int myGold = -1, hisGold = -1;
                    List<int> myTypeIDList = new List<int>();
                    List<int> hisTypeIDList = new List<int>();
                    Dictionary<string, int[]> myCardsD = new Dictionary<string, int[]>();
                    Dictionary<string, int[]> hisCardsD = new Dictionary<string, int[]>();

                    fromCardIDs = tvm.from.cardIds;
                    toCardIDs = tvm.to.cardIds;

                    if (tvm.from.profile.id == App.MyProfile.ProfileInfo.id) //User initiated the trade
                    {
                        fromto = "from";
                        myGold = tvm.from.gold;
                        hisGold = tvm.to.gold;
                        foreach (long i in fromCardIDs)
                        {
                            foreach (Card c in mycards)
                            {
                                if (i == c.getId())
                                {
                                    myTypeIDList.Add(c.getType());
                                }
                            }
                        }
                        foreach (long i in toCardIDs)
                        {
                            foreach (Card c in hiscards)
                            {
                                if (i == c.getId())
                                {
                                    hisTypeIDList.Add(c.getType());
                                }
                            }
                        }
                    }
                    else if (tvm.to.profile.id == App.MyProfile.ProfileInfo.id) //User accepted the trade
                    {
                        fromto = "to";
                        myGold = tvm.to.gold;
                        hisGold = tvm.from.gold;
                        foreach (long i in toCardIDs)
                        {
                            foreach (Card c in mycards)
                            {
                                if (i == c.getId())
                                {
                                    myTypeIDList.Add(c.getType());
                                }
                            }
                        }
                        foreach (long i in fromCardIDs)
                        {
                            foreach (Card c in hiscards)
                            {
                                if (i == c.getId())
                                {
                                    hisTypeIDList.Add(c.getType());
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Wut");
                        return;
                    }

                    myCardsD.Add("cards", myTypeIDList.ToArray());
                    hisCardsD.Add("cards", hisTypeIDList.ToArray());
                    JsonWriter jw = new JsonWriter();
                    string mycards_string = jw.Write(myCardsD);
                    string hiscards_string = jw.Write(hisCardsD);

                    Console.WriteLine("[MyID] "+myid);
                    string myParameters = "fromto=" + fromto + "&mycards=" + mycards_string + "&hiscards=" + hiscards_string + "&mygold=" + myGold + "&hisgold=" + hisGold + "&uid="+myid;
                    WebClientTimeOut wc = new WebClientTimeOut();
                    wc.Headers.Add("user-agent", "ScrollsPCH/2.0");
                    wc.TimeOut = 5000;
                    wc.Headers[System.Net.HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    wc.UploadStringCompleted += (sender, e) =>
                    {
                        proc(e.Result);
                    };
                    wc.UploadStringAsync(new Uri("http://scrollspc.com/spch_gettrade.php"), myParameters);
                    this.me = null;
                    this.him = null;
                    this.mycards = null;
                    this.hiscards = null;
                }
                
            }
            if (msg is LibraryViewMessage && inTrade)
            {
                LibraryViewMessage lvm = (LibraryViewMessage)msg;
                bool flag = (lvm.profileId == App.MyProfile.ProfileInfo.id);

                if (!flag)
                {
                    this.him = lvm;
                    this.hiscards = lvm.cards;
                }
                else
                {
                    this.me = lvm;
                    this.mycards = lvm.cards;
                }

                Console.WriteLine("[SPCH] Cards set");

                /*List<int> bz = new List<int>();
                bz.Add(1);
                bz.Add(2);

                Dictionary<string, object> d = new Dictionary<string, object>();
                d.Add("mama",bz.ToArray());

                
                JsonWriter jsw = new JsonWriter();
                string jw = jsw.Write(d);

                Console.WriteLine("[json] "+ jw);*/


                
            }
        }

		public static MethodDefinition[] GetHooks(TypeDefinitionCollection scrollsTypes, int version)
		{
			try {
				return new MethodDefinition[] {
					scrollsTypes["Communicator"].Methods.GetMethod("sendRequest", new Type[]{typeof(Message)}),
                    scrollsTypes["Lobby"].Methods.GetMethod("Update")[0]
				};
			} catch {
				return new MethodDefinition[] { };
			}
		}

		public override bool BeforeInvoke(InvocationInfo info, out object returnValue)
		{
			returnValue = null;

			if (info.targetMethod.Equals("sendRequest"))
			{
				if (info.arguments[0] is RoomChatMessageMessage)
				{
                    RoomChatMessageMessage rcmm = (RoomChatMessageMessage)info.arguments[0];

					return hooks(rcmm);
				}
			}
            if (info.targetMethod.Equals("Update"))
            {
                Lobby thing = (Lobby)info.target;
                FieldInfo tsInfo = thing.GetType().GetField("tradeSystem", BindingFlags.NonPublic | BindingFlags.Instance);
                Object tradeSystem = tsInfo.GetValue(thing);
                if (tradeSystem != null)
                {
                    TradeSystem tsReal = (TradeSystem)tradeSystem;
                    if (tsReal.IsInTrade())
                    {
                        this.inTrade = true;
                    }
                    else
                    {
                        this.inTrade = false;
                    }
                }
                else
                {
                    Console.WriteLine("tradeSystem == null");
                }
            }
			return false;
		}

		public override void AfterInvoke(InvocationInfo info, ref object returnValue)
		{
			return;
		}

		/**
		 * returns true when the text message is a command for one of the functions
		 */
		private bool hooks(RoomChatMessageMessage rcmm)
		{
			bool h = false;
			foreach (ChatComm cc in commands)
			{
				bool hooksSingle = cc.hooksSend(rcmm);

				h |= hooksSingle;
			}
			return h;
		}

        public void onReconnect()
        {
            //Whutevs
        }

        public void PopupCancel(string popupType)
        {
            throw new NotImplementedException();
        }

        public void PopupOk(string popupType, string choice)
        {
            throw new NotImplementedException();
        }
    }
}
