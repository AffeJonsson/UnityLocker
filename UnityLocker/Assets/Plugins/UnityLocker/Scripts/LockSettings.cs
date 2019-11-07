using UnityEngine;

namespace Alf.UnityLocker
{
	public sealed class LockSettings : ScriptableObject
	{
		[SerializeField]
		private Texture2D m_lockIcon;
		[SerializeField]
		private Texture2D m_lockedByMeIcon;
		[SerializeField]
		private Texture2D m_lockedNowButUnlockedLaterIcon;
		[SerializeField]
		private string m_baseUrl;
		[SerializeField]
		private string m_versionControlName;
		[SerializeField]
		private string m_baseFolderAdditionalPath;
		[SerializeField]
		private int m_parentFolderCount;
		[SerializeField]
		private int m_assetTypeValidators;

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

		public string UnlockAssetsUrl
		{
			get { return m_baseUrl + "/unlock-assets-at-commit"; }
		}

		public string Username
		{
			get { return System.Environment.UserName; }
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
			get
			{
				return VersionControlName != null && m_baseUrl != null;
			}
		}

		public int AssetTypeValidators
		{
			get { return m_assetTypeValidators; }
		}
	}
}