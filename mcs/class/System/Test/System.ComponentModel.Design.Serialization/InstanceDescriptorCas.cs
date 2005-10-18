//
// InstanceDescriptorCas.cs - CAS unit tests for 
//	System.ComponentModel.Design.Serialization.InstanceDescriptor
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
using System.Collections;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

using MonoTests.System.ComponentModel.Design.Serialization;

namespace MonoCasTests.System.ComponentModel.Design.Serialization {

	[TestFixture]
	[Category ("CAS")]
	public class InstanceDescriptorCas {

		[SetUp]
		public virtual void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void UnitTestReuse ()
		{
			InstanceDescriptorTest unit = new InstanceDescriptorTest ();
			unit.FixtureSetUp ();

			unit.Constructor_Null_ICollection ();
			unit.Constructor_MemberInfo_ICollection ();
			unit.Constructor_Null_ICollection_Boolean ();
			unit.Constructor_MemberInfo_ICollection_Boolean ();
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "Mono")]
		[ExpectedException (typeof (SecurityException))]
		public void Ctor2_LinkDemand_Deny_Anything ()
		{
			// denying anything -> not unrestricted
			Type[] types = new Type[2] { typeof (MemberInfo), typeof (ICollection) };
			ConstructorInfo ci = typeof (InstanceDescriptor).GetConstructor (types);
			Assert.IsNotNull (ci, ".ctor(MemberInfo,ICollection)");
			Assert.IsNotNull (ci.Invoke (new object[2] { null, new object[] { } }), "invoke");
		}

		[Test]
		[PermissionSet (SecurityAction.PermitOnly, Unrestricted = true)]
		public void Ctor2_LinkDemand_PermitOnly_Unrestricted ()
		{
			Type[] types = new Type[2] { typeof (MemberInfo), typeof (ICollection) };
			ConstructorInfo ci = typeof (InstanceDescriptor).GetConstructor (types);
			Assert.IsNotNull (ci, ".ctor(MemberInfo,ICollection)");
			Assert.IsNotNull (ci.Invoke (new object[2] { null, new object[] { } }), "invoke");
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "Mono")]
		[ExpectedException (typeof (SecurityException))]
		public void Ctor3_LinkDemand_Deny_Anything ()
		{
			// denying anything -> not unrestricted
			Type[] types = new Type[3] { typeof (MemberInfo), typeof (ICollection), typeof (bool) };
			ConstructorInfo ci = typeof (InstanceDescriptor).GetConstructor (types);
			Assert.IsNotNull (ci, ".ctor(MemberInfo,ICollection,bool)");
			Assert.IsNotNull (ci.Invoke (new object[3] { null, new object[] { }, false }), "invoke");
		}

		[Test]
		[PermissionSet (SecurityAction.PermitOnly, Unrestricted = true)]
		public void Ctor3_LinkDemand_PermitOnly_Unrestricted ()
		{
			Type[] types = new Type[3] { typeof (MemberInfo), typeof (ICollection), typeof (bool) };
			ConstructorInfo ci = typeof (InstanceDescriptor).GetConstructor (types);
			Assert.IsNotNull (ci, ".ctor(MemberInfo,ICollection,bool)");
			Assert.IsNotNull (ci.Invoke (new object[3] { null, new object[] { }, false }), "invoke");
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "Mono")]
		[ExpectedException (typeof (SecurityException))]
		public void Property_LinkDemand_Deny_Anything ()
		{
			InstanceDescriptor id = new InstanceDescriptor (null, new object[] { });
			// denying anything -> not unrestricted
			Type[] types = new Type[3] { typeof (MemberInfo), typeof (ICollection), typeof (bool) };
			MethodInfo mi = typeof (InstanceDescriptor).GetProperty ("IsComplete").GetGetMethod ();
			Assert.IsNotNull (mi, "IsComplete)");
			Assert.IsTrue ((bool)mi.Invoke (id, null), "invoke");
		}

		[Test]
		[PermissionSet (SecurityAction.PermitOnly, Unrestricted = true)]
		public void Property_LinkDemand_PermitOnly_Unrestricted ()
		{
			InstanceDescriptor id = new InstanceDescriptor (null, new object[] { });
			// denying anything -> not unrestricted
			Type[] types = new Type[3] { typeof (MemberInfo), typeof (ICollection), typeof (bool) };
			MethodInfo mi = typeof (InstanceDescriptor).GetProperty ("IsComplete").GetGetMethod ();
			Assert.IsNotNull (mi, "IsComplete)");
			Assert.IsTrue ((bool) mi.Invoke (id, null), "invoke");
		}
	}
}
