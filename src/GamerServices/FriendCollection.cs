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
	public sealed class FriendCollection : GamerCollection<FriendGamer>, IDisposable
	{
		#region Public Properties

		public bool IsDisposed
		{
			get;
			private set;
		}

		#endregion

		#region Internal Constructor

		internal FriendCollection(List<FriendGamer> friends) : base(friends)
		{
			IsDisposed = false;
		}

		#endregion

		#region Public Methods

		public void Dispose()
		{
			if (!IsDisposed)
			{
				collection.Clear();
				IsDisposed = true;
			}
		}

		#endregion
	}
}
