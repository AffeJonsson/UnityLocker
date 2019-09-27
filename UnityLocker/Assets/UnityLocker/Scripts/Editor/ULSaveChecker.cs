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
			var pathsToSave = new List<string>(paths.Length);

			for (var i = 0; i < paths.Length; i++)
			{
				var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(paths[i]);
				if (ULLocker.IsAssetLockedBySomeoneElse(asset))
				{
					Debug.LogError("Tried to save asset " + asset + " at path " + paths[i] + ", but it's locked!");
				}
				else
				{
					pathsToSave.Add(paths[i]);
				}
			}

			// If all assets can be saved, no need to create a new array.
			if (pathsToSave.Count == paths.Length)
			{
				return paths;
			}

			var arr = new string[pathsToSave.Count];
			for (var i = 0; i < pathsToSave.Count; i++)
			{
				arr[i] = pathsToSave[i];
			}

			return arr;
		}
	}
}