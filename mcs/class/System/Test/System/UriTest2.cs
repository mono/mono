using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class UriTest2 : Assertion
	{
		// Segments cannot be validated here...
		public void AssertUri (Uri uri,
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
			AssertEquals ("AbsoluteUri", absoluteUri, uri.AbsoluteUri);
			AssertEquals ("Scheme", scheme, uri.Scheme);
			AssertEquals ("Host", host, uri.Host);
			AssertEquals ("Port", port, uri.Port);
			AssertEquals ("LocalPath", localPath, uri.LocalPath);
			AssertEquals ("query", query, uri.Query);
			AssertEquals ("Fragment", fragment, uri.Fragment);
			AssertEquals ("IsFile", isFile, uri.IsFile);
			AssertEquals ("IsUnc", isUnc, uri.IsUnc);
			AssertEquals ("IsLoopback", isLoopback, uri.IsLoopback);
			AssertEquals ("authority", authority, uri.Authority);
			AssertEquals ("UserEscaped", userEscaped, uri.UserEscaped);
			AssertEquals ("UserInfo", userInfo, uri.UserInfo);
			AssertEquals ("HostNameType", hostNameType, uri.HostNameType);
			AssertEquals ("AbsolutePath", absolutePath, uri.AbsolutePath);
			AssertEquals ("PathAndQuery", pathAndQuery, uri.PathAndQuery);
			AssertEquals ("ToString()", toString, uri.ToString ());
		}

		[Test]
		public void TestFromFile ()
		{
			StreamReader sr = new StreamReader ("System/test-uri-props.txt", Encoding.UTF8);
			while (sr.Peek () > 0) {
				sr.ReadLine (); // skip
				string uriString = sr.ReadLine ();
TextWriter sw = Console.Out;
				sw.WriteLine ("-------------------------");
				sw.WriteLine (uriString);

				if (uriString == null || uriString.Length == 0)
					break;

				Uri uri = new Uri (uriString);

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

				AssertUri (uri,
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
				Console.WriteLine ("Passed: " + uriString);
			}
		}

/*
		public static void Main (string [] args)
		{
			StreamReader sr = new StreamReader ("test-uri-list.txt", Encoding.UTF8);
			StreamWriter sw = new StreamWriter ("test-uri-props.txt", false, Encoding.UTF8);

			GenerateResult (sr, sw, null);
		}

		public static void GenerateResult (TextReader sr, TextWriter sw, Uri baseUri)
		{
			while (sr.Peek () > 0) {
				string uriString = sr.ReadLine ();
				if (uriString.Length == 0 || uriString [0] == '#')
					continue;
				Uri uri = (baseUri == null) ?
					new Uri (uriString) : new Uri (baseUri, uriString);

				sw.WriteLine ("-------------------------");
				sw.WriteLine (uriString);
				sw.WriteLine (uri.ToString ());
				sw.WriteLine (uri.AbsoluteUri);
				sw.WriteLine (uri.Scheme);
				sw.WriteLine (uri.Host);
				sw.WriteLine (uri.LocalPath);
				sw.WriteLine (uri.Query);
				sw.WriteLine (uri.Port);
				sw.WriteLine (uri.IsFile);
				sw.WriteLine (uri.IsUnc);
				sw.WriteLine (uri.IsLoopback);
				sw.WriteLine (uri.UserEscaped);
				sw.WriteLine (uri.HostNameType);
				sw.WriteLine (uri.AbsolutePath);
				sw.WriteLine (uri.PathAndQuery);
				sw.WriteLine (uri.Authority);
				sw.WriteLine (uri.Fragment);
				sw.WriteLine (uri.UserInfo);
			}
			sr.Close ();
			sw.Close ();
		}
*/

	}
}
