//
// System.Xml.Xsl.XsltCompileException.cs
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
	public class XsltCompileException : XsltException
	{
		#region Constructors

		protected XsltCompileException (SerializationInfo info, StreamingContext context )
			: base (info, context)
		{
		}

		public XsltCompileException (Exception inner, String sourceUri, int lineNumber, int linePosition)
			: base (Locale.GetText ("XSLT compile error"), inner, lineNumber, linePosition, sourceUri)
		{
		}

		internal XsltCompileException (string message, Exception innerException, XPathNavigator nav)
			: base (message, innerException, nav)
		{
		}
		#endregion

		#region Properties

		public override string Message {
			get { return base.Message; }
		}

		#endregion

		#region Methods

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
		}

		#endregion
	}
}
