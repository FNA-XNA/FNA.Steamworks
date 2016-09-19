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

		#region Public Events

		public static event EventHandler<SignedInEventArgs> SignedIn;
		public static event EventHandler<SignedOutEventArgs> SignedOut;

		#endregion

		#region Internal Constructor

		internal SignedInGamer(
			bool isSignedInToLive = false,
			bool isGuest = false,
			PlayerIndex playerIndex = PlayerIndex.One
		) {
			IsGuest = isGuest;
			IsSignedInToLive = isSignedInToLive;
			PlayerIndex = playerIndex;

			GameDefaults = new GameDefaults();
			Presence = new GamerPresence();
			Privileges = new GamerPrivileges();
			PartySize = 1;
		}

		#endregion

		#region Public Methods

		public bool IsFriend(Gamer gamer)
		{
			// TODO: Actual stuff?! -flibit
			return false;
		}

		public bool IsHeadset(Microphone microphone)
		{
			// FIXME: Check against Gamer? -flibit
			return microphone.IsHeadset;
		}

		public FriendCollection GetFriends()
		{
			// TODO: Actual stuff?! -flibit
			return null;
		}

		public void AwardAchievement(string achievementKey)
		{
			IAsyncResult result = BeginAwardAchievement(achievementKey, null, null);
			result.AsyncWaitHandle.WaitOne();
			EndAwardAchievement(result);
		}

		public IAsyncResult BeginAwardAchievement(
			string achievementKey,
			AsyncCallback callback,
			object state
		) {
			// TODO: Actual stuff?! -flibit
			return new GamerAction(state, callback);
		}

		public void EndAwardAchievement(IAsyncResult result)
		{
			// TODO: Actual stuff?! -flibit
		}

		public AchievementCollection GetAchievements()
		{
			IAsyncResult result = BeginGetAchievements(null, null);
			result.AsyncWaitHandle.WaitOne();
			return EndGetAchievements(result);
		}

		public IAsyncResult BeginGetAchievements(
			AsyncCallback callback,
			object asyncState
		) {
			// TODO: Actual stuff?! -flibit
			return new GamerAction(asyncState, callback);
		}

		public AchievementCollection EndGetAchievements(IAsyncResult result)
		{

			// TODO: Actual stuff?! -flibit
			return null;
		}

		#endregion
	}
}
