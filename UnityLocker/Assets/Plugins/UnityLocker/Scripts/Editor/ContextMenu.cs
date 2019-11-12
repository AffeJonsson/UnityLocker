using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Alf.UnityLocker.Editor
{
	[InitializeOnLoad]
	public static class ContextMenu
	{
		private const string LockMenuName = "Assets/Lock";
		private const string RevertMenuName = "Assets/Revert Lock";
		private const string FinishLockMenuName = "Assets/Finish Lock";
		private const string OpenSettingsFileMenuName = "Tools/Open Locker Settings File";
		private const int Priority = 600;

#if !UNITY_2019_1_OR_NEWER
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

					UnityEngine.Event.current.Use();
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
#endif
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

		static ContextMenu()
		{
#if UNITY_2019_1_OR_NEWER
			SceneHierarchyHooks.addItemsToSceneHeaderContextMenu += ((menu, scene) => OnAddSceneMenuItem(menu, scene));
			SceneHierarchyHooks.addItemsToGameObjectContextMenu += ((menu, asset) => OnAddGameObjectMenuItem(menu, asset));
#elif UNITY_2017_1_OR_NEWER
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
#endif
		}

		[MenuItem(LockMenuName, priority = Priority)]
		public static void Lock()
		{
			var filtered = Selection.objects.Where(s =>
			{
				if (s == null)
				{
					return false;
				}
				var corr = PrefabUtility.GetCorrespondingObjectFromSource(s);
				return AssetDatabase.Contains(corr ?? s);
			}).ToArray();
			TryLockAssets(filtered);
		}

		[MenuItem(LockMenuName, true)]
		public static bool ValidateLock()
		{
			if (!Container.GetLockSettings().IsEnabled)
			{
				return false;
			}
			if (Selection.objects == null || Selection.objects.Length == 0)
			{
				return false;
			}
			var filtered = Selection.objects.Where(s =>
			{
				if (s == null)
				{
					return false;
				}
				var corr = PrefabUtility.GetCorrespondingObjectFromSource(s);
				return AssetDatabase.Contains(corr ?? s);
			});
			if (filtered.Count() == 0)
			{
				return false;
			}
			foreach (var o in filtered)
			{
				if (!GetIsLockValid(o))
				{
					return false;
				}
			}
			return true;
		}

		[MenuItem(RevertMenuName, priority = Priority + 1)]
		public static void RevertLock()
		{
			var filtered = Selection.objects.Where(s =>
			{
				if (s == null)
				{
					return false;
				}
				var corr = PrefabUtility.GetCorrespondingObjectFromSource(s);
				return AssetDatabase.Contains(corr ?? s);
			}).ToArray();
			TryRevertAssets(filtered);
		}

		[MenuItem(RevertMenuName, true)]
		public static bool ValidateRevertLock()
		{
			if (!Container.GetLockSettings().IsEnabled)
			{
				return false;
			}
			if (Selection.objects == null || Selection.objects.Length == 0)
			{
				return false;
			}
			var filtered = Selection.objects.Where(s =>
			{
				if (s == null)
				{
					return false;
				}
				var corr = PrefabUtility.GetCorrespondingObjectFromSource(s);
				return AssetDatabase.Contains(corr ?? s);
			});
			if (filtered.Count() == 0)
			{
				return false;
			}
			foreach (var o in filtered)
			{
				if (!GetIsRevertLockValid(o))
				{
					return false;
				}
			}
			return true;
		}

		[MenuItem(FinishLockMenuName, priority = Priority + 2)]
		public static void FinishLock()
		{
			var filtered = Selection.objects.Where(s =>
			{
				if (s == null)
				{
					return false;
				}
				var corr = PrefabUtility.GetCorrespondingObjectFromSource(s);
				return AssetDatabase.Contains(corr ?? s);
			}).ToArray();
			TryFinishLockingAssets(filtered);
		}

		[MenuItem(FinishLockMenuName, true)]
		public static bool ValidateFinishLock()
		{
			if (!Container.GetLockSettings().IsEnabled)
			{
				return false;
			}
			if (Selection.objects == null || Selection.objects.Length == 0)
			{
				return false;
			}
			var filtered = Selection.objects.Where(s =>
			{
				if (s == null)
				{
					return false;
				}
				var corr = PrefabUtility.GetCorrespondingObjectFromSource(s);
				return AssetDatabase.Contains(corr ?? s);
			});
			if (filtered.Count() == 0)
			{
				return false;
			}
			foreach (var o in filtered)
			{
				if (!GetIsFinishLockValid(o))
				{
					return false;
				}
			}
			return true;
		}

		[MenuItem(OpenSettingsFileMenuName, priority = 10000)]
		public static void OpenSettingsFile()
		{
			Selection.activeObject = Container.GetLockSettings();
		}

		private static bool GetIsLockValid(UnityEngine.Object obj)
		{
			if (!Container.GetLockSettings().IsEnabled)
			{
				return false;
			}
#if UNITY_2018_3_OR_NEWER
			// Only allow locking the nearest prefab, and not just child objects
			if (PrefabUtility.GetNearestPrefabInstanceRoot(obj) != obj)
			{
				return false;
			}
#endif
			obj = PrefabUtility.GetCorrespondingObjectFromSource(obj) ?? obj;
			return obj != null && !Locker.IsAssetLockedByMe(obj) && !Locker.IsAssetLockedBySomeoneElse(obj) && !Locker.IsAssetLockedNowButUnlockedAtLaterCommit(obj) && Container.GetAssetTypeValidators().IsAssetValid(obj);
		}

		private static bool GetIsRevertLockValid(UnityEngine.Object obj)
		{
			if (!Container.GetLockSettings().IsEnabled)
			{
				return false;
			}
#if UNITY_2018_3_OR_NEWER
			// Only allow locking the nearest prefab, and not just child objects
			if (PrefabUtility.GetNearestPrefabInstanceRoot(obj) != obj)
			{
				return false;
			}
#endif
			obj = PrefabUtility.GetCorrespondingObjectFromSource(obj) ?? obj;
			return obj != null && Locker.IsAssetLockedByMe(obj);
		}

		private static bool GetIsFinishLockValid(UnityEngine.Object obj)
		{
			if (!Container.GetLockSettings().IsEnabled)
			{
				return false;
			}
#if UNITY_2018_3_OR_NEWER
			// Only allow locking the nearest prefab, and not just child objects
			if (PrefabUtility.GetNearestPrefabInstanceRoot(obj) != obj)
			{
				return false;
			}
#endif
			obj = PrefabUtility.GetCorrespondingObjectFromSource(obj) ?? obj;
			return obj != null && Locker.IsAssetLockedByMe(obj);
		}

		private static void OnAddSceneMenuItem(GenericMenu menu, UnityEngine.SceneManagement.Scene scene)
		{
			AddLockItems(menu, AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path));
		}

		private static void OnAddGameObjectMenuItem(GenericMenu menu, UnityEngine.GameObject gameObject)
		{
			AddLockItems(menu, gameObject);
		}

		private static void AddLockItems(GenericMenu menu, UnityEngine.Object asset)
		{
			menu.AddSeparator("");
			AddSingleMenuItem(menu, asset, GetIsLockValid, new UnityEngine.GUIContent("Lock"), () => TryLockAssets(new UnityEngine.Object[] { PrefabUtility.GetCorrespondingObjectFromSource(asset) ?? asset }));
			AddSingleMenuItem(menu, asset, GetIsRevertLockValid, new UnityEngine.GUIContent("Revert Lock"), () => TryRevertAssets(new UnityEngine.Object[] { PrefabUtility.GetCorrespondingObjectFromSource(asset) ?? asset }));
			AddSingleMenuItem(menu, asset, GetIsFinishLockValid, new UnityEngine.GUIContent("Finish Lock"), () => TryFinishLockingAssets(new UnityEngine.Object[] { PrefabUtility.GetCorrespondingObjectFromSource(asset) ?? asset }));
		}

		private static void AddSingleMenuItem(GenericMenu menu, UnityEngine.Object asset, Func<UnityEngine.Object, bool> validationMethod, UnityEngine.GUIContent guiContent, GenericMenu.MenuFunction onClick)
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