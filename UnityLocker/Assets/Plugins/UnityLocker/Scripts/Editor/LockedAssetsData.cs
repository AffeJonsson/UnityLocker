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
			public DateTime Date;

			public AssetLockData(string guid, string lockerName, bool locked, string unlockSha, DateTime date)
			{
				Guid = guid;
				LockerName = lockerName;
				Locked = locked;
				UnlockSha = unlockSha;
				Date = date;
			}
		}

		public readonly Dictionary<UnityEngine.Object, AssetLockData> LockData;

		public LockedAssetsData(AssetLockData[] rawLockData)
		{
			if (LockData == null)
			{
				LockData = new Dictionary<UnityEngine.Object, AssetLockData>(rawLockData.Length);
			}
			else
			{
				LockData.Clear();
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
				}
			}
		}
	}
}