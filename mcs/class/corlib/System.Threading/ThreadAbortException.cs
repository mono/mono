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
	[Serializable]
	public sealed class ThreadAbortException : SystemException
	{
		private ThreadAbortException () {}

		[MonoTODO]
		public object ExceptionState {
			get {
				// FIXME
				return(null);
			}
		}
	}
}
