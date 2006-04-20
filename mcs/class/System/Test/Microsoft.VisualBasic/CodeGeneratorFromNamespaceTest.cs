//
// Microsoft.VisualBasic.* Test Cases
//
// Authors:
// Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) 2006 Novell
//
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Text;

using NUnit.Framework;

namespace MonoTests.Microsoft.VisualBasic
{
	/// <summary>
	/// Test ICodeGenerator's GenerateCodeFromNamespace, along with a 
	/// minimal set CodeDom components.
	/// </summary>
	[TestFixture]
	public class CodeGeneratorFromNamespaceTest : CodeGeneratorTestBase
	{
		CodeNamespace codeNamespace = null;

		[SetUp]
		public void Init ()
		{
			InitBase ();
			codeNamespace = new CodeNamespace ();
		}
		
		protected override string Generate (CodeGeneratorOptions options)
		{
			StringWriter writer = new StringWriter ();
			writer.NewLine = NewLine;

			generator.GenerateCodeFromNamespace (codeNamespace, writer, options);
			writer.Close ();
			return writer.ToString ();
		}
		
		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void NullNamespaceTest ()
		{
			codeNamespace = null;
			Generate ();
		}

		[Test]
		public void NullNamespaceNameTest ()
		{
			codeNamespace.Name = null;
			Assert.AreEqual ("\n", Generate ());
		}

		
		[Test]
		public void DefaultNamespaceTest ()
		{
			Assert.AreEqual ("\n", Generate ());
		}

		[Test]
		public void SimpleNamespaceTest ()
		{
			codeNamespace.Name = "A";
			Assert.AreEqual ("\nNamespace A\nEnd Namespace\n", Generate ());
		}

		[Test]
		public void InvalidNamespaceTest ()
		{
			codeNamespace.Name = "A,B";
			Assert.AreEqual ("\nNamespace A,B\nEnd Namespace\n", Generate ());
		}

		[Test]
		public void CommentOnlyNamespaceTest ()
		{
			CodeCommentStatement comment = new CodeCommentStatement ("a");
			codeNamespace.Comments.Add (comment);
			Assert.AreEqual ("\n'a\n", Generate ());
		}

		[Test]
		public void ImportsTest ()
		{
			codeNamespace.Imports.Add (new CodeNamespaceImport ("System"));
			codeNamespace.Imports.Add (new CodeNamespaceImport ("System.Collections"));

			Assert.AreEqual (string.Format(CultureInfo.InvariantCulture,
				"Imports System{0}" +
				"Imports System.Collections{0}" +
				"{0}", NewLine), Generate (), "#1");

			codeNamespace.Name = "A";

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Imports System{0}" +
				"Imports System.Collections{0}" +
				"{0}" +
				"Namespace A{0}" +
				"End Namespace{0}", NewLine), Generate (), "#2");

			codeNamespace.Name = null;
			codeNamespace.Comments.Add (new CodeCommentStatement ("a"));

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Imports System{0}" +
				"Imports System.Collections{0}" +
				"{0}" +
				"'a{0}", NewLine), Generate (), "#3");

			codeNamespace.Name = "A";

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Imports System{0}" +
				"Imports System.Collections{0}" +
				"{0}" +
				"'a{0}" +
				"Namespace A{0}" +
				"End Namespace{0}", NewLine), Generate (), "#4");
		}

		[Test]
		public void TypeTest ()
		{
			codeNamespace.Types.Add (new CodeTypeDeclaration ("Person"));
			Assert.AreEqual (string.Format(CultureInfo.InvariantCulture,
				"{0}" +
				"{0}" +
				"Public Class Person{0}" +
				"End Class{0}", NewLine), Generate (), "#A1");

			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BlankLinesBetweenMembers = false;
			Assert.AreEqual (string.Format(CultureInfo.InvariantCulture,
				"{0}" +
				"Public Class Person{0}" +
				"End Class{0}", NewLine), Generate (options), "#A2");

			codeNamespace.Name = "A";
			Assert.AreEqual (string.Format(CultureInfo.InvariantCulture,
				"{0}" +
				"Namespace A{0}" +
				"    {0}" +
				"    Public Class Person{0}" +
				"    End Class{0}" +
				"End Namespace{0}", NewLine), Generate (), "#B1");

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"{0}" +
				"Namespace A{0}" +
				"    Public Class Person{0}" +
				"    End Class{0}" +
				"End Namespace{0}", NewLine), Generate (options), "#B2");
		}
	}
}
