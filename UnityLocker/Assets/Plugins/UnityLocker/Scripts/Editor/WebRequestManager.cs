#if UNITY_2018_4_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Networking;

namespace Alf.UnityLocker.Editor
{
	public sealed class WebRequestManager
	{
		private struct WebRequestAction
		{
			public UnityWebRequestAsyncOperation WebRequestOperation;
			public Action OnComplete;
			public Action<string> OnError;

			public WebRequestAction(UnityWebRequestAsyncOperation webRequestOperation, Action onComplete, Action<string> onError)
			{
				WebRequestOperation = webRequestOperation;
				OnComplete = onComplete;
				OnError = onError;
			}
		}

		private List<WebRequestAction> sm_webRequestActions = new List<WebRequestAction>(4);

		public void WaitForWebRequest(UnityWebRequest webRequest, Action onComplete, Action<string> onError)
		{
			if (sm_webRequestActions.Count == 0)
			{
				EditorApplication.update += Update;
			}
			sm_webRequestActions.Add(new WebRequestAction(webRequest.SendWebRequest(), onComplete, onError));
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
#if UNITY_2020_2_OR_NEWER
					if (webRequestAction.WebRequestOperation.webRequest.result == UnityWebRequest.Result.ProtocolError || webRequestAction.WebRequestOperation.webRequest.result == UnityWebRequest.Result.ConnectionError)
#else
					if (webRequestAction.WebRequestOperation.webRequest.isHttpError || webRequestAction.WebRequestOperation.webRequest.isNetworkError)
#endif
					{
						if (webRequestAction.OnError != null)
						{
							webRequestAction.OnError(webRequestAction.WebRequestOperation.webRequest.error);
						}
					}
					else
					{
						if (webRequestAction.OnComplete != null)
						{
							webRequestAction.OnComplete();
						}
					}
				}
			}
		}
	}
}
#endif