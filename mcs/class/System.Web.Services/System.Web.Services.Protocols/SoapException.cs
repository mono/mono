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

		string actor;
		XmlQualifiedName code;
		XmlNode detail;

		#endregion

		#region Constructors

		public SoapException (string message, XmlQualifiedName code)
			: base (message)
		{
			this.code = code;
		}

		public SoapException (string message, XmlQualifiedName code, Exception innerException)
			: base (message, innerException)
		{
			this.code = code;
		}

		public SoapException (string message, XmlQualifiedName code, string actor)
			: base (message)
		{
			this.code = code;
			this.actor = actor;
		}

		public SoapException (string message, XmlQualifiedName code, string actor, Exception innerException)
			: base (message, innerException)
		{
			this.code = code;
			this.actor = actor;
		}

		public SoapException (string message, XmlQualifiedName code, string actor, XmlNode detail)
			: base (message)
		{
			this.code = code;
			this.actor = actor;
			this.detail = detail;
		}

		public SoapException (string message, XmlQualifiedName code, string actor, XmlNode detail, Exception innerException)
			: base (message, innerException)
		{
			this.code = code;
			this.actor = actor;
			this.detail = detail;
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
