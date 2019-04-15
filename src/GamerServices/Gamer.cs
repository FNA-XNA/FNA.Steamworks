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

using Steamworks;
#endregion

namespace Microsoft.Xna.Framework.GamerServices
{
	public abstract class Gamer
	{
		#region Public Properties

		public string DisplayName
		{
			get;
			set;
		}

		public string Gamertag
		{
			get;
			internal set;
		}

		public bool IsDisposed
		{
			get;
			internal set;
		}

		public LeaderboardWriter LeaderboardWriter
		{
			get;
			internal set;
		}

		public object Tag
		{
			get;
			set;
		}

		private static SignedInGamerCollection INTERNAL_signedInGamers;
		public static SignedInGamerCollection SignedInGamers
		{
			get
			{
				if (INTERNAL_signedInGamers == null)
				{
					INTERNAL_signedInGamers = new SignedInGamerCollection(
						new System.Collections.Generic.List<SignedInGamer>()
					);
				}
				return INTERNAL_signedInGamers;
			}
			internal set
			{
				INTERNAL_signedInGamers = value;
			}
		}

		#endregion

		#region Internal Variables

		internal readonly CSteamID steamID;

		#endregion

		#region Async Object Type

		internal class GamerAction : IAsyncResult
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

			public GamerAction(object state, AsyncCallback callback)
			{
				AsyncState = state;
				Callback = callback;
				IsCompleted = false;
				AsyncWaitHandle = new ManualResetEvent(true);
			}
		}

		#endregion

		#region Internal Constructor

		internal Gamer(
			CSteamID id,
			string gamertag,
			string displayName = null
		) {
			steamID = id;
			Gamertag = gamertag;
			DisplayName = displayName ?? gamertag;

			// TODO: Everything below
			LeaderboardWriter = new LeaderboardWriter();
		}

		#endregion

		#region Public Methods

		public GamerProfile GetProfile()
		{
			IAsyncResult result = BeginGetProfile(null, null);
			result.AsyncWaitHandle.WaitOne();
			return EndGetProfile(result);
		}

		public IAsyncResult BeginGetProfile(
			AsyncCallback callback,
			object asyncState
		) {
			return new GamerAction(asyncState, callback)
			{
				IsCompleted = true
			};
		}

		public GamerProfile EndGetProfile(IAsyncResult result)
		{
			return new GamerProfile();
		}

		#endregion

		#region Public Static Methods

		public static Gamer GetFromGamertag(string gamertag)
		{
			throw new NotSupportedException();
		}

		public static IAsyncResult BeginGetFromGamertag(
			string gamertag,
			AsyncCallback callback,
			object asyncState
		) {
			throw new NotSupportedException();
		}

		public static Gamer EndGetFromGamertag(IAsyncResult result)
		{
			throw new NotSupportedException();
		}

		public static string GetPartnerToken(string audienceUri)
		{
			throw new NotSupportedException();
		}

		public static IAsyncResult BeginGetPartnerToken(
			string audienceUri,
			AsyncCallback callback,
			object asyncState
		) {
			throw new NotSupportedException();
		}

		public static string EndGetPartnerToken(IAsyncResult result)
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
