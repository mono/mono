//
// MonoTests.Microsoft.CSharp.CompilerTest
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2002 Jackson Harper, All rights reserved.
//

using System;
using System.Collections;
using Microsoft.CSharp;
using NUnit.Framework;

namespace MonoTests.Microsoft.CSharp  {

	public class CompilerTest : TestCase {

		public CompilerTest () :
			base ("[MonoTests.Microsoft.CSharp.CompilerTest]") {}

		public CompilerTest (string name) : base (name)
		{
		}

		public static ITest Suite {
			get {
				return new TestSuite (typeof (CompilerTest));
			}
		}

		protected override void SetUp () { }

		protected override void TearDown () { }

		// Just try to get through a compile with no errors
		public void TestCompile ()
		{
			string[] texts = {"CompilerTest.cs"};
			string[] imports = {"../System.dll"};
			Hashtable options = new Hashtable ();
			CompilerError[] errors;

			options.Add ("target", "library");
			
			errors = Compiler.Compile (texts, texts, 
				"DeleteMe.dll", imports, options);
		
		}

		public void TestCompileNoFiles ()
		{	
			string[] nothing = { String.Empty };
			CompilerError[] errors;

			errors = Compiler.Compile (nothing,nothing,
				"DeleteMe.dll", null, null);

			AssertEquals ("#A01", errors.Length, 1);
			AssertEquals ("#A02", errors[0].SourceFile, String.Empty);
			AssertEquals ("#A03", errors[0].SourceLine, 0);
			AssertEquals ("#A04", errors[0].SourceColumn, 0);
			AssertEquals ("#A05", errors[0].ErrorNumber, 2008);
			
		}

		public void TestCompileMissingRef ()
		{
			string[] texts = {"CompilerTest.cs"};
			Hashtable options = new Hashtable ();
			CompilerError[] errors;

			options.Add ("target", "library");
			
			errors = Compiler.Compile (texts, texts, 
				"DeleteMe.dll", null, options);

			AssertEquals ("#A06", errors.Length, 1);
			AssertEquals ("#A07", errors[0].SourceFile, texts[0]);
			AssertEquals ("#A08", errors[0].ErrorNumber, 246);
		}

		public void TestCompileBadTarget ()
		{
			string[] texts = {"CompilerTest.cs"};
			string[] imports = {"../System.dll", 
				"../../../../nunit/NUnitCore.dll"};
			Hashtable options = new Hashtable ();
			CompilerError[] errors;

			errors = Compiler.Compile (texts, texts, 
				"DeleteMe.dll", imports, null);
			
			AssertEquals ("#A06", errors.Length, 1);
			AssertEquals ("#A07", errors[0].SourceFile, texts[0]);
			AssertEquals ("#A08", errors[0].ErrorNumber, 5001);
		}

		public void TestCompileNullSource ()
		{
			string[] texts = {"CompilerTest.cs"};
			string[] imports = {"../System.dll", 
				"../../../../nunit/NUnitCore.dll"};
			Hashtable options = new Hashtable ();
			CompilerError[] errors;

			try {
				errors = Compiler.Compile (null, texts, 
					"DeleteMe.dll", imports, null);
			} catch (Exception e) {
				if (!(e is ArgumentNullException))
					Fail ("#F01 incorrect exception thrown " + e);
				return;
			}

			Fail ("#F02 ArgumentNullException not thrown");
		}

		public void TestCompileNullNames ()
		{
			string[] texts = {"CompilerTest.cs"};
			string[] imports = {"../System.dll", 
				"../../../../nunit/NUnitCore.dll"};
			Hashtable options = new Hashtable ();
			CompilerError[] errors;

			try {
				errors = Compiler.Compile (texts, null, 
					"DeleteMe.dll", imports, null);
			} catch (Exception e) {
				if (!(e is ArgumentNullException))
					Fail ("#F03 incorrect exception thrown " + e);
				return;
			}

			Fail ("#F04 ArgumentNullException not thrown");	
		}

		public void TestCompileNullTarget ()
		{
			string[] texts = {"CompilerTest.cs"};
			string[] imports = {"../System.dll", 
				"../../../../nunit/NUnitCore.dll"};
			Hashtable options = new Hashtable ();
			CompilerError[] errors;

			try {
				errors = Compiler.Compile (texts, texts, 
					null, imports, null);
			} catch (Exception e) {
				if (!(e is ArgumentNullException))
					Fail ("#F05 incorrect exception thrown " + e);
				return;
			}

			Fail ("#F06 ArgumentNullException not thrown");	
		}
		
	}

}