//
// FileWebRequestTest.cs - NUnit Test Cases for System.Net.FileWebRequest
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
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

public class FileWebRequestTest : TestCase
{
        public FileWebRequestTest () :
                base ("[MonoTests.System.Net.FileWebRequestTest]") {}

        public FileWebRequestTest (string name) : base (name) {}

        protected override void SetUp () {}

        protected override void TearDown () {}

        public static ITest Suite
        {
                get {
                        return new TestSuite (typeof (FileWebRequestTest));
                }
        }
        
        public void TestAll ()
        {
		string tmpdir = Environment.GetEnvironmentVariable ("TMPDIR");
		if (tmpdir == null || tmpdir.Length == 0) {
			Console.WriteLine ("\n\nSet environment variable TMPDIR to a temporary directory to test FileWebRequest\n");
			return;
		}
		
		tmpdir = tmpdir.Replace ('\\', '/');
		if (tmpdir [tmpdir.Length - 1] != '/')
			tmpdir += "/";
		string tmpFilename = tmpdir + "FileWebRequestTest.tmp";

		try {
			if (File.Exists (tmpFilename)) 
				File.Delete (tmpFilename);
			
			Uri uri = new Uri ("file:///" + tmpFilename);
			
			WebRequest req = WebRequest.Create (uri);
			
			try {
				Stream stream = req.GetRequestStream ();
				Fail ("should throw exception");
			} catch (ProtocolViolationException) {}
			
			req.Method = "PUT";
			
			Stream wstream = req.GetRequestStream ();
			AssertEquals ("#1r", false, wstream.CanRead);
			AssertEquals ("#1w", true, wstream.CanWrite);
			AssertEquals ("#1s", true, wstream.CanSeek);

			wstream.WriteByte (72);
			wstream.WriteByte (101);
			wstream.WriteByte (108);
			wstream.WriteByte (108);
			wstream.WriteByte (111);
			wstream.Close ();
			
			// stream written
			
			req = WebRequest.Create (uri);
			WebResponse res = req.GetResponse ();			
			AssertEquals ("#2 len", (long) 5, res.ContentLength);
			AssertEquals ("#2 type", "binary/octet-stream", res.ContentType);
			AssertEquals ("#2 scheme", "file", res.ResponseUri.Scheme);
			
			Stream rstream = res.GetResponseStream ();			
			AssertEquals ("#3r", true, rstream.CanRead);
			AssertEquals ("#3w", false, rstream.CanWrite);
			AssertEquals ("#3s", true, rstream.CanSeek);
			
			AssertEquals ("#4a", 72, rstream.ReadByte ());
			AssertEquals ("#4b", 101, rstream.ReadByte ());
			AssertEquals ("#4c", 108, rstream.ReadByte ());
			
			rstream.Close ();
			// res.Close ();
			
			try {
				long len = res.ContentLength;
				AssertEquals ("#5", (long) 5, len);
			} catch (ObjectDisposedException) {
				Fail ("#disposed contentlength");				
			}
			try {
				WebHeaderCollection w = res.Headers;
			} catch (ObjectDisposedException) {
				Fail ("#disposed headers");				
			}			
			try {
				res.Close ();				
			} catch (ObjectDisposedException) {
				Fail ("#disposed close");				
			}
			
		} finally {
			try {
				File.Delete (tmpFilename);
			} catch (Exception) { }
		}
	}	
}

}

