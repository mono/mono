#if NET_2_0
using System;
using System.Web;

namespace NunitWeb
{
	class MyHandler : IHttpHandler
	{
		public bool IsReusable
		{
			get { return false; }
		}

		public void ProcessRequest (HttpContext context)
		{
			MyHost.RunDelegate (context, null);
		}
	}
}
#endif
