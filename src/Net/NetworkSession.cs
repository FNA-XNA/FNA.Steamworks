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

using Steamworks;

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
			get
			{
				if (lobby.m_SteamID == 0)
				{
					throw new ObjectDisposedException("this");
				}
				return SteamMatchmaking.GetLobbyData(
					lobby,
					"AllowHostMigration"
				) == "1";
			}
			set
			{
				if (!IsHost)
				{
					throw new InvalidOperationException("You are not the host");
				}
				if (lobby.m_SteamID == 0)
				{
					throw new ObjectDisposedException("this");
				}
				SteamMatchmaking.SetLobbyData(
					lobby,
					"AllowHostMigration",
					value ? "1" : "0"
				);
			}
		}

		public bool AllowJoinInProgress
		{
			get
			{
				if (lobby.m_SteamID == 0)
				{
					throw new ObjectDisposedException("this");
				}
				return SteamMatchmaking.GetLobbyData(
					lobby,
					"AllowJoinInProgress"
				) == "1";
			}
			set
			{
				if (SessionType == NetworkSessionType.Ranked)
				{
					throw new NotSupportedException("This match is Ranked");
				}
				if (!IsHost)
				{
					throw new InvalidOperationException("You are not the host");
				}
				if (lobby.m_SteamID == 0)
				{
					throw new ObjectDisposedException("this");
				}
				SteamMatchmaking.SetLobbyData(
					lobby,
					"AllowJoinInProgress",
					value ? "1" : "0"
				);
			}
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
			get
			{
				foreach (LocalNetworkGamer gamer in LocalGamers)
				{
					if (!gamer.IsReady)
					{
						return false;
					}
				}
				return true;
			}
		}

		public bool IsHost
		{
			get
			{
				foreach (LocalNetworkGamer gamer in LocalGamers)
				{
					if (gamer.IsHost)
					{
						return true;
					}
				}
				return false;
			}
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
			get
			{
				return (NetworkSessionState) Enum.Parse(
					typeof(NetworkSessionState),
					SteamMatchmaking.GetLobbyData(
						lobby,
						"SessionState"
					)
				);
			}
			private set
			{
				SteamMatchmaking.SetLobbyData(
					lobby,
					"SessionState",
					value.ToString()
				);
			}
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

		#region Internal Variables

		internal CSteamID lobby;

		#endregion

		#region Private Variables

		private int maxLocalGamers;

		#endregion

		#region Private Static Variables

		private static NetworkSessionAction activeAction = null;

		private static NetworkSession activeSession = null;

		private static readonly ELobbyType[] SWSessionType = new ELobbyType[]
		{
			ELobbyType.k_ELobbyTypePrivate, // Local
			ELobbyType.k_ELobbyTypePrivate, // LocalWithLeaderboards
			ELobbyType.k_ELobbyTypePrivate, // FIXME: SystemLink
			ELobbyType.k_ELobbyTypePublic, // PlayerMatch
			ELobbyType.k_ELobbyTypePublic // Ranked
		};

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

			public readonly int MaxLocalGamers;
			public readonly IEnumerable<SignedInGamer> LocalGamers;
			public readonly int MaxPrivateSlots;
			public readonly NetworkSessionProperties SessionProperties;
			public readonly NetworkSessionType SessionType;

			public CSteamID Lobby;

			public NetworkSessionAction(
				object state,
				AsyncCallback callback,
				int maxLocal,
				IEnumerable<SignedInGamer> localGamers,
				int maxPrivateSlots,
				NetworkSessionProperties properties,
				NetworkSessionType type
			) {
				AsyncState = state;
				Callback = callback;
				IsCompleted = false;
				AsyncWaitHandle = new ManualResetEvent(true);

				MaxLocalGamers = maxLocal;
				LocalGamers = localGamers;
				MaxPrivateSlots = maxPrivateSlots;
				SessionProperties = properties;
				SessionType = type;

				Lobby = new CSteamID();
			}
		}

		#endregion

		#region Internal Constructor

		internal NetworkSession(
			CSteamID lobby,
			NetworkSessionProperties properties,
			NetworkSessionType type,
			bool isHost,
			int maxGamers,
			int privateGamerSlots,
			int maxLocal,
			IEnumerable<SignedInGamer> localGamers,
			List<NetworkGamer> remoteGamers,
			List<NetworkGamer> previousGamers
		) {
			this.lobby = lobby;
			SessionProperties = properties;
			SessionType = type;
			MaxGamers = maxGamers;
			PrivateGamerSlots = privateGamerSlots;

			// Create Gamer lists

			List<LocalNetworkGamer> locals = new List<LocalNetworkGamer>();
			if (localGamers == null)
			{
				// FIXME: Check for mismatch in SignedInGamers Count -flibit
				maxLocalGamers = maxLocal;
				for (int i = 0; i < maxLocalGamers; i += 1)
				{
					locals.Add(new LocalNetworkGamer(
						Gamer.SignedInGamers[i],
						this
					));
				}
			}
			else
			{
				maxLocalGamers = 0;
				foreach (SignedInGamer gamer in localGamers)
				{
					locals.Add(new LocalNetworkGamer(gamer, this));
					maxLocalGamers += 1;
				}
			}
			LocalGamers = new GamerCollection<LocalNetworkGamer>(locals);

			if (remoteGamers == null)
			{
				remoteGamers = new List<NetworkGamer>();
			}
			RemoteGamers = new GamerCollection<NetworkGamer>(remoteGamers);

			List<NetworkGamer> allGamers = new List<NetworkGamer>();
			allGamers.AddRange(locals);
			allGamers.AddRange(remoteGamers);
			AllGamers = new GamerCollection<NetworkGamer>(allGamers);

			PreviousGamers = new GamerCollection<NetworkGamer>(
				new List<NetworkGamer>()
			);

			// Create host data

			CSteamID host = SteamMatchmaking.GetLobbyOwner(lobby);
			foreach (NetworkGamer gamer in AllGamers)
			{
				if (gamer.steamID == host)
				{
					Host = gamer;
					break;
				}
			}

			if (IsHost)
			{
				AllowHostMigration = false;
				AllowJoinInProgress = false;
				SessionState = NetworkSessionState.Lobby;
			}

			// Other defaults

			SimulatedLatency = TimeSpan.Zero;
			SimulatedPacketLoss = 0.0f;
			IsDisposed = false;

			// TODO: Everything below
			BytesPerSecondReceived = 0;
			BytesPerSecondSent = 0;
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
			if (LocalGamers.Count == maxLocalGamers)
			{
				throw new InvalidOperationException("LocalGamer max limit!");
			}
			LocalNetworkGamer adding = new LocalNetworkGamer(gamer, this);
			LocalGamers.collection.Add(adding);
			AllGamers.collection.Add(adding);
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

			foreach (NetworkGamer gamer in AllGamers)
			{
				gamer.IsReady = false;
			}
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
			while (result.IsCompleted)
			{
				SteamAPI.RunCallbacks();
			}
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
			while (result.IsCompleted)
			{
				SteamAPI.RunCallbacks();
			}
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
			while (result.IsCompleted)
			{
				SteamAPI.RunCallbacks();
			}
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

			CreateLobby(sessionType, maxGamers);

			activeAction = new NetworkSessionAction(
				asyncState,
				callback,
				maxLocalGamers,
				null,
				0,
				null,
				sessionType
			);
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

			CreateLobby(sessionType, maxGamers);

			activeAction = new NetworkSessionAction(
				asyncState,
				callback,
				maxLocalGamers,
				null,
				privateGamerSlots,
				sessionProperties,
				sessionType
			);
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

			CreateLobby(sessionType, maxGamers);

			activeAction = new NetworkSessionAction(
				asyncState,
				callback,
				0,
				localGamers,
				privateGamerSlots,
				sessionProperties,
				sessionType
			);
			return activeAction;
		}

		public static NetworkSession EndCreate(IAsyncResult result)
		{
			if (result != activeAction)
			{
				throw new ArgumentException("result");
			}

			activeSession = new NetworkSession(
				activeAction.Lobby,
				activeAction.SessionProperties,
				activeAction.SessionType,
				true,
				SteamMatchmaking.GetLobbyMemberLimit(activeAction.Lobby),
				activeAction.MaxPrivateSlots,
				activeAction.MaxLocalGamers,
				activeAction.LocalGamers,
				null,
				null
			);

			activeAction = null;
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
			while (result.IsCompleted)
			{
				SteamAPI.RunCallbacks();
			}
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
			while (result.IsCompleted)
			{
				SteamAPI.RunCallbacks();
			}
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
			throw new NotImplementedException();
			//activeAction = new NetworkSessionAction(asyncState, callback);
			//return activeAction;
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
			throw new NotImplementedException();
			//activeAction = new NetworkSessionAction(asyncState, callback);
			//return activeAction;
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
			while (result.IsCompleted)
			{
				SteamAPI.RunCallbacks();
			}
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
			throw new NotImplementedException();
			//activeAction = new NetworkSessionAction(asyncState, callback);
			//return activeAction;
		}

		public static NetworkSession EndJoin(IAsyncResult result)
		{
			if (result != activeAction)
			{
				throw new ArgumentException("result");
			}

			// TODO: Actual stuff?!
			activeAction = null;
			//activeSession = new NetworkSession();
			return activeSession;
		}

		public static NetworkSession JoinInvited(
			int maxLocalGamers
		) {
			IAsyncResult result = BeginJoinInvited(maxLocalGamers, null, null);
			while (result.IsCompleted)
			{
				SteamAPI.RunCallbacks();
			}
			return EndJoinInvited(result);
		}

		public static NetworkSession JoinInvited(
			IEnumerable<SignedInGamer> localGamers
		) {
			IAsyncResult result = BeginJoinInvited(localGamers, null, null);
			while (result.IsCompleted)
			{
				SteamAPI.RunCallbacks();
			}
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
			throw new NotImplementedException();
			//activeAction = new NetworkSessionAction(asyncState, callback);
			//return activeAction;
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
			throw new NotImplementedException();
			//activeAction = new NetworkSessionAction(asyncState, callback);
			//return activeAction;
		}

		public static NetworkSession EndJoinInvited(IAsyncResult result)
		{
			if (result != activeAction)
			{
				throw new ArgumentException("result");
			}

			// TODO: Actual stuff?!
			activeAction = null;
			//activeSession = new NetworkSession();
			return activeSession;
		}

		#endregion

		#region Private Static Methods

		private static CallResult<LobbyCreated_t> lobbyCreated;

		private static void CreateLobby(NetworkSessionType type, int max)
		{
			SteamAPICall_t call = SteamMatchmaking.CreateLobby(
				SWSessionType[(int) type],
				max
			);
			if (call.m_SteamAPICall != 0)
			{
				if (lobbyCreated == null)
				{
					lobbyCreated = CallResult<LobbyCreated_t>.Create();
				}
				lobbyCreated.Set(call, OnLobbyCreated);
			}
		}

		private static void OnLobbyCreated(LobbyCreated_t lobby, bool bIOFailure)
		{
			if (!bIOFailure && lobby.m_eResult == EResult.k_EResultOK)
			{
				activeAction.Lobby = new CSteamID(lobby.m_ulSteamIDLobby);
				for (int i = 0; i < activeAction.SessionProperties.Count; i += 1)
				{
					// FIXME: null property checks! -flibit
					SteamMatchmaking.SetLobbyData(
						activeAction.Lobby,
						i.ToString(),
						activeAction.SessionProperties[i].ToString()
					);
				}
			}
			else
			{
				FNALoggerEXT.LogError(lobby.m_eResult.ToString());
			}
			activeAction.IsCompleted = true;
		}

		#endregion
	}
}
