//
// System.Threading.ThreadInterruptedException.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System.Threading
{
	public class ThreadInterruptedException : SystemException
	{
		public ThreadInterruptedException()
			: base ("Thread interrupted") {
		}

		public ThreadInterruptedException(string message)
			: base (message) {
		}

		protected ThreadInterruptedException(SerializationInfo info, StreamingContext context)
			: base (info, context) {
		}

		public ThreadInterruptedException(string message, Exception innerException)
			: base (message, innerException) {
		}
	}
}
