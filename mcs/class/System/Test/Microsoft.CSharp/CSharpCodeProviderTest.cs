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
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.CSharp;
using NUnit.Framework;
using System.Text;
using System.Linq;
using MonoTests.Helpers;

namespace MonoTests.Microsoft.CSharp
{
	[TestFixture]
	public class CSharpCodeProviderTest
	{
		private TempDirectory _tempDirectory;
		private string _tempDir;
		private CodeDomProvider _codeProvider;

		private static readonly string _sourceLibrary1 = "public class Test1 {}";
		private static readonly string _sourceLibrary2 = "public class Test2 {}";
		private static readonly string _sourceLibrary3 =
			@"public class Test3 { public void F() { } }
			public class Test4 : Test3 { public void F() { } }";
		private static readonly string _sourceExecutable = "public class Program { static void Main () { } }";

		[SetUp]
		public void SetUp ()
		{
			_codeProvider = new CSharpCodeProvider ();
			_tempDirectory = new TempDirectory ();
			_tempDir = _tempDirectory.Path;
		}

		[TearDown]
		public void TearDown ()
		{
			_tempDirectory.Dispose ();
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
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.DeclareIndexerProperties), "#23");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.GenericTypeDeclaration), "#24");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.GenericTypeReference), "#25");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.PartialTypes), "#26");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.Resources), "#27");
		}

		[Test]
		public void CompileFromFile_InMemory ()
		{
			// create source file
			string sourceFile = Path.Combine (_tempDir, "file." + _codeProvider.FileExtension);
			using (FileStream f = new FileStream (sourceFile, FileMode.Create)) {
				using (StreamWriter s = new StreamWriter (f)) {
					s.Write (_sourceLibrary1);
					s.Close ();
				}
				f.Close ();
			}

			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;
			options.TempFiles = new TempFileCollection (_tempDir);
			options.EmbeddedResources.Add (sourceFile);

			ICodeCompiler compiler = _codeProvider.CreateCompiler ();
			CompilerResults results = compiler.CompileAssemblyFromFile (options,
				sourceFile);

			// verify compilation was successful
			AssertCompileResults (results, true);

			Assembly compiledAssembly = results.CompiledAssembly;

			Assert.IsNotNull (compiledAssembly, "#1");
			Assert.AreEqual (string.Empty, compiledAssembly.Location, "#2");
			Assert.IsNull (results.PathToAssembly, "#3");
			Assert.IsNotNull (compiledAssembly.GetType ("Test1"), "#4");
			
			// verify we don't cleanup files in temp directory too agressively
			string[] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (1, tempFiles.Length, "#5");
			Assert.AreEqual (sourceFile, tempFiles[0], "#6");
			
			string[] resources = compiledAssembly.GetManifestResourceNames();
			Assert.IsNotNull (resources, "#7");
			Assert.AreEqual (1, resources.Length, "#8");
			Assert.AreEqual ("file.cs", resources[0], "#9");
			Assert.IsNull (compiledAssembly.GetFile ("file.cs"), "#10");
			Assert.IsNotNull (compiledAssembly.GetManifestResourceStream  ("file.cs"), "#11");
			ManifestResourceInfo info = compiledAssembly.GetManifestResourceInfo ("file.cs");
			Assert.IsNotNull (info, "#12");
			Assert.IsNull (info.FileName, "#13");
			Assert.IsNull (info.ReferencedAssembly, "#14");
			Assert.AreEqual ((ResourceLocation.Embedded | ResourceLocation.ContainedInManifestFile), info.ResourceLocation, "#15");
		}

		[Test]
		public void CompileFromFileBatch_Executable_InMemory ()
		{
			// create source file
			string sourceFile1 = Path.Combine (_tempDir, "file1." + _codeProvider.FileExtension);
			using (FileStream f = new FileStream (sourceFile1, FileMode.Create)) {
				using (StreamWriter s = new StreamWriter (f)) {
					s.Write (_sourceLibrary1);
					s.Close ();
				}
				f.Close ();
			}

			string sourceFile2 = Path.Combine (_tempDir, "file2." + _codeProvider.FileExtension);
			using (FileStream f = new FileStream (sourceFile2, FileMode.Create)) {
				using (StreamWriter s = new StreamWriter (f)) {
					s.Write (_sourceExecutable);
					s.Close ();
				}
				f.Close ();
			}

			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = true;
			options.GenerateInMemory = true;
			options.OutputAssembly = string.Empty;
			options.TempFiles = new TempFileCollection (_tempDir);
			options.EmbeddedResources.Add (sourceFile1);
			options.LinkedResources.Add (sourceFile2);

			ICodeCompiler compiler = _codeProvider.CreateCompiler ();
			CompilerResults results = compiler.CompileAssemblyFromFileBatch (options,
				new string [] { sourceFile1, sourceFile2 });

			// verify compilation was successful
			AssertCompileResults (results, true);

			Assembly compiledAssembly = results.CompiledAssembly;

			Assert.IsNotNull (compiledAssembly, "#A1");
			Assert.AreEqual (string.Empty, compiledAssembly.Location, "#A2");
			Assert.IsNull (results.PathToAssembly, "#A3");
			Assert.IsNotNull (options.OutputAssembly, "#A4");
			Assert.AreEqual (".exe", Path.GetExtension (options.OutputAssembly), "#A5");
			Assert.AreEqual (_tempDir, Path.GetDirectoryName (options.OutputAssembly), "#A6");
			Assert.IsFalse (File.Exists (options.OutputAssembly), "#A7");

			Assert.IsNotNull (compiledAssembly.GetType ("Test1"), "#B1");
			Assert.IsNotNull (compiledAssembly.GetType ("Program"), "#B2");

			// verify we don't cleanup files in temp directory too agressively
			string [] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (2, tempFiles.Length, "#C1");
			Assert.IsTrue (File.Exists (sourceFile1), "#C2");
			Assert.IsTrue (File.Exists (sourceFile2), "#C3");

			string[] resources = compiledAssembly.GetManifestResourceNames();
			Assert.IsNotNull (resources, "#D1");
			Assert.AreEqual (2, resources.Length, "#D2");

			Assert.IsTrue (resources[0] == "file1.cs" || resources [0] == "file2.cs", "#E1");
			Assert.IsNull (compiledAssembly.GetFile ("file1.cs"), "#E2");
			Assert.IsNotNull (compiledAssembly.GetManifestResourceStream  ("file1.cs"), "#E3");
			ManifestResourceInfo info = compiledAssembly.GetManifestResourceInfo ("file1.cs");
			Assert.IsNotNull (info, "#E4");
			Assert.IsNull (info.FileName, "#E5");
			Assert.IsNull (info.ReferencedAssembly, "#E6");
			Assert.AreEqual ((ResourceLocation.Embedded | ResourceLocation.ContainedInManifestFile), info.ResourceLocation, "#E7");

			Assert.IsTrue (resources[1] == "file1.cs" || resources [1] == "file2.cs", "#F1");
			try {
				compiledAssembly.GetFile ("file2.cs");
				Assert.Fail ("#F2");
			} catch (FileNotFoundException) {
			}
			try {
				compiledAssembly.GetManifestResourceStream  ("file2.cs");
				Assert.Fail ("#F3");
			} catch (FileNotFoundException) {
			}
			info = compiledAssembly.GetManifestResourceInfo ("file2.cs");
			Assert.IsNotNull (info, "#F4");
			Assert.IsNotNull (info.FileName, "#F5");
			Assert.AreEqual ("file2.cs", info.FileName, "#F6");
			Assert.IsNull (info.ReferencedAssembly, "#F7");
			Assert.AreEqual ((ResourceLocation) 0, info.ResourceLocation, "#F8");
		}

		[Test]
		public void CompileFromFileBatch_Library_InMemory ()
		{
			// create source file
			string sourceFile1 = Path.Combine (_tempDir, "file1." + _codeProvider.FileExtension);
			using (FileStream f = new FileStream (sourceFile1, FileMode.Create)) {
				using (StreamWriter s = new StreamWriter (f)) {
					s.Write (_sourceLibrary1);
					s.Close ();
				}
				f.Close ();
			}

			string sourceFile2 = Path.Combine (_tempDir, "file2." + _codeProvider.FileExtension);
			using (FileStream f = new FileStream (sourceFile2, FileMode.Create)) {
				using (StreamWriter s = new StreamWriter (f)) {
					s.Write (_sourceLibrary2);
					s.Close ();
				}
				f.Close ();
			}

			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;
			options.TempFiles = new TempFileCollection (_tempDir);
			options.EmbeddedResources.Add (sourceFile1);
			options.LinkedResources.Add (sourceFile2);

			ICodeCompiler compiler = _codeProvider.CreateCompiler ();
			CompilerResults results = compiler.CompileAssemblyFromFileBatch (options,
				new string [] { sourceFile1, sourceFile2 });

			// verify compilation was successful
			AssertCompileResults (results, true);

			Assembly compiledAssembly = results.CompiledAssembly;

			Assert.IsNotNull (compiledAssembly, "#A1");
			Assert.AreEqual (string.Empty, compiledAssembly.Location, "#A2");
			Assert.IsNull (results.PathToAssembly, "#A3");
			Assert.IsNotNull (options.OutputAssembly, "#A4");
			Assert.AreEqual (".dll", Path.GetExtension (options.OutputAssembly), "#A5");
			Assert.AreEqual (_tempDir, Path.GetDirectoryName (options.OutputAssembly), "#A6");
			Assert.IsFalse (File.Exists (options.OutputAssembly), "#A7");

			Assert.IsNotNull (compiledAssembly.GetType ("Test1"), "#B1");
			Assert.IsNotNull (compiledAssembly.GetType ("Test2"), "#B2");

			// verify we don't cleanup files in temp directory too agressively
			string [] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (2, tempFiles.Length, "#C1");
			Assert.IsTrue (File.Exists (sourceFile1), "#C2");
			Assert.IsTrue (File.Exists (sourceFile2), "#C3");

			string[] resources = compiledAssembly.GetManifestResourceNames();
			Assert.IsNotNull (resources, "#D1");
			Assert.AreEqual (2, resources.Length, "#D2");

			Assert.IsTrue (resources[0] == "file1.cs" || resources [0] == "file2.cs", "#E1");
			Assert.IsNull (compiledAssembly.GetFile ("file1.cs"), "#E2");
			Assert.IsNotNull (compiledAssembly.GetManifestResourceStream  ("file1.cs"), "#E3");
			ManifestResourceInfo info = compiledAssembly.GetManifestResourceInfo ("file1.cs");
			Assert.IsNotNull (info, "#E4");
			Assert.IsNull (info.FileName, "#E5");
			Assert.IsNull (info.ReferencedAssembly, "#E6");
			Assert.AreEqual ((ResourceLocation.Embedded | ResourceLocation.ContainedInManifestFile), info.ResourceLocation, "#E7");

			Assert.IsTrue (resources[1] == "file1.cs" || resources [1] == "file2.cs", "#F1");
			try {
				compiledAssembly.GetFile ("file2.cs");
				Assert.Fail ("#F2");
			} catch (FileNotFoundException) {
			}
			try {
				compiledAssembly.GetManifestResourceStream  ("file2.cs");
				Assert.Fail ("#F3");
			} catch (FileNotFoundException) {
			}
			info = compiledAssembly.GetManifestResourceInfo ("file2.cs");
			Assert.IsNotNull (info, "#F4");
			Assert.IsNotNull (info.FileName, "#F5");
			Assert.AreEqual ("file2.cs", info.FileName, "#F6");
			Assert.IsNull (info.ReferencedAssembly, "#F7");
			Assert.AreEqual ((ResourceLocation) 0, info.ResourceLocation, "#F8");
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
				_sourceLibrary1);

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
		public void CompileFromSource_InMemory_Twice ()
		{
			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;

			ICodeCompiler compiler = _codeProvider.CreateCompiler ();

			var src_1 = "class X { ";

			CompilerResults results_1 = compiler.CompileAssemblyFromSource (options, src_1);
			var output_1 = options.OutputAssembly;

			var src_2 = "class X { }";

			CompilerResults results_2 = compiler.CompileAssemblyFromSource (options, src_2);
			var output_2 = options.OutputAssembly;

			// verify compilation was successful
			AssertCompileResults (results_2, true);

			Assert.AreEqual (output_1, output_2, "#1");
		}


		[Test]
		public void CompileFromSource_InMemory_With_Extra_Delete ()
		{
			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;

			ICodeCompiler compiler = _codeProvider.CreateCompiler ();

			var src_1 = "class X { ";

			compiler.CompileAssemblyFromSource (options, src_1);

			options.TempFiles.Delete ();
			options.TempFiles.Delete ();
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

			string outputAssembly = Path.Combine (_tempDir, "sourcebatch.dll");

			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;
			options.OutputAssembly = outputAssembly;
			options.TempFiles = new TempFileCollection (_tempDir);

			ICodeCompiler compiler = _codeProvider.CreateCompiler ();
			CompilerResults results = compiler.CompileAssemblyFromSourceBatch (options,
				new string [] { _sourceLibrary1, _sourceLibrary2 });

			// verify compilation was successful
			AssertCompileResults (results, true);

			Assert.AreEqual (string.Empty, results.CompiledAssembly.Location, "#A1");
			Assert.IsNull (results.PathToAssembly, "#A2");
			Assert.IsNotNull (options.OutputAssembly, "#A3");
			Assert.AreEqual (outputAssembly, options.OutputAssembly, "#A4");
			Assert.IsTrue (File.Exists (outputAssembly), "#A5");

			Assert.IsNotNull (results.CompiledAssembly.GetType ("Test1"), "#B1");
			Assert.IsNotNull (results.CompiledAssembly.GetType ("Test2"), "#B2");

			// verify we don't cleanup files in temp directory too agressively
			string[] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (2, tempFiles.Length, "#C1");
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

		[Test]
		public void MultiLineWarningIsReportedAsOneWarning()
		{
			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;
			options.TempFiles = new TempFileCollection (_tempDir);

			ICodeCompiler compiler = _codeProvider.CreateCompiler ();
			CompilerResults results = compiler.CompileAssemblyFromSource (options,
				_sourceLibrary3);

			// verify compilation was successful
			AssertCompileResults (results, true);
		}

		[Test]
		public void EncodingMismatch ()
		{
			var source = @"
				#warning Trigger Some Warning
				public class MyClass {
					public static string MyMethod () { return ""data""; }
				}";

			var p = new CompilerParameters () {
				GenerateInMemory = false,
				GenerateExecutable = false,
				IncludeDebugInformation = true,
				TreatWarningsAsErrors = false,
				TempFiles = new TempFileCollection (_tempDir, true),
			};

			var prov = new CSharpCodeProvider ();
			CompilerResults results;

			var prev = Console.OutputEncoding;
			try {
				Console.OutputEncoding = Encoding.Unicode;

				results = prov.CompileAssemblyFromSource (p, source);
			} finally {
				Console.OutputEncoding = prev;
			}

			Assert.IsNotNull (results.Errors);
			Assert.IsTrue (results.Output.Cast<string>().ToArray ()[1].Contains ("Trigger Some Warning"));
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

		// do not use the Assert class as this will introduce failures if the
		// nunit.framework assembly is not in the GAC
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

				if (results.CompiledAssembly.Location.Length == 0)
					throw new Exception ("Location should not be empty string");
				if (results.PathToAssembly == null)
					throw new Exception ("PathToAssembly should not be null");

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

				if (results.CompiledAssembly.Location.Length == 0)
					throw new Exception ("Location should not be empty string");
				if (results.PathToAssembly == null)
					throw new Exception ("PathToAssembly should not be null");

				return results.PathToAssembly;
			}
		}
	}
}
