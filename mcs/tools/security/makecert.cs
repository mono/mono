//
// makecert.cs: makecert clone tool
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

using Mono.Security.Authenticode;
using Mono.Security.X509;
using Mono.Security.X509.Extensions;

[assembly: AssemblyTitle("Mono MakeCert")]
[assembly: AssemblyDescription("X.509 Certificate Builder")]

namespace Mono.Tools {

	class MakeCert {

		static private void Header () 
		{
			Console.WriteLine (new AssemblyInfo ().ToString ());
		}

		static private void Help () 
		{
			Console.WriteLine ("Usage: makecert [options] certificate{0}", Environment.NewLine);
			Console.WriteLine (" -# num{0}\tCertificate serial number", Environment.NewLine);
			Console.WriteLine (" -n dn{0}\tSubject Distinguished Name", Environment.NewLine);
			Console.WriteLine (" -in dn{0}\tIssuer Distinguished Name", Environment.NewLine);
			Console.WriteLine (" -r{0}\tCreate a self-signed (root) certificate", Environment.NewLine);
			Console.WriteLine (" -sv pkvfile{0}\tPrivate key file (.PVK) for the subject (created if missing)", Environment.NewLine);
			Console.WriteLine (" -iv pvkfile{0}\tPrivate key file (.PVK) for the issuer", Environment.NewLine);
			Console.WriteLine (" -ic certfile{0}\tExtract the issuer's name from the specified certificate", Environment.NewLine);
			Console.WriteLine (" -?{0}\thelp (display this help message)", Environment.NewLine);
			Console.WriteLine (" -!{0}\textended help (for advanced options)", Environment.NewLine);
		}

		static private void ExtendedHelp () 
		{
			Console.WriteLine ("Usage: makecert [options] certificate{0}", Environment.NewLine);
			Console.WriteLine (" -a hash\tSelect hash algorithm. Only MD5 and SHA1 (default) are supported.");
			Console.WriteLine (" -b date\tThe date since when the certificate is valid (notBefore).");
			Console.WriteLine (" -cy [authority|end]\tBasic constraints. Select Authority or End-Entity certificate.");
			Console.WriteLine (" -e date\tThe date until when the certificate is valid (notAfter).");
			Console.WriteLine (" -eku oid[,oid]\tAdd some extended key usage OID to the certificate.");
			Console.WriteLine (" -h number\tAdd a path length restriction to the certificate chain.");
			Console.WriteLine (" -in name\tTake the issuer's name from the specified parameter.");
			Console.WriteLine (" -m number\tCertificate validity period (in months).");
			Console.WriteLine (" -p12 pkcs12file password\tCreate a new PKCS#12 file with the specified password.");
			Console.WriteLine (" -?\thelp (display basic message)");
		}

		static X509Certificate LoadCertificate (string filename) 
		{
			FileStream fs = new FileStream (filename, FileMode.Open, FileAccess.Read, FileShare.Read);
			byte[] rawcert = new byte [fs.Length];
			fs.Read (rawcert, 0, rawcert.Length);
			fs.Close ();
			return new X509Certificate (rawcert);
		}

		static void WriteCertificate (string filename, byte[] rawcert) 
		{
			FileStream fs = File.Open (filename, FileMode.Create, FileAccess.Write);
			fs.Write (rawcert, 0, rawcert.Length);
			fs.Close ();
		}

