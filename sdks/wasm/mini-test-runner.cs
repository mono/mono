using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;



[TestFixture]
public class JitTests {
	static string[] args = new string[] { "--verbose", "--exclude", "!WASM", "--exclude", "!INTERPRETER"};

	[Test]
	public static void Basic () {
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

	[Test]
	public static void BuiltinTests () {
		int res = TestDriver.RunTests (typeof (BuiltinTests), args);
		Assert.AreEqual (0, res);
	}

	[Test]
	public static void DevirtualizationTests () {
		int res = TestDriver.RunTests (typeof (DevirtualizationTests), args);
		Assert.AreEqual (0, res);
	}

	[Test]
	public static void MixedTests () {
		int res = TestDriver.RunTests (typeof (MixedTests), args);
		Assert.AreEqual (0, res);
	}

	[Test]
	public static void GcTests () {
		int res = TestDriver.RunTests (typeof (GcTests), args);
		Assert.AreEqual (0, res);
	}
}
