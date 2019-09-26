using UnityEngine;

namespace Alf.UnityLocker
{
	[CreateAssetMenu(fileName = "ULLockSettings")]
	public sealed class ULLockSettings : ScriptableObject
	{
		[SerializeField]
		private string m_baseUrl;

		public string GetLockedAssetsUrl
		{
			get { return m_baseUrl + "\\get-locked-assets.json"; }
		}
	}
}