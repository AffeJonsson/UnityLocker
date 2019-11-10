using LibGit2Sharp;

namespace Alf.UnityLocker.Editor.VersionControlHandlers
{
	[VersionControlHandler("Git")]
	public class GitHandler : IVersionControlHandler
	{
		private const int MaxCommitBacktrack = 500;

		public bool IsCommitChildOfHead(string sha)
		{
			var repoPath = Container.GetLockSettings().RepoPath;
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

		public string GetShaOfHead()
		{
			var repoPath = Container.GetLockSettings().RepoPath;
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