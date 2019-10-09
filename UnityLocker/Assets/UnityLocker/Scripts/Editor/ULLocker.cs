using LibGit2Sharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	[InitializeOnLoad]
	public static class ULLocker
	{
		private static Dictionary<UnityEngine.Object, ULLockData.AssetLockData> sm_lockedAssets;
		private static float sm_nextFetchTime;

		private const float TimeBetweenFetches = 10f;

		public static bool HasFetched
		{
			get;
			private set;
		}

		static ULLocker()
		{
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
					onLockComplete(false, "Asset is locked by " + sm_lockedAssets[asset].LockerName);
					return;
				}
				Debug.Log("Locked asset " + asset);
				sm_lockedAssets.Add(asset, new ULLockData.AssetLockData(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset)), ULUserManager.CurrentUser.Name));
				LockAssetAsync(ULLockSettingsHelper.Settings.LockAssetUrl, asset, () =>
				{
					onLockComplete(true, null);
				});
			});
		}

		public static void TryUnlockAsset(UnityEngine.Object asset, Action<bool, string> onUnlockComplete)
		{
			FetchLockedAssets(() =>
			{
				if (!IsAssetLockedByMe(asset))
				{
					Debug.Log("Asset " + asset + " is not locked by you!");
					onUnlockComplete(false, "Asset is not locked by you, it's locked by " + sm_lockedAssets[asset].LockerName);
					return;
				}
				Debug.Log("Unlocked asset " + asset);
				sm_lockedAssets.Remove(asset);
				UnlockAssetAsync(ULLockSettingsHelper.Settings.UnlockAssetUrl, asset, () =>
				{
					onUnlockComplete(true, null);
				});
			});
		}

		public static void TryUnlockAssetAtCurrentCommit(UnityEngine.Object asset, Action<bool, string> onUnlockComplete)
		{
			FetchLockedAssets(() =>
			{
				if (!IsAssetLockedByMe(asset))
				{
					Debug.Log("Asset " + asset + " is not locked by you!");
					onUnlockComplete(false, "Asset is not locked by you, it's locked by " + sm_lockedAssets[asset].LockerName);
					return;
				}
				Debug.Log("Unlocked asset " + asset + " at current commit");
				sm_lockedAssets.Remove(asset);
				UnlockAssetAtCurrentCommitAsync(ULLockSettingsHelper.Settings.UnlockAssetAtCommitUrl, asset, () =>
				{
					onUnlockComplete(true, null);
				});
			});
		}

		public static void FetchLockedAssets(Action onAssetsFetched)
		{
			sm_nextFetchTime = Time.realtimeSinceStartup + TimeBetweenFetches;
			var url = ULLockSettingsHelper.Settings.GetLockedAssetsUrl;
			FecthLockedAssetsAsync(url, (data) =>
			{
				if (string.IsNullOrEmpty(data))
				{
					onAssetsFetched?.Invoke();
					return;
				}
				var lockData = JsonConvert.DeserializeObject<ULLockData>(data);
				sm_lockedAssets = lockData.LockData;
				HasFetched = true;
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
			ULLockData.AssetLockData lockData;
			if (sm_lockedAssets.TryGetValue(asset, out lockData))
			{
				return string.IsNullOrEmpty(lockData.UnlockSha) || !ULGitHandler.IsCommitChildOfHead(lockData.UnlockSha);
			}
			return false;
		}

		public static bool IsAssetLockedByMe(UnityEngine.Object asset)
		{
			ULLockData.AssetLockData lockData;
			if (sm_lockedAssets.TryGetValue(asset, out lockData))
			{
				return (string.IsNullOrEmpty(lockData.UnlockSha) || !ULGitHandler.IsCommitChildOfHead(lockData.UnlockSha)) && lockData.LockerName == ULUserManager.CurrentUser.Name;
			}
			return false;
		}

		public static bool IsAssetLockedBySomeoneElse(UnityEngine.Object asset)
		{
			ULLockData.AssetLockData lockData;
			if (sm_lockedAssets.TryGetValue(asset, out lockData))
			{
				return (string.IsNullOrEmpty(lockData.UnlockSha) || !ULGitHandler.IsCommitChildOfHead(lockData.UnlockSha)) && lockData.LockerName != ULUserManager.CurrentUser.Name;
			}
			return false;
		}

		public static ULUser GetAssetLocker(UnityEngine.Object asset)
		{
			ULLockData.AssetLockData lockData;
			if (sm_lockedAssets.TryGetValue(asset, out lockData))
			{
				return ULUserManager.GetUser(lockData.LockerName);
			}
			return null;
		}

		private static void FecthLockedAssetsAsync(string url, Action<string> onComplete)
		{
			var www = new WWW(url);
			ULWWWManager.WaitForWWW(www, () =>
			{
				onComplete?.Invoke(www.text);
			});
		}

		private static void LockAssetAsync(string url, UnityEngine.Object asset, Action onComplete)
		{
			var form = new WWWForm();
			form.AddField("Guid", AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset)));
			form.AddField("LockerName", ULUserManager.CurrentUser.Name);
			var www = new WWW(url, form);
			ULWWWManager.WaitForWWW(www, onComplete);
		}

		private static void UnlockAssetAsync(string url, UnityEngine.Object asset, Action onComplete)
		{
			var form = new WWWForm();
			form.AddField("Guid", AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset)));
			var www = new WWW(url, form);
			ULWWWManager.WaitForWWW(www, onComplete);
		}

		private static void UnlockAssetAtCurrentCommitAsync(string url, UnityEngine.Object asset, Action onComplete)
		{
			var form = new WWWForm();
			form.AddField("Guid", AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset)));
			form.AddField("Sha", ULGitHandler.GetShaOfHead());
			var www = new WWW(url, form);
			ULWWWManager.WaitForWWW(www, onComplete);
		}
	}
}