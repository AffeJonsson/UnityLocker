using Alf.UnityLocker.Editor.AssetTypeValidators;
using Alf.UnityLocker.Editor.VersionControlHandlers;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	[InitializeOnLoad]
	public static class Container
	{
		private static readonly Dictionary<string, IVersionControlHandler> sm_versionControlHandlers;
		private static readonly AssetTypeValidatorCollection sm_assetTypeValidators;
		private static readonly LockSettings sm_lockSettings;
#if UNITY_2018_4_OR_NEWER
		private static readonly WebRequestManager sm_webRequestManager;
#else
		private static readonly WWWManager sm_wWWManager;
#endif

		static Container()
		{
			var assetGuids = AssetDatabase.FindAssets("t:" + nameof(LockSettings));
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
				if (!AssetDatabase.IsValidFolder("Assets/Plugins"))
				{
					AssetDatabase.CreateFolder("Assets", "Plugins");
				}
				if (!AssetDatabase.IsValidFolder("Assets/Plugins/UnityLocker"))
				{
					AssetDatabase.CreateFolder("Assets/Plugins", "UnityLocker");
				}
				if (!AssetDatabase.IsValidFolder("Assets/Plugins/UnityLocker/Assets"))
				{
					AssetDatabase.CreateFolder("Assets/Plugins/UnityLocker", "Assets");
				}
				AssetDatabase.CreateAsset(sm_lockSettings, "Assets/Plugins/UnityLocker/Assets/LockSettings.asset");
			}

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			if (sm_lockSettings.IsSetUp)
			{
				sm_versionControlHandlers = new Dictionary<string, IVersionControlHandler>(4);
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

			sm_assetTypeValidators = new AssetTypeValidatorCollection();
			for (var i = 0; i < assemblies.Length; i++)
			{
				var types = assemblies[i].GetTypes();
				for (var j = 0; j < types.Length; j++)
				{
					var atvAttribute = types[j].GetCustomAttribute<AssetTypeValidatorAttribute>();
					if (atvAttribute != null)
					{
						sm_assetTypeValidators.Add((IAssetTypeValidator)Activator.CreateInstance(types[j]), atvAttribute.Flag);
					}
				}
			}

#if UNITY_2018_4_OR_NEWER
			sm_webRequestManager = new WebRequestManager();
#else
			sm_wWWManager = new WWWManager();
#endif
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


#if UNITY_2018_4_OR_NEWER
		public static WebRequestManager GetWebRequestManager()
		{
			return sm_webRequestManager;
		}
#else
		public static WWWManager GetWWWManager()
		{
			return sm_wWWManager;
		}
#endif

		public static AssetTypeValidatorCollection GetAssetTypeValidators()
		{
			return sm_assetTypeValidators;
		}
	}
}