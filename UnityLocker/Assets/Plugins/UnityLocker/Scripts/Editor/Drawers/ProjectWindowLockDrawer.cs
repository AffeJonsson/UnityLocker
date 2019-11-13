using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor.Drawers
{
	[InitializeOnLoad]
	public static class ProjectWindowLockDrawer
	{
		static ProjectWindowLockDrawer()
		{
			EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
		}

		private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
		{
			if (!Container.GetLockSettings().IsEnabled)
			{
				return;
			}
			if (!Locker.HasFetched)
			{
				return;
			}

			var assetPath = AssetDatabase.GUIDToAssetPath(guid);
			if (string.IsNullOrEmpty(assetPath))
			{
				return;
			}
			var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
			LockDrawer.TryDrawLock(selectionRect, asset, LockDrawer.DrawType.SmallIcon);
		}
	}
}