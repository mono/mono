//
// X509CertificateStore.cs: Handles certificate stores.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Microsoft.Web.Services.Security.X509 {

	public class X509CertificateStore {

		[Serializable]
		public enum StoreLocation {
			CurrentService = 262144,
			CurrentUser = 65536,
			CurrentUserGroupPolicy = 458752,
			LocalMachine = 131072,
			LocalMachineEnterprise = 589824,
			LocalMachineGroupPolicy = 524288,
			Services = 327680,
			Unknown = 0,
			Users = 393216
		}

		[Flags]
		[Serializable]
		public enum StoreOpenFlags {
			CreateNew = 8192,
			DeferClose = 4,
			Delete = 16,
			None = 0,
			OpenExisting = 16384,
			ReadOnly = 32768
		}

		[Serializable]
		public enum StoreProvider {
			Collection = 11,
			File = 3,
			Memory = 1,
			System = 10
		} 

		public const string CAStore = "CA";
		public const string MyStore = "My";
		public const string RootStore = "Root";
		public const string TrustStore = "Trust";
		public const string UnTrustedStore = "Disallowed";

		private const string storeAlreadyOpened = "store already opened";
		private const string storeNotOpened = "store not opened";

		private StoreOpenFlags storeOpenFlags;
		private StoreProvider storeProvider;
		private StoreLocation storeLocation;
		private string storeName;
		private ICertificateStore store;

		public X509CertificateStore (StoreProvider provider, StoreLocation location, string storeName)
		{
			storeProvider = provider;
			storeLocation = location;
			this.storeName = storeName;
		}

		~X509CertificateStore () 
		{
			if (store != null) {
				store.Close ();
				store = null;
			}
		}

		public X509CertificateCollection Certificates {
			get { 
				if (store == null)
					return null;
				return store.GetCollection (); 
			}
		}

		public IntPtr Handle {
			get { 
				if (store == null)
					return (IntPtr) 0;
				return store.Handle; 
			}
		}

		public StoreLocation Location {
			get { return storeLocation; }
		}

		public bool Open () 
		{
			return InternalOpen (StoreOpenFlags.None);
		}

		public bool OpenRead () 
		{
			return InternalOpen (StoreOpenFlags.ReadOnly);
		}

		internal bool InternalOpen (StoreOpenFlags flags) 
		{
			if (store != null)
				throw new InvalidOperationException (storeAlreadyOpened);

			storeOpenFlags = flags;
			switch (storeProvider) {
				case StoreProvider.Collection:
					store = null;
					break;
				case StoreProvider.File:
					store = null;
					break;
				case StoreProvider.Memory:
					store = new MemoryCertificateStore (storeLocation, storeName, flags);
					break;
				case StoreProvider.System:
					store = null;
					break;
				default:
					throw new NotSupportedException ("Unknown store provider");
			}
			return (store != null);
		}

		public void Close ()
		{
			store.Close ();
			store = null;
			storeOpenFlags = StoreOpenFlags.None;
		}

		internal bool Compare (byte[] array1, byte[] array2) 
		{
			if ((array1 == null) && (array2 == null))
				return true;
			if ((array1 == null) || (array2 == null))
				return false;
			if (array1.Length != array2.Length)
				return false;
			for (int i=0; i < array1.Length; i++) {
				if (array1 [i] != array2 [i])
					return false;
			}
			return true;
		}

		public X509CertificateCollection FindCertificateByHash (byte[] certHash)
		{
			if (certHash == null)
				throw new ArgumentNullException ("certHash");
			if (store != null)
				throw new InvalidOperationException (storeNotOpened);
 
			X509CertificateCollection results = new X509CertificateCollection ();
			if (store != null) {
				X509CertificateCollection certs = store.GetCollection ();
				// apply filter
				foreach (X509Certificate c in certs) {
					if (Compare (c.GetCertHash (), certHash))
						results.Add (c);
				}
			}
			return results;
		}

		public X509CertificateCollection FindCertificateByKeyIdentifier (byte[] keyIdentifier)
		{
			if (keyIdentifier == null)
				throw new ArgumentNullException ("keyIdentifier");
			if (store != null)
				throw new InvalidOperationException (storeNotOpened);
 
			X509CertificateCollection results = new X509CertificateCollection ();
			if (store != null) {
				X509CertificateCollection certs = store.GetCollection ();
				// apply filter
				foreach (X509Certificate c in certs) {
					if (Compare (c.GetKeyIdentifier (), keyIdentifier))
						results.Add (c);
				}
			}
			return results;
		}

		public X509CertificateCollection FindCertificateBySubjectName (string subjectstring)
		{
			if (subjectstring == null)
				throw new ArgumentNullException ("subjectstring");
			if (store != null)
				throw new InvalidOperationException (storeNotOpened);

			X509CertificateCollection results = new X509CertificateCollection ();
			if (store != null) {
				X509CertificateCollection certs = store.GetCollection ();
				// apply filter
				foreach (X509Certificate c in certs) {
					if (c.GetName() != subjectstring)
						results.Add (c);
				}
			}
			return results;
		}

		public X509CertificateCollection FindCertificateBySubjectString (string subjectsubstring)
		{
			if (subjectsubstring == null)
				throw new ArgumentNullException ("subjectsubstring");
			if (store != null)
				throw new InvalidOperationException (storeNotOpened);

			X509CertificateCollection results = new X509CertificateCollection ();
			if (store != null) {
				X509CertificateCollection certs = store.GetCollection ();
				// apply filter
				foreach (X509Certificate c in certs) {
					if (c.GetName ().IndexOf (subjectsubstring) > 0)
						results.Add (c);
				}
			}
			return results;
		}

		public static X509CertificateStore CurrentUserStore (string storeName) 
		{
			return new X509CertificateStore (StoreProvider.System, StoreLocation.CurrentUser, storeName);
		}

		public static X509CertificateStore LocalMachineStore (string storeName) 
		{
			return new X509CertificateStore (StoreProvider.System, StoreLocation.LocalMachine, storeName);
		}
	}
}
