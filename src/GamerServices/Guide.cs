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
using System.Threading;

using Steamworks;
#endregion

namespace Microsoft.Xna.Framework.GamerServices
{
	public static class Guide
	{
		#region Public Static Properties

		public static bool IsScreenSaverEnabled
		{
			// FIXME: Should we use SDL here? -flibit
			get
			{
				return SDL2.SDL.SDL_IsScreenSaverEnabled() == SDL2.SDL.SDL_bool.SDL_TRUE;
			}
			set
			{
				if (value)
				{
					SDL2.SDL.SDL_EnableScreenSaver();
				}
				else
				{
					SDL2.SDL.SDL_DisableScreenSaver();
				}
			}
		}

		public static bool IsTrialMode
		{
			get;
			set;
		}

		public static bool IsVisible
		{
			get
			{
				return overlayActive;
			}
			set
			{
				if (value)
				{
					SteamFriends.ActivateGameOverlay(null);
				}
				else
				{
					// FIXME: No mechanism to close overlay -flibit
				}
			}
		}

		public static NotificationPosition NotificationPosition
		{
			get
			{
				return position;
			}
			set
			{
				if (value != position)
				{
					position = value;
					SteamUtils.SetOverlayNotificationPosition(
						positions[(int) position]
					);
				}
			}
		}

		public static bool SimulateTrialMode
		{
			get;
			set;
		}

		#endregion

		#region Private Static Variables

		private static bool overlayActive = false;
		private static NotificationPosition position = NotificationPosition.BottomRight;

		private static readonly ENotificationPosition[] positions = new ENotificationPosition[]
		{
			ENotificationPosition.k_EPositionBottomRight, // FIXME
			ENotificationPosition.k_EPositionBottomLeft,
			ENotificationPosition.k_EPositionBottomRight,
			ENotificationPosition.k_EPositionBottomRight, // FIXME
			ENotificationPosition.k_EPositionBottomLeft, // FIXME
			ENotificationPosition.k_EPositionBottomRight, // FIXME
			ENotificationPosition.k_EPositionTopRight, // FIXME
			ENotificationPosition.k_EPositionTopLeft,
			ENotificationPosition.k_EPositionTopRight
		};

		private static KeyboardAction keyboardAction;

		#endregion

		#region Async Object Types

		private class KeyboardAction : IAsyncResult
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

			public string TextInput;

			public KeyboardAction(object state, AsyncCallback callback, string textInput)
			{
				TextInput = textInput;

				AsyncState = state;
				Callback = callback;
				IsCompleted = false;
				AsyncWaitHandle = new ManualResetEvent(true);
			}
		}

		#endregion

		#region Static Constructor

		static Guide()
		{
			IsTrialMode = false;
			SimulateTrialMode = false;
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
			SteamUtils.ShowGamepadTextInput(
				usePasswordMode ?
					EGamepadTextInputMode.k_EGamepadTextInputModePassword :
					EGamepadTextInputMode.k_EGamepadTextInputModeNormal,
				EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine,
				title + ": " + description,
				4096, // FIXME
				defaultText
			);
			keyboardAction = new KeyboardAction(state, callback, defaultText);
			return keyboardAction;
		}

		public static string EndShowKeyboardInput(IAsyncResult result)
		{
			string text = keyboardAction.TextInput;
			keyboardAction = null;
			return text;
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
			// FIXME: Surely they don't want us doing this... -flibit
			throw new NotSupportedException();
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
			// FIXME: Surely they don't want us doing this... -flibit
			throw new NotSupportedException();
		}

		public static int? EndShowMessageBox(IAsyncResult result)
		{
			// FIXME: Surely they don't want us doing this... -flibit
			throw new NotSupportedException();
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
			// FIXME: Could be a lobby, could be one recipient? -flibit
			throw new NotSupportedException();
		}

		public static void ShowFriendRequest(PlayerIndex player, Gamer gamer)
		{
			SteamFriends.ActivateGameOverlayToUser(
				"friendadd",
				gamer.steamID
			);
		}

		public static void ShowFriends(PlayerIndex player)
		{
			SteamFriends.ActivateGameOverlay("Friends");
		}

		public static void ShowGameInvite(
			PlayerIndex player,
			IEnumerable<Gamer> recipients
		) {
			// FIXME: Could be a lobby, could be one recipient? -flibit
			throw new NotSupportedException();
		}

		public static void ShowGameInvite(string sessionId)
		{
			// TODO: SteamFriends.ActivateGameOverlayInviteDialog();
			throw new NotSupportedException();
		}

		public static void ShowGamerCard(PlayerIndex player, Gamer gamer)
		{
			SteamFriends.ActivateGameOverlayToUser("steamid", gamer.steamID);
		}

		public static void ShowMarketPlace(PlayerIndex player)
		{
			SteamFriends.ActivateGameOverlayToStore(
				SteamUtils.GetAppID(),
				EOverlayToStoreFlag.k_EOverlayToStoreFlag_None
			);
		}

		public static void ShowMessages(PlayerIndex player)
		{
			// If a message is pending then it'll just be there? -flibit
			SteamFriends.ActivateGameOverlay(null);
		}

		public static void ShowParty(PlayerIndex player)
		{
			// FIXME: Lobbies? -flibit
			throw new NotSupportedException();
		}

		public static void ShowPartySessions(PlayerIndex player)
		{
			// FIXME: Lobbies? -flibit
			throw new NotSupportedException();
		}

		public static void ShowPlayerReview(PlayerIndex player, Gamer gamer)
		{
			// Comments/Rating are on the user profile
			SteamFriends.ActivateGameOverlayToUser("steamid", gamer.steamID);
		}

		public static void ShowPlayers(PlayerIndex player)
		{
			SteamFriends.ActivateGameOverlay("Players");
		}

		public static void ShowSignIn(int paneCount, bool onlineOnly)
		{
			// No-op until multiple users can sign in!
		}

		#endregion

		#region Internal Static Methods

		internal static void OnOverlayActivated(GameOverlayActivated_t active)
		{
			overlayActive = active.m_bActive > 0;
		}

		internal static void OnTextInputDismissed(GamepadTextInputDismissed_t text)
		{
			if (text.m_bSubmitted)
			{
				SteamUtils.GetEnteredGamepadTextInput(
					out keyboardAction.TextInput,
					4096 // FIXME
				);
			}
		}

		#endregion
	}
}
