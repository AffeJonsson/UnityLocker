using UnityEngine;

namespace Alf.UnityLocker.Editor.AssetTypeValidators
{
	public interface IAssetTypeValidator
	{
		bool IsAssetValid(Object asset);
	}
}