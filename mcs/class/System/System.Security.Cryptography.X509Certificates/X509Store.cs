//
// System.Security.Cryptography.X509Certificates.X509Store class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
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

#if SECURITY_DEP

#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
using MX = MonoSecurity::Mono.Security.X509;
#else
using MX = Mono.Security.X509;
#endif

using System.Security.Permissions;

namespace System.Security.Cryptography.X509Certificates {

	public sealed class X509Store : IDisposable {

		private string _name;
		private StoreLocation _location;
		private X509Certificate2Collection list;
		private OpenFlags _flags;
		private MX.X509Store store;

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
			: this (storeName, StoreLocation.CurrentUser)
		{
		}

		public X509Store (StoreLocation storeLocation) 
			: this ("MY", storeLocation)
		{
		}

		public X509Store (StoreName storeName, StoreLocation storeLocation)
		{
			if ((storeName < StoreName.AddressBook) || (storeName > StoreName.TrustedPublisher))
				throw new ArgumentException ("storeName");
			if ((storeLocation < StoreLocation.CurrentUser) || (storeLocation > StoreLocation.LocalMachine))
				throw new ArgumentException ("storeLocation");

			switch (storeName) {
			case StoreName.CertificateAuthority:
				_name = "CA";
				break;
			default:
				_name = storeName.ToString ();
				break;
			}
			_location = storeLocation;
		}

		[MonoTODO ("Mono's stores are fully managed. All handles are invalid.")]
		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode=true)]
		public X509Store (IntPtr storeHandle)
		{
			if (storeHandle == IntPtr.Zero)
				throw new ArgumentNullException ("storeHandle");
			throw new CryptographicException ("Invalid handle.");
		}

		public X509Store (string storeName, StoreLocation storeLocation)
		{
			if ((storeLocation < StoreLocation.CurrentUser) || (storeLocation > StoreLocation.LocalMachine))
				throw new ArgumentException ("storeLocation");

			_name = storeName;
			_location = storeLocation;
		}

		// properties

		public X509Certificate2Collection Certificates {
			get {
				if (list == null)
					list = new X509Certificate2Collection ();
				else if (store == null)
					list.Clear ();

				return list;
			}
		} 

		public StoreLocation Location {
			get { return _location; }
		}

		public string Name {
			get { return _name; }
		}

		private MX.X509Stores Factory {
			get {
				if (_location == StoreLocation.CurrentUser)
					return MX.X509StoreManager.CurrentUser;
				else
					return MX.X509StoreManager.LocalMachine;
			}
		}

		private bool IsOpen {
			get { return (store != null); }
		}

		private bool IsReadOnly {
			get { return ((_flags & OpenFlags.ReadWrite) == OpenFlags.ReadOnly); }
		}

		internal MX.X509Store Store {
			get { return store; }
		}

		[MonoTODO ("Mono's stores are fully managed. Always returns IntPtr.Zero.")]
		public IntPtr StoreHandle {
			get { return IntPtr.Zero; }
		}

		// methods

		public void Add (X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");
			if (!IsOpen)
				throw new CryptographicException (Locale.GetText ("Store isn't opened."));
			if (IsReadOnly)
				throw new CryptographicException (Locale.GetText ("Store is read-only."));

			if (!Exists (certificate)) {
				try {
					store.Import (new MX.X509Certificate (certificate.RawData));
				}
				finally {
					Certificates.Add (certificate);
				}
			}
		}

		[MonoTODO ("Method isn't transactional (like documented)")]
		public void AddRange (X509Certificate2Collection certificates)
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");

			if (certificates.Count == 0)
				return;

			if (!IsOpen)
				throw new CryptographicException (Locale.GetText ("Store isn't opened."));
			if (IsReadOnly)
				throw new CryptographicException (Locale.GetText ("Store is read-only."));

			foreach (X509Certificate2 certificate in certificates) {
				if (!Exists (certificate)) {
					try {
						store.Import (new MX.X509Certificate (certificate.RawData));
					}
					finally {
						Certificates.Add (certificate);
					}
				}
			}
		}

		public void Close () 
		{
			store = null;
			if (list != null)
				list.Clear ();
		}

		public void Dispose ()
		{
			Close ();
		}

		public void Open (OpenFlags flags)
		{
			if (String.IsNullOrEmpty (_name))
				throw new CryptographicException (Locale.GetText ("Invalid store name (null or empty)."));

			/* keep existing Mono installations (pre 2.0) compatible with new stuff */
			string name;
			switch (_name) {
			case "Root":
				name = "Trust";
				break;
			default:
				name = _name;
				break;
			}

			bool create = ((flags & OpenFlags.OpenExistingOnly) != OpenFlags.OpenExistingOnly);
			store = Factory.Open (name, create);
			if (store == null)
				throw new CryptographicException (Locale.GetText ("Store {0} doesn't exists.", _name));
			_flags = flags;

			foreach (MX.X509Certificate x in store.Certificates) {
				var cert2 = new X509Certificate2 (x.RawData);
				cert2.PrivateKey = x.RSA;
				Certificates.Add (cert2);
			}
		}

		public void Remove (X509Certificate2 certificate) 
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");
			if (!IsOpen)
				throw new CryptographicException (Locale.GetText ("Store isn't opened."));

			if (!Exists (certificate))
				return;

			if (IsReadOnly)
				throw new CryptographicException (Locale.GetText ("Store is read-only."));

			try {
				store.Remove (new MX.X509Certificate (certificate.RawData));
			}
			finally {
				Certificates.Remove (certificate);
			}
		}

		[MonoTODO ("Method isn't transactional (like documented)")]
		public void RemoveRange (X509Certificate2Collection certificates) 
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");

			if (certificates.Count == 0)
				return;

			if (!IsOpen)
				throw new CryptographicException (Locale.GetText ("Store isn't opened."));

			bool delete = false;
			foreach (X509Certificate2 certificate in certificates) {
				if (Exists (certificate))
					delete = true;
			}
			if (!delete)
				return;

			if (IsReadOnly)
				throw new CryptographicException (Locale.GetText ("Store is read-only."));

			try {
				foreach (X509Certificate2 certificate in certificates)
					store.Remove (new MX.X509Certificate (certificate.RawData));
			}
			finally {
				Certificates.RemoveRange (certificates);
			}
		}

		private bool Exists (X509Certificate2 certificate)
		{
			if ((store == null) || (list == null) || (certificate == null))
				return false;

			foreach (X509Certificate2 c in list) {
				if (certificate.Equals (c)) {
					return true;
				}
			}
			return false;
		}
	}
}

#endif
