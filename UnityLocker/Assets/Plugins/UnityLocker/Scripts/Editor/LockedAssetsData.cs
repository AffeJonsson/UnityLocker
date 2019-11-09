using System;
using System.Collections.Generic;
using UnityEditor;

namespace Alf.UnityLocker.Editor
{
	[Serializable]
	public sealed class LockedAssetsData
	{
		[Serializable]
		public struct AssetLockData
		{
			public string Guid;
			public string LockerName;
			public bool Locked;
			public string UnlockSha;

			public AssetLockData(string guid, string lockerName)
			{
				Guid = guid;
				LockerName = lockerName;
				Locked = true;
				UnlockSha = null;
			}

			public AssetLockData(string guid, string lockerName, bool locked, string unlockSha)
			{
				Guid = guid;
				LockerName = lockerName;
				Locked = locked;
				UnlockSha = unlockSha;
			}
		}

		public readonly Dictionary<UnityEngine.Object, AssetLockData> LockData;
		public readonly Dictionary<int, AssetLockData> LockDataInstanceId;
		public readonly Dictionary<string, AssetLockData> LockDataGuid;

		public LockedAssetsData(AssetLockData[] rawLockData)
		{
			if (LockData == null)
			{
				LockData = new Dictionary<UnityEngine.Object, AssetLockData>(rawLockData.Length);
				LockDataInstanceId = new Dictionary<int, AssetLockData>(rawLockData.Length);
				LockDataGuid = new Dictionary<string, AssetLockData>(rawLockData.Length);
			}
			else
			{
				LockData.Clear();
				LockDataInstanceId.Clear();
				LockDataGuid.Clear();
			}
			foreach (var data in rawLockData)
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(data.Guid);
				if (string.IsNullOrEmpty(assetPath))
				{
					continue;
				}
				var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
				if (asset != null)
				{
					LockData[asset] = data;
					LockDataInstanceId[asset.GetInstanceID()] = data;
					LockDataGuid[data.Guid] = data;
				}
			}
		}
	}
}