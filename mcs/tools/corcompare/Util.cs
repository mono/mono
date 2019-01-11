using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;

namespace Mono.ApiTools {

	class TypeHelper {

		public TypeHelper (bool ignoreResolutionErrors, bool ignoreInheritedInterfaces)
		{
			IgnoreResolutionErrors = ignoreResolutionErrors;
			IgnoreInheritedInterfaces = ignoreInheritedInterfaces;
		}

		public bool IgnoreResolutionErrors { get; }

		public bool IgnoreInheritedInterfaces { get; }

		public AssemblyResolver Resolver { get; } = new AssemblyResolver();

		internal bool TryResolve (CustomAttribute attribute)
		{
			if (attribute == null)
				throw new ArgumentNullException (nameof (attribute));

			try {
				var has = attribute.HasProperties;
				return true;
			} catch (AssemblyResolutionException) when (IgnoreResolutionErrors) {
				return false;
			}
		}

		internal bool IsPublic (TypeReference typeref)
		{
			if (typeref == null)
				throw new ArgumentNullException ("typeref");

			try {
				var td = typeref.Resolve ();
				if (td == null)
					return false;

				return td.IsPublic || (td.IsNestedPublic && IsPublic (td.DeclaringType));
			} catch (AssemblyResolutionException) when (IgnoreResolutionErrors) {
				return true;
			}
		}

		internal bool IsDelegate (TypeReference typeref)
		{
			return IsDerivedFrom (typeref, "System.MulticastDelegate");
		}

		internal bool IsDerivedFrom (TypeReference type, string derivedFrom)
		{
			bool first = true;
			foreach (var def in WalkHierarchy (type)) {
				if (first) {
					first = false;
					continue;
				}
				
				if (def.FullName == derivedFrom)
					return true;
			}
			
			return false;
		}

		internal IEnumerable<TypeDefinition> WalkHierarchy (TypeReference type)
		{
			for (var def = type.Resolve (); def != null; def = GetBaseType (def))
				yield return def;
		}

		internal IEnumerable<TypeReference> GetInterfaces (TypeReference type)
		{
			var ifaces = new Dictionary<string, TypeReference> ();

			foreach (var def in WalkHierarchy (type)) {
				foreach (var iface in def.Interfaces)
					ifaces [iface.InterfaceType.FullName] = iface.InterfaceType;
				if (IgnoreInheritedInterfaces)
					break;
			}

			return ifaces.Values;
		}

		internal TypeDefinition GetBaseType (TypeDefinition child)
		{
			if (child.BaseType == null)
				return null;

			try {
				return child.BaseType.Resolve ();
			} catch (AssemblyResolutionException) when (IgnoreResolutionErrors) {
				return null;
			}
		}

		internal MethodDefinition GetMethod (MethodReference method)
		{
			if (method == null)
				throw new ArgumentNullException (nameof (method));

			try {
				return method.Resolve ();
			} catch (AssemblyResolutionException) when (IgnoreResolutionErrors) {
				return null;
			}
		}

		internal bool IsPublic (CustomAttribute att)
		{
			return IsPublic (att.AttributeType);
		}

		internal string GetFullName (CustomAttribute att)
		{
			return att.AttributeType.FullName;
		}

		internal TypeDefinition GetTypeDefinition (CustomAttribute att)
		{
			return att.AttributeType.Resolve ();
		}

		bool IsOverride (MethodDefinition method)
		{
			return method.IsVirtual && !method.IsNewSlot;
		}

		public MethodDefinition GetBaseMethodInTypeHierarchy (MethodDefinition method)
		{
			if (!IsOverride (method))
				return method;

			var @base = GetBaseType (method.DeclaringType);
			while (@base != null) {
				MethodDefinition base_method = TryMatchMethod (@base.Resolve (), method);
				if (base_method != null)
					return GetBaseMethodInTypeHierarchy (base_method) ?? base_method;

				@base = GetBaseType (@base);
			}

			return method;
		}

		MethodDefinition TryMatchMethod (TypeDefinition type, MethodDefinition method)
		{
			if (!type.HasMethods)
				return null;

			foreach (MethodDefinition candidate in type.Methods)
				if (MethodMatch (candidate, method))
					return candidate;

			return null;
		}

		bool MethodMatch (MethodDefinition candidate, MethodDefinition method)
		{
			if (!candidate.IsVirtual)
				return false;

			if (candidate.Name != method.Name)
				return false;

			if (!TypeMatch (candidate.ReturnType, method.ReturnType))
				return false;

			if (candidate.Parameters.Count != method.Parameters.Count)
				return false;

			for (int i = 0; i < candidate.Parameters.Count; i++)
				if (!TypeMatch (candidate.Parameters [i].ParameterType, method.Parameters [i].ParameterType))
					return false;

			return true;
		}

		public bool TypeMatch (IModifierType a, IModifierType b)
		{
			if (!TypeMatch (a.ModifierType, b.ModifierType))
				return false;

			return TypeMatch (a.ElementType, b.ElementType);
		}

		public bool TypeMatch (TypeSpecification a, TypeSpecification b)
		{
			if (a is GenericInstanceType)
				return TypeMatch ((GenericInstanceType) a, (GenericInstanceType) b);

			if (a is IModifierType)
				return TypeMatch ((IModifierType) a, (IModifierType) b);

			return TypeMatch (a.ElementType, b.ElementType);
		}

		public bool TypeMatch (GenericInstanceType a, GenericInstanceType b)
		{
			if (!TypeMatch (a.ElementType, b.ElementType))
				return false;

			if (a.GenericArguments.Count != b.GenericArguments.Count)
				return false;

			if (a.GenericArguments.Count == 0)
				return true;

			for (int i = 0; i < a.GenericArguments.Count; i++)
				if (!TypeMatch (a.GenericArguments [i], b.GenericArguments [i]))
					return false;

			return true;
		}

		public bool TypeMatch (TypeReference a, TypeReference b)
		{
			if (a is GenericParameter)
				return true;

			if (a is TypeSpecification || b is TypeSpecification) {
				if (a.GetType () != b.GetType ())
					return false;

				return TypeMatch ((TypeSpecification) a, (TypeSpecification) b);
			}

			return a.FullName == b.FullName;
		}
	}
}
