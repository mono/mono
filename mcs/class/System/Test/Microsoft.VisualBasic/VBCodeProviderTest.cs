//
// Microsoft.VisualBasic.VBCodeProvider.cs
//
// Author:
//   Jochen Wezel (jwezel@compumaster.de)
//
// (C) 2003 Jochen Wezel (CompuMaster GmbH)
//
// Last modifications:
// 2003-12-10 JW: publishing of this file
//

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.VisualBasic;
using NUnit.Framework;

namespace MonoTests.Microsoft.VisualBasic
{
	enum OsType
	{
		Windows,
		Unix,
		Mac
	}

	[TestFixture]
	[Category ("NotWorking")] // we cannot rely on vbnc being available
	public class VBCodeProviderTest
	{
		private string _tempDir;
		private CodeDomProvider _codeProvider;
		private static OsType OS;
		private static char DSC = Path.DirectorySeparatorChar;

		private static readonly string _sourceLibrary1 = "Public Class Test1" +
			Environment.NewLine + "End Class";
		private static readonly string _sourceLibrary2 = "Public Class Test2" +
			Environment.NewLine + "End Class";
		private static readonly string _sourceExecutable = @"
			Public Class Program
			Public Sub Main
			End Sub
			End Class";

		[SetUp]
		public void GetReady ()
		{
			if ('/' == DSC) {
				OS = OsType.Unix;
			} else if ('\\' == DSC) {
				OS = OsType.Windows;
			} else {
				OS = OsType.Mac;
			}

			_codeProvider = new VBCodeProvider ();
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
			Assert.AreEqual ("vb", _codeProvider.FileExtension, "#JW10");
		}

