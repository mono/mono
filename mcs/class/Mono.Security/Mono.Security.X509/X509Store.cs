//
// X509Store.cs: Handles a X.509 certificates/CRLs store
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Pablo Ruiz <pruiz@netway.org>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// (C) 2010 Pablo Ruiz.
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
using System.Globalization;
using System.IO;
using System.Text;
using System.Security.Cryptography;

using Mono.Security.Cryptography;
using Mono.Security.X509.Extensions;

using SSCX = System.Security.Cryptography.X509Certificates;

namespace Mono.Security.X509 {

#if INSIDE_CORLIB || INSIDE_SYSTEM
	internal
#else
	public 
#endif
	class X509Store {

		private string _storePath;
		private X509CertificateCollection _certificates;
		private ArrayList _crls;
		private bool _crl;
		private bool _newFormat;
		private string _name;

		internal X509Store (string path, bool crl, bool newFormat)
		{
			_storePath = path;
			_crl = crl;
			_newFormat = newFormat;
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

		public ArrayList Crls {
			get {
				// CRL aren't applicable to all stores
				// but returning null is a little rude
				if (!_crl) {
					_crls = new ArrayList ();
				}
				if (_crls == null) {
					_crls = BuildCrlsCollection (_storePath);
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
			/* 
			 * Both _certificates and _crls extend CollectionBase, whose Clear() method calls OnClear() and 
			 * OnClearComplete(), which should be overridden in derivative classes. So we should not worry about
			 * other threads that might be holding references to _certificates or _crls. They should be smart enough
			 * to handle this gracefully.  And if not, it's their own fault.
			 */
			ClearCertificates ();
			ClearCrls ();
		}

		void ClearCertificates()
		{
			if (_certificates != null)
				_certificates.Clear ();
			_certificates = null;
		}

		void ClearCrls ()
		{
			if (_crls != null)
				_crls.Clear ();
			_crls = null;
		}

		public void Import (X509Certificate certificate) 
		{
			CheckStore (_storePath, true);

			if (_newFormat) {
				ImportNewFormat (certificate);
				return;
			}

			string filename = Path.Combine (_storePath, GetUniqueName (certificate));
			if (!File.Exists (filename)) {
				filename = Path.Combine (_storePath, GetUniqueNameWithSerial (certificate));
				if (!File.Exists (filename)) {
					using (FileStream fs = File.Create (filename)) {
						byte[] data = certificate.RawData;
						fs.Write (data, 0, data.Length);
						fs.Close ();
					}
					ClearCertificates ();	// We have modified the store on disk.  So forget the old state.
				}
			} else {
				string newfilename = Path.Combine (_storePath, GetUniqueNameWithSerial (certificate));
				if (GetUniqueNameWithSerial (LoadCertificate (filename)) != GetUniqueNameWithSerial (certificate)) {
					using (FileStream fs = File.Create (newfilename)) {
						byte[] data = certificate.RawData;
						fs.Write (data, 0, data.Length);
						fs.Close ();
					}
					ClearCertificates ();	// We have modified the store on disk.  So forget the old state.
				}
			}
#if !MOBILE
			// Try to save privateKey if available..
			CspParameters cspParams = new CspParameters ();
			cspParams.KeyContainerName = CryptoConvert.ToHex (certificate.Hash);

			// Right now this seems to be the best way to know if we should use LM store.. ;)
			if (_storePath.StartsWith (X509StoreManager.LocalMachinePath) || _storePath.StartsWith(X509StoreManager.NewLocalMachinePath))
				cspParams.Flags = CspProviderFlags.UseMachineKeyStore;

			ImportPrivateKey (certificate, cspParams);
#endif
		}

		public void Import (X509Crl crl) 
		{
			CheckStore (_storePath, true);

			if (_newFormat)
				throw new NotSupportedException ();

			string filename = Path.Combine (_storePath, GetUniqueName (crl));
			if (!File.Exists (filename)) {
				using (FileStream fs = File.Create (filename)) {
					byte[] data = crl.RawData;
					fs.Write (data, 0, data.Length);
				}
				ClearCrls ();	// We have modified the store on disk.  So forget the old state.
			}
		}

		public void Remove (X509Certificate certificate) 
		{
			if (_newFormat) {
				RemoveNewFormat (certificate);
				return;
			}

			string filename = Path.Combine (_storePath, GetUniqueNameWithSerial (certificate));
			if (File.Exists (filename)) {
				File.Delete (filename);
				ClearCertificates ();	// We have modified the store on disk.  So forget the old state.
			} else {
				filename = Path.Combine (_storePath, GetUniqueName (certificate));
				if (File.Exists (filename)) {
					File.Delete (filename);
					ClearCertificates ();	// We have modified the store on disk.  So forget the old state.
				}
			}
		}

		public void Remove (X509Crl crl) 
		{
			if (_newFormat)
				throw new NotSupportedException ();

			string filename = Path.Combine (_storePath, GetUniqueName (crl));
			if (File.Exists (filename)) {
				File.Delete (filename);
				ClearCrls ();	// We have modified the store on disk.  So forget the old state.
			}
		}

		// new format

		void ImportNewFormat (X509Certificate certificate)
		{
#if INSIDE_CORLIB
			throw new NotSupportedException ();
#else
			using (var sscxCert = new SSCX.X509Certificate (certificate.RawData)) {
				var hash = SSCX.X509Helper2.GetSubjectNameHash (sscxCert);
				var filename = Path.Combine (_storePath, string.Format ("{0:x8}.0", hash));
				if (!File.Exists (filename)) {
					using (FileStream fs = File.Create (filename))
						SSCX.X509Helper2.ExportAsPEM (sscxCert, fs, true);
					ClearCertificates ();
				}
			}
#endif
		}

		void RemoveNewFormat (X509Certificate certificate)
		{
#if INSIDE_CORLIB
			throw new NotSupportedException ();
#else
			using (var sscxCert = new SSCX.X509Certificate (certificate.RawData)) {
				var hash = SSCX.X509Helper2.GetSubjectNameHash (sscxCert);
				var filename = Path.Combine (_storePath, string.Format ("{0:x8}.0", hash));
				if (File.Exists (filename)) {
					File.Delete (filename);
					ClearCertificates ();
				}
			}
#endif
		}

		// private stuff

		private string GetUniqueNameWithSerial (X509Certificate certificate)
		{
			return GetUniqueName (certificate, certificate.SerialNumber);
		}

		private string GetUniqueName (X509Certificate certificate, byte[] serial = null) 
		{
			string method;
			byte[] name = GetUniqueName (certificate.Extensions, serial);
			if (name == null) {
				method = "tbp"; // thumbprint
				name = certificate.Hash;
			} else {
				method = "ski";
			}
			return GetUniqueName (method, name, ".cer");
		}

		private string GetUniqueName (X509Crl crl) 
		{
			string method;
			byte[] name = GetUniqueName (crl.Extensions);
			if (name == null) {
				method = "tbp"; // thumbprint
				name = crl.Hash;
			} else {
				method = "ski";
			}
			return GetUniqueName (method, name, ".crl");
		}

		private byte[] GetUniqueName (X509ExtensionCollection extensions, byte[] serial = null) 
		{
			// We prefer Subject Key Identifier as the unique name
			// as it will provide faster lookups
			X509Extension ext = extensions ["2.5.29.14"];
			if (ext == null)
				return null;

			SubjectKeyIdentifierExtension ski = new SubjectKeyIdentifierExtension (ext);
			if (serial == null) {
				return ski.Identifier;
			} else {
				byte[] uniqueWithSerial = new byte[ski.Identifier.Length + serial.Length];
				System.Buffer.BlockCopy (ski.Identifier, 0, uniqueWithSerial, 0, ski.Identifier.Length );
				System.Buffer.BlockCopy (serial, 0, uniqueWithSerial, ski.Identifier.Length, serial.Length );
				return uniqueWithSerial;
			}
		}

		private string GetUniqueName (string method, byte[] name, string fileExtension) 
		{
			StringBuilder sb = new StringBuilder (method);
			
			sb.Append ("-");
			foreach (byte b in name) {
				sb.Append (b.ToString ("X2", CultureInfo.InvariantCulture));
			}
			sb.Append (fileExtension);

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
#if !MOBILE
			// If privateKey it's available, load it too..
			CspParameters cspParams = new CspParameters ();
			cspParams.KeyContainerName = CryptoConvert.ToHex (cert.Hash);
			if (_storePath.StartsWith (X509StoreManager.LocalMachinePath) || _storePath.StartsWith(X509StoreManager.NewLocalMachinePath))
				cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
			KeyPairPersistence kpp = new KeyPairPersistence (cspParams);

			try {
				if (!kpp.Load ())
					return cert;
			}
			catch {
				return cert;
			}

			if (cert.RSA != null)
				cert.RSA = new RSACryptoServiceProvider (cspParams);
			else if (cert.DSA != null)
				cert.DSA = new DSACryptoServiceProvider (cspParams);
#endif
			return cert;
		}

		private X509Crl LoadCrl (string filename) 
		{
			byte[] data = Load (filename);
			X509Crl crl = new X509Crl (data);
			return crl;
		}

		private bool CheckStore (string path, bool throwException)
		{
			try {
				if (Directory.Exists (path))
					return true;
				Directory.CreateDirectory (path);
				return Directory.Exists (path);
			}
			catch {
				if (throwException)
					throw;
				return false;
			}
		}

		private X509CertificateCollection BuildCertificatesCollection (string storeName) 
		{
			X509CertificateCollection coll = new X509CertificateCollection ();
			string path = Path.Combine (_storePath, storeName);
			if (!CheckStore (path, false))
				return coll;	// empty collection

			string[] files = Directory.GetFiles (path, _newFormat ? "*.0" : "*.cer");
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

		private ArrayList BuildCrlsCollection (string storeName) 
		{
			ArrayList list = new ArrayList ();
			string path = Path.Combine (_storePath, storeName);
			if (!CheckStore (path, false))
				return list;	// empty list

			string[] files = Directory.GetFiles (path, "*.crl");
			if ((files != null) && (files.Length > 0)) {
				foreach (string file in files) {
					try {
						X509Crl crl = LoadCrl (file);
						list.Add (crl);
					}
					catch {
						// junk catcher
					}
				}
			}
			return list;
		}
#if !MOBILE
		private void ImportPrivateKey (X509Certificate certificate, CspParameters cspParams)
		{
			RSACryptoServiceProvider rsaCsp = certificate.RSA as RSACryptoServiceProvider;
			if (rsaCsp != null) {
				if (rsaCsp.PublicOnly)
					return;

				RSACryptoServiceProvider csp = new RSACryptoServiceProvider(cspParams);
				csp.ImportParameters(rsaCsp.ExportParameters(true));
				csp.PersistKeyInCsp = true;
				return;
			}

			RSAManaged rsaMng = certificate.RSA as RSAManaged;
			if (rsaMng != null) {
				if (rsaMng.PublicOnly)
					return;

				RSACryptoServiceProvider csp = new RSACryptoServiceProvider(cspParams);
				csp.ImportParameters(rsaMng.ExportParameters(true));
				csp.PersistKeyInCsp = true;
				return;
			}

			DSACryptoServiceProvider dsaCsp = certificate.DSA as DSACryptoServiceProvider;
			if (dsaCsp != null) {
				if (dsaCsp.PublicOnly)
					return;

				DSACryptoServiceProvider csp = new DSACryptoServiceProvider(cspParams);
				csp.ImportParameters(dsaCsp.ExportParameters(true));
				csp.PersistKeyInCsp = true;
			}
		}
#endif
	}
}
