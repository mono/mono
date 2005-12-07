//
// Usage:
//    rtest host port url 
//
// This program dumps the results of the HTTP request without any HTTP
// headers
//
using System;
using System.Net;
using System.IO;
using System.Net.Sockets;

class X {
	static NetworkStream ns;
	static StreamWriter sw;
	static StreamReader sr;
	static TcpClient c;
	static bool debug;
	static bool headers;
	static string header;
	
	static void send (string s)
	{
		if (debug)
			Console.WriteLine (s);
		
		sw.Write (s);
		sw.Flush ();
	}
	
	static void Main (string [] args)
	{
		int i = 0;

		while (args [i].StartsWith ("-")){
			if (args [i] == "-debug")
				debug = true;
			if (args [i] == "-headers")
				headers = true;
			if (args [i] == "-header")
				header = args [++i];
			i++;
		}
		
		c = new TcpClient (args [i], Int32.Parse (args [i+1]));
		c.ReceiveTimeout = 1000;
		ns = c.GetStream ();
		
		sw = new StreamWriter (ns);
		sr = new StreamReader (ns);

		string host = args [i];
		if (args [i+1] != "80")
			host += ":" + args [i+1];
		send (String.Format ("GET {0} HTTP/1.1\r\nHost: {1}\r\n\r\n", args [i+2], host));

		MemoryStream ms = new MemoryStream ();
		
		try {
			byte [] buf = new byte [1024];
			int n;
			
			while ((n = ns.Read (buf, 0, 1024)) != 0){
				ms.Write (buf, 0, n);
			}
		} catch {}

		ms.Position = 0;
		sr = new StreamReader (ms);

		string s;
		
		while ((s = sr.ReadLine ()) != null){
			if (s == ""){
				if (headers)
					return;
				
				string x = sr.ReadToEnd ();
				Console.Write (x);
				break;
			}  else {
				if (debug || headers)
					Console.WriteLine (s);
				if (header != null && s.StartsWith (header)){
					Console.WriteLine (s);
					return;
				}
			}
		}
	}
}

