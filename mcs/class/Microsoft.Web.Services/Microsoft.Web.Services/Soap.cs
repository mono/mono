//
// Soap.cs: SOAP definitions for WSE
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Microsoft.Web.Services {

	public class Soap {

		public class AttributeNames {

			public const string Actor = "actor";
			public const string MustUnderstand = "mustUnderstand";

			public AttributeNames() {}
		}

		public class ElementNames {
			public const string Body = "Body";
			public const string Envelope = "Envelope";
			public const string Fault = "Fault";
			public const string FaultActor = "faultactor";
			public const string FaultCode = "faultcode";
			public const string FaultDetail = "detail";
			public const string FaultString = "faultstring";
			public const string Header = "Header";
			public const string Message = "Message";
			public const string StackTrace = "StackTrace";

			public ElementNames () {}
		}

		public const string ActorNext = "http://schemas.xmlsoap.org/soap/actor/next";
		public static readonly Uri ActorNextURI = new Uri (ActorNext);

		public const string DimeContentType = "application/dime";
		public const string FaultNamespaceURI = "http://schemas.microsoft.com/wsdk/2002/10/";
		public const string NamespaceURI = "http://schemas.xmlsoap.org/soap/envelope/";
		public const string Prefix = "soap";
		public const string SoapContentType = "text/xml";

		public Soap() {}
	}
}
