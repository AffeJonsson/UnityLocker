using System.Linq;
using UnityEditor;

namespace Alf.UnityLocker.Editor
{
	public static class ContextMenu
	{
		private const string LockMenuName = "Assets/Lock";
		private const string RevertMenuName = "Assets/Revert Lock";
		private const string UnlockMenuName = "Assets/Unlock";
		private const string OpenSettingsFileMenuName = "Tools/Open Locker Settings File";
		private const int Priority = 600;
		
		[MenuItem(LockMenuName, priority = Priority)]
		public static void Lock()
		{
			var filtered = Selection.objects.Where(s => AssetDatabase.Contains(s)).ToArray();
			Locker.TryLockAssets(filtered, (success, errorMessage) =>
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
			return Selection.activeObject != null && !Locker.IsAssetLocked(Selection.activeObject) && Container.GetAssetTypeValidators().IsAssetValid(Selection.activeObject);
		}

		[MenuItem(RevertMenuName, priority = Priority + 1)]
		public static void RevertLock()
		{
			var filtered = Selection.objects.Where(s => AssetDatabase.Contains(s)).ToArray();
			Locker.TryRevertAssetLocks(filtered, (success, errorMessage) =>
			{
				if (!success)
				{
					EditorUtility.DisplayDialog("Asset reverting failed", "Asset reverting failed\n" + errorMessage, "OK");
				}
			});
		}

		[MenuItem(RevertMenuName, true)]
		public static bool ValidateRevertLock()
		{
			return Selection.activeObject != null && Locker.IsAssetLockedByMe(Selection.activeObject);
		}

		[MenuItem(UnlockMenuName, priority = Priority + 2)]
		public static void Unlock()
		{
			var filtered = Selection.objects.Where(s => AssetDatabase.Contains(s)).ToArray();
			Locker.TryUnlockAssets(filtered, (success, errorMessage) =>
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
			return Selection.activeObject != null && Locker.IsAssetLockedByMe(Selection.activeObject);
		}

		[MenuItem(OpenSettingsFileMenuName, priority = 10000)]
		public static void OpenSettingsFile()
		{
			Selection.activeObject = Container.GetLockSettings();
		}
	}
}