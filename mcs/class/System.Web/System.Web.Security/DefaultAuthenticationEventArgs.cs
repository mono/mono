//
// System.Web.Security.DefaultAuthenticationEventArgs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

namespace System.Web.Security {
	using System;
	using System.Web;
	
public sealed class DefaultAuthenticationEventArgs : EventArgs
{
	private HttpContext _context;

	public DefaultAuthenticationEventArgs (HttpContext context)
	{
		if (context == null)
			throw new ArgumentNullException ("context");

		_context = context;
	}

	public HttpContext Context
	{
		get { return _context; }
	}
}
}

