//
// System.Web.HttpForbiddenHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

namespace System.Web
{
	class HttpForbiddenHandler : IHttpHandler
	{
		public void ProcessRequest (HttpContext context)
		{
			throw new HttpException (403, "Forbidden");
		}

		public bool IsReusable
		{
			get {
				return true;
			}
		}
	}
}

