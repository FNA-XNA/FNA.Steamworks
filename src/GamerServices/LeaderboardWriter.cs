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
	public sealed class LeaderboardWriter
	{
		#region Public Methods

		public LeaderboardEntry GetLeaderboard(LeaderboardIdentity leaderboardId)
		{
			// FIXME: Do these other parameters even matter? -flibit
			return new LeaderboardEntry(
				null,
				0,
				0,
				LeaderboardReader.Leaderboards[leaderboardId.Key]
			);
		}

		#endregion
	}
}
