//
// Microsoft.VisualBasic.VBCodeProvider.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using NUnit.Framework;
using System;
using Microsoft.VisualBasic;
using System.CodeDom.Compiler;
using System.ComponentModel;

namespace MonoTests.Microsoft.VisualBasic
{
	[TestFixture]
	public class VBCodeProviderTest : Assertion {
	
		CodeDomProvider MyVBCodeProvider;

		[SetUp]
		public void GetReady() { 
			MyVBCodeProvider = new VBCodeProvider(); 
		}

		[TearDown]
		public void Clean() {}

		[Test]
		public void FileExtension ()
		{
			AssertEquals ("#JW10", "vb", MyVBCodeProvider.FileExtension);
		}

		[Test]
		public void LanguageOptions ()
		{
			AssertEquals ("#JW20", System.CodeDom.Compiler.LanguageOptions.CaseInsensitive, MyVBCodeProvider.LanguageOptions);
		}

		[Test]
		public void CreateCompiler()
		{
			ICodeCompiler MyVBCodeCompiler;
			MyVBCodeCompiler = MyVBCodeProvider.CreateCompiler();
			Assert ("#JW30 - CreateCompiler", (MyVBCodeCompiler != null));
            System.CodeDom.Compiler.CompilerResults MyVBCodeCompilerResults;
            MyVBCodeCompilerResults = MyVBCodeCompiler.CompileAssemblyFromSource(new System.CodeDom.Compiler.CompilerParameters(), 
                 "public class TestModule\r\n\r\npublic shared sub Main()\r\nSystem.Console.WriteLine(\"Hello world!\")\r\nEnd Sub\r\nEnd Class\r\n");
			System.Collections.Specialized.StringCollection MyOutput;
			MyOutput = MyVBCodeCompilerResults.Output;
			string MyOutStr = "";
			foreach (string MyStr in MyOutput)
			{
				MyOutStr += MyStr + "\r\n\r\n";
			}
			if (MyOutStr != "")
				System.Console.WriteLine ("Error compiling VB.NET Hello world test application\r\n\r\n" + MyOutStr);
            AssertEquals ("#JW31 - Hello world compilation", 0, MyVBCodeCompilerResults.Errors.Count);
		}

		[Test]
		public void CreateGenerator()
		{
			ICodeGenerator MyVBCodeGen;
			MyVBCodeGen = MyVBCodeProvider.CreateGenerator();
			Assert ("#JW40 - CreateGenerator", (MyVBCodeGen != null));
			AssertEquals ("#JW41", true, MyVBCodeGen.Supports (System.CodeDom.Compiler.GeneratorSupport.DeclareEnums));
		}
		
		[Test]
		public void GetConverter ()
		{
			/* TODO
			//don't know why we get an error in MS.Net
			//MonoTests.Microsoft.VisualBasic.VBCodeProviderTest.GetConverter : System.ArgumentNullException : Der Schlüssel kann nicht Null sein. Parametername: key
			AssertEquals ("#JW50", "long", MyVBCodeTypeConv.GetType());
			*/
		}

	}
}
