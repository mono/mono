//
// tree.cs: keeps a tree representation of the generated code
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace CIR
{

	// <summary>
	//   A storage for temporary IL trees
	// </summary>
	
	public class Tree {
		TypeContainer root_types;

		// <summary>
		//   Holds the Array of Assemblies that have been loaded
		//   (either because it is the default or the user used the
		//   -r command line option)
		// </summary>
		ArrayList assemblies;

		// <summary>
		//   This is used to map defined FQN to Types
		// </summary>
		Hashtable types;

		// <summary>
		//   This maps source defined types that are defined
		//   in the source code to their object holders (Class, Struct, 
		//   Interface) and that have not yet been made into real
		//   types. 
		// </summary>
		Hashtable source_types;

		AppDomain current_domain;
		AssemblyBuilder assembly_builder;
		ModuleBuilder   module_builder;
		
		public Tree ()
		{
			root_types = new TypeContainer (null, "");
			assemblies = new ArrayList ();
			types = new Hashtable ();
			source_types = new Hashtable ();
		}

		public int BuilderInit (string name, string output)
		{
			AssemblyName an;
			
			an = new AssemblyName ();
			an.Name = "AssemblyName";
			current_domain = AppDomain.CurrentDomain;
			assembly_builder = current_domain.DefineDynamicAssembly (
				an, AssemblyBuilderAccess.RunAndSave);

			module_builder = assembly_builder.DefineDynamicModule (name, output);

			return 0;
		}

		public AssemblyBuilder AssemblyBuilder {
			get {
				return assembly_builder;
			}
		}

		public ModuleBuilder ModuleBuilder {
			get {
				return module_builder;
			}
		}

		public void RecordType (string name, DeclSpace decl)
		{
			source_types.Add (name, decl);
		}
		
		public TypeContainer Types {
			get {
				return root_types;
			}
		}

		public int ResolveTypeContainerTypes (TypeContainer type)
		{
			return 0;
		}
		
		public int ResolveNames (TypeContainer types)
		{
			int errors = 0;
			
			if (types == null)
				return 0;

			foreach (DictionaryEntry de in types.Types){
				TypeContainer type = (TypeContainer) de.Value;

				errors += ResolveTypeContainerTypes (type);
			}

			return errors;
		}

		public Type ResolveType (string name)
		{
			Type t = (Type) types [name];
			
			if (t != null)
				return t;

			DeclSpace decl;
			decl = (DeclSpace) source_types [name];

			// FIXME: handle using here.
			if (decl == null){
				CSC.CSharpParser.error (234, "The type or namespace name '" + name
						        + "' does not exist in the current space");
				return null;
			}

			return decl.Define (this);
		}
		
		int iscan_errors = 0;
		void interface_scan (TypeContainer container, object data)
		{
			foreach (Interface iface in container.Interfaces){
				Type t = ResolveType (iface.Name);

				if (t != null){
					CSC.CSharpParser.error (101, "There is already a definition for " + iface.Name);
					iscan_errors++;
				}
				iface.Define (this);
			}
		}

		public int ResolveInterfaces (TypeContainer type)
		{
			TypeContainer.VisitContainer iscanner;

			iscanner = new TypeContainer.VisitContainer (interface_scan);
			type.VisitTypes (iscanner, this);
			return iscan_errors;
		}
		
		public int ResolveTypeContainerParents (TypeContainer type)
		{
			return type.ResolveParents (this);
		}

		public int Resolve ()
		{
			int errors = 0;

			errors += ResolveInterfaces (root_types);
			errors += ResolveTypeContainerParents (root_types);
			return 0;
		}

		public void AddAssembly (Assembly a)
		{
			assemblies.Add (a);
			foreach (Type t in a.GetExportedTypes ()){
				types.Add (t.FullName, t);
			}
		}
	}
}
