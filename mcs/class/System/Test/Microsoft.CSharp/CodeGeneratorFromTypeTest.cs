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
using System.IO;

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
			string code = GenerateDefaultType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class  {{{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void DefaultTypeTest_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GenerateDefaultType (options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class {0}" +
				"{{{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public override void NullTypeTest ()
		{
			GenerateNullType (Options);
		}

		[Test]
		public override void SimpleTypeTest ()
		{
			string code = GenerateSimpleType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void SimpleTypeTest_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GenerateSimpleType (options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1{0}" +
				"{{{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void DerivedTypeTest ()
		{
			string code = GenerateDerivedType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
#if NET_2_0
				"internal abstract class Test1 : int, System.Security.Principal.IIdentity, string, System.Security.IPermission {{{0}" +
#else
				"abstract class Test1 : int, System.Security.Principal.IIdentity, string, System.Security.IPermission {{{0}" +
#endif
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void AttributesAndTypeTest ()
		{
			string code = GenerateAttributesAndType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"[A()]{0}" +
				"[B()]{0}" +
				"public class Test1 {{{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest1 ()
		{
			string code = GenerateEventMembersType1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private event void ;{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest2 ()
		{	
			string code = GenerateEventMembersType2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public event int Click;{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void EventImplementationTypes ()
		{
			string code = GenerateEventImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    internal event int Click;{0}" +
#else
				"    /*FamANDAssem*/ internal event int Click;{0}" +
#endif
				"}}{0}", NewLine), code);
		}

		/// <summary>
		/// Ensure no access modifiers are output if PrivateImplementationType
		/// is set.
		/// </summary>
		[Test]
		public override void EventPrivateImplementationType ()
		{
			string code = GenerateEventPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    event int System.Int32.Click;{0}" +
				"}}{0}", NewLine), code);
		}

		/// <summary>
		/// If both ImplementationTypes and PrivateImplementationType are set,
		/// then only ImplementationTypes are output.
		/// </summary>
		[Test]
		public override void EventImplementationTypeOrder ()
		{
			string code = GenerateEventImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    event int System.Int32.Click;{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void FieldMembersAttributesTest ()
		{
			string code = GenerateFieldMembersAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private void ;{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void FieldMembersTypeTest ()
		{
			string code = GenerateFieldMembersType (MemberAttributes.Public, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public int Name = 2;{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void FieldNewSlotTest ()
		{
			string code = GenerateFieldMembersType (MemberAttributes.Assembly |
				MemberAttributes.New, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    internal new int Name = 2;{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void AbstractPropertyTest ()
		{
			string code = GenerateAbstractProperty (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public abstract class Test1 {{{0}" +
				"    {0}" +
				"    public abstract string Name {{{0}" +
				"        get;{0}" +
				"        set;{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void StaticPropertyTest ()
		{
			string code = GenerateStaticProperty (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public static string Name {{{0}" +
				"        set {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest1 ()
		{
			string code = GeneratePropertyMembersAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private void  {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void PropertyMembersTypeTest1_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GeneratePropertyMembersAttributes (options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1{0}" + 
				"{{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private void {0}" + 
				"    {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest2 ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Public,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    protected virtual int Name {{{0}" +
				"        get {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeSetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, true, Options);
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
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetSet ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    protected virtual int Name {{{0}" +
				"        get {{{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void PropertyMembersTypeGetSet_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, true, options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1{0}" + 
				"{{{0}" +
				"    {0}" +
				"    protected virtual int Name{0}" + 
				"    {{{0}" +
				"        get{0}" + 
				"        {{{0}" +
				"        }}{0}" +
				"        set{0}" + 
				"        {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeFamilyOrAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.FamilyOrAssembly,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    protected internal int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    internal virtual int Name {{{0}" +
#else
				"    internal int Name {{{0}" +
#endif
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		/// <summary>
		/// Apparently VB.NET CodeDOM also allows properties that aren't indexers
		/// to have parameters.
		/// </summary>
		[Test]
		public override void PropertyParametersTest ()
		{
			string code = GeneratePropertyParameters (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest1 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual int this[object value1, ref int value2] {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest2 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual int iTem {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerGetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				true, false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    protected virtual int this[object value1, ref int value2] {{{0}" +
				"        get {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerSetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				false, true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    protected virtual int this[object value1, ref int value2] {{{0}" +
				"        set {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypes ()
		{
			string code = GeneratePropertyImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		/// <summary>
		/// Ensure that Overloads keyword is output for a property which has
		/// explicitly been marked as Overloaded.
		/// </summary>
		[Test]
		public override void PropertyOverloadsTest1 ()
		{
			string code = GeneratePropertyOverloads1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    protected virtual int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		/// <summary>
		/// Ensure that Overloads keyword is output if multiple properties with
		/// the same name are defined.
		/// </summary>
		[Test]
		public override void PropertyOverloadsTest2 ()
		{
			string code = GeneratePropertyOverloads2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"    {0}" +
				"    private int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		/// <summary>
		/// Ensure that a property with a PrivateImplementationType and with 
		/// the same name does not qualify as an overload.
		/// </summary>
		[Test]
		public override void PropertyOverloadsTest3 ()
		{
			string code = GeneratePropertyOverloads3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"    {0}" +
				"    int System.Int32.Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		/// <summary>
		/// Ensure no access modifiers are output if PrivateImplementationType
		/// is set. Default keyword is also not output in this case.
		/// </summary>
		[Test]
		public override void PropertyPrivateImplementationType ()
		{
			string code = GeneratePropertyPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    int System.Int32.this[object value1] {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		/// <summary>
		/// If both ImplementationTypes and PrivateImplementationType are set,
		/// then only ImplementationTypes are output.
		/// </summary>
		[Test]
		public override void PropertyImplementationTypeOrder ()
		{
			string code = GeneratePropertyImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    int System.Int32.this[object value1] {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyNewSlotTest ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Private |
				MemberAttributes.New, true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    private new int Name {{{0}" +
				"        get {{{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest1 ()
		{
			string code = GenerateMethodMembersType1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private void () {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void MethodMembersTypeTest1_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GenerateMethodMembersType1 (options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1{0}" + 
				"{{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private void (){0}" + 
				"    {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest2 ()
		{
			string code = GenerateMethodMembersType2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Something(object value1, object value2, out int index, ref int count) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest3 ()
		{
			string code = GenerateMethodMembersType3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Something([A()] [B()] object value, [C(A1=false, A2=true)] [D()] out int ) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypes ()
		{
			string code = GenerateMethodImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    internal virtual int Execute() {{{0}" +
#else
				"    internal int Execute() {{{0}" +
#endif
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest1 ()
		{
			string code = GenerateMethodOverloads1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    internal virtual int Execute() {{{0}" +
#else
				"    internal int Execute() {{{0}" +
#endif
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest2 ()
		{
			string code = GenerateMethodOverloads2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual void Execute() {{{0}" +
				"    }}{0}" +
				"    {0}" +
				"    private int Execute(object value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		/// <summary>
		/// Ensure that a method with a PrivateImplementationType and with 
		/// the same name does not qualify as an overload.
		/// </summary>
		[Test]
		public override void MethodOverloadsTest3 ()
		{
			string code = GenerateMethodOverloads3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public virtual void Execute() {{{0}" +
				"    }}{0}" +
				"    {0}" +
				"    int System.Int32.Execute(object value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		/// <summary>
		/// Ensure no access modifiers are output if PrivateImplementationType
		/// is set.
		/// </summary>
		[Test]
		public override void MethodPrivateImplementationType ()
		{
			string code = GenerateMethodPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    int System.Int32.Execute(object value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		/// <summary>
		/// If both ImplementationTypes and PrivateImplementationType are set,
		/// then only ImplementationTypes are output.
		/// </summary>
		[Test]
		public override void MethodImplementationTypeOrder ()
		{
			string code = GenerateMethodImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    int System.Int32.Execute(object value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodParamArrayAttribute ()
		{
			string code = GenerateMethodParamArrayAttribute (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    public virtual int Something([A()] [B()] params out object value, [C()] ref int ) {{{0}" +
#else
				"    public virtual int Something([A()] [System.ParamArrayAttribute()] [B()] out object value, [C()] ref int ) {{{0}" +
#endif
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodReturnTypeAttributes ()
		{
			string code = GenerateMethodReturnTypeAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
#if NET_2_0
				"    params{0}" +
#else
				"    [System.ParamArrayAttribute()]{0}" +
#endif
				"    [return: C(A1=false, A2=true)]{0}" +
#if !NET_2_0
				"    [return: System.ParamArrayAttribute()]{0}" +
#endif
				"    [return: D()]{0}" +
#if NET_2_0
				"    return: params{0}" +
#endif
				"    public virtual int Execute() {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodNewSlotTest ()
		{
			string code = GenerateMethodNewSlot (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public new virtual int Execute() {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorAttributesTest ()
		{
			string code = GenerateConstructorAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private Test1() {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void ConstructorAttributesTest_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GenerateConstructorAttributes (options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1{0}" + 
				"{{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private Test1(){0}" + 
				"    {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorParametersTest ()
		{
			string code = GenerateConstructorParameters (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public Test1(object value1, object value2, out int index, ref int count) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorParameterAttributesTest ()
		{
			string code = GenerateConstructorParameterAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    private Test1([A()] [B()] object value, [C(A1=false, A2=true)] [D()] out int index) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void BaseConstructorSingleArg ()
		{
			string code = GenerateBaseConstructor (false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    protected Test1(object value1, out int value2) : {0}" +
				"            base(value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void BaseConstructorMultipleArgs ()
		{
			string code = GenerateBaseConstructor (true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    protected Test1(object value1, out int value2) : {0}" +
				"            base(value1, value2) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void ChainedConstructorSingleArg ()
		{
			string code = GenerateChainedConstructor (false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public Test1(object value1, out int value2) : {0}" +
				"            base(value3) : {0}" +
				"            this(value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void ChainedConstructorMultipleArgs ()
		{
			string code = GenerateChainedConstructor (true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
				"    public Test1(object value1, out int value2) : {0}" +
				"            base(value3) : {0}" +
				"            this(value1, value2) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void TypeConstructorTest ()
		{
			string code = GenerateTypeConstructor (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    [A()]{0}" +
				"    [B()]{0}" +
#endif
				"    static Test1() {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void TypeConstructorTest_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GenerateTypeConstructor (options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1{0}" + 
				"{{{0}" +
				"    {0}" +
#if NET_2_0
				"    [A()]{0}" +
				"    [B()]{0}" +
#endif
				"    static Test1(){0}" + 
				"    {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void EntryPointMethodTest ()
		{
			string code = GenerateEntryPointMethod (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    [A()]{0}" +
				"    public static int Main() {{{0}" +
#else
				"    public static void Main() {{{0}" +
#endif
				"        Test.InnerType x;{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		#endregion Override implementation of CodeGeneratorFromTypeTestBase

		[Test]
		public void EscapePropertyName ()
		{
			CodeNamespace cns = new CodeNamespace ();
			CodeTypeDeclaration ctd = new CodeTypeDeclaration ("TestType");
			CodeMemberProperty f = new CodeMemberProperty ();
			f.Type = new CodeTypeReference (typeof (string));
			f.Name = "default";
			f.GetStatements.Add (new CodeMethodReturnStatement (
				new CodePrimitiveExpression (null)));
			ctd.Members.Add (f);
			cns.Types.Add (ctd);
			CSharpCodeProvider p = new CSharpCodeProvider ();
			StringWriter sw = new StringWriter ();
			p.CreateGenerator ().GenerateCodeFromNamespace (cns, sw, null);
			Assert.IsTrue (sw.ToString ().IndexOf ("@default") > 0);
		}

#if NET_2_0
		[Test]
		public void GenericCodeTypeReferencesTest ()
		{
			string code = GenerateGenericCodeTypeReferences (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public class Test {{{0}" +
				"    {0}" +
				"    private System.Nullable<int> Foo;{0}" +
				"    {0}" +
				"    private System.Nullable<> Bar;{0}" +
				"}}{0}", NewLine), code);
		}
#endif
		
#if NET_2_0
		[Test]
		public override void PartialTypeTest ()
		{
			string code = GeneratePartialType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public partial class Test1 {{{0}" +
				"}}{0}", NewLine), code);
		}
#endif
	}

	[TestFixture]
	public class CodeGeneratorFromTypeTest_Delegate : CodeGeneratorFromTypeTestBase
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
			string code = GenerateDefaultType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void ();{0}", NewLine), code);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public override void NullTypeTest ()
		{
			GenerateNullType (Options);
		}

		[Test]
		public override void SimpleTypeTest ()
		{
			string code = GenerateSimpleType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}", NewLine), code);
		}

		[Test]
		public override void DerivedTypeTest ()
		{
			string code = GenerateDerivedType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"delegate void Test1();{0}", NewLine), code);
		}

		[Test]
		public override void AttributesAndTypeTest ()
		{
			CodeTypeDelegate delegateDecl = new CodeTypeDelegate ();
			delegateDecl.ReturnType = new CodeTypeReference (typeof (int));

			_typeDeclaration = delegateDecl;

			string code = GenerateAttributesAndType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"[A()]{0}" +
				"[B()]{0}" +
				"public delegate int Test1();{0}", NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest1 ()
		{
			string code = GenerateEventMembersType1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest2 ()
		{
			string code = GenerateEventMembersType2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void EventImplementationTypes ()
		{
			string code = GenerateEventImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		/// <summary>
		/// Ensure no access modifiers are output if PrivateImplementationType
		/// is set.
		/// </summary>
		[Test]
		public override void EventPrivateImplementationType ()
		{
			string code = GenerateEventPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		/// <summary>
		/// If both ImplementationTypes and PrivateImplementationType are set,
		/// then only ImplementationTypes are output.
		/// </summary>
		[Test]
		public override void EventImplementationTypeOrder ()
		{
			string code = GenerateEventImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void FieldMembersAttributesTest ()
		{
			string code = GenerateFieldMembersAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void FieldMembersTypeTest ()
		{
			string code = GenerateFieldMembersType (MemberAttributes.Public, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void FieldNewSlotTest ()
		{
			string code = GenerateFieldMembersType (MemberAttributes.Assembly |
				MemberAttributes.New, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest1 ()
		{
			string code = GeneratePropertyMembersAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest2 ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Public,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeSetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetSet ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeFamilyOrAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.FamilyOrAssembly,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyParametersTest ()
		{
			string code = GeneratePropertyParameters (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest1 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest2 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerGetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				true, false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerSetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				false, true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypes ()
		{
			string code = GeneratePropertyImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest1 ()
		{
			string code = GeneratePropertyOverloads1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest2 ()
		{
			string code = GeneratePropertyOverloads2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest3 ()
		{
			string code = GeneratePropertyOverloads3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyPrivateImplementationType ()
		{
			string code = GeneratePropertyPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypeOrder ()
		{
			string code = GeneratePropertyImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyNewSlotTest ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Private |
				MemberAttributes.New, true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest1 ()
		{
			string code = GenerateMethodMembersType1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest2 ()
		{
			string code = GenerateMethodMembersType2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest3 ()
		{
			string code = GenerateMethodMembersType3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypes ()
		{
			string code = GenerateMethodImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest1 ()
		{
			string code = GenerateMethodOverloads1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest2 ()
		{
			string code = GenerateMethodOverloads2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest3 ()
		{
			string code = GenerateMethodOverloads3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodPrivateImplementationType ()
		{
			string code = GenerateMethodPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypeOrder ()
		{
			string code = GenerateMethodImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodParamArrayAttribute ()
		{
			string code = GenerateMethodParamArrayAttribute (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodReturnTypeAttributes ()
		{
			string code = GenerateMethodReturnTypeAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodNewSlotTest ()
		{
			string code = GenerateMethodNewSlot (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorAttributesTest ()
		{
			string code = GenerateConstructorAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorParametersTest ()
		{
			string code = GenerateConstructorParameters (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorParameterAttributesTest ()
		{
			string code = GenerateConstructorParameterAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void BaseConstructorSingleArg ()
		{
			string code = GenerateBaseConstructor (false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void BaseConstructorMultipleArgs ()
		{
			string code = GenerateBaseConstructor (true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void ChainedConstructorSingleArg ()
		{
			string code = GenerateChainedConstructor (false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void ChainedConstructorMultipleArgs ()
		{
			string code = GenerateChainedConstructor (true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void TypeConstructorTest ()
		{
			string code = GenerateTypeConstructor (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}", NewLine), code);
		}

		[Test]
		public override void EntryPointMethodTest ()
		{
			string code = GenerateEntryPointMethod (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}{0}" +
#if NET_2_0
				"[A()]{0}" +
				"public static int Main() {{{0}" +
#else
				"public static void Main() {{{0}" +
#endif
				"    Test.InnerType x;{0}" +
				"}}{0}", NewLine), code);
		}

#if NET_2_0
		[Test]
		public override void PartialTypeTest ()
		{
			string code = GeneratePartialType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public delegate void Test1();{0}"
				, NewLine), code);
		}
#endif
		
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
			string code = GenerateDefaultType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface  {{{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void DefaultTypeTest_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GenerateDefaultType (options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface {0}" + 
				"{{{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public override void NullTypeTest ()
		{
			GenerateNullType (Options);
		}

		[Test]
		public override void SimpleTypeTest ()
		{
			string code = GenerateSimpleType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void SimpleTypeTest_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GenerateSimpleType (options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1{0}" + 
				"{{{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void DerivedTypeTest ()
		{
			string code = GenerateDerivedType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
#if NET_2_0
				"internal interface Test1 : int, System.Security.Principal.IIdentity, string, System.Security.IPermission {{{0}" +
#else
				"interface Test1 : int, System.Security.Principal.IIdentity, string, System.Security.IPermission {{{0}" +
#endif
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void AttributesAndTypeTest ()
		{
			string code = GenerateAttributesAndType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"[A()]{0}" +
				"[B()]{0}" +
				"public interface Test1 {{{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest1 ()
		{
			string code = GenerateEventMembersType1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private event void ;{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest2 ()
		{
			string code = GenerateEventMembersType2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    public event int Click;{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void EventImplementationTypes ()
		{
			string code = GenerateEventImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    internal event int Click;{0}" +
#else
				"    /*FamANDAssem*/ internal event int Click;{0}" +
#endif
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void EventPrivateImplementationType ()
		{
			string code = GenerateEventPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    event int System.Int32.Click;{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void EventImplementationTypeOrder ()
		{
			string code = GenerateEventImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    event int System.Int32.Click;{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void FieldMembersAttributesTest ()
		{
			string code = GenerateFieldMembersAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void FieldMembersTypeTest ()
		{
			string code = GenerateFieldMembersType (MemberAttributes.Public, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void FieldNewSlotTest ()
		{
			string code = GenerateFieldMembersType (MemberAttributes.Assembly |
				MemberAttributes.New, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest1 ()
		{
			string code = GeneratePropertyMembersAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    void  {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest2 ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Public,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"        get;{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeSetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"        set;{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetSet ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"        get;{0}" +
				"        set;{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void PropertyMembersTypeGetSet_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, true, options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1{0}" + 
				"{{{0}" +
				"    {0}" +
				"    int Name{0}" + 
				"    {{{0}" +
				"        get;{0}" +
				"        set;{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeFamilyOrAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.FamilyOrAssembly,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyParametersTest ()
		{
			string code = GeneratePropertyParameters (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest1 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int this[object value1, ref int value2] {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest2 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int iTem {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerGetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				true, false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int this[object value1, ref int value2] {{{0}" +
				"        get;{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerSetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				false, true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int this[object value1, ref int value2] {{{0}" +
				"        set;{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypes ()
		{
			string code = GeneratePropertyImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest1 ()
		{
			string code = GeneratePropertyOverloads1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest2 ()
		{
			string code = GeneratePropertyOverloads2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"    }}{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest3 ()
		{
			string code = GeneratePropertyOverloads3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"    }}{0}" +
				"    {0}" +
				"    int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyPrivateImplementationType ()
		{
			string code = GeneratePropertyPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int this[object value1] {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypeOrder ()
		{
			string code = GeneratePropertyImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int this[object value1] {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyNewSlotTest ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Private |
				MemberAttributes.New, true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    new int Name {{{0}" +
				"        get;{0}" +
				"        set;{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest1 ()
		{
			string code = GenerateMethodMembersType1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    void ();{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void MethodMembersTypeTest1_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GenerateMethodMembersType1 (options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1{0}" + 
				"{{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    void ();{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest2 ()
		{
			string code = GenerateMethodMembersType2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Something(object value1, object value2, out int index, ref int count);{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest3 ()
		{
			string code = GenerateMethodMembersType3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Something([A()] [B()] object value, [C(A1=false, A2=true)] [D()] out int );{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypes ()
		{
			string code = GenerateMethodImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Execute();{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest1 ()
		{
			string code = GenerateMethodOverloads1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int Execute();{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest2 ()
		{
			string code = GenerateMethodOverloads2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    void Execute();{0}" +
				"    {0}" +
				"    int Execute(object value1);{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest3 ()
		{
			string code = GenerateMethodOverloads3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    void Execute();{0}" +
				"    {0}" +
				"    int System.Int32.Execute(object value1);{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodPrivateImplementationType ()
		{
			string code = GenerateMethodPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int System.Int32.Execute(object value1);{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypeOrder ()
		{
			string code = GenerateMethodImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    int System.Int32.Execute(object value1);{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodParamArrayAttribute ()
		{
			string code = GenerateMethodParamArrayAttribute (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    int Something([A()] [B()] params out object value, [C()] ref int );{0}" +
#else
				"    int Something([A()] [System.ParamArrayAttribute()] [B()] out object value, [C()] ref int );{0}" +
#endif
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodReturnTypeAttributes ()
		{
			string code = GenerateMethodReturnTypeAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
#if NET_2_0
				"    params{0}" +
#else
				"    [System.ParamArrayAttribute()]{0}" +
#endif
				"    [return: C(A1=false, A2=true)]{0}" +
#if !NET_2_0
				"    [return: System.ParamArrayAttribute()]{0}" +
#endif
				"    [return: D()]{0}" +
#if NET_2_0
				"    return: params{0}" +
#endif
				"    int Execute();{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodNewSlotTest ()
		{
			string code = GenerateMethodNewSlot (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"    new int Execute();{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorAttributesTest ()
		{
			string code = GenerateConstructorAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void ConstructorAttributesTest_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GenerateConstructorAttributes (options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1{0}" + 
				"{{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorParametersTest ()
		{
			string code = GenerateConstructorParameters (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorParameterAttributesTest ()
		{
			string code = GenerateConstructorParameterAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code, "#1");
		}

		[Test]
		public override void BaseConstructorSingleArg ()
		{
			string code = GenerateBaseConstructor (false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void BaseConstructorMultipleArgs ()
		{
			string code = GenerateBaseConstructor (true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void ChainedConstructorSingleArg ()
		{
			string code = GenerateChainedConstructor (false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void ChainedConstructorMultipleArgs ()
		{
			string code = GenerateChainedConstructor (true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void TypeConstructorTest ()
		{
			string code = GenerateTypeConstructor (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void TypeConstructorTest_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GenerateTypeConstructor (options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1{0}" + 
				"{{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void EntryPointMethodTest ()
		{
			string code = GenerateEntryPointMethod (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public interface Test1 {{{0}" + 
				"    {0}" +
#if NET_2_0
				"    [A()]{0}" +
				"    public static int Main() {{{0}" +
#else
				"    public static void Main() {{{0}" +
#endif
				"        Test.InnerType x;{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

#if NET_2_0
		[Test]
		public override void PartialTypeTest ()
		{
			string code = GeneratePartialType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public partial interface Test1 {{{0}" +
				"}}{0}", NewLine), code);
		}
#endif
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
			string code = GenerateDefaultType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct  {{{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void DefaultTypeTest_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GenerateDefaultType (options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct {0}" + 
				"{{{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public override void NullTypeTest ()
		{
			GenerateNullType (Options);
		}

		[Test]
		public override void SimpleTypeTest ()
		{
			string code = GenerateSimpleType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void SimpleTypeTest_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GenerateSimpleType (options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1{0}" + 
				"{{{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void DerivedTypeTest ()
		{
			string code = GenerateDerivedType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
#if NET_2_0
				"internal struct Test1 : int, System.Security.Principal.IIdentity, string, System.Security.IPermission {{{0}" +
#else
				"struct Test1 : int, System.Security.Principal.IIdentity, string, System.Security.IPermission {{{0}" +
#endif
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void AttributesAndTypeTest ()
		{
			string code = GenerateAttributesAndType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"[A()]{0}" +
				"[B()]{0}" +
				"public struct Test1 {{{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest1 ()
		{
			string code = GenerateEventMembersType1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private event void ;{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest2 ()
		{
			string code = GenerateEventMembersType2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public event int Click;{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void EventImplementationTypes ()
		{
			string code = GenerateEventImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    internal event int Click;{0}" +
#else
				"    /*FamANDAssem*/ internal event int Click;{0}" +
#endif
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void EventPrivateImplementationType ()
		{
			string code = GenerateEventPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    event int System.Int32.Click;{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void EventImplementationTypeOrder ()
		{
			string code = GenerateEventImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    event int System.Int32.Click;{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void FieldMembersAttributesTest ()
		{
			string code = GenerateFieldMembersAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private void ;{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void FieldMembersTypeTest ()
		{
			string code = GenerateFieldMembersType (MemberAttributes.Public, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public int Name = 2;{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void FieldNewSlotTest ()
		{
			string code = GenerateFieldMembersType (MemberAttributes.Assembly |
				MemberAttributes.New, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    internal new int Name = 2;{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest1 ()
		{
			string code = GeneratePropertyMembersAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private void  {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest2 ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Public,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    protected virtual int Name {{{0}" +
				"        get {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeSetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, true, Options);
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
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetSet ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    protected virtual int Name {{{0}" +
				"        get {{{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void PropertyMembersTypeGetSet_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, true, options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1{0}" + 
				"{{{0}" +
				"    {0}" +
				"    protected virtual int Name{0}" + 
				"    {{{0}" +
				"        get{0}" + 
				"        {{{0}" +
				"        }}{0}" +
				"        set{0}" + 
				"        {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeFamilyOrAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.FamilyOrAssembly,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    protected internal int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    internal virtual int Name {{{0}" +
#else
				"    internal int Name {{{0}" +
#endif
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyParametersTest ()
		{
			string code = GeneratePropertyParameters (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest1 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual int this[object value1, ref int value2] {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest2 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual int iTem {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerGetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				true, false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    protected virtual int this[object value1, ref int value2] {{{0}" +
				"        get {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerSetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				false, true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    protected virtual int this[object value1, ref int value2] {{{0}" +
				"        set {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypes ()
		{
			string code = GeneratePropertyImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest1 ()
		{
			string code = GeneratePropertyOverloads1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    protected virtual int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest2 ()
		{
			string code = GeneratePropertyOverloads2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"    {0}" +
				"    private int Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest3 ()
		{
			string code = GeneratePropertyOverloads3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Name {{{0}" +
				"    }}{0}" +
				"    {0}" +
				"    int System.Int32.Name {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyPrivateImplementationType ()
		{
			string code = GeneratePropertyPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    int System.Int32.this[object value1] {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypeOrder ()
		{
			string code = GeneratePropertyImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    int System.Int32.this[object value1] {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyNewSlotTest ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Private |
				MemberAttributes.New, true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    private new int Name {{{0}" +
				"        get {{{0}" +
				"        }}{0}" +
				"        set {{{0}" +
				"        }}{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest1 ()
		{
			string code = GenerateMethodMembersType1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private void () {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void MethodMembersTypeTest1_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GenerateMethodMembersType1 (options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1{0}" + 
				"{{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private void (){0}" + 
				"    {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest2 ()
		{
			string code = GenerateMethodMembersType2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Something(object value1, object value2, out int index, ref int count) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest3 ()
		{
			string code = GenerateMethodMembersType3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual int Something([A()] [B()] object value, [C(A1=false, A2=true)] [D()] out int ) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypes ()
		{
			string code = GenerateMethodImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    internal virtual int Execute() {{{0}" +
#else
				"    internal int Execute() {{{0}" +
#endif
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest1 ()
		{
			string code = GenerateMethodOverloads1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    internal virtual int Execute() {{{0}" +
#else
				"    internal int Execute() {{{0}" +
#endif
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest2 ()
		{
			string code = GenerateMethodOverloads2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual void Execute() {{{0}" +
				"    }}{0}" +
				"    {0}" +
				"    private int Execute(object value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest3 ()
		{
			string code = GenerateMethodOverloads3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public virtual void Execute() {{{0}" +
				"    }}{0}" +
				"    {0}" +
				"    int System.Int32.Execute(object value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodPrivateImplementationType ()
		{
			string code = GenerateMethodPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    int System.Int32.Execute(object value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypeOrder ()
		{
			string code = GenerateMethodImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    int System.Int32.Execute(object value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodParamArrayAttribute ()
		{
			string code = GenerateMethodParamArrayAttribute (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    public virtual int Something([A()] [B()] params out object value, [C()] ref int ) {{{0}" +
#else
				"    public virtual int Something([A()] [System.ParamArrayAttribute()] [B()] out object value, [C()] ref int ) {{{0}" +
#endif
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodReturnTypeAttributes ()
		{
			string code = GenerateMethodReturnTypeAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
#if NET_2_0
				"    params{0}" +
#else
				"    [System.ParamArrayAttribute()]{0}" +
#endif
				"    [return: C(A1=false, A2=true)]{0}" +
#if !NET_2_0
				"    [return: System.ParamArrayAttribute()]{0}" +
#endif
				"    [return: D()]{0}" +
#if NET_2_0
				"    return: params{0}" +
#endif
				"    public virtual int Execute() {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodNewSlotTest ()
		{
			string code = GenerateMethodNewSlot (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public new virtual int Execute() {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorAttributesTest ()
		{
			string code = GenerateConstructorAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private Test1() {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void ConstructorAttributesTest_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GenerateConstructorAttributes (options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1{0}" + 
				"{{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    private Test1(){0}" + 
				"    {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorParametersTest ()
		{
			string code = GenerateConstructorParameters (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public Test1(object value1, object value2, out int index, ref int count) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorParameterAttributesTest ()
		{
			string code = GenerateConstructorParameterAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    private Test1([A()] [B()] object value, [C(A1=false, A2=true)] [D()] out int index) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void BaseConstructorSingleArg ()
		{
			string code = GenerateBaseConstructor (false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    protected Test1(object value1, out int value2) : {0}" +
				"            base(value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void BaseConstructorMultipleArgs ()
		{
			string code = GenerateBaseConstructor (true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    protected Test1(object value1, out int value2) : {0}" +
				"            base(value1, value2) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void ChainedConstructorSingleArg ()
		{
			string code = GenerateChainedConstructor (false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public Test1(object value1, out int value2) : {0}" +
				"            base(value3) : {0}" +
				"            this(value1) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void ChainedConstructorMultipleArgs ()
		{
			string code = GenerateChainedConstructor (true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
				"    public Test1(object value1, out int value2) : {0}" +
				"            base(value3) : {0}" +
				"            this(value1, value2) {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void TypeConstructorTest ()
		{
			string code = GenerateTypeConstructor (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    [A()]{0}" +
				"    [B()]{0}" +
#endif
				"    static Test1() {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code, "#1");
		}

		[Test]
		public void TypeConstructorTest_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GenerateTypeConstructor (options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1{0}" +
				"{{{0}" +
				"    {0}" +
#if NET_2_0
				"    [A()]{0}" +
				"    [B()]{0}" +
#endif
				"    static Test1(){0}" + 
				"    {{{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code, "#2");
		}

		[Test]
		public override void EntryPointMethodTest ()
		{
			string code = GenerateEntryPointMethod (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public struct Test1 {{{0}" +
				"    {0}" +
#if NET_2_0
				"    [A()]{0}" +
				"    public static int Main() {{{0}" +
#else
				"    public static void Main() {{{0}" +
#endif
				"        Test.InnerType x;{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

#if NET_2_0
		[Test]
		public override void PartialTypeTest ()
		{
			string code = GeneratePartialType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public partial struct Test1 {{{0}" +
				"}}{0}", NewLine), code);
		}
#endif
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
			string code = GenerateDefaultType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum  {{{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void DefaultTypeTest_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GenerateDefaultType (options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum {0}" + 
				"{{{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public override void NullTypeTest ()
		{
			GenerateNullType (Options);
		}

		[Test]
		public override void SimpleTypeTest ()
		{
			string code = GenerateSimpleType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void SimpleTypeTest_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GenerateSimpleType (options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1{0}" + 
				"{{{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void DerivedTypeTest ()
		{
			string code = GenerateDerivedType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
#if NET_2_0
				"internal enum Test1 : int, System.Security.Principal.IIdentity, string, System.Security.IPermission {{{0}" +
#else
				"enum Test1 : int, System.Security.Principal.IIdentity, string, System.Security.IPermission {{{0}" +
#endif
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void AttributesAndTypeTest ()
		{
			string code = GenerateAttributesAndType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"[A()]{0}" +
				"[B()]{0}" +
				"public enum Test1 {{{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest1 ()
		{
			string code = GenerateEventMembersType1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest2 ()
		{
			string code = GenerateEventMembersType2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void EventImplementationTypes ()
		{
			string code = GenerateEventImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void EventPrivateImplementationType ()
		{
			string code = GenerateEventPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void EventImplementationTypeOrder ()
		{
			string code = GenerateEventImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void FieldMembersAttributesTest ()
		{
			string code = GenerateFieldMembersAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"    [A()]{0}" +
				"    [B()]{0}" +
				"    ,{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void FieldMembersTypeTest ()
		{
			string code = GenerateFieldMembersType (MemberAttributes.Public, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"    Name = 2,{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void FieldNewSlotTest ()
		{
			string code = GenerateFieldMembersType (MemberAttributes.Assembly |
				MemberAttributes.New, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"    Name = 2,{0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest1 ()
		{
			string code = GeneratePropertyMembersAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest2 ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Public,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeSetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetSet ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void PropertyMembersTypeGetSet_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, true, options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1{0}" + 
				"{{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeFamilyOrAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.FamilyOrAssembly,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyParametersTest ()
		{
			string code = GeneratePropertyParameters (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest1 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest2 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerGetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				true, false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerSetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				false, true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypes ()
		{
			string code = GeneratePropertyImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest1 ()
		{
			string code = GeneratePropertyOverloads1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest2 ()
		{
			string code = GeneratePropertyOverloads2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest3 ()
		{
			string code = GeneratePropertyOverloads3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyPrivateImplementationType ()
		{
			string code = GeneratePropertyPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypeOrder ()
		{
			string code = GeneratePropertyImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyNewSlotTest ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Private |
				MemberAttributes.New, true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest1 ()
		{
			string code = GenerateMethodMembersType1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest2 ()
		{
			string code = GenerateMethodMembersType2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest3 ()
		{
			string code = GenerateMethodMembersType3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypes ()
		{
			string code = GenerateMethodImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest1 ()
		{
			string code = GenerateMethodOverloads1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest2 ()
		{
			string code = GenerateMethodOverloads2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest3 ()
		{
			string code = GenerateMethodOverloads3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodPrivateImplementationType ()
		{
			string code = GenerateMethodPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypeOrder ()
		{
			string code = GenerateMethodImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodNewSlotTest ()
		{
			string code = GenerateMethodNewSlot (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodParamArrayAttribute ()
		{
			string code = GenerateMethodParamArrayAttribute (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void MethodReturnTypeAttributes ()
		{
			string code = GenerateMethodReturnTypeAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorAttributesTest ()
		{
			string code = GenerateConstructorAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void ConstructorAttributesTest_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GenerateConstructorAttributes (options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1{0}" + 
				"{{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorParametersTest ()
		{
			string code = GenerateConstructorParameters (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorParameterAttributesTest ()
		{
			string code = GenerateConstructorParameterAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void BaseConstructorSingleArg ()
		{
			string code = GenerateBaseConstructor (false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void BaseConstructorMultipleArgs ()
		{
			string code = GenerateBaseConstructor (true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void ChainedConstructorSingleArg ()
		{
			string code = GenerateChainedConstructor (false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void ChainedConstructorMultipleArgs ()
		{
			string code = GenerateChainedConstructor (true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void TypeConstructorTest ()
		{
			string code = GenerateTypeConstructor (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public void TypeConstructorTest_C ()
		{
			CodeGeneratorOptions options = new CodeGeneratorOptions ();
			options.BracingStyle = "C";

			string code = GenerateTypeConstructor (options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1{0}" + 
				"{{{0}" +
				"    {0}" +
				"}}{0}", NewLine), code);
		}

		[Test]
		public override void EntryPointMethodTest ()
		{
			string code = GenerateEntryPointMethod (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" + 
				"    {0}" +
#if NET_2_0
				"    [A()]{0}" +
				"    public static int Main() {{{0}" +
#else
				"    public static void Main() {{{0}" +
#endif
				"        Test.InnerType x;{0}" +
				"    }}{0}" +
				"}}{0}", NewLine), code);
		}

#if NET_2_0
		[Test]
		public override void PartialTypeTest ()
		{
			string code = GeneratePartialType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"public enum Test1 {{{0}" +
				"}}{0}", NewLine), code);
		}
#endif
		
		#endregion Override implementation of CodeGeneratorFromTypeTestBase
	}
}
