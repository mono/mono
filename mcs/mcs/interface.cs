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
		Type[] baseTypes;
		
		ArrayList defined_method;
		ArrayList defined_indexer;
		ArrayList defined_events;
		ArrayList defined_properties;

		ArrayList method_builders;
		ArrayList property_builders;
		ArrayList event_builders;
		

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

		public Interface (NamespaceEntry ns, TypeContainer parent, string name, int mod,
				  Attributes attrs, Location l)
			: base (ns, parent, name, attrs, l)
		{
			ModFlags = Modifiers.Check (AllowedModifiers, mod, Modifiers.PRIVATE, l);
			
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

			if ((res = IsValid (name, name)) != AdditionResult.Success)
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
			
			if ((res = IsValid (name, name)) != AdditionResult.Success)
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

			if (((bf & BindingFlags.DeclaredOnly) == 0) && (baseTypes != null)) {
				foreach (Type baseType in baseTypes) {
					members.AddRange (TypeContainer.FindMembers (baseType, mt, bf, filter, criteria));
				}
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
			if (im.ReturnType == null)
				return;

			Type return_type = im.ReturnType.Type;
			if (return_type == null)
				return_type = this.ResolveType (im.ReturnType, false, im.Location);
			
			Type [] arg_types = im.ParameterTypes (this);
			MethodBuilder mb;

			if (return_type == null)
				return;

			if (return_type.IsPointer && !im.UnsafeOK (this))
				return;

			if (arg_types == null)
				return;

			foreach (Type t in arg_types){

				if (t == null)
					return;
				
				if (t.IsPointer && !im.UnsafeOK (this))
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
                        // Labelling of parameters is taken care of
                        // during the Emit phase via
                        // MethodCore.LabelParameters method so I am
                        // removing the old code here.
                        //
                        
                        im.SetBuilder (mb);
                        
                }

		//
		// Populates the properties in the interface
		//
		void PopulateProperty (TypeContainer parent, DeclSpace decl_space, InterfaceProperty ip)
		{
			PropertyBuilder pb;

			ip.ReturnType = this.ResolveTypeExpr (ip.ReturnType, false, ip.Location);
			if (ip.ReturnType == null)
				return;
			
			Type prop_type = ip.ReturnType.Type;
			Type [] setter_args = new Type [1];

			if (prop_type == null)
				return;

			if (prop_type.IsPointer && !ip.UnsafeOK (this))
				return;
			
			setter_args [0] = prop_type;

			//
			// FIXME: properties are missing the following
			// flags: hidebysig newslot specialname
			//
			pb = TypeBuilder.DefineProperty (
				ip.Name, PropertyAttributes.None,
				prop_type, null);

                        MethodBuilder get = null, set = null;
                        
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
				parms [0] = new Parameter (ip.ReturnType, "value", Parameter.Modifier.NONE, null);
				InternalParameters ipp = new InternalParameters (
					this, new Parameters (parms, null, Location.Null));
					
				if (!RegisterMethod (set, ipp, setter_args)) {
					Error111 (ip);
					return;
				}
			}

			TypeManager.RegisterProperty (pb, get, set);
			property_builders.Add (pb);
			ip.SetBuilders (pb, get, set);
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
			ie.ReturnType = this.ResolveTypeExpr (ie.ReturnType, false, ie.Location);
			if (ie.ReturnType == null)
				return;
			
			Type event_type = ie.ReturnType.Type;

			if (event_type == null)
				return;

			if (event_type.IsPointer && !ie.UnsafeOK (this))
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
			parms [0] = new Parameter (ie.ReturnType, "value", Parameter.Modifier.NONE, null);
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

			TypeManager.RegisterEvent (eb, add, remove);
			event_builders.Add (eb);

                        ie.SetBuilder (eb);
		}

		//
		// Populates the indexers in the interface
		//
		void PopulateIndexer (TypeContainer parent, DeclSpace decl_space, InterfaceIndexer ii)
		{
			PropertyBuilder pb;
			ii.ReturnType = this.ResolveTypeExpr (ii.ReturnType, false, ii.Location);
			if (ii ==null || ii.ReturnType == null)
				return;
			
			Type prop_type = ii.ReturnType.Type;
			Type [] arg_types = ii.ParameterTypes (this);
			Type [] value_arg_types;

			if (prop_type == null)
				return;

			if (prop_type.IsPointer && !ii.UnsafeOK (this))
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
					if (t.IsPointer && !ii.UnsafeOK (this))
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
				pv [p.Length] = new Parameter (ii.ReturnType, "value", Parameter.Modifier.NONE, null);
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

			property_builders.Add (pb);

			ii.SetBuilders (pb, get_item, set_item);
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

		TypeExpr GetInterfaceTypeByName (string name)
		{
			Type t = FindType (Location, name);

			if (t == null) {
				Report.Error (246, Location, "The type or namespace `" + name +
					      "' could not be found");
				return null;
			}
			
			if (t.IsInterface)
				return new TypeExpression (t, Location);
				
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
		TypeExpr [] GetInterfaceBases (out bool error)
		{
			TypeExpr [] tbases;
			int i;

			error = false;
			if (Bases == null)
				return null;
			
			tbases = new TypeExpr [Bases.Count];
			i = 0;

			foreach (string name in Bases){
				TypeExpr t;

				t = GetInterfaceTypeByName (name);
				if (t == null){
					error = true;
					return null;
				}

				if (!t.AsAccessible (Parent, ModFlags))
					Report.Error (61, Location,
						      "Inconsistent accessibility: base interface `" +
						      t.Name + "' is less accessible than interface `" +
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
			TypeExpr [] ifaces;
			bool error;

			if (TypeBuilder != null)
				return TypeBuilder;
			
			if (InTransit)
				return null;
			
			InTransit = true;
			
			EmitContext ec = new EmitContext (this, this, Location, null, null,
							  ModFlags, false);

			ifaces = GetInterfaceBases (out error);

			if (error)
				return null;

			if (IsTopLevel) {
				if (TypeManager.NamespaceClash (Name, Location))
					return null;
				
				ModuleBuilder builder = CodeGen.Module.Builder;

				TypeBuilder = builder.DefineType (
					Name,
					InterfaceAttr,
					(Type)null,   // Parent Type
					null);
				RootContext.RegisterOrder (this);
			} else {
				TypeBuilder builder = Parent.TypeBuilder;

				TypeBuilder = builder.DefineNestedType (
					Basename,
					InterfaceAttr,
					(Type) null, //parent type
					null);

				TypeContainer tc = TypeManager.LookupTypeContainer (builder);
				tc.RegisterOrder (this);
			}

			if (ifaces != null) {
				baseTypes = new Type[ifaces.Length];
				for (int i = 0; i < ifaces.Length; ++i) {
					Type itype = ifaces [i].ResolveType (ec);
					TypeBuilder.AddInterfaceImplementation (itype);
					baseTypes [i] = itype;
				}
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


                //
                // In the case of Interfaces, there is nothing to do here
                //
		public override bool Define (TypeContainer parent)
		{
			return true;
		}

                /// <summary>
		///   Applies all the attributes.
		/// </summary>
		public void Emit (TypeContainer tc) {
                        if (OptAttributes != null) {
				EmitContext ec = new EmitContext (tc, this, Location, null, null, ModFlags, false);
				Attribute.ApplyAttributes (ec, TypeBuilder, this, OptAttributes);
			}

			EmitSubType (tc, defined_method);
			EmitSubType (tc, defined_properties);
			EmitSubType (tc, defined_indexer);
			EmitSubType (tc, defined_events);
		}

		void EmitSubType (TypeContainer tc, ArrayList subType) {
			if (subType == null)
				return;
                                        
			foreach (InterfaceMemberBase imb in subType) {
				//TODO: set it somewhere earlier
				imb.ModFlags = ModFlags;
				imb.Emit (tc, this);
                        }
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

	public abstract class InterfaceMemberBase: MemberCore, IAttributeSupport {
                // Why is not readonly
		public Expression ReturnType;

		public InterfaceMemberBase (Expression type, string name, int mod_flags, Attributes attrs, Location loc):
			base (name, attrs, loc)
		{
                	ReturnType = type;
			ModFlags = mod_flags;
		}

		public virtual EmitContext Emit (TypeContainer tc, DeclSpace ds) {
			EmitContext ec = null;
			if (OptAttributes != null) {
				ec = new EmitContext (tc, ds, Location, null, null, ModFlags, false);

				Attribute.ApplyAttributes (ec, null, this, OptAttributes);
			}

			return ec;
		}

		#region IAttributeSupport Members
		public abstract void SetCustomAttribute (CustomAttributeBuilder customBuilder);
		#endregion
	
		public override bool Define (TypeContainer parent) {
			throw new NotImplementedException ();
		}
	}

	abstract public class InterfaceSetGetBase: InterfaceMemberBase 
	{
		internal sealed class PropertyAccessor: IAttributeSupport 
		{
			Attributes m_attrs;
			MethodBuilder m_builder;

			public PropertyAccessor (Attributes attrs) {
				m_attrs = attrs;
			}
                        
			public MethodBuilder Builder {
				set {
					m_builder = value;
				}
			}
                        
			public void Emit (EmitContext ec) {
				if (m_attrs != null) {
					Attribute.ApplyAttributes (ec, this, this, m_attrs);
                                }
			}
					
			public void SetCustomAttribute (CustomAttributeBuilder customAttribute) {
				m_builder.SetCustomAttribute (customAttribute);
			}
		}
            

		PropertyAccessor m_get;
		PropertyAccessor m_set;
		protected PropertyBuilder Builder;

		public readonly bool HasSet;
		public readonly bool HasGet;
		
		public InterfaceSetGetBase (Expression type, string name, int modflags,
						bool has_get, bool has_set, Attributes prop_attrs, Attributes get_attrs,
                        Attributes set_attrs, Location loc)
			:base (type, name, modflags, prop_attrs, loc)
		{
			HasGet = has_get;
			HasSet = has_set;
			m_get = new PropertyAccessor (get_attrs);
			m_set = new PropertyAccessor (set_attrs);
		}

		public override EmitContext Emit (TypeContainer tc, DeclSpace ds) {
			EmitContext ec = base.Emit (tc, ds);
			if (ec == null)
				ec = new EmitContext (tc, ds, Location, null, null, ModFlags, false);

			m_get.Emit (ec);
			m_set.Emit (ec);
			return ec;
		}

		// TODO: It would be nice to have this method private
		public void SetBuilders (PropertyBuilder pb, MethodBuilder gb, MethodBuilder sb) {
			Builder = pb;
			m_get.Builder = gb;
			m_set.Builder = sb;
		}

		public override void SetCustomAttribute (CustomAttributeBuilder customBuilder) {
			Builder.SetCustomAttribute (customBuilder);
		}

	}

	public class InterfaceEvent : InterfaceMemberBase {
		MyEventBuilder Builder;
                
		public InterfaceEvent (Expression type, string name, int mod_flags, Attributes attrs,
				       Location loc)
			: base (type, name, mod_flags, attrs, loc)
		{
		}

		public override string GetSignatureForError () {
			return TypeManager.GetFullNameSignature (Builder);
		}

		public void SetBuilder (MyEventBuilder eb) {
			Builder = eb;
		}

		public override void SetCustomAttribute (CustomAttributeBuilder customBuilder) {
			Builder.SetCustomAttribute (customBuilder);
		}
	}
	
	public class InterfaceMethod : InterfaceMemberBase {
		public readonly Parameters Parameters;
		MethodBuilder Builder;
                
		public InterfaceMethod (Expression return_type, string name, int mod_flags, Parameters args,
					Attributes attrs, Location l)
			: base (return_type, name, mod_flags, attrs, l)
		{
			this.Parameters = args;
		}

		public override EmitContext Emit(TypeContainer tc, DeclSpace ds) {
			EmitContext ec = base.Emit(tc, ds);
			if (ec == null) 
				ec = new EmitContext (tc, ds, Location, null, null, ModFlags, false);

			MethodCore.LabelParameters (ec, Builder, Parameters, OptAttributes, Location);
			return ec;
		}

		/// <summary>
		///   Returns the signature for this interface method
		/// </summary>
		public string GetSignature (DeclSpace ds)
		{
			ReturnType = ds.ResolveTypeExpr (ReturnType, false, Location);
			if (ReturnType == null)
				return null;
			
			Type ret = ReturnType.Type;
			string args = Parameters.GetSignature (ds);

			if ((ret == null) || (args == null))
				return null;
			
			return ModFlags + ret.FullName + "(" + args + ")";
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.CSharpSignature (Builder);
		}

		public Type [] ParameterTypes (DeclSpace ds)
		{
			return Parameters.GetParameterInfo (ds);
		}

		public void SetBuilder (MethodBuilder mb) {
			Builder = mb;
		}

		public override void SetCustomAttribute(CustomAttributeBuilder customBuilder) {
			Builder.SetCustomAttribute (customBuilder);
		}
	}

	public class InterfaceProperty : InterfaceSetGetBase 
	{
		public InterfaceProperty (Expression type, string name,
			int mod_flags, bool has_get, bool has_set,
			Attributes prop_attrs, Attributes get_attrs,
			Attributes set_attrs, Location loc)
			: base (type, name, mod_flags, has_get, has_set, prop_attrs, get_attrs, set_attrs, loc)
		{
		}

		public override string GetSignatureForError () {
			return TypeManager.CSharpSignature (Builder, false);
		}	
	}

	public class InterfaceIndexer : InterfaceSetGetBase {
		public readonly Parameters Parameters;
                
		public InterfaceIndexer (Expression type, Parameters args, bool do_get, bool do_set,
					 int mod_flags, Attributes attrs, Attributes get_attrs, Attributes set_attrs,
                                         Location loc)
			: base (type, "Item", mod_flags, do_get, do_set, attrs, get_attrs, set_attrs, loc)
		{
			Parameters = args;
		}

		public override string GetSignatureForError() {
			return TypeManager.CSharpSignature (Builder, true);
		}

		public Type [] ParameterTypes (DeclSpace ds) {
			return Parameters.GetParameterInfo (ds);
		}
	}
}
