//
// SecurityManagerTest.cs - NUnit Test Cases for SecurityManager
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2004 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Collections;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class SecurityManagerTest : Assertion {

		[Test]
		public void IsGranted_Null ()
		{
			// null is always granted
			Assert ("IsGranted_Null", SecurityManager.IsGranted (null));
		}

		[Test]
		public void PolicyHierarchy () 
		{
			IEnumerator e = SecurityManager.PolicyHierarchy ();
			AssertNotNull ("PolicyHierarchy", e);
		}
	}
}
