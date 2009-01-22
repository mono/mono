using System;
using System.Web;

namespace MonoTests.SystemWeb.Framework
{
	internal class MyHandler : IHttpHandler
	{
		public bool IsReusable
		{
			get { return false; }
		}

		public void ProcessRequest (HttpContext context)
		{
			WebTest.CurrentTest.Invoke (null);
		}
	}
}

