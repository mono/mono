//
// ModuleCas.cs - CAS unit tests for System.Reflection.Module
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
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System.Reflection {

	[TestFixture]
	[Category ("CAS")]
	public class ModuleCas {

		private MonoTests.System.Reflection.ModuleTest mt;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			mt = new MonoTests.System.Reflection.ModuleTest ();
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
			mt.SetUp ();
		}

		[TearDown]
		public void TearDown ()
		{
			mt.TearDown ();
		}

		// Partial Trust Tests - i.e. call "normal" unit with reduced privileges

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void PartialTrust_DenyUnrestricted ()
		{
			mt.FindTypes ();
#if NET_2_0
			mt.ResolveType ();
			mt.ResolveMethod ();
			mt.ResolveField ();
			mt.ResolveMember ();
#endif
		}

		// we use reflection to call Module as the GetObjectData method is protected 
		// by LinkDemand (which will be converted into full demand, i.e. a stack walk)
		// when reflection is used (i.e. it gets testable).

		[Test]
		[SecurityPermission (SecurityAction.Deny, SerializationFormatter = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetObjectData ()
		{
			SerializationInfo info = null;
			StreamingContext context = new StreamingContext (StreamingContextStates.All);
			Module[] modules = Assembly.GetExecutingAssembly ().GetModules ();
			MethodInfo mi = typeof (Module).GetMethod ("GetObjectData");
			mi.Invoke (modules [0], new object [2] { info, context });
		}
	}
}
