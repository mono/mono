//
// System.Xml.Xsl.XsltException.cs
//
// Authors:
//  Tim Coleman (tim@timcoleman.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Copyright 2002 Tim Coleman
// (C) 2003 Andreas Nahr
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
using System.Runtime.Serialization;
using System.Xml.XPath;

namespace System.Xml.Xsl
{
	[Serializable]
	public class XsltException : SystemException
	{
		static string CreateMessage (string message, XPathNavigator nav)
		{
			IXmlLineInfo li = nav as IXmlLineInfo;
			int lineNumber = li != null ? li.LineNumber : 0;
			int linePosition = li != null ? li.LinePosition : 0;
			string sourceUri = nav != null ? nav.BaseURI : String.Empty;
			return CreateMessage (lineNumber, linePosition, sourceUri, message);
		}

		static string CreateMessage (int lineNumber, int linePosition, string sourceUri, string msg)
		{
			if (sourceUri != null)
				msg += " " + sourceUri;
			if (lineNumber != 0)
				msg += " line " + lineNumber;
			if (linePosition != 0)
				msg += ", position " + linePosition;
			return msg;
		}

		#region Fields

		int lineNumber;
		int linePosition;
		string sourceUri;

		#endregion

		#region Constructors

#if NET_2_0
		public XsltException ()
			: base (String.Empty, null)
		{
		}

		public XsltException (string message)
			: base (message, null)
		{
		}
#endif

		public XsltException (string message, Exception innerException)
			: base (message, innerException)
		{
//			this.message = message;
		}

		protected XsltException (SerializationInfo info, StreamingContext context)
		{
			lineNumber = info.GetInt32 ("lineNumber");
			linePosition = info.GetInt32 ("linePosition");
			sourceUri = info.GetString ("sourceUri");
		}

		internal XsltException (string message, Exception innerException, int lineNumber, int linePosition, string sourceUri)
			: base (CreateMessage (lineNumber, linePosition, sourceUri, message), innerException)
		{
			this.lineNumber = lineNumber;
			this.linePosition = linePosition;
			this.sourceUri = sourceUri;
		}

		internal XsltException (string message, Exception innerException, XPathNavigator nav)
			: base (CreateMessage (message, nav), innerException)
		{
			IXmlLineInfo li = nav as IXmlLineInfo;
			this.lineNumber = li != null ? li.LineNumber : 0;
			this.linePosition = li != null ? li.LinePosition : 0;
			this.sourceUri = nav != null ? nav.BaseURI : String.Empty;
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
#else
		public override string Message {
			get {
				string msg = base.Message;
				if (sourceUri != null)
					msg += " " + sourceUri;
				if (lineNumber != 0)
					msg += " line " + lineNumber;
				if (linePosition != 0)
					msg += ", position " + linePosition;
				return msg;
			}
		}
#endif

		public string SourceUri {
			get { return sourceUri; }
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
