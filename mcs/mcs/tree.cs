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

namespace CIR
{

	// <summary>
	//   A storage for temporary IL trees
	// </summary>
	
	public class Tree {
		TypeContainer root_types;
		ArrayList assemblies;
		Type [] types;
		
		public Tree ()
		{
			root_types = new TypeContainer (null, "");
			assemblies = new ArrayList ();
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


		public int ResolveTypeContainerParents (TypeContainer type)
		{
			return type.ResolveParents (this);
		}

		public int Resolve ()
		{
			int errors = 0;

			errors += ResolveTypeContainerParents (root_types);
			return 0;
		}

		bool fix_me_hit = false;
		
		public void AddAssembly (Assembly a)
		{
			if (fix_me_hit)
				Console.WriteLine ("AddAssembly currently can only hold types from one assembly");
			fix_me_hit = true;
			assemblies.Add (a);
			types = a.GetExportedTypes ();
		}
	}
}
