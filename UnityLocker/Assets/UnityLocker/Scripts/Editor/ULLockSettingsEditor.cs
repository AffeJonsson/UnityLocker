using System;
using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	[CustomEditor(typeof(ULLockSettings))]
	public class ULLockSettingsEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			if (GUILayout.Button("Fetch"))
			{
				ULLocker.FetchLockedAssets(null);
			}
		}
	}
}