namespace Alf.UnityLocker.Editor.AssetTypeValidators
{
	[AssetTypeValidator("Prefabs", 4)]
	public class PrefabTypeValidator : IAssetTypeValidator
	{
		public bool IsAssetValid(UnityEngine.Object asset)
		{
#if UNITY_EDITOR
			return UnityEditor.PrefabUtility.GetPrefabType(asset) != UnityEditor.PrefabType.None;
#else
			return false;
#endif
		}
	}
}