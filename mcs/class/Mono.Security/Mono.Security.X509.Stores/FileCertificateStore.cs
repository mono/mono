//
// FileCertificateStore.cs: Handles a file-based certificate store.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;

using Mono.Security.Authenticode;
using Mono.Security.X509;
using Mono.Security.X509.Stores;

namespace Mono.Security.X509.Stores {

	public class FileCertificateStore : ICertificateStore {

		private string _name;
		private string _location;
		private bool _readOnly;
		private bool _createIfRequired;
		private bool _includeArchives;
		private bool _saveOnClose;
		private SoftwarePublisherCertificate _spc;

		public FileCertificateStore ()
		{
			_readOnly = true;
			_includeArchives = false;
			_createIfRequired = true;
		}

		// properties

		public X509CertificateCollection Certificates {
			get { 
				if (_spc != null)
					return _spc.Certificates; 
				return null;
			}
		}
                
		public IntPtr Handle {
			get { return (IntPtr) 0; }
		}

		// methods

		public void Open (string name, string location, bool readOnly, bool createIfNonExisting, bool includeArchives) 
		{
			_name = name;
			_location = _location;
			_readOnly = readOnly;
			_createIfRequired = createIfNonExisting;
			_includeArchives = includeArchives;
			_saveOnClose = false;

			if (File.Exists (_name)) {
				_spc = SoftwarePublisherCertificate.CreateFromFile (_name);
			}
			else if (_createIfRequired) {
				_spc = new SoftwarePublisherCertificate ();
				_saveOnClose = true;
			}
		}

		public void Close () 
		{
			if (_saveOnClose) {
				byte[] store = _spc.GetBytes ();
				using (FileStream fs = File.OpenWrite (_name)) {
					fs.Write (store, 0, store.Length);
					fs.Close ();
				}
			}
		}
		
		public void Add (X509Certificate certificate) 
		{
			if ((!_readOnly) && (_spc != null)) {
				_spc.Certificates.Add (certificate);
				_saveOnClose = true;
			}
		}
		
		public void Remove (X509Certificate certificate) 
		{
			if ((!_readOnly) && (_spc != null)) {
				_spc.Certificates.Remove (certificate);
				_saveOnClose = true;
			}
		}
	}
}
