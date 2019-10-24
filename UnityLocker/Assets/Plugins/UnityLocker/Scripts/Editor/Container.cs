using Alf.UnityLocker.Editor.VersionControlHandlers;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	public static class Container
	{
		private static readonly Dictionary<string, IVersionControlHandler> sm_versionControlHandlers;
		private static readonly LockSettings sm_lockSettings;
		private static readonly WWWManager sm_wWWManager;

		static Container()
		{
			var assetGuids = AssetDatabase.FindAssets("t:" + nameof(UnityLocker.LockSettings));
			for (var i = 0; i < assetGuids.Length; i++)
			{
				var asset = AssetDatabase.LoadAssetAtPath<LockSettings>(AssetDatabase.GUIDToAssetPath(assetGuids[i]));
				if (asset != null)
				{
					sm_lockSettings = asset;
					break;
				}
			}

			if (sm_lockSettings == null)
			{
				sm_lockSettings = ScriptableObject.CreateInstance<LockSettings>();
				if (!AssetDatabase.IsValidFolder("Assets/UnityLocker"))
				{
					AssetDatabase.CreateFolder("Assets", "UnityLocker");
				}
				if (!AssetDatabase.IsValidFolder("Assets/UnityLocker/Assets"))
				{
					AssetDatabase.CreateFolder("Assets/UnityLocker", "Assets");
				}
				AssetDatabase.CreateAsset(sm_lockSettings, "Assets/UnityLocker/Assets/LockSettings.asset");
			}

			if (sm_lockSettings.IsSetUp)
			{
				sm_versionControlHandlers = new Dictionary<string, IVersionControlHandler>(4);
				var assemblies = AppDomain.CurrentDomain.GetAssemblies();
				for (var i = 0; i < assemblies.Length; i++)
				{
					var types = assemblies[i].GetTypes();
					for (var j = 0; j < types.Length; j++)
					{
						var vcAttribute = types[j].GetCustomAttribute<VersionControlHandlerAttribute>();
						if (vcAttribute != null)
						{
							sm_versionControlHandlers.Add(vcAttribute.Name, (IVersionControlHandler)Activator.CreateInstance(types[j]));
						}
					}
				}
			}
			else
			{
				Debug.LogError("Lock Settings has not been correctly configured");
			}

			sm_wWWManager = new WWWManager();
		}

		public static IVersionControlHandler GetVersionControlHandler()
		{
			IVersionControlHandler handler;
			if (sm_versionControlHandlers.TryGetValue(sm_lockSettings.VersionControlName, out handler))
			{
				return handler;
			}
			return null;
		}

		public static LockSettings GetLockSettings()
		{
			return sm_lockSettings;
		}

		public static WWWManager GetWWWManager()
		{
			return sm_wWWManager;
		}
	}
}