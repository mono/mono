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
		//   Keeps track of the interfaces defined in the source code
		// </summary>
		ArrayList ifaces;

		// <summary>
		//   Keeps track of the structs defined in the source code
		// </summary>
		ArrayList structs;

		// <summary>
		//   Keeps track of the classes defined in the source code
		// </summary>
		ArrayList classes;

		public Tree ()
		{
			root_types = new TypeContainer (null, "");
		}


		public void RecordInterface (string name, Interface iface)
		{
			if (ifaces == null)
				ifaces = new ArrayList ();

			ifaces.Add (iface);
		}
		
		public void RecordStruct (string name, Struct s)
		{
			if (structs == null)
				structs = new ArrayList ();

			structs.Add (s);
		}
		
		public void RecordClass (string name, Class c)
		{
			if (classes == null)
				classes = new ArrayList ();

			classes.Add (c);
		}
		
		public TypeContainer Types {
			get {
				return root_types;
			}
		}

		public ArrayList Interfaces {
			get {
				return ifaces;
			}
		}

		public ArrayList Classes {
			get {
				return classes;
			}
		}

		public ArrayList Structs {
			get {
				return structs;
			}
		}
	}
}
