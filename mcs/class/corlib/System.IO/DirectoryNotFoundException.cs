//
// System.IO.DirectoryNotFoundException.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;

namespace System.IO {

	[Serializable]
	public class DirectoryNotFoundException : IOException {
		
		// Constructors
		public DirectoryNotFoundException ()
			: base ("Directory not found")
		{
		}

		public DirectoryNotFoundException (string message)
			: base (message)
		{
		}

		public DirectoryNotFoundException (string message, Exception inner)
			: base (message, inner)
		{
		}

		protected DirectoryNotFoundException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
	}
}
