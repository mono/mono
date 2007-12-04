//
// UriTest2.cs - More NUnit Test Cases for System.Uri
//

using System;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class UriTest2
	{
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
#if NET_2_0
		[Ignore ("Tests needs to be updated for 2.0")]
#endif
		public void AbsoluteUriFromFile ()
		{
			FromResource ("test-uri-props.txt", null);
		}
		
		[Test]
		[Category("NotDotNet")]
#if NET_2_0
		[Ignore ("Tests needs to be updated for 2.0")]
#endif
		public void AbsoluteUriFromFileManual ()
		{
			if (Path.DirectorySeparatorChar == '\\')
				return;
			FromResource ("test-uri-props-manual.txt", null);
		}
		
		[Test]
#if NET_2_0
		[Ignore ("Tests needs to be updated for 2.0")]
#endif
		public void RelativeUriFromFile ()
		{
			FromResource ("test-uri-relative-props.txt", new Uri ("http://www.go-mono.com"));
		}
		
		private void FromResource (string res, Uri baseUri)
		{
			Assembly a = Assembly.GetExecutingAssembly ();
			Stream s = a.GetManifestResourceStream (res);
			StreamReader sr = new StreamReader (s, Encoding.UTF8);
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
#if NET_2_0
			Assert.AreEqual ("net.pipe", Uri.UriSchemeNetPipe, "net.pipe");
			Assert.AreEqual ("net.tcp", Uri.UriSchemeNetTcp, "net.tcp");
#endif
		}

		[Test] // bug #71049
		[ExpectedException (typeof (UriFormatException))]
		public void StarsInHost ()
		{
			new Uri ("http://w*w*w.go-mono.com");
		}

		[Test]
#if ONLY_1_1
		[Category ("NotDotNet")] // 1.x throws an UriFormatException
#endif
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
#if ONLY_1_1
		[Category ("NotDotNet")] // 1.x throws an UriFormatException
#endif
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
	}
}
