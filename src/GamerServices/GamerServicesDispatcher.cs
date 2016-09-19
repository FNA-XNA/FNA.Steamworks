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
#endregion

namespace Microsoft.Xna.Framework.GamerServices
{
	public static class GamerServicesDispatcher
	{
		#region Public Static Properties

		public static bool IsInitialized
		{
			get;
			private set;
		}

		public static IntPtr WindowHandle
		{
			get;
			set;
		}

		#endregion

		#region Public Static Events

#pragma warning disable 0067
		// This should never happen, but lol XNA4 compliance -flibit
		public static event EventHandler<EventArgs> InstallingTitleUpdate;
#pragma warning restore 0067

		#endregion

		#region Public Static Methods

		public static void Initialize(IServiceProvider serviceProvider)
		{
			bool success = SteamAPI.Init();
			AppDomain.CurrentDomain.ProcessExit += (o, e) => SteamAPI.Shutdown();

			List<SignedInGamer> startGamers = new List<SignedInGamer>(1);
			startGamers.Add(new SignedInGamer(success));
			Gamer.SignedInGamers = new SignedInGamerCollection(startGamers);
		}

		public static void Update()
		{
			SteamAPI.RunCallbacks();
		}

		#endregion
	}
}