		[Test]
		public void LanguageOptionsTest ()
		{
			Assert.AreEqual (LanguageOptions.CaseInsensitive, _codeProvider.LanguageOptions, "#JW20");
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
		[Category ("NotWorking")] // we cannot rely on vbnc being available
		public void CreateCompiler ()
		{
			// Prepare the compilation
			ICodeCompiler codeCompiler = _codeProvider.CreateCompiler ();
			Assert.IsNotNull (codeCompiler, "#JW30 - CreateCompiler");

			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = true;
			options.IncludeDebugInformation = true;
			options.TreatWarningsAsErrors = true;

			// process compilation
			CompilerResults compilerResults = codeCompiler.CompileAssemblyFromSource (options,
				"public class TestModule" + Environment.NewLine + "public shared sub Main()"
				+ Environment.NewLine + "System.Console.Write(\"Hello world!\")"
				+ Environment.NewLine + "End Sub" + Environment.NewLine + "End Class");

			// Analyse the compilation success/messages
			StringCollection MyOutput = compilerResults.Output;
			string MyOutStr = "";
			foreach (string MyStr in MyOutput) {
				MyOutStr += MyStr + Environment.NewLine + Environment.NewLine;
			}

			if (compilerResults.Errors.Count != 0) {
				Assert.Fail ("#JW31 - Hello world compilation: " + MyOutStr);
			}

			try {
				Assembly MyAss = compilerResults.CompiledAssembly;
			} catch (Exception ex) {
				Assert.Fail ("#JW32 - compilerResults.CompiledAssembly hasn't been an expected object" +
						Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
			}

			// Execute the test app
			ProcessStartInfo NewProcInfo = new ProcessStartInfo ();
			if (Windows) {
				NewProcInfo.FileName = compilerResults.CompiledAssembly.Location;
			} else {
				NewProcInfo.FileName = "mono";
				NewProcInfo.Arguments = compilerResults.CompiledAssembly.Location;
			}
			NewProcInfo.RedirectStandardOutput = true;
			NewProcInfo.UseShellExecute = false;
			NewProcInfo.CreateNoWindow = true;
			string TestAppOutput = "";
			try {
				Process MyProc = Process.Start (NewProcInfo);
				MyProc.WaitForExit ();
				TestAppOutput = MyProc.StandardOutput.ReadToEnd ();
				MyProc.Close ();
				MyProc.Dispose ();
			} catch (Exception ex) {
				Assert.Fail ("#JW34 - " + ex.Message + Environment.NewLine + ex.StackTrace);
			}
			Assert.AreEqual ("Hello world!", TestAppOutput, "#JW33 - Application output");

			// Clean up
			try {
				File.Delete (NewProcInfo.FileName);
			} catch { }
		}

		[Test]
		public void CreateGenerator ()
		{
			ICodeGenerator MyVBCodeGen;
			MyVBCodeGen = _codeProvider.CreateGenerator ();
			Assert.IsNotNull (MyVBCodeGen, "#JW40 - CreateGenerator");
			Assert.IsTrue (MyVBCodeGen.Supports (GeneratorSupport.DeclareEnums), "#JW41");
		}

		[Test]
		[Category ("NotWorking")] // we cannot rely on vbnc being available
		public void CompileFromFile_InMemory ()
		{
			// create vb source file
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
#if NET_2_0
			options.EmbeddedResources.Add (sourceFile);
#endif

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

#if NET_2_0
			string[] resources = compiledAssembly.GetManifestResourceNames();
			Assert.IsNotNull (resources, "#7");
			Assert.AreEqual (1, resources.Length, "#8");
			Assert.AreEqual ("file.vb", resources[0], "#9");
			Assert.IsNull (compiledAssembly.GetFile ("file.vb"), "#10");
			Assert.IsNotNull (compiledAssembly.GetManifestResourceStream  ("file.vb"), "#11");
#endif
		}

		[Test]
		[Category ("NotWorking")] // we cannot rely on vbnc being available
		public void CompileFromFileBatch_Executable_InMemory ()
		{
			// create vb source file
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
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;
			options.OutputAssembly = string.Empty;
			options.TempFiles = new TempFileCollection (_tempDir);
#if NET_2_0
			options.EmbeddedResources.Add (sourceFile1);
			options.LinkedResources.Add (sourceFile2);
#endif

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
			Assert.IsNotNull (compiledAssembly.GetType ("Program"), "#B2");

			// verify we don't cleanup files in temp directory too agressively
			string [] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (2, tempFiles.Length, "#C1");
			Assert.IsTrue (File.Exists (sourceFile1), "#C2");
			Assert.IsTrue (File.Exists (sourceFile2), "#C3");

#if NET_2_0
			string[] resources = compiledAssembly.GetManifestResourceNames();
			Assert.IsNotNull (resources, "#D1");
			Assert.AreEqual (2, resources.Length, "#D2");

			Assert.AreEqual ("file1.vb", resources[0], "#E1");
			Assert.IsNull (compiledAssembly.GetFile ("file1.vb"), "#E2");
			Assert.IsNotNull (compiledAssembly.GetManifestResourceStream  ("file1.vb"), "#E3");
			ManifestResourceInfo info = compiledAssembly.GetManifestResourceInfo ("file1.vb");
			Assert.IsNotNull (info, "#E4");
			Assert.IsNull (info.FileName, "#E5");
			Assert.IsNull (info.ReferencedAssembly, "#E6");
			Assert.AreEqual ((ResourceLocation.Embedded | ResourceLocation.ContainedInManifestFile), info.ResourceLocation, "#E7");

			Assert.AreEqual ("file2.vb", resources[1], "#F1");
			try {
				compiledAssembly.GetFile ("file2.vb");
				Assert.Fail ("#F2");
			} catch (FileNotFoundException) {
			}
			try {
				compiledAssembly.GetManifestResourceStream  ("file2.vb");
				Assert.Fail ("#F3");
			} catch (FileNotFoundException) {
			}
			info = compiledAssembly.GetManifestResourceInfo ("file2.vb");
			Assert.IsNotNull (info, "#F4");
			Assert.IsNotNull (info.FileName, "#F5");
			Assert.AreEqual ("file2.vb", info.FileName, "#F6");
			Assert.IsNull (info.ReferencedAssembly, "#F7");
			Assert.AreEqual ((ResourceLocation) 0, info.ResourceLocation, "#F8");
#endif
		}

		[Test]
		[Category ("NotWorking")] // we cannot rely on vbnc being available
		public void CompileFromFileBatch_Library_InMemory ()
		{
			// create vb source file
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
#if NET_2_0
			options.EmbeddedResources.Add (sourceFile1);
			options.LinkedResources.Add (sourceFile2);
#endif

			ICodeCompiler compiler = _codeProvider.CreateCompiler ();
			CompilerResults results = compiler.CompileAssemblyFromFileBatch (options,
				new string[] { sourceFile1, sourceFile2 });

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
			string[] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (2, tempFiles.Length, "#C1");
			Assert.IsTrue (File.Exists (sourceFile1), "#C2");
			Assert.IsTrue (File.Exists (sourceFile2), "#C3");

#if NET_2_0
			string[] resources = compiledAssembly.GetManifestResourceNames();
			Assert.IsNotNull (resources, "#D1");
			Assert.AreEqual (2, resources.Length, "#D2");

			Assert.AreEqual ("file1.vb", resources[0], "#E1");
			Assert.IsNull (compiledAssembly.GetFile ("file1.vb"), "#E2");
			Assert.IsNotNull (compiledAssembly.GetManifestResourceStream  ("file1.vb"), "#E3");
			ManifestResourceInfo info = compiledAssembly.GetManifestResourceInfo ("file1.vb");
			Assert.IsNotNull (info, "#E4");
			Assert.IsNull (info.FileName, "#E5");
			Assert.IsNull (info.ReferencedAssembly, "#E6");
			Assert.AreEqual ((ResourceLocation.Embedded | ResourceLocation.ContainedInManifestFile), info.ResourceLocation, "#E7");

			Assert.AreEqual ("file2.vb", resources[1], "#F1");
			try {
				compiledAssembly.GetFile ("file2.vb");
				Assert.Fail ("#F2");
			} catch (FileNotFoundException) {
			}
			try {
				compiledAssembly.GetManifestResourceStream  ("file2.vb");
				Assert.Fail ("#F3");
			} catch (FileNotFoundException) {
			}
			info = compiledAssembly.GetManifestResourceInfo ("file2.vb");
			Assert.IsNotNull (info, "#F4");
			Assert.IsNotNull (info.FileName, "#F5");
			Assert.AreEqual ("file2.vb", info.FileName, "#F6");
			Assert.IsNull (info.ReferencedAssembly, "#F7");
			Assert.AreEqual ((ResourceLocation) 0, info.ResourceLocation, "#F8");
#endif
		}

		[Test]
		[Category ("NotWorking")] // we cannot rely on vbnc being available
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
		[Category ("NotWorking")] // we cannot rely on vbnc being available
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
				new string[] { _sourceLibrary1, _sourceLibrary2 });

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
			Assert.AreEqual (tempFile, tempFiles[0], "#C2");
			Assert.AreEqual (outputAssembly, tempFiles [1], "#C3");
		}

		[Test]
		[Category ("NotWorking")] // we cannot rely on vbnc being available
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
		[Category ("NotWorking")] // we cannot rely on vbnc being available
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
		[Category ("NotWorking")] // we cannot rely on vbnc being available
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
		[Category ("NotWorking")] // we cannot rely on vbnc being available
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

		//TODO: [Test]
		public void CreateParser ()
		{
			//System.CodeDom.Compiler.ICodeParser CreateParser()
		}

		//TODO: [Test]
		public void CreateObjRef ()
		{
			//System.Runtime.Remoting.ObjRef CreateObjRef(System.Type requestedType)
		}

		bool Windows
		{
			get {
				return OS == OsType.Windows;
			}
		}

		bool Unix
		{
			get {
				return OS == OsType.Unix;
			}
		}

		bool Mac
		{
			get {
				return OS == OsType.Mac;
			}
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

				throw new Exception (compilerError.ToString ());
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
				CultureInfo.InvariantCulture, new object[0], null);
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

				VBCodeProvider codeProvider = new VBCodeProvider ();
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

				VBCodeProvider codeProvider = new VBCodeProvider ();
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
