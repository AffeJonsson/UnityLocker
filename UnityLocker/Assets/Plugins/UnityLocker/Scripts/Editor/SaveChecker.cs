using System;
using System.Collections.Generic;
using UnityEditor;

namespace Alf.UnityLocker.Editor
{
	public class SaveChecker : UnityEditor.AssetModificationProcessor
	{
		private static string[] OnWillSaveAssets(string[] paths)
		{
			if (!Container.GetLockSettings().IsEnabled)
			{
				return paths;
			}
			if (paths.Length == 0)
			{
				return paths;
			}
			if (!Locker.HasFetched)
			{
				for (var i = 0; i < paths.Length; i++)
				{
					if (paths[i] == AssetDatabase.GetAssetPath(Container.GetLockSettings()))
					{
						return new[] { paths[i] };
					}
				}
				EditorUtility.DisplayDialog("Could not save assets", "The UnityLocker plugin has not yet fetched locket assets from the server.\nMake sure the LockSettings asset is configured correctly and that the server is up and running.", "Ok");
				return new string[0];
			}

			var pathsLocked = string.Empty;
			var indexesLocked = new List<int>(8);

			for (var i = 0; i < paths.Length; i++)
			{
				var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(paths[i]);
				var isAssetLockedNowButUnlockedAtLaterCommit = Locker.IsAssetLockedNowButUnlockedAtLaterCommit(asset);
				if (asset != null && (Locker.IsAssetLockedBySomeoneElse(asset) || isAssetLockedNowButUnlockedAtLaterCommit))
				{
					if (isAssetLockedNowButUnlockedAtLaterCommit)
					{
						pathsLocked += paths[i] + " (Locked by " + Locker.GetAssetLocker(asset) + ", unlocks at commit " + Locker.GetAssetUnlockCommitShaShort(asset) + ")\n";
					}
					else
					{
						pathsLocked += paths[i] + " (Locked by " + Locker.GetAssetLocker(asset) + ")\n";
					}
					indexesLocked.Add(i);
				}
			}

			// If all assets can be saved, no need to create a new array.
			if (indexesLocked.Count == 0)
			{
				return paths;
			}

			if (paths.Length - indexesLocked.Count > 0)
			{
				if (EditorUtility.DisplayDialog("Cannot save some assets", "The following assets are locked:\n" + pathsLocked + "Do you want to save the other assets?", "Yes", "No"))
				{
					var arr = new string[paths.Length - indexesLocked.Count];
					var arrIndex = 0;
					for (var i = 0; i < paths.Length; i++)
					{
						if (!indexesLocked.Contains(i))
						{
							arr[arrIndex++] = paths[i];
						}
					}

					return arr;
				}
			}
			else
			{
				EditorUtility.DisplayDialog("Cannot save assets", "The following assets are locked:\n" + pathsLocked, "Ok");
			}
			return new string[0];
		}
	}
}