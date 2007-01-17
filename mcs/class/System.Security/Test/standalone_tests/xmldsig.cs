//
// xmldsig.cs: XML Digital Signature Tests
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

using Mono.Security.X509;

public class MyClass {
	static int valid = 0;
	static int invalid = 0;
	static int error = 0;
	static int skip = 0;

	static bool exc14n;
	static bool hmacmd5;

	static bool useDecentReader;

	public static void Main(string [] args) 
	{
		foreach (string arg in args)
			if (arg == "--decent-reader")
				useDecentReader = true;
		try {
			// automagically ajust tests to run depending on system config
			exc14n = (CryptoConfig.CreateFromName ("http://www.w3.org/2001/10/xml-exc-c14n#WithComments") != null);
			hmacmd5 = (CryptoConfig.CreateFromName ("HMACMD5") != null);

			Console.WriteLine ("MERLIN");
			Merlin ();
			Console.WriteLine ();
			// Non working on MS runtime;
			// they have insufficient support for namespaces.
 			Console.WriteLine ("PHAOS");
			Phaos ();
		}
		catch (Exception ex) {
			Console.WriteLine (ex);
		}
		finally {
			Console.WriteLine ();
			Console.WriteLine ("TOTAL VALID   {0}", valid);
			Console.WriteLine ("TOTAL INVALID {0}", invalid);
			Console.WriteLine ("TOTAL ERROR   {0}", error);
			Console.WriteLine ("TOTAL SKIP    {0}", skip);

			Console.WriteLine ("Finished.");
		}
	}

	static Mono.Security.X509.X509Certificate LoadCertificate (string filename) 
	{
		Mono.Security.X509.X509Certificate mx = null;
		if (File.Exists (filename)) {
			using (FileStream fs = File.OpenRead (filename)) {
				byte[] data = new byte [fs.Length];
				fs.Read (data, 0, data.Length);
				mx = new Mono.Security.X509.X509Certificate (data);
			}
		}
		return mx;
	}

	static string GetPath (string dir, string name) 
	{
		string path = Path.GetDirectoryName (dir);
		path = Path.Combine (path, "certs");
		path = Path.Combine (path, name);
		return path;
	}

	static TextReader GetReader (string filename)
	{
		XmlResolver resolver = new XmlUrlResolver ();
		Stream stream = resolver.GetEntity (resolver.ResolveUri (null, filename), null, typeof (Stream)) as Stream;
		if (useDecentReader)
			return new XmlSignatureStreamReader (
				new StreamReader (stream));
		else
			return new StreamReader (stream);
	}

	static void Symmetric (string filename, byte[] key) 
	{
		string shortName = Path.GetFileName (filename);

		XmlDocument doc = new XmlDocument ();
		doc.PreserveWhitespace = true;
		XmlTextReader xtr = new XmlTextReader (GetReader (filename));
		XmlValidatingReader xvr = new XmlValidatingReader (xtr);
		xtr.Normalization = true;
		doc.Load (xvr);
                
		try {
			XmlNodeList nodeList = doc.GetElementsByTagName ("Signature", SignedXml.XmlDsigNamespaceUrl);
			XmlElement signature = (XmlElement) nodeList [0];

			SignedXml s = new SignedXml ();
			s.LoadXml (signature);

			HMACSHA1 mac = new HMACSHA1 (key);
			if (s.CheckSignature (mac)) {
				Console.WriteLine ("valid {0}", shortName);
				valid++;
			}
			else {
				Console.WriteLine ("INVALID {0}", shortName);
				invalid++;
			}
		}
		catch (Exception ex) {
			Console.WriteLine ("EXCEPTION " + shortName + " " + ex);
			error++;
		}
	}

