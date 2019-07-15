using System;
using System.Reflection;
using NUnit.Framework;
using Mono;

namespace MonoTests.Mono
{
	[TestFixture]
	public class NativePlatformTest
	{
#if WIN_PLATFORM
		[TestFixtureSetUp]
		public void SetUp ()
		{
			Assert.Ignore ("Mono.Native is not supported on this platform.");
		}
#endif

		[Test]
		public void PlatformType ()
		{
			var type = MonoNativePlatform.GetPlatformType ();
			Assert.That ((int)type, Is.GreaterThan (0), "platform type");
		}

		[Test]
		public void TestInitialize ()
		{
			MonoNativePlatform.Initialize ();
			var initialized = MonoNativePlatform.IsInitialized ();
			Assert.IsTrue (initialized, "MonoNativePlatform.IsInitialized()");
		}
	}
}
