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
		private static Dictionary<UnityEngine.Object, LockedAssetsData.AssetLockData> sm_lockedAssets = new Dictionary<UnityEngine.Object, LockedAssetsData.AssetLockData>();
		private static HashSet<UnityEngine.Object> sm_assetsLockedByMe = new HashSet<UnityEngine.Object>();
		private static HashSet<UnityEngine.Object> sm_assetsLockedBySomeoneElse = new HashSet<UnityEngine.Object>();
		private static HashSet<UnityEngine.Object> sm_assetsLockedButUnlockedLater = new HashSet<UnityEngine.Object>();

		private static float sm_nextFetchTime;

		private const float TimeBetweenFetches = 10f;

		public static event Action OnLockedAssetsChanged;

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
				FetchLockedAssets(null, null);
			}
		}

		public static IEnumerable<LockedAssetsData.AssetLockData> GetAssetsLockedByMe()
		{
			if (sm_lockedAssets == null || sm_lockedAssets.Count == 0)
			{
				yield break;
			}
			var enumerator = sm_lockedAssets.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Key != null && IsAssetLockedByMe(enumerator.Current.Key))
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
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Key != null && IsAssetLockedBySomeoneElse(enumerator.Current.Key))
				{
					yield return enumerator.Current.Value;
				}
			}
		}

		public static IEnumerable<LockedAssetsData.AssetLockData> GetAssetsUnlockedLater()
		{
			if (sm_lockedAssets == null || sm_lockedAssets.Count == 0)
			{
				yield break;
			}
			var enumerator = sm_lockedAssets.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Key != null && IsAssetUnlockedAtLaterCommit(enumerator.Current.Key))
				{
					yield return enumerator.Current.Value;
				}
			}
		}

		public static void TryLockAssets(UnityEngine.Object[] assets, Action onLockComplete, Action<string> onError)
		{
			FetchLockedAssets(() =>
			{
				int lockedIndex;
				if (IsAnyAssetLocked(assets, out lockedIndex))
				{
					Debug.Log("Asset " + assets[lockedIndex] + " is already locked");
					if (onError != null)
					{
						onError("Asset is locked by " + sm_lockedAssets[assets[lockedIndex]].LockerName);
					}

					return;
				}
				for (var i = 0; i < assets.Length; i++)
				{
					var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(assets[i]));
					sm_lockedAssets[assets[i]] = new LockedAssetsData.AssetLockData(guid, Container.GetLockSettings().Username);
					sm_assetsLockedByMe.Add(assets[i]);
				}
				if (OnLockedAssetsChanged != null)
				{
					OnLockedAssetsChanged();
				}
				LockAssetsAsync(Container.GetLockSettings().LockAssetsUrl, assets, onLockComplete, onError);
			}, onError);
		}

		public static void TryRevertAssetLocks(UnityEngine.Object[] assets, Action onRevertComplete, Action<string> onError)
		{
			FetchLockedAssets(() =>
			{
				int faultyIndex;
				if (!AreAssetsLockedByMe(assets, out faultyIndex))
				{
					Debug.Log("Asset " + assets[faultyIndex] + " is not locked by you!");
					if (onError != null)
					{
						onError("Asset is not locked by you, it's locked by " + sm_lockedAssets[assets[faultyIndex]].LockerName);
					}

					return;
				}
				for (var i = 0; i < assets.Length; i++)
				{
					var lockData = sm_lockedAssets[assets[i]];
					sm_lockedAssets.Remove(assets[i]);
					sm_assetsLockedByMe.Remove(assets[i]);
				}
				if (OnLockedAssetsChanged != null)
				{
					OnLockedAssetsChanged();
				}
				RevertAssetLocksAsync(Container.GetLockSettings().RevertAssetsLockUrl, assets, onRevertComplete, onError);
			}, onError);
		}

		public static void TryFinishLockingAssets(UnityEngine.Object[] assets, Action onFinishLockingComplete, Action<string> onError)
		{
			FetchLockedAssets(() =>
			{
				int faultyIndex;
				if (!AreAssetsLockedByMe(assets, out faultyIndex))
				{
					Debug.Log("Asset " + assets[faultyIndex] + " is not locked by you!");
					if (onError != null)
					{
						onError("Asset is not locked by you, it's locked by " + sm_lockedAssets[assets[faultyIndex]].LockerName);
					}

					return;
				}
				for (var i = 0; i < assets.Length; i++)
				{
					sm_lockedAssets.Remove(assets[i]);
					sm_assetsLockedByMe.Remove(assets[i]);
				}
				if (OnLockedAssetsChanged != null)
				{
					OnLockedAssetsChanged();
				}
				FinishLockingAssetsAsync(Container.GetLockSettings().FinishLockingAssetsUrl, assets, onFinishLockingComplete, onError);
			}, onError);
		}

		public static void FetchLockedAssets(Action onAssetsFetched, Action<string> onError)
		{
			sm_nextFetchTime = Time.realtimeSinceStartup + TimeBetweenFetches;
			var url = Container.GetLockSettings().GetLockedAssetsUrl;
			FecthLockedAssetsAsync(url, (data) =>
			{
				HasFetched = true;
				if (string.IsNullOrEmpty(data))
				{
					sm_lockedAssets = new Dictionary<UnityEngine.Object, LockedAssetsData.AssetLockData>();
					if (OnLockedAssetsChanged != null)
					{
						OnLockedAssetsChanged();
					}
					if (onAssetsFetched != null)
					{
						onAssetsFetched.Invoke();
					}

					return;
				}
				var lockData = JsonConvert.DeserializeObject<LockedAssetsData>(data);
				sm_lockedAssets = lockData.LockData;
				sm_assetsLockedButUnlockedLater.Clear();
				sm_assetsLockedByMe.Clear();
				sm_assetsLockedBySomeoneElse.Clear();
				foreach (var asset in sm_lockedAssets.Keys)
				{
					if (IsAssetLockedByMeThurough(asset))
					{
						sm_assetsLockedByMe.Add(asset);
					}
					else
					{
						if (IsAssetUnlockedAtLaterCommit(asset))
						{
							sm_assetsLockedButUnlockedLater.Add(asset);
						}
						if (IsAssetLockedBySomeoneElseThurough(asset))
						{
							sm_assetsLockedBySomeoneElse.Add(asset);
						}
					}
				}
				if (OnLockedAssetsChanged != null)
				{
					OnLockedAssetsChanged();
				}
				if (onAssetsFetched != null)
				{
					onAssetsFetched.Invoke();
				}

				EditorApplication.RepaintHierarchyWindow();
				EditorApplication.RepaintProjectWindow();
			}, onError);
		}

		public static bool IsAssetLocked(UnityEngine.Object asset)
		{
			if (!HasFetched)
			{
				return true;
			}
			return sm_assetsLockedByMe.Contains(asset) || sm_assetsLockedBySomeoneElse.Contains(asset);
		}

		public static bool IsAssetLockedByMe(UnityEngine.Object asset)
		{
			if (!HasFetched)
			{
				return false;
			}
			return sm_assetsLockedByMe.Contains(asset);
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
			return sm_assetsLockedBySomeoneElse.Contains(asset);
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

		private static void FecthLockedAssetsAsync(string url, Action<string> onComplete, Action<string> onError)
		{
#if UNITY_2018_4_OR_NEWER

			var webRequest = UnityWebRequest.Get(url);
			Container.GetWebRequestManager().WaitForWebRequest(webRequest, () =>
			{
				if (onComplete != null)
				{
					onComplete(webRequest.downloadHandler.text);
				}
			}, onError);
#else
			var www = new WWW(url);
			Container.GetWWWManager().WaitForWWW(www, () =>
			{
				if (onComplete != null)
				{
					onComplete(www.text);
				}
			}, onError);
#endif
		}

		private static void LockAssetsAsync(string url, UnityEngine.Object[] assets, Action onComplete, Action<string> onError)
		{
#if UNITY_2018_4_OR_NEWER
			var form = new WWWForm();
			form.AddField("Guid", JsonConvert.SerializeObject(assets.Select(a => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(a))).ToArray()));
			form.AddField("LockerName", Container.GetLockSettings().Username);
			var webRequest = UnityWebRequest.Post(url, form);
			Container.GetWebRequestManager().WaitForWebRequest(webRequest, onComplete, onError);
#else
			var form = new WWWForm();
			form.AddField("Guid", JsonConvert.SerializeObject(assets.Select(a => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(a))).ToArray()));
			form.AddField("LockerName", Container.GetLockSettings().Username);
			var www = new WWW(url, form);
			Container.GetWWWManager().WaitForWWW(www, onComplete, onError);
