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
		private const string UnlockMenuName = "Assets/Unlock";
		private const string OpenSettingsFileMenuName = "Tools/Open Locker Settings File";
		private const int Priority = 600;

		static ContextMenu()
		{
#if UNITY_2019_1_OR_NEWER
			SceneHierarchyHooks.addItemsToSceneHeaderContextMenu += ((menu, scene) => OnAddMenuItem(menu, scene));
#elif UNITY_2017_1_OR_NEWER
			//Unity 2017.1 to 2018.4 doesnt have SceneHierarchyHooks, so therefore we do some reflection magic to add lock buttons to hierarchy scene context menu.
			//Unity 2017.1 to 2018.2 doesnt have SceneHierarchy, so we need to do different reflection magic depending on version
			var context = System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext();
#if UNITY_2018_3_OR_NEWER
			Type sceneHierarchyType = null;
#endif
			Type treeViewType = null;
			Type sceneHierarchyWindowType = null;
			System.Threading.Tasks.Task.Run(() =>
			{
				var assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (var assembly in assemblies)
				{
					if (assembly.FullName.StartsWith("UnityEditor"))
					{
#if UNITY_2018_3_OR_NEWER
						sceneHierarchyType = assembly.GetType("UnityEditor.SceneHierarchy");
#endif
						treeViewType = assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewController");
						sceneHierarchyWindowType = assembly.GetType("UnityEditor.SceneHierarchyWindow");
						break;
					}
				}
			}).ContinueWith((t) =>
			{
				var sceneHierarchyWindow = EditorWindow.GetWindow(sceneHierarchyWindowType);
				var getSceneByHandleMethod = typeof(UnityEditor.SceneManagement.EditorSceneManager).GetMethod("GetSceneByHandle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

#if UNITY_2018_3_OR_NEWER
				var sceneHierarchyField = sceneHierarchyWindowType.GetField("m_SceneHierarchy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var sceneHierarchy = sceneHierarchyField.GetValue(sceneHierarchyWindow);
				var treeViewField = sceneHierarchyType.GetField("m_TreeView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var createMultiSceneHeaderContextClickMethod = sceneHierarchyType.GetMethod("CreateMultiSceneHeaderContextClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var isSceneHeaderInHierarchyWindowMethod = sceneHierarchyType.GetMethod("IsSceneHeaderInHierarchyWindow", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
				var treeView = treeViewField.GetValue(sceneHierarchy);
#else
				var treeViewField = sceneHierarchyWindowType.GetField("m_TreeView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var createMultiSceneHeaderContextClickMethod = sceneHierarchyWindowType.GetMethod("CreateMultiSceneHeaderContextClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var isSceneHeaderInHierarchyWindowMethod = sceneHierarchyWindowType.GetMethod("IsSceneHeaderInHierarchyWindow", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
				var treeView = treeViewField.GetValue(sceneHierarchyWindow);
#endif
				var property = treeViewType.GetProperty("contextClickItemCallback", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
				var e = (Action<int>)property.GetValue(treeView);
				e = (id) =>
				{
					var scene = (UnityEngine.SceneManagement.Scene)getSceneByHandleMethod.Invoke(null, new object[] { id });
					var clickedSceneHeader = (bool)isSceneHeaderInHierarchyWindowMethod.Invoke(null, new object[] { scene });

					if (clickedSceneHeader)
					{
						UnityEngine.Event.current.Use();
						var menu = new GenericMenu();

#if UNITY_2018_3_OR_NEWER
						createMultiSceneHeaderContextClickMethod.Invoke(sceneHierarchy, new object[] { menu, id });
#else
						createMultiSceneHeaderContextClickMethod.Invoke(sceneHierarchyWindow, new object[] { menu, id });
#endif
						OnAddMenuItem(menu, scene);
						menu.ShowAsContext();
					}
				};
				property.SetValue(treeView, e);
			}, context);
#endif
		}

		[MenuItem(LockMenuName, priority = Priority)]
		public static void Lock()
		{
			var filtered = Selection.objects.Where(s => AssetDatabase.Contains(s)).ToArray();
			TryLockAssets(filtered);
		}

		[MenuItem(LockMenuName, true)]
		public static bool ValidateLock()
		{
			if (Selection.objects == null || Selection.objects.Length == 0)
			{
				return false;
			}
			var filtered = Selection.objects.Where(s => s != null && AssetDatabase.Contains(s));
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
			var filtered = Selection.objects.Where(s => AssetDatabase.Contains(s)).ToArray();
			TryRevertAssets(filtered);
		}

		[MenuItem(RevertMenuName, true)]
		public static bool ValidateRevertLock()
		{
			if (Selection.objects == null || Selection.objects.Length == 0)
			{
				return false;
			}
			var filtered = Selection.objects.Where(s => s != null && AssetDatabase.Contains(s));
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

		[MenuItem(UnlockMenuName, priority = Priority + 2)]
		public static void Unlock()
		{
			var filtered = Selection.objects.Where(s => AssetDatabase.Contains(s)).ToArray();
			TryUnlockAssets(filtered);
		}

		[MenuItem(UnlockMenuName, true)]
		public static bool ValidateUnlock()
		{
			if (Selection.objects == null || Selection.objects.Length == 0)
			{
				return false;
			}
			var filtered = Selection.objects.Where(s => s != null && AssetDatabase.Contains(s));
			if (filtered.Count() == 0)
			{
				return false;
			}
			foreach (var o in filtered)
			{
				if (!GetIsUnlockValid(o))
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
			return obj != null && !Locker.IsAssetLocked(obj) && Container.GetAssetTypeValidators().IsAssetValid(obj);
		}

		private static bool GetIsRevertLockValid(UnityEngine.Object obj)
		{
			return obj != null && Locker.IsAssetLockedByMe(obj);
		}

		private static bool GetIsUnlockValid(UnityEngine.Object obj)
		{
			return obj != null && Locker.IsAssetLockedByMe(obj);
		}

		private static void OnAddMenuItem(GenericMenu menu, UnityEngine.SceneManagement.Scene scene)
		{
			AddLockItems(menu, AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path));
		}

		private static void AddLockItems(GenericMenu menu, SceneAsset sceneAsset)
		{
			menu.AddSeparator("");
			AddSingleMenuItem(menu, sceneAsset, GetIsLockValid, new UnityEngine.GUIContent("Lock"), () => TryLockAssets(new UnityEngine.Object[] { sceneAsset }));
			AddSingleMenuItem(menu, sceneAsset, GetIsRevertLockValid, new UnityEngine.GUIContent("Revert Lock"), () => TryRevertAssets(new UnityEngine.Object[] { sceneAsset }));
			AddSingleMenuItem(menu, sceneAsset, GetIsUnlockValid, new UnityEngine.GUIContent("Unlock"), () => TryUnlockAssets(new UnityEngine.Object[] { sceneAsset }));
		}

		private static void AddSingleMenuItem(GenericMenu menu, SceneAsset sceneAsset, Func<UnityEngine.Object, bool> validationMethod, UnityEngine.GUIContent guiContent, GenericMenu.MenuFunction onClick)
		{
			if (validationMethod(sceneAsset))
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

		private static void TryUnlockAssets(UnityEngine.Object[] assets)
		{
			Locker.TryUnlockAssets(assets, null, (errorMessage) =>
			{
				EditorUtility.DisplayDialog("Asset unlocking failed", "Asset unlocking failed\n" + errorMessage, "OK");
			});
		}
	}
}