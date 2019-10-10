using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
			public string UnlockSha;

			public AssetLockData(string guid, string lockerName)
			{
				Guid = guid;
				LockerName = lockerName;
				UnlockSha = null;
			}

			public AssetLockData(string guid, string lockerName, string unlockSha)
			{
				Guid = guid;
				LockerName = lockerName;
				UnlockSha = unlockSha;
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
					Debug.LogError("Asset with GUID " + data.Guid + ", locked by " + data.LockerName + ", was not found");
					continue;
				}
				var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
				LockData[asset] = data;
			}
		}
	}
}