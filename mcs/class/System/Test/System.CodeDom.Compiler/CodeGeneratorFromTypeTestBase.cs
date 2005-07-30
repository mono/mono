//
// Base class for CodeGenerator.GenerateCodeFromType unit tests.
//
// Authors:
// Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) Novell
//

using System.CodeDom;
using System.Reflection;
using System.Security;
using System.Security.Principal;

using NUnit.Framework;

namespace MonoTests.System.CodeDom.Compiler
{
	public abstract class CodeGeneratorFromTypeTestBase : CodeGeneratorTestBase
	{
		protected abstract CodeTypeDeclaration TypeDeclaration
		{
			get;
		}

		[Test]
		public abstract void DefaultTypeTest ();

		[Test]
		public abstract void NullTypeTest ();

		[Test]
		public abstract void SimpleTypeTest ();

		[Test]
		public abstract void DerivedTypeTest ();

		[Test]
		public abstract void AttributesAndTypeTest ();

		[Test]
		public abstract void EventMembersTypeTest1 ();

		[Test]
		public abstract void EventMembersTypeTest2 ();

		[Test]
		public abstract void EventImplementationTypes ();

		[Test]
		public abstract void EventPrivateImplementationType ();

		[Test]
		public abstract void EventImplementationTypeOrder ();

		[Test]
		public abstract void FieldMembersAttributesTest ();

		[Test]
		public abstract void FieldMembersTypeTest ();

		[Test]
		public abstract void FieldNewSlotTest ();

		[Test]
		public abstract void PropertyMembersTypeTest1 ();

		[Test]
		public abstract void PropertyMembersTypeTest2 ();

		[Test]
		public abstract void PropertyMembersTypeGetOnly ();

		[Test]
		public abstract void PropertyMembersTypeSetOnly ();

		[Test]
		public abstract void PropertyMembersTypeGetSet ();

		[Test]
		public abstract void PropertyMembersTypeFamilyOrAssembly ();

		[Test]
		public abstract void PropertyMembersTypeAssembly ();

		[Test]
		public abstract void PropertyParametersTest ();

		[Test]
		public abstract void PropertyIndexerTest1 ();

		[Test]
		public abstract void PropertyIndexerTest2 ();

		[Test]
		public abstract void PropertyIndexerGetOnly ();

		[Test]
		public abstract void PropertyIndexerSetOnly ();

		[Test]
		public abstract void PropertyImplementationTypes ();

		[Test]
		public abstract void PropertyOverloadsTest1 ();

		[Test]
		public abstract void PropertyOverloadsTest2 ();

		[Test]
		public abstract void PropertyOverloadsTest3 ();

		[Test]
		public abstract void PropertyPrivateImplementationType ();

		[Test]
		public abstract void PropertyImplementationTypeOrder ();

		[Test]
		public abstract void PropertyNewSlotTest ();

		[Test]
		public abstract void MethodMembersTypeTest1 ();

		[Test]
		public abstract void MethodMembersTypeTest2 ();

		[Test]
		public abstract void MethodMembersTypeTest3 ();

		[Test]
		public abstract void MethodImplementationTypes ();

		[Test]
		public abstract void MethodOverloadsTest1 ();

		[Test]
		public abstract void MethodOverloadsTest2 ();

		[Test]
		public abstract void MethodOverloadsTest3 ();

		[Test]
		public abstract void MethodPrivateImplementationType ();

		[Test]
		public abstract void MethodImplementationTypeOrder ();

		[Test]
		public abstract void MethodReturnTypeAttributes ();

		[Test]
		public abstract void MethodNewSlotTest ();

		[Test]
		public abstract void ConstructorAttributesTest ();

		[Test]
		public abstract void ConstructorParametersTest ();

		[Test]
		public abstract void ConstructorParameterAttributesTest ();

		[Test]
		public abstract void BaseConstructorSingleArg ();

		[Test]
		public abstract void BaseConstructorMultipleArgs ();

