namespace OnlineAnnounceV2
{
	public class OAInfo
	{
		public int userid;
		public string greet;
		public string leave;
		public bool wasInDatabase;

		public OAInfo(int _userid, bool _inDatabase, string _greet = "", string _leave = "")
		{
			userid = _userid;
			wasInDatabase = _inDatabase;
			greet = _greet;
			leave = _leave;
		}
	}
}
