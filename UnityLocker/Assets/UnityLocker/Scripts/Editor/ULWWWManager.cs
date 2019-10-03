using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	public static class ULWWWManager
	{
		private struct WWWAction
		{
			public WWW Www;
			public Action OnComplete;

			public WWWAction(WWW www, Action onComplete)
			{
				Www = www;
				OnComplete = onComplete;
			}
		}

		private static List<WWWAction> sm_wwwActions = new List<WWWAction>(4);

		public static void WaitForWWW(WWW www, Action onComplete)
		{
			if (sm_wwwActions.Count == 0)
			{
				EditorApplication.update += Update;
			}
			sm_wwwActions.Add(new WWWAction(www, onComplete));
		}

		private static void Update()
		{
			for (var i = sm_wwwActions.Count - 1; i >= 0; i--)
			{
				var wwwAction = sm_wwwActions[i];
				if (wwwAction.Www.isDone)
				{
					sm_wwwActions.RemoveAt(i);
					if (sm_wwwActions.Count == 0)
					{
						EditorApplication.update -= Update;
					}
					wwwAction.OnComplete();
				}
			}
		}
	}
}