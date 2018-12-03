//
// X509StoreTest.cs - NUnit tests for X509Store
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//


using NUnit.Framework;

using System;
using System.Collections;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MonoTests.System.Security.Cryptography.X509Certificates {

	[TestFixture]
	public class X509StoreTest {

		private X509Certificate2 cert_empty;
		private X509Certificate2 cert1;
		private X509Certificate2 cert2;
		private X509Certificate2Collection coll_empty;
		private X509Certificate2Collection coll;

		string ReadWriteStore = "ReadWriteStore" + Process.GetCurrentProcess ().Id;
		string ReadOnlyStore = "ReadOnlyStore" + Process.GetCurrentProcess ().Id;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			cert_empty = new X509Certificate2 ();
			cert1 = new X509Certificate2 (X509Certificate2Test.farscape_pfx, "farscape", X509KeyStorageFlags.Exportable);
			cert2 = new X509Certificate2 (Encoding.ASCII.GetBytes (X509Certificate2Test.base64_cert));
			coll_empty = new X509Certificate2Collection ();
			coll = new X509Certificate2Collection ();
			coll.Add (cert1);
			coll.Add (cert2);

			CleanUpStore (ReadOnlyStore);
		}

		[SetUp]
		public void SetUp ()
		{
			CleanUpStore (ReadWriteStore);
		}

		private void CleanUpStore (string s)
		{
			X509Store xs = new X509Store (s);
			xs.Open (OpenFlags.ReadWrite);
			int n = xs.Certificates.Count;
			if (n > 0) {
				X509Certificate2[] array = new X509Certificate2[n];
				xs.Certificates.CopyTo (array, 0);
				foreach (X509Certificate2 x in array)
					xs.Remove (x);
			}
			xs.Close ();
		}

		private void CheckDefaults (X509Store xs)
		{
			Assert.AreEqual (StoreLocation.CurrentUser, xs.Location, "Location");
			Assert.AreEqual ("MY", xs.Name, "Name");
			Assert.IsNotNull (xs.Certificates, "Certificates");
			// always IntPtr.Zero for Mono, IntPtr.Zero before being opened on Windows
			Assert.AreEqual (IntPtr.Zero, xs.StoreHandle, "StoreHandle");
		}

		[Test]
		public void ConstructorEmpty () 
		{
			X509Store xs = new X509Store ();
			CheckDefaults (xs);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorIntPtr ()
		{
			new X509Store (IntPtr.Zero);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorStoreLocation_Invalid ()
		{
			new X509Store ((StoreLocation) Int32.MinValue);
		}
		
		[Test]
		public void ConstructorStoreLocationCurrentUser () 
		{
			X509Store xs = new X509Store (StoreLocation.CurrentUser);
			CheckDefaults (xs);
		}

		[Test]
		public void ConstructorStoreLocationLocalMachine () 
		{
			X509Store xs = new X509Store (StoreLocation.LocalMachine);
			// default properties
			Assert.AreEqual (StoreLocation.LocalMachine, xs.Location, "Location");
			Assert.AreEqual ("MY", xs.Name, "Name");
			Assert.IsNotNull (xs.Certificates, "Certificates");
		}

		[Test]
		public void ConstructorStoreString_Null ()
		{
			X509Store xs = new X509Store (null);
			Assert.AreEqual (StoreLocation.CurrentUser, xs.Location, "Location");
			Assert.IsNull (xs.Name, "Name");
			Assert.IsNotNull (xs.Certificates, "Certificates");
		}

		[Test]
		public void ConstructorStoreString_Empty ()
		{
			X509Store xs = new X509Store (String.Empty);
			Assert.AreEqual (StoreLocation.CurrentUser, xs.Location, "Location");
			Assert.AreEqual (String.Empty, xs.Name, "Name");
			Assert.IsNotNull (xs.Certificates, "Certificates");
		}

		[Test]
		public void ConstructorStoreStringAddressBook () 
		{
			X509Store xs = new X509Store ("AddressBook");
			// default properties
			Assert.AreEqual (StoreLocation.CurrentUser, xs.Location, "Location");
			Assert.AreEqual ("AddressBook", xs.Name, "Name");
			Assert.IsNotNull (xs.Certificates, "Certificates");
		}

		[Test]
		public void ConstructorStoreStringAuthRoot () 
		{
			X509Store xs = new X509Store ("AuthRoot");
			// default properties
			Assert.AreEqual (StoreLocation.CurrentUser, xs.Location, "Location");
			Assert.AreEqual ("AuthRoot", xs.Name, "Name");
			Assert.IsNotNull (xs.Certificates, "Certificates");
		}

		[Test]
		public void ConstructorStoreStringCertificateAuthority () 
		{
			X509Store xs = new X509Store ("CA");
			// default properties
			Assert.AreEqual (StoreLocation.CurrentUser, xs.Location, "Location");
			Assert.AreEqual ("CA", xs.Name, "Name");
			Assert.IsNotNull (xs.Certificates, "Certificates");
		}

		[Test]
		public void ConstructorStoreStringDisallowed () 
		{
			X509Store xs = new X509Store ("Disallowed");
			// default properties
			Assert.AreEqual (StoreLocation.CurrentUser, xs.Location, "Location");
			Assert.AreEqual ("Disallowed", xs.Name, "Name");
			Assert.IsNotNull (xs.Certificates, "Certificates");
		}

		[Test]
		public void ConstructorStoreStringMy () 
		{
			X509Store xs = new X509Store ("My");
			// default properties
			Assert.AreEqual (StoreLocation.CurrentUser, xs.Location, "Location");
			Assert.AreEqual ("My", xs.Name, "Name");
			Assert.IsNotNull (xs.Certificates, "Certificates");
		}

		[Test]
		public void ConstructorStoreStringRoot () 
		{
			X509Store xs = new X509Store ("Root");
			// default properties
			Assert.AreEqual (StoreLocation.CurrentUser, xs.Location, "Location");
			Assert.AreEqual ("Root", xs.Name, "Name");
			Assert.IsNotNull (xs.Certificates, "Certificates");
		}

		[Test]
		public void ConstructorStoreStringTrustedPeople () 
		{
			X509Store xs = new X509Store ("TrustedPeople");
			// default properties
			Assert.AreEqual (StoreLocation.CurrentUser, xs.Location, "Location");
			Assert.AreEqual ("TrustedPeople", xs.Name, "Name");
			Assert.IsNotNull (xs.Certificates, "Certificates");
		}

		[Test]
		public void ConstructorStoreStringTrustedPublisher () 
		{
			X509Store xs = new X509Store ("TrustedPublisher");
			// default properties
			Assert.AreEqual (StoreLocation.CurrentUser, xs.Location, "Location");
			Assert.AreEqual ("TrustedPublisher", xs.Name, "Name");
			Assert.IsNotNull (xs.Certificates, "Certificates");
		}

		[Test]
		public void ConstructorStoreStringMono () 
		{
			// mono isn't defined the StoreName
			X509Store xs = new X509Store ("Mono");
			// default properties
			Assert.AreEqual (StoreLocation.CurrentUser, xs.Location, "Location");
			Assert.AreEqual ("Mono", xs.Name, "Name");
			Assert.IsNotNull (xs.Certificates, "Certificates");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorStoreName_Invalid ()
		{
			new X509Store ((StoreName) Int32.MinValue);
		}

		[Test]
		public void ConstructorStoreNameAddressBook () 
		{
			X509Store xs = new X509Store (StoreName.AddressBook);
			// default properties
			Assert.AreEqual (StoreLocation.CurrentUser, xs.Location, "Location");
			Assert.AreEqual ("AddressBook", xs.Name, "Name");
			Assert.IsNotNull (xs.Certificates, "Certificates");
		}

		[Test]
		public void ConstructorStoreNameAuthRoot () 
		{
			X509Store xs = new X509Store (StoreName.AuthRoot);
			// default properties
			Assert.AreEqual (StoreLocation.CurrentUser, xs.Location, "Location");
			Assert.AreEqual ("AuthRoot", xs.Name, "Name");
			Assert.IsNotNull (xs.Certificates, "Certificates");
		}

		[Test]
		public void ConstructorStoreNameCertificateAuthority () 
		{
			X509Store xs = new X509Store (StoreName.CertificateAuthority);
			// default properties
			Assert.AreEqual (StoreLocation.CurrentUser, xs.Location, "Location");
			Assert.AreEqual ("CA", xs.Name, "Name");
			Assert.IsNotNull (xs.Certificates, "Certificates");
		}

		[Test]
		public void ConstructorStoreNameDisallowed () 
		{
			X509Store xs = new X509Store (StoreName.Disallowed);
			// default properties
			Assert.AreEqual (StoreLocation.CurrentUser, xs.Location, "Location");
			Assert.AreEqual ("Disallowed", xs.Name, "Name");
			Assert.IsNotNull (xs.Certificates, "Certificates");
		}

		[Test]
		public void ConstructorStoreNameMy () 
		{
			X509Store xs = new X509Store (StoreName.My);
			// default properties
			Assert.AreEqual (StoreLocation.CurrentUser, xs.Location, "Location");
			Assert.AreEqual ("My", xs.Name, "Name");
			Assert.IsNotNull (xs.Certificates, "Certificates");
		}

		[Test]
		public void ConstructorStoreNameRoot () 
		{
			X509Store xs = new X509Store (StoreName.Root);
			// default properties
			Assert.AreEqual (StoreLocation.CurrentUser, xs.Location, "Location");
			Assert.AreEqual ("Root", xs.Name, "Name");
			Assert.IsNotNull (xs.Certificates, "Certificates");
		}

		[Test]
		public void ConstructorStoreNameTrustedPeople () 
		{
			X509Store xs = new X509Store (StoreName.TrustedPeople);
			// default properties
			Assert.AreEqual (StoreLocation.CurrentUser, xs.Location, "Location");
			Assert.AreEqual ("TrustedPeople", xs.Name, "Name");
			Assert.IsNotNull (xs.Certificates, "Certificates");
		}

		[Test]
		public void ConstructorStoreNameTrustedPublisher () 
		{
			X509Store xs = new X509Store (StoreName.TrustedPublisher);
			// default properties
			Assert.AreEqual (StoreLocation.CurrentUser, xs.Location, "Location");
			Assert.AreEqual ("TrustedPublisher", xs.Name, "Name");
			Assert.IsNotNull (xs.Certificates, "Certificates");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add_Null ()
		{
			new X509Store ().Add (null);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Add_NotOpened ()
		{
			// Open wasn't called
			new X509Store ().Add (cert1);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Add_OpenReadOnly ()
		{
			X509Store xs = new X509Store (ReadOnlyStore);
			xs.Open (OpenFlags.ReadOnly);
			xs.Add (cert1);
		}

		[Test]
		public void Add_SameCertificate ()
		{
			X509Store xs = new X509Store (ReadWriteStore);
			xs.Open (OpenFlags.ReadWrite);
			int n = xs.Certificates.Count;
			xs.Add (cert1);
			xs.Add (cert1);
			Assert.AreEqual (n + 1, xs.Certificates.Count, "Count");
			xs.Close ();
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Add_Empty_Certificate ()
		{
			X509Store xs = new X509Store (ReadWriteStore);
			xs.Open (OpenFlags.ReadWrite);
			xs.Add (cert_empty);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Add_ExistingCertificateReadOnly ()
		{
			X509Store xs = new X509Store (ReadWriteStore);
			xs.Open (OpenFlags.ReadWrite);
			xs.Add (cert1);
			xs.Close ();
			xs.Open (OpenFlags.ReadOnly);
			xs.Add (cert1);
			xs.Close ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRange_Null ()
		{
			new X509Store ().AddRange (null);
		}

		[Test]
		public void AddRange_Empty_Closed ()
		{
			X509Store xs = new X509Store (ReadWriteStore);
			xs.AddRange (coll_empty);
			Assert.AreEqual (coll_empty.Count, xs.Certificates.Count, "Count");
		}

		[Test]
		public void AddRange_Empty_ReadOnly ()
		{
			X509Store xs = new X509Store (ReadWriteStore);
			xs.Open (OpenFlags.ReadOnly);
			xs.AddRange (coll_empty);
			Assert.AreEqual (coll_empty.Count, xs.Certificates.Count, "Count");
		}

		[Test]
		public void AddRange_Empty_ReadWrite ()
		{
			X509Store xs = new X509Store (ReadWriteStore);
			xs.Open (OpenFlags.ReadWrite);
			xs.AddRange (coll_empty);
			Assert.AreEqual (coll_empty.Count, xs.Certificates.Count, "Count");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void AddRange_Empty_Certificate ()
		{
			X509Store xs = new X509Store (ReadWriteStore);
			xs.Open (OpenFlags.ReadWrite);
			xs.AddRange (new X509Certificate2Collection (cert_empty));
		}

		[Test]
		public void AddRange ()
		{
			X509Store xs = new X509Store (ReadWriteStore);
			xs.Open (OpenFlags.ReadWrite);
			xs.AddRange (coll);
			Assert.AreEqual (coll.Count, xs.Certificates.Count, "Count");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void AddRange_NotOpened ()
		{
			// Open wasn't called
			new X509Store ().AddRange (coll);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void AddRange_OpenReadOnly ()
		{
			X509Store xs = new X509Store (ReadOnlyStore);
			xs.Open (OpenFlags.ReadOnly);
			xs.AddRange (coll);
		}

		[Test]
		public void Close_NotOpen ()
		{
			new X509Store ().Close ();
		}

		[Test]
		public void Close_Collection ()
		{
			X509Store xs = new X509Store (ReadWriteStore);
			xs.Open (OpenFlags.ReadWrite);
			xs.Add (cert1);
			Assert.AreEqual (1, xs.Certificates.Count, "Open");
			xs.Close ();
			Assert.AreEqual (0, xs.Certificates.Count, "Close");
		}

		[Test]
		public void Open_Invalid ()
		{
			X509Store xs = new X509Store (ReadWriteStore);
			xs.Open ((OpenFlags) Int32.MinValue);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Open_OpenExistingOnly ()
		{
			new X509Store ("doesn't-exists").Open (OpenFlags.OpenExistingOnly);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Open_Store_Null ()
		{
			// ctor is valid (see test) but can't be opened
			new X509Store (null).Open (OpenFlags.ReadOnly);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Open_Store_Empty ()
		{
			// ctor is valid (see test) but can't be opened
			new X509Store (String.Empty).Open (OpenFlags.ReadOnly);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Remove_Null ()
		{
			new X509Store ().Remove (null);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Remove_NotOpened ()
		{
			// Open wasn't called
			new X509Store ().Remove (cert1);
		}

		[Test]
		public void Remove_OpenReadOnly_Unexisting ()
		{
			X509Store xs = new X509Store (ReadOnlyStore);
			xs.Open (OpenFlags.ReadOnly);
			// note: cert1 wasn't present, remove "succeed"
			xs.Remove (cert1);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Remove_OpenReadOnly_Existing ()
		{
			X509Store xs = new X509Store (ReadWriteStore);
			xs.Open (OpenFlags.ReadWrite);
			xs.Add (cert1);
			xs.Close ();
			xs.Open (OpenFlags.ReadOnly);
			xs.Remove (cert1);
		}

		[Test]
		public void Remove_Empty_Certificate ()
		{
			X509Store xs = new X509Store (ReadWriteStore);
			xs.Open (OpenFlags.ReadWrite);
			// note: impossible to add cert_empty, so we add something else
			// to be sure we'll follow the complete code path (loop) of removal
			xs.Add (cert1);
			xs.Remove (cert_empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void RemoveRange_Null ()
		{
			new X509Store ().RemoveRange (null);
		}

		[Test]
		public void RemoveRange_Empty ()
		{
			X509Store xs = new X509Store ();
			xs.RemoveRange (coll_empty);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void RemoveRange_NotOpened ()
		{
			// Open wasn't called
			new X509Store ().RemoveRange (coll);
		}

		[Test]
		public void RemoveRange_OpenReadOnly_Unexisting ()
		{
			X509Store xs = new X509Store (ReadOnlyStore);
			xs.Open (OpenFlags.ReadOnly);
			// note: cert1 wasn't present, RemoveRange "succeed"
			xs.RemoveRange (coll);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void RemoveRange_OpenReadOnly_Existing ()
		{
			X509Store xs = new X509Store (ReadWriteStore);
			xs.Open (OpenFlags.ReadWrite);
			xs.AddRange (coll);
			xs.Close ();
			xs.Open (OpenFlags.ReadOnly);
			xs.RemoveRange (coll);
		}

		[Test]
		public void RemoveRange_Empty_Certificate ()
		{
			X509Store xs = new X509Store (ReadWriteStore);
			xs.Open (OpenFlags.ReadWrite);
			// note: impossible to add cert_empty, so we add something else
			// to be sure we'll follow the complete code path (loop) of removal
			xs.AddRange (coll);
			xs.RemoveRange (new X509Certificate2Collection (cert_empty));
		}

		[Test]
		public void Collection_Add ()
		{
			X509Store xs = new X509Store (ReadWriteStore);
			xs.Certificates.Add (cert1);
			Assert.AreEqual (0, xs.Certificates.Count, "Not Open");
			xs.Close ();
			Assert.AreEqual (0, xs.Certificates.Count, "Close");
		}
	}
}

