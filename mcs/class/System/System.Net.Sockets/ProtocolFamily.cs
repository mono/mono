//
// System.Net.Sockets.ProtocolFamily.cs
//
// Author:
//   Andrew Sutton
//
// (C) Andrew Sutton
//

namespace System.Net.Sockets
{
	public enum ProtocolFamily
	{
		Unknown = -1,
		Unspecified = 0,
		Unix,
		InterNetwork,
		ImpLink,
		Pup,
		Chaos,
		Ipx,
		Iso,
		Ecma,
		DataKit,
		Ccitt,
		Sna,
		DecNet,
		DataLink,
		Lat,
		HyperChannel,
		AppleTalk,
		NetBios,
		VoiceView,
		FireFox,
		Banyan = 0x15,
		Atm,
		InterNetworkV6,
		Cluster,
		Ieee12844,
		Irda,
		NetworkDesigners = 0x1c,
		Max,

		NS = Ipx,
		Osi = Iso,
	}
}
