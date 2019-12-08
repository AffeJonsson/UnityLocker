using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	public class HistoryWindow : EditorWindow
	{
		private Object m_asset;
		private AssetHistory.AssetHistoryData[] m_assetHistory;
		private bool m_isLoadingHistory;
		private Vector2 m_scrollPosition;
		private float m_maxLockerWidth;

		private const int IconSpacing = 2;

		public static void Show(Object asset)
		{
			var window = GetWindow<HistoryWindow>();
			window.titleContent = new GUIContent("Asset History");
			window.m_assetHistory = null;
			window.m_isLoadingHistory = true;
			window.m_asset = asset;
			window.Show();
			Locker.FetchAssetHistory(asset, (data) =>
			{
				window.m_assetHistory = data;
				window.m_isLoadingHistory = false;
				window.m_maxLockerWidth = 0f;
				var maxDateWidth = 0f;
				var iconWidth = Container.GetLockSettings().LockedByMeIconLarge.width;
				for (var i = 0; i < window.m_assetHistory.Length; i++)
				{
					var history = window.m_assetHistory[i];
					var lockerContent = new GUIContent(history.LockerName);
					var lockerWidth = EditorStyles.label.CalcSize(lockerContent).x;
					var dateContent = new GUIContent(history.Date.ToString());
					var dateWidth = EditorStyles.label.CalcSize(dateContent).x;
					if (window.m_maxLockerWidth < lockerWidth)
					{
						window.m_maxLockerWidth = lockerWidth;
					}
					if (maxDateWidth < dateWidth)
					{
						maxDateWidth = dateWidth;
					}
				}
				window.minSize = new Vector2(window.m_maxLockerWidth + maxDateWidth + iconWidth + IconSpacing + IconSpacing + 24, 54);
				window.maxSize = new Vector2(1000000000, 1000000000);
				window.Repaint();
			}, null);
		}

		private void OnEnable()
		{
			m_assetHistory = null;
			m_isLoadingHistory = false;
		}

		private void OnGUI()
		{
			if (m_assetHistory == null)
			{
				if (m_isLoadingHistory)
				{
					EditorGUILayout.LabelField("Loading...");
				}
				else
				{
					EditorGUILayout.LabelField("Select an asset to view history");
				}
				return;
			}
			using (new EditorGUI.DisabledGroupScope(true))
			{
				EditorGUILayout.ObjectField(m_asset, typeof(Object), false);
			}
			{
				var controlRect = EditorGUILayout.GetControlRect(false, 1);
				EditorGUI.DrawRect(controlRect, Color.gray);
			}
			using (var scroll = new EditorGUILayout.ScrollViewScope(m_scrollPosition))
			{
				DrawEntry(Container.GetLockSettings().LockIconLarge, "Date", "User");
				{
					var controlRect = EditorGUILayout.GetControlRect(false, 1);
					EditorGUI.DrawRect(controlRect, Color.gray);
				}
				for (var i = 0; i < m_assetHistory.Length; i++)
				{
					var history = m_assetHistory[i];
					var icon = history.Locked ? Container.GetLockSettings().LockIconLarge : (string.IsNullOrEmpty(history.UnlockSha) ? Container.GetLockSettings().LockedByMeIconLarge : Container.GetLockSettings().LockedNowButUnlockedLaterIconLarge);
					DrawEntry(icon, history.Date.ToString(), history.LockerName);
				}
				m_scrollPosition = scroll.scrollPosition;
			}
		}

		private void DrawEntry(Texture2D icon, string date, string locker)
		{
			var rect = EditorGUILayout.GetControlRect();
			var controlRect = rect;
			var dateContent = new GUIContent(date);
			var dateStyle = new GUIStyle
			{
				alignment = TextAnchor.MiddleCenter
			};
			var lockerContent = new GUIContent(locker);
			var lockerStyle = new GUIStyle
			{
				alignment = TextAnchor.MiddleCenter
			};
			rect.width = icon.width;
			EditorGUI.LabelField(rect, new GUIContent(icon));
			rect.x += rect.width + IconSpacing;
			rect.width = controlRect.width - rect.width - IconSpacing - m_maxLockerWidth - IconSpacing;
			EditorGUI.LabelField(rect, dateContent, dateStyle);
			rect.x = controlRect.x + controlRect.width - m_maxLockerWidth;
			rect.width = m_maxLockerWidth;
			EditorGUI.LabelField(rect, lockerContent, lockerStyle);
		}
	}
}