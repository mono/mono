//
// X509Store.cs - System.Security.Cryptography.X509Certificates.X509Store
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

using Mono.Security.X509;
using Mono.Security.X509.Stores;

namespace System.Security.Cryptography.X509Certificates {

	// Note: Match the definition of framework version 1.2.3400.0 on http://longhorn.msdn.microsoft.com

	public sealed class X509Store {

		private string _name;
		private StoreLocation _location;
		private X509CertificateExCollection _certs;
		private OpenFlags _flags;
		private ICertificateStore _store;

		// constructors

		// BUG: MY when using this constructor - My when using StoreName.My
		public X509Store () 
			: this ("MY", StoreLocation.CurrentUser) {}

		public X509Store (string storeName) 
			: this (storeName, StoreLocation.CurrentUser) {}

		public X509Store (StoreName storeName) 
			: this (StoreNameToString (storeName), StoreLocation.CurrentUser) {}

		public X509Store (StoreLocation storeLocation) 
			: this ("MY", storeLocation) {}

		public X509Store (StoreName storeName, StoreLocation storeLocation)
			: this (StoreNameToString (storeName), StoreLocation.CurrentUser) {}

		public X509Store (string storeName, StoreLocation storeLocation)
		{
			if (storeName == null)
				throw new ArgumentNullException ("storeName");

			_name = storeName;
			_location = storeLocation;
			_store = new Mono.Security.X509.Stores.FileCertificateStore ();
		}

		// properties

		public X509CertificateExCollection Certificates {
			get { 
				if (_certs == null)
					_certs = new X509CertificateExCollection ();
				return _certs; 
			}
		} 

		public StoreLocation Location {
			get { return _location; }
		}

		public string Name {
			get { return _name; }
		}

		private bool ReadOnly {
			get { return ((_flags & OpenFlags.ReadOnly) != OpenFlags.ReadOnly); }
		}

		// methods

		private static string StoreNameToString (StoreName sn) 
		{
			switch (sn) {
				case StoreName.CertificateAuthority:
					return "CA";
				default:
					return sn.ToString ();
			}
		}

		public void Add (X509CertificateEx certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			if ((!ReadOnly) && (_store != null)) {
				try {
					Mono.Security.X509.X509Certificate x = new Mono.Security.X509.X509Certificate (certificate.RawData);
					_store.Add (x);
				}
				catch {
					throw new CryptographicException ("couldn't add certificate");
				}
			}
		}

		public void AddRange (X509CertificateExCollection certificates)
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");

			if (!ReadOnly) {
				foreach (X509CertificateEx certificate in certificates) {
					Add (certificate);
				}
			}
		}

		public void Close () 
		{
			if (_store != null)
				_store.Close ();
		}

		public void Open (OpenFlags flags)
		{
			_flags = flags;
			bool readOnly = ((flags & OpenFlags.ReadOnly) == OpenFlags.ReadOnly);
			bool create = !((flags & OpenFlags.OpenExistingOnly) == OpenFlags.OpenExistingOnly);
			bool archive = ((flags & OpenFlags.IncludeArchived) == OpenFlags.IncludeArchived);
			_store.Open (_name, _location.ToString (), readOnly, create, archive);
		}

		public void Remove (X509CertificateEx certificate) 
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			if ((!ReadOnly) && (_store != null)) {
				try {
					Mono.Security.X509.X509Certificate x = new Mono.Security.X509.X509Certificate (certificate.RawData);
					_store.Remove (x);
				}
				catch {
					throw new CryptographicException ("couldn't remove certificate");
				}
			}
		}

		public void RemoveRange (X509CertificateExCollection certificates) 
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");

			if (!this.ReadOnly) {
				foreach (X509CertificateEx certificate in certificates) {
					Remove (certificate);
				}
			}
		}
	}
}

#endif