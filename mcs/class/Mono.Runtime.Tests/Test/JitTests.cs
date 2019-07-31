using System;
using NUnit.Framework;

//
// This test suite is used to run the JIT regression tests using nunit
//

namespace MonoTests.Runtime {

[TestFixture]
public class JitTests {
#if MONOTOUCH
	static string[] args = new string[] { "--verbose", "--exclude", "!FULLAOT" };
#elif MONODROID
	static string[] args = new string[] { "--verbose" };
#else
	static string[] args = new string[] { "--verbose", "--exclude", "!FULLAOT" };
#endif

	[Test]
	public void Basic () {
		Console.WriteLine ("AAA");
		int res = TestDriver.RunTests (typeof (BasicTests), args);
		Assert.AreEqual (0, res);
	}

	[Test]
	public static void Arrays () {
		int res = TestDriver.RunTests (typeof (ArrayTests), args);
		Assert.AreEqual (0, res);
	}

	[Test]
	public static void Calls () {
		int res = TestDriver.RunTests (typeof (CallsTests), args);
		Assert.AreEqual (0, res);
	}

	[Test]
	public static void Float () {
		int res = TestDriver.RunTests (typeof (FloatTests), args);
		Assert.AreEqual (0, res);
	}

	[Test]
	public static void Long () {
		int res = TestDriver.RunTests (typeof (LongTests), args);
		Assert.AreEqual (0, res);
	}

	[Test]
	public static void Math () {
		int res = TestDriver.RunTests (typeof (MathTests), args);
		Assert.AreEqual (0, res);
	}

	[Test]
	public static void Objects () {
		int res = TestDriver.RunTests (typeof (ObjectTests.Tests), args);
		Assert.AreEqual (0, res);
	}

	[Test]
	public static void Generics () {
		int res = TestDriver.RunTests (typeof (GenericsTests), args);
		Assert.AreEqual (0, res);
	}

	[Test]
	public static void GShared () {
		int res = TestDriver.RunTests (typeof (GSharedTests), args);
		Assert.AreEqual (0, res);
	}

	[Test]
	public static void Exceptions () {
		int res = TestDriver.RunTests (typeof (ExceptionTests), args);
		Assert.AreEqual (0, res);
	}

	[Test]
	public static void Aot () {
		int res = TestDriver.RunTests (typeof (AotTests), args);
		Assert.AreEqual (0, res);
	}
}

}
