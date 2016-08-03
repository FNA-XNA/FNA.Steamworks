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
#endregion

namespace Microsoft.Xna.Framework.Net
{
	public sealed class WriteLeaderboardsEventArgs : EventArgs
	{
		#region Public Properties

		public NetworkGamer Gamer
		{
			get;
			private set;
		}

		public bool IsLeaving
		{
			get;
			private set;
		}

		#endregion

		#region Internal Constructor

		internal WriteLeaderboardsEventArgs(
			NetworkGamer gamer,
			bool isLeaving
		) {
			Gamer = gamer;
			IsLeaving = isLeaving;
		}

		#endregion
	}
}
