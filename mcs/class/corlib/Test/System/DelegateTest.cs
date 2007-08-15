// DelegateTest.cs - NUnit Test Cases for the System.Delegate class
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using NUnit.Framework;
using System;
using System.Reflection;

namespace MonoTests.System
{

[TestFixture]
public class DelegateTest : Assertion
{
#if NET_2_0
	class ParentClass {
	}

	class Subclass : ParentClass {
	}

	delegate ParentClass CoContraVariantDelegate (Subclass s);

	static Subclass CoContraVariantMethod (ParentClass s) {
		return null;
	}

	[Test]
	[Category ("TargetJvmNotWorking")]
	public void CoContraVariance () {
		CoContraVariantDelegate d = (CoContraVariantDelegate)Delegate.CreateDelegate (typeof (CoContraVariantDelegate), typeof (DelegateTest).GetMethod ("CoContraVariantMethod", BindingFlags.NonPublic|BindingFlags.Static));

		d (null);
	}
#endif
}
}
