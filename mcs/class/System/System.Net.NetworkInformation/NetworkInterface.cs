//
// System.Net.NetworkInformation.NetworkInterface
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
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
#if NET_2_0
using System;
using System.Net;

namespace System.Net.NetworkInformation {
	public abstract class NetworkInterface {
		protected NetworkInterface ()
		{
		}

		[MonoTODO]
		public static NetworkInterface [] GetAllNetworkInterfaces ()
		{
			return new NetworkInterface [0];
		}

		[MonoTODO]
		public static bool GetIsNetworkAvailable ()
		{
			return true;
		}

		[MonoTODO]
		public static int LoopbackInterfaceIndex {
			get { return 0; }
		}

		public abstract IPInterfaceProperties GetIPProperties ();
		public abstract IPv4InterfaceStatistics GetIPv4Statistics ();
		public abstract PhysicalAddress GetPhysicalAddress ();
		public abstract bool Supports (NetworkInterfaceComponent networkInterfaceComponent);

		public abstract string Description { get; }
		public abstract string Id { get; }
		public abstract bool IsReceiveOnly { get; }
		public abstract string Name { get; }
		public abstract NetworkInterfaceType NetworkInterfaceType { get; }
		public abstract OperationalStatus OperationalStatus { get; }
		public abstract long Speed { get; }
		public abstract bool SupportsMulticast { get; }
	}
}
#endif

