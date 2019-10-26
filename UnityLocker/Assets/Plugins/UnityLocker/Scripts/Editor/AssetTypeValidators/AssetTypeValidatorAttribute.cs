using System;

namespace Alf.UnityLocker.Editor.AssetTypeValidators
{
	public class AssetTypeValidatorAttribute : Attribute
	{
		public string Name { get; }
		public int Flag { get; }

		public AssetTypeValidatorAttribute(string name, int flag)
		{
			Name = name;
			Flag = flag;
		}
	}
}