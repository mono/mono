//
// System.Threading.ThreadAbortException.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Threading
{
	public sealed class ThreadAbortException : SystemException
	{
		[MonoTODO]
		public object ExceptionState {
			get {
				// FIXME
				return(null);
			}
		}
	}
}
