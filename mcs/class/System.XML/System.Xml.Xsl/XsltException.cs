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

using System;
using System.Runtime.Serialization;
using System.Xml.XPath;

namespace System.Xml.Xsl
{
	[Serializable]
	public class XsltException : SystemException
	{
		#region Fields

		int lineNumber;
		int linePosition;
		string sourceUri;

		#endregion

		#region Constructors

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
			: base (message, innerException)
		{
			this.lineNumber = lineNumber;
			this.linePosition = linePosition;
			this.sourceUri = sourceUri;
		}

		internal XsltException (string message, Exception innerException, XPathNavigator nav)
			: base (message, innerException)
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