	static void Asymmetric (string filename) 
	{
		string shortName = Path.GetFileName (filename);

		XmlDocument doc = new XmlDocument ();
		XmlTextReader xtr = new XmlTextReader (GetReader (filename));
		XmlValidatingReader xvr = new XmlValidatingReader (xtr);
		xtr.Normalization = true;
		doc.PreserveWhitespace = true;
		doc.Load (xvr);

		try {
			SignedXml s = null;
			if (filename.IndexOf ("enveloped") >= 0)
				s = new SignedXml (doc);
			else if (filename.IndexOf ("signature-big") >= 0)
				s = new SignedXml (doc);
			else
				s = new SignedXml ();

			XmlNodeList nodeList = doc.GetElementsByTagName ("Signature", "http://www.w3.org/2000/09/xmldsig#");
			s.LoadXml ((XmlElement) nodeList [0]);
				
#if false // wanna dump?
Console.WriteLine ("\n\nFilename : " + fi.Name);
DumpSignedXml (s);
#endif
			// MS doesn't extract the public key out of the certificates
			// http://www.dotnet247.com/247reference/a.aspx?u=http://www.kbalertz.com/Feedback_320602.aspx
			Mono.Security.X509.X509Certificate mx = null;
			foreach (KeyInfoClause kic in s.KeyInfo) {
				if (kic is KeyInfoX509Data) {
					KeyInfoX509Data kix = (kic as KeyInfoX509Data);
					if ((kix.Certificates != null) && (kix.Certificates.Count > 0)) {
						System.Security.Cryptography.X509Certificates.X509Certificate x509 = (System.Security.Cryptography.X509Certificates.X509Certificate) kix.Certificates [0];
						byte[] data = x509.GetRawCertData ();
						mx = new Mono.Security.X509.X509Certificate (data);
					}
				}
			}

			// special cases
			// 1- Merlin's certificate resolution (manual)
			// 2- Phaos (because Fx doesn't support RetrievalMethod
			switch (shortName) {
				case "signature-keyname.xml":
					mx = LoadCertificate (GetPath (filename, "lugh.crt"));
					break;
				case "signature-retrievalmethod-rawx509crt.xml":
					mx = LoadCertificate (GetPath (filename, "balor.crt"));
					break;
				case "signature-x509-is.xml":
					mx = LoadCertificate (GetPath (filename, "macha.crt"));
					break;
				case "signature-x509-ski.xml":
					mx = LoadCertificate (GetPath (filename, "nemain.crt"));
					break;
				case "signature-x509-sn.xml":
					mx = LoadCertificate (GetPath (filename, "badb.crt"));
					break;
				// Phaos
				case "signature-big.xml":
				case "signature-rsa-manifest-x509-data-issuer-serial.xml":
				case "signature-rsa-manifest-x509-data-ski.xml":
				case "signature-rsa-manifest-x509-data-subject-name.xml":
				case "signature-rsa-detached-xslt-transform-retrieval-method.xml":
					mx = LoadCertificate (GetPath (filename, "rsa-cert.der"));
					break;
				case "signature-rsa-detached-xslt-transform-bad-retrieval-method.xml":
					mx = LoadCertificate (GetPath (filename, "dsa-ca-cert.der"));
					break;
				default:
					break;
			}

			bool result = false;
			if (mx != null) {
				if (mx.RSA != null) {
					result = s.CheckSignature (mx.RSA);
				}
				else if (mx.DSA != null) {
					result = s.CheckSignature (mx.DSA);
				}
			}
			else {
				// use a key existing in the document
				result = s.CheckSignature ();
			}

			if (result) {
				Console.WriteLine ("valid " + shortName);
				valid++;
			}
			else {
				Console.WriteLine ("INVALID {0}", shortName);
				invalid++;
			}
		} 
		catch (Exception ex) {
			Console.WriteLine ("EXCEPTION " + shortName + " " + ex);
			error++;
		}
	}

	static void Merlin () 
	{
		// see README
		byte[] key = Encoding.ASCII.GetBytes ("secret");

		foreach (FileInfo fi in new DirectoryInfo ("merlin-xmldsig-twenty-three").GetFiles ("signature-*.xml")) {
			if (fi.Name.IndexOf ("hmac") >= 0) {
				Symmetric (fi.FullName, key);
			}
			else {
				Asymmetric (fi.FullName);
			}
		}
	}

	static void Phaos ()
	{
		// see README
		byte[] key = Encoding.ASCII.GetBytes ("test");	

		// some documents references other documents in the directory
		Directory.SetCurrentDirectory ("phaos-xmldsig-three");
		foreach (FileInfo fi in new DirectoryInfo (".").GetFiles ("signature-*.xml")) {
#if !NET_2_0
			if ((fi.Name.IndexOf ("exclusive") >= 0) && (!exc14n)) {
				Console.WriteLine ("NOT RUN: " + fi.Name + " : System.Security.dll cannot validate exclusive-c14n.");
				skip++;
				continue;
			}
#endif
			if ((fi.Name.IndexOf ("md5") >= 0) && (!hmacmd5)) {
				Console.WriteLine ("NOT RUN: " + fi.Name + " : System.Security.dll doesn't support HMAC-MD5.");
				skip++;
				continue;
			}
			if (fi.Name.IndexOf ("hmac") >= 0) {
				Symmetric (fi.FullName, key);
			}
			else {
				Asymmetric (fi.FullName);
			}
		}
		// return home before next tests
		Directory.SetCurrentDirectory ("..");
	}

	// dump methods under construction ;-)

