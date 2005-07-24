//
// Microsoft.CSharp.* Test Cases
//
// Authors:
// Eric Lebel (ericlebel@yahoo.ca)
// Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) Novell
//

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Globalization;

using Microsoft.CSharp;

using NUnit.Framework;

using MonoTests.System.CodeDom.Compiler;

namespace MonoTests.Microsoft.CSharp
{
	[TestFixture]
	public class CodeGeneratorFromTypeTest_Class : CodeGeneratorFromTypeTestBase
	{
		private CodeTypeDeclaration _typeDeclaration;
		private ICodeGenerator _codeGenerator;

		#region Override implementation of CodeGeneratorTestBase

		protected override ICodeGenerator CodeGenerator
		{
			get { return _codeGenerator; }
		}

		[SetUp]
		public override void SetUp ()
		{
			base.SetUp ();
			_typeDeclaration = new CodeTypeDeclaration ();

			CodeDomProvider provider = new CSharpCodeProvider ();
			_codeGenerator = provider.CreateGenerator ();
		}

		#endregion Override implementation of CodeGeneratorTestBase

		#region Override implementation of CodeGeneratorFromTypeTestBase

		protected override CodeTypeDeclaration TypeDeclaration
		{
			get { return _typeDeclaration; }
		}

		[Test]
		public override void DefaultTypeTest ()
		{
			string code = GenerateDefaultType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class  {{{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public override void NullTypeTest ()
		{
			GenerateNullType ();
		}

		[Test]
		public override void SimpleTypeTest ()
		{
			string code = GenerateSimpleType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void DerivedTypeTest ()
		{
			string code = GenerateDerivedType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
#if NET_2_0
				"internal abstract class Test1 : int, System.Security.Principal.IIdentity, string, System.Security.IPermission {{{0}" +
#else
				"abstract class Test1 : int, System.Security.Principal.IIdentity, string, System.Security.IPermission {{{0}" +
#endif
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void AttributesAndTypeTest ()
		{
			string code = GenerateAttributesAndType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"[A()]{0}" +
				"[B()]{0}" +
				"public class Test1 {{{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest1 ()
		{
			string code = GenerateEventMembersType1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private event void ;{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest2 ()
		{	
			string code = GenerateEventMembersType2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public event int Click;{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void EventImplementationTypes ()
		{
			string code = GenerateEventImplementationTypes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    internal event int Click;{0}" +
#else
				"    /*FamANDAssem*/ internal event int Click;{0}" +
#endif
				"}}{0}", Writer.NewLine), code);
		}

		/// <summary>
		/// Ensure no access modifiers are output if PrivateImplementationType
		/// is set.
		/// </summary>
		[Test]
		public override void EventPrivateImplementationType ()
		{
			string code = GenerateEventPrivateImplementationType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    event int System.Int32.Click;{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		/// <summary>
		/// If both ImplementationTypes and PrivateImplementationType are set,
		/// then only ImplementationTypes are output.
		/// </summary>
		[Test]
		public override void EventImplementationTypeOrder ()
		{
			string code = GenerateEventImplementationTypeOrder ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    event int System.Int32.Click;{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void FieldMembersTypeTest1 ()
		{
			string code = GenerateFieldMembersType1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private void ;{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void FieldMembersTypeTest2 ()
		{
			string code = GenerateFieldMembersType2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public int Name = 2;{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest1 ()
		{
			string code = GeneratePropertyMembersAttributes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private void  {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest2 ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Public,
				false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    protected virtual int Name {{{0}" +
				"        get {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeSetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    internal virtual int Name {{{0}" +
#else
				"    internal int Name {{{0}" +
#endif
				"        set {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetSet ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    protected virtual int Name {{{0}" +
				"        get {{{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeFamilyOrAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.FamilyOrAssembly,
				false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    protected internal int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    internal virtual int Name {{{0}" +
#else
				"    internal int Name {{{0}" +
#endif
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		/// <summary>
		/// Apparently VB.NET CodeDOM also allows properties that aren't indexers
		/// to have parameters.
		/// </summary>
		[Test]
		public override void PropertyParametersTest ()
		{
			string code = GeneratePropertyParameters ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest1 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual int this[object value1, ref int value2] {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest2 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual int iTem {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyIndexerGetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				true, false, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    protected virtual int this[object value1, ref int value2] {{{0}" +
				"        get {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyIndexerSetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				false, true, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    protected virtual int this[object value1, ref int value2] {{{0}" +
				"        set {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypes ()
		{
			string code = GeneratePropertyImplementationTypes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		/// <summary>
		/// Ensure that Overloads keyword is output for a property which has
		/// explicitly been marked as Overloaded.
		/// </summary>
		[Test]
		public override void PropertyOverloadsTest1 ()
		{
			string code = GeneratePropertyOverloads1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		/// <summary>
		/// Ensure that Overloads keyword is output if multiple properties with
		/// the same name are defined.
		/// </summary>
		[Test]
		public override void PropertyOverloadsTest2 ()
		{
			string code = GeneratePropertyOverloads2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"    {0}" +
				"    private int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		/// <summary>
		/// Ensure that a property with a PrivateImplementationType and with 
		/// the same name does not qualify as an overload.
		/// </summary>
		[Test]
		public override void PropertyOverloadsTest3 ()
		{
			string code = GeneratePropertyOverloads3 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"    {0}" +
				"    int System.Int32.Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		/// <summary>
		/// Ensure no access modifiers are output if PrivateImplementationType
		/// is set. Default keyword is also not output in this case.
		/// </summary>
		[Test]
		public override void PropertyPrivateImplementationType ()
		{
			string code = GeneratePropertyPrivateImplementationType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    int System.Int32.this[object value1] {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		/// <summary>
		/// If both ImplementationTypes and PrivateImplementationType are set,
		/// then only ImplementationTypes are output.
		/// </summary>
		[Test]
		public override void PropertyImplementationTypeOrder ()
		{
			string code = GeneratePropertyImplementationTypeOrder ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    int System.Int32.this[object value1] {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest1 ()
		{
			string code = GenerateMethodMembersType1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private void () {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest2 ()
		{
			string code = GenerateMethodMembersType2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Something(object value1, object value2, out int index, ref int count) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest3 ()
		{
			string code = GenerateMethodMembersType3 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Something([A()] [B()] object value, [C(A1=false, A2=true)] [D()] out int index) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypes ()
		{
			string code = GenerateMethodImplementationTypes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    internal virtual int Execute() {{{0}" +
#else
				"    internal int Execute() {{{0}" +
#endif
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest1 ()
		{
			string code = GenerateMethodOverloads1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Execute() {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest2 ()
		{
			string code = GenerateMethodOverloads2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual void Execute() {{{0}" +
				"    }}{0}" +
				"    {0}" +
				"    private int Execute(object value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		/// <summary>
		/// Ensure that a method with a PrivateImplementationType and with 
		/// the same name does not qualify as an overload.
		/// </summary>
		[Test]
		public override void MethodOverloadsTest3 ()
		{
			string code = GenerateMethodOverloads3 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual void Execute() {{{0}" +
				"    }}{0}" +
				"    {0}" +
				"    int System.Int32.Execute(object value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		/// <summary>
		/// Ensure no access modifiers are output if PrivateImplementationType
		/// is set.
		/// </summary>
		[Test]
		public override void MethodPrivateImplementationType ()
		{
			string code = GenerateMethodPrivateImplementationType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    int System.Int32.Execute(object value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		/// <summary>
		/// If both ImplementationTypes and PrivateImplementationType are set,
		/// then only ImplementationTypes are output.
		/// </summary>
		[Test]
		public override void MethodImplementationTypeOrder ()
		{
			string code = GenerateMethodImplementationTypeOrder ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    int System.Int32.Execute(object value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodReturnTypeAttributes ()
		{
			string code = GenerateMethodReturnTypeAttributes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    [return: C(A1=false, A2=true)]{0}" +
				"    [return: D()]{0}" +
				"    public virtual int Execute() {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ConstructorAttributesTest ()
		{
			string code = GenerateConstructorAttributes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private Test1() {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ConstructorParametersTest ()
		{
			string code = GenerateConstructorParameters ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public Test1(object value1, object value2, out int index, ref int count) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ConstructorParameterAttributesTest ()
		{
			string code = GenerateConstructorParameterAttributes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public Test1([A()] [B()] object value, [C(A1=false, A2=true)] [D()] out int index) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void BaseConstructorSingleArg ()
		{
			string code = GenerateBaseConstructor (false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public Test1(object value1, out int value2) : {0}" +
				"            base(value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void BaseConstructorMultipleArgs ()
		{
			string code = GenerateBaseConstructor (true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public Test1(object value1, out int value2) : {0}" +
				"            base(value1, value2) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ChainedConstructorSingleArg ()
		{
			string code = GenerateChainedConstructor (false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public Test1(object value1, out int value2) : {0}" +
				"            base(value3) : {0}" +
				"            this(value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ChainedConstructorMultipleArgs ()
		{
			string code = GenerateChainedConstructor (true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public Test1(object value1, out int value2) : {0}" +
				"            base(value3) : {0}" +
				"            this(value1, value2) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void TypeConstructorTest ()
		{
			string code = GenerateTypeConstructor ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    [A()]{0}" +
				"    [B()]{0}" +
#endif
				"    static Test1() {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		#endregion Override implementation of CodeGeneratorFromTypeTestBase
	}

	[TestFixture]
	public class dCodeGeneratorFromTypeTest_Delegate : CodeGeneratorFromTypeTestBase
	{
		private CodeTypeDeclaration _typeDeclaration;
		private ICodeGenerator _codeGenerator;

		#region Override implementation of CodeGeneratorTestBase
		
		protected override ICodeGenerator CodeGenerator
		{
			get { return _codeGenerator; }
		}

		[SetUp]
		public override void SetUp ()
		{
			base.SetUp ();
			_typeDeclaration = new CodeTypeDelegate ();

			CodeDomProvider provider = new CSharpCodeProvider ();
			_codeGenerator = provider.CreateGenerator ();
		}

		#endregion Override implementation of CodeGeneratorTestBase

		#region Override implementation of CodeGeneratorFromTypeTestBase

		protected override CodeTypeDeclaration TypeDeclaration
		{
			get { return _typeDeclaration; }
		}

		[Test]
		public override void DefaultTypeTest ()
		{
			string code = GenerateDefaultType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void ();{0}", Writer.NewLine), code);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public override void NullTypeTest ()
		{
			GenerateNullType ();
		}

		[Test]
		public override void SimpleTypeTest ()
		{
			string code = GenerateSimpleType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}", Writer.NewLine), code);
		}

		[Test]
		public override void DerivedTypeTest ()
		{
			string code = GenerateDerivedType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"delegate void Test1();{0}", Writer.NewLine), code);
		}

		[Test]
		public override void AttributesAndTypeTest ()
		{
			CodeTypeDelegate delegateDecl = new CodeTypeDelegate ();
			delegateDecl.ReturnType = new CodeTypeReference (typeof (int));

			_typeDeclaration = delegateDecl;

			string code = GenerateAttributesAndType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"[A()]{0}" +
				"[B()]{0}" +
				"public delegate int Test1();{0}", Writer.NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest1 ()
		{
			string code = GenerateEventMembersType1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest2 ()
		{
			string code = GenerateEventMembersType2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void EventImplementationTypes ()
		{
			string code = GenerateEventImplementationTypes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		/// <summary>
		/// Ensure no access modifiers are output if PrivateImplementationType
		/// is set.
		/// </summary>
		[Test]
		public override void EventPrivateImplementationType ()
		{
			string code = GenerateEventPrivateImplementationType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		/// <summary>
		/// If both ImplementationTypes and PrivateImplementationType are set,
		/// then only ImplementationTypes are output.
		/// </summary>
		[Test]
		public override void EventImplementationTypeOrder ()
		{
			string code = GenerateEventImplementationTypeOrder ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void FieldMembersTypeTest1 ()
		{
			string code = GenerateFieldMembersType1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void FieldMembersTypeTest2 ()
		{
			string code = GenerateFieldMembersType2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest1 ()
		{
			string code = GeneratePropertyMembersAttributes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest2 ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Public,
				false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeSetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetSet ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeFamilyOrAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.FamilyOrAssembly,
				false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyParametersTest ()
		{
			string code = GeneratePropertyParameters ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest1 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest2 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyIndexerGetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				true, false, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyIndexerSetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				false, true, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypes ()
		{
			string code = GeneratePropertyImplementationTypes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest1 ()
		{
			string code = GeneratePropertyOverloads1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest2 ()
		{
			string code = GeneratePropertyOverloads2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest3 ()
		{
			string code = GeneratePropertyOverloads3 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyPrivateImplementationType ()
		{
			string code = GeneratePropertyPrivateImplementationType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypeOrder ()
		{
			string code = GeneratePropertyImplementationTypeOrder ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest1 ()
		{
			string code = GenerateMethodMembersType1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest2 ()
		{
			string code = GenerateMethodMembersType2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest3 ()
		{
			string code = GenerateMethodMembersType3 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypes ()
		{
			string code = GenerateMethodImplementationTypes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest1 ()
		{
			string code = GenerateMethodOverloads1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest2 ()
		{
			string code = GenerateMethodOverloads2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest3 ()
		{
			string code = GenerateMethodOverloads3 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodPrivateImplementationType ()
		{
			string code = GenerateMethodPrivateImplementationType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypeOrder ()
		{
			string code = GenerateMethodImplementationTypeOrder ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodReturnTypeAttributes ()
		{
			string code = GenerateMethodReturnTypeAttributes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ConstructorAttributesTest ()
		{
			string code = GenerateConstructorAttributes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ConstructorParametersTest ()
		{
			string code = GenerateConstructorParameters ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ConstructorParameterAttributesTest ()
		{
			string code = GenerateConstructorParameterAttributes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void BaseConstructorSingleArg ()
		{
			string code = GenerateBaseConstructor (false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void BaseConstructorMultipleArgs ()
		{
			string code = GenerateBaseConstructor (true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ChainedConstructorSingleArg ()
		{
			string code = GenerateChainedConstructor (false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ChainedConstructorMultipleArgs ()
		{
			string code = GenerateChainedConstructor (true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void TypeConstructorTest ()
		{
			string code = GenerateTypeConstructor ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", Writer.NewLine), code);
		}

		#endregion Override implementation of CodeGeneratorFromTypeTestBase
	}

	[TestFixture]
	public class CodeGeneratorFromTypeTest_Interface : CodeGeneratorFromTypeTestBase
	{
		private CodeTypeDeclaration _typeDeclaration;
		private ICodeGenerator _codeGenerator;

		#region Override implementation of CodeGeneratorTestBase

		protected override ICodeGenerator CodeGenerator
		{
			get { return _codeGenerator; }
		}

		[SetUp]
		public override void SetUp ()
		{
			base.SetUp ();
			_typeDeclaration = new CodeTypeDeclaration ();
			_typeDeclaration.IsInterface = true;

			CodeDomProvider provider = new CSharpCodeProvider ();
			_codeGenerator = provider.CreateGenerator ();
		}

		#endregion Override implementation of CodeGeneratorTestBase

		#region Override implementation of CodeGeneratorFromTypeTestBase

		protected override CodeTypeDeclaration TypeDeclaration
		{
			get { return _typeDeclaration; }
		}

		[Test]
		public override void DefaultTypeTest ()
		{
			string code = GenerateDefaultType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface  {{{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public override void NullTypeTest ()
		{
			GenerateNullType ();
		}

		[Test]
		public override void SimpleTypeTest ()
		{
			string code = GenerateSimpleType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void DerivedTypeTest ()
		{
			string code = GenerateDerivedType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
#if NET_2_0
				"internal interface Test1 : int, System.Security.Principal.IIdentity, string, System.Security.IPermission {{{0}" +
#else
				"interface Test1 : int, System.Security.Principal.IIdentity, string, System.Security.IPermission {{{0}" +
#endif
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void AttributesAndTypeTest ()
		{
			string code = GenerateAttributesAndType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"[A()]{0}" +
				"[B()]{0}" +
				"public interface Test1 {{{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest1 ()
		{
			string code = GenerateEventMembersType1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private event void ;{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest2 ()
		{
			string code = GenerateEventMembersType2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    public event int Click;{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void EventImplementationTypes ()
		{
			string code = GenerateEventImplementationTypes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    internal event int Click;{0}" +
#else
				"    /*FamANDAssem*/ internal event int Click;{0}" +
#endif
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void EventPrivateImplementationType ()
		{
			string code = GenerateEventPrivateImplementationType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    event int System.Int32.Click;{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void EventImplementationTypeOrder ()
		{
			string code = GenerateEventImplementationTypeOrder ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    event int System.Int32.Click;{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void FieldMembersTypeTest1 ()
		{
			string code = GenerateFieldMembersType1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void FieldMembersTypeTest2 ()
		{
			string code = GenerateFieldMembersType2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest1 ()
		{
			string code = GeneratePropertyMembersAttributes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    void  {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest2 ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Public,
				false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"        get;{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeSetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"        set;{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetSet ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"        get;{0}" +
				"        set;{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeFamilyOrAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.FamilyOrAssembly,
				false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyParametersTest ()
		{
			string code = GeneratePropertyParameters ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest1 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int this[object value1, ref int value2] {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest2 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int iTem {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyIndexerGetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				true, false, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int this[object value1, ref int value2] {{{0}" +
				"        get;{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyIndexerSetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				false, true, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int this[object value1, ref int value2] {{{0}" +
				"        set;{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypes ()
		{
			string code = GeneratePropertyImplementationTypes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest1 ()
		{
			string code = GeneratePropertyOverloads1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest2 ()
		{
			string code = GeneratePropertyOverloads2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"    }}{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest3 ()
		{
			string code = GeneratePropertyOverloads3 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"    }}{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyPrivateImplementationType ()
		{
			string code = GeneratePropertyPrivateImplementationType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int this[object value1] {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypeOrder ()
		{
			string code = GeneratePropertyImplementationTypeOrder ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int this[object value1] {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest1 ()
		{
			string code = GenerateMethodMembersType1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    void ();{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest2 ()
		{
			string code = GenerateMethodMembersType2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Something(object value1, object value2, out int index, ref int count);{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest3 ()
		{
			string code = GenerateMethodMembersType3 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Something([A()] [B()] object value, [C(A1=false, A2=true)] [D()] out int index);{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypes ()
		{
			string code = GenerateMethodImplementationTypes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Execute();{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest1 ()
		{
			string code = GenerateMethodOverloads1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Execute();{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest2 ()
		{
			string code = GenerateMethodOverloads2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    void Execute();{0}" +
				"    {0}" +
				"    int Execute(object value1);{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest3 ()
		{
			string code = GenerateMethodOverloads3 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    void Execute();{0}" +
				"    {0}" +
				"    int System.Int32.Execute(object value1);{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodPrivateImplementationType ()
		{
			string code = GenerateMethodPrivateImplementationType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int System.Int32.Execute(object value1);{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypeOrder ()
		{
			string code = GenerateMethodImplementationTypeOrder ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int System.Int32.Execute(object value1);{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodReturnTypeAttributes ()
		{
			string code = GenerateMethodReturnTypeAttributes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    [return: C(A1=false, A2=true)]{0}" +
				"    [return: D()]{0}" +
				"    int Execute();{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ConstructorAttributesTest ()
		{
			string code = GenerateConstructorAttributes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ConstructorParametersTest ()
		{
			string code = GenerateConstructorParameters ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ConstructorParameterAttributesTest ()
		{
			string code = GenerateConstructorParameterAttributes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void BaseConstructorSingleArg ()
		{
			string code = GenerateBaseConstructor (false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void BaseConstructorMultipleArgs ()
		{
			string code = GenerateBaseConstructor (true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ChainedConstructorSingleArg ()
		{
			string code = GenerateChainedConstructor (false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ChainedConstructorMultipleArgs ()
		{
			string code = GenerateChainedConstructor (true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void TypeConstructorTest ()
		{
			string code = GenerateTypeConstructor ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		#endregion Override implementation of CodeGeneratorFromTypeTestBase
	}

	[TestFixture]
	public class CodeGeneratorFromTypeTest_Struct : CodeGeneratorFromTypeTestBase
	{
		private CodeTypeDeclaration _typeDeclaration;
		private ICodeGenerator _codeGenerator;

		#region Override implementation of CodeGeneratorTestBase

		protected override ICodeGenerator CodeGenerator
		{
			get { return _codeGenerator; }
		}

		[SetUp]
		public override void SetUp ()
		{
			base.SetUp ();
			_typeDeclaration = new CodeTypeDeclaration ();
			_typeDeclaration.IsStruct = true;

			CodeDomProvider provider = new CSharpCodeProvider ();
			_codeGenerator = provider.CreateGenerator ();
		}

		#endregion Override implementation of CodeGeneratorTestBase

		#region Override implementation of CodeGeneratorFromTypeTestBase

		protected override CodeTypeDeclaration TypeDeclaration
		{
			get { return _typeDeclaration; }
		}

		[Test]
		public override void DefaultTypeTest ()
		{
			string code = GenerateDefaultType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct  {{{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public override void NullTypeTest ()
		{
			GenerateNullType ();
		}

		[Test]
		public override void SimpleTypeTest ()
		{
			string code = GenerateSimpleType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void DerivedTypeTest ()
		{
			string code = GenerateDerivedType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
#if NET_2_0
				"internal struct Test1 : int, System.Security.Principal.IIdentity, string, System.Security.IPermission {{{0}" +
#else
				"struct Test1 : int, System.Security.Principal.IIdentity, string, System.Security.IPermission {{{0}" +
#endif
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void AttributesAndTypeTest ()
		{
			string code = GenerateAttributesAndType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"[A()]{0}" +
				"[B()]{0}" +
				"public struct Test1 {{{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest1 ()
		{
			string code = GenerateEventMembersType1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private event void ;{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest2 ()
		{
			string code = GenerateEventMembersType2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public event int Click;{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void EventImplementationTypes ()
		{
			string code = GenerateEventImplementationTypes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    internal event int Click;{0}" +
#else
				"    /*FamANDAssem*/ internal event int Click;{0}" +
#endif
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void EventPrivateImplementationType ()
		{
			string code = GenerateEventPrivateImplementationType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    event int System.Int32.Click;{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void EventImplementationTypeOrder ()
		{
			string code = GenerateEventImplementationTypeOrder ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    event int System.Int32.Click;{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void FieldMembersTypeTest1 ()
		{
			string code = GenerateFieldMembersType1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private void ;{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void FieldMembersTypeTest2 ()
		{
			string code = GenerateFieldMembersType2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public int Name = 2;{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest1 ()
		{
			string code = GeneratePropertyMembersAttributes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private void  {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest2 ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Public,
				false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    protected virtual int Name {{{0}" +
				"        get {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeSetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    internal virtual int Name {{{0}" +
#else
				"    internal int Name {{{0}" +
#endif
				"        set {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetSet ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    protected virtual int Name {{{0}" +
				"        get {{{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeFamilyOrAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.FamilyOrAssembly,
				false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    protected internal int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    internal virtual int Name {{{0}" +
#else
				"    internal int Name {{{0}" +
#endif
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyParametersTest ()
		{
			string code = GeneratePropertyParameters ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest1 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual int this[object value1, ref int value2] {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest2 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual int iTem {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyIndexerGetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				true, false, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    protected virtual int this[object value1, ref int value2] {{{0}" +
				"        get {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyIndexerSetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				false, true, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    protected virtual int this[object value1, ref int value2] {{{0}" +
				"        set {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypes ()
		{
			string code = GeneratePropertyImplementationTypes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest1 ()
		{
			string code = GeneratePropertyOverloads1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest2 ()
		{
			string code = GeneratePropertyOverloads2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"    {0}" +
				"    private int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest3 ()
		{
			string code = GeneratePropertyOverloads3 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"    {0}" +
				"    int System.Int32.Name {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyPrivateImplementationType ()
		{
			string code = GeneratePropertyPrivateImplementationType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    int System.Int32.this[object value1] {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypeOrder ()
		{
			string code = GeneratePropertyImplementationTypeOrder ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    int System.Int32.this[object value1] {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest1 ()
		{
			string code = GenerateMethodMembersType1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private void () {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest2 ()
		{
			string code = GenerateMethodMembersType2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Something(object value1, object value2, out int index, ref int count) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest3 ()
		{
			string code = GenerateMethodMembersType3 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Something([A()] [B()] object value, [C(A1=false, A2=true)] [D()] out int index) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypes ()
		{
			string code = GenerateMethodImplementationTypes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    internal virtual int Execute() {{{0}" +
#else
				"    internal int Execute() {{{0}" +
#endif
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest1 ()
		{
			string code = GenerateMethodOverloads1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Execute() {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest2 ()
		{
			string code = GenerateMethodOverloads2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual void Execute() {{{0}" +
				"    }}{0}" +
				"    {0}" +
				"    private int Execute(object value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest3 ()
		{
			string code = GenerateMethodOverloads3 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual void Execute() {{{0}" +
				"    }}{0}" +
				"    {0}" +
				"    int System.Int32.Execute(object value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodPrivateImplementationType ()
		{
			string code = GenerateMethodPrivateImplementationType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    int System.Int32.Execute(object value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypeOrder ()
		{
			string code = GenerateMethodImplementationTypeOrder ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    int System.Int32.Execute(object value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodReturnTypeAttributes ()
		{
			string code = GenerateMethodReturnTypeAttributes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    [return: C(A1=false, A2=true)]{0}" +
				"    [return: D()]{0}" +
				"    public virtual int Execute() {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ConstructorAttributesTest ()
		{
			string code = GenerateConstructorAttributes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private Test1() {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ConstructorParametersTest ()
		{
			string code = GenerateConstructorParameters ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public Test1(object value1, object value2, out int index, ref int count) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ConstructorParameterAttributesTest ()
		{
			string code = GenerateConstructorParameterAttributes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public Test1([A()] [B()] object value, [C(A1=false, A2=true)] [D()] out int index) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void BaseConstructorSingleArg ()
		{
			string code = GenerateBaseConstructor (false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public Test1(object value1, out int value2) : {0}" +
				"            base(value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void BaseConstructorMultipleArgs ()
		{
			string code = GenerateBaseConstructor (true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public Test1(object value1, out int value2) : {0}" +
				"            base(value1, value2) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ChainedConstructorSingleArg ()
		{
			string code = GenerateChainedConstructor (false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public Test1(object value1, out int value2) : {0}" +
				"            base(value3) : {0}" +
				"            this(value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ChainedConstructorMultipleArgs ()
		{
			string code = GenerateChainedConstructor (true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public Test1(object value1, out int value2) : {0}" +
				"            base(value3) : {0}" +
				"            this(value1, value2) {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void TypeConstructorTest ()
		{
			string code = GenerateTypeConstructor ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    [A()]{0}" +
				"    [B()]{0}" +
#endif
				"    static Test1() {{{0}" +
				"    }}{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		#endregion Override implementation of CodeGeneratorFromTypeTestBase
	}

	[TestFixture]
	public class CodeGeneratorFromTypeTest_Enum : CodeGeneratorFromTypeTestBase
	{
		private CodeTypeDeclaration _typeDeclaration;
		private ICodeGenerator _codeGenerator;

		#region Override implementation of CodeGeneratorTestBase

		protected override ICodeGenerator CodeGenerator
		{
			get { return _codeGenerator; }
		}

		[SetUp]
		public override void SetUp ()
		{
			base.SetUp ();
			_typeDeclaration = new CodeTypeDeclaration ();
			_typeDeclaration.IsEnum = true;

			CodeDomProvider provider = new CSharpCodeProvider ();
			_codeGenerator = provider.CreateGenerator ();
		}

		#endregion Override implementation of CodeGeneratorTestBase

		#region Override implementation of CodeGeneratorFromTypeTestBase

		protected override CodeTypeDeclaration TypeDeclaration
		{
			get { return _typeDeclaration; }
		}

		[Test]
		public override void DefaultTypeTest ()
		{
			string code = GenerateDefaultType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum  {{{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public override void NullTypeTest ()
		{
			GenerateNullType ();
		}

		[Test]
		public override void SimpleTypeTest ()
		{
			string code = GenerateSimpleType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void DerivedTypeTest ()
		{
			string code = GenerateDerivedType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
#if NET_2_0
				"internal enum Test1 : int, System.Security.Principal.IIdentity, string, System.Security.IPermission {{{0}" +
#else
				"enum Test1 : int, System.Security.Principal.IIdentity, string, System.Security.IPermission {{{0}" +
#endif
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void AttributesAndTypeTest ()
		{
			string code = GenerateAttributesAndType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"[A()]{0}" +
				"[B()]{0}" +
				"public enum Test1 {{{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest1 ()
		{
			string code = GenerateEventMembersType1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest2 ()
		{
			string code = GenerateEventMembersType2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void EventImplementationTypes ()
		{
			string code = GenerateEventImplementationTypes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void EventPrivateImplementationType ()
		{
			string code = GenerateEventPrivateImplementationType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void EventImplementationTypeOrder ()
		{
			string code = GenerateEventImplementationTypeOrder ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void FieldMembersTypeTest1 ()
		{
			string code = GenerateFieldMembersType1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    ,{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void FieldMembersTypeTest2 ()
		{
			string code = GenerateFieldMembersType2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"    Name = 2,{0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest1 ()
		{
			string code = GeneratePropertyMembersAttributes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest2 ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Public,
				false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeSetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetSet ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeFamilyOrAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.FamilyOrAssembly,
				false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyParametersTest ()
		{
			string code = GeneratePropertyParameters ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest1 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest2 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyIndexerGetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				true, false, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyIndexerSetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				false, true, true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypes ()
		{
			string code = GeneratePropertyImplementationTypes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest1 ()
		{
			string code = GeneratePropertyOverloads1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest2 ()
		{
			string code = GeneratePropertyOverloads2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest3 ()
		{
			string code = GeneratePropertyOverloads3 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyPrivateImplementationType ()
		{
			string code = GeneratePropertyPrivateImplementationType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypeOrder ()
		{
			string code = GeneratePropertyImplementationTypeOrder ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest1 ()
		{
			string code = GenerateMethodMembersType1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest2 ()
		{
			string code = GenerateMethodMembersType2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest3 ()
		{
			string code = GenerateMethodMembersType3 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypes ()
		{
			string code = GenerateMethodImplementationTypes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest1 ()
		{
			string code = GenerateMethodOverloads1 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest2 ()
		{
			string code = GenerateMethodOverloads2 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest3 ()
		{
			string code = GenerateMethodOverloads3 ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodPrivateImplementationType ()
		{
			string code = GenerateMethodPrivateImplementationType ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypeOrder ()
		{
			string code = GenerateMethodImplementationTypeOrder ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void MethodReturnTypeAttributes ()
		{
			string code = GenerateMethodReturnTypeAttributes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ConstructorAttributesTest ()
		{
			string code = GenerateConstructorAttributes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ConstructorParametersTest ()
		{
			string code = GenerateConstructorParameters ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ConstructorParameterAttributesTest ()
		{
			string code = GenerateConstructorParameterAttributes ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void BaseConstructorSingleArg ()
		{
			string code = GenerateBaseConstructor (false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void BaseConstructorMultipleArgs ()
		{
			string code = GenerateBaseConstructor (true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ChainedConstructorSingleArg ()
		{
			string code = GenerateChainedConstructor (false);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void ChainedConstructorMultipleArgs ()
		{
			string code = GenerateChainedConstructor (true);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		[Test]
		public override void TypeConstructorTest ()
		{
			string code = GenerateTypeConstructor ();
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", Writer.NewLine), code);
		}

		#endregion Override implementation of CodeGeneratorFromTypeTestBase
	}
}
