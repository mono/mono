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

		p = new WebProxy ("webserver.com", 8080);
		AssertEquals ("#6", new Uri ("http://webserver.com:8080/"), p.Address);
		
		p = new WebProxy ("webserver");
		AssertEquals ("#7", new Uri ("http://webserver"), p.Address);

		p = new WebProxy ("webserver.com");
		AssertEquals ("#8", new Uri ("http://webserver.com"), p.Address);

		p = new WebProxy ("http://webserver.com");
		AssertEquals ("#9", new Uri ("http://webserver.com"), p.Address);

		p = new WebProxy ("file://webserver");
		AssertEquals ("#10", new Uri ("file://webserver"), p.Address);		
		
		p = new WebProxy ("http://www.contoso.com", true, null, null);
		AssertEquals ("#11", 0, p.BypassList.Length);
		AssertEquals ("#12", 0, p.BypassArrayList.Count);
		
		try {
			p = new WebProxy ("http://contoso.com", true, 
				new string [] {"?^!@#$%^&}{]["}, null);
			Fail ("#13: illegal regular expression");
		} catch (ArgumentException) {
		}
	}
	
	public void TestBypassArrayList ()
	{
		Uri proxy1 = new Uri("http://proxy.contoso.com");
		Uri proxy2 = new Uri ("http://proxy2.contoso.com");
		
		WebProxy p = new WebProxy (proxy1, true);
		p.BypassArrayList.Add ("http://proxy2.contoso.com");
		p.BypassArrayList.Add ("http://proxy2.contoso.com");		
		AssertEquals ("#1", 2, p.BypassList.Length);
		Assert ("#2", !p.IsBypassed (new Uri ("http://www.google.com")));
		Assert ("#3", p.IsBypassed (proxy2));
		AssertEquals ("#4", proxy2, p.GetProxy (proxy2));

		p.BypassArrayList.Add ("?^!@#$%^&}{][");
		AssertEquals ("#10", 3, p.BypassList.Length);
		try {
			Assert ("#11", !p.IsBypassed (proxy2));
			Assert ("#12", !p.IsBypassed (new Uri ("http://www.x.com")));		
			AssertEquals ("#13", proxy1, p.GetProxy (proxy2));
			// hmm... although #11 and #13 succeeded before (#3 resp. #4), 
			// it now fails to bypass, and the IsByPassed and GetProxy 
			// methods do not fail.. so when an illegal regular 
			// expression is added through this property it's ignored. 
			// probably an ms.net bug?? :(
		} catch (ArgumentException) {
			Fail ("#15: illegal regular expression");
		}		
	}
	
	public void TestBypassList ()
	{
		Uri proxy1 = new Uri("http://proxy.contoso.com");
		Uri proxy2 = new Uri ("http://proxy2.contoso.com");
		
		WebProxy p = new WebProxy (proxy1, true);
		try {
			p.BypassList = new string [] {"http://proxy2.contoso.com", "?^!@#$%^&}{]["};		
			Fail ("#1");
		} catch (ArgumentException) {
			// weird, this way invalid regex's fail again..
		}
		
		AssertEquals ("#2", 2, p.BypassList.Length);
		// but it did apparenly store the regex's !

		p.BypassList = new string [] {"http://www.x.com"};		
		AssertEquals ("#3", 1, p.BypassList.Length);

		try {
			p.BypassList = null;
			Fail ("#4");
		} catch (ArgumentNullException) {}
		
		AssertEquals ("#4", 1, p.BypassList.Length);		
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
		
		p.BypassList = new string [] { "https" };		
		Assert ("#30", !p.IsBypassed (new Uri ("http://www.google.com")));
		Assert ("#31", p.IsBypassed (new Uri ("https://www.google.com")));
	}
}

}

