//
// XmlException.cs
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Xml
{
	[Serializable]
	public class XmlException : SystemException
	{
		#region Fields

		int lineNumber;
		int linePosition;
		string sourceUri;
		string res;
		string [] messages;

		#endregion

		#region consts

		private const string Xml_DefaultException = "Xml_DefaultException";
		private  const string Xml_UserException = "Xml_UserException";

		#endregion

		#region Constructors

		public XmlException () 
			: base ()
		{
			this.res = Xml_DefaultException;
			this.messages = new string [1];
		}

		public XmlException (string message, Exception innerException) 
			: base (message, innerException)
		{
			this.res = Xml_UserException;
			this.messages = new string [] {message};
		}

		protected XmlException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			this.lineNumber = info.GetInt32 ("lineNumber");
			this.linePosition = info.GetInt32 ("linePosition");
			this.res = info.GetString ("res");
			this.messages = (string []) info.GetValue ("args", typeof(string []));
#if NET_2_0
			this.sourceUri = info.GetString ("sourceUri");
#endif
		}

		public XmlException (string message)
			: base (message)
		{
			this.res = Xml_UserException;
			this.messages = new string [] {message};
		}

		internal XmlException (IXmlLineInfo li,
			string sourceUri,
			string message)
			: this (li, null, sourceUri, message)
		{
		}

		internal XmlException (IXmlLineInfo li,
			Exception innerException,
			string sourceUri,
			string message)
			: this (message, innerException)
		{
			if (li != null) {
				this.lineNumber = li.LineNumber;
				this.linePosition = li.LinePosition;
			}
			this.sourceUri = sourceUri;
		}

		public XmlException (string message, Exception innerException, int lineNumber, int linePosition)
			: this (message, innerException)
		{
			this.lineNumber = lineNumber;
			this.linePosition = linePosition;
		}

#if NET_2_1
		internal XmlException (string message, int lineNumber, int linePosition,
			object sourceObject, string sourceUri, Exception innerException)
			: base (
				GetMessage (message, sourceUri, lineNumber, linePosition, sourceObject),
				innerException)
		{
			//hasLineInfo = true;
			this.lineNumber		= lineNumber;
			this.linePosition	= linePosition;
			//this.sourceObj		= sourceObject;
			this.sourceUri		= sourceUri;
		}

		private static string GetMessage (string message, string sourceUri, int lineNumber, int linePosition, object sourceObj)
		{
			string msg = "XmlSchema error: " + message;
			if (lineNumber > 0)
				msg += String.Format (CultureInfo.InvariantCulture, " XML {0} Line {1}, Position {2}.",
					(sourceUri != null && sourceUri != "") ? "URI: " + sourceUri + " ." : "",
					lineNumber,
					linePosition);
			return msg;
		}
#endif

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

		[SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("lineNumber", lineNumber);
			info.AddValue ("linePosition", linePosition);
			info.AddValue ("res", res);
			info.AddValue ("args", messages);
#if NET_2_0
			info.AddValue ("sourceUri", sourceUri);
#endif
		}

		#endregion
	}
}
