using System;

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Parameter)]
	public sealed class OptionalAttribute : Attribute {
		public OptionalAttribute () {
		}
	}
}
