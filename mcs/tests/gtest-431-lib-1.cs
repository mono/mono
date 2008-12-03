// Compiler options: -target:library -noconfig

namespace System.Runtime.CompilerServices
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
	public sealed class ExtensionAttribute : Attribute { }
}
