//
// Microsoft.Web.Services.Addressing.AttributedUriString
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{

	public abstract class AttributedUriString : OpenAttributeElement
	{
		
		private string _uri;

		[MonoTODO]
		public AttributedUriString ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public AttributedUriString (string uri)
		{
			throw new NotImplementedException ();
		}

		public string Value {
			get { return _uri; }
			set { _uri = value; }
		}
		
	}

}
