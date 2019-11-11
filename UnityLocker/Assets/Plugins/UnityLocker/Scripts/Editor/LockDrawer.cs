using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
#if UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif
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
#if UNITY_2018_3_OR_NEWER
		private static PrefabStage sm_currentStage;
#endif
		private static readonly Rect sm_headerRect = new Rect(7, 7, 21, 21);

		static LockDrawer()
		{
			sm_getSceneMethod = typeof(EditorSceneManager).GetMethod("GetSceneByHandle", BindingFlags.Static | BindingFlags.NonPublic);
			EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
			EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
#if UNITY_2018_3_OR_NEWER
			PrefabStage.prefabStageOpened += OnPrefabStageOpened;
			PrefabStage.prefabStageClosing += OnPrefabStageClosing;
			sm_currentStage = PrefabStageUtility.GetCurrentPrefabStage();
#endif

#if UNITY_2018_2_OR_NEWER
			UnityEditor.Editor.finishedDefaultHeaderGUI += OnFinishedHeaderGUI;
#endif
		}

#if UNITY_2018_3_OR_NEWER
		private static void OnPrefabStageOpened(PrefabStage stage)
		{
			sm_currentStage = stage;
		}

		private static void OnPrefabStageClosing(PrefabStage stage)
		{
			sm_currentStage = null;
		}
#endif

#if UNITY_2018_2_OR_NEWER
		private static void OnFinishedHeaderGUI(UnityEditor.Editor editor)
		{
			if (!Locker.HasFetched)
			{
				return;
			}

			if (Locker.IsAssetLockedByMe(Selection.activeObject) || Locker.IsAssetLockedBySomeoneElse(Selection.activeObject) || Locker.IsAssetLockedNowButUnlockedAtLaterCommit(Selection.activeObject))
			{
				var locker = Locker.GetAssetLocker(Selection.activeObject);
				if (locker != null)
				{
					TryDrawLock(sm_headerRect, Selection.activeObject, true, false);
					EditorGUILayout.LabelField("Asset locked by " + locker, EditorStyles.boldLabel);
					var isUnlockedAtLaterCommit = Locker.IsAssetLockedNowButUnlockedAtLaterCommit(Selection.activeObject);
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
				TryDrawLock(selectionRect, sceneAsset, false, true);
			}
			else
			{
#if UNITY_2018_3_OR_NEWER
				if (sm_currentStage != null && asset == sm_currentStage.prefabContentsRoot)
				{
					var prefab = AssetDatabase.LoadAssetAtPath<Object>(sm_currentStage.prefabAssetPath);
					TryDrawLock(selectionRect, prefab, false, true);
				}
				else
#endif
				{
					TryDrawLock(selectionRect, asset, false, false);
				}
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
			TryDrawLock(selectionRect, asset, false, false);
		}

		private static void TryDrawLock(Rect rect, Object asset, bool largeTexture, bool drawBackground)
		{
			if (Locker.IsAssetLockedByMe(asset))
			{
				DrawBackground(rect, Color.green, 0.05f, drawBackground);
				GUI.Label(rect, largeTexture ? Container.GetLockSettings().LockedByMeIconLarge : Container.GetLockSettings().LockedByMeIcon);
			}
			else if (Locker.IsAssetLockedBySomeoneElse(asset))
			{
				DrawBackground(rect, Color.red, 0.05f, drawBackground);
				GUI.Label(rect, largeTexture ? Container.GetLockSettings().LockIconLarge : Container.GetLockSettings().LockIcon);
			}
			else if (Locker.IsAssetLockedNowButUnlockedAtLaterCommit(asset))
			{
				DrawBackground(rect, Color.yellow, 0.05f, drawBackground);
				GUI.Label(rect, largeTexture ? Container.GetLockSettings().LockedNowButUnlockedLaterIconLarge : Container.GetLockSettings().LockedNowButUnlockedLaterIcon);
			}
		}

		private static void DrawBackground(Rect rect, Color color, float alpha, bool drawBackground)
		{
			if (drawBackground)
			{
				color.a = EditorGUIUtility.isProSkin ? alpha : alpha * 2f;

				const float marginX = 16f;
				const float marginY = 1f;
				rect.x -= marginX;
				rect.y += marginY;
				rect.width += marginX * 2f;

				EditorGUI.DrawRect(rect, color);
			}
		}
	}
}