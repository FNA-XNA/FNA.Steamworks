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
using System.Threading;
using System.Collections.Generic;

using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace Microsoft.Xna.Framework.Net
{
	public sealed class NetworkSession : IDisposable
	{
		#region Public Constants

		public const int MaxSupportedGamers = 31; // FIXME: ???
		public const int MaxPreviousGamers = 100; // FIXME: ???

		#endregion

		#region Public Properties

		public bool IsDisposed
		{
			get;
			private set;
		}

		public GamerCollection<NetworkGamer> AllGamers
		{
			get;
			private set;
		}

		public GamerCollection<LocalNetworkGamer> LocalGamers
		{
			get;
			private set;
		}

		public GamerCollection<NetworkGamer> RemoteGamers
		{
			get;
			private set;
		}

		public GamerCollection<NetworkGamer> PreviousGamers
		{
			get;
			private set;
		}

		public bool AllowHostMigration
		{
			// InvalidOperationException: This session is not the host, and cannot change the value of AllowHostMigration.
			// ObjectDisposedException: The session has been disposed.
			get;
			set;
		}

		public bool AllowJoinInProgress
		{
			// NotSupportedException: Join-in-progress is not supported for multiplayer sessions of type NetworkSessionType.Ranked.
			// InvalidOperationException: This session is not the host, and cannot change the value of AllowJoinInProgress.
			// ObjectDisposedException: The session has been disposed.
			get;
			set;
		}

		public int BytesPerSecondReceived
		{
			get;
			private set;
		}

		public int BytesPerSecondSent
		{
			get;
			private set;
		}

		public NetworkGamer Host
		{
			get;
			private set;
		}

		public bool IsEveryoneReady
		{
			get;
			private set;
		}

		public bool IsHost
		{
			get;
			private set;
		}

		public int MaxGamers
		{
			// ArgumentOutOfRangeException: MaxGamers must be between 2 and 31 players for Windows-based and Xbox 360-based games.
			// InvalidOperationException: This session is not the host, and cannot change the value of MaxGamers.
			// ObjectDisposedException: The session has been disposed.
			get;
			set;
		}

		public int PrivateGamerSlots
		{
			// ArgumentOutOfRangeException: There are not enough slots available to support this new value.
			// InvalidOperationException: This session is not the host, and cannot change the value of PrivateGamerSlots.
			// ObjectDisposedException: The session has been disposed.
			get;
			set;
		}

		public NetworkSessionProperties SessionProperties
		{
			get;
			private set;
		}

		public NetworkSessionState SessionState
		{
			get;
			private set;
		}

		public NetworkSessionType SessionType
		{
			get;
			private set;
		}

		public TimeSpan SimulatedLatency
		{
			get;
			set;
		}

		public float SimulatedPacketLoss
		{
			get;
			set;
		}

		#endregion

		#region Private Static Variables

		private static NetworkSessionAction activeAction = null;

		private static NetworkSession activeSession = null;

		#endregion

		#region Public Events

		public event EventHandler<GameStartedEventArgs> GameStarted;

		public event EventHandler<GameEndedEventArgs> GameEnded;

		public event EventHandler<GamerJoinedEventArgs> GamerJoined;

		public event EventHandler<GamerLeftEventArgs> GamerLeft;

		public event EventHandler<HostChangedEventArgs> HostChanged;

		public event EventHandler<NetworkSessionEndedEventArgs> SessionEnded;

		public event EventHandler<WriteLeaderboardsEventArgs> WriteArbitratedLeaderboard;

		public event EventHandler<WriteLeaderboardsEventArgs> WriteUnarbitratedLeaderboard;

		public event EventHandler<WriteLeaderboardsEventArgs> WriteTrueSkill;

		#endregion

		#region Public Static Events

		public static event EventHandler<InviteAcceptedEventArgs> InviteAccepted;

		#endregion

		#region Async Object Type

		internal class NetworkSessionAction : IAsyncResult
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

			public NetworkSessionAction(object state, AsyncCallback callback)
			{
				AsyncState = state;
				Callback = callback;
				IsCompleted = false;
				AsyncWaitHandle = new ManualResetEvent(true);
			}
		}

		#endregion

		#region Internal Constructor

		internal NetworkSession(
			int maxGamers = MaxSupportedGamers,
			int privateGamerSlots = 0,
			bool allowHostMigration = false,
			bool allowJoinInProgress = false
		) {
			MaxGamers = maxGamers;
			PrivateGamerSlots = privateGamerSlots;
			AllowHostMigration = allowHostMigration;
			AllowJoinInProgress = allowJoinInProgress;
			SimulatedLatency = TimeSpan.Zero;
			SimulatedPacketLoss = 0.0f;
			IsDisposed = false;
		}

		#endregion

		#region Public Methods

		public void Dispose()
		{
			activeSession = null;
			IsDisposed = true;
		}

		public void Update()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("this");
			}

			// TODO: A whole bunch of crap I'm sure!
		}

		public void AddLocalGamer(SignedInGamer gamer)
		{
			// TODO: .Add()...
		}

		public NetworkGamer FindGamerById(byte gameId)
		{
			return null;
		}

		public void ResetReady()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("this");
			}
			if (!IsHost)
			{
				throw new InvalidOperationException("This NetworkSession is not the host");
			}

			// TODO: foreach IsReady = false...
		}

		public void StartGame()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("this");
			}
			if (!IsHost)
			{
				throw new InvalidOperationException("This NetworkSession is not the host");
			}
			if (SessionState == NetworkSessionState.Lobby)
			{
				throw new InvalidOperationException("NetworkSession is not Lobby");
			}

			// TODO: Actually start stuff...
			SessionState = NetworkSessionState.Playing;
			if (GameStarted != null)
			{
				GameStarted(this, new GameStartedEventArgs());
			}
		}

		public void EndGame()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("this");
			}
			if (!IsHost)
			{
				throw new InvalidOperationException("This NetworkSession is not the host");
			}
			if (SessionState == NetworkSessionState.Playing)
			{
				throw new InvalidOperationException("NetworkSession is not Playing");
			}

			// TODO: Actually end stuff...
			SessionState = NetworkSessionState.Lobby;
			if (GameEnded != null)
			{
				GameEnded(this, new GameEndedEventArgs());
			}
		}

		#endregion

		#region Public Static Create Methods

		public static NetworkSession Create(
			NetworkSessionType sessionType,
			int maxLocalGamers,
			int maxGamers
		) {
			IAsyncResult result = BeginCreate(
				sessionType,
				maxLocalGamers,
				maxGamers,
				null,
				null
			);
			result.AsyncWaitHandle.WaitOne();
			return EndCreate(result);
		}

		public static NetworkSession Create(
			NetworkSessionType sessionType,
			int maxLocalGamers,
			int maxGamers,
			int privateGamerSlots,
			NetworkSessionProperties sessionProperties
		) {
			IAsyncResult result = BeginCreate(
				sessionType,
				maxLocalGamers,
				maxGamers,
				privateGamerSlots,
				sessionProperties,
				null,
				null
			);
			result.AsyncWaitHandle.WaitOne();
			return EndCreate(result);
		}

		public static NetworkSession Create(
			NetworkSessionType sessionType,
			IEnumerable<SignedInGamer> localGamers,
			int maxGamers,
			int privateGamerSlots,
			NetworkSessionProperties sessionProperties
		) {
			IAsyncResult result = BeginCreate(
				sessionType,
				localGamers,
				maxGamers,
				privateGamerSlots,
				sessionProperties,
				null,
				null
			);
			result.AsyncWaitHandle.WaitOne();
			return EndCreate(result);
		}

		public static IAsyncResult BeginCreate(
			NetworkSessionType sessionType,
			int maxLocalGamers,
			int maxGamers,
			AsyncCallback callback,
			object asyncState
		) {
			if (maxLocalGamers < 1 || maxLocalGamers > 4)
			{
				throw new ArgumentOutOfRangeException("maxLocalGamers");
			}
			if (activeAction != null || activeSession != null)
			{
				throw new InvalidOperationException();
			}

			// TODO: Actual stuff?!
			activeAction = new NetworkSessionAction(asyncState, callback);
			return activeAction;
		}

		public static IAsyncResult BeginCreate(
			NetworkSessionType sessionType,
			int maxLocalGamers,
			int maxGamers,
			int privateGamerSlots,
			NetworkSessionProperties sessionProperties,
			AsyncCallback callback,
			object asyncState
		) {
			if (maxLocalGamers < 1 || maxLocalGamers > 4)
			{
				throw new ArgumentOutOfRangeException("maxLocalGamers");
			}
			if (privateGamerSlots < 0 || privateGamerSlots > maxGamers)
			{
				throw new ArgumentOutOfRangeException("privateGamerSlots");
			}
			if (activeAction != null || activeSession != null)
			{
				throw new InvalidOperationException();
			}

			// TODO: Actual stuff?!
			activeAction = new NetworkSessionAction(asyncState, callback);
			return activeAction;
		}

		public static IAsyncResult BeginCreate(
			NetworkSessionType sessionType,
			IEnumerable<SignedInGamer> localGamers,
			int maxGamers,
			int privateGamerSlots,
			NetworkSessionProperties sessionProperties,
			AsyncCallback callback,
			object asyncState
		) {
			if (privateGamerSlots < 0 || privateGamerSlots > maxGamers)
			{
				throw new ArgumentOutOfRangeException("privateGamerSlots");
			}
			if (activeAction != null || activeSession != null)
			{
				throw new InvalidOperationException();
			}

			// TODO: Actual stuff?!
			activeAction = new NetworkSessionAction(asyncState, callback);
			return activeAction;
		}

		public static NetworkSession EndCreate(IAsyncResult result)
		{
			if (result != activeAction)
			{
				throw new ArgumentException("result");
			}

			// TODO: Actual stuff?!
			activeAction = null;
			activeSession = new NetworkSession();
			return activeSession;
		}

		#endregion

		#region Public Static Find Methods

		public static AvailableNetworkSessionCollection Find(
			NetworkSessionType sessionType,
			int maxLocalGamers,
			NetworkSessionProperties searchProperties
		) {
			IAsyncResult result = BeginFind(
				sessionType,
				maxLocalGamers,
				searchProperties,
				null,
				null
			);
			result.AsyncWaitHandle.WaitOne();
			return EndFind(result);
		}

		public static AvailableNetworkSessionCollection Find(
			NetworkSessionType sessionType,
			IEnumerable<SignedInGamer> localGamers,
			NetworkSessionProperties searchProperties
		) {
			IAsyncResult result = BeginFind(
				sessionType,
				localGamers,
				searchProperties,
				null,
				null
			);
			result.AsyncWaitHandle.WaitOne();
			return EndFind(result);
		}

		public static IAsyncResult BeginFind(
			NetworkSessionType sessionType,
			int maxLocalGamers,
			NetworkSessionProperties searchProperties,
			AsyncCallback callback,
			object asyncState
		) {
			if (sessionType == NetworkSessionType.Local)
			{
				throw new ArgumentException("sessionType");
			}
			if (maxLocalGamers < 1 || maxLocalGamers > 4)
			{
				throw new ArgumentOutOfRangeException("maxLocalGamers");
			}
			if (activeAction != null || activeSession != null)
			{
				throw new InvalidOperationException();
			}

			// TODO: Actual stuff?!
			activeAction = new NetworkSessionAction(asyncState, callback);
			return activeAction;
		}

		public static IAsyncResult BeginFind(
			NetworkSessionType sessionType,
			IEnumerable<SignedInGamer> localGamers,
			NetworkSessionProperties searchProperties,
			AsyncCallback callback,
			object asyncState
		) {
			if (sessionType == NetworkSessionType.Local)
			{
				throw new ArgumentException("sessionType");
			}
			if (activeAction != null || activeSession != null)
			{
				throw new InvalidOperationException();
			}

			// TODO: Actual stuff?!
			activeAction = new NetworkSessionAction(asyncState, callback);
			return activeAction;
		}

		public static AvailableNetworkSessionCollection EndFind(IAsyncResult result)
		{
			if (result != activeAction)
			{
				throw new ArgumentException("result");
			}

			// TODO: Actual stuff?!
			activeAction = null;
			return null;
		}

		#endregion

		#region Public Static Join Methods

		public static NetworkSession Join(
			AvailableNetworkSession availableSession
		) {
			IAsyncResult result = BeginJoin(availableSession, null, null);
			result.AsyncWaitHandle.WaitOne();
			return EndJoin(result);
		}

		public static IAsyncResult BeginJoin(
			AvailableNetworkSession availableSession,
			AsyncCallback callback,
			object asyncState
		) {
			if (availableSession == null)
			{
				throw new ArgumentNullException("availableSession");
			}
			if (activeAction != null || activeSession != null)
			{
				throw new InvalidOperationException();
			}

			// TODO: Actual stuff?!
			activeAction = new NetworkSessionAction(asyncState, callback);
			return activeAction;
		}

		public static NetworkSession EndJoin(IAsyncResult result)
		{
			if (result != activeAction)
			{
				throw new ArgumentException("result");
			}

			// TODO: Actual stuff?!
			activeAction = null;
			activeSession = new NetworkSession();
			return activeSession;
		}

		public static NetworkSession JoinInvited(
			int maxLocalGamers
		) {
			IAsyncResult result = BeginJoinInvited(maxLocalGamers, null, null);
			result.AsyncWaitHandle.WaitOne();
			return EndJoinInvited(result);
		}

		public static NetworkSession JoinInvited(
			IEnumerable<SignedInGamer> localGamers
		) {
			IAsyncResult result = BeginJoinInvited(localGamers, null, null);
			result.AsyncWaitHandle.WaitOne();
			return EndJoinInvited(result);
		}

		public static IAsyncResult BeginJoinInvited(
			int maxLocalGamers,
			AsyncCallback callback,
			object asyncState
		) {
			if (maxLocalGamers < 1 || maxLocalGamers > 4)
			{
				throw new ArgumentOutOfRangeException("maxLocalGamers");
			}
			if (activeAction != null || activeSession != null)
			{
				throw new InvalidOperationException();
			}

			// TODO: Actual stuff?!
			activeAction = new NetworkSessionAction(asyncState, callback);
			return activeAction;
		}

		public static IAsyncResult BeginJoinInvited(
			IEnumerable<SignedInGamer> localGamers,
			AsyncCallback callback,
			object asyncState
		) {
			if (activeAction != null || activeSession != null)
			{
				throw new InvalidOperationException();
			}

			// TODO: Actual stuff?!
			activeAction = new NetworkSessionAction(asyncState, callback);
			return activeAction;
		}

		public static NetworkSession EndJoinInvited(IAsyncResult result)
		{
			if (result != activeAction)
			{
				throw new ArgumentException("result");
			}

			// TODO: Actual stuff?!
			activeAction = null;
			activeSession = new NetworkSession();
			return activeSession;
		}

		#endregion
	}
}
