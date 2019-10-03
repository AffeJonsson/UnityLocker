using UnityEngine;

namespace Alf.UnityLocker
{
	[CreateAssetMenu(fileName = "ULLockSettings")]
	public sealed class ULLockSettings : ScriptableObject
	{
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

		public string Username
		{
			get { return m_username; }
		}
	}
}