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
using System.IO;

namespace CIR
{

	public interface ITreeDump {
		int  Dump (Tree tree, StreamWriter output);
		void ParseOptions (string options);
	}

	// <summary>
	//   
	//   We store here all the toplevel types that we have parsed,
	//   this is the root of all information we have parsed.
	// 
	// </summary>
	
	public class Tree {
		TypeContainer root_types;

		// <summary>
		//   This maps source defined types that are defined
		//   in the source code to their object holders (Class, Struct, 
		//   Interface) and that have not yet been made into real
		//   types. 
		// </summary>
		Hashtable source_types;

		public Tree ()
		{
			root_types = new TypeContainer (null, "");
			source_types = new Hashtable ();
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

	}
}
