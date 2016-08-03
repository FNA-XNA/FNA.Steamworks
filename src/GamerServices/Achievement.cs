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
using System.IO;
#endregion

namespace Microsoft.Xna.Framework.GamerServices
{
	public sealed class Achievement
	{
		#region Public Properties

		public string Description
		{
			get;
			private set;
		}

		public bool DisplayBeforeEarned
		{
			get;
			private set;
		}

		public DateTime EarnedDateTime
		{
			get;
			private set;
		}

		public bool EarnedOnline
		{
			get;
			private set;
		}

		public int GamerScore
		{
			get;
			private set;
		}

		public string HowToEarn
		{
			get;
			private set;
		}

		public bool IsEarned
		{
			get;
			private set;
		}

		public string Key
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			private set;
		}

		#endregion

		#region Public Methods

		public Stream GetPicture()
		{
			return null;
		}

		#endregion
	}
}
