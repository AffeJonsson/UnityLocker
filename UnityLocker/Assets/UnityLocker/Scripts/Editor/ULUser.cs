using System.Collections.Generic;

namespace Alf.UnityLocker.Editor
{
	public class ULUser
	{
		public readonly string Name;

		public ULUser(string name)
		{
			UnityEngine.Debug.Log("User " + name + " created");
			Name = name;
		}

		public override bool Equals(object obj)
		{
			var user = obj as ULUser;
			if (user == null)
			{
				return false;
			}
			return user.Name == Name;
		}

		public override int GetHashCode()
		{
			return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
		}
	}
}