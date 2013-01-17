//
// System.Net.Sockets.MulticastOption.cs
//
// Author:
//   Andrew Sutton
//
// (C) Andrew Sutton
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
	// <remarks>
	//   Encapsulates a multicast option
	// </remarks>
	public class IPv6MulticastOption
	{
		// Don't change the names of these fields without also
		// changing socket-io.c in the runtime
		private IPAddress group;
		private long ifIndex;

		public IPv6MulticastOption (IPAddress group)
			: this (group, 0)
		{
		}

		public IPv6MulticastOption (IPAddress group, long ifindex)
		{
			if (group == null)
				throw new ArgumentNullException ("group");
			if (ifindex < 0 || ifindex > 0xffffffff)
				throw new ArgumentOutOfRangeException ("ifindex");

			this.group = group;
			this.ifIndex = ifindex;
		}

		public IPAddress Group
		{
			get { return group; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				group = value;
			}
		}

		public long InterfaceIndex
		{
			get { return ifIndex; }
			set {
				if (value < 0 || value > 0xffffffff)
					throw new ArgumentOutOfRangeException ("value");
				ifIndex = value;
			}
		}
	}
}
