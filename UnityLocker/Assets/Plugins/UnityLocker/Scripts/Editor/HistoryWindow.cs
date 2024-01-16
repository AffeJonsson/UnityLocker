using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	public class HistoryWindow : EditorWindow
	{
		private Object m_asset;
		private AssetHistory.AssetHistoryData[] m_assetHistory;
		private bool m_isAssetValid;
		private bool m_isLoadingHistory;
		private Vector2 m_scrollPosition;
		private float m_maxLockerWidth;

		private const int IconSpacing = 2;
		private const string LockerHeader = "User";
		private const string DateHeader = "Date";
		private const string Title = "Asset History";

		[MenuItem("Window/Asset History")]
		public static void ShowWindow()
		{
			GetWindow<HistoryWindow>().Show();
		}

		public static void Show(Object asset)
		{
			asset = Locker.FilterAsset(asset);
			var window = GetWindow<HistoryWindow>();
			window.titleContent = new GUIContent(Title);
			window.m_assetHistory = null;
			window.m_isLoadingHistory = true;
			window.m_asset = asset;
			window.Show();
			Locker.FetchAssetHistory(asset, (data) =>
			{
				window.m_assetHistory = data;
				window.m_isLoadingHistory = false;
				window.m_maxLockerWidth = EditorStyles.label.CalcSize(new GUIContent(LockerHeader)).x;
				var maxDateWidth = EditorStyles.label.CalcSize(new GUIContent(DateHeader)).x;
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
			m_isAssetValid = false;
			if (m_asset != null)
			{
				m_isAssetValid = Locker.IsAssetTypeValid(m_asset);
				if (m_isAssetValid)
				{
					Show(m_asset);
				}
			}
		}

		private void OnGUI()
		{
			using (var scope = new EditorGUI.ChangeCheckScope())
			{
				m_asset = EditorGUILayout.ObjectField(m_asset, typeof(Object), true);
				if (scope.changed)
				{
					m_assetHistory = null;
					m_isAssetValid = Locker.IsAssetTypeValid(m_asset);
					if (m_isAssetValid)
					{
						Show(m_asset);
					}
				}
			}
			var currentEvent = Event.current;
			if (m_assetHistory == null)
			{
				if (m_isLoadingHistory)
				{
					EditorGUILayout.LabelField("Loading...");
				}
				else if (m_asset == null)
				{
					EditorGUILayout.LabelField("Select an asset to view history");
				}
				else if (!m_isAssetValid)
				{
					EditorGUILayout.LabelField("Asset is not a valid lockable asset");
				}
				return;
			}
			{
				var controlRect = EditorGUILayout.GetControlRect(false, 1);
				EditorGUI.DrawRect(controlRect, Color.gray);
			}
			using (var scroll = new EditorGUILayout.ScrollViewScope(m_scrollPosition))
			{
				var repaint = false;
				DrawSimpleEntry(Container.GetLockSettings().LockIconLarge, DateHeader, LockerHeader, "");
				var controlRect = EditorGUILayout.GetControlRect(false, 1);

				if (m_assetHistory.Length > 0)
				{
					var height = EditorGUIUtility.singleLineHeight * m_assetHistory.Length + EditorGUIUtility.standardVerticalSpacing * m_assetHistory.Length;
					GUI.Box(new Rect(controlRect.x, controlRect.y + EditorGUIUtility.standardVerticalSpacing, controlRect.width, height), "", GUI.skin.box);
					for (var i = 0; i < m_assetHistory.Length; i++)
					{
						var rect = EditorGUILayout.GetControlRect();
						var history = m_assetHistory[i];
						var hasUnlockSha = !string.IsNullOrEmpty(history.UnlockSha);
						var icon = history.Locked ? Container.GetLockSettings().LockIconLarge : (hasUnlockSha ? Container.GetLockSettings().LockedNowButUnlockedLaterIconLarge : Container.GetLockSettings().LockedByMeIconLarge);
						DrawSimpleEntry(rect, icon, history.Date.ToString(), history.LockerName, hasUnlockSha ? "Unlocked at commit " + history.UnlockSha + " (right click to copy)" : "");
						if (hasUnlockSha && currentEvent.isMouse && currentEvent.button == 1 && currentEvent.type == EventType.MouseDown && rect.Contains(currentEvent.mousePosition))
						{
							currentEvent.Use();
							EditorGUIUtility.systemCopyBuffer = history.UnlockSha;
						}
						if (i != m_assetHistory.Length - 1)
						{
							EditorGUI.DrawRect(new Rect(rect.x + 1, rect.y + rect.height + 1, rect.width - 2, 1), new Color(0.7f, 0.7f, 0.7f));
						}
					}
				}
				m_scrollPosition = scroll.scrollPosition;
				if (repaint)
				{
					Repaint();
				}
			}
		}

		private void DrawSimpleEntry(Texture2D icon, string date, string locker, string toolTip)
		{
			DrawSimpleEntry(EditorGUILayout.GetControlRect(), icon, date, locker, toolTip);
		}

		private void DrawSimpleEntry(Rect rect, Texture2D icon, string date, string locker, string toolTip)
		{
			rect.x += 1;
			rect.width -= 2;
			var controlRect = rect;
			var dateContent = new GUIContent(date);
			var dateStyle = new GUIStyle
			{
				alignment = TextAnchor.MiddleCenter
			};
			dateStyle.normal.textColor = Color.white;
			var lockerContent = new GUIContent(locker);
			var lockerStyle = new GUIStyle
			{
				alignment = TextAnchor.MiddleCenter
			};
			lockerStyle.normal.textColor = Color.white;
			rect.width = icon.width;
			rect.y += (rect.height - icon.height) / 2 + 1;
			EditorGUI.LabelField(rect, new GUIContent(icon));
			rect.y = controlRect.y;
			rect.x += rect.width + IconSpacing;
			rect.width = controlRect.width - rect.width - IconSpacing - m_maxLockerWidth - IconSpacing;
			EditorGUI.LabelField(rect, dateContent, dateStyle);
			rect.x = controlRect.x + controlRect.width - m_maxLockerWidth;
			rect.width = m_maxLockerWidth;
			EditorGUI.LabelField(rect, lockerContent, lockerStyle);
			if (!string.IsNullOrEmpty(toolTip))
			{
				EditorGUI.LabelField(controlRect, new GUIContent("", toolTip));
			}
		}
	}
}