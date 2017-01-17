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
using System.Globalization;
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
		public void DefaultExpressionTest ()
		{
			using (StringWriter sw = new StringWriter ()) {
				try {
					Generate (new CodeExpression (), sw);
					Assert.Fail ("#1");
				} catch (ArgumentException ex) {
					// Element type System.CodeDom.CodeExpression is not supported
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("e", ex.ParamName, "#5");
				}
			}
		}

		[Test]
		public void NullExpressionTest ()
		{
			using (StringWriter sw = new StringWriter ()) {
				try {
					Generate (null, sw);
					Assert.Fail ("#1");
				} catch (ArgumentNullException ex) {
					Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
					Assert.IsNull (ex.InnerException, "#3");
					Assert.IsNotNull (ex.Message, "#4");
					Assert.AreEqual ("e", ex.ParamName, "#5");
				}
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
				sw.Close ();
			}
		}

		[Test]
		public void PrimitiveExpressionTest ()
		{
			StringBuilder sb = new StringBuilder ();

			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual ("null", Generate (new CodePrimitiveExpression (null), sw), "#1");
				sb.Length = 0;
				Assert.AreEqual ("\"AB\\\"C\"", Generate (new CodePrimitiveExpression ("AB\"C"), sw), "#2");
				sb.Length = 0;
				Assert.AreEqual ("5", Generate (new CodePrimitiveExpression ((byte) 5), sw), "#4");
				sb.Length = 0;
				Assert.AreEqual ("20", Generate (new CodePrimitiveExpression ((short) 20), sw), "#5");
				sb.Length = 0;
				Assert.AreEqual ("243", Generate (new CodePrimitiveExpression (243), sw), "#6");
				sb.Length = 0;
				Assert.AreEqual ("434343", Generate (new CodePrimitiveExpression ((long) 434343), sw), "#7");
				sb.Length = 0;
				Assert.AreEqual ("6.445F", Generate (new CodePrimitiveExpression ((float) 6.445), sw), "#8");
				sb.Length = 0;
				Assert.AreEqual ("5.76D", Generate (new CodePrimitiveExpression ((double) 5.76), sw), "#9");
				sb.Length = 0;
				Assert.AreEqual ("7.667m", Generate (new CodePrimitiveExpression ((decimal) 7.667), sw), "#10");
				sb.Length = 0;
				Assert.AreEqual ("true", Generate (new CodePrimitiveExpression (true), sw), "#11");
				sb.Length = 0;
				Assert.AreEqual ("false", Generate (new CodePrimitiveExpression (false), sw), "#12");
				sw.Close ();
			}
		}

		[Test]
		public void PrimitiveExpressionTest_Char ()
		{
			StringBuilder sb = new StringBuilder ();

			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual ("'\\0'", Generate (new CodePrimitiveExpression ('\0'), sw), "#0");
				sb.Length = 0;
				Assert.AreEqual ("'\x01'", Generate (new CodePrimitiveExpression ('\x01'), sw), "#1");
				sb.Length = 0;
				Assert.AreEqual ("'\x02'", Generate (new CodePrimitiveExpression ('\x02'), sw), "#2");
				sb.Length = 0;
				Assert.AreEqual ("'\x03'", Generate (new CodePrimitiveExpression ('\x03'), sw), "#3");
				sb.Length = 0;
				Assert.AreEqual ("'\x04'", Generate (new CodePrimitiveExpression ('\x04'), sw), "#4");
				sb.Length = 0;
				Assert.AreEqual ("'\x05'", Generate (new CodePrimitiveExpression ('\x05'), sw), "#5");
				sb.Length = 0;
				Assert.AreEqual ("'\x06'", Generate (new CodePrimitiveExpression ('\x06'), sw), "#6");
				sb.Length = 0;
				Assert.AreEqual ("'\a'", Generate (new CodePrimitiveExpression ('\a'), sw), "#7");
				sb.Length = 0;
				Assert.AreEqual ("'\b'", Generate (new CodePrimitiveExpression ('\b'), sw), "#8");
				sb.Length = 0;
				Assert.AreEqual ("'\\t'", Generate (new CodePrimitiveExpression ('\t'), sw), "#9");
				sb.Length = 0;
				Assert.AreEqual ("'\\n'", Generate (new CodePrimitiveExpression ('\n'), sw), "#10");
				sb.Length = 0;
				Assert.AreEqual ("'\v'", Generate (new CodePrimitiveExpression ('\v'), sw), "#11");
				sb.Length = 0;
				Assert.AreEqual ("'\f'", Generate (new CodePrimitiveExpression ('\f'), sw), "#12");
				sb.Length = 0;
				Assert.AreEqual ("'\\r'", Generate (new CodePrimitiveExpression ('\r'), sw), "#13");
				sb.Length = 0;
				Assert.AreEqual ("'\x0E'", Generate (new CodePrimitiveExpression ('\x0E'), sw), "#14");
				sb.Length = 0;
				Assert.AreEqual ("'\x0F'", Generate (new CodePrimitiveExpression ('\x0F'), sw), "#15");
				sb.Length = 0;
				Assert.AreEqual ("'\x10'", Generate (new CodePrimitiveExpression ('\x10'), sw), "#16");
				sb.Length = 0;
				Assert.AreEqual ("'\x11'", Generate (new CodePrimitiveExpression ('\x11'), sw), "#17");
				sb.Length = 0;
				Assert.AreEqual ("'\x12'", Generate (new CodePrimitiveExpression ('\x12'), sw), "#18");
				sb.Length = 0;
				Assert.AreEqual ("'\x13'", Generate (new CodePrimitiveExpression ('\x13'), sw), "#19");
				sb.Length = 0;
				Assert.AreEqual ("'\x14'", Generate (new CodePrimitiveExpression ('\x14'), sw), "#20");
				sb.Length = 0;
				Assert.AreEqual ("'\x15'", Generate (new CodePrimitiveExpression ('\x15'), sw), "#21");
				sb.Length = 0;
				Assert.AreEqual ("'\x16'", Generate (new CodePrimitiveExpression ('\x16'), sw), "#22");
				sb.Length = 0;
				Assert.AreEqual ("'\x17'", Generate (new CodePrimitiveExpression ('\x17'), sw), "#23");
				sb.Length = 0;
				Assert.AreEqual ("'\x18'", Generate (new CodePrimitiveExpression ('\x18'), sw), "#24");
				sb.Length = 0;
				Assert.AreEqual ("'\x19'", Generate (new CodePrimitiveExpression ('\x19'), sw), "#25");
				sb.Length = 0;
				Assert.AreEqual ("'\x1A'", Generate (new CodePrimitiveExpression ('\x1A'), sw), "#26");
				sb.Length = 0;
				Assert.AreEqual ("'\x1B'", Generate (new CodePrimitiveExpression ('\x1B'), sw), "#27");
				sb.Length = 0;
				Assert.AreEqual ("'\x1C'", Generate (new CodePrimitiveExpression ('\x1C'), sw), "#28");
				sb.Length = 0;
				Assert.AreEqual ("'\x1D'", Generate (new CodePrimitiveExpression ('\x1D'), sw), "#29");
				sb.Length = 0;
				Assert.AreEqual ("'\x1E'", Generate (new CodePrimitiveExpression ('\x1E'), sw), "#30");
				sb.Length = 0;
				Assert.AreEqual ("'\x1F'", Generate (new CodePrimitiveExpression ('\x1F'), sw), "#31");
				sb.Length = 0;
				Assert.AreEqual ("'\x20'", Generate (new CodePrimitiveExpression ('\x20'), sw), "#32");
				sb.Length = 0;
				Assert.AreEqual ("'\x21'", Generate (new CodePrimitiveExpression ('\x21'), sw), "#33");
				sb.Length = 0;
				Assert.AreEqual ("'\\\"'", Generate (new CodePrimitiveExpression ('"'), sw), "#34");
				sb.Length = 0;
				Assert.AreEqual ("'\x23'", Generate (new CodePrimitiveExpression ('\x23'), sw), "#35");
				sb.Length = 0;
				Assert.AreEqual ("'\x24'", Generate (new CodePrimitiveExpression ('\x24'), sw), "#36");
				sb.Length = 0;
				Assert.AreEqual ("'\x25'", Generate (new CodePrimitiveExpression ('\x25'), sw), "#37");
				sb.Length = 0;
				Assert.AreEqual ("'\x26'", Generate (new CodePrimitiveExpression ('\x26'), sw), "#38");
				sb.Length = 0;
				Assert.AreEqual ("'\\''", Generate (new CodePrimitiveExpression ('\''), sw), "#39");
				sb.Length = 0;
				Assert.AreEqual ("'\\u2028'", Generate (new CodePrimitiveExpression ('\u2028'), sw), "#40");
				sb.Length = 0;
				Assert.AreEqual ("'\\u2029'", Generate (new CodePrimitiveExpression ('\u2029'), sw), "#41");
				sb.Length = 0;
				Assert.AreEqual ("'\u2030'", Generate (new CodePrimitiveExpression ('\u2030'), sw), "#42");
				sw.Close ();
			}
		}

		[Test]
		public void PrimitiveExpressionTest_SByte ()
		{
			StringBuilder sb = new StringBuilder ();

			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual ("5", Generate (new CodePrimitiveExpression ((sbyte) 5), sw));
				sw.Close ();
			}
		}

		[Test]
		public void PrimitiveExpressionTest_UInt16 ()
		{
			StringBuilder sb = new StringBuilder ();

			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual ("5", Generate (new CodePrimitiveExpression ((ushort) 5), sw));
				sw.Close ();
			}
		}

		[Test]
		public void PrimitiveExpressionTest_UInt32 ()
		{
			StringBuilder sb = new StringBuilder ();

			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual ("5u", Generate (new CodePrimitiveExpression ((uint) 5), sw));
				sw.Close ();
			}
		}

		[Test]
		public void PrimitiveExpressionTest_UInt64 ()
		{
			StringBuilder sb = new StringBuilder ();

			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual ("5ul", Generate (new CodePrimitiveExpression ((ulong) 5), sw));
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

		[Test]
		public void ArrayCreateExpressionTest ()
		{
			StringBuilder sb;

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual (
					string.Format (CultureInfo.InvariantCulture,
						"new int[] {{{0}        5}}",
						Environment.NewLine),
					Generate (new CodeArrayCreateExpression (
							typeof (int),
							new CodeExpression [] {
								new CodePrimitiveExpression (5)
								})
						, sw), "#1");
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual ("new int[5]",
					Generate (new CodeArrayCreateExpression (
							typeof (int),
							new CodePrimitiveExpression (5))
						, sw), "#2");
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual (
					string.Format (CultureInfo.InvariantCulture,
						"new string[] {{{0}" +
						"        \"a\",{0}" +
						"        \"b\",{0}" +
						"        \"c\"}}",
						Environment.NewLine),
					Generate (new CodeArrayCreateExpression (
							typeof (string),
							new CodeExpression [] {
								new CodePrimitiveExpression ("a"),
								new CodePrimitiveExpression ("b"),
								new CodePrimitiveExpression ("c"),
								})
						, sw));
				sw.Close ();
			}
		}

		[Test]
		public void EscapedIdentifierTest ()
		{
			StringBuilder sb;
			string code;

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeVariableReferenceExpression ("using"), sw);
				Assert.AreEqual ("@using", code, "#1");
				sw.Close ();
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeTypeReferenceExpression ("using"), sw);
				Assert.AreEqual ("@using", code, "#2");
				sw.Close ();
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodePropertyReferenceExpression (null, "using"), sw);
				Assert.AreEqual ("@using", code, "#3");
				sw.Close ();
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeMethodReferenceExpression (null, "using"), sw);
				Assert.AreEqual ("@using", code, "#4");
				sw.Close ();
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeFieldReferenceExpression (null, "using"), sw);
				Assert.AreEqual ("@using", code, "#5");
				sw.Close ();
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeEventReferenceExpression (null, "using"), sw);
				Assert.AreEqual ("@using", code, "#6");
				sw.Close ();
			}
		}

		[Test]
		public void EventReferenceTest ()
		{
			StringBuilder sb;
			string code;

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeEventReferenceExpression (null, null), sw);
				Assert.AreEqual (string.Empty, code, "#1");
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeEventReferenceExpression (null, "abc"), sw);
				Assert.AreEqual ("abc", code, "#2");
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeEventReferenceExpression (new CodeThisReferenceExpression (), null), sw);
				Assert.AreEqual ("this.", code, "#3");
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeEventReferenceExpression (new CodeThisReferenceExpression (), "abc"), sw);
				Assert.AreEqual ("this.abc", code, "#4");
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeEventReferenceExpression (new CodePrimitiveExpression ("primitive"), null), sw);
				Assert.AreEqual ("\"primitive\".", code, "#5");
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeEventReferenceExpression (new CodePrimitiveExpression ("primitive"), "abc"), sw);
				Assert.AreEqual ("\"primitive\".abc", code, "#6");
			}
		}

		[Test]
		public void DefaultValueExpressionTest ()
		{
			StringBuilder sb = new StringBuilder ();

			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual ("default(string)", Generate (new CodeDefaultValueExpression (new CodeTypeReference (typeof(string))), sw), "#0");
				sb.Length = 0;
				sw.Close ();
			}
		}

		[Test]
		public void DelegateInvokeTest ()
		{
			StringBuilder sb;
			string code;

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeDelegateInvokeExpression (null, new CodePrimitiveExpression ("abc")), sw);
				Assert.AreEqual ("(\"abc\")", code, "#1");
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeDelegateInvokeExpression (new CodeThisReferenceExpression (), new CodePrimitiveExpression ("abc")), sw);
				Assert.AreEqual ("this(\"abc\")", code, "#2");
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeDelegateInvokeExpression (new CodePrimitiveExpression ("primitive"), new CodePrimitiveExpression ("abc")), sw);
				Assert.AreEqual ("\"primitive\"(\"abc\")", code, "#3");
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeDelegateInvokeExpression (new CodeEventReferenceExpression (new CodeThisReferenceExpression (), "Click"), new CodePrimitiveExpression ("abc")), sw);
				Assert.AreEqual ("this.Click(\"abc\")", code, "#4");
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeDelegateInvokeExpression (new CodeEventReferenceExpression (new CodeThisReferenceExpression (), null), new CodePrimitiveExpression ("abc")), sw);
				Assert.AreEqual ("this.(\"abc\")", code, "#5");
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeDelegateInvokeExpression (new CodeEventReferenceExpression (new CodePrimitiveExpression ("primitive"), "Click"), new CodePrimitiveExpression ("abc")), sw);
				Assert.AreEqual ("\"primitive\".Click(\"abc\")", code, "#6");
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeDelegateInvokeExpression (new CodeEventReferenceExpression (new CodePrimitiveExpression ("primitive"), null), new CodePrimitiveExpression ("abc")), sw);
				Assert.AreEqual ("\"primitive\".(\"abc\")", code, "#7");
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeDelegateInvokeExpression (), sw);
				Assert.AreEqual ("()", code, "#8");
			}
		}

		private string Generate (CodeExpression expression, StringWriter sw)
		{
			generator.GenerateCodeFromExpression (expression, sw, options);
			return sw.ToString ();
		}
	}
}
