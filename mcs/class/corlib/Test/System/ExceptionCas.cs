//
// ExceptionCas.cs - CAS unit tests for System.Exception
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System {

	[TestFixture]
	[Category ("CAS")]
	public class ExceptionCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		// Partial Trust Tests - i.e. call "normal" unit with reduced privileges

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void PartialTrust_DenyUnrestricted_Success ()
		{
			MonoTests.System.ExceptionTest et = new MonoTests.System.ExceptionTest ();
			// call most (all but arguments checking) unit tests from ExceptionTest
			et.TestThrowOnBlockBoundaries ();
			et.Source_InnerException ();
		}

		// CAS tests

		[Test]
		public void NoRestriction ()
		{
			Exception e = new Exception ("message", new Exception ("inner message"));

			Assert.AreEqual ("message", e.Message, "Message");
			Assert.IsNotNull (e.InnerException, "InnerException");
			Assert.IsNotNull (e.ToString (), "ToString");
			Assert.IsNotNull (e.Data, "Data");
			Assert.IsNull (e.HelpLink, "HelpLink");
			Assert.IsNull (e.Source, "Source");
			Assert.IsNull (e.StackTrace, "StackTrace");
			Assert.IsNull (e.TargetSite, "TargetSite");
		}

		[Test]
		public void Throw_NoRestriction ()
		{
			try {
				throw new Exception ("message", new Exception ("inner message"));
			}
			catch (Exception e) {
				Assert.AreEqual ("message", e.Message, "Message");
				Assert.IsNotNull (e.InnerException, "InnerException");
				Assert.IsNotNull (e.ToString (), "ToString");
				Assert.IsNotNull (e.Data, "Data");
				Assert.IsNull (e.HelpLink, "HelpLink");
				Assert.IsNotNull (e.Source, "Source");
				Assert.IsNotNull (e.StackTrace, "StackTrace");
				Assert.IsNotNull (e.TargetSite, "TargetSite");
			}
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void FullRestriction ()
		{
			Exception e = new Exception ("message", new Exception ("inner message"));

			Assert.AreEqual ("message", e.Message, "Message");
			Assert.IsNotNull (e.InnerException, "InnerException");
			Assert.IsNotNull (e.ToString (), "ToString");
			Assert.IsNull (e.HelpLink, "HelpLink");
			Assert.IsNull (e.Source, "Source");
			Assert.IsNull (e.StackTrace, "StackTrace");
			Assert.IsNotNull (e.Data, "Data");
			// throws under 1.x
			Assert.IsNull (e.TargetSite, "TargetSite");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Throw_FullRestriction_Pass ()
		{
			try {
				throw new Exception ("message", new Exception ("inner message"));
			}
			catch (Exception e) {
				Assert.AreEqual ("message", e.Message, "Message");
				Assert.IsNotNull (e.InnerException, "InnerException");
				Assert.IsNull (e.HelpLink, "HelpLink");
				Assert.IsNotNull (e.Source, "Source");
				Assert.IsNotNull (e.Data, "Data");
				// throws under 1.x
				Assert.IsNotNull (e.TargetSite, "TargetSite");
			}
		}

#if !RUN_ONDOTNET || NET_4_0 // Disabled because .net 2 fails to load dll with "Failure decoding embedded permission set object" due to "/" path

		[Test]
		[FileIOPermission (SecurityAction.Deny, PathDiscovery = "/")]
		[ExpectedException (typeof (Exception))]
		public void Throw_FullRestriction_Fail_StackTrace ()
		{
			try {
				throw new Exception ("message");
			}
			catch (Exception e) {
				// throws only under 2.x
				string s = e.StackTrace;
			}
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, PathDiscovery = "/")]
		[ExpectedException (typeof (Exception))]
		public void Throw_FullRestriction_Fail_ToString ()
		{
			try {
				throw new Exception ("message");
			}
			catch (Exception e) {
				// throws only under 2.x
				string s = e.ToString ();
			}
		}
		
#endif
	}
}
