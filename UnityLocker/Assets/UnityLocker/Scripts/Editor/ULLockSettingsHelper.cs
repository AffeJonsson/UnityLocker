using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	public static class ULLockSettingsHelper
	{
		public static ULLockSettings Settings;

		static ULLockSettingsHelper()
		{
			var assetGuids = AssetDatabase.FindAssets("t:ULLockSettings");
			for (var i = 0; i < assetGuids.Length; i++)
			{
				var asset = AssetDatabase.LoadAssetAtPath<ULLockSettings>(AssetDatabase.GUIDToAssetPath(assetGuids[i]));
				if (asset != null)
				{
					Settings = asset;
					break;
				}
			}

			if (Settings == null)
			{
				Settings = ScriptableObject.CreateInstance<ULLockSettings>();
				if (!AssetDatabase.IsValidFolder("Assets/UnityLocker"))
				{
					AssetDatabase.CreateFolder("Assets", "UnityLocker");
				}
				if (!AssetDatabase.IsValidFolder("Assets/UnityLocker/Assets"))
				{
					AssetDatabase.CreateFolder("Assets/UnityLocker", "Assets");
				}
				AssetDatabase.CreateAsset(Settings, "Assets/UnityLocker/Assets/ULLockSettings.asset");

			}
		}
	}
}