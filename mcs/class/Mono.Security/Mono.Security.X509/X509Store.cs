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

using Mono.Security.X509.Extensions;

namespace Mono.Security.X509 {

	public class X509Store {

		private string _storePath;
		private X509CertificateCollection _certificates;
		private ArrayList _crls;
		private bool _crl;

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
				if ((_crls == null) && (_crl)) {
					_crls = BuildCRLsCollection (_storePath);
				}
				return _crls; 
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
			string uniquename = null;

			// We prefer Subject Key Identifier as the unique name
			// as it will provide faster lookups
			X509Extension ext = certificate.Extensions ["2.5.29.14"];
			if (ext != null) {
				SubjectKeyIdentifierExtension ski = new SubjectKeyIdentifierExtension (ext);
				uniquename = Convert.ToBase64String (ski.Identifier);
				method = "ski";
			}
			else {
				method = "tbp"; // thumbprint
				uniquename = Convert.ToBase64String (certificate.Hash);
			}

			return String.Format ("{0}{1}.cer", method, uniquename);
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
					X509Certificate cert = LoadCertificate (file);
					coll.Add (cert);
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
					X509CRL crl = LoadCRL (file);
					list.Add (crl);
				}
			}
			return list;
		}
	}
}
