using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Alf.UnityLocker.Editor
{
	[InitializeOnLoad]
	public static class LockDrawer
	{
		private static int sm_currentSceneIndex;
		private static Dictionary<int, SceneAsset> sm_scenes;

		static LockDrawer()
		{
			sm_scenes = new Dictionary<int, SceneAsset>();
			BuildSceneMap();

			EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
			EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
			
			// finishedDefaultHeaderGUI was added in 2018.2
#if UNITY_2018_2_OR_NEWER
			UnityEditor.Editor.finishedDefaultHeaderGUI += OnFinishedHeaderGUI;
#endif
		}

		private static void BuildSceneMap()
		{
			sm_scenes.Clear();
			foreach (var scene in AssetDatabase.FindAssets("t:Scene"))
			{
				var scenePath = AssetDatabase.GUIDToAssetPath(scene);
				var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
				sm_scenes.Add(SceneManager.GetSceneByPath(scenePath).handle, sceneAsset);
			}
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
					TryDrawLock(new Rect(7, 7, 14, 14), Selection.activeObject);
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
				SceneAsset sceneAsset;
				if (!sm_scenes.TryGetValue(instanceId, out sceneAsset))
				{
					BuildSceneMap();
					sceneAsset = sm_scenes[instanceId];
				}
				TryDrawLock(selectionRect, sceneAsset);
			}
			else
			{
				TryDrawLock(selectionRect, asset);
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
			TryDrawLock(selectionRect, asset);
		}

		private static void TryDrawLock(Rect rect, Object asset)
		{
			if (Locker.IsAssetLockedByMe(asset))
			{
				GUI.Label(rect, Container.GetLockSettings().LockedByMeIcon);
			}
			else if (Locker.IsAssetLockedBySomeoneElse(asset))
			{
				var sha = Locker.GetAssetUnlockCommitSha(asset);
				if (!string.IsNullOrEmpty(sha))
				{
					GUI.Label(rect, Container.GetLockSettings().LockedNowButUnlockedLaterIcon);
				}
				else
				{
					GUI.Label(rect, Container.GetLockSettings().LockIcon);
				}
			}
		}
	}
}