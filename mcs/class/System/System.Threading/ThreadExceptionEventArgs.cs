//
// System.Threading.ThreadExceptionEventArgs.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Threading
{
	public class ThreadExceptionEventArgs : EventArgs
	{
		public ThreadExceptionEventArgs(Exception t) {
			// blah
		}

		public Exception Exception {
			get {
				return new Exception();
			}
		}
		
	}
}
