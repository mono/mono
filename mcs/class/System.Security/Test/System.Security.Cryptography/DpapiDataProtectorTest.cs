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
	public class DpapiDataProtectorTest {
		#region Tests DataProtector.Create
		[Test]
		public void TestCreate ()
		{
			string protectorType = typeof (DpapiDataProtector).AssemblyQualifiedName;
			var specificPurposes = new string [] { "spec1", "spec2" };
			var protector = DataProtector.Create (protectorType, "app", "prim", specificPurposes);
			Assert.IsNotNull (protector, "DpapiDataProtector by AssemblyQualifiedName");

			protectorType = typeof (DpapiDataProtector).FullName;
			protector = DataProtector.Create (protectorType, "app", "prim", specificPurposes);
			Assert.IsNotNull (protector, "DpapiDataProtector by Namespace.TypeName");
		}

		[Test]
		public void TestCreateConstructorRequiredArgumentsWithDpapiDataProtector ()
		{
			string protectorType = "System.Security.Cryptography.DpapiDataProtector";
			try {
				var specificPurposes = new string [] { "spec1", "spec2" };
				var protector = DataProtector.Create (protectorType, "", "prim", specificPurposes);
				Assert.Fail ("should have failed on empty app name");
			} catch (TargetInvocationException tie) {
				Assert.IsInstanceOfType (typeof (ArgumentException), tie.InnerException);
				Assert.AreEqual ("applicationName", ((ArgumentException)tie.InnerException).ParamName);
			}

			try {
				var specificPurposes = new string [] { "spec1", "spec2" };
				var protector = DataProtector.Create (protectorType, (string)null, "prim", specificPurposes);
				Assert.Fail ("should have failed on null app name");
			} catch (TargetInvocationException tie) {
				Assert.IsInstanceOfType (typeof (ArgumentException), tie.InnerException);
				Assert.AreEqual ("applicationName", ((ArgumentException)tie.InnerException).ParamName);
			}

			try {
				var specificPurposes = new string [] { "spec1", "spec2" };
				var protector = DataProtector.Create (protectorType, "  ", "prim", specificPurposes);
				Assert.Fail ("should have failed on whitespace app name");
			} catch (TargetInvocationException tie) {
				Assert.IsInstanceOfType (typeof (ArgumentException), tie.InnerException);
				Assert.AreEqual ("applicationName", ((ArgumentException)tie.InnerException).ParamName);
			}

			try {
				var specificPurposes = (string [])null;
				var protector = DataProtector.Create (protectorType, "app", "", specificPurposes);
				Assert.Fail ("should have failed on empty primaryPurpose");
			} catch (TargetInvocationException tie) {
				Assert.IsInstanceOfType (typeof (ArgumentException), tie.InnerException);
				Assert.AreEqual ("primaryPurpose", ((ArgumentException)tie.InnerException).ParamName);
			}

			try {
				var specificPurposes = (string [])null;
				var protector = DataProtector.Create (protectorType, "app", null, specificPurposes);
				Assert.Fail ("should have failed on null primaryPurpose ");
			} catch (TargetInvocationException tie) {
				Assert.IsInstanceOfType (typeof (ArgumentException), tie.InnerException);
				Assert.AreEqual ("primaryPurpose", ((ArgumentException)tie.InnerException).ParamName);
			}

			try {
				var specificPurposes = (string [])null;
				var protector = DataProtector.Create (protectorType, "app", "\t", specificPurposes);
				Assert.Fail ("should have failed on whitespace primaryPurpose");
			} catch (TargetInvocationException tie) {
				Assert.IsInstanceOfType (typeof (ArgumentException), tie.InnerException);
				Assert.AreEqual ("primaryPurpose", ((ArgumentException)tie.InnerException).ParamName);
			}

			try {
				var specificPurposes = (string [])null;
				var protector = DataProtector.Create (protectorType, "app", "prim", specificPurposes);
				Assert.Fail ("should have failed on null specific purposes");
			} catch (TargetInvocationException tie) {
				Assert.IsInstanceOfType (typeof (ArgumentException), tie.InnerException);
				Assert.AreEqual ("specificPurposes", ((ArgumentException)tie.InnerException).ParamName);
			}

			try {
				var specificPurposes = new string [] { "\r\n", "spec2" };
				var protector = DataProtector.Create (protectorType, "app", "prim", specificPurposes);
				Assert.Fail ("should have failed on null specific purpose");
			} catch (TargetInvocationException tie) {
				Assert.IsInstanceOfType (typeof (ArgumentException), tie.InnerException);
				Assert.AreEqual ("specificPurposes", ((ArgumentException)tie.InnerException).ParamName);
			}

			try {
				var specificPurposes = new string [] { null, "spec2" };
				var protector = DataProtector.Create (protectorType, "app", "prim", specificPurposes);
				Assert.Fail ("should have failed on null specific purpose");
			} catch (TargetInvocationException tie) {
				Assert.IsInstanceOfType (typeof (ArgumentException), tie.InnerException);
				Assert.AreEqual ("specificPurposes", ((ArgumentException)tie.InnerException).ParamName);
			}

			try {
				var specificPurposes = new string [] { "spec1", "" };
				var protector = DataProtector.Create (protectorType, "app", "prim", specificPurposes);
				Assert.Fail ("should have failed on empty specific purpose");
			} catch (TargetInvocationException tie) {
				Assert.IsInstanceOfType (typeof (ArgumentException), tie.InnerException);
				Assert.AreEqual ("specificPurposes", ((ArgumentException)tie.InnerException).ParamName);
			}
		}
		#endregion

		[Test]
		public void TestProtectUnprotectWithDefaultScope ()
		{
			var protector = new DpapiDataProtector ("app", "prim", "spec1");
			var userData = new byte [] { 9, 1, 5, 1, 55, 23, 12 };

			var roundTrip = protector.Unprotect (protector.Protect (userData));
			Assert.AreEqual (userData, roundTrip, "#1");
		}

		[Test]
		public void TestDefaultScopeIsCurrentUserScope ()
		{
			var protector = new DpapiDataProtector ("app", "prim", "spec1");
			Assert.AreEqual (DataProtectionScope.CurrentUser, protector.Scope);
			var userData = new byte [] { 9, 1, 5, 1, 55, 23, 12 };

			var unprotector = new DpapiDataProtector ("app", "prim", "spec1") {
				Scope = DataProtectionScope.CurrentUser
			};
			var roundTrip = unprotector.Unprotect (protector.Protect (userData));
			Assert.AreEqual (userData, roundTrip, "#1");
		}

		[Test]
		public void TestProtectUnprotectWithExplicitUserScope ()
		{
			var protector = new DpapiDataProtector ("app", "prim", "spec1") {
				Scope = DataProtectionScope.CurrentUser
			};
			Assert.AreEqual (DataProtectionScope.CurrentUser, protector.Scope);
			var userData = new byte [] { 9, 1, 5, 1, 55, 23, 12 };

			var roundTrip = protector.Unprotect (protector.Protect (userData));
			Assert.AreEqual (userData, roundTrip, "#1");
		}
		[Test]
		public void TestProtectUnprotectWithExplicitMachineScope ()
		{
			var protector = new DpapiDataProtector ("app", "prim", "spec1") {
				Scope = DataProtectionScope.LocalMachine
			};
			Assert.AreEqual (DataProtectionScope.LocalMachine, protector.Scope);
			var userData = new byte [] { 9, 1, 5, 1, 55, 23, 12 };

			var roundTrip = protector.Unprotect (protector.Protect (userData));
			Assert.AreEqual (userData, roundTrip, "#1");
		}

		[Test]
		[Ignore("MONO's ProtectedData implementation does not detect the scope during Unprotect")]
		public void TestProtectWithUserScopeUnprotectWithMachineScopeIgnoresScopeOnUnprotect ()
		{
			var protector = new DpapiDataProtector ("app", "prim", "spec1") {
				Scope = DataProtectionScope.CurrentUser
			};
			var unprotector = new DpapiDataProtector ("app", "prim", "spec1") {
				Scope = DataProtectionScope.LocalMachine
			};
			var userData = new byte [] { 9, 1, 5, 1, 55, 23, 12 };
			var roundtrip = unprotector.Unprotect (protector.Protect (userData));
			Assert.AreEqual (userData, roundtrip);
		}

		[Test]
		[Ignore("MONO's ProtectedData implementation does not detect the scope during Unprotect")]
		public void TestProtectWithMachineScopeUnprotectWithUserScopeIgnoresScopeOnUnprotect ()
		{
			var protector = new DpapiDataProtector ("app", "prim", "spec1") {
				Scope = DataProtectionScope.LocalMachine
			};
			var unprotector = new DpapiDataProtector ("app", "prim", "spec1") {
				Scope = DataProtectionScope.CurrentUser
			};
			var userData = new byte [] { 9, 1, 5, 1, 55, 23, 12 };
			var roundtrip = unprotector.Unprotect (protector.Protect (userData));
			Assert.AreEqual (userData, roundtrip);
		}
	}
}
