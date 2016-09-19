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
#endregion

namespace Microsoft.Xna.Framework.GamerServices
{
	public static class Guide
	{
		#region Public Static Properties

		public static bool IsScreenSaverEnabled
		{
			// TODO: SDL directly? -flibit
			get;
			set;
		}

		public static bool IsTrialMode
		{
			get;
			set;
		}

		public static bool IsVisible
		{
			get;
			set;
		}

		public static NotificationPosition NotificationPosition
		{
			get;
			set;
		}

		public static bool SimulateTrialMode
		{
			get;
			set;
		}

		#endregion

		#region Public Static Methods

		public static IAsyncResult BeginShowKeyboardInput(
			PlayerIndex player,
			string title,
			string description,
			string defaultText,
			AsyncCallback callback,
			object state
		) {
			return BeginShowKeyboardInput(
				player,
				title,
				description,
				defaultText,
				callback,
				state,
				false
			);
		}

		public static IAsyncResult BeginShowKeyboardInput(
			PlayerIndex player,
			string title,
			string description,
			string defaultText,
			AsyncCallback callback,
			object state,
			bool usePasswordMode
		) {
			// TODO: Actual stuff?! -flibit
			return null;
		}

		public static string EndShowKeyboardInput(IAsyncResult result)
		{
			// TODO: Actual stuff?! -flibit
			return string.Empty;
		}

		public static IAsyncResult BeginShowMessageBox(
			string title,
			string text,
			IEnumerable<string> buttons,
			int focusButton,
			MessageBoxIcon icon,
			AsyncCallback callback,
			object state
		) {
			// TODO: SDL directly? -flibit
			return null;
		}

		public static IAsyncResult BeginShowMessageBox(
			PlayerIndex player,
			string title,
			string text,
			IEnumerable<string> buttons,
			int focusButton,
			MessageBoxIcon icon,
			AsyncCallback callback,
			object state
		) {
			// TODO: SDL directly? -flibit
			return null;
		}

		public static int? EndShowMessageBox(IAsyncResult result)
		{
			// TODO: Actual stuff?! -flibit
			return null;
		}

		public static void DelayNotifications(TimeSpan delay)
		{
			// TODO: How to prevent notifications? Max time is 120 seconds -flibit
		}

		public static void ShowComposeMessage(
			PlayerIndex player,
			string text,
			IEnumerable<Gamer> recipients
		) {
			// TODO: Actual stuff?! -flibit
		}

		public static void ShowFriendRequest(PlayerIndex player, Gamer gamer)
		{
			// TODO: Actual stuff?! -flibit
		}

		public static void ShowFriends(PlayerIndex player)
		{
			// TODO: Actual stuff?! -flibit
		}

		public static void ShowGameInvite(
			PlayerIndex player,
			IEnumerable<Gamer> recipients
		) {
			// TODO: Actual stuff?! -flibit
		}

		public static void ShowGameInvite(string sessionId)
		{
			// TODO: Actual stuff?! -flibit
		}

		public static void ShowGamerCard(PlayerIndex player, Gamer gamer)
		{
			// TODO: Actual stuff?! -flibit
		}

		public static void ShowMarketPlace(PlayerIndex player)
		{
			// TODO: Actual stuff?! -flibit
		}

		public static void ShowMessages(PlayerIndex player)
		{
			// TODO: Actual stuff?! -flibit
		}

		public static void ShowParty(PlayerIndex player)
		{
			// TODO: Actual stuff?! -flibit
		}

		public static void ShowPartySessions(PlayerIndex player)
		{
			// TODO: Actual stuff?! -flibit
		}

		public static void ShowPlayerReview(PlayerIndex player, Gamer gamer)
		{
			// TODO: Actual stuff?! -flibit
		}

		public static void ShowPlayers(PlayerIndex player)
		{
			// TODO: Actual stuff?! -flibit
		}

		public static void ShowSignIn(int paneCount, bool onlineOnly)
		{
			// TODO: Actual stuff?! -flibit
		}

		#endregion
	}
}
