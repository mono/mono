//
// System.Net.Sockets.LingerOption.cs
//
// Author:
//   Andrew Sutton
//
// (C) Andrew Sutton
//

using System;

namespace System.Net.Sockets
{
	// <remarks>
	//   Encapsulates a linger option.
	// </remarks>
	public class LingerOption
	{
		// Don't change the names of these fields without also
		// changing socket-io.c in the runtime
		private bool	enabled;
		protected int	seconds;

		public LingerOption (bool enable, int secs)
		{
			enabled = enable;
			seconds = secs;
		}

		public bool Enabled
		{
			get { return enabled; }
			set { enabled = value; }
		}

		public int LingerTime
		{
			get { return seconds; }
			set { seconds = value; }
		}
	}
}
