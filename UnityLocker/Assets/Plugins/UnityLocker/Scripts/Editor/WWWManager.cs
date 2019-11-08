#if !UNITY_2018_4_OR_NEWER

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	public sealed class WWWManager
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

		private List<WWWAction> sm_wwwActions = new List<WWWAction>(4);

		public void WaitForWWW(WWW www, Action onComplete)
		{
			if (sm_wwwActions.Count == 0)
			{
				EditorApplication.update += Update;
			}
			sm_wwwActions.Add(new WWWAction(www, onComplete));
		}

		private void Update()
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
#endif