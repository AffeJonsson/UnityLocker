using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	[InitializeOnLoad]
	public static class Locker
	{
		private static Dictionary<UnityEngine.Object, LockedAssetsData.AssetLockData> sm_lockedAssets;
		private static float sm_nextFetchTime;

		private const float TimeBetweenFetches = 10f;

		public static bool HasFetched
		{
			get;
			private set;
		}

		static Locker()
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

		public static IEnumerable<LockedAssetsData.AssetLockData> GetAssetsLockedByMe()
		{
			if (sm_lockedAssets == null || sm_lockedAssets.Count == 0)
			{
				yield break;
			}
			foreach (var kvp in sm_lockedAssets)
			{
				Debug.Log(kvp);
			}
			var enumerator = sm_lockedAssets.GetEnumerator();
			while(enumerator.MoveNext())
			{
				if (enumerator.Current.Key == null)
				{
					Debug.LogWarning(enumerator.Current + " is null");
				}
				else if (IsAssetLockedByMe(enumerator.Current.Key))
				{
					yield return enumerator.Current.Value;
				}
			}
		}

		public static IEnumerable<LockedAssetsData.AssetLockData> GetAssetsLockedBySomeoneElse()
		{
			if (sm_lockedAssets == null || sm_lockedAssets.Count == 0)
			{
				yield break;
			}
			var enumerator = sm_lockedAssets.GetEnumerator();
			do
			{
				if (IsAssetLockedBySomeoneElse(enumerator.Current.Key))
				{
					yield return enumerator.Current.Value;
				}
			} while (enumerator.MoveNext());
		}

		public static IEnumerable<LockedAssetsData.AssetLockData> GetAssetsUnlockedLater()
		{
			if (sm_lockedAssets == null || sm_lockedAssets.Count == 0)
			{
				yield break;
			}
			var enumerator = sm_lockedAssets.GetEnumerator();
			do
			{
				if (IsAssetUnlockedAtLaterCommit(enumerator.Current.Key))
				{
					yield return enumerator.Current.Value;
				}
			} while (enumerator.MoveNext());
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
				sm_lockedAssets[asset] = new LockedAssetsData.AssetLockData(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset)), Container.GetLockSettings().Username);
				LockAssetAsync(Container.GetLockSettings().LockAssetUrl, asset, () =>
				{
					onLockComplete(true, null);
				});
			});
		}

		public static void TryRevertAssetLock(UnityEngine.Object asset, Action<bool, string> onUnlockComplete)
		{
			FetchLockedAssets(() =>
			{
				if (!IsAssetLockedByMe(asset))
				{
					Debug.Log("Asset " + asset + " is not locked by you!");
					onUnlockComplete(false, "Asset is not locked by you, it's locked by " + sm_lockedAssets[asset].LockerName);
					return;
				}
				Debug.Log("Reverted lock asset " + asset);
				sm_lockedAssets.Remove(asset);
				RevertAssetLockAsync(Container.GetLockSettings().RevertAssetLockUrl, asset, () =>
				{
					onUnlockComplete(true, null);
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
				Debug.Log("Unlocked asset " + asset + " at current commit");
				sm_lockedAssets.Remove(asset);
				UnlockAssetAsync(Container.GetLockSettings().UnlockAssetUrl, asset, () =>
				{
					onUnlockComplete(true, null);
				});
			});
		}

		public static void FetchLockedAssets(Action onAssetsFetched)
		{
			sm_nextFetchTime = Time.realtimeSinceStartup + TimeBetweenFetches;
			var url = Container.GetLockSettings().GetLockedAssetsUrl;
			FecthLockedAssetsAsync(url, (data) =>
			{
				HasFetched = true;
				if (string.IsNullOrEmpty(data))
				{
					sm_lockedAssets = new Dictionary<UnityEngine.Object, LockedAssetsData.AssetLockData>();
					onAssetsFetched?.Invoke();
					return;
				}
				var lockData = JsonConvert.DeserializeObject<LockedAssetsData>(data);
				sm_lockedAssets = lockData.LockData;
				onAssetsFetched?.Invoke();
				EditorApplication.RepaintHierarchyWindow();
				EditorApplication.RepaintProjectWindow();
			});
		}

		public static bool IsAssetLocked(UnityEngine.Object asset)
		{
			if (!HasFetched)
			{
				return true;
			}
			LockedAssetsData.AssetLockData lockData;
			if (sm_lockedAssets.TryGetValue(asset, out lockData))
			{
				return lockData.Locked || (!string.IsNullOrEmpty(lockData.UnlockSha) && !Container.GetVersionControlHandler().IsCommitChildOfHead(lockData.UnlockSha));
			}
			return false;
		}

		public static bool IsAssetLockedByMe(UnityEngine.Object asset)
		{
			if (!HasFetched)
			{
				return false;
			}
			LockedAssetsData.AssetLockData lockData;
			if (sm_lockedAssets.TryGetValue(asset, out lockData))
			{
				return (lockData.Locked || (!string.IsNullOrEmpty(lockData.UnlockSha) && !Container.GetVersionControlHandler().IsCommitChildOfHead(lockData.UnlockSha))) && lockData.LockerName == Container.GetLockSettings().Username;
			}
			return false;
		}

		public static bool IsAssetLockedBySomeoneElse(UnityEngine.Object asset)
		{
			if (!HasFetched)
			{
				return true;
			}
			LockedAssetsData.AssetLockData lockData;
			if (sm_lockedAssets.TryGetValue(asset, out lockData))
			{
				return (lockData.Locked || (!string.IsNullOrEmpty(lockData.UnlockSha) && !Container.GetVersionControlHandler().IsCommitChildOfHead(lockData.UnlockSha))) && lockData.LockerName != Container.GetLockSettings().Username;
			}
			return false;
		}

		public static string GetAssetLocker(UnityEngine.Object asset)
		{
			LockedAssetsData.AssetLockData lockData;
			if (sm_lockedAssets.TryGetValue(asset, out lockData))
			{
				return lockData.LockerName;
			}
			return null;
		}

		public static bool IsAssetUnlockedAtLaterCommit(UnityEngine.Object asset)
		{
			LockedAssetsData.AssetLockData lockData;
			if (sm_lockedAssets.TryGetValue(asset, out lockData))
			{
				return !lockData.Locked;
			}
			return false;
		}

		public static string GetAssetUnlockCommitSha(UnityEngine.Object asset)
		{
			LockedAssetsData.AssetLockData lockData;
			if (sm_lockedAssets.TryGetValue(asset, out lockData))
			{
				return lockData.UnlockSha ?? string.Empty;
			}
			return string.Empty;
		}

		private static void FecthLockedAssetsAsync(string url, Action<string> onComplete)
		{
			var www = new WWW(url);
			Container.GetWWWManager().WaitForWWW(www, () =>
			{
				onComplete?.Invoke(www.text);
			});
		}

		private static void LockAssetAsync(string url, UnityEngine.Object asset, Action onComplete)
		{
			var form = new WWWForm();
			form.AddField("Guid", AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset)));
			form.AddField("LockerName", Container.GetLockSettings().Username);
			var www = new WWW(url, form);
			Container.GetWWWManager().WaitForWWW(www, onComplete);
		}

		private static void RevertAssetLockAsync(string url, UnityEngine.Object asset, Action onComplete)
		{
			var form = new WWWForm();
			form.AddField("Guid", AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset)));
			var www = new WWW(url, form);
			Container.GetWWWManager().WaitForWWW(www, onComplete);
		}

		private static void UnlockAssetAsync(string url, UnityEngine.Object asset, Action onComplete)
		{
			var form = new WWWForm();
			form.AddField("Guid", AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset)));
			form.AddField("Sha", Container.GetVersionControlHandler().GetShaOfHead());
			var www = new WWW(url, form);
			Container.GetWWWManager().WaitForWWW(www, onComplete);
		}
	}
}