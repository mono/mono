//
// http-get-gzip.cs: sample usage of GZipWebRequest
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.IO;
using System.Net;
using System.Text;

using Mono.Http;

class GZipTest
{
	static string url = "http://127.0.0.1/index.php";

	static void GZWR ()
	{
		WebRequest req = new GZipWebRequest (WebRequest.Create (url));
		WebResponse wr = req.GetResponse ();
		Console.WriteLine ("Content-Encoding: '{0}' (if empty, not compressed)", wr.Headers ["Content-Encoding"]);
		Stream st = wr.GetResponseStream ();
		byte [] b = new byte [4096];
		long total = 0;
		int count;
		while ((count = st.Read (b, 0, 4096)) != 0)
			Console.Write (Encoding.Default.GetString (b, 0, count));

		st.Close ();

		// Console.WriteLine ("Read: {0}", total);
	}
	
	static void Main ()
	{
		GZWR ();
	}
}


