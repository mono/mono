using System;
using System.Web;

namespace TestMonoWeb
{
	public class SyncHandler : IHttpHandler {
		
		public void ProcessRequest(HttpContext context) {
			context.Response.Write("SyncHandler.ProcessRequest<br>\n");
		}
		
		public bool IsReusable {
			get { return false; }  
		}
	}
}
