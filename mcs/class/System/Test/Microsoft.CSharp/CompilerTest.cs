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

		public void TestCompile ()
		{
			string[] texts = {"CompilerTest.cs"};
			string[] imports = {"System.dll"};
			Hashtable options = new Hashtable ();
			CompilerError[] errors;

			options.Add ("target", "library");
			errors = Compiler.Compile (texts, texts, "DeleteMe.dll", imports, options);
	
			foreach (CompilerError error in errors) {
				Console.WriteLine (error);
			}
		}
		
		
	}

}