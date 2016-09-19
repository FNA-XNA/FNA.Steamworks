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

		public static SignedInGamerCollection SignedInGamers
		{
			get;
			internal set;
		}

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
			// TODO: Actual stuff?!
			return new GamerAction(asyncState, callback);
		}

		public GamerProfile EndGetProfile(IAsyncResult result)
		{
			// TODO: Actual stuff?!
			return null;
		}

		#endregion

		#region Public Static Methods

		public static Gamer GetFromGamertag(string gamertag)
		{
			IAsyncResult result = BeginGetFromGamertag(gamertag, null, null);
			result.AsyncWaitHandle.WaitOne();
			return EndGetFromGamertag(result);
		}

		public static IAsyncResult BeginGetFromGamertag(
			string gamertag,
			AsyncCallback callback,
			object asyncState
		) {
			// TODO: Actual stuff?!
			return new GamerAction(asyncState, callback);
		}

		public static Gamer EndGetFromGamertag(IAsyncResult result)
		{
			// TODO: Actual stuff?!
			return null;
		}

		public static string GetPartnerToken(string audienceUri)
		{
			IAsyncResult result = BeginGetPartnerToken(audienceUri, null, null);
			result.AsyncWaitHandle.WaitOne();
			return EndGetPartnerToken(result);
		}

		public static IAsyncResult BeginGetPartnerToken(
			string audienceUri,
			AsyncCallback callback,
			object asyncState
		) {
			// TODO: Actual stuff?!
			return new GamerAction(asyncState, callback);
		}

		public static string EndGetPartnerToken(IAsyncResult result)
		{
			// TODO: Actual stuff?!
			return string.Empty;
		}

		#endregion
	}
}
