using System;
using System.Diagnostics;

namespace Internal.Runtime.CompilerServices
{
	[Conditional ("ALWAYSREMOVED")]
	[AttributeUsage(AttributeTargets.All)]
	class RelocatedTypeAttribute : Attribute
	{
		public RelocatedTypeAttribute(String originalAssemblySimpleName)
		{
		}
	}
}