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

namespace Mono.MonoBASIC
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
		//  Keeps track of namespaces defined in the source code
		// </summary>
		Hashtable namespaces;

		// <summary>
		//   Keeps track of all the types definied (classes, structs, ifaces, enums)
		// </summary>
		Hashtable decls;
		
		public Tree ()
		{
			root_types = new TypeContainer (null, "", null, new Location (-1, 0));

			decls = new Hashtable ();
			namespaces = new Hashtable ();
		}

		public void RecordDecl (string name, DeclSpace ds)
		{
			if (decls.Contains (name)){
				Report.Error (
					101, ds.Location,
					"There is already a definition for `" + name + "'");
				DeclSpace other = (DeclSpace) decls [name];
				Report.Error (0,
					other.Location, "(Location of symbol related to previous error)");
				return;
			}
			decls.Add (name, ds);
		}
		
		public Namespace RecordNamespace (Namespace parent, string file, string name)
		{
			Namespace ns = new Namespace (parent, name);

			if (namespaces.Contains (file)){
				Hashtable ns_ns = (Hashtable) namespaces [file];

				if (ns_ns.Contains (ns.Name))
					return (Namespace) ns_ns [ns.Name];
				ns_ns.Add (ns.Name, ns);
			} else {
				Hashtable new_table = new Hashtable ();
				namespaces [file] = new_table;

				new_table.Add (ns.Name, ns);
			}

			return ns;
		}

		//
		// FIXME: Why are we using Types?
		//
                public TypeContainer Types {
                        get {
                                return root_types;
                        }
                }

		public Hashtable Decls {
			get {
				return decls;
			}
		}

		public Hashtable Namespaces {
			get {
				return namespaces;
			}
		}
	}
}
