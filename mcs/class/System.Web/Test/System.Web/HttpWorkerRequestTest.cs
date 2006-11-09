//
// Tests for System.Web.Hosting.SimpleWorkerRequest.cs 
//
// Author:
//	Miguel de Icaza (miguel@novell.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using NUnit.Framework;
using System;
using System.Globalization;
using System.Web;
using System.Web.Hosting;

namespace MonoTests.System.Web {

		public class FakeHttpWorkerRequest : HttpWorkerRequest {

			public override string GetUriPath()
			{
				return "GetUriPath";
			}
	
			public override string GetQueryString()
			{
				return "GetQueryString";
			}
	
			public override string GetRawUrl()
			{
				return "GetRawUrl";
			}
	
			public override string GetHttpVerbName()
			{
				return "GetVerbName";
			}
	
			public override string GetHttpVersion()
			{
				return "GetHttpVersion";
			}
	
			public override string GetRemoteAddress()
			{
				return "__GetRemoteAddress";
			}
	
			public override int GetRemotePort()
			{
				return 1010;
			}
	
			public override string GetLocalAddress()
			{
				return "GetLocalAddress";
			}
	
			public override int GetLocalPort()
			{
				return 2020;
			}
	
			public override void SendStatus(int s, string x)
			{
			}
	
			public override void SendKnownResponseHeader(int x, string j)
			{
			}
	
			public override void SendUnknownResponseHeader(string a, string b)
			{
			}
		
			public override void SendResponseFromMemory(byte[] arr, int x)
			{
			}
	
			public override void SendResponseFromFile(string a, long b , long c)
			{
			}
	
			public override void SendResponseFromFile (IntPtr a, long b, long c)
			{
			}
	
			public override void FlushResponse(bool x)
			{
			}
	
			public override void EndOfRequest() {
			}

		}

	[TestFixture]
	public class HttpWorkerTests {

