using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace OnlineAnnounceV2
{
	[ApiVersion(2,1)]
	public class OAMain : TerrariaPlugin
	{
		#region PluginInfo
		public override string Author => "Comdar";

		public override string Description => "An announcement plugin for players.";

		public override string Name => "OnlineAnnounceV2";

		public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
		#endregion

		#region Init/Dispose
		public override void Initialize()
		{
			ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
			PlayerHooks.PlayerPostLogin += OnLogin;
			PlayerHooks.PlayerLogout += OnLogout;
			GeneralHooks.ReloadEvent += OnReload;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
				PlayerHooks.PlayerPostLogin -= OnLogin;
				PlayerHooks.PlayerLogout -= OnLogout;
				GeneralHooks.ReloadEvent -= OnReload;
			}
			base.Dispose(disposing);
		}
		#endregion

		public OAMain(Main game)
			: base(game)
		{

		}

		public static string OAString = "OnlineAnnounce.PlayerInfo";
		public static Config config;
		private static string OAPath = Path.Combine(TShock.SavePath, "OnlineAnnounceConfig.json");

		#region Hooks
		private void OnInitialize(EventArgs args)
		{
			DB.Connect();
			Config.Read(OAPath);
			Commands.ChatCommands.Add(new Command("oa.greet", OACommand, "greet", "leave") { AllowServer = false });
			Commands.ChatCommands.Add(new Command("oa.mod", OARead, "readgreet", "readleave"));
			Commands.ChatCommands.Add(new Command("oa.mod", OASet, "setgreet", "setleave"));
		}

		private void OnLogin(PlayerPostLoginEventArgs args)
		{
			string greet = DB.SetInfo(args.Player);
			if (!string.IsNullOrWhiteSpace(greet))
				TSPlayer.All.SendMessage($"[{args.Player.Account.Name}] " + greet, config.ToColor());
		}

		private void OnLogout(PlayerLogoutEventArgs args)
		{
			string leave = args.Player.GetData<OAInfo>(OAString).leave;
			if (!string.IsNullOrWhiteSpace(leave))
				TSPlayer.All.SendMessage($"[{args.Player.Account.Name}] " + leave, config.ToColor());
		}

		private void OnReload(ReloadEventArgs args)
		{
			Config.Read(OAPath);
			foreach(TSPlayer plr in TShock.Players.Where(e => e != null && e.IsLoggedIn))
			{
				DB.SetInfo(plr);
			}
		}
		#endregion

		#region Command
		private void OACommand(CommandArgs args)
		{
			bool isGreet;
			if (args.Message.ToLower().StartsWith("greet"))
				isGreet = true;
			else
				isGreet = false;

			OAInfo info = args.Player.GetData<OAInfo>(OAString);

			if (args.Parameters.Count == 0)
			{
				args.Player.SendErrorMessage($"Invalid syntax: /{(isGreet ? "greet" : "leave")} <announcement/-none>");
				return;
				//if (!info.wasInDatabase)
					

				//if (isGreet)
				//	info.greet = "";
				//else
				//	info.leave = "";

				//args.Player.SendSuccessMessage($"You removed your {isGreet.OAType()} announcement.");
				//args.Player.SetData(OAString, info);
				//DB.UpdateAnnouncement(info);
				//return;
			}

			string message = string.Join(" ", args.Parameters);

			if (!args.Player.HasPermission("oa.mod"))
				message.StripColors();

			if (message.ToLower() == "-none")
				message = "";

			if (isGreet)
				info.greet = message;
			else
				info.leave = message;

			args.Player.SendSuccessMessage($"You set your {isGreet.OAType()} announcement to: " + (isGreet ? info.greet : info.leave));
			args.Player.SetData(OAString, info);

			if (info.wasInDatabase)
				DB.UpdateAnnouncement(info);
			else
				DB.AddAnnouncement(info);
		}

		private void OARead(CommandArgs args)
		{
			bool isGreet = args.Message.StartsWith("readgreet");

			if (args.Parameters.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid player name.");
				return;
			}

			string playerName = string.Join(" ", args.Parameters);

			var pList = TSPlayer.FindByNameOrID(playerName);

			if (pList.Count == 0)
			{
				args.Player.SendErrorMessage("No players found by the name: " + playerName);
				return;
			}
			if (pList.Count > 1)
			{
				args.Player.SendMultipleMatchError(pList.Select(e => e.Name));
				return;
			}
			OAInfo info = pList[0].GetData<OAInfo>(OAString);
			args.Player.SendSuccessMessage($"{pList[0].Name}'s greeting and leaving announcements:");
			args.Player.SendMessage(info.greet, config.ToColor());
			args.Player.SendMessage(info.leave, config.ToColor());
		}

		private void OASet(CommandArgs args)
		{
			bool isGreet = args.Message.StartsWith("setgreet");

			if (args.Parameters.Count < 2)
			{
				args.Player.SendErrorMessage($"Invalid syntax: {(isGreet ? "setgreet" : "setleave")} <player> <announcement>");
				return;
			}

			string playerName = args.Parameters[0];
			string message = string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));

			var pList = TSPlayer.FindByNameOrID(playerName);

			if (pList.Count == 0)
			{
				args.Player.SendErrorMessage("No players found by the name: " + playerName);
				return;
			}
			if (pList.Count > 1)
			{
				args.Player.SendMultipleMatchError(pList.Select(e => e.Name));
				return;
			}
			OAInfo info = pList[0].GetData<OAInfo>(OAString);
			if (isGreet)
				info.greet = message;
			else
				info.leave = message;

			if (info.wasInDatabase)
				DB.UpdateAnnouncement(info);
			else
				DB.AddAnnouncement(info);

			info.wasInDatabase = true;
			pList[0].SetData(OAString, info);
			args.Player.SendSuccessMessage($"You have successfully set {pList[0].Name}'s {(isGreet.OAType())} to {message}");
		}
		#endregion
	}
}
