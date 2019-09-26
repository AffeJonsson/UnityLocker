using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Alf.UnityLocker.Editor
{
	[InitializeOnLoad]
	public static class ULLockDrawer
	{
		private static Texture2D texture;

		static ULLockDrawer()
		{
			texture = Texture2D.whiteTexture;
			EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
		}

		private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
		{
			var assetPath = AssetDatabase.GUIDToAssetPath(guid);
			if (string.IsNullOrEmpty(assetPath))
			{
				return;
			}
			var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
			if (ULLocker.IsAssetLocked(asset))
			{
				var rect = new Rect(selectionRect);
				rect.height = rect.width = rect.width * 0.25f;
				GUI.Label(rect, texture);
			}
		}
	}
}