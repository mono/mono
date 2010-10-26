using System.Collections.Generic;
using Mono.Cecil;

namespace CoreClr.Tools
{
	public class AssemblySetResolver : DefaultAssemblyResolver
	{
		public static void SetUp(IEnumerable<AssemblyDefinition> assemblies)
		{
			new AssemblySetResolver(assemblies);
		}

		AssemblySetResolver(IEnumerable<AssemblyDefinition> assemblySet)
		{
			foreach (var assembly in assemblySet)
			{
				assembly.Resolver = this;
				RegisterAssembly(assembly);
			}
		}
	}
}