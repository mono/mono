//
// rootcontext.cs: keeps track of our tree representation, and assemblies loaded.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)

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
	}
}
	      
