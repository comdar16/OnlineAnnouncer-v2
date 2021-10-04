using System.Text.RegularExpressions;
using TShockAPI;

namespace OnlineAnnounceV2
{
	public static class Extensions
	{
		private static string ColorRegex = @"\[c\/\w{3,6}:\w+\]";

		public static void StripColors(this string str)
		{
			str = Regex.Replace(str, ColorRegex, ReplaceString, RegexOptions.IgnoreCase);
		}

		public static string Specfier(this bool isSilent)
		{
			return isSilent ? TShock.Config.Settings.CommandSilentSpecifier : TShock.Config.Settings.CommandSpecifier;
		}

		public static string OAType(this bool isGreet)
		{
			return isGreet ? "greeting" : "leaving";
		}

		private static string ReplaceString(Match match)
		{
			return match.Value.Substring(match.Value.IndexOf(":") + 1).TrimEnd(']');
		}
	}
}
