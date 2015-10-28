//
// ServicePointTest.cs - NUnit Test Cases for System.Net.ServicePoint
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using NUnit.Framework;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;

namespace MonoTests.System.Net
{

[TestFixture]
public class ServicePointTest
{
	static private int max;
	[SetUp]
	public void SaveMax () {
		max = ServicePointManager.MaxServicePoints;
		ServicePointManager.MaxServicePoints = 0;
	}

	[TearDown]
	public void RestoreMax () {
		ServicePointManager.MaxServicePoints = max;
	}

        [Test]
		[Category ("InetAccess")]
        public void All ()
        {
		ServicePoint p = ServicePointManager.FindServicePoint (new Uri ("mailto:xx@yyy.com"));
		//WriteServicePoint ("A servicepoint that isn't really", p);			
		
		ServicePointManager.MaxServicePoints = 2;
		ServicePoint google = ServicePointManager.FindServicePoint (new Uri ("http://www.google.com"));
		try {			
			ServicePoint slashdot = ServicePointManager.FindServicePoint (new Uri ("http://www.slashdot.org"));
			Assert.Fail ("#1");
		} catch (InvalidOperationException) { }
		ServicePointManager.MaxServicePoints = 0;
		
		//WriteServicePoint ("google before getting a webrequest", google);
		
		HttpWebRequest req = (HttpWebRequest) WebRequest.Create ("http://www.google.com");
		HttpWebResponse res = (HttpWebResponse) req.GetResponse ();			
		
#if FOUND_SOME_OTHER_URL
		// URL is no longer found, disabled the test until a more reliable URL is found :P
		//WriteServicePoint ("google after getting a response", google);
		ServicePoint google2 = ServicePointManager.FindServicePoint (new Uri ("http://www.google.com/dilbert.html"));
		Assert.AreEqual (google, google2, "#equals");
		res.Close ();
#endif
		
		// in both instances property CurrentConnections is 0 according to ms.net.
		// let's see what it says when we do async operations...
		
		HttpWebRequest req2 = (HttpWebRequest) WebRequest.Create ("http://www.google.com");
		req2.Method = "PUT";
		IAsyncResult async = req2.BeginGetRequestStream (null, null);
		//WriteServicePoint ("after async BeginGetRequestStream", google);
		// CurrentConnections: 1
		Stream stream2 = req2.EndGetRequestStream (async);
		//WriteServicePoint ("after async EndGetRequestStream", google);
		// CurrentConnections: 1
		stream2.Close ();
		
		req2 = (HttpWebRequest) WebRequest.Create ("http://www.google.com");
		async = req2.BeginGetResponse (null, null);
		//WriteServicePoint ("after async BeginGetResponse", google);
		// CurrentConnections: 2
		WebResponse res2 = req2.EndGetResponse (async);
		//WriteServicePoint ("after async EndGetResponse", google);
		// CurrentConnections: 0			
		// curious that after you get the webresponse object CurrentConnections is set to 0.
		// you'd think that you'd still be connected until you close the webresponse..
		//Console.WriteLine ("ContentLength: " + res2.ContentLength);
		res2.Close ();
		
		ServicePoint sp2;
#if FOUND_SOME_OTHER_URL
		// unless of course some buffering is taking place.. let's check
		Uri uri2 = new Uri ("http://freedesktop.org/Software/pkgconfig/releases/pkgconfig-0.15.0.tar.gz");
		sp2 = ServicePointManager.FindServicePoint (uri2);
		req2 = (HttpWebRequest) WebRequest.Create (uri2);
		async = req2.BeginGetResponse (null, null);
		//WriteServicePoint ("Large file: after async BeginGetResponse", sp2);
		// CurrentConnections: 1
		res2 = req2.EndGetResponse (async);
		//WriteServicePoint ("Large file: after async EndGetResponse", sp2);
		// CurrentConnections: 1
		// and so it shows
		//Console.WriteLine ("ContentLength: " + res2.ContentLength);
		res2.Close ();
#endif
		
		
		// what's the limit of the cache?
		req2 = (HttpWebRequest) WebRequest.Create ("http://www.apache.org/");
		res2 = req2.GetResponse ();
		sp2 = ServicePointManager.FindServicePoint (new Uri("http://www.apache.org/"));
		//WriteServicePoint ("apache", sp2);
		//Console.WriteLine ("ContentLength: " + res2.ContentLength);
		// CurrentConnections: 1
		res2.Close ();
		// curious other effect: address is actually the full Uri of the previous request
		// anyways, buffer is probably 4096 bytes
	}

	// try getting the stream to 5 web response objects	
	// while ConnectionLimit equals 2

