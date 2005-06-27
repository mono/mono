// Microsoft.Web.Services.Routing.RoutingFormatException.cs
//
// Author: Daniel Kornhauser <dkor@alum.mit.edu>
//
// (C) Ximian, Inc. 2003

using System;
using System.Web.Services.Protocols;
using System.Xml;


namespace Microsoft.Web.Services.Routing {

	[Serializable]
	public class RoutingFormatException : SoapHeaderException
	{
		public RoutingFormatException (string message) : base (message, XmlQualifiedName.Empty)
		{
		}

		public RoutingFormatException (string message, Exception ex) : base (message, XmlQualifiedName.Empty, ex)
                {
                }
	}
}
