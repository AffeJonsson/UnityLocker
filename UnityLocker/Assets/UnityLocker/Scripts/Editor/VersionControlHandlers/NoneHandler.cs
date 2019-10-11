namespace Alf.UnityLocker.Editor.VersionControlHandlers
{
	[VersionControlHandler("None")]
	public class NoneHandler : IVersionControlHandler
	{
		public string GetShaOfHead()
		{
			return string.Empty;
		}

		public bool IsCommitChildOfHead(string sha)
		{
			return true;
		}
	}
}