//
// System.Threading.SynchronizationLockException.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System.Threading
{
	public class SynchronizationLockException : SystemException
	{
		public SynchronizationLockException()
			: base ("Synchronization Error") {
		}

		public SynchronizationLockException(string message)
			: base (message) {
		}

		protected SynchronizationLockException(SerializationInfo info, StreamingContext context)
			: base (info, context) {
		}

		public SynchronizationLockException(string message, Exception innerException)
			: base (message, innerException) {
		}
	}
}
