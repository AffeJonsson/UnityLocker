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
			public Action<string> OnError;

			public WWWAction(WWW www, Action onComplete, Action<string> onError)
			{
				Www = www;
				OnComplete = onComplete;
				OnError = onError;
			}
		}

		private List<WWWAction> sm_wwwActions = new List<WWWAction>(4);

		public void WaitForWWW(WWW www, Action onComplete, Action<string> onError)
		{
			if (sm_wwwActions.Count == 0)
			{
				EditorApplication.update += Update;
			}
			sm_wwwActions.Add(new WWWAction(www, onComplete, onError));
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
					if (string.IsNullOrEmpty(wwwAction.Www.error))
					{
						if (wwwAction.OnComplete != null)
						{
							wwwAction.OnComplete();
						}
					}
					else
					{
						if (wwwAction.OnError != null)
						{
							wwwAction.OnError(wwwAction.Www.error);
						}
					}
				}
			}
		}
	}
}
#endif