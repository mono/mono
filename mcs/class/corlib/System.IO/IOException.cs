//
// System.IO.IOException.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System.IO {
	[Serializable]
	public class IOException : SystemException {

		// Constructors
		public IOException ()
			: base ("I/O Error")
		{
		}

		public IOException (string message)
			: base (message)
		{
		}

		public IOException (string message, Exception inner)
			: base (message, inner)
		{
		}

		protected IOException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public IOException (string message, int hresult)
			: base (message)
		{
			this.HResult = hresult;
		}
	}
}
