//
// System.Threading.ThreadAbortException.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.CompilerServices;

namespace System.Threading
{
	[Serializable]
	public sealed class ThreadAbortException : SystemException
	{
		private ThreadAbortException () : base ("Thread was being aborted")
		{
			HResult = unchecked ((int) 0x80131530);
		}

		public object ExceptionState {
			get {
				return Thread.CurrentThread.abort_state;
			}
		}
	}
}
