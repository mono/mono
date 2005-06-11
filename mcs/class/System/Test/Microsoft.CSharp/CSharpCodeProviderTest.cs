//
// Microsoft.CSharp.CSharpCodeProviderTest.cs
//
// Author:
// Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2005 Novell
//

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.CSharp;
using NUnit.Framework;

namespace MonoTests.Microsoft.CSharp
{
	[TestFixture]
	public class CSharpCodeProviderTest
	{
		private string _tempDir;
		private CodeDomProvider _codeProvider;

		private static readonly string _sourceTest1 = "public class Test1 {}";
		private static readonly string _sourceTest2 = "public class Test2 {}";

		[SetUp]
		public void SetUp ()
		{
			_codeProvider = new CSharpCodeProvider ();
			_tempDir = CreateTempDirectory ();
		}

		[TearDown]
		public void TearDown ()
		{
			RemoveDirectory (_tempDir);
		}

		[Test]
		public void FileExtension ()
		{
			Assert.AreEqual ("cs", _codeProvider.FileExtension);
		}

		[Test]
		public void LanguageOptionsTest ()
		{
			Assert.AreEqual (LanguageOptions.None, _codeProvider.LanguageOptions);
		}

		[Test]
		public void GeneratorSupports ()
		{
			ICodeGenerator codeGenerator = _codeProvider.CreateGenerator ();
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.DeclareEnums), "#1");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.ArraysOfArrays), "#2");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.AssemblyAttributes), "#3");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.ChainedConstructorArguments), "#4");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.ComplexExpressions), "#5");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.DeclareDelegates), "#6");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.DeclareEnums), "#7");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.DeclareEvents), "#8");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.DeclareInterfaces), "#9");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.DeclareValueTypes), "#10");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.EntryPointMethod), "#11");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.GotoStatements), "#12");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.MultidimensionalArrays), "#13");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.MultipleInterfaceMembers), "#14");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.NestedTypes), "#15");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.ParameterAttributes), "#16");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.PublicStaticMembers), "#17");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.ReferenceParameters), "#18");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.ReturnTypeAttributes), "#19");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.StaticConstructors), "#20");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.TryCatchStatements), "#21");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.Win32Resources), "#22");
#if NET_2_0
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.DeclareIndexerProperties), "#23");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.GenericTypeDeclaration), "#24");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.GenericTypeReference), "#25");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.PartialTypes), "#26");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.Resources), "#27");
#endif
		}

		[Test]
		public void CompileFromFile_InMemory ()
		{
			// create source file
			string sourceFile = Path.Combine (_tempDir, "file." + _codeProvider.FileExtension);
			using (FileStream f = new FileStream (sourceFile, FileMode.Create)) {
				using (StreamWriter s = new StreamWriter (f)) {
					s.Write (_sourceTest1);
					s.Close ();
				}
				f.Close ();
			}

			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;
			options.TempFiles = new TempFileCollection (_tempDir);

			ICodeCompiler compiler = _codeProvider.CreateCompiler ();
			CompilerResults results = compiler.CompileAssemblyFromFile (options,
				sourceFile);

			// verify compilation was successful
			AssertCompileResults (results, true);

			Assert.AreEqual (string.Empty, results.CompiledAssembly.Location, "#1");
			Assert.IsNull (results.PathToAssembly, "#2");
			Assert.IsNotNull (results.CompiledAssembly.GetType ("Test1"), "#3");

			// verify we don't cleanup files in temp directory too agressively
			string[] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (1, tempFiles.Length, "#4");
			Assert.AreEqual (sourceFile, tempFiles[0], "#5");

		}

		[Test]
		public void CompileFromFileBatch_InMemory ()
		{
			// create source file
			string sourceFile1 = Path.Combine (_tempDir, "file." + _codeProvider.FileExtension);
			using (FileStream f = new FileStream (sourceFile1, FileMode.Create)) {
				using (StreamWriter s = new StreamWriter (f)) {
					s.Write (_sourceTest1);
					s.Close ();
				}
				f.Close ();
			}

			string sourceFile2 = Path.Combine (_tempDir, "file2." + _codeProvider.FileExtension);
			using (FileStream f = new FileStream (sourceFile2, FileMode.Create)) {
				using (StreamWriter s = new StreamWriter (f)) {
					s.Write (_sourceTest2);
					s.Close ();
				}
				f.Close ();
			}

			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;
			options.TempFiles = new TempFileCollection (_tempDir);

			ICodeCompiler compiler = _codeProvider.CreateCompiler ();
			CompilerResults results = compiler.CompileAssemblyFromFileBatch (options,
				new string[] { sourceFile1, sourceFile2 });

			// verify compilation was successful
			AssertCompileResults (results, true);

			Assert.AreEqual (string.Empty, results.CompiledAssembly.Location, "#1");
			Assert.IsNull (results.PathToAssembly, "#2");

			Assert.IsNotNull (results.CompiledAssembly.GetType ("Test1"), "#3");
			Assert.IsNotNull (results.CompiledAssembly.GetType ("Test2"), "#4");

			// verify we don't cleanup files in temp directory too agressively
			string[] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (2, tempFiles.Length, "#5");
			Assert.IsTrue (File.Exists (sourceFile1), "#6");
			Assert.IsTrue (File.Exists (sourceFile2), "#7");
		}

		[Test]
		public void CompileFromSource_InMemory ()
		{
			// create a file in temp directory to ensure that compiler is not removing
			// too much (temporary) files
			string tempFile = Path.Combine (_tempDir, "file." + _codeProvider.FileExtension);
			using (FileStream fs = File.Create (tempFile)) {
				fs.Close ();
			}

			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;
			options.TempFiles = new TempFileCollection (_tempDir);

			ICodeCompiler compiler = _codeProvider.CreateCompiler ();
			CompilerResults results = compiler.CompileAssemblyFromSource (options,
				_sourceTest1);

			// verify compilation was successful
			AssertCompileResults (results, true);

			Assert.AreEqual (string.Empty, results.CompiledAssembly.Location, "#1");
			Assert.IsNull (results.PathToAssembly, "#2");
			Assert.IsNotNull (results.CompiledAssembly.GetType ("Test1"), "#3");

			// verify we don't cleanup files in temp directory too agressively
			string[] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (1, tempFiles.Length, "#4");
			Assert.AreEqual (tempFile, tempFiles[0], "#5");
		}

		[Test]
		public void CompileFromSourceBatch_InMemory ()
		{
			// create a file in temp directory to ensure that compiler is not removing
			// too much (temporary) files
			string tempFile = Path.Combine (_tempDir, "file." + _codeProvider.FileExtension);
			using (FileStream fs = File.Create (tempFile)) {
				fs.Close ();
			}

			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;
			options.TempFiles = new TempFileCollection (_tempDir);

			ICodeCompiler compiler = _codeProvider.CreateCompiler ();
			CompilerResults results = compiler.CompileAssemblyFromSourceBatch (options,
				new string[] { _sourceTest1, _sourceTest2 });

			// verify compilation was successful
			AssertCompileResults (results, true);

			Assert.AreEqual (string.Empty, results.CompiledAssembly.Location, "#1");
			Assert.IsNull (results.PathToAssembly, "#2");

			Assert.IsNotNull (results.CompiledAssembly.GetType ("Test1"), "#3");
			Assert.IsNotNull (results.CompiledAssembly.GetType ("Test2"), "#4");

			// verify we don't cleanup files in temp directory too agressively
			string[] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (1, tempFiles.Length, "#5");
			Assert.AreEqual (tempFile, tempFiles[0], "#6");
		}

		[Test]
		public void CompileFromDom_NotInMemory ()
		{
			// create a file in temp directory to ensure that compiler is not removing
			// too much (temporary) files
			string tempFile = Path.Combine (_tempDir, "file." + _codeProvider.FileExtension);
			using (FileStream fs = File.Create (tempFile)) {
				fs.Close ();
			}

			// compile and verify result in separate appdomain to avoid file locks
			AppDomain testDomain = CreateTestDomain ();
			CrossDomainTester compileTester = CreateCrossDomainTester (testDomain);

			string outputAssembly = null;

			try {
				outputAssembly = compileTester.CompileAssemblyFromDom (_tempDir);
			} finally {
				AppDomain.Unload (testDomain);
			}

			// there should be two files in temp dir: temp file and output assembly
			string[] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (2, tempFiles.Length, "#1");
			Assert.IsTrue (File.Exists (outputAssembly), "#2");
			Assert.IsTrue (File.Exists (tempFile), "#3");
		}

		[Test]
		public void CompileFromDomBatch_NotInMemory ()
		{
			// create a file in temp directory to ensure that compiler is not removing
			// too much (temporary) files
			string tempFile = Path.Combine (_tempDir, "file." + _codeProvider.FileExtension);
			using (FileStream fs = File.Create (tempFile)) {
				fs.Close ();
			}

			// compile and verify result in separate appdomain to avoid file locks
			AppDomain testDomain = CreateTestDomain ();
			CrossDomainTester compileTester = CreateCrossDomainTester (testDomain);

			string outputAssembly = null;
			try {
				outputAssembly = compileTester.CompileAssemblyFromDomBatch (_tempDir);
			} finally {
				AppDomain.Unload (testDomain);
			}

			// there should be two files in temp dir: temp file and output assembly
			string[] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (2, tempFiles.Length, "#1");
			Assert.IsTrue (File.Exists (outputAssembly), "#2");
			Assert.IsTrue (File.Exists (tempFile), "#3");
		}

		[Test]
		public void CompileFromDom_InMemory ()
		{
			// create a file in temp directory to ensure that compiler is not removing
			// too much (temporary) files
			string tempFile = Path.Combine (_tempDir, "file." + _codeProvider.FileExtension);
			using (FileStream fs = File.Create (tempFile)) {
				fs.Close ();
			}

			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;
			options.TempFiles = new TempFileCollection (_tempDir);

			ICodeCompiler compiler = _codeProvider.CreateCompiler ();
			CompilerResults results = compiler.CompileAssemblyFromDom (options, new CodeCompileUnit ());

			// verify compilation was successful
			AssertCompileResults (results, true);

			Assert.AreEqual (string.Empty, results.CompiledAssembly.Location, "#1");
			Assert.IsNull (results.PathToAssembly, "#2");

			// verify we don't cleanup files in temp directory too agressively
			string[] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (1, tempFiles.Length, "#3");
			Assert.AreEqual (tempFile, tempFiles[0], "#4");
		}

		[Test]
		public void CompileFromDomBatch_InMemory ()
		{
			// create a file in temp directory to ensure that compiler is not removing
			// too much (temporary) files
			string tempFile = Path.Combine (_tempDir, "file." + _codeProvider.FileExtension);
			using (FileStream fs = File.Create (tempFile)) {
				fs.Close ();
			}

			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;
			options.TempFiles = new TempFileCollection (_tempDir);

			ICodeCompiler compiler = _codeProvider.CreateCompiler ();
			CompilerResults results = compiler.CompileAssemblyFromDomBatch (options,
				new CodeCompileUnit[] { new CodeCompileUnit (), new CodeCompileUnit () });

			// verify compilation was successful
			AssertCompileResults (results, true);

			Assert.AreEqual (string.Empty, results.CompiledAssembly.Location, "#1");
			Assert.IsNull (results.PathToAssembly, "#2");

			// verify we don't cleanup files in temp directory too agressively
			string[] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (1, tempFiles.Length, "#3");
			Assert.AreEqual (tempFile, tempFiles[0], "#4");
		}

		private static string CreateTempDirectory ()
		{
			// create a uniquely named zero-byte file
			string tempFile = Path.GetTempFileName ();
			// remove the temporary file
			File.Delete (tempFile);
			// create a directory named after the unique temporary file
			Directory.CreateDirectory (tempFile);
			// return the path to the temporary directory
			return tempFile;
		}

		private static void RemoveDirectory (string path)
		{
			try {
				if (Directory.Exists (path)) {
					string[] directoryNames = Directory.GetDirectories (path);
					foreach (string directoryName in directoryNames) {
						RemoveDirectory (directoryName);
					}
					string[] fileNames = Directory.GetFiles (path);
					foreach (string fileName in fileNames) {
						File.Delete (fileName);
					}
					Directory.Delete (path, true);
				}
			} catch (Exception ex) {
				throw new AssertionException ("Unable to cleanup '" + path + "'.", ex);
			}
		}

		private static void AssertCompileResults (CompilerResults results, bool allowWarnings)
		{
			foreach (CompilerError compilerError in results.Errors) {
				if (allowWarnings && compilerError.IsWarning) {
					continue;
				}

				Assert.Fail (compilerError.ToString ());
			}
		}

		private static AppDomain CreateTestDomain ()
		{
			return AppDomain.CreateDomain ("CompileFromDom", AppDomain.CurrentDomain.Evidence,
				AppDomain.CurrentDomain.SetupInformation);
		}

		private static CrossDomainTester CreateCrossDomainTester (AppDomain domain)
		{
			Type testerType = typeof (CrossDomainTester);

			return (CrossDomainTester) domain.CreateInstanceAndUnwrap (
				testerType.Assembly.FullName, testerType.FullName, false,
				BindingFlags.Public | BindingFlags.Instance, null, new object[0],
				CultureInfo.InvariantCulture, new object[0], domain.Evidence);
		}

		private class CrossDomainTester : MarshalByRefObject
		{
			public string CompileAssemblyFromDom (string tempDir)
			{
				CompilerParameters options = new CompilerParameters ();
				options.GenerateExecutable = false;
				options.GenerateInMemory = false;
				options.TempFiles = new TempFileCollection (tempDir);

				CSharpCodeProvider codeProvider = new CSharpCodeProvider ();
				ICodeCompiler compiler = codeProvider.CreateCompiler ();
				CompilerResults results = compiler.CompileAssemblyFromDom (options, new CodeCompileUnit ());

				// verify compilation was successful
				AssertCompileResults (results, true);

				Assert.IsTrue (results.CompiledAssembly.Location.Length != 0,
					"Location should not be empty string");
				Assert.IsNotNull (results.PathToAssembly, "PathToAssembly should not be null");

				return results.PathToAssembly;
			}

			public string CompileAssemblyFromDomBatch (string tempDir)
			{
				CompilerParameters options = new CompilerParameters ();
				options.GenerateExecutable = false;
				options.GenerateInMemory = false;
				options.TempFiles = new TempFileCollection (tempDir);

				CSharpCodeProvider codeProvider = new CSharpCodeProvider ();
				ICodeCompiler compiler = codeProvider.CreateCompiler ();
				CompilerResults results = compiler.CompileAssemblyFromDomBatch (options, new CodeCompileUnit[] { new CodeCompileUnit (), new CodeCompileUnit () });

				// verify compilation was successful
				AssertCompileResults (results, true);

				Assert.IsTrue (results.CompiledAssembly.Location.Length != 0,
					"Location should not be empty string");
				Assert.IsNotNull (results.PathToAssembly, "PathToAssembly should not be null");

				return results.PathToAssembly;
			}
		}
	}
}
