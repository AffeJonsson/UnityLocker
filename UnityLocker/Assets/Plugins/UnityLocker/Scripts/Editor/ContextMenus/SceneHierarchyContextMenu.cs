using System;
using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor.ContextMenus
{
	[InitializeOnLoad]
	public static class SceneHierarchyContextMenu
	{
#if UNITY_2018_3_OR_NEWER
		private static Type sm_sceneHierarchyType = null;
#endif
		private static Type sm_treeViewType = null;
		private static Type sm_sceneHierarchyWindowType = null;

		private static void ContinusTryCreateMenu()
		{
			try
			{
				var sceneHierarchyWindow = EditorWindow.GetWindow(sm_sceneHierarchyWindowType);
				if (sceneHierarchyWindow == null)
				{
					return;
				}
				var getSceneByHandleMethod = typeof(UnityEditor.SceneManagement.EditorSceneManager).GetMethod("GetSceneByHandle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

#if UNITY_2018_3_OR_NEWER
				var sceneHierarchyField = sm_sceneHierarchyWindowType.GetField("m_SceneHierarchy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var sceneHierarchy = sceneHierarchyField.GetValue(sceneHierarchyWindow);
				var treeViewField = sm_sceneHierarchyType.GetField("m_TreeView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var createMultiSceneHeaderContextClickMethod = sm_sceneHierarchyType.GetMethod("CreateMultiSceneHeaderContextClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var createGameObjectContextClickMethod = sm_sceneHierarchyType.GetMethod("CreateGameObjectContextClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var isSceneHeaderInHierarchyWindowMethod = sm_sceneHierarchyType.GetMethod("IsSceneHeaderInHierarchyWindow", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
				var treeView = treeViewField.GetValue(sceneHierarchy);
#else
				var treeViewField = sm_sceneHierarchyWindowType.GetField("m_TreeView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var createMultiSceneHeaderContextClickMethod = sm_sceneHierarchyWindowType.GetMethod("CreateMultiSceneHeaderContextClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var createGameObjectContextClickMethod = sm_sceneHierarchyWindowType.GetMethod("CreateGameObjectContextClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var isSceneHeaderInHierarchyWindowMethod = sm_sceneHierarchyWindowType.GetMethod("IsSceneHeaderInHierarchyWindow", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
				var treeView = treeViewField.GetValue(sceneHierarchyWindow);
#endif
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

#if UNITY_2018_3_OR_NEWER
						createMultiSceneHeaderContextClickMethod.Invoke(sceneHierarchy, new object[] { menu, id });
#else
						createMultiSceneHeaderContextClickMethod.Invoke(sceneHierarchyWindow, new object[] { menu, id });
#endif
						OnAddSceneMenuItem(menu, scene);
						menu.ShowAsContext();
					}
					else
					{
						var menu = new GenericMenu();
#if UNITY_2018_3_OR_NEWER
						createGameObjectContextClickMethod.Invoke(sceneHierarchy, new object[] { menu, id });
#else
						createGameObjectContextClickMethod.Invoke(sceneHierarchyWindow, new object[] { menu, id });
#endif
						OnAddGameObjectMenuItem(menu, (UnityEngine.GameObject)EditorUtility.InstanceIDToObject(id));
						menu.ShowAsContext();
					}
				};
				property.SetValue(treeView, e);
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError(e);
				return;
			}
			EditorApplication.update -= ContinusTryCreateMenu;
		}
#if UNITY_2018_3_OR_NEWER
		private static void OnPrefabStageOpened(UnityEditor.Experimental.SceneManagement.PrefabStage stage)
		{
			EditorApplication.update += ContinusTryCreateMenu;
		}

		private static void OnPrefabStageClosing(UnityEditor.Experimental.SceneManagement.PrefabStage stage)
		{
			EditorApplication.update += ContinusTryCreateMenu;
		}
#endif

		static SceneHierarchyContextMenu()
		{
#if UNITY_2018_3_OR_NEWER
			UnityEditor.Experimental.SceneManagement.PrefabStage.prefabStageOpened += OnPrefabStageOpened;
			UnityEditor.Experimental.SceneManagement.PrefabStage.prefabStageClosing += OnPrefabStageClosing;
#endif
			//Unity 2017.1 to 2018.4 doesnt have SceneHierarchyHooks, so therefore we do some reflection magic to add lock buttons to hierarchy scene context menu.
			//Unity 2017.1 to 2018.2 doesnt have SceneHierarchy, so we need to do different reflection magic depending on version
			var context = System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext();

			System.Threading.Tasks.Task.Run(() =>
			{
				var assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (var assembly in assemblies)
				{
					if (assembly.FullName.StartsWith("UnityEditor"))
					{
#if UNITY_2018_3_OR_NEWER
						sm_sceneHierarchyType = assembly.GetType("UnityEditor.SceneHierarchy");
#endif
						sm_treeViewType = assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewController");
						sm_sceneHierarchyWindowType = assembly.GetType("UnityEditor.SceneHierarchyWindow");
						break;
					}
				}
			}).ContinueWith((t) =>
			{
				EditorApplication.update += ContinusTryCreateMenu;
			}, context);
		}

		public static void OnAddSceneMenuItem(GenericMenu menu, UnityEngine.SceneManagement.Scene scene)
		{
			AddLockItems(menu, AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path));
		}

		public static void OnAddGameObjectMenuItem(GenericMenu menu, GameObject gameObject)
		{
			AddLockItems(menu, gameObject);
		}

		private static void AddLockItems(GenericMenu menu, UnityEngine.Object asset)
		{
			menu.AddSeparator("");
			AddSingleMenuItem(menu, asset, Locker.GetIsLockValid, new GUIContent(Constants.LockName), () => TryLockAssets(new UnityEngine.Object[] { asset }));
			AddSingleMenuItem(menu, asset, Locker.GetIsRevertLockValid, new GUIContent(Constants.RevertName), () => TryRevertAssets(new UnityEngine.Object[] { asset }));
			AddSingleMenuItem(menu, asset, Locker.GetIsFinishLockValid, new GUIContent(Constants.FinishName), () => TryFinishLockingAssets(new UnityEngine.Object[] { asset }));
			AddSingleMenuItem(menu, asset, Locker.IsAssetTypeValid, new GUIContent(Constants.HistoryName), () => HistoryWindow.Show(asset));
		}

		private static void AddSingleMenuItem(GenericMenu menu, UnityEngine.Object asset, Func<UnityEngine.Object, bool> validationMethod, GUIContent guiContent, GenericMenu.MenuFunction onClick)
		{
			if (validationMethod(asset))
			{
				menu.AddItem(guiContent, false, onClick);
			}
			else
			{
				menu.AddDisabledItem(guiContent);
			}
		}

		private static void TryLockAssets(UnityEngine.Object[] assets)
		{
			Locker.TryLockAssets(assets, null, (errorMessage) =>
			{
				EditorUtility.DisplayDialog("Asset locking failed", "Asset locking failed\n" + errorMessage, "OK");
			});
		}

		private static void TryRevertAssets(UnityEngine.Object[] assets)
		{
			Locker.TryRevertAssetLocks(assets, null, (errorMessage) =>
			{
				EditorUtility.DisplayDialog("Asset reverting failed", "Asset reverting failed\n" + errorMessage, "OK");
			});
		}

		private static void TryFinishLockingAssets(UnityEngine.Object[] assets)
		{
			Locker.TryFinishLockingAssets(assets, null, (errorMessage) =>
			{
				EditorUtility.DisplayDialog("Asset finishing failed", "Asset finishing failed\n" + errorMessage, "OK");
			});
		}
	}
}