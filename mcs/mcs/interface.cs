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

		TypeContainer parent;
		
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
			this.parent = parent;
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

		void Error111 (InterfaceMethod im)
		{
			parent.RootContext.Report.Error (
				111,
				"Interface `" + Name + "' already contains a definition with the " +
				"same return value and paramenter types for method `" + im.Name + "'");
		}
		
		void PopulateMethods ()
		{
			foreach (InterfaceMethod im in defined_method_list){
				Type ReturnType = parent.LookupType (im.ReturnType, true);
			
				TypeBuilder.DefineMethod (
					im.Name, MethodAttributes.Public,
					ReturnType, im.ParameterTypes (parent));
			}
		}

		// <summary>
		//   Performs the semantic analysis for all the interface members
		//   that were declared
		// </summary>
		bool SemanticAnalysis ()
		{
			Hashtable methods = new Hashtable ();

			//
			// First check that all methods with the same name
			// have a different signature.
			//
			foreach (InterfaceMethod im in defined_method_list){
				string sig = im.GetSignature (parent);

				//
				// If there was an undefined Type on the signatures
				// 
				if (sig == null)
					continue;

				if (methods [sig] != null){
					Error111 (im);
					return false;
				}
			}

			return true;
		}

		// <summary>
		//   Performs semantic analysis, and then generates the IL interfaces
		// </summary>
		public void Populate ()
		{
			if (!SemanticAnalysis ())
				return;
			
			PopulateMethods ();
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
		string type;
		
		public InterfaceProperty (string type, string name,
					  bool is_new, bool has_get, bool has_set)
			: base (name)
		{
			this.type = type;
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

		public string Type {
			get {
				return type;
			}
		}
	}

	public class InterfaceEvent : InterfaceMemberBase {
		string type;
		bool is_new;
		
		public InterfaceEvent (string type, string name, bool is_new)
			: base (name)
		{
			this.type = type;
			this.is_new = is_new;
		}

		public string Type {
			get {
				return type;
			}
		}

		public bool IsNew {
			get {
				return is_new;
			}
		}
	}
	
	public class InterfaceMethod : InterfaceMemberBase {
		public readonly string     ReturnType;
		public readonly bool       IsNew;
		public readonly Parameters Parameters;
		
		public InterfaceMethod (string return_type, string name, bool is_new, Parameters args)
			: base (name)
		{
			this.ReturnType = return_type;
			this.IsNew = is_new;
			this.Parameters = args;
		}

		// <summary>
		//   Returns the signature for this interface method
		// </summary>
		public string GetSignature (TypeContainer tc)
		{
			Type ret = tc.LookupType (ReturnType, false);
			string args = Parameters.GetSignature (tc);

			if ((ret == null) || (args == null))
				return null;
			
			return (IsNew ? "new-" : "") + ret.FullName + "(" + args + ")";
		}

		public Type [] ParameterTypes (TypeContainer tc)
		{
			return Parameters.GetTypes (tc);
		}

	}

	public class InterfaceIndexer : InterfaceMemberBase {
		bool do_get, do_set, is_new;
		Parameters args;
		string type;
		
		public InterfaceIndexer (string type, Parameters args, bool do_get, bool do_set, bool is_new)
			: base ("")
		{
			this.type = type;
			this.args = args;
			this.do_get = do_get;
			this.do_set = do_set;
			this.is_new = is_new;
		}

		public string Type {
			get {
				return type;
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
