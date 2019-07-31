//
// HttpWebRequestCas.cs - CAS unit tests for System.Net.WebRequest class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;

using System;
using System.IO;
using System.Net;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace MonoCasTests.System.Net {

	[TestFixture]
	[Category ("CAS")]
	public class HttpWebRequestCas {

		private const int timeout = 30000;

		static ManualResetEvent reset;
		private string message;
		private string uri = "http://www.example.com";

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			reset = new ManualResetEvent (false);
		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
			reset.Close ();
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		// async tests (for stack propagation)

		private void GetRequestStreamCallback (IAsyncResult ar)
		{
			HttpWebRequest hwr = (HttpWebRequest)ar.AsyncState;
			hwr.EndGetRequestStream (ar);
			try {
				// can we do something bad here ?
				Assert.IsNotNull (Environment.GetEnvironmentVariable ("USERNAME"));
				message = "Expected a SecurityException";
			}
			catch (SecurityException) {
				message = null;
				reset.Set ();
			}
			catch (Exception e) {
				message = e.ToString ();
			}
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "USERNAME")]
		public void AsyncGetRequestStream ()
		{
			HttpWebRequest w = (HttpWebRequest)WebRequest.Create (uri);
			w.Method = "PUT";
			message = "AsyncGetRequestStream";
			reset.Reset ();
			IAsyncResult r = w.BeginGetRequestStream (new AsyncCallback (GetRequestStreamCallback), w);
			Assert.IsNotNull (r, "IAsyncResult");
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
		}

		private void GetResponseCallback (IAsyncResult ar)
		{
			HttpWebRequest hwr = (HttpWebRequest)ar.AsyncState;
			hwr.EndGetResponse (ar);
			try {
				// can we do something bad here ?
				Assert.IsNotNull (Environment.GetEnvironmentVariable ("USERNAME"));
				message = "Expected a SecurityException";
			}
			catch (SecurityException) {
				message = null;
				reset.Set ();
			}
			catch (Exception e) {
				message = e.ToString ();
			}
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "USERNAME")]
		public void AsyncGetResponse ()
		{
			HttpWebRequest w = (HttpWebRequest)WebRequest.Create (uri);
			message = "AsyncGetResponse";
			reset.Reset ();
			IAsyncResult r = w.BeginGetResponse (new AsyncCallback (GetResponseCallback), w);
			Assert.IsNotNull (r, "IAsyncResult");
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
		}
	}
}
