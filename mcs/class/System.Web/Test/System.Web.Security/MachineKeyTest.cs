//
// Authors:
//   Marek Habersack <grendel@twistedcode.net>
//
// (C) 2011 Novell, Inc (http://novell.com/)
//

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
using System.Security.Cryptography;


using System;
using System.Text;
using System.Web.Security;

using NUnit.Framework;

namespace MonoTests.System.Web.Security
{
	[TestFixture]
	public class MachineKeyTest
	{
		[Test]
		[MonoTODO ("Find out why the difference in result sizes exists between .NET and Mono")]
		public void Encode ()
		{
#if DOT_NET
			const int ALL_EXPECTED_SIZE = 192;
			const int ENCRYPTION_EXPECTED_SIZE = 128;
#else
			const int ALL_EXPECTED_SIZE = 128;
			const int ENCRYPTION_EXPECTED_SIZE = 64;
#endif
			const int VALIDATION_EXPECTED_SIZE = 64;
			
			Assert.Throws<ArgumentNullException> (() => {
				MachineKey.Encode (null, MachineKeyProtection.All);
			}, "#A1-1");

			string result = MachineKey.Encode (new byte[] {}, (MachineKeyProtection)12345);
			Assert.IsNotNull (result, "#A1-1");
			Assert.AreEqual (0, result.Length, "#A1-2");

			result = MachineKey.Encode (new byte[] {}, MachineKeyProtection.All);
			Assert.IsNotNull (result, "#B1-1");
			Assert.AreEqual (ALL_EXPECTED_SIZE, result.Length, "#B1-2");

			result = MachineKey.Encode (new byte [] { }, MachineKeyProtection.Encryption);
			Assert.IsNotNull (result, "#C1-1");
			Assert.AreEqual (ENCRYPTION_EXPECTED_SIZE, result.Length, "#C1-2");

			result = MachineKey.Encode (new byte [] { }, MachineKeyProtection.Validation);
			Assert.IsNotNull (result, "#D1-1");
			Assert.AreEqual (VALIDATION_EXPECTED_SIZE, result.Length, "#D1-2");
		}

		[Test]
		public void Decode ()
		{
			byte[] decoded;

			Assert.Throws<ArgumentNullException> (() => {
				MachineKey.Decode (null, MachineKeyProtection.All);
			}, "#A1-1");

			Assert.Throws<ArgumentException> (() => {
				decoded = MachineKey.Decode (String.Empty, MachineKeyProtection.All);
			}, "#A1-2");

			var sb = new StringBuilder ().Append ('0', 192);
			decoded = MachineKey.Decode (sb.ToString (), (MachineKeyProtection)12345);
			Assert.IsNotNull (decoded, "#A2-1");
			Assert.AreEqual (96, decoded.Length, "#A2-2");

			sb = new StringBuilder ().Append ('0', 128);
			decoded = MachineKey.Decode (sb.ToString (), (MachineKeyProtection) 12345);
			Assert.IsNotNull (decoded, "#A3-1");
			Assert.AreEqual (64, decoded.Length, "#A3-2");

			sb = new StringBuilder ().Append ('0', 96);
			decoded = MachineKey.Decode (sb.ToString (), (MachineKeyProtection) 12345);
			Assert.IsNotNull (decoded, "#A4-1");
			Assert.AreEqual (48, decoded.Length, "#A4-2");

			sb = new StringBuilder ().Append ('0', 10);
			decoded = MachineKey.Decode (sb.ToString (), (MachineKeyProtection) 12345);
			Assert.IsNotNull (decoded, "#A5-1");
			Assert.AreEqual (5, decoded.Length, "#A5-2");

			Assert.Throws<ArgumentException> (() => {
				decoded = MachineKey.Decode ("test", MachineKeyProtection.All);
			}, "#B1-1");

			Assert.Throws<ArgumentException> (() => {
				decoded = MachineKey.Decode ("test", MachineKeyProtection.Encryption);
			}, "#B1-2");

			Assert.Throws<ArgumentException> (() => {
				decoded = MachineKey.Decode ("test", MachineKeyProtection.Validation);
			}, "#B1-3");

			sb = new StringBuilder ().Append ('0', 1);
			try {
				decoded = MachineKey.Decode (sb.ToString (), MachineKeyProtection.All);
				Assert.Fail ("#C1-2 [no exception]");
			} catch (ArgumentException) {
				// success
			} catch {
				Assert.Fail ("#C1-2 [invalid exception]");
			}

			sb = new StringBuilder ().Append ('0', 2);
			try {
				decoded = MachineKey.Decode (sb.ToString (), MachineKeyProtection.All);
			} catch (ArgumentException ex) {
				Console.WriteLine (ex);
				Assert.Fail ("#C1-3");
			} catch {
				// success
			}

			sb = new StringBuilder ().Append ('0', 193);
			try {
				decoded = MachineKey.Decode (sb.ToString (), MachineKeyProtection.All);
				Assert.Fail ("#C2-1 [no exception]");
			} catch (ArgumentException) {
				// success
			} catch {
				Assert.Fail ("#C2-1 [invalid exception]");
			}

			sb = new StringBuilder ().Append ('0', 129);
			try {
				decoded = MachineKey.Decode (sb.ToString (), MachineKeyProtection.All);
				Assert.Fail ("#C3-1 [no exception]");
			} catch (ArgumentException) {
				// success
			} catch {
				Assert.Fail ("#C3-2 [invalid exception]");
			}

			sb = new StringBuilder ().Append ('0', 64);
			try {
				decoded = MachineKey.Decode (sb.ToString (), MachineKeyProtection.All);
			} catch (ArgumentException) {
				Assert.Fail ("#C4-1");
			} catch {
				// Success
			}
		}

		[Test]
		public void Protect ()
		{
			Assert.Throws<ArgumentNullException> (() =>
				MachineKey.Protect (null, null), 
				"MachineKey.Protect not throwing an ArgumentNullException");

			Assert.Throws<ArgumentNullException> (() => 
				MachineKey.Protect (null, new [] { "test" }), 
				"MachineKey.Protect not throwing an ArgumentNullException");

			var testString = "asfgasd43tqrt4";
			var validUsages = new [] { "usage1", "usage2" };
			var oneUsage = new [] { "usage1" };
			var invalidUsages = new [] { "usage1", "invalidUsage" };

			var plainBytes = Encoding.ASCII.GetBytes (testString);
			var encryptedBytes = MachineKey.Protect (plainBytes, validUsages);
			var validDecryptedBytes = MachineKey.Unprotect (encryptedBytes, validUsages);

			Assert.AreEqual (plainBytes, validDecryptedBytes, "Decryption didn't work");

			Assert.Throws<CryptographicException> (() => 
				MachineKey.Unprotect (encryptedBytes, invalidUsages), 
				"Purposes not encrypting properly");

			Assert.Throws<CryptographicException> (() => 
				MachineKey.Unprotect (encryptedBytes, oneUsage), 
				"Single purpose working when multiple supplied");
		}
	}
}
