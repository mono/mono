//
// Microsoft.CSharp.* Test Cases
//
// Authors:
// 	Erik LeBel (eriklebel@yahoo.ca)
//
// (c) 2003 Erik LeBel
//
using System;
using System.Globalization;
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
			Assert.AreEqual ("public class  {\n}\n", Code);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void NullTypeTest ()
		{
			type = null;
			Generate ();
		}

		[Test]
		public void SimpleTypeTest ()
		{
			type.Name = "Test1";
			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void AttributesAndTypeTest ()
		{
			type.Name = "Test1";

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			type.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			type.CustomAttributes.Add (attrDec);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"[A()]{0}"
				+ "[B()]{0}"
				+ "public class Test1 {{{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void EventMembersTypeTest1 ()
		{
			type.Name = "Test1";

			CodeMemberEvent evt = new CodeMemberEvent ();

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			evt.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			evt.CustomAttributes.Add (attrDec);

			type.Members.Add (evt);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    [A()]{0}"
				+ "    [B()]{0}"
				+ "    private event void ;{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void EventMembersTypeTest2 ()
		{
			type.Name = "Test1";

			CodeMemberEvent evt = new CodeMemberEvent ();
			evt.Name = "OnClick";
			evt.Attributes = MemberAttributes.Public;
			evt.Type = new CodeTypeReference (typeof (int));
			// C# does not support Implementation Types, so this should be ignored
			evt.ImplementationTypes.Add (new CodeTypeReference ("IPolicy"));
			type.Members.Add (evt);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    public event int OnClick;{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void FieldMembersTypeTest1 ()
		{
			type.Name = "Test1";

			CodeMemberField fld = new CodeMemberField ();

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			fld.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			fld.CustomAttributes.Add (attrDec);

			type.Members.Add (fld);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    [A()]{0}"
				+ "    [B()]{0}"
				+ "    private void ;{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void FieldMembersTypeTest2 ()
		{
			type.Name = "Test1";

			CodeMemberField fld = new CodeMemberField ();
			fld.Name = "Name";
			fld.Attributes = MemberAttributes.Public;
			fld.Type = new CodeTypeReference (typeof (int));
			type.Members.Add (fld);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    public int Name;{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void PropertyMembersTypeTest1 ()
		{
			type.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			property.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			property.CustomAttributes.Add (attrDec);

			type.Members.Add (property);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    [A()]{0}"
				+ "    [B()]{0}"
				+ "    private void  {{{0}"
				+ "    }}{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void PropertyMembersTypeTest2 ()
		{
			type.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = "Name";
			property.Attributes = MemberAttributes.Public;
			property.Type = new CodeTypeReference (typeof (int));
			// C# does not support Implementation Types, so this should be ignored
			property.ImplementationTypes.Add (new CodeTypeReference ("IPolicy"));
			type.Members.Add (property);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    public virtual int Name {{{0}"
				+ "    }}{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void PropertyMembersTypeGetOnly ()
		{
			type.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = "Name";
			property.Attributes = MemberAttributes.Family;
			property.HasGet = true;
			property.Type = new CodeTypeReference (typeof (int));
			type.Members.Add (property);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    protected virtual int Name {{{0}"
				+ "        get {{{0}"
				+ "        }}{0}"
				+ "    }}{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void PropertyMembersTypeSetOnly ()
		{
			type.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = "Name";
			property.Attributes = MemberAttributes.FamilyOrAssembly;
			property.HasSet = true;
			property.Type = new CodeTypeReference (typeof (int));
			type.Members.Add (property);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    protected internal int Name {{{0}"
				+ "        set {{{0}"
				+ "        }}{0}"
				+ "    }}{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void PropertyMembersTypeGetSet ()
		{
			type.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = "Name";
			property.Attributes = MemberAttributes.Assembly;
			property.HasGet = true;
			property.HasSet = true;
			property.Type = new CodeTypeReference (typeof (int));
			type.Members.Add (property);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
#if NET_2_0
				+ "    internal virtual int Name {{{0}"
#else
				+ "    internal int Name {{{0}"
#endif
				+ "        get {{{0}"
				+ "        }}{0}"
				+ "        set {{{0}"
				+ "        }}{0}"
				+ "    }}{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void PropertyMembersTypeFamilyAndAssembly ()
		{
			type.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = "Name";
			property.Attributes = MemberAttributes.FamilyAndAssembly;
			property.Type = new CodeTypeReference (typeof (int));

			type.Members.Add (property);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
#if NET_2_0
				+ "    internal int Name {{{0}"
#else
				+ "    /*FamANDAssem*/ internal int Name {{{0}"
#endif
				+ "    }}{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		/// <summary>
		/// C# CodeDOM does not output parameters for properties that aren't
		/// indexers.
		/// </summary>
		[Test]
		public void PropertyParametersTest ()
		{
			type.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = "Name";
			property.Attributes = MemberAttributes.Public;
			property.Type = new CodeTypeReference (typeof (int));

			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			property.Parameters.Add (param);

			param = new CodeParameterDeclarationExpression (
				typeof (int), "value2");
			param.Direction = FieldDirection.Ref;
			property.Parameters.Add (param);

			type.Members.Add (property);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    public virtual int Name {{{0}"
				+ "    }}{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void PropertyIndexerTest1 ()
		{
			type.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			// ensure case-insensitive comparison is done on name of property
			property.Name = "iTem";
			property.Attributes = MemberAttributes.Public;
			property.Type = new CodeTypeReference (typeof (int));

			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			property.Parameters.Add (param);

			param = new CodeParameterDeclarationExpression (
				typeof (int), "value2");
			param.Direction = FieldDirection.Ref;
			property.Parameters.Add (param);

			type.Members.Add (property);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    public virtual int this[object value1, ref int value2] {{{0}"
				+ "    }}{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		/// <summary>
		/// Ensures indexer code is only output if property is named "Item"
		/// (case-insensitive comparison) AND parameters are defined.
		/// </summary>
		[Test]
		public void PropertyIndexerTest2 ()
		{
			type.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			// ensure case-insensitive comparison is done on name of property
			property.Name = "iTem";
			property.Attributes = MemberAttributes.Public;
			property.Type = new CodeTypeReference (typeof (int));
			type.Members.Add (property);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    public virtual int iTem {{{0}"
				+ "    }}{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void MethodMembersTypeTest1 ()
		{
			type.Name = "Test1";

			CodeMemberMethod method = new CodeMemberMethod ();

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			method.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			method.CustomAttributes.Add (attrDec);

			// C# does not support Implementation Types, so this should be ignored
			method.ImplementationTypes.Add (new CodeTypeReference ("IPolicy"));

			type.Members.Add (method);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    [A()]{0}"
				+ "    [B()]{0}"
				+ "    private void () {{{0}"
				+ "    }}{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void MethodMembersTypeTest2 ()
		{
			type.Name = "Test1";

			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = "Something";
			method.Attributes = MemberAttributes.Public;
			method.ReturnType = new CodeTypeReference (typeof (int));

			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			method.Parameters.Add (param);

			param = new CodeParameterDeclarationExpression (
				typeof (object), "value2");
			param.Direction = FieldDirection.In;
			method.Parameters.Add (param);

			param = new CodeParameterDeclarationExpression (typeof (int), "index");
			param.Direction = FieldDirection.Out;
			method.Parameters.Add (param);

			param = new CodeParameterDeclarationExpression (typeof (int), "count");
			param.Direction = FieldDirection.Ref;
			method.Parameters.Add (param);

			type.Members.Add (method);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    public virtual int Something(object value1, object value2, out int index, ref int count) {{{0}"
				+ "    }}{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void MethodMembersTypeTest3 ()
		{
			type.Name = "Test1";

			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = "Something";
			method.Attributes = MemberAttributes.Public;
			method.ReturnType = new CodeTypeReference (typeof (int));

			// first parameter
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value");
			method.Parameters.Add (param);

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			param.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			param.CustomAttributes.Add (attrDec);

			// second parameter
			param = new CodeParameterDeclarationExpression (typeof (int), "index");
			param.Direction = FieldDirection.Out;
			method.Parameters.Add (param);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "C";
			attrDec.Arguments.Add (new CodeAttributeArgument ("A1",
				new CodePrimitiveExpression (false)));
			attrDec.Arguments.Add (new CodeAttributeArgument ("A2",
				new CodePrimitiveExpression (true)));
			param.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "D";
			param.CustomAttributes.Add (attrDec);

			type.Members.Add (method);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    public virtual int Something([A()] [B()] object value, [C(A1=false, A2=true)] [D()] out int index) {{{0}"
				+ "    }}{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void ConstructorAttributesTest ()
		{
			type.Name = "Test1";

			CodeConstructor ctor = new CodeConstructor ();

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			ctor.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			ctor.CustomAttributes.Add (attrDec);

			// C# does not support Implementation Types, so this should be ignored
			ctor.ImplementationTypes.Add (new CodeTypeReference ("IPolicy"));

			type.Members.Add (ctor);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    [A()]{0}"
				+ "    [B()]{0}"
				+ "    private Test1() {{{0}"
				+ "    }}{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void ConstructorParametersTest ()
		{
			type.Name = "Test1";

			CodeConstructor ctor = new CodeConstructor ();
			ctor.Name = "Whatever";
			ctor.Attributes = MemberAttributes.Public;

			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			ctor.Parameters.Add (param);

			param = new CodeParameterDeclarationExpression (
				typeof (object), "value2");
			param.Direction = FieldDirection.In;
			ctor.Parameters.Add (param);

			param = new CodeParameterDeclarationExpression (typeof (int), "index");
			param.Direction = FieldDirection.Out;
			ctor.Parameters.Add (param);

			param = new CodeParameterDeclarationExpression (typeof (int), "count");
			param.Direction = FieldDirection.Ref;
			ctor.Parameters.Add (param);

			type.Members.Add (ctor);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    public Test1(object value1, object value2, out int index, ref int count) {{{0}"
				+ "    }}{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void ConstructorParameterAttributesTest ()
		{
			type.Name = "Test1";

			CodeConstructor ctor = new CodeConstructor ();
			ctor.Name = "Something";
			ctor.Attributes = MemberAttributes.Public;

			// first parameter
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value");
			ctor.Parameters.Add (param);

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			param.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			param.CustomAttributes.Add (attrDec);

			// second parameter
			param = new CodeParameterDeclarationExpression (typeof (int), "index");
			param.Direction = FieldDirection.Out;
			ctor.Parameters.Add (param);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "C";
			attrDec.Arguments.Add (new CodeAttributeArgument ("A1",
				new CodePrimitiveExpression (false)));
			attrDec.Arguments.Add (new CodeAttributeArgument ("A2",
				new CodePrimitiveExpression (true)));
			param.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "D";
			param.CustomAttributes.Add (attrDec);

			type.Members.Add (ctor);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    public Test1([A()] [B()] object value, [C(A1=false, A2=true)] [D()] out int index) {{{0}"
				+ "    }}{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void BaseConstructorSingleArg ()
		{
			type.Name = "Test1";

			CodeConstructor ctor = new CodeConstructor ();
			ctor.Name = "Something";
			ctor.Attributes = MemberAttributes.Public;

			// first parameter
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			ctor.Parameters.Add (param);

			// second parameter
			param = new CodeParameterDeclarationExpression (typeof (int), "value2");
			param.Direction = FieldDirection.Out;
			ctor.Parameters.Add (param);

			// base ctor args
			ctor.BaseConstructorArgs.Add (new CodeVariableReferenceExpression ("value1"));

			type.Members.Add (ctor);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    public Test1(object value1, out int value2) : {0}"
				+ "            base(value1) {{{0}"
				+ "    }}{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void BaseConstructorMultipleArgs ()
		{
			type.Name = "Test1";

			CodeConstructor ctor = new CodeConstructor ();
			ctor.Name = "Something";
			ctor.Attributes = MemberAttributes.Public;

			// first parameter
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			ctor.Parameters.Add (param);

			// second parameter
			param = new CodeParameterDeclarationExpression (typeof (int), "value2");
			param.Direction = FieldDirection.Out;
			ctor.Parameters.Add (param);

			// base ctor args
			ctor.BaseConstructorArgs.Add (new CodeVariableReferenceExpression ("value1"));
			ctor.BaseConstructorArgs.Add (new CodeVariableReferenceExpression ("value2"));

			type.Members.Add (ctor);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    public Test1(object value1, out int value2) : {0}"
				+ "            base(value1, value2) {{{0}"
				+ "    }}{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void ChainedConstructorSingleArg ()
		{
			type.Name = "Test1";

			CodeConstructor ctor = new CodeConstructor ();
			ctor.Name = "Something";
			ctor.Attributes = MemberAttributes.Public;

			// first parameter
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			ctor.Parameters.Add (param);

			// second parameter
			param = new CodeParameterDeclarationExpression (typeof (int), "value2");
			param.Direction = FieldDirection.Out;
			ctor.Parameters.Add (param);

			// chained ctor args
			ctor.ChainedConstructorArgs.Add (new CodeVariableReferenceExpression ("value1"));

			type.Members.Add (ctor);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    public Test1(object value1, out int value2) : {0}"
				+ "            this(value1) {{{0}"
				+ "    }}{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void ChainedConstructorMultipleArgs ()
		{
			type.Name = "Test1";

			CodeConstructor ctor = new CodeConstructor ();
			ctor.Name = "Something";
			ctor.Attributes = MemberAttributes.Public;

			// first parameter
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			ctor.Parameters.Add (param);

			// second parameter
			param = new CodeParameterDeclarationExpression (typeof (int), "value2");
			param.Direction = FieldDirection.Out;
			ctor.Parameters.Add (param);

			// chained ctor args
			ctor.ChainedConstructorArgs.Add (new CodeVariableReferenceExpression ("value1"));
			ctor.ChainedConstructorArgs.Add (new CodeVariableReferenceExpression ("value2"));

			type.Members.Add (ctor);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    public Test1(object value1, out int value2) : {0}"
				+ "            this(value1, value2) {{{0}"
				+ "    }}{0}"
				+ "}}{0}", writer.NewLine), Code);
		}

		[Test]
		public void BaseAndChainedConstructorArg ()
		{
			type.Name = "Test1";

			CodeConstructor ctor = new CodeConstructor ();
			ctor.Name = "Something";
			ctor.Attributes = MemberAttributes.Public;

			// first parameter
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			ctor.Parameters.Add (param);

			// second parameter
			param = new CodeParameterDeclarationExpression (typeof (int), "value2");
			param.Direction = FieldDirection.Out;
			ctor.Parameters.Add (param);

			// base ctor args
			ctor.BaseConstructorArgs.Add (new CodeVariableReferenceExpression ("value1"));

			// chained ctor args
			ctor.ChainedConstructorArgs.Add (new CodeVariableReferenceExpression ("value1"));
			ctor.ChainedConstructorArgs.Add (new CodeVariableReferenceExpression ("value2"));

			type.Members.Add (ctor);

			Generate ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}"
				+ "    {0}"
				+ "    public Test1(object value1, out int value2) : {0}"
				+ "            base(value1) : {0}"
				+ "            this(value1, value2) {{{0}"
				+ "    }}{0}"
				+ "}}{0}", writer.NewLine), Code);
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
