//
// Microsoft.VisualBasic.* Test Cases
//
// Authors:
// 	Frederik Carlier  <frederik.carlier@carlier-online.be>
//	Rolf Bjarne Kvinge <RKvinge@novell.com>
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
	public class CodeGeneratorFromBinaryOperatorTest
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
		public void TypeReferenceExpressionTest ()
		{
			StringBuilder sb = new StringBuilder();

			using (StringWriter sw = new StringWriter (sb)) {
				CodeThisReferenceExpression thisRef = new CodeThisReferenceExpression();
				CodeFieldReferenceExpression parentField = new CodeFieldReferenceExpression();
				parentField.TargetObject = thisRef;
				parentField.FieldName = "Parent";

				CodeBinaryOperatorExpression expression = new CodeBinaryOperatorExpression(
					parentField,
					CodeBinaryOperatorType.IdentityInequality,
					new CodePrimitiveExpression(null));

				Assert.AreEqual ("(Not (Me.Parent) Is Nothing)", Generate (expression, sw), "#1");
				sw.Close ();
			}

			sb = new StringBuilder();
			using (StringWriter sw = new StringWriter (sb)) {
				CodeThisReferenceExpression thisRef = new CodeThisReferenceExpression();
				CodeFieldReferenceExpression parentField = new CodeFieldReferenceExpression();
				parentField.TargetObject = thisRef;
				parentField.FieldName = "Parent";

				CodeBinaryOperatorExpression expression = new CodeBinaryOperatorExpression(
					new CodePrimitiveExpression(null),
					CodeBinaryOperatorType.IdentityInequality,
					parentField);

				Assert.AreEqual ("(Not (Me.Parent) Is Nothing)", Generate (expression, sw), "#2");
				sw.Close ();
			}
		}

		private string Generate (CodeExpression expression, StringWriter sw)
		{
			generator.GenerateCodeFromExpression (expression, sw, options);
			return sw.ToString ();
		}
	}
}