		static string MonoTestRootAgency = "<RSAKeyValue><Modulus>v/4nALBxCE+9JgEC0LnDUvKh6e96PwTpN4Rj+vWnqKT7IAp1iK/JjuqvAg6DQ2vTfv0dTlqffmHH51OyioprcT5nzxcSTsZb/9jcHScG0s3/FRIWnXeLk/fgm7mSYhjUaHNI0m1/NTTktipicjKxo71hGIg9qucCWnDum+Krh/k=</Modulus><Exponent>AQAB</Exponent><P>9jbKxMXEruW2CfZrzhxtull4O8P47+mNsEL+9gf9QsRO1jJ77C+jmzfU6zbzjf8+ViK+q62tCMdC1ZzulwdpXQ==</P><Q>x5+p198l1PkK0Ga2mRh0SIYSykENpY2aLXoyZD/iUpKYAvATm0/wvKNrE4dKJyPCA+y3hfTdgVag+SP9avvDTQ==</Q><DP>ISSjCvXsUfbOGG05eddN1gXxL2pj+jegQRfjpk7RAsnWKvNExzhqd5x+ZuNQyc6QH5wxun54inP4RTUI0P/IaQ==</DP><DQ>R815VQmR3RIbPqzDXzv5j6CSH6fYlcTiQRtkBsUnzhWmkd/y3XmamO+a8zJFjOCCx9CcjpVuGziivBqi65lVPQ==</DQ><InverseQ>iYiu0KwMWI/dyqN3RJYUzuuLj02/oTD1pYpwo2rvNCXU1Q5VscOeu2DpNg1gWqI+1RrRCsEoaTNzXB1xtKNlSw==</InverseQ><D>nIfh1LYF8fjRBgMdAH/zt9UKHWiaCnc+jXzq5tkR8HVSKTVdzitD8bl1JgAfFQD8VjSXiCJqluexy/B5SGrCXQ49c78NIQj0hD+J13Y8/E0fUbW1QYbhj6Ff7oHyhaYe1WOQfkp2t/h+llHOdt1HRf7bt7dUknYp7m8bQKGxoYE=</D></RSAKeyValue>";

		static string defaultIssuer = "CN=Mono Test Root Agency";
		static string defaultSubject = "CN=Poupou's-Software-Factory";

