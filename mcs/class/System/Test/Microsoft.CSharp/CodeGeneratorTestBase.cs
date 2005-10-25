//
// Microsoft.CSharp.* Test Cases
//
// Authors:
// 	Erik LeBel (eriklebel@yahoo.ca)
//
// (c) 2003 Erik LeBel
//
using System;
using System.IO;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

using NUnit.Framework;

namespace MonoTests.Microsoft.CSharp
{
	///
	/// <summary>
	///	Base test for a variety of CodeGenerator GenerateCodeXXX methods.
	///
	///	This testing is a form of hybrid test, it tests the variety of CodeDom
	///	classes as well as the C# code generator.
	///
	///	The implementations bellow provide a template as well as guidlines for
	///	implementing further tests.
	/// </summary>
	///
	public abstract class CodeGeneratorTestBase
	{
		private CodeDomProvider provider = null;
		protected ICodeGenerator generator = null;
		protected CodeGeneratorOptions options = null;
	
		public void InitBase()
		{
			provider = new CSharpCodeProvider ();
			generator = provider.CreateGenerator ();
			options = new CodeGeneratorOptions ();
		}

		protected string Generate ()
		{
			return Generate (options);
		}

		protected virtual string NewLine
		{
			get { return "\n"; }
		}

		protected abstract string Generate (CodeGeneratorOptions options);
	}
}
