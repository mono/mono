//
// System.Xml.Xsl.XsltException.cs
//
// Authors:
//  Tim Coleman (tim@timcoleman.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Copyright 2002 Tim Coleman
// (C) 2003 Andreas Nahr
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
using System.Xml.XPath;
using System.Security.Permissions;

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
			string sourceUri = nav != null ? nav.BaseURI : string.Empty;

			if (lineNumber != 0) {
				return CreateMessage ("{0} at {1}({2},{3}).", message, lineNumber, linePosition, sourceUri);
			}
			return CreateMessage ("{0}.", message, lineNumber, linePosition, sourceUri);
		}

		static string CreateMessage (string msgFormat, string message, int lineNumber, int linePosition, string sourceUri)
		{
			return string.Format (CultureInfo.InvariantCulture, msgFormat,
				message, sourceUri, lineNumber.ToString (CultureInfo.InvariantCulture),
				linePosition.ToString (CultureInfo.InvariantCulture));
		}

		#region Fields

		int lineNumber;
		int linePosition;
		string sourceUri;
		string templateFrames;

		#endregion

		#region Constructors

		public XsltException ()
			: this (string.Empty, (Exception) null)
		{
		}

		public XsltException (string message)
			: this (message, (Exception) null)
		{
		}

		public XsltException (string message, Exception innerException)
			: this ("{0}", message, innerException, 0, 0, (string) null)
		{
		}

		protected XsltException (SerializationInfo info, StreamingContext context)
		{
			lineNumber = info.GetInt32 ("lineNumber");
			linePosition = info.GetInt32 ("linePosition");
			sourceUri = info.GetString ("sourceUri");
			templateFrames = info.GetString ("templateFrames");
		}

		internal XsltException (string msgFormat, string message, Exception innerException, int lineNumber, int linePosition, string sourceUri)
			: base (CreateMessage (msgFormat, message, lineNumber, linePosition, sourceUri), innerException)
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
			this.sourceUri = nav != null ? nav.BaseURI : string.Empty;
		}

		#endregion

		#region Properties

		public virtual int LineNumber {
			get { return lineNumber; }
		}

		public virtual int LinePosition {
			get { return linePosition; }
		}

		public override string Message {
			get {
				return templateFrames != null ? base.Message + templateFrames : base.Message;
			}
		}

		public virtual string SourceUri {
			get { return sourceUri; }
		}

		#endregion

		#region Methods

		[SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("lineNumber", lineNumber);
			info.AddValue ("linePosition", linePosition);
			info.AddValue ("sourceUri", sourceUri);
			info.AddValue ("templateFrames", templateFrames);
		}

		internal void AddTemplateFrame (string frame)
		{
			templateFrames += frame;
		}

		#endregion
	}
}
