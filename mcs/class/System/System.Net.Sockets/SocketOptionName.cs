//
// System.Net.Sockets.SocketOptionName.cs
//
// Author:
//   Andrew Sutton
//
// (C) Andrew Sutton
//

namespace System.Net.Sockets
{
	public enum SocketOptionName
	{
		AcceptConnection,
		AddMembership,
		AddSourceMembership,
		BlockSource,
		Broadcast,
		BsdUrgent,
		ChecksumCoverage,
		Debug,
		DontFragment,
		DontLinger,
		DontRoute,
		DropMembership,
		DropSourceMembership,
		Error,
		ExclusiveAddress,
		Expedited,
		HeaderIncluded,
		IPOptions,
		IPTimeToLive,
		KeepAlive,
		Linger,
		MaxConnections,
		MulticastUnterface,
		MulticastLoopback,
		MulticastTimeToLive,
		NoChecksum,
		NoDelay,
		OutOfBandInline,
		PacketInformation,
		ReceiveBuffer,
		ReceiveLowWater,
		ReceiveTimeout,
		SendBuffer,
		SendLowWater,
		SendTimeout,
		Type,
		TypeOfService,
		UnblockSource,
		UseLoopback
	}
}