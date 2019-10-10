namespace Alf.UnityLocker.Editor
{
	public interface IVersionControlHandler
	{
		bool IsCommitChildOfHead(string sha);
		string GetShaOfHead();
	}
}