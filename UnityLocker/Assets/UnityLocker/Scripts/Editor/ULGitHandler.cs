using LibGit2Sharp;
using System;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	public static class ULGitHandler
	{
		private static Commit sm_currentCommit;

		static ULGitHandler()
		{
			using (var repo = new Repository(@"D:\Git\Repos\UnityLocker"))
			{
				var commit = repo.Head.Tip;
			}
		}
	}
}