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

		public SoapException (string message, XmlQualifiedName code)
			: base (message, code)
		{
			this.code = code;
		}

		public SoapException (string message, XmlQualifiedName code, Exception innerException)
			: this (message, code)
		{
		}

		public SoapException (string message, XmlQualifiedName code, string actor)
			: this (message, code)
		{
			this.actor = actor;
		}

		public SoapException (string message, XmlQualifiedName code, string actor, Exception innerException)
			: this (message, code, actor)
		{
		}

		#endregion // Constructors
	}
}
