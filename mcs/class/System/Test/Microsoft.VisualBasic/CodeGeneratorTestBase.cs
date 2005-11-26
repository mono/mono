//
// Microsoft.VisualBasic.* Test Cases
//
// Authors:
//      Jochen Wezel (jwezel@compumaster.de)
//
// Based on the C# units of
//	Erik LeBel (eriklebel@yahoo.ca)
//
// (c) 2003 Jochen Wezel, CompuMaster GmbH
//
using System;
using System.IO;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.VisualBasic;

using NUnit.Framework;

namespace MonoTests.Microsoft.VisualBasic
{
	///
	/// <summary>
	///	Base test for a variety of CodeGenerator GenerateCodeXXX methods.
	///
	///	This testing is a form of hybrid test, it tests the variety of CodeDom
	///	classes as well as the VB code generator.
	///
	///	The implementations bellow provide a template as well as guidlines for
	///	implementing further tests.
	/// </summary>
	///
	public abstract class CodeGeneratorTestBase
	{
		CodeDomProvider provider = null;
		protected ICodeGenerator generator = null;
		protected CodeGeneratorOptions options = null;
	
		public void InitBase()
		{
			provider = new VBCodeProvider ();
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
