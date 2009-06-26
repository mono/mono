using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MonoTests.Common
{
	class MyHttpContextWrapper : HttpContextBase
	{
		MyHttpRequestWrapper request;

		public override HttpRequestBase Request {
			get {
				if (request == null)
					request = new MyHttpRequestWrapper ();

				return request;
			}
		}
	}
}
