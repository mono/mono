using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Mono;

namespace MonoTests.Mono
{
	[TestFixture]
	public class NativePlatformTest
	{
		[TestFixtureSetUp]
		public void SetUp ()
		{
			if (!MonoNativeConfig.IsSupported)
				Assert.Ignore ("Mono.Native is not supported on this platform.");
		}

		[Test]
		public void Test ()
		{
			var type = MonoNativePlatform.GetPlatformType ();
			Assert.That ((int)type, Is.GreaterThan (0), "platform type");

			var usingCompat = (type & MonoNativePlatformType.MONO_NATIVE_PLATFORM_TYPE_COMPAT) != 0;
			Assert.AreEqual (MonoNativeConfig.UsingCompat, usingCompat, "using compatibility layer");
		}
	}
}
