using System;

namespace Alf.UnityLocker.Editor.VersionControlHandlers
{
	public class VersionControlHandlerAttribute : Attribute
	{
		public string Name { get; private set; }

		public VersionControlHandlerAttribute(string name)
		{
			Name = name;
		}
	}
}