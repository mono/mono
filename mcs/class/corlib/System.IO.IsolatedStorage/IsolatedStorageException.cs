//
// System.IO.IsolatedStorage.IsolatedstorageException
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) 2002, Ximian, Inc. http://www.ximian.com
//

using System.Globalization;
using System.Runtime.Serialization;

namespace System.IO.IsolatedStorage
{
	[Serializable]
        public class IsolatedStorageException : Exception
	{
		public IsolatedStorageException ()
			: base (Locale.GetText ("An Isolated storage operation failed."))
		{
		}

		public IsolatedStorageException (string message)
			: base (message)
		{
		}

		public IsolatedStorageException (string message, Exception inner)
			: base (message, inner)
		{
		}

		protected IsolatedStorageException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
