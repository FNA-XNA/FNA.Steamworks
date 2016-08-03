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

		#region Internal Static Variables

		internal static bool SteamAvailable;

		#endregion

		#region Public Static Methods

		public static void Initialize(IServiceProvider serviceProvider)
		{
			try
			{
				SteamAvailable = SteamAPI.Init();
				if (SteamAvailable)
				{
					AppDomain.CurrentDomain.ProcessExit += (o, e) => SteamAPI.Shutdown();
				}
			}
			catch
			{
				SteamAvailable = false;
			}
		}

		public static void Update()
		{
			if (SteamAvailable)
			{
				SteamAPI.RunCallbacks();
			}
		}

		#endregion
	}
}
