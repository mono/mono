// System.Xml.Xsl.XsltCompileException
//
// Author: Tim Coleman <tim@timcoleman.com>
// (C) Copyright 2002 Tim Coleman

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

		[MonoTODO]
		protected XsltCompileException (SerializationInfo info, StreamingContext context )
			: base (info, context)
		{
		}

		[MonoTODO]
		// I don't think this base() call is right, but what
		// should the message be for XsltException?
		public XsltCompileException (Exception inner, String sourceUri, int lineNumber, int linePosition)
			: base (sourceUri, inner)
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
