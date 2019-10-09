using UnityEngine;

namespace Alf.UnityLocker
{
	[CreateAssetMenu(fileName = "ULLockSettings")]
	public sealed class ULLockSettings : ScriptableObject
	{
		[SerializeField]
		private Texture2D m_lockIcon;
		[SerializeField]
		private Texture2D m_lockedByMeIcon;
		[SerializeField]
		private string m_username;
		[SerializeField]
		private string m_baseUrl;

		public string GetLockedAssetsUrl
		{
			get { return m_baseUrl + "/get-locked-assets"; }
		}

		public string LockAssetUrl
		{
			get { return m_baseUrl + "/lock-asset"; }
		}

		public string UnlockAssetUrl
		{
			get { return m_baseUrl + "/unlock-asset"; }
		}

		public string UnlockAssetAtCommitUrl
		{
			get { return m_baseUrl + "/unlock-asset-at-commit"; }
		}

		public string Username
		{
			get { return m_username; }
		}

		public Texture2D LockIcon
		{
			get { return m_lockIcon; }
		}

		public Texture2D LockedByMeIcon
		{
			get { return m_lockedByMeIcon; }
		}
	}
}