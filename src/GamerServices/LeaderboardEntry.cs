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
			get
			{
				return rating;
			}
			set
			{
				rating = value;
				SteamUserStats.UploadLeaderboardScore(
					leaderboard,
					ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest,
					(int) rating,
					null,
					0
				);
			}
		}

		public int RankingEXT
		{
			get;
			private set;
		}

		#endregion

		#region Private Variables

		private long rating;
		private SteamLeaderboard_t leaderboard;

		#endregion

		#region Internal Constructor

		internal LeaderboardEntry(
			Gamer gamer,
			long rating,
			int ranking,
			SteamLeaderboard_t leaderboard
		) {
			Gamer = gamer;
			this.rating = rating;
			this.leaderboard = leaderboard;

			RankingEXT = ranking;

			Columns = new PropertyDictionary(
				new System.Collections.Generic.Dictionary<string, object>()
			);
		}

		#endregion
	}
}
