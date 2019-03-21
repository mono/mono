//
// Mono codegen gateway functions
//
//

using System;

namespace Mono {
	public static class Codegen
	{
		[AttributeUsage(AttributeTargets.All)]
		public class Reflected : Attribute
		{
		}
	}
}
