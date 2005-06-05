//
// Microsoft.VisualBasic.VBCodeProvider.cs
//
// Author:
//   Jochen Wezel (jwezel@compumaster.de) //
//
// (C) 2003 Jochen Wezel (CompuMaster GmbH)
//
// Last modifications:
// 2003-12-10 JW: publishing of this file
//

using NUnit.Framework;
using System;
using Microsoft.VisualBasic;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Diagnostics;
using System.IO;

namespace MonoTests.Microsoft.VisualBasic
{

	enum OsType {
		Windows,
		Unix,
		Mac
	}

	[TestFixture]
	public class VBCodeProviderTest {
	
		CodeDomProvider MyVBCodeProvider;
		static OsType OS;
		static char DSC = Path.DirectorySeparatorChar;

		[SetUp]
		public void GetReady() { 
			if ('/' == DSC) {
				OS = OsType.Unix;
			} else if ('\\' == DSC) {
				OS = OsType.Windows;
			} else {
				OS = OsType.Mac;
			}

			MyVBCodeProvider = new VBCodeProvider(); 
		}

		[Test]
		public void FileExtension ()
		{
			Assert.AreEqual("vb", MyVBCodeProvider.FileExtension, "#JW10");
		}

		[Test]
		public void LanguageOptionsTest ()
		{
			Assert.AreEqual(LanguageOptions.CaseInsensitive, MyVBCodeProvider.LanguageOptions, "#JW20");
		}

		[Test]
		public void CreateCompiler()
		{
			// prepare the compilation
			ICodeCompiler MyVBCodeCompiler = MyVBCodeProvider.CreateCompiler();
			Assert.IsNotNull(MyVBCodeCompiler, "#JW30 - CreateCompiler");
			
			CompilerParameters options = new CompilerParameters();
			options.GenerateExecutable = true;
			options.IncludeDebugInformation = true;
			options.TreatWarningsAsErrors = true;
			
			// process compilation
			CompilerResults MyVBCodeCompilerResults = MyVBCodeCompiler.CompileAssemblyFromSource(options,
				"public class TestModule" + Environment.NewLine + "public shared sub Main()" 
				+ Environment.NewLine + "System.Console.Write(\"Hello world!\")" 
				+ Environment.NewLine + "End Sub" + Environment.NewLine + "End Class");

			// verify outcome of compilation
			if (MyVBCodeCompilerResults.Errors.Count > 0) {
				Assert.Fail("Hello World compilation failed: " + MyVBCodeCompilerResults.Errors[0].ToString());
			}

			try
			{
				Assembly MyAss = MyVBCodeCompilerResults.CompiledAssembly;
			}
			catch (Exception ex)
			{
				Assert.Fail("#JW32 - MyVBCodeCompilerResults.CompiledAssembly hasn't been an expected object" + 
						Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
			}

			// Execute the test app
			ProcessStartInfo NewProcInfo = new ProcessStartInfo();
			if (Windows) {
				NewProcInfo.FileName = MyVBCodeCompilerResults.CompiledAssembly.Location;
			}
			else {
				NewProcInfo.FileName = "mono";
				NewProcInfo.Arguments = MyVBCodeCompilerResults.CompiledAssembly.Location;
			}
			NewProcInfo.RedirectStandardOutput = true;
			NewProcInfo.UseShellExecute = false;
			NewProcInfo.CreateNoWindow = true;
			string TestAppOutput = "";

			try
			{
				Process MyProc = Process.Start(NewProcInfo);
				MyProc.WaitForExit();
				TestAppOutput = MyProc.StandardOutput.ReadToEnd();
				MyProc.Close();
				MyProc.Dispose();
			}
			catch (Exception ex)
			{
				Assert.Fail("#JW34 - " + ex.Message + Environment.NewLine + ex.StackTrace);
			}
			
			Assert.AreEqual("Hello world!", TestAppOutput, "#JW33 - Application output");

			// Clean up
			try
			{
				File.Delete (NewProcInfo.FileName);
			}
			catch {}
		}

		// NOTE: This test does not clean-up the generated assemblies
		[Test]
		public void CompileAssembly_InMemory ()
		{
			// NOT in memory
			CompilerResults results = CompileAssembly (false);
			Assert.IsTrue(results.CompiledAssembly.Location.Length != 0, "#1");
			Assert.IsNotNull(results.PathToAssembly, "#2");

			// in memory
			results = CompileAssembly (true);
			Assert.AreEqual(string.Empty, results.CompiledAssembly.Location, "#3");
			Assert.IsNull(results.PathToAssembly, "#4");
		}

		[Test]
		public void CreateGenerator()
		{
			ICodeGenerator MyVBCodeGen;
			MyVBCodeGen = MyVBCodeProvider.CreateGenerator();
			Assert.IsNotNull(MyVBCodeGen, "#JW40 - CreateGenerator");
			Assert.IsTrue(MyVBCodeGen.Supports(GeneratorSupport.DeclareEnums), "#JW41");
		}

		
		//TODO: [Test]
		public void	CreateParser()
		{
			//System.CodeDom.Compiler.ICodeParser CreateParser()
		}

		//TODO: [Test]
		public void	CreateObjRef()
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

		private CompilerResults CompileAssembly(bool inMemory) {
			CompilerParameters options = new CompilerParameters();
			options.GenerateExecutable = false;
			options.GenerateInMemory = inMemory;

			VBCodeProvider codeProvider = new VBCodeProvider();
			ICodeCompiler compiler = codeProvider.CreateCompiler();
			return compiler.CompileAssemblyFromDom(options,
								new CodeCompileUnit());
		}
	}
}
