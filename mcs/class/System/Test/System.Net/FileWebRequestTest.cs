//
// FileWebRequestTest.cs - NUnit Test Cases for System.Net.FileWebRequest
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using NUnit.Framework;
using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Net
{

[TestFixture]
public class FileWebRequestTest
{
        [Test]
        public void Async ()
        {
		string tmpFilename = GetFilename ();
		if (tmpFilename == null) {
			Console.WriteLine ("\n\nSet environment variable TMPDIR to a temporary directory to test FileWebRequest\n");
			return;
		}
		
		try {
			if (File.Exists (tmpFilename)) 
				File.Delete (tmpFilename);
			
			Uri uri = new Uri ("file:///" + tmpFilename);
			
			WebRequest req = WebRequest.Create (uri);
			req.Method = "PUT";
			
			req.Timeout = 2 * 1000;
			IAsyncResult async = req.BeginGetRequestStream (null, null);
			try {
				req.BeginGetRequestStream (null, null);
				Assertion.Fail ("#1 should've failed");
			} catch (InvalidOperationException) { 
				//Console.WriteLine ("GOT1: " + e.Message + "\n" + e.StackTrace);				
				// Cannot re-call BeginGetRequestStream/BeginGetResponse while
				// a previous call is still in progress
			}
			/*
			try {
				req.BeginGetResponse (null, null);
				Assertion.Fail ("#2 should've failed");
			} catch (InvalidOperationException) { }
			*/
			try {
				req.GetRequestStream ();
				Assertion.Fail ("#3 should've failed");
			} catch (InvalidOperationException) { 
				// Console.WriteLine ("GOT3: " + e.Message + "\n" + e.StackTrace);
				// Cannot re-call BeginGetRequestStream/BeginGetResponse while
				// a previous call is still in progress
			}

			try {
				req.GetResponse ();
				Assertion.Fail ("#4 should've failed");
			} catch (WebException) { 
				// Console.WriteLine ("4: " + e.Message + "\n" + e.StackTrace);				
				// The operation has timed out
			}

			try {
				IAsyncResult async0 = req.BeginGetResponse (null, null);
				req.EndGetResponse (async0);
				// Console.WriteLine ("X5c");
				Assertion.Fail ("#5 should've failed");
			} catch (InvalidOperationException) { 
				// Console.WriteLine ("5e: " + e.Message + "\n" + e.StackTrace);
				// Cannot re-call BeginGetRequestStream/BeginGetResponse while
				// a previous call is still in progress
			}
			
			// Console.WriteLine ("WEBHEADERS: " + req.Headers);
			
			Stream wstream = req.EndGetRequestStream (async);
			Assertion.AssertEquals ("#1r", false, wstream.CanRead);
			Assertion.AssertEquals ("#1w", true, wstream.CanWrite);
			Assertion.AssertEquals ("#1s", true, wstream.CanSeek);

			wstream.WriteByte (72);
			wstream.WriteByte (101);
			wstream.WriteByte (108);
			wstream.WriteByte (108);
			wstream.WriteByte (111);
			wstream.Close ();
			
			// stream written

			req = WebRequest.Create (uri);
			WebResponse res = req.GetResponse ();	
			
			try {
				req.BeginGetRequestStream (null, null);
				Assertion.Fail ("#20: should've failed");
			} catch (InvalidOperationException) { 
				// Console.WriteLine ("20: " + e.Message + "\n" + e.StackTrace);				
				// Cannot send a content-body with this verb-type
			}
			
			try {
				req.Method = "PUT";
				req.BeginGetRequestStream (null, null);
				Assertion.Fail ("#21: should've failed");
			} catch (InvalidOperationException) { 
				// Console.WriteLine ("21: " + e.Message + "\n" + e.StackTrace);				
				// This operation cannot be perfomed after the request has been submitted.
			}
			
			try {
				//IAsyncResult async2 = req.BeginGetResponse (null, null);
				//Console.WriteLine ("OK!");
				req.GetResponse ();
				//Assertion.Fail ("#22: should've failed");
			} catch (InvalidOperationException) { 
				//Console.WriteLine ("22: " + e.Message + "\n" + e.StackTrace);
				// Cannot re-call BeginGetRequestStream/BeginGetResponse while
				// a previous call is still in progress
				Assertion.Fail ("#22: should not have failed");
			}			
			
			try {
				IAsyncResult async2 = req.BeginGetResponse (null, null);
				
				// this succeeds !!
				
				try {
					WebResponse res2 = req.EndGetResponse (async2);
										
					// and this succeeds
					
					Assertion.AssertEquals ("#23", res, res2) ;
					
					//Assertion.Fail ("#23: should've failed");
				} catch (InvalidOperationException) { 
					//Console.WriteLine ("22: " + e.Message + "\n" + e.StackTrace);				
					// Cannot re-call BeginGetRequestStream/BeginGetResponse while
					// a previous call is still in progress
				}				
				
				// Assertion.Fail ("#22: should've failed");
			} catch (InvalidOperationException) { 
			}			

			Assertion.AssertEquals ("#2 len", (long) 5, res.ContentLength);
			Assertion.AssertEquals ("#2 type", "binary/octet-stream", res.ContentType);
			Assertion.AssertEquals ("#2 scheme", "file", res.ResponseUri.Scheme);
			
			Stream rstream = res.GetResponseStream ();			
			Assertion.AssertEquals ("#3r", true, rstream.CanRead);
			Assertion.AssertEquals ("#3w", false, rstream.CanWrite);
			Assertion.AssertEquals ("#3s", true, rstream.CanSeek);
			
			Assertion.AssertEquals ("#4a", 72, rstream.ReadByte ());
			Assertion.AssertEquals ("#4b", 101, rstream.ReadByte ());
			Assertion.AssertEquals ("#4c", 108, rstream.ReadByte ());

			rstream.Close ();
			// res.Close ();
			
			try {
				long len = res.ContentLength;
				Assertion.AssertEquals ("#5", (long) 5, len);
			} catch (ObjectDisposedException) {
				Assertion.Fail ("#disposed contentlength");				
			}
			try {
				WebHeaderCollection w = res.Headers;
			} catch (ObjectDisposedException) {
				Assertion.Fail ("#disposed headers");				
			}			
			try {
				res.Close ();				
			} catch (ObjectDisposedException) {
				Assertion.Fail ("#disposed close");				
			}
		} catch (Exception) {
			// Console.WriteLine ("ERROR! : " + ee.Message + "\n" + ee.StackTrace);
		} finally {
			try {
				// known bug #24940
				File.Delete (tmpFilename);
			} catch (Exception) { 
				// Console.WriteLine ("ERROR2! : " + ee2.Message + "\n" + ee2.StackTrace);
			}
		}
	}	        
        
        [Test]
        public void Sync ()
        {
		string tmpFilename = GetFilename ();
		if (tmpFilename == null)
			return;
		
		try {		
			if (File.Exists (tmpFilename)) 
				File.Delete (tmpFilename);
			
			Uri uri = new Uri ("file:///" + tmpFilename);
			
			WebRequest req = WebRequest.Create (uri);
			
			try {
				Stream stream = req.GetRequestStream ();
				Assertion.Fail ("should throw exception");
			} catch (ProtocolViolationException) {}
			
			req.Method = "PUT";
			
			Stream wstream = req.GetRequestStream ();
			Assertion.AssertEquals ("#1r", false, wstream.CanRead);
			Assertion.AssertEquals ("#1w", true, wstream.CanWrite);
			Assertion.AssertEquals ("#1s", true, wstream.CanSeek);

			wstream.WriteByte (72);
			wstream.WriteByte (101);
			wstream.WriteByte (108);
			wstream.WriteByte (108);
			wstream.WriteByte (111);
			wstream.Close ();
			
			// stream written
			
			req = WebRequest.Create (uri);
			WebResponse res = req.GetResponse ();			
			Assertion.AssertEquals ("#2 len", (long) 5, res.ContentLength);
			Assertion.AssertEquals ("#2 type", "binary/octet-stream", res.ContentType);
			Assertion.AssertEquals ("#2 scheme", "file", res.ResponseUri.Scheme);
			
			Stream rstream = res.GetResponseStream ();			
			Assertion.AssertEquals ("#3r", true, rstream.CanRead);
			Assertion.AssertEquals ("#3w", false, rstream.CanWrite);
			Assertion.AssertEquals ("#3s", true, rstream.CanSeek);
			
			Assertion.AssertEquals ("#4a", 72, rstream.ReadByte ());
			Assertion.AssertEquals ("#4b", 101, rstream.ReadByte ());
			Assertion.AssertEquals ("#4c", 108, rstream.ReadByte ());
			
			rstream.Close ();
			// res.Close ();
			
			try {
				long len = res.ContentLength;
				Assertion.AssertEquals ("#5", (long) 5, len);
			} catch (ObjectDisposedException) {
				Assertion.Fail ("#disposed contentlength");				
			}
			try {
				WebHeaderCollection w = res.Headers;
			} catch (ObjectDisposedException) {
				Assertion.Fail ("#disposed headers");				
			}			
			try {
				res.Close ();				
			} catch (ObjectDisposedException) {
				Assertion.Fail ("#disposed close");				
			}
			
		} finally {
			try {
				File.Delete (tmpFilename);
			} catch (Exception) { }
		}
	}	
	
	private string GetFilename ()
	{
		string tmpdir = Environment.GetEnvironmentVariable ("TMPDIR");
		if (tmpdir == null || tmpdir.Length == 0) {
			return null;
		}
		
		tmpdir = tmpdir.Replace ('\\', '/');
		if (tmpdir [tmpdir.Length - 1] != '/')
			tmpdir += "/";
		return tmpdir + "FileWebRequestTest.tmp";
	}
}

}
