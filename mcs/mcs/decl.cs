//
// decl.cs: Declaration base class for structs, classes, enums and interfaces.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
// TODO: Move the method verification stuff from the class.cs and interface.cs here
//

using System;
using System.Collections;
using System.Reflection.Emit;

namespace CIR {

	// <summary>
	//   Base class for structs, classes, enums and interfaces.  They all create
	//   new declaration spaces.  This provides the common foundation
	//   for managing those name spaces
	// </summary>
	
	public abstract class DeclSpace {
		// <summary>
		//   this points to the actual definition that is being
		//   created with System.Reflection.Emit
		// </summary>
		TypeBuilder definition;
		
		string name, basename;
		
		// <summary>
		//   The result value from adding an declaration into
		//   a struct or a class
		// </summary>
		public enum AdditionResult {
			//
			// The declaration has been successfully
			// added to the declation space.
			//
			Success,

			//
			// The symbol has already been defined.
			//
			NameExists,

			//
			// Returned if the declation being added to the
			// name space clashes with its container name.
			//
			// The only exceptions for this are constructors
			// and static constructors
			//
			EnclosingClash,

			//
			// Returned if a constructor was created (because syntactically
			// it looked like a constructor) but was not (because the name
			// of the method is not the same as the container class
			//
			NotAConstructor
		}

		public string Name {
			get {
				return name;
			}
		}

		public string Basename {
			get {
				return basename;
			}
		}
		
		// <summary>
		//   defined_names is used for toplevel objects
		// </summary>
		protected Hashtable defined_names;

		public DeclSpace (string name)
		{
			this.name = name;
			this.basename = name.Substring (1 + name.LastIndexOf ('.'));
			defined_names = new Hashtable ();
		}

		// <summary>
		//   Returns a status code based purely on the name
		//   of the member being added
		// </summary>
		protected AdditionResult IsValid (string name)
		{
			if (name == basename)
				return AdditionResult.EnclosingClash;

			if (defined_names.Contains (name))
				return AdditionResult.NameExists;

			return AdditionResult.Success;
		}

		// <summary>
		//   Introduce @name into this declaration space and
		//   associates it with the object @o.  Note that for
		//   methods this will just point to the first method. o
		// </summary>
		protected void DefineName (string name, object o)
		{
			defined_names.Add (name, o);
		}

		bool in_transit = false;
		
		// <summary>
		//   This function is used to catch recursive definitions
		//   in declarations.
		// </summary>
		public bool InTransit {
			get {
				return in_transit;
			}

			set {
				in_transit = value;
			}
		}

		public TypeBuilder TypeBuilder {
			get {
				return definition;
			}

			set {
				definition = value;
			}
		}
	}
}
