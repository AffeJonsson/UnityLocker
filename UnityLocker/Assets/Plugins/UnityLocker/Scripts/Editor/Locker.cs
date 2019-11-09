using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Alf.UnityLocker.Editor
{
	[InitializeOnLoad]
	public static class Locker
	{
		private static Dictionary<UnityEngine.Object, LockedAssetsData.AssetLockData> sm_lockedAssets;
		private static float sm_nextFetchTime;

		private const float TimeBetweenFetches = 10f;

		public static event Action OnLockedAssetsChanged;

		public static bool HasFetched
		{
			get;
			private set;
		}

		public static Dictionary<UnityEngine.Object, LockedAssetsData.AssetLockData> LockedAssets
		{
			get
			{
				return sm_lockedAssets;
			}

			set
			{
				sm_lockedAssets = value;
				OnLockedAssetsChanged?.Invoke();
			}
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
			if (LockedAssets == null || LockedAssets.Count == 0)
			{
				yield break;
			}
			var enumerator = LockedAssets.GetEnumerator();
			while (enumerator.MoveNext())
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
			if (LockedAssets == null || LockedAssets.Count == 0)
			{
				yield break;
			}
			var enumerator = LockedAssets.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (IsAssetLockedBySomeoneElse(enumerator.Current.Key))
				{
					yield return enumerator.Current.Value;
				}
			}
		}

		public static IEnumerable<LockedAssetsData.AssetLockData> GetAssetsUnlockedLater()
		{
			if (LockedAssets == null || LockedAssets.Count == 0)
			{
				yield break;
			}
			var enumerator = LockedAssets.GetEnumerator();
			do
			{
				if (IsAssetUnlockedAtLaterCommit(enumerator.Current.Key))
				{
					yield return enumerator.Current.Value;
				}
			} while (enumerator.MoveNext());
		}

		public static void TryLockAssets(UnityEngine.Object[] assets, Action<bool, string> onLockComplete)
		{
			FetchLockedAssets(() =>
			{
				int lockedIndex;
				if (IsAnyAssetLocked(assets, out lockedIndex))
				{
					Debug.Log("Asset " + assets[lockedIndex] + " is already locked");
					onLockComplete?.Invoke(false, "Asset is locked by " + LockedAssets[assets[lockedIndex]].LockerName);
					return;
				}
				for (var i = 0; i < assets.Length; i++)
				{
					LockedAssets[assets[i]] = new LockedAssetsData.AssetLockData(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(assets[i])), Container.GetLockSettings().Username);
				}
				LockAssetsAsync(Container.GetLockSettings().LockAssetsUrl, assets, () =>
				{
					onLockComplete?.Invoke(true, null);
				});
			});
		}

		public static void TryRevertAssetLocks(UnityEngine.Object[] assets, Action<bool, string> onUnlockComplete)
		{
			FetchLockedAssets(() =>
			{
				int faultyIndex;
				if (!AreAssetsLockedByMe(assets, out faultyIndex))
				{
					Debug.Log("Asset " + assets[faultyIndex] + " is not locked by you!");
					onUnlockComplete?.Invoke(false, "Asset is not locked by you, it's locked by " + LockedAssets[assets[faultyIndex]].LockerName);
					return;
				}
				for (var i = 0; i < assets.Length; i++)
				{
					LockedAssets.Remove(assets[i]);
				}
				RevertAssetLocksAsync(Container.GetLockSettings().RevertAssetsLockUrl, assets, () =>
				{
					onUnlockComplete?.Invoke(true, null);
				});
			});
		}

		public static void TryUnlockAssets(UnityEngine.Object[] assets, Action<bool, string> onUnlockComplete)
		{
			FetchLockedAssets(() =>
			{
				int faultyIndex;
				if (!AreAssetsLockedByMe(assets, out faultyIndex))
				{
					Debug.Log("Asset " + assets[faultyIndex] + " is not locked by you!");
					onUnlockComplete?.Invoke(false, "Asset is not locked by you, it's locked by " + LockedAssets[assets[faultyIndex]].LockerName);
					return;
				}
				for (var i = 0; i < assets.Length; i++)
				{
					LockedAssets.Remove(assets[i]);
				}
				UnlockAssetsAsync(Container.GetLockSettings().UnlockAssetsUrl, assets, () =>
				{
					onUnlockComplete?.Invoke(true, null);
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
					LockedAssets = new Dictionary<UnityEngine.Object, LockedAssetsData.AssetLockData>();
					onAssetsFetched?.Invoke();
					return;
				}
				var lockData = JsonConvert.DeserializeObject<LockedAssetsData>(data);
				LockedAssets = lockData.LockData;
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
			if (LockedAssets.TryGetValue(asset, out lockData))
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
			if (LockedAssets.TryGetValue(asset, out lockData))
			{
				return (lockData.Locked || (!string.IsNullOrEmpty(lockData.UnlockSha) && !Container.GetVersionControlHandler().IsCommitChildOfHead(lockData.UnlockSha))) && lockData.LockerName == Container.GetLockSettings().Username;
			}
			return false;
		}

		public static bool AreAssetsLockedByMe(UnityEngine.Object[] assets, out int fautlyIndex)
		{
			fautlyIndex = -1;
			for (var i = 0; i < assets.Length; i++)
			{
				if (!IsAssetLockedByMe(assets[i]))
				{
					fautlyIndex = i;
					return false;
				}
			}
			return true;
		}

		public static bool IsAnyAssetLocked(UnityEngine.Object[] assets, out int lockedIndex)
		{
			lockedIndex = -1;
			for (var i = 0; i < assets.Length; i++)
			{
				if (IsAssetLocked(assets[i]))
				{
					lockedIndex = i;
					return true;
				}
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
			if (LockedAssets.TryGetValue(asset, out lockData))
			{
				return (lockData.Locked || (!string.IsNullOrEmpty(lockData.UnlockSha) && !Container.GetVersionControlHandler().IsCommitChildOfHead(lockData.UnlockSha))) && lockData.LockerName != Container.GetLockSettings().Username;
			}
			return false;
		}

		public static string GetAssetLocker(UnityEngine.Object asset)
		{
			LockedAssetsData.AssetLockData lockData;
			if (LockedAssets.TryGetValue(asset, out lockData))
			{
				return lockData.LockerName;
			}
			return null;
		}

		public static bool IsAssetUnlockedAtLaterCommit(UnityEngine.Object asset)
		{
			LockedAssetsData.AssetLockData lockData;
			if (LockedAssets.TryGetValue(asset, out lockData))
			{
				return !lockData.Locked;
			}
			return false;
		}

		public static string GetAssetUnlockCommitSha(UnityEngine.Object asset)
		{
			LockedAssetsData.AssetLockData lockData;
			if (LockedAssets.TryGetValue(asset, out lockData))
			{
				return lockData.UnlockSha ?? string.Empty;
			}
			return string.Empty;
		}

		private static void FecthLockedAssetsAsync(string url, Action<string> onComplete)
		{
#if UNITY_2018_4_OR_NEWER

			var webRequest = UnityWebRequest.Get(url);
			Container.GetWebRequestManager().WaitForWebRequest(webRequest, () =>
			{
				onComplete?.Invoke(webRequest.downloadHandler.text);
			});
#else
			var www = new WWW(url);
			Container.GetWWWManager().WaitForWWW(www, () =>
			{
				onComplete?.Invoke(www.text);
			});
#endif
		}

		private static void LockAssetsAsync(string url, UnityEngine.Object[] assets, Action onComplete)
		{
#if UNITY_2018_4_OR_NEWER
			var form = new WWWForm();
			form.AddField("Guid", JsonConvert.SerializeObject(assets.Select(a => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(a))).ToArray()));
			form.AddField("LockerName", Container.GetLockSettings().Username);
			var webRequest = UnityWebRequest.Post(url, form);
			Container.GetWebRequestManager().WaitForWebRequest(webRequest, onComplete);
#else
			var form = new WWWForm();
			form.AddField("Guid", JsonConvert.SerializeObject(assets.Select(a => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(a))).ToArray()));
			form.AddField("LockerName", Container.GetLockSettings().Username);
			var www = new WWW(url, form);
			Container.GetWWWManager().WaitForWWW(www, onComplete);
#endif
		}

		private static void RevertAssetLocksAsync(string url, UnityEngine.Object[] assets, Action onComplete)
		{
#if UNITY_2018_4_OR_NEWER
			var form = new WWWForm();
			form.AddField("Guid", JsonConvert.SerializeObject(assets.Select(a => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(a))).ToArray()));
			var webRequest = UnityWebRequest.Post(url, form);
			Container.GetWebRequestManager().WaitForWebRequest(webRequest, onComplete);
#else
			var form = new WWWForm();
			form.AddField("Guid", JsonConvert.SerializeObject(assets.Select(a => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(a))).ToArray()));
			var www = new WWW(url, form);
			Container.GetWWWManager().WaitForWWW(www, onComplete);
#endif
		}

		private static void UnlockAssetsAsync(string url, UnityEngine.Object[] assets, Action onComplete)
		{
#if UNITY_2018_4_OR_NEWER
			var form = new WWWForm();
			form.AddField("Guid", JsonConvert.SerializeObject(assets.Select(a => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(a))).ToArray()));
			form.AddField("Sha", Container.GetVersionControlHandler().GetShaOfHead());
			var webRequest = UnityWebRequest.Post(url, form);
			Container.GetWebRequestManager().WaitForWebRequest(webRequest, onComplete);
#else
			var form = new WWWForm();
			form.AddField("Guid", JsonConvert.SerializeObject(assets.Select(a => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(a))).ToArray()));
			form.AddField("Sha", Container.GetVersionControlHandler().GetShaOfHead());
			var www = new WWW(url, form);
			Container.GetWWWManager().WaitForWWW(www, onComplete);
#endif
		}
	}
}