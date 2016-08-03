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

namespace Microsoft.Xna.Framework.GamerServices
{
	public class SignedInEventArgs : EventArgs
	{
		#region Public Properties

		public SignedInGamer Gamer
		{
			get;
			private set;
		}

		#endregion

		#region Public Constructor

		public SignedInEventArgs(SignedInGamer gamer)
		{
			Gamer = gamer;
		}

		#endregion
	}
}
