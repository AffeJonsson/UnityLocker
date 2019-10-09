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
			var repoPath = ULLockSettingsHelper.Settings.GitRepoPath;
			if (string.IsNullOrEmpty(repoPath))
			{
				return false;
			}

			using (var repo = new Repository(repoPath))
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
			var repoPath = ULLockSettingsHelper.Settings.GitRepoPath;
			if (string.IsNullOrEmpty(repoPath))
			{
				return string.Empty;
			}

			var sha = string.Empty;
			using (var repo = new Repository(repoPath))
			{
				sha = repo.Head.Tip.Sha;
			}
			return sha;
		}
	}
}