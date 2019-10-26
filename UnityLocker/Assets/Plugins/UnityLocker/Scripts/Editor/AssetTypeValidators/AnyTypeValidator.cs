namespace Alf.UnityLocker.Editor.AssetTypeValidators
{
	[AssetTypeValidator("Any", 1)]
	public class AnyTypeValidator : IAssetTypeValidator
	{
		public bool IsAssetValid(UnityEngine.Object asset)
		{
			return true;
		}
	}
}