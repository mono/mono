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

		static IdentificationTable infos;
		static IdentificationTable locals;

		static TypeManager ()
		{
			infos = new IdentificationTable ();
			locals = new IdentificationTable ();
		}

		internal static void BeginScope ()
		{
			infos.BeginScope ();
			locals.BeginScope ();
		}

		internal static void EndScope ()
		{
			infos.EndScope ();
			locals.EndScope ();
		}

		internal static void Add (string name, object o)
		{
			infos.Enter (Symbol.CreateSymbol (name), o);
		}

		internal static void AddLocal (string name, object o)
		{
			locals.Enter (Symbol.CreateSymbol (name), o);
		}

		internal static object Get (string name)
		{
			return infos.Get (Symbol.CreateSymbol (name));
		}

		internal static object GetLocal (string name)
		{
			return locals.Get (Symbol.CreateSymbol (name));
		}

		internal static void Set (string name, object o)
		{
			object obj = Get (name);
			obj = o;
		}

		internal static object defined_in_current_scope (string id)
		{
			if (infos.InCurrentScope (Symbol.CreateSymbol (id)))
				return Get (id);
			return null;					
		}
	}
}
