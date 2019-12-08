using System;

namespace Alf.UnityLocker.Editor
{
	[Serializable]
	public class AssetHistory
	{
		[Serializable]
		public class AssetHistoryData
		{
			public string Guid;
			public string LockerName;
			public bool Locked;
			public string UnlockSha;
			public DateTime Date;
		}

		public readonly AssetHistoryData[] AssetHistoryDatas;

		public AssetHistory(AssetHistoryData[] assetHistory)
		{
			AssetHistoryDatas = assetHistory;
		}
	}
}