		[Test] public void TestPublicValues ()
		{
			Assert.AreEqual ( 0, HttpWorkerRequest.HeaderCacheControl, "V 0");
			Assert.AreEqual ( 1, HttpWorkerRequest.HeaderConnection, "V 1");
			Assert.AreEqual ( 2, HttpWorkerRequest.HeaderDate, "V 2");
			Assert.AreEqual ( 3, HttpWorkerRequest.HeaderKeepAlive, "V 3");
			Assert.AreEqual ( 4, HttpWorkerRequest.HeaderPragma, "V 4");
			Assert.AreEqual ( 5, HttpWorkerRequest.HeaderTrailer, "V 5");
			Assert.AreEqual ( 6, HttpWorkerRequest.HeaderTransferEncoding, "V 6");
			Assert.AreEqual ( 7, HttpWorkerRequest.HeaderUpgrade, "V 7");
			Assert.AreEqual ( 8, HttpWorkerRequest.HeaderVia, "V 8");
			Assert.AreEqual ( 9, HttpWorkerRequest.HeaderWarning, "V 9");
			Assert.AreEqual ( 10, HttpWorkerRequest.HeaderAllow, "V10");
			Assert.AreEqual ( 11, HttpWorkerRequest.HeaderContentLength, "V11");
			Assert.AreEqual ( 12, HttpWorkerRequest.HeaderContentType, "V12");
			Assert.AreEqual ( 13, HttpWorkerRequest.HeaderContentEncoding, "V13");
			Assert.AreEqual ( 14, HttpWorkerRequest.HeaderContentLanguage, "V14");
			Assert.AreEqual ( 15, HttpWorkerRequest.HeaderContentLocation, "V15");
			Assert.AreEqual ( 16, HttpWorkerRequest.HeaderContentMd5, "V16");
			Assert.AreEqual ( 17, HttpWorkerRequest.HeaderContentRange, "V17");
			Assert.AreEqual ( 18, HttpWorkerRequest.HeaderExpires, "V18");
			Assert.AreEqual ( 19, HttpWorkerRequest.HeaderLastModified, "V19");
			Assert.AreEqual ( 20, HttpWorkerRequest.HeaderAccept, "V20");
			Assert.AreEqual ( 21, HttpWorkerRequest.HeaderAcceptCharset, "V21");
			Assert.AreEqual ( 22, HttpWorkerRequest.HeaderAcceptEncoding, "V22");
			Assert.AreEqual ( 23, HttpWorkerRequest.HeaderAcceptLanguage, "V23");
			Assert.AreEqual ( 24, HttpWorkerRequest.HeaderAuthorization, "V24");
			Assert.AreEqual ( 25, HttpWorkerRequest.HeaderCookie, "V25");
			Assert.AreEqual ( 26, HttpWorkerRequest.HeaderExpect, "V26");
			Assert.AreEqual ( 27, HttpWorkerRequest.HeaderFrom, "V27");
			Assert.AreEqual ( 28, HttpWorkerRequest.HeaderHost, "V28");
			Assert.AreEqual ( 29, HttpWorkerRequest.HeaderIfMatch, "V29");
			Assert.AreEqual ( 30, HttpWorkerRequest.HeaderIfModifiedSince, "V30");
			Assert.AreEqual ( 31, HttpWorkerRequest.HeaderIfNoneMatch, "V31");
			Assert.AreEqual ( 32, HttpWorkerRequest.HeaderIfRange, "V32");
			Assert.AreEqual ( 33, HttpWorkerRequest.HeaderIfUnmodifiedSince, "V33");
			Assert.AreEqual ( 34, HttpWorkerRequest.HeaderMaxForwards, "V34");
			Assert.AreEqual ( 35, HttpWorkerRequest.HeaderProxyAuthorization, "V35");
			Assert.AreEqual ( 36, HttpWorkerRequest.HeaderReferer, "V36");
			Assert.AreEqual ( 37, HttpWorkerRequest.HeaderRange, "V37");
			Assert.AreEqual ( 38, HttpWorkerRequest.HeaderTe, "V38");
			Assert.AreEqual ( 39, HttpWorkerRequest.HeaderUserAgent, "V39");
			Assert.AreEqual ( 40, HttpWorkerRequest.RequestHeaderMaximum, "V40");

			Assert.AreEqual ( 20, HttpWorkerRequest.HeaderAcceptRanges, "V41");
			Assert.AreEqual ( 21, HttpWorkerRequest.HeaderAge, "V42");
			Assert.AreEqual ( 22, HttpWorkerRequest.HeaderEtag, "V43");
			Assert.AreEqual ( 23, HttpWorkerRequest.HeaderLocation, "V44");
			Assert.AreEqual ( 24, HttpWorkerRequest.HeaderProxyAuthenticate, "V45");
			Assert.AreEqual ( 25, HttpWorkerRequest.HeaderRetryAfter, "V46");
			Assert.AreEqual ( 26, HttpWorkerRequest.HeaderServer, "V47");
			Assert.AreEqual ( 27, HttpWorkerRequest.HeaderSetCookie, "V48");
			Assert.AreEqual ( 28, HttpWorkerRequest.HeaderVary, "V49");
			Assert.AreEqual ( 29, HttpWorkerRequest.HeaderWwwAuthenticate, "V50");
			Assert.AreEqual ( 30, HttpWorkerRequest.ResponseHeaderMaximum, "V51");

			Assert.AreEqual ( 1, HttpWorkerRequest.ReasonFileHandleCacheMiss, "R1");
			Assert.AreEqual ( 2, HttpWorkerRequest.ReasonCachePolicy, "R2");
			Assert.AreEqual ( 3, HttpWorkerRequest.ReasonCacheSecurity, "R3");
			Assert.AreEqual ( 4, HttpWorkerRequest.ReasonClientDisconnect, "R4");

			Assert.AreEqual ( 0, HttpWorkerRequest.ReasonDefault, "RR0");
		}

		public class FakeHttpWorkerRequest : HttpWorkerRequest {

			public override string GetUriPath()
			{
				return "GetUriPath";
			}
	
			public override string GetQueryString()
			{
				return "GetQueryString";
			}
	
			public override string GetRawUrl()
			{
				return "GetRawUrl";
			}
	
			public override string GetHttpVerbName()
			{
				return "GetVerbName";
			}
	
			public override string GetHttpVersion()
			{
				return "GetHttpVersion";
			}
	
			public override string GetRemoteAddress()
			{
				return "__GetRemoteAddress";
			}
	
			public override int GetRemotePort()
			{
				return 1010;
			}
	
			public override string GetLocalAddress()
			{
				return "GetLocalAddress";
			}

			public override string GetAppPath ()
			{
				return "BLABA";
			}
			
