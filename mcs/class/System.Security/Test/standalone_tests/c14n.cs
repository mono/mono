//
// c14n.cs - C14N
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;

public class C14N {

	// default transform
	static string url = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";

	public static void Usage (string error) 
	{
		Console.WriteLine ("C14N - Copyright (C) 2004 Novell.{0}", Environment.NewLine);
		if (error != null) {
			Console.WriteLine ("{0}Error: {1}{0}", Environment.NewLine, error);
		}
		Console.WriteLine ("Usage: c14n input [transform_url] [element]");
		Console.WriteLine ("[input]        \tXML document to canonalize");
		Console.WriteLine ("[transform_url]\tTransformation algorithm URL");
		Console.WriteLine ("               \tDefault is{0}", url);
		Console.WriteLine ("[element]      \tPartial C14N from this element and childs");
	}

	public static void Main (string[] args)
	{
		if (args.Length < 1) {
			Usage (null);
			return;
		}

		string filename = args [0];
		if (!File.Exists (filename)) {
			Usage (String.Format ("Missing file {0}", filename));
			return;
		}

		XmlDocument xml = new XmlDocument ();
		xml.PreserveWhitespace = true;
		xml.Load (filename);

		MemoryStream ms = new MemoryStream ();

		for (int i=1; i < args.Length; i++) {
			if (args [i].StartsWith ("http://")) {
				url = args [i];
			}
			else {
				XmlNodeList xnl = xml.GetElementsByTagName (args [i], SignedXml.XmlDsigNamespaceUrl);
				byte[] si = Encoding.UTF8.GetBytes (xnl [0].OuterXml);
				ms.Write (si, 0, si.Length);
			}
		}

		if (ms.Position == 0) {
			// process the whole document
			xml.Save (ms);
		}
		ms.Position = 0;

		Transform t = (Transform) CryptoConfig.CreateFromName (url);
		if (t == null) {
			Usage (String.Format ("Unknown transformation algorithm {0}", url));
			return;
		}
		t.LoadInput (ms);
		StreamReader sr = new StreamReader ((Stream) t.GetOutput (), Encoding.UTF8);

		Console.Write (sr.ReadToEnd ());
	}
}
