using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	[InitializeOnLoad]
	public static class ULLocker
	{
		private static Dictionary<UnityEngine.Object, ULUser> sm_lockedAssets;
		private static float sm_nextFetchTime;

		private const float TimeBetweenFetches = 10f;

		public static Dictionary<UnityEngine.Object, ULUser> GetLockedAssets
		{
			get { return sm_lockedAssets; }
		}

		static ULLocker()
		{
			ULUserManager.CurrentUser = new ULUser(ULLockSettingsHelper.Settings.Username);
			EditorApplication.update += Update;
		}

		private static void Update()
		{
			if (Time.realtimeSinceStartup >= sm_nextFetchTime)
			{
				FetchLockedAssets(null);
			}
		}

		public static void TryLockAsset(UnityEngine.Object asset, Action<bool, string> onLockComplete)
		{
			FetchLockedAssets(() =>
			{
				if (IsAssetLocked(asset))
				{
					Debug.Log("Asset " + asset + " is already locked");
					onLockComplete(false, "Asset is locked by " + sm_lockedAssets[asset].Name);
					return;
				}
				Debug.Log("Locked asset " + asset);
				sm_lockedAssets.Add(asset, ULUserManager.CurrentUser);
				onLockComplete(true, null);
			});
		}

		public static void TryUnlockAsset(UnityEngine.Object asset, Action<bool, string> onUnlockComplete)
		{
			FetchLockedAssets(() =>
			{
				if (!IsAssetLockedByMe(asset))
				{
					Debug.Log("Asset " + asset + " is not locked by you!");
					onUnlockComplete(false, "Asset is not locked by you, it's locked by " + sm_lockedAssets[asset].Name);
					return;
				}
				Debug.Log("Unlocked asset " + asset);
				sm_lockedAssets.Remove(asset);
				onUnlockComplete(true, null);
			});
		}

		public static void FetchLockedAssets(Action onAssetsFetched)
		{
			sm_nextFetchTime = Time.realtimeSinceStartup + TimeBetweenFetches;
			var url = ULLockSettingsHelper.Settings.GetLockedAssetsUrl;
			FecthLockedAssetsAsync(url, (data) =>
			{
				var lockData = JsonConvert.DeserializeObject<ULLockData>(data);
				sm_lockedAssets = lockData.LockData;
				onAssetsFetched?.Invoke();
			});
		}

		public static void IsAssetLocked(UnityEngine.Object asset, Action<bool> onLockedChecked)
		{
			if (sm_lockedAssets == null)
			{
				FetchLockedAssets(() =>
				{
					onLockedChecked?.Invoke(IsAssetLocked(asset));
				});
				return;
			}
			onLockedChecked?.Invoke(IsAssetLocked(asset));
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
				return locker.Equals(ULUserManager.CurrentUser);
			}
			return false;
		}

		public static bool IsAssetLockedBySomeoneElse(UnityEngine.Object asset)
		{
			ULUser locker;
			if (sm_lockedAssets.TryGetValue(asset, out locker))
			{
				return !locker.Equals(ULUserManager.CurrentUser);
			}
			return false;
		}

		private static void FecthLockedAssetsAsync(string url, Action<string> onComplete)
		{
			// TODO: WebRequest
			var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(url);
			if (asset == null)
			{
				Debug.LogError("Asset at url " + url + " is null");
				onComplete("{}");
				return;
			}
			onComplete(asset.text);
		}
	}
}