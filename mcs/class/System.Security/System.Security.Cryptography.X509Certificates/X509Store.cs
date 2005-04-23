//
// System.Security.Cryptography.X509Certificates.X509Store class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using Mono.Security.X509;

namespace System.Security.Cryptography.X509Certificates {

	public sealed class X509Store {

		private string _name;
		private StoreLocation _location;
		private X509Certificate2Collection _certs;
		private OpenFlags _flags;

		// constructors

		// BUG: MY when using this constructor - My when using StoreName.My
		public X509Store () 
			: this ("MY", StoreLocation.CurrentUser) 
		{
		}

		public X509Store (string storeName) 
			: this (storeName, StoreLocation.CurrentUser) 
		{
		}

		public X509Store (StoreName storeName) 
			: this (StoreNameToString (storeName), StoreLocation.CurrentUser)
		{
		}

		public X509Store (StoreLocation storeLocation) 
			: this ("MY", storeLocation)
		{
		}

		public X509Store (StoreName storeName, StoreLocation storeLocation)
			: this (StoreNameToString (storeName), StoreLocation.CurrentUser)
		{
		}

		[MonoTODO ("call Mono.Security.X509.X509Store*")]
		public X509Store (string storeName, StoreLocation storeLocation)
		{
			if (storeName == null)
				throw new ArgumentNullException ("storeName");

			_name = storeName;
			_location = storeLocation;
		}

		// properties

		public X509Certificate2Collection Certificates {
			get { 
				if (_certs == null)
					_certs = new X509Certificate2Collection ();
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

		[MonoTODO ("call Mono.Security.X509.X509Store*")]
		public void Add (X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			if (!ReadOnly) {
				try {
					Mono.Security.X509.X509Certificate x = new Mono.Security.X509.X509Certificate (certificate.RawData);
					// TODO
				}
				catch {
					throw new CryptographicException ("couldn't add certificate");
				}
			}
		}

		public void AddRange (X509Certificate2Collection certificates)
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");

			if (!ReadOnly) {
				foreach (X509Certificate2 certificate in certificates) {
					Add (certificate);
				}
			}
		}

		[MonoTODO ("call Mono.Security.X509.X509Store*")]
		public void Close () 
		{
		}

		[MonoTODO ("call Mono.Security.X509.X509Store*")]
		public void Open (OpenFlags flags)
		{
			_flags = flags;
			bool readOnly = ((flags & OpenFlags.ReadOnly) == OpenFlags.ReadOnly);
			bool create = !((flags & OpenFlags.OpenExistingOnly) == OpenFlags.OpenExistingOnly);
			bool archive = ((flags & OpenFlags.IncludeArchived) == OpenFlags.IncludeArchived);
			// TODO
		}

		[MonoTODO ("call Mono.Security.X509.X509Store*")]
		public void Remove (X509Certificate2 certificate) 
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			if (!ReadOnly) {
				try {
					Mono.Security.X509.X509Certificate x = new Mono.Security.X509.X509Certificate (certificate.RawData);
					// TODO
				}
				catch {
					throw new CryptographicException ("couldn't remove certificate");
				}
			}
		}

		public void RemoveRange (X509Certificate2Collection certificates) 
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");

			if (!this.ReadOnly) {
				foreach (X509Certificate2 certificate in certificates) {
					Remove (certificate);
				}
			}
		}
	}
}

#endif
