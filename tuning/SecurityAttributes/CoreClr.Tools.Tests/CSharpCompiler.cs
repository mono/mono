using System;
using System.CodeDom.Compiler;
using System.IO;

namespace CoreClr.Tools.Tests
{
	public static class CSharpCompiler
	{
		public static void CompileAssemblyFromSource(string outputAssembly, string source, params string[] references)
		{
			var parameters = new CompilerParameters {
				GenerateInMemory = false,
                CompilerOptions = "/unsafe",
				OutputAssembly = outputAssembly };
			
			parameters.ReferencedAssemblies.AddRange(references);

			var results = GetCSharpCodeDomProvider().CompileAssemblyFromSource(parameters, source);
			AssertNoErrors(results);
		}

		public static string CompileTempAssembly(string code, params string[] references)
		{
			var assembly = Path.GetTempFileName();
			CompileAssemblyFromSource(assembly, code, references);
			return assembly;
		}

		static void AssertNoErrors(CompilerResults results)
		{
			if (results.Errors.Count == 0)
				return;

			CompilerError firstError = results.Errors[0];
			throw new ArgumentException(String.Format("{2} at ({0},{1})", firstError.Line, firstError.Column, firstError.ErrorText));
		}

		static CodeDomProvider GetCSharpCodeDomProvider()
		{
			return GetCSharpCompilerInfo().CreateProvider();
		}

		static CompilerInfo GetCSharpCompilerInfo()
		{
			return CodeDomProvider.GetCompilerInfo(CodeDomProvider.GetLanguageFromExtension(".cs"));
		}
	}
}

