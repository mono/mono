//
// namespace.cs: Tracks namespaces
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//
using System;
using System.Collections;
using Mono.Languages;

namespace Mono.MonoBASIC {

	/// <summary>
	///   Keeps track of the namespaces defined in the C# code.
	/// </summary>
	public class Namespace {
		static ArrayList all_namespaces = new ArrayList ();
		
		Namespace parent;
		string name;
				
		/// <summary>
		///   Constructor Takes the current namespace and the
		///   name.  This is bootstrapped with parent == null
		///   and name = ""
		/// </summary>
		public Namespace (Namespace parent, string name)
		{
			this.name = name;
			this.parent = parent;

			all_namespaces.Add (this);
		}

		/// <summary>
		///   The qualified name of the current namespace
		/// </summary>
		public string Name {
			get {
				string pname = parent != null ? parent.Name : "";
				
				if (pname == "")
					return name;
				else
					return parent.Name + "." + name;
			}
		}

		/// <summary>
		///   The parent of this namespace, used by the parser to "Pop"
		///   the current namespace declaration
		/// </summary>
		public Namespace Parent {
			get {
				return parent;
			}
		}
		
		/// <summary>
		///   Show the qualified name of the namespace contained here
		/// </summary>
		public override string ToString() {
			return Name;
		}
		
	}
}
