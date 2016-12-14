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

		#region Network Event Structure

		internal enum NetworkEventType
		{
			PacketSend,
			GamerJoin,
			GamerLeave,
			HostChange,
			StateChange
		}

		internal struct NetworkEvent
		{
			public NetworkEventType Type;

			public NetworkGamer Gamer;
			public byte[] Packet;
			public SendDataOptions Reliable;
			public NetworkSessionState State;
			public NetworkSessionEndReason Reason;
		}

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

		private Queue<NetworkEvent> networkEvents;

		private Callback<LobbyChatUpdate_t> lobbyUpdated;
		private Callback<LobbyDataUpdate_t> lobbyDataUpdated;
		private Callback<P2PSessionRequest_t> p2pRequested;
		private Callback<P2PSessionConnectFail_t> p2pFailed;

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

		private static readonly EP2PSend[] SWSendType = new EP2PSend[]
		{
			EP2PSend.k_EP2PSendUnreliable,
			EP2PSend.k_EP2PSendReliable,
			EP2PSend.k_EP2PSendUnreliable, // FIXME: WithBuffering? -flibit
			EP2PSend.k_EP2PSendReliableWithBuffering,
			EP2PSend.k_EP2PSendReliableWithBuffering
		};

		private static CallResult<LobbyCreated_t> lobbyCreated;
		private static CallResult<LobbyMatchList_t> lobbyFound;
		private static CallResult<LobbyEnter_t> lobbyJoined;

		private static CSteamID inviteLobby;

		#endregion

		#region Public Events

		public event EventHandler<GameStartedEventArgs> GameStarted;

		public event EventHandler<GameEndedEventArgs> GameEnded;

		public event EventHandler<GamerJoinedEventArgs> GamerJoined;

		public event EventHandler<GamerLeftEventArgs> GamerLeft;

		public event EventHandler<HostChangedEventArgs> HostChanged;

		public event EventHandler<NetworkSessionEndedEventArgs> SessionEnded;

// TODO: Leaderboards/TrueSkill
#pragma warning disable 0067
		public event EventHandler<WriteLeaderboardsEventArgs> WriteArbitratedLeaderboard;

		public event EventHandler<WriteLeaderboardsEventArgs> WriteUnarbitratedLeaderboard;

		public event EventHandler<WriteLeaderboardsEventArgs> WriteTrueSkill;
