//
// FileWebRequestTest.cs - NUnit Test Cases for System.Net.FileWebRequest
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Permissions;

using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Net
{
	[TestFixture]
	[Category("NotWasm")]
	public class FileWebRequestTest
	{
		private TempDirectory _tempDirectory;
		private string _tempFile;
		private Uri _tempFileUri;

		[SetUp]
		public void SetUp ()
		{
			_tempDirectory = new TempDirectory ();
			_tempFile = Path.Combine (_tempDirectory.Path, "FileWebRequestTest.tmp");
			_tempFileUri = GetTempFileUri ();
		}

		[TearDown]
		public void TearDown ()
		{
			_tempDirectory.Dispose ();
		}

		[Test]
		[Category("MultiThreaded")]
		public void Async ()
		{
			WebResponse res = null;

			try {
				FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
				req.Method = "PUT";
				req.ContentLength = 1;
				req.ContentType = "image/png";

				req.Timeout = 2 * 1000;
				IAsyncResult async = req.BeginGetRequestStream (null, null);
				try {
					req.BeginGetRequestStream (null, null);
					Assert.Fail ("#1 should've failed");
				} catch (InvalidOperationException) {
					// Cannot re-call BeginGetRequestStream/BeginGetResponse while
					// a previous call is still in progress
				}

				try {
					req.GetRequestStream ();
					Assert.Fail ("#3 should've failed");
				} catch (InvalidOperationException) {
					// Cannot re-call BeginGetRequestStream/BeginGetResponse while
					// a previous call is still in progress
				}

				using (Stream wstream = req.EndGetRequestStream (async)) {
					Assert.IsFalse (wstream.CanRead, "#1r");
					Assert.IsTrue (wstream.CanWrite, "#1w");
					Assert.IsTrue (wstream.CanSeek, "#1s");

					wstream.WriteByte (72);
					wstream.WriteByte (101);
					wstream.WriteByte (108);
					wstream.WriteByte (108);
					wstream.WriteByte (111);
					wstream.Close ();
				}

				Assert.AreEqual (1, req.ContentLength, "#1cl");
				Assert.AreEqual ("image/png", req.ContentType, "#1ct");

				// stream written

				req = (FileWebRequest) WebRequest.Create (_tempFileUri);
				res = req.GetResponse ();

				try {
					req.BeginGetRequestStream (null, null);
					Assert.Fail ("#20: should've failed");
				} catch (InvalidOperationException) {
					// Cannot send a content-body with this verb-type
				}

				try {
					req.Method = "PUT";
					req.BeginGetRequestStream (null, null);
					Assert.Fail ("#21: should've failed");
				} catch (InvalidOperationException) {
					// This operation cannot be perfomed after the request has been submitted.
				}

				req.GetResponse ();

				IAsyncResult async2 = req.BeginGetResponse (null, null);

				// this succeeds !!
				WebResponse res2 = req.EndGetResponse (async2);
				Assert.AreSame (res, res2, "#23");

				Assert.AreEqual (5, res.ContentLength, "#2 len");
				Assert.AreEqual ("application/octet-stream", res.ContentType, "#2 type");
				Assert.AreEqual ("file", res.ResponseUri.Scheme, "#2 scheme");

				Stream rstream = res.GetResponseStream ();
				Assert.IsTrue (rstream.CanRead, "#3r");
				Assert.IsFalse (rstream.CanWrite, "#3w");
				Assert.IsTrue (rstream.CanSeek, "#3s");

				Assert.AreEqual (72, rstream.ReadByte (), "#4a");
				Assert.AreEqual (101, rstream.ReadByte (), "#4b");
				Assert.AreEqual (108, rstream.ReadByte (), "#4c");
				Assert.AreEqual (108, rstream.ReadByte (), "#4d");
				Assert.AreEqual (111, rstream.ReadByte (), "#4e");

				rstream.Close ();

				try {
					long len = res.ContentLength;
					Assert.AreEqual ((long) 5, len, "#5");
				} catch (ObjectDisposedException) {
					Assert.Fail ("#disposed contentlength");
				}
				try {
					WebHeaderCollection w = res.Headers;
				} catch (ObjectDisposedException) {
					Assert.Fail ("#disposed headers");
				}
				try {
					res.Close ();
				} catch (ObjectDisposedException) {
					Assert.Fail ("#disposed close");
				}
			} finally {
				if (res != null)
					res.Close ();
			}
		}

		[Test]
		[Category ("NotWorking")] // bug #323388
		public void Async_GetResponse_Failure ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			req.Method = "PUT";
			req.ContentLength = 1;
			req.ContentType = "image/png";
			req.Timeout = 500;

			IAsyncResult async = req.BeginGetRequestStream (null, null);
			try {
				req.GetResponse ();
				Assert.Fail ("#1");
			} catch (WebException) {
				// The operation has timed out
			}

			try {
				req.BeginGetResponse (null, null);
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
				// Cannot re-call BeginGetRequestStream/BeginGetResponse while
				// a previous call is still in progress
			}

			using (Stream wstream = req.EndGetRequestStream (async)) {
				wstream.WriteByte (72);
			}

			// the temp file should not be in use
			_tempDirectory.Dispose ();
		}

		[Test]
		public void Sync ()
		{
			WebResponse res = null;

			try {
				FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
				req.ContentLength = 1;
				req.ContentType = "image/png";

				try {
					Stream stream = req.GetRequestStream ();
					Assert.Fail ("should throw exception");
				} catch (ProtocolViolationException) {
				}

				req.Method = "PUT";

				Stream wstream = req.GetRequestStream ();
				Assert.IsFalse (wstream.CanRead, "#1r");
				Assert.IsTrue (wstream.CanWrite, "#1w");
				Assert.IsTrue (wstream.CanSeek, "#1s");

				wstream.WriteByte (72);
				wstream.WriteByte (101);
				wstream.WriteByte (108);
				wstream.WriteByte (108);
				wstream.WriteByte (111);
				wstream.Close ();

				Assert.AreEqual (1, req.ContentLength, "#1cl");
				Assert.AreEqual ("image/png", req.ContentType, "#1ct");

				// stream written

				req = (FileWebRequest) WebRequest.Create (_tempFileUri);
				res = req.GetResponse ();
				Assert.AreEqual ((long) 5, res.ContentLength, "#2 len");
				Assert.AreEqual ("application/octet-stream", res.ContentType, "#2 type");
				Assert.AreEqual ("file", res.ResponseUri.Scheme, "#2 scheme");

				Stream rstream = res.GetResponseStream ();
				Assert.IsTrue (rstream.CanRead, "#3r");
				Assert.IsFalse (rstream.CanWrite, "#3w");
				Assert.IsTrue (rstream.CanSeek, "#3s");

				Assert.AreEqual (72, rstream.ReadByte (), "#4a");
				Assert.AreEqual (101, rstream.ReadByte (), "#4b");
				Assert.AreEqual (108, rstream.ReadByte (), "#4c");
				Assert.AreEqual (108, rstream.ReadByte (), "#4d");
				Assert.AreEqual (111, rstream.ReadByte (), "#4e");

				rstream.Close ();

				try {
					long len = res.ContentLength;
					Assert.AreEqual ((long) 5, len, "#5");
				} catch (ObjectDisposedException) {
					Assert.Fail ("#disposed contentlength");
				}
				try {
					WebHeaderCollection w = res.Headers;
				} catch (ObjectDisposedException) {
					Assert.Fail ("#disposed headers");
				}
				try {
					res.Close ();
				} catch (ObjectDisposedException) {
					Assert.Fail ("#disposed close");
				}
			} finally {
				if (res != null)
					res.Close ();
			}
		}

		[Test]
		[Category ("NotWorking")] // bug #323388
		public void Sync_GetResponse_Failure ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			req.Method = "PUT";
			req.ContentLength = 1;
			req.ContentType = "image/png";
			req.Timeout = 500;

			using (Stream rs = req.GetRequestStream ()) {
				try {
					req.GetResponse ();
					Assert.Fail ("#1");
				} catch (WebException) {
					// The operation has timed out
				}

				try {
					req.BeginGetResponse (null, null);
					Assert.Fail ("#2");
				} catch (InvalidOperationException) {
					// Cannot re-call BeginGetRequestStream/BeginGetResponse while
					// a previous call is still in progress
				}
			}

			// the temp file should not be in use
			_tempDirectory.Dispose ();
		}

		[Test]
		public void ConnectionGroupName ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			Assert.IsNull (req.ConnectionGroupName, "#A");
			req.ConnectionGroupName = "whatever";
			Assert.IsNotNull (req.ConnectionGroupName, "#B1");
			Assert.AreEqual ("whatever", req.ConnectionGroupName, "#B2");
			req.ConnectionGroupName = string.Empty;
			Assert.IsNotNull (req.ConnectionGroupName, "#C1");
			Assert.AreEqual (string.Empty, req.ConnectionGroupName, "#C2");
			req.ConnectionGroupName = null;
			Assert.IsNull (req.ConnectionGroupName, "#D");
		}

		[Test]
		public void ContentLength ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			Assert.AreEqual (0, req.Headers.Count, "#A1");
			Assert.AreEqual (0, req.ContentLength, "#A2");
			req.ContentLength = 5;
			Assert.AreEqual (5, req.ContentLength, "#A3");
			Assert.AreEqual (0, req.Headers.Count, "#A4");

			req.Method = "PUT";
			using (Stream s = req.GetRequestStream ()) {
				s.WriteByte (5);
				Assert.AreEqual (5, req.ContentLength, "#B1");
				s.WriteByte (4);
				Assert.AreEqual (5, req.ContentLength, "#B2");
				s.Flush ();
				Assert.AreEqual (5, req.ContentLength, "#B3");
			}
			Assert.AreEqual (5, req.ContentLength, "#B4");
		}

		[Test]
		public void ContentLength_Negative ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			try {
				req.ContentLength = -1;
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsFalse (ex.Message == "value", "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("value", ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void ContentType ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			Assert.AreEqual (0, req.Headers.Count, "#A1");
			Assert.IsNull (req.ContentType, "#A2");

			req.ContentType = "application/x-gzip";
			Assert.AreEqual (1, req.Headers.Count, "#B1");
			Assert.AreEqual ("Content-Type", req.Headers.GetKey (0), "#B2");
			Assert.AreEqual ("application/x-gzip", req.Headers.Get (0), "#B3");
			Assert.AreEqual ("application/x-gzip", req.ContentType, "#B4");

			req.Headers.Set ("Content-Type", "image/png");
			Assert.AreEqual ("image/png", req.ContentType, "#C1");

			req.ContentType = null;
			Assert.AreEqual (1, req.Headers.Count, "#D1");
			Assert.AreEqual ("Content-Type", req.Headers.GetKey (0), "#D2");
			Assert.AreEqual (string.Empty, req.Headers.Get (0), "#D3");
			Assert.AreEqual (string.Empty, req.ContentType, "#D4");

			req.Headers.Remove ("Content-Type");
			Assert.AreEqual (0, req.Headers.Count, "#E1");
			Assert.IsNull (req.ContentType, "#E2");
		}

		[Test]
		public void Credentials ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			Assert.IsNull (req.Credentials, "#1");
			req.Credentials = new NetworkCredential ();
			Assert.IsNotNull (req.Credentials, "#2");
			req.Credentials = null;
			Assert.IsNull (req.Credentials, "#3");
		}

		[Test]
		[Category ("NotWorking")]
		public void GetRequestStream ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (
				_tempFileUri);
			req.Timeout = 1000;
			req.Method = "POST";
			FileStream fsA = null;
			FileStream fsB = null;
			try {
				fsA = req.GetRequestStream () as FileStream;
				Assert.IsNotNull (fsA, "#A1");
				try {
					req.GetRequestStream ();
					Assert.Fail ("#A2");
				} catch (WebException) {
					// The operation has timed out
				}
				fsA.Close ();
				try {
					req.GetRequestStream ();
					Assert.Fail ("#A3");
				} catch (InvalidOperationException) {
					// Cannot re-call BeginGetRequestStream/BeginGetResponse 
					// while a previous call is still in progress.
				}
			} finally {
				if (fsA != null)
					fsA.Close ();
				if (fsB != null)
					fsB.Close ();
			}

			req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			req.Timeout = 1000;
			req.Method = "POST";
			try {
				fsA = req.GetRequestStream () as FileStream;
				Assert.IsNotNull (fsA, "#B1");
				fsA.Close ();
				try {
					req.GetRequestStream ();
					Assert.Fail ("#B2");
				} catch (WebException) {
					// The operation has timed out
				}
				fsA.Close ();
				try {
					req.GetRequestStream ();
					Assert.Fail ("#B3");
				} catch (InvalidOperationException) {
					// Cannot re-call BeginGetRequestStream/BeginGetResponse 
					// while a previous call is still in progress.
				}
			} finally {
				if (fsA != null)
					fsA.Close ();
				if (fsB != null)
					fsB.Close ();
			}
		}

		[Test]
		[Category("MultiThreaded")]
		public void GetRequestStream_File_Exists ()
		{
			Stream s = File.Create (_tempFile);
			s.Close ();
			FileWebRequest req = (FileWebRequest) WebRequest.Create (
				_tempFileUri);
			req.Method = "POST";
			s = req.GetRequestStream ();
			s.Close ();
		}

		[Test]
		public void GetRequestStream_Method_Valid ()
		{
			string [] methods = new string [] { "PUT", "POST", "CHECKOUT",
				"DELETE", "OPTIONS", "TRACE", "GET ", "DUNNO" };

			foreach (string method in methods) {
				FileWebRequest req = (FileWebRequest) WebRequest.Create (
					_tempFileUri);
				req.Method = method;
				using (Stream s = req.GetRequestStream ()) {
					Assert.IsNotNull (s, "#1:" + method);
					Assert.IsFalse (s.CanRead, "#2:" + method);
					Assert.IsTrue (s.CanSeek, "#3:" + method);
					Assert.IsFalse (s.CanTimeout, "#4:" + method);
					Assert.IsTrue (s.CanWrite, "#5:" + method);
					Assert.AreEqual (0, s.Length, "#6:" + method);
					Assert.AreEqual (0, s.Position, "#7:" + method);
					try {
						int i = s.ReadTimeout;
						Assert.Fail ("#8:" + method + "=>" + i);
					} catch (InvalidOperationException) {
					}
					try {
						int i = s.WriteTimeout;
						Assert.Fail ("#9:" + method + "=>" + i);
					} catch (InvalidOperationException) {
					}
				}
			}
		}

		[Test]
		public void GetRequestStream_Method_Invalid ()
		{
			string [] methods = new string [] { "GET", "get", "HEAD", "head",
				"CONNECT", "connect"};
			foreach (string method in methods) {
				FileWebRequest req = (FileWebRequest) WebRequest.Create (
					_tempFileUri);
				req.Method = method;
				try {
					req.GetRequestStream ();
					Assert.Fail ("#1:" + method);
				} catch (ProtocolViolationException ex) {
					Assert.AreEqual (typeof (ProtocolViolationException), ex.GetType (), "#2:" + method);
					Assert.IsNotNull (ex.Message, "#3:" + method);
					Assert.IsNull (ex.InnerException, "#4:" + method);
				}
			}
		}

		[Test]
		public void GetResponse_File_Exists ()
		{
			Stream s = File.Create (_tempFile);
			s.Close ();
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			FileWebResponse respA = null;
			FileWebResponse respB = null;
			try {
				respA = req.GetResponse () as FileWebResponse;
				Assert.IsNotNull (respA, "#1");
				respB = req.GetResponse () as FileWebResponse;
				Assert.IsNotNull (respB, "#2");
				Assert.AreSame (respA, respB, "#3");
			} finally {
				if (respA != null)
					respA.Close ();
				if (respB != null)
					respB.Close ();
			}
		}

		[Test]
		public void GetResponse_File_DoesNotExist ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			try {
				req.GetResponse ();
				Assert.Fail ("#1");
			} catch (WebException ex) {
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#1");
				Assert.IsNotNull (ex.Message, "#2");
				Assert.IsTrue (ex.Message.IndexOf ("FileWebRequestTest.tmp") != -1, "#3");				
				Assert.IsNull (ex.Response, "#4");
				Assert.IsNotNull (ex.InnerException, "#5");

			}
		}

		[Test]
		public void Method ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			Assert.IsNotNull (req.Method, "#A1");
			Assert.AreEqual ("GET", req.Method, "#A2");
			req.Method = "whatever";
			Assert.IsNotNull (req.Method, "#B1");
			Assert.AreEqual ("whatever", req.Method, "#B2");
			req.Method = "get ";
			Assert.IsNotNull (req.Method, "#C1");
			Assert.AreEqual ("get ", req.Method, "#C2");
		}

		[Test]
		public void Method_Empty ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			try {
				req.Method = string.Empty;
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.AreEqual ("value", ex.ParamName, "#4");
				Assert.IsNull (ex.InnerException, "#5");
			}
		}

		[Test]
		public void Method_Null ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			try {
				req.Method = null;
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.AreEqual ("value", ex.ParamName, "#4");
				Assert.IsNull (ex.InnerException, "#5");
			}
		}

		[Test]
		public void PreAuthenticate ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			Assert.IsFalse (req.PreAuthenticate, "#1");
			req.PreAuthenticate = true;
			Assert.IsTrue (req.PreAuthenticate, "#2");
		}

		[Test]
		public void Proxy ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			Assert.IsNull (req.Proxy, "#1");
			req.Proxy = new WebProxy ();
			Assert.IsNotNull (req.Proxy, "#2");
			req.Proxy = null;
			Assert.IsNull (req.Proxy, "#3");
		}

		[Test]
		public void RequestUri ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			Assert.AreSame (_tempFileUri, req.RequestUri);
		}

		[Test]
		public void Timeout ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			Assert.AreEqual (100000, req.Timeout, "#1");
			req.Timeout = int.MaxValue;
			Assert.AreEqual (int.MaxValue, req.Timeout, "#2");
			req.Timeout = 0;
			Assert.AreEqual (0, req.Timeout, "#3");
		}

		[Test]
		public void Timeout_Negative ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			req.Timeout = -1;
			Assert.AreEqual (-1, req.Timeout, "#1");
			try {
				req.Timeout = -2;
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("value", ex.ParamName, "#6");
				Assert.IsNull (ex.InnerException, "#7");
			}
		}

		[Test]
		public void GetObjectData ()
		{
			FileWebRequest fwr = (FileWebRequest) WebRequest.Create ("file:///test.txt");
			fwr.ConnectionGroupName = "CGN";
			fwr.ContentLength = 10;
			fwr.ContentType = "image/png";
			fwr.Credentials = new NetworkCredential ("Miguel", "de Icaza", "Novell");
			fwr.Headers.Add ("Disposition", "attach");
			fwr.Method = "PUT";
			fwr.PreAuthenticate = true;
			fwr.Proxy = new WebProxy ("proxy.ximian.com");
			fwr.Timeout = 20;

			SerializationInfo si = new SerializationInfo (typeof (FileWebRequest),
				new FormatterConverter ());
			((ISerializable) fwr).GetObjectData (si, new StreamingContext ());
			Assert.AreEqual (9, si.MemberCount, "#A1");
			int i = 0;
			foreach (SerializationEntry entry in si) {
				Assert.IsNotNull (entry.Name, "#B1:" + i);
				Assert.IsNotNull (entry.ObjectType, "#B2:" + i);
				Assert.IsNotNull (entry.Value, "#B3:" + i);

				switch (i) {
				case 0:
					Assert.AreEqual ("headers", entry.Name, "#B4:" + i);
					Assert.AreEqual (typeof (WebHeaderCollection), entry.ObjectType, "#B5:" + i);
					break;
				case 1:
					Assert.AreEqual ("proxy", entry.Name, "#B4:" + i);
					Assert.AreEqual (typeof (IWebProxy), entry.ObjectType, "#B5:" + i);
					break;
				case 2:
					Assert.AreEqual ("uri", entry.Name, "#B4:" + i);
					Assert.AreEqual (typeof (Uri), entry.ObjectType, "#B5:" + i);
					break;
				case 3:
					Assert.AreEqual ("connectionGroupName", entry.Name, "#B4:" + i);
					Assert.AreEqual (typeof (string), entry.ObjectType, "#B5:" + i);
					Assert.AreEqual ("CGN", entry.Value, "#B6:" + i);
					break;
				case 4:
					Assert.AreEqual ("method", entry.Name, "#B4:" + i);
					Assert.AreEqual (typeof (string), entry.ObjectType, "#B5:" + i);
					Assert.AreEqual ("PUT", entry.Value, "#B6:" + i);
					break;
				case 5:
					Assert.AreEqual ("contentLength", entry.Name, "#B4:" + i);
					Assert.AreEqual (typeof (long), entry.ObjectType, "#B5:" + i);
					Assert.AreEqual (10, entry.Value, "#B6:" + i);
					break;
				case 6:
					Assert.AreEqual ("timeout", entry.Name, "#B4:" + i);
					Assert.AreEqual (typeof (int), entry.ObjectType, "#B5:" + i);
					Assert.AreEqual (20, entry.Value, "#B6:" + i);
					break;
				case 7:
					Assert.AreEqual ("fileAccess", entry.Name, "#B4:" + i);
					Assert.AreEqual (typeof (FileAccess), entry.ObjectType, "#B5:" + i);
					Assert.AreEqual (FileAccess.Read, entry.Value, "#B6:" + i);
					break;
				case 8:
					Assert.AreEqual ("preauthenticate", entry.Name, "#B4:" + i);
					Assert.AreEqual (typeof (bool), entry.ObjectType, "#B5:" + i);
					Assert.AreEqual (false, entry.Value, "#B6:" + i);
					break;
				}
				i++;
			}
		}

		[Test]
		[Category ("NotWorking")] // Difference at index 272: 20 instead of 19
		public void Serialize ()
		{
			FileWebRequest fwr = (FileWebRequest) WebRequest.Create ("file://test.txt/");
			fwr.ConnectionGroupName = "CGN";
			fwr.ContentLength = 10;
			fwr.ContentType = "image/png";
			fwr.Credentials = new NetworkCredential ("Miguel", "de Icaza", "Novell");
			fwr.Headers.Add ("Disposition", "attach");
			fwr.Method = "PUT";
			fwr.PreAuthenticate = true;
			fwr.Proxy = new WebProxy ("proxy.ximian.com");
			fwr.Timeout = 20;

			BinaryFormatter bf = new BinaryFormatter ();
			bf.AssemblyFormat = FormatterAssemblyStyle.Full;

			MemoryStream ms = new MemoryStream ();
			bf.Serialize (ms, fwr);
			ms.Position = 0;

			byte [] buffer = new byte [ms.Length];
			ms.Read (buffer, 0, buffer.Length);
			Assert.AreEqual (_serialized, buffer);
		}

		[Test]
		public void Deserialize ()
		{
			MemoryStream ms = new MemoryStream ();
			ms.Write (_serialized, 0, _serialized.Length);
			ms.Position = 0;

			BinaryFormatter bf = new BinaryFormatter ();
			FileWebRequest req = (FileWebRequest) bf.Deserialize (ms);
			Assert.AreEqual ("CGN", req.ConnectionGroupName, "#A1");
			Assert.AreEqual (10, req.ContentLength, "#A2");
			Assert.AreEqual ("image/png", req.ContentType, "#A3");
			Assert.IsNull (req.Credentials, "#A4");
			Assert.AreEqual ("PUT", req.Method, "#A5");
			Assert.IsFalse (req.PreAuthenticate, "#A6");
			Assert.AreEqual ("file://test.txt/", req.RequestUri.AbsoluteUri, "#A7");
			Assert.AreEqual (20, req.Timeout, "#A8");

			WebHeaderCollection headers = req.Headers;
			Assert.IsNotNull (headers, "#C1");
			Assert.AreEqual (2, headers.Count, "#C2");
			Assert.AreEqual ("Content-Type", req.Headers.GetKey (0), "#C3");
			Assert.AreEqual ("image/png", req.Headers.Get (0), "#C4");
			Assert.AreEqual ("Disposition", req.Headers.GetKey (1), "#C5");
			Assert.AreEqual ("attach", req.Headers.Get (1), "#C6");

			WebProxy proxy = req.Proxy as WebProxy;
			Assert.IsNotNull (proxy, "#D1");
			Assert.AreEqual ("http://proxy.ximian.com/", proxy.Address.AbsoluteUri, "#D2");
			Assert.IsNotNull (proxy.BypassArrayList, "#D3");
			Assert.AreEqual (0, proxy.BypassArrayList.Count, "#D4");
			Assert.IsNotNull (proxy.BypassList, "#D5");
			Assert.AreEqual (0, proxy.BypassList.Length, "#D6");
			Assert.IsFalse (proxy.BypassProxyOnLocal, "#D7");
			Assert.IsNull (proxy.Credentials, "#D8");
		}

		private Uri GetTempFileUri ()
		{
			string tempFile = _tempFile;
			if (RunningOnUnix) {
				// remove leading slash for absolute paths
				tempFile = tempFile.TrimStart ('/');
			} else {
				tempFile = tempFile.Replace ('\\', '/');
			}
			return new Uri ("file:///" + tempFile);
		}

		private bool RunningOnUnix {
			get {
				// check for Unix platforms - see FAQ for more details
				// http://www.mono-project.com/FAQ:_Technical#How_to_detect_the_execution_platform_.3F
				int platform = (int) Environment.OSVersion.Platform;
				return ((platform == 4) || (platform == 128) || (platform == 6));
			}
		}

		private static readonly byte [] _serialized = new byte [] {
			0x00, 0x01, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0x01, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0c, 0x02, 0x00, 0x00, 0x00,
			0x49, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2c, 0x20, 0x56, 0x65,
			0x72, 0x73, 0x69, 0x6f, 0x6e, 0x3d, 0x32, 0x2e, 0x30, 0x2e, 0x30,
			0x2e, 0x30, 0x2c, 0x20, 0x43, 0x75, 0x6c, 0x74, 0x75, 0x72, 0x65,
			0x3d, 0x6e, 0x65, 0x75, 0x74, 0x72, 0x61, 0x6c, 0x2c, 0x20, 0x50,
			0x75, 0x62, 0x6c, 0x69, 0x63, 0x4b, 0x65, 0x79, 0x54, 0x6f, 0x6b,
			0x65, 0x6e, 0x3d, 0x62, 0x37, 0x37, 0x61, 0x35, 0x63, 0x35, 0x36,
			0x31, 0x39, 0x33, 0x34, 0x65, 0x30, 0x38, 0x39, 0x05, 0x01, 0x00,
			0x00, 0x00, 0x19, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e, 0x4e,
			0x65, 0x74, 0x2e, 0x46, 0x69, 0x6c, 0x65, 0x57, 0x65, 0x62, 0x52,
			0x65, 0x71, 0x75, 0x65, 0x73, 0x74, 0x09, 0x00, 0x00, 0x00, 0x07,
			0x68, 0x65, 0x61, 0x64, 0x65, 0x72, 0x73, 0x05, 0x70, 0x72, 0x6f,
			0x78, 0x79, 0x03, 0x75, 0x72, 0x69, 0x13, 0x63, 0x6f, 0x6e, 0x6e,
			0x65, 0x63, 0x74, 0x69, 0x6f, 0x6e, 0x47, 0x72, 0x6f, 0x75, 0x70,
			0x4e, 0x61, 0x6d, 0x65, 0x06, 0x6d, 0x65, 0x74, 0x68, 0x6f, 0x64,
			0x0d, 0x63, 0x6f, 0x6e, 0x74, 0x65, 0x6e, 0x74, 0x4c, 0x65, 0x6e,
			0x67, 0x74, 0x68, 0x07, 0x74, 0x69, 0x6d, 0x65, 0x6f, 0x75, 0x74,
			0x0a, 0x66, 0x69, 0x6c, 0x65, 0x41, 0x63, 0x63, 0x65, 0x73, 0x73,
			0x0f, 0x70, 0x72, 0x65, 0x61, 0x75, 0x74, 0x68, 0x65, 0x6e, 0x74,
			0x69, 0x63, 0x61, 0x74, 0x65, 0x04, 0x04, 0x04, 0x01, 0x01, 0x00,
			0x00, 0x03, 0x00, 0x1e, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e,
			0x4e, 0x65, 0x74, 0x2e, 0x57, 0x65, 0x62, 0x48, 0x65, 0x61, 0x64,
			0x65, 0x72, 0x43, 0x6f, 0x6c, 0x6c, 0x65, 0x63, 0x74, 0x69, 0x6f,
			0x6e, 0x02, 0x00, 0x00, 0x00, 0x13, 0x53, 0x79, 0x73, 0x74, 0x65,
			0x6d, 0x2e, 0x4e, 0x65, 0x74, 0x2e, 0x57, 0x65, 0x62, 0x50, 0x72,
			0x6f, 0x78, 0x79, 0x02, 0x00, 0x00, 0x00, 0x0a, 0x53, 0x79, 0x73,
			0x74, 0x65, 0x6d, 0x2e, 0x55, 0x72, 0x69, 0x02, 0x00, 0x00, 0x00,
			0x09, 0x08, 0x14, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e, 0x49,
			0x4f, 0x2e, 0x46, 0x69, 0x6c, 0x65, 0x41, 0x63, 0x63, 0x65, 0x73,
			0x73, 0x01, 0x02, 0x00, 0x00, 0x00, 0x09, 0x03, 0x00, 0x00, 0x00,
			0x09, 0x04, 0x00, 0x00, 0x00, 0x09, 0x05, 0x00, 0x00, 0x00, 0x06,
			0x06, 0x00, 0x00, 0x00, 0x03, 0x43, 0x47, 0x4e, 0x06, 0x07, 0x00,
			0x00, 0x00, 0x03, 0x50, 0x55, 0x54, 0x0a, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00, 0x04, 0xf8, 0xff, 0xff,
			0xff, 0x14, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e, 0x49, 0x4f,
			0x2e, 0x46, 0x69, 0x6c, 0x65, 0x41, 0x63, 0x63, 0x65, 0x73, 0x73,
			0x01, 0x00, 0x00, 0x00, 0x07, 0x76, 0x61, 0x6c, 0x75, 0x65, 0x5f,
			0x5f, 0x00, 0x08, 0x01, 0x00, 0x00, 0x00, 0x00, 0x05, 0x03, 0x00,
			0x00, 0x00, 0x1e, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e, 0x4e,
			0x65, 0x74, 0x2e, 0x57, 0x65, 0x62, 0x48, 0x65, 0x61, 0x64, 0x65,
			0x72, 0x43, 0x6f, 0x6c, 0x6c, 0x65, 0x63, 0x74, 0x69, 0x6f, 0x6e,
			0x05, 0x00, 0x00, 0x00, 0x05, 0x43, 0x6f, 0x75, 0x6e, 0x74, 0x01,
			0x30, 0x01, 0x32, 0x01, 0x31, 0x01, 0x33, 0x00, 0x01, 0x01, 0x01,
			0x01, 0x08, 0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x06,
			0x09, 0x00, 0x00, 0x00, 0x0c, 0x43, 0x6f, 0x6e, 0x74, 0x65, 0x6e,
			0x74, 0x2d, 0x54, 0x79, 0x70, 0x65, 0x06, 0x0a, 0x00, 0x00, 0x00,
			0x09, 0x69, 0x6d, 0x61, 0x67, 0x65, 0x2f, 0x70, 0x6e, 0x67, 0x06,
			0x0b, 0x00, 0x00, 0x00, 0x0b, 0x44, 0x69, 0x73, 0x70, 0x6f, 0x73,
			0x69, 0x74, 0x69, 0x6f, 0x6e, 0x06, 0x0c, 0x00, 0x00, 0x00, 0x06,
			0x61, 0x74, 0x74, 0x61, 0x63, 0x68, 0x05, 0x04, 0x00, 0x00, 0x00,
			0x13, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e, 0x4e, 0x65, 0x74,
			0x2e, 0x57, 0x65, 0x62, 0x50, 0x72, 0x6f, 0x78, 0x79, 0x04, 0x00,
			0x00, 0x00, 0x0e, 0x5f, 0x42, 0x79, 0x70, 0x61, 0x73, 0x73, 0x4f,
			0x6e, 0x4c, 0x6f, 0x63, 0x61, 0x6c, 0x0d, 0x5f, 0x50, 0x72, 0x6f,
			0x78, 0x79, 0x41, 0x64, 0x64, 0x72, 0x65, 0x73, 0x73, 0x0b, 0x5f,
			0x42, 0x79, 0x70, 0x61, 0x73, 0x73, 0x4c, 0x69, 0x73, 0x74, 0x16,
			0x5f, 0x55, 0x73, 0x65, 0x44, 0x65, 0x66, 0x61, 0x75, 0x6c, 0x74,
			0x43, 0x72, 0x65, 0x64, 0x65, 0x6e, 0x74, 0x69, 0x61, 0x6c, 0x73,
			0x00, 0x04, 0x02, 0x00, 0x01, 0x0a, 0x53, 0x79, 0x73, 0x74, 0x65,
			0x6d, 0x2e, 0x55, 0x72, 0x69, 0x02, 0x00, 0x00, 0x00, 0x01, 0x02,
			0x00, 0x00, 0x00, 0x00, 0x09, 0x0d, 0x00, 0x00, 0x00, 0x0a, 0x00,
			0x05, 0x05, 0x00, 0x00, 0x00, 0x0a, 0x53, 0x79, 0x73, 0x74, 0x65,
			0x6d, 0x2e, 0x55, 0x72, 0x69, 0x01, 0x00, 0x00, 0x00, 0x0b, 0x41,
			0x62, 0x73, 0x6f, 0x6c, 0x75, 0x74, 0x65, 0x55, 0x72, 0x69, 0x01,
			0x02, 0x00, 0x00, 0x00, 0x06, 0x0e, 0x00, 0x00, 0x00, 0x10, 0x66,
			0x69, 0x6c, 0x65, 0x3a, 0x2f, 0x2f, 0x74, 0x65, 0x73, 0x74, 0x2e,
			0x74, 0x78, 0x74, 0x2f, 0x01, 0x0d, 0x00, 0x00, 0x00, 0x05, 0x00,
			0x00, 0x00, 0x06, 0x0f, 0x00, 0x00, 0x00, 0x18, 0x68, 0x74, 0x74,
			0x70, 0x3a, 0x2f, 0x2f, 0x70, 0x72, 0x6f, 0x78, 0x79, 0x2e, 0x78,
			0x69, 0x6d, 0x69, 0x61, 0x6e, 0x2e, 0x63, 0x6f, 0x6d, 0x2f, 0x0b
		};
	}
}
