//
// CertMgr.cs: Certificate Manager clone tool (CLI version)
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using SSCX = System.Security.Cryptography.X509Certificates;
using System.Text;

using Mono.Security.Authenticode;
using Mono.Security.Cryptography;
using Mono.Security.X509;
using Mono.Security.Protocol.Tls;

[assembly: AssemblyTitle ("Mono Certificate Manager")]
[assembly: AssemblyDescription ("Manage X.509 certificates and CRL from stores.")]

namespace Mono.Tools {

	class CertificateManager {

		static private void Header () 
		{
			Console.WriteLine (new AssemblyInfo ().ToString ());
		}

		static private void Help () 
		{
			Console.WriteLine ("Usage: certmgr [action] [object-type] [options] store [filename]");
			Console.WriteLine ("   or: certmgr -list object-type [options] store");
			Console.WriteLine ("   or: certmgr -del object-type [options] store certhash");
			Console.WriteLine ("   or: certmgr -ssl [options] url");
			Console.WriteLine ();
			Console.WriteLine ("actions");
			Console.WriteLine ("\t-add\tAdd a certificate, CRL or CTL to specified store");
			Console.WriteLine ("\t-del\tRemove a certificate, CRL or CTL to specified store");
			Console.WriteLine ("\t-put\tCopy a certificate, CRL or CTL from a store to a file");
			Console.WriteLine ("\t-list\tList certificates, CRL ot CTL in the specified store.");
			Console.WriteLine ("\t-ssl\tDownload and add certificates from an SSL session");
			Console.WriteLine ("object types");
			Console.WriteLine ("\t-c\tadd/del/put certificates");
			Console.WriteLine ("\t-crl\tadd/del/put certificate revocation lists");
			Console.WriteLine ("\t-ctl\tadd/del/put certificate trust lists [unsupported]");
			Console.WriteLine ("other options");
			Console.WriteLine ("\t-m\tuse the machine certificate store (default to user)");
			Console.WriteLine ("\t-v\tverbose mode (display status for every steps)");
			Console.WriteLine ("\t-?\th[elp]\tDisplay this help message");
			Console.WriteLine ();
		}

		static string GetCommand (string arg) 
		{
			if ((arg == null) || (arg.Length < 1))
				return null;

			switch (arg [0]) {
				case '/':
					return arg.Substring (1).ToUpper ();
				case '-':
					if (arg.Length < 2)
						return null;
					int n = ((arg [1] == '-') ? 2 : 1);
					return arg.Substring (n).ToUpper ();
				default:
					return arg;
			}
		}

		enum Action {
			None,
			Add,
			Delete,
			Put,
			List,
			Ssl
		}

		static Action GetAction (string arg) 
		{
			Action action = Action.None;
			switch (GetCommand (arg)) {
				case "ADD":
					action = Action.Add;
					break;
				case "DEL":
				case "DELETE":
					action = Action.Delete;
					break;
				case "PUT":
					action = Action.Put;
					break;
				case "LST":
				case "LIST":
					action = Action.List;
					break;
				case "SSL":
				case "TLS":
					action = Action.Ssl;
					break;
			}
			return action;
		}

		enum ObjectType {
			None,
			Certificate,
			CRL,
			CTL
		}

		static ObjectType GetObjectType (string arg) 
		{
			ObjectType type = ObjectType.None;
			switch (GetCommand (arg)) {
				case "C":
				case "CERT":
				case "CERTIFICATE":
					type = ObjectType.Certificate;
					break;
				case "CRL":
					type = ObjectType.CRL;
					break;
				case "CTL":
					type = ObjectType.CTL;
					break;
			}
			return type;
		}

		static X509Store GetStoreFromName (string storeName, bool machine) 
		{
			X509Stores stores = ((machine) ? X509StoreManager.LocalMachine : X509StoreManager.CurrentUser);
			X509Store store = null;
			switch (storeName) {
				case X509Stores.Names.Personal:
					return stores.Personal;
				case X509Stores.Names.OtherPeople:
					return stores.OtherPeople;
				case X509Stores.Names.IntermediateCA:
					return stores.IntermediateCA;
				case "Root": // special case (same as trusted root)
				case X509Stores.Names.TrustedRoot:
					return stores.TrustedRoot;
				case X509Stores.Names.Untrusted:
					return stores.Untrusted;
			}
			return store;
		}

