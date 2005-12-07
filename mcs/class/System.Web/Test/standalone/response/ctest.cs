//
// This test program uses the Http client to fetch the contents
// of a request.  As opposed to the rtest program, this one is
// used merely to compare the contents, not look at the traffic
// produced.
//
using System;
using System.IO;
using System.Net;
using System.Web;

class X {

	static void Main (string [] args)
	{
		string url = String.Format ("http://{0}:{1}/{2}", args [0], args [1], args [2]);
		
		HttpWebRequest web = (HttpWebRequest) WebRequest.Create (url);

		Stream s = web.GetResponse ().GetResponseStream ();

		StreamReader sr = new StreamReader (s);
		Console.WriteLine (sr.ReadToEnd ());
	}
}
