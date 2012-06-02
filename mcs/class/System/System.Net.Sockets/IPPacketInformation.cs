//
// System.Net.Sockets.IPPacketInformation.cs
//
// Author:
//	Dick Porter (dick@ximian.com)
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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

using System;
using System.Net;

namespace System.Net.Sockets
{
	public struct IPPacketInformation 
	{
		IPAddress address;
		int iface;
		
		internal IPPacketInformation (IPAddress address, int iface)
		{
			this.address = address;
			this.iface = iface;
		}
		
		public IPAddress Address
		{
			get {
				return(address);
			}
		}
		
		public int Interface
		{
			get {
				return(iface);
			}
		}

		public override bool Equals (object comparand)
		{
			if (!(comparand is IPPacketInformation)) {
				return(false);
			}
			
			IPPacketInformation packet = (IPPacketInformation)comparand;
			
			if (packet.iface != iface) {
				return(false);
			}
			
			return(packet.address.Equals (address));
		}
		
		public override int GetHashCode ()
		{
			/* FIXME: see if we can work out the MS algorithm */
			return(address.GetHashCode () + iface);
		}
		
		public static bool operator== (IPPacketInformation p1,
					       IPPacketInformation p2)
		{
			return(p1.Equals (p2));
		}

		public static bool operator!= (IPPacketInformation p1,
					       IPPacketInformation p2)
		{
			return(!p1.Equals (p2));
		}
	}
}

