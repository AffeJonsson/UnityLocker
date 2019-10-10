using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	public static class Container
	{
		public static LockSettings LockSettings;
		public static IVersionControlHandler VersionControlHandler;
		public static WWWManager WWWManager;

		static Container()
		{
			var assetGuids = AssetDatabase.FindAssets("t:" + nameof(UnityLocker.LockSettings));
			for (var i = 0; i < assetGuids.Length; i++)
			{
				var asset = AssetDatabase.LoadAssetAtPath<LockSettings>(AssetDatabase.GUIDToAssetPath(assetGuids[i]));
				if (asset != null)
				{
					LockSettings = asset;
					break;
				}
			}

			if (LockSettings == null)
			{
				LockSettings = ScriptableObject.CreateInstance<LockSettings>();
				if (!AssetDatabase.IsValidFolder("Assets/UnityLocker"))
				{
					AssetDatabase.CreateFolder("Assets", "UnityLocker");
				}
				if (!AssetDatabase.IsValidFolder("Assets/UnityLocker/Assets"))
				{
					AssetDatabase.CreateFolder("Assets/UnityLocker", "Assets");
				}
				AssetDatabase.CreateAsset(LockSettings, "Assets/UnityLocker/Assets/LockSettings.asset");
			}

			switch (LockSettings.VersionControlType)
			{
				case VersionControlType.Git:
					VersionControlHandler = new GitHandler();
					break;
				default:
					Debug.LogError("Version Control Type not implemented: " + LockSettings.VersionControlType);
					break;
			}

			WWWManager = new WWWManager();
		}
	}
}