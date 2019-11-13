#if UNITY_2019_1_OR_NEWER
using UnityEditor;

namespace Alf.UnityLocker.Editor.ContextMenus
{
	[InitializeOnLoad]
	public class SceneHierarchy2019_1ContextMenu
	{
		static SceneHierarchy2019_1ContextMenu()
		{
			SceneHierarchyHooks.addItemsToSceneHeaderContextMenu += ((menu, scene) => SceneHierarchyContextMenu.OnAddSceneMenuItem(menu, scene));
			SceneHierarchyHooks.addItemsToGameObjectContextMenu += ((menu, asset) => SceneHierarchyContextMenu.OnAddGameObjectMenuItem(menu, asset));
		}
	}
}
#endif