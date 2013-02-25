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
	private Uri googleUri;
	private Uri yahooUri;
	private Uri apacheUri;
	private int maxIdle;
	
	[SetUp]
        public void GetReady () 
	{
		maxIdle = ServicePointManager.MaxServicePointIdleTime;
		ServicePointManager.MaxServicePointIdleTime = 10;
		googleUri = new Uri ("http://www.google.com");
		yahooUri = new Uri ("http://www.yahoo.com");
		apacheUri = new Uri ("http://www.apache.org");
	}

	[TearDown]
	public void Finish ()
	{
		ServicePointManager.MaxServicePointIdleTime = maxIdle;
	}

        [Test, ExpectedException (typeof (InvalidOperationException))]
		[Category ("InetAccess")]
#if TARGET_JVM
	[Ignore ("Unsupported property - ServicePointManager.MaxServicePoints")]
#endif
        public void MaxServicePointManagers ()
        {
		Assert.AreEqual (0, ServicePointManager.MaxServicePoints, "#1");
		
		DoWebRequest (googleUri);
		Thread.Sleep (100);
		DoWebRequest (yahooUri);
		Thread.Sleep (100);
		DoWebRequest (apacheUri);
		Thread.Sleep (100);
		
		ServicePoint sp = ServicePointManager.FindServicePoint (googleUri);
		//WriteServicePoint (sp);
		sp = ServicePointManager.FindServicePoint (yahooUri);
		//WriteServicePoint (sp);
		sp = ServicePointManager.FindServicePoint (apacheUri);
		//WriteServicePoint (sp);
		
		ServicePointManager.MaxServicePoints = 1;

		sp = ServicePointManager.FindServicePoint (googleUri);
		//WriteServicePoint (sp);
		sp = ServicePointManager.FindServicePoint (yahooUri);
		//WriteServicePoint (sp);
		sp = ServicePointManager.FindServicePoint (apacheUri);
		//WriteServicePoint (sp);
		
		GC.Collect ();
		
		// hmm... aparently ms.net still has the service points even
		// though I set it to a max of 1.
		
		// this should force an exception then...		
		sp = ServicePointManager.FindServicePoint (new Uri ("http://www.microsoft.com"));
		//WriteServicePoint (sp);
	}
	
        [Test]
	public void FindServicePoint ()
	{
		ServicePointManager.MaxServicePoints = 0;
		ServicePoint sp = ServicePointManager.FindServicePoint (googleUri, new WebProxy (apacheUri));
		Assert.AreEqual (apacheUri, sp.Address, "#1");
#if NET_2_1
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

