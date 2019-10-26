using Alf.UnityLocker.Editor.AssetTypeValidators;
using Alf.UnityLocker.Editor.VersionControlHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	[CustomEditor(typeof(LockSettings))]
	public class LockSettingsEditor : UnityEditor.Editor
	{
		private static List<string> sm_versionControllers;
		private static List<string> sm_assetTypeValidatorNames;

		private void OnEnable()
		{
			if (sm_versionControllers != null && sm_assetTypeValidatorNames != null)
			{
				return;
			}
			sm_versionControllers = new List<string>(4);
			var assetValidators = new Dictionary<string, int>();
			
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (var i = 0; i < assemblies.Length; i++)
			{
				var types = assemblies[i].GetTypes();
				for (var j = 0; j < types.Length; j++)
				{
					var vcAttribute = types[j].GetCustomAttribute<VersionControlHandlerAttribute>();
					if (vcAttribute != null)
					{
						sm_versionControllers.Add(vcAttribute.Name);
					}
					var atAttribute = types[j].GetCustomAttribute<AssetTypeValidatorAttribute>();
					if (atAttribute != null)
					{
						assetValidators.Add(atAttribute.Name, atAttribute.Flag);
					}
				}
			}
			
			sm_assetTypeValidatorNames = assetValidators.Keys.OrderBy(n => assetValidators[n]).ToList();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_lockIcon"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_lockedByMeIcon"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_lockedNowButUnlockedLaterIcon"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_baseUrl"));

			if (!string.IsNullOrEmpty(serializedObject.FindProperty("m_versionControlName").stringValue))
			{
				using (var scope = new EditorGUI.ChangeCheckScope())
				{
					var parentCountProperty = serializedObject.FindProperty("m_parentFolderCount");
					EditorGUILayout.DelayedIntField(parentCountProperty);
					if (scope.changed)
					{
						if (parentCountProperty.intValue < 0)
						{
							parentCountProperty.intValue = 0;
						}
						var baseFolder = string.Empty;
						for (var i = 0; i < parentCountProperty.intValue; i++)
						{
							baseFolder += @"..\";
						}
						serializedObject.FindProperty("m_baseFolderAdditionalPath").stringValue = baseFolder;
					}
				}
				EditorGUILayout.LabelField("Repo folder: " + (target as LockSettings).RepoPath, EditorStyles.miniLabel);
			}

			if (sm_versionControllers == null || sm_versionControllers.Count == 0)
			{
				GUI.enabled = false;
				EditorGUILayout.Popup("Version Control", 0, new string[0]);
				GUI.enabled = true;
			}
			else
			{
				var property = serializedObject.FindProperty("m_versionControlName");
				var currentIndex = sm_versionControllers.IndexOf(property.stringValue);
				using (var changeCheck = new EditorGUI.ChangeCheckScope())
				{
					var newIndex = EditorGUILayout.Popup("Version Control", currentIndex, sm_versionControllers.ToArray());
					if (changeCheck.changed)
					{
						property.stringValue = sm_versionControllers[newIndex];
					}
				}
			}

			if (sm_assetTypeValidatorNames == null || sm_assetTypeValidatorNames.Count == 0)
			{
				GUI.enabled = false;
				EditorGUILayout.Popup("Valid Asset Types", 0, new string[0]);
				GUI.enabled = true;
			}
			else
			{
				var property = serializedObject.FindProperty("m_assetTypeValidators");
				var rect = EditorGUILayout.GetControlRect();
				property.intValue = EditorGUI.MaskField(rect, "Valid Asset Types", property.intValue, sm_assetTypeValidatorNames.ToArray());
			}
			

			serializedObject.ApplyModifiedProperties();
		}
	}
}