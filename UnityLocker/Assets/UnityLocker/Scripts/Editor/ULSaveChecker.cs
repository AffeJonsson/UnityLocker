using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	public class ULSaveChecker : UnityEditor.AssetModificationProcessor
	{
		private static string[] OnWillSaveAssets(string[] paths)
		{
			var pathsLocked = string.Empty;
			var indexesLocked = new List<int>(8);

			for (var i = 0; i < paths.Length; i++)
			{
				var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(paths[i]);
				if (ULLocker.IsAssetLockedBySomeoneElse(asset))
				{
					pathsLocked += paths[i] + "\n";
					indexesLocked.Add(i);
				}
			}

			// If all assets can be saved, no need to create a new array.
			if (indexesLocked.Count == 0)
			{
				return paths;
			}

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
			return new string[0];
		}
	}
}