	[Test]
	[Category ("InetAccess")]
	public void ConnectionLimit ()
	{		
		// the default is already 2, just in case it isn't..
		ServicePointManager.DefaultConnectionLimit = 5;
		
		Uri uri = new Uri ("http://www.go-mono.com/");
		ServicePoint sp = ServicePointManager.FindServicePoint (uri);			
		WebResponse [] res = new WebResponse [5];
		for (int i = 0; i < 5; i++) {
			//Console.WriteLine ("GOT1 : " + i);
			HttpWebRequest req = (HttpWebRequest) WebRequest.Create (uri);
			//Console.WriteLine ("GOT2 : " + i);
			res [i] = req.GetResponse ();
			//WriteServicePoint ("after getting " + (i + 1) + " web response objects", sp);
		}
		
		for (int i = 0; i < 5; i++) {
			Stream stream = res [i].GetResponseStream();
			//Console.WriteLine ("Reading stream: " + i + " : " + stream);
			int len = 0;
			while (stream.ReadByte () != -1)
				len++;
			//Console.WriteLine ("Finished reading: " + len + " bytes");
		}
		
		for (int i = 0; i < 5; i++) {
			res [i].Close ();
		}
	}

	[Test]
	[Category ("InetAccess")]
	[Category ("AndroidNotWorking")] // #A1 fails
	public void EndPointBind ()
	{
		Uri uri = new Uri ("http://www.go-mono.com/");
		ServicePoint sp = ServicePointManager.FindServicePoint (uri);

		HttpWebRequest req = (HttpWebRequest) WebRequest.Create (uri);

		bool called = false;
		sp.BindIPEndPointDelegate = delegate {
			Assert.IsTrue (!called);
			called = true;
			return null;
		};
		req.GetResponse ().Close ();

		Assert.IsTrue (called, "#A1");

		req = (HttpWebRequest) WebRequest.Create (uri);
		called = false;
		sp.BindIPEndPointDelegate = delegate(ServicePoint point, IPEndPoint remote, int times) {
			Assert.IsTrue (times < 5);
			called = true;
			return new IPEndPoint(IPAddress.Parse("0.0.0.0"), 12345 + times);
		};
		req.GetResponse ().Close ();

		Assert.IsTrue (called, "#A2");
	}

	public static void GetRequestStreamCallback (IAsyncResult asynchronousResult)
	{
	}

	[Test] //Covers #19823
	public void CloseConnectionGroupConcurency ()
	{
		// Try with multiple service points
		for (var i = 0; i < 10; i++) {
			Uri targetUri = new Uri ("http://" + i + ".mono-project.com");
			var req = (HttpWebRequest) HttpWebRequest.Create (targetUri);
			req.ContentType = "application/x-www-form-urlencoded";
			req.Method = "POST";
			req.ConnectionGroupName = "" + i;
			req.ServicePoint.MaxIdleTime = 1;

			req.BeginGetRequestStream (new AsyncCallback (GetRequestStreamCallback), req);
			Thread.Sleep (1);
			req.ServicePoint.CloseConnectionGroup (req.ConnectionGroupName);
		}
	}


	[Test]
	public void DnsRefreshTimeout ()
	{
		const int dnsRefreshTimeout = 2;

		ServicePoint sp;
		IPHostEntry host0, host1, host2;
		Uri uri;
		PropertyInfo hostEntryProperty;

		ServicePointManager.DnsRefreshTimeout = dnsRefreshTimeout;

		uri = new Uri ("http://ww.google.com/");
		sp = ServicePointManager.FindServicePoint (uri);

		hostEntryProperty = typeof (ServicePoint).GetProperty ("HostEntry", BindingFlags.NonPublic | BindingFlags.Instance);

		host0 = hostEntryProperty.GetValue (sp, null) as IPHostEntry;
		host1 = hostEntryProperty.GetValue (sp, null) as IPHostEntry;

		Assert.AreEqual (host0, host1, "HostEntry should result in the same IPHostEntry object.");

		Thread.Sleep (dnsRefreshTimeout);
		host2 = hostEntryProperty.GetValue (sp, null) as IPHostEntry;

		Assert.AreNotEqual(host0, host2, "HostEntry should result in a new IPHostEntry " +
				"object when DnsRefreshTimeout is reached.");
	}

// Debug code not used now, but could be useful later
/*
	private void WriteServicePoint (string label, ServicePoint sp)
	{
		Console.WriteLine ("\n" + label);
		Console.WriteLine ("Address: " + sp.Address);
		Console.WriteLine ("ConnectionLimit: " + sp.ConnectionLimit);
		Console.WriteLine ("ConnectionName: " + sp.ConnectionName);
		Console.WriteLine ("CurrentConnections: " + sp.CurrentConnections);
		Console.WriteLine ("IdleSince: " + sp.IdleSince);
		Console.WriteLine ("MaxIdletime: " + sp.MaxIdleTime);
		Console.WriteLine ("ProtocolVersion: " + sp.ProtocolVersion);
		Console.WriteLine ("SupportsPipelining: " + sp.SupportsPipelining);		
	}
*/
}
}

