#region License
/* FNA.Steamworks - XNA4 Xbox Live Reimplementation for Steamworks
 * Copyright 2016 Ethan "flibitijibibo" Lee
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace Microsoft.Xna.Framework.Net
{
	public sealed class LocalNetworkGamer : NetworkGamer
	{
		#region Public Properties

		public bool IsDataAvailable
		{
			get;
			private set;
		}

		public SignedInGamer SignedInGamer
		{
			get;
			private set;
		}

		#endregion

		#region Public Methods

		public void EnableSendVoice(NetworkGamer remoteGamer, bool enable)
		{
			// TODO: Actual stuff?! -flibit
		}

		public void SendPartyInvites()
		{
			// TODO: Actual stuff?! -flibit
		}

		public int ReceiveData(byte[] data, out NetworkGamer gamer)
		{
			return ReceiveData(data, 0, out gamer);
		}

		public int ReceiveData(byte[] data, int offset, out NetworkGamer sender)
		{
			// TODO: Actual stuff?! -flibit
			sender = null;
			return 0;
		}

		public int ReceiveData(PacketReader data, out NetworkGamer sender)
		{
			// TODO: Actual stuff?! -flibit
			sender = null;
			return 0;
		}

		public void SendData(byte[] data, SendDataOptions options)
		{
			SendData(data, 0, data.Length, options);
		}

		public void SendData(
			byte[] data,
			int offset,
			int count,
			SendDataOptions options
		) {
			foreach (NetworkGamer gamer in Session.AllGamers)
			{
				SendData(data, offset, count, options, gamer);
			}
		}

		public void SendData(
			byte[] data,
			SendDataOptions options,
			NetworkGamer recipient
		) {
			SendData(data, 0, data.Length, options, recipient);
		}

		public void SendData(
			byte[] data,
			int offset,
			int count,
			SendDataOptions options,
			NetworkGamer recipient
		) {
			// TODO: Actual stuff?! -flibit
		}

		public void SendData(PacketWriter data, SendDataOptions options)
		{
			// FIXME: Do we want to alloc like this? -flibit
			byte[] mem = (data.BaseStream as System.IO.MemoryStream).ToArray();
			foreach (NetworkGamer gamer in Session.AllGamers)
			{
				SendData(
					mem,
					0,
					mem.Length,
					options,
					gamer
				);
			}
		}

		public void SendData(
			PacketWriter data,
			SendDataOptions options,
			NetworkGamer recipient
		) {
			// FIXME: Do we want to alloc like this? -flibit
			byte[] mem = (data.BaseStream as System.IO.MemoryStream).ToArray();
			SendData(
				mem,
				0,
				mem.Length,
				options,
				recipient
			);
		}

		#endregion
	}
}
