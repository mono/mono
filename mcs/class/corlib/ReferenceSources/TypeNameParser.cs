using System.Reflection;
using System.Threading;

namespace System
{
	internal sealed class TypeNameParser
	{
		internal static Type GetType(
            string typeName,
            Func<AssemblyName, Assembly> assemblyResolver,
            Func<Assembly, string, bool, Type> typeResolver,
            bool throwOnError,
            bool ignoreCase,
            ref StackCrawlMark stackMark)
		{
			TypeSpec spec = TypeSpec.Parse (typeName);
			return spec.Resolve (assemblyResolver, typeResolver, throwOnError, ignoreCase, ref stackMark);
		}
	}
}