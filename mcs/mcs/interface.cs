//
// interface.cs: Interface handler
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
#define CACHE
using System.Collections;
using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp {

	/// <summary>
	///   Interfaces
	/// </summary>
	public class Interface : DeclSpace, IMemberContainer {
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
		ArrayList event_builders;
		
		Attributes OptAttributes;

		public string IndexerName;

		IMemberContainer parent_container;
		MemberCache member_cache;

		bool members_defined;

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
			event_builders = new ArrayList ();
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

		//
		// This might trigger a definition of the methods.  This happens only
		// with Attributes, as Attribute classes are processed before interfaces.
		// Ideally, we should make everything just define recursively in terms
		// of its dependencies.
		//
		public MethodInfo [] GetMethods (TypeContainer container)
		{
			int n = 0;
			
			if (!members_defined){
				if (DefineMembers (container))
					n = method_builders.Count;
			} else
				n = method_builders.Count;
			
			MethodInfo [] mi = new MethodInfo [n];
			
			method_builders.CopyTo (mi, 0);

			return mi;
		}

		// Hack around System.Reflection as found everywhere else
		public override MemberList FindMembers (MemberTypes mt, BindingFlags bf,
							MemberFilter filter, object criteria)
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

			if ((mt & MemberTypes.Event) != 0) {
				foreach (MyEventBuilder eb in event_builders)
				        if (filter (eb, criteria))
				                members.Add (eb);
			}

			if (((bf & BindingFlags.DeclaredOnly) == 0) && (TypeBuilder.BaseType != null)) {
				MemberList parent_mi;
				
				parent_mi = TypeContainer.FindMembers (
					TypeBuilder.BaseType, mt, bf, filter, criteria);

				members.AddRange (parent_mi);
			}

			return new MemberList (members);
		}

		public override MemberCache MemberCache {
			get {
				return member_cache;
			}
		}

		//
		// Populates the methods in the interface
		//
		void PopulateMethod (TypeContainer parent, DeclSpace decl_space, InterfaceMethod im)
		{
			Type return_type = im.ReturnType.Type;
			if (return_type == null)
				return_type = this.ResolveType (im.ReturnType, false, im.Location);
			
			Type [] arg_types = im.ParameterTypes (this);
			MethodBuilder mb;
			Parameter [] p;
			int i;

			if (return_type == null)
				return;

			if (return_type.IsPointer && !UnsafeOK (this))
				return;

			if (arg_types == null)
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
				Attribute.ApplyAttributes (ec, mb, im, im.OptAttributes);
		}

		//
		// Populates the properties in the interface
		//
		void PopulateProperty (TypeContainer parent, DeclSpace decl_space, InterfaceProperty ip)
		{
			PropertyBuilder pb;
			MethodBuilder get = null, set = null;
			ip.Type = this.ResolveTypeExpr (ip.Type, false, ip.Location);
			if (ip.Type == null)
				return;
			
			Type prop_type = ip.Type.Type;
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
					(null_types, Parameters.EmptyReadOnlyParameters);
				
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
				Attribute.ApplyAttributes (ec, pb, ip, ip.OptAttributes);

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
			MyEventBuilder eb;
			MethodBuilder add = null, remove = null;
			ie.Type = this.ResolveTypeExpr (ie.Type, false, ie.Location);
			if (ie.Type == null)
				return;
			
			Type event_type = ie.Type.Type;

			if (event_type == null)
				return;

			if (event_type.IsPointer && !UnsafeOK (this))
				return;

			Type [] parameters = new Type [1];
			parameters [0] = event_type;

			eb = new MyEventBuilder (null, TypeBuilder, ie.Name,
						 EventAttributes.None, event_type);

			//
			// Now define the accessors
			//
			string add_name = "add_" + ie.Name;
			
			add = TypeBuilder.DefineMethod (
				add_name, property_attributes, null, parameters);
			add.DefineParameter (1, ParameterAttributes.None, "value");
			eb.SetAddOnMethod (add);

			string remove_name = "remove_" + ie.Name;
			remove = TypeBuilder.DefineMethod (
				remove_name, property_attributes, null, parameters);
			remove.DefineParameter (1, ParameterAttributes.None, "value");
			eb.SetRemoveOnMethod (remove);

			Parameter [] parms = new Parameter [1];
			parms [0] = new Parameter (ie.Type, "value", Parameter.Modifier.NONE, null);
			InternalParameters ip = new InternalParameters (
				this, new Parameters (parms, null, Location.Null));

			if (!RegisterMethod (add, ip, parameters)) {
				Error111 (ie);
				return;
			}
			
			if (!RegisterMethod (remove, ip, parameters)) {
				Error111 (ie);
				return;
			}

			EmitContext ec = new EmitContext (parent, decl_space, Location, null,
							  null, ModFlags, false);


			if (ie.OptAttributes != null)
				Attribute.ApplyAttributes (ec, eb, ie, ie.OptAttributes);

			TypeManager.RegisterEvent (eb, add, remove);
			event_builders.Add (eb);
		}

		//
		// Populates the indexers in the interface
		//
		void PopulateIndexer (TypeContainer parent, DeclSpace decl_space, InterfaceIndexer ii)
		{
			PropertyBuilder pb;
			ii.Type = this.ResolveTypeExpr (ii.Type, false, ii.Location);
			if (ii.Type == null)
				return;
			
			Type prop_type = ii.Type.Type;
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

			EmitContext ec = new EmitContext (parent, decl_space, Location, null,
							  null, ModFlags, false);

			IndexerName = Attribute.ScanForIndexerName (ec, ii.OptAttributes);
			if (IndexerName == null)
				IndexerName = "Item";
			
			pb = TypeBuilder.DefineProperty (
				IndexerName, PropertyAttributes.None,
				prop_type, arg_types);
			
			MethodBuilder set_item = null, get_item = null;
			if (ii.HasGet){
				Parameter [] p = ii.Parameters.FixedParameters;
				
				get_item = TypeBuilder.DefineMethod (
					"get_" + IndexerName, property_attributes,
					prop_type, arg_types);
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
					"set_" + IndexerName, property_attributes,
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

			if (ii.OptAttributes != null)
				Attribute.ApplyAttributes (ec, pb, ii, ii.OptAttributes);

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
			Type t = FindType (Location, name);

			if (t == null) {
				Report.Error (246, Location, "The type or namespace `" + name +
					      "' could not be found");
				return null;
			}
			
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

				if (!Parent.AsAccessible (t, ModFlags))
					Report.Error (61, Location,
						      "Inconsistent accessibility: base interface `" +
						      TypeManager.CSharpName (t) + "' is less " +
						      "accessible than interface `" +
						      Name + "'");

				tbases [i++] = t;
			}
			
			return TypeManager.ExpandInterfaces (tbases);
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

		//
		// Defines the indexers, and also verifies that the IndexerNameAttribute in the
		// interface is consistent.  Either it is `Item' or it is the name defined by all the
		// indexers with the `IndexerName' attribute.
		//
		// Turns out that the IndexerNameAttribute is applied to each indexer,
		// but it is never emitted, instead a DefaultName attribute is attached
		// to the interface
		//
		void DefineIndexers (TypeContainer parent)
		{
			string interface_indexer_name = null;

			foreach (InterfaceIndexer ii in defined_indexer){

				PopulateIndexer (parent, this, ii);

				if (interface_indexer_name == null){
					interface_indexer_name = IndexerName;
					continue;
				}
				
				if (IndexerName == interface_indexer_name)
					continue;
				
				Report.Error (
					668, "Two indexers have different names, " +
					" you should use the same name for all your indexers");
			}
			if (interface_indexer_name == null)
				interface_indexer_name = "Item";
			IndexerName = interface_indexer_name;
		}
		
		/// <summary>
		///   Performs semantic analysis, and then generates the IL interfaces
		/// </summary>
		public override bool DefineMembers (TypeContainer parent)
		{
			if (members_defined)
				return true;
			
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

			if (defined_indexer != null) {
				DefineIndexers (parent);

				CustomAttributeBuilder cb = EmitDefaultMemberAttr (
					parent, IndexerName, ModFlags, Location);
				if (cb != null)
					TypeBuilder.SetCustomAttribute (cb);
 			}

#if CACHE
			if (TypeBuilder.BaseType != null)
				parent_container = TypeManager.LookupMemberContainer (TypeBuilder.BaseType);

			member_cache = new MemberCache (this);
#endif
			members_defined = true;
			return true;
		}

		/// <summary>
		///   Applies all the attributes.
		/// </summary>
		public override bool Define (TypeContainer parent)
		{
			if (OptAttributes != null) {
				EmitContext ec = new EmitContext (parent, this, Location, null, null,
								  ModFlags, false);
				Attribute.ApplyAttributes (ec, TypeBuilder, this, OptAttributes);
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

		//
		// IMemberContainer
		//

		string IMemberContainer.Name {
			get {
				return Name;
			}
		}

		Type IMemberContainer.Type {
			get {
				return TypeBuilder;
			}
		}

		IMemberContainer IMemberContainer.Parent {
			get {
				return parent_container;
			}
		}

		MemberCache IMemberContainer.MemberCache {
			get {
				return member_cache;
			}
		}

		bool IMemberContainer.IsInterface {
			get {
				return true;
			}
		}

		MemberList IMemberContainer.GetMembers (MemberTypes mt, BindingFlags bf)
		{
			// Interfaces only contain instance members.
			if ((bf & BindingFlags.Instance) == 0)
				return MemberList.Empty;
			if ((bf & BindingFlags.Public) == 0)
				return MemberList.Empty;

			ArrayList members = new ArrayList ();

			if ((mt & MemberTypes.Method) != 0)
				members.AddRange (method_builders);

			if ((mt & MemberTypes.Property) != 0)
				members.AddRange (property_builders);

			if ((mt & MemberTypes.Event) != 0)
				members.AddRange (event_builders);

			return new MemberList (members);
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
		public readonly Location Location;
		public Expression Type;
		
		public InterfaceProperty (Expression type, string name,
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
		public readonly Location Location;
		public Expression Type;
		
		public InterfaceEvent (Expression type, string name, bool is_new, Attributes attrs,
				       Location loc)
			: base (name, is_new, attrs)
		{
			Type = type;
			Location = loc;
		}
	}
	
	public class InterfaceMethod : InterfaceMemberBase {
		public Expression ReturnType;
		public readonly Parameters Parameters;
		public readonly Location Location;
		
		public InterfaceMethod (Expression return_type, string name, bool is_new, Parameters args,
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
			ReturnType = ds.ResolveTypeExpr (ReturnType, false, Location);
			Type ret = ReturnType.Type;
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
		public readonly Location Location;
		public Expression Type;
		
		public InterfaceIndexer (Expression type, Parameters args, bool do_get, bool do_set,
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
