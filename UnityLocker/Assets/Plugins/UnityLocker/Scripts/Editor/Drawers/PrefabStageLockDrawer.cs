#if UNITY_2018_3_OR_NEWER
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

namespace Alf.UnityLocker.Editor.Drawers
{
	//[InitializeOnLoad]
	//public static class PrefabStageLockDrawer
	//{
	//	private static PrefabStage sm_currentStage;

	//	static PrefabStageLockDrawer()
	//	{
	//		EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
	//		PrefabStage.prefabStageOpened += OnPrefabStageOpened;
	//		PrefabStage.prefabStageClosing += OnPrefabStageClosing;
	//		sm_currentStage = PrefabStageUtility.GetCurrentPrefabStage();
	//	}

	//	private static void OnPrefabStageOpened(PrefabStage stage)
	//	{
	//		sm_currentStage = stage;
	//	}

	//	private static void OnPrefabStageClosing(PrefabStage stage)
	//	{
	//		sm_currentStage = null;
	//	}

	//	private static void OnHierarchyWindowItemOnGUI(int instanceId, Rect selectionRect)
	//	{
	//		if (!Container.GetLockSettings().IsEnabled)
	//		{
	//			return;
	//		}
	//		if (!Locker.HasFetched)
	//		{
	//			return;
	//		}
	//		if (sm_currentStage == null)
	//		{
	//			return;
	//		}
	//		var extra = selectionRect.x;
	//		selectionRect.width += extra * 2;
	//		selectionRect.x = 0;
	//		var asset = EditorUtility.InstanceIDToObject(instanceId);

	//		if (asset != null)
	//		{
	//			if (sm_currentStage != null && asset == sm_currentStage.prefabContentsRoot)
	//			{
	//				var prefab = AssetDatabase.LoadAssetAtPath<Object>(sm_currentStage.prefabAssetPath);
	//				LockDrawer.TryDrawLock(selectionRect, prefab, LockDrawer.DrawType.Background);
	//			}
	//			else
	//			{
	//				var corr = PrefabUtility.GetCorrespondingObjectFromSource(asset);
	//				LockDrawer.TryDrawLock(selectionRect, corr ?? asset, LockDrawer.DrawType.Background);
	//			}
	//		}
	//	}
	//}
}
#endif