	static void DumpSignedXml (SignedXml s) 
	{
		Console.WriteLine ("*** SignedXml ***");
		Console.WriteLine (s.SigningKeyName);
		Console.WriteLine (s.SigningKey);
		if (s.Signature != null)
			DumpSignature (s.Signature);
		if (s.SignedInfo != null)
			DumpSignedInfo (s.SignedInfo);
		Console.WriteLine (s.SignatureMethod);
		Console.WriteLine (s.SignatureLength);
		Console.WriteLine (s.SignatureValue);
		if (s.KeyInfo != null)
			DumpKeyInfo (s.KeyInfo);
	}

	static void DumpSignature (Signature s) 
	{
		Console.WriteLine ("*** Signature ***");
		Console.WriteLine ("Id: " + s.Id);
		if (s.KeyInfo != null)
			DumpKeyInfo (s.KeyInfo);
		Console.WriteLine ("ObjectList: " + s.ObjectList);
		Console.WriteLine ("SignatureValue: " + s.SignatureValue);
		if (s.SignedInfo != null)
			DumpSignedInfo (s.SignedInfo);
	}

	static void DumpSignedInfo (SignedInfo s) 
	{
		Console.WriteLine ("*** SignedInfo ***");
		Console.WriteLine ("CanonicalizationMethod: " + s.CanonicalizationMethod);
		Console.WriteLine ("Id: " + s.Id);
		Console.WriteLine ("References: " + s.References);
		Console.WriteLine ("SignatureLength: " + s.SignatureLength);
		Console.WriteLine ("SignatureMethod: " + s.SignatureMethod);
	}

	static void DumpKeyInfo (KeyInfo ki) 
	{
		Console.WriteLine ("*** KeyInfo ***" + ki);
		Console.WriteLine ("Id: " + ki.Id);
		Console.WriteLine ("Count: " + ki.Count);
		foreach (KeyInfoClause kic in ki)
			DumpKeyInfoClause (kic);
	}

	static void DumpKeyInfoClause (KeyInfoClause kic) 
	{
		KeyInfoName kn = kic as KeyInfoName;
		if (kn != null) {
			Console.WriteLine ("*** KeyInfoName ***");
			Console.WriteLine ("Value: " + kn.Value);
			return;
		}
		KeyInfoX509Data k509 = kic as KeyInfoX509Data;
		if (k509 != null) {
			Console.WriteLine ("*** KeyInfoX509Data ***");
			Console.WriteLine ("Certificates : " + k509.Certificates);
			Console.WriteLine ("CRL : " + k509.CRL);
			Console.WriteLine ("IssuerSerials : " + k509.IssuerSerials);
			Console.WriteLine ("SubjectKeyIds : " + k509.SubjectKeyIds);
			Console.WriteLine ("SubjectNames : " + k509.SubjectNames);
			return;
		}
	}
}

class MySignedXml : SignedXml
{
	public void TestKey ()
	{
		Console.WriteLine (GetPublicKey () == null);
	}
}

// below is a copy from our System.Security.dll source.
	internal class XmlSignatureStreamReader : TextReader
	{
		TextReader source;
		int cache = int.MinValue;

		public XmlSignatureStreamReader (TextReader input)
		{
			source =input;
		}

		public override void Close ()
		{
			source.Close ();
		}

		public override int Peek ()
		{
			if (cache != int.MinValue)
				return cache;
			cache = source.Read ();
			if (cache != '\r')
				return cache;
			// cache must be '\r' here.
			if (source.Peek () != '\n')
				return '\r';
			// Now Peek() returns '\n', so clear cache.
			cache = int.MinValue;
			return '\n';
		}

		public override int Read ()
		{
			if (cache != int.MinValue) {
				int ret = cache;
				cache = int.MinValue;
				return ret;
			}
			int i = source.Read ();
			if (i != '\r')
				return i;
			// read one more char (after '\r')
			cache = source.Read ();
			if (cache != '\n')
				return '\r';
			cache = int.MinValue;
			return '\n';
		}

		public override int ReadBlock (
			[In, Out] char [] buffer, int index, int count)
		{
			char [] tmp = new char [count];
			source.ReadBlock (tmp, 0, count);
			int j = index;
			for (int i = 0; i < count; j++) {
				if (tmp [i] == '\r') {
					if (++i < tmp.Length && tmp [i] == '\n')
						buffer [j] = tmp [i++];
					else
						buffer [j] = '\r';
				}
				else
					buffer [j] = tmp [i];
			}
			while (j < count) {
				int d = Read ();
				if (d < 0)
					break;
				buffer [j++] = (char) d;
			}
			return j;
		}

		public override string ReadLine ()
		{
			return source.ReadLine ().Replace ("\r\n", "\n");
		}

		public override string ReadToEnd ()
		{
			return source.ReadToEnd ().Replace ("\r\n", "\n");
		}

	}

