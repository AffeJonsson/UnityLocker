namespace Alf.UnityLocker.Editor
{
	public static class ULUserManager
	{
		private static ULUser sm_currentUser;

		public static ULUser CurrentUser
		{
			get
			{
				if (sm_currentUser == null)
				{
					// TODO: Require creation of user.
				}

				return sm_currentUser;
			}
			set
			{
				sm_currentUser = value;
			}
		}
	}
}