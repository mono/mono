//
// Microsoft.Web.Services.Addressing.AttributedUri
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Net;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{

	public abstract class AttributedUri : OpenAttributeElement
	{

		private Uri _value;

		public Uri Value {
			get { return _value; }
			set { _value = value; }
		} 

	}

}
