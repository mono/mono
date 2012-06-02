//
// System.Net.Sockets.IOControlCode.cs
//
// Author:
//	Dick Porter (dick@ximian.com)
//
// (C) Copyright 2006 Novell, Inc (http://www.novell.com)
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
	public enum IOControlCode: long
	{
		AbsorbRouterAlert			= 0x98000005,
		AddMulticastGroupOnInterface		= 0x9800000A,
		AddressListChange			= 0x28000017,
		AddressListQuery			= 0x48000016,
		AddressListSort				= 0xC8000019,
		AssociateHandle				= 0x88000001,
		AsyncIO					= 0x8004667D,
		BindToInterface				= 0x98000008,
		DataToRead				= 0x4004667F,
		DeleteMulticastGroupFromInterface	= 0x9800000B,
		EnableCircularQueuing			= 0x28000002,
		Flush					= 0x28000004,
		GetBroadcastAddress			= 0x48000005,
		GetExtensionFunctionPointer		= 0xC8000006,
		GetGroupQos				= 0xC8000008,
		GetQos					= 0xC8000007,
		KeepAliveValues				= 0x98000004,
		LimitBroadcasts				= 0x98000007,
		MulticastInterface			= 0x98000009,
		MulticastScope				= 0x8800000A,
		MultipointLoopback			= 0x88000009,
		NamespaceChange				= 0x88000019,
		NonBlockingIO				= 0x8004667E,
		OobDataRead				= 0x40047307,
		QueryTargetPnpHandle			= 0x48000018,
		ReceiveAll				= 0x98000001,
		ReceiveAllIgmpMulticast			= 0x98000003,
		ReceiveAllMulticast			= 0x98000002,
		RoutingInterfaceChange			= 0x88000015,
		RoutingInterfaceQuery			= 0xC8000014,
		SetGroupQos				= 0x8800000C,
		SetQos					= 0x8800000B,
		TranslateHandle				= 0xC800000D,
		UnicastInterface			= 0x98000006,
	}
}
