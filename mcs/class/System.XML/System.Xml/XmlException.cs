//
// XmlException.cs
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
// (C) 2004 Novell Inc.
//

//
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

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Xml
{
	[Serializable]
	public class XmlException : SystemException
	{
		#region Fields

		int lineNumber;
		int linePosition;
		string sourceUri;

		#endregion

		#region Constructors

#if NET_1_0
#else
		public XmlException () 
			: base ()
		{
		}
#endif
		public XmlException (string message, Exception innerException) 
			: base (message, innerException)
		{
		}

		protected XmlException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			this.lineNumber = info.GetInt32 ("lineNumber");
			this.linePosition = info.GetInt32 ("linePosition");
			this.sourceUri = info.GetString ("sourceUri");
		}

#if NET_1_0
		internal XmlException (string message)
#else
		public XmlException (string message)
#endif
			: base (message)
		{
		}

		internal XmlException (IXmlLineInfo li, string sourceUri, string message) : base (message)
		{
			if (li != null) {
				this.lineNumber = li.LineNumber;
				this.linePosition = li.LinePosition;
			}
			this.sourceUri = sourceUri;
		}
		
#if NET_1_0
		internal XmlException (string message, Exception innerException, int lineNumber, int linePosition)
#else
		public XmlException (string message, Exception innerException, int lineNumber, int linePosition)
#endif
			: base (message, innerException)
		{
			this.lineNumber = lineNumber;
			this.linePosition = linePosition;
		}

		#endregion

		#region Properties

		public int LineNumber {
			get { return lineNumber; }
		}

		public int LinePosition {
			get { return linePosition; }
		}

#if NET_2_0
		public string SourceUri {
			get { return sourceUri; }
		}
#endif

		public override string Message {
			get {
				if (lineNumber == 0)
					return base.Message;

				return String.Format (CultureInfo.InvariantCulture, "{0} {3} Line {1}, position {2}.",
						      base.Message, lineNumber, linePosition, sourceUri);
			}
		}

		#endregion

		#region Methods

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("lineNumber", lineNumber);
			info.AddValue ("linePosition", linePosition);
			info.AddValue ("sourceUri", sourceUri);
		}

		#endregion
	}
}
