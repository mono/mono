//
// Microsoft.CSharp.CSharpCodeProvider.cs
//
// Author:
// Gert Driesen (drieseng@users.sourceforge.net)
//

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;

using Microsoft.CSharp;

using NUnit.Framework;

namespace MonoTests.Microsoft.CSharp
{
	[TestFixture]
	public class CSharpCodeProviderTest
	{
		// NOTE: This test does not clean-up the generated assemblies
		[Test]
		public void CompileAssembly_InMemory ()
		{
			// NOT in memory
			CompilerResults results = CompileAssembly (false);
			Assert.IsTrue (results.CompiledAssembly.Location.Length	!= 0, "#1");
			Assert.IsNotNull (results.PathToAssembly, "#2");

			// in memory
			results = CompileAssembly (true);
			Assert.AreEqual (string.Empty, results.CompiledAssembly.Location, "#3");
			Assert.IsNull (results.PathToAssembly, "#4");
		}

		private CompilerResults CompileAssembly (bool inMemory)
		{
			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = false;
			options.GenerateInMemory = inMemory;

			CSharpCodeProvider codeProvider = new CSharpCodeProvider ();
			ICodeCompiler compiler = codeProvider.CreateCompiler ();
			return compiler.CompileAssemblyFromDom (options,
				new CodeCompileUnit ());
		}
	}
}
