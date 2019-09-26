using UnityEditor;

namespace Alf.UnityLocker.Editor
{
	public static class ULLockSettingsHelper
	{
		public static ULLockSettings Settings;

		static ULLockSettingsHelper()
		{
			Settings = AssetDatabase.LoadAssetAtPath<ULLockSettings>("Assets/UnityLocker/Assets/ULLockSettings.asset");
		}
	}
}