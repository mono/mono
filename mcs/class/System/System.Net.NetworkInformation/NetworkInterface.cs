//
// System.Net.NetworkInformation.NetworkInterface
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//      Miguel de Icaza (miguel@novell.com)
//      Eric Butler (eric@extremeboredom.net)
//      Marek Habersack (mhabersack@novell.com)
//  Marek Safar (marek.safar@gmail.com)
//
// Copyright (c) 2006-2008 Novell, Inc. (http://www.novell.com)
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
using System;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Globalization;

namespace System.Net.NetworkInformation {
	static class SystemNetworkInterface {

		static readonly NetworkInterfaceFactory nif = NetworkInterfaceFactory.Create ();

		public static NetworkInterface [] GetNetworkInterfaces ()
		{
			try {
				return nif.GetAllNetworkInterfaces ();
			} catch {
				return new NetworkInterface [0];
			}
		}

		public static bool InternalGetIsNetworkAvailable ()
		{
			// TODO:
			return true;
		}

		public static int InternalLoopbackInterfaceIndex {
			get {
				return nif.GetLoopbackInterfaceIndex ();
			}
		}

		public static int InternalIPv6LoopbackInterfaceIndex {
			get {
				throw new NotImplementedException ();
			}
		}

		public static IPAddress GetNetMask (IPAddress address)
		{
			return nif.GetNetMask (address);
		}
	}

	abstract class NetworkInterfaceFactory
	{
		public abstract NetworkInterface [] GetAllNetworkInterfaces ();
		public abstract int GetLoopbackInterfaceIndex ();
		public abstract IPAddress GetNetMask (IPAddress address);

		public static NetworkInterfaceFactory Create ()
		{
			return NetworkInterfaceFactoryPal.Create ();
		}
	}
}
