//
// Microsoft.VisualBasic.* Test Cases
//
// Authors:
// Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) Novell
//

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Globalization;

using Microsoft.VisualBasic;

using NUnit.Framework;

using MonoTests.System.CodeDom.Compiler;

namespace MonoTests.Microsoft.VisualBasic
{
	[TestFixture]
	public class CodeGeneratorFromTypeTest_Class : CodeGeneratorFromTypeTestBase
	{
		private CodeTypeDeclaration _typeDeclaration;
		private ICodeGenerator _codeGenerator;

		protected override ICodeGenerator CodeGenerator
		{
			get { return _codeGenerator; }
		}

		protected override CodeTypeDeclaration TypeDeclaration
		{
			get { return _typeDeclaration; }
		}

		[SetUp]
		public override void SetUp ()
		{
			base.SetUp ();
			_typeDeclaration = new CodeTypeDeclaration ();

			CodeDomProvider provider = new VBCodeProvider ();
			_codeGenerator = provider.CreateGenerator ();
		}

		[Test]
		public override void DefaultTypeTest ()
		{
			string code = GenerateDefaultType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class {0}" +
				"End Class{0}", NewLine), code);
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
				"Public Class Test1{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void DerivedTypeTest ()
		{
			string code = GenerateDerivedType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Friend MustInherit Class Test1{0}" +
				"    Inherits Integer{0}" +
				"    Implements System.Security.Principal.IIdentity, String, System.Security.IPermission{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void AttributesAndTypeTest ()
		{
			string code = GenerateAttributesAndType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<A(),  _{0}" +
				" B()>  _{0}" +
				"Public Class Test1{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest1 ()
		{
			string code = GenerateEventMembersType1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    <A(),  _{0}" +
				"     B()>  _{0}" +
				"    Private Event __exception As System.Void{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest2 ()
		{
			string code = GenerateEventMembersType2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Public Event Click As Integer{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void EventImplementationTypes ()
		{
			string code = GenerateEventImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Friend Event Click As Integer Implements IPolicy.Click , IWhatever.Click{0}" +
				"End Class{0}", NewLine), code);
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
				"Public Class Test1{0}" +
				"    {0}" +
				"    Protected Event System_Int32_Click As Integer Implements Integer.Click{0}" +
				"End Class{0}", NewLine), code);
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
				"Public Class Test1{0}" +
				"    {0}" +
				"    Public Event System_Int32_Click As Integer Implements IPolicy.Click{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void FieldMembersAttributesTest ()
		{
			string code = GenerateFieldMembersAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    <A(),  _{0}" +
				"     B()>  _{0}" +
				"    Private __exception As System.Void{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void FieldMembersTypeTest ()
		{
			string code = GenerateFieldMembersType (MemberAttributes.Public, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Public Name As Integer = 2{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void FieldNewSlotTest ()
		{
			string code = GenerateFieldMembersType (MemberAttributes.Assembly |
				MemberAttributes.New, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Friend Shadows Name As Integer = 2{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public void AbstractPropertyTest ()
		{
			string code = GenerateAbstractProperty (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public MustInherit Class Test1{0}" +
				"    {0}" +
				"    Public MustOverride Property Name() As String{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public void StaticPropertyTest ()
		{
			string code = GenerateStaticProperty (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Public Shared WriteOnly Property Name() As String{0}" +
				"        Set{0}" +
				"        End Set{0}" +
				"    End Property{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest1 ()
		{
			string code = GeneratePropertyMembersAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    <A(),  _{0}" +
				"     B()>  _{0}" +
				"    Private Property () As System.Void{0}" +
				"    End Property{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest2 ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Public,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Public Overridable Property Name() As Integer{0}" +
				"    End Property{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Protected Overridable ReadOnly Property Name() As Integer{0}" +
				"        Get{0}" +
				"        End Get{0}" +
				"    End Property{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeSetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Friend Overridable WriteOnly Property Name() As Integer{0}" +
				"        Set{0}" +
				"        End Set{0}" +
				"    End Property{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetSet ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Protected Overridable Property Name() As Integer{0}" +
				"        Get{0}" +
				"        End Get{0}" +
				"        Set{0}" +
				"        End Set{0}" +
				"    End Property{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeFamilyOrAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.FamilyOrAssembly,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Protected Friend Property Name() As Integer{0}" +
				"    End Property{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Friend Overridable Property Name() As Integer{0}" +
				"    End Property{0}" +
				"End Class{0}", NewLine), code);
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
				"Public Class Test1{0}" +
				"    {0}" +
				"    Public Overridable Property Name(ByVal value1 As Object, ByRef value2 As Integer) As Integer{0}" +
				"    End Property{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest1 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Public Overridable Default Property iTem(ByVal value1 As Object, ByRef value2 As Integer) As Integer{0}" +
				"    End Property{0}" +
				"End Class{0}", NewLine), code);
		}

		/// <summary>
		/// Ensures Default keyword is only output if property is named "Item"
		/// (case-insensitive comparison) AND parameters are defined.
		/// </summary>
		[Test]
		public override void PropertyIndexerTest2 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Public Overridable Property iTem() As Integer{0}" +
				"    End Property{0}" +
				"End Class{0}", NewLine), code);
		}

		/// <summary>
		/// Ensures Default keyword is output after ReadOnly modifier.
		/// </summary>
		[Test]
		public override void PropertyIndexerGetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				true, false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Protected Overridable Default ReadOnly Property iTem(ByVal value1 As Object, ByRef value2 As Integer) As Integer{0}" +
				"        Get{0}" +
				"        End Get{0}" +
				"    End Property{0}" +
				"End Class{0}", NewLine), code);
		}

		/// <summary>
		/// Ensures Default keyword is output after WriteOnly modifier.
		/// </summary>
		[Test]
		public override void PropertyIndexerSetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				false, true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Protected Overridable Default WriteOnly Property iTem(ByVal value1 As Object, ByRef value2 As Integer) As Integer{0}" +
				"        Set{0}" +
				"        End Set{0}" +
				"    End Property{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypes ()
		{
			string code = GeneratePropertyImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Public Overridable Property Name() As Integer Implements IPolicy.Name , IWhatever.Name{0}" +
				"    End Property{0}" +
				"End Class{0}", NewLine), code);
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
				"Public Class Test1{0}" +
				"    {0}" +
				"    Protected Overloads Overridable Property Name() As Integer{0}" +
				"    End Property{0}" +
				"End Class{0}", NewLine), code);
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
				"Public Class Test1{0}" +
				"    {0}" +
				"    Public Overloads Overridable Property Name() As Integer{0}" +
				"    End Property{0}" +
				"    {0}" +
				"    Private Overloads Property Name(ByVal value1 As Object) As Integer{0}" +
				"    End Property{0}" +
				"End Class{0}", NewLine), code);
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
				"Public Class Test1{0}" +
				"    {0}" +
				"    Public Overridable Property Name() As Integer{0}" +
				"    End Property{0}" +
				"    {0}" +
				"    Property System_Int32_Name(ByVal value1 As Object) As Integer Implements Integer.Name{0}" +
				"    End Property{0}" +
				"End Class{0}", NewLine), code);
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
				"Public Class Test1{0}" +
				"    {0}" +
				"    Overridable Property System_Int32_Item(ByVal value1 As Object) As Integer Implements Integer.Item{0}" +
				"    End Property{0}" +
				"End Class{0}", NewLine), code);
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
				"Public Class Test1{0}" +
				"    {0}" +
				"    Overridable Property System_Int32_Item(ByVal value1 As Object) As Integer Implements IPolicy.Item{0}" +
				"    End Property{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void PropertyNewSlotTest ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Private |
				MemberAttributes.New, true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Private Shadows Property Name() As Integer{0}" +
				"        Get{0}" +
				"        End Get{0}" +
				"        Set{0}" +
				"        End Set{0}" +
				"    End Property{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest1 ()
		{
			string code = GenerateMethodMembersType1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    <A(),  _{0}" +
				"     B()>  _{0}" +
				"    Private Sub (){0}" +
				"    End Sub{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest2 ()
		{
			string code = GenerateMethodMembersType2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Public Overridable Function Something(ByVal value1 As Object, ByVal value2 As Object, ByRef index As Integer, ByRef count As Integer) As Integer{0}" +
				"    End Function{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest3 ()
		{
			string code = GenerateMethodMembersType3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Public Overridable Function Something(<A(), B()> ByVal value As Object, <C(A1:=false, A2:=true), D()> ByRef __exception As Integer) As Integer{0}" +
				"    End Function{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypes ()
		{
			string code = GenerateMethodImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Friend Overridable Function Execute() As Integer Implements IPolicy.Execute , IWhatever.Execute{0}" +
				"    End Function{0}" +
				"End Class{0}", NewLine), code);
		}

		/// <summary>
		/// Ensure that Overloads keyword is output for a method which has
		/// explicitly been marked as Overloaded.
		/// </summary>
		[Test]
		public override void MethodOverloadsTest1 ()
		{
			string code = GenerateMethodOverloads1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Friend Overloads Overridable Function Execute() As Integer{0}" +
				"    End Function{0}" +
				"End Class{0}", NewLine), code);
		}

		/// <summary>
		/// Ensure that Overloads keyword is output if multiple methods with
		/// the same name are defined.
		/// </summary>
		[Test]
		public override void MethodOverloadsTest2 ()
		{
			string code = GenerateMethodOverloads2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Public Overloads Overridable Sub Execute(){0}" +
				"    End Sub{0}" +
				"    {0}" +
				"    Private Overloads Function Execute(ByVal value1 As Object) As Integer{0}" +
				"    End Function{0}" +
				"End Class{0}", NewLine), code);
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
				"Public Class Test1{0}" +
				"    {0}" +
				"    Public Overridable Sub Execute(){0}" +
				"    End Sub{0}" +
				"    {0}" +
				"    Function System_Int32_Execute(ByVal value1 As Object) As Integer Implements Integer.Execute{0}" +
				"    End Function{0}" +
				"End Class{0}", NewLine), code);
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
				"Public Class Test1{0}" +
				"    {0}" +
				"    Overridable Function System_Int32_Execute(ByVal value1 As Object) As Integer Implements Integer.Execute{0}" +
				"    End Function{0}" +
				"End Class{0}", NewLine), code);
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
				"Public Class Test1{0}" +
				"    {0}" +
				"    Overridable Function System_Int32_Execute(ByVal value1 As Object) As Integer Implements IPolicy.Execute{0}" +
				"    End Function{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void MethodParamArrayAttribute ()
		{
			string code = GenerateMethodParamArrayAttribute (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Public Overridable Function Something(<A(), System.ParamArrayAttribute(), B()> ByRef value As Object, <C()> ByRef __exception As Integer) As Integer{0}" +
				"    End Function{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void MethodReturnTypeAttributes ()
		{
			string code = GenerateMethodReturnTypeAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    <A(),  _{0}" +
				"     B(),  _{0}" +
				"     System.ParamArrayAttribute()>  _{0}" +
				"    Public Overridable Function Execute() As <C(A1:=false, A2:=true), System.ParamArrayAttribute(), D()> Integer{0}" +
				"    End Function{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void MethodNewSlotTest ()
		{
			string code = GenerateMethodNewSlot (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Public Shadows Overridable Function Execute() As Integer{0}" +
				"    End Function{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorAttributesTest ()
		{
			// FIXME: updated to reflect mbas workaround
			string code = GenerateConstructorAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    <A(),  _{0}" +
				"     B()>  _{0}" +
				"    Private Sub New(){0}" +
				"        MyBase.New{0}" +
				"    End Sub{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorParametersTest ()
		{
			// FIXME: updated to reflect mbas workaround
			string code = GenerateConstructorParameters (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Public Sub New(ByVal value1 As Object, ByVal value2 As Object, ByRef index As Integer, ByRef count As Integer){0}" +
				"        MyBase.New{0}" +
				"    End Sub{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorParameterAttributesTest ()
		{
			// FIXME: updated to reflect mbas workaround
			string code = GenerateConstructorParameterAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Private Sub New(<A(), B()> ByVal value As Object, <C(A1:=false, A2:=true), D()> ByRef index As Integer){0}" +
				"        MyBase.New{0}" +
				"    End Sub{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void BaseConstructorSingleArg ()
		{
			string code = GenerateBaseConstructor (false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Protected Sub New(ByVal value1 As Object, ByRef value2 As Integer){0}" +
				"        MyBase.New(value1){0}" +
				"    End Sub{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void BaseConstructorMultipleArgs ()
		{
			string code = GenerateBaseConstructor (true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Protected Sub New(ByVal value1 As Object, ByRef value2 As Integer){0}" +
				"        MyBase.New(value1, value2){0}" +
				"    End Sub{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void ChainedConstructorSingleArg ()
		{
			string code = GenerateChainedConstructor (false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Public Sub New(ByVal value1 As Object, ByRef value2 As Integer){0}" +
				"        Me.New(value1){0}" +
				"    End Sub{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void ChainedConstructorMultipleArgs ()
		{
			string code = GenerateChainedConstructor (true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    Public Sub New(ByVal value1 As Object, ByRef value2 As Integer){0}" +
				"        Me.New(value1, value2){0}" +
				"    End Sub{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void TypeConstructorTest ()
		{
			string code = GenerateTypeConstructor (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    <A(),  _{0}" +
				"     B()>  _{0}" +
				"    Shared Sub New(){0}" +
				"    End Sub{0}" +
				"End Class{0}", NewLine), code);
		}

		[Test]
		public override void EntryPointMethodTest ()
		{
			string code = GenerateEntryPointMethod (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Class Test1{0}" +
				"    {0}" +
				"    <A()>  _{0}" +
				"    Public Shared Sub Main(){0}" +
				"        Dim x As Test.InnerType{0}" +
				"    End Sub{0}" +
				"End Class{0}", NewLine), code);
		}
		
		[Test]
		public override void PartialTypeTest ()
		{
			string code = GeneratePartialType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Partial Public Class Test1{0}" +
				"End Class{0}", NewLine), code);
		}
	}

	[TestFixture]
	public class CodeGeneratorFromTypeTest_Delegate : CodeGeneratorFromTypeTestBase
	{
		private CodeTypeDeclaration _typeDeclaration;
		private ICodeGenerator _codeGenerator;

		protected override ICodeGenerator CodeGenerator
		{
			get { return _codeGenerator; }
		}

		protected override CodeTypeDeclaration TypeDeclaration
		{
			get { return _typeDeclaration; }
		}

		[SetUp]
		public override void SetUp ()
		{
			base.SetUp ();
			_typeDeclaration = new CodeTypeDelegate ();

			CodeDomProvider provider = new VBCodeProvider ();
			_codeGenerator = provider.CreateGenerator ();
		}

		[Test]
		public override void DefaultTypeTest ()
		{
			string code = GenerateDefaultType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub (){0}", NewLine), code);
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
				"Public Delegate Sub Test1(){0}", NewLine), code);
		}

		[Test]
		public override void DerivedTypeTest ()
		{
			string code = GenerateDerivedType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Delegate Sub Test1(){0}", NewLine), code);
		}

		[Test]
		public override void AttributesAndTypeTest ()
		{
			CodeTypeDelegate delegateDecl = new CodeTypeDelegate ();
			delegateDecl.ReturnType = new CodeTypeReference (typeof (int));

			_typeDeclaration = delegateDecl;

			string code = GenerateAttributesAndType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<A(),  _{0}" +
				" B()>  _{0}" +
				"Public Delegate Function Test1() As Integer{0}", NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest1 ()
		{
			string code = GenerateEventMembersType1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest2 ()
		{
			string code = GenerateEventMembersType2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void EventImplementationTypes ()
		{
			string code = GenerateEventImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
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
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
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
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void FieldMembersAttributesTest ()
		{
			string code = GenerateFieldMembersAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void FieldMembersTypeTest ()
		{
			string code = GenerateFieldMembersType (MemberAttributes.Public, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void FieldNewSlotTest ()
		{
			string code = GenerateFieldMembersType (MemberAttributes.Assembly |
				MemberAttributes.New, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest1 ()
		{
			string code = GeneratePropertyMembersAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest2 ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Public,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeSetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetSet ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeFamilyOrAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.FamilyOrAssembly,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyParametersTest ()
		{
			string code = GeneratePropertyParameters (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest1 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest2 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerGetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				true, false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerSetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				false, true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypes ()
		{
			string code = GeneratePropertyImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest1 ()
		{
			string code = GeneratePropertyOverloads1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest2 ()
		{
			string code = GeneratePropertyOverloads2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest3 ()
		{
			string code = GeneratePropertyOverloads3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyPrivateImplementationType ()
		{
			string code = GeneratePropertyPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypeOrder ()
		{
			string code = GeneratePropertyImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void PropertyNewSlotTest ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Private |
				MemberAttributes.New, true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest1 ()
		{
			string code = GenerateMethodMembersType1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest2 ()
		{
			string code = GenerateMethodMembersType2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest3 ()
		{
			string code = GenerateMethodMembersType3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypes ()
		{
			string code = GenerateMethodImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest1 ()
		{
			string code = GenerateMethodOverloads1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest2 ()
		{
			string code = GenerateMethodOverloads2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest3 ()
		{
			string code = GenerateMethodOverloads3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodPrivateImplementationType ()
		{
			string code = GenerateMethodPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypeOrder ()
		{
			string code = GenerateMethodImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodParamArrayAttribute ()
		{
			string code = GenerateMethodParamArrayAttribute (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodReturnTypeAttributes ()
		{
			string code = GenerateMethodReturnTypeAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void MethodNewSlotTest ()
		{
			string code = GenerateMethodNewSlot (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorAttributesTest ()
		{
			string code = GenerateConstructorAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorParametersTest ()
		{
			string code = GenerateConstructorParameters (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorParameterAttributesTest ()
		{
			string code = GenerateConstructorParameterAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void BaseConstructorSingleArg ()
		{
			string code = GenerateBaseConstructor (false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void BaseConstructorMultipleArgs ()
		{
			string code = GenerateBaseConstructor (true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void ChainedConstructorSingleArg ()
		{
			string code = GenerateChainedConstructor (false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void ChainedConstructorMultipleArgs ()
		{
			string code = GenerateChainedConstructor (true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void TypeConstructorTest ()
		{
			string code = GenerateTypeConstructor (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}", NewLine), code);
		}

		[Test]
		public override void EntryPointMethodTest ()
		{
			string code = GenerateEntryPointMethod (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}{0}" +
				"<A()>  _{0}" +
				"Public Shared Sub Main(){0}" +
				"    Dim x As Test.InnerType{0}" +
				"End Sub{0}", NewLine), code);
		}
		
		[Test]
		public void DelegateWithParametersTest ()
		{
			CodeTypeDelegate type = new CodeTypeDelegate("A");
			type.Parameters.Add (new CodeParameterDeclarationExpression ("type", "param"));
			
			string code = GenerateCodeFromType (type, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture, 
				"Public Delegate Sub A(ByVal param As type){0}", NewLine), code);
		}
		
		[Test]
		public override void PartialTypeTest ()
		{
			string code = GeneratePartialType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Delegate Sub Test1(){0}", NewLine), code);
		}
	}

	[TestFixture]
	public class CodeGeneratorFromTypeTest_Interface : CodeGeneratorFromTypeTestBase
	{
		private CodeTypeDeclaration _typeDeclaration;
		private ICodeGenerator _codeGenerator;

		protected override ICodeGenerator CodeGenerator
		{
			get { return _codeGenerator; }
		}

		protected override CodeTypeDeclaration TypeDeclaration
		{
			get { return _typeDeclaration; }
		}

		[SetUp]
		public override void SetUp ()
		{
			base.SetUp ();
			_typeDeclaration = new CodeTypeDeclaration ();
			_typeDeclaration.IsInterface = true;

			CodeDomProvider provider = new VBCodeProvider ();
			_codeGenerator = provider.CreateGenerator ();
		}

		[Test]
		public override void DefaultTypeTest ()
		{
			string code = GenerateDefaultType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface {0}" +
				"End Interface{0}", NewLine), code);
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
				"Public Interface Test1{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void DerivedTypeTest ()
		{
			string code = GenerateDerivedType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Friend Interface Test1{0}" +
				"    Inherits Integer, System.Security.Principal.IIdentity, String, System.Security.IPermission{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void AttributesAndTypeTest ()
		{
			string code = GenerateAttributesAndType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<A(),  _{0}" +
				" B()>  _{0}" +
				"Public Interface Test1{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest1 ()
		{
			string code = GenerateEventMembersType1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    <A(),  _{0}" +
				"     B()>  _{0}" +
				"    Private Event __exception As System.Void{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest2 ()
		{
			string code = GenerateEventMembersType2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Public Event Click As Integer{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void EventImplementationTypes ()
		{
			string code = GenerateEventImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Friend Event Click As Integer Implements IPolicy.Click , IWhatever.Click{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void EventPrivateImplementationType ()
		{
			string code = GenerateEventPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Protected Event System_Int32_Click As Integer Implements Integer.Click{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void EventImplementationTypeOrder ()
		{
			string code = GenerateEventImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Public Event System_Int32_Click As Integer Implements IPolicy.Click{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void FieldMembersAttributesTest ()
		{
			string code = GenerateFieldMembersAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void FieldMembersTypeTest ()
		{
			string code = GenerateFieldMembersType (MemberAttributes.Public, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void FieldNewSlotTest ()
		{
			string code = GenerateFieldMembersType (MemberAttributes.Assembly |
				MemberAttributes.New, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest1 ()
		{
			string code = GeneratePropertyMembersAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    <A(),  _{0}" +
				"     B()>  _{0}" +
				"    Property () As System.Void{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest2 ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Public,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Property Name() As Integer{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    ReadOnly Property Name() As Integer{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeSetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    WriteOnly Property Name() As Integer{0}" +
 				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetSet ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Property Name() As Integer{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeFamilyOrAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.FamilyOrAssembly,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Property Name() As Integer{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Property Name() As Integer{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void PropertyParametersTest ()
		{
			string code = GeneratePropertyParameters (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Property Name(ByVal value1 As Object, ByRef value2 As Integer) As Integer{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest1 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Default Property iTem(ByVal value1 As Object, ByRef value2 As Integer) As Integer{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest2 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Property iTem() As Integer{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerGetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				true, false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Default ReadOnly Property iTem(ByVal value1 As Object, ByRef value2 As Integer) As Integer{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerSetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				false, true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Default WriteOnly Property iTem(ByVal value1 As Object, ByRef value2 As Integer) As Integer{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypes ()
		{
			string code = GeneratePropertyImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Property Name() As Integer Implements IPolicy.Name , IWhatever.Name{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest1 ()
		{
			string code = GeneratePropertyOverloads1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Property Name() As Integer{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest2 ()
		{
			string code = GeneratePropertyOverloads2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Property Name() As Integer{0}" +
 				"    {0}" +
				"    Property Name(ByVal value1 As Object) As Integer{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest3 ()
		{
			string code = GeneratePropertyOverloads3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Property Name() As Integer{0}" +
				"    {0}" +
				"    Property System_Int32_Name(ByVal value1 As Object) As Integer Implements Integer.Name{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void PropertyPrivateImplementationType ()
		{
			string code = GeneratePropertyPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Property System_Int32_Item(ByVal value1 As Object) As Integer Implements Integer.Item{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypeOrder ()
		{
			string code = GeneratePropertyImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Property System_Int32_Item(ByVal value1 As Object) As Integer Implements IPolicy.Item{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void PropertyNewSlotTest ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Private |
				MemberAttributes.New, true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Shadows Property Name() As Integer{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest1 ()
		{
			string code = GenerateMethodMembersType1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    <A(),  _{0}" +
				"     B()>  _{0}" +
				"    Sub (){0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest2 ()
		{
			string code = GenerateMethodMembersType2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Function Something(ByVal value1 As Object, ByVal value2 As Object, ByRef index As Integer, ByRef count As Integer) As Integer{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest3 ()
		{
			string code = GenerateMethodMembersType3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Function Something(<A(), B()> ByVal value As Object, <C(A1:=false, A2:=true), D()> ByRef __exception As Integer) As Integer{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypes ()
		{
			string code = GenerateMethodImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Function Execute() As Integer Implements IPolicy.Execute , IWhatever.Execute{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest1 ()
		{
			string code = GenerateMethodOverloads1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Function Execute() As Integer{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest2 ()
		{
			string code = GenerateMethodOverloads2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Sub Execute(){0}" +
				"    {0}" +
				"    Function Execute(ByVal value1 As Object) As Integer{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest3 ()
		{
			string code = GenerateMethodOverloads3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Sub Execute(){0}" +
				"    {0}" +
				"    Function System_Int32_Execute(ByVal value1 As Object) As Integer Implements Integer.Execute{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void MethodPrivateImplementationType ()
		{
			string code = GenerateMethodPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Function System_Int32_Execute(ByVal value1 As Object) As Integer Implements Integer.Execute{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypeOrder ()
		{
			string code = GenerateMethodImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Function System_Int32_Execute(ByVal value1 As Object) As Integer Implements IPolicy.Execute{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void MethodParamArrayAttribute ()
		{
			string code = GenerateMethodParamArrayAttribute (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Function Something(<A(), System.ParamArrayAttribute(), B()> ByRef value As Object, <C()> ByRef __exception As Integer) As Integer{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void MethodReturnTypeAttributes ()
		{
			string code = GenerateMethodReturnTypeAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    <A(),  _{0}" +
				"     B(),  _{0}" +
				"     System.ParamArrayAttribute()>  _{0}" +
				"    Function Execute() As <C(A1:=false, A2:=true), System.ParamArrayAttribute(), D()> Integer{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void MethodNewSlotTest ()
		{
			string code = GenerateMethodNewSlot (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    Shadows Function Execute() As Integer{0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorAttributesTest ()
		{
			string code = GenerateConstructorAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorParametersTest ()
		{
			string code = GenerateConstructorParameters (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorParameterAttributesTest ()
		{
			string code = GenerateConstructorParameterAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void BaseConstructorSingleArg ()
		{
			string code = GenerateBaseConstructor (false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void BaseConstructorMultipleArgs ()
		{
			string code = GenerateBaseConstructor (true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void ChainedConstructorSingleArg ()
		{
			string code = GenerateChainedConstructor (false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void ChainedConstructorMultipleArgs ()
		{
			string code = GenerateChainedConstructor (true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void TypeConstructorTest ()
		{
			string code = GenerateTypeConstructor (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"End Interface{0}", NewLine), code);
		}

		[Test]
		public override void EntryPointMethodTest ()
		{
			string code = GenerateEntryPointMethod (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Interface Test1{0}" +
				"    {0}" +
				"    <A()>  _{0}" +
				"    Public Shared Sub Main(){0}" +
				"        Dim x As Test.InnerType{0}" +
				"    End Sub{0}" +
				"End Interface{0}", NewLine), code);
		}
		
		[Test]
		public override void PartialTypeTest ()
		{
			string code = GeneratePartialType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Partial Public Interface Test1{0}" +
				"End Interface{0}", NewLine), code);
		}
	}

	[TestFixture]
	public class CodeGeneratorFromTypeTest_Struct : CodeGeneratorFromTypeTestBase
	{
		private CodeTypeDeclaration _typeDeclaration;
		private ICodeGenerator _codeGenerator;

		protected override ICodeGenerator CodeGenerator
		{
			get { return _codeGenerator; }
		}

		protected override CodeTypeDeclaration TypeDeclaration
		{
			get { return _typeDeclaration; }
		}

		[SetUp]
		public override void SetUp ()
		{
			base.SetUp ();
			_typeDeclaration = new CodeTypeDeclaration ();
			_typeDeclaration.IsStruct = true;

			CodeDomProvider provider = new VBCodeProvider ();
			_codeGenerator = provider.CreateGenerator ();
		}

		[Test]
		public override void DefaultTypeTest ()
		{
			string code = GenerateDefaultType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure {0}" +
				"End Structure{0}", NewLine), code);
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
				"Public Structure Test1{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void DerivedTypeTest ()
		{
			string code = GenerateDerivedType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Friend Structure Test1{0}" +
				"    Implements Integer, System.Security.Principal.IIdentity, String, System.Security.IPermission{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void AttributesAndTypeTest ()
		{
			string code = GenerateAttributesAndType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<A(),  _{0}" +
				" B()>  _{0}" +
				"Public Structure Test1{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest1 ()
		{
			string code = GenerateEventMembersType1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    <A(),  _{0}" +
				"     B()>  _{0}" +
				"    Private Event __exception As System.Void{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest2 ()
		{
			string code = GenerateEventMembersType2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Public Event Click As Integer{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void EventImplementationTypes ()
		{
			string code = GenerateEventImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Friend Event Click As Integer Implements IPolicy.Click , IWhatever.Click{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void EventPrivateImplementationType ()
		{
			string code = GenerateEventPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Protected Event System_Int32_Click As Integer Implements Integer.Click{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void EventImplementationTypeOrder ()
		{
			string code = GenerateEventImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Public Event System_Int32_Click As Integer Implements IPolicy.Click{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void FieldMembersAttributesTest ()
		{
			string code = GenerateFieldMembersAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    <A(),  _{0}" +
				"     B()>  _{0}" +
				"    Private __exception As System.Void{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void FieldMembersTypeTest ()
		{
			string code = GenerateFieldMembersType (MemberAttributes.Public, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Public Name As Integer = 2{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void FieldNewSlotTest ()
		{
			string code = GenerateFieldMembersType (MemberAttributes.Assembly |
				MemberAttributes.New, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Friend Shadows Name As Integer = 2{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest1 ()
		{
			string code = GeneratePropertyMembersAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    <A(),  _{0}" +
				"     B()>  _{0}" +
				"    Private Property () As System.Void{0}" +
				"    End Property{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest2 ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Public,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Public Overridable Property Name() As Integer{0}" +
				"    End Property{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Protected Overridable ReadOnly Property Name() As Integer{0}" +
				"        Get{0}" +
				"        End Get{0}" +
				"    End Property{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeSetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Friend Overridable WriteOnly Property Name() As Integer{0}" +
				"        Set{0}" +
				"        End Set{0}" +
				"    End Property{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetSet ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Protected Overridable Property Name() As Integer{0}" +
				"        Get{0}" +
				"        End Get{0}" +
				"        Set{0}" +
				"        End Set{0}" +
				"    End Property{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeFamilyOrAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.FamilyOrAssembly,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Protected Friend Property Name() As Integer{0}" +
				"    End Property{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Friend Overridable Property Name() As Integer{0}" +
				"    End Property{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void PropertyParametersTest ()
		{
			string code = GeneratePropertyParameters (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Public Overridable Property Name(ByVal value1 As Object, ByRef value2 As Integer) As Integer{0}" +
				"    End Property{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest1 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Public Overridable Default Property iTem(ByVal value1 As Object, ByRef value2 As Integer) As Integer{0}" +
				"    End Property{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest2 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Public Overridable Property iTem() As Integer{0}" +
				"    End Property{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerGetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				true, false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Protected Overridable Default ReadOnly Property iTem(ByVal value1 As Object, ByRef value2 As Integer) As Integer{0}" +
				"        Get{0}" +
				"        End Get{0}" +
				"    End Property{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerSetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				false, true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Protected Overridable Default WriteOnly Property iTem(ByVal value1 As Object, ByRef value2 As Integer) As Integer{0}" +
				"        Set{0}" +
				"        End Set{0}" +
				"    End Property{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypes ()
		{
			string code = GeneratePropertyImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Public Overridable Property Name() As Integer Implements IPolicy.Name , IWhatever.Name{0}" +
				"    End Property{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest1 ()
		{
			string code = GeneratePropertyOverloads1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Protected Overloads Overridable Property Name() As Integer{0}" +
				"    End Property{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest2 ()
		{
			string code = GeneratePropertyOverloads2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Public Overloads Overridable Property Name() As Integer{0}" +
				"    End Property{0}" +
				"    {0}" +
				"    Private Overloads Property Name(ByVal value1 As Object) As Integer{0}" +
				"    End Property{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest3 ()
		{
			string code = GeneratePropertyOverloads3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Public Overridable Property Name() As Integer{0}" +
				"    End Property{0}" +
				"    {0}" +
				"    Property System_Int32_Name(ByVal value1 As Object) As Integer Implements Integer.Name{0}" +
				"    End Property{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void PropertyPrivateImplementationType ()
		{
			string code = GeneratePropertyPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Overridable Property System_Int32_Item(ByVal value1 As Object) As Integer Implements Integer.Item{0}" +
				"    End Property{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypeOrder ()
		{
			string code = GeneratePropertyImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Overridable Property System_Int32_Item(ByVal value1 As Object) As Integer Implements IPolicy.Item{0}" +
				"    End Property{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void PropertyNewSlotTest ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Private |
				MemberAttributes.New, true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Private Shadows Property Name() As Integer{0}" +
				"        Get{0}" +
				"        End Get{0}" +
				"        Set{0}" +
				"        End Set{0}" +
				"    End Property{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest1 ()
		{
			string code = GenerateMethodMembersType1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    <A(),  _{0}" +
				"     B()>  _{0}" +
				"    Private Sub (){0}" +
				"    End Sub{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest2 ()
		{
			string code = GenerateMethodMembersType2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Public Overridable Function Something(ByVal value1 As Object, ByVal value2 As Object, ByRef index As Integer, ByRef count As Integer) As Integer{0}" +
				"    End Function{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest3 ()
		{
			string code = GenerateMethodMembersType3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Public Overridable Function Something(<A(), B()> ByVal value As Object, <C(A1:=false, A2:=true), D()> ByRef __exception As Integer) As Integer{0}" +
				"    End Function{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypes ()
		{
			string code = GenerateMethodImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Friend Overridable Function Execute() As Integer Implements IPolicy.Execute , IWhatever.Execute{0}" +
				"    End Function{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest1 ()
		{
			string code = GenerateMethodOverloads1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Friend Overloads Overridable Function Execute() As Integer{0}" +
				"    End Function{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest2 ()
		{
			string code = GenerateMethodOverloads2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Public Overloads Overridable Sub Execute(){0}" +
				"    End Sub{0}" +
				"    {0}" +
				"    Private Overloads Function Execute(ByVal value1 As Object) As Integer{0}" +
				"    End Function{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest3 ()
		{
			string code = GenerateMethodOverloads3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Public Overridable Sub Execute(){0}" +
				"    End Sub{0}" +
				"    {0}" +
				"    Function System_Int32_Execute(ByVal value1 As Object) As Integer Implements Integer.Execute{0}" +
				"    End Function{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void MethodPrivateImplementationType ()
		{
			string code = GenerateMethodPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Overridable Function System_Int32_Execute(ByVal value1 As Object) As Integer Implements Integer.Execute{0}" +
				"    End Function{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypeOrder ()
		{
			string code = GenerateMethodImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Overridable Function System_Int32_Execute(ByVal value1 As Object) As Integer Implements IPolicy.Execute{0}" +
				"    End Function{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void MethodParamArrayAttribute ()
		{
			string code = GenerateMethodParamArrayAttribute (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Public Overridable Function Something(<A(), System.ParamArrayAttribute(), B()> ByRef value As Object, <C()> ByRef __exception As Integer) As Integer{0}" +
				"    End Function{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void MethodReturnTypeAttributes ()
		{
			string code = GenerateMethodReturnTypeAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    <A(),  _{0}" +
				"     B(),  _{0}" +
				"     System.ParamArrayAttribute()>  _{0}" +
				"    Public Overridable Function Execute() As <C(A1:=false, A2:=true), System.ParamArrayAttribute(), D()> Integer{0}" +
				"    End Function{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void MethodNewSlotTest ()
		{
			string code = GenerateMethodNewSlot (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Public Shadows Overridable Function Execute() As Integer{0}" +
				"    End Function{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorAttributesTest ()
		{
			// FIXME: updated to reflect mbas workaround
			string code = GenerateConstructorAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    <A(),  _{0}" +
				"     B()>  _{0}" +
				"    Private Sub New(){0}" +
				"    End Sub{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorParametersTest ()
		{
			// FIXME: updated to reflect mbas workaround
			string code = GenerateConstructorParameters (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Public Sub New(ByVal value1 As Object, ByVal value2 As Object, ByRef index As Integer, ByRef count As Integer){0}" +
				"    End Sub{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorParameterAttributesTest ()
		{
			// FIXME: updated to reflect mbas workaround
			string code = GenerateConstructorParameterAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Private Sub New(<A(), B()> ByVal value As Object, <C(A1:=false, A2:=true), D()> ByRef index As Integer){0}" +
				"    End Sub{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void BaseConstructorSingleArg ()
		{
			string code = GenerateBaseConstructor (false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Protected Sub New(ByVal value1 As Object, ByRef value2 As Integer){0}" +
				"        MyBase.New(value1){0}" +
				"    End Sub{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void BaseConstructorMultipleArgs ()
		{
			string code = GenerateBaseConstructor (true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Protected Sub New(ByVal value1 As Object, ByRef value2 As Integer){0}" +
				"        MyBase.New(value1, value2){0}" +
				"    End Sub{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void ChainedConstructorSingleArg ()
		{
			string code = GenerateChainedConstructor (false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Public Sub New(ByVal value1 As Object, ByRef value2 As Integer){0}" +
				"        Me.New(value1){0}" +
				"    End Sub{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void ChainedConstructorMultipleArgs ()
		{
			string code = GenerateChainedConstructor (true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    Public Sub New(ByVal value1 As Object, ByRef value2 As Integer){0}" +
				"        Me.New(value1, value2){0}" +
				"    End Sub{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void TypeConstructorTest ()
		{
			string code = GenerateTypeConstructor (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    <A(),  _{0}" +
				"     B()>  _{0}" +
				"    Shared Sub New(){0}" +
				"    End Sub{0}" +
				"End Structure{0}", NewLine), code);
		}

		[Test]
		public override void EntryPointMethodTest ()
		{
			string code = GenerateEntryPointMethod (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Structure Test1{0}" +
				"    {0}" +
				"    <A()>  _{0}" +
				"    Public Shared Sub Main(){0}" +
				"        Dim x As Test.InnerType{0}" +
				"    End Sub{0}" +
				"End Structure{0}", NewLine), code);
		}
		
		[Test]
		public override void PartialTypeTest ()
		{
			string code = GeneratePartialType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Partial Public Structure Test1{0}" +
				"End Structure{0}", NewLine), code);
		}
	}

	[TestFixture]
	public class CodeGeneratorFromTypeTest_Enum : CodeGeneratorFromTypeTestBase
	{
		private CodeTypeDeclaration _typeDeclaration;
		private ICodeGenerator _codeGenerator;

		protected override ICodeGenerator CodeGenerator
		{
			get { return _codeGenerator; }
		}

		protected override CodeTypeDeclaration TypeDeclaration
		{
			get { return _typeDeclaration; }
		}

		[SetUp]
		public override void SetUp ()
		{
			base.SetUp ();
			_typeDeclaration = new CodeTypeDeclaration ();
			_typeDeclaration.IsEnum = true;

			CodeDomProvider provider = new VBCodeProvider ();
			_codeGenerator = provider.CreateGenerator ();
		}

		[Test]
		public override void DefaultTypeTest ()
		{
			string code = GenerateDefaultType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum {0}" +
				"End Enum{0}", NewLine), code);
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
				"Public Enum Test1{0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void DerivedTypeTest ()
		{
			string code = GenerateDerivedType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Friend Enum Test1 As Integer{0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void AttributesAndTypeTest ()
		{
			string code = GenerateAttributesAndType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<A(),  _{0}" +
				" B()>  _{0}" +	
				"Public Enum Test1{0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest1 ()
		{
			string code = GenerateEventMembersType1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void EventMembersTypeTest2 ()
		{
			string code = GenerateEventMembersType2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void EventImplementationTypes ()
		{
			string code = GenerateEventImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void EventPrivateImplementationType ()
		{
			string code = GenerateEventPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void EventImplementationTypeOrder ()
		{
			string code = GenerateEventImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void FieldMembersAttributesTest ()
		{
			string code = GenerateFieldMembersAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"    <A(),  _{0}" +
				"     B()>  _{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void FieldMembersTypeTest ()
		{
			string code = GenerateFieldMembersType (MemberAttributes.Public, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"    Name = 2{0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void FieldNewSlotTest ()
		{
			string code = GenerateFieldMembersType (MemberAttributes.Assembly |
				MemberAttributes.New, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"    Name = 2{0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest1 ()
		{
			string code = GeneratePropertyMembersAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeTest2 ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Public,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeSetOnly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeGetSet ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Family,
				true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeFamilyOrAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.FamilyOrAssembly,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void PropertyMembersTypeAssembly ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Assembly,
				false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void PropertyParametersTest ()
		{
			string code = GeneratePropertyParameters (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest1 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerTest2 ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Public,
				false, false, false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerGetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				true, false, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void PropertyIndexerSetOnly ()
		{
			string code = GeneratePropertyIndexer (MemberAttributes.Family,
				false, true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypes ()
		{
			string code = GeneratePropertyImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest1 ()
		{
			string code = GeneratePropertyOverloads1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest2 ()
		{
			string code = GeneratePropertyOverloads2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void PropertyOverloadsTest3 ()
		{
			string code = GeneratePropertyOverloads3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void PropertyPrivateImplementationType ()
		{
			string code = GeneratePropertyPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void PropertyImplementationTypeOrder ()
		{
			string code = GeneratePropertyImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void PropertyNewSlotTest ()
		{
			string code = GeneratePropertyMembersType (MemberAttributes.Private |
				MemberAttributes.New, true, true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest1 ()
		{
			string code = GenerateMethodMembersType1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest2 ()
		{
			string code = GenerateMethodMembersType2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void MethodMembersTypeTest3 ()
		{
			string code = GenerateMethodMembersType3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypes ()
		{
			string code = GenerateMethodImplementationTypes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest1 ()
		{
			string code = GenerateMethodOverloads1 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest2 ()
		{
			string code = GenerateMethodOverloads2 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void MethodOverloadsTest3 ()
		{
			string code = GenerateMethodOverloads3 (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void MethodPrivateImplementationType ()
		{
			string code = GenerateMethodPrivateImplementationType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void MethodImplementationTypeOrder ()
		{
			string code = GenerateMethodImplementationTypeOrder (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void MethodParamArrayAttribute ()
		{
			string code = GenerateMethodParamArrayAttribute (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void MethodReturnTypeAttributes ()
		{
			string code = GenerateMethodReturnTypeAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void MethodNewSlotTest ()
		{
			string code = GenerateMethodNewSlot (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorAttributesTest ()
		{
			string code = GenerateConstructorAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorParametersTest ()
		{
			string code = GenerateConstructorParameters (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void ConstructorParameterAttributesTest ()
		{
			string code = GenerateConstructorParameterAttributes (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void BaseConstructorSingleArg ()
		{
			string code = GenerateBaseConstructor (false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void BaseConstructorMultipleArgs ()
		{
			string code = GenerateBaseConstructor (true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void ChainedConstructorSingleArg ()
		{
			string code = GenerateChainedConstructor (false, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void ChainedConstructorMultipleArgs ()
		{
			string code = GenerateChainedConstructor (true, Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void TypeConstructorTest ()
		{
			string code = GenerateTypeConstructor (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"End Enum{0}", NewLine), code);
		}

		[Test]
		public override void EntryPointMethodTest ()
		{
			string code = GenerateEntryPointMethod (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Public Enum Test1{0}" +
				"    {0}" +
				"    <A()>  _{0}" +
				"    Public Shared Sub Main(){0}" +
				"        Dim x As Test.InnerType{0}" +
				"    End Sub{0}" +
				"End Enum{0}", NewLine), code);
		}
		
		[Test]
		public override void PartialTypeTest ()
		{
			string code = GeneratePartialType (Options);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"Partial Public Enum Test1{0}" +
				"End Enum{0}", NewLine), code);
		}
	}
}
