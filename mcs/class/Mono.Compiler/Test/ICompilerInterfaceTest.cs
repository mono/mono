using System;
using System.Reflection.Emit;
using NUnit.Framework;
using Mono.Compiler;

namespace MonoTests.Mono.CompilerInterface
{
	[TestFixture]
	public class ICompilerTests
	{
		CompilerToRuntime c2r = null;
		ICompiler compiler = null;

		[TestFixtureSetUp]
		public void Init () {
			c2r = new CompilerToRuntime ();
			compiler = new ManagedJIT ();
		}

		
		public int AddMethod (int a, int b) {
			return a + b;
		}

		[Test]
		[Ignore("Not ready yet")]
		public void TestAddMethod () {
			ICompilerInformation cinfo = null;
			MethodInfo methodInfo = null; // TODO: get EmptyMethod somehow?
			NativeCodeHandle nativeCode;

			CompilationResult result = compiler.CompileMethod (cinfo, methodInfo, CompilationFlags.None, out nativeCode);
			bool installation = c2r.InstallCompilationResult (result, nativeCode);
			Assert.True (installation);
			
			int addition = (int) c2r.ExecuteInstalledMethod (nativeCode, 1, 2);
			Assert.Equals (addition, 3);
		}

		[Test]
		public unsafe void TestInstallCompilationResultAndExecute () {
			CompilationResult result = CompilationResult.Ok;
			NativeCodeHandle nativeCode = new NativeCodeHandle (null, 0); // TODO: point to memory buffer that contains `add args; ret` in AMD64 assembly

			bool installation = c2r.InstallCompilationResult (result, nativeCode);
			Assert.True (installation);

			int addition = (int) c2r.ExecuteInstalledMethod (nativeCode, 1, 2);
			Assert.Equals (addition, 3);

		}

		[Test]
		public void TestRetrieveBytecodes () {
			ClassInfo ci = c2r.GetClassInfoFor ("ICompilerTests");
			MethodInfo mi = c2r.GetMethodInfoFor (ci, "AddMethod");

			Assert.True (mi.Stream.Length > 2); // TODO: better verification.
		}

		[Test]
		public unsafe void TestSimpleRet () {
			OpCode[] input = { OpCodes.Ret };
			MethodInfo mi = new MethodInfo (null, "simpleRet", input);
			NativeCodeHandle nativeCode;

			compiler.CompileMethod (null, mi, CompilationFlags.None, out nativeCode);

			Assert.True (*nativeCode.Blob == 0xc3); // 0xc3 is 'RET' in amd64 assembly
		}
	}
}
