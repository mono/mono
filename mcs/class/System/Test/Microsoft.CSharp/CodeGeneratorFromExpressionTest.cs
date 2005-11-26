//
// Microsoft.CSharp.* Test Cases
//
// Authors:
// 	Erik LeBel (eriklebel@yahoo.ca)
//
// (c) 2003 Erik LeBel
//
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;

using Microsoft.CSharp;

using NUnit.Framework;

namespace MonoTests.Microsoft.CSharp
{
	/// <summary>
	/// Test ICodeGenerator's GenerateCodeFromExpression, along with a 
	/// minimal set CodeDom components.
	/// </summary>
	[TestFixture]
	public class CodeGeneratorFromExpressionTest
	{
		private CSharpCodeProvider provider;
		private ICodeGenerator generator;
		private CodeGeneratorOptions options;

		[SetUp]
		public void SetUp ()
		{
			provider = new CSharpCodeProvider ();
			generator = provider.CreateGenerator ();
			options = new CodeGeneratorOptions ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DefaultExpressionTest ()
		{
			using (StringWriter sw = new StringWriter ()) {
				Generate (new CodeExpression (), sw);
				sw.Close ();
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullExpressionTest ()
		{
			using (StringWriter sw = new StringWriter ()) {
				Generate (null, sw);
			}
		}

		[Test]
		public void TypeReferenceExpressionTest ()
		{
			StringBuilder sb = new StringBuilder ();

			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual ("bool", Generate (new CodeTypeReferenceExpression (typeof (bool)), sw), "#1");
				sb.Length = 0;
				Assert.AreEqual ("char", Generate (new CodeTypeReferenceExpression (typeof (char)), sw), "#2");
				sb.Length = 0;
				Assert.AreEqual (typeof(DateTime).FullName, Generate (new CodeTypeReferenceExpression (typeof (DateTime)), sw), "#3");
				sb.Length = 0;
				Assert.AreEqual ("short", Generate (new CodeTypeReferenceExpression (typeof (short)), sw), "#4");
				sb.Length = 0;
				Assert.AreEqual ("int", Generate (new CodeTypeReferenceExpression (typeof (int)), sw), "#5");
				sb.Length = 0;
				Assert.AreEqual ("long", Generate (new CodeTypeReferenceExpression (typeof (long)), sw), "#6");
				sb.Length = 0;
				Assert.AreEqual ("object", Generate (new CodeTypeReferenceExpression (typeof (object)), sw), "#7");
				sb.Length = 0;
				Assert.AreEqual ("void", Generate (new CodeTypeReferenceExpression (typeof (void)), sw), "#8");
				sb.Length = 0;
				Assert.AreEqual ("void", Generate (new CodeTypeReferenceExpression ((string) null), sw), "#9");
				sb.Length = 0;
				Assert.AreEqual ("void", Generate (new CodeTypeReferenceExpression (""), sw), "#10");
				sb.Length = 0;
#if NET_2_0
				Assert.AreEqual ("byte", Generate (new CodeTypeReferenceExpression (typeof (byte)), sw), "#11");
				sb.Length = 0;
				Assert.AreEqual ("decimal", Generate (new CodeTypeReferenceExpression (typeof (decimal)), sw), "#12");
				sb.Length = 0;
				Assert.AreEqual ("double", Generate (new CodeTypeReferenceExpression (typeof (double)), sw), "#13");
				sb.Length = 0;
				Assert.AreEqual ("sbyte", Generate (new CodeTypeReferenceExpression (typeof (sbyte)), sw), "#14");
				sb.Length = 0;
				Assert.AreEqual ("ushort", Generate (new CodeTypeReferenceExpression (typeof (ushort)), sw), "#15");
				sb.Length = 0;
				Assert.AreEqual ("uint", Generate (new CodeTypeReferenceExpression (typeof (uint)), sw), "#16");
				sb.Length = 0;
				Assert.AreEqual ("ulong", Generate (new CodeTypeReferenceExpression (typeof (ulong)), sw), "#17");
				sb.Length = 0;
				Assert.AreEqual ("float", Generate (new CodeTypeReferenceExpression (typeof (float)), sw), "#18");
				sb.Length = 0;
#else
				Assert.AreEqual (typeof (byte).FullName, Generate (new CodeTypeReferenceExpression (typeof (byte)), sw), "#19");
				sb.Length = 0;
				Assert.AreEqual (typeof (decimal).FullName, Generate (new CodeTypeReferenceExpression (typeof (decimal)), sw), "#20");
				sb.Length = 0;
				Assert.AreEqual (typeof (double).FullName, Generate (new CodeTypeReferenceExpression (typeof (double)), sw), "#21");
				sb.Length = 0;
				Assert.AreEqual (typeof (sbyte).FullName, Generate (new CodeTypeReferenceExpression (typeof (sbyte)), sw), "#22");
				sb.Length = 0;
				Assert.AreEqual (typeof (ushort).FullName, Generate (new CodeTypeReferenceExpression (typeof (ushort)), sw), "#23");
				sb.Length = 0;
				Assert.AreEqual (typeof (uint).FullName, Generate (new CodeTypeReferenceExpression (typeof (uint)), sw), "#24");
				sb.Length = 0;
				Assert.AreEqual (typeof (ulong).FullName, Generate (new CodeTypeReferenceExpression (typeof (ulong)), sw), "#25");
				sb.Length = 0;
				Assert.AreEqual (typeof (float).FullName, Generate (new CodeTypeReferenceExpression (typeof (float)), sw), "#26");
				sb.Length = 0;
#endif
				sw.Close ();
			}
		}

		[Test]
		public void ParameterDeclarationExpressionTest ()
		{
			CodeParameterDeclarationExpression cpde = null;

			StringBuilder sb = new StringBuilder ();

			using (StringWriter sw = new StringWriter (sb)) {
				cpde = new CodeParameterDeclarationExpression ();
				Assert.AreEqual ("void ", Generate (cpde, sw), "#1");
				sb.Length = 0;

				cpde = new CodeParameterDeclarationExpression ((string) null,
					(string) null);
				Assert.AreEqual ("void ", Generate (cpde, sw), "#2");
				sb.Length = 0;

				cpde = new CodeParameterDeclarationExpression ("A", (string) null);
				Assert.AreEqual ("A ", Generate (cpde, sw), "#4");
				sb.Length = 0;

				cpde = new CodeParameterDeclarationExpression ((string) null, "B");
				Assert.AreEqual ("void B", Generate (cpde, sw), "#4");
				sb.Length = 0;

				cpde = new CodeParameterDeclarationExpression ("A", "B");
				Assert.AreEqual ("A B", Generate (cpde, sw), "#5");
				sb.Length = 0;

				cpde.Direction = FieldDirection.Out;
				Assert.AreEqual ("out A B", Generate (cpde, sw), "#6");
				sb.Length = 0;

				cpde.Direction = FieldDirection.Ref;
				Assert.AreEqual ("ref A B", Generate (cpde, sw), "#7");
				sb.Length = 0;
			}
		}

		private string Generate (CodeExpression expression, StringWriter sw)
		{
			generator.GenerateCodeFromExpression (expression, sw, options);
			return sw.ToString ();
		}
	}
}
