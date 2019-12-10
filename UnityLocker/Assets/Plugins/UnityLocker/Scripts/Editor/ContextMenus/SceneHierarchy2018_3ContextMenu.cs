#if UNITY_2018_3_OR_NEWER && !UNITY_2019_1_OR_NEWER
using System;
using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor.ContextMenus
{
	[InitializeOnLoad]
	public static class SceneHierarchy2018_3ContextMenu
	{
		private static Type sm_sceneHierarchyType = null;
		private static Type sm_treeViewType = null;
		private static Type sm_sceneHierarchyWindowType = null;

		private static void ContinousTryCreateMenu()
		{
			try
			{
				var sceneHierarchyWindow = EditorWindow.GetWindow(sm_sceneHierarchyWindowType);
				var getSceneByHandleMethod = typeof(UnityEditor.SceneManagement.EditorSceneManager).GetMethod("GetSceneByHandle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

				var sceneHierarchyField = sm_sceneHierarchyWindowType.GetField("m_SceneHierarchy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var sceneHierarchy = sceneHierarchyField.GetValue(sceneHierarchyWindow);
				var treeViewField = sm_sceneHierarchyType.GetField("m_TreeView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var createMultiSceneHeaderContextClickMethod = sm_sceneHierarchyType.GetMethod("CreateMultiSceneHeaderContextClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var createGameObjectContextClickMethod = sm_sceneHierarchyType.GetMethod("CreateGameObjectContextClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var isSceneHeaderInHierarchyWindowMethod = sm_sceneHierarchyType.GetMethod("IsSceneHeaderInHierarchyWindow", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
				var treeView = treeViewField.GetValue(sceneHierarchy);

				var property = sm_treeViewType.GetProperty("contextClickItemCallback", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
				var e = (Action<int>)property.GetValue(treeView);
				e = (id) =>
				{
					var scene = (UnityEngine.SceneManagement.Scene)getSceneByHandleMethod.Invoke(null, new object[] { id });
					var clickedSceneHeader = (bool)isSceneHeaderInHierarchyWindowMethod.Invoke(null, new object[] { scene });

					Event.current.Use();
					if (clickedSceneHeader)
					{
						var menu = new GenericMenu();
						createMultiSceneHeaderContextClickMethod.Invoke(sceneHierarchy, new object[] { menu, id });
						SceneHierarchyContextMenu.OnAddSceneMenuItem(menu, scene);
						menu.ShowAsContext();
					}
					else
					{
						var menu = new GenericMenu();
						createGameObjectContextClickMethod.Invoke(sceneHierarchy, new object[] { menu, id });
						SceneHierarchyContextMenu.OnAddGameObjectMenuItem(menu, (GameObject)EditorUtility.InstanceIDToObject(id));
						menu.ShowAsContext();
					}
				};
				property.SetValue(treeView, e);
			}
			catch (Exception e)
			{
				Debug.LogError(e);
				return;
			}
			EditorApplication.update -= ContinousTryCreateMenu;
		}

		private static void OnPrefabStageOpened(UnityEditor.Experimental.SceneManagement.PrefabStage stage)
		{
			EditorApplication.update += ContinousTryCreateMenu;
		}

		private static void OnPrefabStageClosing(UnityEditor.Experimental.SceneManagement.PrefabStage stage)
		{
			EditorApplication.update += ContinousTryCreateMenu;
		}

		static SceneHierarchy2018_3ContextMenu()
		{
			UnityEditor.Experimental.SceneManagement.PrefabStage.prefabStageOpened += OnPrefabStageOpened;
			UnityEditor.Experimental.SceneManagement.PrefabStage.prefabStageClosing += OnPrefabStageClosing;
			var context = System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext();

			System.Threading.Tasks.Task.Run(() =>
			{
				var assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (var assembly in assemblies)
				{
					if (assembly.FullName.StartsWith("UnityEditor"))
					{
						sm_sceneHierarchyType = assembly.GetType("UnityEditor.SceneHierarchy");
						sm_treeViewType = assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewController");
						sm_sceneHierarchyWindowType = assembly.GetType("UnityEditor.SceneHierarchyWindow");
						break;
					}
				}
			}).ContinueWith((t) =>
			{
				EditorApplication.update += ContinousTryCreateMenu;
			}, context);
		}
	}
}
#endif