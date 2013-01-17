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
using Socks = System.Net.Sockets;


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
#if NET_2_0
			NonAbstractWebRequest w = new NonAbstractWebRequest (null, new StreamingContext ());
			Assert.IsNotNull (w);
#else
			try {
				new NonAbstractWebRequest (null, new StreamingContext ());
				Assert.Fail ("#1");
			} catch (NotImplementedException) {
			}
#endif
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
		Assert.IsTrue (req is HttpWebRequest, "#1");
		req = WebRequest.Create ("https://www.contoso.com");
		Assert.IsTrue (req is HttpWebRequest, "#2");
		req = WebRequest.Create ("file://www.contoso.com");
		Assert.IsTrue (req is FileWebRequest, "#3");
#if NET_2_0
		req = WebRequest.Create ("ftp://www.contoso.com");
		Assert.IsTrue (req is FtpWebRequest, "#4");
#endif
		WebRequest.RegisterPrefix ("http://www.contoso.com", new TestWebRequestCreator ());
		bool ret = WebRequest.RegisterPrefix ("http://WWW.contoso.com", new TestWebRequestCreator ());
		Assert.AreEqual (false, ret, "#5a");
		ret = WebRequest.RegisterPrefix ("http://www.contoso.com/foo/bar", new TestWebRequestCreator2 ());
		Assert.AreEqual (true, ret, "#5b");
		ret = WebRequest.RegisterPrefix ("http://www", new TestWebRequestCreator3 ());
		Assert.AreEqual (true, ret, "#5c");

		req = WebRequest.Create ("http://WWW.contoso.com");
		Assert.IsTrue (req is TestWebRequest, "#6"); 

		req = WebRequest.Create ("http://WWW.contoso.com/foo/bar/index.html");
		Assert.IsTrue (req is TestWebRequest2, "#7"); 
		
		req = WebRequest.Create ("http://WWW.x.com");
		Assert.IsTrue (req is TestWebRequest3, "#8"); 

		req = WebRequest.Create ("http://WWW.c");
		Assert.IsTrue (req is TestWebRequest3, "#9"); 

		req = WebRequest.CreateDefault (new Uri("http://WWW.contoso.com"));
		Assert.IsTrue (req is HttpWebRequest, "#10");

		try {
			req = WebRequest.Create ("tcp://www.contoso.com");
			Assert.Fail ("#11 should have failed with NotSupportedException");
		} catch (NotSupportedException) {
		}
	}

	[Test]
	public void Create_RequestUriString_Null ()
	{
		try {
			WebRequest.Create ((string) null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("requestUriString", ex.ParamName, "#6");
		}
	}

	[Test]
	public void CreateDefault_RequestUri_Null ()
	{
		try {
			WebRequest.CreateDefault ((Uri) null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("requestUri", ex.ParamName, "#6");
		}
	}

#if NET_2_0
	[Test]
	public void DefaultWebProxy ()
	{
		WebProxy proxy = new WebProxy ("proxy.intern.com", 83);

		WebRequest.DefaultWebProxy = proxy;
		Assert.IsNotNull (WebRequest.DefaultWebProxy, "#A1");
		Assert.AreSame (proxy, WebRequest.DefaultWebProxy, "#A2");

		HttpWebRequest req = (HttpWebRequest) WebRequest.CreateDefault (
			new Uri ("http://www.mono-project.com"));
		Assert.IsNotNull (req.Proxy, "#B1");
		Assert.AreSame (proxy, req.Proxy, "#B2");

		WebRequest.DefaultWebProxy = null;
		Assert.IsNull (WebRequest.DefaultWebProxy, "#C1");
		Assert.IsNotNull (req.Proxy, "#C2");
		Assert.AreSame (proxy, req.Proxy, "#C3");

		req = (HttpWebRequest) WebRequest.CreateDefault (
			new Uri ("http://www.mono-project.com"));
		Assert.IsNull (req.Proxy, "#D");
	}
#endif

	[Test]
	public void RegisterPrefix_Creator_Null ()
	{
		try {
			WebRequest.RegisterPrefix ("http://www.mono-project.com", (IWebRequestCreate) null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("creator", ex.ParamName, "#6");
		}
	}

	[Test]
	public void RegisterPrefix_Prefix_Null ()
	{
		try {
			WebRequest.RegisterPrefix ((string) null, new TestWebRequestCreator ());
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsNotNull (ex.ParamName, "#5");
			Assert.AreEqual ("prefix", ex.ParamName, "#6");
		}
	}

	[Test] //BNC#323452
	public void TestFailedConnection ()
	{
		try {
			WebRequest.Create ("http://127.0.0.1:0/non-existant.txt").GetResponse ();
			Assert.Fail ("Should have raised an exception");
		} catch (Exception e) {
			Assert.IsTrue (e is WebException, "Got " + e.GetType ().Name + ": " + e.Message);
			//#if NET_2_0 e.Message == "Unable to connect to the remote server"
			//#if NET_1_1 e.Message == "The underlying connection was closed: Unable to connect to the remote server."

			Assert.AreEqual (((WebException)e).Status, WebExceptionStatus.ConnectFailure);

			//#if !NET_1_1 (this is not true in .NET 1.x)
			Assert.IsNotNull (e.InnerException);
			Assert.IsTrue (e.InnerException is Socks.SocketException, "InnerException should be SocketException");
			//e.Message == "The requested address is not valid in its context 127.0.0.1:0"
			//#endif
		}
	}

	[Test] //BNC#323452
	public void TestFailedResolution ()
	{
		try {
			var req = WebRequest.Create ("http://thisdomaindoesnotexist.monotestcase.x/non-existant.txt").GetResponse ();
			/*
			 * Work around broken t-online.de DNS Server.
			 * 
			 * T-Online's DNS Server for DSL Customers resolves
			 * non-exisitng domain names to
			 * http://navigationshilfe1.t-online.de/dnserror?url=....
			 * instead of reporting an error.
			 */
			if (req.ResponseUri.DnsSafeHost.Equals ("navigationshilfe1.t-online.de"))
				return;

			Assert.Fail ("Should have raised an exception");
		} catch (Exception e) {
			Assert.IsTrue (e is WebException);
			//#if NET_2_0 e.Message == "The underlying connection was closed: The remote name could not be resolved."
			//#if NET_1_1 e.Message == "The remote name could not be resolved: 'thisdomaindoesnotexist.monotestcase.x'"
			Assert.AreEqual (((WebException)e).Status, WebExceptionStatus.NameResolutionFailure);
			Assert.IsNull (e.InnerException);
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

