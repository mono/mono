#if NET_2_0
using System;

using NUnit.Framework;

namespace MonoTests.System
{
[TestFixture]
public class IntegerTryParse {
	[Test]
	public void Int8() {
		sbyte i;
		Assert.IsFalse(sbyte.TryParse("128", out i));
		Assert.IsFalse(sbyte.TryParse("-129", out i));
	}

	[Test]
	public void UInt8() {
		byte i;
		Assert.IsFalse(byte.TryParse("256", out i));
		Assert.IsFalse(byte.TryParse("-1", out i));
	}

	[Test]
	public void Int16() {
		short i;
		Assert.IsFalse(short.TryParse("32768", out i));
		Assert.IsFalse(short.TryParse("-32769", out i));
	}

	[Test]
	public void UInt16() {
		ushort i;
		Assert.IsFalse(ushort.TryParse("65536", out i));
		Assert.IsFalse(ushort.TryParse("-1", out i));
	}

	[Test]
	public void Int32() {
		int i;
		Assert.IsFalse(int.TryParse("2147483648", out i));
		Assert.IsFalse(int.TryParse("-2147483649", out i));
	}

	[Test]
	public void UInt32() {
		uint i;
		Assert.IsFalse(uint.TryParse("4294967296", out i));
		Assert.IsFalse(uint.TryParse("-1", out i));
	}

	[Test]
	public void Int64() {
		long i;
		Assert.IsFalse(long.TryParse("9223372036854775808", out i));
		Assert.IsFalse(long.TryParse("-9223372036854775809", out i));
	}

	[Test]
	public void UInt64() {
		ulong i;
		Assert.IsFalse(ulong.TryParse("18446744073709551616", out i));
		Assert.IsFalse(ulong.TryParse("-1", out i));
	}
}
}
#endif
