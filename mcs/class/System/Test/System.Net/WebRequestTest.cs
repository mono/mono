//
// WebRequestTest.cs - NUnit Test Cases for System.Net.WebRequest
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using NUnit.Framework;
using System;
using System.Net;
using System.Collections;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Net
{

[TestFixture]
public class WebRequestTest
{
        [Test]
        public void All ()
        {
		WebRequest req = WebRequest.Create ("http://www.contoso.com");
		Assertion.Assert ("#1", req is HttpWebRequest);
		req = WebRequest.Create ("https://www.contoso.com");
		Assertion.Assert ("#2", req is HttpWebRequest);
		req = WebRequest.Create ("file://www.contoso.com");
		Assertion.Assert ("#3", req is FileWebRequest);
		
		WebRequest.RegisterPrefix ("http://www.contoso.com", new TestWebRequestCreator ());
		bool ret = WebRequest.RegisterPrefix ("http://WWW.contoso.com", new TestWebRequestCreator ());
		Assertion.AssertEquals ("#4a", false, ret);
		ret = WebRequest.RegisterPrefix ("http://www.contoso.com/foo/bar", new TestWebRequestCreator2 ());
		Assertion.AssertEquals ("#4b", true, ret);
		ret = WebRequest.RegisterPrefix ("http://www", new TestWebRequestCreator3 ());
		Assertion.AssertEquals ("#4c", true, ret);

		req = WebRequest.Create ("http://WWW.contoso.com");
		Assertion.Assert ("#5", req is TestWebRequest); 

		req = WebRequest.Create ("http://WWW.contoso.com/foo/bar/index.html");
		Assertion.Assert ("#6", req is TestWebRequest2); 
		
		req = WebRequest.Create ("http://WWW.x.com");
		Assertion.Assert ("#7", req is TestWebRequest3); 

		req = WebRequest.Create ("http://WWW.c");
		Assertion.Assert ("#8", req is TestWebRequest3); 

		req = WebRequest.CreateDefault (new Uri("http://WWW.contoso.com"));
		Assertion.Assert ("#9", req is HttpWebRequest);

		try {
			req = WebRequest.Create ("tcp://www.contoso.com");
			Assertion.Fail ("#10 should have failed with NotSupportedException");			
		} catch (NotSupportedException) {			
		}		
	}
	
	internal class TestWebRequestCreator : IWebRequestCreate
	{
		internal TestWebRequestCreator () { }
		
		public WebRequest Create (Uri uri)
		{
			return new TestWebRequest ();
		}
	}
	
	internal class TestWebRequest : WebRequest
	{
		internal TestWebRequest () { }
	}

	internal class TestWebRequestCreator2 : IWebRequestCreate
	{
		internal TestWebRequestCreator2 () { }
		
		public WebRequest Create (Uri uri)
		{
			return new TestWebRequest2 ();
		}
	}
	
	internal class TestWebRequest2 : WebRequest
	{
		internal TestWebRequest2 () { }
	}

	internal class TestWebRequestCreator3 : IWebRequestCreate
	{
		internal TestWebRequestCreator3 () { }
		
		public WebRequest Create (Uri uri)
		{
			return new TestWebRequest3 ();
		}
	}
	
	internal class TestWebRequest3 : WebRequest
	{
		internal TestWebRequest3 () { }
	}
}

}

