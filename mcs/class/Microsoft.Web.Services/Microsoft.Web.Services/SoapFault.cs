//
// SoapFault.cs: Implements SoapFault
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Microsoft.Web.Services {

	[Serializable]
	public class SoapFault {

		public const string InvalidSoapMessage = "Invalid Soap Message";
		public const string InvalidXmlMessage = "Invalid Xml Message";
		public const string ServerUnavailable = "Server Unavailable";

		public SoapFault () {}
	}
}
