//
// System.Timers.ElapsedEventArgs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;

namespace System.Timers
{
	public class ElapsedEventArgs : EventArgs
	{
		DateTime time;

		internal ElapsedEventArgs (DateTime time)
		{
			this.time = time;
		}

		public DateTime SignalTime
		{
			get { return time; }
		}
	}
}

