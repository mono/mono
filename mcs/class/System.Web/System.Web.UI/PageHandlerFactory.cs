//
// System.Web.UI.PageHandlerFactory
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System.Web;
using System.Web.Compilation;

namespace System.Web.UI
{
	class PageHandlerFactory : IHttpHandlerFactory
	{
		public virtual IHttpHandler GetHandler (HttpContext context, string requestType, string url, string path)
		{
			return PageParser.GetCompiledPageInstance (url, path, context);
		}

		public virtual void ReleaseHandler (IHttpHandler handler)
		{
		}
	}
}

