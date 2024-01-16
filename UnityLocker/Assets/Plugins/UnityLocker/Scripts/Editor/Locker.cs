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
		private const int MaxShortShaLength = 8;

		public static event Action OnLockedAssetsChanged;

		public static bool HasFetched
		{
			get;
			private set;
		}

		public static string ErrorMessage
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
			if (!Container.GetLockSettings().IsEnabled)
			{
				return;
			}
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
				if (enumerator.Current.Key != null && sm_assetsLockedByMe.Contains(enumerator.Current.Key))
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
				if (enumerator.Current.Key != null && sm_assetsLockedBySomeoneElse.Contains(enumerator.Current.Key))
				{
					yield return enumerator.Current.Value;
				}
			}
		}

		public static IEnumerable<LockedAssetsData.AssetLockData> GetAssetsLockedNowButUnlockedLater()
		{
			if (sm_lockedAssets == null || sm_lockedAssets.Count == 0)
			{
				yield break;
			}
			var enumerator = sm_lockedAssets.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Key != null && sm_assetsLockedButUnlockedLater.Contains(enumerator.Current.Key))
				{
					yield return enumerator.Current.Value;
				}
			}
		}

		public static void TryLockAssets(UnityEngine.Object[] assets, Action onLockComplete, Action<string> onError)
		{
			FetchLockedAssets(() =>
			{
				assets = GetAssetsWhereLockIsValid(assets);
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
					sm_lockedAssets[assets[i]] = new LockedAssetsData.AssetLockData(guid, Container.GetLockSettings().Username, true, "", DateTime.UtcNow);
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
				assets = GetAssetsWhereRevertLockIsValid(assets);
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
				assets = GetAssetsWhereFinishLockIsValid(assets);
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

		public static void TryClearLocks(Action onClearLocksComplete, Action<string> onError)
		{
			ClearLocks(Container.GetLockSettings().ClearLocksUrl, () => FetchLockedAssets(onClearLocksComplete, onError), onError);
		}

		public static void FetchLockedAssets(Action onAssetsFetched, Action<string> onError)
		{
			sm_nextFetchTime = Time.realtimeSinceStartup + TimeBetweenFetches;
			var url = Container.GetLockSettings().GetLockedAssetsUrl;
			FetchLockedAssetsAsync(url, (data) =>
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

					foreach (var editor in ActiveEditorTracker.sharedTracker.activeEditors)
					{
						editor.Repaint();
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
						if (IsAssetLockedNowButUnlockedAtLaterCommitThurough(asset))
						{
							sm_assetsLockedButUnlockedLater.Add(asset);
						}
						else if (IsAssetLockedBySomeoneElseThurough(asset))
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
				foreach (var editor in ActiveEditorTracker.sharedTracker.activeEditors)
				{
					editor.Repaint();
				}
			}, onError);
		}

		public static void FetchAssetHistory(UnityEngine.Object asset, Action<AssetHistory.AssetHistoryData[]> onHistoryFetched, Action<string> onError)
		{
			var url = Container.GetLockSettings().GetAssetHistoryUrl;
			FetchAssetHistoryAsync(url, asset, (data) =>
			{
				var history = JsonConvert.DeserializeObject<AssetHistory>(data);
				if (onHistoryFetched != null)
				{
					onHistoryFetched.Invoke(history.AssetHistoryDatas);
				}
			}, onError);
		}

		public static bool IsAssetLockedByMe(UnityEngine.Object asset)
		{
			if (!HasFetched)
			{
				return false;
			}
			return sm_assetsLockedByMe.Contains(FilterAsset(asset));
		}

		public static bool IsAssetLockedBySomeoneElse(UnityEngine.Object asset)
		{
			if (!HasFetched)
			{
				return true;
			}
			return sm_assetsLockedBySomeoneElse.Contains(FilterAsset(asset));
		}

		public static bool IsAssetLockedNowButUnlockedAtLaterCommit(UnityEngine.Object asset)
		{
			if (!HasFetched)
			{
				return true;
			}
			return sm_assetsLockedButUnlockedLater.Contains(FilterAsset(asset));
		}

		public static bool AreAllAssetsLockedByMe(UnityEngine.Object[] assets)
		{
			for (var i = 0; i < assets.Length; i++)
			{
				if (!IsAssetLockedByMe(assets[i]))
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsAnyAssetLockedBySomeoneElse(UnityEngine.Object[] assets)
		{
			for (var i = 0; i < assets.Length; i++)
			{
				if (IsAssetLockedBySomeoneElse(assets[i]))
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsAnyAssetLockedNowButUnlockedAtLaterCommit(UnityEngine.Object[] assets)
		{
			for (var i = 0; i < assets.Length; i++)
			{
				if (IsAssetLockedNowButUnlockedAtLaterCommit(assets[i]))
				{
					return true;
				}
			}
			return false;
		}

		public static string GetAssetLocker(UnityEngine.Object asset)
		{
			LockedAssetsData.AssetLockData lockData;
			if (sm_lockedAssets.TryGetValue(FilterAsset(asset), out lockData))
			{
				return lockData.LockerName;
			}
			return null;
		}

		public static string GetAssetUnlockCommitSha(UnityEngine.Object asset)
		{
			LockedAssetsData.AssetLockData lockData;
			if (sm_lockedAssets.TryGetValue(FilterAsset(asset), out lockData))
			{
				return lockData.UnlockSha ?? string.Empty;
			}
			return string.Empty;
		}

		public static string GetAssetUnlockCommitShaShort(UnityEngine.Object asset)
		{
			LockedAssetsData.AssetLockData lockData;
			if (sm_lockedAssets.TryGetValue(FilterAsset(asset), out lockData))
			{
				if (string.IsNullOrEmpty(lockData.UnlockSha))
				{
					return string.Empty;
				}
				return lockData.UnlockSha.Substring(0, Mathf.Min(MaxShortShaLength, lockData.UnlockSha.Length));
			}
			return string.Empty;
		}

		public static bool GetIsLockValid(UnityEngine.Object asset)
		{
			if (!Container.GetLockSettings().IsEnabled)
			{
				return false;
			}
			asset = FilterAsset(asset);
			return asset != null && !IsAssetLockedByMe(asset) && !IsAssetLockedBySomeoneElse(asset) && !IsAssetLockedNowButUnlockedAtLaterCommit(asset) && Container.GetAssetTypeValidators().IsAssetValid(asset);
		}

		public static bool GetIsRevertLockValid(UnityEngine.Object asset)
		{
			if (!Container.GetLockSettings().IsEnabled)
			{
				return false;
			}
			asset = FilterAsset(asset);
			return asset != null && IsAssetLockedByMe(asset);
		}

		public static bool GetIsFinishLockValid(UnityEngine.Object asset)
		{
			if (!Container.GetLockSettings().IsEnabled)
			{
				return false;
			}
			asset = FilterAsset(asset);
			return asset != null && IsAssetLockedByMe(asset);
		}

		public static bool IsAssetTypeValid(UnityEngine.Object asset)
		{
			return Container.GetAssetTypeValidators().IsAssetValid(FilterAsset(asset));
		}

		public static bool AreAssetTypesValid(UnityEngine.Object[] assets)
		{
			for (var i = 0; i < assets.Length; i++)
			{
				if (!IsAssetTypeValid(assets[i]))
				{
					return false;
				}
			}
			return true;
		}

		public static UnityEngine.Object FilterAsset(UnityEngine.Object asset)
		{
			if (asset == null)
			{
				return null;
			}
#if UNITY_2018_3_OR_NEWER
			if (!AssetDatabase.Contains(asset))
			{
				var currentStage = 
#if UNITY_2021_2_OR_NEWER
				UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#else
				UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#endif
				if (currentStage != null && currentStage.prefabContentsRoot == asset)
				{
#if UNITY_2021_2_OR_NEWER
					return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(currentStage.assetPath);
#else
					return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(currentStage.prefabAssetPath);
#endif
				}
				if (PrefabUtility.GetPrefabAssetType(asset) == PrefabAssetType.NotAPrefab)
				{
					return null;
				}
				// Only allow locking the nearest prefab, and not just child objects
				if (PrefabUtility.GetNearestPrefabInstanceRoot(asset) != asset)
				{
					return null;
				}
			}
#endif
			var importer = asset as AssetImporter;
			if (importer != null)
			{
				asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(importer.assetPath);
			}
#if UNITY_2018_2_OR_NEWER
			var source = PrefabUtility.GetCorrespondingObjectFromSource(asset);
			if (source != null)
			{
				asset = source;
			}
#elif UNITY_2017_1_OR_NEWER
			var source = PrefabUtility.GetPrefabParent(asset);
			if (source != null)
			{
				asset = source;
			}
#endif
			return asset;
		}

		#region Private API
		private static UnityEngine.Object[] GetAssetsWhereLockIsValid(UnityEngine.Object[] assets)
		{
			return assets.Select(o => FilterAsset(o)).Where(o => GetIsLockValid(o)).ToArray();
		}

		private static UnityEngine.Object[] GetAssetsWhereRevertLockIsValid(UnityEngine.Object[] assets)
		{
			return assets.Select(o => FilterAsset(o)).Where(o => GetIsRevertLockValid(o)).ToArray();
		}

		private static UnityEngine.Object[] GetAssetsWhereFinishLockIsValid(UnityEngine.Object[] assets)
		{
			return assets.Select(o => FilterAsset(o)).Where(o => GetIsFinishLockValid(o)).ToArray();
		}

		private static bool IsAssetLocked(UnityEngine.Object asset)
		{
			if (!HasFetched)
			{
				return true;
			}
			return sm_assetsLockedByMe.Contains(asset) || sm_assetsLockedBySomeoneElse.Contains(asset);
		}

		private static bool AreAssetsLockedByMe(UnityEngine.Object[] assets, out int fautlyIndex)
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

		private static bool IsAnyAssetLocked(UnityEngine.Object[] assets, out int lockedIndex)
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

		private static void FetchLockedAssetsAsync(string url, Action<string> onComplete, Action<string> onError)
		{
#if UNITY_2018_4_OR_NEWER

			var webRequest = UnityWebRequest.Get(url);
			Container.GetWebRequestManager().WaitForWebRequest(webRequest, () =>
			{
				ErrorMessage = string.Empty;
				if (onComplete != null)
				{
					onComplete(webRequest.downloadHandler.text);
				}
			}, (error) =>
			{
				ErrorMessage = error;
				if (onError != null)
				{
					onError(error);
				}
			});
#else
			var www = new WWW(url);
			Container.GetWWWManager().WaitForWWW(www, () =>
			{
				ErrorMessage = string.Empty;
				if (onComplete != null)
				{
					onComplete(www.text);
				}
			}, (error) =>
			{
				ErrorMessage = error;
				if (onError != null)
				{
					onError(error);
				}
			});
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

		private static void FetchAssetHistoryAsync(string url, UnityEngine.Object asset, Action<string> onComplete, Action<string> onError)
		{
#if UNITY_2018_4_OR_NEWER
			var form = new WWWForm();
			form.AddField("Guid", JsonConvert.SerializeObject(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset))));
			var webRequest = UnityWebRequest.Post(url, form);
			Container.GetWebRequestManager().WaitForWebRequest(webRequest, () =>
			{
				ErrorMessage = string.Empty;
				if (onComplete != null)
				{
					onComplete(webRequest.downloadHandler.text);
				}
			}, (error) =>
			{
				ErrorMessage = error;
				if (onError != null)
				{
					onError(error);
				}
			});
#else
			var form = new WWWForm();
			form.AddField("Guid", JsonConvert.SerializeObject(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset))));
			var www = new WWW(url, form);
			Container.GetWWWManager().WaitForWWW(www, () =>
			{
				ErrorMessage = string.Empty;
				if (onComplete != null)
				{
					onComplete(www.text);
				}
			}, (error) =>
			{
				ErrorMessage = error;
				if (onError != null)
				{
					onError(error);
				}
			});
#endif
		}

		private static void ClearLocks(string url, Action onComplete, Action<string> onError)
		{
#if UNITY_2018_4_OR_NEWER
			var form = new WWWForm();
			var webRequest = UnityWebRequest.Post(url, form);
			Container.GetWebRequestManager().WaitForWebRequest(webRequest, onComplete, onError);
#else
			var form = new WWWForm();
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

		private static bool IsAssetLockedNowButUnlockedAtLaterCommitThurough(UnityEngine.Object asset)
		{
			if (!HasFetched)
			{
				return true;
			}
			LockedAssetsData.AssetLockData lockData;
			if (sm_lockedAssets.TryGetValue(asset, out lockData))
			{
				return !lockData.Locked && !string.IsNullOrEmpty(lockData.UnlockSha) && !Container.GetVersionControlHandler().IsCommitChildOfHead(lockData.UnlockSha) && lockData.LockerName != Container.GetLockSettings().Username;
			}
			return false;
		}
		#endregion
	}
}