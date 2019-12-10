using System.Linq;
using UnityEditor;

namespace Alf.UnityLocker.Editor.ContextMenus
{
	[InitializeOnLoad]
	public static class AssetContextMenu
	{
		private const string LockMenuName = "Assets/" + Constants.LockName;
		private const string RevertMenuName = "Assets/" + Constants.RevertName;
		private const string FinishLockMenuName = "Assets/" + Constants.FinishName;
		private const string ViewHistoryMenuName = "Assets/" + Constants.HistoryName;

		private const int Priority = 600;

		[MenuItem(LockMenuName, priority = Priority)]
		public static void Lock()
		{
			var filtered = Selection.objects.Where(s => s != null).ToArray();
			TryLockAssets(filtered);
		}

		[MenuItem(LockMenuName, true)]
		public static bool ValidateLock()
		{
			if (!Container.GetLockSettings().IsEnabled)
			{
				return false;
			}
			if (Selection.objects == null || Selection.objects.Length == 0)
			{
				return false;
			}
			var filtered = Selection.objects.Where(s => s != null);
			if (filtered.Count() == 0)
			{
				return false;
			}
			foreach (var o in filtered)
			{
				if (!Locker.GetIsLockValid(o))
				{
					return false;
				}
			}
			return true;
		}

		[MenuItem(RevertMenuName, priority = Priority + 1)]
		public static void RevertLock()
		{
			var filtered = Selection.objects.Where(s => s != null).ToArray();
			TryRevertAssets(filtered);
		}

		[MenuItem(RevertMenuName, true)]
		public static bool ValidateRevertLock()
		{
			if (!Container.GetLockSettings().IsEnabled)
			{
				return false;
			}
			if (Selection.objects == null || Selection.objects.Length == 0)
			{
				return false;
			}
			var filtered = Selection.objects.Where(s => s != null);
			if (filtered.Count() == 0)
			{
				return false;
			}
			foreach (var o in filtered)
			{
				if (!Locker.GetIsRevertLockValid(o))
				{
					return false;
				}
			}
			return true;
		}

		[MenuItem(FinishLockMenuName, priority = Priority + 2)]
		public static void FinishLock()
		{
			var filtered = Selection.objects.Where(s => s != null).ToArray();
			TryFinishLockingAssets(filtered);
		}

		[MenuItem(FinishLockMenuName, true)]
		public static bool ValidateFinishLock()
		{
			if (!Container.GetLockSettings().IsEnabled)
			{
				return false;
			}
			if (Selection.objects == null || Selection.objects.Length == 0)
			{
				return false;
			}
			var filtered = Selection.objects.Where(s => s != null);
			if (filtered.Count() == 0)
			{
				return false;
			}
			foreach (var o in filtered)
			{
				if (!Locker.GetIsFinishLockValid(o))
				{
					return false;
				}
			}
			return true;
		}

		[MenuItem(ViewHistoryMenuName, priority = Priority + 3)]
		public static void ViewHistory()
		{
			HistoryWindow.Show(Selection.activeObject);
		}

		[MenuItem(ViewHistoryMenuName, true)]
		public static bool ValidateViewHistory()
		{
			if (!Container.GetLockSettings().IsEnabled)
			{
				return false;
			}
			if (Selection.activeObject == null)
			{
				return false;
			}
			return Locker.IsAssetTypeValid(Selection.activeObject);
		}

		private static void TryLockAssets(UnityEngine.Object[] assets)
		{
			Locker.TryLockAssets(assets, null, (errorMessage) =>
			{
				EditorUtility.DisplayDialog("Asset locking failed", "Asset locking failed\n" + errorMessage, "OK");
			});
		}

		private static void TryRevertAssets(UnityEngine.Object[] assets)
		{
			Locker.TryRevertAssetLocks(assets, null, (errorMessage) =>
			{
				EditorUtility.DisplayDialog("Asset reverting failed", "Asset reverting failed\n" + errorMessage, "OK");
			});
		}

		private static void TryFinishLockingAssets(UnityEngine.Object[] assets)
		{
			Locker.TryFinishLockingAssets(assets, null, (errorMessage) =>
			{
				EditorUtility.DisplayDialog("Asset finishing failed", "Asset finishing failed\n" + errorMessage, "OK");
			});
		}
	}
}