//
// rootcontext.cs: keeps track of our tree representation, and assemblies loaded.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace CIR {

	public class RootContext {

		//
		// Contains the parsed tree
		//
		Tree tree;

		//
		// Contains loaded assemblies and our generated code as we go.
		//
		TypeManager type_manager;

		//
		// The System.Reflection.Emit CodeGenerator
		//
		CilCodeGen cg;

		ModuleBuilder mb;
		
		public RootContext ()
		{
			tree = new Tree ();
			type_manager = new TypeManager ();
		}

		public TypeManager TypeManager {
			get {
				return type_manager;
			}
		}

		public Tree Tree {
			get {
				return tree;
			}
		}

		public CilCodeGen CodeGen {
			get {
				return cg;
			}

			set {
				//
				// Temporary hack, we should probably
				// intialize `cg' rather than depending on
				// external initialization of it.
				//
				cg = value;
				mb = cg.ModuleBuilder;
			}
		}

		//
		// Returns the list of interfaces that this interface implements
		// Null on error
		//
		Type [] GetInterfaces (Interface iface)
		{
			ArrayList bases = iface.Bases;
			Type [] tbases = new Type [bases.Count];
			int i = 0;
			Hashtable source_ifaces = tree.Interfaces;
			
			foreach (string name in iface.Bases){
				Type t = type_manager.LookupType (name);

				if (t != null){
					tbases [i++] = t;
					continue;
				}
				n = source_ifaces [name];
				if (n == null){
				}
			}

			return tbases;
		}
		
		//
		// Creates the Interface @iface using the ModuleBuilder
		//
		// TODO:
		//   Resolve recursively dependencies.
		//
		bool CreateInterface (Interface iface)
		{
			TypeBuilder tb;

			if (iface.InTransit)
				return false;
			iface.InTransit = true;

			string name = iface.Name;
			Type [] ifaces = GetInterfaces (iface);
			
			tb = mb.DefineType (name,
					    TypeAttributes.Interface |
					    TypeAttributes.Public |
					    TypeAttributes.Abstract);
			iface.Definition = tb;

			//
			// if Recursive_Def (child) == false
			//      error (child.Name recursive def with iface.Name)
			//
			type_manager.AddUserType (name, tb);

			iface.InTransit = false;
			return true;
		}
		
		public void ResolveInterfaceBases ()
		{
			ArrayList ifaces = tree.Interfaces;

			foreach (Interface iface in ifaces){
				string name = iface.Name;

				DefineInterface (iface);
			}
		}

		public void ResolveClassBases ()
		{
		}

		// <summary>
		//   Closes all open types
		// </summary>
		//
		// <remarks>
		//   We usually use TypeBuilder types.  When we are done
		//   creating the type (which will happen after we have addded
		//   methods, fields, etc) we need to "Define" them before we
		//   can save the Assembly
		// </remarks>
		public void CloseTypes ()
		{
			
		}
	}
}
	      
