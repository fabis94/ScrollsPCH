using Mono.Cecil;
using ScrollsModLoader.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScrollsPCH
{
	public class ScrollsPCHelper : BaseMod
	{
		private List<ChatComm> commands = new List<ChatComm>();

		public ScrollsPCHelper()
		{
			commands.Add(new PriceCheck());
		}

		public static string GetName()
		{
			return "ScrollsPC Helper";
		}

		public static int GetVersion()
		{
			return 1;
		}

		public static MethodDefinition[] GetHooks(TypeDefinitionCollection scrollsTypes, int version)
		{
			try {
				return new MethodDefinition[] {
					scrollsTypes["Communicator"].Methods.GetMethod("sendRequest", new Type[]{typeof(Message)})
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
	}
}
