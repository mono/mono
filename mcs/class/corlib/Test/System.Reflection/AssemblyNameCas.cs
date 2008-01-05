//
// AssemblyNameCas.cs - CAS unit tests for System.Reflection.AssemblyName
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
using System.Configuration.Assemblies;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System.Reflection {

	[TestFixture]
	[Category ("CAS")]
	public class AssemblyNameCas {

		private MonoTests.System.Reflection.AssemblyNameTest ant;
		private AssemblyName main;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			ant = new MonoTests.System.Reflection.AssemblyNameTest ();
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
			ant.SetUp ();
		}

		[TearDown]
		public void TearDown ()
		{
			ant.TearDown ();
		}

		// Partial Trust Tests

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void PartialTrust_Deny_Unrestricted ()
		{
			// call "normal" unit with reduced privileges
			ant.Constructor0 ();
			ant.SetPublicKey ();
			ant.SetPublicKeyToken ();
			ant.Clone_Empty ();
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, SerializationFormatter = true)]
		public void PartialTrust_PermitOnly_SerializationFormatter ()
		{
			ant.Serialization_WithoutStrongName ();
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		public void PartialTrust_PermitOnly_UnmanagedCode ()
		{
			ant.KeyPair ();
		}


		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true, SerializationFormatter = true)]
		public void PartialTrust_PermitOnly_UnmanagedCodeSerializationFormatter ()
		{
			// UnmanagedCode is required to create a StrongNameKeyPair instance
			ant.Serialization ();
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, SerializationFormatter = true)]
		[ExpectedException (typeof (SecurityException))]
		public void PartialTrust_Deny_SerializationFormatter ()
		{
			ant.Serialization ();
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void PartialTrust_Deny_UnmanagedCode ()
		{
			ant.KeyPair ();
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void PartialTrust_PermitOnly_FileIOPermission ()
		{
			// call "normal" unit with reduced privileges
			ant.FullName_Name ();
			ant.FullName_Version ();
			ant.FullName_Culture ();
			ant.FullName_PublicKey ();
			ant.FullName_PublicKeyToken ();
			ant.FullName_VersionCulture ();
			ant.FullName_VersionPublicKey ();
			ant.FullName_CulturePublicKey ();
			ant.HashAlgorithm ();
			ant.Clone_Empty ();
			// mostly because they call Assembly.GetName
		}
	}
}
