#if UNITY_2018_2_OR_NEWER
using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor.Drawers
{
	[InitializeOnLoad]
	public static class HeaderLockDrawer
	{
		private static readonly Rect sm_headerRect = new Rect(7, 7, 21, 21);

		static HeaderLockDrawer()
		{
			UnityEditor.Editor.finishedDefaultHeaderGUI += OnFinishedHeaderGUI;
		}

		private static void OnFinishedHeaderGUI(UnityEditor.Editor editor)
		{
			if (editor.GetType() == typeof(MaterialEditor))
			{
				return;
			}
			if (!Container.GetLockSettings().IsEnabled)
			{
				return;
			}
			if (!Locker.HasFetched)
			{
				return;
			}
			if (!Locker.AreAssetTypesValid(editor.targets))
			{
				return;
			}

			var isLockedByMe = Locker.AreAllAssetsLockedByMe(editor.targets);
			var isLockedBySomeoneElse = Locker.IsAnyAssetLockedBySomeoneElse(editor.targets);
			var isLockedNowButUnlockedAtLaterCommit = Locker.IsAnyAssetLockedNowButUnlockedAtLaterCommit(editor.targets);

			using (new GUILayout.HorizontalScope())
			{
				using (new EditorGUI.DisabledGroupScope(isLockedByMe || isLockedBySomeoneElse || isLockedNowButUnlockedAtLaterCommit))
				{
					if (GUILayout.Button(new GUIContent(Constants.LockName), EditorStyles.miniButton))
					{
						Locker.TryLockAssets(editor.targets, null, (errorMessage) =>
						{
							EditorUtility.DisplayDialog("Asset locking failed", "Asset locking failed\n" + errorMessage, "OK");
						});
					}
				}
				using (new EditorGUI.DisabledGroupScope(!isLockedByMe))
				{
					if (GUILayout.Button(new GUIContent(Constants.RevertName), EditorStyles.miniButton))
					{
						Locker.TryRevertAssetLocks(editor.targets, null, (errorMessage) =>
						{
							EditorUtility.DisplayDialog("Asset reverting failed", "Asset reverting failed\n" + errorMessage, "OK");
						});
					}
					if (GUILayout.Button(new GUIContent(Constants.FinishName), EditorStyles.miniButton))
					{
						Locker.TryFinishLockingAssets(editor.targets, null, (errorMessage) =>
						{
							EditorUtility.DisplayDialog("Asset finishing failed", "Asset finishing failed\n" + errorMessage, "OK");
						});
					}
				}
				if (GUILayout.Button(new GUIContent(Constants.HistoryName), EditorStyles.miniButton))
				{
					HistoryWindow.Show(editor.target);
				}
			}

			if (isLockedByMe || isLockedBySomeoneElse || isLockedNowButUnlockedAtLaterCommit)
			{
				var hasMultipleLockers = false;
				var locker = Locker.GetAssetLocker(editor.targets[0]);
				for (var i = 1; i < editor.targets.Length; i++)
				{
					if (locker != Locker.GetAssetLocker(editor.targets[1]))
					{
						hasMultipleLockers = true;
						break;
					}
				}
				if (!string.IsNullOrEmpty(locker))
				{
					LockDrawer.TryDrawLock(sm_headerRect, editor.target, LockDrawer.DrawType.LargeIcon);
					EditorGUILayout.LabelField("Asset" + (editor.targets.Length > 1 ? "s" : "") + " locked by " + (hasMultipleLockers ? "multiple users" : locker), EditorStyles.boldLabel);
					if (isLockedNowButUnlockedAtLaterCommit)
					{
						var hasMultipleUnlockShas = false;
						var sha = Locker.GetAssetUnlockCommitShaShort(editor.targets[0]);
						for (var i = 1; i < editor.targets.Length; i++)
						{
							if (sha != Locker.GetAssetUnlockCommitShaShort(editor.targets[1]))
							{
								hasMultipleUnlockShas = true;
								break;
							}
						}
						if (!string.IsNullOrEmpty(sha))
						{
							if (hasMultipleUnlockShas)
							{
								EditorGUILayout.LabelField("(Unlocked at multiple commits)");
							}
							else
							{
								EditorGUILayout.LabelField("(Unlocked at commit " + sha + ")");
							}
						}
					}
				}
			}
		}

	}
}
#endif