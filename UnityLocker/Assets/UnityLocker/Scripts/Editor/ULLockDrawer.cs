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
		private static Texture2D sm_texture;

		static ULLockDrawer()
		{
			sm_texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/UnityLocker/Assets/lock.png");
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
					GUI.Label(new Rect(9, 9, 14, 14), sm_texture);
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
				if (ULLocker.IsAssetLocked(asset))
				{
					GUI.Label(selectionRect, sm_texture);
				}
				return;
			}
			if (ULLocker.IsAssetLocked(asset))
			{
				GUI.Label(selectionRect, sm_texture);
			}
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
			if (ULLocker.IsAssetLocked(asset))
			{
				GUI.Label(selectionRect, sm_texture);
			}
		}
	}
}