//
// rootcontext.cs: keeps track of our tree representation, and assemblies loaded.
//
// Author: Miguel de Icaza (miguel@ximian.com)
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

		Report report;

		Type object_type;
		
		public RootContext ()
		{
			object o = null;
			
			tree = new Tree ();
			type_manager = new TypeManager ();
			report = new Report ();

			object_type = o.GetType ();
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
		Type [] GetInterfaces (Interface iface, out bool error)
		{
			ArrayList bases = iface.Bases;
			Hashtable source_ifaces;
			Type [] tbases;
			int i;

			error = false;
			if (bases == null)
				return null;
			
			tbases = new Type [bases.Count];
			i = 0;
			source_ifaces = tree.Interfaces;

			foreach (string name in iface.Bases){
				Type t = type_manager.LookupType (name);
				Interface parent;
				
				if (t != null){
					tbases [i++] = t;
					continue;
				}
				parent = (Interface) source_ifaces [name];
				if (parent == null){
					error = true;
					report.Error (246, "Can not find type `"+name+
						      "' while defining interface `"+iface.Name+"'");
					return null;
				}
				t = CreateInterface (parent);
				if (t == null){
					report.Error (529,
						      "Inherited interface `"+name+"' in `"+
						      iface.Name+"' is recursive");
					error = true;
					return null;
				}
				tbases [i++] = t;
			}

			return tbases;
		}
		
		//
		// Creates the Interface @iface using the ModuleBuilder
		//
		// TODO:
		//   Resolve recursively dependencies.
		//
		TypeBuilder CreateInterface (Interface iface)
		{
			TypeBuilder tb = iface.Definition;
			bool error;

			if (tb != null)
				return tb;
			
			if (iface.InTransit)
				return null;
			iface.InTransit = true;

			string name = iface.Name;
			Type [] ifaces = GetInterfaces (iface, out error);

			if (error)
				return null;

			tb = mb.DefineType (name,
					    TypeAttributes.Interface |
					    TypeAttributes.Public |
					    TypeAttributes.Abstract,
					    null,   // Parent Type
					    ifaces);
			iface.Definition = tb;

			//
			// if Recursive_Def (child) == false
			//      error (child.Name recursive def with iface.Name)
			//
			type_manager.AddUserType (name, tb);

			iface.InTransit = false;
			return tb;
		}

		// <remarks>
		//   This function is used to resolve the interfaces:
		//   it creates the Type's as it scans for the user defined
		//   interfaces.  This is required so we can tell the
		//   interfaces from classes appart on the class resolution.
		// </remarks>
		public void ResolveInterfaceBases ()
		{
			Hashtable ifaces = tree.Interfaces;

			if (ifaces == null)
				return;

			foreach (DictionaryEntry de in ifaces)
				CreateInterface ((Interface) de.Value);
		}

		Type [] GetBases (out Type parent, out bool error)
		{
			error = false;
			parent = null;

			return null;
		}
		
		TypeBuilder CreateClass (Class c)
		{
			TypeBuilder tb = c.Definition;
			Type parent;
			Type [] ifaces;
			bool error;
			
			if (tb != null)
				return tb;

			if (c.InTransit)
				return null;
			c.InTransit = true;

			string name = c.Name;

			if (c.Bases != null){
				ifaces = GetBases (out parent, out error);
			} else {
				ifaces = null;
				parent = object_type;
			}

			return tb;
		}
		       
		// <remarks>
		//   This function resolves the parenthood of the classes
		//   This should be invoked after the ResolveInterfaceBases,
		//   as we need to be able to tell the difference between interfaces
		//   and classes in the list of `bases' that was provided in the source
		// </remarks>
		public void ResolveClassBases ()
		{
			Hashtable classes = tree.Classes;

			if (classes == null)
				return;

			foreach (DictionaryEntry de in classes){
				CreateClass ((Class) (de.Value));
			}
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
			foreach (TypeBuilder t in type_manager.UserTypes){
				t.CreateType ();
			}
		}
	}
}
	      
