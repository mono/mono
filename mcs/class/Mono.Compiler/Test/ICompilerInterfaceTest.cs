using System;
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
			// TODO: compiler == ??
		}

		
		public int AddMethod (int a, int b) {
			return a + b;
		}

		[Test]
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
	}
}
