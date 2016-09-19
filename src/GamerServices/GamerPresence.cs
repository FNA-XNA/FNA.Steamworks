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
	public sealed class GamerPresence
	{
		#region Public Properties

		public GamerPresenceMode PresenceMode
		{
			get;
			set;
		}

		public int PresenceValue
		{
			get;
			set;
		}

		#endregion

		#region Internal Constructor

		internal GamerPresence()
		{
			PresenceMode = new GamerPresenceMode();
			PresenceValue = 0;
		}

		#endregion
	}
}
