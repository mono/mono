//
// FileCertificateStore.cs: Handles a file-based certificate store.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using Mono.Security.Authenticode;

namespace Microsoft.Web.Services.Security.X509 {

	internal class FileCertificateStore : ICertificateStore {

		private string _storeName;
		private X509CertificateStore.StoreOpenFlags _flags;
		private X509CertificateStore.StoreLocation _location;
		private IntPtr _handle;

		public FileCertificateStore (X509CertificateStore.StoreLocation location, string storeName, X509CertificateStore.StoreOpenFlags flags) 
		{
			_location = location;
			_storeName = storeName;
			_flags = flags;
		}

		public IntPtr Handle {
			get { return (IntPtr) 0; }
		}

		public X509CertificateCollection GetCollection () 
		{
			if (_spc == null) {
				_spc = SoftwarePublisherCertificate.CreateFromFile (_storeName);
			}
			X509CertificateCollection coll = new X509CertificateCollection ();
			Mono.Security.X509.X509CertificateCollection spcoll = _spc.Certificates;
			foreach (Mono.Security.X509.X509Certificate x in spcoll) {
				coll.Add (new X509Certificate (x.RawData));
			}
			return coll;
		}

		public void Close () {}
	}
}
