//
// System.Net.Sockets.MulticastOption.cs
//
// Author:
//   Andrew Sutton
//
// (C) Andrew Sutton
//

using System;
using System.Net;

namespace System.Net.Sockets
{
#if NET_1_1
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

		public IPv6MulticastOption (IPAddress group, long ifIndex)
		{
			if (group == null)
				throw new ArgumentNullException ("grp");

			this.group = group;
			this.ifIndex = ifIndex;
		}

		public IPAddress Group
		{
			get { return group; }
			set { group = value; }
		}

		public long InterfaceIndex
		{
			get { return ifIndex; }
			set { ifIndex = value; }
		}
	}
#endif
}
