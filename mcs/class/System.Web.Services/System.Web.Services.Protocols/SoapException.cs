// 
// System.Web.Services.Protocols.SoapException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

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
