//
// HttpWebRequestTest.cs - NUnit Test Cases for System.Net.HttpWebRequest
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using NUnit.Framework;
using System;
using System.Net;
using System.Collections;

namespace MonoTests.System.Net
{

public class HttpWebRequestTest : TestCase
{
        public HttpWebRequestTest () :
                base ("[MonoTests.System.Net.HttpWebRequestTest]") {}

        public HttpWebRequestTest (string name) : base (name) {}

        protected override void SetUp () {}

        protected override void TearDown () {}

        public static ITest Suite
        {
                get {
                        return new TestSuite (typeof (HttpWebRequestTest));
                }
        }
        
        public void TestSync ()
        {
		try {
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create ("http://www.google.com");
			req.UserAgent = "MonoClient v1.0";
			Console.WriteLine ("req:If Modified Since: " + req.IfModifiedSince);
			WriteHeaders ("req:", req.Headers);		

			HttpWebResponse res = (HttpWebResponse) req.GetResponse ();
			Console.WriteLine ("res:HttpStatusCode: " + res.StatusCode);
			Console.WriteLine ("res:HttpStatusDescription: " + res.StatusDescription);
			
			WriteHeaders ("res:", res.Headers);		
			Console.WriteLine("Last Modified: " + res.LastModified);
			
			WriteCookies ("res:", res.Cookies);
				
			WriteHeaders ("req:", req.Headers);		
				
			res.Close ();
		} catch (WebException e) {
			Console.WriteLine("\nThe following Exception was raised : {0}", e.Message);
		}
	}
	
        public void TestAsync ()
        {
	}
	
	public void TestAddRange ()
	{
		HttpWebRequest req = (HttpWebRequest) WebRequest.Create ("http://www.google.com");
		req.AddRange (10);
		req.AddRange (50, 90);
		req.AddRange ("bytes", 100); 
		req.AddRange ("bytes", 100, 120);
		AssertEquals ("#1", "bytes=10-,50-90,100-,100-120", req.Headers ["Range"]);
		try {
			req.AddRange ("bits", 2000);
			Fail ("#2");
		} catch (InvalidOperationException) {}
	}
	
	private void WriteHeaders (string label, WebHeaderCollection col) 
	{
		label += "Headers";
		if (col.Count == 0)
			Console.WriteLine (label + "Nothing in web headers collection\n");
		else
			Console.WriteLine (label);
		for (int i = 0; i < col.Count; i++)
			Console.WriteLine ("\t" + col.GetKey (i) + ": " + col.Get (i));
	}
	
	private void WriteCookies (string label, CookieCollection col) 
	{
		label += "Cookies";
		if (col.Count == 0)
			Console.WriteLine (label + "Nothing in cookies collection\n");
		else
			Console.WriteLine (label);
		for (int i = 0; i < col.Count; i++)
			Console.WriteLine ("\t" + col [i]);
	}
}

}

