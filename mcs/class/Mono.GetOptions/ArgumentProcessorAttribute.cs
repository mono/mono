//
// ArgumentProcessorAttribute.cs
//
// Author: Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2002 Rafael Teixeira
//
using System;

namespace Mono.GetOptions
{

	[AttributeUsage(AttributeTargets.Method)]
	public class ArgumentProcessorAttribute : Attribute
	{
		public ArgumentProcessorAttribute() {}
	}

}
