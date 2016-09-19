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
	public sealed class QualityOfService
	{
		#region Public Properties

		public TimeSpan AverageRoundtripTime
		{
			get;
			private set;
		}

		public int BytesPerSecondDownstream
		{
			get;
			private set;
		}

		public int BytesPerSecondUpstream
		{
			get;
			private set;
		}

		public bool IsAvailable
		{
			get;
			private set;
		}

		public TimeSpan MinimumRoundtripTime
		{
			get;
			private set;
		}

		#endregion
	}
}
