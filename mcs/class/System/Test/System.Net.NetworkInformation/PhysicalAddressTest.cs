using NUnit.Framework;
using System;
#if NET_2_0
using System.Net.NetworkInformation;

namespace MonoTests.System.Net.NetworkInformation
{
	[TestFixture]
	public class PhysicalAddressTest
	{
		[Test] 
		public void CreateNormal()
		{
			// Normal case, creation of physical address
			PhysicalAddress phys = new PhysicalAddress(new byte [] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 });
		}

		[Test]
		public void CreateWithLarger()
		{
			// MS.NET 2.0 Allows Physical Address to be created if array larger than normal
			PhysicalAddress phys = new PhysicalAddress(new byte [] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 });
		}

		[Test]
		public void CreateWithSmaller()
		{
			// MS.NET 2.0 Allows Physical Address to be created if array smaller than normal
			PhysicalAddress phys = new PhysicalAddress(new byte [] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 });
		}

		[Test]
		public void ParseNull()
		{
			// MS.NET 2.0 returns empty PhysicalAddress when passed null
			PhysicalAddress phys = PhysicalAddress.Parse(null);
			Assert.AreEqual(0, phys.GetAddressBytes().Length);
		}

		[Test]
		[ExpectedException(typeof(FormatException))]
		public void ParseEmpty()
		{
			// MS.NET 2.0 Fails parse when empty
			PhysicalAddress phys = PhysicalAddress.Parse("");
		}

		[Test]
		public void ParseWithoutDash()
		{
			// MS.NET 2.0 Parses without the dash separator
			PhysicalAddress phys = PhysicalAddress.Parse("010203040506");
		}

		[Test]
		[ExpectedException(typeof(FormatException))]
		public void ParseWithoutDashToFewChars()
		{
			// MS.NET 2.0 Fails parse when too few characters
			PhysicalAddress phys = PhysicalAddress.Parse("01020304050");
		}

		[Test]
		[ExpectedException(typeof(FormatException))]
		public void ParseWithoutDashToManyChars()
		{
			// MS.NET 2.0 Fails parse when too many characters
			PhysicalAddress phys = PhysicalAddress.Parse("0102030405060");
		}

		[Test]
		public void ParseWithDash()
		{
			PhysicalAddress phys = PhysicalAddress.Parse("01-02-03-04-05-06");
		}

		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void ParseWithDashToFewChars()
		{
			PhysicalAddress phys = PhysicalAddress.Parse("01-02-03-04-05-0");
		}

		[Test]
		[ExpectedException(typeof(FormatException))]
		public void ParseWithDashToManyChars()
		{
			PhysicalAddress phys = PhysicalAddress.Parse("01-02-03-04-05-060");
		}

		[Test]
		public void GetHashCodeEqual()
		{
			PhysicalAddress phys1 = new PhysicalAddress(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 });
			PhysicalAddress phys2 = new PhysicalAddress(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 });

			Assert.AreEqual(phys1.GetHashCode(), phys2.GetHashCode());
		}

		[Test]
		public void GetHashCodeNotEqual()
		{
			PhysicalAddress phys1 = new PhysicalAddress(new byte[] { 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 });
			PhysicalAddress phys2 = new PhysicalAddress(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 });

			Assert.IsFalse(phys1.GetHashCode().Equals(phys2.GetHashCode()));
		}

		[Test]
		public void ToStringTest()
		{
			PhysicalAddress phys1 = new PhysicalAddress(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 });
			Assert.AreEqual("010203040506", phys1.ToString());
		}

		[Test]
		public void EqualsNormal()
		{
			PhysicalAddress phys1 = new PhysicalAddress(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 });
			PhysicalAddress phys2 = new PhysicalAddress(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 });

			Assert.IsTrue(phys1.Equals(phys2));
		}

		[Test]
		public void EqualsNot()
		{
			PhysicalAddress phys1 = new PhysicalAddress(new byte[] { 0x06, 0x5, 0x04, 0x03, 0x02, 0x01 });
			PhysicalAddress phys2 = new PhysicalAddress(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 });

			Assert.IsTrue(!phys1.Equals(phys2));
		}
	}
}

#endif
