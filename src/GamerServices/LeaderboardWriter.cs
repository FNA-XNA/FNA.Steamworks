#region License
/* FNA.Steamworks - XNA4 Xbox Live Reimplementation for Steamworks
 * Copyright 2016 Ethan "flibitijibibo" Lee
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using Steamworks;
#endregion

namespace Microsoft.Xna.Framework.GamerServices
{
	public sealed class LeaderboardWriter
	{
		#region Private Variables

		private string latestKey;

		#endregion

		#region Public Methods

		public LeaderboardEntry GetLeaderboard(LeaderboardIdentity leaderboardId)
		{
			// FIXME: Should we just magically have leaderboards by this point? -flibit
			if (!LeaderboardReader.Leaderboards.ContainsKey(leaderboardId.Key))
			{
				latestKey = leaderboardId.Key;
				SteamAPICall_t call = SteamUserStats.FindLeaderboard(leaderboardId.Key);
				if (call.m_SteamAPICall != 0)
				{
					CallResult<LeaderboardFindResult_t> foundLeaderboard;
					foundLeaderboard = new CallResult<LeaderboardFindResult_t>();
					foundLeaderboard.Set(
						call,
						OnLeaderboardFound
					);
					while (GamerServicesDispatcher.UpdateAsync())
					{
						if (string.IsNullOrEmpty(latestKey))
						{
							break;
						}
					}
				}
			}

			// FIXME: Do these other parameters even matter? -flibit
			return new LeaderboardEntry(
				null,
				0,
				0,
				LeaderboardReader.Leaderboards[leaderboardId.Key]
			);
		}

		#endregion

		#region Private Methods

		private void OnLeaderboardFound(
			LeaderboardFindResult_t board,
			bool bIOFailure
		) {
			if (!bIOFailure && board.m_bLeaderboardFound > 0)
			{
				LeaderboardReader.Leaderboards.Add(
					latestKey,
					board.m_hSteamLeaderboard
				);
			}
			latestKey = null;
		}

		#endregion
	}
}
