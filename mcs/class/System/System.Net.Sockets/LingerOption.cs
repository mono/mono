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
		protected bool	enabled;
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

		public override bool Equals (object o)
		{
			return (enabled == ((LingerOption)o).enabled) &&
				(seconds == ((LingerOption)o).seconds);
		}

		public override int GetHashCode()
		{
			return seconds;
		}

		public override string ToString()
		{
			string ret;
			if (enabled) {
				ret = "off";
			}
			else {
				ret = seconds.ToString();
			}
			return ret;
		}
	}
}
