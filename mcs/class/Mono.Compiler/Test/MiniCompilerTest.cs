using System;
using NUnit.Framework;
using Mono.Compiler;

namespace MonoTests.Mono.Compiler
{
	[TestFixture]
	public class MiniCompilerTests
	{
		IRuntimeInformation runtimeInfo = null;
		ICompiler compiler = null;

		[TestFixtureSetUp]
		public void Init () {
			runtimeInfo = new RuntimeInformation ();
			compiler = new MiniCompiler ();
		}

		public static void EmptyMethod () {
			return;
		}

		public static int AddMethod (int a, int b) {
			return a + b;
		}

		public static int AddMethod3 (int a, int b, int c) {
			return a + b + c;
		}

		[Test]
		public unsafe void TestEmptyMethod ()
		{
			MethodInfo methodInfo = new MethodInfo(this.GetType().GetMethod("EmptyMethod").MethodHandle);

			CompilationResult result = compiler.CompileMethod (runtimeInfo, methodInfo, CompilationFlags.None, out NativeCodeHandle nativeCode);
			Assert.AreEqual(result, CompilationResult.Ok);
			Assert.AreNotEqual((IntPtr)nativeCode.Blob, IntPtr.Zero);
			// AssertHelper.Greater(nativeCode.Length, 0);

			InstalledRuntimeCode irc = runtimeInfo.InstallCompilationResult (result, methodInfo, nativeCode);
			runtimeInfo.ExecuteInstalledMethod (irc);
		}

		[Test]
		public unsafe void TestAddMethod ()
		{
			MethodInfo methodInfo = new MethodInfo(this.GetType().GetMethod("AddMethod").MethodHandle);

			CompilationResult result = compiler.CompileMethod (runtimeInfo, methodInfo, CompilationFlags.None, out NativeCodeHandle nativeCode);
			Assert.AreEqual(result, CompilationResult.Ok);
			Assert.AreNotEqual((IntPtr)nativeCode.Blob, IntPtr.Zero);
			// AssertHelper.Greater(nativeCode.Length, 0);

			InstalledRuntimeCode irc = runtimeInfo.InstallCompilationResult (result, methodInfo, nativeCode);
			int addition = (int) runtimeInfo.ExecuteInstalledMethod (irc, 1, 2);
			Assert.AreEqual (addition, 3);
		}

		[Test]
		public unsafe void TestAddMethod3 ()
		{
			MethodInfo methodInfo = new MethodInfo(this.GetType().GetMethod("AddMethod3").MethodHandle);

			CompilationResult result = compiler.CompileMethod (runtimeInfo, methodInfo, CompilationFlags.None, out NativeCodeHandle nativeCode);
			Assert.AreEqual(result, CompilationResult.Ok);
			Assert.AreNotEqual((IntPtr)nativeCode.Blob, IntPtr.Zero);
			// AssertHelper.Greater(nativeCode.Length, 0);

			InstalledRuntimeCode irc = runtimeInfo.InstallCompilationResult (result, methodInfo, nativeCode);
			int addition = (int) runtimeInfo.ExecuteInstalledMethod (irc, 1, 2, 3);
			Assert.AreEqual (addition, 6);
		}
	}
}
