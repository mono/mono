//
// Microsoft.Web.Services.Addressing.EndpointReference
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{

	public class EndpointReference : EndpointReferenceType, IXmlElement
	{

		public XmlElement GetXml (XmlDocument document)
		{
			throw new NotImplementedException ();
		}

		public void LoadXml (XmlElement element)
		{
			throw new NotImplementedException ();
		}

	}

}
