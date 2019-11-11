using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Alf.UnityLocker.Editor
{
	[InitializeOnLoad]
	public static class LockDrawer
	{
		private static int sm_currentSceneIndex;
		private static MethodInfo sm_getSceneMethod;

		static LockDrawer()
		{
			sm_getSceneMethod = typeof(EditorSceneManager).GetMethod("GetSceneByHandle", BindingFlags.Static | BindingFlags.NonPublic);
			EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
			EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;

			// finishedDefaultHeaderGUI was added in 2018.2
#if UNITY_2018_2_OR_NEWER
			UnityEditor.Editor.finishedDefaultHeaderGUI += OnFinishedHeaderGUI;
#endif
		}

#if UNITY_2018_2_OR_NEWER
		private static void OnFinishedHeaderGUI(UnityEditor.Editor editor)
		{
			if (!Locker.HasFetched)
			{
				return;
			}

			if (Locker.IsAssetLocked(Selection.activeObject))
			{
				var locker = Locker.GetAssetLocker(Selection.activeObject);
				if (locker != null)
				{
					TryDrawLock(new Rect(7, 7, 21, 21), Selection.activeObject, true);
					EditorGUILayout.LabelField("Asset locked by " + locker, EditorStyles.boldLabel);
					var isUnlockedAtLaterCommit = Locker.IsAssetUnlockedAtLaterCommit(Selection.activeObject);
					if (isUnlockedAtLaterCommit)
					{
						var sha = Locker.GetAssetUnlockCommitSha(Selection.activeObject);
						if (!string.IsNullOrEmpty(sha))
						{
							EditorGUILayout.LabelField("(Unlocked at commit " + sha.Substring(0, 8) + ")");
						}
					}
				}
			}
		}
#endif

		private static void OnHierarchyWindowItemOnGUI(int instanceId, Rect selectionRect)
		{
			if (!Locker.HasFetched)
			{
				return;
			}
			var asset = EditorUtility.InstanceIDToObject(instanceId) as GameObject;

			if (asset == null)
			{
				var scene = (Scene)sm_getSceneMethod.Invoke(null, new object[] { instanceId });
				var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
				TryDrawLock(selectionRect, sceneAsset, false);
			}
			else
			{
				TryDrawLock(selectionRect, asset, false);
			}
		}

		private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
		{
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
			TryDrawLock(selectionRect, asset, false);
		}

		private static void TryDrawLock(Rect rect, Object asset, bool largeTexture)
		{
			if (Locker.IsAssetLockedByMe(asset))
			{
				GUI.Label(rect, largeTexture ? Container.GetLockSettings().LockedByMeIconLarge : Container.GetLockSettings().LockedByMeIcon);
			}
			else if (Locker.IsAssetLockedBySomeoneElse(asset))
			{
				var sha = Locker.GetAssetUnlockCommitSha(asset);
				if (!string.IsNullOrEmpty(sha))
				{
					GUI.Label(rect, largeTexture ? Container.GetLockSettings().LockedNowButUnlockedLaterIconLarge : Container.GetLockSettings().LockedNowButUnlockedLaterIcon);
				}
				else
				{
					GUI.Label(rect, largeTexture ? Container.GetLockSettings().LockIconLarge : Container.GetLockSettings().LockIcon);
				}
			}
		}
	}
}
