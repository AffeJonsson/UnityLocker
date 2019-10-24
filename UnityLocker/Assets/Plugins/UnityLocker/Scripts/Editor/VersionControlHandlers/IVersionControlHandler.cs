namespace Alf.UnityLocker.Editor.VersionControlHandlers
{
	public interface IVersionControlHandler
	{
		bool IsCommitChildOfHead(string sha);
		string GetShaOfHead();
	}
}