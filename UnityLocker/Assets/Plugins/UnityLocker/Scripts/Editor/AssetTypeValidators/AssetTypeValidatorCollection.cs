using System.Collections.Generic;

namespace Alf.UnityLocker.Editor.AssetTypeValidators
{
	public class AssetTypeValidatorCollection : Dictionary<IAssetTypeValidator, int>
	{
		public bool IsAssetValid(UnityEngine.Object asset)
		{
			var flag = Container.GetLockSettings().AssetTypeValidators;
			foreach (var kvp in this)
			{
				if ((flag & kvp.Value) != 0)
				{
					if (kvp.Key.IsAssetValid(asset))
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}