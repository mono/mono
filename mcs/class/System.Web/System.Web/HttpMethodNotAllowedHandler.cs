//
// System.Web.HttpMethodNotAllowedHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

namespace System.Web
{
	class HttpMethodNotAllowedHandler : IHttpHandler
	{
		public void ProcessRequest (HttpContext context)
		{
			throw new HttpException (405, "Forbidden");
		}


		public bool IsReusable
		{
			get {
				return true;
			}
		}
	}
}

