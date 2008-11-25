// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.IO;
using System.CodeDom.Compiler;
using NUnit.Framework;
using NUnit.Core;
using NUnit.Util;

namespace NUnit.Fixtures.Tests
{
	/// <summary>
	/// Summary description for CompilationTests.
	/// </summary>
	[TestFixture,Platform(Exclude="Mono",Reason="Holds output file open")]
	public class CompilationTests
	{
		private TestCompiler compiler;
		private static string[] references = new string[] { "System.dll", "nunit.framework.dll" };
		private static string outputName = "test.dll";
		private static string goodCode = 
@"using System;
using NUnit.Framework;

namespace My.Namespace
{
    [TestFixture] public class SomeClass
    {
		[Test] public void ThisMethod() { }
		[Test] public void ThatMethod() { }
	}
}";

		[SetUp]
		public void CreateCompiler()
		{
			this.compiler = new TestCompiler( references, outputName );
		}

		[TearDown]
		public void RemoveOutputFile()
		{
			if ( File.Exists( outputName ) )
				File.Delete( outputName );
		}

		[Test]
		public void CheckDefaultSettings()
		{
			CollectionAssert.AreEqual( references, compiler.Options.ReferencedAssemblies );
			Assert.AreEqual( outputName, compiler.Options.OutputAssembly );

			Assert.IsFalse( compiler.Options.IncludeDebugInformation, "IncludeDebugInformation" );
			Assert.IsFalse( compiler.Options.GenerateInMemory, "GenerateInMemory" );
			Assert.IsFalse( compiler.Options.GenerateExecutable, "GenerateExecutable" );
		}

		[Test]
		public void CompileToFile()
		{
			CompilerResults results = compiler.CompileCode( goodCode );
			Assert.AreEqual( 0, results.NativeCompilerReturnValue );
			Assert.IsNotNull( results.CompiledAssembly );
			Assert.IsTrue( File.Exists( outputName ) );
		}

		[Test]
		public void CompilingBadCodeGivesAnError()
		{
			string badCode = goodCode.Replace( "void", "vide" );
		    Assert.AreNotEqual( 0, compiler.CompileCode( badCode ).NativeCompilerReturnValue );
		}

		[Test]
		public void LoadTestsFromCompiledAssembly()
		{
			CompilerResults results = compiler.CompileCode( goodCode );
			Assert.AreEqual( 0, results.NativeCompilerReturnValue );

			TestRunner runner = new SimpleTestRunner();

			try
			{
				Assert.IsTrue( runner.Load( new TestPackage( outputName ) ) );
				Assert.AreEqual( 2, runner.Test.TestCount );
			}
			finally
			{
				runner.Unload();
			}
		}
	}
}
