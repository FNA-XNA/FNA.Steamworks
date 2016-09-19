#region License
/* FNA.Steamworks - XNA4 Xbox Live Reimplementation for Steamworks
 * Copyright 2016 Ethan "flibitijibibo" Lee
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Net
{
	public sealed class AvailableNetworkSession
	{
		#region Public Properties

		public int CurrentGamerCount
		{
			get;
			private set;
		}

		public string HostGamertag
		{
			get;
			private set;
		}

		public int OpenPrivateGamerSlots
		{
			get;
			private set;
		}

		public int OpenPublicGamerSlots
		{
			get;
			private set;
		}

		public QualityOfService QualityOfService
		{
			get;
			private set;
		}

		public NetworkSessionProperties SessionProperties
		{
			get;
			private set;
		}

		#endregion
	}
}
