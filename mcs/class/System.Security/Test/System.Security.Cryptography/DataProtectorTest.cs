//
// DataProtectorTest.cs: Test the DataProtector base class for simple data protectors
//
// Author:
//	Robert J. van der Boon  <rjvdboon@gmail.com>
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

using System;
using System.Security.Cryptography;
using System.Reflection;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography {
	[TestFixture]
	public class DataProtectorTest {
		#region Dummy Data Protector to help test DataProtector base class
		public class DummyDataProtector : DataProtector {
			internal bool prepend_hash;
			public DummyDataProtector (string appName, string primPurp, params string [] specPurp)
				: base (appName, primPurp, specPurp)
			{
			}
			public override bool IsReprotectRequired (byte [] encryptedData)
			{
				return true;
			}

			protected override byte [] ProviderProtect (byte [] userData)
			{
				byte[] protectedData = new byte [userData.Length];
				for (int i = 0; i < userData.Length; i++)
					protectedData [i] = userData [userData.Length - 1 - i];
				return protectedData;
			}

			protected override byte [] ProviderUnprotect (byte [] encryptedData)
			{
				byte[] unprotectedData = new byte [encryptedData.Length];
				for (int i = 0; i < encryptedData.Length; i++)
					unprotectedData [i] = encryptedData [encryptedData.Length - 1 - i];
				return unprotectedData;
			}
			protected override bool PrependHashedPurposeToPlaintext {
				get {
					return prepend_hash;
				}
			}
		}
		#endregion

		#region Tests DataProtector.Create
		[Test]
		public void TestCreate ()
		{
			string protectorType = typeof (DummyDataProtector).AssemblyQualifiedName;
			var specificPurposes = new string [] { "spec1", "spec2" };
			var protector = DataProtector.Create (protectorType, "app", "prim", specificPurposes);
			Assert.IsNotNull (protector, "DummyDataProtector");

			protectorType = "System.Security.Cryptography.NonExistingDataProtector";
			protector = DataProtector.Create (protectorType, "app", "prim", specificPurposes);
			Assert.IsNull (protector, "NonExistingDataProtector");
		}

		[Test]
		public void TestCreateConstructorRequiredArgumentsWithDummyDataProtector ()
		{
			string protectorType = typeof (DummyDataProtector).AssemblyQualifiedName;
			try {
				var specificPurposes = new string [] { "spec1", "spec2" };
				var protector = DataProtector.Create (protectorType, "", "prim", specificPurposes);
				Assert.Fail ("should have failed on empty app name");
			} catch (TargetInvocationException tie) {
				Assert.IsInstanceOfType (typeof(ArgumentException), tie.InnerException);
				Assert.AreEqual ("applicationName", ((ArgumentException)tie.InnerException).ParamName);
			}

			try {
				var specificPurposes = new string [] { "spec1", "spec2" };
				var protector = DataProtector.Create (protectorType, (string)null, "prim", specificPurposes);
				Assert.Fail ("should have failed on null app name");
			} catch (TargetInvocationException tie) {
				Assert.IsInstanceOfType (typeof(ArgumentException), tie.InnerException);
				Assert.AreEqual ("applicationName", ((ArgumentException)tie.InnerException).ParamName);
			}

			try {
				var specificPurposes = new string [] { "spec1", "spec2" };
				var protector = DataProtector.Create (protectorType, "  ", "prim", specificPurposes);
				Assert.Fail ("should have failed on whitespace app name");
			} catch (TargetInvocationException tie) {
				Assert.IsInstanceOfType (typeof(ArgumentException), tie.InnerException);
				Assert.AreEqual ("applicationName", ((ArgumentException)tie.InnerException).ParamName);
			}

			try {
				var specificPurposes = (string [])null;
				var protector = DataProtector.Create (protectorType, "app", "", specificPurposes);
				Assert.Fail ("should have failed on empty primaryPurpose");
			} catch (TargetInvocationException tie) {
				Assert.IsInstanceOfType (typeof(ArgumentException), tie.InnerException);
				Assert.AreEqual ("primaryPurpose", ((ArgumentException)tie.InnerException).ParamName);
			}

			try {
				var specificPurposes = (string [])null;
				var protector = DataProtector.Create (protectorType, "app", null, specificPurposes);
				Assert.Fail ("should have failed on null primaryPurpose ");
			} catch (TargetInvocationException tie) {
				Assert.IsInstanceOfType (typeof(ArgumentException), tie.InnerException);
				Assert.AreEqual ("primaryPurpose", ((ArgumentException)tie.InnerException).ParamName);
			}

			try {
				var specificPurposes = (string [])null;
				var protector = DataProtector.Create (protectorType, "app", "\t", specificPurposes);
				Assert.Fail ("should have failed on whitespace primaryPurpose");
			} catch (TargetInvocationException tie) {
				Assert.IsInstanceOfType (typeof(ArgumentException), tie.InnerException);
				Assert.AreEqual ("primaryPurpose", ((ArgumentException)tie.InnerException).ParamName);
			}

			try {
				var specificPurposes = (string [])null;
				var protector = DataProtector.Create (protectorType, "app", "prim", specificPurposes);
				Assert.Fail ("should have failed on null specific purposes");
			} catch (TargetInvocationException tie) {
				Assert.IsInstanceOfType (typeof(ArgumentException), tie.InnerException);
				Assert.AreEqual ("specificPurposes", ((ArgumentException)tie.InnerException).ParamName);
			}

			try {
				var specificPurposes = new string [] { "\r\n", "spec2" };
				var protector = DataProtector.Create (protectorType, "app", "prim", specificPurposes);
				Assert.Fail ("should have failed on null specific purpose");
			} catch (TargetInvocationException tie) {
				Assert.IsInstanceOfType (typeof(ArgumentException), tie.InnerException);
				Assert.AreEqual ("specificPurposes", ((ArgumentException)tie.InnerException).ParamName);
			}

			try {
				var specificPurposes = new string [] { null, "spec2" };
				var protector = DataProtector.Create (protectorType, "app", "prim", specificPurposes);
				Assert.Fail ("should have failed on null specific purpose");
			} catch (TargetInvocationException tie) {
				Assert.IsInstanceOfType (typeof(ArgumentException), tie.InnerException);
				Assert.AreEqual ("specificPurposes", ((ArgumentException)tie.InnerException).ParamName);
			}

			try {
				var specificPurposes = new string [] { "spec1", "" };
				var protector = DataProtector.Create (protectorType, "app", "prim", specificPurposes);
				Assert.Fail ("should have failed on empty specific purpose");
			} catch (TargetInvocationException tie) {
				Assert.IsInstanceOfType (typeof(ArgumentException), tie.InnerException);
				Assert.AreEqual ("specificPurposes", ((ArgumentException)tie.InnerException).ParamName);
			}
		}
		#endregion

		[Test]
		public void TestDataIsPrependedWithHashBeforeProtectingWhenPrependHashedPurposeToPlaintextIsTrue ()
		{
			var protector = new DummyDataProtector ("app", "prim");
			protector.prepend_hash = true;
			byte [] userData = new byte [] { 1, 2, 3, 4 };
			byte[] encryptedData = protector.Protect (userData);
			// SHA256 hash size is 32 bytes
			Assert.AreEqual (userData.Length + 32, encryptedData.Length, "hash length");
			for (int i = 0; i < userData.Length; i++) {
				Assert.AreEqual (userData [i], encryptedData [userData.Length - 1 - i], "encrypted data #" + i.ToString());
			}

			// Protect again, now with differnt primary purpose should result in different protected data
			var protectorWithDifferentPrimaryPurpose = new DummyDataProtector ("app", "prim2");
			protectorWithDifferentPrimaryPurpose.prepend_hash = true;
			byte [] encryptedData1 = protectorWithDifferentPrimaryPurpose.Protect (userData);
			bool allBytesEqual = true;
			for (int i = 4; i < encryptedData.Length; i++) {
				if (encryptedData [i] != encryptedData1 [i]) {
					allBytesEqual = false;
					break;
				}
			}
			Assert.IsFalse (allBytesEqual, "different primary purpose must result in different encrypted data");

			// Protect again, now with specific purpose should result in different protected data
			var protectorWithSpecificPurpose = new DummyDataProtector ("app", "prim", "spec1", "spec2");
			protectorWithSpecificPurpose.prepend_hash = true;
			byte [] encryptedData2 = protectorWithSpecificPurpose.Protect (userData);
			allBytesEqual = true;
			for (int i = 4; i < encryptedData.Length; i++) {
				if (encryptedData [i] != encryptedData2 [i]) {
					allBytesEqual = false;
					break;
				}
			}
			Assert.IsFalse(allBytesEqual, "with specific purpose must result in different encrypted data");

			// Protect again, now with different app name should result in different protected data
			var protectorWithDifferentAppName = new DummyDataProtector ("app2", "prim");
			protectorWithDifferentAppName.prepend_hash = true;
			byte [] encryptedData3 = protectorWithDifferentAppName.Protect (userData);
			allBytesEqual = true;
			for (int i = 4; i < encryptedData.Length; i++) {
				if (encryptedData [i] != encryptedData3 [i]) {
					allBytesEqual = false;
					break;
				}
			}
			Assert.IsFalse (allBytesEqual, "different applicationName must result in different encrypted data");
		}
		[Test]
		public void TestDataIsNotPrependedWithHashBeforeProtectingWhenPrependHashedPurposeToPlaintextIsFalse ()
		{
			var protector = new DummyDataProtector ("app", "prim");
			protector.prepend_hash = false;
			byte [] userData = new byte [] { 1, 2, 3, 4 };
			byte [] encryptedData = protector.Protect (userData);
			Assert.AreEqual(encryptedData.Length, userData.Length, "length");
			for (int i = 0; i < userData.Length; i++)
				Assert.AreEqual (userData [i], encryptedData [userData.Length - 1 - i], "#" + i.ToString());
		}
		[Test]
		public void TestProtectUnprotect ()
		{
			var protector = new DummyDataProtector ("app", "prim", "spec1");
			protector.prepend_hash = false;
			var userData = new byte [] { 9, 1, 5, 1, 55, 23, 12 };

			var roundTrip = protector.Unprotect (protector.Protect (userData));
			Assert.AreEqual (userData, roundTrip, "#1");
		}
		[Test]
		public void TestProtectUnprotectWithPrependedHash ()
		{
			var protector = new DummyDataProtector ("app", "prim", "spec1");
			protector.prepend_hash = true;
			var userData = new byte [] { 9, 1, 5, 1, 55, 23, 12 };

			var roundTrip = protector.Unprotect (protector.Protect (userData));
			Assert.AreEqual (userData, roundTrip, "#1");

			// Should fail on tampered hash
			var protectedData = protector.Protect (userData);
			protectedData [protectedData.Length - 5] = (byte)(protectedData [protectedData.Length - 5] ^ 0x12);
			try {
				protector.Unprotect (protectedData);
				Assert.Fail ("Should have failed on invalid hash");
			} catch (CryptographicException cryptoEx) {
				Assert.AreEqual("The purpose of the protected blob does not match the expected purpose value of this data protector instance.", cryptoEx.Message, "#2");
			}

		}
	}
}