		static byte[] PEM (string type, byte[] data) 
		{
			string pem = Encoding.ASCII.GetString (data);
			string header = String.Format ("-----BEGIN {0}-----", type);
			string footer = String.Format ("-----END {0}-----", type);
			int start = pem.IndexOf (header) + header.Length;
			int end = pem.IndexOf (footer, start);
			string base64 = pem.Substring (start, (end - start));
			return Convert.FromBase64String (base64);
		}

		static X509CertificateCollection LoadCertificates (string filename) 
		{
			X509Certificate x509 = null;
			X509CertificateCollection coll = new X509CertificateCollection ();
			switch (Path.GetExtension (filename).ToUpper ()) {
				case ".P7B":
				case ".SPC":
					SoftwarePublisherCertificate spc = SoftwarePublisherCertificate.CreateFromFile (filename);
					coll.AddRange (spc.Certificates);
					spc = null;
					break;
				case ".CER":
				case ".CRT":
					using (FileStream fs = File.OpenRead (filename)) {
						byte[] data = new byte [fs.Length];
						fs.Read (data, 0, data.Length);
						if (data [0] != 0x30) {
							// maybe it's ASCII PEM base64 encoded ?
							data = PEM ("CERTIFICATE", data);
						}
						if (data != null)
							x509 = new X509Certificate (data);
					}
					if (x509 != null)
						coll.Add (x509);
					break;
				case ".P12":
				case ".PFX":
					// TODO - support PKCS12 with passwords
					PKCS12 p12 = PKCS12.LoadFromFile (filename);
					coll.AddRange (p12.Certificates);
					p12 = null;
					break;
				default:
					Console.WriteLine ("Unknown file extension: {0}", 
						Path.GetExtension (filename));
					break;
			}
			return coll;
		}

		static ArrayList LoadCRLs (string filename) 
		{
			X509Crl crl = null;
			ArrayList list = new ArrayList ();
			switch (Path.GetExtension (filename).ToUpper ()) {
				case ".P7B":
				case ".SPC":
					SoftwarePublisherCertificate spc = SoftwarePublisherCertificate.CreateFromFile (filename);
					list.AddRange (spc.Crls);
					spc = null;
					break;
				case ".CRL":
					using (FileStream fs = File.OpenRead (filename)) {
						byte[] data = new byte [fs.Length];
						fs.Read (data, 0, data.Length);
						crl = new X509Crl (data);
					}
					list.Add (crl);
					break;
				default:
					Console.WriteLine ("Unknown file extension: {0}", 
						Path.GetExtension (filename));
					break;
			}
			return list;
		}

		static void Add (ObjectType type, X509Store store, string file, bool verbose) 
		{
			switch (type) {
				case ObjectType.Certificate:
					X509CertificateCollection coll = LoadCertificates (file);
					foreach (X509Certificate x509 in coll) {
						store.Import (x509);
					}
					Console.WriteLine ("{0} certificate(s) added to store {1}.", 
						coll.Count, store.Name);
					break;
				case ObjectType.CRL:
					ArrayList list = LoadCRLs (file);
					foreach (X509Crl crl in list) {
						store.Import (crl);
					}
					Console.WriteLine ("{0} CRL(s) added to store {1}.", 
						list.Count, store.Name);
					break;
				default:
					throw new NotSupportedException (type.ToString ());
			}
		}

		static void Delete (ObjectType type, X509Store store, string hash, bool verbose) 
		{
			switch (type) {
				case ObjectType.Certificate:
					foreach (X509Certificate x509 in store.Certificates) {
						if (hash == CryptoConvert.ToHex (x509.Hash)) {
							store.Remove (x509);
							Console.WriteLine ("Certificate removed from store.");
							return;
						}
					}
					break;
				case ObjectType.CRL:
					foreach (X509Crl crl in store.Crls) {
						if (hash == CryptoConvert.ToHex (crl.Hash)) {
							store.Remove (crl);
							Console.WriteLine ("CRL removed from store.");
							return;
						}
					}
					break;
				default:
					throw new NotSupportedException (type.ToString ());
			}
		}

		static void Put (ObjectType type, X509Store store, string file, bool verbose) 
		{
			throw new NotImplementedException ("Put not yet supported");
/*			switch (type) {
				case ObjectType.Certificate:
					break;
				case ObjectType.CRL:
					// TODO
					break;
				default:
					throw new NotSupportedException (type.ToString ());
			}*/
		}

