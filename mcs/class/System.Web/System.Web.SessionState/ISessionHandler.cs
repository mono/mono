//
// System.Web.SessionState.ISessionHandler
//
// Authors:
//	Stefan Görling, (stefan@gorling.se)
//
// (C) 2003 Stefan Görling
//
// This interface is simple, but as it's internal it shouldn't be hard to change it if we need to.
//
namespace System.Web.SessionState
{
	internal interface ISessionHandler
	{
	      void Dispose ();
	      void Init (HttpApplication context);
	      bool UpdateContext (HttpContext context);
	}
}

