//
// WebProxyTest.cs - NUnit Test Cases for System.Net.WebProxy
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using NUnit.Framework;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Threading;

namespace MonoTests.System.Net
{

public class WebProxyTest : TestCase
{
	private Uri googleUri;
	private Uri yahooUri;
	private Uri apacheUri;
	
        public WebProxyTest () :
                base ("[MonoTests.System.Net.WebProxyTest]") {}

        public WebProxyTest (string name) : base (name) {}

        protected override void SetUp () {
		googleUri = new Uri ("http://www.google.com");
		yahooUri = new Uri ("http://www.yahoo.com");
		apacheUri = new Uri ("http://www.apache.org");
	}

        protected override void TearDown () {}

        public static ITest Suite
        {
                get {
                        return new TestSuite (typeof (WebProxyTest));
                }
        }
        
        public void TestConstructors ()
        {
		WebProxy p = new WebProxy ();
		Assert("#1", p.Address == null);
		AssertEquals ("#2", 0, p.BypassArrayList.Count);
		AssertEquals ("#3", 0, p.BypassList.Length);
		AssertEquals ("#4", false, p.BypassProxyOnLocal);
		try {
			p.BypassList = null;
			Fail ("#5 not spec'd, but should follow ms.net implementation");
		} catch (ArgumentNullException) {}
	}
	
	public void TestGetProxy ()
	{
	}	
	
	public void TestIsByPassed ()
	{
		WebProxy p = new WebProxy ("http://proxy.contoso.com", true);
		Assert ("#1", !p.IsBypassed (new Uri ("http://www.google.com")));
		Assert ("#2", p.IsBypassed (new Uri ("http://localhost/index.html")));
		Assert ("#3", p.IsBypassed (new Uri ("http://localhost:8080/index.html")));
		Assert ("#4", p.IsBypassed (new Uri ("http://loopback:8080/index.html")));
		Assert ("#5", p.IsBypassed (new Uri ("http://127.0.0.01:8080/index.html")));
		Assert ("#6", p.IsBypassed (new Uri ("http://webserver/index.html")));
		Assert ("#7", !p.IsBypassed (new Uri ("http://webserver.com/index.html")));
		try {
			p.IsBypassed (null);
			Fail ("#8 not spec'd, but should follow ms.net implementation");
		} catch (NullReferenceException) {}
		
		p = new WebProxy ("http://proxy.contoso.com", false);
		Assert ("#11", !p.IsBypassed (new Uri ("http://www.google.com")));
		Assert ("#12: lamespec of ms.net", p.IsBypassed (new Uri ("http://localhost/index.html")));
		Assert ("#13: lamespec of ms.net", p.IsBypassed (new Uri ("http://localhost:8080/index.html")));
		Assert ("#14: lamespec of ms.net", p.IsBypassed (new Uri ("http://loopback:8080/index.html")));
		Assert ("#15: lamespec of ms.net", p.IsBypassed (new Uri ("http://127.0.0.01:8080/index.html")));
		Assert ("#16", !p.IsBypassed (new Uri ("http://webserver/index.html")));
		
		p.BypassList = new string [] { "google.com", "contoso.com" };
		Assert ("#20", p.IsBypassed (new Uri ("http://www.google.com")));
		Assert ("#21", p.IsBypassed (new Uri ("http://www.GOOGLE.com")));
		Assert ("#22", p.IsBypassed (new Uri ("http://www.contoso.com:8080/foo/bar/index.html")));
		Assert ("#23", !p.IsBypassed (new Uri ("http://www.contoso2.com:8080/foo/bar/index.html")));
		Assert ("#24", !p.IsBypassed (new Uri ("http://www.foo.com:8080/contoso.com.html")));
	}
}

}

