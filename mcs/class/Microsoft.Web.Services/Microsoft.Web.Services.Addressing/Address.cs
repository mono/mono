//
// Microsoft.Web.Services.Addressing.Address.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{

	public class Address : AttributedUri, IXmlElement
	{
		
		[MonoTODO]
		public XmlElement GetXml (XmlDocument document)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void LoadXml (XmlElement element)
		{
			throw new NotImplementedException ();
		}
	}
}
