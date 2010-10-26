using NUnit.Framework;

namespace CoreClr.Tools.Tests
{
	[TestFixture]
	public class SecurityAttributeDescriptorTest
	{
		[Test]
		public void ToStringFormat()
		{
			AssertString("SC-M: System.Void T1.M1()",
				new SecurityAttributeDescriptor(
					SecurityAttributeType.Critical, TargetKind.Method, "System.Void T1.M1()"));

			AssertString("SSC-M: System.Void T1.M2()",
				new SecurityAttributeDescriptor(
					SecurityAttributeType.SafeCritical, TargetKind.Method, "System.Void T1.M2()"));

			AssertString("SC-T: T1",
				new SecurityAttributeDescriptor(
					SecurityAttributeType.Critical, TargetKind.Type, "T1"));

			AssertString("SSC-T: T2",
				new SecurityAttributeDescriptor(
					SecurityAttributeType.SafeCritical, TargetKind.Type, "T2"));

		}

		[Test]
		public void ToStringOverrideFormat()
		{
			AssertString("+SC-M: System.Void T1.M1()",
				new SecurityAttributeDescriptor(
					SecurityAttributeOverride.Add, SecurityAttributeType.Critical, TargetKind.Method, "System.Void T1.M1()"));

			AssertString("+SSC-M: System.Void T1.M2()",
				new SecurityAttributeDescriptor(
					SecurityAttributeOverride.Add, SecurityAttributeType.SafeCritical, TargetKind.Method, "System.Void T1.M2()"));

			AssertString("+SC-T: T1",
				new SecurityAttributeDescriptor(
					SecurityAttributeOverride.Add, SecurityAttributeType.Critical, TargetKind.Type, "T1"));

			AssertString("+SSC-T: T2",
				new SecurityAttributeDescriptor(
					SecurityAttributeOverride.Add, SecurityAttributeType.SafeCritical, TargetKind.Type, "T2"));
		}

		private void AssertString(string expected, SecurityAttributeDescriptor actual)
		{
			Assert.AreEqual(expected, actual.ToString());
		}
	}
}
