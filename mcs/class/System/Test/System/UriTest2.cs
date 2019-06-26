//
// UriTest2.cs - More NUnit Test Cases for System.Uri
//

using System;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System
{
	// help bring Moonlight tests back to mono/mcs nunit

	public delegate void TestCode ();

	static class Assert2 {

		public static void Throws<TException> (TestCode code, string message) where TException : Exception
		{
			Throws (code, typeof (TException), null, message);
		}

		public static void Throws (TestCode code, Type expected_exception, Type expected_inner_exception, string message)
		{
			bool failed = false;
			try {
				code ();
				failed = true;
			}
			catch (Exception ex) {
				if (!(ex.GetType () == expected_exception))
					throw new AssertionException (string.Format ("Expected '{0}', got '{1}'. {2}", expected_exception.FullName, ex.GetType ().FullName, message));
				//System.Diagnostics.Debug.WriteLine (ex.ToString ());
				if (expected_inner_exception != null) {
					// we only check if the inner exception was supplied
					if (ex.InnerException.GetType () != expected_inner_exception)
						throw new AssertionException (string.Format ("Expected InnerException '{0}', got '{1}'. {2}", expected_inner_exception.FullName, ex.InnerException.GetType ().FullName, message));
				}
			}
			if (failed)
				throw new AssertionException (string.Format ("Expected '{0}', but got no exception. {1}", expected_exception.FullName, message));
		}
	}

	[TestFixture]
	public class UriTest2
	{
		protected bool isWin32 = false;
		public bool IriParsing;
		
		[SetUp]
		public void SetUp ()
		{
			isWin32 = (Path.DirectorySeparatorChar == '\\');

			//Make sure Uri static constructor is called
			Uri.EscapeDataString ("");

			FieldInfo iriParsingField = typeof (Uri).GetField ("s_IriParsing",
				BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic);
			if (iriParsingField != null)
				IriParsing = (bool)iriParsingField.GetValue (null);
		}

		// Segments cannot be validated here...
		public void AssertUri (string relsrc, Uri uri,
			string toString,
			string absoluteUri,
			string scheme,
			string host,
			string localPath,
			string query,
			int port,
			bool isFile,
			bool isUnc,
			bool isLoopback,
			bool userEscaped,
			UriHostNameType hostNameType,
			string absolutePath,
			string pathAndQuery,
			string authority,
			string fragment,
			string userInfo)
		{
			Assert.AreEqual (absoluteUri, uri.AbsoluteUri, relsrc + " AbsoluteUri");
			Assert.AreEqual (scheme, uri.Scheme, relsrc + " Scheme");
			Assert.AreEqual (host, uri.Host, relsrc + " Host");
			Assert.AreEqual (port, uri.Port, relsrc + " Port");
			// Windows UNC path is not automatically testable on *nix environment,
			if (relsrc.StartsWith ("\\\\") && Path.DirectorySeparatorChar == '\\')
				Assert.AreEqual (localPath, uri.LocalPath, relsrc + " LocalPath");
			Assert.AreEqual (query, uri.Query, relsrc + " Query");
			Assert.AreEqual (fragment, uri.Fragment, relsrc + " Fragment");
			Assert.AreEqual (isFile, uri.IsFile, relsrc + " IsFile");
			Assert.AreEqual (isUnc, uri.IsUnc, relsrc + " IsUnc");
			Assert.AreEqual (isLoopback, uri.IsLoopback, relsrc + " IsLoopback");
			Assert.AreEqual (authority, uri.Authority, relsrc + " Authority");
			Assert.AreEqual (userEscaped, uri.UserEscaped, relsrc + " UserEscaped");
			Assert.AreEqual (userInfo, uri.UserInfo, relsrc + " UserInfo");
			Assert.AreEqual (hostNameType, uri.HostNameType, relsrc + " HostNameType");
			Assert.AreEqual (absolutePath, uri.AbsolutePath, relsrc + " AbsolutePath");
			Assert.AreEqual (pathAndQuery, uri.PathAndQuery, relsrc + " PathAndQuery");
			Assert.AreEqual (toString, uri.ToString (), relsrc + " ToString()");
		}

		[Test]
		[Ignore ("Tests needs to be updated for 2.0")]
		public void AbsoluteUriFromFile ()
		{
			FromResource ("Test/System/test-uri-props.txt", null);
		}
		
		[Test]
		[Category("NotDotNet")]
		[Ignore ("Tests needs to be updated for 2.0")]
		public void AbsoluteUriFromFileManual ()
		{
			if (Path.DirectorySeparatorChar == '\\')
				return;
			FromResource ("Test/System/test-uri-props-manual.txt", null);
		}
		
		[Test]
		[Ignore ("Tests needs to be updated for 2.0")]
		public void RelativeUriFromFile ()
		{
			FromResource ("Test/System/test-uri-relative-props.txt", new Uri ("http://www.example.com"));
		}
		
		private void FromResource (string res, Uri baseUri)
		{
			StreamReader sr = new StreamReader (TestResourceHelper.GetFullPathOfResource (res), Encoding.UTF8);
			while (sr.Peek () > 0) {
				sr.ReadLine (); // skip
				string uriString = sr.ReadLine ();
/*
TextWriter sw = Console.Out;
				sw.WriteLine ("-------------------------");
				sw.WriteLine (uriString);
*/
				if (uriString == null || uriString.Length == 0)
					break;

				try {
					Uri uri = baseUri == null ? new Uri (uriString) : new Uri (baseUri, uriString);
/*
				sw.WriteLine ("ToString(): " + uri.ToString ());
				sw.WriteLine (uri.AbsoluteUri);
				sw.WriteLine (uri.Scheme);
				sw.WriteLine (uri.Host);
				sw.WriteLine (uri.LocalPath);
				sw.WriteLine (uri.Query);
				sw.WriteLine ("Port: " + uri.Port);
				sw.WriteLine (uri.IsFile);
				sw.WriteLine (uri.IsUnc);
				sw.WriteLine (uri.IsLoopback);
				sw.WriteLine (uri.UserEscaped);
				sw.WriteLine ("HostNameType: " + uri.HostNameType);
				sw.WriteLine (uri.AbsolutePath);
				sw.WriteLine ("PathAndQuery: " + uri.PathAndQuery);
				sw.WriteLine (uri.Authority);
				sw.WriteLine (uri.Fragment);
				sw.WriteLine (uri.UserInfo);
*/
					AssertUri (uriString, uri,
						sr.ReadLine (),
						sr.ReadLine (),
						sr.ReadLine (),
						sr.ReadLine (),
						sr.ReadLine (),
						sr.ReadLine (),
						int.Parse (sr.ReadLine ()),
						bool.Parse (sr.ReadLine ()),
						bool.Parse (sr.ReadLine ()),
						bool.Parse (sr.ReadLine ()),
						bool.Parse (sr.ReadLine ()),
						(UriHostNameType) Enum.Parse (typeof (UriHostNameType), sr.ReadLine (), false),
						sr.ReadLine (),
						sr.ReadLine (),
						sr.ReadLine (),
						sr.ReadLine (),
						sr.ReadLine ());
//				Console.WriteLine ("Passed: " + uriString);
				} catch (UriFormatException ex) {
					Assert.Fail (String.Format ("Construction failed: [{0}] {1}", uriString, ex.Message));
				}
			}
		}

		[Test]
		public void MoreUnescape () // bug 733316
		{
			int index = 0;
			char unesc = Uri.HexUnescape ("%F6", ref index);
			Assert.AreEqual (3, index, "#01");
			Assert.AreEqual (0xf6, unesc, "#02");
		}

		[Test]
		public void UriScheme ()
		{
			Assert.AreEqual ("://", Uri.SchemeDelimiter, "://");
			Assert.AreEqual ("file", Uri.UriSchemeFile, "file");
			Assert.AreEqual ("ftp", Uri.UriSchemeFtp, "ftp");
			Assert.AreEqual ("gopher", Uri.UriSchemeGopher, "gopher");
			Assert.AreEqual ("http", Uri.UriSchemeHttp, "http");
			Assert.AreEqual ("https", Uri.UriSchemeHttps, "https");
			Assert.AreEqual ("mailto", Uri.UriSchemeMailto, "mailto");
			Assert.AreEqual ("news", Uri.UriSchemeNews, "news");
			Assert.AreEqual ("nntp", Uri.UriSchemeNntp, "file");
			Assert.AreEqual ("net.pipe", Uri.UriSchemeNetPipe, "net.pipe");
			Assert.AreEqual ("net.tcp", Uri.UriSchemeNetTcp, "net.tcp");
		}

		[Test] // bug #71049
		[ExpectedException (typeof (UriFormatException))]
		public void StarsInHost ()
		{
			new Uri ("http://w*w*w.example.com");
		}

		[Test]
		public void NoHostName1_Bug76146 ()
		{
			Uri u = new Uri ("foo:///?bar");
			Assert.AreEqual ("/", u.AbsolutePath, "AbsolutePath");
			Assert.AreEqual ("foo:///?bar", u.AbsoluteUri, "AbsoluteUri");
			Assert.AreEqual (String.Empty, u.Authority, "Authority");
			Assert.AreEqual (String.Empty, u.Fragment, "Fragment");
			Assert.AreEqual (String.Empty, u.Host, "Host");
			// FIXME (2.0) - Normally this is never Basic without an Host name :(
			// Assert.AreEqual (UriHostNameType.Basic, u.HostNameType, "HostNameType");
			Assert.IsTrue (u.IsDefaultPort, "IsDefaultPort");
			Assert.IsFalse (u.IsFile, "IsFile");
			// FIXME Assert.IsTrue (u.IsLoopback, "IsLoopback");
			Assert.IsFalse (u.IsUnc, "IsUnc");
			Assert.AreEqual ("/", u.LocalPath, "LocalPath");
			Assert.AreEqual ("/?bar", u.PathAndQuery, "PathAndQuery");
			Assert.AreEqual ("foo", u.Scheme, "Scheme");
			Assert.IsFalse (u.UserEscaped, "UserEscaped");
			Assert.AreEqual (String.Empty, u.UserInfo, "UserInfo");
		}

		[Test]
		public void NoHostName2_Bug76146 ()
		{
			Uri u = new Uri ("foo:///bar");
			Assert.AreEqual ("/bar", u.AbsolutePath, "AbsolutePath");
			Assert.AreEqual ("foo:///bar", u.AbsoluteUri, "AbsoluteUri");
			Assert.AreEqual (String.Empty, u.Authority, "Authority");
			Assert.AreEqual (String.Empty, u.Fragment, "Fragment");
			Assert.AreEqual (String.Empty, u.Host, "Host");
			// FIXME (2.0) - Normally this is never Basic without an Host name :(
			// Assert.AreEqual (UriHostNameType.Basic, u.HostNameType, "HostNameType");
			Assert.IsTrue (u.IsDefaultPort, "IsDefaultPort");
			Assert.IsFalse (u.IsFile, "IsFile");
			// FIXME Assert.IsTrue (u.IsLoopback, "IsLoopback");
			Assert.IsFalse (u.IsUnc, "IsUnc");
			Assert.AreEqual ("/bar", u.LocalPath, "LocalPath");
			Assert.AreEqual ("/bar", u.PathAndQuery, "PathAndQuery");
			Assert.AreEqual ("foo", u.Scheme, "Scheme");
			Assert.IsFalse (u.UserEscaped, "UserEscaped");
			Assert.AreEqual (String.Empty, u.UserInfo, "UserInfo");
		}

		[Test]
		[ExpectedException (typeof (UriFormatException))]
		public void InvalidIPAddress_Bug76659 ()
		{
			new Uri ("http://127.0.0.1::::/");
		}

		[Test]
		public void File ()
		{
			string s = "file:///dir1%2f..%2fdir%2fapp.xap#header";
			Uri uri = new Uri (s);
			Assert.AreEqual ("/dir/app.xap", uri.AbsolutePath, "AbsolutePath");
			// default port is removed
			Assert.AreEqual ("file:///dir/app.xap#header", uri.AbsoluteUri, "AbsoluteUri");
			Assert.AreEqual (String.Empty, uri.DnsSafeHost, "DnsSafeHost");
			Assert.AreEqual ("#header", uri.Fragment, "Fragment");
			Assert.AreEqual (String.Empty, uri.Host, "Host");
			Assert.IsTrue (uri.IsAbsoluteUri, "IsAbsoluteUri");
			Assert.IsFalse (uri.IsUnc, "IsUnc");
			Assert.AreEqual ("/dir/app.xap", uri.LocalPath, "LocalPath");
			Assert.AreEqual (s, uri.OriginalString, "OriginalString");
			Assert.AreEqual (-1, uri.Port, "Port");
			Assert.AreEqual (String.Empty, uri.Query, "Query");
			Assert.AreEqual ("file", uri.Scheme, "Scheme");
			Assert.IsFalse (uri.UserEscaped, "UserEscaped");
			Assert.AreEqual (String.Empty, uri.UserInfo, "UserInfo");
			Assert.AreEqual (uri.AbsoluteUri, uri.ToString (), "ToString");
		}

		[Test]
		public void HttpWithDefaultPort ()
		{
			string s = "HTTP://host.domain.com:80/app.xap";
			Uri uri = new Uri (s);
			Assert.AreEqual ("/app.xap", uri.AbsolutePath, "AbsolutePath");
			// default port is removed
			Assert.AreEqual ("http://host.domain.com/app.xap", uri.AbsoluteUri, "AbsoluteUri");
			Assert.AreEqual ("host.domain.com", uri.DnsSafeHost, "DnsSafeHost");
			Assert.AreEqual (String.Empty, uri.Fragment, "Fragment");
			Assert.AreEqual ("host.domain.com", uri.Host, "Host");
			Assert.IsTrue (uri.IsAbsoluteUri, "IsAbsoluteUri");
			Assert.IsFalse (uri.IsUnc, "IsUnc");
			Assert.AreEqual ("/app.xap", uri.LocalPath, "LocalPath");
			Assert.AreEqual (s, uri.OriginalString, "OriginalString");
			Assert.AreEqual (80, uri.Port, "Port");
			Assert.AreEqual (String.Empty, uri.Query, "Query");
			Assert.AreEqual ("http", uri.Scheme, "Scheme");
			Assert.IsFalse (uri.UserEscaped, "UserEscaped");
			Assert.AreEqual (String.Empty, uri.UserInfo, "UserInfo");
			Assert.AreEqual (uri.AbsoluteUri, uri.ToString (), "ToString");
		}

		[Test]
		public void HttpWithoutPort ()
		{
			string s = "Http://host.DOMAIN.com/dir/app.xap#options";
			Uri uri = new Uri (s);
			Assert.AreEqual ("/dir/app.xap", uri.AbsolutePath, "AbsolutePath");
			Assert.AreEqual ("http://host.domain.com/dir/app.xap#options", uri.AbsoluteUri, "AbsoluteUri");
			Assert.AreEqual ("host.domain.com", uri.DnsSafeHost, "DnsSafeHost");
			Assert.AreEqual ("#options", uri.Fragment, "Fragment");
			Assert.AreEqual ("host.domain.com", uri.Host, "Host");
			Assert.IsTrue (uri.IsAbsoluteUri, "IsAbsoluteUri");
			Assert.IsFalse (uri.IsUnc, "IsUnc");
			Assert.AreEqual ("/dir/app.xap", uri.LocalPath, "LocalPath");
			Assert.AreEqual (s, uri.OriginalString, "OriginalString");
			Assert.AreEqual (80, uri.Port, "Port");
			Assert.AreEqual (String.Empty, uri.Query, "Query");
			Assert.AreEqual ("http", uri.Scheme, "Scheme");
			Assert.IsFalse (uri.UserEscaped, "UserEscaped");
			Assert.AreEqual (String.Empty, uri.UserInfo, "UserInfo");
			Assert.AreEqual (uri.AbsoluteUri, uri.ToString (), "ToString");
		}

		[Test]
		public void HttpWithNonStandardPort ()
		{
			string s = "http://monkey:s3kr3t@HOST.domain.Com:8080/dir/../app.xap?option=1";
			Uri uri = new Uri (s);
			Assert.AreEqual ("/app.xap", uri.AbsolutePath, "AbsolutePath");
			// non-standard port is present
			Assert.AreEqual ("http://monkey:s3kr3t@host.domain.com:8080/app.xap?option=1", uri.AbsoluteUri, "AbsoluteUri");
			Assert.AreEqual ("host.domain.com", uri.DnsSafeHost, "DnsSafeHost");
			Assert.AreEqual (String.Empty, uri.Fragment, "Fragment");
			Assert.AreEqual ("host.domain.com", uri.Host, "Host");
			Assert.IsTrue (uri.IsAbsoluteUri, "IsAbsoluteUri");
			Assert.IsFalse (uri.IsUnc, "IsUnc");
			Assert.AreEqual ("/app.xap", uri.LocalPath, "LocalPath");
			Assert.AreEqual (s, uri.OriginalString, "OriginalString");
			Assert.AreEqual (8080, uri.Port, "Port");
			Assert.AreEqual ("?option=1", uri.Query, "Query");
			Assert.AreEqual ("http", uri.Scheme, "Scheme");
			Assert.IsFalse (uri.UserEscaped, "UserEscaped");
			Assert.AreEqual ("monkey:s3kr3t", uri.UserInfo, "UserInfo");
			Assert.AreEqual (uri.AbsoluteUri, uri.ToString (), "ToString");
		}

		[Test]
		public void HttpsWithDefaultPort ()
		{
			string s = "httpS://host.domain.com:443/";
			Uri uri = new Uri (s);
			Assert.AreEqual ("/", uri.AbsolutePath, "AbsolutePath");
			// default port is removed
			Assert.AreEqual ("https://host.domain.com/", uri.AbsoluteUri, "AbsoluteUri");
			Assert.AreEqual ("host.domain.com", uri.DnsSafeHost, "DnsSafeHost");
			Assert.AreEqual (String.Empty, uri.Fragment, "Fragment");
			Assert.AreEqual ("host.domain.com", uri.Host, "Host");
			Assert.IsTrue (uri.IsAbsoluteUri, "IsAbsoluteUri");
			Assert.IsFalse (uri.IsUnc, "IsUnc");
			Assert.AreEqual ("/", uri.LocalPath, "LocalPath");
			Assert.AreEqual (s, uri.OriginalString, "OriginalString");
			Assert.AreEqual (443, uri.Port, "Port");
			Assert.AreEqual (String.Empty, uri.Query, "Query");
			Assert.AreEqual ("https", uri.Scheme, "Scheme");
			Assert.IsFalse (uri.UserEscaped, "UserEscaped");
			Assert.AreEqual (String.Empty, uri.UserInfo, "UserInfo");
			Assert.AreEqual (uri.AbsoluteUri, uri.ToString (), "ToString");
		}

		[Test]
		public void HttpsWithoutPort ()
		{
			string s = "Https://host.DOMAIN.com/dir%2fapp.xap#";
			Uri uri = new Uri (s);

			if (IriParsing)	{
				Assert.AreEqual ("/dir%2fapp.xap", uri.AbsolutePath, "AbsolutePath");
				Assert.AreEqual ("https://host.domain.com/dir%2fapp.xap#", uri.AbsoluteUri, "AbsoluteUri");
			} else {
				Assert.AreEqual ("/dir/app.xap", uri.AbsolutePath, "AbsolutePath");
				Assert.AreEqual ("https://host.domain.com/dir/app.xap#", uri.AbsoluteUri, "AbsoluteUri");
			}

			Assert.AreEqual ("host.domain.com", uri.DnsSafeHost, "DnsSafeHost");
			Assert.AreEqual ("#", uri.Fragment, "Fragment");
			Assert.AreEqual ("host.domain.com", uri.Host, "Host");
			Assert.IsTrue (uri.IsAbsoluteUri, "IsAbsoluteUri");
			Assert.IsFalse (uri.IsUnc, "IsUnc");
			Assert.AreEqual ("/dir/app.xap", uri.LocalPath, "LocalPath");
			Assert.AreEqual (s, uri.OriginalString, "OriginalString");
			Assert.AreEqual (443, uri.Port, "Port");
			Assert.AreEqual (String.Empty, uri.Query, "Query");
			Assert.AreEqual ("https", uri.Scheme, "Scheme");
			Assert.IsFalse (uri.UserEscaped, "UserEscaped");
			Assert.AreEqual (String.Empty, uri.UserInfo, "UserInfo");
			Assert.AreEqual (uri.AbsoluteUri, uri.ToString (), "ToString");
		}

		[Test]
		public void HttpsWithNonStandardPort ()
		{
			string s = "https://monkey:s3kr3t@HOST.domain.Com:4430/dir/..%5Capp.xap?";
			Uri uri = new Uri (s);

			if (IriParsing) {
				Assert.AreEqual ("/dir/..%5Capp.xap", uri.AbsolutePath, "AbsolutePath");
				Assert.AreEqual ("https://monkey:s3kr3t@host.domain.com:4430/dir/..%5Capp.xap?", uri.AbsoluteUri, "AbsoluteUri");
				Assert.AreEqual ("/dir/..\\app.xap", uri.LocalPath, "LocalPath");
			} else {
				Assert.AreEqual ("/app.xap", uri.AbsolutePath, "AbsolutePath");
				Assert.AreEqual ("https://monkey:s3kr3t@host.domain.com:4430/app.xap?", uri.AbsoluteUri, "AbsoluteUri");
				Assert.AreEqual ("/app.xap", uri.LocalPath, "LocalPath");
			}

			Assert.AreEqual ("host.domain.com", uri.DnsSafeHost, "DnsSafeHost");
			Assert.AreEqual (String.Empty, uri.Fragment, "Fragment");
			Assert.AreEqual ("host.domain.com", uri.Host, "Host");
			Assert.IsTrue (uri.IsAbsoluteUri, "IsAbsoluteUri");
			Assert.IsFalse (uri.IsUnc, "IsUnc");
			Assert.AreEqual (s, uri.OriginalString, "OriginalString");
			Assert.AreEqual (4430, uri.Port, "Port");
			Assert.AreEqual ("?", uri.Query, "Query");
			Assert.AreEqual ("https", uri.Scheme, "Scheme");
			Assert.IsFalse (uri.UserEscaped, "UserEscaped");
			Assert.AreEqual ("monkey:s3kr3t", uri.UserInfo, "UserInfo");
			Assert.AreEqual (uri.AbsoluteUri, uri.ToString (), "ToString");
		}

		[Test]
		public void Relative ()
		{
			Uri relative = new Uri ("/Moonlight", UriKind.Relative);

			Assert2.Throws<ArgumentNullException> (delegate {
				new Uri (null, "/Moonlight");
			}, "null,string");
			Assert2.Throws<ArgumentNullException> (delegate {
				new Uri (null, relative);
			}, "null,Uri");

			Assert2.Throws<ArgumentOutOfRangeException> (delegate {
				new Uri (relative, "/Moonlight");
			}, "Uri,string");
			Assert2.Throws<ArgumentOutOfRangeException> (delegate {
				new Uri (relative, relative);
			}, "Uri,Uri");

			Assert2.Throws<ArgumentOutOfRangeException> (delegate {
				new Uri (relative, (string) null);
			}, "Uri,string-null");
			Assert2.Throws<ArgumentOutOfRangeException> (delegate {
				new Uri (relative, (Uri) null);
			}, "Uri,Uri-null");
		}

		private void CheckRelativeUri (Uri uri)
		{
			Assert2.Throws<InvalidOperationException> (delegate {
				Assert.IsNotNull (uri.AbsolutePath);
			}, "AbsolutePath");
			Assert2.Throws<InvalidOperationException> (delegate {
				Assert.IsNotNull (uri.AbsoluteUri);
			}, "AbsoluteUri");
			Assert2.Throws<InvalidOperationException> (delegate {
				Assert.IsNotNull (uri.DnsSafeHost);
			}, "DnsSafeHost");
			Assert2.Throws<InvalidOperationException> (delegate {
				Assert.IsNotNull (uri.Fragment);
			}, "Fragment");
			Assert2.Throws<InvalidOperationException> (delegate {
				Assert.IsNotNull (uri.Host);
			}, "Host");

			Assert.IsFalse (uri.IsAbsoluteUri, "IsAbsoluteUri");

			Assert2.Throws<InvalidOperationException> (delegate {
				Assert.IsNotNull (uri.IsUnc);
			}, "IsUnc");
			Assert2.Throws<InvalidOperationException> (delegate {
				Assert.IsNotNull (uri.LocalPath);
			}, "LocalPath");

			Assert.AreEqual ("/Moonlight", uri.OriginalString, "OriginalString");

			Assert2.Throws<InvalidOperationException> (delegate {
				Assert.IsNotNull (uri.Port);
			}, "Port");
			Assert2.Throws<InvalidOperationException> (delegate {
				Assert.IsNotNull (uri.Query);
			}, "Query");
			Assert2.Throws<InvalidOperationException> (delegate {
				Assert.IsNotNull (uri.Scheme);
			}, "Scheme");

			Assert.IsFalse (uri.UserEscaped, "UserEscaped");

			Assert2.Throws<InvalidOperationException> (delegate {
				Assert.IsNotNull (uri.UserInfo);
			}, "UserInfo");

			Assert.AreEqual ("/Moonlight", uri.ToString (), "ToString");
		}

		[Test]
		public void Relative_AsRelative ()
		{
			Uri uri = new Uri ("/Moonlight", UriKind.Relative);
			CheckRelativeUri (uri);
		}

		[Test]
		public void Bug496783 ()
		{
			string s = "tcp://csve2.csse.unimelb.edu.au:9090/Aneka";
			Uri uri = new Uri (s);
			// this is not parsed by a known UriParser
			Assert.IsFalse (UriParser.IsKnownScheme (uri.Scheme), "UriParser");

			Uri uri2 = new Uri ("tcp://csve2.csse.unimelb.edu.au:9090/");
			Assert.IsTrue (uri2.IsBaseOf (uri), "IsBaseOf");

			Assert.AreEqual (uri.AbsoluteUri, uri.GetComponents (UriComponents.AbsoluteUri, UriFormat.Unescaped), "AbsoluteUri");
			Assert.AreEqual (uri.Fragment, uri.GetComponents (UriComponents.Fragment, UriFormat.Unescaped), "Fragment");
			Assert.AreEqual (uri.Host, uri.GetComponents (UriComponents.Host, UriFormat.Unescaped), "Host");
			Assert.AreEqual (uri.Authority, uri.GetComponents (UriComponents.HostAndPort, UriFormat.Unescaped), "HostAndPort");
			Assert.AreEqual (uri.AbsoluteUri, uri.GetComponents (UriComponents.HttpRequestUrl, UriFormat.Unescaped), "HttpRequestUrl");
			Assert.AreEqual (String.Empty, uri.GetComponents (UriComponents.KeepDelimiter, UriFormat.Unescaped), "KeepDelimiter");
			Assert.AreEqual ("Aneka", uri.GetComponents (UriComponents.Path, UriFormat.Unescaped), "Path");
			Assert.AreEqual (uri.LocalPath, uri.GetComponents (UriComponents.PathAndQuery, UriFormat.Unescaped), "PathAndQuery");
			Assert.AreEqual (uri.Port.ToString (), uri.GetComponents (UriComponents.Port, UriFormat.Unescaped), "Port");
			Assert.AreEqual (uri.Query, uri.GetComponents (UriComponents.Query, UriFormat.Unescaped), "Query");
			Assert.AreEqual (uri.Scheme, uri.GetComponents (UriComponents.Scheme, UriFormat.Unescaped), "Scheme");
			Assert.AreEqual ("tcp://csve2.csse.unimelb.edu.au:9090", uri.GetComponents (UriComponents.SchemeAndServer, UriFormat.Unescaped), "SchemeAndServer");
			Assert.AreEqual (uri.OriginalString, uri.GetComponents (UriComponents.SerializationInfoString, UriFormat.Unescaped), "SerializationInfoString");
			Assert.AreEqual (uri.Authority, uri.GetComponents (UriComponents.StrongAuthority, UriFormat.Unescaped), "StrongAuthority");
			Assert.AreEqual (uri.Port.ToString (), uri.GetComponents (UriComponents.StrongPort, UriFormat.Unescaped), "StrongPort");
			Assert.AreEqual (uri.UserInfo, uri.GetComponents (UriComponents.UserInfo, UriFormat.Unescaped), "UserInfo");
		}

		[Test]
		public void Merge_Query_Fragment ()
		{
			Uri absolute = new Uri ("http://host/dir/subdir/weird;name?moonlight");
			Assert.AreEqual ("?moonlight", absolute.Query, "absolute.Query");

			Uri merged = new Uri (absolute, "#mono");
			Assert.AreEqual ("#mono", merged.Fragment, "merged.Fragment");
			Assert.AreEqual ("?moonlight", merged.Query, "merged.Query");
			Assert.AreEqual ("http://host/dir/subdir/weird;name?moonlight#mono", merged.ToString (), "merged.ToString");
		}

		[Test]
		public void Merge_Query_Query ()
		{
			Uri absolute = new Uri ("http://host/dir/subdir/weird;name?moonlight");
			Assert.AreEqual ("?moonlight", absolute.Query, "absolute.Query");

			Uri merged = new Uri (absolute, "?moon");
			Assert.AreEqual ("?moon", merged.Query, "merged.Query");
			Assert.AreEqual ("http://host/dir/subdir/weird;name?moon", merged.ToString (), "merged.ToString");
		}

		[Test]
		public void Merge_Query_RelativePath ()
		{
			Uri absolute = new Uri ("http://host/dir/subdir/weird;name?moonlight");
			Assert.AreEqual ("?moonlight", absolute.Query, "absolute.Query");

			Uri merged = new Uri (absolute, "../");
			Assert.AreEqual (String.Empty, merged.Query, "../Query");
			Assert.AreEqual ("http://host/dir/", merged.ToString (), "../ToString");

			merged = new Uri (absolute, "..");
			Assert.AreEqual (String.Empty, merged.Query, "..Query");
			Assert.AreEqual ("http://host/dir/", merged.ToString (), "..ToString");

			merged = new Uri (absolute, "./");
			Assert.AreEqual (String.Empty, merged.Query, "./Query");
			Assert.AreEqual ("http://host/dir/subdir/", merged.ToString (), "./ToString");

			merged = new Uri (absolute, ".");
			Assert.AreEqual (String.Empty, merged.Query, ".Query");
			Assert.AreEqual ("http://host/dir/subdir/", merged.ToString (), ".ToString");

			merged = new Uri (absolute, "/");
			Assert.AreEqual (String.Empty, merged.Query, "/Query");
			Assert.AreEqual ("http://host/", merged.ToString (), "/ToString");

			merged = new Uri (absolute, "index.html");
			Assert.AreEqual (String.Empty, merged.Query, "index.html Query");
			Assert.AreEqual ("http://host/dir/subdir/index.html", merged.ToString (), "index.html ToString");

			merged = new Uri (absolute, "i");
			Assert.AreEqual (String.Empty, merged.Query, "i Query");
			Assert.AreEqual ("http://host/dir/subdir/i", merged.ToString (), "i ToString");

			merged = new Uri (absolute, String.Empty);
			Assert.AreEqual ("?moonlight", merged.Query, "Query");
			Assert.AreEqual ("http://host/dir/subdir/weird;name?moonlight", merged.ToString (), "ToString");
		}

		[Test]
		public void Merge_Fragment_RelativePath ()
		{
			Uri absolute = new Uri ("http://host/dir/subdir/weird;name#mono");
			Assert.AreEqual ("#mono", absolute.Fragment, "absolute.Fragment");

			Uri merged = new Uri (absolute, "../");
			Assert.AreEqual (String.Empty, merged.Fragment, "../Fragment");
			Assert.AreEqual ("http://host/dir/", merged.ToString (), "../ToString");

			merged = new Uri (absolute, "..");
			Assert.AreEqual (String.Empty, merged.Fragment, "..Fragment");
			Assert.AreEqual ("http://host/dir/", merged.ToString (), "..ToString");

			merged = new Uri (absolute, "./");
			Assert.AreEqual (String.Empty, merged.Fragment, "./Fragment");
			Assert.AreEqual ("http://host/dir/subdir/", merged.ToString (), "./ToString");

			merged = new Uri (absolute, ".");
			Assert.AreEqual (String.Empty, merged.Fragment, ".Fragment");
			Assert.AreEqual ("http://host/dir/subdir/", merged.ToString (), ".ToString");

			merged = new Uri (absolute, "/");
			Assert.AreEqual (String.Empty, merged.Fragment, "/Fragment");
			Assert.AreEqual ("http://host/", merged.ToString (), "/ToString");

			merged = new Uri (absolute, "index.html");
			Assert.AreEqual (String.Empty, merged.Fragment, "index.html Fragment");
			Assert.AreEqual ("http://host/dir/subdir/index.html", merged.ToString (), "index.html ToString");

			merged = new Uri (absolute, "i");
			Assert.AreEqual (String.Empty, merged.Fragment, "i Fragment");
			Assert.AreEqual ("http://host/dir/subdir/i", merged.ToString (), "i ToString");

			merged = new Uri (absolute, String.Empty);
			Assert.AreEqual ("#mono", merged.Fragment, "Fragment");
			Assert.AreEqual ("http://host/dir/subdir/weird;name#mono", merged.ToString (), "ToString");
		}

		[Test]
		public void Host_Drive ()
		{
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName ("c:"), "c:");
			Assert.AreEqual (UriHostNameType.Dns, Uri.CheckHostName ("c"), "c");

			Uri uri = new Uri ("http://c:/dir/subdir/file");
			Assert.AreEqual ("c", uri.Authority, "http.Authority");
			Assert.AreEqual ("c", uri.DnsSafeHost, "http.DnsSafeHost");
			Assert.AreEqual ("c", uri.Host, "http.Host");
			Assert.AreEqual (UriHostNameType.Dns, uri.HostNameType, "http.HostNameType");
			Assert.AreEqual ("http://c/dir/subdir/file", uri.ToString (), "http.ToString");

			uri = new Uri ("https://c:/dir/subdir/file");
			Assert.AreEqual ("c", uri.Authority, "https.Authority");
			Assert.AreEqual ("c", uri.DnsSafeHost, "https.DnsSafeHost");
			Assert.AreEqual ("c", uri.Host, "https.Host");
			Assert.AreEqual (UriHostNameType.Dns, uri.HostNameType, "https.HostNameType");
			Assert.AreEqual ("https://c/dir/subdir/file", uri.ToString (), "https.ToString");

			uri = new Uri ("ftp://c:/dir/subdir/file");
			Assert.AreEqual ("c", uri.Authority, "ftp.Authority");
			Assert.AreEqual ("c", uri.DnsSafeHost, "ftp.DnsSafeHost");
			Assert.AreEqual ("c", uri.Host, "ftp.Host");
			Assert.AreEqual (UriHostNameType.Dns, uri.HostNameType, "ftp.HostNameType");
			Assert.AreEqual ("ftp://c/dir/subdir/file", uri.ToString (), "ftp.ToString");

			uri = new Uri ("nntp://c:/123456@c");
			Assert.AreEqual ("c", uri.Authority, "nntp.Authority");
			Assert.AreEqual ("c", uri.DnsSafeHost, "nntp.DnsSafeHost");
			Assert.AreEqual ("c", uri.Host, "nntp.Host");
			Assert.AreEqual (UriHostNameType.Dns, uri.HostNameType, "nntp.HostNameType");
			Assert.AreEqual ("nntp://c/123456@c", uri.ToString (), "nntp.ToString");

			uri = new Uri ("file://c:/dir/subdir/file");
			Assert.AreEqual (String.Empty, uri.Authority, "file.Authority");
			Assert.AreEqual (String.Empty, uri.DnsSafeHost, "file.DnsSafeHost");
			Assert.AreEqual (String.Empty, uri.Host, "file.Host");
			Assert.AreEqual (UriHostNameType.Basic, uri.HostNameType, "file.HostNameType");
			Assert.AreEqual ("file:///c:/dir/subdir/file", uri.ToString (), "file.ToString");
		}

		[Test]
		public void UnknownScheme ()
		{
			Uri uri = new Uri ("mono:c:\\dir\\subdir\\file");
			Assert.IsFalse (uri.IsWellFormedOriginalString (), "IsWellFormedOriginalString");
			Assert.AreEqual (String.Empty, uri.Host, "Host");
			Assert.AreEqual ("c:\\dir\\subdir\\file", uri.LocalPath, "LocalPath");
			// make the next assert work on both Windows and Mac (wrt Silverlight)
			Assert.AreEqual ("mono:c:/dir/subdir/file", uri.ToString ().Replace ("%5C", "/"), "ToString");

			uri = new Uri ("mono://host/dir/subdir/file");
			Assert.IsTrue (uri.IsWellFormedOriginalString (), "2/IsWellFormedOriginalString");
			Assert.AreEqual ("host", uri.Host, "2/Host");
			Assert.AreEqual ("/dir/subdir/file", uri.AbsolutePath, "2/AbsolutePath");
			Assert.AreEqual ("/dir/subdir/file", uri.LocalPath, "2/LocalPath");

			uri = new Uri ("mono:///host/dir/subdir/file");
			Assert.IsTrue (uri.IsWellFormedOriginalString (), "3/IsWellFormedOriginalString");
			Assert.AreEqual (String.Empty, uri.Host, "3/Host");
			Assert.AreEqual ("/host/dir/subdir/file", uri.AbsolutePath, "3/AbsolutePath");
			Assert.AreEqual ("/host/dir/subdir/file", uri.LocalPath, "3/LocalPath");

			uri = new Uri ("mono:////host/dir/subdir/file");
			Assert.IsTrue (uri.IsWellFormedOriginalString (), "4/IsWellFormedOriginalString");
			Assert.AreEqual (String.Empty, uri.Host, "4/Host");
			Assert.AreEqual ("//host/dir/subdir/file", uri.AbsolutePath, "4/AbsolutePath");
			Assert.AreEqual ("//host/dir/subdir/file", uri.LocalPath, "4/LocalPath");

			// query and fragment
			uri = new Uri ("mono://host/dir/subdir/file?query#fragment");
			Assert.AreEqual ("/dir/subdir/file", uri.AbsolutePath, "qf.AbsolutePath");
			Assert.AreEqual ("?query", uri.Query, "qf.Query");
			Assert.AreEqual ("#fragment", uri.Fragment, "qf.Fragment");

			// special characters
			uri = new Uri ("mono://host/<>%\"{}|\\^`;/:@&=+$,[]#abc");
			if (IriParsing)
				Assert.AreEqual ("/%3C%3E%25%22%7B%7D%7C/%5E%60;/:@&=+$,[]", uri.AbsolutePath, "Special");
			else
				Assert.AreEqual ("/%3C%3E%25%22%7B%7D%7C/%5E%60;/:@&=+$,%5B%5D", uri.AbsolutePath, "Special");
		}

		[Test]
		public void DriveAndForwardSlashes_Segments ()
		{
			Uri uri = new Uri ("mono:c:\\dir\\subdir\\file");
			string [] segments = uri.Segments;
			Assert.AreEqual (4, segments.Length, "segments");
			// make the tests work on both Windows and Mac (wrt Silverlight)
			Assert.AreEqual ("c:/", segments [0].Replace ("%5C", "/"), "s[0]");
			Assert.AreEqual ("dir/", segments [1].Replace ("%5C", "/"), "s[1]");
			Assert.AreEqual ("subdir/", segments [2].Replace ("%5C", "/"), "s[2]");
			Assert.AreEqual ("file", segments [3], "s[3]");
		}

		[Test]
		public void NewsScheme ()
		{
			Uri uri = new Uri ("news:novell.mono.moonlight/uri?query");

			Assert.AreEqual ("novell.mono.moonlight/uri%3Fquery", uri.AbsolutePath, "AbsolutePath");
			Assert.AreEqual ("news:novell.mono.moonlight/uri%3Fquery", uri.AbsoluteUri, "AbsoluteUri");
			Assert.AreEqual (String.Empty, uri.Authority, "Authority");
			Assert.AreEqual (String.Empty, uri.DnsSafeHost, "DnsSafeHost");
			Assert.AreEqual (String.Empty, uri.Fragment, "Fragment");
			Assert.AreEqual (String.Empty, uri.Host, "Host");
			Assert.AreEqual (UriHostNameType.Unknown, uri.HostNameType, "HostNameType");
			Assert.IsTrue (uri.IsAbsoluteUri, "IsAbsoluteUri");
			Assert.IsTrue (uri.IsDefaultPort, "IsDefaultPort");
			Assert.IsFalse (uri.IsFile, "IsFile");
			Assert.IsFalse (uri.IsLoopback, "IsLoopback");
			Assert.IsFalse (uri.IsUnc, "IsUnc");
			Assert.AreEqual ("novell.mono.moonlight/uri?query", uri.LocalPath, "LocalPath");
			Assert.AreEqual ("news:novell.mono.moonlight/uri?query", uri.OriginalString, "OriginalString");
			Assert.AreEqual ("novell.mono.moonlight/uri%3Fquery", uri.PathAndQuery, "PathAndQuery");
			Assert.AreEqual (-1, uri.Port, "Port");
			Assert.AreEqual (String.Empty, uri.Query, "Query");
			Assert.AreEqual ("news", uri.Scheme, "Scheme");
			Assert.AreEqual ("novell.mono.moonlight/", uri.Segments [0], "Segments [0]");
			Assert.AreEqual ("uri%3Fquery", uri.Segments [1], "Segments [1]");
			Assert.IsFalse (uri.UserEscaped, "UserEscaped");
			Assert.AreEqual (String.Empty, uri.UserInfo, "UserInfo");

			// special escaped characters - they differs a bit from other URI
			uri = new Uri ("news:novell.mono.moonlight/<>%\"{}|\\^`;/?:@&=+$,[]#abc");
			Assert.AreEqual ("#abc", uri.Fragment, "Special/Fragment");
			if (IriParsing)
				Assert.AreEqual ("novell.mono.moonlight/%3C%3E%25%22%7B%7D%7C%5C%5E%60;/%3F:@&=+$,[]", uri.AbsolutePath, "Special");
			else
				Assert.AreEqual ("novell.mono.moonlight/%3C%3E%25%22%7B%7D%7C%5C%5E%60;/%3F:@&=+$,%5B%5D", uri.AbsolutePath, "Special");
		}

		[Test]
		public void NntpScheme ()
		{
			Uri uri = new Uri ("nntp://news.example.com/novell.mono.moonlight/uri?query");

			Assert.AreEqual ("/novell.mono.moonlight/uri%3Fquery", uri.AbsolutePath, "AbsolutePath");
			Assert.AreEqual ("nntp://news.example.com/novell.mono.moonlight/uri%3Fquery", uri.AbsoluteUri, "AbsoluteUri");
			Assert.AreEqual ("news.example.com", uri.Authority, "Authority");
			Assert.AreEqual ("news.example.com", uri.DnsSafeHost, "DnsSafeHost");
			Assert.AreEqual (String.Empty, uri.Fragment, "Fragment");
			Assert.AreEqual ("news.example.com", uri.Host, "Host");
			Assert.AreEqual (UriHostNameType.Dns, uri.HostNameType, "HostNameType");
			Assert.IsTrue (uri.IsAbsoluteUri, "IsAbsoluteUri");
			Assert.IsTrue (uri.IsDefaultPort, "IsDefaultPort");
			Assert.IsFalse (uri.IsFile, "IsFile");
			Assert.IsFalse (uri.IsLoopback, "IsLoopback");
			Assert.IsFalse (uri.IsUnc, "IsUnc");
			Assert.AreEqual ("/novell.mono.moonlight/uri?query", uri.LocalPath, "LocalPath");
			Assert.AreEqual ("nntp://news.example.com/novell.mono.moonlight/uri?query", uri.OriginalString, "OriginalString");
			Assert.AreEqual ("/novell.mono.moonlight/uri%3Fquery", uri.PathAndQuery, "PathAndQuery");
			Assert.AreEqual (119, uri.Port, "Port");
			Assert.AreEqual (String.Empty, uri.Query, "Query");
			Assert.AreEqual ("nntp", uri.Scheme, "Scheme");
			Assert.AreEqual ("/", uri.Segments [0], "Segments [0]");
			Assert.AreEqual ("novell.mono.moonlight/", uri.Segments [1], "Segments [1]");
			Assert.AreEqual ("uri%3Fquery", uri.Segments [2], "Segments [2]");
			Assert.IsFalse (uri.UserEscaped, "UserEscaped");
			Assert.AreEqual (String.Empty, uri.UserInfo, "UserInfo");

			// special escaped characters - they differs a bit from other URI
			uri = new Uri ("nntp://news.example.com/novell.mono.moonlight/<>%\"{}|\\^`;/?:@&=+$,[]#abc");
			Assert.AreEqual ("#abc", uri.Fragment, "Special/Fragment");
			if (IriParsing)
				Assert.AreEqual ("/novell.mono.moonlight/%3C%3E%25%22%7B%7D%7C%5C%5E%60;/%3F:@&=+$,[]", uri.AbsolutePath, "Special");
			else
				Assert.AreEqual ("/novell.mono.moonlight/%3C%3E%25%22%7B%7D%7C%5C%5E%60;/%3F:@&=+$,%5B%5D", uri.AbsolutePath, "Special");
		}

		[Test]
		public void FtpScheme ()
		{
			// user, password, custom port and a "query"
			Uri uri = new Uri ("ftp://user:password@ftp.example.com:2121/mono.zip?latest-n-greatest");
			Assert.AreEqual ("/mono.zip%3Flatest-n-greatest", uri.AbsolutePath, "AbsolutePath");
			Assert.AreEqual ("ftp://user:password@ftp.example.com:2121/mono.zip%3Flatest-n-greatest", uri.AbsoluteUri, "AbsoluteUri");
			Assert.AreEqual ("ftp.example.com:2121", uri.Authority, "Authority");
			Assert.AreEqual ("ftp.example.com", uri.DnsSafeHost, "DnsSafeHost");
			Assert.AreEqual (String.Empty, uri.Fragment, "Fragment");
			Assert.AreEqual ("ftp.example.com", uri.Host, "Host");
			Assert.AreEqual (UriHostNameType.Dns, uri.HostNameType, "HostNameType");
			Assert.IsTrue (uri.IsAbsoluteUri, "IsAbsoluteUri");
			Assert.IsFalse (uri.IsDefaultPort, "IsDefaultPort");
			Assert.IsFalse (uri.IsFile, "IsFile");
			Assert.IsFalse (uri.IsLoopback, "IsLoopback");
			Assert.IsFalse (uri.IsUnc, "IsUnc");
			Assert.AreEqual ("/mono.zip?latest-n-greatest", uri.LocalPath, "LocalPath");
			Assert.AreEqual ("ftp://user:password@ftp.example.com:2121/mono.zip?latest-n-greatest", uri.OriginalString, "OriginalString");
			Assert.AreEqual ("/mono.zip%3Flatest-n-greatest", uri.PathAndQuery, "PathAndQuery");
			Assert.AreEqual (2121, uri.Port, "Port");
			Assert.AreEqual (String.Empty, uri.Query, "Query");
			Assert.AreEqual ("ftp", uri.Scheme, "Scheme");
			Assert.AreEqual ("/", uri.Segments [0], "Segments [0]");
			Assert.AreEqual ("mono.zip%3Flatest-n-greatest", uri.Segments [1], "Segments [1]");
			Assert.IsFalse (uri.UserEscaped, "UserEscaped");
			Assert.AreEqual ("user:password", uri.UserInfo, "UserInfo");

			// special characters and fragment
			uri = new Uri ("ftp://ftp.example.com/<>%\"{}|\\^`;/?:@&=+$,[]#abc");
			Assert.AreEqual ("#abc", uri.Fragment, "Special/Fragment");
			if (IriParsing)
				Assert.AreEqual ("/%3C%3E%25%22%7B%7D%7C/%5E%60;/%3F:@&=+$,[]", uri.AbsolutePath, "Special");
			else
				Assert.AreEqual ("/%3C%3E%25%22%7B%7D%7C/%5E%60;/%3F:@&=+$,%5B%5D", uri.AbsolutePath, "Special");
		}

		[Test]
		public void FileScheme ()
		{
			Uri uri = new Uri ("file://host/dir/subdir/file?this-is-not-a-query#but-this-is-a-fragment");

			if (IriParsing)	{
				Assert.AreEqual ("/dir/subdir/file", uri.AbsolutePath, "AbsolutePath");
				Assert.AreEqual ("file://host/dir/subdir/file?this-is-not-a-query#but-this-is-a-fragment", uri.AbsoluteUri, "AbsoluteUri");
				Assert.AreEqual ("\\\\host\\dir\\subdir\\file", uri.LocalPath, "LocalPath");
				Assert.AreEqual ("/dir/subdir/file?this-is-not-a-query", uri.PathAndQuery, "PathAndQuery");
				Assert.AreEqual ("?this-is-not-a-query", uri.Query, "Query");
				Assert.AreEqual ("file", uri.Segments [3], "Segments [3]");
			} else {
				Assert.AreEqual ("/dir/subdir/file%3Fthis-is-not-a-query", uri.AbsolutePath, "AbsolutePath");
				Assert.AreEqual ("file://host/dir/subdir/file%3Fthis-is-not-a-query#but-this-is-a-fragment", uri.AbsoluteUri, "AbsoluteUri");
				Assert.AreEqual (isWin32 ? "\\\\host\\dir\\subdir\\file?this-is-not-a-query" : "/dir/subdir/file?this-is-not-a-query", uri.LocalPath, "LocalPath");
				Assert.AreEqual ("/dir/subdir/file%3Fthis-is-not-a-query", uri.PathAndQuery, "PathAndQuery");
				Assert.AreEqual (String.Empty, uri.Query, "Query");
				Assert.AreEqual ("file%3Fthis-is-not-a-query", uri.Segments [3], "Segments [3]");
			}

			Assert.AreEqual ("host", uri.Authority, "Authority");
			Assert.AreEqual ("host", uri.DnsSafeHost, "DnsSafeHost");
			Assert.AreEqual ("#but-this-is-a-fragment", uri.Fragment, "Fragment");
			Assert.AreEqual ("host", uri.Host, "Host");
			Assert.AreEqual (UriHostNameType.Dns, uri.HostNameType, "HostNameType");
			Assert.IsTrue (uri.IsAbsoluteUri, "IsAbsoluteUri");
			Assert.IsTrue (uri.IsDefaultPort, "IsDefaultPort");
			Assert.IsTrue (uri.IsFile, "IsFile");
			Assert.IsFalse (uri.IsLoopback, "IsLoopback");
			Assert.AreEqual (isWin32, uri.IsUnc, "IsUnc");
			Assert.AreEqual ("file://host/dir/subdir/file?this-is-not-a-query#but-this-is-a-fragment", uri.OriginalString, "OriginalString");
			Assert.AreEqual (-1, uri.Port, "Port");
			Assert.AreEqual ("file", uri.Scheme, "Scheme");
			Assert.AreEqual ("/", uri.Segments [0], "Segments [0]");
			Assert.AreEqual ("dir/", uri.Segments [1], "Segments [1]");
			Assert.AreEqual ("subdir/", uri.Segments [2], "Segments [2]");
			Assert.IsFalse (uri.UserEscaped, "UserEscaped");
			Assert.AreEqual (String.Empty, uri.UserInfo, "UserInfo");

			// special characters
			uri = new Uri ("file://host/<>%\"{}|\\^`;/:@&=+$,[]?#abc");
			if (IriParsing)
				Assert.AreEqual ("/%3C%3E%25%22%7B%7D%7C/%5E%60;/:@&=+$,[]", uri.AbsolutePath, "Special");
			else
				Assert.AreEqual ("/%3C%3E%25%22%7B%7D%7C/%5E%60;/:@&=+$,%5B%5D%3F", uri.AbsolutePath, "Special");
		}

		[Test]
		public void LocalFile ()
		{
			Uri uri = new Uri ("file:///c:/subdir/file");

			Assert.AreEqual ("c:/subdir/file", uri.AbsolutePath, "AbsolutePath");
			Assert.AreEqual ("file:///c:/subdir/file", uri.AbsoluteUri, "AbsoluteUri");
			Assert.AreEqual ("c:\\subdir\\file", uri.LocalPath, "LocalPath");
			Assert.AreEqual ("c:/subdir/file", uri.PathAndQuery, "PathAndQuery");
			Assert.AreEqual (String.Empty, uri.Query, "Query");
			Assert.AreEqual ("file", uri.Segments [3], "Segments [3]");

			Assert.AreEqual (String.Empty, uri.Authority, "Authority");
			Assert.AreEqual (String.Empty, uri.DnsSafeHost, "DnsSafeHost");
			Assert.AreEqual (String.Empty, uri.Fragment, "Fragment");
			Assert.AreEqual (String.Empty, uri.Host, "Host");
			Assert.AreEqual (UriHostNameType.Basic, uri.HostNameType, "HostNameType");
			Assert.IsTrue (uri.IsAbsoluteUri, "IsAbsoluteUri");
			Assert.IsTrue (uri.IsDefaultPort, "IsDefaultPort");
			Assert.IsTrue (uri.IsFile, "IsFile");
			Assert.IsTrue (uri.IsLoopback, "IsLoopback");
			Assert.IsFalse (uri.IsUnc, "IsUnc");
			Assert.AreEqual ("file:///c:/subdir/file", uri.OriginalString, "OriginalString");
			Assert.AreEqual (-1, uri.Port, "Port");
			Assert.AreEqual ("file", uri.Scheme, "Scheme");
			Assert.AreEqual ("/", uri.Segments [0], "Segments [0]");
			Assert.AreEqual ("c:/", uri.Segments [1], "Segments [1]");
			Assert.AreEqual ("subdir/", uri.Segments [2], "Segments [2]");
			Assert.IsFalse (uri.UserEscaped, "UserEscaped");
			Assert.AreEqual (String.Empty, uri.UserInfo, "UserInfo");
		}

		[Test]
		public void LocalhostWinFile ()
		{
			Uri uri = new Uri ("file://localhost/c:/subdir/file");

			Assert.AreEqual ("/c:/subdir/file", uri.AbsolutePath, "AbsolutePath");
			Assert.AreEqual ("file://localhost/c:/subdir/file", uri.AbsoluteUri, "AbsoluteUri");
			Assert.AreEqual (isWin32 ? "\\\\localhost\\c:\\subdir\\file" : "/c:/subdir/file", uri.LocalPath, "LocalPath");
			Assert.AreEqual ("/c:/subdir/file", uri.PathAndQuery, "PathAndQuery");
			Assert.AreEqual (String.Empty, uri.Query, "Query");
			Assert.AreEqual ("file", uri.Segments [3], "Segments [3]");

			Assert.AreEqual ("localhost", uri.Authority, "Authority");
			Assert.AreEqual ("localhost", uri.DnsSafeHost, "DnsSafeHost");
			Assert.AreEqual (String.Empty, uri.Fragment, "Fragment");
			Assert.AreEqual ("localhost", uri.Host, "Host");
			Assert.AreEqual (UriHostNameType.Dns, uri.HostNameType, "HostNameType");
			Assert.IsTrue (uri.IsAbsoluteUri, "IsAbsoluteUri");
			Assert.IsTrue (uri.IsDefaultPort, "IsDefaultPort");
			Assert.IsTrue (uri.IsFile, "IsFile");
			Assert.IsTrue (uri.IsLoopback, "IsLoopback");
			Assert.AreEqual (isWin32, uri.IsUnc, "IsUnc");
			Assert.AreEqual ("file://localhost/c:/subdir/file", uri.OriginalString, "OriginalString");
			Assert.AreEqual (-1, uri.Port, "Port");
			Assert.AreEqual ("file", uri.Scheme, "Scheme");
			Assert.AreEqual ("/", uri.Segments [0], "Segments [0]");
			Assert.AreEqual ("c:/", uri.Segments [1], "Segments [1]");
			Assert.AreEqual ("subdir/", uri.Segments [2], "Segments [2]");
			Assert.IsFalse (uri.UserEscaped, "UserEscaped");
			Assert.AreEqual (String.Empty, uri.UserInfo, "UserInfo");
		}

		[Test]
		public void LocalhostFile ()
		{
			Uri uri = new Uri ("file://localhost/dir/subdir/file");

			Assert.AreEqual ("/dir/subdir/file", uri.AbsolutePath, "AbsolutePath");
			Assert.AreEqual ("file://localhost/dir/subdir/file", uri.AbsoluteUri, "AbsoluteUri");
			Assert.AreEqual (isWin32 ? "\\\\localhost\\dir\\subdir\\file" : "/dir/subdir/file", uri.LocalPath, "LocalPath");
			Assert.AreEqual ("/dir/subdir/file", uri.PathAndQuery, "PathAndQuery");
			Assert.AreEqual (String.Empty, uri.Query, "Query");
			Assert.AreEqual ("file", uri.Segments [3], "Segments [3]");

			Assert.AreEqual ("localhost", uri.Authority, "Authority");
			Assert.AreEqual ("localhost", uri.DnsSafeHost, "DnsSafeHost");
			Assert.AreEqual (String.Empty, uri.Fragment, "Fragment");
			Assert.AreEqual ("localhost", uri.Host, "Host");
			Assert.AreEqual (UriHostNameType.Dns, uri.HostNameType, "HostNameType");
			Assert.IsTrue (uri.IsAbsoluteUri, "IsAbsoluteUri");
			Assert.IsTrue (uri.IsDefaultPort, "IsDefaultPort");
			Assert.IsTrue (uri.IsFile, "IsFile");
			Assert.IsTrue (uri.IsLoopback, "IsLoopback");
			Assert.AreEqual (isWin32, uri.IsUnc, "IsUnc");
			Assert.AreEqual ("file://localhost/dir/subdir/file", uri.OriginalString, "OriginalString");
			Assert.AreEqual (-1, uri.Port, "Port");
			Assert.AreEqual ("file", uri.Scheme, "Scheme");
			Assert.AreEqual ("/", uri.Segments [0], "Segments [0]");
			Assert.AreEqual ("dir/", uri.Segments [1], "Segments [1]");
			Assert.AreEqual ("subdir/", uri.Segments [2], "Segments [2]");
			Assert.IsFalse (uri.UserEscaped, "UserEscaped");
			Assert.AreEqual (String.Empty, uri.UserInfo, "UserInfo");
		}

		[Test]
		public void PathReduction_2e ()
		{
			Uri uri = new Uri ("http://host/dir/%2e%2E/file");
			Assert.AreEqual ("/file", uri.AbsolutePath, "AbsolutePath");
			Assert.AreEqual ("http://host/file", uri.AbsoluteUri, "AbsoluteUri");
			Assert.AreEqual ("/", uri.Segments [0], "Segments [0]");
			Assert.AreEqual ("file", uri.Segments [1], "Segments [1]");
		}

		[Test]
		public void ColonButNoPort ()
		{
			Uri uri = new Uri ("http://host:");
			Assert.AreEqual ("http", uri.Scheme, "1.Scheme");
			Assert.AreEqual ("host", uri.Host, "1.Host");
			Assert.AreEqual (80, uri.Port, "1.Port");
			Assert.IsTrue (uri.IsDefaultPort, "1.IsDefaultPort");

			uri = new Uri ("ftp://host:/dir/file");
			Assert.AreEqual ("ftp", uri.Scheme, "2.Scheme");
			Assert.AreEqual ("host", uri.Host, "2.Host");
			Assert.AreEqual (21, uri.Port, "2.Port");
			Assert.IsTrue (uri.IsDefaultPort, "2.IsDefaultPort");
		}

		[Test]
		public void IPv6SafeDnsName ()
		{
			Uri uri = new Uri ("http://[1:2:3:4:5:6:7:8]");
			Assert.AreEqual (UriHostNameType.IPv6, uri.HostNameType, "1.HostNameType");
			if (IriParsing) {
				Assert.AreEqual ("[1:2:3:4:5:6:7:8]", uri.Authority, "1.Authority");
				Assert.AreEqual ("1:2:3:4:5:6:7:8", uri.DnsSafeHost, "1.DnsSafeHost");
				Assert.AreEqual ("[1:2:3:4:5:6:7:8]", uri.Host, "1.Host");
			} else {
				Assert.AreEqual ("[0001:0002:0003:0004:0005:0006:0007:0008]", uri.Authority, "1.Authority");
				Assert.AreEqual ("0001:0002:0003:0004:0005:0006:0007:0008", uri.DnsSafeHost, "1.DnsSafeHost");
				Assert.AreEqual ("[0001:0002:0003:0004:0005:0006:0007:0008]", uri.Host, "1.Host");
			}

			uri = new Uri ("http://[fe80::200:39ff:fe36:1a2d%4]/temp/example.htm");
			Assert.AreEqual (UriHostNameType.IPv6, uri.HostNameType, "1.HostNameType");
			if (IriParsing)	{
				Assert.AreEqual ("[fe80::200:39ff:fe36:1a2d]", uri.Authority, "2.Authority");
				Assert.AreEqual ("fe80::200:39ff:fe36:1a2d%4", uri.DnsSafeHost, "2.DnsSafeHost");
				Assert.AreEqual ("[fe80::200:39ff:fe36:1a2d]", uri.Host, "2.Host");
			} else {
				Assert.AreEqual ("[FE80:0000:0000:0000:0200:39FF:FE36:1A2D]", uri.Authority, "2.Authority");
				Assert.AreEqual ("FE80:0000:0000:0000:0200:39FF:FE36:1A2D%4", uri.DnsSafeHost, "2.DnsSafeHost");
				Assert.AreEqual ("[FE80:0000:0000:0000:0200:39FF:FE36:1A2D]", uri.Host, "2.Host");
			}
		}

		[Test]
		public void RelativeEscapes ()
		{
			Uri uri = new Uri ("%2e%2e/dir/%2e%2e/subdir/file?query#fragment", UriKind.Relative);
			if (IriParsing)
				Assert.AreEqual ("../dir/../subdir/file?query#fragment", uri.ToString (), "1.ToString");
			else
				Assert.AreEqual ("%2e%2e/dir/%2e%2e/subdir/file?query#fragment", uri.ToString (), "1.ToString");
		}

		[Test]
		public void BadUri ()
		{
			Assert2.Throws<UriFormatException> (delegate {
				new Uri ("a:b", UriKind.Absolute);
			}, "a:b - Absolute");

			Uri abs = new Uri ("http://novell.com", UriKind.Absolute);
			Assert2.Throws<UriFormatException> (delegate {
				new Uri (abs, "a:b");
			}, "a:b - Relative");
		}

		[Test]
		public void MergeWithConfusingRelativeUri ()
		{
			Uri abs = new Uri ("http://novell.com", UriKind.Absolute);

			// note: invalid scheme
			string srel = "http@ftp://example.com/dir/file";
			Uri uri = new Uri (abs, srel);
			Assert.AreEqual ("http://novell.com/http@ftp://example.com/dir/file", uri.ToString (), "1.ToString");

			Uri rel = new Uri (srel, UriKind.Relative);
			Assert.AreEqual ("http@ftp://example.com/dir/file", rel.ToString (), "2.ToString");

			uri = new Uri (abs, rel);
			Assert.AreEqual ("http://novell.com/http@ftp://example.com/dir/file", uri.ToString (), "3.ToString");
		}

		[Test]
		public void EmptyUserInfo ()
		{
			Uri uri = new Uri ("http://@www.example.com");
			Assert.AreEqual ("http://@www.example.com/", uri.AbsoluteUri, "AbsoluteUri");
			Assert.AreEqual ("http://@www.example.com", uri.GetLeftPart (UriPartial.Authority), "UriPartial.Authority");
			Assert.AreEqual ("http://@www.example.com/", uri.GetLeftPart (UriPartial.Path), "UriPartial.Path");
			Assert.AreEqual (String.Empty, uri.UserInfo, "UserInfo");
		}

		[Test]
		public void Fragment_SpecialCharacters ()
		{
			Uri uri = new Uri ("http://host/dir/file#fragment <>%\"{}|\\^`;/?:@&=+$,[]#second");
			Assert.AreEqual ("http://host/dir/file#fragment <>%25\"{}|\\^`;/?:@&=+$,[]%23second", uri.ToString (), "ToString");
			if (IriParsing)
				Assert.AreEqual ("#fragment%20%3C%3E%25%22%7B%7D%7C%5C%5E%60;/?:@&=+$,[]#second", uri.Fragment, "Fragment");
			else
				Assert.AreEqual ("#fragment%20%3C%3E%25%22%7B%7D%7C%5C%5E%60;/?:@&=+$,%5B%5D%23second", uri.Fragment, "Fragment");
		}

		[Test]
		public void Query_SpecialCharacters ()
		{
			Uri uri = new Uri ("http://host/dir/file?query <>%\"{}|\\^`;/?:@&=+$,[]");
			Assert.AreEqual ("http://host/dir/file?query <>%25\"{}|\\^`;/?:@&=+$,[]", uri.ToString (), "ToString");
			if (IriParsing)
				Assert.AreEqual ("?query%20%3C%3E%25%22%7B%7D%7C%5C%5E%60;/?:@&=+$,[]", uri.Query, "Query");
			else
				Assert.AreEqual ("?query%20%3C%3E%25%22%7B%7D%7C%5C%5E%60;/?:@&=+$,%5B%5D", uri.Query, "Query");
		}

		[Test]
		public void OriginalPathEscaped ()
		{
			Uri uri = new Uri ("http://www.example.com/%41/%42/%43", UriKind.Absolute);
			Assert.AreEqual ("/A/B/C", uri.LocalPath, "LocalPath");
			if (IriParsing)	{
				Assert.AreEqual ("http://www.example.com/A/B/C", uri.AbsoluteUri, "AbsoluteUri");
				Assert.AreEqual ("/A/B/C", uri.AbsolutePath, "AbsolutePath");
				Assert.AreEqual ("http://www.example.com/A/B/C", uri.GetLeftPart (UriPartial.Path), "GetLeftPart(Path)");
			} else {
				Assert.AreEqual ("http://www.example.com/%41/%42/%43", uri.AbsoluteUri, "AbsoluteUri");
				Assert.AreEqual ("/%41/%42/%43", uri.AbsolutePath, "AbsolutePath");
				Assert.AreEqual ("http://www.example.com/%41/%42/%43", uri.GetLeftPart (UriPartial.Path), "GetLeftPart(Path)");
			}
		}

		[Test]
		public void CheckHostName ()
		{
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName ("host;machine"), "CheckHostName ;");
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName ("www..example.com"), "CheckHostName ..");
			Assert.AreEqual (UriHostNameType.Unknown, Uri.CheckHostName ("www.example.com\\"), "CheckHostName \\");
		}

		[Test]
		public void Ports ()
		{
			Assert2.Throws<UriFormatException> (delegate {
				new Uri ("http://host:-1/");
			}, "negative");

			Uri uri = new Uri ("http://host:0/");
			Assert.AreEqual (0, uri.Port, "Port = 0");

			Assert2.Throws<UriFormatException> (delegate {
				new Uri ("http://host:+1/");
			}, "positive");

			uri = new Uri ("http://host:" + UInt16.MaxValue.ToString ());
			Assert.AreEqual (65535, uri.Port, "Port = 65535");

			Assert2.Throws<UriFormatException> (delegate {
				new Uri ("http://host:" + (UInt16.MaxValue + 1).ToString ());
			}, "too big");

			Assert2.Throws<UriFormatException> (delegate {
				new Uri ("http://host:3.14");
			}, "float");
		}

		[Test]
		public void NonAsciiHost ()
		{
			Uri uri = new Uri ("ftp://β:2121/", UriKind.Absolute);
			Assert.AreEqual ("/", uri.AbsolutePath, "AbsolutePath");
			Assert.AreEqual ("ftp://β:2121/", uri.AbsoluteUri, "AbsoluteUri");
			Assert.AreEqual ("β:2121", uri.Authority, "Authority");
			Assert.AreEqual ("β", uri.DnsSafeHost, "DnsSafeHost");
			Assert.AreEqual ("β", uri.Host, "Host");
			Assert.IsTrue (uri.IsAbsoluteUri, "IsAbsoluteUri");
			Assert.IsFalse (uri.IsDefaultPort, "IsDefaultPort");
			Assert.AreEqual ("/", uri.LocalPath, "LocalPath");
			Assert.AreEqual ("ftp://β:2121/", uri.OriginalString, "OriginalString");
			Assert.AreEqual ("/", uri.PathAndQuery, "PathAndQuery");
			Assert.AreEqual (2121, uri.Port, "Port");
			Assert.AreEqual ("ftp", uri.Scheme, "Scheme");
			Assert.AreEqual ("/", uri.Segments [0], "Segments [0]");
		}
	}
}
