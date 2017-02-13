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
				Assert.AreEqual ("SByte", Generate (new CodeTypeReferenceExpression (typeof (sbyte)), sw), "#15");
				sb.Length = 0;
				Assert.AreEqual ("UShort", Generate (new CodeTypeReferenceExpression (typeof (ushort)), sw), "#16");
				sb.Length = 0;
				Assert.AreEqual ("UInteger", Generate (new CodeTypeReferenceExpression (typeof (uint)), sw), "#17");
				sb.Length = 0;
				Assert.AreEqual ("ULong", Generate (new CodeTypeReferenceExpression (typeof (ulong)), sw), "#18");
				sb.Length = 0;
				sw.Close ();
			}
		}

		[Test]
		public void PrimitiveExpressionTest ()
		{
			StringBuilder sb = new StringBuilder ();

			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual ("Nothing", Generate (new CodePrimitiveExpression (null), sw), "#1");
				sb.Length = 0;
				Assert.AreEqual ("\"AB\"\"C\"", Generate (new CodePrimitiveExpression ("AB\"C"), sw), "#2");
				sb.Length = 0;
				Assert.AreEqual ("5", Generate (new CodePrimitiveExpression ((byte) 5), sw), "#4");
				sb.Length = 0;
				Assert.AreEqual ("20", Generate (new CodePrimitiveExpression ((short) 20), sw), "#5");
				sb.Length = 0;
				Assert.AreEqual ("243", Generate (new CodePrimitiveExpression (243), sw), "#6");
				sb.Length = 0;
				Assert.AreEqual ("434343", Generate (new CodePrimitiveExpression ((long) 434343), sw), "#7");
				sb.Length = 0;
				Assert.AreEqual ("6.445!", Generate (new CodePrimitiveExpression ((float) 6.445), sw), "#8");
				sb.Length = 0;
				Assert.AreEqual ("5.76R", Generate (new CodePrimitiveExpression ((double) 5.76), sw), "#9");
				sb.Length = 0;
				Assert.AreEqual ("7.667D", Generate (new CodePrimitiveExpression ((decimal) 7.667), sw), "#10");
				sb.Length = 0;
				Assert.AreEqual ("true", Generate (new CodePrimitiveExpression (true), sw), "#11");
				sb.Length = 0;
				Assert.AreEqual ("false", Generate (new CodePrimitiveExpression (false), sw), "#12");
				sw.Close ();
			}
		}

		[Test]
		public void ArrayIndexerExpressionTest ()
		{
			StringBuilder sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual ("x(5)", Generate (new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("x"), new CodePrimitiveExpression(5)), sw), "#1");
				sb.Length = 0;
			}
		}
	
		[Test]
		public void PrimitiveExpressionTest_Char ()
		{
			string vbNs = "Global.Microsoft.VisualBasic";
			StringBuilder sb = new StringBuilder ();

			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual (vbNs + ".ChrW(0)", Generate (new CodePrimitiveExpression ('\0'), sw), "#0");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(1)", Generate (new CodePrimitiveExpression ('\x01'), sw), "#1");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(2)", Generate (new CodePrimitiveExpression ('\x02'), sw), "#2");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(3)", Generate (new CodePrimitiveExpression ('\x03'), sw), "#3");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(4)", Generate (new CodePrimitiveExpression ('\x04'), sw), "#4");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(5)", Generate (new CodePrimitiveExpression ('\x05'), sw), "#5");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(6)", Generate (new CodePrimitiveExpression ('\x06'), sw), "#6");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(7)", Generate (new CodePrimitiveExpression ('\a'), sw), "#7");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(8)", Generate (new CodePrimitiveExpression ('\b'), sw), "#8");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(9)", Generate (new CodePrimitiveExpression ('\t'), sw), "#9");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(10)", Generate (new CodePrimitiveExpression ('\n'), sw), "#10");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(11)", Generate (new CodePrimitiveExpression ('\v'), sw), "#11");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(12)", Generate (new CodePrimitiveExpression ('\f'), sw), "#12");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(13)", Generate (new CodePrimitiveExpression ('\r'), sw), "#13");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(14)", Generate (new CodePrimitiveExpression ('\x0E'), sw), "#14");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(15)", Generate (new CodePrimitiveExpression ('\x0F'), sw), "#15");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(16)", Generate (new CodePrimitiveExpression ('\x10'), sw), "#16");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(17)", Generate (new CodePrimitiveExpression ('\x11'), sw), "#17");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(18)", Generate (new CodePrimitiveExpression ('\x12'), sw), "#18");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(19)", Generate (new CodePrimitiveExpression ('\x13'), sw), "#19");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(20)", Generate (new CodePrimitiveExpression ('\x14'), sw), "#20");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(21)", Generate (new CodePrimitiveExpression ('\x15'), sw), "#21");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(22)", Generate (new CodePrimitiveExpression ('\x16'), sw), "#22");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(23)", Generate (new CodePrimitiveExpression ('\x17'), sw), "#23");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(24)", Generate (new CodePrimitiveExpression ('\x18'), sw), "#24");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(25)", Generate (new CodePrimitiveExpression ('\x19'), sw), "#25");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(26)", Generate (new CodePrimitiveExpression ('\x1A'), sw), "#26");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(27)", Generate (new CodePrimitiveExpression ('\x1B'), sw), "#27");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(28)", Generate (new CodePrimitiveExpression ('\x1C'), sw), "#28");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(29)", Generate (new CodePrimitiveExpression ('\x1D'), sw), "#29");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(30)", Generate (new CodePrimitiveExpression ('\x1E'), sw), "#30");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(31)", Generate (new CodePrimitiveExpression ('\x1F'), sw), "#31");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(32)", Generate (new CodePrimitiveExpression ('\x20'), sw), "#32");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(33)", Generate (new CodePrimitiveExpression ('\x21'), sw), "#33");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(34)", Generate (new CodePrimitiveExpression ('"'), sw), "#34");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(35)", Generate (new CodePrimitiveExpression ('\x23'), sw), "#35");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(36)", Generate (new CodePrimitiveExpression ('\x24'), sw), "#36");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(37)", Generate (new CodePrimitiveExpression ('\x25'), sw), "#37");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(38)", Generate (new CodePrimitiveExpression ('\x26'), sw), "#38");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(39)", Generate (new CodePrimitiveExpression ('\''), sw), "#39");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(8232)", Generate (new CodePrimitiveExpression ('\u2028'), sw), "#40");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(8233)", Generate (new CodePrimitiveExpression ('\u2029'), sw), "#41");
				sb.Length = 0;
				Assert.AreEqual (vbNs + ".ChrW(8240)", Generate (new CodePrimitiveExpression ('\u2030'), sw), "#42");
				sw.Close ();
			}
		}

		[Test]
		public void PrimitiveExpressionTest_SByte ()
		{
			StringBuilder sb = new StringBuilder ();

			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual ("CSByte(5)", Generate (new CodePrimitiveExpression ((sbyte) 5), sw));
				sw.Close ();
			}
		}

		[Test]
		public void PrimitiveExpressionTest_UInt16 ()
		{
			StringBuilder sb = new StringBuilder ();

			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual ("5US", Generate (new CodePrimitiveExpression ((ushort) 5), sw));
				sw.Close ();
			}
		}

		[Test]
		public void PrimitiveExpressionTest_UInt32 ()
		{
			StringBuilder sb = new StringBuilder ();

			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual ("5UI", Generate (new CodePrimitiveExpression ((uint) 5), sw));
				sw.Close ();
			}
		}

		[Test]
		public void PrimitiveExpressionTest_UInt64 ()
		{
			StringBuilder sb = new StringBuilder ();

			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual ("5UL", Generate (new CodePrimitiveExpression ((ulong) 5), sw));
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
				Assert.AreEqual ("ByVal __exception As System.Void", Generate (cpde, sw), "#1");
				sb.Length = 0;

				cpde = new CodeParameterDeclarationExpression ((string) null,
					(string) null);
				Assert.AreEqual ("ByVal __exception As System.Void", Generate (cpde, sw), "#2");
				sb.Length = 0;

				cpde = new CodeParameterDeclarationExpression ("A", (string) null);
				Assert.AreEqual ("ByVal __exception As A", Generate (cpde, sw), "#3");
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
	
		
		[Test]
		public void ArrayCreateExpressionTest ()
		{
			StringBuilder sb;

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual ("New Integer() {5}", 
					Generate (new CodeArrayCreateExpression(
							typeof(int), 
							new CodeExpression [] {
								new CodePrimitiveExpression (5)
								})
						, sw), "#1");
				sw.Close ();
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual ("New Integer((5) - 1) {}", 
					Generate (new CodeArrayCreateExpression(
							typeof(int), 
							new CodePrimitiveExpression (5))
						, sw), "#2");
				sw.Close ();
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				Assert.AreEqual ("New String() {\"a\", \"b\", \"c\"}", 
					Generate (new CodeArrayCreateExpression(
							typeof(string), 
							new CodeExpression [] {
								new CodePrimitiveExpression ("a"),
								new CodePrimitiveExpression ("b"),
								new CodePrimitiveExpression ("c"),
								})
						, sw), "#3");
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
				code = Generate (new CodeVariableReferenceExpression ("set"), sw); 
				Assert.AreEqual ("[set]", code, "#01");
				sw.Close ();
			}
			
			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeTypeReferenceExpression ("set"), sw); 
				Assert.AreEqual ("[set]", code, "#02");
				sw.Close ();
			}
			
			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodePropertyReferenceExpression (null, "set"), sw); 
				Assert.AreEqual ("[set]", code, "#03");
				sw.Close ();
			}
			
			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeMethodReferenceExpression (null, "set"), sw); 
				Assert.AreEqual ("[set]", code, "#04");
				sw.Close ();
			}
			
			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeFieldReferenceExpression (null, "set"), sw); 
				Assert.AreEqual ("[set]", code, "#05");
				sw.Close ();
			}
			
			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeEventReferenceExpression (null, "set"), sw); 
				Assert.AreEqual ("setEvent", code, "#06");
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
				Assert.AreEqual ("[Event]", code, "#01");
			}
			
			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeEventReferenceExpression (null, "abc"), sw);
				Assert.AreEqual ("abcEvent", code, "#02");
			}
			
			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeEventReferenceExpression (new CodeThisReferenceExpression (), null), sw);
				Assert.AreEqual ("Me.Event", code, "#03");
			}
			
			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeEventReferenceExpression (new CodeThisReferenceExpression (), "abc"), sw);
				Assert.AreEqual ("Me.abcEvent", code, "#04");
			}
			
			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeEventReferenceExpression (new CodePrimitiveExpression ("primitive"), null), sw);
				Assert.AreEqual ("\"primitive\".", code, "#05");
			}
			
			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeEventReferenceExpression (new CodePrimitiveExpression ("primitive"), "abc"), sw);
				Assert.AreEqual ("\"primitive\".abc", code, "#06");
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
				Assert.AreEqual ("(\"abc\")", code, "#01");
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeDelegateInvokeExpression (new CodeThisReferenceExpression (), new CodePrimitiveExpression ("abc")), sw);
				Assert.AreEqual ("Me(\"abc\")", code, "#02");
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeDelegateInvokeExpression (new CodePrimitiveExpression ("primitive"), new CodePrimitiveExpression ("abc")), sw);
				Assert.AreEqual ("\"primitive\"(\"abc\")", code, "#03");
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeDelegateInvokeExpression (new CodeEventReferenceExpression (new CodeThisReferenceExpression (), "Click"), new CodePrimitiveExpression ("abc")), sw);
				Assert.AreEqual ("RaiseEvent Click(\"abc\")", code, "#04");
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeDelegateInvokeExpression (new CodeEventReferenceExpression (new CodeThisReferenceExpression (), null), new CodePrimitiveExpression ("abc")), sw);
				Assert.AreEqual ("RaiseEvent (\"abc\")", code, "#05");
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeDelegateInvokeExpression (new CodeEventReferenceExpression (new CodePrimitiveExpression ("primitive"), "Click"), new CodePrimitiveExpression ("abc")), sw);
				Assert.AreEqual ("RaiseEvent \"primitive\".Click(\"abc\")", code, "#06");
			}

			sb = new StringBuilder ();
			using (StringWriter sw = new StringWriter (sb)) {
				code = Generate (new CodeDelegateInvokeExpression (new CodeEventReferenceExpression (new CodePrimitiveExpression ("primitive"), null), new CodePrimitiveExpression ("abc")), sw);
				Assert.AreEqual ("RaiseEvent \"primitive\".(\"abc\")", code, "#07");
			}
		}
	}
}
