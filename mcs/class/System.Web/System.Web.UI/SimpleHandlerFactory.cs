//
// System.Web.UI.SimpleHandlerFactory
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
//

using System.Web;

namespace System.Web.UI
{
	class SimpleHandlerFactory : IHttpHandlerFactory
	{
		public virtual IHttpHandler GetHandler (HttpContext context,
							string requestType,
							string virtualPath,
							string path)
		{
			Type type = WebHandlerParser.GetCompiledType (context, virtualPath, path);
			if (!(typeof (IHttpHandler).IsAssignableFrom (type)))
				throw new HttpException ("Type does not implement IHttpHandler: " + type.FullName);

			return Activator.CreateInstance (type) as IHttpHandler;
		}

		public virtual void ReleaseHandler (IHttpHandler handler)
		{
		}
	}
}

