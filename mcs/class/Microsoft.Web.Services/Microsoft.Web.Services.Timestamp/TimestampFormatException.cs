// Microsoft.Web.Services.Timestamp.TimestampFormatException.cs
//
// Author: Daniel Kornhauser <dkor@alum.mit.edu>
//
// (C) Ximian, Inc. 2003.

using System;
using System.Web.Services.Protocols;
using System.Xml;


namespace Microsoft.Web.Services.Timestamp {

	[Serializable]
	public class TimestampFormatException : SoapHeaderException
	{
		public TimestampFormatException (string message)
                        : base (message, XmlQualifiedName.Empty)
		{
		}
		
		public TimestampFormatException (string message, Exception ex)
                        : base (message, XmlQualifiedName.Empty, ex)
		{
		}
	}
}
