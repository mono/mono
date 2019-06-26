//
// uri-test-generator.cs : URI test result generator.
//
// Author:
//	Atsushi Enomoto <atushi@ximian.com>
//
// (C)2003 Novell inc.
//
// See test-uri-list.txt for usage.
//
using System;
using System.IO;
using System.Text;

namespace MonoTests.System
{
	public class UriTestGenerator
	{
		public static void Main (string [] args)
		{
			StreamReader sr = new StreamReader ("test-uri-list.txt", Encoding.UTF8);
			StreamWriter sw = new StreamWriter ("test-uri-props.txt", false, Encoding.UTF8);

			GenerateResult (sr, sw, null);

			sr = new StreamReader ("test-uri-relative-list.txt", Encoding.UTF8);
			sw = new StreamWriter ("test-uri-relative-props.txt", false, Encoding.UTF8);

			Uri baseUri = new Uri ("http://www.example.com");
			GenerateResult (sr, sw, baseUri);
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
				sw.Flush ();
			}
			sr.Close ();
			sw.Close ();
		}

	}
}