#endif
		}

		private static void RevertAssetLocksAsync(string url, UnityEngine.Object[] assets, Action onComplete, Action<string> onError)
		{
#if UNITY_2018_4_OR_NEWER
			var form = new WWWForm();
			form.AddField("Guid", JsonConvert.SerializeObject(assets.Select(a => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(a))).ToArray()));
			var webRequest = UnityWebRequest.Post(url, form);
			Container.GetWebRequestManager().WaitForWebRequest(webRequest, onComplete, onError);
#else
			var form = new WWWForm();
			form.AddField("Guid", JsonConvert.SerializeObject(assets.Select(a => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(a))).ToArray()));
			var www = new WWW(url, form);
			Container.GetWWWManager().WaitForWWW(www, onComplete, onError);
#endif
		}

		private static void FinishLockingAssetsAsync(string url, UnityEngine.Object[] assets, Action onComplete, Action<string> onError)
		{
#if UNITY_2018_4_OR_NEWER
			var form = new WWWForm();
			form.AddField("Guid", JsonConvert.SerializeObject(assets.Select(a => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(a))).ToArray()));
			form.AddField("Sha", Container.GetVersionControlHandler().GetShaOfHead());
			var webRequest = UnityWebRequest.Post(url, form);
			Container.GetWebRequestManager().WaitForWebRequest(webRequest, onComplete, onError);
#else
			var form = new WWWForm();
			form.AddField("Guid", JsonConvert.SerializeObject(assets.Select(a => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(a))).ToArray()));
			form.AddField("Sha", Container.GetVersionControlHandler().GetShaOfHead());
			var www = new WWW(url, form);
			Container.GetWWWManager().WaitForWWW(www, onComplete, onError);
#endif
		}

		private static bool IsAssetLockedByMeThurough(UnityEngine.Object asset)
		{
			if (!HasFetched)
			{
				return true;
			}
			LockedAssetsData.AssetLockData lockData;
			if (sm_lockedAssets.TryGetValue(asset, out lockData))
			{
				return (lockData.Locked || (!string.IsNullOrEmpty(lockData.UnlockSha) && !Container.GetVersionControlHandler().IsCommitChildOfHead(lockData.UnlockSha))) && lockData.LockerName == Container.GetLockSettings().Username;
			}
			return false;
		}

		private static bool IsAssetLockedBySomeoneElseThurough(UnityEngine.Object asset)
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
	}
}