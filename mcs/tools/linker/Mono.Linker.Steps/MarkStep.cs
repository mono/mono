//
// MarkStep.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// (C) 2006 Jb Evain
// (C) 2007 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mono.Linker.Steps {

	public class MarkStep : IStep {

		protected LinkContext _context;
		protected Queue _methods;
		protected ArrayList _virtual_methods;

		public AnnotationStore Annotations {
			get { return _context.Annotations; }
		}

		public MarkStep ()
		{
			_methods = new Queue ();
			_virtual_methods = new ArrayList ();
		}

		public virtual void Process (LinkContext context)
		{
			_context = context;

			Initialize ();
			Process ();
		}

		void Initialize ()
		{
			foreach (AssemblyDefinition assembly in _context.GetAssemblies ())
				InitializeAssembly (assembly);
		}

		protected virtual void InitializeAssembly (AssemblyDefinition assembly)
		{
			MarkAssembly (assembly);

			foreach (TypeDefinition type in assembly.MainModule.Types)
				InitializeType (type);
		}

		void InitializeType (TypeDefinition type)
		{
			if (type.HasNestedTypes) {
				foreach (var nested in type.NestedTypes)
					InitializeType (nested);
			}

			if (!Annotations.IsMarked (type))
				return;

			MarkType (type);

			if (type.HasFields)
				InitializeFields (type);
			if (type.HasMethods)
				InitializeMethods (type.Methods);
		}

		void InitializeFields (TypeDefinition type)
		{
			foreach (FieldDefinition field in type.Fields)
				if (Annotations.IsMarked (field))
					MarkField (field);
		}

		void InitializeMethods (ICollection methods)
		{
			foreach (MethodDefinition method in methods)
				if (Annotations.IsMarked (method))
					EnqueueMethod (method);
		}

		void Process ()
		{
			if (QueueIsEmpty ())
				throw new InvalidOperationException ("No entry methods");

			while (!QueueIsEmpty ()) {
				ProcessQueue ();
				ProcessVirtualMethods ();
			}
		}

		void ProcessQueue ()
		{
			while (!QueueIsEmpty ()) {
				MethodDefinition method = (MethodDefinition) _methods.Dequeue ();
				Annotations.Push (method);
				ProcessMethod (method);
				Annotations.Pop ();
			}
		}

		bool QueueIsEmpty ()
		{
			return _methods.Count == 0;
		}

		protected virtual void EnqueueMethod (MethodDefinition method)
		{
			_methods.Enqueue (method);
		}

		void ProcessVirtualMethods ()
		{
			foreach (MethodDefinition method in _virtual_methods) {
				Annotations.Push (method);
				ProcessVirtualMethod (method);
				Annotations.Pop ();
			}
		}

		void ProcessVirtualMethod (MethodDefinition method)
		{
			IList overrides = Annotations.GetOverrides (method);
			if (overrides == null)
				return;

			foreach (MethodDefinition @override in overrides)
				ProcessOverride (@override);
		}

		void ProcessOverride (MethodDefinition method)
		{
			if (!Annotations.IsMarked (method.DeclaringType))
				return;

			if (Annotations.IsProcessed (method))
				return;

			if (Annotations.IsMarked (method))
				return;

			MarkMethod (method);
			ProcessVirtualMethod (method);
		}

		void MarkMarshalSpec (IMarshalInfoProvider spec)
		{
			if (!spec.HasMarshalInfo)
				return;

			var marshaler = spec.MarshalInfo as CustomMarshalInfo;
			if (marshaler == null)
				return;

			MarkType (marshaler.ManagedType);
		}

		void MarkCustomAttributes (ICustomAttributeProvider provider)
		{
			if (!provider.HasCustomAttributes)
				return;

			foreach (CustomAttribute ca in provider.CustomAttributes)
				MarkCustomAttribute (ca);
		}

		protected virtual void MarkCustomAttribute (CustomAttribute ca)
		{
			Annotations.Push (ca);
			MarkMethod (ca.Constructor);

			MarkCustomAttributeArguments (ca);

			TypeReference constructor_type = ca.Constructor.DeclaringType;
			TypeDefinition type = constructor_type.Resolve ();
			if (type == null) {
				Annotations.Pop ();
				throw new ResolutionException (constructor_type);
			}

			MarkCustomAttributeProperties (ca, type);
			MarkCustomAttributeFields (ca, type);
			Annotations.Pop ();
		}

		protected void MarkSecurityDeclarations (ISecurityDeclarationProvider provider)
		{
			// most security declarations are removed (if linked) but user code might still have some
			// and if the attribtues references types then they need to be marked too
			if ((provider == null) || !provider.HasSecurityDeclarations)
				return;

			foreach (var sd in provider.SecurityDeclarations)
				MarkSecurityDeclaration (sd);
		}

		protected virtual void MarkSecurityDeclaration (SecurityDeclaration sd)
		{
			if (!sd.HasSecurityAttributes)
				return;
			
			foreach (var sa in sd.SecurityAttributes)
				MarkSecurityAttribute (sa);
		}

		protected virtual void MarkSecurityAttribute (SecurityAttribute sa)
		{
			TypeReference security_type = sa.AttributeType;
			TypeDefinition type = security_type.Resolve ();
			if (type == null)
				throw new ResolutionException (security_type);
			
			MarkType (security_type);
			MarkSecurityAttributeProperties (sa, type);
			MarkSecurityAttributeFields (sa, type);
		}

		protected void MarkSecurityAttributeProperties (SecurityAttribute sa, TypeDefinition attribute)
		{
			if (!sa.HasProperties)
				return;

			foreach (var named_argument in sa.Properties)
				MarkCustomAttributeProperty (named_argument, attribute);
		}

		protected void MarkSecurityAttributeFields (SecurityAttribute sa, TypeDefinition attribute)
		{
			if (!sa.HasFields)
				return;

			foreach (var named_argument in sa.Fields)
				MarkCustomAttributeField (named_argument, attribute);
		}

		protected void MarkCustomAttributeProperties (CustomAttribute ca, TypeDefinition attribute)
		{
			if (!ca.HasProperties)
				return;

			foreach (var named_argument in ca.Properties)
				MarkCustomAttributeProperty (named_argument, attribute);
		}

		protected void MarkCustomAttributeProperty (CustomAttributeNamedArgument namedArgument, TypeDefinition attribute)
		{
			PropertyDefinition property = GetProperty (attribute, namedArgument.Name);
			Annotations.Push (property);
			if (property != null)
				MarkMethod (property.SetMethod);

			MarkIfType (namedArgument.Argument);
			Annotations.Pop ();
		}

		PropertyDefinition GetProperty (TypeDefinition type, string propertyname)
		{
			while (type != null) {
				PropertyDefinition property = type.Properties.FirstOrDefault (p => p.Name == propertyname);
				if (property != null)
					return property;

				type = type.BaseType != null ? ResolveTypeDefinition (type.BaseType) : null;
			}

			return null;
		}

		protected void MarkCustomAttributeFields (CustomAttribute ca, TypeDefinition attribute)
		{
			if (!ca.HasFields)
				return;

			foreach (var named_argument in ca.Fields)
				MarkCustomAttributeField (named_argument, attribute);
		}

		protected void MarkCustomAttributeField (CustomAttributeNamedArgument namedArgument, TypeDefinition attribute)
		{
			FieldDefinition field = GetField (attribute, namedArgument.Name);
			if (field != null)
				MarkField (field);

			MarkIfType (namedArgument.Argument);
		}

		FieldDefinition GetField (TypeDefinition type, string fieldname)
		{
			while (type != null) {
				FieldDefinition field = type.Fields.FirstOrDefault (f => f.Name == fieldname);
				if (field != null)
					return field;

				type = type.BaseType != null ? ResolveTypeDefinition (type.BaseType) : null;
			}

			return null;
		}

		void MarkCustomAttributeArguments (CustomAttribute ca)
		{
			if (!ca.HasConstructorArguments)
				return;

			foreach (var argument in ca.ConstructorArguments)
				MarkIfType (argument);
		}

		void MarkIfType (CustomAttributeArgument argument)
		{
			var at = argument.Type;
			if (at.IsArray) {
				var et = at.GetElementType ();
				if (et.Namespace != "System" || et.Name != "Type")
					return;

				MarkType (et);
				if (argument.Value == null)
					return;

				foreach (var cac in (CustomAttributeArgument[]) argument.Value)
					MarkWithResolvedScope ((TypeReference) cac.Value);
			} else if (at.Namespace == "System" && at.Name == "Type") {
				MarkType (argument.Type);
				MarkWithResolvedScope ((TypeReference) argument.Value);
			}
		}

		// custom attributes encoding means it's possible to have a scope that will point into a PCL facade
		// even if we (just before saving) will resolve all type references (bug #26752)
		void MarkWithResolvedScope (TypeReference type)
		{
			if (type == null)
				return;

			// a GenericInstanceType can could contains generic arguments with scope that
			// needs to be updated out of the PCL facade (bug #28823)
			var git = (type as GenericInstanceType);
			if ((git != null) && git.HasGenericArguments) {
				foreach (var ga in git.GenericArguments)
					MarkWithResolvedScope (ga);
			}
			// we cannot set the Scope of a TypeSpecification but it's element type can be set
			// e.g. System.String[] -> System.String
			var ts = (type as TypeSpecification);
			if (ts != null) {
				MarkWithResolvedScope (ts.ElementType);
				return;
			}

			var td = type.Resolve ();
			if (td != null)
				type.Scope = td.Scope;
			MarkType (type);
		}

		protected bool CheckProcessed (IMetadataTokenProvider provider)
		{
			if (Annotations.IsProcessed (provider))
				return true;

			Annotations.Processed (provider);
			return false;
		}

		protected void MarkAssembly (AssemblyDefinition assembly)
		{
			if (CheckProcessed (assembly))
				return;

			ProcessModule (assembly);

			MarkCustomAttributes (assembly);
			MarkSecurityDeclarations (assembly);

			foreach (ModuleDefinition module in assembly.Modules)
				MarkCustomAttributes (module);
		}

		void ProcessModule (AssemblyDefinition assembly)
		{
			// Pre-mark <Module> if there is any methods as they need to be executed 
			// at assembly load time
			foreach (TypeDefinition type in assembly.MainModule.Types)
			{
				if (type.Name == "<Module>" && type.HasMethods)
				{
					MarkType (type);
					break;
				}
			}
		}

		protected void MarkField (FieldReference reference)
		{
//			if (IgnoreScope (reference.DeclaringType.Scope))
//				return;

			if (reference.DeclaringType is GenericInstanceType)
				MarkType (reference.DeclaringType);

			FieldDefinition field = ResolveFieldDefinition (reference);

			if (field == null)
				throw new ResolutionException (reference);

			if (CheckProcessed (field))
				return;

			MarkType (field.DeclaringType);
			MarkType (field.FieldType);
			MarkCustomAttributes (field);
			MarkMarshalSpec (field);

			Annotations.Mark (field);
		}

		protected virtual bool IgnoreScope (IMetadataScope scope)
		{
			AssemblyDefinition assembly = ResolveAssembly (scope);
			return Annotations.GetAction (assembly) != AssemblyAction.Link;
		}

		FieldDefinition ResolveFieldDefinition (FieldReference field)
		{
			FieldDefinition fd = field as FieldDefinition;
			if (fd == null)
				fd = field.Resolve ();

			return fd;
		}

		void MarkScope (IMetadataScope scope)
		{
			var provider = scope as IMetadataTokenProvider;
			if (provider == null)
				return;

			Annotations.Mark (provider);
		}

		protected virtual void MarkSerializable (TypeDefinition type)
		{
			MarkDefaultConstructor (type);
			MarkMethodsIf (type.Methods, IsSpecialSerializationConstructorPredicate);
		}

		protected virtual TypeDefinition MarkType (TypeReference reference)
		{
			if (reference == null)
				return null;

			reference = GetOriginalType (reference);

			if (reference is GenericParameter)
				return null;

//			if (IgnoreScope (reference.Scope))
//				return;

			TypeDefinition type = ResolveTypeDefinition (reference);

			if (type == null)
				throw new ResolutionException (reference);

			if (CheckProcessed (type))
				return null;

			Annotations.Push (type);

			MarkScope (type.Scope);
			MarkType (type.BaseType);
			MarkType (type.DeclaringType);
			MarkCustomAttributes (type);
			MarkSecurityDeclarations (type);

			if (IsMulticastDelegate (type)) {
				MarkMethodCollection (type.Methods);
			}

			if (IsSerializable (type))
				MarkSerializable (type);

			MarkTypeSpecialCustomAttributes (type);

			MarkGenericParameterProvider (type);

			// keep fields for value-types and for classes with LayoutKind.Sequential or Explicit
			if (type.IsValueType || !type.IsAutoLayout)
				MarkFields (type, type.IsEnum);

			if (type.HasInterfaces) {
				foreach (var iface in type.Interfaces)
					MarkType (iface.InterfaceType);
			}

			if (type.HasMethods) {
				MarkMethodsIf (type.Methods, IsVirtualAndHasPreservedParent);
				MarkMethodsIf (type.Methods, IsStaticConstructorPredicate);
				MarkMethodsIf (type.Methods, HasSerializationAttribute);
			}

			DoAdditionalTypeProcessing (type);

			Annotations.Pop ();

			Annotations.Mark (type);

			ApplyPreserveInfo (type);

			return type;
		}

		// Allow subclassers to mark additional things when marking a method
		protected virtual void DoAdditionalTypeProcessing (TypeDefinition method)
		{
		}

		void MarkTypeSpecialCustomAttributes (TypeDefinition type)
		{
			if (!type.HasCustomAttributes)
				return;

			foreach (CustomAttribute attribute in type.CustomAttributes) {
				switch (attribute.Constructor.DeclaringType.FullName) {
				case "System.Xml.Serialization.XmlSchemaProviderAttribute":
					MarkXmlSchemaProvider (type, attribute);
					break;
				}
			}
		}

		void MarkMethodSpecialCustomAttributes (MethodDefinition method)
		{
			if (!method.HasCustomAttributes)
				return;

			foreach (CustomAttribute attribute in method.CustomAttributes) {
				switch (attribute.Constructor.DeclaringType.FullName) {
				case "System.Web.Services.Protocols.SoapHeaderAttribute":
					MarkSoapHeader (method, attribute);
					break;
				}
			}
		}

		void MarkXmlSchemaProvider (TypeDefinition type, CustomAttribute attribute)
		{
			string method_name;
			if (!TryGetStringArgument (attribute, out method_name))
				return;

			MarkNamedMethod (type, method_name);
		}

		static bool TryGetStringArgument (CustomAttribute attribute, out string argument)
		{
			argument = null;

			if (attribute.ConstructorArguments.Count < 1)
				return false;

			argument = attribute.ConstructorArguments [0].Value as string;

			return argument != null;
		}

		protected int MarkNamedMethod (TypeDefinition type, string method_name)
		{
			if (!type.HasMethods)
				return 0;

			int count = 0;
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name != method_name)
					continue;

				MarkMethod (method);
				count++;
			}

			return count;
		}

		void MarkSoapHeader (MethodDefinition method, CustomAttribute attribute)
		{
			string member_name;
			if (!TryGetStringArgument (attribute, out member_name))
				return;

			MarkNamedField (method.DeclaringType, member_name);
			MarkNamedProperty (method.DeclaringType, member_name);
		}

		void MarkNamedField (TypeDefinition type, string field_name)
		{
			if (!type.HasFields)
				return;

			foreach (FieldDefinition field in type.Fields) {
				if (field.Name != field_name)
					continue;

				MarkField (field);
			}
		}

		void MarkNamedProperty (TypeDefinition type, string property_name)
		{
			if (!type.HasProperties)
				return;

			foreach (PropertyDefinition property in type.Properties) {
				if (property.Name != property_name)
					continue;

				Annotations.Push (property);
				MarkMethod (property.GetMethod);
				MarkMethod (property.SetMethod);
				Annotations.Pop ();
			}
		}

		void MarkGenericParameterProvider (IGenericParameterProvider provider)
		{
			if (!provider.HasGenericParameters)
				return;

			foreach (GenericParameter parameter in provider.GenericParameters)
				MarkGenericParameter (parameter);
		}

		void MarkGenericParameter (GenericParameter parameter)
		{
			MarkCustomAttributes (parameter);
			foreach (TypeReference constraint in parameter.Constraints)
				MarkType (constraint);
		}

		bool IsVirtualAndHasPreservedParent (MethodDefinition method)
		{
			if (!method.IsVirtual)
				return false;

			var base_list = Annotations.GetBaseMethods (method);
			if (base_list == null)
				return false;

			foreach (MethodDefinition @base in base_list) {
				if (IgnoreScope (@base.DeclaringType.Scope))
					return true;

				if (IsVirtualAndHasPreservedParent (@base))
					return true;
			}

			return false;
		}

		static MethodPredicate IsSpecialSerializationConstructorPredicate = new MethodPredicate (IsSpecialSerializationConstructor);

		static bool IsSpecialSerializationConstructor (MethodDefinition method)
		{
			if (!IsConstructor (method))
				return false;

			var parameters = method.Parameters;
			if (parameters.Count != 2)
				return false;

			return parameters [0].ParameterType.Name == "SerializationInfo" &&
				parameters [1].ParameterType.Name == "StreamingContext";
		}

		delegate bool MethodPredicate (MethodDefinition method);

		void MarkMethodsIf (ICollection methods, MethodPredicate predicate)
		{
			foreach (MethodDefinition method in methods)
				if (predicate (method)) {
					Annotations.Push (predicate);
					MarkMethod (method);
					Annotations.Pop ();
				}
		}

		static MethodPredicate IsDefaultConstructorPredicate = new MethodPredicate (IsDefaultConstructor);

		static bool IsDefaultConstructor (MethodDefinition method)
		{
			return IsConstructor (method) && !method.HasParameters;
		}

		static bool IsConstructor (MethodDefinition method)
		{
			return method.IsConstructor && !method.IsStatic;
		}

		protected void MarkDefaultConstructor (TypeDefinition type)
		{
			if ((type == null) || !type.HasMethods)
				return;

			MarkMethodsIf (type.Methods, IsDefaultConstructorPredicate);
		}

		static MethodPredicate IsStaticConstructorPredicate = new MethodPredicate (IsStaticConstructor);

		static bool IsStaticConstructor (MethodDefinition method)
		{
			return method.IsConstructor && method.IsStatic;
		}

		static bool HasSerializationAttribute (MethodDefinition method)
		{
			if (!method.HasCustomAttributes)
				return false;
			foreach (var ca in method.CustomAttributes) {
				var cat = ca.AttributeType;
				if (cat.Namespace != "System.Runtime.Serialization")
					continue;
				switch (cat.Name) {
				case "OnDeserializedAttribute":
				case "OnDeserializingAttribute":
				case "OnSerializedAttribute":
				case "OnSerializingAttribute":
					return true;
				}
			}
			return false;
		}

		static bool IsSerializable (TypeDefinition td)
		{
			return (td.Attributes & TypeAttributes.Serializable) != 0;
		}

		static bool IsMulticastDelegate (TypeDefinition td)
		{
			return td.BaseType != null && td.BaseType.FullName == "System.MulticastDelegate";
		}

		protected TypeDefinition ResolveTypeDefinition (TypeReference type)
		{
			TypeDefinition td = type as TypeDefinition;
			if (td == null)
				td = type.Resolve ();

			return td;
		}

		protected TypeReference GetOriginalType (TypeReference type)
		{
			while (type is TypeSpecification) {
				GenericInstanceType git = type as GenericInstanceType;
				if (git != null)
					MarkGenericArguments (git);

				var mod = type as IModifierType;
				if (mod != null)
					MarkModifierType (mod);

				type = ((TypeSpecification) type).ElementType;
			}

			return type;
		}

		void MarkModifierType (IModifierType mod)
		{
			MarkType (mod.ModifierType);
		}

		void MarkGenericArguments (IGenericInstance instance)
		{
			foreach (TypeReference argument in instance.GenericArguments)
				MarkType (argument);

			MarkGenericArgumentConstructors (instance);
		}

		void MarkGenericArgumentConstructors (IGenericInstance instance)
		{
			var arguments = instance.GenericArguments;

			var generic_element = GetGenericProviderFromInstance (instance);
			if (generic_element == null)
				return;

			var parameters = generic_element.GenericParameters;

			if (arguments.Count != parameters.Count)
				return;

			for (int i = 0; i < arguments.Count; i++) {
				var argument = arguments [i];
				var parameter = parameters [i];

				if (!parameter.HasDefaultConstructorConstraint)
					continue;

				var argument_definition = ResolveTypeDefinition (argument);
				if (argument_definition == null)
					continue;

				MarkMethodsIf (argument_definition.Methods, ctor => !ctor.IsStatic && !ctor.HasParameters);
			}
		}

		IGenericParameterProvider GetGenericProviderFromInstance (IGenericInstance instance)
		{
			var method = instance as GenericInstanceMethod;
			if (method != null)
				return ResolveMethodDefinition (method.ElementMethod);

			var type = instance as GenericInstanceType;
			if (type != null)
				return ResolveTypeDefinition (type.ElementType);

			return null;
		}

		void ApplyPreserveInfo (TypeDefinition type)
		{
			ApplyPreserveMethods (type);

			if (!Annotations.IsPreserved (type))
				return;

			switch (Annotations.GetPreserve (type)) {
			case TypePreserve.All:
				MarkFields (type, true);
				MarkMethods (type);
				break;
			case TypePreserve.Fields:
				MarkFields (type, true);
				break;
			case TypePreserve.Methods:
				MarkMethods (type);
				break;
			}
		}

		void ApplyPreserveMethods (TypeDefinition type)
		{
			var list = Annotations.GetPreservedMethods (type);
			if (list == null)
				return;

			MarkMethodCollection (list);
		}

		void ApplyPreserveMethods (MethodDefinition method)
		{
			var list = Annotations.GetPreservedMethods (method);
			if (list == null)
				return;

			MarkMethodCollection (list);
		}

		protected void MarkFields (TypeDefinition type, bool includeStatic)
		{
			if (!type.HasFields)
				return;

			foreach (FieldDefinition field in type.Fields) {
				if (!includeStatic && field.IsStatic)
					continue;
				MarkField (field);
			}
		}

		protected virtual void MarkMethods (TypeDefinition type)
		{
			if (type.HasMethods)
				MarkMethodCollection (type.Methods);
		}

		void MarkMethodCollection (IEnumerable methods)
		{
			foreach (MethodDefinition method in methods)
				MarkMethod (method);
		}

		protected virtual MethodDefinition MarkMethod (MethodReference reference)
		{
			reference = GetOriginalMethod (reference);

			if (reference.DeclaringType is ArrayType)
				return null;

			Annotations.Push (reference);
			if (reference.DeclaringType is GenericInstanceType)
				MarkType (reference.DeclaringType);

//			if (IgnoreScope (reference.DeclaringType.Scope))
//				return;

			MethodDefinition method = ResolveMethodDefinition (reference);

			if (method == null) {
				Annotations.Pop ();
				throw new ResolutionException (reference);
			}

			if (Annotations.GetAction (method) == MethodAction.Nothing)
				Annotations.SetAction (method, MethodAction.Parse);

			EnqueueMethod (method);

			Annotations.Pop ();
			Annotations.AddDependency (method);

			return method;
		}

		AssemblyDefinition ResolveAssembly (IMetadataScope scope)
		{
			AssemblyDefinition assembly = _context.Resolve (scope);
			MarkAssembly (assembly);
			return assembly;
		}

		protected MethodReference GetOriginalMethod (MethodReference method)
		{
			while (method is MethodSpecification) {
				GenericInstanceMethod gim = method as GenericInstanceMethod;
				if (gim != null)
					MarkGenericArguments (gim);

				method = ((MethodSpecification) method).ElementMethod;
			}

			return method;
		}

		MethodDefinition ResolveMethodDefinition (MethodReference method)
		{
			MethodDefinition md = method as MethodDefinition;
			if (md == null)
				md = method.Resolve ();

			return md;
		}

		protected virtual void ProcessMethod (MethodDefinition method)
		{
			if (CheckProcessed (method))
				return;

			Annotations.Push (method);
			MarkType (method.DeclaringType);
			MarkCustomAttributes (method);
			MarkSecurityDeclarations (method);

			MarkGenericParameterProvider (method);

			if (IsPropertyMethod (method))
				MarkProperty (GetProperty (method));
			else if (IsEventMethod (method))
				MarkEvent (GetEvent (method));

			if (method.HasParameters) {
				foreach (ParameterDefinition pd in method.Parameters) {
					MarkType (pd.ParameterType);
					MarkCustomAttributes (pd);
					MarkMarshalSpec (pd);
				}
			}

			if (method.HasOverrides) {
				foreach (MethodReference ov in method.Overrides)
					MarkMethod (ov);
			}

			MarkMethodSpecialCustomAttributes (method);

			if (method.IsVirtual)
				_virtual_methods.Add (method);

			MarkBaseMethods (method);

			MarkType (method.ReturnType);
			MarkCustomAttributes (method.MethodReturnType);
			MarkMarshalSpec (method.MethodReturnType);

			if (ShouldParseMethodBody (method))
				MarkMethodBody (method.Body);

			DoAdditionalMethodProcessing (method);

			Annotations.Mark (method);

			ApplyPreserveMethods (method);
			Annotations.Pop ();
		}

		// Allow subclassers to mark additional things when marking a method
		protected virtual void DoAdditionalMethodProcessing (MethodDefinition method)
		{
		}

		void MarkBaseMethods (MethodDefinition method)
		{
			IList base_methods = Annotations.GetBaseMethods (method);
			if (base_methods == null)
				return;

			foreach (MethodDefinition base_method in base_methods) {
				if (base_method.DeclaringType.IsInterface && !method.DeclaringType.IsInterface)
					continue;

				MarkMethod (base_method);
				MarkBaseMethods (base_method);
			}
		}

		bool ShouldParseMethodBody (MethodDefinition method)
		{
			if (!method.HasBody)
				return false;

			AssemblyDefinition assembly = ResolveAssembly (method.DeclaringType.Scope);
			return (Annotations.GetAction (method) == MethodAction.ForceParse ||
				(Annotations.GetAction (assembly) == AssemblyAction.Link && Annotations.GetAction (method) == MethodAction.Parse));
		}

		static internal bool IsPropertyMethod (MethodDefinition md)
		{
			return (md.SemanticsAttributes & MethodSemanticsAttributes.Getter) != 0 ||
				(md.SemanticsAttributes & MethodSemanticsAttributes.Setter) != 0;
		}

		static bool IsEventMethod (MethodDefinition md)
		{
			return (md.SemanticsAttributes & MethodSemanticsAttributes.AddOn) != 0 ||
				(md.SemanticsAttributes & MethodSemanticsAttributes.Fire) != 0 ||
				(md.SemanticsAttributes & MethodSemanticsAttributes.RemoveOn) != 0;
		}

		static internal PropertyDefinition GetProperty (MethodDefinition md)
		{
			TypeDefinition declaringType = (TypeDefinition) md.DeclaringType;
			foreach (PropertyDefinition prop in declaringType.Properties)
				if (prop.GetMethod == md || prop.SetMethod == md)
					return prop;

			return null;
		}

		static EventDefinition GetEvent (MethodDefinition md)
		{
			TypeDefinition declaringType = (TypeDefinition) md.DeclaringType;
			foreach (EventDefinition evt in declaringType.Events)
				if (evt.AddMethod == md || evt.InvokeMethod == md || evt.RemoveMethod == md)
					return evt;

			return null;
		}

		protected void MarkProperty (PropertyDefinition prop)
		{
			MarkCustomAttributes (prop);
		}

		protected void MarkEvent (EventDefinition evt)
		{
			MarkCustomAttributes (evt);
			MarkMethodIfNotNull (evt.AddMethod);
			MarkMethodIfNotNull (evt.InvokeMethod);
			MarkMethodIfNotNull (evt.RemoveMethod);
		}

		void MarkMethodIfNotNull (MethodReference method)
		{
			if (method == null)
				return;

			MarkMethod (method);
		}

		protected virtual void MarkMethodBody (MethodBody body)
		{
			foreach (VariableDefinition var in body.Variables)
				MarkType (var.VariableType);

			foreach (ExceptionHandler eh in body.ExceptionHandlers)
				if (eh.HandlerType == ExceptionHandlerType.Catch)
					MarkType (eh.CatchType);

			foreach (Instruction instruction in body.Instructions)
				MarkInstruction (instruction);
		}

		protected virtual void MarkInstruction (Instruction instruction)
		{
			switch (instruction.OpCode.OperandType) {
			case OperandType.InlineField:
				MarkField ((FieldReference) instruction.Operand);
				break;
			case OperandType.InlineMethod:
				MarkMethod ((MethodReference) instruction.Operand);
				break;
			case OperandType.InlineTok:
				object token = instruction.Operand;
				if (token is TypeReference)
					MarkType ((TypeReference) token);
				else if (token is MethodReference)
					MarkMethod ((MethodReference) token);
				else
					MarkField ((FieldReference) token);
				break;
			case OperandType.InlineType:
				MarkType ((TypeReference) instruction.Operand);
				break;
			default:
				break;
			}
		}
	}
}
