// Compiler options: -t:library

using System;

namespace System.Runtime.CompilerServices
{
	[AttributeUsage (AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
	public sealed class ExtensionAttribute : Attribute {
	}
}