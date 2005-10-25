//
// CompilerInfoCas.cs -
//	CAS unit tests for System.CodeDom.Compiler.CompilerInfo
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
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

using MCSharp = Microsoft.CSharp;
using MonoTests.System.CodeDom.Compiler;

namespace MonoCasTests.System.CodeDom.Compiler {

	[TestFixture]
	[Category ("CAS")]
	[Category ("NotWorking")] // FIXME: missing config stuff ???
	public class CompilerInfoCas {

		private CompilerInfo ci;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			ci = CodeDomProvider.GetCompilerInfo ("c#");
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}
		
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Default ()
		{
			Assert.AreEqual (typeof (MCSharp.CSharpCodeProvider), ci.CodeDomProviderType, "CodeDomProviderType");
			Assert.IsTrue (ci.IsCodeDomProviderTypeValid, "IsCodeDomProviderTypeValid");

			Assert.IsTrue (ci.Equals (ci), "Equals");
			Assert.IsTrue (ci.GetHashCode () != 0, "GetHashCode");
			Assert.AreEqual (2, ci.GetExtensions ().Length, "GetExtensions"); // .cs cs
			Assert.AreEqual (3, ci.GetLanguages ().Length, "GetLanguages"); // c# cs csharp

			try {
				Assert.IsNotNull (ci.CreateDefaultCompilerParameters (), "CreateDefaultCompilerParameters");
			}
			catch (NotImplementedException) {
				// mono
			}
		}

		[Test]
		// no restriction
		public void CreateProvider_No_Restriction ()
		{
			Assert.IsNotNull (ci.CreateProvider (), "CreateProvider");
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "Mono")]
		[ExpectedException (typeof (SecurityException))]
		public void CreateProvider_Deny_Anything ()
		{
			ci.CreateProvider ();
		}

		[Test]
		public void LinkDemand_No_Restriction ()
		{
			MethodInfo mi = typeof (CompilerInfo).GetProperty ("IsCodeDomProviderTypeValid").GetGetMethod ();
			Assert.IsNotNull (mi, "IsCodeDomProviderTypeValid");
			Assert.IsTrue ((bool) mi.Invoke (ci, null), "invoke");
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "Mono")]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_Deny_Anything ()
		{
			// denying anything results in a non unrestricted permission set
			MethodInfo mi = typeof (CompilerInfo).GetProperty ("IsCodeDomProviderTypeValid").GetGetMethod ();
			Assert.IsNotNull (mi, "IsCodeDomProviderTypeValid");
			Assert.IsTrue ((bool) mi.Invoke (ci, null), "invoke");
		}
	}
}

#endif
