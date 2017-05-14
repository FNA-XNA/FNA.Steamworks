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

using Steamworks;

using Microsoft.Xna.Framework.Audio;
#endregion

namespace Microsoft.Xna.Framework.GamerServices
{
	public sealed class SignedInGamer : Gamer
	{
		#region Public Properties

		public GameDefaults GameDefaults
		{
			get;
			private set;
		}

		public bool IsGuest
		{
			get;
			private set;
		}

		public bool IsSignedInToLive
		{
			get;
			private set;
		}

		public int PartySize
		{
			get;
			set; // FIXME: Actually read-only but okay guys -flibit
		}

		public PlayerIndex PlayerIndex
		{
			// ObjectDisposedException: Thrown if the associated profile is no longer valid. For example, the profile may have signed out.
			get;
			private set;
		}

		public GamerPresence Presence
		{
			get;
			private set;
		}

		public GamerPrivileges Privileges
		{
			get;
			private set;
		}

		#endregion

		#region Private Variables

#pragma warning disable 0414
		private Callback<UserStatsStored_t> statsStored;
		private Callback<UserStatsReceived_t> statsReceived;
#pragma warning restore 0414

		private GamerAction statStoreAction;
		private GamerAction statReceiveAction;

		#endregion

		#region Public Events

		public static event EventHandler<SignedInEventArgs> SignedIn;
		public static event EventHandler<SignedOutEventArgs> SignedOut;

		#endregion

		#region Internal Constructor

		internal SignedInGamer(
			CSteamID id,
			string gamertag,
			bool isSignedInToLive = false,
			bool isGuest = false,
			PlayerIndex playerIndex = PlayerIndex.One
		) : base(id, gamertag, gamertag) {
			IsGuest = isGuest;
			IsSignedInToLive = isSignedInToLive;
			PlayerIndex = playerIndex;

			statsStored = Callback<UserStatsStored_t>.Create(OnStatsStored);
			statsReceived = Callback<UserStatsReceived_t>.Create(OnStatsReceived);

			// TODO: Everything below
			GameDefaults = new GameDefaults();
			Presence = new GamerPresence();
			Privileges = new GamerPrivileges();
			PartySize = 1;
		}

		#endregion

		#region Public Methods

		public bool IsFriend(Gamer gamer)
		{
			EFriendRelationship efr = SteamFriends.GetFriendRelationship(gamer.steamID);
			return (	efr == EFriendRelationship.k_EFriendRelationshipFriend ||
					efr == EFriendRelationship.k_EFriendRelationshipIgnoredFriend	);
		}

		public bool IsHeadset(Microphone microphone)
		{
			// FIXME: Check against Gamer? -flibit
			return microphone.IsHeadset;
		}

		public FriendCollection GetFriends()
		{
			EFriendFlags flags = (
				EFriendFlags.k_EFriendFlagImmediate |
				EFriendFlags.k_EFriendFlagRequestingFriendship |
				EFriendFlags.k_EFriendFlagFriendshipRequested
			);
			int friendCount = SteamFriends.GetFriendCount(flags);
			List<FriendGamer> friends = new List<FriendGamer>(friendCount);
			for (int i = 0; i < friendCount; i += 1)
			{
				CSteamID id = SteamFriends.GetFriendByIndex(i, flags);
				EFriendRelationship relationship = SteamFriends.GetFriendRelationship(id);
				EPersonaState state = SteamFriends.GetFriendPersonaState(id);
				FriendGameInfo_t whoCares;
				friends.Add(new FriendGamer(
					id,
					SteamFriends.GetFriendPersonaName(id),
					SteamFriends.GetPlayerNickname(id),
					state != EPersonaState.k_EPersonaStateOffline,
					SteamFriends.GetFriendGamePlayed(id, out whoCares),
					state == EPersonaState.k_EPersonaStateAway,
					state == EPersonaState.k_EPersonaStateBusy,
					relationship == EFriendRelationship.k_EFriendRelationshipRequestRecipient,
					relationship == EFriendRelationship.k_EFriendRelationshipRequestInitiator
				));
			}
			return new FriendCollection(friends);
		}

		public void AwardAchievement(string achievementKey)
		{
			SteamUserStats.SetAchievement(achievementKey);
			SteamUserStats.StoreStats();
		}

		public IAsyncResult BeginAwardAchievement(
			string achievementKey,
			AsyncCallback callback,
			object state
		) {
			if (statStoreAction != null)
			{
				throw new InvalidOperationException();
			}
			SteamUserStats.SetAchievement(achievementKey);
			statStoreAction = new GamerAction(state, callback);
			SteamUserStats.StoreStats();
			return statStoreAction;
		}

		public void EndAwardAchievement(IAsyncResult result)
		{
			statStoreAction = null;
		}

		public AchievementCollection GetAchievements()
		{
			IAsyncResult result = BeginGetAchievements(null, null);
			while (!result.IsCompleted)
			{
				if (!GamerServicesDispatcher.UpdateAsync())
				{
					statReceiveAction.IsCompleted = true;
				}
			}
			return EndGetAchievements(result);
		}

		public IAsyncResult BeginGetAchievements(
			AsyncCallback callback,
			object asyncState
		) {
			if (statReceiveAction != null)
			{
				throw new InvalidOperationException();
			}
			statReceiveAction = new GamerAction(asyncState, callback);
			SteamUserStats.RequestUserStats(steamID);
			return statReceiveAction;
		}

		public AchievementCollection EndGetAchievements(IAsyncResult result)
		{
			uint numAch = SteamUserStats.GetNumAchievements();
			List<Achievement> achievements = new List<Achievement>((int) numAch);
			for (uint i = 0; i < numAch; i += 1)
			{
				string key = SteamUserStats.GetAchievementName(i);
				string name = SteamUserStats.GetAchievementDisplayAttribute(key, "name");
				string desc = SteamUserStats.GetAchievementDisplayAttribute(key, "desc");
				string hide = SteamUserStats.GetAchievementDisplayAttribute(key, "hidden");
				bool earned;
				uint unlockTime;
				SteamUserStats.GetUserAchievementAndUnlockTime(
					steamID,
					key,
					out earned,
					out unlockTime
				);
				DateTime unlockDT = new DateTime(1970, 1, 1, 0, 0, 0);
				unlockDT.AddSeconds(unlockTime);
				achievements.Add(new Achievement(
					key,
					name,
					desc,
					hide == "0",
					earned,
					unlockDT
				));
			}
			statReceiveAction = null;
			return new AchievementCollection(achievements);
		}

		#endregion

		#region Private Methods

		private void OnStatsStored(UserStatsStored_t stats)
		{
			// FIXME: Pray that we don't get overlap -flibit
			if (statStoreAction != null)
			{
				statStoreAction.IsCompleted = true;
			}
		}

		private void OnStatsReceived(UserStatsReceived_t stats)
		{
			if (stats.m_steamIDUser == steamID)
			{
				// FIXME: Initial stat acquisition...? -flibit
				if (statReceiveAction != null)
				{
					statReceiveAction.IsCompleted = true;
				}
			}
		}

		#endregion

		#region Internal Static Methods

		internal static void OnSignIn(SignedInGamer gamer)
		{
			if (SignedIn != null)
			{
				SignedIn(null, new SignedInEventArgs(gamer));
			}
		}

		internal static void OnSignOut(SignedInGamer gamer)
		{
			if (SignedOut != null)
			{
				SignedOut(null, new SignedOutEventArgs(gamer));
			}
		}

		#endregion
	}
}
