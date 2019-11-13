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
			if (!Container.GetLockSettings().IsEnabled)
			{
				return;
			}
			if (!Locker.HasFetched)
			{
				return;
			}

			if (Locker.IsAssetLockedByMe(Selection.activeObject) || Locker.IsAssetLockedBySomeoneElse(Selection.activeObject) || Locker.IsAssetLockedNowButUnlockedAtLaterCommit(Selection.activeObject))
			{
				var locker = Locker.GetAssetLocker(Selection.activeObject);
				if (!string.IsNullOrEmpty(locker))
				{
					LockDrawer.TryDrawLock(sm_headerRect, Selection.activeObject, LockDrawer.DrawType.LargeIcon);
					EditorGUILayout.LabelField("Asset locked by " + locker, EditorStyles.boldLabel);
					var isUnlockedAtLaterCommit = Locker.IsAssetLockedNowButUnlockedAtLaterCommit(Selection.activeObject);
					if (isUnlockedAtLaterCommit)
					{
						var sha = Locker.GetAssetUnlockCommitShaShort(Selection.activeObject);
						if (!string.IsNullOrEmpty(sha))
						{
							EditorGUILayout.LabelField("(Unlocked at commit " + sha + ")");
						}
					}
				}
			}
		}

	}
}
#endif