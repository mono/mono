// 
// System.Web.Services.Protocols.SoapException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Xml;

namespace System.Web.Services.Protocols {
	public class SoapException : SystemException {

		#region Fields

		public static readonly XmlQualifiedName ClientFaultCode = new XmlQualifiedName ("Client", "http://schemas.xmlsoap.org/soap/envelope/");
		public static readonly XmlQualifiedName DetailElementName = new XmlQualifiedName ("detail");
		public static readonly XmlQualifiedName MustUnderstandFaultCode = new XmlQualifiedName ("MustUnderstand", "http://schemas.xmlsoap.org/soap/envelope/");
		public static readonly XmlQualifiedName ServerFaultCode = new XmlQualifiedName ("Server", "http://schemas.xmlsoap.org/soap/envelope/");
		public static readonly XmlQualifiedName VersionMismatchFaultCode = new XmlQualifiedName ("VersionMismatch", "http://schemas.xmlsoap.org/soap/envelope/");

		protected string actor;
		protected XmlQualifiedName code;
		protected XmlNode detail;

		#endregion

		#region Constructors

		public SoapException (string message, XmlQualifiedName code)
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

		public SoapException (string message, XmlQualifiedName code, string actor, XmlNode detail)
			: this (message, code, actor)
		{
			this.detail = detail;
		}

		public SoapException (string message, XmlQualifiedName code, string actor, XmlNode detail, Exception innerException)
			: this (message, code, actor, detail)
		{
		}

		#endregion // Constructors

		#region Properties

		public string Actor {
			get { return actor; }
		}

		public XmlQualifiedName Code {
			get { return code; }
		}

		public XmlNode Detail {
			get { return detail; }
		}

		#endregion // Properties
	}
}
