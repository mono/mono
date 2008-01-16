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

using NUnit.Framework;

namespace MonoTests.Microsoft.CSharp
{
	/// <summary>
	/// Test ICodeGenerator's GenerateCodeFromNamespace, along with a 
	/// minimal set CodeDom components.
	/// </summary>
	[TestFixture]
	public class CodeGeneratorFromNamespaceTest : CodeGeneratorTestBase
	{
		CodeNamespace codeNamespace;

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
			string code = null;

			codeNamespace.Name = "A";
			code = Generate ();
			Assert.AreEqual ("namespace A {\n    \n}\n", code, "#1");

			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";
			code = Generate (options);
			Assert.AreEqual ("namespace A\n{\n    \n}\n", code, "#2");
		}

		[Test]
		public void InvalidNamespaceTest ()
		{
			codeNamespace.Name = "A,B";
			Assert.AreEqual ("namespace A,B {\n    \n}\n", Generate ());
		}

		[Test]
		public void CommentOnlyNamespaceTest ()
		{
			CodeCommentStatement comment = new CodeCommentStatement ("a");
			codeNamespace.Comments.Add (comment);
			Assert.AreEqual ("// a\n\n", Generate ());
		}

		[Test]
		public void ImportsTest ()
		{
			codeNamespace.Imports.Add (new CodeNamespaceImport ("System"));
			codeNamespace.Imports.Add (new CodeNamespaceImport ("System.Collections"));

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"using System;{0}" +
				"using System.Collections;{0}" +
				"{0}", NewLine), Generate (), "#1");

			codeNamespace.Name = "A";

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace A {{{0}" +
				"    using System;{0}" +
				"    using System.Collections;{0}" +
				"    {0}" +
				"}}{0}", NewLine), Generate (), "#2");

			codeNamespace.Name = null;
			codeNamespace.Comments.Add (new CodeCommentStatement ("a"));

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"// a{0}" +
				"using System;{0}" +
				"using System.Collections;{0}" +
				"{0}", NewLine), Generate (), "#3");

			codeNamespace.Name = "A";

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"// a{0}" +
				"namespace A {{{0}" +
				"    using System;{0}" +
				"    using System.Collections;{0}" +
				"    {0}" +
				"}}{0}", NewLine), Generate (), "#4");
		}

		[Test]
		public void TypeTest ()
		{
			codeNamespace.Types.Add (new CodeTypeDeclaration ("Person"));
			Assert.AreEqual (string.Format(CultureInfo.InvariantCulture,
				"{0}" +
				"{0}" +
				"public class Person {{{0}" +
				"}}{0}", NewLine), Generate (), "#A1");

			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BlankLinesBetweenMembers = false;
			Assert.AreEqual (string.Format(CultureInfo.InvariantCulture,
				"{0}" +
				"public class Person {{{0}" +
				"}}{0}", NewLine), Generate (options), "#A2");

			codeNamespace.Name = "A";
			Assert.AreEqual (string.Format(CultureInfo.InvariantCulture,
				"namespace A {{{0}" +
				"    {0}" +
				"    {0}" +
				"    public class Person {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate (), "#B1");

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace A {{{0}" +
				"    {0}" +
				"    public class Person {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate (options), "#B2");
		}

#if NET_2_0
		[Test]
		public void Type_TypeParameters ()
		{
			codeNamespace.Name = "SomeNS";

			CodeTypeDeclaration type = new CodeTypeDeclaration ("SomeClass");
			codeNamespace.Types.Add (type);
			type.TypeParameters.Add (new CodeTypeParameter ("T"));

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace SomeNS {{{0}" +
				"    {0}" +
				"    {0}" +
				"    public class SomeClass<T>{0}" +
				"     {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate (), "#1");

			type.TypeParameters.Add (new CodeTypeParameter ("as"));
			type.TypeParameters.Add (new CodeTypeParameter ("is"));

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace SomeNS {{{0}" +
				"    {0}" +
				"    {0}" +
				"    public class SomeClass<T, as, is>{0}" +
				"    {0}" +
				"    {0}" +
				"     {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate (), "#2");

			CodeTypeParameter typeParamR = new CodeTypeParameter ("R");
			typeParamR.Constraints.Add (new CodeTypeReference (typeof (IComparable)));
			type.TypeParameters.Add (typeParamR);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace SomeNS {{{0}" +
				"    {0}" +
				"    {0}" +
				"    public class SomeClass<T, as, is, R>{0}" +
				"    {0}" +
				"    {0}" +
				"    {0}" +
				"        where R : System.IComparable {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate (), "#3");

			type.TypeParameters.Add (new CodeTypeParameter ("S"));

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace SomeNS {{{0}" +
				"    {0}" +
				"    {0}" +
				"    public class SomeClass<T, as, is, R, S>{0}" +
				"    {0}" +
				"    {0}" +
				"    {0}" +
				"        where R : System.IComparable{0}" +
				"     {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate (), "#4");
		}


		[Test]
		public void Type_TypeParameters_Constraints ()
		{
			codeNamespace.Name = "SomeNS";

			CodeTypeDeclaration type = new CodeTypeDeclaration ("SomeClass");
			codeNamespace.Types.Add (type);

			CodeTypeParameter typeParamT = new CodeTypeParameter ("T");
			typeParamT.Constraints.Add (new CodeTypeReference (typeof (IComparable)));
			type.TypeParameters.Add (typeParamT);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace SomeNS {{{0}" +
				"    {0}" +
				"    {0}" +
				"    public class SomeClass<T>{0}" +
				"        where T : System.IComparable {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate (), "#1");

			typeParamT.Constraints.Add (new CodeTypeReference (typeof (ICloneable)));

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace SomeNS {{{0}" +
				"    {0}" +
				"    {0}" +
				"    public class SomeClass<T>{0}" +
				"        where T : System.IComparable, System.ICloneable {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate (), "#2");

			typeParamT.HasConstructorConstraint = true;

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace SomeNS {{{0}" +
				"    {0}" +
				"    {0}" +
				"    public class SomeClass<T>{0}" +
				"        where T : System.IComparable, System.ICloneable, new () {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate (), "#3");

			CodeTypeParameter typeParamS = new CodeTypeParameter ("S");
			typeParamS.Constraints.Add (new CodeTypeReference (typeof (IDisposable)));
			type.TypeParameters.Add (typeParamS);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace SomeNS {{{0}" +
				"    {0}" +
				"    {0}" +
				"    public class SomeClass<T, S>{0}" +
				"        where T : System.IComparable, System.ICloneable, new (){0}" +
				"        where S : System.IDisposable {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate (), "#4");

			CodeTypeParameter typeParamR = new CodeTypeParameter ("R");
			typeParamR.HasConstructorConstraint = true;
			type.TypeParameters.Add (typeParamR);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace SomeNS {{{0}" +
				"    {0}" +
				"    {0}" +
				"    public class SomeClass<T, S, R>{0}" +
				"        where T : System.IComparable, System.ICloneable, new (){0}" +
				"        where S : System.IDisposable{0}" +
				"        where R : new() {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate (), "#5");
		}

		[Test]
		public void Type_TypeParameters_ConstructorConstraint ()
		{
			codeNamespace.Name = "SomeNS";

			CodeTypeDeclaration type = new CodeTypeDeclaration ("SomeClass");
			codeNamespace.Types.Add (type);

			CodeTypeParameter typeParam = new CodeTypeParameter ("T");
			typeParam.HasConstructorConstraint = true;
			type.TypeParameters.Add (typeParam);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace SomeNS {{{0}" +
				"    {0}" +
				"    {0}" +
				"    public class SomeClass<T>{0}" +
				"        where T : new() {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate ());
		}

		[Test]
		public void Method_TypeParameters ()
		{
			codeNamespace.Name = "SomeNS";

			CodeTypeDeclaration type = new CodeTypeDeclaration ("SomeClass");
			codeNamespace.Types.Add (type);

			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = "SomeMethod";
			type.Members.Add (method);

			method.TypeParameters.Add (new CodeTypeParameter ("T"));

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace SomeNS {{{0}" +
				"    {0}" +
				"    {0}" +
				"    public class SomeClass {{{0}" +
				"        {0}" +
				"        private void SomeMethod<T>(){0}" +
				"         {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate (), "#1");

			method.TypeParameters.Add (new CodeTypeParameter ("as"));
			method.TypeParameters.Add (new CodeTypeParameter ("is"));

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace SomeNS {{{0}" +
				"    {0}" +
				"    {0}" +
				"    public class SomeClass {{{0}" +
				"        {0}" +
				"        private void SomeMethod<T, as, is>(){0}" +
				"        {0}" +
				"        {0}" +
				"         {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate (), "#2");

			CodeTypeParameter typeParamR = new CodeTypeParameter ("R");
			typeParamR.Constraints.Add (new CodeTypeReference (typeof (IComparable)));
			method.TypeParameters.Add (typeParamR);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace SomeNS {{{0}" +
				"    {0}" +
				"    {0}" +
				"    public class SomeClass {{{0}" +
				"        {0}" +
				"        private void SomeMethod<T, as, is, R>(){0}" +
				"        {0}" +
				"        {0}" +
				"        {0}" +
				"            where R : System.IComparable {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate (), "#3");

			method.TypeParameters.Add (new CodeTypeParameter ("S"));

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace SomeNS {{{0}" +
				"    {0}" +
				"    {0}" +
				"    public class SomeClass {{{0}" +
				"        {0}" +
				"        private void SomeMethod<T, as, is, R, S>(){0}" +
				"        {0}" +
				"        {0}" +
				"        {0}" +
				"            where R : System.IComparable{0}" +
				"         {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate (), "#4");
		}

		[Test]
		public void Method_TypeParameters_Constraints ()
		{
			codeNamespace.Name = "SomeNS";

			CodeTypeDeclaration type = new CodeTypeDeclaration ("SomeClass");
			codeNamespace.Types.Add (type);

			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = "SomeMethod";
			type.Members.Add (method);

			CodeTypeParameter typeParamT = new CodeTypeParameter ("T");
			typeParamT.Constraints.Add (new CodeTypeReference (typeof (IComparable)));
			method.TypeParameters.Add (typeParamT);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace SomeNS {{{0}" +
				"    {0}" +
				"    {0}" +
				"    public class SomeClass {{{0}" +
				"        {0}" +
				"        private void SomeMethod<T>(){0}" +
				"            where T : System.IComparable {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate (), "#1");

			typeParamT.Constraints.Add (new CodeTypeReference (typeof (ICloneable)));

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace SomeNS {{{0}" +
				"    {0}" +
				"    {0}" +
				"    public class SomeClass {{{0}" +
				"        {0}" +
				"        private void SomeMethod<T>(){0}" +
				"            where T : System.IComparable, System.ICloneable {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate (), "#2");

			typeParamT.HasConstructorConstraint = true;

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace SomeNS {{{0}" +
				"    {0}" +
				"    {0}" +
				"    public class SomeClass {{{0}" +
				"        {0}" +
				"        private void SomeMethod<T>(){0}" +
				"            where T : System.IComparable, System.ICloneable, new () {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate (), "#3");

			CodeTypeParameter typeParamS = new CodeTypeParameter ("S");
			typeParamS.Constraints.Add (new CodeTypeReference (typeof (IDisposable)));
			method.TypeParameters.Add (typeParamS);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace SomeNS {{{0}" +
				"    {0}" +
				"    {0}" +
				"    public class SomeClass {{{0}" +
				"        {0}" +
				"        private void SomeMethod<T, S>(){0}" +
				"            where T : System.IComparable, System.ICloneable, new (){0}" +
				"            where S : System.IDisposable {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate (), "#4");

			CodeTypeParameter typeParamR = new CodeTypeParameter ("R");
			typeParamR.HasConstructorConstraint = true;
			method.TypeParameters.Add (typeParamR);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace SomeNS {{{0}" +
				"    {0}" +
				"    {0}" +
				"    public class SomeClass {{{0}" +
				"        {0}" +
				"        private void SomeMethod<T, S, R>(){0}" +
				"            where T : System.IComparable, System.ICloneable, new (){0}" +
				"            where S : System.IDisposable{0}" +
				"            where R : new() {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate (), "#5");
		}

		[Test] // bug #353921
		public void Method_TypeParameters_ConstructorConstraint ()
		{
			codeNamespace.Name = "SomeNS";

			CodeTypeDeclaration type = new CodeTypeDeclaration ("SomeClass");
			codeNamespace.Types.Add (type);

			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = "SomeMethod";
			type.Members.Add (method);

			CodeTypeParameter typeParam = new CodeTypeParameter ("T");
			typeParam.HasConstructorConstraint = true;
			method.TypeParameters.Add (typeParam);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"namespace SomeNS {{{0}" +
				"    {0}" +
				"    {0}" +
				"    public class SomeClass {{{0}" +
				"        {0}" +
				"        private void SomeMethod<T>(){0}" +
				"            where T : new() {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), Generate ());
		}
#endif
	}
}
