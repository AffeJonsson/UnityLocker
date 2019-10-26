namespace Alf.UnityLocker.Editor.AssetTypeValidators
{
	[AssetTypeValidator("Text Assets", 8)]
	public class TextAssetTypeValidator : IAssetTypeValidator
	{
		public bool IsAssetValid(UnityEngine.Object asset)
		{
#if UNITY_EDITOR
			return asset is UnityEngine.TextAsset && !(asset is UnityEditor.MonoScript);
#else
			return false;
#endif
		}
	}
}