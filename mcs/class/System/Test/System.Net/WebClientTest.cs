//
// WebClientTest.cs - NUnit Test Cases for System.Net.WebClient
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Net
{
	[TestFixture]
	public class WebClientTest
	{
		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (WebException))] // Something catches the PlatformNotSupportedException and re-throws an WebException
#endif
		[Category ("InetAccess")]
		public void DownloadTwice ()
		{
			WebClient wc = new WebClient();
			string filename = Path.GetTempFileName();
			
			// A new, but empty file has been created. This is a test case
			// for bug 81005
			wc.DownloadFile("http://example.com/", filename);
			
			// Now, remove the file and attempt to download again.
			File.Delete(filename);
			wc.DownloadFile("http://example.com/", filename);
		}

		[Test]
		public void DownloadData1_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadData ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // DownloadData (string)
		[Category ("BitcodeNotSupported")]
		public void DownloadData1_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadData ("tp://scheme.notsupported");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // DownloadData (Uri)
		public void DownloadData2_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadData ((Uri) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // DownloadData (Uri)
		[Category ("BitcodeNotSupported")]
		public void DownloadData2_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadData (new Uri ("tp://scheme.notsupported"));
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test]
		public void DownloadFile1_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadFile ((string) null, "tmp.out");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // DownloadFile (string, string)
		[Category ("BitcodeNotSupported")]
		public void DownloadFile1_Address_SchemeNotSupported ()
		{
			using (var tmpdir = new TempDirectory ()) {
				string file = Path.Combine (tmpdir.Path, "tmp.out");
				WebClient wc = new WebClient ();
				try {
					wc.DownloadFile ("tp://scheme.notsupported", file);
					Assert.Fail ("#1");
				} catch (WebException ex) {
					// An error occurred performing a WebClient request
					Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
					Assert.IsNotNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.Response, "#5");
					Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

					// The URI prefix is not recognized
					Exception inner = ex.InnerException;
					Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
					Assert.IsNull (inner.InnerException, "#8");
					Assert.IsNotNull (inner.Message, "#9");
				}
			}
		}

		[Test] // DownloadFile (string, string)
		[Category ("BitcodeNotSupported")]
		public void DownloadFile1_FileName_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadFile ("tp://scheme.notsupported",
					(string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("fileName", ex.ParamName, "#6");
			}
		}

		[Test] // DownloadFile (Uri, string)
		[Category ("BitcodeNotSupported")]
		public void DownloadFile2_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadFile ((Uri) null, "tmp.out");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // DownloadFile (Uri, string)
		[Category ("BitcodeNotSupported")]
		public void DownloadFile2_Address_SchemeNotSupported ()
		{
			using (var tmpdir = new TempDirectory ()) {
				string file = Path.Combine (tmpdir.Path, "tmp.out");
				WebClient wc = new WebClient ();
				try {
					wc.DownloadFile (new Uri ("tp://scheme.notsupported"), file);
					Assert.Fail ("#1");
				} catch (WebException ex) {
					// An error occurred performing a WebClient request
					Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
					Assert.IsNotNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.IsNull (ex.Response, "#5");
					Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

					// The URI prefix is not recognized
					Exception inner = ex.InnerException;
					Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
					Assert.IsNull (inner.InnerException, "#8");
					Assert.IsNotNull (inner.Message, "#9");
				}
			}
		}

		[Test] // DownloadFile (Uri, string)
		[Category ("BitcodeNotSupported")]
		public void DownloadFile2_FileName_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadFile (new Uri ("tp://scheme.notsupported"),
					(string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("fileName", ex.ParamName, "#6");
			}
		}

		[Test] // DownloadString (string)
		public void DownloadString1_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadString ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // DownloadString (string)
		[Category ("BitcodeNotSupported")]
		public void DownloadString1_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadString ("tp://scheme.notsupported");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // DownloadString (Uri)
		public void DownloadString2_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadString ((Uri) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // DownloadString (Uri)
		[Category ("BitcodeNotSupported")]
		public void DownloadString2_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadString (new Uri ("tp://scheme.notsupported"));
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test]
		public void EncodingTest ()
		{
			WebClient wc = new WebClient ();
			Assert.AreSame (Encoding.Default, wc.Encoding, "#1");
			wc.Encoding = Encoding.ASCII;
			Assert.AreSame (Encoding.ASCII, wc.Encoding, "#2");
		}

		[Test]
		public void Encoding_Value_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.Encoding = null;
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("Encoding", ex.ParamName, "#6");
			}
		}

		[Test] // OpenRead (string)
		public void OpenRead1_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenRead ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // OpenRead (string)
		[Category ("BitcodeNotSupported")]
		public void OpenRead1_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenRead ("tp://scheme.notsupported");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // OpenRead (Uri)
		[Category ("BitcodeNotSupported")]
		public void OpenRead2_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenRead ((Uri) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // OpenRead (Uri)
		[Category ("BitcodeNotSupported")]
		public void OpenRead2_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenRead (new Uri ("tp://scheme.notsupported"));
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test]
		[Category ("MobileNotWorking")]
		public void OpenReadTaskAsyncOnFile ()
		{
			var tmp = Path.GetTempFileName ();
			string url = "file://" + tmp;

			var client = new WebClient ();
			var task = client.OpenReadTaskAsync (url);

			Assert.IsTrue (task.Wait (2000));
		}

		[Test] // OpenWrite (string)
		public void OpenWrite1_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenWrite ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // OpenWrite (string)
		[Category ("BitcodeNotSupported")]
		public void OpenWrite1_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenWrite ("tp://scheme.notsupported");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // OpenWrite (string, string)
		public void OpenWrite2_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenWrite ((string) null, "PUT");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // OpenWrite (string, string)
		[Category ("BitcodeNotSupported")]
		public void OpenWrite2_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenWrite ("tp://scheme.notsupported", "PUT");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // OpenWrite (Uri)
		[Category ("BitcodeNotSupported")]
		public void OpenWrite3_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenWrite ((Uri) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // OpenWrite (Uri)
		[Category ("BitcodeNotSupported")]
		public void OpenWrite3_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenWrite (new Uri ("tp://scheme.notsupported"));
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // OpenWrite (Uri, string)
		[Category ("BitcodeNotSupported")]
		public void OpenWrite4_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenWrite ((Uri) null, "POST");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // OpenWrite (Uri, string)
		[Category ("BitcodeNotSupported")]
		public void OpenWrite4_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenWrite (new Uri ("tp://scheme.notsupported"),
					"POST");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadData (string, byte [])
		public void UploadData1_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData ((string) null, new byte [] { 0x1a });
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadData (string, byte [])
		[Category ("BitcodeNotSupported")]
		public void UploadData1_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData ("tp://scheme.notsupported", new byte [] { 0x1a });
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadData (string, byte [])
		public void UploadData1_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData ("http://www.example.com",
					(byte []) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
		}

		[Test] // UploadData (Uri, byte [])
		public void UploadData2_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData ((Uri) null, new byte [] { 0x1a });
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadData (Uri, byte [])
		[Category ("BitcodeNotSupported")]
		public void UploadData2_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData (new Uri ("tp://scheme.notsupported"),
					new byte [] { 0x1a });
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadData (Uri, byte [])
		public void UploadData2_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData (new Uri ("http://www.example.com"),
					(byte []) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
		}

		[Test] // UploadData (string, string, byte [])
		public void UploadData3_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData ((string) null, "POST",
					new byte [] { 0x1a });
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadData (string, string, byte [])
		[Category ("BitcodeNotSupported")]
		public void UploadData3_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData ("tp://scheme.notsupported",
					"POST", new byte [] { 0x1a });
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadData (string, string, byte [])
		public void UploadData3_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData ("http://www.example.com",
					"POST", (byte []) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
		}

		[Test] // UploadData (Uri, string, byte [])
		public void UploadData4_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData ((Uri) null, "POST", new byte [] { 0x1a });
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadData (Uri, string, byte [])
		[Category ("BitcodeNotSupported")]
		public void UploadData4_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData (new Uri ("tp://scheme.notsupported"),
					"POST", new byte [] { 0x1a });
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadData (Uri, string, byte [])
		public void UploadData4_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData (new Uri ("http://www.example.com"),
					"POST", (byte []) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
		}

		[Test] // UploadFile (string, string)
		public void UploadFile1_Address_Null ()
		{
			string tempFile = Path.GetTempFileName ();

			WebClient wc = new WebClient ();
			try {
				wc.UploadFile ((string) null, tempFile);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			} finally {
				if (File.Exists (tempFile))
					File.Delete (tempFile);
			}
		}

		[Test] // UploadFile (string, string)
		[Category ("BitcodeNotSupported")]
		public void UploadFile1_Address_SchemeNotSupported ()
		{
			string tempFile = Path.GetTempFileName ();

			WebClient wc = new WebClient ();
			try {
				wc.UploadFile ("tp://scheme.notsupported",
					tempFile);
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			} finally {
				if (File.Exists (tempFile))
					File.Delete (tempFile);
			}
		}

		[Test] // UploadFile (string, string)
		[Category ("BitcodeNotSupported")]
		public void UploadFile1_FileName_NotFound ()
		{
			using (var tempPath = new TempDirectory ()) {
				string tempFile = Path.Combine (tempPath.Path, Path.GetRandomFileName ());

				WebClient wc = new WebClient ();
				try {
					wc.UploadFile ("tp://scheme.notsupported",
						tempFile);
					Assert.Fail ("#1");
				} catch (WebException ex) {
					// An error occurred performing a WebClient request
					Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
					Assert.IsNotNull (ex.Message, "#3");
					Assert.IsNull (ex.Response, "#4");
					Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

					// Could not find file "..."
					FileNotFoundException inner = ex.InnerException
						as FileNotFoundException;
					Assert.IsNotNull (inner, "#6");
					Assert.AreEqual (typeof (FileNotFoundException), inner.GetType (), "#7");
					Assert.IsNotNull (inner.FileName, "#8");
					Assert.AreEqual (tempFile, inner.FileName, "#9");
					Assert.IsNull (inner.InnerException, "#10");
					Assert.IsNotNull (inner.Message, "#11");
				}
			}
		}

		[Test] // UploadFile (string, string)
		public void UploadFile1_FileName_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadFile ("tp://scheme.notsupported",
					(string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("fileName", ex.ParamName, "#6");
			}
		}

		[Test] // UploadFile (Uri, string)
		public void UploadFile2_Address_Null ()
		{
			string tempFile = Path.GetRandomFileName ();

			WebClient wc = new WebClient ();
			try {
				wc.UploadFile ((Uri) null, tempFile);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadFile (Uri, string)
		[Category ("BitcodeNotSupported")]
		public void UploadFile2_Address_SchemeNotSupported ()
		{
			string tempFile = Path.GetTempFileName ();

			WebClient wc = new WebClient ();
			try {
				wc.UploadFile (new Uri ("tp://scheme.notsupported"),
					tempFile);
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			} finally {
				if (File.Exists (tempFile))
					File.Delete (tempFile);
			}
		}

		[Test] // UploadFile (Uri, string)
		[Category ("BitcodeNotSupported")]
		public void UploadFile2_FileName_NotFound ()
		{
			using (var tempPath = new TempDirectory ()) {
				string tempFile = Path.Combine (tempPath.Path, Path.GetRandomFileName ());

				WebClient wc = new WebClient ();
				try {
					wc.UploadFile (new Uri ("tp://scheme.notsupported"),
						tempFile);
					Assert.Fail ("#1");
				} catch (WebException ex) {
					// An error occurred performing a WebClient request
					Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
					Assert.IsNotNull (ex.Message, "#3");
					Assert.IsNull (ex.Response, "#4");
					Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

					// Could not find file "..."
					FileNotFoundException inner = ex.InnerException
						as FileNotFoundException;
					Assert.IsNotNull (inner, "#6");
					Assert.AreEqual (typeof (FileNotFoundException), inner.GetType (), "#7");
					Assert.IsNotNull (inner.FileName, "#8");
					Assert.AreEqual (tempFile, inner.FileName, "#9");
					Assert.IsNull (inner.InnerException, "#10");
					Assert.IsNotNull (inner.Message, "#11");
				}
			}
		}

		[Test] // UploadFile (Uri, string)
		public void UploadFile2_FileName_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadFile (new Uri ("tp://scheme.notsupported"),
					null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("fileName", ex.ParamName, "#6");
			}
		}

		[Test] // UploadFile (string, string, string)
		public void UploadFile3_Address_Null ()
		{
			string tempFile = Path.GetRandomFileName ();

			WebClient wc = new WebClient ();
			try {
				wc.UploadFile ((string) null, "POST", tempFile);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadFile (string, string, string)
		[Category ("BitcodeNotSupported")]
		public void UploadFile3_Address_SchemeNotSupported ()
		{
			string tempFile = Path.GetTempFileName ();

			WebClient wc = new WebClient ();
			try {
				wc.UploadFile ("tp://scheme.notsupported",
					"POST", tempFile);
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			} finally {
				if (File.Exists (tempFile))
					File.Delete (tempFile);
			}
		}

		[Test] // UploadFile (string, string, string)
		[Category ("BitcodeNotSupported")]
		public void UploadFile3_FileName_NotFound ()
		{
			using (var tempPath = new TempDirectory ()) {
				string tempFile = Path.Combine (tempPath.Path, Path.GetRandomFileName ());

				WebClient wc = new WebClient ();
				try {
					wc.UploadFile ("tp://scheme.notsupported",
						"POST", tempFile);
					Assert.Fail ("#1");
				} catch (WebException ex) {
					// An error occurred performing a WebClient request
					Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
					Assert.IsNotNull (ex.Message, "#3");
					Assert.IsNull (ex.Response, "#4");
					Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

					// Could not find file "..."
					FileNotFoundException inner = ex.InnerException
						as FileNotFoundException;
					Assert.IsNotNull (inner, "#6");
					Assert.AreEqual (typeof (FileNotFoundException), inner.GetType (), "#7");
					Assert.IsNotNull (inner.FileName, "#8");
					Assert.AreEqual (tempFile, inner.FileName, "#9");
					Assert.IsNull (inner.InnerException, "#10");
					Assert.IsNotNull (inner.Message, "#11");
				}
			}
		}

		[Test] // UploadFile (string, string, string)
		public void UploadFile3_FileName_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadFile ("tp://scheme.notsupported",
					"POST", (string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("fileName", ex.ParamName, "#6");
			}
		}

		[Test] // UploadFile (Uri, string, string)
		public void UploadFile4_Address_Null ()
		{
			string tempFile = Path.GetRandomFileName ();

			WebClient wc = new WebClient ();
			try {
				wc.UploadFile ((Uri) null, "POST", tempFile);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadFile (Uri, string, string)
		[Category ("BitcodeNotSupported")]
		public void UploadFile4_Address_SchemeNotSupported ()
		{
			string tempFile = Path.GetTempFileName ();

			WebClient wc = new WebClient ();
			try {
				wc.UploadFile (new Uri ("tp://scheme.notsupported"),
					"POST", tempFile);
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			} finally {
				if (File.Exists (tempFile))
					File.Delete (tempFile);
			}
		}

		[Test] // UploadFile (Uri, string, string)
		[Category ("BitcodeNotSupported")]
		public void UploadFile4_FileName_NotFound ()
		{
			using (var tempPath = new TempDirectory ()) {
				string tempFile = Path.Combine (tempPath.Path, Path.GetRandomFileName ());

				WebClient wc = new WebClient ();
				try {
					wc.UploadFile (new Uri ("tp://scheme.notsupported"),
						"POST", tempFile);
					Assert.Fail ("#1");
				} catch (WebException ex) {
					// An error occurred performing a WebClient request
					Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
					Assert.IsNotNull (ex.Message, "#3");
					Assert.IsNull (ex.Response, "#4");
					Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

					// Could not find file "..."
					FileNotFoundException inner = ex.InnerException
						as FileNotFoundException;
					Assert.IsNotNull (inner, "#6");
					Assert.AreEqual (typeof (FileNotFoundException), inner.GetType (), "#7");
					Assert.IsNotNull (inner.FileName, "#8");
					Assert.AreEqual (tempFile, inner.FileName, "#9");
					Assert.IsNull (inner.InnerException, "#10");
					Assert.IsNotNull (inner.Message, "#11");
				}
			}
		}

		[Test] // UploadFile (Uri, string, string)
		public void UploadFile4_FileName_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadFile (new Uri ("tp://scheme.notsupported"),
					"POST", (string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("fileName", ex.ParamName, "#6");
			}
		}

		[Test] // UploadString (string, string)
		public void UploadString1_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString ((string) null, (string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadString (string, string)
		[Category ("BitcodeNotSupported")]
		public void UploadString1_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString ("tp://scheme.notsupported", "abc");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadString (string, string)
		public void UploadString1_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString ("tp://scheme.notsupported", (string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
		}

		[Test] // UploadString (Uri, string)
		public void UploadString2_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString ((Uri) null, (string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadString (Uri, string)
		[Category ("BitcodeNotSupported")]
		public void UploadString2_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString (new Uri ("tp://scheme.notsupported"), "abc");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadString (Uri, string)
		public void UploadString2_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString (new Uri ("tp://scheme.notsupported"),
					(string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
		}

		[Test] // UploadString (string, string, string)
		public void UploadString3_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString ((string) null, (string) null,
					(string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadString (string, string, string)
		[Category ("BitcodeNotSupported")]
		public void UploadString3_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString ("tp://scheme.notsupported",
					"POST", "abc");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadString (string, string, string)
		public void UploadString3_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString ("tp://scheme.notsupported",
					"POST", (string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
		}

		[Test] // UploadString (Uri, string, string)
		public void UploadString4_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString ((Uri) null, (string) null,
					(string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadString (Uri, string, string)
		[Category ("BitcodeNotSupported")]
		public void UploadString4_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString (new Uri ("tp://scheme.notsupported"),
					"POST", "abc");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadString (Uri, string, string)
		public void UploadString4_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString (new Uri ("tp://scheme.notsupported"),
					"POST", (string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
		}

		[Test]
		[Category ("MobileNotWorking")] // Fails when ran as part of the entire BCL test suite. Works when only this fixture is ran
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void UploadValues1 ()
		{
			using (SocketResponder responder = new SocketResponder (out var ep, s => EchoRequestHandler (s))) {
				string url = "http://" + ep.ToString () + "/test/";
				WebClient wc = new WebClient ();
				wc.Encoding = Encoding.ASCII;

				NameValueCollection nvc = new NameValueCollection ();
				nvc.Add ("Name", "\u0041\u2262\u0391\u002E");
				nvc.Add ("Address", "\u002E\u2262\u0041\u0391");

				byte [] buffer = wc.UploadValues (url, nvc);
				string response = Encoding.UTF8.GetString (buffer);
				Assert.AreEqual ("Name=A%e2%89%a2%ce%91.&Address=.%e2%89%a2A%ce%91\r\n", response);
			}
		}

		[Test] // UploadValues (string, NameValueCollection)
		public void UploadValues1_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues ((string) null, new NameValueCollection ());
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadValues (string, NameValueCollection)
		[Category ("BitcodeNotSupported")]
		public void UploadValues1_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues ("tp://scheme.notsupported",
					new NameValueCollection ());
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadValues (string, NameValueCollection)
		[Category ("BitcodeNotSupported")]
		public void UploadValues1_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues ("http://www.example.com",
					(NameValueCollection) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
		}

		[Test] // UploadValues (Uri, NameValueCollection)
		[Category ("BitcodeNotSupported")]
		public void UploadValues2_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues ((Uri) null, new NameValueCollection ());
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadValues (Uri, NameValueCollection)
		[Category ("BitcodeNotSupported")]
		public void UploadValues2_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues (new Uri ("tp://scheme.notsupported"),
					new NameValueCollection ());
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadValues (Uri, NameValueCollection)
		[Category ("BitcodeNotSupported")]
		public void UploadValues2_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues (new Uri ("http://www.example.com"),
					(NameValueCollection) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
		}

		[Test] // UploadValues (string, string, NameValueCollection)
		public void UploadValues3_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues ((string) null, "POST",
					new NameValueCollection ());
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadValues (string, string, NameValueCollection)
		[Category ("BitcodeNotSupported")]
		public void UploadValues3_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues ("tp://scheme.notsupported",
					"POST", new NameValueCollection ());
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadValues (string, string, NameValueCollection)
		[Category ("BitcodeNotSupported")]
		public void UploadValues3_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues ("http://www.example.com",
					"POST", (NameValueCollection) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
		}

		[Test] // UploadValues (Uri, string, NameValueCollection)
		[Category ("BitcodeNotSupported")]
		public void UploadValues4_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues ((Uri) null, "POST",
					new NameValueCollection ());
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadValues (Uri, string, NameValueCollection)
		[Category ("BitcodeNotSupported")]
		public void UploadValues4_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues (new Uri ("tp://scheme.notsupported"),
					"POST", new NameValueCollection ());
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadValues (Uri, string, NameValueCollection)
		[Category ("BitcodeNotSupported")]
		public void UploadValues4_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues (new Uri ("http://www.example.com"),
					"POST", (NameValueCollection) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		// We throw a PlatformNotSupportedException deeper, which is caught and re-thrown as WebException
		[ExpectedException (typeof (WebException))]
#endif
		[Category ("MobileNotWorking")] // https://github.com/xamarin/xamarin-macios/issues/3827
		[Category ("InetAccess")]
		public void GetWebRequestOverriding ()
		{
			GetWebRequestOverridingTestClass testObject = new GetWebRequestOverridingTestClass ();
			testObject.DownloadData ("http://www.example.com");

			Assert.IsTrue (testObject.overridedCodeRan, "Overrided code wasn't called");
		}
		
		class GetWebRequestOverridingTestClass : WebClient
		{
			internal bool overridedCodeRan = false;
			protected override WebRequest GetWebRequest(Uri address)
			{
				overridedCodeRan = true;
				return base.GetWebRequest (address);
			}
		}

		static byte [] EchoRequestHandler (Socket socket)
		{
			MemoryStream ms = new MemoryStream ();
			byte [] buffer = new byte [4096];
			int bytesReceived = socket.Receive (buffer);
			while (bytesReceived > 0) {
				ms.Write (buffer, 0, bytesReceived);
				 // We don't check for Content-Length or anything else here, so we give the client a little time to write
				 // after sending the headers
				Thread.Sleep (200);
				if (socket.Available > 0) {
					bytesReceived = socket.Receive (buffer);
				} else {
					bytesReceived = 0;
				}
			}
			ms.Flush ();
			ms.Position = 0;

			StringBuilder sb = new StringBuilder ();

			string expect = null;

			StreamReader sr = new StreamReader (ms, Encoding.UTF8);
			string line = null;
			byte state = 0;
			while ((line = sr.ReadLine ()) != null) {
				if (state > 0) {
					state = 2;
					sb.Append (line);
					sb.Append ("\r\n");
				} if (line.Length == 0) {
					state = 1;
				} else if (line.StartsWith ("Expect:")) {
					expect = line.Substring (8);
				}
			}

			StringWriter sw = new StringWriter ();

			if (expect == "100-continue" && state != 2) {
				sw.WriteLine ("HTTP/1.1 100 Continue");
				sw.WriteLine ();
				sw.Flush ();

				socket.Send (Encoding.UTF8.GetBytes (sw.ToString ()));

				// receive body
				ms = new MemoryStream ();
				buffer = new byte [4096];
				bytesReceived = socket.Receive (buffer);
				while (bytesReceived > 0) {
					ms.Write (buffer, 0, bytesReceived);
					Thread.Sleep (200);
					if (socket.Available > 0) {
						bytesReceived = socket.Receive (buffer);
					} else {
						bytesReceived = 0;
					}
				}
				ms.Flush ();
				ms.Position = 0;

				sb = new StringBuilder ();
				sr = new StreamReader (ms, Encoding.UTF8);
				line = sr.ReadLine ();
				while (line != null) {
					sb.Append (line);
					sb.Append ("\r\n");
					line = sr.ReadLine ();
				}
			}

			sw = new StringWriter ();
			sw.WriteLine ("HTTP/1.1 200 OK");
			sw.WriteLine ("Content-Type: text/xml");
			sw.WriteLine ("Content-Length: " + sb.Length.ToString (CultureInfo.InvariantCulture));
			sw.WriteLine ();
			sw.Write (sb.ToString ());
			sw.Flush ();

			return Encoding.UTF8.GetBytes (sw.ToString ());
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void DefaultProxy ()
		{
			WebClient wc = new WebClient ();
			// this is never null on .net
			Assert.IsNotNull (wc.Proxy);
			// and return the same instance as WebRequest.DefaultWebProxy
			Assert.AreSame (wc.Proxy, WebRequest.DefaultWebProxy);
		}
		 
		[Test]
		[Category ("MobileNotWorking")] // Fails when ran as part of the entire BCL test suite. Works when only this fixture is ran
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void UploadStringAsyncCancelEvent ()
		{
			UploadAsyncCancelEventTest (9301, (webClient, uri, cancelEvent) =>
			{

				webClient.UploadStringCompleted += (sender, args) =>
				{
					if (args.Cancelled)
						cancelEvent.Set ();
				};

				webClient.UploadStringAsync (uri, "PUT", "text");
			});
		}

		[Test]
		[Category ("MobileNotWorking")] // Fails when ran as part of the entire BCL test suite. Works when only this fixture is ran
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void UploadDataAsyncCancelEvent ()
		{
			UploadAsyncCancelEventTest (9302, (webClient, uri, cancelEvent) =>
			{
				webClient.UploadDataCompleted += (sender, args) =>
				{
					if (args.Cancelled)
						cancelEvent.Set ();
				};

				webClient.UploadDataAsync (uri, "PUT", new byte[] { });
			});
		}
		
		[Test]
		[Category ("MobileNotWorking")] // Fails when ran as part of the entire BCL test suite. Works when only this fixture is ran
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void UploadValuesAsyncCancelEvent ()
		{
			UploadAsyncCancelEventTest (9303, (webClient, uri, cancelEvent) =>
			{
				webClient.UploadValuesCompleted += (sender, args) =>
				{
					if (args.Cancelled)
						cancelEvent.Set ();
				};

				webClient.UploadValuesAsync (uri, "PUT", new NameValueCollection ());
			});
		}

		[Test]
		[Category ("MobileNotWorking")] // Fails when ran as part of the entire BCL test suite. Works when only this fixture is ran
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void UploadFileAsyncCancelEvent ()
		{
			UploadAsyncCancelEventTest (9304,(webClient, uri, cancelEvent) =>
			{
				string tempFile = Path.GetTempFileName ();

				webClient.UploadFileCompleted += (sender, args) =>
				{
					if (args.Cancelled)
						cancelEvent.Set ();
				};

				webClient.UploadFileAsync (uri, "PUT", tempFile);
			});
		}

		[Test]
		[Category ("MobileNotWorking")] // Test suite hangs if the tests runs as part of the entire BCL suite. Works when only this fixture is ran
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void UploadFileAsyncContentType ()
		{
			var filename = Path.GetTempFileName ();

			HttpListener listener = NetworkHelpers.CreateAndStartHttpListener ("http://localhost:", out var port, "/", out var serverUri);

			using (var client = new WebClient ())
			{
				client.UploadFileTaskAsync (new Uri (serverUri), filename);
				var request = listener.GetContext ().Request;

				var expected = "multipart/form-data; boundary=---------------------";
				Assert.AreEqual (expected.Length + 15, request.ContentType.Length);
				Assert.AreEqual (expected, request.ContentType.Substring (0, expected.Length));
			}
			listener.Close ();
		}

		public void UploadAsyncCancelEventTest (int port, Action<WebClient, Uri, EventWaitHandle> uploadAction)
		{
			using (var responder = new SocketResponder (out var ep, s => EchoRequestHandler (s)))
			{
				string url = "http://" + ep.ToString() + "/test/";
				var webClient = new WebClient ();

				var cancellationTokenSource = new CancellationTokenSource ();
				cancellationTokenSource.Token.Register (webClient.CancelAsync);

				var cancelEvent = new ManualResetEvent (false);

				uploadAction.Invoke (webClient, new Uri (url), cancelEvent);

				cancellationTokenSource.Cancel ();

				Assert.IsTrue (cancelEvent.WaitOne (1000));
			}
		}
	}
}