		static void DisplayCertificate (X509Certificate x509, bool verbose)
		{
			Console.WriteLine ("{0}X.509 v{1} Certificate", (x509.IsSelfSigned ? "Self-signed " : String.Empty), x509.Version);
			Console.WriteLine ("  Serial Number: {0}", CryptoConvert.ToHex (x509.SerialNumber));
			Console.WriteLine ("  Issuer Name:   {0}", x509.IssuerName);
			Console.WriteLine ("  Subject Name:  {0}", x509.SubjectName);
			Console.WriteLine ("  Valid From:    {0}", x509.ValidFrom);
			Console.WriteLine ("  Valid Until:   {0}", x509.ValidUntil);
			Console.WriteLine ("  Unique Hash:   {0}", CryptoConvert.ToHex (x509.Hash));
			if (verbose) {
				Console.WriteLine ("  Key Algorithm:        {0}", x509.KeyAlgorithm);
				Console.WriteLine ("  Algorithm Parameters: {0}", (x509.KeyAlgorithmParameters == null) ? "None" :
					CryptoConvert.ToHex (x509.KeyAlgorithmParameters));
				Console.WriteLine ("  Public Key:           {0}", CryptoConvert.ToHex (x509.PublicKey));
				Console.WriteLine ("  Signature Algorithm:  {0}", x509.SignatureAlgorithm);
				Console.WriteLine ("  Algorithm Parameters: {0}", (x509.SignatureAlgorithmParameters == null) ? "None" :
					CryptoConvert.ToHex (x509.SignatureAlgorithmParameters));
				Console.WriteLine ("  Signature:            {0}", CryptoConvert.ToHex (x509.Signature));
			}
			Console.WriteLine ();
		}

		static void List (ObjectType type, X509Store store, string file, bool verbose) 
		{
			switch (type) {
				case ObjectType.Certificate:
					foreach (X509Certificate x509 in store.Certificates) {
						DisplayCertificate (x509, verbose);
					}
					break;
				case ObjectType.CRL:
					// TODO
					break;
				default:
					throw new NotSupportedException (type.ToString ());
			}
		}

