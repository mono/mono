//
// interface.cs: Interface handler
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//

using System.Collections;
using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace CIR {

	public class Interface : DeclSpace {
		ArrayList bases;
		int mod_flags;
		
		ArrayList defined_method_list;
		ArrayList defined_indexer_list;
		
		Hashtable defined_events;
		Hashtable defined_properties;

		// These will happen after the semantic analysis
		
		// Hashtable defined_indexers;
		// Hashtable defined_methods;
		
		// <summary>
		//   Modifiers allowed in a class declaration
		// </summary>
		public const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Interface (TypeContainer parent, string name, int mod) : base (name)
		{
			defined_events = new Hashtable ();
			defined_method_list = new ArrayList ();
			defined_indexer_list = new ArrayList ();
			defined_properties = new Hashtable ();

			this.mod_flags = Modifiers.Check (AllowedModifiers, mod, Modifiers.PUBLIC);
		}

		public AdditionResult AddMethod (InterfaceMethod imethod)
		{
			string name = imethod.Name;
			Object value = defined_names [name];

			if (value != null){
				if (!(value is InterfaceMethod))
					return AdditionResult.NameExists;
			} 

			defined_method_list.Add (imethod);
			if (value == null)
				DefineName (name, imethod);
			
			return AdditionResult.Success;
		}

		public AdditionResult AddProperty (InterfaceProperty iprop)
		{
			AdditionResult res;
			string name = iprop.Name;

			if ((res = IsValid (name)) != AdditionResult.Success)
				return res;

			DefineName (name, iprop);

			defined_properties.Add (name, iprop);
			return AdditionResult.Success;
		}

		public AdditionResult AddEvent (InterfaceEvent ievent)
		{
			string name = ievent.Name;
			AdditionResult res;
			
			if ((res = IsValid (name)) != AdditionResult.Success)
				return res;

			DefineName (name, ievent);

			defined_events.Add (name, ievent);
			return AdditionResult.Success;
		}

		public bool AddIndexer (InterfaceIndexer iindexer)
		{
			defined_indexer_list.Add (iindexer);
			return true;
		}
		
		public Hashtable InterfaceMethods {
			get {
				return null; // defined_methods;
			}
		}

		public Hashtable InterfaceProperties {
			get {
				return defined_properties;
			}
		}

		public Hashtable InterfaceEvents {
			get {
				return defined_events;
			}
		}

		public Hashtable InterfaceIndexers {
			get {
				return null; // defined_indexers;
			}
		}

		public int ModFlags {
			get {
				return mod_flags;
			}
		}
		
		public ArrayList Bases {
			get {
				return bases;
			}

			set {
				bases = value;
			}
		}
	}

	public class InterfaceMemberBase {
		string name;

		public InterfaceMemberBase (string name)
		{
			this.name = name;
		}
		
		public string Name {
			get {
				return name;
			}
		}
	}
	
	public class InterfaceProperty : InterfaceMemberBase {
		bool has_get, has_set, is_new;
		TypeRef typeref;
		
		public InterfaceProperty (TypeRef typeref, string name,
					  bool is_new, bool has_get, bool has_set)
			: base (name)
		{
			this.typeref = typeref;
			this.is_new = is_new;
			this.has_get = has_get;
			this.has_set = has_set;
		}

		public bool HasGet {
			get {
				return has_get;
			}
		}

		public bool HasSet {
			get {
				return has_set;
			}
		}

		public bool IsNew {
			get {
				return is_new;
			}
		}

		public Type Type {
			get {
				return typeref.Type;
			}
		}
	}

	public class InterfaceEvent : InterfaceMemberBase {
		TypeRef typeref;
		bool is_new;
		
		public InterfaceEvent (TypeRef typeref, string name, bool is_new)
			: base (name)
		{
			this.typeref = typeref;
			this.is_new = is_new;
		}

		public Type Type {
			get {
				return typeref.Type;
			}
		}

		public bool IsNew {
			get {
				return is_new;
			}
		}
	}
	
	public class InterfaceMethod : InterfaceMemberBase {
		TypeRef return_type;
		bool is_new;
		Parameters args;
		
		public InterfaceMethod (TypeRef return_type, string name, bool is_new, Parameters args)
			: base (name)
		{
			this.return_type = return_type;
			this.is_new = is_new;
			this.args = args;
		}

		public Type ReturnType {
			get {
				return return_type.Type;
			}
		}

		public bool IsNew {
			get {
				return is_new;
			}
		}

		public Parameters Parameters {
			get {
				return args;
			}
		}
	}

	public class InterfaceIndexer : InterfaceMemberBase {
		bool do_get, do_set, is_new;
		Parameters args;
		TypeRef typeref;
		
		public InterfaceIndexer (TypeRef typeref, Parameters args, bool do_get, bool do_set, bool is_new)
			: base ("")
		{
			this.typeref = typeref;
			this.args = args;
			this.do_get = do_get;
			this.do_set = do_set;
			this.is_new = is_new;
		}

		public Type Type {
			get {
				return typeref.Type;
			}
		}

		public Parameters Parameters {
			get {
				return args;
			}
		}

		public bool HasGet {
			get {
				return do_get;
			}
		}

		public bool HasSet {
			get {
				return do_set;
			}
		}

		public bool IsNew {
			get {
				return is_new;
			}
		}
	}
}
