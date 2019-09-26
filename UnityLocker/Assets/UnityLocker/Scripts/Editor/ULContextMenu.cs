using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	public static class ULContextMenu
	{
		private const string LockMenuName = "Assets/UnityLocker/Lock";
		private const string UnlockMenuName = "Assets/UnityLocker/Unlock";

		[MenuItem(LockMenuName)]
		public static void Lock()
		{
			ULLocker.TryLockAsset(Selection.activeObject, (success) => { });
		}

		[MenuItem(LockMenuName, true)]
		public static bool ValidateLock()
		{
			return Selection.activeObject is SceneAsset;
		}

		[MenuItem(UnlockMenuName)]
		public static void Unlock()
		{
			ULLocker.TryUnlockAsset(Selection.activeObject, (success) => { });
		}

		[MenuItem(UnlockMenuName, true)]
		public static bool ValidateUnlock()
		{
			return Selection.activeObject is SceneAsset;
		}
	}
}