			public override int GetLocalPort()
			{
				return 2020;
			}
	
			public override void SendStatus(int s, string x)
			{
			}
	
			public override void SendKnownResponseHeader(int x, string j)
			{
			}
	
			public override void SendUnknownResponseHeader(string a, string b)
			{
			}
		
			public override void SendResponseFromMemory(byte[] arr, int x)
			{
			}
	
			public override void SendResponseFromFile(string a, long b , long c)
			{
			}
	
			public override void SendResponseFromFile (IntPtr a, long b, long c)
			{
			}
	
			public override void FlushResponse(bool x)
			{
			}
	
			public override void EndOfRequest() {
			}

		}

#if TARGET_JVM //BUG #6499
		[Category ("NotWorking")]
#endif
		[Test] public void TestDefaults ()
		{
			FakeHttpWorkerRequest f = new FakeHttpWorkerRequest ();

			Assert.AreEqual (null, f.MachineConfigPath, "F1");
			Assert.AreEqual (null, f.MapPath ("x"), "F2");
			Assert.AreEqual (null, f.MachineConfigPath, "F3");
			Assert.AreEqual (null, f.MachineInstallDirectory, "F4");
			Assert.AreEqual ("BLABA", f.GetAppPath (), "F5");
			Assert.AreEqual (null, f.GetAppPathTranslated (), "F5");
			Assert.AreEqual (null, f.GetAppPoolID (), "F6");
			Assert.AreEqual (0, f.GetBytesRead (), "F7");
			Assert.AreEqual (new byte [0], f.GetClientCertificate (), "F8");
			Assert.AreEqual (new byte [0], f.GetClientCertificateBinaryIssuer (), "F9");
			Assert.AreEqual (0, f.GetClientCertificateEncoding (), "F10");
			Assert.AreEqual (new byte [0], f.GetClientCertificatePublicKey (), "F11");

			Assert.AreEqual (0, f.GetConnectionID (), "F14");
			Assert.AreEqual (null, f.GetFilePathTranslated () , "F16");
			Assert.AreEqual ("", f.GetPathInfo () , "F17");
			Assert.AreEqual (null, f.GetPreloadedEntityBody () , "F18");
			Assert.AreEqual ("http", f.GetProtocol () , "F19");
			Assert.AreEqual (null, f.GetQueryStringRawBytes () , "F20");			
			Assert.AreEqual ("__GetRemoteAddress", f.GetRemoteName () , "F21");
			Assert.AreEqual (0, f.GetRequestReason () , "F22");
			Assert.AreEqual ("GetLocalAddress", f.GetServerName () , "F23");
			Assert.AreEqual (null, f.GetServerVariable ("A") , "F24");
			Assert.AreEqual (null, f.GetUnknownRequestHeader ("IAMTHEUKNOWNN"), "F25");
			Assert.AreEqual (null, f.GetUnknownRequestHeaders (), "F26");
			Assert.AreEqual (0, f.GetUrlContextID (), "F27");
			Assert.AreEqual (IntPtr.Zero, f.GetUserToken (), "F28");
			Assert.AreEqual (IntPtr.Zero, f.GetVirtualPathToken (), "F29");
			Assert.AreEqual (false, f.HasEntityBody (), "F30");
			Assert.AreEqual (true, f.HeadersSent (), "F31");
			Assert.AreEqual (true, f.IsClientConnected (), "F32");
			Assert.AreEqual (false, f.IsEntireEntityBodyIsPreloaded (), "F33");
			Assert.AreEqual (false, f.IsSecure (), "F34");

			Assert.AreEqual (0, f.ReadEntityBody (null, Int32.MaxValue), "ReadEntityBody(byte[],int)");
#if NET_2_0
			Assert.AreEqual (Guid.Empty.ToString (), f.RequestTraceIdentifier.ToString (), "RequestTraceIdentifier");
			Assert.IsNull (f.RootWebConfigPath, "RootWebConfigPath");
			Assert.AreEqual (0, f.GetPreloadedEntityBody (null, Int32.MinValue), "GetPreloadedEntityBody(byte[],int)");
			Assert.AreEqual (0, f.GetPreloadedEntityBodyLength (), "GetPreloadedEntityBodyLength");
			Assert.AreEqual (0, f.GetTotalEntityBodyLength (), "GetTotalEntityBodyLength");
			Assert.AreEqual (0, f.ReadEntityBody (null, 0, 0), "ReadEntityBody(byte[],int,int)");
#endif
		}

