//
// XmlSchemaException.cs
//
// Author:
// 	Dwivedi, Ajay kumar Adwiv@Yahoo.com
//	Enomoto, Atsushi atsushi@ximian.com
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaException.
	/// </summary>
	[Serializable]
	public class XmlSchemaException : System.SystemException
	{
		//fields
		private bool hasLineInfo;
		private int lineNumber;
		private int linePosition;
		private XmlSchemaObject sourceObj;
		private string sourceUri;

#if NET_2_0
		public XmlSchemaException ()
			: this ("A schema error occured.", null)
		{
		}
#endif

#if NET_2_0
		public
#else
		internal
#endif
		XmlSchemaException (string message)
			: this (message, null)
		{
		}

		protected XmlSchemaException(SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			this.hasLineInfo = info.GetBoolean ("hasLineInfo");
			this.lineNumber = info.GetInt32 ("lineNumber");
			this.linePosition = info.GetInt32 ("linePosition");
			this.sourceUri = info.GetString ("sourceUri");
			this.sourceObj = info.GetValue ("sourceObj", typeof (XmlSchemaObject)) as XmlSchemaObject;
		}

#if NET_2_0
		public XmlSchemaException (string message, Exception innerException, int lineNumber, int linePosition)
			: this (message, lineNumber, linePosition, null, null, innerException)
		{
		}
#endif

		internal XmlSchemaException (string message, int lineNumber, int linePosition,
			XmlSchemaObject sourceObject, string sourceUri, Exception innerException)
			: base (
				GetMessage (message, sourceUri, lineNumber, linePosition, sourceObject),
				innerException)
		{
			hasLineInfo = true;
			this.lineNumber		= lineNumber;
			this.linePosition	= linePosition;
			this.sourceObj		= sourceObject;
			this.sourceUri		= sourceUri;
		}

		internal XmlSchemaException (string message, object sender,
			string sourceUri, XmlSchemaObject sourceObject, Exception innerException)
			: base (GetMessage (message, sourceUri, sender, sourceObject), innerException)
		{
			IXmlLineInfo li = sender as IXmlLineInfo;
			if (li != null && li.HasLineInfo ()) {
				hasLineInfo = true;
				this.lineNumber = li.LineNumber;
				this.linePosition = li.LinePosition;
			}
			this.sourceObj = sourceObject;
		}

		internal XmlSchemaException(string message, XmlSchemaObject sourceObject,
			Exception innerException)
			: base (
				GetMessage (message, null, 0, 0, sourceObject),
				innerException)
		{
			hasLineInfo = true;
#if !MOBILE
			this.lineNumber = sourceObject.LineNumber;
			this.linePosition = sourceObject.LinePosition;
			this.sourceObj	=	sourceObject;
			this.sourceUri	=	sourceObject.SourceUri;
#endif
		}

		public XmlSchemaException(string message, Exception innerException)
			: base (
				GetMessage (message, null, 0, 0, null),
				innerException )
		{
		}

		// Properties
		public int LineNumber
		{ 
			get{ return this.lineNumber;} 
		}
		public int LinePosition 
		{ 
			get{ return this.linePosition;} 
		}
		public XmlSchemaObject SourceSchemaObject 
		{
			get{ return this.sourceObj; } 
		}
		public string SourceUri 
		{ 
			get{ return this.sourceUri; } 
		}

		private static string GetMessage (string message, string sourceUri, object sender, XmlSchemaObject sourceObj)
		{
			IXmlLineInfo li = sender as IXmlLineInfo;
			if (li == null)
				return GetMessage (message, sourceUri, 0, 0, sourceObj);
			else
				return GetMessage (message, sourceUri, li.LineNumber, li.LinePosition, sourceObj);
		}

		private static string GetMessage (string message, string sourceUri, int lineNumber, int linePosition, XmlSchemaObject sourceObj)
		{
			string msg = "XmlSchema error: " + message;
			if (lineNumber > 0)
				msg += String.Format (CultureInfo.InvariantCulture, " XML {0} Line {1}, Position {2}.",
					(sourceUri != null && sourceUri != "") ? "URI: " + sourceUri + " ." : "",
					lineNumber,
					linePosition);
#if !MOBILE
			if (sourceObj != null)
				msg += String.Format (CultureInfo.InvariantCulture, " Related schema item SourceUri: {0}, Line {1}, Position {2}.",
					sourceObj.SourceUri, sourceObj.LineNumber, sourceObj.LinePosition);
#endif
			return msg;
		}

		public override string Message {
			get { return base.Message; }
		}

		// Methods

		[SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("hasLineInfo", hasLineInfo);
			info.AddValue ("lineNumber", lineNumber);
			info.AddValue ("linePosition", linePosition);
			info.AddValue ("sourceUri", sourceUri);
			info.AddValue ("sourceObj", sourceObj);
		}
	}
}
