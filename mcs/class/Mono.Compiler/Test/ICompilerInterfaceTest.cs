using System;
using NUnit.Framework;
using Mono.Compiler;

using SimpleJit.CIL;

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
			//compiler = new ManagedJIT ();
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
			Assert.AreEqual (addition, 3);
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
			Assert.AreEqual (addition, 3);
		}

		[Test]
		public void TestRetrieveBytecodes () {
			ClassInfo ci = runtimeInfo.GetClassInfoFor (typeof (ICompilerTests).AssemblyQualifiedName);
			MethodInfo mi = runtimeInfo.GetMethodInfoFor (ci, "AddMethod");

			Assert.AreEqual (4, mi.Body.Body.Length);

			var it = mi.Body.GetIterator ();

			var move1 = it.MoveNext ();
			Assert.IsTrue (move1);
			Assert.AreEqual (Opcode.Ldarg1, it.Opcode, "instr 1");
			Assert.IsTrue (it.HasNext);

			var move2 = it.MoveNext ();
			Assert.IsTrue (move2);
			Assert.AreEqual (Opcode.Ldarg2, it.Opcode, "instr 2");
			Assert.IsTrue (it.HasNext);

			var move3 = it.MoveNext ();
			Assert.IsTrue (move3);
			Assert.AreEqual (Opcode.Add, it.Opcode, "instr 3");
			Assert.IsTrue (it.HasNext);

			var move4 = it.MoveNext ();
			Assert.IsTrue (move4);
			Assert.AreEqual (Opcode.Ret, it.Opcode, "instr 4");
			Assert.IsFalse (it.HasNext);

			var move5 = it.MoveNext ();
			Assert.IsFalse (move5);
		}

		[Test]
		public unsafe void TestSimpleRet () {
			byte[] input = { 0x2a /* OpCodes.Ret*/ };
			var body = new SimpleJit.Metadata.MethodBody (input);
			MethodInfo mi = new MethodInfo (null, "simpleRet", body);
			NativeCodeHandle nativeCode;

			compiler.CompileMethod (runtimeInfo, mi, CompilationFlags.None, out nativeCode);

			Assert.AreEqual (*nativeCode.Blob, (byte) 0xc3); // 0xc3 is 'RET' in amd64 assembly
		}
	}
}
