using UnityEditor;

namespace Alf.UnityLocker.Editor
{
	public static class ContextMenu
	{
		private const string LockMenuName = "Assets/UnityLocker/Lock";
		private const string UnlockMenuName = "Assets/UnityLocker/Unlock (Globally)";
		private const string UnlockFromCurrentCommitMenuName = "Assets/UnityLocker/Unlock (From current commit)";
		private const string OpenSettingsFileMenuName = "Assets/UnityLocker/Open Settings File";

		[MenuItem(LockMenuName)]
		public static void Lock()
		{
			Locker.TryLockAsset(Selection.activeObject, (success, errorMessage) =>
			{
				if (!success)
				{
					EditorUtility.DisplayDialog("Asset locking failed", "Asset locking failed\n" + errorMessage, "OK");
				}
			});
		}

		[MenuItem(LockMenuName, true)]
		public static bool ValidateLock()
		{
			return (Selection.activeObject is SceneAsset || PrefabUtility.GetPrefabType(Selection.activeObject) != PrefabType.None) && !Locker.IsAssetLocked(Selection.activeObject);
		}

		[MenuItem(UnlockMenuName)]
		public static void Unlock()
		{
			Locker.TryUnlockAsset(Selection.activeObject, (success, errorMessage) =>
			{
				if (!success)
				{
					EditorUtility.DisplayDialog("Asset unlocking failed", "Asset unlocking failed\n" + errorMessage, "OK");
				}
			});
		}

		[MenuItem(UnlockMenuName, true)]
		public static bool ValidateUnlock()
		{
			return (Selection.activeObject is SceneAsset || PrefabUtility.GetPrefabType(Selection.activeObject) != PrefabType.None) && Locker.IsAssetLockedByMe(Selection.activeObject);
		}

		[MenuItem(UnlockFromCurrentCommitMenuName)]
		public static void UnlockFromCurrentCommit()
		{
			Locker.TryUnlockAssetAtCurrentCommit(Selection.activeObject, (success, errorMessage) =>
			{
				if (!success)
				{
					EditorUtility.DisplayDialog("Asset unlocking failed", "Asset unlocking failed\n" + errorMessage, "OK");
				}
			});
		}

		[MenuItem(UnlockFromCurrentCommitMenuName, true)]
		public static bool ValidateUnlockFromCurrentCommit()
		{
			return Container.GetLockSettings().VersionControlName != "None" && (Selection.activeObject is SceneAsset || PrefabUtility.GetPrefabType(Selection.activeObject) != PrefabType.None) && Locker.IsAssetLockedByMe(Selection.activeObject);
		}

		[MenuItem(OpenSettingsFileMenuName, priority = 10000)]
		public static void OpenSettingsFile()
		{
			Selection.activeObject = Container.GetLockSettings();
		}
	}
}