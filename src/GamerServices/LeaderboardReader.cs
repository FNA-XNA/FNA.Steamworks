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
			get;
			private set;
		}

		public bool CanPageUp
		{
			get;
			private set;
		}

		public ReadOnlyCollection<LeaderboardEntry> Entries
		{
			get;
			private set;
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

			public LeaderboardReaderAction(object state, AsyncCallback callback)
			{
				AsyncState = state;
				Callback = callback;
				IsCompleted = false;
				AsyncWaitHandle = new ManualResetEvent(true);
			}
		}

		#endregion

		#region Internal Constructor

		internal LeaderboardReader()
		{
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
			result.AsyncWaitHandle.WaitOne();
			EndPageDown(result);
		}

		public IAsyncResult BeginPageDown(
			AsyncCallback callback,
			object asyncState
		) {
			// TODO: Actual stuff?! -flibit
			return new LeaderboardReaderAction(asyncState, callback);
		}

		public void EndPageDown(IAsyncResult result)
		{
			// TODO: Actual stuff?! -flibit
		}

		public void PageUp()
		{
			IAsyncResult result = BeginPageUp(null, null);
			result.AsyncWaitHandle.WaitOne();
			EndPageUp(result);
		}

		public IAsyncResult BeginPageUp(
			AsyncCallback callback,
			object asyncState
		) {
			// TODO: Actual stuff?! -flibit
			return new LeaderboardReaderAction(asyncState, callback);
		}

		public void EndPageUp(IAsyncResult result)
		{
			// TODO: Actual stuff?! -flibit
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
			result.AsyncWaitHandle.WaitOne();
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
			result.AsyncWaitHandle.WaitOne();
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
			result.AsyncWaitHandle.WaitOne();
			return EndRead(result);
		}

		public static IAsyncResult BeginRead(
			LeaderboardIdentity leaderboardId,
			int pageStart,
			int pageSize,
			AsyncCallback callback,
			object asyncState
		) {
			// TODO: Actual stuff?! -flibit
			return new LeaderboardReaderAction(asyncState, callback);
		}

		public static IAsyncResult BeginRead(
			LeaderboardIdentity leaderboardId,
			Gamer pivotGamer,
			int pageSize,
			AsyncCallback callback,
			object asyncState
		) {
			// TODO: Actual stuff?! -flibit
			return new LeaderboardReaderAction(asyncState, callback);
		}

		public static IAsyncResult BeginRead(
			LeaderboardIdentity leaderboardId,
			IEnumerable<Gamer> gamers,
			Gamer pivotGamer,
			int pageSize,
			AsyncCallback callback,
			object asyncState
		) {
			// TODO: Actual stuff?! -flibit
			return new LeaderboardReaderAction(asyncState, callback);
		}

		public static LeaderboardReader EndRead(IAsyncResult result)
		{
			// TODO: Actual stuff?! -flibit
			return null;
		}

		#endregion
	}
}
