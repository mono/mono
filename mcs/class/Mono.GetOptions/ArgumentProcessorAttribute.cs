using System;

namespace Mono.GetOptions
{

	[AttributeUsage(AttributeTargets.Method)]
	public class ArgumentProcessorAttribute : Attribute
	{
		public ArgumentProcessorAttribute() {}
	}

}
