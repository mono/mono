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
	// <remarks>
	//   Encapsulates a multicast option
	// </remarks>
	public class MulticastOption
	{
		// Don't change the names of these fields without also
		// changing socket-io.c in the runtime
		protected IPAddress group;
		protected IPAddress local;

		public MulticastOption (IPAddress grp)
		{
			group = grp;
		}

		public MulticastOption (IPAddress grp, IPAddress addr)
		{
			group = grp;
			local = addr;
		}

		public IPAddress Group
		{
			get { return group; }
			set { group = value; }
		}

		public IPAddress LocalAddress
		{
			get { return local; }
			set { local = value; }
		}
	}
}
