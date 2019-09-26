using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	public class ULLockData
	{
		public Dictionary<UnityEngine.Object, ULUser> LockData;

		public ULLockData(Dictionary<string, string> rawLockData)
		{
			Debug.Log("Create ULLockData");
			LockData = new Dictionary<UnityEngine.Object, ULUser>(rawLockData.Count);
			foreach (var data in rawLockData)
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(data.Key);
				if (string.IsNullOrEmpty(assetPath))
				{
					Debug.LogError("Asset with GUID " + data.Key + ", locked by " + data.Value + ", was not found");
					continue;
				}
				var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
				// TODO: Fetch user instead of creating a new one
				LockData[asset] = new ULUser(data.Value);
			}
		}
	}
}