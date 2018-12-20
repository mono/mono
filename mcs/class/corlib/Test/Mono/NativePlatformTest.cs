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

		[Test]
		public void TestReflectionInitialize ()
		{
			var asm = typeof (string).Assembly;
			var type = asm.GetType ("Mono.MonoNativePlatform");
			Assert.IsNotNull (type, "MonoNativePlatform");

			var method = type.GetMethod ("Initialize", BindingFlags.Static | BindingFlags.Public);
			Assert.IsNotNull (method, "MonoNativePlatform.Initialize");

			var method2 = type.GetMethod ("IsInitialized", BindingFlags.Static | BindingFlags.Public);
			Assert.IsNotNull (method2, "MonoNativePlatform.IsInitialized");

			method.Invoke (null, null);

			var result = (bool)method2.Invoke (null, null);
			Assert.IsTrue (result, "MonoNativePlatform.IsInitialized()");
		}

		[Test]
		public void TestInternalCounter ()
		{
			MonoNativePlatform.Initialize ();

			var asm = typeof (string).Assembly;
			var type = asm.GetType ("Mono.MonoNativePlatform");
			Assert.IsNotNull (type, "MonoNativePlatform");

			var method = type.GetMethod ("TestInternalCounter", BindingFlags.Static | BindingFlags.NonPublic);
			Assert.IsNotNull (method, "MonoNativePlatform.TestInternalCounter");
			var result = method.Invoke (null, null);

			Assert.That (result, Is.GreaterThan (0), "MonoNativePlatform.TestInternalCounter()");
		}
	}
}
