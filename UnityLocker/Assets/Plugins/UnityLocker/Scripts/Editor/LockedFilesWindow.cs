using System;
using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	public class LockedFilesWindow : EditorWindow
	{
		[MenuItem("Window/Locked Files")]
		public static void ShowWindow()
		{
			GetWindow<LockedFilesWindow>().Show();
		}

		private void OnGUI()
		{
			var lockedAssets = Locker.GetAssetsLockedByMe();
			var rect = EditorGUILayout.GetControlRect();
			GUI.enabled = false;
			foreach (var lockedAsset in lockedAssets)
			{
				EditorGUI.ObjectField(rect, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(lockedAsset.Guid)), typeof(UnityEngine.Object), false);
			}
			GUI.enabled = true;
		}
	}
}