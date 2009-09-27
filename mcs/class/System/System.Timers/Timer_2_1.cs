//
// System.Timers.Timer - Moonlight specific version for sockets
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// Copyright (C) 2005,2009 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.Threading;

namespace System.Timers {

	internal class Timer {
		double interval;
		bool autoReset;
		System.Threading.Timer timer;
		object _lock = new object ();

		public event ElapsedEventHandler Elapsed;

		public Timer ()
		{
			autoReset = true;
			Interval = 100;
		}

		public bool AutoReset
		{
			get { return autoReset; }
			set { autoReset = value; }
		}

		public bool Enabled
		{
			get {
				lock (_lock)
					return timer != null;
			}
			set {
				lock (_lock) {
					bool enabled = timer != null;
					if (enabled == value)
						return;

					if (value) {
						timer = new System.Threading.Timer (Callback, this, (int)interval, autoReset ? (int)interval: 0);
					} else {
						timer.Dispose ();
						timer = null;
					}
				}
			}
		}

		public double Interval
		{
			get { return interval; }
			set { 
				// The doc says 'less than 0', but 0 also throws the exception
				if (value <= 0)
					throw new ArgumentException ("Invalid value: " + value);

				lock (_lock) {
					interval = value;
					if (timer != null)
						timer.Change ((int)interval, autoReset? (int)interval: 0);
				}
			}
		}

		static void Callback (object state)
		{
			Timer timer = (Timer) state;
			if (timer.Enabled == false)
				return;
			ElapsedEventHandler events = timer.Elapsed;
			if (!timer.autoReset)
				timer.Enabled = false;
			if (events == null)
				return;

			ElapsedEventArgs arg = new ElapsedEventArgs (DateTime.Now);

			try {
				events (timer, arg);
			} catch {
			}
		}
	}
}
