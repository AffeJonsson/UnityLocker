using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Alf.UnityLocker.Editor.Drawers
{
	[InitializeOnLoad]
	public static class HierarchyWindowLockDrawer
	{
		private static MethodInfo sm_getSceneMethod;

		static HierarchyWindowLockDrawer()
		{
			sm_getSceneMethod = typeof(EditorSceneManager).GetMethod("GetSceneByHandle", BindingFlags.Static | BindingFlags.NonPublic);
			EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
		}

		private static void OnHierarchyWindowItemOnGUI(int instanceId, Rect selectionRect)
		{
			if (!Container.GetLockSettings().IsEnabled)
			{
				return;
			}
			if (!Locker.HasFetched)
			{
				return;
			}
			var extra = selectionRect.x;
			selectionRect.width += extra;
#if UNITY_2018_3_OR_NEWER
			selectionRect.width += extra;
#endif
			selectionRect.x = 0;
			var asset = EditorUtility.InstanceIDToObject(instanceId);

			if (asset == null)
			{
				var scene = (Scene)sm_getSceneMethod.Invoke(null, new object[] { instanceId });
				var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
				LockDrawer.TryDrawLock(selectionRect, sceneAsset, LockDrawer.DrawType.Background);
			}
			else
			{
				asset = Locker.FilterAsset(asset);
				LockDrawer.TryDrawLock(selectionRect, asset, LockDrawer.DrawType.Background);
			}
		}
	}
}