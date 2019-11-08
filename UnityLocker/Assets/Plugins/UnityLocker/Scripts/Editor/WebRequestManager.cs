using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Alf.UnityLocker.Editor
{
	public sealed class WebRequestManager
	{
		private struct WebRequestAction
		{
			public UnityWebRequestAsyncOperation WebRequestOperation;
			public Action OnComplete;

			public WebRequestAction(UnityWebRequestAsyncOperation webRequestOperation, Action onComplete)
			{
				WebRequestOperation = webRequestOperation;
				OnComplete = onComplete;
			}
		}

		private List<WebRequestAction> sm_webRequestActions = new List<WebRequestAction>(4);

		public void WaitForWebRequest(UnityWebRequest webRequest, Action onComplete)
		{
			if (sm_webRequestActions.Count == 0)
			{
				EditorApplication.update += Update;
			}
			sm_webRequestActions.Add(new WebRequestAction(webRequest.SendWebRequest(), onComplete));
		}

		private void Update()
		{
			for (var i = sm_webRequestActions.Count - 1; i >= 0; i--)
			{
				var webRequestAction = sm_webRequestActions[i];
				if (webRequestAction.WebRequestOperation.isDone)
				{
					sm_webRequestActions.RemoveAt(i);
					if (sm_webRequestActions.Count == 0)
					{
						EditorApplication.update -= Update;
					}
					webRequestAction.OnComplete();
				}
			}
		}
	}
}