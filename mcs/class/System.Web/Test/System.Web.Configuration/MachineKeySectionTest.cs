//
// Unit tests for MachineKeySection
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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

using System;
using System.Configuration;
using System.Web.Configuration;
using NUnit.Framework;

namespace MonoTests.System.Web.Configuration {

	[TestFixture]
	public class MachineKeySectionTest {

		[Test]
		public void DefaultValues ()
		{
			MachineKeySection section = new MachineKeySection ();
#if NET_4_0
			Assert.AreEqual (MachineKeyCompatibilityMode.Framework20SP1, section.CompatibilityMode, "CompatibilityMode");
#endif
			Assert.AreEqual ("Auto", section.Decryption, "Decryption");
			Assert.AreEqual ("AutoGenerate,IsolateApps", section.DecryptionKey, "DecryptionKey");
#if NET_4_0
			Assert.AreEqual (MachineKeyValidation.HMACSHA256, section.Validation, "Validation");
			Assert.AreEqual ("HMACSHA256", section.ValidationAlgorithm, "ValidationAlgorithm");
#else
			Assert.AreEqual (MachineKeyValidation.SHA1, section.Validation, "Validation");
#endif
			Assert.AreEqual ("AutoGenerate,IsolateApps", section.ValidationKey, "ValidationKey");
		}

		[Test]
#if NET_4_0
		[ExpectedException (typeof (NullReferenceException))]
#else
		[ExpectedException (typeof (ConfigurationErrorsException))]
#endif
		public void Decryption_Null ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Decryption = null;
		}

		[Test]
		[ExpectedException (typeof (ConfigurationErrorsException))]
		public void Decryption_Empty ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Decryption = String.Empty;
		}
#if NET_4_0
		[Test]
		public void Decryption_RC2 ()
		{
			MachineKeySection section = new MachineKeySection ();
			Assert.AreEqual ("Auto", section.Decryption, "before");

			section.Decryption = "alg:RC2";
			Assert.AreEqual ("alg:RC2", section.Decryption, "after");
		}

		[Test]
		public void Decryption_InvalidName ()
		{
			MachineKeySection section = new MachineKeySection ();
			Assert.AreEqual ("Auto", section.Decryption, "before");

			section.Decryption = "alg:UnexistingType";
			// looks like the problem is found (much) later
			Assert.AreEqual ("alg:UnexistingType", section.Decryption, "Decryption");
		}

		[Test]
		public void ValidationAlgorithm_Null ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.ValidationAlgorithm = null;
			Assert.AreEqual ("HMACSHA256", section.ValidationAlgorithm, "ValidationAlgorithm");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ValidationAlgorithm_Empty ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.ValidationAlgorithm = String.Empty;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Validation_Custom ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.Custom;
			// cannot be set directly
		}

		[Test]
		public void Validation ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.Validation = MachineKeyValidation.AES;
			Assert.AreEqual ("AES", section.ValidationAlgorithm, "AES");
			section.Validation = MachineKeyValidation.HMACSHA256;
			Assert.AreEqual ("HMACSHA256", section.ValidationAlgorithm, "HMACSHA256");
			section.Validation = MachineKeyValidation.HMACSHA384;
			Assert.AreEqual ("HMACSHA384", section.ValidationAlgorithm, "HMACSHA384");
			section.Validation = MachineKeyValidation.HMACSHA512;
			Assert.AreEqual ("HMACSHA512", section.ValidationAlgorithm, "HMACSHA512");
			section.Validation = MachineKeyValidation.MD5;
			Assert.AreEqual ("MD5", section.ValidationAlgorithm, "MD5");
			section.Validation = MachineKeyValidation.SHA1;
			Assert.AreEqual ("SHA1", section.ValidationAlgorithm, "SHA1");
			// special case, enum value and algorithm names differs
			section.Validation = MachineKeyValidation.TripleDES;
			Assert.AreEqual ("3DES", section.ValidationAlgorithm, "3DES");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ValidationAlgorithm ()
		{
			MachineKeySection section = new MachineKeySection ();
			section.ValidationAlgorithm = "HMACRIPEMD160";
			// syntax is: alg:something-deriving-from-KeyedHashAlgorithm
		}

		[Test]
		public void ValidationAlgorithm_RIPEMD160 ()
		{
			MachineKeySection section = new MachineKeySection ();
			Assert.AreEqual (MachineKeyValidation.HMACSHA256, section.Validation, "before");

			section.ValidationAlgorithm = "alg:HMACRIPEMD160";
			Assert.AreEqual (MachineKeyValidation.Custom, section.Validation, "after");
			Assert.AreEqual ("alg:HMACRIPEMD160", section.ValidationAlgorithm, "ValidationAlgorithm");
		}

		[Test]
		public void ValidationAlgorithm_InvalidName ()
		{
			MachineKeySection section = new MachineKeySection ();
			Assert.AreEqual (MachineKeyValidation.HMACSHA256, section.Validation, "before");

			section.ValidationAlgorithm = "alg:UnexistingType";
			// looks like the problem is found (much) later
			Assert.AreEqual (MachineKeyValidation.Custom, section.Validation, "after");
			Assert.AreEqual ("alg:UnexistingType", section.ValidationAlgorithm, "ValidationAlgorithm");
		}
#endif
	}
}