		[Test] public void Test_GetKnownHeaderName ()
		{
			//
			// GetKnownRequestHeaderName
			//
			Assert.AreEqual ("Cache-Control", HttpWorkerRequest.GetKnownRequestHeaderName (0), "F17");
			Assert.AreEqual ("Connection", HttpWorkerRequest.GetKnownRequestHeaderName (1), "F18");
			Assert.AreEqual ("Date", HttpWorkerRequest.GetKnownRequestHeaderName (2), "F19");
			Assert.AreEqual ("Keep-Alive", HttpWorkerRequest.GetKnownRequestHeaderName (3), "F20");
			Assert.AreEqual ("Pragma", HttpWorkerRequest.GetKnownRequestHeaderName (4), "F21");
			Assert.AreEqual ("Trailer", HttpWorkerRequest.GetKnownRequestHeaderName (5), "F22");
			Assert.AreEqual ("Transfer-Encoding", HttpWorkerRequest.GetKnownRequestHeaderName (6), "F23");
			Assert.AreEqual ("Upgrade", HttpWorkerRequest.GetKnownRequestHeaderName (7), "F24");
			Assert.AreEqual ("Via", HttpWorkerRequest.GetKnownRequestHeaderName (8), "F25");
			Assert.AreEqual ("Warning", HttpWorkerRequest.GetKnownRequestHeaderName (9), "F26");
			Assert.AreEqual ("Allow", HttpWorkerRequest.GetKnownRequestHeaderName (10), "F27");
			Assert.AreEqual ("Content-Length", HttpWorkerRequest.GetKnownRequestHeaderName (11), "F28");
			Assert.AreEqual ("Content-Type", HttpWorkerRequest.GetKnownRequestHeaderName (12), "F29");
			Assert.AreEqual ("Content-Encoding", HttpWorkerRequest.GetKnownRequestHeaderName (13), "F30");
			Assert.AreEqual ("Content-Language", HttpWorkerRequest.GetKnownRequestHeaderName (14), "F31");
			Assert.AreEqual ("Content-Location", HttpWorkerRequest.GetKnownRequestHeaderName (15), "F32");
			Assert.AreEqual ("Content-MD5", HttpWorkerRequest.GetKnownRequestHeaderName (16), "F33");
			Assert.AreEqual ("Content-Range", HttpWorkerRequest.GetKnownRequestHeaderName (17), "F34");
			Assert.AreEqual ("Expires", HttpWorkerRequest.GetKnownRequestHeaderName (18), "F35");
			Assert.AreEqual ("Last-Modified", HttpWorkerRequest.GetKnownRequestHeaderName (19), "F36");
			Assert.AreEqual ("Accept", HttpWorkerRequest.GetKnownRequestHeaderName (20), "F37");
			Assert.AreEqual ("Accept-Charset", HttpWorkerRequest.GetKnownRequestHeaderName (21), "F38");
			Assert.AreEqual ("Accept-Encoding", HttpWorkerRequest.GetKnownRequestHeaderName (22), "F39");
			Assert.AreEqual ("Accept-Language", HttpWorkerRequest.GetKnownRequestHeaderName (23), "F40");
			Assert.AreEqual ("Authorization", HttpWorkerRequest.GetKnownRequestHeaderName (24), "F41");
			Assert.AreEqual ("Cookie", HttpWorkerRequest.GetKnownRequestHeaderName (25), "F42");
			Assert.AreEqual ("Expect", HttpWorkerRequest.GetKnownRequestHeaderName (26), "F43");
			Assert.AreEqual ("From", HttpWorkerRequest.GetKnownRequestHeaderName (27), "F44");
			Assert.AreEqual ("Host", HttpWorkerRequest.GetKnownRequestHeaderName (28), "F45");
			Assert.AreEqual ("If-Match", HttpWorkerRequest.GetKnownRequestHeaderName (29), "F46");
			Assert.AreEqual ("If-Modified-Since", HttpWorkerRequest.GetKnownRequestHeaderName (30), "F47");
			Assert.AreEqual ("If-None-Match", HttpWorkerRequest.GetKnownRequestHeaderName (31), "F48");
			Assert.AreEqual ("If-Range", HttpWorkerRequest.GetKnownRequestHeaderName (32), "F49");
			Assert.AreEqual ("If-Unmodified-Since", HttpWorkerRequest.GetKnownRequestHeaderName (33), "F50");
			Assert.AreEqual ("Max-Forwards", HttpWorkerRequest.GetKnownRequestHeaderName (34), "F51");
			Assert.AreEqual ("Proxy-Authorization", HttpWorkerRequest.GetKnownRequestHeaderName (35), "F52");
			Assert.AreEqual ("Referer", HttpWorkerRequest.GetKnownRequestHeaderName (36), "F53");
			Assert.AreEqual ("Range", HttpWorkerRequest.GetKnownRequestHeaderName (37), "F54");
			Assert.AreEqual ("TE", HttpWorkerRequest.GetKnownRequestHeaderName (38), "F55");
			Assert.AreEqual ("User-Agent", HttpWorkerRequest.GetKnownRequestHeaderName (39), "F56");
		}

