// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.CodeDom.Compiler;
using NUnit.Core;
using NUnit.Util;

namespace NUnit.Fixtures
{
	/// <summary>
	/// Abstract base class for fixtures that compile a snippet of code.
	/// The fixture is basically a column fixture with one input column
	/// dedicated to containing the code that is to be compiled. This
	/// will normally be the first column
	/// </summary>
	public class SnippetRunner : TestLoadFixture
	{
		public string Code;

		private static readonly string testAssembly = "test.dll";

		// Override doCell to handle the 'Code' column. We compile
		// the code and optionally load and run the tests.
		public override void doCell(fit.Parse cell, int columnNumber)
		{
			base.doCell (cell, columnNumber);

			FieldInfo field = columnBindings[columnNumber].field;
			if ( field != null && field.Name == "Code" && CompileCodeSnippet( cell, Code ) )
				LoadAndRunTestAssembly( cell, testAssembly );
		}

		private bool CompileCodeSnippet( fit.Parse cell, string code )
		{
			TestCompiler compiler = new TestCompiler( 
				new string[] { "system.dll", "nunit.framework.dll" }, 
				testAssembly );

			CompilerResults results = compiler.CompileCode( code );
			if ( results.NativeCompilerReturnValue == 0 )
				return true;

			cell.addToBody( "<font size=-1 color=\"#c08080\"><i>Compiler errors</i></font>" );

			wrong( cell );
			cell.addToBody( "<hr>" );
				
			foreach( string line in results.Output )
				cell.addToBody( line + "<br>" );

			return true;
		}

		public TestTree Tree()
		{
			if ( testRunner.Test == null )
				return new TestTree( "NULL" );

			if ( testRunner.Test.Tests.Count == 0 )
				return new TestTree( "EMPTY" );

			StringBuilder sb = new StringBuilder();
			AppendTests( sb, "", testRunner.Test.Tests );

			return new TestTree( sb.ToString() );
		}

		private void AppendTests( StringBuilder sb, string prefix, IList tests )
		{
			foreach( TestNode test in tests )
			{
				sb.Append( prefix );
				sb.Append( test.TestName.Name );
				sb.Append( Environment.NewLine );
				if ( test.Tests != null )
					AppendTests( sb, prefix + ">", test.Tests );
			}
		}
	}
}
