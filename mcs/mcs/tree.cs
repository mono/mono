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
		Hashtable ifaces;

		// <summary>
		//   Keeps track of the structs defined in the source code
		// </summary>
		Hashtable structs;

		// <summary>
		//   Keeps track of the classes defined in the source code
		// </summary>
		Hashtable classes;

		public Tree ()
		{
			root_types = new TypeContainer (null, "");
		}


		public void RecordInterface (string name, Interface iface)
		{
			if (ifaces == null)
				ifaces = new Hashtable ();

			ifaces.Add (name, iface);
		}
		
		public void RecordStruct (string name, Struct s)
		{
			if (structs == null)
				structs = new Hashtable ();

			structs.Add (name, s);
		}
		
		public void RecordClass (string name, Class c)
		{
			if (classes == null)
				classes = new Hashtable ();

			classes.Add (name, c);
		}
		
		public TypeContainer Types {
			get {
				return root_types;
			}
		}

		public Hashtable Interfaces {
			get {
				return ifaces;
			}
		}

		public Hashtable Classes {
			get {
				return classes;
			}
		}

		public Hashtable Structs {
			get {
				return structs;
			}
		}
	}
}
