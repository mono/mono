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

		enum ManagedType {
			Method,
			Local
		}
	
		enum Operation {
			Add,
			Get
		}

		static Hashtable methods;
		static Hashtable locals;

		static TypeManager ()
		{
			methods = new Hashtable ();
			locals = new Hashtable ();
		}

		internal static void AddMethod (string name, MethodBuilder builder)
		{
			Operate (Operation.Add, ManagedType.Method, name, builder);
		}

		internal static MethodBuilder GetMethod (string name)
		{
			return (MethodBuilder) Operate (Operation.Get, ManagedType.Method, name, null);
		}

		internal static void AddLocal (string name, LocalBuilder loc_builder)
		{
			Operate (Operation.Add, ManagedType.Local, name, loc_builder);
		}

		internal static LocalBuilder GetLocal (string name)
		{
			return (LocalBuilder) Operate (Operation.Get, ManagedType.Local, name, null);
		}

		static object Operate (Operation op, ManagedType managed_type, string name, object obj)
		{
			switch (managed_type) {
			case ManagedType.Method:
				if (op == Operation.Add) {
					methods.Add (name, obj);
					return null;
				} else if (op == Operation.Get)
					return methods [name];
				break;
			case ManagedType.Local:
				if (op == Operation.Add) {
					locals.Add (name, obj);
					return null;
				} else if (op == Operation.Get)
					return locals [name];
				break;
			default:
				return null;
			}
			throw new Exception ("Operate, invalid arguments were supplied.");
		}
	}
}
