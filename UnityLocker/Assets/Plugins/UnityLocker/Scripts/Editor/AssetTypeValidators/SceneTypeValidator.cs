namespace Alf.UnityLocker.Editor.AssetTypeValidators
{
	[AssetTypeValidator("Scenes", 2)]
	public class SceneTypeValidator : IAssetTypeValidator
	{
		public bool IsAssetValid(UnityEngine.Object asset)
		{
#if UNITY_EDITOR
			return asset is UnityEditor.SceneAsset;
#else
			return false;
#endif
		}
	}
}