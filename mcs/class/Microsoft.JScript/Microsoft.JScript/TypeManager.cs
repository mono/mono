//
// TypeManager.cs: Here we keep Builders and Info's
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2004, Cesar Lopez Nataren
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;

namespace Microsoft.JScript {

	internal class TypeManager {

		internal static Hashtable methods;

		static TypeManager ()
		{
			methods = new Hashtable ();
		}

		internal static void AddMethod (string name, MethodBuilder builder)
		{
			methods.Add (name, builder);
		}

		internal static MethodInfo GetMethod (string name)
		{
			return (MethodInfo) methods [name];
		}
	}
}
