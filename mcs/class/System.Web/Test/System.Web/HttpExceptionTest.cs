using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Management;

using NUnit.Framework;

namespace MonoTests.System.Web
{
	[TestFixture]
	public class HttpExceptionTest
	{
#if NET_4_0
		[Test]
		public void WebEventCode ()
		{
			var ex = new HttpException ();
			Assert.AreEqual (WebEventCodes.UndefinedEventCode, ex.WebEventCode);
		}
#endif
	}
}
