using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Mono;

namespace MonoTests.Mono
{
	[TestFixture]
	public class NativePlatformTest
	{
		[DllImport ("System.Native")]
		extern static int mono_native_get_platform_type ();

		[TestFixtureSetUp]
		public void SetUp ()
		{
			if (!MonoNativeConfig.IsSupported)
				Assert.Ignore ("Mono.Native is not supported on this platform.");
		}

		[Test]
		public void Test ()
		{
			var type = mono_native_get_platform_type ();
			Assert.That (type, Is.GreaterThan (0), "platform type");

			var usingCompat = (type & 16384) != 0; // MONO_NATIVE_PLATFORM_TYPE_COMPAT
			Assert.AreEqual (usingCompat, MonoNativeConfig.UsingCompat, "using compatibility layer");
		}
	}
}