		[Test]
		public abstract void ChainedConstructorSingleArg ();

		[Test]
		public abstract void ChainedConstructorMultipleArgs ();

		[Test]
		public abstract void TypeConstructorTest ();

		protected string GenerateDefaultType ()
		{
			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateNullType ()
		{
			return GenerateCodeFromType (null);
		}

		protected string GenerateSimpleType ()
		{
			TypeDeclaration.Name = "Test1";
			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateDerivedType ()
		{
			TypeDeclaration.Name = "Test1";
			TypeDeclaration.TypeAttributes |= TypeAttributes.NestedFamily | 
				TypeAttributes.Abstract;
			TypeDeclaration.BaseTypes.Add (new CodeTypeReference (typeof (int)));
			TypeDeclaration.BaseTypes.Add (new CodeTypeReference (typeof (IIdentity)));
			TypeDeclaration.BaseTypes.Add (new CodeTypeReference (typeof (string)));
			TypeDeclaration.BaseTypes.Add (new CodeTypeReference (typeof (IPermission)));
			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateAttributesAndType ()
		{
			TypeDeclaration.Name = "Test1";

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			TypeDeclaration.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			TypeDeclaration.CustomAttributes.Add (attrDec);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateEventMembersType1 ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberEvent evt = new CodeMemberEvent ();

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			evt.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			evt.CustomAttributes.Add (attrDec);

			TypeDeclaration.Members.Add (evt);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateEventMembersType2 ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberEvent evt = new CodeMemberEvent ();
			evt.Name = "Click";
			evt.Attributes = MemberAttributes.Public | MemberAttributes.Override
				| MemberAttributes.Static | MemberAttributes.Abstract |
				MemberAttributes.New;
			evt.Type = new CodeTypeReference (typeof (int));
			TypeDeclaration.Members.Add (evt);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateEventImplementationTypes ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberEvent evt = new CodeMemberEvent ();
			evt.Name = "Click";
			evt.Attributes = MemberAttributes.FamilyAndAssembly;
			evt.Type = new CodeTypeReference (typeof (int));
			evt.ImplementationTypes.Add (new CodeTypeReference ("IPolicy"));
			evt.ImplementationTypes.Add (new CodeTypeReference ("IWhatever"));
			TypeDeclaration.Members.Add (evt);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateEventPrivateImplementationType ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberEvent evt = new CodeMemberEvent ();
			evt.Name = "Click";
			evt.Attributes = MemberAttributes.Family | MemberAttributes.Overloaded;
			evt.Type = new CodeTypeReference (typeof (int));
			evt.PrivateImplementationType = new CodeTypeReference (typeof (int));
			TypeDeclaration.Members.Add (evt);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateEventImplementationTypeOrder ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberEvent evt = new CodeMemberEvent ();
			evt.Name = "Click";
			evt.Attributes = MemberAttributes.Public | MemberAttributes.Overloaded;
			evt.Type = new CodeTypeReference (typeof (int));
			evt.PrivateImplementationType = new CodeTypeReference (typeof (int));
			evt.ImplementationTypes.Add (new CodeTypeReference ("IPolicy"));
			TypeDeclaration.Members.Add (evt);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateFieldMembersAttributes ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberField fld = new CodeMemberField ();

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			fld.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			fld.CustomAttributes.Add (attrDec);

			TypeDeclaration.Members.Add (fld);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateFieldMembersType (MemberAttributes memberAttributes)
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberField fld = new CodeMemberField ();
			fld.Name = "Name";
			fld.Attributes = memberAttributes;
			fld.Type = new CodeTypeReference (typeof (int));
			fld.InitExpression = new CodePrimitiveExpression (2);
			TypeDeclaration.Members.Add (fld);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GeneratePropertyMembersAttributes ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			TypeDeclaration.Members.Add (property);

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			property.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			property.CustomAttributes.Add (attrDec);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GeneratePropertyMembersType (MemberAttributes memberAttributes, bool hasGet, bool hasSet)
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = "Name";
			property.Attributes = memberAttributes;
			property.HasGet = hasGet;
			property.HasSet = hasSet;
			property.Type = new CodeTypeReference (typeof (int));
			TypeDeclaration.Members.Add (property);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GeneratePropertyParameters ()
		{
			TypeDeclaration.Name = "Test1";

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

			TypeDeclaration.Members.Add (property);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GeneratePropertyIndexer (MemberAttributes memberAttributes, bool hasGet, bool hasSet, bool addParameters)
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			// ensure case-insensitive comparison is done on name of property
			property.Name = "iTem";
			property.Attributes = memberAttributes;
			property.HasGet = hasGet;
			property.HasSet = hasSet;
			property.Type = new CodeTypeReference (typeof (int));
			TypeDeclaration.Members.Add (property);

			if (addParameters) {
				CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
					typeof (object), "value1");
				property.Parameters.Add (param);

				param = new CodeParameterDeclarationExpression (
					typeof (int), "value2");
				param.Direction = FieldDirection.Ref;
				property.Parameters.Add (param);
			}

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GeneratePropertyImplementationTypes ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = "Name";
			property.Attributes = MemberAttributes.Public;
			property.Type = new CodeTypeReference (typeof (int));
			property.ImplementationTypes.Add (new CodeTypeReference ("IPolicy"));
			property.ImplementationTypes.Add (new CodeTypeReference ("IWhatever"));
			TypeDeclaration.Members.Add (property);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GeneratePropertyOverloads1 ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = "Name";
			property.Attributes = MemberAttributes.Family | MemberAttributes.Overloaded;
			property.Type = new CodeTypeReference (typeof (int));
			TypeDeclaration.Members.Add (property);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GeneratePropertyOverloads2 ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = "Name";
			property.Attributes = MemberAttributes.Public;
			property.Type = new CodeTypeReference (typeof (int));
			TypeDeclaration.Members.Add (property);

			property = new CodeMemberProperty ();
			property.Name = "Name";
			property.Attributes = MemberAttributes.Private;
			property.Type = new CodeTypeReference (typeof (int));
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			property.Parameters.Add (param);
			TypeDeclaration.Members.Add (property);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GeneratePropertyOverloads3 ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = "Name";
			property.Attributes = MemberAttributes.Public;
			property.Type = new CodeTypeReference (typeof (int));
			TypeDeclaration.Members.Add (property);

			property = new CodeMemberProperty ();
			property.Name = "Name";
			property.Attributes = MemberAttributes.Private;
			property.Type = new CodeTypeReference (typeof (int));
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			property.Parameters.Add (param);
			property.PrivateImplementationType = new CodeTypeReference (typeof (int));
			TypeDeclaration.Members.Add (property);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GeneratePropertyPrivateImplementationType ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = "Item";
			property.Attributes = MemberAttributes.Public | MemberAttributes.Overloaded;
			property.Type = new CodeTypeReference (typeof (int));
			property.PrivateImplementationType = new CodeTypeReference (typeof (int));
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			property.Parameters.Add (param);
			TypeDeclaration.Members.Add (property);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GeneratePropertyImplementationTypeOrder ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = "Item";
			property.Attributes = MemberAttributes.Public | MemberAttributes.Overloaded;
			property.Type = new CodeTypeReference (typeof (int));
			property.PrivateImplementationType = new CodeTypeReference (typeof (int));
			property.ImplementationTypes.Add (new CodeTypeReference ("IPolicy"));
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			property.Parameters.Add (param);
			TypeDeclaration.Members.Add (property);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateMethodMembersType1 ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberMethod method = new CodeMemberMethod ();

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			method.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			method.CustomAttributes.Add (attrDec);

			TypeDeclaration.Members.Add (method);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateMethodMembersType2 ()
		{
			TypeDeclaration.Name = "Test1";

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

			TypeDeclaration.Members.Add (method);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateMethodMembersType3 ()
		{
			TypeDeclaration.Name = "Test1";

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

			TypeDeclaration.Members.Add (method);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateMethodImplementationTypes ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = "Execute";
			method.Attributes = MemberAttributes.Assembly;
			method.ReturnType = new CodeTypeReference (typeof (int));
			method.ImplementationTypes.Add (new CodeTypeReference ("IPolicy"));
			method.ImplementationTypes.Add (new CodeTypeReference ("IWhatever"));
			TypeDeclaration.Members.Add (method);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateMethodOverloads1 ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = "Execute";
			method.Attributes = MemberAttributes.Assembly | MemberAttributes.Overloaded;
			method.ReturnType = new CodeTypeReference (typeof (int));
			TypeDeclaration.Members.Add (method);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateMethodOverloads2 ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = "Execute";
			method.Attributes = MemberAttributes.Public;
			TypeDeclaration.Members.Add (method);

			method = new CodeMemberMethod ();
			method.Name = "Execute";
			method.Attributes = MemberAttributes.Private;
			method.ReturnType = new CodeTypeReference (typeof (int));
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			method.Parameters.Add (param);
			TypeDeclaration.Members.Add (method);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateMethodOverloads3 ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = "Execute";
			method.Attributes = MemberAttributes.Public;
			TypeDeclaration.Members.Add (method);

			method = new CodeMemberMethod ();
			method.Name = "Execute";
			method.Attributes = MemberAttributes.Private;
			method.ReturnType = new CodeTypeReference (typeof (int));
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			method.Parameters.Add (param);
			method.PrivateImplementationType = new CodeTypeReference (typeof (int));
			TypeDeclaration.Members.Add (method);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateMethodPrivateImplementationType ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = "Execute";
			method.Attributes = MemberAttributes.Public | MemberAttributes.Overloaded;
			method.ReturnType = new CodeTypeReference (typeof (int));
			method.PrivateImplementationType = new CodeTypeReference (typeof (int));
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			method.Parameters.Add (param);
			TypeDeclaration.Members.Add (method);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateMethodImplementationTypeOrder ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = "Execute";
			method.Attributes = MemberAttributes.Public;
			method.ReturnType = new CodeTypeReference (typeof (int));
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			method.Parameters.Add (param);
			method.PrivateImplementationType = new CodeTypeReference (typeof (int));
			method.ImplementationTypes.Add (new CodeTypeReference ("IPolicy"));
			TypeDeclaration.Members.Add (method);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateMethodReturnTypeAttributes ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = "Execute";
			method.Attributes = MemberAttributes.Public;
			method.ReturnType = new CodeTypeReference (typeof (int));
			TypeDeclaration.Members.Add (method);

			// method custom attributes
			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			method.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			method.CustomAttributes.Add (attrDec);

			// return TypeDeclaration custom attributes
			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "C";
			attrDec.Arguments.Add (new CodeAttributeArgument ("A1",
				new CodePrimitiveExpression (false)));
			attrDec.Arguments.Add (new CodeAttributeArgument ("A2",
				new CodePrimitiveExpression (true)));
			method.ReturnTypeCustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "D";
			method.ReturnTypeCustomAttributes.Add (attrDec);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateConstructorAttributes ()
		{
			TypeDeclaration.Name = "Test1";

			CodeConstructor ctor = new CodeConstructor ();

			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			ctor.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			ctor.CustomAttributes.Add (attrDec);

			TypeDeclaration.Members.Add (ctor);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateMethodNewSlot ()
		{
			TypeDeclaration.Name = "Test1";

			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = "Execute";
			method.Attributes = MemberAttributes.Public | MemberAttributes.New;
			method.ReturnType = new CodeTypeReference (typeof (int));
			TypeDeclaration.Members.Add (method);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateConstructorParameters ()
		{
			TypeDeclaration.Name = "Test1";

			CodeConstructor ctor = new CodeConstructor ();
			ctor.Name = "Whatever";
			ctor.Attributes = MemberAttributes.Public;

			// scope and vtable modifiers should be ignored
			ctor.Attributes |= MemberAttributes.Abstract | MemberAttributes.Const
				| MemberAttributes.Final | MemberAttributes.New
				| MemberAttributes.Overloaded | MemberAttributes.Override
				| MemberAttributes.Static;

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

			TypeDeclaration.Members.Add (ctor);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateConstructorParameterAttributes ()
		{
			TypeDeclaration.Name = "Test1";

			CodeConstructor ctor = new CodeConstructor ();
			ctor.Name = "Something";
			ctor.Attributes = MemberAttributes.Private;

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

			TypeDeclaration.Members.Add (ctor);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateBaseConstructor (bool multipleArgs)
		{
			TypeDeclaration.Name = "Test1";

			CodeConstructor ctor = new CodeConstructor ();
			ctor.Name = "Something";
			ctor.Attributes = MemberAttributes.Family;

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

			if (multipleArgs) {
				ctor.BaseConstructorArgs.Add (new CodeVariableReferenceExpression ("value2"));
			}

			TypeDeclaration.Members.Add (ctor);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateChainedConstructor (bool multipleArgs)
		{
			TypeDeclaration.Name = "Test1";

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

			// implementation types should be ignored on ctors
			ctor.ImplementationTypes.Add (new CodeTypeReference ("IPolicy"));

			// chained ctor args
			ctor.ChainedConstructorArgs.Add (new CodeVariableReferenceExpression ("value1"));

			if (multipleArgs) {
				ctor.ChainedConstructorArgs.Add (new CodeVariableReferenceExpression ("value2"));
			}

			// should be ignored as chained ctor args should take precedence over base 
			// ctor args
			ctor.BaseConstructorArgs.Add (new CodeVariableReferenceExpression ("value3"));

			TypeDeclaration.Members.Add (ctor);

			return GenerateCodeFromType (TypeDeclaration);
		}

		protected string GenerateTypeConstructor ()
		{
			TypeDeclaration.Name = "Test1";

			CodeTypeConstructor typeCtor = new CodeTypeConstructor ();
			// access, scope and vtable modifiers should be ignored
			typeCtor.Attributes |= MemberAttributes.Public | MemberAttributes.Abstract
				| MemberAttributes.Const | MemberAttributes.Final
				| MemberAttributes.New | MemberAttributes.Overloaded
				| MemberAttributes.Override | MemberAttributes.Static;
			TypeDeclaration.Members.Add (typeCtor);

			// custom attributes
			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			typeCtor.CustomAttributes.Add (attrDec);

			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "B";
			typeCtor.CustomAttributes.Add (attrDec);

			// parameter should be ignored
			CodeParameterDeclarationExpression param = new CodeParameterDeclarationExpression (
				typeof (object), "value1");
			typeCtor.Parameters.Add (param);

			// implementation types should be ignored on type ctors
			typeCtor.ImplementationTypes.Add (new CodeTypeReference ("IPolicy"));

			// private immplementation type should be ignored on type ctors
			typeCtor.PrivateImplementationType = new CodeTypeReference (typeof (int));

			// return type should be ignored on type ctors
			typeCtor.ReturnType = new CodeTypeReference (typeof (int));

			// return TypeDeclaration custom attributes
			attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			attrDec.Arguments.Add (new CodeAttributeArgument ("A1",
				new CodePrimitiveExpression (false)));
			attrDec.Arguments.Add (new CodeAttributeArgument ("A2",
				new CodePrimitiveExpression (true)));
			typeCtor.ReturnTypeCustomAttributes.Add (attrDec);

			return GenerateCodeFromType (TypeDeclaration);
		}
	}
}
