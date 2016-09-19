#region License
/* FNA.Steamworks - XNA4 Xbox Live Reimplementation for Steamworks
 * Copyright 2016 Ethan "flibitijibibo" Lee
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.GamerServices
{
	public sealed class FriendGamer : Gamer
	{
		#region Public Properties

		public bool FriendRequestReceivedFrom
		{
			get;
			private set;
		}

		public bool FriendRequestSentTo
		{
			get;
			private set;
		}

		public bool HasVoice
		{
			get;
			private set;
		}

		public bool InviteAccepted
		{
			get;
			private set;
		}

		public bool InviteReceivedFrom
		{
			get;
			private set;
		}

		public bool InviteRejected
		{
			get;
			private set;
		}

		public bool InviteSentTo
		{
			get;
			private set;
		}

		public bool IsAway
		{
			get;
			private set;
		}

		public bool IsBusy
		{
			get;
			private set;
		}

		public bool IsJoinable
		{
			get;
			private set;
		}

		public bool IsOnline
		{
			get;
			private set;
		}

		public bool IsPlaying
		{
			get;
			private set;
		}

		public string Presence
		{
			get;
			private set;
		}

		#endregion
	}
}