#pragma warning restore 0067

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
			public CSteamID[] Lobbies;

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
			int maxGamers,
			int privateGamerSlots,
			int maxLocal,
			IEnumerable<SignedInGamer> localGamers,
			List<CSteamID> remoteIDs
		) {
			this.lobby = lobby;
			SessionProperties = properties;
			SessionType = type;
			MaxGamers = maxGamers;
			PrivateGamerSlots = privateGamerSlots;

			// Hook up lobby events
			lobbyUpdated = Callback<LobbyChatUpdate_t>.Create(
				OnLobbyUpdated
			);
			lobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(
				OnLobbyDataUpdated
			);
			p2pRequested = Callback<P2PSessionRequest_t>.Create(
				OnP2PRequested
			);
			p2pFailed = Callback<P2PSessionConnectFail_t>.Create(
				OnP2PConnectFailed
			);

			// Create Gamer lists

			List<LocalNetworkGamer> locals = new List<LocalNetworkGamer>();
			if (localGamers == null)
			{
				// FIXME: Check for mismatch in SignedInGamers Count -flibit
				maxLocalGamers = maxLocal;
				for (int i = 0; i < Gamer.SignedInGamers.Count && i < maxLocalGamers; i += 1)
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

			List<NetworkGamer> remoteGamers = new List<NetworkGamer>();
			if (remoteIDs != null)
			{
				foreach (CSteamID id in remoteIDs)
				{
					remoteGamers.Add(new NetworkGamer(
						id,
						this
					));
				}
			}
			RemoteGamers = new GamerCollection<NetworkGamer>(remoteGamers);

			List<NetworkGamer> allGamers = new List<NetworkGamer>();
			allGamers.AddRange(remoteGamers);
			allGamers.AddRange(locals);
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

				SteamMatchmaking.SetLobbyData(
					lobby,
					"HostGamertag",
					SteamFriends.GetFriendPersonaName(Host.steamID)
				);
				SteamMatchmaking.SetLobbyData(
					lobby,
					"CurrentGamerCount",
					AllGamers.Count.ToString()
				);
				SteamMatchmaking.SetLobbyData(
					lobby,
					"OpenPrivateSlots",
					MaxSupportedGamers.ToString() // FIXME
				);
				SteamMatchmaking.SetLobbyData(
					lobby,
					"OpenPublicSlots",
					MaxSupportedGamers.ToString() // FIXME
				);
			}

			// Event hookups
			networkEvents = new Queue<NetworkEvent>();
			foreach (NetworkGamer gamer in AllGamers)
			{
				NetworkSession.NetworkEvent evt = new NetworkEvent()
				{
					Type = NetworkEventType.GamerJoin,
					Gamer = gamer
				};
				SendNetworkEvent(evt);
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
			// Unhook lobby events
			lobbyUpdated.Unregister();
			lobbyDataUpdated.Unregister();
			p2pRequested.Unregister();
			p2pFailed.Unregister();

			// TODO: CloseP2PSessionWithUser -flibit

			SteamMatchmaking.LeaveLobby(lobby);

			activeSession = null;
			IsDisposed = true;
		}

		public void Update()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("this");
			}

			while (networkEvents.Count > 0)
			{
				NetworkEvent evt = networkEvents.Dequeue();

				if (evt.Type == NetworkEventType.PacketSend)
				{
					SteamNetworking.SendP2PPacket(
						evt.Gamer.steamID,
						evt.Packet,
						(uint) evt.Packet.Length,
						SWSendType[(int) evt.Reliable],
						0
					);
				}
				else if (evt.Type == NetworkEventType.GamerJoin)
				{
					if (GamerJoined != null)
					{
						GamerJoined(
							this,
							new GamerJoinedEventArgs(evt.Gamer)
						);
					}
				}
				else if (evt.Type == NetworkEventType.GamerLeave)
				{
					if (GamerLeft != null)
					{
						GamerLeft(
							this,
							new GamerLeftEventArgs(evt.Gamer)
						);
					}
				}
				else if (evt.Type == NetworkEventType.HostChange)
				{
					if (HostChanged != null)
					{
						HostChanged(
							this,
							new HostChangedEventArgs(
								Host,
								evt.Gamer
							)
						);
					}

					// FIXME: Is the timing on this accurate? -flibit
					Host = evt.Gamer;
				}
				else if (evt.Type == NetworkEventType.StateChange)
				{
					if (evt.State == NetworkSessionState.Playing)
					{
						if (IsHost)
						{
							SteamMatchmaking.SetLobbyJoinable(
								lobby,
								AllowJoinInProgress
							);
						}
						if (GameStarted != null)
						{
							GameStarted(this, new GameStartedEventArgs());
						}
					}
					else if (evt.State == NetworkSessionState.Lobby)
					{
						if (IsHost)
						{
							SteamMatchmaking.SetLobbyJoinable(lobby, true);
						}
						if (GameEnded != null)
						{
							GameEnded(this, new GameEndedEventArgs());
						}
					}
					else // if (evt.State == NetworkSessionState.Ended)
					{
						SessionEnded(
							this,
							new NetworkSessionEndedEventArgs(evt.Reason)
						);
					}

					// FIXME: Is the timing on this accurate? -flibit
					SessionState = evt.State;
				}
			}

			uint packetSize;
			while (SteamNetworking.IsP2PPacketAvailable(out packetSize))
			{
				CSteamID id;
				NetworkEvent evt = new NetworkEvent()
				{
					Packet = new byte[packetSize]
				};
				SteamNetworking.ReadP2PPacket(
					evt.Packet,
					(uint) evt.Packet.Length,
					out packetSize,
					out id
				);
				foreach (NetworkGamer gamer in AllGamers)
				{
					if (id == gamer.steamID)
					{
						evt.Gamer = gamer;
						break;
					}
				}
				foreach (LocalNetworkGamer gamer in LocalGamers)
				{
					gamer.packetQueue.Enqueue(evt);
				}
			}
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
			foreach (NetworkGamer g in AllGamers)
			{
				if (g.Id == gameId)
				{
					return g;
				}
			}
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
			if (SessionState != NetworkSessionState.Lobby)
			{
				throw new InvalidOperationException("NetworkSession is not Lobby");
			}

			NetworkEvent evt = new NetworkEvent()
			{
				Type = NetworkEventType.StateChange,
				State = NetworkSessionState.Playing
			};
			SendNetworkEvent(evt);
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
			if (SessionState != NetworkSessionState.Playing)
			{
				throw new InvalidOperationException("NetworkSession is not Playing");
			}

			NetworkEvent evt = new NetworkEvent()
			{
				Type = NetworkEventType.StateChange,
				State = NetworkSessionState.Lobby
			};
			SendNetworkEvent(evt);
		}

		#endregion

		#region Internal Methods

		internal void SendNetworkEvent(NetworkEvent evt)
		{
			networkEvents.Enqueue(evt);
		}

		#endregion

		#region Private Methods

		private void OnLobbyUpdated(LobbyChatUpdate_t update)
		{
			NetworkGamer gamer = null;
			CSteamID lobby = new CSteamID(update.m_ulSteamIDLobby);
			CSteamID user = new CSteamID(update.m_ulSteamIDUserChanged);
			if (lobby != this.lobby)
			{
				return; // ???
			}
			EChatMemberStateChange change = (EChatMemberStateChange) update.m_rgfChatMemberStateChange;
			if (change == EChatMemberStateChange.k_EChatMemberStateChangeEntered)
			{
				SignedInGamer localGamer = null;
				foreach (SignedInGamer g in Gamer.SignedInGamers)
				{
					if (user == g.steamID)
					{
						localGamer = g;
						break;
					}
				}
				if (localGamer != null)
				{
					gamer = new LocalNetworkGamer(
						localGamer,
						this
					);
					LocalGamers.collection.Add(gamer as LocalNetworkGamer);
				}
				else
				{
					gamer = new NetworkGamer(user, this);
					RemoteGamers.collection.Add(gamer);
				}
				AllGamers.collection.Add(gamer);

				NetworkEvent evt = new NetworkEvent()
				{
					Type = NetworkEventType.GamerJoin,
					Gamer = gamer
				};
				SendNetworkEvent(evt);
			}
			else if (	change == EChatMemberStateChange.k_EChatMemberStateChangeLeft ||
					change == EChatMemberStateChange.k_EChatMemberStateChangeDisconnected	)
			{
				foreach (NetworkGamer g in AllGamers)
				{
					if (user == g.steamID)
					{
						gamer = g;
						break;
					}
				}
				if (gamer == null)
				{
					return; // ???
				}
				if (gamer.IsLocal)
				{
					LocalGamers.collection.Remove(gamer as LocalNetworkGamer);
				}
				else
				{
					RemoteGamers.collection.Remove(gamer);
				}
				AllGamers.collection.Remove(gamer);

				NetworkEvent evt = new NetworkEvent()
				{
					Type = NetworkEventType.GamerLeave,
					Gamer = gamer
				};
				SendNetworkEvent(evt);

				if (gamer == Host)
				{
					CSteamID newHost = SteamMatchmaking.GetLobbyOwner(lobby);
					foreach (NetworkGamer g in AllGamers)
					{
						if (g.steamID == newHost)
						{
							evt = new NetworkEvent()
							{
								Type = NetworkEventType.HostChange,
								Gamer = g
							};
							SendNetworkEvent(evt);
							break;
						}
					}
				}
			}
			else
			{
				FNALoggerEXT.LogInfo(change.ToString());
			}
		}

		private void OnLobbyDataUpdated(LobbyDataUpdate_t data)
		{
			// FIXME: Assuming SessionChange! -flibit
			NetworkEvent evt = new NetworkEvent()
			{
				Type = NetworkEventType.StateChange,
				State = SessionState
			};
			SendNetworkEvent(evt);
		}

		private void OnP2PRequested(P2PSessionRequest_t request)
		{
			foreach (NetworkGamer g in RemoteGamers)
			{
				if (g.steamID == request.m_steamIDRemote)
				{
					SteamNetworking.AcceptP2PSessionWithUser(
						request.m_steamIDRemote
					);
					return;
				}
			}
		}

		private void OnP2PConnectFailed(P2PSessionConnectFail_t failure)
		{
			FNALoggerEXT.LogError(
				"Error connecting to " + failure.m_steamIDRemote +
				": " + ((EP2PSessionError) failure.m_eP2PSessionError).ToString()
			);
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

			CreateLobby(sessionType, maxGamers, privateGamerSlots);

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

			CreateLobby(sessionType, maxGamers, privateGamerSlots);

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
				SteamMatchmaking.GetLobbyMemberLimit(activeAction.Lobby),
				activeAction.MaxPrivateSlots,
				activeAction.MaxLocalGamers,
				activeAction.LocalGamers,
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

			FindLobby(
				sessionType,
				searchProperties,
				maxLocalGamers
			);
			activeAction = new NetworkSessionAction(
				asyncState,
				callback,
				maxLocalGamers,
				null,
				0,
				searchProperties,
				sessionType
			);
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

			int locals = 0;
			foreach (SignedInGamer gamer in localGamers)
			{
				locals += 1;
			}

			FindLobby(
				sessionType,
				searchProperties,
				locals
			);
			activeAction = new NetworkSessionAction(
				asyncState,
				callback,
				locals,
				localGamers,
				0,
				searchProperties,
				sessionType
			);
			return activeAction;
		}

		public static AvailableNetworkSessionCollection EndFind(IAsyncResult result)
		{
			if (result != activeAction)
			{
				throw new ArgumentException("result");
			}

			List<AvailableNetworkSession> sessions = new List<AvailableNetworkSession>(
				activeAction.Lobbies.Length
			);

			for (int i = 0; i < activeAction.Lobbies.Length; i += 1)
			{
				sessions.Add(new AvailableNetworkSession(
					activeAction.Lobbies[i],
					int.Parse(SteamMatchmaking.GetLobbyData(
						activeAction.Lobbies[i],
						"CurrentGamerCount"
					)),
					SteamMatchmaking.GetLobbyData(
						activeAction.Lobbies[i],
						"HostGamertag"
					),
					int.Parse(SteamMatchmaking.GetLobbyData(
						activeAction.Lobbies[i],
						"OpenPrivateSlots"
					)),
					int.Parse(SteamMatchmaking.GetLobbyData(
						activeAction.Lobbies[i],
						"OpenPublicSlots"
					)),
					activeAction.SessionProperties,
					new QualityOfService() // FIXME
				));
			}

			activeAction = null;
			return new AvailableNetworkSessionCollection(sessions);
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

			SteamAPICall_t call = SteamMatchmaking.JoinLobby(availableSession.lobby);
			if (call.m_SteamAPICall != 0)
			{
				if (lobbyJoined == null)
				{
					lobbyJoined = CallResult<LobbyEnter_t>.Create();
				}
				lobbyJoined.Set(
					call,
					OnLobbyJoined
				);
				activeAction = new NetworkSessionAction(
					asyncState,
					callback,
					4, // FIXME
					null,
					0,
					null,
					NetworkSessionType.PlayerMatch // FIXME
				);
				return activeAction;
			}
			return null;
		}

		public static NetworkSession EndJoin(IAsyncResult result)
		{
			if (result != activeAction)
			{
				throw new ArgumentException("result");
			}

			int numMems = SteamMatchmaking.GetNumLobbyMembers(activeAction.Lobby);
			List<CSteamID> remotes = new List<CSteamID>(
				numMems - Gamer.SignedInGamers.Count
			);
			for (int i = 0; i < numMems; i += 1)
			{
				CSteamID id = SteamMatchmaking.GetLobbyMemberByIndex(
					activeAction.Lobby,
					i
				);
				bool isRemote = true;
				for (int j = 0; j < Gamer.SignedInGamers.Count; j += 1)
				{
					if (id == Gamer.SignedInGamers[j].steamID)
					{
						isRemote = false;
						break;
					}
				}
				if (isRemote)
				{
					remotes.Add(id);
				}
			}

			activeSession = new NetworkSession(
				activeAction.Lobby,
				null, // FIXME
				NetworkSessionType.PlayerMatch, // FIXME
				MaxSupportedGamers, // FIXME
				4, // FIXME
				activeAction.MaxLocalGamers,
				activeAction.LocalGamers,
				remotes
			);
			activeAction = null;
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

			SteamAPICall_t call = SteamMatchmaking.JoinLobby(
				inviteLobby
			);
			if (call.m_SteamAPICall != 0)
			{
				if (lobbyJoined == null)
				{
					lobbyJoined = CallResult<LobbyEnter_t>.Create();
				}
				lobbyJoined.Set(
					call,
					OnLobbyJoined
				);
				activeAction = new NetworkSessionAction(
					asyncState,
					callback,
					maxLocalGamers,
					null,
					0,
					null,
					NetworkSessionType.PlayerMatch // FIXME
				);
				return activeAction;
			}
			return null;
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

			SteamAPICall_t call = SteamMatchmaking.JoinLobby(
				inviteLobby
			);
			if (call.m_SteamAPICall != 0)
			{
				if (lobbyJoined == null)
				{
					lobbyJoined = CallResult<LobbyEnter_t>.Create();
				}
				lobbyJoined.Set(
					call,
					OnLobbyJoined
				);
				activeAction = new NetworkSessionAction(
					asyncState,
					callback,
					0,
					localGamers,
					0,
					null,
					NetworkSessionType.PlayerMatch // FIXME
				);
				return activeAction;
			}
			return null;
		}

		public static NetworkSession EndJoinInvited(IAsyncResult result)
		{
			if (result != activeAction)
			{
				throw new ArgumentException("result");
			}

			int numMems = SteamMatchmaking.GetNumLobbyMembers(activeAction.Lobby);
			List<CSteamID> remotes = new List<CSteamID>(
				numMems - Gamer.SignedInGamers.Count
			);
			for (int i = 0; i < numMems; i += 1)
			{
				CSteamID id = SteamMatchmaking.GetLobbyMemberByIndex(
					activeAction.Lobby,
					i
				);
				bool isRemote = true;
				for (int j = 0; j < Gamer.SignedInGamers.Count; j += 1)
				{
					if (id == Gamer.SignedInGamers[j].steamID)
					{
						isRemote = false;
						break;
					}
				}
				if (isRemote)
				{
					remotes.Add(id);
				}
			}

			activeSession = new NetworkSession(
				activeAction.Lobby,
				null, // FIXME
				NetworkSessionType.PlayerMatch, // FIXME
				MaxSupportedGamers, // FIXME
				4, // FIXME
				activeAction.MaxLocalGamers,
				activeAction.LocalGamers,
				remotes
			);
			activeAction = null;
			return activeSession;
		}

		#endregion

		#region Internal Static Methods

		internal static void OnInviteAccepted(GameLobbyJoinRequested_t request)
		{
			inviteLobby = request.m_steamIDLobby;
			if (InviteAccepted != null)
			{
				InviteAccepted(
					null,
					new InviteAcceptedEventArgs(
						Gamer.SignedInGamers[0], // FIXME
						activeSession != null && inviteLobby == activeSession.lobby
					)
				);
			}
		}

		#endregion

		#region Private Static Methods

		private static void CreateLobby(
			NetworkSessionType type,
			int maxGamers,
			int maxPrivateGamers = 0
		) {
			ELobbyType lobbyType = SWSessionType[(int) type];
			if (maxPrivateGamers > 0)
			{
				// FIXME: Better way to determine private lobby... -flibit
				lobbyType = ELobbyType.k_ELobbyTypePrivate;
			}
			SteamAPICall_t call = SteamMatchmaking.CreateLobby(
				SWSessionType[(int) type],
				maxGamers
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

		private static void FindLobby(
			NetworkSessionType type,
			NetworkSessionProperties properties,
			int localGamers
		) {
			// TODO: type? -flibit
			for (int i = 0; i < properties.Count; i += 1)
			{
				// FIXME: null property checks -flibit
				SteamMatchmaking.AddRequestLobbyListNumericalFilter(
					i.ToString(),
					properties[i].Value,
					ELobbyComparison.k_ELobbyComparisonEqual
				);
			}
			SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(localGamers);
			SteamAPICall_t call = SteamMatchmaking.RequestLobbyList();
			if (call.m_SteamAPICall != 0)
			{
				if (lobbyFound == null)
				{
					lobbyFound = CallResult<LobbyMatchList_t>.Create();
				}
				lobbyFound.Set(
					call,
					OnLobbyFound
				);
			}
		}

		private static void OnLobbyFound(LobbyMatchList_t lobby, bool bIOFailure)
		{
			activeAction.Lobbies = new CSteamID[lobby.m_nLobbiesMatching];
			if (!bIOFailure && lobby.m_nLobbiesMatching > 0)
			{
				// Just pick the first one, whatevs -flibit
				for (int i = 0; i < activeAction.Lobbies.Length; i += 1)
				{
					activeAction.Lobbies[i] = SteamMatchmaking.GetLobbyByIndex(i);
				}
			}
			activeAction.IsCompleted = true;
		}

		private static void OnLobbyJoined(LobbyEnter_t lobby, bool bIOFailure)
		{
			if (!bIOFailure && lobby.m_ulSteamIDLobby != 0)
			{
				activeAction.Lobby = new CSteamID(lobby.m_ulSteamIDLobby);
			}
			activeAction.IsCompleted = true;
		}

		#endregion
	}
}
