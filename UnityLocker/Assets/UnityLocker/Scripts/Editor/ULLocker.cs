using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	public static class ULLocker
	{
		private static Dictionary<UnityEngine.Object, ULUser> sm_lockedAssets;

		public static Dictionary<UnityEngine.Object, ULUser> GetLockedAssets
		{
			get { return sm_lockedAssets; }
		}

		static ULLocker()
		{
			FetchLockedAssets();
		}

		public static bool TryLockAsset(UnityEngine.Object asset)
		{
			if (IsAssetLocked(asset))
			{
				return false;
			}
			sm_lockedAssets.Add(asset, ULUserManager.CurrentUser);
			return true;
		}

		public static bool TryUnlockAsset(UnityEngine.Object asset)
		{
			if (!IsAssetLockedByMe(asset))
			{
				return false;
			}
			sm_lockedAssets.Remove(asset);
			return true;
		}

		public static bool IsAssetLocked(UnityEngine.Object asset)
		{
			return sm_lockedAssets.ContainsKey(asset);
		}

		public static bool IsAssetLockedByMe(UnityEngine.Object asset)
		{
			ULUser locker;
			if (sm_lockedAssets.TryGetValue(asset, out locker))
			{
				return locker == ULUserManager.CurrentUser;
			}
			return false;
		}

		public static bool IsAssetLockedBySomeoneElse(UnityEngine.Object asset)
		{
			ULUser locker;
			if (sm_lockedAssets.TryGetValue(asset, out locker))
			{
				return locker != ULUserManager.CurrentUser;
			}
			return false;
		}

		public static void FetchLockedAssets()
		{
			var www = new WWW(ULLockSettingsHelper.Settings.GetLockedAssetsUrl);
			FecthLockedAssetsAsync(www, (data) =>
			{
				var lockData = JsonConvert.DeserializeObject<ULLockData>(data);
				sm_lockedAssets = lockData.LockData;
			});
		}

		private static async void FecthLockedAssetsAsync(WWW webRequest, Action<string> onComplete)
		{
			while (!webRequest.isDone)
			{
				await Task.Delay(25);
			}
			onComplete(webRequest.text);
		}
	}
}