#region License
/* FNA.Steamworks - XNA4 Xbox Live Reimplementation for Steamworks
 * Copyright 2016 Ethan "flibitijibibo" Lee
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System.Collections.Generic;
#endregion

namespace Microsoft.Xna.Framework.GamerServices
{
	public sealed class SignedInGamerCollection : GamerCollection<SignedInGamer>
	{
		#region Public Properties

		public SignedInGamer this[PlayerIndex index]
		{
			get
			{
				return collection[(int) index];
			}
		}

		#endregion

		#region Internal Constructor

		internal SignedInGamerCollection(List<SignedInGamer> collection) : base(collection)
		{
		}

		#endregion
	}
}
