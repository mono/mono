//
// System.Threading.Monitor.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Threading
{
	public sealed class Monitor
	{
		public static void Enter(object obj) {
			if(obj==null) {
				throw new ArgumentNullException("obj");
			}
			if(obj.GetType().IsValueType==true) {
				throw new ArgumentException("Value type");
			}
			// FIXME
		}

		public static void Exit(object obj) {
			if(obj==null) {
				throw new ArgumentNullException("obj");
			}
			if(obj.GetType().IsValueType==true) {
				throw new ArgumentException("Value type");
			}
			// FIXME
		}

		public static void Pulse(object obj) {
			if(obj==null) {
				throw new ArgumentNullException("obj");
			}
			// FIXME
		}

		public static void PulseAll(object obj) {
			if(obj==null) {
				throw new ArgumentNullException("obj");
			}
			// FIXME
		}

		public static bool TryEnter(object obj) {
			if(obj==null) {
				throw new ArgumentNullException("obj");
			}
			if(obj.GetType().IsValueType==true) {
				throw new ArgumentException("Value type");
			}
			
			// FIXME
			return(false);
		}

		public static bool TryEnter(object obj, int millisecondsTimeout) {
			if(obj==null) {
				throw new ArgumentNullException("obj");
			}
			if(obj.GetType().IsValueType==true) {
				throw new ArgumentException("Value type");
			}
			if(millisecondsTimeout<0) {
				throw new ArgumentException("millisecondsTimeout negative");
			}
			// FIXME
			return(false);
		}

		public static bool TryEnter(object obj, TimeSpan timeout) {
			if(obj==null) {
				throw new ArgumentNullException("obj");
			}
			if(obj.GetType().IsValueType==true) {
				throw new ArgumentException("Value type");
			}
			if(timeout.Milliseconds < 0 || timeout.Milliseconds > Int32.MaxValue) {
				throw new ArgumentOutOfRangeException("timeout out of range");
			}
			
			// FIXME
			return(false);
		}

		public static bool Wait(object obj) {
			if(obj==null) {
				throw new ArgumentNullException("obj");
			}
			// FIXME
			return(false);
		}

		public static bool Wait(object obj, int millisecondsTimeout) {
			if(obj==null) {
				throw new ArgumentNullException("obj");
			}
			// FIXME
			return(false);
		}

		public static bool Wait(object obj, TimeSpan timeout) {
			if(obj==null) {
				throw new ArgumentNullException("obj");
			}
			// LAMESPEC: says to throw ArgumentException too
			if(timeout.Milliseconds < 0 || timeout.Milliseconds > Int32.MaxValue) {
				throw new ArgumentOutOfRangeException("timeout out of range");
			}
			
			// FIXME
			return(false);
		}

		public static bool Wait(object obj, int millisecondsTimeout, bool exitContext) {
			if(obj==null) {
				throw new ArgumentNullException("obj");
			}
			// FIXME
			return(false);
		}

		public static bool Wait(object obj, TimeSpan timeout, bool exitContext) {
			if(obj==null) {
				throw new ArgumentNullException("obj");
			}
			// LAMESPEC: says to throw ArgumentException too
			if(timeout.Milliseconds < 0 || timeout.Milliseconds > Int32.MaxValue) {
				throw new ArgumentOutOfRangeException("timeout out of range");
			}
			
			// FIXME
			return(false);
		}
	}
}
