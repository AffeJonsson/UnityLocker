using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	public class LockedFilesWindow : EditorWindow
	{
		private List<Object> m_toggledAssets = new List<Object>(16);

		[MenuItem("Window/Locked Files")]
		public static void ShowWindow()
		{
			GetWindow<LockedFilesWindow>().Show();
		}

		private void OnEnable()
		{
			Locker.OnLockedAssetsChanged += OnLockedAssetsChanged;
			titleContent = new GUIContent("Locked Files");
		}

		private void OnDisable()
		{
			Locker.OnLockedAssetsChanged -= OnLockedAssetsChanged;
		}

		private void OnLockedAssetsChanged()
		{
			var newToggled = new List<Object>(8);
			foreach (var asset in Locker.GetAssetsLockedByMe())
			{
				for (var i = 0; i < m_toggledAssets.Count; i++)
				{
					if (AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(asset.Guid)) == m_toggledAssets[i])
					{
						newToggled.Add(m_toggledAssets[i]);
					}
				}
			}
			m_toggledAssets = newToggled;
			Repaint();
		}

		private void OnGUI()
		{
			if (m_toggledAssets.Count == 0)
			{
				GUI.enabled = false;
			}
			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Revert Lock"))
				{
					RevertLock();
				}
				if (GUILayout.Button("Finish Lock"))
				{
					FinishLock();
				}
			}
			GUI.enabled = true;

			EditorGUILayout.LabelField("Assets locked by you", EditorStyles.boldLabel);

			var lockedAssets = Locker.GetAssetsLockedByMe();
			foreach (var lockedAsset in lockedAssets)
			{
				var rect = EditorGUILayout.GetControlRect();
				var asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(lockedAsset.Guid));
				var currentlyToggled = m_toggledAssets.Contains(asset);
				var toggleRect = new Rect(rect.x, rect.y, rect.height, rect.height);
				var toggled = EditorGUI.Toggle(toggleRect, currentlyToggled);
				if (toggled && !currentlyToggled)
				{
					m_toggledAssets.Add(asset);
				}
				else if (!toggled && currentlyToggled)
				{
					m_toggledAssets.Remove(asset);
				}
				rect.x += toggleRect.width;
				rect.width -= toggleRect.width - 15f;
				GUI.enabled = false;
				EditorGUI.ObjectField(rect, asset, typeof(Object), false);
				GUI.enabled = true;
			}

			var lockedBySomeoneElse = Locker.GetAssetsLockedBySomeoneElse();
			var longestNameWidth = -1f;
			foreach (var lockedAsset in lockedBySomeoneElse)
			{
				var size = GUIStyle.none.CalcSize(new GUIContent(lockedAsset.LockerName));
				if (size.x > longestNameWidth)
				{
					longestNameWidth = size.x;
				}
			}
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Assets locked by someone else", EditorStyles.boldLabel);
			if (longestNameWidth >= 0)
			{
				foreach (var lockedAsset in lockedBySomeoneElse)
				{
					var rect = EditorGUILayout.GetControlRect();
					rect.width += 16;
					var texture = lockedAsset.Locked ? Container.GetLockSettings().LockIcon : Container.GetLockSettings().LockedNowButUnlockedLaterIcon;
					var originalY = rect.y;
					rect.y += (rect.height / 2 - texture.height / 2) / 2;
					GUI.Label(rect, texture);
					rect.y = originalY;
					rect.x += texture.width;
					rect.width -= texture.width;
					var nameRect = new Rect(rect.x, rect.y, longestNameWidth + 5, rect.height);
					rect.width -= nameRect.width;
					rect.x += nameRect.width;
					var asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(lockedAsset.Guid));
					GUI.enabled = false;
					EditorGUI.ObjectField(rect, asset, typeof(Object), false);
					GUI.enabled = true;
					var guiContent = lockedAsset.Locked ? new GUIContent(lockedAsset.LockerName) : new GUIContent(lockedAsset.LockerName, "Unlocked at commit " + lockedAsset.UnlockSha);
					EditorGUI.LabelField(nameRect, guiContent);
				}
			}
		}

		private void RevertLock()
		{
			Locker.TryRevertAssetLocks(m_toggledAssets.ToArray(), null, (errorMessage) =>
			{
				EditorUtility.DisplayDialog("Asset reverting failed", "Asset reverting failed\n" + errorMessage, "OK");
			});
		}

		private void FinishLock()
		{
			Locker.TryFinishLockingAssets(m_toggledAssets.ToArray(), null, (errorMessage) =>
			{
				EditorUtility.DisplayDialog("Asset finishing failed", "Asset finishing failed\n" + errorMessage, "OK");
			});
		}
	}
}