//
// X509StoreTest.cs - NUnit tests for X509Store
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using NUnit.Framework;

using System;
using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace MonoTests.System.Security.Cryptography.X509Certificates {

	[TestFixture]
	public class X509StoreTest : Assertion {

		[Test]
		public void ConstructorEmpty () 
		{
			X509Store xs = new X509Store ();
			// default properties
			AssertEquals ("Location", StoreLocation.CurrentUser, xs.Location);
			AssertEquals ("Name", "MY", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
		}


		[Test]
		public void ConstructorStoreLocationCurrentUser () 
		{
			X509Store xs = new X509Store (StoreLocation.CurrentUser);
			// default properties
			AssertEquals ("Location", StoreLocation.CurrentUser, xs.Location);
			AssertEquals ("Name", "MY", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
		}

		[Test]
		public void ConstructorStoreLocationLocalMachine () 
		{
			X509Store xs = new X509Store (StoreLocation.LocalMachine);
			// default properties
			AssertEquals ("Location", StoreLocation.LocalMachine, xs.Location);
			AssertEquals ("Name", "MY", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
		}


		[Test]
		public void ConstructorStoreStringAddressBook () 
		{
			X509Store xs = new X509Store ("AddressBook");
			// default properties
			AssertEquals ("Location", StoreLocation.CurrentUser, xs.Location);
			AssertEquals ("Name", "AddressBook", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
		}

		[Test]
		public void ConstructorStoreStringAuthRoot () 
		{
			X509Store xs = new X509Store ("AuthRoot");
			// default properties
			AssertEquals ("Location", StoreLocation.CurrentUser, xs.Location);
			AssertEquals ("Name", "AuthRoot", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
		}

		[Test]
		public void ConstructorStoreStringCertificateAuthority () 
		{
			X509Store xs = new X509Store ("CA");
			// default properties
			AssertEquals ("Location", StoreLocation.CurrentUser, xs.Location);
			AssertEquals ("Name", "CA", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
		}

		[Test]
		public void ConstructorStoreStringDisallowed () 
		{
			X509Store xs = new X509Store ("Disallowed");
			// default properties
			AssertEquals ("Location", StoreLocation.CurrentUser, xs.Location);
			AssertEquals ("Name", "Disallowed", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
		}

		[Test]
		public void ConstructorStoreStringMy () 
		{
			X509Store xs = new X509Store ("My");
			// default properties
			AssertEquals ("Location", StoreLocation.CurrentUser, xs.Location);
			AssertEquals ("Name", "My", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
		}

		[Test]
		public void ConstructorStoreStringRoot () 
		{
			X509Store xs = new X509Store ("Root");
			// default properties
			AssertEquals ("Location", StoreLocation.CurrentUser, xs.Location);
			AssertEquals ("Name", "Root", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
		}

		[Test]
		public void ConstructorStoreStringTrustedPeople () 
		{
			X509Store xs = new X509Store ("TrustedPeople");
			// default properties
			AssertEquals ("Location", StoreLocation.CurrentUser, xs.Location);
			AssertEquals ("Name", "TrustedPeople", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
		}

		[Test]
		public void ConstructorStoreStringTrustedPublisher () 
		{
			X509Store xs = new X509Store ("TrustedPublisher");
			// default properties
			AssertEquals ("Location", StoreLocation.CurrentUser, xs.Location);
			AssertEquals ("Name", "TrustedPublisher", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
		}

		[Test]
		public void ConstructorStoreStringMono () 
		{
			// mono isn't defined the StoreName
			X509Store xs = new X509Store ("Mono");
			// default properties
			AssertEquals ("Location", StoreLocation.CurrentUser, xs.Location);
			AssertEquals ("Name", "Mono", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
		}

		[Test]
		public void ConstructorStoreNameAddressBook () 
		{
			X509Store xs = new X509Store (StoreName.AddressBook);
			// default properties
			AssertEquals ("Location", StoreLocation.CurrentUser, xs.Location);
			AssertEquals ("Name", "AddressBook", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
		}

		[Test]
		public void ConstructorStoreNameAuthRoot () 
		{
			X509Store xs = new X509Store (StoreName.AuthRoot);
			// default properties
			AssertEquals ("Location", StoreLocation.CurrentUser, xs.Location);
			AssertEquals ("Name", "AuthRoot", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
		}

		[Test]
		public void ConstructorStoreNameCertificateAuthority () 
		{
			X509Store xs = new X509Store (StoreName.CertificateAuthority);
			// default properties
			AssertEquals ("Location", StoreLocation.CurrentUser, xs.Location);
			AssertEquals ("Name", "CA", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
		}

		[Test]
		public void ConstructorStoreNameDisallowed () 
		{
			X509Store xs = new X509Store (StoreName.Disallowed);
			// default properties
			AssertEquals ("Location", StoreLocation.CurrentUser, xs.Location);
			AssertEquals ("Name", "Disallowed", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
		}

		[Test]
		public void ConstructorStoreNameMy () {
			X509Store xs = new X509Store (StoreName.My);
			// default properties
			AssertEquals ("Location", StoreLocation.CurrentUser, xs.Location);
			AssertEquals ("Name", "My", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
		}

		[Test]
		public void ConstructorStoreNameRoot () 
		{
			X509Store xs = new X509Store (StoreName.Root);
			// default properties
			AssertEquals ("Location", StoreLocation.CurrentUser, xs.Location);
			AssertEquals ("Name", "Root", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
		}

		[Test]
		public void ConstructorStoreNameTrustedPeople () 
		{
			X509Store xs = new X509Store (StoreName.TrustedPeople);
			// default properties
			AssertEquals ("Location", StoreLocation.CurrentUser, xs.Location);
			AssertEquals ("Name", "TrustedPeople", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
		}

		[Test]
		public void ConstructorStoreNameTrustedPublisher () 
		{
			X509Store xs = new X509Store (StoreName.TrustedPublisher);
			// default properties
			AssertEquals ("Location", StoreLocation.CurrentUser, xs.Location);
			AssertEquals ("Name", "TrustedPublisher", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
		}


		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void AddEmptyCertificateToReadOnlyNonExistingStore () 
		{
			// mono isn't defined the StoreName
			X509Store xs = new X509Store ("NonExistingStore");
			// default properties
			AssertEquals ("Location", StoreLocation.CurrentUser, xs.Location);
			AssertEquals ("Name", "NonExistingStore", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
			xs.Open (OpenFlags.ReadOnly);
			xs.Add (new X509CertificateEx ());
			xs.Close ();
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void AddEmptyCertificateToReadWriteNonExistingStore () 
		{
			// mono isn't defined the StoreName
			X509Store xs = new X509Store ("NonExistingStore");
			// default properties
			AssertEquals ("Location", StoreLocation.CurrentUser, xs.Location);
			AssertEquals ("Name", "NonExistingStore", xs.Name);
			AssertNotNull ("Certificates", xs.Certificates);
			xs.Open (OpenFlags.ReadWrite);
			xs.Add (new X509CertificateEx ());
			xs.Close ();
		}
	}
}

#endif
