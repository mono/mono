//
// KeyPairPersistenceTest.cs: Unit tests for keypair persistence
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;

using Mono.Security.Cryptography;

namespace MonoTests.Mono.Security.Cryptography {

	[TestFixture]
	public class KeyPairPersistenceTest : Assertion {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_Null () 
		{
			KeyPairPersistence kpp = new KeyPairPersistence (null);
		}

		private void Compare (KeyPairPersistence saved, KeyPairPersistence loaded) 
		{
			// Note: there is an additional Environement.NewLine 
			// at the end of the loaded string - that's why we do
			// not use AssertEquals (for strings)
			Assert ("Filename", loaded.Filename.StartsWith (saved.Filename));
			Assert ("KeyValue", loaded.KeyValue.StartsWith (saved.KeyValue));
			Assert ("Parameters.KeyContainerName", loaded.Parameters.KeyContainerName.StartsWith (saved.Parameters.KeyContainerName));
			AssertEquals ("Parameters.KeyNumber", saved.Parameters.KeyNumber, loaded.Parameters.KeyNumber);
			Assert ("Parameters.ProviderName", loaded.Parameters.ProviderName.StartsWith (saved.Parameters.ProviderName));
			AssertEquals ("Parameters.ProviderType", saved.Parameters.ProviderType, loaded.Parameters.ProviderType);
		}

		[Test]
		public void CspType () 
		{
			CspParameters cp = new CspParameters (-1);
			KeyPairPersistence kpp = new KeyPairPersistence (cp, "<keypair/>");
			kpp.Save ();

			Assert ("Save-Exists", File.Exists (kpp.Filename));
			// we didn't supply a name so we can't load it back

			kpp.Remove ();
			Assert ("Remove-!Exists", !File.Exists (kpp.Filename));
		}

		[Test]
		public void CspTypeProvider () 
		{
			CspParameters cp = new CspParameters (-2, "Provider");
			KeyPairPersistence kpp = new KeyPairPersistence (cp, "<keypair/>");
			kpp.Save ();

			Assert ("Save-Exists", File.Exists (kpp.Filename));
			// we didn't supply a name so we can't load it back

			kpp.Remove ();
			Assert ("Remove-!Exists", !File.Exists (kpp.Filename));
		}

		[Test]
		public void CspTypeProviderContainer () 
		{
			CspParameters cp = new CspParameters (-3, "Provider", "Container");
			KeyPairPersistence kpp = new KeyPairPersistence (cp, "<keypair/>");
			kpp.Save ();

			Assert ("Save-Exists", File.Exists (kpp.Filename));
			KeyPairPersistence kpp2 = new KeyPairPersistence (cp);
			Assert ("Load", kpp2.Load ());

			Compare (kpp, kpp2);
			kpp.Remove ();
			Assert ("Remove-!Exists", !File.Exists (kpp.Filename));
		}

		[Test]
		public void CspTypeProviderContainerKeyNumber () 
		{
			CspParameters cp = new CspParameters (-4, "Provider", "Container");
			cp.KeyNumber = 0;
			KeyPairPersistence kpp = new KeyPairPersistence (cp, "<keypair/>");
			kpp.Save ();

			Assert ("Save-Exists", File.Exists (kpp.Filename));
			KeyPairPersistence kpp2 = new KeyPairPersistence (cp);
			Assert ("Load", kpp2.Load ());

			Compare (kpp, kpp2);
			kpp.Remove ();
			Assert ("Remove-!Exists", !File.Exists (kpp.Filename));
		}

		[Test]
		public void CspFlagsDefault () 
		{
			CspParameters cp = new CspParameters (-5, "Provider", "Container");
			cp.Flags = CspProviderFlags.UseDefaultKeyContainer;
			KeyPairPersistence kpp = new KeyPairPersistence (cp, "<keypair/>");
			kpp.Save ();

			Assert ("Save-Exists", File.Exists (kpp.Filename));
			KeyPairPersistence kpp2 = new KeyPairPersistence (cp);
			Assert ("Load", kpp2.Load ());

			Compare (kpp, kpp2);
			kpp.Remove ();
			Assert ("Remove-!Exists", !File.Exists (kpp.Filename));
		}

		[Test]
		public void CspFlagsMachine () 
		{
			CspParameters cp = new CspParameters (-6, "Provider", "Container");
			cp.Flags = CspProviderFlags.UseMachineKeyStore;
			KeyPairPersistence kpp = new KeyPairPersistence (cp, "<keypair/>");
			try {
				kpp.Save ();

				Assert ("Save-Exists", File.Exists (kpp.Filename));
				KeyPairPersistence kpp2 = new KeyPairPersistence (cp);
				Assert ("Load", kpp2.Load ());

				Compare (kpp, kpp2);
				kpp.Remove ();
				Assert ("Remove-!Exists", !File.Exists (kpp.Filename));
			}
			catch (CryptographicException ce) {
				// not everyone can write to the machine store
				if (!(ce.InnerException is UnauthorizedAccessException))
					throw;
			}
		}

		[Test]
		public void CspFlagsDefaultMachine () 
		{
			CspParameters cp = new CspParameters (-7, "Provider", "Container");
			cp.Flags = CspProviderFlags.UseDefaultKeyContainer | CspProviderFlags.UseMachineKeyStore;
			KeyPairPersistence kpp = new KeyPairPersistence (cp, "<keypair/>");
			try {
				kpp.Save ();

				Assert ("Save-Exists", File.Exists (kpp.Filename));
				KeyPairPersistence kpp2 = new KeyPairPersistence (cp);
				Assert ("Load", kpp2.Load ());

				Compare (kpp, kpp2);
				kpp.Remove ();
				Assert ("Remove-!Exists", !File.Exists (kpp.Filename));
			}
			catch (CryptographicException ce) {
				// not everyone can write to the machine store
				if (!(ce.InnerException is UnauthorizedAccessException))
					throw;
			}
		}

		[Test]
		public void CspNoChangesPermitted () 
		{
			CspParameters cp = new CspParameters (-8, "Provider", "Container");
			cp.KeyNumber = 0;
			cp.Flags = CspProviderFlags.UseMachineKeyStore;

			KeyPairPersistence kpp = new KeyPairPersistence (cp);
			CspParameters copy = kpp.Parameters;
			copy.Flags = CspProviderFlags.UseDefaultKeyContainer;
			copy.KeyContainerName = "NewContainerName";
			copy.KeyNumber = 1;
			copy.ProviderName = "NewProviderName";
			copy.ProviderType = -9;

			Assert ("Flags", cp.Flags != copy.Flags);
			Assert ("KeyContainerName", cp.KeyContainerName != copy.KeyContainerName);
			Assert ("KeyNumber", cp.KeyNumber != copy.KeyNumber);
			Assert ("ProviderName", cp.ProviderName != copy.ProviderName);
			Assert ("ProviderType", cp.ProviderType != copy.ProviderType);
		}
	}
}
