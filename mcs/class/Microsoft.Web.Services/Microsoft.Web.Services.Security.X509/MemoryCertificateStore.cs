//
// MemoryCertificateStore.cs: Handles an in-memory certificate store.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Microsoft.Web.Services.Security.X509 {

	internal class MemoryCertificateStore : ICertificateStore {

		private string _storeName;
		private X509CertificateStore.StoreOpenFlags _flags;
		private X509CertificateStore.StoreLocation _location;
		private IntPtr _handle;
		private X509CertificateCollection _coll;

		public MemoryCertificateStore (X509CertificateStore.StoreLocation location, string storeName, X509CertificateStore.StoreOpenFlags flags) 
		{
			_location = location;
			_storeName = storeName;
			_flags = flags;
			_coll = new X509CertificateCollection ();
		}

		public void Close () 
		{
		}

		public IntPtr Handle {
			get { return (IntPtr) _coll.GetHashCode (); }
		}

		public X509CertificateCollection GetCollection () 
		{
			if (_flags == X509CertificateStore.StoreOpenFlags.ReadOnly) {
				// return a copy of the collection so changes aren't persisted
				X509CertificateCollection copy = new X509CertificateCollection ();
				foreach (X509Certificate x in _coll) {
					copy.Add (x);
				}
				return copy;
			}
			else
				return _coll;
		}
	}
}
