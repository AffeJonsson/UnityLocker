using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	public static class ULContextMenu
	{
		private const string LockMenuName = "Assets/UnityLocker/Lock";
		private const string UnlockMenuName = "Assets/UnityLocker/Unlock (Globally)";
		private const string UnlockFromCurrentCommitMenuName = "Assets/UnityLocker/Unlock (From current commit)";

		[MenuItem(LockMenuName)]
		public static void Lock()
		{
			ULLocker.TryLockAsset(Selection.activeObject, (success, errorMessage) =>
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
			return (Selection.activeObject is SceneAsset || PrefabUtility.GetPrefabType(Selection.activeObject) != PrefabType.None) && !ULLocker.IsAssetLocked(Selection.activeObject);
		}

		[MenuItem(UnlockMenuName)]
		public static void Unlock()
		{
			ULLocker.TryUnlockAsset(Selection.activeObject, (success, errorMessage) =>
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
			return (Selection.activeObject is SceneAsset || PrefabUtility.GetPrefabType(Selection.activeObject) != PrefabType.None) && ULLocker.IsAssetLockedByMe(Selection.activeObject);
		}

		[MenuItem(UnlockFromCurrentCommitMenuName)]
		public static void UnlockFromCurrentCommit()
		{
			ULLocker.TryUnlockAssetAtCurrentCommit(Selection.activeObject, (success, errorMessage) =>
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
			return (Selection.activeObject is SceneAsset || PrefabUtility.GetPrefabType(Selection.activeObject) != PrefabType.None) && ULLocker.IsAssetLockedByMe(Selection.activeObject);
		}
	}
}