//
// Microsoft.Web.Services.Addressing
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Addressing
{

	public abstract class EndpointReferenceType : OpenElement
	{

		private Address address;

		public Address Address {
			get { return address; }
			set { address = value; }
		}

	}

}
