using System;

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Parameter, Inherited=false)]
	public sealed class OptionalAttribute : Attribute {
		public OptionalAttribute () {
		}
	}
}
