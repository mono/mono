//
// System.Web.Handlers.TraceHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System.Web;
using System.Web.UI;

namespace System.Web.Handlers
{
	public class TraceHandler : IHttpHandler
	{
		[MonoTODO]
		void IHttpHandler.ProcessRequest (HttpContext context)
		{
			//TODO: This should generate the trace page.
			throw new NotImplementedException ();
		}

		bool IHttpHandler.IsReusable
		{
			get {
				return false;
			}
		}
	}
}

