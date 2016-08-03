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
	public sealed class NetworkSession : IDisposable
	{
		#region Public Properties

		public bool IsDisposed
		{
			get;
			private set;
		}

		#endregion

		#region Internal Constructor

		internal NetworkSession()
		{
			IsDisposed = false;
		}

		#endregion

		#region Public Methods

		public void Dispose()
		{
			IsDisposed = true;
		}

		#endregion
	}
}
