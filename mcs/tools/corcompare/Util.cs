using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;

using GuiCompare;

namespace CorCompare {

	static class TypeHelper {

		public static AssemblyResolver Resolver = new AssemblyResolver ();

		internal static bool IsPublic (TypeReference typeref)
		{
			if (typeref == null)
				throw new ArgumentNullException ("typeref");

			TypeDefinition td = typeref.Resolve ();
			return td.IsPublic;
		}

		internal static bool IsDelegate (TypeReference typeref)
		{
			return IsDerivedFrom (typeref, "System.MulticastDelegate");
		}

		internal static bool IsDerivedFrom (TypeReference type, string derivedFrom)
		{
			foreach (var def in WalkHierarchy (type))
				if (def.FullName == derivedFrom)
					return true;

			return false;
		}

		internal static IEnumerable<TypeDefinition> WalkHierarchy (TypeReference type)
		{
			for (var def = type.Resolve (); def != null; def = GetBaseType (def))
				yield return def;
		}

		internal static IEnumerable<TypeReference> GetInterfaces (TypeReference type)
		{
			var ifaces = new Dictionary<string, TypeReference> ();

			foreach (var def in WalkHierarchy (type))
				foreach (TypeReference iface in def.Interfaces)
					ifaces [iface.FullName] = iface;

			return ifaces.Values;
		}

		internal static TypeDefinition GetBaseType (TypeDefinition child)
		{
			if (child.BaseType == null)
				return null;

			return child.BaseType.Resolve ();
		}

		internal static bool IsPublic (CustomAttribute att)
		{
			return IsPublic (att.Constructor.DeclaringType);
		}

		internal static string GetFullName (CustomAttribute att)
		{
			return att.Constructor.DeclaringType.FullName;
		}

		internal static TypeDefinition GetTypeDefinition (CustomAttribute att)
		{
			return att.Constructor.DeclaringType.Resolve ();
		}
	}
}
