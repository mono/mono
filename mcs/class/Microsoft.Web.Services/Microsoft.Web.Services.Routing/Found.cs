//
// Microsoft.Web.Services.Routing.Found.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using Microsoft.Web.Services;

namespace Microsoft.Web.Services.Routing
{

	public class Found
	{
		private UriList _uris = new UriList ();

		public UriList At {
			get { return _uris; }
		}
	}
}
