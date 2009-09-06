// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Miguel de Icaza (miguel@ximian.com)
//

using System;
using System.Runtime.Serialization;
using System.Security;

namespace System.IO {

	[Serializable]
	public class FileFormatException : FormatException, ISerializable
	{
		Uri source_uri;
		
		public FileFormatException () : base ()
		{
		}

		public FileFormatException (string message) : base (message)
		{
		}

		public FileFormatException (Uri sourceUri)
		{
			this.source_uri = sourceUri;
		}

		protected FileFormatException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			
			source_uri = (Uri) info.GetValue ("sourceUri", typeof (Uri));
		}

		public FileFormatException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		public FileFormatException (Uri sourceUri, Exception innerException)
			: base ("", innerException)
		{
			source_uri = sourceUri;
		}

		public FileFormatException (Uri sourceUri, string message)
			: base (message)
		{
			source_uri = sourceUri;
		}

		public FileFormatException (Uri sourceUri, string message, Exception innerException)
			: base (message, innerException)
		{
			source_uri = sourceUri;
		}

		public Uri SourceUri {
			get { return source_uri; }
		}

		[SecurityCritical]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("sourceUri", source_uri);
		}
	}
}

