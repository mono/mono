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
		// Creates the Interface @iface using the ModuleBuilder
		//
		// TODO:
		//   Resolve recursively dependencies.
		//
		bool CreateInterface (Interface iface)
		{
			TypeBuilder tb;
			string name = iface.Name;

			if (iface.InTransit)
				return false;
			
			iface.InTransit = true;
			tb = mb.DefineType (name,
					    TypeAttributes.Interface |
					    TypeAttributes.Public |
					    TypeAttributes.Abstract);
			tb.CreateType ();
			iface.Definition = tb;

			//
			// if Recursive_Def (child) == false
			//      error (child.Name recursive def with iface.Name)
			//
			type_manager.AddType (name, tb);

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
	}
}
	      
