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

		public override bool Equals (object o)
		{	
			return false;
		}

		public override int GetHashCode()
		{
			return group.GetHashCode();
		}

		public override string ToString()
		{
			return group.ToString() + " " + local.ToString();
		}
	}
}
