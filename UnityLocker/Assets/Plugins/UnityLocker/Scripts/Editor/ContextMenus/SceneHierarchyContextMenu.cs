using System;
using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor.ContextMenus
{
	public static class SceneHierarchyContextMenu
	{
		public static void OnAddSceneMenuItem(GenericMenu menu, UnityEngine.SceneManagement.Scene scene)
		{
			AddLockItems(menu, AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path));
		}

		public static void OnAddGameObjectMenuItem(GenericMenu menu, GameObject gameObject)
		{
			AddLockItems(menu, gameObject);
		}

		private static void AddLockItems(GenericMenu menu, UnityEngine.Object asset)
		{
			menu.AddSeparator("");
			AddSingleMenuItem(menu, asset, Locker.GetIsLockValid, new GUIContent(Constants.LockName), () => TryLockAssets(new UnityEngine.Object[] { asset }));
			AddSingleMenuItem(menu, asset, Locker.GetIsRevertLockValid, new GUIContent(Constants.RevertName), () => TryRevertAssets(new UnityEngine.Object[] { asset }));
			AddSingleMenuItem(menu, asset, Locker.GetIsFinishLockValid, new GUIContent(Constants.FinishName), () => TryFinishLockingAssets(new UnityEngine.Object[] { asset }));
			AddSingleMenuItem(menu, asset, Locker.IsAssetTypeValid, new GUIContent(Constants.HistoryName), () => HistoryWindow.Show(asset));
		}

		private static void AddSingleMenuItem(GenericMenu menu, UnityEngine.Object asset, Func<UnityEngine.Object, bool> validationMethod, GUIContent guiContent, GenericMenu.MenuFunction onClick)
		{
			if (validationMethod(asset))
			{
				menu.AddItem(guiContent, false, onClick);
			}
			else
			{
				menu.AddDisabledItem(guiContent);
			}
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