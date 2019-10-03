using System.Collections.Generic;

namespace Alf.UnityLocker.Editor
{
	public static class ULUserManager
	{
		private static ULUser sm_currentUser;
		private static Dictionary<string, ULUser> sm_users = new Dictionary<string, ULUser>(8);

		public static ULUser CurrentUser { get; set; }

		static ULUserManager()
		{
			CurrentUser = GetUser(ULLockSettingsHelper.Settings.Username);
		}

		public static ULUser GetUser(string name)
		{
			ULUser user;
			if (sm_users.TryGetValue(name, out user))
			{
				return user;
			}
			user = new ULUser(name);
			sm_users.Add(name, user);
			return user;
		}
	}
}