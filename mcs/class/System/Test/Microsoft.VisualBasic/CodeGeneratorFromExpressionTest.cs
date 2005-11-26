//
// Microsoft.VisualBasic.* Test Cases
//
// Authors:
// 	Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) 2005 Novell
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
	[TestFixture]
	public class CodeGeneratorFromExpressionTest
	{
		VBCodeProvider provider;
		ICodeGenerator generator;
		CodeGeneratorOptions options;

		[SetUp]
		public void SetUp ()
		{
			provider = new VBCodeProvider ();
			generator = provider.CreateGenerator ();
			options = new CodeGeneratorOptions ();
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DefaultExpressionTest ()
		{
			using (StringWriter sw = new StringWriter ()) {
				Generate(new CodeExpression (), sw);
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
			StringBuilder sb = new StringBuilder();

			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual ("Boolean", Generate (new CodeTypeReferenceExpression (typeof (bool)), sw), "#1");
				sb.Length = 0;
				Assert.AreEqual ("Byte", Generate (new CodeTypeReferenceExpression (typeof (byte)), sw), "#2");
				sb.Length = 0;
				Assert.AreEqual ("Char", Generate (new CodeTypeReferenceExpression (typeof (char)), sw), "#3");
				sb.Length = 0;
				Assert.AreEqual ("Date", Generate (new CodeTypeReferenceExpression (typeof (DateTime)), sw), "#4");
				sb.Length = 0;
				Assert.AreEqual ("Decimal", Generate (new CodeTypeReferenceExpression (typeof (decimal)), sw), "#5");
				sb.Length = 0;
				Assert.AreEqual ("Double", Generate (new CodeTypeReferenceExpression (typeof (double)), sw), "#6");
				sb.Length = 0;
				Assert.AreEqual ("Short", Generate (new CodeTypeReferenceExpression (typeof (short)), sw), "#7");
				sb.Length = 0;
				Assert.AreEqual ("Integer", Generate (new CodeTypeReferenceExpression (typeof (int)), sw), "#8");
				sb.Length = 0;
				Assert.AreEqual ("Long", Generate (new CodeTypeReferenceExpression (typeof (long)), sw), "#9");
				sb.Length = 0;
				Assert.AreEqual ("Single", Generate (new CodeTypeReferenceExpression (typeof (float)), sw), "#10");
				sb.Length = 0;
				Assert.AreEqual ("Object", Generate (new CodeTypeReferenceExpression (typeof (object)), sw), "#11");
				sb.Length = 0;
				Assert.AreEqual (typeof (void).FullName, Generate (new CodeTypeReferenceExpression (typeof (void)), sw), "#12");
				sb.Length = 0;
				Assert.AreEqual (typeof (void).FullName, Generate (new CodeTypeReferenceExpression ((string) null), sw), "#13");
				sb.Length = 0;
				Assert.AreEqual (typeof (void).FullName, Generate (new CodeTypeReferenceExpression (""), sw), "#14");
				sb.Length = 0;
#if NET_2_0
				Assert.AreEqual ("SByte", Generate (new CodeTypeReferenceExpression (typeof (sbyte)), sw), "#15");
				sb.Length = 0;
				Assert.AreEqual ("UShort", Generate (new CodeTypeReferenceExpression (typeof (ushort)), sw), "#16");
				sb.Length = 0;
				Assert.AreEqual ("UInteger", Generate (new CodeTypeReferenceExpression (typeof (uint)), sw), "#17");
				sb.Length = 0;
				Assert.AreEqual ("ULong", Generate (new CodeTypeReferenceExpression (typeof (ulong)), sw), "#18");
				sb.Length = 0;
#else
				Assert.AreEqual (typeof (sbyte).FullName, Generate (new CodeTypeReferenceExpression (typeof (sbyte)), sw), "#19");
				sb.Length = 0;
				Assert.AreEqual (typeof(ushort).FullName, Generate (new CodeTypeReferenceExpression (typeof (ushort)), sw), "#20");
				sb.Length = 0;
				Assert.AreEqual (typeof(uint).FullName, Generate (new CodeTypeReferenceExpression (typeof (uint)), sw), "#21");
				sb.Length = 0;
				Assert.AreEqual (typeof(ulong).FullName, Generate (new CodeTypeReferenceExpression (typeof (ulong)), sw), "#22");
				sb.Length = 0;
#endif
				sw.Close ();
			}
		}

		[Test]
		public void ParameterDeclarationExpressionTest ()
		{
			CodeParameterDeclarationExpression cpde = null;
			
			StringBuilder sb = new StringBuilder();

			using (StringWriter sw = new StringWriter (sb)) {
				cpde = new CodeParameterDeclarationExpression ();
#if NET_2_0
				Assert.AreEqual ("ByVal __exception As System.Void", Generate (cpde, sw), "#1");
#else
				Assert.AreEqual ("ByVal  As System.Void", Generate (cpde, sw), "#1");
#endif
				sb.Length = 0;

				cpde = new CodeParameterDeclarationExpression ((string) null,
					(string) null);
#if NET_2_0
				Assert.AreEqual ("ByVal __exception As System.Void", Generate (cpde, sw), "#2");
#else
				Assert.AreEqual ("ByVal  As System.Void", Generate (cpde, sw), "#2");
#endif
				sb.Length = 0;

				cpde = new CodeParameterDeclarationExpression ("A", (string) null);
#if NET_2_0
				Assert.AreEqual ("ByVal __exception As A", Generate (cpde, sw), "#3");
#else
				Assert.AreEqual ("ByVal  As A", Generate (cpde, sw), "#3");
#endif
				sb.Length = 0;

				cpde = new CodeParameterDeclarationExpression ((string) null, "B");
				Assert.AreEqual ("ByVal B As System.Void", Generate (cpde, sw), "#4");
				sb.Length = 0;

				cpde = new CodeParameterDeclarationExpression ("A", "B");
				Assert.AreEqual ("ByVal B As A", Generate (cpde, sw), "#5");
				sb.Length = 0;

				cpde.Direction = FieldDirection.Out;
				Assert.AreEqual ("ByRef B As A", Generate (cpde, sw), "#6");
				sb.Length = 0;

				cpde.Direction = FieldDirection.Ref;
				Assert.AreEqual ("ByRef B As A", Generate (cpde, sw), "#7");
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
