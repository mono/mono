//
// System.Threading.Mutex.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Threading
{
	public sealed class Mutex : WaitHandle 
	{
		public Mutex() {
			// FIXME
		}

		public Mutex(bool initiallyOwned) {
			// FIXME
		}

		public Mutex(bool initiallyOwned, string name) {
			// FIXME
		}

		public Mutex(bool initiallyOwned, string name, out bool gotOwnership) {
			// FIXME
			gotOwnership=false;
		}
	}
}
