//
// ApplicationSecurityManagerTest.cs - 
//	NUnit Test Cases for ApplicationSecurityManager
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

#if NET_2_0

using NUnit.Framework;
using System;
using System.Collections;
using System.Security;
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class ApplicationSecurityManagerTest {

		private string defaultTrustManagerTypeName;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			defaultTrustManagerTypeName = ApplicationSecurityManager.ApplicationTrustManager.GetType ().AssemblyQualifiedName;
		}

		[Test]
		public void ApplicationTrustManager ()
		{
			Assert.IsNotNull (ApplicationSecurityManager.ApplicationTrustManager);
		}

		[Test]
		public void UserApplicationTrusts ()
		{
			Assert.AreEqual (0, ApplicationSecurityManager.UserApplicationTrusts.Count);
		}

		// FIXME: creating an ActivationContext here seems not easy

		[Test]
//		[ExpectedException (typeof (ArgumentNullException))]
		[ExpectedException (typeof (NullReferenceException))]
		public void DetermineApplicationTrust_Null_Null ()
		{
			ApplicationSecurityManager.DetermineApplicationTrust (null, null);
		}

		[Test]
//		[ExpectedException (typeof (ArgumentNullException))]
		[ExpectedException (typeof (NullReferenceException))]
		public void DetermineApplicationTrust_Null_TrustManagerContext ()
		{
			ApplicationSecurityManager.DetermineApplicationTrust (null, new TrustManagerContext ());
		}

		// testing the default application security manager here

		// FIXME: creating an ActivationContext here seems not easy

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DefaultTrustManager_DetermineApplicationTrust_Null_Null ()
		{
			ApplicationSecurityManager.ApplicationTrustManager.DetermineApplicationTrust (null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DefaultTrustManager_DetermineApplicationTrust_Null_TrustManagerContext ()
		{
			ApplicationSecurityManager.ApplicationTrustManager.DetermineApplicationTrust (null, new TrustManagerContext ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DefaultTrustManager_FromXml_Null ()
		{
			ApplicationSecurityManager.ApplicationTrustManager.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DefaultTrustManager_FromXml_BadTag ()
		{
			SecurityElement se = new SecurityElement (String.Empty);
			ApplicationSecurityManager.ApplicationTrustManager.FromXml (se);
		}

		private void CheckXml (SecurityElement se)
		{
			Assert.AreEqual (defaultTrustManagerTypeName, se.Attribute ("class"), "class");
			Assert.AreEqual ("1", se.Attribute ("version"), "version");
			Assert.AreEqual (2, se.Attributes.Count, "Count");
			Assert.IsNull (se.Children, "Children");
		}

		[Test]
		public void DefaultTrustManager_FromXml_NoAttributes ()
		{
			SecurityElement se = new SecurityElement ("IApplicationTrustManager");
			ApplicationSecurityManager.ApplicationTrustManager.FromXml (se);
			// accepted
			CheckXml (ApplicationSecurityManager.ApplicationTrustManager.ToXml ());
		}

		[Test]
		public void DefaultTrustManager_FromXml_BadClass ()
		{
			SecurityElement se = new SecurityElement ("IApplicationTrustManager");
			se.AddAttribute ("class", "System.DoesntExist");
			se.AddAttribute ("version", "1");
			ApplicationSecurityManager.ApplicationTrustManager.FromXml (se);
			// accepted
			CheckXml (ApplicationSecurityManager.ApplicationTrustManager.ToXml ());
		}

		[Test]
		public void DefaultTrustManager_FromXml_BadVersion ()
		{
			SecurityElement se = new SecurityElement ("IApplicationTrustManager");
			se.AddAttribute ("class", defaultTrustManagerTypeName);
			se.AddAttribute ("version", "42");
			ApplicationSecurityManager.ApplicationTrustManager.FromXml (se);
			// accepted
			CheckXml (ApplicationSecurityManager.ApplicationTrustManager.ToXml ());
		}

		[Test]
		public void DefaultTrustManager_FromXml ()
		{
			SecurityElement se = new SecurityElement ("IApplicationTrustManager");
			se.AddAttribute ("class", defaultTrustManagerTypeName);
			se.AddAttribute ("version", "1");
			ApplicationSecurityManager.ApplicationTrustManager.FromXml (se);
			// accepted
			CheckXml (ApplicationSecurityManager.ApplicationTrustManager.ToXml ());
		}

		[Test]
		public void DefaultTrustManager_ToXml ()
		{
			CheckXml (ApplicationSecurityManager.ApplicationTrustManager.ToXml ());
		}
	}
}

#endif
