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
		private Exception exception;
		
		public ThreadExceptionEventArgs(Exception t) {
			exception=t;
		}

		public Exception Exception {
			get {
				return(exception);
			}
		}
	}
}
