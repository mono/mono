//
// WebRequestTest.cs - NUnit Test Cases for System.Net.WebRequest
//
// Authors:
//	Lawrence Pit (loz@cable.a2000.nl)
//	Martin Willemoes Hansen (mwh@sysrq.dk)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Martin Willemoes Hansen
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Net;
using System.Collections;
using System.Runtime.Serialization;

namespace MonoTests.System.Net {

	// WebRequest is abstract

	public class NonAbstractWebRequest : WebRequest
	{

		public NonAbstractWebRequest ()
		{
		}

		public NonAbstractWebRequest (SerializationInfo si, StreamingContext sc)
			: base (si, sc)
		{
		}
	}

	[TestFixture]
	public class WebRequestTest {

		private void Callback (IAsyncResult ar)
		{
			Assert.Fail ("Callback");
		}

		[Test]
		public void SerializationConstructor ()
		{
			NonAbstractWebRequest w = new NonAbstractWebRequest (null, new StreamingContext ());
			Assert.IsNotNull (w);
		}

		// properties (only test 'get'ter)

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void ConnectionGroupName ()
		{
			NonAbstractWebRequest w = new NonAbstractWebRequest ();
			Assert.IsNull (w.ConnectionGroupName);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void ContentLength ()
		{
			NonAbstractWebRequest w = new NonAbstractWebRequest ();
			Assert.IsNull (w.ContentLength);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void ContentType ()
		{
			NonAbstractWebRequest w = new NonAbstractWebRequest ();
			Assert.IsNull (w.ContentType);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void Credentials ()
		{
			NonAbstractWebRequest w = new NonAbstractWebRequest ();
			Assert.IsNull (w.Credentials);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void Headers ()
		{
			NonAbstractWebRequest w = new NonAbstractWebRequest ();
			Assert.IsNull (w.Headers);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void Method ()
		{
			NonAbstractWebRequest w = new NonAbstractWebRequest ();
			Assert.IsNull (w.Method);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void PreAuthenticate ()
		{
			NonAbstractWebRequest w = new NonAbstractWebRequest ();
			Assert.IsTrue (w.PreAuthenticate);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void Proxy ()
		{
			NonAbstractWebRequest w = new NonAbstractWebRequest ();
			Assert.IsNull (w.Proxy);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void RequestUri ()
		{
			NonAbstractWebRequest w = new NonAbstractWebRequest ();
			Assert.IsNull (w.RequestUri);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void Timeout ()
		{
			NonAbstractWebRequest w = new NonAbstractWebRequest ();
			Assert.IsNull (w.Timeout);
		}

		// methods

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void Abort ()
		{
			NonAbstractWebRequest w = new NonAbstractWebRequest ();
			w.Abort ();
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void BeginGetRequestStream ()
		{
			NonAbstractWebRequest w = new NonAbstractWebRequest ();
			IAsyncResult r = w.BeginGetRequestStream (new AsyncCallback (Callback), w);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void BeginGetResponse ()
		{
			NonAbstractWebRequest w = new NonAbstractWebRequest ();
			IAsyncResult r = w.BeginGetResponse (new AsyncCallback (Callback), w);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void EndGetRequestStream ()
		{
			NonAbstractWebRequest w = new NonAbstractWebRequest ();
			w.EndGetRequestStream (null);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void EndGetResponse ()
		{
			NonAbstractWebRequest w = new NonAbstractWebRequest ();
			w.EndGetResponse (null);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void GetRequestStream ()
		{
			NonAbstractWebRequest w = new NonAbstractWebRequest ();
			w.GetRequestStream ();
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void GetResponse ()
		{
			NonAbstractWebRequest w = new NonAbstractWebRequest ();
			w.GetResponse ();
		}

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

