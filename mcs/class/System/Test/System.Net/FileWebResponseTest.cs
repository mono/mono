//
// FileWebResponseTest.cs - NUnit Test Cases for System.Net.FileWebResponse
//
// Authors:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2007 Gert Driesen
//

using System;
using System.IO;
using System.Net;

using NUnit.Framework;

using MonoTests.Helpers;


namespace MonoTests.System.Net
{
	[TestFixture]
	[Category("NotWasm")]
	public class FileWebResponseTest
	{
		private TempDirectory _tempDirectory;
		private string _tempFile;
		private Uri _tempFileUri;

		[SetUp]
		public void SetUp ()
		{
			_tempDirectory = new TempDirectory ();
			_tempFile = Path.Combine (_tempDirectory.Path, "FileWebResponseTest.tmp");
			_tempFileUri = GetTempFileUri ();
		}

		[TearDown]
		public void TearDown ()
		{
			_tempDirectory.Dispose ();
		}

		[Test]
		public void ContentLength ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			req.Method = "PUT";
			req.ContentLength = 100;
			using (Stream s = req.GetRequestStream ()) {
				s.WriteByte (72);
				s.WriteByte (110);
				s.WriteByte (80);
				s.Flush ();
			}
			req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			using (FileWebResponse resp = (FileWebResponse) req.GetResponse ()) {
				Assert.AreEqual (3, resp.ContentLength, "#1");
				Assert.AreEqual (2, resp.Headers.Count, "#2");
				Assert.AreEqual ("Content-Length", resp.Headers.Keys [0], "#3");
				Assert.AreEqual ("3", resp.Headers.Get (0), "#4");
				resp.Headers.Clear ();
				Assert.AreEqual (3, resp.ContentLength, "#5");
				Assert.AreEqual (0, resp.Headers.Count, "#6");
			}
		}

		[Test]
		public void ContentType ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			req.Method = "PUT";
			req.ContentType = "image/png";
			using (Stream s = req.GetRequestStream ()) {
				s.WriteByte (72);
				s.WriteByte (110);
				s.WriteByte (80);
				s.Flush ();
			}
			req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			using (FileWebResponse resp = (FileWebResponse) req.GetResponse ()) {
				Assert.AreEqual ("application/octet-stream", resp.ContentType, "#1");
				Assert.AreEqual (2, resp.Headers.Count, "#2");
				Assert.AreEqual ("Content-Type", resp.Headers.Keys [1], "#3");
				Assert.AreEqual ("application/octet-stream", resp.Headers.Get (1), "#4");
				resp.Headers.Clear ();
				Assert.AreEqual ("application/octet-stream", resp.ContentType, "#5");
				Assert.AreEqual (0, resp.Headers.Count, "#6");
			}
		}

		[Test]
		public void GetResponseStream ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			req.Method = "PUT";
			req.ContentType = "image/png";
			using (Stream s = req.GetRequestStream ()) {
				s.WriteByte (72);
				s.WriteByte (110);
				s.WriteByte (80);
				s.Flush ();
			}
			req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			FileWebResponse respA = null;
			FileWebResponse respB = null;
			FileStream fsA = null;
			FileStream fsB = null;
			try {
				respA = (FileWebResponse) req.GetResponse ();
				fsA = respA.GetResponseStream () as FileStream;
				Assert.IsNotNull (fsA, "#A1");
				Assert.IsTrue (fsA.CanRead, "#A2");
				Assert.IsTrue (fsA.CanSeek, "#A3");
				Assert.IsFalse (fsA.CanTimeout, "#A4");
				Assert.IsFalse (fsA.CanWrite, "#A5");
				Assert.AreEqual (3, fsA.Length, "#A6");
				Assert.AreEqual (0, fsA.Position, "#A7");
				try {
					int i = fsA.ReadTimeout;
					Assert.Fail ("#A8:" + i);
				} catch (InvalidOperationException) {
				}
				try {
					int i = fsA.WriteTimeout;
					Assert.Fail ("#A9:" + i);
				} catch (InvalidOperationException) {
				}

				respB = (FileWebResponse) req.GetResponse ();
				fsB = respB.GetResponseStream () as FileStream;
				Assert.IsNotNull (fsB, "#B1");
				Assert.IsTrue (fsB.CanRead, "#B2");
				Assert.IsTrue (fsB.CanSeek, "#B3");
				Assert.IsFalse (fsB.CanTimeout, "#B4");
				Assert.IsFalse (fsB.CanWrite, "#B5");
				Assert.AreEqual (3, fsB.Length, "#B6");
				Assert.AreEqual (0, fsB.Position, "#B7");
				try {
					int i = fsB.ReadTimeout;
					Assert.Fail ("#B8:" + i);
				} catch (InvalidOperationException) {
				}
				try {
					int i = fsB.WriteTimeout;
					Assert.Fail ("#B9:" + i);
				} catch (InvalidOperationException) {
				}
			} finally {
				if (respA != null)
					respA.Close ();
				if (respB != null)
					respB.Close ();
			}
		}

		[Test]
		public void Headers ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			req.Method = "PUT";
			req.Headers.Add ("Disposition", "attach");
			using (Stream s = req.GetRequestStream ()) {
				s.WriteByte (72);
				s.WriteByte (110);
				s.WriteByte (80);
				s.Flush ();
			}
			req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			using (FileWebResponse resp = (FileWebResponse) req.GetResponse ()) {
				Assert.AreEqual (2, resp.Headers.Count, "#1");
				Assert.AreEqual ("Content-Length", resp.Headers.Keys [0], "#2");
				Assert.AreEqual ("Content-Type", resp.Headers.Keys [1], "#3");
			}
		}

		[Test]
		[Category("MultiThreaded")]
		public void ResponseUri ()
		{
			FileWebRequest req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			req.Method = "PUT";
			req.ContentType = "image/png";
			using (Stream s = req.GetRequestStream ()) {
				s.WriteByte (72);
				s.WriteByte (110);
				s.WriteByte (80);
				s.Flush ();
			}
			req = (FileWebRequest) WebRequest.Create (_tempFileUri);
			using (FileWebResponse resp = (FileWebResponse) req.GetResponse ()) {
				Assert.AreEqual (_tempFileUri, resp.ResponseUri);
			}
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
				return ((platform == 4) || (platform == 128));
			}
		}
	}
}
