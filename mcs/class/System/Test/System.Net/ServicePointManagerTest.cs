//
// ServicePointManagerTest.cs - NUnit Test Cases for System.Net.ServicePointManager
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
using System.Threading;

namespace MonoTests.System.Net
{

[TestFixture]
public class ServicePointManagerTest
{
	private Uri exampleComUri;
	private Uri exampleOrgUri;
	private int maxIdle;
	
	[SetUp]
        public void GetReady () 
	{
#if !FEATURE_NO_BSD_SOCKETS
		maxIdle = ServicePointManager.MaxServicePointIdleTime;
		ServicePointManager.MaxServicePointIdleTime = 10;
#endif
		exampleComUri = new Uri ("http://www.example.com");
		exampleOrgUri = new Uri ("http://www.example.org");
	}

	[TearDown]
	public void Finish ()
	{
#if !FEATURE_NO_BSD_SOCKETS
		ServicePointManager.MaxServicePointIdleTime = maxIdle;
#endif
	}

        [Test, ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")]
		[Category ("InetAccess")]
        public void MaxServicePointManagers ()
        {
		Assert.AreEqual (0, ServicePointManager.MaxServicePoints, "#1");
		
		DoWebRequest (exampleComUri);
		Thread.Sleep (100);
		DoWebRequest (exampleOrgUri);
		Thread.Sleep (100);
		
		ServicePoint sp = ServicePointManager.FindServicePoint (exampleComUri);
		//WriteServicePoint (sp);
		sp = ServicePointManager.FindServicePoint (exampleOrgUri);
		//WriteServicePoint (sp);
		
		ServicePointManager.MaxServicePoints = 1;

		sp = ServicePointManager.FindServicePoint (exampleComUri);
		//WriteServicePoint (sp);
		sp = ServicePointManager.FindServicePoint (exampleOrgUri);
		//WriteServicePoint (sp);
		
		GC.Collect ();
		
		// hmm... aparently ms.net still has the service points even
		// though I set it to a max of 1.
		
		// this should force an exception then...		
		sp = ServicePointManager.FindServicePoint (new Uri ("http://www.microsoft.com"));
		//WriteServicePoint (sp);
	}
	
	[Test]
	[Category ("InetAccess")]
#if FEATURE_NO_BSD_SOCKETS
	[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
	public void FindServicePoint ()
	{
		ServicePointManager.MaxServicePoints = 0;
		ServicePoint sp = ServicePointManager.FindServicePoint (exampleComUri, new WebProxy (exampleOrgUri));
		Assert.AreEqual (exampleOrgUri, sp.Address, "#1");
#if MOBILE
		Assert.AreEqual (10, sp.ConnectionLimit, "#2");
#else
		Assert.AreEqual (2, sp.ConnectionLimit, "#2");
#endif
		Assert.AreEqual ("http", sp.ConnectionName, "#3");
	}
	
	private void DoWebRequest (Uri uri)
	{
		WebRequest.Create (uri).GetResponse ().Close ();
	}

/* Unused code for now, but might be useful later for debugging
	private void WriteServicePoint (ServicePoint sp)
	{
		Console.WriteLine ("\nAddress: " + sp.Address);
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

