using System;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class UriTest2 : Assertion
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
			AssertEquals (relsrc + " AbsoluteUri", absoluteUri, uri.AbsoluteUri);
			AssertEquals (relsrc + " Scheme", scheme, uri.Scheme);
			AssertEquals (relsrc + " Host", host, uri.Host);
			AssertEquals (relsrc + " Port", port, uri.Port);
			// Windows UNC path is not automatically testable on *nix environment,
			if (relsrc.StartsWith ("\\\\") && Path.DirectorySeparatorChar == '\\')
				AssertEquals (relsrc + " LocalPath", localPath, uri.LocalPath);
			AssertEquals (relsrc + " Query", query, uri.Query);
			AssertEquals (relsrc + " Fragment", fragment, uri.Fragment);
			AssertEquals (relsrc + " IsFile", isFile, uri.IsFile);
			AssertEquals (relsrc + " IsUnc", isUnc, uri.IsUnc);
			AssertEquals (relsrc + " IsLoopback", isLoopback, uri.IsLoopback);
			AssertEquals (relsrc + " Authority", authority, uri.Authority);
			AssertEquals (relsrc + " UserEscaped", userEscaped, uri.UserEscaped);
			AssertEquals (relsrc + " UserInfo", userInfo, uri.UserInfo);
			AssertEquals (relsrc + " HostNameType", hostNameType, uri.HostNameType);
			AssertEquals (relsrc + " AbsolutePath", absolutePath, uri.AbsolutePath);
			AssertEquals (relsrc + " PathAndQuery", pathAndQuery, uri.PathAndQuery);
			AssertEquals (relsrc + " ToString()", toString, uri.ToString ());
		}

		[Test]
		public void AbsoluteUriFromFile ()
		{
			FromResource ("test-uri-props.txt", null);
		}
		
		[Test]
		[Category("NotDotNet")]
		public void AbsoluteUriFromFileManual ()
		{
			FromResource ("test-uri-props-manual.txt", null);
		}
		
		[Test]
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
					Fail (String.Format ("Construction failed: [{0}] {1}", uriString, ex.Message));
				}
			}
		}

		[Test]
		public void MoreUnescape () // bug 733316
		{
			int index = 0;
			char unesc = Uri.HexUnescape ("%F6", ref index);
			AssertEquals ("#01", 3, index);
			AssertEquals ("#02", 0xf6, unesc);
		}

	}
}
