// Microsoft.Web.Services.SoapFormatException.cs
//
// Author: Daniel Kornhauser <dkor@alum.mit.edu>
//
// (C) Ximian, Inc. 2003.

using System;
using System.Web.Services.Protocols;
using System.Xml;

namespace Microsoft.Web.Services {

	[Serializable]
	public class SoapFormatException : SoapHeaderException
	{
		public SoapFormatException ()
                        : base (String.Empty, XmlQualifiedName.Empty)
		{
		}

		public SoapFormatException (string message)
                        : base (message, XmlQualifiedName.Empty)
		{
		}
	}
}
