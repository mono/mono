//
// Microsoft.Web.Services.Addressing.Action.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman
//

using System;
using System.Xml;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{

	public class Action : AttributedUriString, IXmlElement
	{


		[MonoTODO]
		public static implicit operator Action(string obj)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static implicit operator string(Action obj)
		{
			throw new NotImplementedException ();
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

	}
}