		static X509CertificateCollection GetCertificatesFromSslSession (string url) 
		{
			Uri uri = new Uri (url);
			IPHostEntry host = Dns.Resolve (uri.Host);
			IPAddress ip = host.AddressList [0];
			Socket socket = new Socket (ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			socket.Connect (new IPEndPoint (ip, uri.Port));
			NetworkStream ns = new NetworkStream (socket, false);
			SslClientStream ssl = new SslClientStream (ns, uri.Host, false, Mono.Security.Protocol.Tls.SecurityProtocolType.Default, null);
			ssl.ServerCertValidationDelegate += new CertificateValidationCallback (CertificateValidation);

			try {
				// we don't really want to write to the server (as we don't know
				// the protocol it using) but we must send something to be sure the
				// SSL handshake is done (so we receive the X.509 certificates).
				StreamWriter sw = new StreamWriter (ssl);
				sw.WriteLine (Environment.NewLine);
				sw.Flush ();
				socket.Poll (30000, SelectMode.SelectRead);
			}
			finally {
				socket.Close ();
			}

			// we need a little reflection magic to get this information
			PropertyInfo pi = typeof (SslStreamBase).GetProperty ("ServerCertificates", BindingFlags.Instance | BindingFlags.NonPublic);
			if (pi == null) {
				Console.WriteLine ("Sorry but you need a newer version of Mono.Security.dll to use this feature.");
				return null;
			}
			return (X509CertificateCollection) pi.GetValue (ssl, null);
		}

		static bool CertificateValidation (SSCX.X509Certificate certificate, int[] certificateErrors)
		{
			// the main reason to download it is that it's not trusted
			return true;
			// OTOH we ask user confirmation before adding certificates into the stores
		}

		static void Ssl (string host, bool machine, bool verbose) 
		{
			if (verbose) {
				Console.WriteLine ("Importing certificates from '{0}' into the {1} stores.",
					host, machine ? "machine" : "user");
			}
			int n=0;

			X509CertificateCollection coll = GetCertificatesFromSslSession (host);
			if (coll != null) {
				X509Store store = null;
				// start by the end (root) so we can stop adding them anytime afterward
				for (int i = coll.Count - 1; i >= 0; i--) {
					X509Certificate x509 = coll [i];
					bool selfsign = false;
					bool failed = false;
					try {
						selfsign = x509.IsSelfSigned;
					}
					catch {
						// sadly it's hard to interpret old certificates with MD2
						// without manually changing the machine.config file
						failed = true;
					}

					if (selfsign) {
						// this is a root
						store = GetStoreFromName (X509Stores.Names.TrustedRoot, machine);
					} else if (i == 0) {
						// server certificate isn't (generally) an intermediate CA
						store = GetStoreFromName (X509Stores.Names.OtherPeople, machine);
					} else {
						// all other certificates should be intermediate CA
						store = GetStoreFromName (X509Stores.Names.IntermediateCA, machine);
					}

					Console.WriteLine ("{0}{1}X.509 Certificate v{2}", 	
						Environment.NewLine,
						selfsign ? "Self-signed " : String.Empty,
						x509.Version);
					Console.WriteLine ("   Issued from: {0}", x509.IssuerName);
					Console.WriteLine ("   Issued to:   {0}", x509.SubjectName);
					Console.WriteLine ("   Valid from:  {0}", x509.ValidFrom);
					Console.WriteLine ("   Valid until: {0}", x509.ValidUntil);

					if (!x509.IsCurrent)
						Console.WriteLine ("   *** WARNING: Certificate isn't current ***");
					if ((i > 0) && !selfsign) {
						X509Certificate signer = coll [i-1];
						bool signed = false;
						try {
							if (signer.RSA != null) {
								signed = x509.VerifySignature (signer.RSA);
							} else if (signer.DSA != null) {
								signed = x509.VerifySignature (signer.DSA);
							} else {
								Console.WriteLine ("   *** WARNING: Couldn't not find who signed this certificate ***");
								signed = true; // skip next warning
							}

							if (!signed)
								Console.WriteLine ("   *** WARNING: Certificate signature is INVALID ***");
						}
						catch {
							failed = true;
						}
					}
					if (failed) {
						Console.WriteLine ("   *** ERROR: Couldn't decode certificate properly ***");
						Console.WriteLine ("   *** try 'man certmgr' for additional help or report to bugzilla.novell.com ***");
						break;
					}

					if (store.Certificates.Contains (x509)) {
						Console.WriteLine ("This certificate is already in the {0} store.", store.Name);
					} else {
						Console.Write ("Import this certificate into the {0} store ?", store.Name);
						string answer = Console.ReadLine ().ToUpper ();
						if ((answer == "YES") || (answer == "Y")) {
							store.Import (x509);
							n++;
						} else {
							if (verbose) {
								Console.WriteLine ("Certificate not imported into store {0}.", 
									store.Name);
							}
							break;
						}
					}
				}
			}

			Console.WriteLine ();
			if (n == 0) {
				Console.WriteLine ("No certificate were added to the stores.");
			} else {
				Console.WriteLine ("{0} certificate{1} added to the stores.", 
					n, (n == 1) ? String.Empty : "s");
			}
		}

		[STAThread]
		static void Main (string[] args)
		{
			Header ();
			if (args.Length < 2) {
				Help ();
				return;
			}

			Action action = GetAction (args [0]);
			ObjectType type = ObjectType.None;

			int n = 1;
			if (action != Action.Ssl) {
				type = GetObjectType (args [n]);
				if (type != ObjectType.None)
					n++;
			}
			
			bool verbose = (GetCommand (args [n]) == "V");
			if (verbose)
				n++;
			bool machine = (GetCommand (args [n]) == "M");
			if (machine)
				n++;

			X509Store store = null;
			string storeName = null;
			if (action != Action.Ssl) {
				if ((action == Action.None) || (type == ObjectType.None)) {
					Help ();
					return;
				}
				if (type == ObjectType.CTL) {
					Console.WriteLine ("CTL are not supported");
					return;
				}

				storeName = args [n++];
				store = GetStoreFromName (storeName, machine);
				if (store == null) {
					Console.WriteLine ("Invalid Store: {0}", storeName);
					Console.WriteLine ("Valid stores are: {0}, {1}, {2}, {3} and {4}",
						X509Stores.Names.Personal,
						X509Stores.Names.OtherPeople, 
						X509Stores.Names.IntermediateCA, 
						X509Stores.Names.TrustedRoot, 
						X509Stores.Names.Untrusted);
					return;
				}
			}

			string file = (n < args.Length) ? args [n] : null;

			// now action!
			try {
				switch (action) {
				case Action.Add:
					Add (type, store, file, verbose);
					break;
				case Action.Delete:
					Delete (type, store, file, verbose);
					break;
				case Action.Put:
					Put (type, store, file, verbose);
					break;
				case Action.List:
					List (type, store, file, verbose);
					break;
				case Action.Ssl:
					Ssl (file, machine, verbose);
					break;
				default:
					throw new NotSupportedException (action.ToString ());
				}
			}
			catch (UnauthorizedAccessException uae) {
				Console.WriteLine ("Access to the {0} '{1}' certificate store has been denied.", 
					(machine ? "machine" : "user"), storeName);
				if (verbose) {
					Console.WriteLine (uae);
				}
			}
		}
	}
}
