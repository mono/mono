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
	public class MulticastOption
	{
		// Don't change the names of these fields without also
		// changing socket-io.c in the runtime
		private IPAddress group;
		private IPAddress local;
#if NET_2_0
		int iface_index;
#endif

		public MulticastOption (IPAddress grp)
			: this (grp, IPAddress.Any)
		{
			group = grp;
		}
#if NET_2_0
		[MonoTODO ("Get interface IP from interface index")]
		public MulticastOption (IPAddress group, int interfaceIndex)
		{
			this.group = group;
		}
#endif
		public MulticastOption (IPAddress grp, IPAddress addr)
		{
			if (grp == null)
				throw new ArgumentNullException ("grp");

			if (addr == null)
				throw new ArgumentNullException ("addr");

			group = grp;
			local = addr;
		}

		public IPAddress Group {
			get { return group; }
			set { group = value; }
		}

		public IPAddress LocalAddress {
			get { return local; }
			set { local = value; }
		}

#if NET_2_0
		public int InterfaceIndex {
			get { return iface_index; }
			set { iface_index = value; }
		}
#endif
	}
}

