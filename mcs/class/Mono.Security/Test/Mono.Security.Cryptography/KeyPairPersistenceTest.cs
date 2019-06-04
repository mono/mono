//
// KeyPairPersistenceTest.cs: Unit tests for keypair persistence
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
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

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;

using Mono.Security.Cryptography;

namespace MonoTests.Mono.Security.Cryptography {

	[TestFixture]
	public class KeyPairPersistenceTest {

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
			Assert.IsTrue (loaded.Filename.StartsWith (saved.Filename), "Filename");
			Assert.IsTrue (loaded.KeyValue.StartsWith (saved.KeyValue), "KeyValue");
			Assert.IsTrue (loaded.Parameters.KeyContainerName.StartsWith (saved.Parameters.KeyContainerName), "Parameters.KeyContainerName");
			Assert.AreEqual (saved.Parameters.KeyNumber, loaded.Parameters.KeyNumber, "Parameters.KeyNumber");
			Assert.IsTrue (loaded.Parameters.ProviderName.StartsWith (saved.Parameters.ProviderName), "Parameters.ProviderName");
			Assert.AreEqual (saved.Parameters.ProviderType, loaded.Parameters.ProviderType, "Parameters.ProviderType");
		}

		[Test]
		public void CspType () 
		{
			try {
				CspParameters cp = new CspParameters (-1);
				KeyPairPersistence kpp = new KeyPairPersistence (cp, "<keypair/>");
				kpp.Save ();

				Assert.IsTrue (File.Exists (kpp.Filename), "Save-Exists");
				// we didn't supply a name so we can't load it back

				kpp.Remove ();
				Assert.IsFalse (File.Exists (kpp.Filename), "Remove-!Exists");
			}
			catch (UnauthorizedAccessException) {
				Assert.Ignore ("Access denied to key containers files.");
			}
		}

		[Test]
		public void CspTypeProvider () 
		{
			try {
				CspParameters cp = new CspParameters (-2, "Provider");
				KeyPairPersistence kpp = new KeyPairPersistence (cp, "<keypair/>");
				kpp.Save ();

				Assert.IsTrue (File.Exists (kpp.Filename), "Save-Exists");
				// we didn't supply a name so we can't load it back

				kpp.Remove ();
				Assert.IsFalse (File.Exists (kpp.Filename), "Remove-!Exists");
			}
			catch (UnauthorizedAccessException) {
				Assert.Ignore ("Access denied to key containers files.");
			}
		}

		[Test]
		public void CspTypeProviderContainer () 
		{
			try {
				CspParameters cp = new CspParameters (-3, "Provider", "Container");
				KeyPairPersistence kpp = new KeyPairPersistence (cp, "<keypair/>");
				kpp.Save ();

				Assert.IsTrue (File.Exists (kpp.Filename), "Save-Exists");
				KeyPairPersistence kpp2 = new KeyPairPersistence (cp);
				Assert.IsTrue (kpp2.Load (), "Load");

				Compare (kpp, kpp2);
				kpp.Remove ();
				Assert.IsFalse (File.Exists (kpp.Filename), "Remove-!Exists");
			}
			catch (UnauthorizedAccessException) {
				Assert.Ignore ("Access denied to key containers files.");
			}
		}

		[Test]
		public void CspTypeProviderContainerKeyNumber () 
		{
			try {
				CspParameters cp = new CspParameters (-4, "Provider", "Container");
				cp.KeyNumber = 0;
				KeyPairPersistence kpp = new KeyPairPersistence (cp, "<keypair/>");
				kpp.Save ();

				Assert.IsTrue (File.Exists (kpp.Filename), "Save-Exists");
				KeyPairPersistence kpp2 = new KeyPairPersistence (cp);
				Assert.IsTrue (kpp2.Load (), "Load");

				Compare (kpp, kpp2);
				kpp.Remove ();
				Assert.IsFalse (File.Exists (kpp.Filename), "Remove-!Exists");
			}
			catch (UnauthorizedAccessException) {
				Assert.Ignore ("Access denied to key containers files.");
			}
		}

