//
// Microsoft.Web.Services.DimeFormatException.cs
//
// Author: Daniel Kornhauser dkor@alum.mit.edu
//
// (C) Copyright, Ximian, Inc.
//

using System;
using System.Web.Services.Protocols;
using System.Xml;

namespace Microsoft.Web.Services.Dime {

	[Serializable]
	public class DimeFormatException: SoapHeaderException
	{
		public DimeFormatException (string message)
                        : base (String.Empty, XmlQualifiedName.Empty)
		{
		}
		
		public DimeFormatException (string message, Exception ex)
                        : base (message, XmlQualifiedName.Empty, ex)
		{
		}
	}
}
		 