		[STAThread]
		static int Main (string[] args)
		{
			if (args.Length < 1) {
				Header ();
				Console.WriteLine ("ERROR: Missing output filename {0}", Environment.NewLine);
				Help ();
				return -1;
			}

			string fileName = args [args.Length - 1];

			// default values
			byte[] sn = Guid.NewGuid ().ToByteArray ();
			string subject = defaultSubject;
			string issuer = defaultIssuer;
			DateTime notBefore = DateTime.Now;
			DateTime notAfter = new DateTime (643445675990000000); // 12/31/2039 23:59:59Z

			RSA issuerKey = (RSA)RSA.Create ();
			issuerKey.FromXmlString (MonoTestRootAgency);
			RSA subjectKey = (RSA)RSA.Create ();

			bool selfSigned = false;
			string hashName = "SHA1";

			CspParameters subjectParams = new CspParameters ();
			CspParameters issuerParams = new CspParameters ();
			BasicConstraintsExtension bce = null;
			ExtendedKeyUsageExtension eku = null;
			SubjectAltNameExtension alt = null;
			string p12file = null;
			string p12pwd = null;
			X509Certificate issuerCertificate = null;

			Header();
			try {
				int i=0;
				while (i < args.Length) {
					switch (args [i++]) {
						// Basic options
						case "-#":
							// Serial Number
							sn = BitConverter.GetBytes (Convert.ToInt32 (args [i++]));
							break;
						case "-n":
							// Subject Distinguish Name
							subject = args [i++];
							break;
						case "-$":
							// (authenticode) commercial or individual
							// CRITICAL KeyUsageRestriction extension
							// hash algorithm
							string usageRestriction = args [i++].ToLower ();
							switch (usageRestriction) {
								case "commercial":
								case "individual":
									Console.WriteLine ("WARNING: Unsupported deprecated certification extension KeyUsageRestriction not included");
//									Console.WriteLine ("WARNING: ExtendedKeyUsage for codesigning has been included.");
									break;
								default:
									Console.WriteLine ("Unsupported restriction " + usageRestriction);
									return -1;
							}
							break;
						// Extended Options
						case "-a":
							// hash algorithm
							switch (args [i++].ToLower ()) {
								case "sha1":
									hashName = "SHA1";
									break;
								case "md5":
									Console.WriteLine ("WARNING: MD5 is no more safe for this usage.");
									hashName = "MD5";
									break;
								default:
									Console.WriteLine ("Unsupported hash algorithm");
									break;
							}
							break;
						case "-b":
							// Validity / notBefore
							notBefore = DateTime.Parse (args [i++] + " 23:59:59", CultureInfo.InvariantCulture);
							break;
						case "-cy":
							// basic constraints - autority or end-entity
							switch (args [i++].ToLower ()) {
								case "authority":
									if (bce == null)
										bce = new BasicConstraintsExtension ();
									bce.CertificateAuthority = true;
									break;
								case "end":
									// do not include extension
									bce = null;
									break;
								case "both":
									Console.WriteLine ("ERROR: No more supported in X.509");
									return -1;
								default:
									Console.WriteLine ("Unsupported certificate type");
									return -1;
							}
							break;
						case "-d":
							// CN private extension ?
							Console.WriteLine ("Unsupported option");
							break;
						case "-e":
							// Validity / notAfter
							notAfter = DateTime.Parse (args [i++] + " 23:59:59", CultureInfo.InvariantCulture);
							break;
						case "-eku":
							// extendedKeyUsage extension
							char[] sep = { ',' };
							string[] purposes = args [i++].Split (sep);
							if (eku == null)
								eku = new ExtendedKeyUsageExtension ();
							foreach (string purpose in purposes) {
								eku.KeyPurpose.Add (purpose);
							}
							break;
						case "-h":
							// pathLength (basicConstraints)
							// MS use an old basicConstrains (2.5.29.10) which 
							// allows both CA and End-Entity. This is no
							// more supported with 2.5.29.19.
							if (bce == null) {
								bce = new BasicConstraintsExtension ();
								bce.CertificateAuthority = true;
							}
							bce.PathLenConstraint = Convert.ToInt32 (args [i++]);
							break;
						case "-alt":
							if (alt == null) {
								string [] dnsNames = File.ReadAllLines (args [i++]);
								alt = new SubjectAltNameExtension (null, dnsNames, null, null);
							}
							break;
						case "-ic":
							issuerCertificate = LoadCertificate (args [i++]);
							issuer = issuerCertificate.SubjectName;
							break;
						case "-in":
							issuer = args [i++];
							break;
						case "-iv":
							// TODO password
							PrivateKey pvk = PrivateKey.CreateFromFile (args [i++]);
							issuerKey = pvk.RSA;
							break;
						case "-l":
							// link (URL)
							// spcSpAgencyInfo private extension
							Console.WriteLine ("Unsupported option");
							break;
						case "-m":
							// validity period (in months)
							notAfter = notBefore.AddMonths (Convert.ToInt32 (args [i++]));
							break;
						case "-nscp":
							// Netscape's private extensions - NetscapeCertType
							// BasicContraints - End Entity
							Console.WriteLine ("Unsupported option");
							break;
						case "-r":
							selfSigned = true;
							break;
						case "-sc":
							// subject certificate ? renew ?
							Console.WriteLine ("Unsupported option");
							break;
						// Issuer CspParameters options
						case "-ik":
							issuerParams.KeyContainerName = args [i++];
							break;
						case "-iky":
							// select a key in the provider
							string ikn = args [i++].ToLower ();
							switch (ikn) {
								case "signature":
									issuerParams.KeyNumber = 0;
									break;
								case "exchange":
									issuerParams.KeyNumber = 1;
									break;
								default:
									issuerParams.KeyNumber = Convert.ToInt32 (ikn);
									break;
							}
							break;
						case "-ip":
							issuerParams.ProviderName = args [i++];
							break;
						case "-ir":
							switch (args [i++].ToLower ()) {
								case "localmachine":
									issuerParams.Flags = CspProviderFlags.UseMachineKeyStore;
									break;
								case "currentuser":
									issuerParams.Flags = CspProviderFlags.UseDefaultKeyContainer;
									break;
								default:
									Console.WriteLine ("Unknown key store for issuer");
									return -1;
							}
							break;
						case "-is":
							Console.WriteLine ("Unsupported option");
							return -1;
						case "-iy":
							issuerParams.ProviderType = Convert.ToInt32 (args [i++]);
							break;
						// Subject CspParameters Options
						case "-sk":
							subjectParams.KeyContainerName = args [i++];
							break;
						case "-sky":
							// select a key in the provider
							string skn = args [i++].ToLower ();
							switch (skn) {
								case "signature":
									subjectParams.KeyNumber = 0;
									break;
								case "exchange":
									subjectParams.KeyNumber = 1;
									break;
								default:
									subjectParams.KeyNumber = Convert.ToInt32 (skn);
									break;
							}
							break;
						case "-sp":
							subjectParams.ProviderName = args [i++];
							break;
						case "-sr":
							switch (args [i++].ToLower ()) {
								case "localmachine":
									subjectParams.Flags = CspProviderFlags.UseMachineKeyStore;
									break;
								case "currentuser":
									subjectParams.Flags = CspProviderFlags.UseDefaultKeyContainer;
									break;
								default:
									Console.WriteLine ("Unknown key store for subject");
									return -1;
							}
							break;
						case "-ss":
							Console.WriteLine ("Unsupported option");
							return -1;
						case "-sv":
							string pvkFile = args [i++];
							if (File.Exists (pvkFile)) {
								PrivateKey key = PrivateKey.CreateFromFile (pvkFile);
								subjectKey = key.RSA;
							}
							else {
								PrivateKey key = new PrivateKey ();
								key.RSA = subjectKey;
								key.Save (pvkFile);
							}
							break;
						case "-sy":
							subjectParams.ProviderType = Convert.ToInt32 (args [i++]);
							break;
						// Mono Specific Options
						case "-p12":
							p12file = args [i++];
							p12pwd = args [i++];
							break;
						// Other options
						case "-?":
							Help ();
							return 0;
						case "-!":
							ExtendedHelp ();
							return 0;
						default:
							if (i != args.Length) {
								Console.WriteLine ("ERROR: Unknown parameter");
								Help ();
								return -1;
							}
							break;
					}
				}

				// serial number MUST be positive
				if ((sn [0] & 0x80) == 0x80)
					sn [0] -= 0x80;

				if (selfSigned) {
					if (subject != defaultSubject) {
						issuer = subject;
						issuerKey = subjectKey;
					}
					else {
						subject = issuer;
						subjectKey = issuerKey;
					}
				}

				if (subject == null)
					throw new Exception ("Missing Subject Name");

				X509CertificateBuilder cb = new X509CertificateBuilder (3);
				cb.SerialNumber = sn;
				cb.IssuerName = issuer;
				cb.NotBefore = notBefore;
				cb.NotAfter = notAfter;
				cb.SubjectName = subject;
				cb.SubjectPublicKey = subjectKey;
				// extensions
				if (bce != null)
					cb.Extensions.Add (bce);
				if (eku != null)
					cb.Extensions.Add (eku);
				if (alt != null)
					cb.Extensions.Add (alt);
				// signature
				cb.Hash = hashName;
				byte[] rawcert = cb.Sign (issuerKey);

				if (p12file == null) {
					WriteCertificate (fileName, rawcert);
				} else {
					PKCS12 p12 = new PKCS12 ();
					p12.Password = p12pwd;

					ArrayList list = new ArrayList ();
					// we use a fixed array to avoid endianess issues 
					// (in case some tools requires the ID to be 1).
					list.Add (new byte [4] { 1, 0, 0, 0 });
					Hashtable attributes = new Hashtable (1);
					attributes.Add (PKCS9.localKeyId, list);

					p12.AddCertificate (new X509Certificate (rawcert), attributes);
					if (issuerCertificate != null)
						p12.AddCertificate (issuerCertificate);
					p12.AddPkcs8ShroudedKeyBag (subjectKey, attributes);
					p12.SaveToFile (p12file);
				}
				Console.WriteLine ("Success");
				return 0;
			}
			catch (Exception e) {
				Console.WriteLine ("ERROR: " + e.ToString ());
				Help ();
			}
			return 1;
		}
	}
}
