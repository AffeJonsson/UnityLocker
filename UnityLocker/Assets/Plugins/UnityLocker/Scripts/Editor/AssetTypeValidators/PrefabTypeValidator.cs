namespace Alf.UnityLocker.Editor.AssetTypeValidators
{
	[AssetTypeValidator("Prefabs", 4)]
	public class PrefabTypeValidator : IAssetTypeValidator
	{
		public bool IsAssetValid(UnityEngine.Object asset)
		{
#if UNITY_EDITOR
			if (asset == null)
			{
				return false;
			}
	#if UNITY_2018_3_OR_NEWER
			return UnityEditor.PrefabUtility.GetPrefabAssetType(asset) != UnityEditor.PrefabAssetType.NotAPrefab;
	#else
			return UnityEditor.PrefabUtility.GetPrefabType(asset) != UnityEditor.PrefabType.None;
	#endif
#else
			return false;
#endif
		}
	}
}