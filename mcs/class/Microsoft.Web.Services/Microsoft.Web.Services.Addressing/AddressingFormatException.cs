//
// Microsoft.Web.Services.Addressing.AddressingFormatException.cs
//
// Author: Daniel Kornhauser dkor@alum.mit.edu
//
// (C) Copyright, Ximian, Inc.
//

using System;
using System.Web.Services.Protocols;
using System.Xml;

namespace Microsoft.Web.Services.Addressing {
	
	[Serializable]	
	public class AddressingFormatException : SoapHeaderException
	{
		public static readonly string MissingActionElement 
			= "The <{0}> element should not have any child nodes other than text.";
		
		
		public AddressingFormatException (string message)
                        : base (message, XmlQualifiedName.Empty)
		{
		}
	}
}

