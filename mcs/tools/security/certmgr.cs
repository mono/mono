//
// CertMgr.cs: Certificate Manager clone tool (CLI version)
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using Mono.Security.Authenticode;
using Mono.Security.X509;

[assembly: AssemblyTitle ("Mono Certificate Manager")]
[assembly: AssemblyDescription ("Add/Remove certificates and CRL from stores")]

namespace Mono.Tools {

	class CertificateManager {

		static private void Header () 
		{
			Assembly a = Assembly.GetExecutingAssembly ();
			AssemblyName an = a.GetName ();

			object [] att = a.GetCustomAttributes (typeof (AssemblyTitleAttribute), false);
			string title = ((att.Length > 0) ? ((AssemblyTitleAttribute) att [0]).Title : "Mono Certificate Manager");

			att = a.GetCustomAttributes (typeof (AssemblyCopyrightAttribute), false);
			string copyright = ((att.Length > 0) ? ((AssemblyCopyrightAttribute) att [0]).Copyright : "");

			Console.WriteLine ("{0} {1}", title, an.Version.ToString ());
			Console.WriteLine ("{0}{1}", copyright, Environment.NewLine);
		}

		static private void Help () 
		{
			Console.WriteLine ("Usage: certmgr [action] [object type] [options] store [filename]{0}", Environment.NewLine);
			Console.WriteLine ("actions");
			Console.WriteLine ("\t-add\tAdd a certificate, CRL or CTL to specified store");
			Console.WriteLine ("\t-del\tRemove a certificate, CRL or CTL to specified store");
			Console.WriteLine ("\t-put\tCopy a certificate, CRL or CTL from a store to a file");
			Console.WriteLine ("object types");
			Console.WriteLine ("\t-c\tadd/del/put certificates");
			Console.WriteLine ("\t-crl\tadd/del/put certificate revocation lists");
			Console.WriteLine ("\t-ctl\tadd/del/put certificate trust lists [unsupported]");
			Console.WriteLine ("other options");
			Console.WriteLine ("\t-m\tuse the machine certificate store (default to user)");
			Console.WriteLine ("\t-v\tverbose mode (display status for every steps)");
			Console.WriteLine ("\t-?\th[elp]\tDisplay this help message");
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
			Put
		}

		static Action GetAction (string arg) 
		{
			Action action = Action.None;
			switch (GetCommand (arg)) {
				case "ADD":
					action = Action.Add;
					break;
				case "DEL":
					action = Action.Delete;
					break;
				case "PUT":
					action = Action.Put;
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
						// TODO
						throw new NotImplementedException ("Add CRL not yet supported");
					}
					break;
				default:
					throw new NotSupportedException (type.ToString ());
			}
		}

		static void Delete (ObjectType type, X509Store store, string file, bool verbose) 
		{
			throw new NotImplementedException ("Delete not yet supported");
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

		[STAThread]
		static void Main (string[] args)
		{
			Header ();
			if (args.Length < 4) {
				Help ();
				return;
			}

			Action action = GetAction (args [0]);
			ObjectType type = GetObjectType (args [1]);

			if ((action == Action.None) || (type == ObjectType.None)) {
				Help ();
				return;
			}
			if (type == ObjectType.CTL) {
				Console.WriteLine ("CTL are not supported");
				return;
			}

			int n = 2;
			bool verbose = (GetCommand (args [n]) == "V");
			if (verbose)
				n++;
			bool machine = (GetCommand (args [n]) == "M");
			if (machine)
				n++;

			string storeName = args [n++];
			X509Store store = GetStoreFromName (storeName, machine);
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

			string file = args [n];

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
				default:
					throw new NotSupportedException (action.ToString ());
				}
			}
			catch (UnauthorizedAccessException) {
				Console.WriteLine ("Access to the {0} '{1}' certificate store has been denied.", 
					(machine ? "machine" : "user"), storeName);
			}
		}
	}
}
