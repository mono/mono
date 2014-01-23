//
// SecurityCriticalAttributeTest.cs -
//	NUnit Test Cases for SecurityCriticalAttribute
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

namespace MonoTests.System.Security {

	[TestFixture]
	public class SecurityCriticalAttributeTest {
#if !MOBILE
		[Test]
		public void Constructor_Default ()
		{
			SecurityCriticalAttribute sca = new SecurityCriticalAttribute ();
			Assert.AreEqual (SecurityCriticalScope.Explicit, sca.Scope);
		}

		[Test]
		public void Constructor_Scope_Everything ()
		{
			SecurityCriticalAttribute sca = new SecurityCriticalAttribute (SecurityCriticalScope.Everything);
			Assert.AreEqual (SecurityCriticalScope.Everything, sca.Scope);
		}

		[Test]
		public void Constructor_Scope_Explicit ()
		{
			SecurityCriticalAttribute sca = new SecurityCriticalAttribute (SecurityCriticalScope.Explicit);
			Assert.AreEqual (SecurityCriticalScope.Explicit, sca.Scope);
		}

		[Test]
		public void Constructor_Scope_Bad ()
		{
			SecurityCriticalScope scs = (SecurityCriticalScope)UInt32.MinValue;
			SecurityCriticalAttribute sca = new SecurityCriticalAttribute (scs);
			Assert.AreEqual (SecurityCriticalScope.Explicit, sca.Scope);
		}
#endif
		[Test]
		public void Attributes ()
		{
			Type t = typeof (SecurityCriticalAttribute);
			Assert.IsFalse (t.IsSerializable, "IsSerializable");

			object [] attrs = t.GetCustomAttributes (typeof (AttributeUsageAttribute), false);
			Assert.AreEqual (1, attrs.Length, "AttributeUsage");
			AttributeUsageAttribute aua = (AttributeUsageAttribute)attrs [0];
			Assert.IsFalse (aua.AllowMultiple, "AllowMultiple");
			Assert.IsFalse (aua.Inherited, "Inherited");
#if NET_4_0 && !MOBILE
			AttributeTargets at = (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Delegate);
#else
			AttributeTargets at = (AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate);
#endif
			Assert.AreEqual (at, aua.ValidOn, "ValidOn");
		}
	}
}
