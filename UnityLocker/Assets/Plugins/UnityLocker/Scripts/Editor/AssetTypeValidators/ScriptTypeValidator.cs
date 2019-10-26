namespace Alf.UnityLocker.Editor.AssetTypeValidators
{
	[AssetTypeValidator("Scripts", 16)]
	public class ScriptTypeValidator : IAssetTypeValidator
	{
		public bool IsAssetValid(UnityEngine.Object asset)
		{
#if UNITY_EDITOR
			return asset is UnityEditor.MonoScript;
#else
			return false;
#endif
		}
	}
}