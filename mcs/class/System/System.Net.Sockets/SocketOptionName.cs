// SocketOptionName.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:33:02 UTC
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


namespace System.Net.Sockets {


	/// <summary>
	/// </summary>
	public enum SocketOptionName {

		/// <summary>
		/// </summary>
		Debug = 1,

		/// <summary>
		/// </summary>
		AcceptConnection = 2,

		/// <summary>
		/// </summary>
		ReuseAddress = 4,

		/// <summary>
		/// </summary>
		KeepAlive = 8,

		/// <summary>
		/// </summary>
		DontRoute = 16,

		/// <summary>
		/// </summary>
		Broadcast = 32,

		/// <summary>
		/// </summary>
		UseLoopback = 64,

		/// <summary>
		/// </summary>
		Linger = 128,

		/// <summary>
		/// </summary>
		OutOfBandInline = 256,

		/// <summary>
		/// </summary>
		DontLinger = -129,

		/// <summary>
		/// </summary>
		ExclusiveAddressUse = -5,

		/// <summary>
		/// </summary>
		SendBuffer = 4097,

		/// <summary>
		/// </summary>
		ReceiveBuffer = 4098,

		/// <summary>
		/// </summary>
		SendLowWater = 4099,

		/// <summary>
		/// </summary>
		ReceiveLowWater = 4100,

		/// <summary>
		/// </summary>
		SendTimeout = 4101,

		/// <summary>
		/// </summary>
		ReceiveTimeout = 4102,

		/// <summary>
		/// </summary>
		Error = 4103,

		/// <summary>
		/// </summary>
		Type = 4104,

		/// <summary>
		/// </summary>
		MaxConnections = 2147483647,

		/// <summary>
		/// </summary>
		IPOptions = 1,

		/// <summary>
		/// </summary>
		HeaderIncluded = 2,

		/// <summary>
		/// </summary>
		TypeOfService = 3,

		/// <summary>
		/// </summary>
		IpTimeToLive = 4,

		/// <summary>
		/// </summary>
		MulticastInterface = 9,

		/// <summary>
		/// </summary>
		MulticastTimeToLive = 10,

		/// <summary>
		/// </summary>
		MulticastLoopback = 11,

		/// <summary>
		/// </summary>
		AddMembership = 12,

		/// <summary>
		/// </summary>
		DropMembership = 13,

		/// <summary>
		/// </summary>
		DontFragment = 14,

		/// <summary>
		/// </summary>
		AddSourceMembership = 15,

		/// <summary>
		/// </summary>
		DropSourceMembership = 16,

		/// <summary>
		/// </summary>
		BlockSource = 17,

		/// <summary>
		/// </summary>
		UnblockSource = 18,

		/// <summary>
		/// </summary>
		PacketInformation = 19,

		/// <summary>
		/// </summary>
		NoDelay = 1,

		/// <summary>
		/// </summary>
		BsdUrgent = 2,

		/// <summary>
		/// </summary>
		Expedited = 2,

		/// <summary>
		/// </summary>
		NoChecksum = 1,

		/// <summary>
		/// </summary>
		ChecksumCoverage = 20,

		HopLimit = 21,
		UpdateAcceptContext = 28683,
		UpdateConnectContext = 28688,

	} // SocketOptionName

} // System.Net.Sockets
