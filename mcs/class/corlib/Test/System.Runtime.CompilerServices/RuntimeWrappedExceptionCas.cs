//
// RuntimeWrappedExceptionCas.cs - CAS Test Cases for 
//	System.Runtime.CompilerServices.RuntimeWrappedException
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

using MonoTests.System.Runtime.CompilerServices;

namespace MonoCasTests.System.Runtime.CompilerServices {

	[TestFixture]
	[Category ("CAS")]
	public class RuntimeWrappedExceptionCas {

		private RuntimeWrappedExceptionTest unit;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			unit = new RuntimeWrappedExceptionTest ();
			unit.FixtureSetUp ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void UnitTests_Deny_Unrestricted ()
		{
			unit.WrappedException ();
			unit.GetObjectData ();
			// this means GetObjectData isn't protected by a Demand
			// (but it may be, like documented, a LinkDemand)
		}

		// we use reflection to call RuntimeWrappedException as the GetObjectData method
		// is protected by LinkDemand (which will be converted into full demand, i.e. a 
		// stack walk) when reflection is used (i.e. it gets testable).

		[Test]
		[SecurityPermission (SecurityAction.Deny, SerializationFormatter = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetObjectData_Deny_SerializationFormatter ()
		{
			SerializationInfo info = null;
			StreamingContext context = new StreamingContext (StreamingContextStates.All);
			MethodInfo mi = typeof (RuntimeWrappedException).GetMethod ("GetObjectData");
			mi.Invoke (unit.rwe, new object[2] { info, context });
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, SerializationFormatter = true)]
		public void GetObjectData_PermitOnly_SerializationFormatter ()
		{
			SerializationInfo info = new SerializationInfo (typeof (RuntimeWrappedException), new FormatterConverter ());
			StreamingContext context = new StreamingContext (StreamingContextStates.All);
			MethodInfo mi = typeof (RuntimeWrappedException).GetMethod ("GetObjectData");
			mi.Invoke (unit.rwe, new object[2] { info, context });
		}
	}
}

#endif
