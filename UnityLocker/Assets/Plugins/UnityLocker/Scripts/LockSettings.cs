using UnityEngine;

namespace Alf.UnityLocker
{
	public sealed class LockSettings : ScriptableObject
	{
		[SerializeField]
		private Texture2D m_lockIcon = null;
		[SerializeField]
		private Texture2D m_lockedByMeIcon = null;
		[SerializeField]
		private Texture2D m_lockedNowButUnlockedLaterIcon = null;
		[SerializeField]
		private Texture2D m_lockIconLarge = null;
		[SerializeField]
		private Texture2D m_lockedByMeIconLarge = null;
		[SerializeField]
		private Texture2D m_lockedNowButUnlockedLaterIconLarge = null;
		[SerializeField]
		private string m_baseUrl = null;
		[SerializeField]
		private string m_versionControlName = null;
		[SerializeField]
		private string m_baseFolderAdditionalPath = null;
		[SerializeField]
#pragma warning disable CS0414
		// This field is used in LockSettingsEditor
		private int m_parentFolderCount = 0;
#pragma warning restore CS0414
		[SerializeField]
		private int m_assetTypeValidators = 0;
		[SerializeField]
		private bool m_isEnabled = true;

		public string GetLockedAssetsUrl
		{
			get { return m_baseUrl + "/get-locked-assets"; }
		}

		public string LockAssetsUrl
		{
			get { return m_baseUrl + "/lock-assets"; }
		}

		public string RevertAssetsLockUrl
		{
			get { return m_baseUrl + "/unlock-assets"; }
		}

		public string FinishLockingAssetsUrl
		{
			get { return m_baseUrl + "/unlock-assets-at-commit"; }
		}

		public string ClearLocksUrl
		{
			get { return m_baseUrl + "/clear-locks"; }
		}

		public string GetAssetHistoryUrl
		{
			get { return m_baseUrl + "/get-history"; }
		}

		public string Username
		{
#if UNITY_EDITOR
			get { return UnityEditor.EditorPrefs.GetString("LockerName", System.Environment.UserName); }
#else
			get { return string.Empty; }
#endif
		}

		public Texture2D LockIcon
		{
			get { return m_lockIcon; }
		}

		public Texture2D LockedByMeIcon
		{
			get { return m_lockedByMeIcon; }
		}

		public Texture2D LockedNowButUnlockedLaterIcon
		{
			get { return m_lockedNowButUnlockedLaterIcon; }
		}

		public Texture2D LockIconLarge
		{
			get { return m_lockIconLarge; }
		}

		public Texture2D LockedByMeIconLarge
		{
			get { return m_lockedByMeIconLarge; }
		}

		public Texture2D LockedNowButUnlockedLaterIconLarge
		{
			get { return m_lockedNowButUnlockedLaterIconLarge; }
		}

		public string RepoPath
		{
			get { return System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, m_baseFolderAdditionalPath)).TrimEnd('\\'); }
		}

		public string VersionControlName
		{
			get { return m_versionControlName; }
		}

		public bool IsSetUp
		{
			get { return VersionControlName != null && m_baseUrl != null; }
		}

		public int AssetTypeValidators
		{
			get { return m_assetTypeValidators; }
		}

		public bool IsEnabled
		{
			get { return m_isEnabled; }
		}
	}
}