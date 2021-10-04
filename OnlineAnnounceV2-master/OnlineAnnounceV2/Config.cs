using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace OnlineAnnounceV2
{
	public class Config
	{
		public List<string> badwords = new List<string>()
		{
			"admin",
			"mod",
			"staff",
			"owner"
		};

		public int defaultR = 127;
		public int defaultG = 255;
		public int defaultB = 212;

		public void Write(string path)
		{
			File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
		}

		public static void Read(string path)
		{
			OAMain.config = !File.Exists(path)
				? new Config()
				: JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
		}

		public Color ToColor()
		{
			return new Color(defaultR, defaultG, defaultB);
		}
	}
}
