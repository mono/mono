using System;
using System.Reflection.Emit;
using NUnit.Framework;
using Mono.Compiler;

namespace MonoTests.Mono.CompilerInterface
{
	[TestFixture]
	public class ICompilerTests
	{
		IRuntimeInformation runtimeInfo = null;
		ICompiler compiler = null;

		[TestFixtureSetUp]
		public void Init () {
			runtimeInfo = new RuntimeInformation ();
			compiler = new ManagedJIT ();
		}

		
		public int AddMethod (int a, int b) {
			return a + b;
		}

		[Test]
		[Ignore("Not ready yet")]
		public void TestAddMethod () {
			MethodInfo methodInfo = null; // TODO: get EmptyMethod somehow?
			NativeCodeHandle nativeCode;

			CompilationResult result = compiler.CompileMethod (runtimeInfo, methodInfo, CompilationFlags.None, out nativeCode);
			InstalledRuntimeCode irc = runtimeInfo.InstallCompilationResult (result, nativeCode);
			
			int addition = (int) runtimeInfo.ExecuteInstalledMethod (irc, 1, 2);
			Assert.Equals (addition, 3);
		}

		[Test]
		public unsafe void TestInstallCompilationResultAndExecute () {
			CompilationResult result = CompilationResult.Ok;
			byte[] amd64addblob = { 0x48, 0x8d, 0x04, 0x37, /* lea rax, [rdi + rsi * 1] */
									0xc3};                  /* ret */

			NativeCodeHandle nativeCode = null;
			fixed (byte *b = amd64addblob) {
				nativeCode = new NativeCodeHandle (b, amd64addblob.Length);
			}

			InstalledRuntimeCode irc = runtimeInfo.InstallCompilationResult (result, nativeCode);

			int addition = (int) runtimeInfo.ExecuteInstalledMethod (irc, 1, 2);
			Assert.Equals (addition, 3);

		}

		[Test]
		public void TestRetrieveBytecodes () {
			ClassInfo ci = runtimeInfo.GetClassInfoFor ("ICompilerTests");
			MethodInfo mi = runtimeInfo.GetMethodInfoFor (ci, "AddMethod");

			Assert.True (mi.Stream.Length > 2); // TODO: better verification.
		}

		[Test]
		public unsafe void TestSimpleRet () {
			OpCode[] input = { OpCodes.Ret };
			MethodInfo mi = new MethodInfo (null, "simpleRet", input);
			NativeCodeHandle nativeCode;

			compiler.CompileMethod (runtimeInfo, mi, CompilationFlags.None, out nativeCode);

			Assert.True (*nativeCode.Blob == 0xc3); // 0xc3 is 'RET' in amd64 assembly
		}
	}
}
