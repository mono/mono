//
// FileWebRequestCas.cs - CAS unit tests for System.Net.WebRequest class
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
	public class FileWebRequestCas {

		private const int timeout = 30000;

		static ManualResetEvent reset;
		private string message;
		private string uri;
		private string file;

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

			// unique uri for each test
			file = Path.GetTempFileName ();
			// to please both Windows and Unix file systems
			uri = ((file [0] == '/') ? "file://" : "file:///") + file.Replace ('\\', '/');
		}

		// async tests (for stack propagation)

		private void GetRequestStreamCallback (IAsyncResult ar)
		{
			FileWebRequest fwr = (FileWebRequest)ar.AsyncState;
			fwr.EndGetRequestStream (ar);
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
			if (File.Exists (file))
				File.Delete (file);
			FileWebRequest w = (FileWebRequest)WebRequest.Create (uri);
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
			FileWebRequest fwr = (FileWebRequest)ar.AsyncState;
			fwr.EndGetResponse (ar);
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
			FileWebRequest w = (FileWebRequest)WebRequest.Create (uri);
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
