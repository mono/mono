using System;
using System.Web;
using System.Web.UI;

namespace MonoTests.SystemWeb.Framework
{
	class MyPageHandlerFactory : IHttpHandlerFactory
	{
		public virtual IHttpHandler GetHandler (HttpContext context, string requestType, string url, string path)
		{
			IHttpHandler h = PageParser.GetCompiledPageInstance (url, path, context);
			Page p = (Page)h; //some configuration error if cannot cast, let it crash
			WebTest.CurrentTest.Invoke (p);
			return h;
		}

		public virtual void ReleaseHandler (IHttpHandler handler)
		{
		}
	}

}
