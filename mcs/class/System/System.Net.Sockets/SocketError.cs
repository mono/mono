//
// System.Net.Sockets.SocketError.cs
//
// Author:
//	Robert Jordan  <robertj@gmx.net>
//
// Copyright (C) 2005 Novell, Inc. (http://www.novell.com)
//

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

namespace System.Net.Sockets
{
	public enum SocketError
	{
		AccessDenied = 10013,
		AddressAlreadyInUse = 10048,
		AddressFamilyNotSupported = 10047,
		AddressNotAvailable = 10049,
		AlreadyInProgress = 10037,
		ConnectionAborted = 10053,
		ConnectionRefused = 10061,
		ConnectionReset = 10054,
		DestinationAddressRequired = 10039,
		Disconnecting = 10101,
		Fault = 10014,
		HostDown = 10064,
		HostNotFound = 11001,
		HostUnreachable = 10065,
		InProgress = 10036,
		Interrupted = 10004,
		InvalidArgument = 10022,
		IOPending = 997,
		IsConnected = 10056,
		MessageSize = 10040,
		NetworkDown = 10050,
		NetworkReset = 10052,
		NetworkUnreachable = 10051,
		NoBufferSpaceAvailable = 10055,
		NoData = 11004,
		NoRecovery = 11003,
		NotConnected = 10057,
		NotInitialized = 10093,
		NotSocket = 10038,
		OperationAborted = 995,
		OperationNotSupported = 10045,
		ProcessLimit = 10067,
		ProtocolFamilyNotSupported = 10046,
		ProtocolNotSupported = 10043,
		ProtocolOption = 10042,
		ProtocolType = 10041,
		Shutdown = 10058,
		SocketError = -1,
		SocketNotSupported = 10044,
		Success = 0,
		SystemNotReady = 10091,
		TimedOut = 10060,
		TooManyOpenSockets = 10024,
		TryAgain = 11002,
		TypeNotFound = 10109,
		VersionNotSupported = 10092,
		WouldBlock = 10035
	}
}
