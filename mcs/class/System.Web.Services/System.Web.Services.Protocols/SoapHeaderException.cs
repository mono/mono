// 
// System.Web.Services.Protocols.SoapHeaderException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Xml;

namespace System.Web.Services.Protocols {
	public class SoapHeaderException : SoapException {

		#region Constructors

		public SoapHeaderException (string message, XmlQualifiedName code)
			: base (message, code)
		{
		}

		public SoapHeaderException (string message, XmlQualifiedName code, Exception innerException)
			: base (message, code, innerException)
		{
		}

		public SoapHeaderException (string message, XmlQualifiedName code, string actor)
			: base (message, code, actor)
		{
		}

		public SoapHeaderException (string message, XmlQualifiedName code, string actor, Exception innerException)
			: base (message, code, actor, innerException)
		{
		}

		#endregion // Constructors
	}
}
