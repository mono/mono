//
// mget: Mono Web Get
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;

class MonoGet : ICertificatePolicy {

	public bool CheckValidationResult (ServicePoint servicePoint, X509Certificate certificate,
		WebRequest webRequest, int certificateProblem)
	{
		return true;
	}

	public long Get (string url, string filename)
	{
		byte[] buffer = new byte [16 * 1024];
		long total = 0;
		int b;

		WebRequest req = WebRequest.Create (url);
		HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
		Stream s = resp.GetResponseStream ();

		using (FileStream fs = new FileStream (filename, FileMode.Create, FileAccess.Write)) {
			using (BinaryWriter br = new BinaryWriter (fs)) {
				while ((b = s.Read (buffer, 0, buffer.Length)) != 0) {
					br.Write (buffer, 0, b);
					total += b;
				}
				s.Close ();
			}
		}
		return total;
	}

	static int Help ()
	{
		Console.WriteLine ("mono mget.exe [options] url [url ...]");
		Console.WriteLine ("where options can be one, or more,of the following:");
		Console.WriteLine ("\t--no-check-certificate\tDon't validate the server SSL/TLS certificate");
		Console.WriteLine ("\t--secure-protocol=protocol\tWhere protocol can be auto, SSLv3 or TLSv1");
		return 1;
	}

	static int Main (string[] args)
	{
		if (args.Length == 0)
			return Help ();

		MonoGet mget = new MonoGet ();
		ArrayList urls = new ArrayList ();

		foreach (string arg in args) {
			switch (arg.ToLower ()) {
			case "--no-check-certificate":
				ServicePointManager.CertificatePolicy = mget;
				break;
			case "--secure-protocol=auto":
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
				break;
			case "--secure-protocol=sslv3":
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
				break;
			case "--secure-protocol=tlsv1":
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
				break;
			case "--secure-protocol=sslv2":
				Console.WriteLine ("SSLv2 isn't supported in the framework");
				return Help ();
			case "--help":
			case "--?":
				return Help ();
			default:
				if (arg.StartsWith ("--")) {
					Console.WriteLine ("Unknown {0} option.", arg);
					return Help ();
				} else {
					urls.Add (arg);
				}
				break;
			}
		}

		if (urls.Count == 0)
			return Help ();

		int status = 0;
		foreach (string url in urls) {
			string filename = Path.GetFileName (url);
			if (filename.Length == 0)
				filename = "default";
			string savename = filename;

			int n = 1;
			while (File.Exists (savename)) {
				savename = String.Concat (filename, ".", n.ToString ());
				n++;
			}
			
			Console.WriteLine ("{0}\tGET {1}", DateTime.UtcNow.TimeOfDay, url);
			Console.WriteLine ("\tsaving to: {0}", savename);

			long size = -1;
			DateTime start = DateTime.UtcNow;
			try {
				size = mget.Get (url, savename);
			}
			catch (Exception e) {
				Console.WriteLine ("transfer incomplete due to exception: {0}", e);
				status++;
			}
			finally {
				Console.WriteLine ("completed {0} bytes in {1}.", size, (DateTime.UtcNow - start));
			}
		}

		return status;
	}
}