		[Test]
		public void CspFlagsDefault () 
		{
			try {
				CspParameters cp = new CspParameters (-5, "Provider", "Container");
				cp.Flags = CspProviderFlags.UseDefaultKeyContainer;
				KeyPairPersistence kpp = new KeyPairPersistence (cp, "<keypair/>");
				kpp.Save ();

				Assert.IsTrue (File.Exists (kpp.Filename), "Save-Exists");
				KeyPairPersistence kpp2 = new KeyPairPersistence (cp);
				Assert.IsTrue (kpp2.Load (), "Load");

				Compare (kpp, kpp2);
				kpp.Remove ();
				Assert.IsFalse (File.Exists (kpp.Filename), "Remove-!Exists");
			}
			catch (UnauthorizedAccessException) {
				Assert.Ignore ("Access denied to key containers files.");
			}
		}

		[Test]
		public void CspFlagsMachine () 
		{
			try {
				CspParameters cp = new CspParameters (-6, "Provider", "Container");
				cp.Flags = CspProviderFlags.UseMachineKeyStore;
				KeyPairPersistence kpp = new KeyPairPersistence (cp, "<keypair/>");
				kpp.Save ();

				Assert.IsTrue (File.Exists (kpp.Filename), "Save-Exists");
				KeyPairPersistence kpp2 = new KeyPairPersistence (cp);
				Assert.IsTrue (kpp2.Load (), "Load");

				Compare (kpp, kpp2);
				kpp.Remove ();
				Assert.IsFalse (File.Exists (kpp.Filename), "Remove-!Exists");
			}
			catch (CryptographicException ce) {
				// not everyone can write to the machine store
				if (!(ce.InnerException is UnauthorizedAccessException) && !(ce.InnerException is IOException ioe && ioe.HResult == 30 /* Read-only file system */))
					throw;
				Assert.Ignore ("Access denied to key containers files.");
			}
			catch (UnauthorizedAccessException) {
				Assert.Ignore ("Access denied to key containers files.");
			}
		}

		[Test]
		public void CspFlagsDefaultMachine () 
		{
			try {
				CspParameters cp = new CspParameters (-7, "Provider", "Container");
				cp.Flags = CspProviderFlags.UseDefaultKeyContainer | CspProviderFlags.UseMachineKeyStore;
				KeyPairPersistence kpp = new KeyPairPersistence (cp, "<keypair/>");
				kpp.Save ();

				Assert.IsTrue (File.Exists (kpp.Filename), "Save-Exists");
				KeyPairPersistence kpp2 = new KeyPairPersistence (cp);
				Assert.IsTrue (kpp2.Load (), "Load");

				Compare (kpp, kpp2);
				kpp.Remove ();
				Assert.IsFalse (File.Exists (kpp.Filename), "Remove-!Exists");
			}
			catch (CryptographicException ce) {
				// not everyone can write to the machine store
				if (!(ce.InnerException is UnauthorizedAccessException) && !(ce.InnerException is IOException ioe && ioe.HResult == 30 /* Read-only file system */))
					throw;
				Assert.Ignore ("Access denied to key containers files.");
			}
			catch (UnauthorizedAccessException) {
				Assert.Ignore ("Access denied to key containers files.");
			}
		}

		[Test]
		public void CspNoChangesPermitted () 
		{
			try {
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

				Assert.IsTrue (cp.Flags != copy.Flags, "Flags");
				Assert.IsTrue (cp.KeyContainerName != copy.KeyContainerName, "KeyContainerName");
				Assert.IsTrue (cp.KeyNumber != copy.KeyNumber, "KeyNumber");
				Assert.IsTrue (cp.ProviderName != copy.ProviderName, "ProviderName");
				Assert.IsTrue (cp.ProviderType != copy.ProviderType, "ProviderType");
			}
			catch (UnauthorizedAccessException) {
				Assert.Ignore ("Access denied to key containers files.");
			}
		}
	}
}
