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

namespace Mono.CSharp {

	/// <summary>
	///   Interfaces
	/// </summary>
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
		
		ArrayList defined_method;
		ArrayList defined_indexer;
		ArrayList defined_events;
		ArrayList defined_properties;

		ArrayList method_builders;
		ArrayList property_builders;
		
		Attributes OptAttributes;

		// These will happen after the semantic analysis
		
		// Hashtable defined_indexers;
		// Hashtable defined_methods;
		
		/// <summary>
		///   Modifiers allowed in a class declaration
		/// </summary>
		public const int AllowedModifiers =
			Modifiers.NEW       |
			Modifiers.PUBLIC    |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL  |
		 	Modifiers.UNSAFE    |
			Modifiers.PRIVATE;

		public Interface (TypeContainer parent, string name, int mod, Attributes attrs, Location l)
			: base (parent, name, l)
		{
			ModFlags = Modifiers.Check (AllowedModifiers, mod, Modifiers.PRIVATE, l);
			OptAttributes = attrs;
			
			method_builders = new ArrayList ();
			property_builders = new ArrayList ();
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

		public ArrayList Bases {
			get {
				return bases;
			}

			set {
				bases = value;
			}
		}

		public virtual TypeAttributes InterfaceAttr {
			get {
				TypeAttributes x = TypeAttributes.Interface | TypeAttributes.Abstract;

				if (IsTopLevel == false) {
					
					if ((ModFlags & Modifiers.PROTECTED) != 0
					    && (ModFlags & Modifiers.INTERNAL) != 0)
						x |= TypeAttributes.NestedFamORAssem;
					else if ((ModFlags & Modifiers.PROTECTED) != 0)
						x |= TypeAttributes.NestedFamily;
					else if ((ModFlags & Modifiers.INTERNAL) != 0)
						x |= TypeAttributes.NestedAssembly;
					else if ((ModFlags & Modifiers.PUBLIC) != 0)
						x |= TypeAttributes.NestedPublic;
					else
						x |= TypeAttributes.NestedPrivate;
				} else {
					if ((ModFlags & Modifiers.PUBLIC) != 0)
						x |= TypeAttributes.Public;
					else if ((ModFlags & Modifiers.PRIVATE) != 0)
						x |= TypeAttributes.NotPublic;
				}
				
				if ((ModFlags & Modifiers.ABSTRACT) != 0)
					x |= TypeAttributes.Abstract;
				
				if ((ModFlags & Modifiers.SEALED) != 0)
					x |= TypeAttributes.Sealed;

				return x;
			}
		}
		
		void Error111 (InterfaceMemberBase ib)
		{
			Report.Error (
				111,
				"Interface `" + Name + "' already contains a definition with the " +
				"same return value and parameter types for member `" + ib.Name + "'");
		}

		bool RegisterMethod (MethodBase mb, InternalParameters ip, Type [] types)
		{
			if (!TypeManager.RegisterMethod (mb, ip, types))
				return false;

			method_builders.Add (mb);
			return true;
		}

		public MethodInfo [] GetMethods ()
		{
			int n = method_builders.Count;
			MethodInfo [] mi = new MethodInfo [n];
			
			method_builders.CopyTo (mi, 0);

			return mi;
		}

		// Hack around System.Reflection as found everywhere else
		public MemberInfo [] FindMembers (MemberTypes mt, BindingFlags bf, MemberFilter filter, object criteria)
		{
			ArrayList members = new ArrayList ();

			if ((mt & MemberTypes.Method) != 0) {
				foreach (MethodBuilder mb in method_builders)
					if (filter (mb, criteria))
						members.Add (mb);
			}

			if ((mt & MemberTypes.Property) != 0) {
				foreach (PropertyBuilder pb in property_builders)
				        if (filter (pb, criteria))
				                members.Add (pb);
			}

			if (((bf & BindingFlags.DeclaredOnly) == 0) && (TypeBuilder.BaseType != null)) {
				MemberInfo [] parent_mi;
				
				parent_mi = TypeContainer.FindMembers (
					TypeBuilder.BaseType, mt, bf, filter, criteria);

				if (parent_mi != null)
					members.AddRange (parent_mi);
			}
			
			// The rest of the cases, if any, are unhandled at present.

			int count = members.Count;

			if (count > 0) {
				MemberInfo [] mi = new MemberInfo [count];
				members.CopyTo (mi, 0);
				return mi;
			}

			return null;
		}

		//
		// Populates the methods in the interface
		//
		void PopulateMethod (TypeContainer parent, DeclSpace decl_space, InterfaceMethod im)
		{
			Type return_type = RootContext.LookupType (this, im.ReturnType, false, im.Location);
			Type [] arg_types = im.ParameterTypes (this);
			MethodBuilder mb;
			Parameter [] p;
			int i;

			if (return_type == null)
				return;

			if (return_type.IsPointer && !UnsafeOK (this))
				return;

			foreach (Type t in arg_types){

				if (t == null)
					return;
				
				if (t.IsPointer && !UnsafeOK (this))
					return;
			}
			
			//
			// Create the method
			//
			mb = TypeBuilder.DefineMethod (
				im.Name, interface_method_attributes,
				return_type, arg_types);

			InternalParameters ip = new InternalParameters (arg_types, im.Parameters);
			
			if (!RegisterMethod (mb, ip, arg_types)) {
				Error111 (im);
				return;
			}

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

			EmitContext ec = new EmitContext (parent, decl_space, Location, null,
							  return_type, ModFlags, false);

			if (im.OptAttributes != null)
				Attribute.ApplyAttributes (ec, mb, im, im.OptAttributes, Location);
		}

		//
		// Populates the properties in the interface
		//
		void PopulateProperty (TypeContainer parent, DeclSpace decl_space, InterfaceProperty ip)
		{
			PropertyBuilder pb;
			MethodBuilder get = null, set = null;
			Type prop_type = RootContext.LookupType (this, ip.Type, false, ip.Location);
			Type [] setter_args = new Type [1];

			if (prop_type == null)
				return;

			if (prop_type.IsPointer && !UnsafeOK (this))
				return;
			
			setter_args [0] = prop_type;

			//
			// FIXME: properties are missing the following
			// flags: hidebysig newslot specialname
			//
			pb = TypeBuilder.DefineProperty (
				ip.Name, PropertyAttributes.None,
				prop_type, null);

			if (ip.HasGet){
				get = TypeBuilder.DefineMethod (
					"get_" + ip.Name, property_attributes ,
					prop_type, null);

				//
				// HACK because System.Reflection.Emit is lame
				//
				Type [] null_types = null;
				InternalParameters inp = new InternalParameters
					(null_types, Parameters.GetEmptyReadOnlyParameters ());
				
				if (!RegisterMethod (get, inp, null)) {
					Error111 (ip);
					return;
				}
				
				pb.SetGetMethod (get);
			}

			if (ip.HasSet){
				setter_args [0] = prop_type;

				set = TypeBuilder.DefineMethod (
					"set_" + ip.Name, property_attributes,
					TypeManager.void_type, setter_args);

				set.DefineParameter (1, ParameterAttributes.None, "value");
				pb.SetSetMethod (set);

				//
				// HACK because System.Reflection.Emit is lame
				//
				Parameter [] parms = new Parameter [1];
				parms [0] = new Parameter (ip.Type, "value", Parameter.Modifier.NONE, null);
				InternalParameters ipp = new InternalParameters (
					this, new Parameters (parms, null, Location.Null));
					
				if (!RegisterMethod (set, ipp, setter_args)) {
					Error111 (ip);
					return;
				}
			}

			EmitContext ec = new EmitContext (parent, decl_space, Location, null,
							  null, ModFlags, false);

			if (ip.OptAttributes != null)
				Attribute.ApplyAttributes (ec, pb, ip, ip.OptAttributes, Location);

			TypeManager.RegisterProperty (pb, get, set);
			property_builders.Add (pb);
		}

		//
		// Populates the events in the interface
		//
		void PopulateEvent (TypeContainer parent, DeclSpace decl_space, InterfaceEvent ie)
		{
			//
		        // FIXME: We need to do this after delegates have been
			// declared or we declare them recursively.
			//
		}

		//
		// Populates the indexers in the interface
		//
		void PopulateIndexer (TypeContainer parent, DeclSpace decl_space, InterfaceIndexer ii)
		{
			PropertyBuilder pb;
			Type prop_type = RootContext.LookupType (this, ii.Type, false, ii.Location);
			Type [] arg_types = ii.ParameterTypes (this);
			Type [] value_arg_types;

			if (prop_type == null)
				return;

			if (prop_type.IsPointer && !UnsafeOK (this))
				return;
			
			//
			// Sets up the extra invisible `value' argument for setters.
			// 
			if (arg_types != null){
				int count = arg_types.Length;
				value_arg_types = new Type [count + 1];

				arg_types.CopyTo (value_arg_types, 0);
				value_arg_types [count] = prop_type;

				foreach (Type t in arg_types){
					if (t.IsPointer && !UnsafeOK (this))
						return;
				}
			} else {
				value_arg_types = new Type [1];

				value_arg_types [1] = prop_type;
			}

			pb = TypeBuilder.DefineProperty (
				"Item", PropertyAttributes.None,
				prop_type, arg_types);
			
			MethodBuilder set_item = null, get_item = null;
			if (ii.HasGet){
				Parameter [] p = ii.Parameters.FixedParameters;
				
				get_item = TypeBuilder.DefineMethod (
					"get_Item", property_attributes, prop_type, arg_types);
				pb.SetGetMethod (get_item);
				//
				// HACK because System.Reflection.Emit is lame
				//
				InternalParameters ip = new InternalParameters (
					arg_types, ii.Parameters);
				
				if (!RegisterMethod (get_item, ip, arg_types)) {
					Error111 (ii);
					return;
				}

				if (p != null){
					for (int i = 0; i < p.Length; i++)
						get_item.DefineParameter (
							i + 1,
							p [i].Attributes, p [i].Name);
				}
			}

			if (ii.HasSet){
				Parameter [] p = ii.Parameters.FixedParameters;
				Parameter [] pv;
				int i = 0;
				
				pv = new Parameter [p.Length + 1];
				p.CopyTo (pv, 0);
				pv [p.Length] = new Parameter (ii.Type, "value", Parameter.Modifier.NONE, null);
				Parameters value_params = new Parameters (pv, null, Location.Null);
				value_params.GetParameterInfo (decl_space);
				
				set_item = TypeBuilder.DefineMethod (
					"set_Item", property_attributes,
					TypeManager.void_type, value_arg_types);
				pb.SetSetMethod (set_item);
				//
				// HACK because System.Reflection.Emit is lame
				//
				InternalParameters ip = new InternalParameters (
					value_arg_types, value_params);
				if (!RegisterMethod (set_item, ip, value_arg_types)) {
					Error111 (ii);
					return;
				}

				if (p != null){
					for (; i < p.Length; i++)
						set_item.DefineParameter (
							i + 1,
							p [i].Attributes, p [i].Name);
				}
				
				set_item.DefineParameter (i + 1, ParameterAttributes.None, "value");
			}

			EmitContext ec = new EmitContext (parent, decl_space, Location, null,
							  null, ModFlags, false);

			if (ii.OptAttributes != null)
				Attribute.ApplyAttributes (ec, pb, ii, ii.OptAttributes, Location);

			property_builders.Add (pb);
		}

		/// <summary>
		///   Performs the semantic analysis for all the interface members
		///   that were declared
		/// </summary>
		bool SemanticAnalysis ()
		{
			Hashtable methods = new Hashtable ();

			
			if (defined_method != null){
				foreach (InterfaceMethod im in defined_method){
					string sig = im.GetSignature (this);
					
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

		Type GetInterfaceTypeByName (string name)
		{
			Type t = FindType (name);

			if (t == null)
				return null;
			
			if (t.IsInterface)
				return t;
				
			string cause;
			
			if (t.IsValueType)
				cause = "is a struct";
			else if (t.IsClass) 
				cause = "is a class";
			else
				cause = "Should not happen.";
			
			Report.Error (527, Location, "`"+name+"' " + cause +
				      ", need an interface instead");
			
			return null;
		}
		
		//
		// Returns the list of interfaces that this interface implements
		// Or null if it does not implement any interface.
		//
		// Sets the error boolean accoringly.
		//
		Type [] GetInterfaceBases (out bool error)
		{
			Type [] tbases;
			int i;

			error = false;
			if (Bases == null)
				return null;
			
			tbases = new Type [Bases.Count];
			i = 0;

			foreach (string name in Bases){
				Type t;

				t = GetInterfaceTypeByName (name);
				if (t == null){
					error = true;
					return null;
				}
				
				tbases [i++] = t;
			}
			
			return tbases;
		}
		
		//
		// <summary>
		//  Defines the Interface in the appropriate ModuleBuilder or TypeBuilder
		// </summary>
		//
		// TODO:
		//   Rework the way we recurse, because for recursive
		//   definitions of interfaces (A:B and B:A) we report the
		//   error twice, rather than once.  
		
		public override TypeBuilder DefineType ()
		{
			Type [] ifaces;
			bool error;

			if (TypeBuilder != null)
				return TypeBuilder;
			
			if (InTransit)
				return null;
			
			InTransit = true;
			
			ifaces = GetInterfaceBases (out error);

			if (error)
				return null;

			if (IsTopLevel) {
				ModuleBuilder builder = CodeGen.ModuleBuilder;

				TypeBuilder = builder.DefineType (
					Name,
					InterfaceAttr,
					(Type)null,   // Parent Type
					ifaces);
				RootContext.RegisterOrder (this);
			} else {
				TypeBuilder builder = Parent.TypeBuilder;

				TypeBuilder = builder.DefineNestedType (
					Basename,
					InterfaceAttr,
					(Type) null, //parent type
					ifaces);

				TypeContainer tc = TypeManager.LookupTypeContainer (builder);
				tc.RegisterOrder (this);
			}

			TypeManager.AddUserInterface (Name, TypeBuilder, this, ifaces);
			InTransit = false;
			
			return TypeBuilder;
		}
		
		/// <summary>
		///   Performs semantic analysis, and then generates the IL interfaces
		/// </summary>
		public override bool Define (TypeContainer parent)
		{
			if (!SemanticAnalysis ())
				return false;

			if (defined_method != null){
				foreach (InterfaceMethod im in defined_method)
					PopulateMethod (parent, this, im);
			}

			if (defined_properties != null){
				foreach (InterfaceProperty ip in defined_properties)
					PopulateProperty (parent, this, ip);
			}

			if (defined_events != null)
				foreach (InterfaceEvent ie in defined_events)
					PopulateEvent (parent, this, ie);

			//
			// FIXME: Pull the right indexer name out of the `IndexerName' attribute
			//
			if (defined_indexer != null) {
				foreach (InterfaceIndexer ii in defined_indexer)
					PopulateIndexer (parent, this, ii);

				CustomAttributeBuilder cb = EmitDefaultMemberAttr (
					parent, "Item", ModFlags, Location);
				if (cb != null)
					TypeBuilder.SetCustomAttribute (cb);
 			}
			
			return true;
		}

		public static CustomAttributeBuilder EmitDefaultMemberAttr (TypeContainer parent,
									    string name,
									    int flags,
									    Location loc)
		{
			EmitContext ec = new EmitContext (parent, loc, null, null, flags);

			Expression ml = Expression.MemberLookup (ec, TypeManager.default_member_type,
								 ".ctor", MemberTypes.Constructor,
								 BindingFlags.Public | BindingFlags.Instance,
								 Location.Null);
			
			if (!(ml is MethodGroupExpr)) {
				Console.WriteLine ("Internal error !!!!");
				return null;
			}
			
			MethodGroupExpr mg = (MethodGroupExpr) ml;

			MethodBase constructor = mg.Methods [0];

			string [] vals = { name };

			CustomAttributeBuilder cb = null;
			try {
				cb = new CustomAttributeBuilder ((ConstructorInfo) constructor, vals);
			} catch {
				Report.Warning (-100, "Can not set the indexer default member attribute");
			}

			return cb;
		}

	}

	public class InterfaceMemberBase {
		public readonly string Name;
		public readonly bool IsNew;
		public Attributes OptAttributes;
		
		public InterfaceMemberBase (string name, bool is_new, Attributes attrs)
		{
			Name = name;
			IsNew = is_new;
			OptAttributes = attrs;
		}
	}
	
	public class InterfaceProperty : InterfaceMemberBase {
		public readonly bool HasSet;
		public readonly bool HasGet;
		public readonly string Type;
		public readonly string type;
		public readonly Location Location;
		
		public InterfaceProperty (string type, string name,
					  bool is_new, bool has_get, bool has_set,
					  Attributes attrs, Location loc)
			: base (name, is_new, attrs)
		{
			Type = type;
			HasGet = has_get;
			HasSet = has_set;
			Location = loc;
		}
	}

	public class InterfaceEvent : InterfaceMemberBase {
		public readonly string Type;
		
		public InterfaceEvent (string type, string name, bool is_new, Attributes attrs)
			: base (name, is_new, attrs)
		{
			Type = type;
		}
	}
	
	public class InterfaceMethod : InterfaceMemberBase {
		public readonly string     ReturnType;
		public readonly Parameters Parameters;
		public readonly Location Location;
		
		public InterfaceMethod (string return_type, string name, bool is_new, Parameters args,
					Attributes attrs, Location l)
			: base (name, is_new, attrs)
		{
			this.ReturnType = return_type;
			this.Parameters = args;
			Location = l;
		}

		/// <summary>
		///   Returns the signature for this interface method
		/// </summary>
		public string GetSignature (DeclSpace ds)
		{
			Type ret = RootContext.LookupType (ds, ReturnType, false, Location);
			string args = Parameters.GetSignature (ds);

			if ((ret == null) || (args == null))
				return null;
			
			return (IsNew ? "new-" : "") + ret.FullName + "(" + args + ")";
		}

		public Type [] ParameterTypes (DeclSpace ds)
		{
			return Parameters.GetParameterInfo (ds);
		}
	}

	public class InterfaceIndexer : InterfaceMemberBase {
		public readonly bool HasGet, HasSet;
		public readonly Parameters Parameters;
		public readonly string Type;
		public readonly Location Location;
		
		public InterfaceIndexer (string type, Parameters args, bool do_get, bool do_set,
					 bool is_new, Attributes attrs, Location loc)
			: base ("", is_new, attrs)
		{
			Type = type;
			Parameters = args;
			HasGet = do_get;
			HasSet = do_set;
			Location = loc;
		}

		public Type [] ParameterTypes (DeclSpace ds)
		{
			return Parameters.GetParameterInfo (ds);
		}
	}
}
