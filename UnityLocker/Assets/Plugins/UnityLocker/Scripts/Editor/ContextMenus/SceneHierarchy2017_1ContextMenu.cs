#if UNITY_2017_1_OR_NEWER && !UNITY_2018_3_OR_NEWER
using System;
using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor.ContextMenus
{
	[InitializeOnLoad]
	public static class SceneHierarchy2017_1ContextMenu
	{
		private static Type sm_treeViewType = null;
		private static Type sm_sceneHierarchyWindowType = null;

		private static void ContinousTryCreateMenu()
		{
			try
			{
				var sceneHierarchyWindow = EditorWindow.GetWindow(sm_sceneHierarchyWindowType);
				var getSceneByHandleMethod = typeof(UnityEditor.SceneManagement.EditorSceneManager).GetMethod("GetSceneByHandle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

				var treeViewField = sm_sceneHierarchyWindowType.GetField("m_TreeView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var createMultiSceneHeaderContextClickMethod = sm_sceneHierarchyWindowType.GetMethod("CreateMultiSceneHeaderContextClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var createGameObjectContextClickMethod = sm_sceneHierarchyWindowType.GetMethod("CreateGameObjectContextClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var isSceneHeaderInHierarchyWindowMethod = sm_sceneHierarchyWindowType.GetMethod("IsSceneHeaderInHierarchyWindow", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
				var treeView = treeViewField.GetValue(sceneHierarchyWindow);
				if (treeView == null)
				{
					return;
				}
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
						createMultiSceneHeaderContextClickMethod.Invoke(sceneHierarchyWindow, new object[] { menu, id });
						SceneHierarchyContextMenu.OnAddSceneMenuItem(menu, scene);
						menu.ShowAsContext();
					}
					else
					{
						var menu = new GenericMenu();
						createGameObjectContextClickMethod.Invoke(sceneHierarchyWindow, new object[] { menu, id });
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

		static SceneHierarchy2017_1ContextMenu()
		{
			var context = System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext();

			System.Threading.Tasks.Task.Run(() =>
			{
				var assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (var assembly in assemblies)
				{
					if (assembly.FullName.StartsWith("UnityEditor"))
					{
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