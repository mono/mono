//
// System.Web.UI.SimpleHandlerFactory
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System.Web;

namespace System.Web.UI
{
	class SimpleHandlerFactory : IHttpHandlerFactory
	{
		[MonoTODO]
		public virtual IHttpHandler GetHandler (HttpContext context,
							string requestType,
							string virtualPath,
							string path)
		{
			// This should handle *.ashx files
			throw new NotImplementedException ();
		}

		public virtual void ReleaseHandler (System.Web.IHttpHandler handler)
		{
		}
	}
}

