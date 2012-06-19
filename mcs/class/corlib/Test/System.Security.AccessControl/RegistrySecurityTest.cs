//
// RawSecurityDescriptorTest.cs - NUnit Test Cases for RawSecurityDescriptor
//
// Author:
//	Kenneth Bell
//

using System;
using System.Security.AccessControl;
using System.Security.Principal;
using NUnit.Framework;

namespace MonoTests.System.Security.AccessControl {

	[TestFixture]
	public class RegistrySecurityTest {
		[Test]
		public void CheckSddlRoundTrip()
		{
			RegistrySecurity rs = new RegistrySecurity();
			rs.SetSecurityDescriptorSddlForm("O:BAG:BAD:(A;;;;;BA)", AccessControlSections.All);

			Assert.AreEqual("O:BAG:BAD:(A;;;;;BA)", rs.GetSecurityDescriptorSddlForm(AccessControlSections.All));
		}
	}
}
