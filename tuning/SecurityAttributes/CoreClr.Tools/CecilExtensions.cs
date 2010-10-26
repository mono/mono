using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace CoreClr.Tools
{
	public static class CecilExtensions
	{
		public static string SimpleName(this AssemblyDefinition assembly)
		{
			return assembly.Name.Name;
		}

		public static IEnumerable<MethodDefinition> AllMethodDefinitions(this AssemblyDefinition assembly)
		{
			foreach (TypeDefinition type in assembly.MainModule.Types)
			{
				foreach (MethodDefinition m in type.Methods)
					yield return m;
				foreach (MethodDefinition c in type.Constructors)
					yield return c;
			}
		}

		public static string CecilTypeName(this Type type)
		{
			return type.FullName.Replace('+', '/');
		}

		public static IEnumerable<MethodDefinition> AllMethodsAndConstructors(this TypeDefinition type)
		{
			return type.Methods.Cast<MethodDefinition>().Concat(type.Constructors.Cast<MethodDefinition>());
		}
	}
}
