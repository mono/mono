//
// Microsoft.Web.Services.Addressing.ServiceName.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{
	public class ServiceName : AttributedQName, IXmlElement
	{
		private string _port;

		public ServiceName (XmlElement element) : base ()
		{
			LoadXml (element);
		}

		public ServiceName (QualifiedName qname) : base (qname)
		{
		}

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

		public string PortName {
			get { return _port; }
			set { _port = value; }
		}
	}
}
