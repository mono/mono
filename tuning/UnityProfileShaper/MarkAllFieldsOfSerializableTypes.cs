using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Mono.Cecil;

namespace Mono.Linker.Steps
{
	public class MarkAllFieldsOfSerializableTypes : ResolveStep
	{
 		public override void Process(LinkContext context)
		{
			foreach(var assembly in context.GetAssemblies())
			{
				foreach (TypeDefinition type in assembly.MainModule.Types)
				{
					if (type.IsSerializable) //The C# [Serializable] attribute, does not map to an IL attribute. in IL "serializable" is an IL level thing.
					{
						Annotations.SetPreserve(type,TypePreserve.Fields);
					}
				}
			}
		}
	}
}
