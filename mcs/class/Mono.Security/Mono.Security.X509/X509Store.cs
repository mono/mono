//
// X509Store.cs: Handles a X.509 certificates/CRLs store
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Collections;
using System.IO;
using System.Text;

using Mono.Security.X509.Extensions;

namespace Mono.Security.X509 {

	public class X509Store {

		private string _storePath;
		private X509CertificateCollection _certificates;
		private ArrayList _crls;
		private bool _crl;
		private string _name;

		internal X509Store (string path, bool crl) 
		{
			_storePath = path;
			_crl = crl;
		}

		// properties

		public X509CertificateCollection Certificates {
			get { 
				if (_certificates == null) {
					_certificates = BuildCertificatesCollection (_storePath);
				}
				return _certificates; 
			}
		}

		public ArrayList CRLs {
			get {
				// CRL aren't applicable to all stores
				// but returning null is a little rude
				if (!_crl) {
					_crls = new ArrayList ();
				}
				if (_crls == null) {
					_crls = BuildCRLsCollection (_storePath);
				}
				return _crls; 
			}
		}

		public string Name {
			get {
				if (_name == null) {
					int n = _storePath.LastIndexOf (Path.DirectorySeparatorChar);
					_name = _storePath.Substring (n+1);
				}
				return _name;
			}
		}

		// methods

		public void Clear () 
		{
			if (_certificates != null)
				_certificates.Clear ();
			_certificates = null;
			if (_crls != null)
				_crls.Clear ();
			_crls = null;
		}

		public void Import (X509Certificate certificate) 
		{
			if (!Directory.Exists (_storePath)) {
				Directory.CreateDirectory (_storePath);
			}

			string filename = Path.Combine (_storePath, GetUniqueName (certificate));
			if (!File.Exists (filename)) {
				using (FileStream fs = File.OpenWrite (filename)) {
					byte[] data = certificate.RawData;
					fs.Write (data, 0, data.Length);
					fs.Close ();
				}
			}
		}

		public void Remove (X509Certificate certificate) 
		{
			string filename = Path.Combine (_storePath, GetUniqueName (certificate));
			if (File.Exists (filename)) {
				File.Delete (filename);
			}
		}

		// private stuff

		private string GetUniqueName (X509Certificate certificate) 
		{
			string method = null;
			byte[] name = null;

			// We prefer Subject Key Identifier as the unique name
			// as it will provide faster lookups
			X509Extension ext = certificate.Extensions ["2.5.29.14"];
			if (ext != null) {
				SubjectKeyIdentifierExtension ski = new SubjectKeyIdentifierExtension (ext);
				name = ski.Identifier;
				method = "ski";
			}
			else {
				method = "tbp"; // thumbprint
				name = certificate.Hash;
			}

			StringBuilder sb = new StringBuilder (method);
			sb.Append ("-");
			foreach (byte b in name) {
				sb.Append (b.ToString ("X2"));
			}
			sb.Append (".cer");

			return sb.ToString ();
		}

		private byte[] Load (string filename) 
		{
			byte[] data = null;
			using (FileStream fs = File.OpenRead (filename)) {
				data = new byte [fs.Length];
				fs.Read (data, 0, data.Length);
				fs.Close ();
			}
			return data;
		}

		private X509Certificate LoadCertificate (string filename) 
		{
			byte[] data = Load (filename);
			X509Certificate cert = new X509Certificate (data);
			return cert;
		}

		private X509CRL LoadCRL (string filename) 
		{
			byte[] data = Load (filename);
			X509CRL crl = new X509CRL (data);
			return crl;
		}

		private X509CertificateCollection BuildCertificatesCollection (string storeName) 
		{
			string path = Path.Combine (_storePath, storeName);
			if (!Directory.Exists (path)) {
				Directory.CreateDirectory (path);
			}

			X509CertificateCollection coll = new X509CertificateCollection ();
			string[] files = Directory.GetFiles (path, "*.cer");
			if ((files != null) && (files.Length > 0)) {
				foreach (string file in files) {
					try {
						X509Certificate cert = LoadCertificate (file);
						coll.Add (cert);
					}
					catch {
						// in case someone is dumb enough
						// (like me) to include a base64
						// encoded certs (or other junk 
						// into the store).
					}
				}
			}
			return coll;
		}

		private ArrayList BuildCRLsCollection (string storeName) 
		{
			ArrayList list = new ArrayList ();
			string path = Path.Combine (_storePath, storeName);
			string[] files = Directory.GetFiles (path, "*.crl");
			if ((files != null) && (files.Length > 0)) {
				foreach (string file in files) {
					try {
						X509CRL crl = LoadCRL (file);
						list.Add (crl);
					}
					catch {
						// junk catcher
					}
				}
			}
			return list;
		}
	}
}
