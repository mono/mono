//
// Microsoft.CSharp.* Test Cases
//
// Authors:
// 	Erik LeBel (eriklebel@yahoo.ca)
//
// (c) 2003 Erik LeBel
//
using System;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;

using NUnit.Framework;

namespace MonoTests.Microsoft.CSharp
{
	///
	/// <summary>
	///	Test ICodeGenerator's GenerateCodeFromType, along with a 
	///	minimal set CodeDom components.
	/// </summary>
	///
	[TestFixture]
	public class CodeGeneratorFromTypeTest: CodeGeneratorTestBase
	{
		CodeTypeDeclaration type = null;

		[SetUp]
		public void Init ()
		{
			InitBase ();
			type = new CodeTypeDeclaration ();
		}
		
		protected override void Generate ()
		{
			generator.GenerateCodeFromType (type, writer, options);
			writer.Close ();
		}
		
		[Test]
		public void DefaultTypeTest ()
		{
			Generate ();
			Assertion.AssertEquals ("public class  {\n}\n", Code);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]		    
		public void NullTypeTest ()
		{
			type = null;
			Generate ();
		}

		/*
		[Test]
		public void ReferencedTest ()
		{
			codeUnit.ReferencedAssemblies.Add ("System.dll");
			Generate ();
			Assertion.AssertEquals ("", Code);
		}
		*/
	}
}
