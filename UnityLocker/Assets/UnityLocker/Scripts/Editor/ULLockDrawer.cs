using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

namespace Alf.UnityLocker.Editor
{
	[InitializeOnLoad]
	public static class ULLockDrawer
	{
		private static int sm_currentSceneIndex;

		static ULLockDrawer()
		{
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
			if (!ULLocker.HasFetched)
			{
				return;
			}

			if (editor.serializedObject.targetObject is SceneAsset || PrefabUtility.GetPrefabType(editor.serializedObject.targetObject) != PrefabType.None)
			{
				if (ULLocker.IsAssetLocked(editor.serializedObject.targetObject))
				{
					var locker = ULLocker.GetAssetLocker(editor.serializedObject.targetObject);
					if (locker != null)
					{
						TryDrawLock(new Rect(9, 9, 14, 14), editor.serializedObject.targetObject);
						EditorGUILayout.LabelField("Asset locked by " + locker.Name, EditorStyles.boldLabel);
						var sha = ULLocker.GetAssetUnlockCommitSha(editor.serializedObject.targetObject);
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
			if (!ULLocker.HasFetched)
			{
				return;
			}

			HierarchyProperty.FilterSingleSceneObject(instanceId, false);
			var asset = EditorUtility.InstanceIDToObject(instanceId);
			if (asset == null)
			{
				var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(sm_currentSceneIndex++);
				if (sm_currentSceneIndex >= UnityEngine.SceneManagement.SceneManager.sceneCount)
				{
					sm_currentSceneIndex = 0;
				}
				asset = AssetDatabase.LoadAssetAtPath<Object>(scene.path);
			}
			TryDrawLock(selectionRect, asset);
		}

		private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
		{
			if (!ULLocker.HasFetched)
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
			if (ULLocker.IsAssetLockedByMe(asset))
			{
				GUI.Label(rect, ULLockSettingsHelper.Settings.LockedByMeIcon);
			}
			else if (ULLocker.IsAssetLockedBySomeoneElse(asset))
			{
				GUI.Label(rect, ULLockSettingsHelper.Settings.LockIcon);
			}
		}
	}
}