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

namespace System.Xml.Xsl
{
	[Serializable]
	public class XsltCompileException : XsltException
	{
		#region Fields

		string message;

		#endregion

		#region Constructors

		protected XsltCompileException (SerializationInfo info, StreamingContext context )
			: base (info, context)
		{
		}

		public XsltCompileException (Exception inner, String sourceUri, int lineNumber, int linePosition)
			: base (Locale.GetText ("XSLT compile error"), inner, lineNumber, linePosition, sourceUri)
		{
		}

		#endregion

		#region Properties

		public override string Message {
			get { return message; }
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
