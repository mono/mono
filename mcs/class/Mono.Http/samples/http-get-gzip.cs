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
	static void GZWR (string url)
	{
		WebRequest req = WebRequest.Create ("gziphttp://" + url);
		WebResponse wr = req.GetResponse ();
		Stream st = wr.GetResponseStream ();
		byte [] b = new byte [4096];
		long total = 0;
		int count;
		while ((count = st.Read (b, 0, 4096)) != 0) {
			Console.Write (Encoding.Default.GetString (b, 0, count));
			total += count;
		}

		st.Close ();

		Console.WriteLine ("\nContent-Encoding: '{0}' (if empty, not compressed)",
				    wr.Headers ["Content-Encoding"]);
	}
	
	static void Main (string [] args)
	{
		if (args.Length != 1) {
			Console.WriteLine ("You should provide a HTTP URL without 'http://'");
			return;
		}

		GZWR (args [0]);
	}
}


