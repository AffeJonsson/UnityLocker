using LibGit2Sharp;
using System;
using UnityEngine;

namespace Alf.UnityLocker.Editor
{
	public static class ULGitHandler
	{
		private const int MaxCommitBacktrack = 500;

		public static bool IsCommitChildOfHead(string sha)
		{
			using (var repo = new Repository(@"D:\Git\Repos\UnityLocker"))
			{
				var count = 0;
				foreach (var commit in repo.Commits)
				{
					if (count++ > MaxCommitBacktrack)
					{
						return false;
					}
					if (commit.Sha == sha)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static string GetShaOfHead()
		{
			var sha = string.Empty;
			using (var repo = new Repository(@"D:\Git\Repos\UnityLocker"))
			{
				sha = repo.Head.Tip.Sha;
			}
			return sha;
		}
	}
}