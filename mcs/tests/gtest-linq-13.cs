using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

class Program {

	public static void Main ()
	{
	}

	static void Foo (TypeDefinition type)
	{
		var res = from MethodDefinition meth in type.Methods
					select meth;
	}
}

interface IFoo
{
}

static class Extension
{
	public static IEnumerable<T> Cast<T> (this IFoo i)
	{
		return null;
	}
}

public class MethodDefinition
{
}

public class TypeDefinition
{
	public MethodDefinitionCollection Methods { get { return null; } set {} }
}

public class MethodDefinitionCollection : CollectionBase 
{
}