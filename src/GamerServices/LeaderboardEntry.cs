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
	public sealed class LeaderboardEntry
	{
		#region Public Properties

		public PropertyDictionary Columns
		{
			// TODO: https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.gamerservices.leaderboardentry.columns.aspx
			get;
			private set;
		}

		public Gamer Gamer
		{
			get;
			private set;
		}

		public long Rating
		{
			get;
			set;
		}

		#endregion
	}
}
