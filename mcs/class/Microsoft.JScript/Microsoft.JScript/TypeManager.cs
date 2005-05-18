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
		static IdentificationTable local_script_functions;

		static TypeManager ()
		{
			infos = new IdentificationTable ();
			local_script_functions = new IdentificationTable ();
		}

		internal static void BeginScope ()
		{
			infos.BeginScope ();
			local_script_functions.BeginScope ();
		}

		internal static void EndScope ()
		{
			infos.EndScope ();
			local_script_functions.EndScope ();
		}

		internal static void Add (string name, object o)
		{
			infos.Enter (Symbol.CreateSymbol (name), o);
		}

		internal static void AddLocalScriptFunction (string name, object o)
		{
			local_script_functions.Enter (Symbol.CreateSymbol (name), o);
		}

		internal static object Get (string name)
		{
			return infos.Get (Symbol.CreateSymbol (name));
		}

		internal static object GetLocalScriptFunction (string name)
		{
			return local_script_functions.Get (Symbol.CreateSymbol (name));
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

		internal static object [] CurrentLocals {
			get { return infos.CurrentLocals; }
		}

		internal static DictionaryEntry [] LocalsAtDepth (int n)
		{
			return infos.LocalsAtDepth (n);
		}
	}
}