		[ExpectedException (typeof (IndexOutOfRangeException))]
		[Test] public void Test_OutOfRangeHeaderName ()
		{
			HttpWorkerRequest.GetKnownRequestHeaderName (HttpWorkerRequest.RequestHeaderMaximum);
		}

		[ExpectedException (typeof (IndexOutOfRangeException))]
		[Test] public void Test_OutOfRangeHeaderName2 ()
		{
			HttpWorkerRequest.GetKnownRequestHeaderName (-1);
		}
		
		[Test] public void Test_GetKnownHeaderIndex ()
		{
			Assert.AreEqual (0, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Cache-Control"), "N0");
			Assert.AreEqual (1, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Connection"), "N1");
			Assert.AreEqual (2, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Date"), "N2");
			Assert.AreEqual (3, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Keep-Alive"), "N3");
			Assert.AreEqual (4, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Pragma"), "N4");
			Assert.AreEqual (5, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Trailer"), "N5");
			Assert.AreEqual (6, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Transfer-Encoding"), "N6");
			Assert.AreEqual (7, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Upgrade"), "N7");
			Assert.AreEqual (8, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Via"), "N8");
			Assert.AreEqual (9, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Warning"), "N9");
			Assert.AreEqual (10, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Allow"), "N10");
			Assert.AreEqual (11, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Content-Length"), "N11");
			Assert.AreEqual (12, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Content-Type"), "N12");
			Assert.AreEqual (13, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Content-Encoding"), "N13");
			Assert.AreEqual (14, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Content-Language"), "N14");
			Assert.AreEqual (15, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Content-Location"), "N15");
			Assert.AreEqual (16, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Content-MD5"), "N16");
			Assert.AreEqual (17, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Content-Range"), "N17");
			Assert.AreEqual (18, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Expires"), "N18");
			Assert.AreEqual (19, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Last-Modified"), "N19");
			Assert.AreEqual (20, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Accept"), "N20");
			Assert.AreEqual (21, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Accept-Charset"), "N21");
			Assert.AreEqual (22, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Accept-Encoding"), "N22");
			Assert.AreEqual (23, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Accept-Language"), "N23");
			Assert.AreEqual (24, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Authorization"), "N24");
			Assert.AreEqual (25, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Cookie"), "N25");
			Assert.AreEqual (26, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Expect"), "N26");
			Assert.AreEqual (27, HttpWorkerRequest.GetKnownRequestHeaderIndex ("From"), "N27");
			Assert.AreEqual (28, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Host"), "N28");
			Assert.AreEqual (29, HttpWorkerRequest.GetKnownRequestHeaderIndex ("If-Match"), "N29");
			Assert.AreEqual (30, HttpWorkerRequest.GetKnownRequestHeaderIndex ("If-Modified-Since"), "N30");
			Assert.AreEqual (31, HttpWorkerRequest.GetKnownRequestHeaderIndex ("If-None-Match"), "N31");
			Assert.AreEqual (32, HttpWorkerRequest.GetKnownRequestHeaderIndex ("If-Range"), "N32");
			Assert.AreEqual (33, HttpWorkerRequest.GetKnownRequestHeaderIndex ("If-Unmodified-Since"), "N33");
			Assert.AreEqual (34, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Max-Forwards"), "N34");
			Assert.AreEqual (35, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Proxy-Authorization"), "N35");
			Assert.AreEqual (36, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Referer"), "N36");
			Assert.AreEqual (37, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Range"), "N37");
			Assert.AreEqual (38, HttpWorkerRequest.GetKnownRequestHeaderIndex ("TE"), "N38");
			Assert.AreEqual (39, HttpWorkerRequest.GetKnownRequestHeaderIndex ("User-Agent"), "N39");
			Assert.AreEqual (-1, HttpWorkerRequest.GetKnownRequestHeaderIndex ("Blablabla"), "N40");
		}
		
		[Test] public void Test_GetKnownResponseIndex ()
		{
			Assert.AreEqual (0, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Cache-Control"), "M0");
			Assert.AreEqual (1, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Connection"), "M1");
			Assert.AreEqual (2, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Date"), "M2");
			Assert.AreEqual (3, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Keep-Alive"), "M3");
			Assert.AreEqual (4, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Pragma"), "M4");
			Assert.AreEqual (5, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Trailer"), "M5");
			Assert.AreEqual (6, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Transfer-Encoding"), "M6");
			Assert.AreEqual (7, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Upgrade"), "M7");
			Assert.AreEqual (8, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Via"), "M8");
			Assert.AreEqual (9, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Warning"), "M9");
			Assert.AreEqual (10, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Allow"), "M10");
			Assert.AreEqual (11, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Content-Length"), "M11");
			Assert.AreEqual (12, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Content-Type"), "M12");
			Assert.AreEqual (13, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Content-Encoding"), "M13");
			Assert.AreEqual (14, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Content-Language"), "M14");
			Assert.AreEqual (15, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Content-Location"), "M15");
			Assert.AreEqual (16, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Content-MD5"), "M16");
			Assert.AreEqual (17, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Content-Range"), "M17");
			Assert.AreEqual (18, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Expires"), "M18");
			Assert.AreEqual (19, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Last-Modified"), "M19");
			Assert.AreEqual (20, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Accept-Ranges"), "M20");
			Assert.AreEqual (21, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Age"), "M21");
			Assert.AreEqual (22, HttpWorkerRequest.GetKnownResponseHeaderIndex ("ETag"), "M22");
			Assert.AreEqual (23, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Location"), "M23");
			Assert.AreEqual (24, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Proxy-Authenticate"), "M24");
			Assert.AreEqual (25, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Retry-After"), "M25");
			Assert.AreEqual (26, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Server"), "M26");
			Assert.AreEqual (27, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Set-Cookie"), "M27");
			Assert.AreEqual (28, HttpWorkerRequest.GetKnownResponseHeaderIndex ("Vary"), "M28");
			Assert.AreEqual (29, HttpWorkerRequest.GetKnownResponseHeaderIndex ("WWW-Authenticate"), "M29");
		}

		[Test] public void Test_GetKnownResponseName ()
		{
			Assert.AreEqual ("Cache-Control", HttpWorkerRequest.GetKnownResponseHeaderName (0), "P0");
			Assert.AreEqual ("Connection", HttpWorkerRequest.GetKnownResponseHeaderName (1), "P1");
			Assert.AreEqual ("Date", HttpWorkerRequest.GetKnownResponseHeaderName (2), "P2");
			Assert.AreEqual ("Keep-Alive", HttpWorkerRequest.GetKnownResponseHeaderName (3), "P3");
			Assert.AreEqual ("Pragma", HttpWorkerRequest.GetKnownResponseHeaderName (4), "P4");
			Assert.AreEqual ("Trailer", HttpWorkerRequest.GetKnownResponseHeaderName (5), "P5");
			Assert.AreEqual ("Transfer-Encoding", HttpWorkerRequest.GetKnownResponseHeaderName (6), "P6");
			Assert.AreEqual ("Upgrade", HttpWorkerRequest.GetKnownResponseHeaderName (7), "P7");
			Assert.AreEqual ("Via", HttpWorkerRequest.GetKnownResponseHeaderName (8), "P8");
			Assert.AreEqual ("Warning", HttpWorkerRequest.GetKnownResponseHeaderName (9), "P9");
			Assert.AreEqual ("Allow", HttpWorkerRequest.GetKnownResponseHeaderName (10), "P10");
			Assert.AreEqual ("Content-Length", HttpWorkerRequest.GetKnownResponseHeaderName (11), "P11");
			Assert.AreEqual ("Content-Type", HttpWorkerRequest.GetKnownResponseHeaderName (12), "P12");
			Assert.AreEqual ("Content-Encoding", HttpWorkerRequest.GetKnownResponseHeaderName (13), "P13");
			Assert.AreEqual ("Content-Language", HttpWorkerRequest.GetKnownResponseHeaderName (14), "P14");
			Assert.AreEqual ("Content-Location", HttpWorkerRequest.GetKnownResponseHeaderName (15), "P15");
			Assert.AreEqual ("Content-MD5", HttpWorkerRequest.GetKnownResponseHeaderName (16), "P16");
			Assert.AreEqual ("Content-Range", HttpWorkerRequest.GetKnownResponseHeaderName (17), "P17");
			Assert.AreEqual ("Expires", HttpWorkerRequest.GetKnownResponseHeaderName (18), "P18");
			Assert.AreEqual ("Last-Modified", HttpWorkerRequest.GetKnownResponseHeaderName (19), "P19");
			Assert.AreEqual ("Accept-Ranges", HttpWorkerRequest.GetKnownResponseHeaderName (20), "P20");
			Assert.AreEqual ("Age", HttpWorkerRequest.GetKnownResponseHeaderName (21), "P21");
			Assert.AreEqual ("ETag", HttpWorkerRequest.GetKnownResponseHeaderName (22), "P22");
			Assert.AreEqual ("Location", HttpWorkerRequest.GetKnownResponseHeaderName (23), "P23");
			Assert.AreEqual ("Proxy-Authenticate", HttpWorkerRequest.GetKnownResponseHeaderName (24), "P24");
			Assert.AreEqual ("Retry-After", HttpWorkerRequest.GetKnownResponseHeaderName (25), "P25");
			Assert.AreEqual ("Server", HttpWorkerRequest.GetKnownResponseHeaderName (26), "P26");
			Assert.AreEqual ("Set-Cookie", HttpWorkerRequest.GetKnownResponseHeaderName (27), "P27");
			Assert.AreEqual ("Vary", HttpWorkerRequest.GetKnownResponseHeaderName (28), "P28");
			Assert.AreEqual ("WWW-Authenticate", HttpWorkerRequest.GetKnownResponseHeaderName (29), "P29");
		}

		[ExpectedException (typeof (IndexOutOfRangeException))]
		[Test] public void Test_OutOfRangeHeaderResponseName ()
		{
			HttpWorkerRequest.GetKnownResponseHeaderName (HttpWorkerRequest.ResponseHeaderMaximum);
		}

		[ExpectedException (typeof (IndexOutOfRangeException))]
		[Test] public void Test_OutOfRangeResponseName2 ()
		{
			HttpWorkerRequest.GetKnownRequestHeaderName (-1);
		}

		[Test] public void Test_GetStatusDescription ()
		{
			Console.WriteLine ("*****" + HttpWorkerRequest.GetStatusDescription (424));
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (100) != "", "D1");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (101) != "", "D2");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (102) != "", "D3");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (200) != "", "D4");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (201) != "", "D5");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (202) != "", "D6");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (203) != "", "D7");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (204) != "", "D8");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (205) != "", "D9");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (206) != "", "D10");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (207) != "", "D11");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (300) != "", "D12");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (301) != "", "D13");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (302) != "", "D14");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (303) != "", "D15");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (304) != "", "D16");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (305) != "", "D17");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (307) != "", "D18");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (400) != "", "D19");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (401) != "", "D20");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (402) != "", "D21");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (403) != "", "D22");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (404) != "", "D23");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (405) != "", "D24");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (406) != "", "D25");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (407) != "", "D26");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (408) != "", "D27");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (409) != "", "D28");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (410) != "", "D29");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (411) != "", "D30");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (412) != "", "D31");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (413) != "", "D32");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (414) != "", "D33");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (415) != "", "D34");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (416) != "", "D35");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (417) != "", "D36");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (422) != "", "D37");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (423) != "", "D38");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (424) != "", "D39");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (500) != "", "D40");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (501) != "", "D41");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (502) != "", "D42");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (503) != "", "D43");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (504) != "", "D44");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (505) != "", "D45");
			Assert.AreEqual (true, HttpWorkerRequest.GetStatusDescription (507) != "", "D46");
		}
	}
}
