// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.CodeDom.Compiler;

namespace NUnit.Fixtures
{
	/// <summary>
	/// Summary description for CSharpCompiler.
	/// </summary>
	public class TestCompiler
	{
		ICodeCompiler compiler;
		CompilerParameters options;

		public TestCompiler() : this( null, null ) { }

		public TestCompiler( string[] assemblyNames ) : this( assemblyNames, null ) { }

		public TestCompiler( string[] assemblyNames, string outputName )
		{
			Microsoft.CSharp.CSharpCodeProvider provider = new Microsoft.CSharp.CSharpCodeProvider();
			this.compiler = provider.CreateCompiler();
			this.options = new CompilerParameters();

			if ( assemblyNames != null && assemblyNames.Length > 0 )
				options.ReferencedAssemblies.AddRange( assemblyNames );
			if ( outputName != null )
				options.OutputAssembly = outputName;

			options.IncludeDebugInformation = false;
			options.TempFiles = new TempFileCollection( ".", false );
			options.GenerateInMemory = false;
		}

		public CompilerParameters Options
		{
			get { return options; }
		}

		public CompilerResults CompileCode( string code )
		{
			return compiler.CompileAssemblyFromSource( options, code );
		}
	}
}
