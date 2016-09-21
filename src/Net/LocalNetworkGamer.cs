#region License
/* FNA.Steamworks - XNA4 Xbox Live Reimplementation for Steamworks
 * Copyright 2016 Ethan "flibitijibibo" Lee
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using Steamworks;

using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace Microsoft.Xna.Framework.Net
{
	public sealed class LocalNetworkGamer : NetworkGamer
	{
		#region Public Properties

		public bool IsDataAvailable
		{
			get
			{
				uint data;
				return SteamNetworking.IsP2PPacketAvailable(out data);
			}
		}

		public SignedInGamer SignedInGamer
		{
			get;
			private set;
		}

		#endregion

		#region Internal Constructor

		internal LocalNetworkGamer(
			SignedInGamer gamer,
			NetworkSession session
		) : base(
			gamer.steamID,
			session
		) {
			SignedInGamer = gamer;
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
			uint len = 0;
			sender = null;

			if (!SteamNetworking.IsP2PPacketAvailable(out len))
			{
				return (int) len;
			}

			CSteamID id;
			SteamNetworking.ReadP2PPacket(
				data, // FIXME: offset! -flibit
				(uint) data.Length,
				out len,
				out id
			);

			foreach (NetworkGamer gamer in Session.AllGamers)
			{
				if (gamer.steamID == id)
				{
					sender = gamer;
					return (int) len;
				}
			}

			// We should never get here!
			return (int) len;
		}

		public int ReceiveData(PacketReader data, out NetworkGamer sender)
		{
			uint len = 0;
			sender = null;

			if (!SteamNetworking.IsP2PPacketAvailable(out len))
			{
				return (int) len;
			}

			// FIXME: Do we want to alloc like this? -flibit
			byte[] buf = new byte[len];

			CSteamID id;
			SteamNetworking.ReadP2PPacket(
				buf,
				(uint) buf.Length,
				out len,
				out id
			);

			data.BaseStream.Write(buf, 0, (int) len);
			data.BaseStream.Seek(-len, System.IO.SeekOrigin.Current);

			foreach (NetworkGamer gamer in Session.AllGamers)
			{
				if (gamer.steamID == id)
				{
					sender = gamer;
					return (int) len;
				}
			}

			// We should never get here!
			return (int) len;
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
			SteamNetworking.SendP2PPacket(
				recipient.steamID,
				data,
				(uint) data.Length,
				EP2PSend.k_EP2PSendUnreliable // FIXME
			);
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
