using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor.Drawers
{
	[InitializeOnLoad]
	public static class LockDrawer
	{
		public enum DrawType
		{
			SmallIcon = 1 << 0,
			LargeIcon = 1 << 1,
			Background = 1 << 2
		}

		public static void TryDrawLock(Rect rect, Object asset, DrawType drawType)
		{
			if (Locker.IsAssetLockedByMe(asset))
			{
				if ((drawType & DrawType.Background) != 0)
				{
					DrawBackground(rect, Color.green, 0.05f);
				}
				if ((drawType & DrawType.SmallIcon) != 0)
				{
					GUI.Label(rect, Container.GetLockSettings().LockedByMeIcon);
				}
				if ((drawType & DrawType.LargeIcon) != 0)
				{
					GUI.Label(rect, Container.GetLockSettings().LockedByMeIconLarge);
				}
			}
			else if (Locker.IsAssetLockedBySomeoneElse(asset))
			{
				if ((drawType & DrawType.Background) != 0)
				{
					DrawBackground(rect, Color.red, 0.05f);
				}
				if ((drawType & DrawType.SmallIcon) != 0)
				{
					GUI.Label(rect, Container.GetLockSettings().LockIcon);
				}
				if ((drawType & DrawType.LargeIcon) != 0)
				{
					GUI.Label(rect, Container.GetLockSettings().LockIconLarge);
				}
			}
			else if (Locker.IsAssetLockedNowButUnlockedAtLaterCommit(asset))
			{
				if ((drawType & DrawType.Background) != 0)
				{
					DrawBackground(rect, Color.yellow, 0.05f);
				}
				if ((drawType & DrawType.SmallIcon) != 0)
				{
					GUI.Label(rect, Container.GetLockSettings().LockedNowButUnlockedLaterIcon);
				}
				if ((drawType & DrawType.LargeIcon) != 0)
				{
					GUI.Label(rect, Container.GetLockSettings().LockedNowButUnlockedLaterIconLarge);
				}
			}
		}

		private static void DrawBackground(Rect rect, Color color, float alpha)
		{
			color.a = EditorGUIUtility.isProSkin ? alpha : alpha * 4f;
			EditorGUI.DrawRect(rect, color);
		}
	}
}