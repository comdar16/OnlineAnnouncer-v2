using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using TShockAPI.DB;

namespace OnlineAnnounceV2
{
	public static class DB
	{
		private static IDbConnection db;

		public static void Connect()
		{
			switch (TShock.Config.Settings.StorageType.ToLower())
			{
				case "mysql":
					string[] dbHost = TShock.Config.Settings.MySqlHost.Split(':');
					db = new MySqlConnection()
					{
						ConnectionString = string.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
							dbHost[0],
							dbHost.Length == 1 ? "3306" : dbHost[1],
							TShock.Config.Settings.MySqlDbName,
							TShock.Config.Settings.MySqlUsername,
							TShock.Config.Settings.MySqlPassword)

					};
					break;

				case "sqlite":
					string sql = Path.Combine(TShock.SavePath, "OnlineAnnounce.sqlite");
					db = new SqliteConnection(string.Format("uri=file://{0},Version=3", sql));
					break;

			}

			SqlTableCreator sqlcreator = new SqlTableCreator(db, db.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
            
			sqlcreator.EnsureTableStructure(new SqlTable("onlineannounce",
				new SqlColumn("userid", MySqlDbType.Int32) { Primary = true, Unique = true, Length = 6 },
				new SqlColumn("greet", MySqlDbType.Text) { Length = 100 },
				new SqlColumn("leaving", MySqlDbType.Text) { Length = 100 }));
		}

		public static void AddAnnouncement(OAInfo info)
		{
            		int result = db.Query("INSERT INTO `onlineannounce` (`userid`, `greet`, `leaving`) VALUES (@0, @1, @2);", info.userid, info.greet, info.leave);
			if (result != 1)
			{
				TShock.Log.ConsoleError("Error adding entry to database for user: " + info.userid);
			}
		}

		public static void UpdateAnnouncement(OAInfo info)
		{
            		int result = db.Query("UPDATE `onlineannounce` SET `greet` = @0, `leaving` = @1 WHERE `userid` = @2; ", info.greet, info.leave, info.userid);

            		if (result != 1)
			{
				TShock.Log.ConsoleError("Error updating entry in database for user: " + info.userid);
			}
		} 

        	public static void DeleteAnnouncement(int userid)
		{
           		int result = db.Query("DELETE FROM `onlineannounce` WHERE `userid` = @0;", userid);
			if (result != 1)
			{
				TShock.Log.ConsoleError("Error deleting entry in database for user: " + userid);
			}
		}

		public static string SetInfo(TSPlayer plr)
		{
            		//Using null to signify that it was not in database
			OAInfo newInfo = new OAInfo(plr.Account.ID, false, null, null);

			using (var reader = db.QueryReader("SELECT * FROM `onlineannounce` WHERE `userid` = @0;", plr.Account.ID))
			{
				if (reader.Read())
				{
					newInfo.wasInDatabase = true;
					newInfo.greet = reader.Get<string>("greet");
					newInfo.leave = reader.Get<string>("leaving");
				}
			}

			plr.SetData(OAMain.OAString, newInfo);
			return newInfo.greet;
		}
	}
}
