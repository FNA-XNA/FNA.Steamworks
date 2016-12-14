#region License
/* FNA.Steamworks - XNA4 Xbox Live Reimplementation for Steamworks
 * Copyright 2016 Ethan "flibitijibibo" Lee
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

using Steamworks;
#endregion

namespace Microsoft.Xna.Framework.GamerServices
{
	public sealed class LeaderboardReader : IDisposable
	{
		#region Public Properties

		public bool IsDisposed
		{
			get;
			private set;
		}

		public bool CanPageDown
		{
			get
			{
				if (entryCache.Count == 0)
				{
					return false;
				}
				if (isFriendBoard)
				{
					return (PageStart + pageSize) < entryCache.Count;
				}
				return (	PageStart < entryCache.Count ||
						entryCache[entryCache.Count - 1].RankingEXT < TotalLeaderboardSize	);
			}
		}

		public bool CanPageUp
		{
			get
			{
				if (entryCache.Count == 0)
				{
					return false;
				}
				if (isFriendBoard)
				{
					return (PageStart - pageSize) >= 0;
				}
				return (	PageStart > 0 ||
						entryCache[0].RankingEXT > 1	);
			}
		}

		public ReadOnlyCollection<LeaderboardEntry> Entries
		{
			get
			{
				return new ReadOnlyCollection<LeaderboardEntry>(entries);
			}
		}

		public LeaderboardIdentity LeaderboardIdentity
		{
			get;
			private set;
		}

		public int PageStart
		{
			get;
			private set;
		}

		public int TotalLeaderboardSize
		{
			get;
			private set;
		}

		#endregion

		#region Private Variables

		private int pageSize;
		private SteamLeaderboard_t leaderboard;
		private List<LeaderboardEntry> entries;
		private List<LeaderboardEntry> entryCache;
		private bool isFriendBoard;

		#endregion

		#region Internal Static Variables

		internal static readonly Dictionary<string, SteamLeaderboard_t> Leaderboards =
			new Dictionary<string, SteamLeaderboard_t>();

		#endregion

		#region Private Static Variables

		private static LeaderboardReaderAction readAction;

		#endregion

		#region Leaderboard Entry Gamer Container

		internal class LeaderboardGamer : Gamer
		{
			public LeaderboardGamer(CSteamID id) : base(
				id,
				SteamFriends.GetFriendPersonaName(id),
				SteamFriends.GetPlayerNickname(id)
			) {
			}
		}

		#endregion

		#region Async Object Type

		internal class LeaderboardReaderAction : IAsyncResult
		{
			public object AsyncState
			{
				get;
				private set;
			}

			public bool CompletedSynchronously
			{
				get
				{
					return false;
				}
			}

			public bool IsCompleted
			{
				get;
				internal set;
			}

			public WaitHandle AsyncWaitHandle
			{
				get;
				private set;
			}

			public readonly AsyncCallback Callback;

			public readonly LeaderboardIdentity ID;
			public readonly int PageSize;
			public readonly Gamer PivotGamer;
			public readonly IEnumerable<Gamer> Gamers;

			public int PageStart;

			public SteamLeaderboard_t Leaderboard;
			public List<LeaderboardEntry> Entries;

			public LeaderboardReaderAction(
				object state,
				AsyncCallback callback,
				LeaderboardIdentity identity,
				int start,
				int size,
				Gamer pivot,
				IEnumerable<Gamer> gamers
			) {
				AsyncState = state;
				Callback = callback;
				IsCompleted = false;
				AsyncWaitHandle = new ManualResetEvent(true);

				ID = identity;
				PageStart = start;
				PageSize = size;
				PivotGamer = pivot;
				Gamers = gamers;

				Leaderboards.TryGetValue(ID.Key, out Leaderboard);
			}
		}

		#endregion

		#region Internal Constructor

		internal LeaderboardReader(
			LeaderboardIdentity identity,
			int start,
			int size,
			SteamLeaderboard_t board,
			List<LeaderboardEntry> entries,
			bool friends
		) {
			LeaderboardIdentity = identity;
			PageStart = start;
			pageSize = size;
			leaderboard = board;
			TotalLeaderboardSize = SteamUserStats.GetLeaderboardEntryCount(leaderboard);
			isFriendBoard = friends;

			entryCache = entries;
			this.entries = new List<LeaderboardEntry>(pageSize);
			for (int i = PageStart; i < pageSize && i < entryCache.Count; i += 1)
			{
				this.entries.Add(entryCache[i]);
			}

			IsDisposed = false;
		}

		#endregion

		#region Public Methods

		public void Dispose()
		{
			IsDisposed = true;
		}

		public void PageDown()
		{
			IAsyncResult result = BeginPageDown(null, null);
			while (!result.IsCompleted)
			{
				SteamAPI.RunCallbacks();
			}
			EndPageDown(result);
		}

		public IAsyncResult BeginPageDown(
			AsyncCallback callback,
			object asyncState
		) {
			readAction = new LeaderboardReaderAction(
				asyncState,
				callback,
				LeaderboardIdentity,
				PageStart,
				pageSize,
				null,
				null
			);

			if (PageStart + pageSize * 2 <= entryCache.Count)
			{
				readAction.IsCompleted = true;
			}
			else if (!CanPageDown)
			{
				readAction.IsCompleted = true;

				// Cannot increment page start, bail immediately
				return readAction;
			}
			else if (isFriendBoard)
			{
				// Friend max is 100, no need to download ever
				readAction.IsCompleted = true;
			}
			else
			{
				SteamAPICall_t call = SteamUserStats.DownloadLeaderboardEntries(
					Leaderboards[LeaderboardIdentity.Key],
					ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal,
					entryCache[entryCache.Count - 1].RankingEXT + 1,
					entryCache[entryCache.Count - 1].RankingEXT + pageSize
				);
				if (call.m_SteamAPICall != 0)
				{
					CallResult<LeaderboardScoresDownloaded_t> scoresDownloaded;
					scoresDownloaded = new CallResult<LeaderboardScoresDownloaded_t>();
					scoresDownloaded.Set(
						call,
						DoPageDown
					);
				}
			}
			readAction.PageStart += pageSize;
			return readAction;
		}

		public void EndPageDown(IAsyncResult result)
		{
			PageStart += readAction.PageStart;

			entries = new List<LeaderboardEntry>(pageSize);
			for (int i = PageStart; i < pageSize && i < entryCache.Count; i += 1)
			{
				entries.Add(entryCache[i]);
			}
		}

		public void PageUp()
		{
			IAsyncResult result = BeginPageUp(null, null);
			while (!result.IsCompleted)
			{
				SteamAPI.RunCallbacks();
			}
			EndPageUp(result);
		}

		public IAsyncResult BeginPageUp(
			AsyncCallback callback,
			object asyncState
		) {
			readAction = new LeaderboardReaderAction(
				asyncState,
				callback,
				LeaderboardIdentity,
				PageStart,
				pageSize,
				null,
				null
			);

			if (PageStart >= pageSize)
			{
				readAction.PageStart -= pageSize;
				readAction.IsCompleted = true;
			}
			else if (!CanPageUp)
			{
				readAction.IsCompleted = true;
			}
			else
			{
				SteamAPICall_t call = SteamUserStats.DownloadLeaderboardEntries(
					Leaderboards[LeaderboardIdentity.Key],
					ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal,
					entryCache[0].RankingEXT - pageSize,
					entryCache[0].RankingEXT - 1
				);
				if (call.m_SteamAPICall != 0)
				{
					CallResult<LeaderboardScoresDownloaded_t> scoresDownloaded;
					scoresDownloaded = new CallResult<LeaderboardScoresDownloaded_t>();
					scoresDownloaded.Set(
						call,
						DoPageUp
					);
				}
			}

			return readAction;
		}

		public void EndPageUp(IAsyncResult result)
		{
			PageStart = readAction.PageStart;

			entries = new List<LeaderboardEntry>(pageSize);
			for (int i = PageStart; i < pageSize && i < entryCache.Count; i += 1)
			{
				entries.Add(entryCache[i]);
			}
		}

		#endregion

		#region Private Methods

		private void DoPageDown(
			LeaderboardScoresDownloaded_t scores,
			bool bioFailure
		) {
			if (scores.m_hSteamLeaderboardEntries.m_SteamLeaderboardEntries != 0)
			{
				LeaderboardEntry_t entry;
				for (int i = 0; i < scores.m_cEntryCount; i += 1)
				{
					SteamUserStats.GetDownloadedLeaderboardEntry(
						scores.m_hSteamLeaderboardEntries,
						i,
						out entry,
						null,
						0
					);
					entryCache.Add(
						new LeaderboardEntry(
							new LeaderboardGamer(entry.m_steamIDUser),
							entry.m_nScore,
							entry.m_nGlobalRank,
							readAction.Leaderboard
						)
					);
				}
			}
			readAction.IsCompleted = true;
		}

		private void DoPageUp(
			LeaderboardScoresDownloaded_t scores,
			bool bioFailure
		) {
			if (scores.m_hSteamLeaderboardEntries.m_SteamLeaderboardEntries != 0)
			{
				LeaderboardEntry_t entry;
				for (int i = scores.m_cEntryCount - 1; i >= 0; i -= 1)
				{
					SteamUserStats.GetDownloadedLeaderboardEntry(
						scores.m_hSteamLeaderboardEntries,
						i,
						out entry,
						null,
						0
					);
					entryCache.Insert(
						0,
						new LeaderboardEntry(
							new LeaderboardGamer(entry.m_steamIDUser),
							entry.m_nScore,
							entry.m_nGlobalRank,
							readAction.Leaderboard
						)
					);
				}
				PageStart = PageStart + scores.m_cEntryCount - pageSize;
			}
			readAction.IsCompleted = true;
		}

		#endregion

		#region Public Static Methods

		public static LeaderboardReader Read(
			LeaderboardIdentity leaderboardId,
			int pageStart,
			int pageSize
		) {
			IAsyncResult result = BeginRead(
				leaderboardId,
				pageStart,
				pageSize,
				null,
				null
			);
			while (!result.IsCompleted)
			{
				SteamAPI.RunCallbacks();
			}
			return EndRead(result);
		}

		public static LeaderboardReader Read(
			LeaderboardIdentity leaderboardId,
			Gamer pivotGamer,
			int pageSize
		) {
			IAsyncResult result = BeginRead(
				leaderboardId,
				pivotGamer,
				pageSize,
				null,
				null
			);
			while (!result.IsCompleted)
			{
				SteamAPI.RunCallbacks();
			}
			return EndRead(result);
		}

		public static LeaderboardReader Read(
			LeaderboardIdentity leaderboardId,
			IEnumerable<Gamer> gamers,
			Gamer pivotGamer,
			int pageSize
		) {
			IAsyncResult result = BeginRead(
				leaderboardId,
				gamers,
				pivotGamer,
				pageSize,
				null,
				null
			);
			while (!result.IsCompleted)
			{
				SteamAPI.RunCallbacks();
			}
			return EndRead(result);
		}

		public static IAsyncResult BeginRead(
			LeaderboardIdentity leaderboardId,
			int pageStart,
			int pageSize,
			AsyncCallback callback,
			object asyncState
		) {
			readAction = new LeaderboardReaderAction(
				asyncState,
				callback,
				leaderboardId,
				pageStart,
				pageSize,
				null,
				null
			);
			if (readAction.Leaderboard.m_SteamLeaderboard == 0)
			{
				FindLeaderboard(leaderboardId.Key);
			}
			else
			{
				DownloadEntries();
			}
			return readAction;
		}

		public static IAsyncResult BeginRead(
			LeaderboardIdentity leaderboardId,
			Gamer pivotGamer,
			int pageSize,
			AsyncCallback callback,
			object asyncState
		) {
			readAction = new LeaderboardReaderAction(
				asyncState,
				callback,
				leaderboardId,
				0,
				pageSize,
				pivotGamer,
				null
			);
			if (readAction.Leaderboard.m_SteamLeaderboard == 0)
			{
				FindLeaderboard(leaderboardId.Key);
			}
			else
			{
				DownloadEntries();
			}
			return readAction;
		}

		public static IAsyncResult BeginRead(
			LeaderboardIdentity leaderboardId,
			IEnumerable<Gamer> gamers,
			Gamer pivotGamer,
			int pageSize,
			AsyncCallback callback,
			object asyncState
		) {
			readAction = new LeaderboardReaderAction(
				asyncState,
				callback,
				leaderboardId,
				0,
				pageSize,
				pivotGamer,
				gamers
			);
			if (readAction.Leaderboard.m_SteamLeaderboard == 0)
			{
				FindLeaderboard(leaderboardId.Key);
			}
			else
			{
				DownloadEntries();
			}
			return readAction;
		}

		public static LeaderboardReader EndRead(IAsyncResult result)
		{
			LeaderboardReader reader = new LeaderboardReader(
				readAction.ID,
				readAction.PageStart,
				readAction.PageSize,
				readAction.Leaderboard,
				readAction.Entries,
				readAction.Gamers != null // FIXME
			);
			readAction = null;
			return reader;
		}

		#endregion

		#region Private Static Methods

		private static void FindLeaderboard(string key)
		{
			SteamAPICall_t call = SteamUserStats.FindLeaderboard(key);
			if (call.m_SteamAPICall != 0)
			{
				CallResult<LeaderboardFindResult_t> foundLeaderboard;
				foundLeaderboard = new CallResult<LeaderboardFindResult_t>();
				foundLeaderboard.Set(
					call,
					OnLeaderboardFound
				);
			}
		}

		private static void OnLeaderboardFound(
			LeaderboardFindResult_t board,
			bool bIOFailure
		) {
			if (!bIOFailure && board.m_bLeaderboardFound > 0)
			{
				Leaderboards.Add(readAction.ID.Key, board.m_hSteamLeaderboard);
				readAction.Leaderboard = board.m_hSteamLeaderboard;
				DownloadEntries();
			}
			else
			{
				readAction.IsCompleted = true;
			}
		}

		private static void DownloadEntries()
		{
			const int InitialCacheSize = 100;

			SteamAPICall_t result;

			if (readAction.Gamers != null)
			{
				/* FIXME: We're just going to assume "Friends" here...
				 * There is DownloadLeaderboardEntriesForUsers, but holy shit,
				 * have you seen the way they expect you to get friend gamers?
				 * -flibit
				 */
				result = SteamUserStats.DownloadLeaderboardEntries(
					readAction.Leaderboard,
					ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends,
					0,
					InitialCacheSize
				);
			}
			else if (readAction.PivotGamer == null)
			{
				result = SteamUserStats.DownloadLeaderboardEntries(
					readAction.Leaderboard,
					ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal,
					1,
					InitialCacheSize
				);
			}
			else
			{
				if (readAction.PivotGamer.steamID != SteamUser.GetSteamID())
				{
					throw new NotSupportedException(
						"Global score around user other than host"
					);
				}
				result = SteamUserStats.DownloadLeaderboardEntries(
					readAction.Leaderboard,
					ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser,
					readAction.PageStart - (InitialCacheSize / 2),
					readAction.PageStart + (InitialCacheSize / 2)
				);
			}

			if (result.m_SteamAPICall != 0)
			{
				CallResult<LeaderboardScoresDownloaded_t> downloaded;
				downloaded = new CallResult<LeaderboardScoresDownloaded_t>();
				downloaded.Set(
					result,
					OnScoresDownloaded
				);
			}
			else
			{
				readAction.IsCompleted = true;
			}
		}

		private static void OnScoresDownloaded(
			LeaderboardScoresDownloaded_t scores,
			bool bIOFailure
		) {
			if (	!bIOFailure &&
				scores.m_hSteamLeaderboardEntries.m_SteamLeaderboardEntries != 0	)
			{
				readAction.Entries = new List<LeaderboardEntry>(scores.m_cEntryCount);
				LeaderboardEntry_t entry;
				for (int i = 0; i < scores.m_cEntryCount; i += 1)
				{
					SteamUserStats.GetDownloadedLeaderboardEntry(
						scores.m_hSteamLeaderboardEntries,
						i,
						out entry,
						null,
						0
					);
					readAction.Entries.Add(
						new LeaderboardEntry(
							new LeaderboardGamer(entry.m_steamIDUser),
							entry.m_nScore,
							entry.m_nGlobalRank,
							readAction.Leaderboard
						)
					);
				}
			}
			readAction.IsCompleted = true;
		}

		#endregion
	}
}
