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
		const MethodAttributes interface_method_attributes =
			MethodAttributes.Public |
			MethodAttributes.Abstract |
			MethodAttributes.HideBySig |
			MethodAttributes.NewSlot |
			MethodAttributes.Virtual;

		const MethodAttributes property_attributes =
			MethodAttributes.Public |
			MethodAttributes.Abstract |
			MethodAttributes.HideBySig |
			MethodAttributes.NewSlot |
			MethodAttributes.SpecialName |
			MethodAttributes.Virtual;
		
		ArrayList bases;
		int mod_flags;
		
		ArrayList defined_method;
		ArrayList defined_indexer;
		ArrayList defined_events;
		ArrayList defined_properties;

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

			if (defined_method == null)
				defined_method = new ArrayList ();

			defined_method.Add (imethod);
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

			if (defined_properties == null)
				defined_properties = new ArrayList ();

			defined_properties.Add (iprop);
			return AdditionResult.Success;
		}

		public AdditionResult AddEvent (InterfaceEvent ievent)
		{
			string name = ievent.Name;
			AdditionResult res;
			
			if ((res = IsValid (name)) != AdditionResult.Success)
				return res;

			DefineName (name, ievent);

			if (defined_events == null)
				defined_events = new ArrayList ();

			defined_events.Add (ievent);
			return AdditionResult.Success;
		}

		public bool AddIndexer (InterfaceIndexer iindexer)
		{
			if (defined_indexer == null)
				defined_indexer = new ArrayList ();
			
			defined_indexer.Add (iindexer);
			return true;
		}
		
		public ArrayList InterfaceMethods {
			get {
				return defined_method;
			}
		}

		public ArrayList InterfaceProperties {
			get {
				return defined_properties;
			}
		}

		public ArrayList InterfaceEvents {
			get {
				return defined_events;
			}
		}

		public ArrayList InterfaceIndexers {
			get {
				return defined_indexer;
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

		//
		// Populates the methods in the interface
		//
		void PopulateMethod (InterfaceMethod im)
		{
			Type return_type = parent.LookupType (im.ReturnType, true);
			Type [] arg_types = im.ParameterTypes (parent);
			MethodBuilder mb;
			Parameter [] p;
			int i;
			
			//
			// Create the method
			//
			mb = TypeBuilder.DefineMethod (
				im.Name, interface_method_attributes,
				return_type, arg_types);
			
			//
			// Define each type attribute (in/out/ref) and
			// the argument names.
			//
			p = im.Parameters.FixedParameters;
			if (p != null){
				for (i = 0; i < p.Length; i++)
					mb.DefineParameter (i + 1, p [i].Attributes, p [i].Name);

				if (i != arg_types.Length)
					Console.WriteLine ("Implement the type definition for params");
			}
		}

		//
		// Populates the properties in the interface
		//
		void PopulateProperty (InterfaceProperty ip)
		{
			PropertyBuilder pb;
			MethodBuilder mb;
			Type prop_type = parent.LookupType (ip.Type, true);
			Type [] setter_args = new Type [1];

			setter_args [0] = prop_type;

			//
			// FIXME: properties are missing the following
			// flags: hidebysig newslot specialname
			// 
			pb = TypeBuilder.DefineProperty (
				ip.Name, PropertyAttributes.None,
				prop_type, null);

			if (ip.HasGet){
				mb = TypeBuilder.DefineMethod (
					"get_" + ip.Name, property_attributes ,
					prop_type, null);

				pb.SetGetMethod (mb);
			}

			if (ip.HasSet){
				setter_args [0] = prop_type;

				mb = TypeBuilder.DefineMethod (
					"set_" + ip.Name, property_attributes,
					null, setter_args);

				mb.DefineParameter (1, ParameterAttributes.None, "value");
				pb.SetSetMethod (mb);
			}
		}

		//
		// Populates the events in the interface
		//
		void PopulateEvent (InterfaceEvent ie)
		{
			//
		        // FIXME: We need to do this after delegates have been
			// declared or we declare them recursively.
			//
		}

		//
		// Populates the indexers in the interface
		//
		void PopulateIndexer (InterfaceIndexer ii)
		{
			PropertyBuilder pb;
			Type prop_type = parent.LookupType (ii.Type, true);
			Type [] arg_types = ii.ParameterTypes (parent);
			Type [] value_arg_types;

			//
			// Sets up the extra invisible `value' argument for setters.
			// 
			if (arg_types != null){
				int count = arg_types.Length;
				value_arg_types = new Type [count + 1];

				arg_types.CopyTo (value_arg_types, 0);
				value_arg_types [count] = prop_type;
			} else {
				value_arg_types = new Type [1];

				value_arg_types [1] = prop_type;
			}

			pb = TypeBuilder.DefineProperty (
				"Item", PropertyAttributes.None,
				prop_type, arg_types);

			if (ii.HasGet){
				MethodBuilder get_item;
				Parameter [] p = ii.Parameters.FixedParameters;
				
				get_item = TypeBuilder.DefineMethod (
					"get_Item", property_attributes, prop_type, arg_types);
				pb.SetGetMethod (get_item);

				if (p != null){
					for (int i = 0; i < p.Length; i++)
						get_item.DefineParameter (
							i + 1,
							p [i].Attributes, p [i].Name);
				}
			}

			if (ii.HasSet){
				Parameter [] p = ii.Parameters.FixedParameters;
				MethodBuilder set_item;
				int i = 0;
				
				set_item = TypeBuilder.DefineMethod (
					"set_Item", property_attributes, null, value_arg_types);
				pb.SetSetMethod (set_item);

				if (p != null){
					for (; i < p.Length; i++)
						set_item.DefineParameter (
							i + 1,
							p [i].Attributes, p [i].Name);
				}
				set_item.DefineParameter (i + 1, ParameterAttributes.None, "value");
			}
		}

		// <summary>
		//   Performs the semantic analysis for all the interface members
		//   that were declared
		// </summary>
		bool SemanticAnalysis ()
		{
			Hashtable methods = new Hashtable ();

			
			if (defined_method != null){
				foreach (InterfaceMethod im in defined_method){
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
			}

			//
			// FIXME: Here I should check i
			// 
			return true;
		}

		// <summary>
		//   Performs semantic analysis, and then generates the IL interfaces
		// </summary>
		public void Populate ()
		{
			if (!SemanticAnalysis ())
				return;

			if (defined_method != null){
				foreach (InterfaceMethod im in defined_method)
					PopulateMethod (im);
			}

			if (defined_properties != null){
				foreach (InterfaceProperty ip in defined_properties)
					PopulateProperty (ip);
			}

			if (defined_events != null)
				foreach (InterfaceEvent ie in defined_events)
					PopulateEvent (ie);

			if (defined_indexer != null)
				foreach (InterfaceIndexer ii in defined_indexer)
					PopulateIndexer (ii);
		}
	}

	public class InterfaceMemberBase {
		public readonly string Name;
		public readonly bool IsNew;
		
		public InterfaceMemberBase (string name, bool is_new)
		{
			Name = name;
			IsNew = is_new;
		}
	}
	
	public class InterfaceProperty : InterfaceMemberBase {
		public readonly bool HasSet;
		public readonly bool HasGet;
		public readonly string Type;
		public readonly string type;
		
		public InterfaceProperty (string type, string name,
					  bool is_new, bool has_get, bool has_set)
			: base (name, is_new)
		{
			Type = type;
			HasGet = has_get;
			HasSet = has_set;
		}
	}

	public class InterfaceEvent : InterfaceMemberBase {
		public readonly string Type;
		
		public InterfaceEvent (string type, string name, bool is_new)
			: base (name, is_new)
		{
			Type = type;
		}
	}
	
	public class InterfaceMethod : InterfaceMemberBase {
		public readonly string     ReturnType;
		public readonly Parameters Parameters;
		
		public InterfaceMethod (string return_type, string name, bool is_new, Parameters args)
			: base (name, is_new)
		{
			this.ReturnType = return_type;
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
			return Parameters.GetParameterInfo (tc);
		}
	}

	public class InterfaceIndexer : InterfaceMemberBase {
		public readonly bool HasGet, HasSet;
		public readonly Parameters Parameters;
		public readonly string Type;
		
		public InterfaceIndexer (string type, Parameters args, bool do_get, bool do_set, bool is_new)
			: base ("", is_new)
		{
			Type = type;
			Parameters = args;
			HasGet = do_get;
			HasSet = do_set;
		}

		public Type [] ParameterTypes (TypeContainer tc)
		{
			return Parameters.GetParameterInfo (tc);
		}
	}
}
